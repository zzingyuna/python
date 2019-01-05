import os, stat
import shutil
import subprocess

def backup_filename(prefix):
    '''백업 파일 이름 생성.

    파일 형식: <prefix>_날짜

    Usage:
    >>> backup_filename('test')
    test_20160101
	
	코드1: 파일 이름을 생성하는 함수를 호출
	코드2: tar파일에 들어갈 파일들이 있는 디렉토리 지정
	코드3: base_dir은 root_dir 기준으로 압축할 서브 디텍토리 지정(생략시 root_dir과 동일하게 설정된 것으로 간주)
	
	
	make_archive(백업, 타입)
	타입 종류
	zip - .zip
	tar - .tar
	gztar - .tar .gz
	bztar - .tar .bz
	xztar - .tar .xz
    '''

    from datetime import date
    today = date.today()
    return prefix + today.strftime('_%Y%m%d') #파일명은 날짜를 이용해서 만든다(중복방지)

backup_fname = backup_filename('nginx-log')  #<---- 1
root_dir = os.path.expanduser('~/logs')      #<---- 2
shutil.make_archive(backup_fname, 'gztar'
, root_dir=root_dir
, base_dir='nginx/') #<---- 3

# 생성된 tar 파일 목록 보기
print('file name : ', backup_fname)  
subprocess.call('tar -tzvf {}.tar.gz'.format(backup_fname)
                , shell=True) #<--- 4
