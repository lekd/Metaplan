
// Init the libraries 
var fs = require('fs'),
    RestfulMongo = require('./restfulMongo')



// Setup http and https servers
var credentials = {
  key: fs.readFileSync('cert/server.key'),
  cert: fs.readFileSync('cert/server.crt')
};

var sessionApiURL = '/user/:userName/json';
var mongodbUri = 'mongodb://localhost:27017/myproject';

// ---- REST ----

var query = function(db, req, callback) {
    console.log("Querying...");
    console.log(req.params);
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

var insert = function(db, req, callback)
{
    var collection = db.collection('documents');
    var json = req.body;
    // Insert some documents
    collection.insert(json, function(err, result) {
        callback(result);
    }); 
};

var update = function(db, req, callback)
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

var del = function(db, req, callback)
{
    var collection = db.collection('documents');
    query = JSON.parse(req.params.query);
      // Delete some documents
    collection.remove(query, function(err, result) {
        console.log("Deleted");
        callback(result);
    }); 
};

var monogApi = new RestfulMongo.bridge();
monogApi.credentials = credentials;
monogApi.query = query;
monogApi.update = update;
monogApi.insert = insert;
monogApi.del = del;
monogApi.securityCheck = function(req)
{

};

monogApi.connect(mongodbUri, sessionApiURL);
monogApi.start();
