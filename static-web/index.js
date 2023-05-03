//const url = 'http://127.0.0.1:5000/';
const url = '<FUNCTION_AUTH>'

function setCookie(cname, cvalue, exmins) {
    var d = new Date();
    d.setTime(d.getTime() + (exmins * 60 * 1000));
    var expires = "expires=" + d.toGMTString();
    document.cookie = cname + "=" + cvalue + "; " + expires;
}

function getCookie(cname) {
    var name = cname + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i].trim();
        if (c.indexOf(name) == 0) return c.substring(name.length, c.length);
    }
    return "";
}

function ShowTables() {
    $('#questionform').show();
    $('#leaderboardform').show();
    $('#dictationform').show();
    $('#sentenceform').show();
    $('#accountform').show();

    $('#question_table').show();
    $('#mark_table').show();
    $('#dictation_table').show();
    $('#sentence_table').show();
}

function HideTables() {
    $('#question_table').hide();
    $('#mark_table').hide();
    $('#dictation_table').hide();
    $('#sentence_table').hide();
}

function questiontable_delete(elem) {
    let td = elem.parentElement;
    let tds = td.parentElement;
    let key = tds.childNodes[0].innerHTML;
    if (confirm(key))
        delete_data('questions', 'question', key);
}

function marktable_delete(elem) {
    let td = elem.parentElement;
    let tds = td.parentElement;
    let key = tds.childNodes[1].innerHTML;
    if (confirm(key))
        delete_data('marks', 'date', key);
}

function dictationtable_delete(elem) {
    let td = elem.parentElement;
    let tds = td.parentElement;
    let key = tds.childNodes[0].innerHTML;
    if (confirm(key))
        delete_data('dictation', 'text', key);
}

function sentencetable_delete(elem) {
    let td = elem.parentElement;
    let tds = td.parentElement;
    let key = tds.childNodes[0].innerHTML;
    if (confirm(key))
        delete_data('sentence', 'text', key);
}

function accounttable_delete(elem) {
    let td = elem.parentElement;
    let tds = td.parentElement;
    let key = tds.childNodes[0].innerHTML;
    if (confirm(key))
        delete_data('account', 'username', key);
}

function UserAuth(jump=false, role=0) {
    let username = getCookie('username');
    let password = getCookie('password');
    fetch(url, {
        method: 'POST',
        headers: {'Content-Type':'application/x-www-form-urlencoded'}, // this line is important, if this content-type is not set it wont work
        body:   'username='+username +'&'+ 
                'password='+password +'&'+ 
                'role='+role
    })
    .then(response => response.json())
    .then(data => {
        $('#loginpage').show();

        if (data['success'] == true) {
            $('#loginform').hide();
            $('#errorbox').hide();
            ShowTables();
            $('#logout').show();
            
            if (data['questions'] != null) {
                let questions = [];
                for (let i in data['questions']) {
                    questions.push(data['questions'][i])
                }
                //window.alert(questions);
                let question_data = {
                    columns: [{
                        field: 'question',
                        title: 'Question'
                    }, {
                        field: 'optionA',
                        title: 'Option A'
                    }, {
                        field: 'optionB',
                        title: 'Option B'
                    }, {
                        field: 'optionC',
                        title: 'Option C'
                    }, {
                        field: 'optionD',
                        title: 'Option D'
                    }, {
                        field: 'answer',
                        title: 'Answer'
                    }],
                    data: questions
                };
                $('#question_table').bootstrapTable(question_data);
            }

            if (data['marks'] != null) {
                let marks = [];
                for (let i in data['marks']) {
                    marks.push(data['marks'][i])
                }
                let mark_data = {
                    columns: [{
                        field: 'name',
                        title: 'Student Name'
                    }, {
                        field: 'date',
                        title: 'Date'
                    }, {
                        field: 'marks',
                        title: 'Marks'
                    }],
                    data: marks
                };
                $('#mark_table').bootstrapTable(mark_data);
            }

            if (data['dictation'] != null) {
                let dictation = [];
                for (let i in data['dictation']) {
                    dictation.push(data['dictation'][i])
                }
                let dictation_data = {
                    columns: [{
                        field: 'text',
                        title: 'Dictation'
                    }],
                    data: dictation
                };
                $('#dictation_table').bootstrapTable(dictation_data);
            }

            if (data['sentence'] != null) {
                let sentence = [];
                for (let i in data['sentence']) {
                    sentence.push(data['sentence'][i])
                }
                let sentence_data = {
                    columns: [{
                        field: 'text',
                        title: 'Sentence'
                    }],
                    data: sentence
                };
                $('#sentence_table').bootstrapTable(sentence_data);
            }

            if (data['accounts'] != null) {
                let accounts = [];
                for (let i in data['accounts']) {
                    accounts.push(data['accounts'][i])
                }
                let accounts_data = {
                    columns: [{
                        field: 'username',
                        title: 'Username'
                    }, {
                        field: 'password',
                        title: 'Password'
                    }],
                    data: accounts
                };
                $('#account_table').bootstrapTable(accounts_data);
            }

            $('#question_table tbody tr').append('<td style="width: 1%"><button class="btn btn-danger" onclick="questiontable_delete(this)">Delete </button></td>');
            $('#mark_table tbody tr').append('<td style="width: 1%"><button class="btn btn-danger" onclick="marktable_delete(this)">Delete </button></td>');
            $('#dictation_table tbody tr').append('<td style="width: 1%"><button class="btn btn-danger" onclick="dictationtable_delete(this)">Delete </button></td>');
            $('#sentence_table tbody tr').append('<td style="width: 1%"><button class="btn btn-danger" onclick="sentencetable_delete(this)">Delete </button></td>');
            $('#account_table tbody tr').append('<td style="width: 1%"><button class="btn btn-danger" onclick="accounttable_delete(this)">Delete </button></td>');

        } else {
            if (role >= 1)
            {
                alert('You have not permission!');
                location.href = 'index.html';
                return;
            }

            $('#username').val('');
            $('#password').val('');
            $('#loginform').show();
            setCookie('username', '', 0);
            setCookie('password', '', 0);
            $('#errorbox').html('wrong username or password');
            $('#errorbox').show();
            $('#logout').hide();
            if (jump)
                location.href = 'index.html';
        }
    })
    .catch(err => {
        $('#loginpage').show();

        window.alert(err);
        $('#errorbox').html('connection failed');
        $('#errorbox').show();
        setCookie('username', '', 0);
        setCookie('password', '', 0);
        $('#logout').hide();
        if (jump)
            location.href = 'index.html';
    });
}

