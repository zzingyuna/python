using System;
using System.Collections;//큐나 어레이등의 콜렉션 사용
using System.Collections.Generic;
using System.Text;
using System.Xml; //xml 리드용
using System.IO;
using Crawl.ClassLib;
using System.Data;
using System.Net;
using System.Text.RegularExpressions;//정규식 이용
using System.Threading;
using System.Configuration;
using System.Security.Cryptography;
using System.Web;
using System.Linq;

/// <summary>
/// 프로그램 기본 흐름
/// 1. site_info에서 데드링크 패턴을 가져와
///    채널큐에 넣는다.
/// 2. 채널큐에 쌓여 있는 작업 대상을 하나씩 꺼내어 대상이 되는 URL을 뽑아낸다.
/// 3. 채널 하나당 하나의 잡큐에 해당 URL을 넣고 스레드 하나를 배정한다.
/// 4. 위와 같은 작업을 최대 10개 까지 실시하여 10개의 스레드가 10채널씩 작업 하도록 한다.
/// </summary>
namespace DeadLinkSolution
{

    /// <summary>
    /// 웹서비스 접근시 암호화 키
    /// </summary>
    class WATCrypt
    {
        byte[] Skey = new byte[8];
        public WATCrypt(string strKey)
        {
            Skey = Encoding.UTF8.GetBytes(strKey);
        }
        public string Encrypt(string p_data)
        {
            if (Skey.Length != 8)
            {
                throw (new Exception("Invalid key."));
            }

            DESCryptoServiceProvider rc2 = new DESCryptoServiceProvider();

            rc2.Key = Skey;
            rc2.IV = Skey;

            MemoryStream ms = new MemoryStream();
            CryptoStream cryStream = new CryptoStream(ms, rc2.CreateEncryptor(), CryptoStreamMode.Write);
            byte[] data = Encoding.UTF8.GetBytes(p_data.ToCharArray());

            cryStream.Write(data, 0, data.Length);
            cryStream.FlushFinalBlock();

            return Convert.ToBase64String(ms.ToArray());
        }
    }

    #region Channel
    // 채널에 관한 정보를 로딩하고 채널 정보를 하나씩 큐에서 내어 주는 역할을 하는 클래스 
    public class Channel
    {
        //각 채널들 정보를 저장해 두는 채널큐
        private Queue channelQueue;

        //큐에 들어갈 구조체를 정의 한다.
        public struct channelInfo
        {
            public int idleTime;//휴지시간(조회간격)       
            public string channelNm;//채널이름
            public string pattern;//존재 패턴명
            public string existYN;
            public int ipBlockYN;//ip방화벽패턴 막힘여부
        }
        /// <summary>
        /// 채널 객체의 생성자
        /// </summary>
        public Channel()
        {
            this.channelQueue = new Queue();//큐 인스턴스 생성
            this.LoadChannelInfo();
        }

        /// <summary>
        /// 설정 파일에서 각 채널에 대한 정보를 로딩하여 채널큐에 삽입한다.
        /// </summary>
        /// <param name="fileName">설정파일명</param>
        private void LoadChannelInfo()
        {

            List<SiteInfoModel> list = new List<SiteInfoModel>();

            list = GetLoadPattern();

            if (list != null && list.Count > 0)
            {
                try
                {

                    foreach (SiteInfoModel row in list)
                    {
                        // 테스트 
                        if (row.SourceSiteNm == "인크루트" || row.SourceSiteNm == "사람인")
                        {
                            channelInfo channelStruct = new channelInfo();
                            channelStruct.channelNm = row.SourceSiteNm;
                            channelStruct.pattern = row.DeadlinkPattern1;
                            if (row.DeadlinkIdleTime == "")
                            {
                                channelStruct.idleTime = 10;
                            }
                            else
                            {
                                channelStruct.idleTime = int.Parse(row.DeadlinkIdleTime);
                            }
                            channelStruct.existYN = row.DeadlinkPattern2 == null || row.DeadlinkPattern2 == "" ? "Y" : "N";
                            channelStruct.ipBlockYN = row.IpBlockYN;
                            //채널큐에 구조체를 엔큐함.
                            try
                            {
                                this.channelQueue.Enqueue(channelStruct);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("LoadChannelInfo: " + ex.ToString());
                                Log.WriteLine(ex.ToString());
                            }
                        } // 테스트 if (row.SourceSiteNm == "인크루트" || row.SourceSiteNm == "사람인")

                    }
                }
                catch (XmlException xe)
                {
                    Console.WriteLine("XML Parsing Error: " + xe);
                    Log.WriteLine("XML Parsing Error: " + xe);
                }
                catch (IOException ioe)
                {
                    Console.WriteLine("File I/O Error: " + ioe);
                    Log.WriteLine("File I/O Error: " + ioe);
                }
            }
            else
            {
                Console.WriteLine("잡그룹을 로드할 수 없음");
                Log.WriteLine("잡그룹을 로드할 수 없음");
            }
        }

        /// <summary>
        /// 채널큐의 다음 작업을 리턴한다.
        /// </summary>
        public channelInfo GetChannelInfo()
        {
            return (channelInfo)channelQueue.Dequeue();
        }
        public int GetRemainCount()
        {
            return channelQueue.Count;
        }

