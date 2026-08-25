using System.Runtime.CompilerServices;

namespace Rasm.Architecture.Tests;

// --- [COMPOSITION] ---------------------------------------------------------------------
public static class VerifyModuleInitializer {
    [ModuleInitializer]
    public static void Initialize() => VerifyDiffPlex.Initialize();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed class SnapshotHygieneLaws {
    [Fact(Explicit = true)]
    public Task VerifyConventionsHoldSolutionWide() => VerifyChecks.Run();

#pragma warning disable VerifyDanglingSnapshots
    [Fact]
    public void DanglingSnapshotsFailTheBuildServer() => DanglingSnapshots.Run();
#pragma warning restore VerifyDanglingSnapshots
}
