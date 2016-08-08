"use strict";
// Init the libraries 
var fs = require('fs'),
    https = require('https'),
    http = require('http');


// Setup http and https servers
var credentials = {
  key: fs.readFileSync('/etc/ssl/private/server.key'),
  cert: fs.readFileSync('/etc/ssl/certs/server.crt')
};

var sessionApiURL = '/user/:userName/json';
var mongodbUri = 'mongodb://localhost:27017/myproject';

// ---- REST ----

//manipulate a users' session

var query = function(db, json, callback) {
    console.log("Querying...");
  // Get the documents collection
  var collection = db.collection('documents');
  // query some documents
  collection.find(json).toArray(function(err, docs) {
    console.log("Found the following records");
    console.dir(docs)
    callback(docs);
  });      
};

var insert = function(db, json, callback)
{
    var collection = db.collection('documents');
    // Insert some documents
    collection.insert(json, function(err, result) {
        callback(result);
    }); 
};

var update = function(db, json, callback)
{
    var collection = db.collection('documents');
      // Update some documents
    console.log ("query = " + json.query);
    console.log ("updates = " + json.updates);
    collection.update(json.query, json.updates
    , function(err, result) {
        console.log("Updated");
        callback(result);
    }); 
};

var del = function(db, json, callback)
{
    var collection = db.collection('documents');
      // Delete some documents
    collection.remove(json, function(err, result) {
        console.log("Deleted");
        callback(result);
    }); 
};



function RestfullMogo() {

    var express = require('express'),    
        bodyParser = require('body-parser');
    
    var self = this;

    this.credentials = null;
    this.app = express();

    // Prepare the this.app for json data handling
    this.app.use(bodyParser.json());
    this.db = null;

    // helper
    function createResponse(func) {

        return  function(req, res) {
            console.log("AA");
            func(self.db, req.body, function(docs) { 
                console.log(req.body);
                console.log(docs);
                res.json(docs);        
            })
        };
    }

    

    this.connect = function(mongodbUri, httpUri)
    {
        this.URI = httpUri;
        // Set up the db connection
        var MongoClient = require('mongodb').MongoClient;
        // Use connect method to connect to the server
        MongoClient.connect(mongodbUri, function(err, database) {
          console.log("Connected successfully to server");
          self.db = database;
        });

        // QUERY
        this.app.get(this.URI, createResponse(this.query));

        // UPDATE
        this.app.put(this.URI, createResponse(this.update));

        // INSERT
        this.app.post(this.URI, createResponse(this.insert));

        // DELETE
        this.app.delete(this.URI, createResponse(this.del));

        // Error handler
        function logErrors(err, req, res, next) {
          console.error(err.stack);
          next(err);
        }

        function errorHandler(err, req, res, next) {
          res.status(500);
          res.render('error', { error: err });
        }

        this.app.use(logErrors);
        this.app.use(errorHandler);    
    };

    this.start = function(servers) {
        if (arguments.length == 0)
            servers = [ 
                { 
                    connector: function(app) { return http.createServer(app); }, 
                    port: 80
                },
                {   
                    connector: function(app) 
                    { 
                        if (!self.credentials) 
                            return null;

                        return https.createServer(self.credentials, app); 
                    }, 
                    port: 443
                }
            ];

        for (let s of servers)
        {   
            var t = s.connector(this.app);
            if (t)
            {  
                t.listen(s.port);                        
                console.log("Listening on port " + s.port + "...");
            }
        }
    };
}

var monogApi = new RestfullMogo();
monogApi.credentials = credentials;
monogApi.query = query;
monogApi.update = update;
monogApi.insert = insert;
monogApi.del = del;

monogApi.connect(mongodbUri, sessionApiURL);

monogApi.start();