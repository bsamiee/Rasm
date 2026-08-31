namespace Lab.T07;

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
internal sealed partial class ProductName {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        string? trimmed = value.TrimOrNullify(maxLength: 50);
        if (trimmed is null) {
            validationError = new ValidationError("Product name must not be empty.");
            return;
        }
        value = trimmed;
    }
}

internal static class Trimming {
    public static string? Shortened(string? text) => text.TrimOrNullify(maxLength: 8);
    public static string Stored(string raw) => ProductName.Create(raw);
}
