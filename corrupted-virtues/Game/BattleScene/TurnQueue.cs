using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TurnQueue
{
    private const int ACTION_THRESHOLD = 100;
    private List<Unit> unitQueue = new List<Unit>();

    public void InitializeQueue(List<Team> teams)
    {
        unitQueue.Clear();
        foreach (var team in teams)
        {
            unitQueue.AddRange(team.Units);
        }
    }

    public void AccumulateActionPoints()
    {
        foreach (Unit unit in unitQueue)
        {
            unit.AccumulateActionPoints();
        }

        // Sort units based on AP
        unitQueue = unitQueue.OrderByDescending(unit => unit.ActionPoints).ToList();
    }

    public Unit GetNextUnit()
    {
        // Get the unit with the **highest AP** that has at least ACTION_THRESHOLD
        Unit nextUnit = unitQueue
            .Where(unit => unit.ActionPoints >= ACTION_THRESHOLD)
            .OrderByDescending(unit => unit.ActionPoints) // Sort to get the highest AP
            .FirstOrDefault(); // Get the top one

        if (nextUnit != null)
        {
            nextUnit.SpendActionPoints(ACTION_THRESHOLD);
            unitQueue = unitQueue.OrderByDescending(unit => unit.ActionPoints).ToList();
            return nextUnit;
        }

        // No unit is ready, accumulate more AP and check again
        AccumulateActionPoints();
        return GetNextUnit(); // Recursively check again
    }

    public bool OnlyOneTeamRemaining()
    {
        // Get unique teams with living units
        var aliveTeams = unitQueue.Where(unit => unit.IsAlive).Select(unit => unit.Team).Distinct().ToList();
        return aliveTeams.Count <= 1; // If only one team remains, battle should end
    }
}

