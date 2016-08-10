var request = require('request');

var options = function (uri, method, obj){ 
    return {
      uri: uri,
      port: 5000,
      method: method,
      json: obj,
      rejectUnauthorized: false    
    }
};
var method = process.argv[2];
var uri = process.argv[3];
var obj = JSON.parse(process.argv[4]);
console.log("Method: " + method);
console.log("Obj: " + obj);

request(options(uri, method, obj), function (error, response, body) {
  if (error)
  {
    console.log('errors: ', error);
    return;
   }
  console.log('statusCode: ', response.statusCode);
  console.log('headers: ', response.headers);
  console.log(body);
});

