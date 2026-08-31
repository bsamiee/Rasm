namespace Lab.F13;

internal readonly record struct Point(double X, double Y);

internal sealed record Circle(Point Center, double Radius);

internal static class Shapes {
    public static Circle Scale(Circle circle, double factor) =>
        new(circle.Center, circle.Radius * factor);
}
