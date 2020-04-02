using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace UnitedTools.Chart
{
    public class Frame : FrameworkElement
    {
        DrawingGroup _DrawingGroup = new DrawingGroup();

        public Frame()
        {
        }

        public bool ShowOnlyPoints = false;

        private double _LastXValue = 0;
        private double _LastYValue = 0;

        public double YAxisMinValue = -100;

        public virtual void Build(List<Graph> graphs)
        {
            if (graphs.Count > 0)
            {
                _DrawingGroup.Children.Clear();

                foreach (Graph item in graphs)
                {
                    if (item.Points.Count > 0)
                        for (int counter = 0; counter < item.Points.Count; counter++)
                        {
                            if (ShowOnlyPoints)
                            {

                                EllipseGeometry point = new EllipseGeometry(item.Points[counter], item.PointRadius, item.PointRadius);
                                Brush brush = item.Color;
                                brush.Freeze();

                                GeometryDrawing drawing = new GeometryDrawing(brush, null, point);

                                _DrawingGroup.Children.Add(drawing);
                            }
                            else
                            {
                                if(counter > 0)
                                {
                                    LineGeometry line = new LineGeometry();
                                    line.StartPoint = new Point(_LastXValue, _LastYValue);
                                    line.EndPoint = new Point(item.Points[counter].X, item.Points[counter].Y);

                                    Pen pen = new Pen(item.Color, 1);

                                    Brush brush = item.Color;
                                    brush.Freeze();

                                    GeometryDrawing drawing = new GeometryDrawing(brush, pen, line);

                                    _DrawingGroup.Children.Add(drawing);

                                }
                                _LastXValue = item.Points[counter].X;
                                _LastYValue = item.Points[counter].Y;
                            }
                        }
                }
            }


        }

        public virtual void Clear()
        {
            _DrawingGroup.Children.Clear();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            drawingContext.DrawDrawing(_DrawingGroup);
        }
    }
}
