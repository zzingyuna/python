import os
import os.path

# ~/logs 디렉토리를 조회하겠다.
root_dir = os.path.expanduser("~/logs")

"""
dirs는 현재 디렉토리에 포함되어있는 디렉토리이자 다음번에 조사할 디렉토리 값.
이 값에서 검색하지 않은 디렉토리를 선택해서 삭제하면 해당 디렉토리는 조사 대상에서 빠진다
아래 코드에서 'apache' 디렉토리는 제거하고 출력한다
"""

# walk 결과는 현재 디렉토리, 내부 디렉토리명들, 파일명들 임.
for root, dirs, files in os.walk(root_dir):
    # dirs에서 apache로 되어 있는 부분들 제거한다.
    # 그러면 apache 디렉토리를 조사하지 않는다.
    if 'apache' in dirs:                #<---- 1 
        dirs.remove('apache')           #<---- 2

    # 디렉토리들
    for d in dirs:
        print("[D] {}/{}".format(root, d))
    # 파일들
    for f in files:
        print("[F] {}/{}".format(root, f))
