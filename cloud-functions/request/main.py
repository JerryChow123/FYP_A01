import firebase_admin
from firebase_admin import db, credentials

from flask import jsonify, request


def getQuestions(headers):
    username = request.form['username']
    password = request.form['password']

    account_table = db.reference('/account/').get()
    for key in account_table:
        i = account_table[key]
        if i['username'] == username and i['password'] == password:
            question_table = db.reference('/questions/').get()

            questions = []
            for q in question_table:
                questions.append(question_table[q])

            jsonResp = {
                'success': True,
                'questions': question_table
            }
            return jsonResp
            return (jsonify(jsonResp), 200, headers)

    return {'success': False}
    return (jsonify({
        'success': False
    }), 200, headers)


def getMarks(headers):
    username = request.form['username']
    password = request.form['password']

    account_table = db.reference('/account/').get()
    for key in account_table:
        i = account_table[key]
        if i['username'] == username and i['password'] == password:
            mark_table = db.reference('/marks/').get()
            jsonResp = {
                'success': True,
                'marks': mark_table
            }
            return jsonResp
            return (jsonify(jsonResp), 200, headers)

    return {'success': False}
    return (jsonify({
        'success': False
    }), 200, headers)


def userAuth(headers):
    username = request.form['username']
    password = request.form['password']

    # check account
    login_success = False

    account_table = db.reference('/account/').get()
    for key in account_table:
        i = account_table[key]
        if i['username'] == username and i['password'] == password:
            login_success = True

    #app.logger.info('User Auth : %s (%s)' % (username, str(login_success)))
    jsonResp = {
        'success': login_success
    }

    if (login_success):
        q = getQuestions(headers)
        m = getMarks(headers)
        jsonResp['questions'] = q['questions']
        jsonResp['marks'] = m['marks']
        #app.logger.info(jsonResp)

    return (jsonify(jsonResp), 200, headers)


def web_response(request):
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

    cert = {
        "type": "service_account",
        "project_id": "learning-rpg-game",
        "private_key_id": "4b55517f4b1f350faa0a999889a38b02f5afb65f",
        "private_key": "-----BEGIN PRIVATE KEY-----\nMIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQDLIElj7CzDWcap\nXtlo5IYiSxv4BUmqscZQaEvjBXaQNcNKwpy13jpVW2DkMCfVDgX6pJUy58uE+MA8\nebWRHm+x8f0liF0cU9iRCGjZPDaxIG7zw5U6UZsqe+5A2p03azF+TxocHk/K/hGx\nMQ0l6j3sLoBxRm/VWiLAH9vkFfZb6dAFPBccQqppHan2XU51TDzRYFlev+Nh4gy/\n0eS+uxx1KoERIxtZ96Kr/DA/BOl4WGu/Qbe/mrc/n9gyAsZMlkzx+3nTuscs0RR3\nyEjajl6FNgrwdHUKpI3UIF2RUGSsI0RhbvD00ycrMh/knaBtek1i+Myqg6MwVcfT\ndc2O6PhdAgMBAAECggEABxthFfrscgVRGxnzd2UPgGGqX9ongq5npuaPSSkmxdLr\nu1Dl1MMqvVPOJw28RwsPkH5zre1IRKXDbBpCDQrIk6xI1ChaQaR5XlGyAW/G5hmh\naCv+q6SxTiI56tD7UTYR7D5uRm+VESAklFOIBzyT4BCcc9ooDYnyz1AODIPsM7kB\nkT16N4x9juSnnB+oUIw8aPfbY9HPhdiDsCahM1fSwIEK2+W1hDqKanPou5RQOMtb\nCY0x51fjJndLGjgdFDxyLZfYe+eZvlH3s3VDqclPMv/GkzovLiVSBZE9Nb8cHc8k\nbylQlxCeQrA2zrpQHiUQF/i1h36lLyNszOa/WwNB4QKBgQDvi5xj09vrW9k6RaDH\n9dbAdWMf/nK3BtpCLMYB0gMF/9aUr31E+wmr8Owd5rwVqwg2BBAj7p+RXv37SM/V\nLZTVckKagYCzxxcPdeZwNVSeHbQkS2xWu7OX22DBk76ZzJem65Ngqh3FkTrlkekG\nVs4lCzwaiYs08OIJEO4Chluy2QKBgQDZFD73KNa2orwsnfNLMSKJ9QYBCDEcw4Zy\not1KpScNJ8ITWhquvn2T7SebnNQuceyHzM64td/2nNCUD83CTZ1GmCtpHjD10g3A\nE011KjJieieg3eKsDz+KIs8nGceeyGf5Mq0JKPf/GNolHf75Ya1b6yEen8dX1OAo\nj+T8vee3JQKBgGFj/JhlqVL+S8oyRtUlcUNcNonqdI9PPOuMspBI5yvIQWDAHIkw\nG1WJpSXfCncwyyzS6BED0mJTIaXJi54bYxiI1OVmY4I0Hn8NqzvRou6KGxRqiciV\nlcXWznbFb8et87ZMruWtJF+P1UdC1F/caQGMmModQ7ipeuy+slXadGu5AoGBAIv1\nX7tfG0BZn9VsaQZbwZcM4TgdHf0bmz2h/+c2n/z9W0W63GU58CFF0DmmXa8sSIIt\n7EJvEN0hseEkZ1cVT4fKaIK3sn1rVu42A8S81gtkEtTG+nRQyHi5hLIDDw8yysaC\ng8naHjrcvRkdQ91puqn+6TWjcpUQbQ356HuDvTf5AoGBAIFmG+nTJbNdQzPo2Ilx\nZPlf5PSeXLw1QlkMp8YtcmnNRl1Y3EIV3X95b9nQqz62Js7wrM3O8zqGuEFgXZRr\n+gccanQ563+u6dNomNcmhHAdq/YOBJMXMCuS373Joo2mUkfS7IGNIttdMzRuuTsD\njVQ90JW5/7drWNGwyL06p+HT\n-----END PRIVATE KEY-----\n",
        "client_email": "firebase-adminsdk-ocgg9@learning-rpg-game.iam.gserviceaccount.com",
        "client_id": "110573471095907490131",
        "auth_uri": "https://accounts.google.com/o/oauth2/auth",
        "token_uri": "https://oauth2.googleapis.com/token",
        "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
        "client_x509_cert_url": "https://www.googleapis.com/robot/v1/metadata/x509/firebase-adminsdk-ocgg9%40learning-rpg-game.iam.gserviceaccount.com"
    }

    if not firebase_admin._apps:
        cred = credentials.Certificate(cert)
        default_app = firebase_admin.initialize_app(cred, {
            'databaseURL': 'https://learning-rpg-game-default-rtdb.asia-southeast1.firebasedatabase.app/'
        })

    if request.method == 'POST':
        return userAuth(headers)

    return ('Nothing', 200, headers)