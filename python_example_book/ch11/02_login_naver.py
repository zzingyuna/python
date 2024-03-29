#!/usr/bin/env python3

import re
import requests
from pprint import pprint
from selenium import webdriver
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC

def naver_login(nid, npw):
    naver_url = 'https://github.com/login'
    
    driver = webdriver.PhantomJS()                             #<- selenum을 제어하기 위해 드라이버를 하나 만든다
    #driver = webdriver.Firefox();
    
    driver.get(naver_url)                                      #<- 접근할 사이트 주소 입력 
    driver.set_window_size(1024,768)

    # selenimum manual
    # 주로 CSS selection을 사용
    # http://selenium-python.readthedocs.io/navigating.html
    text_id = driver.find_element_by_css_selector('#login_field')
    text_id.send_keys(nid)
    
    text_pw = driver.find_element_by_css_selector("#password")
    text_pw.send_keys(npw)
    
    bt_login = driver.find_element_by_css_selector('input.btn-block')
    bt_login.click()                                           #<- 로그인 버튼을 누른다


    # 네이버 본 화면으로 넘어갈 때까지 기다린다.
    # time.sleep(2)로 얼마간 기다려도 된다.
    # http://selenium-python.readthedocs.io/waits.html
    wait = WebDriverWait(driver, 10)
    element = wait.until(EC.title_is('GitHub'))

    # 쿠키를 dict 타입으로 간략화 한다.
    cookies = {}
    for c in driver.get_cookies():
        cookies[c['name']] = c['value']
        
    driver.close()
    return cookies

user_id = '아디'
user_pw = '비번'

# login 후 쿠키값을 가져온다.
cookies = naver_login(user_id, user_pw)
print(cookies)