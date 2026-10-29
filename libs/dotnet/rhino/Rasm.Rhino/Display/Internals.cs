using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Runtime;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Sequential)]
internal readonly record struct DisplayMaterialRef(Guid ViewportId, Guid DisplayMaterialId);

// --- [SERVICES] ------------------------------------------------------------------------
internal static class DisplayMaterialRefs {
    // --- [EXPORTS]
    private static readonly nint OpenNurbs = OperatingSystem.IsWindows() ? NativeLibrary.Load("opennurbs") : NativeLibrary.GetMainProgramHandle();

    private static readonly unsafe delegate* unmanaged<nint, DisplayMaterialRef*, DisplayMaterialRef*, byte> FindExport =
        (delegate* unmanaged<nint, DisplayMaterialRef*, DisplayMaterialRef*, byte>)Export(
            "?FindDisplayMaterialRef@ON_3dmObjectAttributes@@QEBA_NAEBVON_DisplayMaterialRef@@PEAV2@@Z",
            "_ZNK22ON_3dmObjectAttributes22FindDisplayMaterialRefERK21ON_DisplayMaterialRefPS0_");

    private static readonly unsafe delegate* unmanaged<nint, DisplayMaterialRef, byte> AddExport =
        (delegate* unmanaged<nint, DisplayMaterialRef, byte>)Export(
            "?AddDisplayMaterialRef@ON_3dmObjectAttributes@@QEAA_NVON_DisplayMaterialRef@@@Z",
            "_ZN22ON_3dmObjectAttributes21AddDisplayMaterialRefE21ON_DisplayMaterialRef");

    private static readonly unsafe delegate* unmanaged<nint, Guid, Guid, byte> RemoveExport =
        (delegate* unmanaged<nint, Guid, Guid, byte>)Export(
            "?RemoveDisplayMaterialRef@ON_3dmObjectAttributes@@QEAA_NUON_UUID_struct@@0@Z",
            "_ZN22ON_3dmObjectAttributes24RemoveDisplayMaterialRefE14ON_UUID_structS0_");

    private static nint Export(string microsoft, string itanium) => NativeLibrary.GetExport(OpenNurbs, OperatingSystem.IsWindows() ? microsoft : itanium);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "ConstPointer")]
    private static extern nint ConstPointer(CommonObject target);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "NonConstPointer")]
    private static extern nint NonConstPointer(ObjectAttributes target);

    // --- [SLOTS]
    internal static unsafe Option<Guid> DisplayMaterialRef(ObjectAttributes attributes, Guid viewport) {
        DisplayMaterialRef search = default(DisplayMaterialRef) with { ViewportId = viewport };
        DisplayMaterialRef found = search;
        bool held = FindExport(ConstPointer(attributes), &search, &found) is not 0;
        GC.KeepAlive(attributes);
        return Conversions.Present(held && found.ViewportId == viewport, found.DisplayMaterialId);
    }

    internal static unsafe void AllViewportsRemoved(ObjectAttributes staged, Guid key) {
        DisplayMaterialRef marked = default(DisplayMaterialRef) with { DisplayMaterialId = key };
        _ = AddExport(NonConstPointer(staged), marked);
        _ = RemoveExport(NonConstPointer(staged), marked.ViewportId, marked.DisplayMaterialId);
        GC.KeepAlive(staged);
    }
}

internal static class PipelineAttributes {
    // --- [COPIES]
    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    internal static extern DisplayPipelineAttributes Copy(DisplayPipelineAttributes other);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "CopyContents")]
    internal static extern void CopyContents(DisplayPipelineAttributes target, DisplayPipelineAttributes other);
}
