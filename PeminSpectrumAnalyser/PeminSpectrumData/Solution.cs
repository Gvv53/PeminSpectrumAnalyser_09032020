using Communications;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace PeminSpectrumData
{
    [Serializable]
    public class Solution
    {
        public Experiment Experiment1 { get; set; } = new Experiment();
        public Experiment Experiment2 { get; set; } = new Experiment();

        public void SaveToFile(string path) => new XMLSerializer<Solution>().SaveToFile(path, this);
        public string SaveToString() => new XMLSerializer<Solution>().SaveToString(this);
        public static Solution LoadFromFile(string path) => new XMLSerializer<Solution>().ReadFromFile(path);
        public static Solution LoadFromString(string str) => new XMLSerializer<Solution>().LoadFromString(str);

        public bool ExportSolution()
        {
            bool result = false;

            SaveFileDialog fd = new SaveFileDialog();

            Experiment ExportExperiment = new Experiment();
            ExportExperiment.ExperimentSettings.ExperimentPath = Experiment1.ExperimentSettings.ExperimentPath;
            ExportExperiment.Intervals.AddRange(Experiment1.Intervals);
            ExportExperiment.Intervals.AddRange(Experiment2.Intervals);

            fd.InitialDirectory = ExportExperiment.ExperimentSettings.ExperimentPath;
            fd.DefaultExt = "EXPORT_PEMIN_DATA";
            fd.Filter = "EXPORT PEMIN DATA FILES (*.EXPORT_PEMIN_DATA)|*.EXPORT_PEMIN_DATA";
            fd.Title = "SAVE SOLUTION EXPORT PEMIN DATA FILE";

            if ((bool)fd.ShowDialog())
            {
                try
                {
                    ExportExperiment.SaveToFile(fd.FileName);

                    List<Interval> intervals = new List<Interval>();
                    intervals.AddRange(Experiment1.Intervals);
                    intervals.AddRange(Experiment2.Intervals);

                    Solution.ExportToSCV(intervals, fd.FileName + @".csv");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ОШИБКА ЗАПИСИ ФАЙЛА ЭКСПОРТА ДАННЫХ! " + ex.ToString());
                }
            }
            return result;
        }

        public  void ExportToExchangeContract(ExchangeContract exchange, bool magic = false)
        {
            try
            {
                List<Interval> intervals = new List<Interval>();
                intervals.AddRange(Experiment1.Intervals);
                intervals.AddRange(Experiment2.Intervals);

                exchange.Frequencys.Clear();
                exchange.Signal.Clear();
                exchange.Noise.Clear();
                exchange.OriginalSignal.Clear();
                exchange.OriginalNoise.Clear();

                foreach (Interval current in intervals)
                {
                    try
                    {

                        if (magic)
                        {
                            current.BeforeMagic();
                            current.MagicOn();
                        }

                        if (current.Markers.Count > 0)
                        {
                            foreach (int position in current.Markers)
                            {
                                if (position < current.Frequencys.Count)
                                    if (current.Frequencys[position] < current.IntervalSettings.FrequencyStop)
                                    {
                                        exchange.Frequencys.Add(current.Frequencys[position]);
                                        exchange.Signal.Add(position < current.Signal.Count ? current.Signal[position] : 0);
                                        exchange.Noise.Add(position < current.Noise.Count ? current.Noise[position] : 0);
                                        exchange.OriginalSignal.Add(position < current.OriginalSignal.Count ? current.OriginalSignal[position] : 0);
                                        exchange.OriginalNoise.Add(position < current.OriginalNoise.Count ? current.OriginalNoise[position] : 0);
                                    }
                            }

                        }

                        if(magic)
                        {
                            current.AfterMagic();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("ОШИБКА ПРИ ЭКСПОРТЕ" + ex.ToString());
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ОШИБКА ПРИ ЭКСПОРТЕ " + ex.ToString());
            }
        }


        public static void ExportToSCV(List<Interval> intervals, string fileNamePath)
        {
            try
            {
                FileStream f = new FileStream(fileNamePath, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(f, Encoding.Unicode);

                StringBuilder reprotString = new StringBuilder();

                reprotString.Append(@"Freq (MHz):").Append("\t")
                            .Append(@"Signal: dbMkv").Append("\t")
                            .Append(@"Noise:  dbMkv").Append("\t")
                            .Append("\t")
                            .Append(@"Orig.Signal: dbMkv").Append("\t")
                            .Append(@"Orig.Noise:  dbMkv").Append("\t");

                sw.WriteLine(reprotString);

                foreach (Interval current in intervals)
                {
                    try
                    {
                        if (current.Markers.Count > 0)
                        {
                            foreach (int position in current.Markers)
                            {
                                if (position < current.Frequencys.Count)
                                    if (current.Frequencys[position] <= current.IntervalSettings.FrequencyStop)
                                    {
                                        reprotString.Clear()
                                        .Append((current.Frequencys[position] / 1000000).ToString("F6")).Append("\t")
                                        .Append(position < current.Signal.Count ? current.Signal[position].ToString("F2") : "0").Append("\t")
                                        .Append(position < current.Noise.Count ? current.Noise[position].ToString("F2") : "0").Append("\t")
                                        .Append("\t")
                                        .Append(position < current.OriginalSignal.Count ? current.OriginalSignal[position].ToString("F2") : "0").Append("\t")
                                        .Append(position < current.OriginalNoise.Count ? current.OriginalNoise[position].ToString("F2") : "0").Append("\t");

                                        sw.WriteLine(reprotString);
                                    }
                            }

                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("ОШИБКА ПРИ ЗАПИСИ ФАЙЛА CSV" + ex.ToString());
                    }

                }
                sw.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ОШИБКА ЗАПИСИ ФАЙЛА CSV! " + ex.ToString());
            }
         }


        public string SaveSolution(string oldSolutionName,  int copyCount = 0)
        {
            string result = oldSolutionName; 

            SaveFileDialog fd = new SaveFileDialog();

            fd.InitialDirectory = Experiment1.ExperimentSettings.ExperimentPath;
            fd.DefaultExt = "SOLUTION_PEMIN_DATA";
            fd.Filter = "SOLUTION PEMIN DATA FILES (*.SOLUTION_PEMIN_DATA)|*.SOLUTION_PEMIN_DATA";
            fd.Title = "SAVE SOLUTION PEMIN DATA FILE";

            if ((bool)fd.ShowDialog())
                try
                {
                    SaveToFile(fd.FileName);
                    List<Interval> intervals = new List<Interval>();



                    foreach (var item in Experiment1.Intervals)
                        intervals.Add(item);

                    foreach (var item in Experiment2.Intervals)
                        intervals.Add(item);


                    string fileNameShort = Path.GetFileNameWithoutExtension(fd.FileName);
                    string dirName = Path.GetDirectoryName(fd.FileName);
                    string dirNameCSV = dirName + "\\CSV";

                    if (!Directory.Exists(dirNameCSV))
                        Directory.CreateDirectory(dirNameCSV);

                    ExportToSCV(intervals, dirNameCSV + "\\" + fileNameShort + ".SOLUTION_PEMIN_DATA.CSV");


                    result = fd.FileName;

                    if (copyCount > 0)
                    {
                        try
                        {
                            foreach (var item in Experiment1.Intervals)
                                item.BeforeMagic();

                            foreach (var item in Experiment2.Intervals)
                                item.BeforeMagic();


                            for (int counter = 0; counter < copyCount; counter++)
                            {
                                List<Interval> intervals_ = new List<Interval>();



                                foreach (var item in Experiment1.Intervals)
                                {
                                    item.MagicOn();
                                    intervals_.Add(item);
                                }

                                foreach (var item in Experiment2.Intervals)
                                {
                                    item.MagicOn();
                                    intervals_.Add(item);
                                }
                                                                
                                SaveToFile(dirName + "\\" + fileNameShort + "__" + counter.ToString() + ".SOLUTION_PEMIN_DATA");
                                ExportToSCV(intervals_, dirNameCSV + "\\" + fileNameShort + "__" + counter.ToString() + ".SOLUTION_PEMIN_DATA.CSV");

                            }

                            foreach (var item in Experiment1.Intervals)
                                item.AfterMagic();

                            foreach (var item in Experiment2.Intervals)
                                item.AfterMagic();
                        }
                        finally
                        {

                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ОШИБКА ЗАПИСИ ФАЙЛА ЭКСПЕРИМЕНТА! " + ex.ToString());
                }

            return result;
        }

        public static Solution LoadSolution(string experimentPath, out string currentSolutionName)
        {

            currentSolutionName = "";

            OpenFileDialog fd = new OpenFileDialog();

            fd.InitialDirectory = experimentPath;
            fd.DefaultExt = "SOLUTION_PEMIN_DATA";
            fd.Filter = "SOLUTION PEMIN DATA FILES (*.SOLUTION_PEMIN_DATA)|*.SOLUTION_PEMIN_DATA";
            fd.Title = "OPEN SOLUTION PEMIN DATA FILE";

            if ((bool)fd.ShowDialog())
            {
                Solution result = Solution.LoadFromFile(fd.FileName);

                currentSolutionName = fd.FileName;

                return result;
            }
            return null;
        }
    }
}
