using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace UnitedTools.Chart
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ChartCtrl : UserControl
    {
        public Brush GraphColor = Brushes.Black;
        public Brush GridColor = Brushes.Silver;
        public Brush MarkerColor = Brushes.Maroon;
        public Brush MouseAxisColor = Brushes.Red;

        public List<Graph> Graphs = new List<Graph>();
        public List<Graph> Grids = new List<Graph>();
        public List<Graph> Markers = new List<Graph>();
        public List<Graph> MouseAxis = new List<Graph>();

        private Frame Frame;

        public void Draw()
        {
            GraphGrid.Children.Clear();
            Frame = new Frame();

            List<Graph> InGraphs = new List<Graph>();
            List<Graph> OutGraphs = new List<Graph>();

            CreateGrid();

            InGraphs.AddRange(Graphs);
            InGraphs.AddRange(Grids);
            InGraphs.AddRange(Markers);
            InGraphs.AddRange(MouseAxis);

            foreach (Graph item in InGraphs)
            {
                Graph currentScreenGraph = new Graph();

                currentScreenGraph.Color = item.Color;
                currentScreenGraph.PointRadius = item.PointRadius;

                if (item.ContainedScreenCoordinates)
                {
                    for (int counter = 0; counter < item.Points.Count; counter++)
                    {
                        Point currentPoint = new Point();


                        currentPoint.X = item.Points[counter].X;
                        currentPoint.Y = item.Points[counter].Y;
                        currentScreenGraph.Points.Add(currentPoint);
                    }
                }
                else
                {
                    for (int counter = 0; counter < item.Points.Count; counter++)
                    {
                        Point currentPoint = new Point();
                        currentPoint.X = _XRealToScreen(item.Points[counter].X);
                        currentPoint.Y = _YRealToScreen(item.Points[counter].Y);

                        currentScreenGraph.Points.Add(currentPoint);
                    }
                }

                OutGraphs.Add(currentScreenGraph);
            }

            Frame.Build(OutGraphs);

            GraphGrid.Children.Add(Frame);
        }

        public ChartCtrl()
        {
            InitializeComponent();
            MinWidth = 100;
            MinHeight = 100;
        }

        private void GraphGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _ScreenYAxisEndValue = e.NewSize.Height;
            _ScreenXAxisEndValue = e.NewSize.Width;
            RebuildConverters();
            Draw();
        }


        private double _XConverter = 1;
        private double _YConverter = 1;

        private void RebuildConverters()
        {

            _XConverter = (_XAxisEndValue - _XAxisBeginValue) / (_ScreenXAxisEndValue - _ScreenXAxisBeginValue);
            _YConverter = (_YAxisEndValue - _YAxisBeginValue) / (_ScreenYAxisEndValue - _ScreenYAxisBeginValue);

            XAxisMin.Content = _XAxisBeginValue.ToString("F2");
            XAxisMax.Content = _XAxisEndValue.ToString("F2");
            YAxisMin.Content = _YAxisBeginValue.ToString("F2");
            YAxisMax.Content = _YAxisEndValue.ToString("F2");

        }

        private double _XRealToScreen(double x) => (x - _XAxisBeginValue) / _XConverter;
        private double _YRealToScreen(double y) => (_YAxisEndValue - y) / _YConverter; 

        private double _XScreenToReal(double x) => _XAxisBeginValue + x * _XConverter;
        private double _YScreenToReal(double y) => _YAxisEndValue - y *_YConverter;

        private double _ScreenXAxisBeginValue = 0;
        private double _ScreenXAxisEndValue = 100;
        private double _ScreenYAxisBeginValue = 0;
        private double _ScreenYAxisEndValue = 100;


       



        private double _XAxisBeginValue
        {
            get => __XAxisBeginValue - ((__XAxisEndValue - __XAxisEndValue) / 100 * _XScalePercentShift);
            set => __XAxisBeginValue = value;
        }

        private double _XAxisEndValue
        {
            get => __XAxisEndValue + ((__XAxisEndValue - __XAxisEndValue) / 100 * _XScalePercentShift);
            set => __XAxisEndValue = value;
        }

        private double _YAxisBeginValue
        {
            get => __YAxisBeginValue - ((__YAxisEndValue - __YAxisEndValue) / 100 * _YScalePercentShift);
            set => __YAxisBeginValue = value;
        }

        private double _YAxisEndValue
        {
            get => __YAxisEndValue + ((__YAxisEndValue - __YAxisEndValue) / 100 * _YScalePercentShift);
            set => __YAxisEndValue = value;
        }

        private double __XAxisBeginValue = 0;
        private double __XAxisEndValue = 100;
        private double __YAxisBeginValue = 0;
        private double __YAxisEndValue = 100;

        double _XScalePercentShift;
        double _YScalePercentShift;

        public void SetScalePercentX (double signedScalePercent)
        {
            if(signedScalePercent != Double.MinValue)
             _XScalePercentShift = (((__XAxisEndValue - __XAxisEndValue) / 100) * signedScalePercent);
        }

        public void SetScalePercentY(double signedScalePercent)
        {
            if (signedScalePercent != Double.MinValue)
                _YScalePercentShift = (((__YAxisEndValue - __YAxisEndValue) / 100) * signedScalePercent);
        }

        public double XAxisBeginValue
        {
            set
            {
                if (__XAxisBeginValue != value)
                {
                    __XAxisBeginValue = value;

                    _XAxisBeginValue = value;

                    RebuildConverters();
                    Draw();
                }
            }
            get => _XAxisBeginValue;
        }


        public double XAxisEndValue
        {
            set
            {
                if (__XAxisEndValue != value)
                {
                    __XAxisEndValue = value;


                    _XAxisEndValue = value;


                    RebuildConverters();
                    Draw();
                }
            }
            get =>  _XAxisEndValue;
        }

        public  double YAxisBeginValue
        {
            set
            {
                if (__YAxisBeginValue != value)
                {
                    __YAxisBeginValue = value;

                    _YAxisBeginValue = value;

                    RebuildConverters();
                    Draw();
                }
            }
            get => _YAxisBeginValue;
        }

        public  double YAxisEndValue
        {
            set
            {
                if (__YAxisEndValue != value)
                {
                    __YAxisEndValue = value;

                    _YAxisEndValue = value;

                    RebuildConverters();
                    Draw();
                }
            }
            get => _YAxisEndValue;
        }

        private void GraphGrid_MouseMove(object sender, MouseEventArgs e)
        {
            XRealMouse.Content = _XScreenToReal(e.GetPosition(GraphGrid).X).ToString("F2");
            YRealMouse.Content = _YScreenToReal(e.GetPosition(GraphGrid).Y).ToString("F2");

            CreateMouseAxis(e.GetPosition(GraphGrid).X, e.GetPosition(GraphGrid).Y);
            Draw();

            if (e.LeftButton == MouseButtonState.Pressed)
                NewXShift?.Invoke(_XScreenToReal(e.GetPosition(GraphGrid).X));
        }

        public Action<double> NewXShift;


        private void CreateMouseAxis(double x, double y)
        {
            MouseAxis.Clear();

            Graph YCenter = new Graph();
            YCenter.ContainedScreenCoordinates = true;
            YCenter.Color = MouseAxisColor;
            YCenter.Points.Add(new Point(_ScreenXAxisBeginValue, y));
            YCenter.Points.Add(new Point(_ScreenXAxisEndValue, y));

            Graph XCenter = new Graph();
            XCenter.ContainedScreenCoordinates = true;
            XCenter.Color = MouseAxisColor;
            XCenter.Points.Add(new Point(x, _ScreenYAxisBeginValue));
            XCenter.Points.Add(new Point(x, _ScreenYAxisEndValue));

            MouseAxis.Add(YCenter);
            MouseAxis.Add(XCenter);
        }

        private void CreateMouseMarker(double x, double y)
        {
            Markers.Clear();

            Graph XCenter = new Graph();
            XCenter.ContainedScreenCoordinates = true;
            XCenter.Color = MouseAxisColor;
            XCenter.Points.Add(new Point(x, _ScreenYAxisBeginValue));
            XCenter.Points.Add(new Point(x, _ScreenYAxisEndValue));

            MouseAxis.Add(XCenter);
        }





        private void CreateGrid()
        {
            Grids.Clear();

            Graph left = new Graph();
            left.Color = GridColor;
            left.Points.Add(new Point(_XAxisBeginValue, _YAxisBeginValue));
            left.Points.Add(new Point(_XAxisBeginValue, _YAxisEndValue));

            Graph right = new Graph();
            right.Color = GridColor;
            right.Points.Add(new Point(_XAxisEndValue, _YAxisBeginValue));
            right.Points.Add(new Point(_XAxisEndValue, _YAxisEndValue));

            Graph top = new Graph();
            top.Color = GridColor;
            top.Points.Add(new Point(_XAxisBeginValue, _YAxisEndValue));
            top.Points.Add(new Point(_XAxisEndValue, _YAxisEndValue));

            Graph bottom = new Graph();
            bottom.Color = GridColor;
            bottom.Points.Add(new Point(_XAxisBeginValue, _YAxisBeginValue));
            bottom.Points.Add(new Point(_XAxisEndValue, _YAxisBeginValue));

            Graph YCenter = new Graph();
            YCenter.Color = GridColor;
            YCenter.Points.Add(new Point(_XAxisBeginValue, 0));
            YCenter.Points.Add(new Point(_XAxisEndValue, 0));

            double YGridStep = (_YAxisEndValue - _YAxisBeginValue) / 10;
            for(int counter = 0; counter < 10; counter++)
            {
                Graph YGrid = new Graph();
                YGrid.Color = GridColor;
                YGrid.Points.Add(new Point(_XAxisBeginValue, _YAxisBeginValue + YGridStep * counter));
                YGrid.Points.Add(new Point(_XAxisEndValue, _YAxisBeginValue + YGridStep * counter));
                Grids.Add(YGrid);


            }

            double XGridStep = (_XAxisEndValue - _XAxisBeginValue) / 10;
            for (int counter = 0; counter < 10; counter++)
            {
                Graph XGrid = new Graph();
                XGrid.Color = GridColor;
                XGrid.Points.Add(new Point(_XAxisBeginValue + XGridStep * counter, _YAxisBeginValue));
                XGrid.Points.Add(new Point(_XAxisBeginValue + XGridStep * counter, _YAxisEndValue));
                Grids.Add(XGrid);
            }

            Graph XCenter = new Graph();
            XCenter.Color = GridColor;
            XCenter.Points.Add(new Point(_XAxisEndValue - ((_XAxisEndValue - _XAxisBeginValue) / 2), _YAxisBeginValue));
            XCenter.Points.Add(new Point(_XAxisEndValue - ((_XAxisEndValue - _XAxisBeginValue) / 2), _YAxisEndValue));

            Grids.Add(left);
            Grids.Add(right);
            Grids.Add(top);
            Grids.Add(bottom);
            Grids.Add(YCenter);
            Grids.Add(XCenter);
        }

        private void GraphGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CreateMouseMarker(e.GetPosition(GraphGrid).X, e.GetPosition(GraphGrid).Y);
        }
    }
}
