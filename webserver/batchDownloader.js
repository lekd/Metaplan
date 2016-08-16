module.exports.batchDownload = batchDownload;

var fs = require('fs');
function batchDownload(path, lastTimeStamp, callback) {
    console.log(path);
    console.log(lastTimeStamp);
    fs.stat(path,
        function (err, stats) {
            if (err) {
                callback(null, err);
                return;
            }

            console.log(stats);
            if (stats.isDirectory()) {
                var result = [];
                fs.readdir(path,
                    function (err, files) {
                        if (files.length === 0)
                            callback(null, `No files found!`);
                        files.forEach(
                    		function (file) {
                    		    // TODO: Change to path.join
                    		    var fullPath = path + "/" + file; 
                    		    console.log(fullPath);
                    		    fs.stat(fullPath, filterFiles(fullPath, files.length, callback));
                    		});
                    }
                );
            }
            else if (stats.isFile()) {
                callback(null, `${path} is file.`);
            }
            else
                callback(null, "Path not supported!");
        }
    );
}

function filterFiles(path, totalFiles, callback) {
    return (err, stats) => {
        // TODO: Should I send partially prepared result?
        if (err) {
            callback(null, err);
            return;
        }
        filterFiles.Result.push({ FullPath: path, ModifiedTime: stats.mtime });
        filterFiles.Counter++;
        if (filterFiles.Counter == totalFiles)
            downloadFiles(filterFiles.Result, callback);
    }
}
filterFiles.Result = [];
filterFiles.Counter = 0;

function downloadFiles(files, callback) {
    files.forEach(
		function (file) {
		    console.log(file);
		    fs.readFile(file.FullPath,
	            function (err, data) {
	                if (err) {
	                    callback(null, err);
	                    return;
	                }

	                file.Content = data;
	                downloadFiles.Result.push(file);
	                // If this is the last file, send result                                                   
	                // checks if this was the last item in the files
	                downloadFiles.Counter++;
	                if (downloadFiles.Counter == files.length)
	                    callback(files, null);
	            }
		    );
		});
}

downloadFiles.Result = [];
downloadFiles.Counter = 0;
