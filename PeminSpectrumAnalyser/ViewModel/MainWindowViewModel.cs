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


//LINQ Delete() ������� ������������ ���������, ����� �������� �� ��������!!!

namespace PeminSpectrumAnalyser.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {

     #region ����
        PIPEClient client;
        public event Action event_1;
     #endregion ����
     #region ��������        
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
        //IP ����� ��1
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

        #endregion ��������

        public MainWindowViewModel()
        {
        }
        //����� ������ ���������

     #region ������
        public void SaveData(Object o)
        { }
        public bool canCom(Object o)
        {
            return true;
        }

        public void Com(Object o)
        { }

        #endregion ������

        #region �������
        public ICommand SaveDataCommand { get { return new RelayCommand<Object>(SaveData); } }
        public ICommand ComCommand { get { return new RelayCommand<Object>(Com, canCom); } }
     #endregion �������

    }
}
   
    
