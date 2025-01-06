using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class AstarPathfinding : Node3D
{
    private AStar3D astar = new AStar3D();
    private const int CELL_SIZE = 2;
    public Vector3 GridToWorld(Vector3 gridPosition)
    {
        return new Vector3(gridPosition.X * CELL_SIZE, 2, gridPosition.Z * CELL_SIZE);  // Offset Y by 2
    }

    public void SetupGridMap(GridMap gridMap)
    {
        astar.Clear();  // Reset existing pathfinding data

        foreach (Vector3I cell in gridMap.GetUsedCells())
        {
            Vector3 cellPosition = new Vector3(cell.X * CELL_SIZE, 0, cell.Z * CELL_SIZE);
            astar.AddPoint(cell.X * 1000 + cell.Z, cellPosition);
        }

        foreach (long pointId in astar.GetPointIds())
        {
            Vector3 point = astar.GetPointPosition(pointId);
            ConnectNeighbors(pointId, point);
        }
    }
    public Vector3[] GetPath(Vector3 start, Vector3 end)
    {
        int startId = (int)(start.X / CELL_SIZE) * 1000 + (int)(start.Z / CELL_SIZE);
        int endId = (int)(end.X / CELL_SIZE) * 1000 + (int)(end.Z / CELL_SIZE);
        return astar.GetPointPath(startId, endId).ToArray();
    }
    private void ConnectNeighbors(long pointId, Vector3 point)
    {
        List<Vector3> neighbors = GetNeighborPoints(point);
        foreach (Vector3 neighbor in neighbors)
        {
            int neighborId = (int)(neighbor.X / CELL_SIZE) * 1000 + (int)(neighbor.Z / CELL_SIZE);
            if (astar.HasPoint(neighborId))
            {
                astar.ConnectPoints((int)pointId, neighborId);
            }
        }
    }
    private List<Vector3> GetNeighborPoints(Vector3 point)
    {
        return new List<Vector3>
        {
            point + new Vector3(CELL_SIZE, 0, 0),
            point + new Vector3(-CELL_SIZE, 0, 0),
            point + new Vector3(0, 0, CELL_SIZE),
            point + new Vector3(0, 0, -CELL_SIZE)
        };
    }

    public void VisualizePath(Vector3[] path)
    {
        ImmediateMesh lineMesh = new ImmediateMesh();
        MeshInstance3D meshInstance = new MeshInstance3D
        {
            Mesh = lineMesh,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
        };
        AddChild(meshInstance);

        lineMesh.SurfaceBegin(Mesh.PrimitiveType.Lines);

        for (int i = 0; i < path.Length - 1; i++)
        {
            Vector3 start = path[i] + new Vector3(0, 2.5f, 0);  // Raise slightly above grid
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
}