        private List<SiteInfoModel> GetLoadPattern()
        {
            List<SiteInfoModel> returnlist = new List<SiteInfoModel>();

            try
            {
                //Channel scrobj = new Channel();
                string secure = "dkfqk12!";

                WATCrypt m_crypt = new WATCrypt(secure);

                DeadLinkSolutionGW.DeadLinkSolutionGWSoapClient deadlinkService = new DeadLinkSolutionGW.DeadLinkSolutionGWSoapClient();

                DataTable table = null;
                using (DataSet ds = deadlinkService.GetLoadPattern(m_crypt.Encrypt("getLoadPattern").Replace("+", "")))
                {
                    if (!Helper.IsNullOrEmpty(ds, 0))
                    {
                        table = ds.Tables[0];
                    }
                }

                List<SiteInfoModel> list = new List<SiteInfoModel>();
                if (table.Rows.Count > 0)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        SiteInfoModel addItem = new SiteInfoModel();
                        addItem.SeqNo = row["Seq_No"] == null ? 0 : int.Parse(row["Seq_No"].ToString());
                        addItem.FileNm = row["FileNm"] == null ? "" : row["FileNm"].ToString();
                        addItem.PcIp = row["PC_IP"] == null ? "" : row["PC_IP"].ToString();
                        addItem.PcNm = row["PC_Nm"] == null ? "" : row["PC_Nm"].ToString();
                        addItem.PathNm = row["Path_Nm"] == null ? "" : row["Path_Nm"].ToString();
                        addItem.SourceSiteNm = row["SourceSiteNm"] == null ? "" : row["SourceSiteNm"].ToString();
                        addItem.StopAfterNo = row["StopAfter_No"] == null ? 0 : int.Parse(row["StopAfter_No"].ToString());
                        addItem.CategoryNm = row["Category_YN"] == null ? 0 : int.Parse(row["Category_YN"].ToString());
                        addItem.ClStartDt = row["Cl_Start_Dt"] == null || row["Cl_Start_Dt"].ToString() == "" ? DateTime.Now : DateTime.Parse(row["Cl_Start_Dt"].ToString());
                        addItem.ClEndDt = row["Cl_End_Dt"] == null || row["Cl_End_Dt"].ToString() == "" ? DateTime.Now : DateTime.Parse(row["Cl_End_Dt"].ToString());
                        addItem.SetStopAfterNo = row["SetStopAfter_No"] == null || row["SetStopAfter_No"].ToString() == "" ? 0 : int.Parse(row["SetStopAfter_No"].ToString());
                        addItem.DeadlinkPattern1 = row["Dead_Link_Pattern1"] == null ? "" : row["Dead_Link_Pattern1"].ToString();
                        addItem.DeadlinkPattern2 = row["Dead_Link_Pattern2"] == null ? "" : row["Dead_Link_Pattern2"].ToString();
                        addItem.DeadlinkIdleTime = row["Dead_Link_Idle_Time"] == null ? "" : row["Dead_Link_Idle_Time"].ToString();
                        //addItem.IpBlockYN = row["IpBlock_YN"] == null || row["IpBlock_YN"].ToString() == "" ? 0 : int.Parse(row["IpBlock_YN"].ToString());
                        addItem.IpBlockYN = 1; // 테스트
                        list.Add(addItem);
                    }

                    // SourceSiteNm 중복 하나로!!
                    foreach (SiteInfoModel listItem in list)
                    {
                        int registCnt = returnlist.Where(p => p.SourceSiteNm == listItem.SourceSiteNm).Count();
                        if (registCnt < 1)
                        {
                            returnlist.Add(listItem);
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine("GetLoadPattern: " + exp.Message);
                Log.WriteLine("GetLoadPattern: " + exp.Message + " --> " + exp.InnerException);
            }

            return returnlist;
        }
    }
    #endregion

    #region JobQueue
    // 각채널의 작업 내용을 작업 큐로 작업을 배분하고 큐에서 작업을 빼내게 하는 클래스
    public class JobQueue
    {
        //대상이 되는 큐의 순서를 정하기 위해서 아래 변수를 정의한다.

        private int totalCount;
        public int TotalCount
        {
            get
            {
                return totalCount;
            }
        }
        //잡큐에 들어갈 데이타 구조체
        public struct jobPostRows
        {
            public int jobPostNo;
            public string linkUrl;
            public string pattern;
            public string existYn;
            public int idleTime;
            public int ipBlockYN;
        }
        private Queue[] jobQueue;//잡큐 배열


        /// <summary>
        /// 해당 잡큐 번호를 조회하여 큐에 있는 채용공고(작업대상)을 하나 리턴한다.
        /// </summary>
        /// <param name="jobNum">잡큐번호</param>
        /// <returns>작업대상 구조체</returns>
        public jobPostRows GetJobQueue(int jobNum)
        {
            jobPostRows tmpRows = new jobPostRows();
            tmpRows = (jobPostRows)jobQueue[jobNum].Dequeue();

            return tmpRows;
        }

        /// <summary>
        /// 해당 작업 번호의 잡큐의 남아 있는 갯수를 리턴한다.
        /// </summary>
        /// <param name="jobNum">잡큐번호</param>
        /// <returns>해당 번호의 큐의 크기</returns>

        public int GetCount(int jobNum)
        {
            int intTmp = 0;
            intTmp = jobQueue[jobNum].Count;
            return intTmp;
        }

        /// <summary>
        /// 큐의 전체 갯수를 리턴함. 
        /// </summary>
        /// <returns></returns>
        public int GetQueueTotalCount()
        {
            int intTmp = 0;
            if (jobQueue != null)
            {
                intTmp = jobQueue.Length;
            }
            return intTmp;
        }
        //생성자 인스턴스를 생성하고 변수를 초기화함
        public JobQueue(Channel jobChannel, string limitNo, string orderBy, string datefromnow)
        {
            totalCount = 0;
            for (int i = 0; i < jobChannel.GetRemainCount(); i++)
            {
                jobQueue = new Queue[jobChannel.GetRemainCount()];
            }

            //객체를 생성하자마자 각 잡큐에 작업을 채운다.
            this.FillJob(jobChannel, limitNo, orderBy, datefromnow);
        }


        //채널큐에 있는 잡을 조회해서 모든 잡큐에 잡을 모두 할당한다.
        private void FillJob(Channel jobChannel, string limitNo, string orderBy, string datefromnow)
        {
            //채널 객체를 생성
            //각 잡업 큐에 모든 작업을 배치한다.
            int jobNum = 0;

            Console.WriteLine("┌────────────────────────────────┐");
            while (jobChannel.GetRemainCount() > 0)
            {
                //10개의 큐에 잡을 할당한다.                         
                this.AssignJob(jobChannel, jobNum, limitNo, orderBy, datefromnow);
                jobNum++;
            }
            Console.WriteLine("│총:" + jobNum + "채널 대상 건수 :" + TotalCount.ToString() + "건                                                     │");
            Console.WriteLine("└────────────────────────────────┘");
        }

