using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BezierCurve
{
    public partial class BezierDrawingArea : UserControl
    {
        private const double NearDistanceSquare = (2 * EllipseRadius) * (2 * EllipseRadius);
        
        private bool _isMouseMove;
        private Point _initialMousePosition;
        private int _capturedPointIndex;

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (DrawingInProgress || e.ChangedButton != MouseButton.Left)
                return;

            var mousePosition = e.GetPosition(this);
            
            _isMouseMove = false;
            _initialMousePosition = mousePosition;
            _capturedPointIndex = FindCapturedPointIndex(mousePosition);
            
            this.CaptureMouse();
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
            
            if (!_isMouseMove)
                AddOrRemovePoint(e);
        }

        private void AddOrRemovePoint(MouseButtonEventArgs e)
        {
            if (_capturedPointIndex < 0)
                _splineBasePoints.Add(e.GetPosition(this));
            else
                _splineBasePoints.RemoveAt(_capturedPointIndex);
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
            _splineBasePoints[_capturedPointIndex] = new Point(x, y);
        }

        private int FindCapturedPointIndex(Point mousePosition)
        {
            var result = -1;
            var bestDistanceSquare = NearDistanceSquare;
            
            for(var i = 0; i < _splineBasePoints.Count; i++)
            {
                var distanceSquare = DistanceSquare(mousePosition, _splineBasePoints[i]);
                if (distanceSquare < bestDistanceSquare)
                {
                    bestDistanceSquare = distanceSquare;
                    result = i;
                }
            }

            return result;
        }

        private static double DistanceSquare(Point point1, Point point2) =>
            Square(point2.X - point1.X) + Square(point2.Y - point1.Y);

        private static double Square(double x) => x * x;

        private static bool IsNear(Point point1, Point point2) => DistanceSquare(point1, point2) < NearDistanceSquare;
    }
}