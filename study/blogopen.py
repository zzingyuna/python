
from urllib2 import urlopen
from bs4 import BeautifulSoup
import requests
import datetime
import random
import re
import sys
import webbrowser
import os
import time

######################################
# 블로그 열기 닫기
######################################


random.seed(datetime.datetime.now())

def getSearchResult(articleUrl):
    html = urlopen(articleUrl)
    bsObj = BeautifulSoup(html, "html.parser")
    mainlist = bsObj.find("div", {"class":"sp_website section"}).findAll("a", re.compile("[http][s]*\w{3}[blog]\w[naver]\w.*$"))
    return bsObj.find("div", {"class":"sp_website section"}).findAll("a", re.compile("[http][s]*\w{3}[blog]\w[naver]\w.*$"))

def getMainFrame(articleUrl):
    html = requests.get(articleUrl).text
    bsObj = BeautifulSoup(html, "html.parser")
    linkUrl = bsObj.find("frame", {"id":"mainFrame"}).attrs["src"]
    return linkUrl

def getBoardList(articleUrl):
    html = requests.get(articleUrl).text
    bsObj = BeautifulSoup(html, "html.parser")
    return bsObj.find("div", {"id":"prologue"}).findAll("a", re.compile("[http][s]*\w*[blog]\w*[naver]\w*[com]\w*[PostView]\w*[nhn]\w*"))

#네이버에서 검색한 결과 페이지 url
links = getSearchResult("https://search.naver.com/search.naver?where=nexearch&sm=top_hty&fbm=1&ie=utf8&query=%EB%8C%80%EB%B0%95%EB%95%85%ED%88%AC%EC%9E%90+%EC%9E%A5%EC%B0%A8%EC%9E%A5")
linksCnt = len(links)
while linksCnt > 0:
    newArticle = links[len(links)-1].attrs["href"]
    sublink = getMainFrame(newArticle)
    boardlist = getBoardList(sublink)
    boardlistCnt = len(boardlist)
    linksCnt = linksCnt - 1
    while boardlistCnt > 0:
        blog = boardlist[boardlistCnt-1].attrs["href"]
        boardlistCnt = boardlistCnt - 1
        webbrowser.open_new(blog) #블로그 페이지 열기
        '''
        f = open('memo.txt', 'a')
        f.write(blog)
        f.write('\n')
        f.close()
        '''

time.sleep(30)
browserExe = "chrome.exe"
os.system("taskkill /f /im "+browserExe) #모든 열린 크롬창을 닫아준다
