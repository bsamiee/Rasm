namespace Lab.T02;

internal sealed record BoundaryValidationError(string Message, decimal? Lower, decimal? Upper) : IValidationError<BoundaryValidationError> {
    public static BoundaryValidationError Create(string message) => new(message, Lower: null, Upper: null);
    public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"{Message} (Lower={Lower}|Upper={Upper})");
}

[ComplexValueObject]
[ValidationError<BoundaryValidationError>]
internal sealed partial class Interval {
    public decimal Lower { get; }
    public decimal Upper { get; }
    static partial void ValidateFactoryArguments(ref BoundaryValidationError? validationError, ref decimal lower, ref decimal upper) {
        if (lower > upper)
            validationError = new BoundaryValidationError("Lower boundary must be less than upper boundary.", lower, upper);
    }
}
