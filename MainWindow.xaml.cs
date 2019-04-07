using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BezierCurve
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private CancellationTokenSource _cancellationTokenSource;
        
        public MainWindow()
        {
            InitializeComponent();
            
            FillWithSampleData();
        }

        private void btnStop_OnClick(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
        }

        private async void btnDrawBezier_OnClick(object sender, RoutedEventArgs e)
        {
            var btn = (Button) sender;
            _cancellationTokenSource = new CancellationTokenSource();

            btn.Visibility = Visibility.Collapsed;
            await bezierDrawingArea.DrawBezierCurve(_cancellationTokenSource.Token);
            btn.Visibility = Visibility.Visible;
            
            _cancellationTokenSource = null;
        }

        private void btnClear_OnClick(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            bezierDrawingArea.Clear();   
        }

        private void BezierDrawer_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            bezierDrawingArea.AddOrRemoveSplinePoint(e.GetPosition(bezierDrawingArea));
        }

        private void FillWithSampleData()
        {
            bezierDrawingArea.AddOrRemoveSplinePoint(new Point(170, 500));
            bezierDrawingArea.AddOrRemoveSplinePoint(new Point(20, 10));
            bezierDrawingArea.AddOrRemoveSplinePoint(new Point(770, 25));
            bezierDrawingArea.AddOrRemoveSplinePoint(new Point(1020, 500));
            bezierDrawingArea.AddOrRemoveSplinePoint(new Point(1170, 100));
            bezierDrawingArea.AddOrRemoveSplinePoint(new Point(360, 200));
        }
    }
}