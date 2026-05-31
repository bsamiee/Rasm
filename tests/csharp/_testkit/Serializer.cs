using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Rhino.Geometry;
using Xunit.Sdk;

namespace Rasm.TestKit;

// --- [SERVICES] -----------------------------------------------------------------------------
// Singular testkit serializer surface. Adding a type here also requires registering it via Directory.Build.props.
public sealed class GeometrySerializer : IXunitSerializer {
    public bool IsSerializable(Type type, object? value, [NotNullWhen(false)] out string? failureReason) {
        ArgumentNullException.ThrowIfNull(argument: type);
        bool supported = type == typeof(Point3d) || type == typeof(Vector3d) || type == typeof(BoundingBox);
        failureReason = supported ? null : $"GeometrySerializer does not support type '{type.FullName}'";
        return supported;
    }
    public string Serialize(object value) {
        ArgumentNullException.ThrowIfNull(argument: value);
        return value switch {
            Point3d p => Join(p.X, p.Y, p.Z),
            Vector3d v => Join(v.X, v.Y, v.Z),
            BoundingBox b => Join(b.Min.X, b.Min.Y, b.Min.Z, b.Max.X, b.Max.Y, b.Max.Z),
            _ => throw new ArgumentException($"GeometrySerializer cannot serialize '{value.GetType().FullName}'", nameof(value)),
        };
    }
    public object Deserialize(Type type, string serializedValue) {
        ArgumentNullException.ThrowIfNull(argument: type);
        ArgumentNullException.ThrowIfNull(argument: serializedValue);
        return (type, serializedValue.Split(separator: ';')) switch {
            (Type t, [string x, string y, string z]) when t == typeof(Point3d) =>
                new Point3d(x: Parse(component: x, serializedValue: serializedValue), y: Parse(component: y, serializedValue: serializedValue),
                    z: Parse(component: z, serializedValue: serializedValue)),
            (Type t, [string x, string y, string z]) when t == typeof(Vector3d) =>
                new Vector3d(x: Parse(component: x, serializedValue: serializedValue), y: Parse(component: y, serializedValue: serializedValue),
                    z: Parse(component: z, serializedValue: serializedValue)),
            (Type t, [string mx, string my, string mz, string xx, string xy, string xz]) when t == typeof(BoundingBox) =>
                new BoundingBox(
                    min: new Point3d(x: Parse(component: mx, serializedValue: serializedValue), y: Parse(component: my, serializedValue: serializedValue),
                        z: Parse(component: mz, serializedValue: serializedValue)),
                    max: new Point3d(x: Parse(component: xx, serializedValue: serializedValue), y: Parse(component: xy, serializedValue: serializedValue),
                        z: Parse(component: xz, serializedValue: serializedValue))),
            _ => throw new FormatException($"GeometrySerializer cannot deserialize '{serializedValue}' as '{type.FullName}'"),
        };
    }
    private static string Join(params double[] components) =>
        string.Join(separator: ';', values: components.Select(static d => d.ToString(format: "R", provider: CultureInfo.InvariantCulture)));
    private static double Parse(string component, string serializedValue) =>
        double.TryParse(s: component, style: NumberStyles.Float, provider: CultureInfo.InvariantCulture, result: out double value)
            ? value
            : throw new FormatException($"GeometrySerializer cannot parse component '{component}' in '{serializedValue}'");
}
