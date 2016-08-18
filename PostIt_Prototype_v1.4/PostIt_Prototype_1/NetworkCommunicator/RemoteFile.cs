﻿using System;
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

        public string Type;

        private INotifyPropertyChanged _notifyPropertyChangedImplementation;

        public RemoteFile(byte[] content, string name, string modifiedTime, string type)
        {
            Content = content;
            Name = name;
            ModifiedTime = modifiedTime;
            Type = type;
        }

        public RemoteFile(JToken jToken)

        {
            Content = Convert.FromBase64String(jToken["content"].ToString());
            Name = jToken["name"].ToString();
            ModifiedTime = jToken["modifiedTime"].ToString();
            Type = jToken["type"].ToString();
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
