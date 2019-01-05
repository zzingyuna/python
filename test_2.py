from urllib2 import urlopen
from urllib2 import HTTPError
from bs4 import BeautifulSoup

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
		themelist = bsObj.findAll("span",{"class":"td_t"})
		for theme in themelist:
			print(theme.encode('euc-kr'))
		test = bsObj.findAll(id="da_access")
		print("test result..")
		print(test)
	except AttributeError as ae:
		return None
	return title

"""
findAll(tag, attributes, recursive, text, limit, keywords)
ex) findAll({"h1","h2","h3"}) - h1, h2, h3 태그를 모두 찾는다
    findAll("span",{"class":{"green","red"}}) - span 태그 중 빨간색과 초록색을 모두 찾는다
	findAll(text="the prince") - 태그안에 'the prince'문자열이 들어간 태그를 모두 찾는다
	findAll(id="testid") - id값을 설정한 태그를 찾는다
find(tag, attributes, recursive, text, keywords)
"""

title = getTitle("http://www.naver.com")
if title == None:
	print("Title could not be found")
else:
	print(title.encode('euc-kr'))
	
getThemecast("http://www.naver.com")