function delete_data(table, key, value) {
    let username = getCookie('username');
    let password = getCookie('password');

    fetch(url, {
        method: 'POST',
        headers: {'Content-Type':'application/x-www-form-urlencoded'}, // this line is important, if this content-type is not set it wont work
        body:   'username='+username+'&'+ 
                'password='+password+'&'+
                'operation=2&'+
                'table='+table+'&'+
                'key='+key+'&'+
                'value='+value
    })
    .then(response => response.json())
    .then(data => {
        if (data['success'] == true) {
            if (data['deleted'] > 0) {
                alert(data['message']);
                location.reload();
            }
            else {
                $('errorbox').html(data['message']);
                $('errorbox').show();
            }
        } else {
            $('#errorbox').html('wrong username or password');
            $('#errorbox').show();
        }
    })
    .catch(err => {
        $('#errorbox').html('connection failed');
        $('#errorbox').show();
    });
}

function OnLogin() {
    var username = $('#username').val();
    var password = $('#password').val();
    setCookie('username', username, 30);
    setCookie('password', password, 30);
    UserAuth();
}

function OnLogout() {
    setCookie('username', '', 0);
    setCookie('password', '', 0);
    location.reload();
}

var str;

function OnSubmitQuestions() {
    if (!$('form')[0].checkValidity())
        return false;

    let username = getCookie('username');
    let password = getCookie('password');

    str =       'question: ' + $('#Question').val() + '\n' +
                'optionA: ' + $('#OptionsA').val() + '\n' +
                'optionB: ' + $('#OptionsB').val() + '\n' +
                'optionC: ' + $('#OptionsC').val() + '\n' +
                'optionD: ' + $('#OptionsD').val() + '\n' +
                'answer: ' + $('#answer').val() + '\n'

    if (!confirm(str))
        return false;

    fetch(url, {
        method: 'POST',
        headers: {'Content-Type':'application/x-www-form-urlencoded'}, // this line is important, if this content-type is not set it wont work
        body:   'username='+username+'&'+ 
                'password='+password+'&'+
                'table=questions&'+
                'key=question&'+
                'operation=1&'+
                'question='+$('#Question').val()+'&'+
                'optionA='+$('#OptionsA').val()+'&'+
                'optionB='+$('#OptionsB').val()+'&'+
                'optionC='+$('#OptionsC').val()+'&'+
                'optionD='+$('#OptionsD').val()+'&'+
                'answer='+$('#answer').val()
    })
    .then(response => response.json())
    .then(data => {
        if (data['success'] == true) {
            if (data['inserted'] == true) {
                alert(data['message']);
                location.reload();
            }
            else {
                $('errorbox').html(data['message']);
                $('errorbox').show();
            }
        } else {
            $('#errorbox').html('wrong username or password');
            $('#errorbox').show();
        }
    })
    .catch(err => {
        $('#errorbox').html('connection failed');
        $('#errorbox').show();
    });

    return false;
}

