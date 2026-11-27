namespace Rasm.Geometry;

// --- [ERRORS] --------------------------------------------------------------------------

[SmartEnum]
public sealed partial class GeometryRule : IFaultReason {
    public static readonly GeometryRule Count = new("wrong element count");
    public static readonly GeometryRule Order = new("out of order");
    public static readonly GeometryRule Shape = new("wrong shape");
    public static readonly GeometryRule Range = new("out of range");
    public static readonly GeometryRule Degenerate = new("degenerate extent");
    public static readonly GeometryRule Singular = new("singular map");
    public static readonly GeometryRule Distortion = new("map distorts distance unevenly");

    public string Message { get; }
}
