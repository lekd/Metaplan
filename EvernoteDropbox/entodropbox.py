from evernote.api.client import EvernoteClient
from evernote.edam.notestore.ttypes import NoteFilter, NotesMetadataResultSpec

import json
import sys
import dropbox
from threading import Timer

def getconfig():    
    try:    
        with open('config.json') as config_file:    
            config_json = json.load(config_file)            
            return config_json
    except Exception :
        print "ERROR: Cannot open config.json. Make sure it exists and it is a valid json file"
        print sys.exc_info()[0]
        sys.exit(-1)

def initEvernote(config):                
    client = EvernoteClient(token=config["en_devtoken"], sandbox=config["en_sandbox"]=="True")
    noteStore = client.get_note_store()
    return noteStore

def getEvernoteResources(config, notebook_guid):
    resources = []
    client = EvernoteClient(token=config["en_devtoken"], sandbox=config["en_sandbox"]=="True")
    noteStore = client.get_note_store()
    # get notes from a specific notebook
    noteFilter = NoteFilter(notebookGuid = notebook_guid)
    result_spec = NotesMetadataResultSpec(includeTitle=True)
    
    filteredNotes = noteStore.findNotesMetadata(noteFilter, 0, 10, result_spec)
    notebooks = noteStore.listNotebooks()    

    for n in filteredNotes.notes:
        print n.title, n.guid
        fullnote = noteStore.getNote(n.guid, True, True, False, True)        
        for r in fullnote.resources:
            print r.guid            
            resources += [(r.guid, r.data.body)]

    return resources

def uploadToDropbox(dpbx, resources):    
    a = dpbx.users_get_current_account() 
    for resource in resources:
        name, data = resource
        print ">>Uploading ", name
        dpbx.files_upload(data, "/fromEvernote/" + name + ".png", mode=dropbox.files.WriteMode('overwrite', None))
        print ">>Done"

def syncProcess(config, notebook_guid, dpbx, period):    

    resources = getEvernoteResources(config, notebook_guid)    
    uploadToDropbox(dpbx, resources)
    Timer(period, syncProcess, (config, notebook_guid, dpbx, period)).start()

def main():
    config = getconfig()    
    dpbx = dropbox.Dropbox(config["dpbx_accesstoken"])       
    #notestore = initEvernote(config)
    notebook_guid = config["en_notebook_guid"]
    syncProcess(config, notebook_guid, dpbx, period = 30)

if __name__ == "__main__":
    main()