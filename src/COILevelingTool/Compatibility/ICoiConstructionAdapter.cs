using System;

namespace COILevelingTool.Compatibility;

public interface ICoiConstructionAdapter
{
    event Action<object>? Constructed;
    AdapterResult RemoveTemporaryEntity(object entity, string reason);
}
