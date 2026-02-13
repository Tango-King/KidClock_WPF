using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KidClock.Core.Interfaces;
using KidClock.Core.Models;
using KidClock.Modules.Games.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace KidClock.Modules.Games.ViewModels.Logic
{
    public enum MazeCommand
    {
        Forward = 0,
        TurnLeft = 1,
        TurnRight = 2
    }

    public enum Facing
    {
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3
    }

    public partial class MazeCell : ObservableObject
    {
        public int Row { get; }
        public int Col { get; }

        [ObservableProperty]
        private bool _isWall;

        [ObservableProperty]
        private bool _isStart;

        [ObservableProperty]
        private bool _isGoal;

        [ObservableProperty]
        private bool _isPlayer;

        public MazeCell(int row, int col)
        {
            Row = row;
            Col = col;
        }
    }

    public partial class CommandMazeViewModel : GameSessionViewModelBase
    {
        private readonly IDataService _dataService;
        private readonly Random _random = new();
        private DateTime _sessionStartUtc;
        private readonly ObservableCollection<string> _mistakeTags = new();

        [ObservableProperty]
        private int _rows = 4;

        [ObservableProperty]
        private int _columns = 4;

        [ObservableProperty]
        private ObservableCollection<MazeCell> _cells = new();

        [ObservableProperty]
        private ObservableCollection<MazeCommand> _program = new();

        [ObservableProperty]
        private string _bigPrompt = "排好指令，然后点击“运行”";

        [ObservableProperty]
        private bool _isExecuting;

        private (int Row, int Col) _start;
        private (int Row, int Col) _goal;
        private Facing _startFacing = Facing.Right;

        public CommandMazeViewModel(IDataService dataService)
        {
            _dataService = dataService;
        }

        protected override void OnStartedCore()
        {
            _sessionStartUtc = DateTime.UtcNow;
            _mistakeTags.Clear();
            GenerateMaze();
            Program.Clear();
            BigPrompt = Mode == GameMode.Practice ? "排好指令（前进/左转/右转），再点击运行（可随时撤销）" : "排好指令，然后运行";
        }

        protected override void OnResetCore()
        {
            _mistakeTags.Clear();
            Cells.Clear();
            Program.Clear();
            BigPrompt = "排好指令，然后点击“运行”";
        }

        protected override void OnDifficultyChanged()
        {
            if (IsRunning)
            {
                GenerateMaze();
                Program.Clear();
            }
        }

        protected override void OnSessionEnded()
        {
            SaveSession();
        }

        protected override void OnPaused()
        {
            IsExecuting = false;
        }

        [RelayCommand]
        public void AddForward()
        {
            if (!IsRunning || IsPaused) return;
            Program.Add(MazeCommand.Forward);
        }

        [RelayCommand]
        public void AddLeft()
        {
            if (!IsRunning || IsPaused) return;
            Program.Add(MazeCommand.TurnLeft);
        }

        [RelayCommand]
        public void AddRight()
        {
            if (!IsRunning || IsPaused) return;
            Program.Add(MazeCommand.TurnRight);
        }

        [RelayCommand]
        public void Undo()
        {
            if (!IsRunning || IsPaused) return;
            if (Program.Count <= 0) return;
            Program.RemoveAt(Program.Count - 1);
        }

        [RelayCommand]
        public void ClearProgram()
        {
            if (!IsRunning || IsPaused) return;
            Program.Clear();
        }

        [RelayCommand]
        public void Run()
        {
            if (!IsRunning || IsPaused) return;
            if (IsExecuting) return;
            IsExecuting = true;

            bool ok = ExecuteProgram();

            if (Mode == GameMode.Practice && !ok)
            {
                BigPrompt = "再试一次：你可以先检查转向，再检查前进步数";
                AddMistakeTag("迷宫-路径");
                IsExecuting = false;
                return;
            }

            RecordAnswer(ok, 15);
            if (ok)
            {
                BigPrompt = "到达终点！继续下一关";
                TriggerSuccess();
                GenerateMaze();
                Program.Clear();
            }
            else
            {
                BigPrompt = "没关系，再来一关（提示：先转向，再前进）";
                AddMistakeTag("迷宫-路径");
                TriggerHint();
                GenerateMaze();
                Program.Clear();
            }

            IsExecuting = false;
        }

        private bool ExecuteProgram()
        {
            var player = Cells.FirstOrDefault(c => c.IsPlayer);
            if (player == null) return false;

            int r = player.Row;
            int c = player.Col;
            Facing facing = _startFacing;

            foreach (var cmd in Program)
            {
                if (cmd == MazeCommand.TurnLeft)
                {
                    facing = (Facing)(((int)facing + 3) % 4);
                    continue;
                }
                if (cmd == MazeCommand.TurnRight)
                {
                    facing = (Facing)(((int)facing + 1) % 4);
                    continue;
                }

                (int nr, int nc) = facing switch
                {
                    Facing.Up => (r - 1, c),
                    Facing.Right => (r, c + 1),
                    Facing.Down => (r + 1, c),
                    _ => (r, c - 1)
                };

                if (nr < 0 || nr >= Rows || nc < 0 || nc >= Columns)
                {
                    return false;
                }

                var next = Cells.First(x => x.Row == nr && x.Col == nc);
                if (next.IsWall)
                {
                    return false;
                }

                r = nr;
                c = nc;
            }

            return r == _goal.Row && c == _goal.Col;
        }

        private void GenerateMaze()
        {
            Rows = DifficultyTier switch
            {
                1 => 4,
                2 => 5,
                _ => 6
            };
            Columns = Rows;

            int tries = 0;
            while (tries++ < 50)
            {
                Cells.Clear();
                for (int r = 0; r < Rows; r++)
                {
                    for (int c = 0; c < Columns; c++)
                    {
                        Cells.Add(new MazeCell(r, c));
                    }
                }

                _start = (0, 0);
                _goal = (Rows - 1, Columns - 1);

                foreach (var cell in Cells)
                {
                    cell.IsWall = false;
                    cell.IsStart = cell.Row == _start.Row && cell.Col == _start.Col;
                    cell.IsGoal = cell.Row == _goal.Row && cell.Col == _goal.Col;
                    cell.IsPlayer = cell.IsStart;
                }

                int wallCount = DifficultyTier switch
                {
                    1 => 1,
                    2 => 4,
                    _ => 7
                };

                var candidates = Cells.Where(c => !c.IsStart && !c.IsGoal).OrderBy(_ => _random.Next()).ToList();
                for (int i = 0; i < Math.Min(wallCount, candidates.Count); i++)
                {
                    candidates[i].IsWall = true;
                }

                if (HasPath())
                {
                    return;
                }
            }
        }

        private bool HasPath()
        {
            var start = Cells.First(c => c.IsStart);
            var goal = Cells.First(c => c.IsGoal);

            var visited = new bool[Rows, Columns];
            var queue = new Queue<(int r, int c)>();
            queue.Enqueue((start.Row, start.Col));
            visited[start.Row, start.Col] = true;

            while (queue.Count > 0)
            {
                var (r, c) = queue.Dequeue();
                if (r == goal.Row && c == goal.Col) return true;

                foreach (var (nr, nc) in new[] { (r - 1, c), (r + 1, c), (r, c - 1), (r, c + 1) })
                {
                    if (nr < 0 || nr >= Rows || nc < 0 || nc >= Columns) continue;
                    if (visited[nr, nc]) continue;
                    var cell = Cells.First(x => x.Row == nr && x.Col == nc);
                    if (cell.IsWall) continue;
                    visited[nr, nc] = true;
                    queue.Enqueue((nr, nc));
                }
            }

            return false;
        }

        private void AddMistakeTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) return;
            if (_mistakeTags.Count >= 50) return;
            _mistakeTags.Add(tag);
        }

        private void SaveSession()
        {
            int duration = (int)Math.Max(0, (DateTime.UtcNow - _sessionStartUtc).TotalSeconds);
            var result = new GameSessionResult(
                LogicGameKeys.CommandMaze,
                Mode.ToString(),
                DifficultyTier,
                CorrectCount,
                TotalCount,
                duration,
                StarRating,
                DateTime.UtcNow.ToString("O"),
                string.Join(",", _mistakeTags)
            );
            _dataService.SaveGameSession(result);
        }
    }
}
