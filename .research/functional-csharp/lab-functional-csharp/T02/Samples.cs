namespace Lab.T02;

internal static class Samples {
    public static Fin<Unit> Run() =>
        DeclarationsSample()
            .Bind(static _ => HookParametersSample())
            .Bind(static _ => ValidationErrorsSample())
            .Bind(static _ => OperatorsSample())
            .Bind(static _ => CustomKeySample())
            .Bind(static _ => CompositionSample())
            .Bind(static _ => GenericKeySample())
            .Bind(static _ => FileUrnSample())
            .Bind(static _ => JsonSample());

    private static Fin<Unit> DeclarationsSample() =>
        Check(
            nameof(DeclarationsSample),
            ("trim", string.Equals(ProductName.Create(" Widget "), "Widget", StringComparison.Ordinal)),
            ("short rejected", !ProductName.TryCreate("ab", out _)),
            ("short reason", !ProductName.TryCreate("ab", out _, out ValidationError? error) && error is not null),
            ("validate", ProductName.Validate("ab", provider: null, out _) is not null),
            ("create throws", Try.lift(static () => ProductName.Create("ab")).Run().IsFail),
            ("cast", string.Equals((ProductName)"Widget", "Widget", StringComparison.Ordinal)),
            ("parse rejected", !ProductName.TryParse("ab", provider: null, out _)),
            ("equality ignores case", ProductName.Create("Widget") == ProductName.Create("WIDGET")),
            ("boundary rounds", string.Equals(Boundary.Create(1.234m, 2.567m).ToString(), "{ Lower = 1.23, Upper = 2.57 }", StringComparison.Ordinal)),
            ("boundary rejects", !Boundary.TryCreate(2m, 1m, out _)));

    private static Fin<Unit> HookParametersSample() =>
        Check(
            nameof(HookParametersSample),
            ("default rounding", Money.Create(19.999m) == 20.00m),
            ("down rounding", Money.Create(19.999m, MidpointRounding.ToNegativeInfinity) == 19.99m),
            ("up rounding", Money.Create(19.991m, MidpointRounding.ToPositiveInfinity) == 20.00m),
            ("negative rejected", Try.lift(static () => Money.Create(-1m)).Run().IsFail),
            ("times int", Money.Create(19.99m) * 2 == 39.98m),
            ("int times", 3 * Money.Create(19.99m) == 59.97m),
            ("zero", Money.Zero == default));

    private static Fin<Unit> ValidationErrorsSample() =>
        Check(
            nameof(ValidationErrorsSample),
            ("try create fails", !Interval.TryCreate(2, 1, out _, out BoundaryValidationError? error)),
            ("error text", string.Equals(error?.ToString(), "Lower boundary must be less than upper boundary. (Lower=2|Upper=1)", StringComparison.Ordinal)),
            ("create throws", Try.lift(static () => Interval.Create(2, 1)).Run().IsFail),
            ("create succeeds", Interval.Create(1, 2).Upper == 2));

    private static Fin<Unit> OperatorsSample() {
        Amount amount = Amount.Create(1m);
        return Check(
            nameof(OperatorsSample),
            ("key overload right", amount + 42m == 43m),
            ("key overload left", 42m + amount == 43m),
            ("comparison overload", !(amount > 42m)),
            ("equality overload", amount == 1m),
            ("invariant after subtraction", Try.lift(static () => Amount.Create(1m) - 5m).Run().IsFail),
            ("zero is default", Amount.Zero == default),
            ("parse", Amount.Parse("25.5", CultureInfo.InvariantCulture) == 25.5m),
            ("parse rejects", Try.lift(static () => Amount.Parse("-1", CultureInfo.InvariantCulture)).Run().IsFail),
            ("format", string.Equals(Amount.Create(42.1m).ToString("000.00", CultureInfo.InvariantCulture), "042.10", StringComparison.Ordinal)));
    }

