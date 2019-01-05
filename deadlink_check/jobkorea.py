import requests
from bs4 import BeautifulSoup
import re
import time
import random


url = "http://www.jobkorea.co.kr/Recruit/GI_Read/21755484?rPageCode=SL"
i = 0

while i > -1:

	if i % 2 == 0:
		url = "http://www.jobkorea.co.kr/Recruit/GI_Read/23755484?rPageCode=SL"
	else:
		url = "http://www.jobkorea.co.kr/Recruit/GI_Read/21755484?rPageCode=SL"

	time.sleep(random.randrange(3,10))

	html = requests.get(url).text
	soup = BeautifulSoup(html, 'html.parser')

	liveNotis = soup.findAll(id="devApplyBtn")
	if len(liveNotis) > 0:
		print("live..")
		#print(jobDetailStr)
	else:
		print("dead link..")

	i = i + 1


"""
file = open("C:/study/python/{}result.html".format(i), "wb")
file.write(jobDetailStr.decode("utf-8"))
file.close();
"""
	
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
