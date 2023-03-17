import os
from google.cloud import dialogflow_v2 as dialogflow
from google.api_core.exceptions import InvalidArgument

import winsound

#os.environ["GOOGLE_APPLICATION_CREDENTIALS"] = r'mc-fqsi-ce300d0ae114.json'
#DIALOGFLOW_PROJECT_ID = 'mc-fqsi'
os.environ["GOOGLE_APPLICATION_CREDENTIALS"] = r'jerryagent_k--ueeq-4f8197f294d6.json'
DIALOGFLOW_PROJECT_ID = 'k--ueeq'
# 改改語言 'zh-tw' or 'en'
DIALOGFLOW_LANGUAGE_CODE = 'zh-hk'
SESSION_ID = 'anything'
# 改錄音時間 (秒)
RECORD_DURATION = 3.0

print('[Start Recording]')

def detect_intent_audio(project_id, session_id, audio_file_path, language_code):
    """Returns the result of detect intent with an audio file as input.
    Using the same `session_id` between requests allows continuation
    of the conversation."""
    #from google.cloud import dialogflow

    session_client = dialogflow.SessionsClient()

    # Note: hard coding audio_encoding and sample_rate_hertz for simplicity.
    audio_encoding = dialogflow.AudioEncoding.AUDIO_ENCODING_LINEAR_16
    sample_rate_hertz = 16000

    session = session_client.session_path(project_id, session_id)
    print("Session path: {}\n".format(session))

    with open(audio_file_path, "rb") as audio_file:
        input_audio = audio_file.read()

    audio_config = dialogflow.InputAudioConfig(
        audio_encoding=audio_encoding,
        language_code=language_code,
        sample_rate_hertz=sample_rate_hertz,
    )
    query_input = dialogflow.QueryInput(audio_config=audio_config)

    output_audio_config = dialogflow.OutputAudioConfig(
        audio_encoding=dialogflow.OutputAudioEncoding.OUTPUT_AUDIO_ENCODING_LINEAR_16,
        synthesize_speech_config=dialogflow.SynthesizeSpeechConfig(
            speaking_rate=0.8,
            pitch=3.0,
            voice=dialogflow.VoiceSelectionParams(
                name='yue-HK-Standard-A'
            )
        )
    )

    request = dialogflow.DetectIntentRequest(
        session=session,
        query_input=query_input,
        input_audio=input_audio,
        output_audio_config=output_audio_config
    )
    response = session_client.detect_intent(request=request)

    print("=" * 20)
    print("Query text: {}".format(response.query_result.query_text))
    print(
        "Detected intent: {} (confidence: {})\n".format(
            response.query_result.intent.display_name,
            response.query_result.intent_detection_confidence,
        )
    )
    print("Fulfillment text: {}\n".format(response.query_result.fulfillment_text))

    with open("output.wav", "wb") as out:
        out.write(response.output_audio)
        print('Audio content written to file "output.wav"')

    winsound.PlaySound('output.wav', winsound.SND_FILENAME)


from recorder import Recorder
r = Recorder(channels=1, rate=16000)
np_data = r.record(RECORD_DURATION, output='out.wav')
detect_intent_audio(DIALOGFLOW_PROJECT_ID, SESSION_ID, 'out.wav', DIALOGFLOW_LANGUAGE_CODE)
exit(0)

while True:
    text_to_be_analyzed = input('say: ')

    if text_to_be_analyzed == 'quit':
        break

    # Set the query parameters with sentiment analysis
    output_audio_config = dialogflow.OutputAudioConfig(
        audio_encoding=dialogflow.OutputAudioEncoding.OUTPUT_AUDIO_ENCODING_LINEAR_16,
        synthesize_speech_config=dialogflow.SynthesizeSpeechConfig(
            speaking_rate=0.75,
            pitch=3.0,
            voice=dialogflow.VoiceSelectionParams(
                name='yue-HK-Standard-A'
            )
        )
    )

    session_client = dialogflow.SessionsClient()
    session = session_client.session_path(DIALOGFLOW_PROJECT_ID, SESSION_ID)
    text_input = dialogflow.types.TextInput(text=text_to_be_analyzed, language_code=DIALOGFLOW_LANGUAGE_CODE)
    query_input = dialogflow.types.QueryInput(text=text_input)

    request = dialogflow.DetectIntentRequest(
        session=session,
        query_input=query_input,
        output_audio_config=output_audio_config,
    )

    try:
        response = session_client.detect_intent(request=request)
    except InvalidArgument:
        raise

    print("輸入文字:", response.query_result.query_text)
    print("得到的 intent:", response.query_result.intent.display_name)
    print("偵測到 intent 的 confidence:", response.query_result.intent_detection_confidence)
    print("回應的話:", response.query_result.fulfillment_text)

    with open("output.wav", "wb") as out:
        out.write(response.output_audio)
        print('Audio content written to file "output.wav"')

    winsound.PlaySound('output.wav', winsound.SND_FILENAME)
    break