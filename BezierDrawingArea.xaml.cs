using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BezierCurve
{
    public partial class BezierDrawingArea : UserControl
    {
        private const double EllipseRadius = 4;
        
        private static readonly Pen PrimaryLinePen = InitPrimaryLinePen();
        
        private readonly FreezableCollection<Point> _splineBasePoints = new FreezableCollection<Point>();
        
        private BezierDrawer _bezierDrawer;

        public BezierDrawingArea()
        {
            InitializeComponent();
            _splineBasePoints.CollectionChanged += SplinePointsOnCollectionChanged;
        }

        public bool DrawingInProgress => _splineBasePoints.Frozen;

        public ObservableCollection<Point> SplineBasePoints => _splineBasePoints;

        public async Task DrawBezierCurve(CancellationToken cancellationToken)
        {
            using (_splineBasePoints.Frost())
            {
                var bezierDrawer = _bezierDrawer = new BezierDrawer(_splineBasePoints);
                while (!cancellationToken.IsCancellationRequested && bezierDrawer.MoveToNextPoint())
                {
                    InvalidateVisual();
                    try { await Task.Delay(5, cancellationToken); }
                    catch (TaskCanceledException) { }
                }
            }
            
            InvalidateVisual();
        }
        
        protected override void OnRender(DrawingContext drawingContext)
        {
            RenderBezierLine(drawingContext, _bezierDrawer);
            base.OnRender(drawingContext);
        }

        private void RenderBezierLine(DrawingContext drawingContext, BezierDrawer bezierDrawer)
        {
            if (bezierDrawer == null)
                return;
            
            if (DrawingInProgress)
                bezierDrawer.DrawIntermediateLines(drawingContext);
            
            var primaryLine = bezierDrawer.PrimaryLinePoints;
            
            for (var i = 1; i < primaryLine.Count; i++)
                drawingContext.DrawLine(PrimaryLinePen, primaryLine[i - 1], primaryLine[i]);
                
            if (!bezierDrawer.Finished)
                drawingContext.DrawEllipse(Brushes.Green, null, primaryLine.Last(), EllipseRadius, EllipseRadius);
        }
        
        private static Pen InitPrimaryLinePen()
        {
            var pen = new Pen(Brushes.Black, 2)
            {
                LineJoin = PenLineJoin.Round,
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round,
            };
            
            pen.Freeze();
            return pen;
        }
    }
}
