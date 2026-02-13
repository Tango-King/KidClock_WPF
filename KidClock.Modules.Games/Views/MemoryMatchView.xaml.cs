using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using KidClock.Modules.Games.ViewModels;

namespace KidClock.Modules.Games.Views
{
    public partial class MemoryMatchView : UserControl
    {
        private MemoryMatchViewModel? _vm;
        private ControlTemplate? _cardButtonTemplate;

        public MemoryMatchView()
        {
            InitializeComponent();
            DataContextChanged += MemoryMatchView_DataContextChanged;
            Loaded += MemoryMatchView_Loaded;
        }

        private void MemoryMatchView_Loaded(object sender, RoutedEventArgs e)
        {
            AttachToDataContext(DataContext);
        }

        private void MemoryMatchView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DetachFromViewModel();
            AttachToDataContext(e.NewValue);
        }

        private void AttachToDataContext(object? dataContext)
        {
            if (dataContext is not MemoryMatchViewModel vm) return;

            _vm = vm;
            _vm.PropertyChanged += Vm_PropertyChanged;
            _cardButtonTemplate = TryFindResource("CardButtonTemplate") as ControlTemplate;
            RebuildGrid(_vm);
        }

        private void DetachFromViewModel()
        {
            if (_vm != null)
            {
                _vm.PropertyChanged -= Vm_PropertyChanged;
                _vm = null;
            }
        }

        private void Vm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_vm == null) return;

            if (e.PropertyName == nameof(MemoryMatchViewModel.GridRows) ||
                e.PropertyName == nameof(MemoryMatchViewModel.GridColumns) ||
                e.PropertyName == nameof(MemoryMatchViewModel.Cards))
            {
                RebuildGrid(_vm);
            }
        }

        private void RebuildGrid(MemoryMatchViewModel vm)
        {
            if (GameGridContainer == null) return;

            try
            {
                GameGridContainer.Children.Clear();
                GameGridContainer.RowDefinitions.Clear();
                GameGridContainer.ColumnDefinitions.Clear();

                int rows = vm.GridRows;
                int cols = vm.GridColumns;

                for (int i = 0; i < rows; i++)
                    GameGridContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

                for (int j = 0; j < cols; j++)
                    GameGridContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var cards = vm.Cards;
                if (cards == null || cards.Count == 0) return;

                int expected = rows * cols;
                int count = Math.Min(expected, cards.Count);

                for (int index = 0; index < count; index++)
                {
                    int r = index / cols;
                    int c = index % cols;
                    var button = CreateCardButton(cards[index], vm);
                    Grid.SetRow(button, r);
                    Grid.SetColumn(button, c);
                    GameGridContainer.Children.Add(button);
                }
            }
            catch
            {
                vm.GameStatus = "加载失败，请重试";
                GameGridContainer.Children.Clear();
            }
        }

        private Button CreateCardButton(MemoryCard card, MemoryMatchViewModel vm)
        {
            var btn = new Button
            {
                Margin = new Thickness(5),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Command = vm.CardClickCommand,
                CommandParameter = card,
                DataContext = card
            };

            if (_cardButtonTemplate != null)
            {
                btn.Template = _cardButtonTemplate;
            }
            else
            {
                btn.Content = "?";
                btn.Foreground = Brushes.White;
                btn.Background = new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00));
            }

            return btn;
        }
    }
}
