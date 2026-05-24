namespace Rasm.TestingTools.Tests;

// --- [ALGEBRAIC] --------------------------------------------------------------------------
public sealed class TestingRailSnapshotLaws {
    [Fact]
    public Task TestingRailManifestIsStableAsync() =>
        Verify(new {
            Unit = "xUnit v3/VSTest + CsCheck",
            Coverage = "coverlet managed opt-in",
            Mutation = "Stryker diagnostic until non-zero discovery",
            Snapshot = "Verify stable artifacts only",
            Architecture = "ArchUnitNET boundary laws",
            Benchmark = "BenchmarkDotNet tests/csharp executable measurement rail",
            Fuzz = "SharpFuzz tests/csharp executable harness rail",
        });
}
