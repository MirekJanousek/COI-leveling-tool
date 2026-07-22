using System.Collections.Generic;

namespace COILevelingTool.Compatibility;

public interface ICoiTerrainAdapter
{
    AdapterResult Snapshot(IReadOnlyList<GridPoint> vertices, out TerrainState snapshot);
    AdapterResult ApplyWithoutPhysics(TerrainState snapshot, TerrainHeight elevation);
    bool Verify(IReadOnlyList<GridPoint> vertices, TerrainHeight elevation);
    AdapterResult Restore(TerrainState snapshot);
}

