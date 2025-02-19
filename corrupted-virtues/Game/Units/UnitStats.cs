using Godot;

[GlobalClass]
public partial class UnitStats : Resource
{
    [Export] public string UnitName { get; set; } = "Default Unit";
    [Export] public int MaxHealth { get; set; } = 100;
    [Export] public int Speed { get; set; } = 10;
    [Export] public PackedScene UnitVisual { get; set; }
}
