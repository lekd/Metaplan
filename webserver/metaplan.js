// Metaplan logic
module.exports = (function () {

    const fs = require("fs");
    const StorageRoot = "sessions";
    const crypto = require('crypto');
    const BatchDownloader = require("./batchDownloader");

    function hash(str) {
        return crypto.createHash('sha1').update(str).digest('hex');
    }
    function getUserFolder(owner) {
        return [StorageRoot, hash(owner)].join("/");
    }

    function Metaplan(owner, sessionID) {
        this.userFolder = getUserFolder(owner);
        this.rootFolder = [this.userFolder, sessionID].join("/");
        this.snapshotFolder = [this.rootFolder, "snapshots"].join("/");
        this.noteFolder = [this.rootFolder, "notes"].join("/");
    }

    function chainFolderCreate(folder, onSuccess, onAlreadyExist, onFail) {
        fs.exists(folder,
        (exists) => {
            if (!exists) {
                fs.mkdir(folder,
                    0o774,
                    function (err) {
                        if (err) {
                            if (onFail)
                                onFail(err);
                        } else {
                            if (onSuccess)
                                onSuccess();
                        }
                    });
            } else {
                if (onAlreadyExist)
                    onAlreadyExist();
                else if (onSuccess)
                    onSuccess();
            }
        });
    }
    // Returns true on a successful creation (at least one of the directories were created
    // Returns false if folder structure already exists
    // Returns an error otherwise
    Metaplan.prototype.createSession = function (callback) {
        chainFolderCreate(this.rootFolder,
        () => {
            chainFolderCreate(this.noteFolder,
            () => {
                chainFolderCreate(this.snapshotFolder, () => { callback(true); }, (result) => { callback(false); }, (err) => { callback(err); });
            });
        });
    };

    Metaplan.prototype.storeSnapshot = function (buffer, callback) {
        const filename = `${this.snapshotFolder}/${Date.now()}.png`;
        fs.writeFile(filename,
            buffer, callback);
    };

    Metaplan.prototype.storeNote = function (noteName, buffer, callback) {
        const filename = `${this.notes}/${noteName}.png`;
        fs.writeFile(filename,
            buffer, callback);
    };
    Metaplan.prototype.sendNotes = function (lastTimeStamp, callback) {
        const b = new BatchDownloader();
        b.batchDownload(this.noteFolder,
                lastTimeStamp || null, callback);
    }
    // static methods
    Metaplan.initUser = function (userName, callback) {
        var folder = getUserFolder(userName);
        console.log(folder);
        fs.exists(folder,
        (exists) => {
            if (!exists) {
                fs.mkdir(folder,
                    0o774, callback);
            } else {
                callback(null);
            }
        });
            
    };    

    return Metaplan;
}());