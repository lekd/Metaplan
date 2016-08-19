"use strict";

this.Bridge = function () {
    const _this = this;

    const express = require("express"),
        bodyParser = require("body-parser");
    //helmet = require( "helmet" );

    this.http_port = 80;
    this.https_port = 443;
    this.credentials = null;
    this.app = express();

    this.app.set("view engine", "jade");

    // Prepare the this.app for json data handling
    this.app.use(bodyParser.json({ limit: '50mb' }));
    this.app.use(bodyParser.urlencoded({ limit: '50mb', extended: true }));
    // Add security
    //this.app.use(helmet());

    this.db = null;

    function makeJson(string) {
        var json = {};
        //console.log("makeJson of "" + string + """);
        if (string !== null && string !== "" && string != "/" && string != undefined) {
            var stringArray = string.split("/");
            json.collection = stringArray[0];
            var query = {};
            let i = 1, j = 2;
            while (j < stringArray.length) {
                let key = stringArray[i];
                let val = stringArray[j];
                if (val.startsWith("$")) {
                    val = { val: stringArray[j + 1] };
                    j++;
                }
                console.log(key);
                console.log(val);
                query[key] = JSON.parse(val);
                i = j + 1;
                j = i + 1;
            }

            json.query = query;
        }
        return json;
    }
    // helper
    function createResponse(func) {
        return function (req, res) {
            console.log("<decoded url>");
            console.log(decodeURI(req.url));
            console.log("<decoded path>");
            console.log(decodeURI(req.path));

            var command = (req.path === null) ? null : makeJson(decodeURI(req.path).substring(1));
            console.log("<command>");
            console.log(command);
            if (!_this.securityCheck(_this.db, command, req))
                res.sendStatus(403);
            else {
                func(_this.db, req, command, res);
            }
        };
    }

    this.connect = function (mongodbUri, httpUri) {
        this.URI = httpUri;
        // Set up the db connection
        var MongoClient = require("mongodb").MongoClient;
        // Use connect method to connect to the server
        MongoClient.connect(mongodbUri, function (err, database) {
            console.log("Connected successfully to server.");
            _this.db = database;
        });

        // UPDATE
        this.app.put([this.URI, this.URI + "/:path*"], createResponse(this.update));

        // INSERT
        this.app.post([this.URI, this.URI + "/:path*"], createResponse(this.insert));

        // DELETE
        this.app.delete([this.URI, this.URI + "/:path*"], createResponse(this.del));

        console.log(this.URI + "/:path*");
        // QUERY
        this.app.get([this.URI, this.URI + "/:path*"], createResponse(this.query));

        // Error handler
        function logErrors(err, req, res, next) {
            console.error(err.stack);
            next(err);
        }

        function errorHandler(err, req, res, next) {
            res.status(500);
            res.render("error", { error: err });
        }

        this.app.use(logErrors);
        this.app.use(errorHandler);
    };

    this.start = function (servers) {

        if (arguments.length === 0)
            var https = require("https"),
                http = require("http");

        servers = [
            {
                connector: function (app) { return http.createServer(app); },
                port: _this.http_port
            },
            {
                connector: function (app) {
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
            if (t && s.port) {
                t.listen(s.port);
                console.log("Listening on port " + s.port + "...");
            }
        }
    };
};
