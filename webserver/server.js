"use strict";

// Init the libraries 
const Fs = require("fs");
const RestfulMongo = require("./restfulMongo");
const DbActions = require("./dbActions");

const verifier = require("./tokenVerifier");
if (Fs == null || RestfulMongo == null || DbActions == null) process.exit();

const PostEvents = new DbActions();
const PreEvents = new DbActions();
// ---- REST ----
const MonogApi = new RestfulMongo.Bridge();

if (MonogApi == null)
    process.exit();

/* Response to a path query:
 If path is a directory, send the list of files in the directory
    if lastTimeStamp parameter is present, only send newer files than lastTimeStamp
    If path is a list of files, download them. */

function handleCallbacks(res, action) {
    return (result, err, stop) => {
        if (err)
            res.status(500).send(err);
        else {
            if (stop)
                res.json(result);
            else
                action();
        }
    };
}

MonogApi.query = function (db, req, command, res) {
    console.log("Query...");
    console.log(command);
    switch (command.collection) {
        case "files":
            var Metaplan = new metaplan(command.query.owner, command.query.sessionID);
            console.log(command.query);

            Metaplan.sendNotes(command.query.lastTimeStamp,
                function (result, err) {
                    if (err)
                        res.status(500).send(err);
                    else {
                        res.send(result);
                    }
                });
            break;
        case "verify":
            console.log("VERIFY!");
            verifier.verifyToken(command.query.id_token, (result, err) => {
                if (!err) {
                    console.log(result);
                    // Now, result could be false, meaning token was invalid, or true, meaning that token was valid                    
                    // if token is valid
                    if (result) {
                        metaplan.initUser(result.email, (err) => {
                            if (err)
                                console.log(err);
                            else {
                                console.log("User initialized!");
                                res.send([{ response: result.email }]);
                            }
                        });
                        
                    } else
                        res.send([{ response: "INVALID" }]);                    
                } else {
                    console.log(err);
                    res.status(500).send(err);
                }

            });
            break;

        default:
            // Get the documents collection
            const collection = db.collection(command.collection);
            // query some documents
            collection
                .find(command.query || {})
                .toArray(
                    function (err, docs) {
                        if (!err) {
                            PostEvents.Query(command.collection, command.query, (result, err) => {
                                if (err)
                                    res.status(500).send(err);
                                else {
                                    res.json(docs);
                                }
                            });
                            res.json(docs);
                        } else {
                            res.status(500).send(err);
                        }
                    }
                );
    }
};

MonogApi.insert = function (db, req, command, res) {
    console.log("Insert...");
    console.log(command);
    var json = req.body;
    switch (command.collection) {
        case "users":
            console.log(metaplan);
            metaplan.initUser(json.userName, (err) => {
                if (err)
                    console.log(err);
                else {
                    console.log("User created successfully.");
                }
            });
            break;
        case "sessions":
            var Metaplan = new metaplan(json.owner, json.sessionID);
            console.log(Metaplan);
            Metaplan.createSession((result) => {
                if (result === true)
                    console.log("Session created successfully.");
                else if (result === false)
                    console.log("Session already exists.");
                else
                    console.log(result);
            });
            break;
        case "files":
            var Metaplan = new metaplan(json.owner, json.sessionID);
            console.log(Metaplan);
            Metaplan.createSession((result) => {
                if (result === true)
                    console.log("Session created successfully.");
                else if (result === false)
                    console.log("Session already exists.");
                else
                    console.log(result);
            });
            break;
    }
    // Insert into db
    const collection = db.collection(command.collection);
    collection.insert(json,
        function (err, result) {
            if (!err) {
                PostEvents.Insert(command.collection, json);
                res.json(result);
            } else {
                res.status(500).send(err);
            }
        });
};


MonogApi.update = function (db, req, command, res) {
    console.log("Update...");
    var json = req.body;
    console.log(json);

    const collection = db.collection(command.collection);
    console.log(collection);
    collection.update(json.query,
        json.updates,
        function (err, result) {
            if (!err) {
                PostEvents.Update(command.collection, json);
                res.json(result);
            } else {
                res.status(500).send(err);
            }
        });
};

MonogApi.del = function (db, req, command, res) {
    console.log("Delete...");

    const collection = db.collection(command.collection);

    // Delete some documents
    collection.remove(command.query || {}, function (err, result) {
        if (!err) {
            PostEvents.Delete(command.collection, command.query);
            res.json(result);
        }
        else {
            res.status(500).send(err);
        }
    });
};

MonogApi.securityCheck = function (db, req) {
    return true;
    if (!req.tokenId)
        return false;

    const collection = db.collection(command.collection);
    var query = {};
    if (req.sessionName && req.sessionOwner)
        query = { tokenId: req.tokenId }
    collection.find({ tokenId: req.tokenId, sessionName: req.sessionName }).toArray(function (err, docs) {
        res.json(docs);
    });
};

var PrimaryPort = 4003, // For SSL, if available, otherwise for HTTP
    SecondaryPort = 4004; // For HTTP if HTTPS is available, otherwise unused.

// Setup http and https servers, if certs.json file exists. Otherwise just the http
try {
    var Certfiles = JSON.parse(Fs.readFileSync("certs.json"));
    // Setup http and https servers
    MonogApi.credentials = {
        key: Fs.readFileSync(Certfiles.key),
        cert: Fs.readFileSync(Certfiles.cert)
    };

    MonogApi.https_port = PrimaryPort;
    MonogApi.http_port = SecondaryPort;
}

catch (E) {
    MonogApi.http_port = PrimaryPort;
    MonogApi.https_port = 0;
}

var metaplan = require("./metaplan");

var MongodbUri = "mongodb://localhost:27017/myproject";
var StorageRoot = "sessions";

MonogApi.connect(MongodbUri, "");
MonogApi.start();