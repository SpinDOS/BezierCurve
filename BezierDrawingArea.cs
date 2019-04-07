using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BezierCurve
{
    public class BezierDrawingArea : Canvas
    {
        private readonly List<Point> _splinePoints = new List<Point>();

        private BezierDrawer _bezierDrawer;
        
        public bool Drawing { get; private set; }

        public async Task DrawBezierCurve(CancellationToken cancellationToken)
        {
            if (_splinePoints.Count < 2)
            {
                MessageBox.Show("Fill bezier curve base points");
                return;
            }
            
            var bezierDrawer = _bezierDrawer = new BezierDrawer(_splinePoints);
            Drawing = true;
            
            while (!cancellationToken.IsCancellationRequested && bezierDrawer.MoveToNextPoint())
            {
                InvalidateVisual();
                try { await Task.Delay(5, cancellationToken); }
                catch (TaskCanceledException) { }
            }

            Drawing = false;
            InvalidateVisual();
        }

        public void AddOrRemoveSplinePoint(Point point)
        {
            if (Drawing)
                return;

            var removed = false;
            while (true)
            {
                var indexToRemove = _splinePoints.FindIndex(p => IsNear(p, point));
                if (indexToRemove < 0)
                {
                    break;
                }

                removed = true;
                for (var i = indexToRemove + 1; i < _splinePoints.Count; i++)
                {
                    ((Label) Children[i * 2]).Content = i;
                }
                
                _splinePoints.RemoveAt(indexToRemove);
                Children.RemoveAt(indexToRemove * 2);
                Children.RemoveAt(indexToRemove * 2);
            }

            if (removed)
                return;
            
            _splinePoints.Add(point);
            Children.Add(CreateLabel(point));
            Children.Add(CreateEllipse(point));
        }

        public void Clear()
        {
            _bezierDrawer = null;
            _splinePoints.Clear();
            Children.Clear();
            InvalidateVisual();
        }
        
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            _bezierDrawer?.Draw(drawingContext, Drawing);
        }

        private Ellipse CreateEllipse(Point point)
        {
            const double radius = 4;
            var ellipse = new Ellipse();
            ellipse.Fill = Brushes.Red;
            ellipse.Width = ellipse.Height = radius * 2;
            ellipse.SetValue(Canvas.LeftProperty, point.X - radius);
            ellipse.SetValue(Canvas.TopProperty, point.Y - radius);
            return ellipse;
        }

        private Label CreateLabel(Point point)
        {
            var label = new Label();
            label.Content = _splinePoints.Count;
            label.SetValue(Canvas.LeftProperty, point.X);
            label.SetValue(Canvas.TopProperty, point.Y);
            return label;
        }

        private static bool IsNear(Point point1, Point point2) => 
            Math.Abs(Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2)) < 20;
    }
}