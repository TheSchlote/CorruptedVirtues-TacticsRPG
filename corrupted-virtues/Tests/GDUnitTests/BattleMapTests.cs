using GdUnit4;
using Godot;
using static GdUnit4.Assertions;

namespace CorruptedVirtues.Tests.GDUnitTests
{
    [TestSuite]
    public class BattleMapTests
    {
        private GridMap gridMapScene;
        private Node3D unitScene;
        //private Unit unit;

        [BeforeTest]
        public void Setup()
        {
            gridMapScene = GD.Load<PackedScene>("res://Game/BattleScene/BattleMap.tscn").Instantiate<GridMap>();
            unitScene = GD.Load<PackedScene>("res://Game/Units/Unit.tscn").Instantiate<Node3D>();
        }

        [TestCase]
        public void UnitMovesToAdjacentTile()
        {
            gridMapScene.AddChild(unitScene);
            unitScene.Position = new Vector3(2, 0, 2);  // Starting position

            // Call a MoveTo method that doesn't exist yet (causing a compile error)
            ((Unit)unitScene).MoveTo(new Vector3(1, 0, 0));  // Attempt to move 1 tile to the right

            AssertVector(unitScene.Position).IsEqual(new Vector3(4, 0, 2));  // Expect the unit to move right by 2 units (due to 2x2x2 grid)
        }


    }
}
