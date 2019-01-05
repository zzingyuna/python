import requests
from bs4 import BeautifulSoup
import re
import time
import random


url = "http://job.incruit.com/jobdb_info/jobpost.asp?job=1791170005175"
i = 0

while i > -1:

	if i % 2 == 0:
		print('live go->')
		url = "http://job.incruit.com/jobdb_info/jobpost.asp?job=1801170005175"
	elif i % 3 == 0:
		print('delete go->')
		url = "http://job.incruit.com/jobdb_info/jobpost.asp?job=1801145009999"
	else:
		print('dead link go->')
		url="http://job.incruit.com/jobdb_info/jobpost.asp?job=1801170000006"

	time.sleep(random.randrange(3,10))

	html = requests.get(url).text
	soup = BeautifulSoup(html, 'html.parser')

	liveNotis = soup.findAll(id='btnMidApply')
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
