using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace BezierCurve
{
    public class BezierDrawer
    {
        private const double PointStep = 0.0005;
        
        private static readonly int PrimaryLinePointsCount = (int) Math.Ceiling(1.0 / PointStep);
        private static readonly Random Rnd = new Random();

        private readonly Point[] _splineBasePoints;
        
        private readonly Point[] _workingPointSet;
        
        private readonly Dictionary<(int, int), Pen> _intermediateLinesPens = new Dictionary<(int, int), Pen>();
        
        private readonly List<Point> _primaryLine = new List<Point>(PrimaryLinePointsCount);
        
        private double _progress;

        public bool Finished => _progress >= 1;

        public IReadOnlyList<Point> PrimaryLinePoints => _primaryLine;

        public BezierDrawer(IEnumerable<Point> splineBasePoints)
        {
            _splineBasePoints = splineBasePoints.ToArray();

            if (_splineBasePoints.Length < 2)
                throw new ArgumentOutOfRangeException(nameof(splineBasePoints), "Spline points must contain at least 2 elements");
            
            _workingPointSet = new Point[_splineBasePoints.Length];
            _primaryLine.Add(_splineBasePoints[0]);
        }

        public bool MoveToNextPoint()
        {
            if (Finished)
                return false;
            
            _progress = Math.Min(_progress + PointStep, 1);
            SetupBezierLines(null);
            _primaryLine.Add(Finished ? _splineBasePoints.Last() : _workingPointSet[0]);

            return true;
        }

        public IReadOnlyList<Point> BuildSpline()
        {
            while (MoveToNextPoint()) { }

            return PrimaryLinePoints;
        }

        public void DrawIntermediateLines(DrawingContext drawingContext)
        {
            SetupBezierLines(DrawIntermediateLine);

            void DrawIntermediateLine((int lineLevel, int lineIndex) lineDescriptor, Point lineStart, Point lineEnd)
            {
                drawingContext.DrawLine(GetPenForIntermediateLine(lineDescriptor), lineStart, lineEnd);
            }
        }

        private void SetupBezierLines(Action<(int, int), Point, Point> actionForEachLine)
        {
            Array.Copy(_splineBasePoints, _workingPointSet, _workingPointSet.Length);
            
            for (var lineLevel = 1; lineLevel < _workingPointSet.Length; lineLevel++)
            for (var lineIndex = 0; lineIndex < _workingPointSet.Length - lineLevel; lineIndex++)
            {
                var lineStart = _workingPointSet[lineIndex];
                var lineEnd = _workingPointSet[lineIndex + 1];
                
                actionForEachLine?.Invoke((lineLevel, lineIndex), lineStart, lineEnd);
                
                _workingPointSet[lineIndex] = GetPointInsideLine(lineStart, lineEnd, _progress);
            }
        }

        private Pen GetPenForIntermediateLine((int lineLevel, int lineIndex) lineDescriptor)
        {
            if (_intermediateLinesPens.TryGetValue(lineDescriptor, out var pen))
                return pen;
            
            var color = Color.FromRgb((byte) Rnd.Next(256), (byte) Rnd.Next(256), (byte) Rnd.Next(256));
            _intermediateLinesPens[lineDescriptor] = pen = new Pen(new SolidColorBrush(color), 1);
            pen.Freeze();
            return pen;
        }
        
        private static Point GetPointInsideLine(Point lineStart, Point lineEnd, double t) => 
            new Point(GetInterpolated(lineStart.X, lineEnd.X, t), GetInterpolated(lineStart.Y, lineEnd.Y, t));
        
        private static double GetInterpolated(double start, double end, double t) => start + (end - start) * t;
    }
}