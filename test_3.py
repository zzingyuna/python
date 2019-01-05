from urllib2 import urlopen
from urllib2 import HTTPError
from bs4 import BeautifulSoup
import re

"""
설명..
findAll(tag, attributes, recursive, text, limit, keywords)
ex) findAll({"h1","h2","h3"}) - h1, h2, h3 태그를 모두 찾는다
    findAll("span",{"class":{"green","red"}}) - span 태그 중 빨간색과 초록색을 모두 찾는다
	findAll(text="the prince") - 태그안에 'the prince'문자열이 들어간 태그를 모두 찾는다
	findAll(id="testid") - id값을 설정한 태그를 찾는다
find(tag, attributes, recursive, text, keywords)

lxml : html과 xml 문서를 모두 파싱할 수 있다
http://lxml.de/
HTML 파서 : 파이선에 내장된 파싱 라이브러리
https://docs.python.org/3/library/html.parser.html
"""
def getTitle(url):
	try:
		html = urlopen(url)
	except HTTPError as e:
		return None
	
	try:
		bsObj = BeautifulSoup(html.read(), "html.parser")
		title = bsObj.body.h1 #tag 객체를 바로 얻어오기
	except AttributeError as ae:
		return None
	return title

def getThemecast(url):
	try:
		html = urlopen(url)
	except HTTPError as e:
		return None
	
	try:
		bsObj = BeautifulSoup(html.read(), "html.parser")
		#themelist = bsObj.findAll("span",{"class":"td_t"}) - 'td_t'를 클래스로 지정된 span 태그들 모두 가져온다
		#themelist = bsObj.find("table",{"id":"giftList"}).children - id='giftList' 값을 가진 테이블의 자식 태그들을 가져온다
		#themelist = bsObj.find("table",{"id":"giftList"}).tr.next_siblings  - id='giftList' 값을 가진 테이블의 tr 형제 태그들을 가져온다
		themelist = bsObj.findAll("img")
		for theme in themelist:
			print(theme)
			print(theme.attrs['src'])
		
	except AttributeError as ae:
		return None
	
def getParentNode(url):
	try:
		html = urlopen(url)
	except HTTPError as e:
		return None
	
	try:
		bsObj = BeautifulSoup(html.read(), "html.parser")
		themelist = bsObj.find("img",{"src":"../img/gifts/img2.jpg"}).parent.previous_sibling.get_text()
		# 부모태그에서 형제 태그의 값을 가져온다
		print(themelist)
	except AttributeError as ae:
		return None
	
def getRamda(url):
	try:
		html = urlopen(url)
	except HTTPError as e:
		return None
	
	try:
		bsObj = BeautifulSoup(html.read(), "html.parser")
		result = bsObj.findAll(lambda tag: len(tag.attrs) == 2)
		# attr이 두개 들어있는 태그들을 찾아낸다
		print(result)
	except AttributeError as ae:
		return None


title = getTitle("http://www.naver.com")
if title == None:
	print("Title could not be found")
else:
	print(title.encode('euc-kr'))

getThemecast("http://www.pythonscraping.com/pages/page3.html")

getParentNode("http://www.pythonscraping.com/pages/page3.html")

getRamda("http://www.pythonscraping.com/pages/page3.html")
