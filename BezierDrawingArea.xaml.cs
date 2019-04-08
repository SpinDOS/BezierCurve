using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BezierCurve
{
    public partial class BezierDrawingArea : UserControl
    {
        private const double EllipseRadius = 4;
        
        private int _capturedPointIndex = -1;
        private bool _isMouseMove;
        private Point _initialMousePosition;
        
        private BezierDrawer _bezierDrawer;

        public BezierDrawingArea()
        {
            InitializeComponent();
            SplinePoints.CollectionChanged += SplinePointsOnCollectionChanged;
        }

        public bool DrawingInProgress { get; private set; }
        
        public ObservableCollection<Point> SplinePoints { get; } = new ObservableCollection<Point>();

        public async Task DrawBezierCurve(CancellationToken cancellationToken)
        {
            var bezierDrawer = _bezierDrawer = new BezierDrawer(SplinePoints);
            DrawingInProgress = true;
            
            while (!cancellationToken.IsCancellationRequested && bezierDrawer.MoveToNextPoint())
            {
                InvalidateVisual();
                try { await Task.Delay(5, cancellationToken); }
                catch (TaskCanceledException) { }
            }

            DrawingInProgress = false;
            InvalidateVisual();
        }
        
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            _bezierDrawer?.Draw(drawingContext, DrawingInProgress);
        }

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
                case NotifyCollectionChangedAction.Reset:
                    _canvas.Children.Clear();
                    _bezierDrawer = null;
                    InvalidateVisual();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    _canvas.Children.RemoveAt(e.OldStartingIndex * 2);
                    _canvas.Children.RemoveAt(e.OldStartingIndex * 2);
                    FixLabels(e.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    FixPositions(e.NewStartingIndex);
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

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (DrawingInProgress || e.ChangedButton != MouseButton.Left || !this.CaptureMouse())
                return;
            
            _isMouseMove = false;
            var mousePosition = _initialMousePosition = e.GetPosition(this);
            
            var nearestPoint = SplinePoints.OrderBy(point => DistanceSquare(point, mousePosition)).FirstOrDefault();

            if (IsNear(mousePosition, nearestPoint))
                _capturedPointIndex = SplinePoints.IndexOf(nearestPoint);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (IsMouseCaptured)
                MovePointToMouse(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            if (!IsMouseCaptured)
                return;

            MovePointToMouse(e);
            this.ReleaseMouseCapture();

            var capturedPointIndex = _capturedPointIndex;
            _capturedPointIndex = -1;

            if (_isMouseMove)
                return;
            
            if (capturedPointIndex < 0)
                SplinePoints.Add(e.GetPosition(this));
            else
                SplinePoints.RemoveAt(capturedPointIndex);
        }

        private void MovePointToMouse(MouseEventArgs e)
        {
            if (_isMouseMove && _capturedPointIndex < 0) // no need to calculate anything
                return;
            
            var mousePosition = e.GetPosition(this);
            _isMouseMove = _isMouseMove || !IsNear(_initialMousePosition, mousePosition);

            if (_capturedPointIndex < 0)
                return;
            
            var x = Math.Max(0, Math.Min(this.ActualWidth, mousePosition.X));
            var y = Math.Max(0, Math.Min(this.ActualHeight, mousePosition.Y));
            SplinePoints[_capturedPointIndex] = new Point(x, y);
        }

        private void FixPositions(int index)
        {
            var point = SplinePoints[index];
            
            var label = (Label) _canvas.Children[2 * index];
            label.SetValue(Canvas.LeftProperty, point.X);
            label.SetValue(Canvas.TopProperty, point.Y);

            var ellipse = (Ellipse) _canvas.Children[2 * index + 1];
            ellipse.SetValue(Canvas.LeftProperty, point.X - EllipseRadius);
            ellipse.SetValue(Canvas.TopProperty, point.Y - EllipseRadius);
        }

        private void FixLabels(int startIndex) => FixLabels(startIndex, SplinePoints.Count);

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

        private static double DistanceSquare(Point point1, Point point2) =>
            Square(point2.X - point1.X) + Square(point2.Y - point1.Y);

        private static double Square(double x) => x * x;

        private static bool IsNear(Point point1, Point point2) => 
            DistanceSquare(point1, point2) <= (2 * EllipseRadius) * (2 * EllipseRadius);
    }
}