        /// <summary>
        /// 채널큐에 있는 내용하나 디큐하여 작업큐에 넣는 작업을 한다.
        /// </summary>
        private void AssignJob(Channel channel, int jobNum, string limitNo, string orderBy, string datefromnow)
        {
            int assignCnt = 0;

            Channel.channelInfo tmpStruct;
            tmpStruct = channel.GetChannelInfo();

            string channelNm = tmpStruct.channelNm;//소스 사이트
            string existYN = tmpStruct.existYN;//존재여부

            Program.ChannelName.Add(channelNm);

            string pattern = tmpStruct.pattern;//해당 패턴
            int idleTime = tmpStruct.idleTime;//유휴시간

            //쿼리설명: 마감공고일이 남아 있는 채용공고중 마감일자가 가장 빠른 채용공고부터
            //조회를 할수있도록 한다.

            this.jobQueue[jobNum] = new Queue();
            //DeadLinkFunction.DeadLinkFunction deadlinkFunction=new DeadLinkSolution.DeadLinkFunction.DeadLinkFunction();            
            //deadlinkFunction.Timeout = 3000000;
            //DataSet ds = deadlinkFunction.GetJobPostList(channelNm, nationCd, orderBy);

            DataTable ds = GetJobPostList(channelNm, limitNo, orderBy, datefromnow);
            assignCnt = ds.Rows.Count;
            foreach (DataRow row in ds.Rows)
            {

                //채용공고의 하나의 Row를 저장할 구조체 생성
                jobPostRows chRows = new jobPostRows();
                chRows.jobPostNo = int.Parse(row["jobno"].ToString());
                chRows.linkUrl = row["url"].ToString();
                chRows.pattern = pattern;
                chRows.existYn = existYN;
                chRows.idleTime = idleTime;
                chRows.ipBlockYN = tmpStruct.ipBlockYN;
                this.jobQueue[jobNum].Enqueue(chRows);
            }


            totalCount = totalCount + jobQueue[jobNum].Count; //총 갯수 저장

            if (Program.DEBUG) //디버그 모드라면 주요 사항들을 출력함.
            {
                Console.WriteLine("│채널명:" + channelNm + " 대상갯수:" + jobQueue[jobNum].Count.ToString() + "개 큐넘버:" + jobNum.ToString());
                Console.WriteLine("├────────────────────────────────┤");
            }
            Console.WriteLine("│채널명:" + channelNm + " 대상갯수:" + jobQueue[jobNum].Count.ToString() + "개 큐넘버:" + jobNum.ToString());
            Console.WriteLine("├────────────────────────────────┤");

        }

        private DataTable GetJobPostList(string channelNm, string limitNo, string orderBy, string datefromnow)
        {
            //질의 결과를 저장할 데이타 테이블
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("jobno", typeof(string));
            dataTable.Columns.Add("sourcesitenm", typeof(string));
            dataTable.Columns.Add("url", typeof(string));

            string solrIndexUrl = Helper.GetConfig("SolrIndexUrl");
            string requestURL = "";

            if (orderBy.ToLower() == "desc")
            {
                try
                {
                    int datefromnowInt = Int32.Parse(datefromnow);
                    if (datefromnowInt <= 30 && datefromnowInt >= 1)
                    {
                        requestURL = solrIndexUrl + "proc_jobs_kr/select/?q=*:*&fq=insertdate:[* TO NOW-" + datefromnow + "DAY]&fq=sourcesitenm:" + WebUtility.UrlEncode(channelNm.Trim()) + "&rows=" + limitNo + "&sort=insertdate " + orderBy;
                    }
                    else
                    {
                        requestURL = solrIndexUrl + "proc_jobs_kr/select/?q=*:*&fq=insertdate:[* TO NOW-6DAY]&fq=sourcesitenm:" + WebUtility.UrlEncode(channelNm.Trim()) + "&rows=" + limitNo + "&sort=insertdate " + orderBy;
                    }
                }
                catch
                {
                    requestURL = solrIndexUrl + "proc_jobs_kr/select/?q=*:*&fq=insertdate:[* TO NOW-6DAY]&fq=sourcesitenm:" + WebUtility.UrlEncode(channelNm.Trim()) + "&rows=" + limitNo + "&sort=insertdate " + orderBy;
                }

            }
            else
            {
                requestURL = solrIndexUrl + "proc_jobs_kr/select/?q=*:*&fq=sourcesitenm:" + WebUtility.UrlEncode(channelNm.Trim()) + "&rows=" + limitNo + "&sort=insertdate " + orderBy;

            }
            XmlDocument doc = new XmlDocument();
            try
            {
                //Log.WriteLine(string.Format("solr query = {0}", requestURL));
                Console.WriteLine(string.Format("solr query = {0}", requestURL));
                doc.Load(requestURL);
            }
            catch (Exception ex)
            {
                return null;
            }

            //3단계 해당 Item을 가져온다.
            XmlElement root = doc.DocumentElement;
            XmlNodeList nodes = root.SelectNodes("/response/result/doc");

            foreach (XmlNode node in nodes)
            {

                if (node.Name == "doc")
                {
                    DataRow newRow = dataTable.NewRow();
                    string jobno = "";
                    foreach (XmlNode inode in node)
                    {
                        string[] str = inode.OuterXml.Split('\"');
                        //Console.WriteLine(str[1]);
                        //Console.WriteLine(inode.InnerText);
                        switch (str[1])
                        {
                            case "jobno":
                                jobno = inode.InnerText;
                                newRow["jobno"] = inode.InnerText;
                                break;
                            case "url":
                                newRow["url"] = inode.InnerText;

                                break;

                            case "sourcesitenm":
                                newRow["sourcesitenm"] = inode.InnerText;
                                break;

                        }
                    }
                    dataTable.Rows.Add(newRow);//새로운 행 추가
                }

            }
            return dataTable;
        }

    }
    #endregion

    #region ThreadWithParam
    // 스레드에 인자값을 넘기기 위해서 아래 클래스를 정의함.
    public class ThreadWithParam
    {
        public delegate void MyThreadProcType(int num, JobQueue[] o);

        private class ParamProc
        {
            public JobQueue[] param;
            public int num;
            public MyThreadProcType proc;

            public void work()
            {
                proc(num, param);
            }
        }

        public static Thread CreateThread(MyThreadProcType proc, int num, params JobQueue[] arg)
        {
            ParamProc pp = new ParamProc();
            pp.param = arg;
            pp.num = num;
            pp.proc = proc;
            Thread t = new Thread(new ThreadStart(pp.work));
            return t;
        }

    }
    #endregion

    #region DeadLinkProc
    // 전체적인 데드링크의 작업을 관장하는 클래스이다.
    class DeadLinkProc
    {
        static Hashtable tempLog = null;

        /// <summary>
        /// 객체를 초기화 하는 생성자
        /// </summary>
        public DeadLinkProc()
        {
            tempLog = new Hashtable();
        }

