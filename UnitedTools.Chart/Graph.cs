using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace UnitedTools.Chart
{
    public class Graph
    {
        public List<Point> Points = new List<Point>();
        public double PointRadius { get; set; } = 1;

        public bool ContainedScreenCoordinates { get; set; } = false;

        private Brush _Color = Brushes.Black;
        public Brush Color
        {
            get => _Color;
            set
            {
                _Color = value;
                _Color.Freeze();
            }
        }
    }
}
