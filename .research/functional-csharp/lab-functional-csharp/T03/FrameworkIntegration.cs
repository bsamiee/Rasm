namespace Lab.T03;

[Union<string, int>(T1Name = "Text", T2Name = "Number")]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.SystemTextJson)]
internal sealed partial class TextOrNumberSerializable {
    public string ToValue() => Switch(
        text: static text => $"Text|{text}",
        number: static number => string.Create(CultureInfo.InvariantCulture, $"Number|{number}"));

    public static ValidationError? Validate(string? value, IFormatProvider? provider, out TextOrNumberSerializable? item) {
        if (value is not null && value.StartsWith("Text|", StringComparison.Ordinal)) {
            item = value[5..];
            return null;
        }
        if (value is not null && value.StartsWith("Number|", StringComparison.Ordinal) && int.TryParse(value.AsSpan(7), NumberStyles.Integer, provider, out int number)) {
            item = number;
            return null;
        }
        item = null;
        return new ValidationError("Expected 'Text|<text>' or 'Number|<digits>'.");
    }
}
