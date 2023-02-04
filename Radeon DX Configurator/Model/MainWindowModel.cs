using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Radeon_DX_Configurator.Model
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<string> currentValue;
        public ObservableCollection<string> CurrentValue
        {
            get { return currentValue; }
            set
            {
                currentValue = value;
                OnPropertyChanged("CurrentValue");
            }
        }
            
        public ObservableCollection<string> currentWOWValue;
        public ObservableCollection<string> CurrentWOWValue
        {
            get { return currentWOWValue; }
            set
            {
                currentWOWValue = value;
                OnPropertyChanged("CurrentWOWValue");
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

    }
}

