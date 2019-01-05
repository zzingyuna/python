from os.path import basename, dirname
python_path = '/usr/local/bin/python3'

print("python3 is loaded at: ")
print("   DIR : {}".format(dirname(python_path)))
print("   FILE: {}".format(basename(python_path)))

"""
실행결과 
DIR : /user/local/bin
FILE : python3
"""
