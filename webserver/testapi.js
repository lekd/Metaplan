var request = require('request');

var options = function (method, obj){ 
    return {
      uri: 'https://127.0.0.1/user/ali/json/',
      port: 443,
      method: method,
      rejectUnauthorized: false,
      json: obj
    }
};
var method = process.argv[2];
var obj = JSON.parse(process.argv[3]);
console.log("Method: " + method);
console.log("Obj: " + obj);

request(options(method, obj), function (error, response, body) {
  if (error)
  {
    console.log('errors: ', error);
    return;
   }
  console.log('statusCode: ', response.statusCode);
  console.log('headers: ', response.headers);
  console.log(body);
});

