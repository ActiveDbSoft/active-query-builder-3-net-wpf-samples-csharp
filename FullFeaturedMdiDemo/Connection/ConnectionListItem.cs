//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2021 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.ComponentModel;
using FullFeaturedMdiDemo.Annotations;

namespace FullFeaturedMdiDemo.Connection
{
    public class ConnectionListItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _name;
        private string _type;
        private object _tag;
        private string _userQueries;

        public string Name
        {
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
            get { return _name; }
        }

        public string Type
        {
            set
            {
                _type = value;
                OnPropertyChanged("Type");
            }
            get { return _type; }
        }

        public object Tag
        {
            set
            {
                _tag = value;
                OnPropertyChanged("Tag");
            }
            get { return _tag; }
        }

        public string UserQueries
        {
            set
            {
                _userQueries = value;
                OnPropertyChanged("UserQueries");
            }
            get { return _userQueries; }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