        /// <summary>
        /// 스레드를 생성하고..각 종 작업을 진행한다.
        /// </summary>
        /// <param name="limitNo">int, row개수</param>
        /// <param name="orderBy">asc/desc, 오름내림차순</param>
        /// <param name="datefromnow">int, 몇일전까지의 데이터를 가져올것인지</param>
        public void DoRun(string limitNo, string orderBy, string datefromnow)
        {
            //채널 객체를 생성한다.
            //DeadLinks.xml에 있는 채널 정보가 로드된다.
            Channel jobChannel = new Channel();

            ////잡큐 객체를 생성후 각 큐에 작업이 분산 배치된다.
            JobQueue jobProc = new JobQueue(jobChannel, limitNo, orderBy, datefromnow);

            Thread[] aThread = new Thread[jobProc.GetQueueTotalCount()];
            Console.WriteLine("작업 시작");


            //DeadLinkFunction.DeadLinkFunction deadlinkFunction = new DeadLinkSolution.DeadLinkFunction.DeadLinkFunction();
            //deadlinkFunction.SaveToErrorLog(nationcd, linkUrl, jobPostNo, "방화벽의심", System.Environment.MachineName);
            //deadlinkFunction.SaveToRunableAgent(System.Environment.MachineName);

            for (int i = 0; i < jobProc.GetQueueTotalCount(); i++)
            {
                aThread[i] = ThreadWithParam.CreateThread((new ThreadWithParam.MyThreadProcType(ThreadRun)), i, jobProc);
            }
            foreach (Thread t in aThread)
            {
                t.Start();
            }

            //////2차 스레드가 종료되기 전에 1차 스레드를 잠시 정지해둔다.
            //////만약 2차 스레드의 데드락이 우려되는 상황에서는  Join()에 인자를 주어
            //////2차 스레드의 종료를 기다리는 시간을 밀리세컨드로 넣어주면된다.
            foreach (Thread t in aThread)
            {
                t.Join();
            }

            // SaveToLog(tempLog);

        }


        /// <summary>
        /// 스레드 작업이 
        /// </summary>
        /// <param name="num">작업큐 번호 스레드가 움직이면서 자신의 큐를 조회해서 작업을 실시한다.</param>
        /// <param name="p"></param>
        private void ThreadRun(int num, JobQueue[] jobProc)
        {
            // app.config에 설정된 테스트여부 값을 가져온다.
            string isTest = ConfigurationSettings.AppSettings["IsTest"];

            int startCount = jobProc[0].GetCount(num);//처음 가지고온 작업양을 저장해 둔다.
            int findCount = 0; //찾아낸 숫자

            //Console.WriteLine("쓰레드넘버:" + num.ToString());
            //해당 작업번호의 큐에 작업이 남아 있다면..
            int jobPostNo = 0;
            string linkUrl = "";
            string pattern = "";
            string existyn = "";
            int idleTime = 0;
            int result = 0;
            int exist = 0;
            string msg = "";
            string linkResult = " ";  // 테스트
            Hashtable fireWallCnt = new Hashtable();
            while (jobProc[0].GetCount(num) > 0)
            {
                msg = "";
                jobPostNo = 0;
                linkUrl = "";
                pattern = "";
                idleTime = 0;
                JobQueue.jobPostRows tmpJob = jobProc[0].GetJobQueue(num);

                if (fireWallCnt.ContainsKey(num.ToString()))
                {
                    if (int.Parse(fireWallCnt[num.ToString()].ToString()) > 5)
                    {
                        //5회 이상 방화벽에 걸렸으면? 정지
                        continue;
                    }
                }
                jobPostNo = tmpJob.jobPostNo;
                linkUrl = tmpJob.linkUrl.ToString();
                pattern = tmpJob.pattern.ToString();
                existyn = tmpJob.existYn.ToString();
                idleTime = tmpJob.idleTime;
                linkResult = linkUrl + " [ " + jobPostNo + " ] ";  // 테스트
                if (existyn == "Y")
                {
                    exist = 1;
                }
                else
                {
                    exist = 2;
                }
                if (!Program.DEBUG) //디버그 모드가 아니라면 실행함
                {

                    // 너무 빨라서 파일 쓰기 에러발생으로 시간간격을 준다
                    Random randomObject = new Random();
                    int startTime = (int)(idleTime / 2);
                    int tmpNo = randomObject.Next(startTime, idleTime);
                    Thread.Sleep(tmpNo * 500);

                    string userAgent = GetUserAgent();

                    //데드링크 패턴을 찾아내서 결과를 리턴함
                    /* 검수로 인해서 실제 타 사이트 접근은 주석 처리함 김지훈*/
                    //리턴값 정의
                    //0 데드링크 아님
                    //1 단순 데드링크
                    //2 사이트 점검으로 예상
                    //3 방화벽 막히는것으로 예상
                    if (tmpJob.ipBlockYN > 0)
                    {
                        /*
                        //불규칙한 접속 패턴을 위한 랜덤 객체를 생성한다.
                        randomObject = new Random();
                        startTime = (int)(idleTime / 2);
                        tmpNo = randomObject.Next(startTime, idleTime);
                        Thread.Sleep(tmpNo * 500);
                        */
                        Proxy p = new Proxy();
                        string proxyhost = p.GetProxy();
                        linkResult = linkResult + " / " + proxyhost + " / " + userAgent; // 테스트
                        result = this.FindPattern(jobPostNo, linkUrl, pattern, exist, proxyhost, userAgent);
                    }
                    else
                    {
                        result = this.FindPattern(jobPostNo, linkUrl, pattern, exist, "", userAgent);
                    }

                    //데드링크를 찾아내면 진입한다.
                    if (result > 0)
                    {
                        msg = "스레드번호:" + num.ToString() + "|Job_Post_No:" + jobPostNo + "|URL:" + linkUrl;
                        if (result == 3)
                        {
                            if (!fireWallCnt.ContainsKey(num.ToString()))
                            {
                                fireWallCnt.Add(num.ToString(), 1);
                            }
                            else
                            {
                                fireWallCnt[num.ToString()] = int.Parse(fireWallCnt[num.ToString()].ToString()) + 1;
                            }

                            msg += "\r\nRESULT:방화벽 막힘의심!!";
                        }
                        else
                        {
                            if (isTest == "0")
                            {
                                if (this.DeleteJobPost(jobPostNo))
                                {
                                    msg += "\r\nRESULT:찾아내어 삭제함";
                                    findCount++;
                                    Program.totalDeadCnt++; //찾아내고 처리한 데드링크의 총수를 구한다.
                                }
                                else
                                {
                                    msg += "\r\nRESULT:찾아냈지만 디비 삭제 이상";
                                }
                            }
                        }
                        Console.WriteLine(msg);
                    }

                    // 테스트
                    if (isTest == "1")
                    {
                        string resultStr = "";
                        switch (result)
                        {
                            case 0: resultStr = "live"; break;
                            case 1: resultStr = "!! 데드링크 !!"; break;
                            case 2: resultStr = "확인대상"; break;
                            case 3: resultStr = "firewall"; break;
                            default: resultStr = "none"; break;
                        }
                        linkResult = linkResult + " ->" + resultStr;
                        Log.WriteLine(linkResult);
                    } //테스트 if (isTest != "0")

                }
                Program.totalCnt++;
            }
            Console.WriteLine("┌────────────────────────────────┐");
            Console.WriteLine("│채널명:" + Program.ChannelName[num].ToString() + "(" + num.ToString() + ")" + " 대상갯수:" + findCount.ToString() + "/" + startCount.ToString() + "종료함!!");
            Console.WriteLine("└────────────────────────────────┘");

            //통계 저장
            SaveToLog(Program.ChannelName[num].ToString(), startCount, findCount);

        }




