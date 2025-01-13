using GdUnit4;
using Godot;
using System.Linq;
using System.Threading.Tasks;
using static GdUnit4.Assertions;

namespace CorruptedVirtues.Tests.GDUnitTests
{
    [TestSuite]
    public class PathfindingTests
    {
        private GridMap gridMap;
        private AstarPathfinding astar;
        private Unit unit; 
        private Node3D battleMapRoot;


        [BeforeTest]
        public void Setup()
        {
            battleMapRoot = GD.Load<PackedScene>("res://Game/BattleScene/BattleMap.tscn").Instantiate<Node3D>();
            astar = battleMapRoot.GetNode<AstarPathfinding>("PathFinding");
            gridMap = astar.GetNode<GridMap>("Map");
            unit = GD.Load<PackedScene>("res://Game/Units/Unit.tscn").Instantiate<Unit>();
            Node3D? unitsNode = battleMapRoot.GetNode<Node3D>("Units");
            unitsNode.AddChild(unit);
            unit.Position = new Vector3(0, 2, 0);  // Start on top of the grid
            SceneTree? tree = Engine.GetMainLoop() as SceneTree;
            tree?.Root.AddChild(battleMapRoot);
            astar.SetupGridMap(gridMap);           // Setup the grid for pathfinding
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
        public void GetPath_ValidStartAndEnd_ReturnsPath()
        {
            // Arrange
            Vector3 start = new Vector3(0, 0, 0);
            Vector3 end = new Vector3(8, 0, 0);

            // Act
            Vector3[] path = astar.GetPath(start, end);

            // Assert
            AssertThat(path).IsNotEmpty();
            AssertThat(path.First()).IsEqual(start);
            AssertThat(path.Last()).IsEqual(end);
        }

        [TestCase]
        public void GetPath_InvalidStartOrEnd_ReturnsEmpty()
        {
            // Arrange
            Vector3 invalidStart = new Vector3(-10, 2, -10);
            Vector3 validEnd = new Vector3(4, 2, 0);

            // Act
            Vector3[] path = astar.GetPath(invalidStart, validEnd);

            // Assert
            AssertThat(path).IsEmpty();
        }

        [TestCase]
        public void IsWalkableCell_ValidWalkableCell_ReturnsTrue()
        {
            // Arrange
            Vector3I walkableCell = new Vector3I(0, 0, 0);

            // Act
            bool isWalkable = gridMap != null && astar.IsWalkableCell(walkableCell, gridMap);

            // Assert
            AssertThat(isWalkable).IsTrue();
        }

        [TestCase]
        public void IsWalkableCell_NonWalkableCell_ReturnsFalse()
        {
            // Arrange
            Vector3I nonWalkableCell = new Vector3I(-1, 0, -1); // Example invalid cell

            // Act
            bool isWalkable = gridMap != null && astar.IsWalkableCell(nonWalkableCell, gridMap);

            // Assert
            AssertThat(isWalkable).IsFalse();
        }
        //[TestCase]
        //public async Task UnitMovesToDestination()
        //{
        //    // Arrange
        //    Vector3 start = unit.Position;
        //    Vector3 destination = new Vector3(8, 2, 0); // Goal position

        //    // Act
        //    Vector3[] path = astar.GetPath(start, destination);
        //    astar.VisualizePath(path);
        //    await unit.Move(path);

        //    // Assert
        //    AssertThat(unit.Position).IsEqual(destination);
        //}

    }
}