using System.Drawing;
using System.Runtime.CompilerServices;
using Rhino.DocObjects;

namespace Rasm.Rhino.Annotation.Styles;

// --- [SERVICES] ------------------------------------------------------------------------
internal static class DimensionStyleFields {
    // --- [GETTERS]
    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    internal static extern double GetDouble(DimensionStyle style, DimensionStyle.Field field);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    internal static extern int GetInt(DimensionStyle style, DimensionStyle.Field field);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    internal static extern bool GetBool(DimensionStyle style, DimensionStyle.Field field);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    internal static extern string GetString(DimensionStyle style, DimensionStyle.Field field);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    internal static extern Color GetColor(DimensionStyle style, DimensionStyle.Field field);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    internal static extern Guid GetGuid(DimensionStyle style, DimensionStyle.Field field);
}

internal static class LinetypePreviews {
    // --- [EXTENT]
    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "LinetypeMaxShapeExtent")]
    internal static extern double MaxExtent([UnsafeAccessorType("Rhino.UI.DrawingUtilities, RhinoCommon")] object? owner, Linetype linetype);
}
