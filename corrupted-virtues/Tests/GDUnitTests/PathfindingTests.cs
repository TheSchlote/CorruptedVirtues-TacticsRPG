using GdUnit4;
using Godot;
using System.Threading.Tasks;
using static GdUnit4.Assertions;

namespace CorruptedVirtues.Tests.GDUnitTests
{
    [TestSuite]
    public class PathfindingTests
    {
        private GridMap? gridMap;
        private AstarPathfinding? astar;
        private Unit? unit; 
        private Node3D? battleMapRoot;


        [BeforeTest]
        public void Setup()
        {
            battleMapRoot = GD.Load<PackedScene>("res://Game/BattleScene/BattleMap.tscn").Instantiate<Node3D>();
            gridMap = battleMapRoot.GetNode<GridMap>("Map");
            astar = battleMapRoot.GetNode<AstarPathfinding>("PathFinding");
            unit = GD.Load<PackedScene>("res://Game/Units/Unit.tscn").Instantiate<Unit>();
            Node3D? unitsNode = battleMapRoot.GetNode<Node3D>("Units");
            unitsNode.AddChild(unit);
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
        public async Task UnitMovesToDestination()
        {
            // Arrange
            unit.Position = new Vector3(0, 2, 0);  // Start on top of the grid
            astar.SetupGridMap(gridMap);           // Setup the grid for pathfinding

            Vector3 start = unit.Position;
            Vector3 destination = new Vector3(8, 2, 0); // Goal position

            // Act
            Vector3[] path = astar.GetPath(start, destination);
            astar.VisualizePath(path);
            await unit.MoveAlongPath(path);

            // Assert
            AssertThat(unit.Position).IsEqual(destination);
        }

    }
}