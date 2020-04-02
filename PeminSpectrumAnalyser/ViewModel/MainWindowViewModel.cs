using PeminSpectrumData;
using PeminSpectrumAnalyser.Helpers;
using Communications;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;
using MessageBox = System.Windows.Forms.MessageBox;


//LINQ Delete() требует пересоздания контекста, иначе контекст не обновить!!!

namespace PeminSpectrumAnalyser.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {

     #region Поля
        PIPEClient client;
        public event Action event_1;
     #endregion Поля
     #region Свойства        
        private bool _var;
        public bool var
        {
            get
            {
                return _var;
            }
            set
            {
                _var = value;
                RaisePropertyChanged(() => var);
            }
        }
        //IP адрес ИП1
        private string _IPadres1;
        public string IPadres1
        {
            get
            {
                return _IPadres1;
            }
            set
            {
                _IPadres1 = value;
                RaisePropertyChanged(() => IPadres1);
            }
        }

        #endregion Свойства

        public MainWindowViewModel()
        {
        }
        //новый объект контекста

     #region Методы
        public void SaveData(Object o)
        { }
        public bool canCom(Object o)
        {
            return true;
        }

        public void Com(Object o)
        { }

        #endregion Методы

        #region Команды
        public ICommand SaveDataCommand { get { return new RelayCommand<Object>(SaveData); } }
        public ICommand ComCommand { get { return new RelayCommand<Object>(Com, canCom); } }
     #endregion Команды

    }
}
   
    
