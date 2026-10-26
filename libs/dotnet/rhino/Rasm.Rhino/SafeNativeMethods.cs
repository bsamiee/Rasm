using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Rhino.DocObjects;
using Rhino.Runtime;

namespace Rasm.Rhino;

// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Sequential)]
internal readonly record struct MethodDescription(nint Name, nint Types);

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct DisplayMaterialRef(Guid ViewportId, Guid DisplayMaterialId);

// --- [SERVICES] ------------------------------------------------------------------------
internal static partial class SafeNativeMethods {
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
    internal static partial MethodDescription protocol_getMethodDescription(nint protocol, nint selector, [MarshalAs(UnmanagedType.I1)] bool required, [MarshalAs(UnmanagedType.I1)] bool instance);

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

    // --- [OPENNURBS]
    private static readonly nint OpenNurbs = OperatingSystem.IsWindows() ? NativeLibrary.Load("opennurbs") : NativeLibrary.GetMainProgramHandle();

    private static readonly unsafe delegate* unmanaged<nint, DisplayMaterialRef*, DisplayMaterialRef*, byte> FindDisplayMaterialRefExport =
        (delegate* unmanaged<nint, DisplayMaterialRef*, DisplayMaterialRef*, byte>)Export(
            "?FindDisplayMaterialRef@ON_3dmObjectAttributes@@QEBA_NAEBVON_DisplayMaterialRef@@PEAV2@@Z",
            "_ZNK22ON_3dmObjectAttributes22FindDisplayMaterialRefERK21ON_DisplayMaterialRefPS0_");

    private static readonly unsafe delegate* unmanaged<nint, DisplayMaterialRef, byte> AddDisplayMaterialRefExport =
        (delegate* unmanaged<nint, DisplayMaterialRef, byte>)Export(
            "?AddDisplayMaterialRef@ON_3dmObjectAttributes@@QEAA_NVON_DisplayMaterialRef@@@Z",
            "_ZN22ON_3dmObjectAttributes21AddDisplayMaterialRefE21ON_DisplayMaterialRef");

    private static readonly unsafe delegate* unmanaged<nint, Guid, Guid, byte> RemoveDisplayMaterialRefExport =
        (delegate* unmanaged<nint, Guid, Guid, byte>)Export(
            "?RemoveDisplayMaterialRef@ON_3dmObjectAttributes@@QEAA_NUON_UUID_struct@@0@Z",
            "_ZN22ON_3dmObjectAttributes24RemoveDisplayMaterialRefE14ON_UUID_structS0_");

    private static nint Export(string microsoft, string itanium) => NativeLibrary.GetExport(OpenNurbs, OperatingSystem.IsWindows() ? microsoft : itanium);

    internal static unsafe Option<DisplayMaterialRef> FindDisplayMaterialRef(ObjectAttributes attributes, DisplayMaterialRef search) {
        DisplayMaterialRef found = default;
        bool held = FindDisplayMaterialRefExport(ConstPointer(attributes), &search, &found) is not 0;
        GC.KeepAlive(attributes);
        return Callbacks.Found(held, found);
    }

    internal static unsafe bool AddDisplayMaterialRef(ObjectAttributes attributes, DisplayMaterialRef displayMaterial) {
        bool added = AddDisplayMaterialRefExport(NonConstPointer(attributes), displayMaterial) is not 0;
        GC.KeepAlive(attributes);
        return added;
    }

    internal static unsafe bool RemoveDisplayMaterialRef(ObjectAttributes attributes, Guid viewportId, Guid displayMaterialId) {
        bool removed = RemoveDisplayMaterialRefExport(NonConstPointer(attributes), viewportId, displayMaterialId) is not 0;
        GC.KeepAlive(attributes);
        return removed;
    }

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "ConstPointer")]
    private static extern nint ConstPointer(CommonObject target);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "NonConstPointer")]
    private static extern nint NonConstPointer(ObjectAttributes target);
}
