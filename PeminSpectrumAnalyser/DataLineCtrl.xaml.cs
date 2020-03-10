using Communications;
using System;
using System.Windows;
using System.Windows.Controls;

namespace PeminSpectrumAnalyser
{
    /// <summary>
    /// Interaction logic for DataLineCtrl.xaml
    /// </summary>
    public partial class DataLineCtrl : UserControl
    {
        ExchangeContract _ExchangeContract = new ExchangeContract();


        public ExchangeContract GetExchangeContract()
        {
            return _ExchangeContract;
        }

        public void SetID(int id)
        {
            IDLabel.Content = id;
            _ExchangeContract.ID = id;
        }

        public void SetR2(double r2)
        {
            R2.Content = r2.ToString();
        }

        public int GetID() => _ExchangeContract.ID;

        public void SetDescription(string description)
        {
            DescriptionTextBox.Text = description;
            _ExchangeContract.Description = description;
        }

        public Action Delete;

        public Action<ExchangeContract> SendExchangeContractEvent;
        public Action<ExchangeContract, bool> GetExchangeContractEvent;


        public DataLineCtrl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// SendSignalData
        /// </summary>
        private void buttonSend_Click(object sender, RoutedEventArgs e)
        {
            _ExchangeContract.Description = DescriptionTextBox.Text;
            SendExchangeContractEvent?.Invoke(_ExchangeContract);
        }

        /// <summary>
        /// Close tool
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e) =>  Delete?.Invoke();

        private void GetDataButton_Click(object sender, RoutedEventArgs e)
        {
            bool magic = false;
            if (Magic.IsChecked == true)
                magic = true;


                GetExchangeContractEvent?.Invoke(_ExchangeContract, magic);
            HaveDataLabel.Content = DateTime.Now.ToString();
        }
    }
}
