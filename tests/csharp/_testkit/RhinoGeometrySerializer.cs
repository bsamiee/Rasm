using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Rhino.Geometry;
using Xunit.Sdk;

namespace Rasm.TestKit;

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class RhinoGeometrySerializer : IXunitSerializer {
    public bool IsSerializable(Type type, object? value, [NotNullWhen(false)] out string? failureReason) {
        ArgumentNullException.ThrowIfNull(argument: type);
        bool supported = type == typeof(Point3d) || type == typeof(Vector3d) || type == typeof(BoundingBox);
        failureReason = supported ? null : $"RhinoGeometrySerializer does not support type '{type.FullName}'";
        return supported;
    }
    public string Serialize(object value) {
        ArgumentNullException.ThrowIfNull(argument: value);
        return value switch {
            Point3d p => Join(p.X, p.Y, p.Z),
            Vector3d v => Join(v.X, v.Y, v.Z),
            BoundingBox b => Join(b.Min.X, b.Min.Y, b.Min.Z, b.Max.X, b.Max.Y, b.Max.Z),
            _ => throw new ArgumentException($"RhinoGeometrySerializer cannot serialize '{value.GetType().FullName}'", nameof(value)),
        };
    }
    public object Deserialize(Type type, string serializedValue) {
        ArgumentNullException.ThrowIfNull(argument: type);
        ArgumentNullException.ThrowIfNull(argument: serializedValue);
        return (type, serializedValue.Split(separator: ';')) switch {
            (Type t, [string x, string y, string z]) when t == typeof(Point3d) =>
                new Point3d(x: Parse(x), y: Parse(y), z: Parse(z)),
            (Type t, [string x, string y, string z]) when t == typeof(Vector3d) =>
                new Vector3d(x: Parse(x), y: Parse(y), z: Parse(z)),
            (Type t, [string mx, string my, string mz, string xx, string xy, string xz]) when t == typeof(BoundingBox) =>
                new BoundingBox(min: new Point3d(x: Parse(mx), y: Parse(my), z: Parse(mz)), max: new Point3d(x: Parse(xx), y: Parse(xy), z: Parse(xz))),
            _ => throw new FormatException($"RhinoGeometrySerializer cannot deserialize '{serializedValue}' as '{type.FullName}'"),
        };
    }
    private static string Join(params double[] components) =>
        string.Join(separator: ';', values: components.Select(static d => d.ToString(format: "R", provider: CultureInfo.InvariantCulture)));
    private static double Parse(string component) =>
        double.Parse(s: component, provider: CultureInfo.InvariantCulture);
}
