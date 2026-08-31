namespace Lab.F12;

internal sealed record InvalidNumberType() : Expected("number type is not mobile or home", 1201), IValidationError<InvalidNumberType> {
    public static InvalidNumberType Create(string message) => new();
}

internal sealed record InvalidCountryCode() : Expected("country code is not two upper-case letters", 1202), IValidationError<InvalidCountryCode> {
    public static InvalidCountryCode Create(string message) => new();
}

internal sealed record InvalidNumber() : Expected("number is not six to twelve digits", 1203), IValidationError<InvalidNumber> {
    public static InvalidNumber Create(string message) => new();
}

[SmartEnum<string>]
[ValidationError<InvalidNumberType>]
internal sealed partial class NumberType {
    public static readonly NumberType Mobile = new("mobile");
    public static readonly NumberType Home = new("home");
}

[ValueObject<string>]
[ValidationError<InvalidCountryCode>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
internal readonly partial struct CountryCode {
    static partial void ValidateFactoryArguments(ref InvalidCountryCode? validationError, ref string value) {
        if (value is not [char first, char second] || !char.IsAsciiLetterUpper(first) || !char.IsAsciiLetterUpper(second))
            validationError = new InvalidCountryCode();
    }
}

[ValueObject<string>]
[ValidationError<InvalidNumber>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
internal readonly partial struct Number {
    static partial void ValidateFactoryArguments(ref InvalidNumber? validationError, ref string value) {
        if (value.Length is < 6 or > 12 || !value.All(char.IsAsciiDigit))
            validationError = new InvalidNumber();
    }
}

internal sealed record PhoneNumber(NumberType Type, CountryCode Country, Number Number);

internal static class PhoneNumbers {
    public static Validation<Error, NumberType> ValidNumberType(string type) =>
        NumberType.Validate(type, provider: null, out NumberType? item) is { } error ? error : item!;
    public static Validation<Error, CountryCode> ValidCountryCode(string country) =>
        CountryCode.Validate(country, provider: null, out CountryCode item) is { } error ? error : item;
    public static Validation<Error, Number> ValidNumber(string number) =>
        Number.Validate(number, provider: null, out Number item) is { } error ? error : item;
    public static Validation<Error, PhoneNumber> CreatePhoneNumber(string type, string country, string number) =>
        (ValidNumberType(type), ValidCountryCode(country), ValidNumber(number))
            .Apply(static (t, c, n) => new PhoneNumber(t, c, n))
            .As();
}

internal static class PhoneNumbersProbe {
    private static Validation<Error, PhoneNumber> CreatePhoneNumber(string type, string country, string number) =>
        PhoneNumbers.CreatePhoneNumber(type, country, number);

    public static Fin<Unit> Run() {
        Validation<Error, PhoneNumber> valid = CreatePhoneNumber("mobile", "GB", "7700900123");

        Validation<Error, PhoneNumber> invalid = CreatePhoneNumber("fax", "gb", "abc");
        int errorCount = invalid.Match(Fail: static e => e.Count, Succ: static _ => 0);
        // 3
        bool typed = invalid.Match(Fail: static e => e.Head.IsType<InvalidNumberType>(), Succ: static _ => false);
        return Samples.Check(
            nameof(PhoneNumbers),
            ("valid.IsSuccess", valid.IsSuccess),
            ("errorCount == 3", errorCount == 3),
            ("typed", typed),
            ("upper-case key accepted", CreatePhoneNumber("MOBILE", "GB", "7700900123").IsSuccess),
            ("lower-case country rejected", PhoneNumbers.ValidCountryCode("gb").Match(Fail: static e => e.IsType<InvalidCountryCode>(), Succ: static _ => false)));
    }
}
