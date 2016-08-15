using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhiteboardApp.NetworkCommunicator
{
    public sealed class ObservableRemoteFiles : ObservableCollection<RemoteFile>
    {
        public ObservableRemoteFiles()
        {
            CollectionChanged += ObservableRemoteFilesChanged;
        }

        public ObservableRemoteFiles(IEnumerable<RemoteFile> collection) : base(collection)
        {
        }

        public ObservableRemoteFiles(List<RemoteFile> list) : base(list)
        {
        }
        private void ObservableRemoteFilesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {                    
                    
                    ((INotifyPropertyChanged)item).PropertyChanged += ItemPropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    ((INotifyPropertyChanged)item).PropertyChanged -= ItemPropertyChanged;
                }
            }
        }

        private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
                                                            sender, 
                                                            sender, 
                                                            IndexOf((RemoteFile)sender));
            OnCollectionChanged(args);
        }

    }
}
