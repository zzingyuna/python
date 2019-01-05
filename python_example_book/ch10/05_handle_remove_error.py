import os, stat
import shutil

"""
삭제하고자 하는 파일 원한 문제로 삭제하지 못했을때
속성을 변경해서 다시 한번 삭제를 시도하는 소스
"""

def remove_readonly(func, path, _):
    "Clear the readonly bit and reattempt the removal"
    os.chmod(path, stat.S_IWRITE)
    func(path)

shutil.rmtree(directory, onerror=remove_readonly)
