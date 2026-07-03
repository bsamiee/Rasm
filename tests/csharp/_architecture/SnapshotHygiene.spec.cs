using System.Runtime.CompilerServices;

namespace Rasm.Architecture.Tests;

// --- [COMPOSITION] --------------------------------------------------------------------------
// One per-assembly Verify registration: DiffPlex renders string diffs on snapshot mismatches.
public static class VerifyModuleInitializer {
    [ModuleInitializer]
    public static void Initialize() => VerifyDiffPlex.Initialize();
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
// The repo-wide snapshot-hygiene gate pair. VerifyChecks resolves the solution directory and
// audits orphaned *.received.* litter, csproj-imported snapshot nestings, and the .gitignore /
// .gitattributes / .editorconfig conventions for every *.verified.* extension in the tree.
// DanglingSnapshots fails a build-server run on *.verified.* files no executed test tracked;
// off CI the check is inert by design, so the local gate is VerifyChecks alone.
public sealed class SnapshotHygieneLaws {
    [Fact]
    public Task VerifyConventionsHoldSolutionWide() => VerifyChecks.Run();

#pragma warning disable VerifyDanglingSnapshots // experimental surface, deliberately armed as the CI dangling-snapshot gate
    [Fact]
    public void DanglingSnapshotsFailTheBuildServer() => DanglingSnapshots.Run();
#pragma warning restore VerifyDanglingSnapshots
}
