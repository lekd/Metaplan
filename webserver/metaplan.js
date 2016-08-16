// Metaplan logic
module.exports = (function () {

    const fs = require("fs");
    const StorageRoot = "sessions";

    function Metaplan(owner, sessionID) {
        this.rootFolder = [StorageRoot, owner, sessionID].join("/");
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

    // static methods
    Metaplan.createUser = function (userName, callback) {
        fs.mkdir([StorageRoot, userName].join("/"),
            0o774, callback);
    };

    return Metaplan;
}());