namespace Lab.T03;

[Union]
internal abstract partial class Jurisdiction {
    [ValueObject<string>(KeyMemberName = "IsoCode")]
    [KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
    [KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
    internal sealed partial class Country : Jurisdiction;

    [ComplexValueObject(SkipFactoryMethods = true)]
    internal sealed partial class Unknown : Jurisdiction {
        public static readonly Unknown Instance = new();
    }

    [SmartEnum<string>]
    internal sealed partial class Continent : Jurisdiction {
        public static readonly Continent Europe = new("Europe");
        public static readonly Continent Asia = new("Asia");
    }
}
