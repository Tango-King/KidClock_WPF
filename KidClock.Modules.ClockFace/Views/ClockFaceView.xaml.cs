using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace KidClock.Modules.ClockFace.Views
{
    public partial class ClockFaceView : UserControl
    {
        public ClockFaceView()
        {
            InitializeComponent();
            InitializeClockFace();
        }

        private void InitializeClockFace()
        {
            GenerateTicks();
            GenerateNumbers();
        }

        private void GenerateTicks()
        {
            TicksContainer.Children.Clear();
            double centerY = 240; // 旋转中心向下偏移 240 像素，到达圆心

            for (int i = 0; i < 60; i++)
            {
                bool isHourTick = i % 5 == 0;
                
                // 刻度线参数
                double width = isHourTick ? 6 : 2;
                double height = isHourTick ? 20 : 10;
                Brush color = isHourTick ? Brushes.Black : Brushes.Gray;
                double angle = i * 6;

                var tick = new Rectangle
                {
                    Width = width,
                    Height = height,
                    Fill = color,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, 15, 0, 0), // Increased margin to pull ticks inward
                    RenderTransformOrigin = new Point(0.5, 0.5)
                };

                // 直接设置 RenderTransform，避免 Binding
                var transformGroup = new TransformGroup();
                transformGroup.Children.Add(new RotateTransform(angle, width / 2, centerY)); 
                // 注意：RotateTransform 的 CenterX/Y 是相对于元素自身的坐标系的。
                // 元素的中心是 width/2, height/2。
                // 我们希望围绕 (width/2, centerY) 旋转。
                
                tick.RenderTransform = transformGroup;

                TicksContainer.Children.Add(tick);
            }
        }

        private void GenerateNumbers()
        {
            NumbersContainer.Children.Clear();
            double radius = 190; // 数字所在的半径
            
            // Grid 大小 500x500，中心是 250,250
            // 数字是 TextBlock，HorizontalAlignment=Center, VerticalAlignment=Center 意味着它们默认在中心。
            // 我们只需要 TranslateTransform 移动它们。

            for (int i = 1; i <= 12; i++)
            {
                double angleDeg = i * 30 - 90;
                double angleRad = angleDeg * Math.PI / 180;

                double x = radius * Math.Cos(angleRad);
                double y = radius * Math.Sin(angleRad);

                var number = new TextBlock
                {
                    Text = i.ToString(),
                    FontSize = 40,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    RenderTransformOrigin = new Point(0.5, 0.5)
                };

                number.RenderTransform = new TranslateTransform(x, y);

                NumbersContainer.Children.Add(number);
            }
        }
    }
}