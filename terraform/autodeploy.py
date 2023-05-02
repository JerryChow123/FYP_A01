import os, time

region = 'asia-east2'
zone = f'{region}-a'
dbregion = 'asia-southeast1'

DATABASE_URL = ""
REQUEST_URL = ""
SPEECH_TEXT_URL = ""
TEXT_SPEECH_URL = ""
VISIONAI_URL = ""
DIALOGFLOW_URL = ""


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


def config_py(filename, target, replacement):
    with open(filename, 'r+b') as file:
        data = file.read().decode()
        data = data.replace(target, replacement)
        file.seek(0)
        file.write(data.encode())


projectid = os.environ['DEVSHELL_PROJECT_ID']
os.system('mkdir first second third')
config_provider(projectid, region, zone)
projectid = "3n1jn154512"
config_terraform('firebase.tf', 'first/', projectid)
config_terraform('dialogflow.tf', 'first/', projectid)
config_terraform('cloudfunction.tf', 'second/', projectid)
config_terraform('staticwebsite.tf', 'third/', projectid)
os.system('cp request_main.py ../cloud-functions/request/main.py && \
    cp dialogflowbot_main.py ../cloud-functions/dialogflowbot/main.py && \
    cp staticweb_index.js ../static-web/index.js')
os.chdir('first')
os.system('terraform init && terraform apply && \
    cp firebase_admin_key.json ../../cloud-functions/request/ && \
    cp dialogflow_admin_key.json ../../cloud-functions/dialogflowbot/')
with open('firebase_database_url.txt', 'r') as file:
    DATABASE_URL = file.read()
os.chdir('../nodejs')
os.system('npm install firebase-admin --save')
os.chdir('../../cloud-functions/request')
config_py('main.py', '<DATABASE_URL>', DATABASE_URL)
os.system('zip request.zip *')
os.chdir('../speechtext')
os.system('zip speechtext.zip *')
os.chdir('../textspeech')
os.system('zip textspeech.zip *')
os.chdir('../visionai')
os.system('zip visionai.zip *')
os.chdir('../dialogflowbot')
config_py('main.py', '<PROJECT_ID>', projectid)
os.system('zip dialogflowbot.zip *')
os.chdir('../../terraform/second')
os.system('terraform init && terraform apply')
with open('request_url.txt', 'r') as file:
    REQUEST_URL = file.read()
with open('speechtext_url.txt', 'r') as file:
    SPEECH_TEXT_URL = file.read()
with open('textspeech_url.txt', 'r') as file:
    TEXT_SPEECH_URL = file.read()
with open('visionai_url.txt', 'r') as file:
    VISIONAI_URL = file.read()
with open('dialogflowbot_url.txt', 'r') as file:
    DIALOGFLOW_URL = file.read()
with open('../../static-web/index.js', 'r+b') as file:
    data = file.read().decode()
    file.seek(0)
    data = data.replace('<FUNCTION_AUTH>', REQUEST_URL)
    file.write(data.encode())
    file.truncate()
os.chdir('../third')
os.system('terraform init && terraform apply && \
    chmod +x ../autodelete.sh')

config = open('../../configure.ini', 'w')
config.write(f'REQUEST={REQUEST_URL}\n')
config.write(f'SPEECH_TEXT={SPEECH_TEXT_URL}\n')
config.write(f'TEXT_SPEECH={TEXT_SPEECH_URL}\n')
config.write(f'VISIONAI={VISIONAI_URL}\n')
config.write(f'DIALOGFLOW={DIALOGFLOW_URL}')
config.close()

os.chdir('../nodejs/')
os.system('node firebase_db_setrule.js')
print('wait for FireBase Rule...')
import time
time.sleep(5)
os.system('node firebase_db_root.js')