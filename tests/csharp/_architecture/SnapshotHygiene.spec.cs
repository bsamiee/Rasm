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
// The walk is whole-tree by design (VerifyChecks exposes no scope) so the fact is Explicit:
// the default lane stays fast, and the hygiene lane runs it via `-- --explicit only`.
// DanglingSnapshots fails a build-server run on *.verified.* files no executed test tracked;
// off CI the check is inert by design.
public sealed class SnapshotHygieneLaws {
    [Fact(Explicit = true)]
    public Task VerifyConventionsHoldSolutionWide() => VerifyChecks.Run();

#pragma warning disable VerifyDanglingSnapshots // experimental surface, deliberately armed as the CI dangling-snapshot gate
    [Fact]
    public void DanglingSnapshotsFailTheBuildServer() => DanglingSnapshots.Run();
#pragma warning restore VerifyDanglingSnapshots
}
