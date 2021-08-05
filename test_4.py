import re
import os

path = "C:\\Users\\yuna\\Desktop\\test"
file_list = os.listdir(path)


class SCDObj:
    title = ''
    docid = ''
    content = ''
    imageurl = ''
    pass

tag = ['DOCID', 'TITLE'];
listall = [];


def read_write(file):
    if not(file.endswith(".SCD")):
        return
    
    f = open("C:\\Users\\yuna\\Desktop\\test\\"+file, 'r', encoding='UTF8')
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
            fa = open("C:\\Users\\yuna\\Desktop\\test\\DOCID.txt", 'a', encoding='UTF8')
            fa.write(e.docid)
            fa.write('____________________________________\n')
            fa.close();
        elif line.find('<TITLE>') != -1 :
            e.title = line.replace('<TITLE>', '')
            lastfield = 'TITLE'
            fa = open("C:\\Users\\yuna\\Desktop\\test\\TITLE.txt", 'a', encoding='UTF8')
            fa.write(e.title)
            fa.write('____________________________________\n')
            fa.close();
        elif line.find('<CONTENT>') != -1 :
            e.content = line.replace('<CONTENT>', '')
            lastfield = 'CONTENT'
            fa = open("C:\\Users\\yuna\\Desktop\\test\\CONTENT.txt", 'a', encoding='UTF8')
            fa.write('____________________________________\n')
            fa.write(e.content)
            fa.close();
        elif line.find('<WRITER>') != -1 :
            e.writer = line.replace('<WRITER>', '')
            lastfield = 'WRITER'
            fa = open("C:\\Users\\yuna\\Desktop\\test\\WRITER.txt", 'a', encoding='UTF8')
            fa.write(e.writer)
            fa.write('____________________________________\n')
            fa.close();
        elif p.match(line) == None:
            if lastfield == "CONTENT":
                e.content = e.content + line
                fa = open("C:\\Users\\yuna\\Desktop\\test\\CONTENT.txt", 'a', encoding='UTF8')
                fa.write(line)
                fa.close();
        # print(p.match(line))

    if len(e.docid) > 0 :
        listall.append(e)

    f.close()
    return listall;

def print_result():
    for item in listall:
        print(item.content)
        print('______________')
    return;


for file in file_list:
    read_write(file)

print_result()