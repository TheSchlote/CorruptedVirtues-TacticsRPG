using GdUnit4;
using Godot;
using System.Linq;
using System.Threading.Tasks;
using static GdUnit4.Assertions;

namespace CorruptedVirtues.Tests.GDUnitTests
{
    [TestSuite]
    public partial class SelectionCursorTest
    {
        private SelectionCursor? cursor;
        private GridMap? gridMap;
        private Node3D? battleMapRoot;

        [BeforeTest]
        public void Setup()
        {
            battleMapRoot = GD.Load<PackedScene>("res://Game/BattleScene/JoeyTest.tscn").Instantiate<Node3D>();
            gridMap = battleMapRoot.GetNode<GridMap>("Map");

            cursor = new SelectionCursor();
            battleMapRoot.AddChild(cursor);

            SceneTree? tree = Engine.GetMainLoop() as SceneTree;
            tree?.Root.AddChild(battleMapRoot);
        }

        [AfterTest]
        public void TearDownTest()
        {
            if (battleMapRoot != null && battleMapRoot.IsInsideTree())
            {
                battleMapRoot.QueueFree();
            }
        }

        //[TestCase]
        //public void Cursor_ShouldSnap_ToGrid()
        //{
        //    cursor!.SetGridPosition(new Vector3(3, 0, 4));

        //    AssertThat(cursor.GetSelectedTile()).IsEqual(new Vector3(3, 0, 4));
        //}
        //[TestCase]
        //public void Cursor_ShouldMove_OneStepRight()
        //{
        //    cursor!.SetGridPosition(new Vector3(2, 0, 2));

        //    cursor.HandleInput(Vector3.Right); // Move Right

        //    AssertThat(cursor.GetSelectedTile()).IsEqual(new Vector3(4, 0, 2)); // Should snap to grid size
        //}

    }
}
