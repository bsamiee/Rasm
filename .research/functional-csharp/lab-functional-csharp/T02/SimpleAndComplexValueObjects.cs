namespace Lab.T02;

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
internal sealed partial class ProductName {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        if (string.IsNullOrWhiteSpace(value)) {
            validationError = new ValidationError("Product name cannot be empty.");
            return;
        }

        value = value.Trim();

        if (value.Length < 3)
            validationError = new ValidationError("Product name must be at least three characters long.");
    }
}

[ComplexValueObject]
internal sealed partial class Boundary {
    public decimal Lower { get; }
    public decimal Upper { get; }
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref decimal lower, ref decimal upper) {
        if (lower > upper) {
            validationError = new ValidationError(string.Create(CultureInfo.InvariantCulture, $"Lower boundary '{lower}' must be less than or equal to upper boundary '{upper}'."));
            return;
        }

        lower = Math.Round(lower, 2, MidpointRounding.ToEven);
        upper = Math.Round(upper, 2, MidpointRounding.ToEven);
    }
}
