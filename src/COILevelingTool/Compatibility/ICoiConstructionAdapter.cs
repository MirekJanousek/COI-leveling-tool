using System;

namespace COILevelingTool.Compatibility;

public interface ICoiConstructionAdapter
{
    event Action<object>? Constructed;
    AdapterResult CaptureLedger(object entity, out ChargeLedger ledger);
    AdapterResult RemoveTemporaryEntity(object entity, string reason);
    CompensationResult Refund(ChargeLedger ledger);
}

