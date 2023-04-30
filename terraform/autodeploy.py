import os, time

region = 'asia-east2'
zone = f'{region}-a'
dbregion = 'asia-southeast1'


def config_provider(projectid, region, zone):
    with open('provider.tf', 'r') as file:
        data = file.read()
    data = data.replace('<PROJECT_ID>', projectid)
    data = data.replace('<REGION>', region)
    data = data.replace('<ZONE>', zone)
    with open('first/provider.tf', 'w') as file:
        file.write(data)
    os.system('cp first/provider.tf second/provider.tf')
    os.system('cp first/provider.tf third/provider.tf')


def config_terraform(filename, folder, projectid):
    with open(filename) as file:
        data = file.read()
    data = data.replace('<PROJECT_ID>', projectid)
    with open(f'{folder}{filename}', 'w') as file:
        file.write(data)


projectid = os.environ['DEVSHELL_PROJECT_ID']
config_provider(projectid, region, zone)
projectid = "qwheb128376123"
config_terraform('firebase.tf', 'first/', projectid)
config_terraform('dialogflow.tf', 'first/', projectid)
config_terraform('cloudfunction.tf', 'second/', projectid)
config_terraform('staticwebsite.tf', 'third/', projectid)
os.chdir('first')
os.system('terraform init && terraform apply && \
    cp firebase_admin_key.json ../request/ && \
    cp firebase_database_url.txt ../request/')
os.chdir('../nodejs')
os.system('npm install firebase-admin --save')
os.system('node firebase_db_setrule.js')
os.chdir('../request')
os.system('zip request.zip *')
os.chdir('../second')
os.system('terraform init && terraform apply')
with open('request-function-url.txt', 'r') as file:
    url = file.read()
print(url)
with open('../../static-website/index.js', 'r+b') as file:
    data = file.read().decode()
    file.seek(0)
    data = data.replace('<FUNCTION_AUTH>', url)
    file.write(data.encode())
    file.truncate()
os.chdir('../third')
os.system('terraform init && terraform apply')
os.chdir('chmod +x ../autodelete.sh')