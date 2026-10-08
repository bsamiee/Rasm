using Rasm.Rhino.Document;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral")]
[ValidationError<ValidationFailure>]
public readonly partial struct Exposure {
    public const float Lower = -32f;

    public const float Upper = 32f;

    public const double Step = 0.01;

    public const int Precision = 3;

    static partial void ValidateFactoryArguments(ref ValidationFailure? validationError, ref float value) =>
        validationError = Limits.AtLeast(Lower).AtMost(Upper).Violated(value, nameof(Exposure));
}
