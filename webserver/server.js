
// Init the libraries 
var fs = require('fs'),
         RestfulMongo = require('./restfulMongo');

// ---- REST ----
var monogApi = new RestfulMongo.bridge();

function makeJson(string)
{    
    var stringArray = string.split('/');
    var j = {};
    for (var i=0; i < stringArray.length/2; i++)
        j[stringArray[i*2]] = stringArray[i*2+1];

    return j;
}

monogApi.query = function(db, req, callback) {
    console.log("Querying...");
    console.log((req.path.substring(1)));
    console.log(makeJson(req.path.substring(1)));
    query = JSON.parse(req.params.query);
  // Get the documents collection
  var collection = db.collection('documents');
  // query some documents
  collection.find(query).toArray(function(err, docs) {
    console.log("Found the following records");
    console.dir(docs)
    callback(docs);
  });      
};

monogApi.insert = function(db, req, callback)
{
    var collection = db.collection('documents');
    var json = req.body;
    // Insert some documents
    collection.insert(json, function(err, result) {
        callback(result);
    }); 
};

monogApi.update = function(db, req, callback)
{
    var collection = db.collection('documents');
    var json = req.body;
      // Update some documents
    console.log ("query = " + json.query);
    console.log ("updates = " + json.updates);
    collection.update(json.query, json.updates
    , function(err, result) {
        console.log("Updated");
        callback(result);
    }); 
};

monogApi.del = function(db, req, callback)
{
    var collection = db.collection('documents');
    query = JSON.parse(req.params.query);
      // Delete some documents
    collection.remove(query, function(err, result) {
        console.log("Deleted");
        callback(result);
    }); 
};

monogApi.securityCheck = function(db, req)
{
    return true;
    if (!req.tokenId)
        return false;

    var collection = db.collection('documents');
    var query = {};
    if (req.sessionName && req.sessionOwner)
        query = {tokenId: req.tokenId}
    collection.find({tokenId: req.tokenId, sessionName: req.sessionName}).toArray(function(err, docs) {
    console.log("Found the following records");
    console.dir(docs)
    callback(docs);
  });      
};

var testServer = false;
if (testServer == true)
{
    // Setup http and https servers
    monogApi.credentials = {
      key: fs.readFileSync('cert/server.key'),
      cert: fs.readFileSync('cert/server.crt')
    };

    monogApi.http_port = 0;
    monogApi.https_port = 4003;
}
else
{
    monogApi.http_port = 5000;
    monogApi.https_port = 0;
}

var sessionApiURL = '/user';
var mongodbUri = 'mongodb://localhost:27017/myproject';
monogApi.connect(mongodbUri, sessionApiURL);
monogApi.start();
