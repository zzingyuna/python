import os
import os.path

"""
walk()함수는 주어진 디렉터리를 시작으로 내부에 있는 모든 디렉토리와 파일들을 찾는다
// walk(top, topdown=true, onerror=none, floowlinks=false): ///
top은 조사할 디렉터리 지정
topdown은 파일과 디렉토리를 반환하는 순서 결정 (true:상위디렉토리->하위드렉토리, false:하위디렉토리->상위디렉토리)

코드2~3: 디렉토리 경로
코드4~5: 파일들 출력
"""

# ~/logs 디렉토리를 조회하겠다.
root_dir = os.path.expanduser("~/logs")

# walk 결과는 현재 디렉토리, 내부 디렉토리명들, 파일명들 임.
for root, dirs, files in os.walk(root_dir):     #<---- 1
    # 디렉토리들
    for d in dirs:                              #<---- 2
        print("[D] {}/{}".format(root, d))      #<---- 3
    # 파일들
    for f in files:                             #<---- 4
        print("[F] {}/{}".format(root, f))      #<---- 5
