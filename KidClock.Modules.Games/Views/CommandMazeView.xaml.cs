using KidClock.Modules.Games.ViewModels.Logic;
using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace KidClock.Modules.Games.Views
{
    public partial class CommandMazeView : UserControl
    {
        private CommandMazeViewModel? _vm;
        private Style? _tileBorderStyle;
        private Style? _tileTextStyle;

        public CommandMazeView()
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
            if (dc is not CommandMazeViewModel vm) return;
            _vm = vm;
            _vm.PropertyChanged += Vm_PropertyChanged;
            _vm.Cells.CollectionChanged += Cells_CollectionChanged;
            _tileBorderStyle = TryFindResource("MazeTileBorderStyle") as Style;
            _tileTextStyle = TryFindResource("MazeTileTextStyle") as Style;
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
            if (e.PropertyName == nameof(CommandMazeViewModel.Rows) ||
                e.PropertyName == nameof(CommandMazeViewModel.Columns))
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
            if (MazeGridContainer == null) return;

            try
            {
                MazeGridContainer.Children.Clear();
                MazeGridContainer.RowDefinitions.Clear();
                MazeGridContainer.ColumnDefinitions.Clear();

                int rows = Math.Max(1, _vm.Rows);
                int cols = Math.Max(1, _vm.Columns);

                for (int r = 0; r < rows; r++)
                    MazeGridContainer.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                for (int c = 0; c < cols; c++)
                    MazeGridContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                foreach (var cell in _vm.Cells)
                {
                    var text = new TextBlock();
                    if (_tileTextStyle != null)
                    {
                        text.Style = _tileTextStyle;
                    }

                    var border = new Border
                    {
                        DataContext = cell,
                        Child = text
                    };

                    if (_tileBorderStyle != null)
                    {
                        border.Style = _tileBorderStyle;
                    }

                    Grid.SetRow(border, cell.Row);
                    Grid.SetColumn(border, cell.Col);
                    MazeGridContainer.Children.Add(border);
                }
            }
            catch
            {
                if (_vm != null)
                {
                    _vm.BigPrompt = "加载失败，请重试";
                }
                MazeGridContainer.Children.Clear();
            }
        }
    }
}

