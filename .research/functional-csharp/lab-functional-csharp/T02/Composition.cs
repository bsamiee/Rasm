namespace Lab.T02;

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
internal sealed partial class PostalCode {
    public int Length => _value.Length;
}

[SmartEnum<string>]
internal sealed partial class CountryCode {
    public static readonly CountryCode DE = new("DE", 5);
    public static readonly CountryCode CH = new("CH", 4);
    public int PostalCodeLength { get; }
}

[ComplexValueObject]
internal sealed partial class Address {
    public PostalCode PostalCode { get; }
    public CountryCode Country { get; }
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref PostalCode postalCode, ref CountryCode country) {
        if (postalCode.Length != country.PostalCodeLength)
            validationError = new ValidationError(string.Create(CultureInfo.InvariantCulture, $"Postal code length for country {country} must be {country.PostalCodeLength}."));
    }
}
