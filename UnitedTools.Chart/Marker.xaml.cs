using System;
using System.Windows;
using System.Windows.Controls;

namespace UnitedTools.Chart
{
    /// <summary>
    /// Interaction logic for Marker.xaml
    /// </summary>
    public partial class Marker : UserControl
    {

        public Action<Marker> StartEdit;

        Point[] _Points;

        public int MarkedItemInMarkedGraph { get; set; }

        public void SetGraph(Graph graph) => _Points = graph.Points.ToArray();
        


        //public double XCoordinate
        //{
        //    get
        //    {
        //        if (_Points != null)
        //            if (MarkedItemInMarkedGraph < _Points.Count)
        //            {


        //            }

        //        return _XCoordinate;
        //    }
        //}

        //public double YCoordinate
        //{

        //    get
        //    {
        //        return _YCoordinate;
        //    }
        //}





        public Marker()
        {
            InitializeComponent();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            StartEdit?.Invoke(this);
        }
    }
}
