using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;

namespace PostIt_Prototype_1.NetworkCommunicator
{
    public class ParticipantManager
    {
        // How authentication works:
        // In order to ensure all the participants are authenticated, the moderator adds a list of participants 
        // who can access the session using their Gmail accounts.
        // The participants who want to join also type in the Gmail account of the moderator on their client apps
        // The root session folder is named as the moderator's Google ID.
        // 
        // When a participant creates and uploads a file, her Google ID is also attached to the file as a header,
        // and it is inserted into the session only if he or she is on the list.
        
        static ParticipantManager()
        {
            
        }
    }
}
