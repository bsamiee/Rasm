namespace Lab.T06;

[SmartEnum<string>]
internal sealed partial class ShippingMethod {
    public static readonly ShippingMethod Standard = new("standard");
    public static readonly ShippingMethod Express = new("express");
}

[Union<int, string>]
internal readonly partial struct IdOrName;