        /// <summary>
        /// 링크를 조회하여 해당 패턴이 존재하는지 찾아내는 멤버 함수이다.
        /// </summary>
        /// <param name="jobPostNo"></param>
        /// <param name="linkUrl"></param>
        /// <param name="pattern"></param>
        /// <param name="exists"></param>
        /// <returns></returns>
        private int FindPattern(int jobPostNo, string linkUrl, string pattern, int exists, string proxyinfo, string userAgent)
        {
            //리턴값 정의
            //0 데드링크 아님
            //1 단순 데드링크
            //2 사이트 점검으로 예상
            //3 방화벽 막히는것으로 예상
            string content = "";
            try
            {
                HttpWebRequest wrq = (HttpWebRequest)WebRequest.Create(linkUrl);

                if (proxyinfo.Length > 0)
                {
                    string[] hostinfo = proxyinfo.Split(',');
                    // 프록시 값을 지정하여 접근
                    //WebProxy proxy = new WebProxy(hostinfo[0], int.Parse(hostinfo[1]));

                    WebProxy proxy = new WebProxy("128.199.190.243", 80);
                    // 로컬 주소에 대해 프록시 서버를 사용하지 않으려면 true이고, 그렇지 않으면 false입니다. 기본값은 false입니다.
                    proxy.BypassProxyOnLocal = false;

                    wrq.Proxy = proxy;
                    wrq.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
                }

                //헤더값 세팅
                //wrq.ContentType = "application/x-www-form-urlencoded";
                //wrq.AllowAutoRedirect = false;
                wrq.Method = "GET";
                wrq.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                wrq.UserAgent = userAgent;
                wrq.KeepAlive = false;
                wrq.Referer = linkUrl;

                ////신호 응답을 받는다.
                HttpWebResponse wrs = (HttpWebResponse)wrq.GetResponse();
                // 받아온 컨텐츠의 인코딩 값 확인
                string charSet = wrs.CharacterSet;
                Encoding encoding;
                if (String.IsNullOrEmpty(charSet) || charSet == "ISO-8859-1")
                    encoding = Encoding.GetEncoding("euc-kr");
                else
                    encoding = Encoding.GetEncoding(charSet);

                //응답 컨텐츠를 읽어오기 위해 스트림 객체 추출;
                Stream strm = wrs.GetResponseStream();
                StreamReader sr = new StreamReader(strm, encoding);
                content = sr.ReadToEnd(); //처음부터 끝까지 읽어들인다.                
                WebHeaderCollection webHeaders = wrq.Headers; //헤더 정보를 받아온다.

                //객체를 닫아준다.
                sr.Close();
                wrs.Close();
                strm.Close();

                // 받아온 content데이터를 html로 저장할것인가..(1이면 저장, 아니면 저장안함)
                string isSaveHtml = ConfigurationSettings.AppSettings["SaveHtml"];
                if (isSaveHtml == "1")
                {
                    string savepath = ConfigurationSettings.AppSettings["XmlfilePath"].ToString() + @"htmls\" + jobPostNo.ToString() + ".html";
                    System.IO.File.WriteAllText(savepath, content, encoding);
                }

            }
            catch (IOException iex)
            {
                return 0;
            }
            catch (WebException wex)
            {

                if (wex.Message.ToString().IndexOf("404") > 0)
                {
                    //Log.WriteLine("404페이지입니다.(" + linkUrl + ")");
                    SaveToDeadlinkLog(jobPostNo, linkUrl, "404에러", wex.Status.ToString(), wex.Message.ToString());
                    return 1;
                }
                if (wex.Status == WebExceptionStatus.ConnectionClosed)
                {
                    SaveToDeadlinkLog(jobPostNo, linkUrl, "ConnectionClosed", wex.Status.ToString(), wex.Message.ToString());
                    return 1;
                }

                if (wex.Message.ToString().IndexOf("403") > 0)
                {
                    if (linkUrl.IndexOf("gemcom.co.kr") > -1) //갬콤의 경우 예외처리
                    {
                        return 0;
                    }
                    else
                    {
                        SaveToDeadlinkLog(jobPostNo, linkUrl, "403에러", wex.Status.ToString(), wex.Message.ToString());
                        //403페이지라면...데드링크로 간주하고.true를 리턴
                        //Log.WriteLine("403페이지입니다.(" + linkUrl + ")");
                        return 2;
                    }
                }
                else if (wex.Message.ToString().IndexOf("502") > 0)
                {
                    // 잘못된 게이트웨이
                    SaveToDeadlinkLog(jobPostNo, linkUrl, "502에러", wex.Status.ToString(), wex.Message.ToString());
                    Proxy p = new Proxy();
                    p.DeleteProxy(proxyinfo);
                    return 2;
                }
                else if (wex.Message.ToString().IndexOf("500") > 0)
                {
                    //500 에러를 데드링크로 볼것인가? 아니면 일시적 장애로 볼것인가...
                    //Console.WriteLine("500페이지입니다.");
                    //Console.WriteLine("예외발생지점:" + linkUrl);
                    //Console.WriteLine("예외설명:" + wex.Message);
                    //Console.WriteLine("원격호스트응답값:" + wex.Response);
                    //Console.WriteLine("예외상태:" + wex.Status);
                    //Console.WriteLine("예외상태:" + wex.InnerException);
                    //Console.WriteLine("예외상태:" + wex.HelpLink);
                    //Console.WriteLine("예외상태:" + wex.TargetSite);
                    //Console.WriteLine("예외상태:" + wex.StackTrace);
                    //Console.WriteLine("예외상태:" + wex.Source);
                    //Console.WriteLine("예외상태:" + wex.ToString());
                    //2008년 1월 10일
                    //작성자:김지훈 대리
                    //일단 데드링크로 본다.
                    //하지만 개발자 및 서버에 의한 500에러는 대비 해야 한다.
                    //데이터가 한순간에 날라 갈수 있음        
                    //Log.WriteLine("500페이지입니다.(" + linkUrl + ")");

                    //2008 2 18
                    //일부 사이트에서 너무 데드링크를 많이 잡아내서 일단..데드링크 처리를 하지 않는다.

                    //2008. 5월 15일
                    //데드링크로 보기로 변경
                    SaveToDeadlinkLog(jobPostNo, linkUrl, "500에러", wex.Status.ToString(), wex.Message.ToString());
                    return 2;
                }
                else if (wex.Status.ToString() == "ConnectFailure")
                {

                    //Console.WriteLine("방화벽에서 막혔습니다.");
                    //Console.WriteLine("예외발생지점:" + linkUrl);
                    //Console.WriteLine("예외발생지점:" + linkUrl);
                    //Console.WriteLine("예외설명:" + wex.Message);
                    //Console.WriteLine("원격호스트응답값:" + wex.Response);
                    //Console.WriteLine("예외상태:" + wex.Status);
                    //Log.WriteLine("예외발생지점:" + linkUrl);
                    //Log.WriteLine("!!!!방화벽에서 막혔습니다.!!!!");
                    //방화벽에 막힌것은 데드링크로 간주하지 않는다.
                    //다만 데드링크를 체크하지 못할 뿐이다.     
                    SaveToDeadlinkLog(jobPostNo, linkUrl, "방화벽막힘", wex.Status.ToString(), wex.Message.ToString());
                    return 3;
                }
                else if (wex.Status.ToString() == "NameResolutionFailure")
                {
                    //도메인 낙장은 데드링크이다.
                    //Console.WriteLine("도메인을 찾을수 없습니다.");
                    //Console.WriteLine("예외발생지점:" + linkUrl);
                    //Console.WriteLine("예외발생지점:" + linkUrl);
                    //Console.WriteLine("예외설명:" + wex.Message);
                    //Console.WriteLine("원격호스트응답값:" + wex.Response);
                    //Console.WriteLine("예외상태:" + wex.Status);
                    ////Log.WriteLine("예외발생지점:"+linkUrl);
                    //Log.WriteLine(wex.ToString());
                    //Log.WriteLine("도메인찾을수 없음(" + linkUrl + ")");
                    SaveToDeadlinkLog(jobPostNo, linkUrl, "도메인낙장", wex.Status.ToString(), wex.Message.ToString());
                    return 2;
                }
                else
                {
                    //Log.WriteLine("기타 예외 사항(" + linkUrl + ")");
                    //Console.WriteLine("기타 사항들");
                    //Console.WriteLine("예외발생지점:" + linkUrl);
                    //Console.WriteLine("예외설명:"+ wex.Message);
                    //Console.WriteLine("원격호스트응답값:"+wex.Response);
                    //Console.WriteLine("예외상태:"+wex.Status);                    
                    /* ErrorEvent(wex.ToString(), linkUrl);*/
                    SaveToDeadlinkLog(jobPostNo, linkUrl, "기타에러", wex.Status.ToString(), wex.Message.ToString());
                    return 0;
                }
            }
            catch (Exception ex)
            {

            }
            try
            {
                string[] patternArray = pattern.Split('★');
                int result = 0;
                for (int i = 0; i < patternArray.Length; i++)
                {
                    if (patternArray[i].Length > 6)
                    {
                        if (exists == 1) //패턴이 있는것을 찾음
                        {
                            Regex rx = new Regex(patternArray[i], RegexOptions.IgnoreCase | RegexOptions.Compiled);
                            if (rx.IsMatch(content))
                            {
                                result = 1;
                            }
                            else
                            {
                                result = 0;
                            }
                        }
                        else //패턴이 없는것을 찾음
                        {
                            Regex rx = new Regex(patternArray[i], RegexOptions.IgnoreCase | RegexOptions.Compiled);
                            if (!rx.IsMatch(content))
                            {
                                result = 1;
                            }
                            else
                            {
                                result = 0;
                            }
                        }
                    }
                    else
                    {
                        result = 0;
                    }
                    if (result > 0) //먼가를 찾은 경우 그만 한다.
                    {
                        break;
                    }
                }
                return result;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// UserAgent 랜덤 세팅
        /// </summary>
        /// <returns></returns>
        private string GetUserAgent()
        {
            string returnStr = ".NET Framework Test Client";
            Random _random = new Random();
            int randomNum = _random.Next(1, 5);
            switch (randomNum)
            {
                case 1: returnStr = "Mozilla/5.0 (iPhone; CPU iPhone OS 9_1 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Version/9.0 Mobile/13B143 Safari/601.1"; break;
                case 2: returnStr = "Mozilla/5.0 (Linux; Android 5.0; SM-G900P Build/LRX21T) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.94 Mobile Safari/537.36"; break;
                case 3: returnStr = "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.94 Mobile Safari/537.36"; break;
                case 4: returnStr = "Mozilla/5.0 (iPhone; CPU iPhone OS 9_1 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Version/9.0 Mobile/13B143 Safari/601.1"; break;
                case 5: returnStr = "Mozilla/5.0 (iPhone; CPU iPhone OS 9_1 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Version/9.0 Mobile/13B143 Safari/601.1"; break;
                case 6: returnStr = "Mozilla/5.0 (iPad; CPU OS 9_1 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Version/9.0 Mobile/13B143 Safari/601.1"; break;
                case 7: returnStr = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/534.54.16 (KHTML, like Gecko) Version/5.1.4 Safari/534.54.16"; break;
                    /*
                    case 1: returnStr = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.0; EmbeddedWB 14.52 from: http://www.bsalsa.com/ EmbeddedWB 14.52)"; break;
                    case 2: returnStr = @"Mozilla/5.0 (Windows Phone 10.0;  Android 4.2.1; Nokia; Lumia 520) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.71 Mobile Safari/537.36 Edge/12.0"; break;
                    case 3: returnStr = @"Mozilla/5.0 (Macintosh; U; Intel Mac OS X 10_6_3; en-US) AppleWebKit/533.4 (KHTML, like Gecko) Chrome/5.0.375.70 Safari/533.4"; break;
                    case 4: returnStr = @"Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; Trident/4.0; IPMS/A640400A-14D460801A1-000000426571; TCO_20110131100426; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; InfoPath.2; Tablet PC 2.0)"; break;
                    case 5: returnStr = "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko"; break;
                    */
            }
            return returnStr;
        }




        // 공고를 삭제한다.
        private bool DeleteJobPost(int jobPostNo)
        {
            bool result = false;
            string linkUrl = ConfigurationSettings.AppSettings["DeleteJobPostUrl"] + jobPostNo.ToString();
            //삭제를 위한 쿼리
            string content = "";
            try
            {
                HttpWebRequest wrq = (HttpWebRequest)WebRequest.Create(linkUrl);
                //wrq.AllowAutoRedirect = false;
                ////헤더값 세팅
                //헤더값 세팅
                wrq.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.0; EmbeddedWB 14.52 from: http://www.bsalsa.com/ EmbeddedWB 14.52)";
                wrq.ContentType = "application/x-www-form-urlencoded";

                ////신호 응답을 받는다.
                HttpWebResponse wrs = (HttpWebResponse)wrq.GetResponse();
                //if (wrs.StatusCode == HttpStatusCode.Moved)
                //{

                //}
                //응답 컨텐츠를 읽어오기 위해 스트림 객체 추출;
                Stream strm = wrs.GetResponseStream();
                StreamReader sr = new StreamReader(strm, System.Text.UnicodeEncoding.GetEncoding("euc-kr"));
                //처음부터 끝까지 읽어들인다.
                content = sr.ReadToEnd();
                //헤더 정보를 받아온다.
                WebHeaderCollection webHeaders = wrq.Headers;
                //객체를 닫아준다.

                sr.Close();
                wrs.Close();
                strm.Close();
            }
            catch (IOException iex)
            {
                return false;
            }
            if (content.Trim() == "1")
            {
                result = true;
            }
            else
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// 데드링크 여부 판별 로그 저장(방화벽 막힘, 500에러, 404에러 등..)
        /// </summary>
        /// <param name="sourceSiteNm"></param>
        /// <param name="scan_cnt"></param>
        /// <param name="deadlink_cnt"></param>
        private void SaveToLog(string sourceSiteNm, int scan_cnt, int deadlink_cnt)
        {
            string Reg_Dt = DateTime.Now.ToString("yyyyMMdd");
            string machineName = Environment.MachineName;

            string secure = "dkfqk12!";

            WATCrypt m_crypt = new WATCrypt(secure);

            DeadLinkSolutionGW.DeadLinkSolutionGWSoapClient deadlinkService = new DeadLinkSolutionGW.DeadLinkSolutionGWSoapClient();
            try
            {
                /****오픈 마루쪽에 업데이트 할수 있는 쿼리가 들어감. 차후 SP로 변경 요망*****/
                /*string queryString = "INSERT INTO dbo.deadlink_statistics" +
                        "(Reg_Dt,Scan_Cnt,DeadLink_Cnt,SourceSite_Nm,Machine_Nm)" +
                        "VALUES" +
                        "(@Reg_Dt,@Scan_Cnt,@DeadLink_Cnt,@SourceSite_Nm,@Machine_Nm)";
                ParameterCollection pCollection = new ParameterCollection();
                pCollection.Add(new CustomParameter("@Reg_Dt", Reg_Dt));
                pCollection.Add(new CustomParameter("@Scan_Cnt", scan_cnt));
                pCollection.Add(new CustomParameter("@DeadLink_Cnt", deadlink_cnt));
                pCollection.Add(new CustomParameter("@SourceSite_Nm", sourceSiteNm));
                pCollection.Add(new CustomParameter("@Machine_Nm", machineName));

                rowCount = SqlHelper.ExecuteNonQuery(
                    SqlHelper.GetConnection(), queryString, CommandType.Text, pCollection);*/
                deadlinkService.SaveToLog(sourceSiteNm, scan_cnt, deadlink_cnt, machineName, m_crypt.Encrypt(sourceSiteNm).Replace("+", ""));

            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// sourceSiteNm별로 데드링크 처리 후 로그테이블에 결과 입력
        /// </summary>
        /// <param name="jobPostNo"></param>
        /// <param name="linkurl"></param>
        /// <param name="type"></param>
        /// <param name="status"></param>
        /// <param name="message"></param>
        private void SaveToDeadlinkLog(int jobPostNo, string linkurl, string type, string status, string message)
        {
            string secure = "dkfqk12!";

            WATCrypt m_crypt = new WATCrypt(secure);

            DeadLinkSolutionGW.DeadLinkSolutionGWSoapClient deadlinkService = new DeadLinkSolutionGW.DeadLinkSolutionGWSoapClient();

            try
            {
                /****오픈 마루쪽에 업데이트 할수 있는 쿼리가 들어감. 차후 SP로 변경 요망*****/
                /*string queryString = "INSERT INTO dbo.deadlink_log" +
                        "(Job_No,Link_Url,Type,Status,Message)" +
                        "VALUES" +
                        "(@Job_No,@Link_Url,@Type,@Status,@Message)";
                ParameterCollection pCollection = new ParameterCollection();
                pCollection.Add(new CustomParameter("@Job_No", jobPostNo));
                pCollection.Add(new CustomParameter("@Link_Url", linkurl));
                pCollection.Add(new CustomParameter("@Type", type));
                pCollection.Add(new CustomParameter("@Status", status));
                pCollection.Add(new CustomParameter("@Message", message));

                rowCount = SqlHelper.ExecuteNonQuery(
                    SqlHelper.GetConnection(), queryString, CommandType.Text, pCollection);
                */
                deadlinkService.SaveToDeadlinkLog(jobPostNo, linkurl, type, status, message, m_crypt.Encrypt(jobPostNo.ToString()).Replace("+", ""));

            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.ToString());
            }
        }

    }
    #endregion


    #region Proxy

    /// <summary>
    /// 프록시 정보를 담거나 가져오는 클래스
    /// </summary>
    class Proxy
    {
        /// <summary>
        /// 프록시 사이트에서 정보를 가져다가 xml 파일로 저장
        /// </summary>
        public void ProxyFileSave()
        {
            try
            {
                string baseUrl = ConfigurationSettings.AppSettings["ProxylistSite"];
                string filepath = ConfigurationSettings.AppSettings["XmlfilePath"] + "ProxyData.xml";   // 저장할 xml 파일명
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.NewLineOnAttributes = true;
                XmlWriter xmlWriter = XmlWriter.Create(filepath);
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("root");

                List<string> templist = new List<string>();
                bool containsIps = true;
                int pnum = 0;
                while (containsIps)
                {
                    string result = string.Empty;
                    string linkUrl = baseUrl + "&amp;pnum=" + pnum.ToString() + "#table";

                    HttpWebRequest wrq = (HttpWebRequest)WebRequest.Create(linkUrl);
                    HttpWebResponse wrs = (HttpWebResponse)wrq.GetResponse();   //신호 응답을 받는다.                    
                    Stream strm = wrs.GetResponseStream();                      //응답 컨텐츠를 읽어오기 위해 스트림 객체 추출
                    StreamReader sr = new StreamReader(strm, System.Text.UnicodeEncoding.GetEncoding("euc-kr"));
                    result = sr.ReadToEnd();                        //처음부터 끝까지 읽어들인다.                    
                    WebHeaderCollection webHeaders = wrq.Headers;   //헤더 정보를 받아온다.

                    //객체를 닫아준다.
                    sr.Close();
                    wrs.Close();
                    strm.Close();


                    Regex ipRegex = new Regex("host=(.*?)&port=(.*?)&");
                    MatchCollection ips = ipRegex.Matches(result);
                    // 프록시 서버 데이터 저장
                    if (ips.Count > 0)
                    {
                        foreach (var item in ips)
                        {
                            string[] strSpt = item.ToString().Split('&');
                            string host = string.Format("{0},{1}", strSpt[0].Replace("host=", ""), strSpt[1].Replace("port=", ""));

                            // 중복되는 host, port 값이 있으면 넣는 작업 중단!!
                            var searchItem = templist.Where(p => p == host).FirstOrDefault();
                            if (searchItem == null)
                            {
                                templist.Add(host);
                            }
                            else
                            {
                                containsIps = false;
                                break;
                            }

                        }

                        pnum = pnum + 1; // 다음 페이지에서 프록시 서버 정보를 더 가져올 수 있도록..
                    }
                    else
                    {
                        containsIps = false;
                    }


                }

                for (int i = 0; i < templist.Count; i++)
                {
                    xmlWriter.WriteStartElement("ProxyItem");
                    xmlWriter.WriteElementString("PROXY_HOST", templist[i]);
                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndDocument();

                xmlWriter.Flush(); // xml 파일을 쓴다.
                xmlWriter.Close(); // 반드시 닫아준다.

            }
            catch (WebException wex)
            {
                if (wex.Response != null)
                {
                    Log.WriteLine("ERROR (web exception, response generated): " + Environment.NewLine + new StreamReader(wex.Response.GetResponseStream()).ReadToEnd());
                }
                else
                {
                    Log.WriteLine("ERROR (web exception, NO RESPONSE): " + wex.Message + wex.StackTrace);
                }
            }
            catch (Exception e)
            {
                Log.WriteLine("[Proxy > ProxyFileSave]" + e.Message);
            }
        }

        /// <summary>
        /// 해당 프록시 서버 정보를 지운다
        /// </summary>
        /// <param name="PROXY_HOST"></param>
        /// <param name="PROXY_PORT"></param>
        public void DeleteProxy(string hostport)
        {
            try
            {
                string[] stringSplit = hostport.Split(',');
                string host = stringSplit[0];
                string port = stringSplit[1];
                string filepath = ConfigurationSettings.AppSettings["XmlfilePath"] + "ProxyData.xml";
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.Load(filepath);
                XmlElement root = xmldoc.DocumentElement;

                foreach (XmlNode item in root.ChildNodes)
                {
                    if (item["PROXY_HOST"].InnerText == host)
                    {
                        item.ParentNode.RemoveChild(item);
                    }
                }
                string newXML = xmldoc.OuterXml;

                // save to file or whatever....
                xmldoc.Save(filepath);


                // 기존 proxy정보가 한개밖에 안남았다면.. 새로 리스트를 만들어준다!
                if (root.ChildNodes.Count < 2)
                    GetProxy();

            }
            catch (Exception e)
            {
                Log.WriteLine("[Proxy > DeleteProxy]" + e.Message);
            }
        }

        /// <summary>
        /// 랜덤으로 프록시 서버 주소를 가져온다
        /// </summary>
        /// <returns></returns>
        public string GetProxy()
        {
            string returnitem = "";
            try
            {
                string filepath = ConfigurationSettings.AppSettings["XmlfilePath"] + "ProxyData.xml";
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.Load(filepath);
                XmlElement root = xmldoc.DocumentElement;

                Random rd = new Random();
                int idx = rd.Next(0, root.ChildNodes.Count - 1);

                foreach (XmlNode item in root.ChildNodes[idx].ChildNodes)
                {
                    switch (item.Name)
                    {
                        case "PROXY_HOST": returnitem = item.InnerText; break;
                    }
                }

            }
            catch (Exception e)
            {
                Log.WriteLine("[Proxy > GetProxy]" + e.Message);
            }

            return returnitem;
        }

    }

    #endregion

    class Program
    {
        public static ArrayList ChannelName = null;//채널명과 큐번호를 임시로 저장함. 원활한 출력을 위해
        public static int totalCnt = 0;
        public static int totalDeadCnt = 0;
        public static int maxCpu = 0;
        public static int maxMem = 0;
        public static string beginDt = "";
        public static string endDt = "";

        public static bool DEBUG = false;
        public static bool BATCH = false;
        public static bool RANGE = false;
        /// <summary>
        /// 생성자 프로그램이 로딩되면서 초기화를 시킴
        /// </summary>
        Program()
        {
        }
        static int Main(string[] args)
        {
            if (!HandleArgs(args)) return 1;

            Log.WriteLine("Program Start..");

            //메인 프로그램의 메인 객체 생성
            //프로그램 시작 시간 체크
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            // 받아온 content데이터를 html로 저장할것인가..(1이면 저장, 아니면 저장안함)
            string isSaveProxy = ConfigurationSettings.AppSettings["SaveProxy"];
            if (isSaveProxy == "1")
            {
                //프록시 xml 파일 생성
                Proxy p = new Proxy();
                p.ProxyFileSave();
            }

            ChannelName = new ArrayList();

            DeadLinkProc deadlink = new DeadLinkProc();
            deadlink.DoRun(args[0], args[1], args[2]);

            stopWatch.Stop();
            TimeSpan timeSpan = stopWatch.Elapsed;
            Console.WriteLine(timeSpan.TotalSeconds.ToString() + "초 소요되었습니다");

            Console.WriteLine(">> 총 사용 메모리 : {0} Byte", GC.GetTotalMemory(true).ToString("000,000"));
            Console.WriteLine(totalCnt + "건이 실행됨");
            Console.WriteLine(totalDeadCnt + "건이 데드링크 처리됨");

            Log.WriteLine("Program End..");
            return 0;
        }

        private static bool HandleArgs(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: DeadLinkSolution [Limit] [OrderBy] [datefromnow]");
                return false;
            }

            if (args[0].Equals("-debug"))
            {
                DEBUG = true;
                Console.WriteLine("DEBUG 모드입니다.");
            }

            return true;
        }
    }
}


