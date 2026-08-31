namespace Lab.T04;

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[ObjectFactory<ReadOnlySpan<char>>(UseForSerialization = SerializationFrameworks.SystemTextJson)]
internal readonly partial struct Region {
    public static ValidationError? Validate(ReadOnlySpan<char> value, IFormatProvider? provider, out Region item) =>
        value switch {
            "eu" => Validate("eu", provider, out item),
            "us" => Validate("us", provider, out item),
            _ => Validate(value.ToString(), provider, out item),
        };

    public ReadOnlySpan<char> ToValue() => _value;
}
