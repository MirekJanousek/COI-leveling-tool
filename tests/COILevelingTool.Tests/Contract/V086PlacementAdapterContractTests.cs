using System.Collections.Generic;
using COILevelingTool.Compatibility;
using COILevelingTool.Compatibility.V086;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace COILevelingTool.Tests.Contract;

[TestClass]
public sealed class V086PlacementAdapterContractTests
{
    [TestMethod]
    public void PreviewAndCommitRevalidateWholeLineAtSharedElevation()
    {
        var fake = new FakePlacement();
        var adapter = new CoiPlacementAdapterV086(fake, () => true);
        var request = Request(3);

        Assert.IsTrue(adapter.Preview(request).Succeeded);
        var result = adapter.Commit(request);

        Assert.IsTrue(result.Result.Succeeded);
        Assert.AreEqual(3, result.CreatedEntities.Count);
        Assert.AreEqual(6, fake.TargetValidations);
        Assert.AreEqual(2, fake.BatchValidations);
    }

    [TestMethod]
    public void InvalidTargetCreatesNothing()
    {
        var fake = new FakePlacement { InvalidX = 1 };
        var result = new CoiPlacementAdapterV086(fake, () => true).Commit(Request(3));

        Assert.IsFalse(result.Result.Succeeded);
        Assert.AreEqual(0, fake.Created);
    }

    [TestMethod]
    public void MidBatchFailureRemovesEveryCreatedEntity()
    {
        var fake = new FakePlacement { FailCreationAt = 2 };
        var result = new CoiPlacementAdapterV086(fake, () => true).Commit(Request(3));

        Assert.AreEqual(AdapterFailure.PlacementFailed, result.Result.Failure);
        Assert.AreEqual(0, result.CreatedEntities.Count);
        Assert.AreEqual(2, fake.Removed);
    }

    [TestMethod]
    public void SelfCollisionIsRejectedBeforeCreation()
    {
        var targets = new[] { new GridPoint(0, 0), new GridPoint(1, 0), new GridPoint(0, 0) };
        var fake = new FakePlacement();
        var result = new CoiPlacementAdapterV086(fake, () => true)
            .Commit(new PlacementRequest(targets, new TerrainHeight(100)));

        Assert.AreEqual(AdapterFailure.InvalidTarget, result.Result.Failure);
        Assert.AreEqual(0, fake.Created);
    }

    [TestMethod]
    public void NonLinearSelectionIsRejectedAsAWhole()
    {
        var targets = new[] { new GridPoint(0, 0), new GridPoint(1, 0), new GridPoint(1, 1) };
        var fake = new FakePlacement { RejectNonLinear = true };
        var result = new CoiPlacementAdapterV086(fake, () => true)
            .Commit(new PlacementRequest(targets, new TerrainHeight(100)));

        Assert.AreEqual(AdapterFailure.InvalidTarget, result.Result.Failure);
        Assert.AreEqual(0, fake.Created);
    }

    private static PlacementRequest Request(int count)
    {
        var points = new List<GridPoint>();
        for (var i = 0; i < count; i++) points.Add(new GridPoint(i, 0));
        return new PlacementRequest(points, new TerrainHeight(100));
    }

    private sealed class FakePlacement : IPlacementAccessV086
    {
        public int? InvalidX { get; set; }
        public int? FailCreationAt { get; set; }
        public bool RejectNonLinear { get; set; }
        public int TargetValidations { get; private set; }
        public int BatchValidations { get; private set; }
        public int Created { get; private set; }
        public int Removed { get; private set; }
        public AdapterResult Validate(GridPoint target, TerrainHeight elevation)
        {
            TargetValidations++;
            return target.X == InvalidX ? AdapterResult.Fail(AdapterFailure.InvalidTarget, "injected") : AdapterResult.Success();
        }
        public AdapterResult ValidateBatch(IReadOnlyList<GridPoint> targets, TerrainHeight elevation)
        {
            BatchValidations++;
            if (RejectNonLinear)
            {
                var sameX = true;
                var sameY = true;
                for (var i = 1; i < targets.Count; i++)
                {
                    sameX &= targets[i].X == targets[0].X;
                    sameY &= targets[i].Y == targets[0].Y;
                }
                if (!sameX && !sameY) return AdapterResult.Fail(AdapterFailure.InvalidTarget, "Selection is not straight.");
            }
            return AdapterResult.Success();
        }
        public AdapterResult Create(GridPoint target, TerrainHeight elevation, out object? entity)
        {
            entity = new object();
            if (Created == FailCreationAt) return AdapterResult.Fail(AdapterFailure.PlacementFailed, "injected");
            Created++;
            return AdapterResult.Success();
        }
        public AdapterResult RemoveCreated(object entity) { Removed++; return AdapterResult.Success(); }
    }
}
