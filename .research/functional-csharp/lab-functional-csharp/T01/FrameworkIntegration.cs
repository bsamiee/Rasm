namespace Lab.T01;

internal abstract record Shape;
internal sealed record Circle(double Radius) : Shape;
internal sealed record Square(double Side) : Shape;

[SmartEnum<string>]
internal sealed partial class ShapeKind {
    public static readonly ShapeKind Circle = new("Circle", Read<Circle>);
    public static readonly ShapeKind Square = new("Square", Read<Square>);

    [UseDelegateFromConstructor]
    public partial Shape? Read(ref System.Text.Json.Utf8JsonReader reader, System.Text.Json.JsonSerializerOptions options);

    private static Shape? Read<T>(ref System.Text.Json.Utf8JsonReader reader, System.Text.Json.JsonSerializerOptions options) where T : Shape =>
        System.Text.Json.JsonSerializer.Deserialize<T>(ref reader, options);
}
