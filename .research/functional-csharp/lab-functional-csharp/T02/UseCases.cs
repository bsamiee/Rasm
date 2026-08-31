namespace Lab.T02;

[ComplexValueObject(DefaultStringComparison = StringComparison.OrdinalIgnoreCase)]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.All, UseWithEntityFramework = true, HasCorrespondingConstructor = true)]
internal sealed partial class FileUrn {
    private FileUrn(string value) {
        string[] parts = value.Split(':', 2);
        FileStore = parts[0];
        Urn = parts[1];
    }

    public string FileStore { get; }
    public string Urn { get; }

    public static ValidationError? Validate(string? value, IFormatProvider? provider, out FileUrn? item) {
        item = null;

        if (string.IsNullOrWhiteSpace(value))
            return new ValidationError("FileUrn cannot be empty.");

        int separatorIndex = value.IndexOf(':', StringComparison.Ordinal);
        return separatorIndex <= 0 || separatorIndex == value.Length - 1
            ? new ValidationError("Invalid FileUrn format. Expected 'fileStore:urn'.")
            : Validate(value[..separatorIndex], value[(separatorIndex + 1)..], out item);
    }

    public string ToValue() => $"{FileStore}:{Urn}";
}
