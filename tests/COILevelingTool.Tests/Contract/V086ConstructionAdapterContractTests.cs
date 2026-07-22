using System;
using COILevelingTool.Compatibility;
using COILevelingTool.Compatibility.V086;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace COILevelingTool.Tests.Contract;

[TestClass]
public sealed class V086ConstructionAdapterContractTests
{
    [TestMethod]
    public void ForwardsConstructionCompletion()
    {
        var entity = new object();
        var fake = new FakeConstruction();
        var adapter = new CoiConstructionAdapterV086(fake);
        object? completed = null;
        adapter.Constructed += value => completed = value;

        fake.Raise(entity);

        Assert.AreSame(entity, completed);
    }

    [TestMethod]
    public void DuplicateCompletionForSameEntityIsForwardedOnce()
    {
        var entity = new object();
        var fake = new FakeConstruction();
        var adapter = new CoiConstructionAdapterV086(fake);
        var calls = 0;
        adapter.Constructed += _ => calls++;

        fake.Raise(entity);
        fake.Raise(entity);

        Assert.AreEqual(1, calls);
    }

    [TestMethod]
    public void RemovesTemporaryEntityThroughNormalCleanup()
    {
        var entity = new object();
        var fake = new FakeConstruction();
        var result = new CoiConstructionAdapterV086(fake).RemoveTemporaryEntity(entity, "completed");

        Assert.IsTrue(result.Succeeded);
        Assert.AreSame(entity, fake.RemovedEntity);
        Assert.AreEqual("completed", fake.RemovalReason);
        Assert.AreEqual(0, fake.AdditionalChargeCalls);
    }

    [TestMethod]
    public void CleanupFailureIsExplicitAndDoesNotChargeOrRefund()
    {
        var fake = new FakeConstruction { RemovalSucceeds = false };
        var result = new CoiConstructionAdapterV086(fake).RemoveTemporaryEntity(new object(), "failed-no-refund");

        Assert.AreEqual(AdapterFailure.CleanupFailed, result.Failure);
        StringAssert.Contains(result.Detail, "no refund was attempted");
        Assert.AreEqual(0, fake.AdditionalChargeCalls);
    }

    private sealed class FakeConstruction : IConstructionAccessV086
    {
        public event Action<object>? Constructed;
        public bool RemovalSucceeds { get; set; } = true;
        public object? RemovedEntity { get; private set; }
        public string? RemovalReason { get; private set; }
        public int AdditionalChargeCalls { get; private set; }

        public void Raise(object entity) => Constructed?.Invoke(entity);
        public bool TryRemove(object entity, string reason)
        {
            RemovedEntity = entity;
            RemovalReason = reason;
            return RemovalSucceeds;
        }
    }
}
