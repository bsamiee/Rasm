namespace Lab.T02;

[ValueObject<decimal>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Zero", MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
internal readonly partial struct Money : System.Numerics.IMultiplyOperators<Money, int, Money> {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref decimal value, MidpointRounding rounding) {
        if (value < 0) {
            validationError = new ValidationError("Amount cannot be negative.");
            return;
        }

        value = decimal.Round(value, 2, rounding);
    }

    public static Money Create(decimal amount, MidpointRounding rounding) => CreateCore(amount, rounding);
    public static Money operator *(Money left, int right) => Create(left._value * right);
    public static Money operator *(int left, Money right) => Create(right._value * left);
}
