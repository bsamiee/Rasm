namespace Lab.T02;

[ValueObject<DateOnly>(
    SkipKeyMember = true,
    KeyMemberName = nameof(Date),
    DefaultInstancePropertyName = "Infinite",
    EqualityComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads,
    ComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads,
    AllowDefaultStructs = true,
    SkipToString = true,
    SkipIFormattable = true)]
internal readonly partial struct OpenEndDate {
    private readonly DateOnly? _date;
    private DateOnly Date {
        get => _date ?? DateOnly.MaxValue;
        init => _date = value;
    }

    public override string ToString() => this == Infinite ? "Infinite" : Date.ToString("O", CultureInfo.InvariantCulture);
}
