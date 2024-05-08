FROM python:3.12

WORKDIR /app
COPY requirements.lock .
RUN pip install -r requirements.lock

COPY . .

CMD ["python", "main.py"]
