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
            unit.Position = new Vector3(0, 0, 0);
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
            Vector3 invalidStart = new Vector3(-10, 0, -10);
            Vector3 validEnd = new Vector3(4, 0, 0);

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
        [TestCase]
        public void MarkCellAsOccupied_UpdatesConnections()
        {
            // Arrange
            Vector3I cell = new Vector3I(4, 0, 4);
            astar.SetupGridMap(gridMap);

            // Act
            astar.MarkCellAsOccupied(cell);

            // Assert
            AssertThat(astar.IsCellOccupied(cell)).IsTrue();
            AssertThat(astar.IsWalkableCell(cell, gridMap)).IsFalse();
        }

        [TestCase]
        public void MarkCellAsUnoccupied_UpdatesConnections()
        {
            // Arrange
            Vector3I cell = new Vector3I(4, 0, 4);
            astar.SetupGridMap(gridMap);
            astar.MarkCellAsOccupied(cell);

            // Act
            astar.MarkCellAsUnoccupied(cell);

            // Assert
            AssertThat(astar.IsCellOccupied(cell)).IsFalse();
            AssertThat(astar.IsWalkableCell(cell, gridMap)).IsTrue();
        }

        [TestCase]
        public void SpawnUnit_OnValidCell_Success()
        {
            // Arrange
            BattleManager battleManager = (BattleManager)battleMapRoot;
            Vector3 spawnPosition = new Vector3(8, 0, 8);

            // Act
            Unit spawnedUnit = battleManager.SpawnUnit(spawnPosition);

            // Assert
            AssertThat(spawnedUnit).IsNotNull();
            AssertThat(spawnedUnit.Position).IsEqual(spawnPosition);
            AssertThat(astar.IsCellOccupied(astar.LocalToMap(spawnPosition))).IsTrue();
        }

        [TestCase]
        public void SpawnUnit_OnInvalidCell_Fails()
        {
            // Arrange
            BattleManager battleManager = (BattleManager)battleMapRoot;
            Vector3 invalidSpawnPosition = new Vector3(-10, 0, -10); // Non-walkable

            // Act
            Unit spawnedUnit = battleManager.SpawnUnit(invalidSpawnPosition);

            // Assert
            AssertThat(spawnedUnit).IsNull();
        }
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
        public async Task Unit_CanPathfindOutOfOccupiedStartingCell()
        {
            // Arrange
            BattleManager battleManager = (BattleManager)battleMapRoot;
            astar.SetupGridMap(gridMap);

            // Spawn a unit in an unwalkable spot
            Vector3 startPosition = new Vector3(4, 0, 4); // Initially occupied
            Unit unit = battleManager.SpawnUnit(startPosition);
            AssertThat(unit).IsNotNull();

            // Confirm the starting cell is marked as occupied
            Vector3I startCell = astar.LocalToMap(startPosition);
            AssertThat(astar.IsCellOccupied(startCell)).IsTrue();

            // Define a valid destination
            Vector3 destination = new Vector3(8, 0, 0);

            // Act
            await unit.MoveTo(destination, astar);

            // Assert
            // Confirm the unit moved to the destination
            AssertThat(unit.Position).IsEqual(destination);

            // Confirm the starting cell is now unoccupied
            AssertThat(astar.IsCellOccupied(startCell)).IsFalse();

            // Confirm the destination cell is occupied
            Vector3I destinationCell = astar.LocalToMap(destination);
            AssertThat(astar.IsCellOccupied(destinationCell)).IsTrue();
        }

        [TestCase]
        public async Task Unit_PathfindsAroundOtherUnit()
        {
            // Arrange
            BattleManager battleManager = (BattleManager)battleMapRoot;
            astar.SetupGridMap(gridMap);

            // Spawn the first unit at (0, 0, 0)
            Vector3 firstUnitPosition = new Vector3(0, 0, 0);
            Unit firstUnit = battleManager.SpawnUnit(firstUnitPosition);
            AssertThat(firstUnit).IsNotNull();

            // Spawn the second unit at (4, 0, 4), which will act as an obstacle
            Vector3 secondUnitPosition = new Vector3(4, 0, 4);
            Unit secondUnit = battleManager.SpawnUnit(secondUnitPosition);
            AssertThat(secondUnit).IsNotNull();

            // Define the target destination for the first unit
            Vector3 targetPosition = new Vector3(8, 0, 0);

            // Get the path and visualize it
            Vector3[] path = astar.GetPath(firstUnitPosition, targetPosition);

            // Act
            await firstUnit.MoveTo(targetPosition, astar);

            // Assert
            AssertThat(firstUnit.Position).IsEqual(targetPosition);

            // Ensure the path does not include the occupied cell
            Vector3I occupiedCell = astar.LocalToMap(secondUnitPosition);
            foreach (Vector3 step in path)
            {
                Vector3I cell = astar.LocalToMap(step);
                AssertThat(cell).IsNotEqual(occupiedCell);
            }
        }
        [TestCase]
        public async Task Unit_PathfindsAroundOtherUnits()
        {
            // Arrange
            BattleManager battleManager = (BattleManager)battleMapRoot;
            astar.SetupGridMap(gridMap);

            // Spawn the first unit at (0, 0, 0)
            Vector3 firstUnitPosition = new Vector3(0, 0, 0);
            Unit firstUnit = battleManager.SpawnUnit(firstUnitPosition);
            AssertThat(firstUnit).IsNotNull();

            // Spawn the second unit at (6, 0, 4), which will act as an obstacle
            Vector3 secondUnitPosition = new Vector3(6, 0, 4);
            Unit secondUnit = battleManager.SpawnUnit(secondUnitPosition);
            AssertThat(secondUnit).IsNotNull();

            // Spawn the third unit at (0, 0, 8), which will act as an obstacle
            Vector3 thirdUnitPosition = new Vector3(0, 0, 8);
            Unit thirdUnit = battleManager.SpawnUnit(thirdUnitPosition);
            AssertThat(thirdUnit).IsNotNull();

            // Define the target destination for the first unit
            Vector3 targetPosition = new Vector3(8, 0, 0);

            // Ensure that all obstacles are marked as occupied
            Vector3I secondUnitCell = astar.LocalToMap(secondUnitPosition);
            Vector3I thirdUnitCell = astar.LocalToMap(thirdUnitPosition);
            AssertThat(astar.IsCellOccupied(secondUnitCell)).IsTrue();
            AssertThat(astar.IsCellOccupied(thirdUnitCell)).IsTrue();

            // Get the path and visualize it
            Vector3[] path = astar.GetPath(firstUnitPosition, targetPosition);
            astar.VisualizePath(path, battleMapRoot);

            // Act
            await firstUnit.MoveTo(targetPosition, astar);

            // Assert
            // Confirm that the first unit successfully moved to the target position
            AssertThat(firstUnit.Position).IsEqual(targetPosition);

            // Verify that the path avoids occupied cells
            foreach (Vector3 step in path)
            {
                Vector3I cell = astar.LocalToMap(step);
                AssertThat(cell).IsNotEqual(secondUnitCell);
                AssertThat(cell).IsNotEqual(thirdUnitCell);
            }
        }

    }
}