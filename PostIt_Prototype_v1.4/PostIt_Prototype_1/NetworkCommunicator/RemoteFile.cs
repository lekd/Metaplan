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

        public long ModifiedDate { get; set; }

        public byte[] Content;

        public string Type;

        private INotifyPropertyChanged _notifyPropertyChangedImplementation;

        public RemoteFile(byte[] content, string name, long modifiedDate, string type)
        {
            Content = content;
            Name = name;
            ModifiedDate = modifiedDate;
            Type = type;
        }

        public RemoteFile(JToken jToken)

        {
            Content = Convert.FromBase64String(jToken["content"].ToString());
            Name = jToken["name"]?.ToString();
            ModifiedDate = (long)jToken["modifiedDate"];
            Type = jToken["type"]?.ToString();
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            var e = obj as RemoteFile;
            
            return e != null && ((e.Name == this.Name) && (e.ModifiedDate == this.ModifiedDate));
        }
    }
}
