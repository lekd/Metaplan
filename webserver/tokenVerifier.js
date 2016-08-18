module.exports = (function () {

    // Sample token: "eyJhbGciOiJSUzI1NiIsImtpZCI6IjA2MTY3ZGRhODkwNjgxMzE4YzFjNTc3YzdiMjE2YTkzNWI4MzViOTEifQ.eyJpc3MiOiJodHRwczovL2FjY291bnRzLmdvb2dsZS5jb20iLCJhdWQiOiIxMDEyNjA3NjYxNDM2LTg1MDY2Y2xpaGdqOWw4YXRtYWtkY3ZwMmVqZ21raTd0LmFwcHMuZ29vZ2xldXNlcmNvbnRlbnQuY29tIiwic3ViIjoiMTA5NzgxNzk4NDYyMjY2Mjc3ODk3IiwiZW1haWxfdmVyaWZpZWQiOnRydWUsImF6cCI6IjEwMTI2MDc2NjE0MzYtbXU3ampmNGkzcmtncnM1cDVsbmk3cTdqZ3A4bzE1c3YuYXBwcy5nb29nbGV1c2VyY29udGVudC5jb20iLCJlbWFpbCI6ImFsaWFsYXZpYUBnbWFpbC5jb20iLCJpYXQiOjE0NzA0MDkwNzksImV4cCI6MTQ3MDQxMjY3OSwibmFtZSI6IkFsaSBBbGF2aSIsImdpdmVuX25hbWUiOiJBbGkiLCJmYW1pbHlfbmFtZSI6IkFsYXZpIiwibG9jYWxlIjoiZW4ifQ.G6Cr6AElOBwJMyhHBUUZtqnO95XFUuKLbgJHVxioEaGTCt5rktQsBbZYo1vzhVAFk6SIsLV3MZz4r4z6DPJdO9muyLBzwUrDoZYfowabzfks82jIecihlegzehZhwXhgbuiuMAO5bfBpxm6y3-srHsDRoGaYGJiU0BcdlmDjQTowkcJhk-ZvzCozmzL4LmVUUr6YN1vZTGPUmuAO3UudwKVAScDwMnbd8XFtMBCVo8vZNmC6pHVRoyj55j36M5GgkyxPRBaIulyuz3Pdttx0BBAKGGoEfRJC7vMDNl1dA3OB4x4sRJctTvRSkWfNElCr1DF0pqw148Ag8HXEK2Eqbg"
    /*    Test Name:	SignInTest
        Test Outcome:	Passed
        Result StandardOutput:	Debug Trace:
            eyJhbGciOiJSUzI1NiIsImtpZCI6ImEzYzczN2E3Yjc5NTAyNjIxN2QwNWJlOThmODczNmJkMDlhNjlkMGQifQ.eyJpc3MiOiJhY2NvdW50cy5nb29nbGUuY29tIiwiYXRfaGFzaCI6InN5NkdHbDRadzZTYS14cnJUVzVObnciLCJhdWQiOiIxMDEyNjA3NjYxNDM2LTRkMGo3czhicjkzMTFpY3Q3cGcyNXVqODMzanJrcm5pLmFwcHMuZ29vZ2xldXNlcmNvbnRlbnQuY29tIiwic3ViIjoiMTA5NzgxNzk4NDYyMjY2Mjc3ODk3IiwiYXpwIjoiMTAxMjYwNzY2MTQzNi00ZDBqN3M4YnI5MzExaWN0N3BnMjV1ajgzM2pya3JuaS5hcHBzLmdvb2dsZXVzZXJjb250ZW50LmNvbSIsImlhdCI6MTQ3MTQyNzUzMCwiZXhwIjoxNDcxNDMxMTMwfQ.Exeoy2TCqFXr8WjXYsxXM0e6EZPmreWzZC-M8urk1ZcK_u9yLi-tB4oFPJCWbZpjnQHpfl0L2sE8YVQhIm3SgFJHOxEKlSP5KCA2breLArVQ0gszL_Yz6RsXv04Gu2E1SYPQYwFJ9JE6ztkIAPA65Zq0V38SCB_gMIzIJ5sDM5Fd2jLmkap8CJYDvZdUAWXbZRY2Xq3CD7IAc0o_GmJ9Hp4BapOZ483nUI_mfSrQwsUuoORUC4bjseUAf5MNhkVd9RqQTfUDBy0oNBoiEJsnEUlobZENqhTl3FbsjiK910rWnHWljUl9Z7UW3I6nnD9LfjURVVgfY8xYR979XskqYg
            */ /*
    Test Name:	SignInTest
    Test Outcome:	Passed
    Result StandardOutput:	Debug Trace:
        eyJhbGciOiJSUzI1NiIsImtpZCI6ImEzYzczN2E3Yjc5NTAyNjIxN2QwNWJlOThmODczNmJkMDlhNjlkMGQifQ.eyJpc3MiOiJhY2NvdW50cy5nb29nbGUuY29tIiwiYXRfaGFzaCI6IkRNTFBTUmp0MXIycGRmMXNCalVHakEiLCJhdWQiOiIxMDEyNjA3NjYxNDM2LTRkMGo3czhicjkzMTFpY3Q3cGcyNXVqODMzanJrcm5pLmFwcHMuZ29vZ2xldXNlcmNvbnRlbnQuY29tIiwic3ViIjoiMTA5NzgxNzk4NDYyMjY2Mjc3ODk3IiwiYXpwIjoiMTAxMjYwNzY2MTQzNi00ZDBqN3M4YnI5MzExaWN0N3BnMjV1ajgzM2pya3JuaS5hcHBzLmdvb2dsZXVzZXJjb250ZW50LmNvbSIsImlhdCI6MTQ3MTQyODM0MywiZXhwIjoxNDcxNDMxOTQzfQ.c3sFrpZmwNWKSShFBB74CXNsKdtgdASdvzQML-iYxZRiwN3vMIEopvxODbifnUkZByS7QlL7XaflORBEKM7eB3B-DARCw-3YhWveVcouxdya5G8tep3GBrbJwv50magYjXwwj9pYJr_3ru_bmDINb4QP-uapjyAhOgHinHDdVwMlypLsZNenKdgjlD5AR4xhgWCx8Vtid-LXDDOc_KXmQd-bfhB98lSSRQeMocrs8ri1H5a9t5gVa8lRbpOrYB31HKc1aj6Km5DZ1lWY1i_uKDHXgMDGYdAS9o9PUo-ElWONPJ7oHmHVIM4Y-7Xv6cuJ0zRBGRgb8u63uSnNWnHEFA

    */
    function getRequest(uri, callback) {

        const https = require("https");
        console.log(uri);
        https.get(uri,
            (res) => {
                //console.log('statusCode:', res.statusCode);
                //console.log('headers:', res.headers);

                res.on('data',
                (d) => {
                    //process.stdout.write(d);
                    callback(d, null);
                });

            })
            .on('error',
            (err) => { callback(null, err); });
    }

    var my = {};
    var validClientIDs = ["1012607661436-4d0j7s8br9311ict7pg25uj833jrkrni.apps.googleusercontent.com"];                           
    my.verifyToken = function (token, callback) {
        console.log("verifying Token!");
        getRequest(`https://www.googleapis.com/oauth2/v3/tokeninfo?id_token=${token}`,
        (result, err) => {
            if (err) {
                callback(null, err);
            } else {
                var res = JSON.parse(result);
                console.log(res);
                if (!res.aud || validClientIDs.indexOf(res.aud) < 0)
                    callback(null, null);
                else
                    callback(res, null);
            }
        });
    };

    return my;
}());
