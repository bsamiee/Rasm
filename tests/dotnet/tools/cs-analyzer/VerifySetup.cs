using System.Runtime.CompilerServices;

namespace Rasm.Csp.Tests;

// --- [COMPOSITION] ---------------------------------------------------------------------

internal static class VerifySetup {
    [ModuleInitializer]
    internal static void Initialize() => VerifyDiffPlex.Initialize();
}
