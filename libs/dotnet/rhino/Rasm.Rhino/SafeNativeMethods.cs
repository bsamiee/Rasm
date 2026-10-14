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
internal static partial class SafeNativeMethods {
    // --- [POWER]
    [LibraryImport("kernel32")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SupportedOSPlatform("windows")]
    internal static partial ExecutionState SetThreadExecutionState(ExecutionState flags);

    // --- [POINTER]
    [LibraryImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    [SupportedOSPlatform("macos")]
    internal static partial int CGAssociateMouseAndMouseCursorPosition([MarshalAs(UnmanagedType.Bool)] bool connected);

    // --- [OBJECTIVE_C]
    private const string ObjectiveCLibrary = "/usr/lib/libobjc.dylib";

    [LibraryImport(ObjectiveCLibrary)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    [SupportedOSPlatform("macos")]
    internal static unsafe partial nint objc_msgSend(nint receiver, nint selector, ulong mask, void* handler);

    [LibraryImport(ObjectiveCLibrary, StringMarshalling = StringMarshalling.Utf8)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    [SupportedOSPlatform("macos")]
    internal static partial nint objc_allocateClassPair(nint superclass, string name, nuint extraBytes);

    [LibraryImport(ObjectiveCLibrary)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    [SupportedOSPlatform("macos")]
    internal static partial void objc_registerClassPair(nint cls);

    [LibraryImport(ObjectiveCLibrary)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    [SupportedOSPlatform("macos")]
    internal static partial void objc_disposeClassPair(nint cls);

    [LibraryImport(ObjectiveCLibrary)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    [SupportedOSPlatform("macos")]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static partial bool class_addMethod(nint cls, nint selector, nint implementation, nint encoding);

    [LibraryImport(ObjectiveCLibrary)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    [SupportedOSPlatform("macos")]
    internal static partial nint method_getTypeEncoding(nint method);

    [LibraryImport(ObjectiveCLibrary)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    [SupportedOSPlatform("macos")]
    internal static partial nint method_getImplementation(nint method);

    [LibraryImport(ObjectiveCLibrary)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    [SupportedOSPlatform("macos")]
    internal static partial nint object_setClass(nint instance, nint cls);

    [LibraryImport(ObjectiveCLibrary)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
    [SupportedOSPlatform("macos")]
    internal static partial nint object_getClass(nint instance);
}
