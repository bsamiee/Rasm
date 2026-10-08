using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Rasm.Rhino;

// --- [TYPES] ---------------------------------------------------------------------------
[Flags]
internal enum ExecutionState : uint {
    None = 0,
    SystemRequired = 1u << 0,
    DisplayRequired = 1u << 1,
    Continuous = 1u << 31,
}

// --- [SERVICES] ------------------------------------------------------------------------
internal static class SafeNativeMethods {
    [DllImport("kernel32")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SupportedOSPlatform("windows")]
    internal static extern ExecutionState SetThreadExecutionState(ExecutionState flags);

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    [SupportedOSPlatform("macos")]
    internal static extern int CGAssociateMouseAndMouseCursorPosition(bool connected);
}
