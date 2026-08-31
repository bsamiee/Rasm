namespace Lab.T04;

[ComplexValueObject(DefaultStringComparison = StringComparison.OrdinalIgnoreCase)]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.Json, UseWithEntityFramework = true, UseForModelBinding = true, HasCorrespondingConstructor = true)]
internal sealed partial class FileLocation {
    public string Store { get; }
    public string Path { get; }

    private FileLocation(string value) => (Store, Path) = Split(value);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string store, ref string path) {
        if (string.IsNullOrWhiteSpace(store) || store.Contains(':', StringComparison.Ordinal)) validationError = new ValidationError("Store must be non-empty and must not contain ':'");
        else if (string.IsNullOrWhiteSpace(path)) validationError = new ValidationError("Path must be non-empty");
        store = store.Trim();
        path = path.Trim();
    }

    public static ValidationError? Validate(string? value, IFormatProvider? provider, out FileLocation? item) {
        item = null;
        if (string.IsNullOrWhiteSpace(value)) return new ValidationError("A file location is not empty");
        (string store, string path) = Split(value);
        return Validate(store, path, out item);
    }

    public string ToValue() => $"{Store}:{Path}";

    private static (string Store, string Path) Split(string value) {
        int separator = value.IndexOf(':', StringComparison.Ordinal);
        return separator < 0 ? (value, "") : (value[..separator], value[(separator + 1)..]);
    }
}
