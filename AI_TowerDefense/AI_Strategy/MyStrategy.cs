using System;
using System.Collections.Generic;
using GameFramework;

namespace AI_Strategy
{
    public class MyStrategy : AbstractStrategy
    {
        private Random rand = new Random();

        public MyStrategy(Player player) : base(player) { }

        public override void DeploySoldiers()
        {
            if (player.Gold < 2)
                return;

            int width = PlayerLane.WIDTH;

            while (player.Gold >= 2)
            {
                // pick a random column among the weakest 3? columns
                List<int> candidateColumns = new List<int>();
                int minHP = int.MaxValue;
                for (int x = 0; x < width; x++)
                {
                    int columnHP = 0;
                    for (int y = PlayerLane.HEIGHT_OF_SAFETY_ZONE; y < PlayerLane.HEIGHT; y++)
                    {
                        var cell = player.EnemyLane.GetCellAt(x, y);
                        if (cell.Unit is Tower t)
                            columnHP += t.Health;
                    }

                    if (columnHP < minHP)
                    {
                        candidateColumns.Clear();
                        candidateColumns.Add(x);
                        minHP = columnHP;
                    }
                    else if (columnHP == minHP)
                    {
                        candidateColumns.Add(x);
                    }
                }

                // randomly choose one column from the candidates
                int targetColumn = candidateColumns[rand.Next(candidateColumns.Count)];

                // try to deploy soldier at y=0
                if (player.TryBuySoldier<Soldier>(targetColumn, out var soldier) != Player.SoldierPlacementResult.Success)
                {
                    // if failed (cell occupied) try neighboring columns
                    int left = targetColumn - 1;
                    int right = targetColumn + 1;
                    bool placed = false;

                    if (left >= 0)
                        placed = player.TryBuySoldier<Soldier>(left, out soldier) == Player.SoldierPlacementResult.Success;
                    if (!placed && right < width)
                        placed = player.TryBuySoldier<Soldier>(right, out soldier) == Player.SoldierPlacementResult.Success;

                    if (!placed)
                        break; // can't place more soldiers
                }
            }
        }

        public override void DeployTowers()
        {
            int nextTowerCost = Tower.GetNextTowerCosts(player.HomeLane);
            if (player.Gold < nextTowerCost)
                return;

            int width = PlayerLane.WIDTH;

            for (int attempt = 0; attempt < 5; attempt++)
            {
                int threatenedColumn = GetMostThreatenedColumn();

                int x = (threatenedColumn + rand.Next(-1, 2) + width) % width;

                int y = rand.Next(PlayerLane.HEIGHT_OF_SAFETY_ZONE, PlayerLane.HEIGHT);

                if (player.HomeLane.GetCellAt(x, y).Unit == null)
                {
                    player.TryBuyTower<Tower>(x, y, out var tower);
                    break;
                }
            }
        }

        public override List<Soldier> SortedSoldierArray(List<Soldier> list)
        {
            list.Sort((a, b) => b.PosY.CompareTo(a.PosY));
            return list;
        }

        public override List<Tower> SortedTowerArray(List<Tower> list)
        {
            list.Sort((a, b) => a.Health.CompareTo(b.Health));
            return list;
        }
     
        private int GetMostThreatenedColumn()
        {
            int width = PlayerLane.WIDTH;
            int[] threat = new int[width];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < PlayerLane.HEIGHT; y++)
                {
                    var cell = player.EnemyLane.GetCellAt(x, y);
                    if (cell.Unit is Soldier s)
                        threat[x] += s.Health;
                }
            }

            int worst = 0, max = 0;
            for (int x = 0; x < width; x++)
                if (threat[x] > max) { max = threat[x]; worst = x; }

            return worst;
        }

      /*  private int GetWeakestEnemyColumn()
        {
            int width = PlayerLane.WIDTH;
            int[] towerHP = new int[width];

            for (int x = 0; x < width; x++)
            {
                for (int y = PlayerLane.HEIGHT_OF_SAFETY_ZONE; y < PlayerLane.HEIGHT; y++)
                {
                    var cell = player.EnemyLane.GetCellAt(x, y);
                    if (cell.Unit is Tower t)
                        towerHP[x] += t.Health;
                }
            }

            int weakest = 0, min = int.MaxValue;
            for (int x = 0; x < width; x++)
                if (towerHP[x] < min) { min = towerHP[x]; weakest = x; }

            return weakest;
        }*/
    }
}
