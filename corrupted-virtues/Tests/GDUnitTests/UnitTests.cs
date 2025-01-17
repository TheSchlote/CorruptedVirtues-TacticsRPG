using GdUnit4;
using Godot;
using System.Linq;
using System.Threading.Tasks;
using static GdUnit4.Assertions;

namespace CorruptedVirtues.Tests.GDUnitTests
{
    [TestSuite]
    public partial class UnitTests
    {
        private GridMap? gridMap;
        private AstarPathfinding? astar;
        private Unit? unit;
        private Node3D? battleMapRoot;


        [BeforeTest]
        public void Setup()
        {
            battleMapRoot = GD.Load<PackedScene>("res://Game/BattleScene/BattleMap.tscn").Instantiate<Node3D>();
            astar = battleMapRoot.GetNode<AstarPathfinding>("PathFinding");
            gridMap = astar.GetNode<GridMap>("Map");
            unit = GD.Load<PackedScene>("res://Game/Units/Unit.tscn").Instantiate<Unit>();
            Node3D? unitsNode = battleMapRoot.GetNode<Node3D>("Units");
            unitsNode.AddChild(unit);
            unit.Position = new Vector3(0, 0, 0);

            SceneTree? tree = Engine.GetMainLoop() as SceneTree;
            tree?.Root.AddChild(battleMapRoot);

        }
        [AfterTest]
        public void TearDownTest()
        {
            if (battleMapRoot != null && battleMapRoot.IsInsideTree())
            {
                battleMapRoot.QueueFree();  // Free the root and all children
            }
        }
        [TestCase]
        public async Task Unit_FindsPathAndMovesToDestination()
        {
            // Arrange
            Vector3 destination = new Vector3(8, 0, 0); // Destination
            astar.SetupGridMap(gridMap);

            // Act
            await unit.MoveTo(destination, astar);

            // Assert
            AssertThat(unit.Position)
                  .IsEqual(destination);
        }
        //[TestCase]
        //public async Task Unit_MovesThroughPath_StepByStep()
        //{
        //    // Arrange
        //    Vector3 start = unit.Position;
        //    Vector3 destination = new Vector3(8, 0, 0);
        //    astar.SetupGridMap(gridMap);

        //    // Act
        //    await unit.MoveTo(destination, astar);

        //    // Assert
        //    AssertThat(unit.Position).IsEqual(destination);
        //    Vector3[] path = astar.GetPath(start, destination);
        //    foreach (Vector3 step in path)
        //    {
        //        AssertThat(gridMap.IsPositionOnMap(step)).IsTrue();
        //    }
        //}
        [TestCase]
        public void GetPath_UnreachableDestination_ReturnsEmpty()
        {
            // Arrange
            Vector3 start = new Vector3(0, 0, 0);
            Vector3 unreachableEnd = new Vector3(-10, 0, -10);

            // Act
            Vector3[] path = astar.GetPath(start, unreachableEnd);

            // Assert
            AssertThat(path).IsEmpty();
        }

        [TestCase]
        public void Unit_InteractsWithOccupiedCell()
        {
            // Arrange
            Vector3I cell = astar.LocalToMap(new Vector3(4, 0, 4));
            astar.MarkCellAsOccupied(cell);

            // Act
            bool isOccupied = astar.IsCellOccupied(cell);
            bool isWalkable = astar.IsWalkableCell(cell, gridMap);

            // Assert
            AssertThat(isOccupied).IsTrue();
            AssertThat(isWalkable).IsFalse();
        }

    }
}