function OnSubmitDictation() {
    if (!$('form')[0].checkValidity())
        return false;

    let username = getCookie('username');
    let password = getCookie('password');

    str =       'Vocabulary: ' + $('#vocabulary').val();

    if (!confirm(str))
        return false;

    fetch(url, {
        method: 'POST',
        headers: {'Content-Type':'application/x-www-form-urlencoded'}, // this line is important, if this content-type is not set it wont work
        body:   'username='+username+'&'+ 
                'password='+password+'&'+
                'table=dictation&'+
                'key=text&'+
                'operation=1&'+
                'text='+$('#vocabulary').val()
    })
    .then(response => response.json())
    .then(data => {
        if (data['success'] == true) {
            if (data['inserted'] == true) {
                alert(data['message']);
                location.reload();
            }
            else {
                $('errorbox').html(data['message']);
                $('errorbox').show();
            }
        } else {
            $('#errorbox').html('wrong username or password');
            $('#errorbox').show();
        }
    })
    .catch(err => {
        $('#errorbox').html('connection failed');
        $('#errorbox').show();
    });

    return false;
}

function OnSubmitSentence() {
    if (!$('form')[0].checkValidity())
        return false;

    let username = getCookie('username');
    let password = getCookie('password');

    str =       'Sentence: ' + $('#sentence').val();

    if (!confirm(str))
        return false;

    fetch(url, {
        method: 'POST',
        headers: {'Content-Type':'application/x-www-form-urlencoded'}, // this line is important, if this content-type is not set it wont work
        body:   'username='+username+'&'+ 
                'password='+password+'&'+
                'table=sentence&'+
                'key=text&'+
                'operation=1&'+
                'text='+$('#sentence').val()
    })
    .then(response => response.json())
    .then(data => {
        if (data['success'] == true) {
            if (data['inserted'] == true) {
                alert(data['message']);
                location.reload();
            }
            else {
                $('errorbox').html(data['message']);
                $('errorbox').show();
            }
        } else {
            $('#errorbox').html('wrong username or password');
            $('#errorbox').show();
        }
    })
    .catch(err => {
        $('#errorbox').html('connection failed');
        $('#errorbox').show();
    });

    return false;
}

function OnSubmitAccount() {
    if (!$('form')[0].checkValidity())
        return false;

    let username = getCookie('username');
    let password = getCookie('password');

    str =       'Account: ' + $('#username').val() + '\n' +
                'Role: ' + $('#userrole option:selected').text();

    if (!confirm(str))
        return false;

    fetch(url, {
        method: 'POST',
        headers: {'Content-Type':'application/x-www-form-urlencoded'}, // this line is important, if this content-type is not set it wont work
        body:   'username='+username+'&'+ 
                'password='+password+'&'+
                'table=account&'+
                'key=username&'+
                'operation=1&'+
                'add_username='+$('#username').val()+'&'+
                'add_password='+$('#password').val()+'&'+
                'userrole='+$('#userrole').val()
    })
    .then(response => response.json())
    .then(data => {
        if (data['success'] == true) {
            if (data['inserted'] == true) {
                alert(data['message']);
                location.reload();
            }
            else {
                $('errorbox').html(data['message']);
                $('errorbox').show();
            }
        } else {
            $('#errorbox').html('wrong username or password');
            $('#errorbox').show();
        }
    })
    .catch(err => {
        $('#errorbox').html('connection failed');
        $('#errorbox').show();
    });

    return false;
}

function SignedRefresh(role=0) {
    if (getCookie('username') != '') {
        UserAuth(true, role);
    }
    else {
        alert('You must log-in first!');
        location.href = 'index.html';
    }
}

function Refresh() {
    $('#loginform').hide();
    $('#errorbox').hide();
    $('#logout').hide();
    
    if (getCookie('username') != '') {
        UserAuth(false);
    }
    else {
        $('#loginform').show();
        HideTables();
    }
}