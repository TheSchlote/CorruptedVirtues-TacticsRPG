using System.Collections.Generic;
using System.Linq;

public class Team
{
    public int TeamID { get; private set; }  // Unique ID for the team
    public bool IsPlayerControlled { get; private set; }  // Determines if this team is controlled by the player
    public List<Unit> Units { get; private set; } = new List<Unit>();  // List of all units in this team

    public Team(int id, bool isPlayerControlled)
    {
        TeamID = id;
        IsPlayerControlled = isPlayerControlled;
    }

    public void AddUnit(Unit unit)
    {
        if (!Units.Contains(unit))
        {
            Units.Add(unit);
        }
    }

    public void RemoveUnit(Unit unit)
    {
        if (Units.Contains(unit))
        {
            Units.Remove(unit);
        }
    }

    public bool HasLivingUnits()
    {
        return Units.Any(unit => unit.IsAlive);
    }
}
