using KidClock.Modules.Games.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace KidClock.Modules.Games.Views
{
    public partial class TimeDetectiveView : UserControl
    {
        private bool _isDragging = false;
        private double _lastAngle = 0;

        public TimeDetectiveView()
        {
            InitializeComponent();
            GenerateTicks();
        }

        private void GenerateTicks()
        {
            TicksContainer.Children.Clear();
            
            // 表盘参数
            double radius = 250;
            double centerX = 250;
            double centerY = 250;

            for (int i = 0; i < 60; i++)
            {
                bool isHourTick = i % 5 == 0;
                double angle = i * 6; // 每分钟 6 度

                // 1. 绘制刻度线
                Rectangle tick = new Rectangle();
                tick.Width = isHourTick ? 6 : 2;
                tick.Height = isHourTick ? 20 : 10;
                tick.Fill = Brushes.Black;

                // 设置渲染中心
                tick.RenderTransformOrigin = new Point(0.5, 0.5);

                // 计算位置
                double distanceFromEdge = 10;
                double tickCenterRadius = radius - distanceFromEdge - tick.Height / 2;
                
                // 角度转弧度 (0度在12点方向，需要减90度)
                double rad = (angle - 90) * Math.PI / 180;

                double x = centerX + tickCenterRadius * Math.Cos(rad) - tick.Width / 2;
                double y = centerY + tickCenterRadius * Math.Sin(rad) - tick.Height / 2;

                tick.Margin = new Thickness(x, y, 0, 0);
                tick.HorizontalAlignment = HorizontalAlignment.Left;
                tick.VerticalAlignment = VerticalAlignment.Top;
                tick.RenderTransform = new RotateTransform(angle);

                TicksContainer.Children.Add(tick);

                // 2. 绘制数字 (仅整点)
                if (isHourTick)
                {
                    int hour = i / 5;
                    if (hour == 0) hour = 12;

                    TextBlock text = new TextBlock();
                    text.Text = hour.ToString();
                    text.FontSize = 32;
                    text.FontWeight = FontWeights.Bold;
                    text.Foreground = Brushes.Black;

                    // 测量文本大小
                    text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    Size textSize = text.DesiredSize;

                    // 数字半径
                    double textRadius = radius - 50;

                    double tx = centerX + textRadius * Math.Cos(rad) - textSize.Width / 2;
                    double ty = centerY + textRadius * Math.Sin(rad) - textSize.Height / 2;

                    text.Margin = new Thickness(tx, ty, 0, 0);
                    text.HorizontalAlignment = HorizontalAlignment.Left;
                    text.VerticalAlignment = VerticalAlignment.Top;

                    TicksContainer.Children.Add(text);
                }
            }
        }

        private void Clock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as Grid;
            if (grid == null) return;

            _isDragging = true;
            _lastAngle = GetAngle(e.GetPosition(grid), grid);
            grid.CaptureMouse();
        }

        private void Clock_MouseMove(object sender, MouseEventArgs e)
        {
            var grid = sender as Grid;
            if (grid == null || !_isDragging) return;

            double currentAngle = GetAngle(e.GetPosition(grid), grid);
            double delta = currentAngle - _lastAngle;
            
            // Handle wrap around
            if (delta > 180) delta -= 360;
            if (delta < -180) delta += 360;

            if (Math.Abs(delta) > 0.5) // Filter small jitters
            {
                if (DataContext is TimeDetectiveViewModel vm)
                {
                    // 6 degrees = 1 minute
                    vm.AddMinutes(delta / 6.0);
                }
                _lastAngle = currentAngle;
            }
        }

        private void Clock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            var grid = sender as Grid;
            grid?.ReleaseMouseCapture();
        }

        private double GetAngle(Point mousePos, FrameworkElement clockElement)
        {
            if (clockElement == null) return 0;

            double centerX = clockElement.ActualWidth / 2;
            double centerY = clockElement.ActualHeight / 2;

            double dx = mousePos.X - centerX;
            double dy = mousePos.Y - centerY;

            double angleRad = Math.Atan2(dy, dx);
            double angleDeg = angleRad * 180 / Math.PI;

            angleDeg += 90;
            if (angleDeg < 0) angleDeg += 360;

            return angleDeg;
        }
    }
}