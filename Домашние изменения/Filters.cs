using System;
using System.Linq;

namespace PeminSpectrumData
{
    public class Filters
    {

        Random _Rnd = new Random();


        public void FilterMaximum(double[] data, double value)
        {
            for (int counter = 0; counter < data.Count(); counter++)
                if (data[counter] > value)
                {
                    double bufferValue = value;

                    bufferValue = _Rnd.NextDouble() * 2;

                    data[counter] = value - bufferValue;
                }
        }

        public void FilterMinimum(double[] data, double value)
        {
            for (int counter = 0; counter < data.Count(); counter++)
                if (data[counter] < value)
                {
                    double bufferValue = value;

                    bufferValue = _Rnd.NextDouble() * 2;

                    data[counter] = value + bufferValue; 
                }
        }

        public void FilterShift(double[] data, double value)
        {
            for (int counter = 0; counter < data.Count(); counter++)
                data[counter] += value;
        }


        Random rnd = new Random();
        public void FilterShiftNoiseOverSignal(double[] signal, double[] noise, double value)
        {
            if (signal?.Count() == 0)
                return;

            if (noise?.Count() == 0)
                return;

            //double signalMid = 0;

            //for (int counter = 0; counter < signal.Count(); counter++)
            //{
            //    signalMid += signal[counter];
            //    if (counter > 0)
            //        signalMid /= 2;
            //}

            //double noiseMid = 0;

            //for (int counter = 0; counter < noise.Count(); counter++)
            //{
            //    noiseMid += noise[counter];
            //    if (counter > 0)
            //        noiseMid /= 2;
            //}

            //if (signalMid - noiseMid > value)
            //{
            //    for (int counter = 0; counter < noise.Count(); counter++)
            //    {
            //        signal[counter] -= (signalMid - noiseMid - value);
            //    }
            //}

            //if (signalMid - noiseMid < value)
            //{
            //    for (int counter = 0; counter < noise.Count(); counter++)
            //    {
            //        signal[counter] += (value - (signalMid - noiseMid));
            //    }
            //}


            for (int counter = 0; counter < signal.Count(); counter++)
            {
                if ((signal[counter] - noise[counter] < value) && ((signal[counter] - noise[counter] > 0.5 * value)))
                    signal[counter] = signal[counter] -  0.5 * value;


                if (signal[counter] - noise[counter] >= value)
                    signal[counter] = noise[counter] + value - (0.1 * rnd.NextDouble());

                if (signal[counter] < noise[counter] )
                    signal[counter] = noise[counter] + (0.1 * rnd.NextDouble());

            }
        }

        public void FilterDeltaAfterFrequency(double frequency, double[] frequencys, double[] data)
        {

            double firstHalf = 0;  //сумма сигналов, не превышающих частоту предусилителя
            double secondHalf = 0; //сумма сигналов,  превышающих частоту предусилителя
            int firstHalfCounter = 0;
            int secondHalfCounter = 0;
            double secondMiddle = 0;
            double firstMiddle = 0;
            double resultShift;
            int position = 0;

            foreach (double item in frequencys)
            {
                if (item <= frequency)
                {
                    firstHalf += data[position];
                    firstHalfCounter++;
                }

                if (item > frequency)
                {
                    secondHalf += data[position];
                    secondHalfCounter++;
                }
                position++;
            }

            if (firstHalfCounter != 0)
                firstMiddle = firstHalf / firstHalfCounter; //среднее значение сигналов, не превышающих заданное значение frequency
            if (secondHalfCounter != 0)
               secondMiddle = secondHalf / secondHalfCounter; //среднее значение сигналов, превышающих заданное значение frequency
            resultShift = secondMiddle - firstMiddle;  //разница средних

            position = 0;
            foreach (double item in frequencys)
            {
                if (item > frequency)
                {
                    data[position] = data[position] - resultShift; // значение на частоте, превышающей частоту предусилителя = среднему значению всех сигналов
                                                                   //правильно ли это?
                }
                position++;
            }
        }
    }
}
