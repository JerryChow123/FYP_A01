
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

const source = `{
    "rules": {
        ".read": "auth.uid !== null",
        ".write": "auth.uid !== null"
    }
}`;

var db = admin.database();
db.setRules(source)
.then(e => {
    process.exit();
});