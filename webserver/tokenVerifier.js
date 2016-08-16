module.exports = (function () {
    // Sample token: "eyJhbGciOiJSUzI1NiIsImtpZCI6IjA2MTY3ZGRhODkwNjgxMzE4YzFjNTc3YzdiMjE2YTkzNWI4MzViOTEifQ.eyJpc3MiOiJodHRwczovL2FjY291bnRzLmdvb2dsZS5jb20iLCJhdWQiOiIxMDEyNjA3NjYxNDM2LTg1MDY2Y2xpaGdqOWw4YXRtYWtkY3ZwMmVqZ21raTd0LmFwcHMuZ29vZ2xldXNlcmNvbnRlbnQuY29tIiwic3ViIjoiMTA5NzgxNzk4NDYyMjY2Mjc3ODk3IiwiZW1haWxfdmVyaWZpZWQiOnRydWUsImF6cCI6IjEwMTI2MDc2NjE0MzYtbXU3ampmNGkzcmtncnM1cDVsbmk3cTdqZ3A4bzE1c3YuYXBwcy5nb29nbGV1c2VyY29udGVudC5jb20iLCJlbWFpbCI6ImFsaWFsYXZpYUBnbWFpbC5jb20iLCJpYXQiOjE0NzA0MDkwNzksImV4cCI6MTQ3MDQxMjY3OSwibmFtZSI6IkFsaSBBbGF2aSIsImdpdmVuX25hbWUiOiJBbGkiLCJmYW1pbHlfbmFtZSI6IkFsYXZpIiwibG9jYWxlIjoiZW4ifQ.G6Cr6AElOBwJMyhHBUUZtqnO95XFUuKLbgJHVxioEaGTCt5rktQsBbZYo1vzhVAFk6SIsLV3MZz4r4z6DPJdO9muyLBzwUrDoZYfowabzfks82jIecihlegzehZhwXhgbuiuMAO5bfBpxm6y3-srHsDRoGaYGJiU0BcdlmDjQTowkcJhk-ZvzCozmzL4LmVUUr6YN1vZTGPUmuAO3UudwKVAScDwMnbd8XFtMBCVo8vZNmC6pHVRoyj55j36M5GgkyxPRBaIulyuz3Pdttx0BBAKGGoEfRJC7vMDNl1dA3OB4x4sRJctTvRSkWfNElCr1DF0pqw148Ag8HXEK2Eqbg"

    var my = {};
    var validClientIDs = ["1012607661436-85066clihgj9l8atmakdcvp2ejgmki7t.apps.googleusercontent.com"];
    my.verifyToken = function (token) {
        console.log("verifying Token!");
        const https = require("https");
        var uri = `https://www.googleapis.com/oauth2/v3/tokeninfo?id_token=${token}`;
        console.log(uri);
        https.get(uri,
            (res) => {
            console.log('statusCode:', res.statusCode);
            console.log('headers:', res.headers);

            res.on('data', (d) => {
                console.log("data...");
                process.stdout.write(d);
            });

        }).on('error', (e) => {
            console.error(e);
        });
    }
    return my;
}());
