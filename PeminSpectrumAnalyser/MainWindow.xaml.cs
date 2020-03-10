using Communications;
using IOMeasurementData;
using PeminSpectrumData;
using System;
using System.Windows;
using System.Windows.Media;

namespace PeminSpectrumAnalyser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Solution Solution = new Solution();

        PIPEService Service = null;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                Service = new PIPEService();
            }
            catch (Exception ex)
            {
                MessageBox.Show(" Возникло исключение при вызове конструктора PIPEService().   ТЕКСТ ИСКЛЮЧЕНИЯ:  " + ex.ToString() + " ВНУТРЕННЕЕ ИСКЛЮЧЕНИЕ:  " + ex.InnerException.Message);
            }

            Service.IncomingExchangeContract += (data) =>
            {
                try
                { 
                    if (data == null)
                    {
                        MessageBox.Show("пришедшие данные == null");
                        return;
                    }

                    CreateNewDataLine(data.ID, data.Description);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(" Возникло исключение при вызове SendExchangeContract.   ТЕКСТ ИСКЛЮЧЕНИЯ:  " + ex.ToString() + " ВНУТРЕННЕЕ ИСКЛЮЧЕНИЕ:  " + ex.InnerException.Message);
                }
            };

            Service.IncomingR2 += (id, r2) =>
            {
                try
                {
                    SetR2(id, r2);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(" Возникло исключение при вызове IncomingR2.   ТЕКСТ ИСКЛЮЧЕНИЯ:  " + ex.ToString() + " ВНУТРЕННЕЕ ИСКЛЮЧЕНИЕ:  " + ex.InnerException.Message);
                }
            };


            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            unit1.ExperimentExplorer.Experiment = Solution.Experiment1;
            unit2.ExperimentExplorer.Experiment = Solution.Experiment2;

            NewSolution();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //this.DragMove();
        }

        private void NewSolution()
        {
            unit1.NewExperiment();
            unit2.NewExperiment();
            unit1.AddNewInterval();
            unit2.AddNewInterval();

            unit1.ExperimentExplorer.Experiment.ExperimentSettings.HardwareSettings.IP = "192.168.12.233";
            unit2.ExperimentExplorer.Experiment.ExperimentSettings.HardwareSettings.IP = "192.168.12.234";

            Solution.Experiment1 = unit1.ExperimentExplorer.Experiment;
            Solution.Experiment2 = unit2.ExperimentExplorer.Experiment;
        }

        private void FullNew_Click(object sender, RoutedEventArgs e)
        {
            NewSolution();
        }

        private void FullLoad_Click(object sender, RoutedEventArgs e)
        {


            string solutionName = "";
            Solution = Solution.LoadSolution(unit1.ExperimentExplorer.Experiment.ExperimentSettings.ExperimentPath, out solutionName);

            CurrentSolutionLabel.Content = solutionName;

            if (Solution != null)
            {

                unit1.ClearIntervalsUIList();
                unit2.ClearIntervalsUIList();

                unit1.ExperimentExplorer.Experiment = Solution.Experiment1;
                unit2.ExperimentExplorer.Experiment = Solution.Experiment2;

                if (Solution.Experiment1.Intervals.Count > 0)
                    for (int counter = 0; counter < Solution.Experiment1.Intervals.Count; counter++)
                        unit1.LoadInterval(Solution.Experiment1.Intervals[counter]);

                if (Solution.Experiment2.Intervals.Count > 0)
                    for (int counter = 0; counter < Solution.Experiment2.Intervals.Count; counter++)
                        unit2.LoadInterval(Solution.Experiment2.Intervals[counter]);

                unit1.AddressRefresh();
                unit2.AddressRefresh();
            }
        }


        private void FullSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Solution.Experiment1 = unit1.ExperimentExplorer.Experiment;
            }
            catch
            {
                MessageBox.Show("В левом блоке отсутствуют данные по измерениям. Сохранение решения невозможно");
                return;
            }

            try
            {
                Solution.Experiment2 = unit2.ExperimentExplorer.Experiment;
            }
            catch
            {
                MessageBox.Show("В правом блоке отсутствуют данные по измерениям. Сохранение решения невозможно");
                return;
            }



            int copyCount = 0;

            try
            {
                copyCount = int.Parse(CopyTextBox.Text);
            }
            catch
            {
                MessageBox.Show("Ошибка при вводе количества копий");
            }
            finally
            {
                CurrentSolutionLabel.Content = Solution.SaveSolution(CurrentSolutionLabel.Content.ToString(), copyCount);
            }
        }


        private void FullNoise_Click(object sender, RoutedEventArgs e)
        {
            unit1.Connect();
            unit2.Connect();
            unit1.StartNoiseScan();
            unit2.StartNoiseScan();
        }

        private void FullSignal_Click(object sender, RoutedEventArgs e)
        {
            unit1.Connect();
            unit2.Connect();
            unit1.StartSignalScan();
            unit2.StartSignalScan();
        }

        private void FullExport_Click(object sender, RoutedEventArgs e)
        {
            Solution.ExportSolution();
        }


        private void MenuItemHide_Click(object sender, RoutedEventArgs e)
        {
            Topmost = true;
            Top = 0;
            Height = 115;
            this.MaxHeight = 115;
            this.MinHeight = 115;
            WindowStyle = WindowStyle.None;
        }

        private void MenuItemShow_Click(object sender, RoutedEventArgs e)
        {
            this.MaxHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            Height = System.Windows.SystemParameters.PrimaryScreenHeight;
            Topmost = false;
            WindowStyle = WindowStyle.ToolWindow;
        }

        private void FullConnect_Click(object sender, RoutedEventArgs e)
        {
            unit1.ExperimentExplorer.SequenceCtrl.Disconnect();
            unit2.ExperimentExplorer.SequenceCtrl.Disconnect();

            unit1.ExperimentExplorer.SequenceCtrl.Connect();
            unit2.ExperimentExplorer.SequenceCtrl.Connect();
        }

        private void NewDataLine_Click(object sender, RoutedEventArgs e)
        {
            CreateNewDataLine();
        }

        private void SendAllDataLines_Click(object sender, RoutedEventArgs e)
        {

        }

        //---------------------------------------------------------------------
        // Взаимодействие с программой расчетов
       
        private void SetR2(int id, double r2)
        {
            foreach (DataLineCtrl item in DataLinesListView.Items)
            {
                if (item.GetID() == id)
                    item.SetR2(r2);
            }
        }

        private void CreateNewDataLine(int id = 0, string description = "Блок данных создан без описания")
        {
            if(id != 0)
                foreach (DataLineCtrl item in DataLinesListView.Items)
                    if (item.GetID() == id)
                        return;


            DataLineCtrl dataLine = new DataLineCtrl();

            dataLine.SetID(id);
            dataLine.SetDescription(description);

            dataLine.Delete += () => 
            { 
                 DataLinesListView.Items.Remove(dataLine); 
            };

            dataLine.GetExchangeContractEvent += (exchangeData, magic) =>
            {
                Solution.ExportToExchangeContract(exchangeData, magic);
            };

            dataLine.SendExchangeContractEvent += (exchangeData) =>
            {
                if (ImplementationService.Callback == null)
                {
                    MessageBox.Show("Программа расчетов по методикам не запущена или не подключена к программе измерений сигнала!");
                    return;
                }

                try
                {
                    Service.SendExchangeContract(exchangeData);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(" Возникло исключение при вызове SendExchangeContract.   ТЕКСТ ИСКЛЮЧЕНИЯ:  " + ex.ToString() +  " ВНУТРЕННЕЕ ИСКЛЮЧЕНИЕ:  " + ex.InnerException.Message);
                }
             
            };

            DataLinesListView.Items.Add(dataLine);
        }

        private void fullClear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (Interval item in Solution?.Experiment1?.Intervals)
                    item.ClearAll();

                foreach (Interval item in Solution?.Experiment2?.Intervals)
                    item.ClearAll();
            }
            finally
            {
                MessageBox.Show("Очистка завершена");
            }
        }

        private void NotChangeRBW_Click(object sender, RoutedEventArgs e)
        {
            RBWAndVBW.RBW = (bool)NotChangeRBW.IsChecked;
        }

        private void NotChangeVBW_Click(object sender, RoutedEventArgs e)
        {
            RBWAndVBW.VBW = (bool)NotChangeVBW.IsChecked;
        }
    }
}
