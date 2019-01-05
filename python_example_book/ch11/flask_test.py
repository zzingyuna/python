from flask import Flask
app = Flask(__name__)

@app.rout("/")
def hello():
    return "hello world"

if __name__ == "__name__":
	app.run()