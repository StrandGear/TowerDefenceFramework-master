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
        private const int InitialRounds = 5;
        private const int SoldierCost = 2;

        private int _currentInitialRound = 0;
        private bool _initialPhaseFinished = false;
        private enum BuildStage { Row4Even, Row4Odd, Row5Even, Row5Odd, Row6Even, Row6Odd, Complete }
        private BuildStage _stage = BuildStage.Row4Even;

        private readonly int[] _evens;
        private readonly int[] _odds;
        private readonly int[] _rows = new int[] { 4, 5, 6 }; 

        public AntiJuanStrat(Player player) : base(player)
        {
            List<int> ev = new List<int>();
            List<int> od = new List<int>();

            for (int x = 0; x < PlayerLane.WIDTH; x++)
                if ((x % 2) == 0) ev.Add(x); else od.Add(x);

            _evens = ev.ToArray();
            _odds = od.ToArray();
        }


        public override void DeployTowers()
        {
            if (!_initialPhaseFinished || _stage == BuildStage.Complete)
                return;

            TryFillStage();
        }
        public override void DeploySoldiers()
        {
            int width = PlayerLane.WIDTH;

            if (!_initialPhaseFinished)
            {
                if (player.Gold < SoldierCost) return;

                for (int x = 0; x < width && player.Gold >= SoldierCost; x++)
                    player.TryBuySoldier<MySoldier>(x);

                _currentInitialRound++;
                if (_currentInitialRound >= InitialRounds)
                    _initialPhaseFinished = true;

                return;
            }

            if (_stage != BuildStage.Complete) return;

            for (int x = 0; x < width && player.Gold >= SoldierCost; x++)
                player.TryBuySoldier<MySoldier>(x);
        }


        private void TryFillStage()
        {
            int rowIndex = (int)_stage / 2;
            bool useEven = ((int)_stage % 2 == 0);

            int y = _rows[rowIndex];
            int[] cols = useEven ? _evens : _odds;

            int cost = Tower.GetNextTowerCosts(player.HomeLane);
            if (player.Gold < cost) return;

            foreach (int x in cols)
            {
                if (player.HomeLane.GetCellAt(x, y).Unit == null)
                {
                    var res = player.TryBuyTower<Tower>(x, y);
                    if (res == Player.TowerPlacementResult.NotEnoughGold) return;
                    return;
                }
            }

            _stage++;
            if (_stage > BuildStage.Row6Odd)
                _stage = BuildStage.Complete;
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
    }
}