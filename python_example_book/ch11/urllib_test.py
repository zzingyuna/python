from urllib.request import urlopen
html = urlopen('https://en.wikipedia.org/wiki/Main_Page')
doc = html.read().decode('utf-8')
print(doc)