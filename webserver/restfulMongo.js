"use strict";
this.HTTP_PORT = 0;
this.HTTPS_PORT = 4003;
this.bridge = function() {

    var express = require('express'),    
        bodyParser = require('body-parser');
    
    var self = this;

    this.credentials = null;
    this.app = express();

    this.app.set("view engine", "jade");

    // Prepare the this.app for json data handling
    this.app.use(bodyParser.json());
    this.db = null;

    // helper
    function createResponse(func) {

        return  function(req, res) {
            if (!this.securityCheck(req))
                res.sendStatus(403);
            else
            {
                func(self.db, req, function(docs) { 
                    // Send json response
                    res.json(docs);        
                })
            }
        };
    }

    this.connect = function(mongodbUri, httpUri)
    {
        this.URI = httpUri;
        // Set up the db connection
        var MongoClient = require('mongodb').MongoClient;
        // Use connect method to connect to the server
        MongoClient.connect(mongodbUri, function(err, database) {
          console.log("Connected successfully to server.");
          self.db = database;
        });



        // UPDATE
        this.app.put(this.URI, createResponse(this.update));

        // INSERT
        this.app.post(this.URI, createResponse(this.insert));

        // DELETE
        this.app.delete(this.URI + "/:query", createResponse(this.del));

        console.log(this.URI + "/:query");

        // QUERY
        this.app.get(this.URI + "/:query", createResponse(this.query));
        
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
            var https = require('https'),
                http = require('http');

            servers = [ 
                { 
                    connector: function(app) { return http.createServer(app); }, 
                    port: HTTP_PORT
                },
                {   
                    connector: function(app) 
                    { 
                        if (!self.credentials) 
                            return null;

                        return https.createServer(self.credentials, app); 
                    }, 
                    port: HTTPS_PORT
                }
            ];

        for (let s of servers)
        {   
            var t = s.connector(this.app);
            if (t && s.port)
            {  
                t.listen(s.port);                        
                console.log("Listening on port " + s.port + "...");
            }
        }
    };
}
