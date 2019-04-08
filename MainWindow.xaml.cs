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

        private async void btnDrawBezier_OnClick(object sender, RoutedEventArgs e)
        {
            if (bezierDrawingArea.SplinePoints.Count < 2)
            {
                MessageBox.Show("Fill at least 2 points for bezier curve building");
                return;
            }
            
            var btn = (Button) sender;
            _cancellationTokenSource = new CancellationTokenSource();

            btn.Visibility = Visibility.Collapsed;
            await bezierDrawingArea.DrawBezierCurve(_cancellationTokenSource.Token);
            btn.Visibility = Visibility.Visible;
            
            _cancellationTokenSource = null;
        }

        private void btnStop_OnClick(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
        }

        private void btnClear_OnClick(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            bezierDrawingArea.SplinePoints.Clear();   
        }

        private void FillWithSampleData()
        {
            bezierDrawingArea.SplinePoints.Add(new Point(170, 500));
            bezierDrawingArea.SplinePoints.Add(new Point(20, 10));
            bezierDrawingArea.SplinePoints.Add(new Point(770, 25));
            bezierDrawingArea.SplinePoints.Add(new Point(1020, 500));
            bezierDrawingArea.SplinePoints.Add(new Point(1170, 100));
            bezierDrawingArea.SplinePoints.Add(new Point(360, 200));
        }
    }
}