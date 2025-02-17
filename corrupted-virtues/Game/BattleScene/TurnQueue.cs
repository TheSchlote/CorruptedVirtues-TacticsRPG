using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    public class TurnQueue
    {
        private List<Unit> unitQueue = new List<Unit>();

        public void InitializeQueue(List<Team> teams)
        {
            foreach (var team in teams)
            {
                unitQueue.AddRange(team.Units);
            }
            unitQueue = unitQueue.OrderByDescending(unit => unit.Initiative).ToList();
        }

        public Unit GetNextUnit()
        {
            if (unitQueue.Count == 0)
                return null;

            Unit nextUnit = unitQueue[0];
            unitQueue.RemoveAt(0);
            return nextUnit;
        }

        public void AddUnitBackToQueue(Unit unit)
        {
            unit.IncreaseInitiative();  // Adjust initiative for the next turn
            unitQueue.Add(unit);
            unitQueue = unitQueue.OrderByDescending(u => u.Initiative).ToList();
        }
    }
