import os
import google.cloud.dialogflow_v2 as dialogflow
from google.api_core.exceptions import InvalidArgument

os.environ["GOOGLE_APPLICATION_CREDENTIALS"] = r'mc-fqsi-ce300d0ae114.json'
DIALOGFLOW_PROJECT_ID = 'mc-fqsi'
DIALOGFLOW_LANGUAGE_CODE = 'en'
SESSION_ID = 'anything'

while True:
    text_to_be_analyzed = input('say: ')

    if text_to_be_analyzed == 'quit':
        break

    session_client = dialogflow.SessionsClient()
    session = session_client.session_path(DIALOGFLOW_PROJECT_ID, SESSION_ID)
    text_input = dialogflow.types.TextInput(text=text_to_be_analyzed, language_code=DIALOGFLOW_LANGUAGE_CODE)
    query_input = dialogflow.types.QueryInput(text=text_input)
    try:
        response = session_client.detect_intent(session=session, query_input=query_input)
    except InvalidArgument:
        raise

    print("輸入文字:", response.query_result.query_text)
    print("得到的 intent:", response.query_result.intent.display_name)
    print("偵測到 intent 的 confidence:", response.query_result.intent_detection_confidence)
    print("回應的話:", response.query_result.fulfillment_text)