class SCDObj:
    title = ''
    docid = ''
    content = ''
    imageurl = ''
    pass

tag = ['DOCID', 'TITLE'];
listall = [];

f = open("C:\\Users\\yuna\\Desktop\\test\\test1.SCD", 'r', encoding='UTF8')

lines = f.readlines()
e = SCDObj();
for line in lines:
    if line.find('<DOCID>') != -1 :
        if len(e.docid) > 0 :
            listall.append(e)
        
        e.docid = line.replace('<DOCID>', '')
    elif line.find('<TITLE>') != -1 :
        e.title = line.replace('<TITLE>', '')
    #print(line)

f.close()


for item in listall:
    print(item.title)
    