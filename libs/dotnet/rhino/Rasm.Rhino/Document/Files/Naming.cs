using System.Buffers;
using System.Globalization;

namespace Rasm.Rhino.Document.Files;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<InvalidRhinoValue>]
public sealed partial class NamePart {
    internal static readonly SearchValues<char> Reserved = SearchValues.Create([.. "\"*/:<>?\\|", .. Enumerable.Range(0, 32).Select(static code => (char)code)]);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref string value) {
        ReadOnlySpan<char> trimmed = value.AsSpan().Trim();
        value = string.Create(trimmed.Length, trimmed, static (target, source) => source.ReplaceAny(target, Reserved, '_'));
        validationError = trimmed.Trim('.').IsEmpty ? new InvalidRhinoValue() : null;
    }
}

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<InvalidRhinoValue>]
public sealed partial class FileExtension {
    internal const char Mark = '.';

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref string value) =>
        validationError = value is [Mark, _, ..] && !value.AsSpan(1).ContainsAny(NamePart.Reserved) && !value.AsSpan(1).Contains(Mark) ? null : new InvalidRhinoValue();
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct OutputVersion {
    public static OutputVersion MinValue { get; } = new(1);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value >= MinValue._value ? null : new InvalidRhinoValue();
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct SequenceNumber {
    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value >= 0 ? null : new InvalidRhinoValue();
}

[Union]
public abstract partial record OutputVersioning {
    public sealed record Unversioned : OutputVersioning;
    public sealed record Pinned(OutputVersion Version) : OutputVersioning;
    public sealed record NextFree : OutputVersioning;
}

public sealed record OutputName(Seq<NamePart> Scope, OutputVersioning Version, Seq<NamePart> Parts, Option<SequenceNumber> Number, FileExtension Extension);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Naming {
    private const char Separator = '_';
    private const string VersionMark = "v";

    public static Option<TCounter> Next<TCounter>(this TCounter counter) where TCounter : IObjectFactory<TCounter, int, InvalidRhinoValue>, IConvertible<int> =>
        counter.ToValue() is var held and < int.MaxValue ? Conversions.Validated<TCounter, int, InvalidRhinoValue>(held + 1).ToOption() : None;

    public static string Compose(NamePart stem, OutputName name, Option<OutputVersion> version) =>
        string.Concat([
            string.Join(Separator, [stem, .. name.Scope, .. version.ToSeq().Map(static held => string.Create(CultureInfo.InvariantCulture, $"{VersionMark}{held:D3}")), .. name.Parts]),
            .. name.Number.ToSeq().Map(static held => string.Create(CultureInfo.InvariantCulture, $"{FileExtension.Mark}{held:D4}")),
            name.Extension,
        ]);

    public static Option<OutputVersion> VersionOf(NamePart stem, Seq<NamePart> scope, string entry) {
        string lead = string.Join(Separator, [stem, .. scope, VersionMark]);
        ReadOnlySpan<char> rest = entry.StartsWith(lead, StringComparison.OrdinalIgnoreCase) ? entry.AsSpan(lead.Length) : [];
        return int.TryParse(rest.IndexOfAny(Separator, FileExtension.Mark) is >= 0 and var stop ? rest[..stop] : rest, NumberStyles.None, CultureInfo.InvariantCulture, out int number)
            ? Conversions.Validated<OutputVersion, int, InvalidRhinoValue>(number).ToOption()
            : None;
    }
}
