using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadLinkSolution
{
    public class SiteInfoModel
    {
        public int SeqNo { get; set; }
        public string FileNm { get; set; }
        public string PcIp { get; set; }
        public string PcNm { get; set; }
        public string PathNm { get; set; }
        public string SourceSiteNm { get; set; }
        public int StopAfterNo { get; set; }
        public int CategoryNm { get; set; }
        public DateTime ClStartDt { get; set; }
        public DateTime ClEndDt { get; set; }
        public int SetStopAfterNo { get; set; }
        public string DeadlinkPattern1 { get; set; }
        public string DeadlinkPattern2 { get; set; }
        public string DeadlinkIdleTime { get; set; }
        public int IpBlockYN { get; set; }
    }
}
