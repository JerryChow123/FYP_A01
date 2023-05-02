import firebase_admin, json, copy, datetime
from firebase_admin import db, credentials

from flask import jsonify, request

cert = "firebase_admin_key.json"
database_url = '<DATABASE_URL>'


def getQuestions(headers):
    if userAuth(headers, True):
        question_table = db.reference('/questions/').get()
        jsonResp = {
            'success': True,
            'questions': question_table
        }
        return jsonResp

    return {'success': False}


def getMarks(headers):
    if userAuth(headers, True):
        mark_table = db.reference('/marks/').get()
        jsonResp = {
            'success': True,
            'marks': mark_table
        }
        return jsonResp

    return {'success': False}


def getDictations(headers):
    if userAuth(headers, True):
        dictation_table = db.reference('/dictation/').get()
        jsonResp = {
            'success': True,
            'dictation': dictation_table
        }
        return jsonResp

    return {'success': False}


def getSentences(headers):
    if userAuth(headers, True):
        sentence_table = db.reference('/sentence/').get()
        jsonResp = {
            'success': True,
            'sentence': sentence_table
        }
        return jsonResp

    return {'success': False}


def userAuth(headers, auth_only=False, get_role=False):
    username = request.form['username']
    password = request.form['password']
    try:
        role = int(request.form["role"])
    except:
        role = 0

    # check account
    login_success = False
    user_role = 0

    account_table = db.reference('/account/').get()
    for key in account_table:
        i = account_table[key]
        if i['username'] == username and i['password'] == password:
            login_success = (int(i['role']) >= role)
            user_role = int(i['role'])

    if auth_only:
        if get_role:
            return login_success, user_role
        else:
            return login_success

    #app.logger.info('User Auth : %s (%s)' % (username, str(login_success)))
    jsonResp = {
        'success': login_success,
    }

    if login_success:
        m = getMarks(headers)
        jsonResp['marks'] = m['marks']
        if role >= 2:
            accounts = copy.deepcopy(account_table)
            for key in accounts:
                ac = accounts[key]
                pw = ''
                for i in range(len(ac['password'])):
                    pw += '*'
                ac['password'] = pw
            jsonResp['accounts'] = accounts
        elif role >= 1:
            q = getQuestions(headers)
            d = getDictations(headers)
            s = getSentences(headers)
            jsonResp['questions'] = q['questions']
            jsonResp['dictation'] = d['dictation']
            jsonResp['sentence'] = s['sentence']
        else:
            try:
                if int(request.form['game']) == 1:
                    q = getQuestions(headers)
                    d = getDictations(headers)
                    s = getSentences(headers)
                    jsonResp['questions'] = q['questions']
                    jsonResp['dictation'] = d['dictation']
                    jsonResp['sentence'] = s['sentence']
            except:
                pass
        #app.logger.info(json.dumps(jsonResp, indent=2))

    return (jsonify(jsonResp), 200, headers)


def delete_data(headers, table_name):
    login_success, user_role = userAuth(headers, True, True)

    if login_success:
        if table_name == 'account':
            login_success = (user_role >= 2)
        elif table_name == 'questions' or table_name == 'dictation' or table_name == 'sentence':
            login_success = (user_role >= 1)

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
    login_success, user_role = userAuth(headers, True, True)

    if login_success:
        if table_name == 'account':
            login_success = (user_role >= 2)
        elif table_name == 'questions' or table_name == 'dictation' or table_name == 'sentence':
            login_success = (user_role >= 1)

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
                    if _key == 'username' or _key == 'password':
                        exists = (v[_key] == request.form['add_' + _key])
                    else:
                        exists = (v[_key] == request.form[_key])
                    if exists:
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
                elif table_name == 'account':
                    table.push({
                        'username': request.form['add_username'],
                        'password': request.form['add_password'],
                        'role': request.form['userrole']
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