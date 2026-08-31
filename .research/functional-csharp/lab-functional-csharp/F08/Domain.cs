namespace Lab.F08;

internal sealed record InvalidAge() : Expected("age out of range", 1001), IValidationError<InvalidAge> {
    public static InvalidAge Create(string message) => new();
}

[ValueObject<int>]
[ValidationError<InvalidAge>]
internal readonly partial struct Age {
    public static Fin<Age> From(int value) => Validate(value, provider: null, out Age item) is { } error ? error : item;

    static partial void ValidateFactoryArguments(ref InvalidAge? validationError, ref int value) {
        if (value is < 0 or >= 120)
            validationError = new InvalidAge();
    }
}

internal sealed record Risk(string Level);

internal sealed record Subject(string Name, Option<Age> Age);

internal sealed record Pet(string Name);

internal sealed record Neighbor(string Name, Seq<Pet> Pets);
