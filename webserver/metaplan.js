// Metaplan logic
module.exports = (function () {
    var my = {};
    const fs = require("fs");
    const StorageRoot = "sessions";
    var snapshotFolder, noteFolder;

    my.createSession = function (owner, sessionID) {
        var rootFolder = [StorageRoot, owner, sessionID].join("/");
        snapshotFolder = [rootFolder, "snapshots"].join("/");
        noteFolder = [rootFolder, "notes"].join("/");

        fs.mkdir(rootFolder,
            0o774,
            function(err) { console.log(err); });
        fs.mkdir(noteFolder,
            0o774,
            function(err) { console.log(err); });
        fs.mkdir(snapshotFolder,
            0o774,
            function (err) { console.log(err); });


    };

    my.createUser = function (userName) {
        fs.mkdir([StorageRoot, userName].join("/"),
            0o774,
            function(err) { console.log(err); });
    }

    my.storeSnapshot = function(buffer) {
        const filename = `${snapshotFolder}/${Date.now()}.png`;
        fs.writeFile(filename,
            buffer,
            (err) => {
                if (err)
                    console.log(`Error ${err}`);
            });
    };

    return my;
}());