import requests
from bs4 import BeautifulSoup
import re
import time
import random
import json


from suds.client import Client
from suds.xsd.doctor import Import, ImportDoctor
import logging
import base64

url = 'http://gw.job.co.kr/DeadLinkSolutionGW.asmx?WSDL'
imp = Import('http://www.w3.org/2001/XMLSchema')    # the schema to import.
imp.filter.add('http://gw.job.co.kr/DeadLinkSolutionGW.asmx')  # the schema to import into.
d = ImportDoctor(imp)
client = Client(url, doctor=d)

test = "egYELBe4oLcb0sZxUEjhw=="

res = client.service.GetLoadPattern(test)
dataset = res[1]
datatable = dataset[0]
i = 0

# print(datatable[0])
for dt in datatable[0]:
	deadLinkPattern = dt['Dead_Link_Pattern1'].encode('euc-kr')
	sourceSiteNm = dt['SourceSiteNm']
	print(sourceSiteNm)



