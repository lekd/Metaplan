
// Init the libraries 
var fs = require('fs'),
         RestfulMongo = require('./restfulMongo'),
         dbActions = require('./dbActions');

if (fs == null || RestfulMongo == null || dbActions == null)
    process.exit();

var postEvents = new dbActions(),
    preEvents = new dbActions();
// ---- REST ----
var monogApi = new RestfulMongo.bridge();

if (monogApi == null)
    process.exit();

monogApi.query = function(db, req, command, callback) {
    console.log("Query...");
    console.log(command);

    preEvents.Query(command.collection, command.query);
    // Get the documents collection
    var collection = db.collection(command.collection);
    // query some documents
    collection.find(command.query || {}).toArray(function(err, docs) {
        if (!err)        
        {
            postEvents.Query(command.collection, command.query);
            callback(docs);
        }
        else
        {
            //TODO: Response with error
        }
    
    });
};

monogApi.insert = function(db, req, command, callback)
{
    console.log("Insert...");    
    console.log (command);
    var json = req.body;    
    preEvents.Insert(command.collection, json);
    var collection = db.collection(command.collection);
    
    // Insert some documents
    collection.insert(json, function(err, result) {
        if (!err)        
        {
            postEvents.Insert(command.collection, json);
            callback(result);
        }
        else
        {
            //TODO: Response with error
        }
    }); 
};

monogApi.update = function(db, req, command, callback)
{
    console.log("Update...");    
    var json = req.body;    
    preEvents.Update(command.collection, json);
    var collection = db.collection(command.collection);

    collection.update(json.query, json.updates
    , function(err, result) {
        if (!err)        
        {
            postEvents.Update(command.collection, json);
            callback(result);
        }
        else
        {
            //TODO: Response with error
        }
    }); 
};

monogApi.del = function(db, req, command, callback)
{
    console.log("Delete...");  
    preEvents.Delete(command.collection, command.query);  
    var collection = db.collection(command.collection);

      // Delete some documents
    collection.remove(command.query || {}, function(err, result) {
        if (!err)        
        {
            postEvents.Delete(command.collection, command.query);
            callback(result);
        }
        else
        {
            //TODO: Response with error
        }
    }); 
};

monogApi.securityCheck = function(db, req)
{
    return true;
    if (!req.tokenId)
        return false;

    var collection = db.collection(command.collection);
    var query = {};
    if (req.sessionName && req.sessionOwner)
        query = {tokenId: req.tokenId}
    collection.find({tokenId: req.tokenId, sessionName: req.sessionName}).toArray(function(err, docs) {
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
var storageRoot = 'sessions';
postEvents.Query = function(collection, params) {
    console.log("postQuery:");
    console.log(collection);
    console.log(params);
}

postEvents.Insert = function(collection, params) {
    console.log("postInsert:");
    console.log(collection);
    console.log(params);
    switch(collection)
    {
        case "users":
            fs.mkdir([storageRoot, params.owner].join("/"), 0o774, function(err) { console.log(err); });
        case "sessions":
            fs.mkdir([storageRoot, params.owner, params.sessionID].join("/"), 0o774, function(err) { console.log(err); });            
    }    
}

postEvents.Update = function(collection, params) {
    // console.log("postUpdate:");
    // console.log(collection);
    // console.log(params);
}

postEvents.Delete = function(collection, params) {
    // console.log("postDelete:");
    // console.log(collection);
    // console.log(params);
}

monogApi.connect(mongodbUri, '');
monogApi.start();