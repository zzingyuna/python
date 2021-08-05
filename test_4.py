import re

class SCDObj:
    title = ''
    docid = ''
    content = ''
    imageurl = ''
    pass

tag = ['DOCID', 'TITLE'];
listall = [];

f = open("C:\\Users\\yuna\\Desktop\\test\\test1.SCD", 'r', encoding='UTF8')
p = re.compile("\<[A-Z]*\>")
lines = f.readlines()
e = SCDObj()
lastfield = ''
for line in lines:
    if line.find('<DOCID>') != -1 :
        if len(e.docid) > 0 :
            listall.append(e)
            e = SCDObj()
        
        e.docid = line.replace('<DOCID>', '')
        lastfield = 'DOCID'
    elif line.find('<TITLE>') != -1 :
        e.title = line.replace('<TITLE>', '')
        lastfield = 'TITLE'
    elif line.find('<CONTENT>') != -1 :
        e.content = line.replace('<CONTENT>', '')
        lastfield = 'CONTENT'
    elif line.find('<WRITER>') != -1 :
        e.writer = line.replace('<WRITER>', '')
        lastfield = 'WRITER'
    elif p.match(line) == None:
        if lastfield == "CONTENT":
            e.content = e.content + line
    # print(p.match(line))

if len(e.docid) > 0 :
    listall.append(e)

f.close()


for item in listall:
    print(item.content)
    print('______________')
    