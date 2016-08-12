using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhiteboardApp.NetworkCommunicator
{
    public class Participant
    {
        public string TokenID { get; set; }
        public string Email { get; set; }
        public bool IsOwner { get; private set; }
        public Participant(string tokenId, string email, bool isOwner)
        {
            TokenID = tokenId;
            Email = email;
            IsOwner = isOwner;
        }

        public async Task CreateFolder()
        {
            
        }
    }
}
