
from suds.client import Client
from suds.xsd.doctor import Import, ImportDoctor
import logging
import base64

######################################
# 파이썬에서 웹서비스 호출(.asmx)
######################################


url = 'http://도메인/DeadLinkSolutionGW.asmx?WSDL'
imp = Import('http://www.w3.org/2001/XMLSchema')    # the schema to import.
imp.filter.add('http://도메인/DeadLinkSolutionGW.asmx')  # the schema to import into.
d = ImportDoctor(imp)
client = Client(url, doctor=d)

tokenKey = "토큰키값"

res = client.service.GetLoadPattern(tokenKey)
dataset = res[1]
datatable = dataset[0]
i = 0

for dt in datatable[0]:
	deadLinkPattern = dt['Dead_Link_Pattern1'].encode('euc-kr')
	sourceSiteNm = dt['SourceSiteNm']
	print(sourceSiteNm)

