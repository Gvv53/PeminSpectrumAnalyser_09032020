using PeminSpectrumAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;


namespace PeminSpectrumData
{
    [Serializable]
    public class Interval
    {
        public bool isActive {get; set;}


        public double Magic { get; set; } =  PeminSpectrumAnalyser.Properties.Settings.Default.MAGIC;

        /// <summary>
        /// Зарезервировано
        /// </summary>
        public string MetaData { get; set; } = "";

        [NonSerialized]
        [XmlIgnore]
        ExperimentExplorer _ExperimentExplorer;

        public ExperimentExplorer GetExperimentExplorer() =>  _ExperimentExplorer;

        public void SetExperimentExplorer(ExperimentExplorer experimentExplorer)
        {
            _ExperimentExplorer = experimentExplorer;
            IntervalSettings.PointsQuantity = _ExperimentExplorer.Experiment.ExperimentSettings.HardwareSettings.PointsQuantity;
        }


        public IntervalSettings IntervalSettings { get; set; } = new IntervalSettings();

        public List<double> Noise { get; set; } = new List<double>();
        public List<double> Signal { get; set; } = new List<double>();
        public List<double> Frequencys { get; set; } = new List<double>();
        public List<double> OriginalNoise { get; set; } = new List<double>();
        public List<double> OriginalSignal { get; set; } = new List<double>();
        public List<int> Markers { get; set; } = new List<int>();
        public List<long> CenterFrequencys { get; set; } = new List<long>();



        public int CurrentIteration { get; set; } = 0;
        public int TotalIterations => CenterFrequencys.Count;

        public void GoToBegin() => CurrentIteration = 0;
        public void GoToEnd() => CurrentIteration = TotalIterations - 1;

        public void GoToBegin1() { if (CurrentIteration > 0) CurrentIteration--;}
        public void GoToEnd1() { if (CurrentIteration < TotalIterations - 1) CurrentIteration++; }

        public long CenterFrequency
        {
            get
            {
                if (CenterFrequencys.Count > 0)
                    return CenterFrequencys[CurrentIteration];
                else
                    return 0;}
            set
            {
                if(CenterFrequencys.Count > CurrentIteration)
                    CenterFrequencys[CurrentIteration] = value;
            }
        }

        public double[] ConvertXDataForChartView(int physicPixelsCount, List<double> data)
        {
            int coeff = data.Count / physicPixelsCount;

            List<double> result = new List<double>();

            int shift = 0;

            if (coeff > 1)
            {
                foreach (var item in data)
                {
                    if (shift == 0) result.Add(item);

                    if (shift < coeff) shift++;
                    else
                        shift = 0;
                }
                return result.ToArray();
            }
            return data.ToArray();
        }

        public double[] ConvertYDataForChartView(int physicPixelsCount, List<double> data)
        {

            if (data.Count < 1)
                return new double[0];

            int coeff = data.Count / physicPixelsCount;

            List<double> result = new List<double>();

            int shift = 0;
            double resultMax = double.MinValue;

            if (coeff > 1)
            {
                foreach (var item in data)
                {
                    if (item > resultMax) resultMax = item;

                    if (shift < coeff) shift++;
                    else
                    {
                        result.Add(resultMax);
                        resultMax = double.MinValue;
                        shift = 0;
                    }
                }
                return result.ToArray();
            }
            return data.ToArray();
        }

        public void SignalClear()
        {
            Signal.Clear();
            OriginalSignal.Clear();
        }

        public void NoiseClear()
        {
            Noise.Clear();
            OriginalNoise.Clear();
        }

        public void ClearAll()
        {
            SignalClear();
            NoiseClear();
            Frequencys.Clear();
            CenterFrequencys.Clear();
            Markers.Clear();
        }

        public void Restore()
        {
            Noise.Clear();
            Signal.Clear();

            Noise.AddRange(OriginalNoise);
            Signal.AddRange(OriginalSignal);
        }

        Random _Random = new Random();


        List<double> oldSignal = new List<double>();
        List<double> oldNoise = new List<double>();
        List<double> oldOriginalSignal = new List<double>();
        List<double> oldOriginalNoise = new List<double>();


        public void BeforeMagic()
        {
            oldSignal.Clear();
            oldNoise.Clear();
            oldOriginalSignal.Clear();
            oldOriginalNoise.Clear();



            foreach (double item in Signal)
            {
                oldSignal.Add(item);
            }

            foreach (double item in Noise)
            {
                oldNoise.Add(item);
            }

            foreach (double item in OriginalSignal)
            {
                oldOriginalSignal.Add(item);
            }

            foreach (double item in OriginalNoise)
            {
                oldOriginalNoise.Add(item);
            }

        }

        public void AfterMagic()
        {
            Signal.Clear();
            Noise.Clear();
            OriginalSignal.Clear();
            OriginalNoise.Clear();



            foreach (double item in oldSignal)
            {
                Signal.Add(item);
            }

            foreach (double item in oldNoise)
            {
                Noise.Add(item);
            }

            foreach (double item in oldOriginalSignal)
            {
                OriginalSignal.Add(item);
            }

            foreach (double item in oldOriginalNoise)
            {
                OriginalNoise.Add(item);
            }

        }

        public void MagicOn()
        {
            List<double> result = new List<double>();

            foreach (double item in OriginalSignal)
            {
                result.Add(item + _Random.NextDouble() * Magic - _Random.NextDouble() * Magic);
            }

            OriginalSignal.Clear();

            foreach (double item in result)
                OriginalSignal.Add(item);


            result.Clear();

            foreach (double item in OriginalNoise)
            {
                result.Add(item + _Random.NextDouble() * Magic - _Random.NextDouble() * Magic);
            }

            OriginalNoise.Clear();

            foreach (double item in result)
                OriginalNoise.Add(item);

            Computer(); 
        }


