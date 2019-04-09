using System;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BezierCurve
{
    public partial class BezierDrawingArea : UserControl
    {
        private void SplinePointsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    _canvas.Children.Insert(e.NewStartingIndex * 2, CreateLabel());
                    _canvas.Children.Insert(e.NewStartingIndex * 2 + 1, CreateEllipse());
                    FixPositions(e.NewStartingIndex);
                    FixLabels(e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    _canvas.Children.RemoveAt(e.OldStartingIndex * 2);
                    _canvas.Children.RemoveAt(e.OldStartingIndex * 2);
                    FixLabels(e.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    FixPositions(e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _canvas.Children.Clear();
                    _bezierDrawer = null;
                    InvalidateVisual();
                    break;
                case NotifyCollectionChangedAction.Move:
                    var indexToRemove = e.OldStartingIndex * 2;
                    
                    var label = _canvas.Children[indexToRemove];
                    _canvas.Children.RemoveAt(indexToRemove);
                    var ellipse = _canvas.Children[indexToRemove];
                    _canvas.Children.RemoveAt(indexToRemove);
                    
                    _canvas.Children.Insert(e.NewStartingIndex * 2, label);
                    _canvas.Children.Insert(e.NewStartingIndex * 2 + 1, ellipse);

                    var minIndex = Math.Min(e.NewStartingIndex, e.OldStartingIndex);
                    var maxIndex = Math.Max(e.NewStartingIndex, e.OldStartingIndex);
                    
                    FixLabels(minIndex, maxIndex + 1);
                    break;
            }
        }

        private void FixPositions(int index)
        {
            var point = _splineBasePoints[index];
            
            var label = (Label) _canvas.Children[2 * index];
            label.SetValue(Canvas.LeftProperty, point.X);
            label.SetValue(Canvas.TopProperty, point.Y);

            var ellipse = (Ellipse) _canvas.Children[2 * index + 1];
            ellipse.SetValue(Canvas.LeftProperty, point.X - EllipseRadius);
            ellipse.SetValue(Canvas.TopProperty, point.Y - EllipseRadius);
        }

        private void FixLabels(int startIndex) => FixLabels(startIndex, _splineBasePoints.Count);

        private void FixLabels(int startIndex, int endIndex)
        {
            for (var i = startIndex; i < endIndex; i++)
            {
                var label = (Label) _canvas.Children[2 * i];
                label.Content = (i + 1).ToString();
            }
        }

        private static Label CreateLabel() => new Label();
        
        private static Ellipse CreateEllipse() =>
            new Ellipse { Fill = Brushes.Red, Height = 2 * EllipseRadius, Width = 2 * EllipseRadius, };
    }
}