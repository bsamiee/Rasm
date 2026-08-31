namespace Lab.T01;

[SmartEnum<TypeParamRef1>]
internal sealed partial class Metric<T> where T : System.Numerics.INumber<T> {
    public static readonly Metric<T> Temperature = new(T.Zero);
    public static readonly Metric<T> Humidity = new(T.One);
}