        public void Computer()
        {

            Restore();

            Filters filters = new Filters();

            if (IntervalSettings.MaxNoise != 0)
            {
                var buffer = Noise.ToArray();
                if (buffer.Count() > 0)
                {
                    filters.FilterMaximum(buffer, IntervalSettings.MaxNoise);
                    Noise.Clear();
                    Noise.AddRange(buffer);
                }
            }

            if (IntervalSettings.MinNoise != 0)
            {
                var buffer = Noise.ToArray();
                if (buffer.Count() > 0)
                {
                    filters.FilterMinimum(buffer, IntervalSettings.MinNoise);
                    Noise.Clear();
                    Noise.AddRange(buffer);
                }
            }

            if (IntervalSettings.MaxSignal != 0)
            {
                var buffer = Signal.ToArray();
                if (buffer.Count() > 0)
                {
                    filters.FilterMaximum(buffer, IntervalSettings.MaxSignal);
                    Signal.Clear();
                    Signal.AddRange(buffer);
                }
            }

            if (IntervalSettings.MinSignal != 0)
            {
                var buffer = Signal.ToArray();
                if (buffer.Count() > 0)
                {
                    filters.FilterMinimum(buffer, IntervalSettings.MinSignal);
                    Signal.Clear();
                    Signal.AddRange(buffer);
                }
            }

            if (IntervalSettings.ShiftNoise != 0)
            {
                var buffer = Noise.ToArray();
                if (buffer.Count() > 0)
                {
                    filters.FilterShift(buffer, IntervalSettings.ShiftNoise);
                    Noise.Clear();
                    Noise.AddRange(buffer);
                }
            }

            if (IntervalSettings.ShiftSignal != 0)
            {
                var buffer = Signal.ToArray();
                if (buffer.Count() > 0)
                {
                    filters.FilterShift(buffer, IntervalSettings.ShiftSignal);
                    Signal.Clear();
                    Signal.AddRange(buffer);
                }
            }

            if (IntervalSettings.DeltaShiftFrequency != 0)
            {
                var buffer = Signal.ToArray();
                if (buffer.Count() > 0)
                {
                    filters.FilterDeltaAfterFrequency(IntervalSettings.DeltaShiftFrequency, Frequencys.ToArray(), buffer);
                    Signal.Clear();
                    Signal.AddRange(buffer);
                }

                buffer = Noise.ToArray();
                if (buffer.Count() > 0)
                {
                    filters.FilterDeltaAfterFrequency(IntervalSettings.DeltaShiftFrequency, Frequencys.ToArray(), buffer);
                    Noise.Clear();
                    Noise.AddRange(buffer);
                }
            }

            if (IntervalSettings.SpecialSignalNoiseShift != 0)
            {
                var signals = Signal.ToArray();
                var noises = Noise.ToArray();

                filters.FilterShiftNoiseOverSignal(signals, noises, IntervalSettings.SpecialSignalNoiseShift);

                Signal.Clear();
                Signal.AddRange(signals);
                Noise.Clear();
                Noise.AddRange(noises);

            }

        }

        public void BuildAutomaticPoints()
        {
            ClearAll();

            if (!IntervalSettings.isAuto)
            {
                Markers.Add(((IntervalSettings.PointsQuantity - 1) / 2) + 1);
                CenterFrequencys.Add(IntervalSettings.HandCenterFrequency);
            }
            else
            {

                if (IntervalSettings.FrequencyInnerStep <= 0)
                {
                    MessageBox.Show("ШАГ СКАНИРОВАНИЯ ДОЛЖЕН БЫТЬ БОЛЬШЕ 0");
                    return;
                }

                if (IntervalSettings.FrequencyStart > IntervalSettings.FrequencyStop)
                {
                    MessageBox.Show("КОНЕЦ ДИАПАЗОНА ДОЛЖЕН БЫТЬ БОЛЬШЕ НАЧАЛА ДИАПАЗОНА");
                    return;
                }

                long CurrentCenterFrequency;

                long innerStepQuantity = (IntervalSettings.FrequencyStop - IntervalSettings.FrequencyStart) / IntervalSettings.FrequencyInnerStep;

                if (innerStepQuantity <= IntervalSettings.PointsQuantity - 1)
                {
                    IntervalSettings.Span = IntervalSettings.FrequencyStop - IntervalSettings.FrequencyStart;

                    CurrentCenterFrequency = IntervalSettings.FrequencyStart + IntervalSettings.Span / 2;

                    CenterFrequencys.Add(CurrentCenterFrequency);

                    for (int counter = 0; counter <= innerStepQuantity; counter++)
                    {
                        Markers.Add(counter * ((IntervalSettings.PointsQuantity - 1) / (int)innerStepQuantity));
                    }
                }
                else
                {
                    IntervalSettings.Span = (IntervalSettings.PointsQuantity - 1) * IntervalSettings.FrequencyInnerStep;

                    CurrentCenterFrequency = IntervalSettings.FrequencyStart + IntervalSettings.Span / 2;
                    CenterFrequencys.Add(CurrentCenterFrequency);

                    while ((CurrentCenterFrequency + IntervalSettings.Span / 2) < IntervalSettings.FrequencyStop)
                    {
                        CurrentCenterFrequency += IntervalSettings.Span;
                        CenterFrequencys.Add(CurrentCenterFrequency);
                    }

                    for (int counter = 0; counter <= innerStepQuantity; counter++)
                    {
                        Markers.Add(counter);
                    }
                }
            }
        }
    }
}
