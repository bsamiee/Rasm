using System.Buffers;
using System.Globalization;
using Rasm.Imaging.Pixels;

namespace Rasm.Rhino.Document.Files;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NamePart {
    internal static readonly SearchValues<char> Reserved = SearchValues.Create([.. """*/:<"?>\|""", .. Enumerable.Range(0, 32).Select(static code => (char)code)]);

    public static NamePart Ordinal(int position) => new(position.ToString(CultureInfo.InvariantCulture));

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        ReadOnlySpan<char> trimmed = value.AsSpan().Trim();
        value = string.Create(trimmed.Length, trimmed, static (target, source) => source.ReplaceAny(target, Reserved, '_'));
        validationError = trimmed.Trim('.').IsEmpty ? new ValidationError("name part is blank or dots alone") : null;
    }
}

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FileExtension {
    internal const char Mark = '.';

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = value is [Mark, _, ..] && !value.AsSpan(1).ContainsAny(NamePart.Reserved) && !value.AsSpan(1).Contains(Mark) ? null : new ValidationError($"extension {value} is no mark followed by reserved-free text without a second mark");
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct OutputVersion {
    public static OutputVersion MinValue { get; } = new(1);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value >= MinValue._value ? null : new ValidationError(string.Create(CultureInfo.InvariantCulture, $"output version {value} lies below {MinValue._value}"));
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct SequenceNumber {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value >= 0 ? null : new ValidationError(string.Create(CultureInfo.InvariantCulture, $"sequence number {value} is negative"));
}

[Union]
public abstract partial record OutputVersioning {
    public Option<OutputVersion> Taken =>
        Switch(unversioned: static _ => Option<OutputVersion>.None, pinned: static pinned => Some(pinned.Version), nextFree: static _ => Option<OutputVersion>.None);

    public sealed record Unversioned : OutputVersioning;
    public sealed record Pinned(OutputVersion Version) : OutputVersioning;
    public sealed record NextFree : OutputVersioning;
}

public sealed record OutputName(Seq<NamePart> Scope, OutputVersioning Version, Seq<NamePart> Parts, Option<SequenceNumber> Number, FileExtension Extension);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Naming {
    private const char Separator = '_';
    private const string VersionMark = "v";

    extension<TCounter>(TCounter counter) where TCounter : IObjectFactory<TCounter, int, ValidationError>, IConvertible<int> {
        public Option<TCounter> Next() =>
            counter.ToValue() is var held and < int.MaxValue ? Admission.Validated<TCounter, int>(held + 1).ToOption() : None;
    }

    public static string Compose(NamePart stem, OutputName name, Option<OutputVersion> version) =>
        string.Concat([
            string.Join(Separator, [stem, .. name.Scope, .. version.ToSeq().Map(static held => string.Create(CultureInfo.InvariantCulture, $"{VersionMark}{held:D3}")), .. name.Parts]),
            .. name.Number.ToSeq().Map(static held => string.Create(CultureInfo.InvariantCulture, $"{FileExtension.Mark}{held:D4}")),
            name.Extension,
        ]);

    public static Option<OutputVersion> VersionOf(NamePart stem, Seq<NamePart> scope, string entry) {
        string lead = string.Join(Separator, [stem, .. scope, VersionMark]);
        ReadOnlySpan<char> rest = entry.StartsWith(lead, StringComparison.OrdinalIgnoreCase) ? entry.AsSpan(lead.Length) : [];
        return Invariant.Number<int>(rest.IndexOfAny(Separator, FileExtension.Mark) is >= 0 and var stop ? rest[..stop] : rest, NumberStyles.None)
            .Bind(static number => OutputVersion.Validated(number).ToOption());
    }
}
