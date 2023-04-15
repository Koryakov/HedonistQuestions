using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hedonist.Wpf {
    public class State : INotifyPropertyChanged {
        public bool IsSelected {
            get { return _isSelected; }
            set { _isSelected = value; FirePropertyChanged("IsSelected"); }
        }

        private bool _isSelected;

        public int StateId {
            get { return _stateId; }
            set { _stateId = value; FirePropertyChanged("StateId"); }
        }

        public string StateName {
            get { return _stateName; }
            set { _stateName = value; FirePropertyChanged("StateName"); }
        }

        private int _stateId;

        private string _stateName;

        public event PropertyChangedEventHandler PropertyChanged;

        private void FirePropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class StateViewModel : INotifyPropertyChanged {
        private List<State> _states;

        public List<State> States {
            get { return _states; }
            set { _states = value; FirePropertyChanged("States"); }
        }

        public StateViewModel() {
            States = new List<State>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void FirePropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
