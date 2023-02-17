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

function UserAuth() {
    let username = getCookie('username');
    let password = getCookie('password');
    //const url = 'http://127.0.0.1:5000/';
    const url = 'https://asia-east2-industrial-silo-356001.cloudfunctions.net/learning-rpg-game';
    fetch(url, {
        method: 'POST',
        headers: {'Content-Type':'application/x-www-form-urlencoded'}, // this line is important, if this content-type is not set it wont work
        body:   'username='+username +'&'+ 
                'password='+password
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
                $('#question_table tbody tr').append('<td><a href="#" class="btn btn-danger"><i class="fa fa-trash-o"></i> Delete </a></td>');
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
                        field: 'string',
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
                        field: 'string',
                        title: 'Sentence'
                    }],
                    data: sentence
                };
                $('#sentence_table').bootstrapTable(sentence_data);
            }
        } else {
            $('#username').val('');
            $('#password').val('');
            $('#loginform').show();
            setCookie('username', '', 0);
            setCookie('password', '', 0);
            $('#errorbox').html('wrong username or password');
            $('#errorbox').show();
            $('#logout').hide();
        }
    })
    .catch(err => {
        $('#loginpage').show();

        //window.alert(err);
        $('#errorbox').html('connection failed');
        $('#errorbox').show();
        setCookie('username', '', 0);
        setCookie('password', '', 0);
        $('#logout').hide();
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
    window.location.reload();
}

function Refresh() {
    $('#loginform').hide();
    $('#errorbox').hide();
    $('#logout').hide();
    
    if (getCookie('username') != '')
    {
        UserAuth();
    }
    else
    {
        $('#loginform').show();
        HideTables();
    }
}
