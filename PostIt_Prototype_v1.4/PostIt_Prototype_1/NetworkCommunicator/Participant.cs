using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostIt_Prototype_1.NetworkCommunicator
{
    public class Participant
    {
        public string TokenID { get; set; }
        public string Email { get; set; }

        public Participant(string tokenId, string email)
        {
            TokenID = tokenId;
            Email = email;
        }


    }
}
