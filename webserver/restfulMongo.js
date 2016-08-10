"use strict";

this.bridge = function() {
    var _this = this;

    var express = require('express'),    
        bodyParser = require('body-parser'),
        helmet = require('helmet');
    
    this.http_port = 80;
    this.https_port = 443;    
    this.credentials = null;
    this.app = express();

    this.app.set("view engine", "jade");

    // Prepare the this.app for json data handling
    this.app.use(bodyParser.json());
    
    // Add security
    this.app.use(helmet());

    this.db = null;

    // helper
    function createResponse(func) {
        return  function(req, res) {
            console.log(req.url);
            if (!_this.securityCheck(_this.db, req))
                res.sendStatus(403);
            else
            {
                func(_this.db, req, function(docs) { 
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
          _this.db = database;
        });

        // UPDATE
        this.app.put(this.URI, createResponse(this.update));

        // INSERT
        this.app.post(this.URI, createResponse(this.insert));

        // DELETE
        this.app.delete(this.URI + "/:path*", createResponse(this.del));

        console.log(this.URI + "/:path*");
        // QUERY
        this.app.get(this.URI + "/:path*", createResponse(this.query));
        
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
                    port: _this.http_port
                },
                {   
                    connector: function(app) 
                    { 
                        if (!_this.credentials) 
                            return null;

                        return https.createServer(_this.credentials, app); 
                    }, 
                    port: _this.https_port
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
};
