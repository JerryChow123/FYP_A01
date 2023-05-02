from google.cloud import dialogflow_v2 as dialogflow
from google.oauth2.service_account import Credentials
from flask import make_response, send_file
from io import BytesIO
import base64

DIALOGFLOW_PROJECT_ID = 'chatbot-<PROJECT_ID>'
DIALOGFLOW_LANGUAGE_CODE = 'zh-hk'
SESSION_ID = 'anything'
RECORD_DURATION = 3.0

key = 'dialogflow_admin_key.json'


def detect_intent_audio(audio_file_path=None, audio_byte=None,
        project_id=DIALOGFLOW_PROJECT_ID, session_id=SESSION_ID, language_code=DIALOGFLOW_LANGUAGE_CODE):
    """Returns the result of detect intent with an audio file as input.
    Using the same `session_id` between requests allows continuation
    of the conversation."""

    session_client = dialogflow.SessionsClient(
        credentials=Credentials.from_service_account_file(key)
    )

    # Note: hard coding audio_encoding and sample_rate_hertz for simplicity.
    audio_encoding = dialogflow.AudioEncoding.AUDIO_ENCODING_LINEAR_16
    sample_rate_hertz = 48000

    session = session_client.session_path(project_id, session_id)
    print("Session path: {}\n".format(session))

    if audio_byte:
        input_audio = audio_byte
    else:
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

    #with open("output.wav", "wb") as out:
    #    out.write(response.output_audio)
    #    print('Audio content written to file "output.wav"')
    return response


def dialogflowbot(request):
    # Set CORS headers for the preflight request
    if request.method == 'OPTIONS':
        # Allows GET requests from any origin with the Content-Type
        # header and caches preflight response for an 3600s
        headers = {
            'Access-Control-Allow-Origin': '*',
            'Access-Control-Allow-Methods': 'GET',
            'Access-Control-Allow-Headers': 'Content-Type',
            'Access-Control-Max-Age': '3600'
        }

        return ('', 204, headers)

    # Set CORS headers for the main request
    headers = {
        'Access-Control-Allow-Origin': '*'
    }

    if request.method != 'POST':
        return ('', 204, headers)

    file = request.files['file']

    if file.filename == '':
        return ('No selected file', 200, headers)

    input_audio = file.stream.read()
    out = detect_intent_audio(None, input_audio)
    audio_file = BytesIO(out.output_audio)
    resp = {
        'query_text': out.query_result.query_text,
        'response_text': out.query_result.fulfillment_text,
        'content': base64.b64encode(out.output_audio).decode()
    }
    return (resp, 200, headers)
    
    #resp = make_response(send_file(audio_file, download_name='output_audio.wav'))
    #resp.headers = headers
    #return resp

    #return ('', 200, headers)