using System.Runtime.CompilerServices;

namespace Rasm.Csp.Tests;

// --- [COMPOSITION] ---------------------------------------------------------------------

// Assembly-wide Verify wiring: DiffPlex renders inline diffs on snapshot mismatches.
internal static class VerifySetup {
    [ModuleInitializer]
    internal static void Initialize() => VerifyDiffPlex.Initialize();
}
