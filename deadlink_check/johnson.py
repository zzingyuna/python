import requests
from bs4 import BeautifulSoup
import re

# html 태그를 가져오는 메소드
def getHtmltags(linkStr):
	htmlStr = requests.get(linkStr).text
	return htmlStr


html = getHtmltags("https://jobs.jnj.com/jobs?page=1&location=Korea%2C+Republic+of")
jobs = re.search('window.JOBS = \[\{(.*?)\}\];', html)
#print(jobs.group(0))

i = 0


sublinks = re.findall('"apply_url":"(.*?)"', jobs.group(0))
for sublink in sublinks:
	#print(sublink)
	subhtml = getHtmltags(sublink)
	jobDetail = re.search('id="initialHistory" value="(.*?)"', subhtml).group(0).replace("id=\"initialHistory\" value=","").replace("\"","").replace("\"","")
	#jobDetailStr = jobDetail.replace("%22","\"").replace("%26","&").replace("%2F","/").replace("%3B", ";")
	jobDetailStr = jobDetail

	#print(jobDetailStr)
	i= i+1
	


	
	file = open("C:/study/python/{}result.html".format(i), "wb")
	file.write(jobDetailStr.decode("utf-8"))
	file.close();
	
	
"""
	titles = re.findall('"title":"(.*?)"', jobDetail)
	strTitle = ""
	for title in titles:
		print(title)
		strTitle = title
	
	posteds = re.findall('"datePosted":"(.*?)"', jobDetail)
	strPosted = ""
	for posted in posteds:
		print(posted)
		strPosted = posted

	content = "title : " + strTitle + " <br/> Post date : " + strPosted

	file = open("C:/study/python/{}result.html".format(i), "wb")
	file.write(content.encode("utf-8"))
	file.close();

"""



"""
file = open("C:/study/python/result.html", "wb")
file.write(html.encode("utf-8"))
file.close();
"""
