namespace COILevelingTool.Diagnostics;

public enum LevelingFailureCode
{
    UnsupportedGameVersion, UnsupportedCoreVersion, MissingCapability, InvalidTarget,
    OutOfBounds, Occupied, Reserved, WallOverlap, MissingCompletedWall, InvalidElevation,
    NonLinearSelection, SelfCollision, TerrainWriteFailed, TerrainVerificationFailed,
    TerrainRollbackFailed, EntityCleanupFailed, DuplicateOperation
}
