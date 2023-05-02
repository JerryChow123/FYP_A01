
const admin = require('firebase-admin');
const serviceKey = require('../first/firebase_admin_key.json');
const fs = require('fs');

try {
    var dburl = fs.readFileSync('../first/firebase_database_url.txt', 'utf8');
} catch (err) {
    console.error(err);
    process.exit();
}

admin.initializeApp({
    credential: admin.credential.cert(serviceKey),
    databaseURL: dburl,
});

var db = admin.database();
var account = db.ref("/account/");
account.get()
.then(snapshot => {
    data = snapshot.val();
    for (let record in data) {
        if (data[record]['username'] === 'root') {
            console.log("User 'root' exists");
            process.exit();
        }
    }
    account.push({
        'username': 'root',
        'password': 'root',
        'role': 2
    })
    .then(e => {
        console.log("User 'root' created (password: root)");
        process.exit();
    });
});