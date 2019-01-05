from PIL import Image
import subprocess

def cleanFile(filePath, newFilePath):
    image = Image.open(filePath)

    # 회색 임계점을 설정하고 이미지를 저장합니다.
    image = image.point(lambda x: 0 if x<143 else 255)
    image.save(newFilePath)

    # 새로 만든 이미지를 테서렉트로 읽습니다.
    subprocess.call(["tesseract", newFilePath, "output"], shell=True)

    # 결과 데이터 파일을 열어 읽습니다.
    outputFile = open("output.txt", 'r')
    print(outputFile.read())
    outputFile.close()

cleanFile("text_2.tif", "text_2_clean.png")
