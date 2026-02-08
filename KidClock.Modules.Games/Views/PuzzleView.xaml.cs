using KidClock.Modules.Games.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace KidClock.Modules.Games.Views
{
    public partial class PuzzleView : UserControl
    {
        private bool _isDragging = false;
        private FrameworkElement? _draggedElement;
        private Point _clickOffset;
        private Point _originalPosition;
        
        private List<Border> _puzzlePieces = new List<Border>();
        private Random _random = new Random();

        public PuzzleView()
        {
            InitializeComponent();
            this.Loaded += PuzzleView_Loaded;
            this.SizeChanged += PuzzleView_SizeChanged;
            this.DataContextChanged += PuzzleView_DataContextChanged;
        }

        private void PuzzleView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is PuzzleViewModel oldVm)
            {
                oldVm.PropertyChanged -= Vm_PropertyChanged;
            }
            if (e.NewValue is PuzzleViewModel vm)
            {
                vm.PropertyChanged += Vm_PropertyChanged;
            }
        }

        private void Vm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PuzzleViewModel.ResetTrigger))
            {
                ResetPuzzlePieces();
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            // Direct UI reset
            ResetPuzzlePieces();
            
            // Also call ViewModel reset
            if (DataContext is PuzzleViewModel vm)
            {
                vm.ResetGame();
            }
        }

        private void PuzzleView_Loaded(object sender, RoutedEventArgs e)
        {
            // Only initialize if empty (first load)
            if (_puzzlePieces.Count == 0)
            {
                InitializePuzzlePieces();
            }
        }

        private void PuzzleView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Re-calculate positions for non-placed pieces in the parts bin
            // We iterate through all pieces. If they are NOT placed (IsHitTestVisible == true), we update their position.
            // Placed pieces (IsHitTestVisible == false) should stay on the clock face, which might also need adjustment if clock moves,
            // but for now let's focus on keeping the bin tidy.
            
            // However, the grid logic depends on the index. We need to know the original index or current index in the bin.
            // Since we shuffle, we can just re-layout them based on their current order in _puzzlePieces.
            
            // We need to filter only pieces that are NOT placed.
            // Placed pieces have IsHitTestVisible = false.
            
            int binIndex = 0;
            foreach (var piece in _puzzlePieces)
            {
                if (piece.IsHitTestVisible)
                {
                    MoveToGridBinPosition(piece, binIndex);
                    binIndex++;
                }
                else
                {
                    // For placed pieces, we should ideally re-snap them to the new clock center.
                    // Recalculate target position based on new size.
                    int number = (int)piece.Tag;
                    double clockAreaWidth = this.ActualWidth - 300;
                    double cx = clockAreaWidth / 2;
                    double cy = this.ActualHeight / 2;
                    double r = 200;
                    double angleDeg = (number * 30) - 90;
                    double angleRad = angleDeg * Math.PI / 180;
                    double targetX = cx + r * Math.Cos(angleRad) - (piece.Width / 2);
                    double targetY = cy + r * Math.Sin(angleRad) - (piece.Height / 2);
                    
                    Canvas.SetLeft(piece, targetX);
                    Canvas.SetTop(piece, targetY);
                }
            }
        }

        private void InitializePuzzlePieces()
        {
            DragCanvas.Children.Clear();
            _puzzlePieces.Clear();

            // Create a shuffled list of numbers 1-12
            var numbers = new List<int>();
            for (int i = 1; i <= 12; i++) numbers.Add(i);
            
            // Fisher-Yates shuffle
            int n = numbers.Count;
            while (n > 1)
            {
                n--;
                int k = _random.Next(n + 1);
                int value = numbers[k];
                numbers[k] = numbers[n];
                numbers[n] = value;
            }

            for (int i = 0; i < 12; i++)
            {
                int number = numbers[i];
                var piece = CreatePuzzlePiece(number);
                _puzzlePieces.Add(piece);
                DragCanvas.Children.Add(piece);
                
                // Grid position in the right panel
                MoveToGridBinPosition(piece, i);
            }
        }

        private void MoveToGridBinPosition(FrameworkElement piece, int index)
        {
            // Right panel is 300px wide (visually). 
            // It starts at ActualWidth - 300.
            double panelX = this.ActualWidth - 300;
            // The header stack panel + Parts Box header is about 200-220px. 
            // We want to start inside the "Parts Box" border.
            // Let's try 330.
            double startY = 330; 
            
            // Grid Layout: 3 Columns (12 items -> 4 rows)
            // Column offsets:
            // Width 300. Piece 50.
            // Spacing around columns?
            // Col 0: 40, Col 1: 125, Col 2: 210
            int col = index % 3;
            int row = index / 3;
            
            double offsetX = 0;
            if (col == 0) offsetX = 40;
            else if (col == 1) offsetX = 125;
            else offsetX = 210;

            double offsetY = row * 70; // 70px vertical spacing

            double x = panelX + offsetX;
            double y = startY + offsetY;

            // Ensure we don't go out of bounds (basic check)
            if (x < 0) x = 0;
            if (y < 0) y = 0;

            Canvas.SetLeft(piece, x);
            Canvas.SetTop(piece, y);
            
            // Mark as not placed
            ((Border)piece).Background = Brushes.White;
            ((Border)piece).BorderBrush = Brushes.Purple;
            piece.IsHitTestVisible = true;
        }

        private void ResetPuzzlePieces()
        {
            // If dragging, force release
            if (_isDragging && _draggedElement != null)
            {
                _draggedElement.ReleaseMouseCapture();
                _isDragging = false;
                _draggedElement = null;
            }

            DragCanvas.Children.Clear();
            _puzzlePieces.Clear(); // Ensure list is cleared before re-populating
            InitializePuzzlePieces();
        }

        private Border CreatePuzzlePiece(int number)
        {
            var border = new Border
            {
                Width = 50,
                Height = 50,
                CornerRadius = new CornerRadius(25),
                Background = Brushes.White,
                BorderBrush = Brushes.Purple,
                BorderThickness = new Thickness(2),
                Tag = number, // Store the number
                Cursor = Cursors.Hand,
                Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 5, ShadowDepth = 2, Opacity = 0.3 }
            };

            var text = new TextBlock
            {
                Text = number.ToString(),
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Purple,
                IsHitTestVisible = false // Critical: Ensure clicks go to the Border
            };

            border.Child = text;
            return border;
        }

        // --- Drag & Drop Logic ---

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Robustly find the border element
            if (e.OriginalSource is DependencyObject source)
            {
                // Walk up the tree to find the Border with the Tag
                var element = source as FrameworkElement;
                while (element != null && !(element is Border && element.Tag is int))
                {
                    element = VisualTreeHelper.GetParent(element) as FrameworkElement;
                }

                if (element is Border border && border.Tag is int)
                {
                    // If already placed (Green), don't move
                    if (border.BorderBrush == Brushes.Green) return;

                    _isDragging = true;
                    _draggedElement = border;
                    _originalPosition = new Point(Canvas.GetLeft(border), Canvas.GetTop(border));
                    
                    Point clickPoint = e.GetPosition(DragCanvas);
                    _clickOffset = new Point(clickPoint.X - _originalPosition.X, clickPoint.Y - _originalPosition.Y);
                    
                    _draggedElement.CaptureMouse();
                    
                    // Bring to front
                    Panel.SetZIndex(_draggedElement, 100);
                    
                    e.Handled = true;
                }
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && _draggedElement != null)
            {
                Point currentPoint = e.GetPosition(DragCanvas);
                Canvas.SetLeft(_draggedElement, currentPoint.X - _clickOffset.X);
                Canvas.SetTop(_draggedElement, currentPoint.Y - _clickOffset.Y);
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging && _draggedElement != null)
            {
                _draggedElement.ReleaseMouseCapture();
                _isDragging = false;
                Panel.SetZIndex(_draggedElement, 0);

                CheckDropPosition(_draggedElement);
                _draggedElement = null;
                
                e.Handled = true;
            }
        }

        private void CheckDropPosition(FrameworkElement piece)
        {
            if (piece.Tag is not int number) return;
            
            // Calculate Correct Position
            // Clock Center
            double clockAreaWidth = this.ActualWidth - 300; // Left column
            double cx = clockAreaWidth / 2;
            double cy = this.ActualHeight / 2;
            
            // Radius where numbers sit. 
            // Clock circle is 500x500 (Radius 250). 
            // Numbers should be around Radius 200.
            double r = 200;
            
            // Angle: 12 -> -90 deg, 3 -> 0 deg.
            // Formula: angle = (number * 30) - 90
            double angleDeg = (number * 30) - 90;
            double angleRad = angleDeg * Math.PI / 180;

            double targetX = cx + r * Math.Cos(angleRad) - (piece.Width / 2);
            double targetY = cy + r * Math.Sin(angleRad) - (piece.Height / 2);

            // Current Pos
            double currentX = Canvas.GetLeft(piece);
            double currentY = Canvas.GetTop(piece);

            // Distance
            double dist = Math.Sqrt(Math.Pow(currentX - targetX, 2) + Math.Pow(currentY - targetY, 2));

            // Tolerance: 50px
            if (dist < 50)
            {
                // Snap!
                Canvas.SetLeft(piece, targetX);
                Canvas.SetTop(piece, targetY);
                
                // Lock
                var border = (Border)piece;
                border.Background = Brushes.LightGreen;
                border.BorderBrush = Brushes.Green;
                border.IsHitTestVisible = false; 

                // Update Score
                if (DataContext is PuzzleViewModel vm)
                {
                    vm.IncrementScore();
                }
            }
            else
            {
                // Bounce back
                Canvas.SetLeft(piece, _originalPosition.X);
                Canvas.SetTop(piece, _originalPosition.Y);
            }
        }
    }
}
