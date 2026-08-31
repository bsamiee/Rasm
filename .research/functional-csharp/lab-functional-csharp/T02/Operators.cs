namespace Lab.T02;

[ValueObject<decimal>(
    AllowDefaultStructs = true,
    DefaultInstancePropertyName = "Zero",
    EqualityComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads,
    ComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads,
    AdditionOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads,
    SubtractionOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads)]
internal readonly partial struct Amount {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref decimal value) {
        if (value < 0)
            validationError = new ValidationError("Amount must be positive.");
    }
}
