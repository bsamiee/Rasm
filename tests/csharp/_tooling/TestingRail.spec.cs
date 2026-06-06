namespace Rasm.TestingTools.Tests;

// --- [OPERATIONS] --------------------------------------------------------------------------
public sealed class TestingRailSnapshotLaws {
    [Fact]
    public Task TestingRailManifestIsStableAsync() =>
        Verify(new {
            Unit = "xUnit v3/MTP + CsCheck",
            Coverage = "coverlet.MTP managed opt-in",
            Mutation = "Stryker MTP explicit changed/full modes",
            Snapshot = "Verify stable artifacts only",
            Architecture = "ArchUnitNET boundary laws",
            Benchmark = "BenchmarkDotNet tests/csharp executable measurement rail",
            Fuzz = "SharpFuzz tests/csharp executable harness rail",
        });
}
