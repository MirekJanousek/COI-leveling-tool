namespace COILevelingTool.Compatibility;

public interface ICoiPlacementAdapter
{
    AdapterResult Preview(PlacementRequest request);
    PlacementBatchResult Commit(PlacementRequest request);
}

