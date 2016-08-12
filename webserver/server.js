
// Init the libraries 
var fs = require('fs'),
         RestfulMongo = require('./restfulMongo');

if (fs == null || RestfulMongo == null)
    process.exit();
// ---- REST ----
var monogApi = new RestfulMongo.bridge();

if (monogApi == null)
    process.exit();

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
    var query = makeJson(req.path.substring(1));
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
    var query = makeJson(req.path.substring(1));
    var collection = db.collection('documents');

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

var PRIMARY_PORT = 4003, // For SSL, if available, otherwise for HTTP
    SECONDARY_PORT = 4004; // For HTTP if HTTPS is available, otherwise unused.
    
// Setup http and https servers, if certs.json file exists. Otherwise just the http
try
{
    var certfiles = JSON.parse(fs.readFileSync('certs.json'));
        // Setup http and https servers
    monogApi.credentials = {
      key: fs.readFileSync(certfiles.key),
      cert: fs.readFileSync(certfiles.cert)
    };

    monogApi.https_port = PRIMARY_PORT;
    monogApi.http_port = SECONDARY_PORT;

}
catch(e){
    monogApi.http_port = PRIMARY_PORT;
    monogApi.https_port = 0;
}

var mongodbUri = 'mongodb://localhost:27017/myproject';

monogApi.connect(mongodbUri, '');
monogApi.start();