import time
from urllib.request import urlretrieve
import subprocess
from selenium import webdriver

# 셀레니움 드라이버를 만듭니다.
#driver = webdriver.PhantomJS()
# 가끔 팬텀JS가 이 페이지에 있는 요소를 찾아내지 못할 때가 있습니다.역주
# 그런 일이 발생한다면 다음 행의 주석을 제거해 셀레니움 대신 크롬드라이버를 쓰세요.
driver = webdriver.Chrome()

driver.get(
    "http://www.amazon.com/Alice-Wonderland-Large-Lewis-Carroll/dp/145155558X")
time.sleep(2)

# 책 미리보기 버튼을 클릭합니다.
driver.find_element_by_id("sitbLogoImg").click()
imageList = set()

# 페이지 로드를 기다립니다.
time.sleep(5)

# 오른쪽 화살표를 클릭할 수 있으면 계속 클릭해서 페이지를 넘깁니다.
while "pointer" in  driver.find_element_by_id(
        "sitbReaderRightPageTurner").get_attribute("style"):
    driver.find_element_by_id("sitbReaderRightPageTurner").click()
    time.sleep(2)
    # 새로 불러온 페이지를 가져옵니다. 한 번에 여러 페이지를 불러올 때가 있지만,
    # 세트에는 중복된 페이지가 들어가지는 않습니다.
    pages = driver.find_elements_by_xpath("//div[@class='pageImage']/div/img")
    for page in pages:
        image = page.get_attribute("src")
        imageList.add(image)

driver.quit()

# 수집된 이미지를 테서렉트로 처리합니다.
i = 0
for image in sorted(imageList):
    urlretrieve(image, "page"+str(i)+".jpg")
    p = subprocess.Popen(["tesseract", "page"+str(i)+".jpg", "page"+str(i)],
                             stdout=subprocess.PIPE,stderr=subprocess.PIPE)
    p.wait()
    f = open("page"+str(i)+".txt", "r", encoding="utf-8")
    print(f.read())
    i += 1
