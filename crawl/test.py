import requests
from bs4 import BeautifulSoup


req = requests.get("https://pg.taleo.net/careersection/10000/jobsearch.ftl?lang=en&portal=640200078&searchtype=3&f=null&LOCATION=200002540&s=4")
#print(req.text)

file = open("D:/crawl/result/result.html", "wb")
file.write(req.text.encode('utf-8'))
file.close()

html = BeautifulSoup(req.text, "html.parser")
#print(html)
