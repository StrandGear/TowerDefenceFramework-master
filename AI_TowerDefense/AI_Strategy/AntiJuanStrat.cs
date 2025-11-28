using GameFramework;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using GameFramework;

namespace AI_Strategy
{
    public class AntiJuanStrat : AbstractStrategy
    {
        private const int InitialRounds = 8;
        private const int SoldierCost = 2;

        private int _currentInitialRound = 0;
        private bool _initialPhaseFinished = false;

        private enum BuildStage { TwoRowsEven, TwoRowsFillOdds, ThirdRowEven, ThirdRowFillOdds, Complete }
        private BuildStage _stage = BuildStage.TwoRowsEven;

        private readonly int[] _evenColumns;
        private readonly int[] _oddColumns;
        private readonly int[] _targetRows;

        public AntiJuanStrat(Player player) : base(player)
        {
            int width = PlayerLane.WIDTH;
            List<int> evens = new List<int>();
            List<int> odds = new List<int>();
            for (int x = 0; x < width; x++)
            {
                if (x % 2 == 0) evens.Add(x); else odds.Add(x);
            }
            _evenColumns = evens.ToArray();
            _oddColumns = odds.ToArray();
            _targetRows = new int[] { 3, 4, 2 };
        }

        public override void DeployTowers()
        {
            if (!_initialPhaseFinished) return;

            int width = PlayerLane.WIDTH;
            if (_stage == BuildStage.Complete)
            {
                ReplaceMissingTowers();
                return;
            }

            ReplaceMissingTowers();

            if (_stage == BuildStage.Complete) return;

            int nextCost = Tower.GetNextTowerCosts(player.HomeLane);
            if (player.Gold < nextCost) return;

            if (_stage == BuildStage.TwoRowsEven)
            {
                for (int r = 0; r < 2; r++)
                {
                    int y = _targetRows[r];
                    foreach (int x in _evenColumns)
                    {
                        if (player.HomeLane.GetCellAt(x, y).Unit == null)
                        {
                            var res = player.TryBuyTower<Tower>(x, y);
                            if (res == Player.TowerPlacementResult.NotEnoughGold) return;
                            AdvanceStageIfNeeded();
                            return;
                        }
                    }
                }
                AdvanceStageIfNeeded();
            }
            else if (_stage == BuildStage.TwoRowsFillOdds)
            {
                for (int r = 0; r < 2; r++)
                {
                    int y = _targetRows[r];
                    foreach (int x in _oddColumns)
                    {
                        if (player.HomeLane.GetCellAt(x, y).Unit == null)
                        {
                            var res = player.TryBuyTower<Tower>(x, y);
                            if (res == Player.TowerPlacementResult.NotEnoughGold) return;
                            AdvanceStageIfNeeded();
                            return;
                        }
                    }
                }
                AdvanceStageIfNeeded();
            }
            else if (_stage == BuildStage.ThirdRowEven)
            {
                int y = _targetRows[2];
                foreach (int x in _evenColumns)
                {
                    if (player.HomeLane.GetCellAt(x, y).Unit == null)
                    {
                        var res = player.TryBuyTower<Tower>(x, y);
                        if (res == Player.TowerPlacementResult.NotEnoughGold) return;
                        AdvanceStageIfNeeded();
                        return;
                    }
                }
                AdvanceStageIfNeeded();
            }
            else if (_stage == BuildStage.ThirdRowFillOdds)
            {
                int y = _targetRows[2];
                foreach (int x in _oddColumns)
                {
                    if (player.HomeLane.GetCellAt(x, y).Unit == null)
                    {
                        var res = player.TryBuyTower<Tower>(x, y);
                        if (res == Player.TowerPlacementResult.NotEnoughGold) return;
                        AdvanceStageIfNeeded();
                        return;
                    }
                }
                AdvanceStageIfNeeded();
            }
        }

        public override void DeploySoldiers()
        {
            int width = PlayerLane.WIDTH;

            if (!_initialPhaseFinished)
            {
                if (player.Gold < SoldierCost) return;

                for (int x = 0; x < width; x++)
                {
                    if (player.Gold < SoldierCost) break;
                    if (player.EnemyLane.GetCellAt(x, 0).Unit == null)
                    {
                        player.TryBuySoldier<MySoldier>(x);
                    }
                }

                _currentInitialRound++;
                if (_currentInitialRound >= InitialRounds) _initialPhaseFinished = true;
                return;
            }

            if (_stage != BuildStage.Complete)
            {
                return;
            }

            if (player.Gold < SoldierCost) return;
            for (int x = 0; x < width; x++)
            {
                if (player.Gold < SoldierCost) break;
                if (player.EnemyLane.GetCellAt(x, 0).Unit == null)
                {
                    player.TryBuySoldier<MySoldier>(x);
                }
            }
        }

        public override List<Soldier> SortedSoldierArray(List<Soldier> unsortedList)
        {
            unsortedList.Sort((a, b) => b.PosY.CompareTo(a.PosY));
            return unsortedList;
        }

        public override List<Tower> SortedTowerArray(List<Tower> unsortedList)
        {
            unsortedList.Sort((a, b) => a.Health.CompareTo(b.Health));
            return unsortedList;
        }

        private void ReplaceMissingTowers()
        {
            int width = PlayerLane.WIDTH;
            foreach (int y in _targetRows)
            {
                for (int x = 0; x < width; x++)
                {
                    if (player.HomeLane.GetCellAt(x, y).Unit == null)
                    {
                        int nextCost = Tower.GetNextTowerCosts(player.HomeLane);
                        if (player.Gold < nextCost) return;
                        var res = player.TryBuyTower<Tower>(x, y);
                        if (res == Player.TowerPlacementResult.NotEnoughGold) return;
                        return;
                    }
                }
            }
        }

        private void AdvanceStageIfNeeded()
        {
            if (_stage == BuildStage.TwoRowsEven)
            {
                bool done = true;
                for (int r = 0; r < 2 && done; r++)
                {
                    int y = _targetRows[r];
                    foreach (int x in _evenColumns)
                    {
                        if (player.HomeLane.GetCellAt(x, y).Unit == null) { done = false; break; }
                    }
                }
                if (done) _stage = BuildStage.TwoRowsFillOdds;
            }
            if (_stage == BuildStage.TwoRowsFillOdds)
            {
                bool done = true;
                for (int r = 0; r < 2 && done; r++)
                {
                    int y = _targetRows[r];
                    foreach (int x in _oddColumns)
                    {
                        if (player.HomeLane.GetCellAt(x, y).Unit == null) { done = false; break; }
                    }
                }
                if (done) _stage = BuildStage.ThirdRowEven;
            }
            if (_stage == BuildStage.ThirdRowEven)
            {
                int y = _targetRows[2];
                bool done = true;
                foreach (int x in _evenColumns)
                {
                    if (player.HomeLane.GetCellAt(x, y).Unit == null) { done = false; break; }
                }
                if (done) _stage = BuildStage.ThirdRowFillOdds;
            }
            if (_stage == BuildStage.ThirdRowFillOdds)
            {
                int y = _targetRows[2];
                bool done = true;
                foreach (int x in _oddColumns)
                {
                    if (player.HomeLane.GetCellAt(x, y).Unit == null) { done = false; break; }
                }
                if (done) _stage = BuildStage.Complete;
            }
        }
    }
}

