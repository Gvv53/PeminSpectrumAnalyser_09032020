using System;
using System.Collections.Generic;

namespace PeminSpectrumData
{
    [Serializable]
    public class Experiment
    {
        //Тактовая частота для ДС
        public long Ft
        {
            set;
            get;
        }
        /// <summary>
        /// Описание проводимых измерений
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// Комментарии к проведенным измерениям
        /// </summary>
        public string Comments { get; set; } = "";

        /// <summary>
        /// Зарезервировано
        /// </summary>
        public string MetaData { get; set; } = "";

        /// <summary>
        /// Настройки эксперимента, коэффициенты, параметры аппаратуры и проч.
        /// </summary>
        public ExperimentSettings ExperimentSettings { get; set; } = new ExperimentSettings();

        /// <summary>
        /// Собранные данные
        /// </summary>
        public List<Interval> Intervals { get; set; } = new List<Interval>();

        public List<ReportLine> GetReport()
        {
            List<ReportLine> results = new List<ReportLine>();

            foreach (Interval currentInterval in Intervals)
                if (currentInterval.Markers.Count > 0)
                    foreach (int position in currentInterval.Markers)
                        if ((position < currentInterval.Frequencys.Count) && 
                            (currentInterval.Frequencys[position] < currentInterval.IntervalSettings.FrequencyStop))
                            {
                                ReportLine reportLine = new ReportLine();
                                    
                                reportLine.Frequency = currentInterval.Frequencys[position];

                                if (position < currentInterval.Signal.Count)
                                    reportLine.Signal = currentInterval.Signal[position];

                                if (position < currentInterval.Noise.Count)
                                    reportLine.Noise = currentInterval.Noise[position];

                                results.Add(reportLine);
                            }
            return results;
        }

        public void SaveToFile(string path) => new XMLSerializer<Experiment>().SaveToFile(path, this);
        public string SaveToString() => new XMLSerializer<Experiment>().SaveToString(this);
        public static Experiment LoadFromFile(string path) => new XMLSerializer<Experiment>().ReadFromFile(path);
        public static Experiment LoadFromString(string str) => new XMLSerializer<Experiment>().LoadFromString(str);
    }
}