    private static Fin<Unit> CustomKeySample() =>
        Check(
            nameof(CustomKeySample),
            ("default is infinite", OpenEndDate.Infinite == default),
            ("infinite is max", OpenEndDate.Infinite == DateOnly.MaxValue),
            ("ordering", OpenEndDate.Create(new DateOnly(2023, 12, 31)) < OpenEndDate.Infinite),
            ("to string", string.Equals(OpenEndDate.Infinite.ToString(), "Infinite", StringComparison.Ordinal)),
            ("date to string", string.Equals(OpenEndDate.Create(new DateOnly(2023, 12, 31)).ToString(), "2023-12-31", StringComparison.Ordinal)));

    private static Fin<Unit> CompositionSample() =>
        Check(
            nameof(CompositionSample),
            ("length mismatch", !Address.TryCreate(PostalCode.Create("1234"), CountryCode.DE, out _)),
            ("length match", Address.TryCreate(PostalCode.Create("12345"), CountryCode.DE, out _)),
            ("swiss", Address.TryCreate(PostalCode.Create("8000"), CountryCode.CH, out _)));

    private static Fin<Unit> GenericKeySample() =>
        Check(
            nameof(GenericKeySample),
            ("addition", Measure<decimal>.Create(1.5m) + Measure<decimal>.Create(2m) == Measure<decimal>.Create(3.5m)),
            ("parse", Measure<int>.Parse("42", CultureInfo.InvariantCulture) == Measure<int>.Create(42)),
            ("comparison", Measure<int>.Create(1) < Measure<int>.Create(2)));

    private static Fin<Unit> FileUrnSample() {
        FileUrn document = FileUrn.Create("blob", "a/b.pdf");
        return Check(
            nameof(FileUrnSample),
            ("to value", string.Equals(document.ToValue(), "blob:a/b.pdf", StringComparison.Ordinal)),
            ("json string", string.Equals(System.Text.Json.JsonSerializer.Serialize(document), "\"blob:a/b.pdf\"", StringComparison.Ordinal)),
            ("json read", System.Text.Json.JsonSerializer.Deserialize<FileUrn>("\"blob:a/b.pdf\"") == document),
            ("parse", FileUrn.Parse("blob:a/b.pdf", provider: null) == document),
            ("parse rejects", Try.lift(static () => FileUrn.Parse("nocolon", provider: null)).Run().IsFail),
            ("empty store", !FileUrn.TryParse(":x", provider: null, out _)),
            ("empty input", !FileUrn.TryParse("", provider: null, out _)),
            ("constructor trusts the value", FileUrn.Parse("blob:x:y", provider: null).Urn.Length == 3));
    }

    private static Fin<Unit> JsonSample() =>
        Check(
            nameof(JsonSample),
            ("amount", string.Equals(System.Text.Json.JsonSerializer.Serialize(Amount.Create(10.5m)), "10.5", StringComparison.Ordinal)),
            ("boundary", string.Equals(System.Text.Json.JsonSerializer.Serialize(Boundary.Create(1m, 2m)), "{\"Lower\":1,\"Upper\":2}", StringComparison.Ordinal)),
            ("boundary read", System.Text.Json.JsonSerializer.Deserialize<Boundary>("{\"Lower\":1,\"Upper\":2}") == Boundary.Create(1m, 2m)),
            ("invalid read", Try.lift(static () => System.Text.Json.JsonSerializer.Deserialize<Boundary>("{\"Lower\":10,\"Upper\":1}")).Run().IsFail),
            ("invalid amount", Try.lift(static () => System.Text.Json.JsonSerializer.Deserialize<Amount>("-1")).Run().IsFail));

    private static Fin<Unit> Check(string sample, params (string Name, bool Ok)[] checks) {
        Seq<string> failed = toSeq(checks).Choose(static check => check.Ok ? Option<string>.None : Some(check.Name));
        return guard(failed.IsEmpty, Error.New($"{sample}: {string.Join(" | ", failed)}")).ToFin();
    }
}
