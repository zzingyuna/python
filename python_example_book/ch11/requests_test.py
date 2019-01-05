"""
헤더값 변경하기
"""
import requests
headers ={'user-agent':'my-app/0.0.1'}
r = requests.get('https://en.wikipedia.org/wiki/Main_Page', headers=headers)
print(r.text)