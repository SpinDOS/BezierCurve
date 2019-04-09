using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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

        private async void btnDrawBezier_OnClick(object sender, RoutedEventArgs e)
        {
            if (_bezierDrawingArea.SplineBasePoints.Count < 2)
            {
                MessageBox.Show("Fill at least 2 points for bezier curve building");
                return;
            }
            
            var btn = (Button) sender;
            _cancellationTokenSource = new CancellationTokenSource();

            btn.Visibility = Visibility.Collapsed;
            await _bezierDrawingArea.DrawBezierCurve(_cancellationTokenSource.Token);
            btn.Visibility = Visibility.Visible;
            
            _cancellationTokenSource = null;
        }

        private void btnStop_OnClick(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
        }

        private async void btnClear_OnClick(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();

            while (_bezierDrawingArea.DrawingInProgress)
                await Task.Yield();
            
            _bezierDrawingArea.SplineBasePoints.Clear();   
        }

        private void FillWithSampleData()
        {
            _bezierDrawingArea.SplineBasePoints.Add(new Point(170, 500));
            _bezierDrawingArea.SplineBasePoints.Add(new Point(20, 10));
            _bezierDrawingArea.SplineBasePoints.Add(new Point(770, 25));
            _bezierDrawingArea.SplineBasePoints.Add(new Point(1020, 500));
            _bezierDrawingArea.SplineBasePoints.Add(new Point(1170, 100));
            _bezierDrawingArea.SplineBasePoints.Add(new Point(360, 200));
        }
    }
}