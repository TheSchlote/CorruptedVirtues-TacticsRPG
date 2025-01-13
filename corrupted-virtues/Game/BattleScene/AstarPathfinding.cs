using Godot;
using System.Linq;

public partial class AstarPathfinding : Node3D
{
    private const int CELL_SIZE = 2;
    public AStar3D astar = new AStar3D();
    private const float SPHERE_HEIGHT_OFFSET = 3f;
    public void VisualizePath(Vector3[] path, Node3D parent)
    {
        ImmediateMesh lineMesh = new ImmediateMesh();
        MeshInstance3D meshInstance = new MeshInstance3D
        {
            Mesh = lineMesh,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
        };
        parent.AddChild(meshInstance);

        lineMesh.SurfaceBegin(Mesh.PrimitiveType.Lines);

        for (int i = 0; i < path.Length - 1; i++)
        {
            Vector3 start = path[i] + new Vector3(0, 2.5f, 0); // Raise slightly above grid
            Vector3 end = path[i + 1] + new Vector3(0, 2.5f, 0);

            lineMesh.SurfaceAddVertex(start);
            lineMesh.SurfaceAddVertex(end);
        }

        lineMesh.SurfaceEnd();

        StandardMaterial3D mat = new StandardMaterial3D
        {
            AlbedoColor = Colors.Cyan,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded
        };
        meshInstance.MaterialOverride = mat;
    }

    public void VisualizeNeighbors(GridMap gridMap, AStar3D astar, Node3D parent)
    {
        GD.Print("Visualizing neighbors...");

        // Create reusable line mesh for neighbor connections
        ImmediateMesh lineMesh = new ImmediateMesh();
        var lineMeshInstance = new MeshInstance3D
        {
            Mesh = lineMesh,
            MaterialOverride = new StandardMaterial3D
            {
                AlbedoColor = Colors.Cyan,
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded
            }
        };
        parent.AddChild(lineMeshInstance);

        foreach (int cellId in astar.GetPointIds())
        {
            Vector3 worldPosition = astar.GetPointPosition(cellId);

            // Spawn sphere for the cell
            SpawnSphere(worldPosition, parent);

            // Visualize connections to neighbors
            foreach (int neighborId in astar.GetPointConnections(cellId))
            {
                Vector3 neighborPosition = astar.GetPointPosition(neighborId);
                DrawConnection(lineMesh, worldPosition, neighborPosition);
            }
        }
    }

    private void SpawnSphere(Vector3 position, Node3D parent)
    {
        var sphere = new MeshInstance3D
        {
            Mesh = new SphereMesh { Radius = 0.3f },
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
            GlobalTransform = new Transform3D(Basis.Identity, position + new Vector3(0, SPHERE_HEIGHT_OFFSET, 0))
        };
        parent.AddChild(sphere);
    }

    private void DrawConnection(ImmediateMesh lineMesh, Vector3 from, Vector3 to)
    {
        lineMesh.SurfaceBegin(Mesh.PrimitiveType.Lines);
        lineMesh.SurfaceAddVertex(from + new Vector3(0, SPHERE_HEIGHT_OFFSET, 0));
        lineMesh.SurfaceAddVertex(to + new Vector3(0, SPHERE_HEIGHT_OFFSET, 0));
        lineMesh.SurfaceEnd();
    }
    public override void _Ready()
    {
        GD.Print("AstarPathfinding: Initializing...");

        // Load and set up the GridMap
        GridMap gridMap = GetNode<GridMap>("Map");
        if (gridMap == null)
        {
            GD.PrintErr("GridMap 'Map' not found. Please ensure it's in the scene.");
            return;
        }

        SetupGridMap(gridMap);
        // Visualize neighbors
        VisualizeNeighbors(gridMap, astar, this);
    }

    public void SetupGridMap(GridMap gridMap)
    {
        astar.Clear();

        foreach (Vector3I cell in gridMap.GetUsedCells())
        {
            if (IsWalkableCell(cell, gridMap))
            {
                AddCellToAStar(cell, gridMap);
            }
        }

        ConnectWalkableCells(gridMap);
    }

    public Vector3[] GetPath(Vector3 start, Vector3 end)
    {
        int startId = GetCellIdFromPosition(LocalToMap(start));
        int endId = GetCellIdFromPosition(LocalToMap(end));

        if (!astar.HasPoint(startId) || !astar.HasPoint(endId))
        {
            GD.PrintErr($"Invalid path request. Start: {start}, End: {end}");
            return System.Array.Empty<Vector3>();
        }

        return astar.GetPointPath(startId, endId).ToArray();
    }

    private void AddCellToAStar(Vector3I cell, GridMap gridMap)
    {
        int cellId = GetCellIdFromPosition(cell);
        Vector3 localPosition = gridMap.MapToLocal(cell);
        Vector3 worldPosition = gridMap.GlobalTransform.Origin + localPosition;
        astar.AddPoint(cellId, worldPosition, 1);
    }

    private void ConnectWalkableCells(GridMap gridMap)
    {
        foreach (int cellId in astar.GetPointIds())
        {
            ConnectCellNeighbors(cellId, gridMap);
        }
    }

    private void ConnectCellNeighbors(int cellId, GridMap gridMap)
    {
        Vector3I cellPosition = GetPositionFromCellId(cellId);
        Vector3I[] directions = { Vector3I.Right, Vector3I.Left, Vector3I.Forward, Vector3I.Back };

        foreach (Vector3I direction in directions)
        {
            Vector3I horizontalNeighbor = cellPosition + direction;
            Vector3I upperNeighbor = horizontalNeighbor + Vector3I.Up;
            Vector3I lowerNeighbor = horizontalNeighbor + Vector3I.Down;

            if (IsWalkableCell(horizontalNeighbor, gridMap))
            {
                ConnectIfPossible(cellPosition, horizontalNeighbor);
            }
            else if (IsWalkableCell(upperNeighbor, gridMap))
            {
                ConnectIfPossible(cellPosition, upperNeighbor);
            }
            else if (IsWalkableCell(lowerNeighbor, gridMap))
            {
                ConnectIfPossible(cellPosition, lowerNeighbor);
            }
        }
    }

    private void ConnectIfPossible(Vector3I from, Vector3I to)
    {
        int fromId = GetCellIdFromPosition(from);
        int toId = GetCellIdFromPosition(to);
        if (astar.HasPoint(toId))
        {
            astar.ConnectPoints(fromId, toId, true);
        }
    }

    public bool IsWalkableCell(Vector3I cell, GridMap gridMap)
    {
        int tileId = gridMap.GetCellItem(cell);
        return tileId == GetMeshLibraryItemIdByName(gridMap, "Walkable");
    }

    private int GetMeshLibraryItemIdByName(GridMap gridMap, string name)
    {
        foreach (int key in gridMap.MeshLibrary.GetItemList())
        {
            if (gridMap.MeshLibrary.GetItemName(key) == name)
            {
                return key;
            }
        }
        return -1; // Return invalid ID if not found
    }

    private int GetCellIdFromPosition(Vector3I position)
    {
        return position.X + position.Y * 1000 + position.Z * 1000000;
    }

    private Vector3I GetPositionFromCellId(int cellId)
    {
        int x = cellId % 1000;
        int y = (cellId / 1000) % 1000;
        int z = cellId / 1000000;
        return new Vector3I(x, y, z);
    }

    private Vector3I LocalToMap(Vector3 position)
    {
        return new Vector3I((int)position.X / CELL_SIZE, (int)position.Y / CELL_SIZE, (int)position.Z / CELL_SIZE);
    }
}