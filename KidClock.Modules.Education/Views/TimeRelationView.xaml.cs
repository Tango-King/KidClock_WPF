using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace KidClock.Modules.Education.Views
{
    public partial class TimeRelationView : UserControl
    {
        public TimeRelationView()
        {
            InitializeComponent();
            DrawClockFace();
        }

        private void DrawClockFace()
        {
            ClockFaceContainer.Children.Clear();

            // 表盘半径 (假设容器为 500x500，半径为 250)
            double radius = 250;
            double centerX = 250;
            double centerY = 250;

            // 绘制刻度
            for (int i = 0; i < 60; i++)
            {
                bool isHourTick = i % 5 == 0;
                double angle = i * 6; // 每分钟 6 度

                Rectangle tick = new Rectangle();
                tick.Width = isHourTick ? 6 : 2;
                tick.Height = isHourTick ? 20 : 10;
                tick.Fill = Brushes.Black;

                // 计算刻度位置
                TransformGroup transform = new TransformGroup();
                // 1. 先旋转自身
                // 2. 移动到顶部中间 (0, 10)
                // 3. 绕表盘中心旋转
                
                // 更简单的方法：直接设置 Margin 和 Transform
                // 我们将刻度放在 (0,0) 然后旋转和平移
                
                tick.RenderTransformOrigin = new Point(0.5, 0.5);
                
                // 计算位置：从中心向外延伸
                // 刻度顶端距离边缘 10px
                double distanceFromEdge = 10;
                double tickCenterRadius = radius - distanceFromEdge - tick.Height / 2;

                // 角度转弧度 (减去 90 度，因为 0 度是 3 点钟方向，我们需要 0 度是 12 点钟方向)
                double rad = (angle - 90) * Math.PI / 180;
                
                double x = centerX + tickCenterRadius * Math.Cos(rad) - tick.Width / 2;
                double y = centerY + tickCenterRadius * Math.Sin(rad) - tick.Height / 2;

                tick.Margin = new Thickness(x, y, 0, 0);
                tick.HorizontalAlignment = HorizontalAlignment.Left;
                tick.VerticalAlignment = VerticalAlignment.Top;
                
                // 旋转刻度
                tick.RenderTransform = new RotateTransform(angle);

                ClockFaceContainer.Children.Add(tick);

                // 绘制数字 (仅整点)
                if (isHourTick)
                {
                    int hour = i / 5;
                    if (hour == 0) hour = 12;

                    TextBlock text = new TextBlock();
                    text.Text = hour.ToString();
                    text.FontSize = 32;
                    text.FontWeight = FontWeights.Bold;
                    text.Foreground = Brushes.Black;
                    
                    // 测量文本大小以居中
                    text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    Size textSize = text.DesiredSize;

                    // 数字距离边缘稍远一点
                    double textRadius = radius - 50; 
                    
                    double tx = centerX + textRadius * Math.Cos(rad) - textSize.Width / 2;
                    double ty = centerY + textRadius * Math.Sin(rad) - textSize.Height / 2;

                    text.Margin = new Thickness(tx, ty, 0, 0);
                    text.HorizontalAlignment = HorizontalAlignment.Left;
                    text.VerticalAlignment = VerticalAlignment.Top;

                    ClockFaceContainer.Children.Add(text);
                }
            }
        }
    }
}