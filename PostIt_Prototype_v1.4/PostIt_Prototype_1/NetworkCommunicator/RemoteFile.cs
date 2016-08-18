using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using WhiteboardApp.Annotations;

namespace WhiteboardApp.NetworkCommunicator
{
    public class RemoteFile : INotifyPropertyChanged

    {
        public string Name { get; set; }

        public string ModifiedTime { get; set; }

        public byte[] Content;
        private INotifyPropertyChanged _notifyPropertyChangedImplementation;

        public RemoteFile(byte[] content, string name, string modifiedTime)
        {
            Content = content;
            Name = name;
            ModifiedTime = modifiedTime;
        }

        public RemoteFile(JToken jToken) :
            this((from b in jToken["Content"]["data"] select (byte)b).ToArray(),
                    jToken["FullPath"].ToString(),
                    jToken["ModifiedTime"].ToString())
        { }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
