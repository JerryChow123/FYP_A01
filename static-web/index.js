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

function UserAuth() {
    let username = getCookie('username');
    let password = getCookie('password');
    const url = ''
    fetch(url, {
        method: 'POST',
        headers: {'Content-Type':'application/x-www-form-urlencoded'}, // this line is important, if this content-type is not set it wont work
        body: 'username='+username +'&'+ 'password='+password
    })
    .then(response => response.json())
    .then(data => {
        if (data['success'] == true) {
            $('#loginform').hide();
            $('#errorbox').hide();
        } else {
            $('#username').val('');
            $('#password').val('');
            $('#loginform').show();
            setCookie('username', '', 0);
            setCookie('password', '', 0);
            $('#errorbox').html('wrong username or password');
            $('#errorbox').show();
        }
    })
    .catch(err => {
        $('#errorbox').html('connection failed');
        $('#errorbox').show();
        setCookie('username', '', 0);
        setCookie('password', '', 0);
    });
}

function OnLogin() {
    var username = $('#username').val();
    var password = $('#password').val();
    setCookie('username', username, 30);
    setCookie('password', password, 30);
    UserAuth();
}
`use strict`;
function refreshTime() {
  const timeDisplay = document.getElementById("time");
  const dateString = new Date().toLocaleString();
  const formattedString = dateString.replace(", ", " - ");
  timeDisplay.textContent = formattedString;
}
  setInterval(refreshTime, 1000);


function Refresh() {
    $('#loginform').hide();
    $('#errorbox').hide();
    
    if (getCookie('username') != '')
        UserAuth();
    else
        $('#loginform').show();

    data = {
        columns: [{
            field: 'id',
            title: 'Item ID'
        }, {
            field: 'name',
            title: 'Item Name'
        }, {
            field: 'price',
            title: 'Item Price'
        }],
        data: [{
            id: 1,
            name: 'Item 1',
            price: '$1'
        }, {
            id: 2,
            name: 'Item 2',
            price: '$2'
        }, {
            id: 3,
            name: 'Item 3',
            price: '$3'
        },{
            id: 4,
            name: 'Item 4',
            price: '$4'
        }, {
            id: 5,
            name: 'Item 5',
            price: '$5'
        }, {
            id: 6,
            name: 'Item 6',
            price: '$6'
        }, {
            id: 7,
            name: 'Item 7',
            price: '$7'
        }, {
            id: 8,
            name: 'Item 8',
            price: '$8'
        }, {
            id: 9,
            name: 'Item 9',
            price: '$9'
        }, {
            id: 10,
            name: 'Item 10',
            price: '$10'
        },
    
    
    ]
    };

    $('#table').bootstrapTable(data);
}
