namespace Lab.F21;

[ValueObject<int>]
[ValidationError<InvalidAge>]
internal readonly partial struct Age {
    public static Fin<Age> From(int value) => Validate(value, provider: null, out Age item) is { } error ? error : item;

    static partial void ValidateFactoryArguments(ref InvalidAge? validationError, ref int value) {
        if (value is < 0 or >= 120)
            validationError = new InvalidAge();
    }
}

internal static class Lifts {
    public static Fin<int> FromValue(int value) => value;

    public static Fin<int> FromError(Error error) => error;

    public static Fin<int> Halve(int value) => value <= 100 ? Pure(value / 2) : Fail<Error>(new TooLarge());
}
