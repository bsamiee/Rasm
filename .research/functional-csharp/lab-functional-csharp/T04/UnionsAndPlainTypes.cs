namespace Lab.T04;

[Union<string, int>(T1Name = "Text", T2Name = "Number")]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.All, UseForModelBinding = true)]
internal sealed partial class TextOrNumber {
    public static ValidationError? Validate(string? value, IFormatProvider? provider, out TextOrNumber? item) {
        item = null;
        if (value is null) return null;
        if (value.StartsWith("text:", StringComparison.Ordinal)) item = value["text:".Length..];
        else if (value.StartsWith("number:", StringComparison.Ordinal) && int.TryParse(value["number:".Length..], NumberStyles.Integer, CultureInfo.InvariantCulture, out int number)) item = number;
        return item is null ? new ValidationError($"Unknown text-or-number '{value}'") : null;
    }

    public string ToValue() => Switch(
        text: static text => $"text:{text}",
        number: static number => string.Create(CultureInfo.InvariantCulture, $"number:{number}"));
}
