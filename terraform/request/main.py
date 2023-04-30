import firebase_admin, json
from firebase_admin import db, credentials

from flask import jsonify, request

cert = "firebase_admin_key.json"
f = open('firebase_database_url.txt')
database_url = f.read()
f.close()


def getQuestions(headers):
    username = request.form['username']
    password = request.form['password']

    account_table = db.reference('/account/').get()
    for key in account_table:
        i = account_table[key]
        if i['username'] == username and i['password'] == password:
            question_table = db.reference('/questions/').get()
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


def getDictations(headers):
    username = request.form['username']
    password = request.form['password']

    account_table = db.reference('/account/').get()
    for key in account_table:
        i = account_table[key]
        if i['username'] == username and i['password'] == password:
            dictation_table = db.reference('/dictation/').get()
            jsonResp = {
                'success': True,
                'dictation': dictation_table
            }
            return jsonResp
            return (jsonify(jsonResp), 200, headers)

    return {'success': False}
    return (jsonify({
        'success': False
    }), 200, headers)


def getSentences(header):
    username = request.form['username']
    password = request.form['password']

    account_table = db.reference('/account/').get()
    for key in account_table:
        i = account_table[key]
        if i['username'] == username and i['password'] == password:
            sentence_table = db.reference('/sentence/').get()
            jsonResp = {
                'success': True,
                'sentence': sentence_table
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
        d = getDictations(headers)
        s = getSentences(headers)
        jsonResp['questions'] = q['questions']
        jsonResp['marks'] = m['marks']
        jsonResp['dictation'] = d['dictation']
        jsonResp['sentence'] = s['sentence']
        #app.logger.info(jsonResp)

    return (jsonify(jsonResp), 200, headers)


def delete_data(headers, table_name):
    username = request.form['username']
    password = request.form['password']

    # check account
    login_success = False

    account_table = db.reference('/account/').get()
    for key in account_table:
        i = account_table[key]
        if i['username'] == username and i['password'] == password:
            login_success = True

    jsonResp = {
        'success': login_success,
    }

    if login_success:
        table = db.reference('/'+table_name+'/')
        deleted = 0
        table_key = request.form['key']
        value = request.form['value']

        try:
            for k, v in table.get().items():
                if v[table_key] == value:
                    table.child(k).delete()
                    deleted += 1

            message = str(deleted) + " rows deleted"
        except Exception as err:
            message = 'ERROR: ' + str(err)

        jsonResp['table'] = table_name
        jsonResp['target'] = value
        jsonResp['message'] = message
        jsonResp['deleted'] = deleted

    return (jsonify(jsonResp), 200, headers)


def insert_data(headers, table_name):
    username = request.form['username']
    password = request.form['password']

    # check account
    login_success = False

    account_table = db.reference('/account/').get()
    for key in account_table:
        i = account_table[key]
        if i['username'] == username and i['password'] == password:
            login_success = True

    jsonResp = {
        'success': login_success,
    }

    if login_success:
        table = db.reference('/' + table_name + '/')
        success = 1

        _key = request.form['key']
        if _key != 'date':
            try:
                for k, v in table.get().items():
                    if v[_key] == request.form[_key]:
                        message = 'ERROR: same primary key value'
                        success = 0
                        break
            except:
                pass

        if success > 0:
            try:
                if table_name == 'questions':
                    table.push({
                        'answer': request.form['answer'],
                        'optionA': request.form['optionA'],
                        'optionB': request.form['optionB'],
                        'optionC': request.form['optionC'],
                        'optionD': request.form['optionD'],
                        'question': request.form['question'],
                    })
                elif table_name == 'marks':
                    table.push({
                        'date': str(datetime.datetime.now()),
                        'marks': request.form['marks'],
                        'name': request.form['name'],
                    })
                elif table_name == 'dictation':
                    table.push({
                        'text': request.form['text']
                    })
                elif table_name == 'sentence':
                    table.push({
                        'text': request.form['text']
                    })
                else:
                    success = 0

                message = str(success) + ' rows inserted'
            except Exception as err:
                message = 'ERROR: ' + str(err)

        jsonResp['table'] = table_name
        jsonResp['message'] = message
        jsonResp['inserted'] = bool(success)

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

    if not firebase_admin._apps:
        cred = credentials.Certificate(cert)
        default_app = firebase_admin.initialize_app(cred, {
            'databaseURL': database_url
        })

    if request.method == 'POST':
        # MODIFY DATA
        print(json.dumps(request.form, indent=2))
        if 'operation' in request.form:
            otype = int(request.form['operation'])
            table = request.form['table']
            #app.logger.info('Modify Data ' + otype)

            if otype == 1:
                return insert_data(headers, table)
            elif otype == 2:
                return delete_data(headers, table)

        # USER LOGIN
        return userAuth(headers)

    return ('Nothing', 200, headers)