using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Vectors;

// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SupportSpace {
    private SupportSpace(object value) => Value = value;
    internal object Value { get; }
    public static Fin<SupportSpace> Of(object? value, Op? key = null) {
        Op op = key ?? Op.Of(name: nameof(SupportSpace));
        return from source in Optional(value).ToFin(op.InvalidInput())
               let type = source.GetType()
               from _ in guard(type != typeof(object) && type != typeof(GeometryBase) && GeometryKernel.CanClosest(type: type), op.Unsupported(type, typeof(ClosestHit)))
               from __ in guard(OpAcceptance.ValidityOf(source: source).IfNone(false), op.InvalidInput())
               select new SupportSpace(value: source);
    }
    internal Fin<ClosestHit> Closest(Point3d sample, Op key) =>
        GeometryKernel.ClosestOf(geometry: Value, target: sample, key: key);
}
