using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace COILevelingTool.Compatibility.V086;

public interface IConstructionAccessV086
{
    event Action<object> Constructed;
    bool TryRemove(object entity, string reason);
}

public sealed class CoiConstructionAdapterV086 : ICoiConstructionAdapter
{
    private readonly IConstructionAccessV086 m_access;
    private readonly HashSet<object> m_completed = new(ReferenceComparer.Instance);

    public CoiConstructionAdapterV086(IConstructionAccessV086 access)
    {
        m_access = access ?? throw new ArgumentNullException(nameof(access));
        m_access.Constructed += entity =>
        {
            if (m_completed.Add(entity)) Constructed?.Invoke(entity);
        };
    }

    public event Action<object>? Constructed;

    public AdapterResult RemoveTemporaryEntity(object entity, string reason) =>
        m_access.TryRemove(entity, reason)
            ? AdapterResult.Success()
            : AdapterResult.Fail(AdapterFailure.CleanupFailed, "Normal entity cleanup failed; no refund was attempted.");

    private sealed class ReferenceComparer : IEqualityComparer<object>
    {
        public static ReferenceComparer Instance { get; } = new();
        public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);
        public int GetHashCode(object value) => RuntimeHelpers.GetHashCode(value);
    }
}
