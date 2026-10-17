using System.Globalization;

namespace Thinktecture;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class ValidationResults {
    public static ValidationError? OutsideRange<TKey>(TKey value, TKey low, TKey high) where TKey : IComparable<TKey>, IFormattable =>
        value.CompareTo(low) >= 0 && value.CompareTo(high) <= 0 ? null : new ValidationError(string.Create(CultureInfo.InvariantCulture, $"{value} lies outside [{low}, {high}]"));

    public static Fin<TValue> ToFin<TValue>(ValidationError? error, TValue? value) =>
        error is null ? value! : Error.New(ErrorCodes.ValidationFailed, error.Message);

    extension<TValue, TKey>(IObjectFactory<TValue, TKey, ValidationError>) where TValue : IObjectFactory<TValue, TKey, ValidationError> where TKey : notnull {
        public static Fin<TValue> Validated(TKey key) => ToFin(TValue.Validate(key, provider: null, out TValue? value), value);
    }

    extension<TValue>(ISmartEnum<string, TValue, ValidationError>) where TValue : ISmartEnum<string, TValue, ValidationError> {
        public static Fin<TValue> Validated(string key) => Validated<TValue, string>(key);
    }
}
