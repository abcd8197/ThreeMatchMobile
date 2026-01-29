using System;
using System.Collections.Generic;

namespace ThreeMatch
{
    public sealed class MissionService : IDisposable
    {
        private readonly StageData _stage;
        private readonly Dictionary<string, int> _progress = new();
        private bool _ended;
        private int _score;

        public event Action<StageGoalData, int> OnProgressChanged; // (goal, current, target)
        public event Action OnSuccess;
        public event Action OnFail;

        public MissionService(StageData stage)
        {
            _stage = stage ?? throw new ArgumentNullException(nameof(stage));
            Init();
        }

        public void Reset() => Init();
        public void ApplyScore(int deltaScore)
        {
            if (_ended)
                return;

            if (deltaScore <= 0)
                return;

            _score += deltaScore;

            if (_stage.Goals == null || _stage.Goals.Count == 0)
                return;

            for (int i = 0; i < _stage.Goals.Count; i++)
            {
                var g = _stage.Goals[i];

                if (g == null)
                    continue;

                if (g.GoalType != StageGoalType.Score)
                    continue;

                int before = _progress.TryGetValue(g.Key, out var cur) ? cur : 0;
                int after = _score;

                if (after != before)
                {
                    _progress[g.Key] = after;
                    OnProgressChanged?.Invoke(g, after);
                }
            }
        }

        public void ApplyChanges(IReadOnlyList<BoardChange> changes)
        {
            if (_ended)
                return;

            if (_stage.Goals == null || _stage.Goals.Count == 0)
                return;

            if (changes == null || changes.Count == 0)
                return;

            for (int i = 0; i < changes.Count; i++)
            {
                if (changes[i] is not RemoveChange rm)
                    continue;

                var removed = rm.Removed;
                if (removed == null || removed.Count == 0)
                    continue;

                for (int r = 0; r < removed.Count; r++)
                {
                    var info = removed[r];

                    for (int gIdx = 0; gIdx < _stage.Goals.Count; gIdx++)
                    {
                        var goal = _stage.Goals[gIdx];

                        if (goal == null)
                            continue;


                        if (goal.GoalType == StageGoalType.Score)
                            continue;

                        if (!CountsForGoal(goal, info))
                            continue;

                        int before = _progress.TryGetValue(goal.Key, out var cur) ? cur : 0;
                        int after = before + 1;

                        _progress[goal.Key] = after;
                        OnProgressChanged?.Invoke(goal, after);
                    }
                }
            }
        }

        public bool IsCleared()
        {
            if (_stage.Goals == null || _stage.Goals.Count == 0)
                return true;

            for (int i = 0; i < _stage.Goals.Count; i++)
            {
                var g = _stage.Goals[i];

                if (g == null)
                    continue;

                int cur = _progress.TryGetValue(g.Key, out var v) ? v : 0;

                if (cur < g.GoalValue)
                    return false;
            }

            return true;
        }

        public void TryEnd(int remainMove)
        {
            if (_ended)
                return;

            if (IsCleared())
            {
                _ended = true;
                OnSuccess?.Invoke();
                return;
            }

            if (remainMove <= 0)
            {
                _ended = true;
                OnFail?.Invoke();
            }
        }

        private void Init()
        {
            _progress.Clear();
            _ended = false;
            _score = 0;

            if (_stage.Goals == null)
                return;

            for (int i = 0; i < _stage.Goals.Count; i++)
            {
                var g = _stage.Goals[i];

                if (g == null)
                    continue;

                _progress[g.Key] = (g.GoalType == StageGoalType.Score) ? _score : 0;
                OnProgressChanged?.Invoke(g, _progress[g.Key]);
            }
        }

        private static bool CountsForGoal(StageGoalData goal, RemovedCellInfo info)
        {
            if (goal.GoalType != StageGoalType.CollectColor)
                return false;

            bool needPiece = goal.TargetPieceType != PieceType.None;
            bool needColor = goal.TargetColor != ColorType.None;

            if (needPiece && info.PieceType != goal.TargetPieceType)
                return false;
            if (needColor && info.ColorType != goal.TargetColor)
                return false;

            return true;
        }

        public void Dispose()
        {
            _progress?.Clear();
            OnFail = null;
            OnProgressChanged = null;
            OnSuccess = null;
        }
    }
}
