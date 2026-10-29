using System.Runtime.CompilerServices;
using Rhino;

namespace Rasm.Rhino.Annotation;

// --- [SERVICES] ------------------------------------------------------------------------
internal static class DimensionLayers {
    // --- [NATIVES]
    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "RHC_RhUseDimensionLayer")]
    private static extern bool Used([UnsafeAccessorType("UnsafeNativeMethods, RhinoCommon")] object? natives, uint document);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "RHC_RhGetDimensionLayerIndex")]
    private static extern int Index([UnsafeAccessorType("UnsafeNativeMethods, RhinoCommon")] object? natives, uint document);

    // --- [LAYER]
    internal static Option<int> Of(RhinoDoc doc) =>
        Used(natives: null, doc.RuntimeSerialNumber) ? Conversions.Present(Index(natives: null, doc.RuntimeSerialNumber)) : None;
}
