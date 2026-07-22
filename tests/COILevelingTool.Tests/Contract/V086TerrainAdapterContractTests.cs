using System;
using System.Collections.Generic;
using System.Linq;
using COILevelingTool.Compatibility;
using COILevelingTool.Compatibility.V086;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace COILevelingTool.Tests.Contract;

[TestClass]
public sealed class V086TerrainAdapterContractTests
{
    private static readonly GridPoint[] Vertices =
    {
        new(4, 4), new(4, 5), new(5, 4), new(5, 5)
    };

    [TestMethod]
    public void SnapshotDeduplicatesAndOrdersFourVertices()
    {
        var fake = FakeTerrain.Create(Vertices, 10);
        var adapter = new CoiTerrainAdapterV086(fake, () => true);
        var input = new[] { Vertices[3], Vertices[0], Vertices[2], Vertices[1], Vertices[0] };

        var result = adapter.Snapshot(input, out var snapshot);

        Assert.IsTrue(result.Succeeded);
        CollectionAssert.AreEqual(Vertices, snapshot.Heights.Keys.ToArray());
    }

    [TestMethod]
    public void ApplyNotifiesEveryWriteAndTracksChangedVertices()
    {
        var fake = FakeTerrain.Create(Vertices, 10);
        var adapter = new CoiTerrainAdapterV086(fake, () => true);
        adapter.Snapshot(Vertices, out var snapshot);

        var result = adapter.ApplyWithoutPhysics(snapshot, new TerrainHeight(20));

        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(4, fake.Notifications.Count);
        CollectionAssert.AreEquivalent(Vertices, fake.Changed.ToArray());
    }

    [TestMethod]
    public void InvalidVertexFailsBeforeAnyWrite()
    {
        var fake = FakeTerrain.Create(Vertices, 10);
        fake.Invalid.Add(Vertices[2]);
        var adapter = new CoiTerrainAdapterV086(fake, () => true);

        var result = adapter.Snapshot(Vertices, out _);

        Assert.AreEqual(AdapterFailure.OutOfBounds, result.Failure);
        Assert.AreEqual(0, fake.SetCalls);
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    public void PartialWriteFailureRestoresEveryOriginalHeight(int failingWrite)
    {
        var fake = FakeTerrain.Create(Vertices, 10);
        var adapter = new CoiTerrainAdapterV086(fake, () => true, n =>
        {
            if (n == failingWrite) throw new InvalidOperationException("injected");
        });
        adapter.Snapshot(Vertices, out var snapshot);

        var result = adapter.ApplyWithoutPhysics(snapshot, new TerrainHeight(20));

        Assert.AreEqual(AdapterFailure.TerrainWriteFailed, result.Failure);
        Assert.IsTrue(Vertices.All(x => fake.Heights[x] == 10));
    }

    private sealed class FakeTerrain : ITerrainAccessV086
    {
        public Dictionary<GridPoint, long> Heights { get; } = new();
        public HashSet<GridPoint> Invalid { get; } = new();
        public List<GridPoint> Notifications { get; } = new();
        public HashSet<GridPoint> Changed { get; } = new();
        public int SetCalls { get; private set; }

        public static FakeTerrain Create(IEnumerable<GridPoint> points, long height)
        {
            var result = new FakeTerrain();
            foreach (var point in points) result.Heights.Add(point, height);
            return result;
        }

        public bool IsValid(GridPoint point) => Heights.ContainsKey(point) && !Invalid.Contains(point);
        public long GetHeight(GridPoint point) => Heights[point];
        public void SetHeightWithoutPhysics(GridPoint point, long rawHeight)
        {
            SetCalls++;
            Heights[point] = rawHeight;
        }
        public void NotifyChanged(GridPoint point)
        {
            Notifications.Add(point);
            Changed.Add(point);
        }
    }
}
