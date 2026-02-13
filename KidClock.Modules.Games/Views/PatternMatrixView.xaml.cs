using KidClock.Modules.Games.ViewModels.Logic;
using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace KidClock.Modules.Games.Views
{
    public partial class PatternMatrixView : UserControl
    {
        private PatternMatrixViewModel? _vm;
        private DataTemplate? _tokenTemplate;

        public PatternMatrixView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Attach(DataContext);
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Detach();
            Attach(e.NewValue);
        }

        private void Attach(object? dc)
        {
            if (dc is not PatternMatrixViewModel vm) return;
            _vm = vm;
            _vm.PropertyChanged += Vm_PropertyChanged;
            _vm.Cells.CollectionChanged += Cells_CollectionChanged;
            _tokenTemplate = TryFindResource("MatrixTokenTemplate") as DataTemplate;
            RebuildGrid();
        }

        private void Detach()
        {
            if (_vm == null) return;
            _vm.PropertyChanged -= Vm_PropertyChanged;
            _vm.Cells.CollectionChanged -= Cells_CollectionChanged;
            _vm = null;
        }

        private void Vm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PatternMatrixViewModel.GridSize))
            {
                RebuildGrid();
            }
        }

        private void Cells_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RebuildGrid();
        }

        private void RebuildGrid()
        {
            if (_vm == null) return;
            if (MatrixGridContainer == null) return;

            try
            {
                MatrixGridContainer.Children.Clear();
                MatrixGridContainer.RowDefinitions.Clear();
                MatrixGridContainer.ColumnDefinitions.Clear();

                int size = Math.Max(1, _vm.GridSize);
                for (int i = 0; i < size; i++)
                {
                    MatrixGridContainer.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    MatrixGridContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                }

                foreach (var cell in _vm.Cells)
                {
                    int row = cell.Index / size;
                    int col = cell.Index % size;

                    FrameworkElement element;
                    if (cell.IsBlank)
                    {
                        element = new Border
                        {
                            Width = 64,
                            Height = 64,
                            Background = Brushes.White,
                            BorderBrush = Brushes.Gray,
                            BorderThickness = new Thickness(3),
                            CornerRadius = new CornerRadius(10),
                            Margin = new Thickness(6),
                            Child = new TextBlock
                            {
                                Text = "?",
                                FontSize = 34,
                                FontWeight = FontWeights.Bold,
                                Foreground = Brushes.Gray,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center,
                                TextAlignment = TextAlignment.Center
                            }
                        };
                    }
                    else
                    {
                        element = new Border
                        {
                            Width = 64,
                            Height = 64,
                            Background = Brushes.White,
                            BorderBrush = new SolidColorBrush(Color.FromRgb(0x27, 0xAE, 0x60)),
                            BorderThickness = new Thickness(3),
                            CornerRadius = new CornerRadius(10),
                            Margin = new Thickness(6),
                            Child = new ContentPresenter
                            {
                                Content = cell.Token,
                                ContentTemplate = _tokenTemplate,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center
                            }
                        };
                    }

                    Grid.SetRow(element, row);
                    Grid.SetColumn(element, col);
                    MatrixGridContainer.Children.Add(element);
                }
            }
            catch
            {
                if (_vm != null)
                {
                    _vm.BigPrompt = "加载失败，请重试";
                }
                MatrixGridContainer.Children.Clear();
            }
        }
    }
}

