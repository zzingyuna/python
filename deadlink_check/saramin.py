import requests
from bs4 import BeautifulSoup
import re
import time
import random


url = "http://www.saramin.co.kr/zf_user/jobs/relay/recruit-view?view_type=temp&rec_idx=32828764&gz=1&recommend_ids=eJxNz7ENxGAIg9FprrchgF3fILf%2FFhel%2BEn59AESGU4X%2FavmZ74ZihLncCBnvqtZW0VgqQHAw7YR2lr3rR0mObovOx%2F6AoRDXdEdu9uEr0OjUL01OwtL1fh54Q%2FVCS%2Fm&t_ref=area_recruit&t_ref_content=general#seq=0"
i = 0

while i > -1:

	if i % 2 == 0:
		print('live go->')
		url = "http://www.saramin.co.kr/zf_user/jobs/relay/recruit-view?view_type=temp&rec_idx=32939520&gz=1&recommend_ids=eJxNz7ENxGAIg9FprrchgF3fILf%2FFhel%2BEn59AESGU4X%2FavmZ74ZihLncCBnvqtZW0VgqQHAw7YR2lr3rR0mObovOx%2F6AoRDXdEdu9uEr0OjUL01OwtL1fh54Q%2FVCS%2Fm&t_ref=area_recruit&t_ref_content=general#seq=0"
	elif i % 3 == 0:
		print('delete go->')
		url = "http://www.saramin.co.kr/zf_user/jobs/relay/recruit-view?view_type=temp&rec_idx=11008764&gz=1&recommend_ids=eJxNz7ENxGAIg9FprrchgF3fILf%2FFhel%2BEn59AESGU4X%2FavmZ74ZihLncCBnvqtZW0VgqQHAw7YR2lr3rR0mObovOx%2F6AoRDXdEdu9uEr0OjUL01OwtL1fh54Q%2FVCS%2Fm&t_ref=area_recruit&t_ref_content=general#seq=0"
	else:
		print('dead link go->')
		url="http://www.saramin.co.kr/zf_user/jobs/relay/recruit-view?view_type=temp&rec_idx=32619125&gz=1&recommend_ids=eJxNz7ENxGAIg9FprrchgF3fILf%2FFhel%2BEn59AESGU4X%2FavmZ74ZihLncCBnvqtZW0VgqQHAw7YR2lr3rR0mObovOx%2F6AoRDXdEdu9uEr0OjUL01OwtL1fh54Q%2FVCS%2Fm&t_ref=area_recruit&t_ref_content=general#seq=0"

	time.sleep(random.randrange(3,10))

	html = requests.get(url).text
	soup = BeautifulSoup(html, 'html.parser')

	liveNotis = soup(text=re.compile(r"_VIEW_MTRX_"))
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
