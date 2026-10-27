using System.Buffers;
using System.Globalization;

namespace Rasm.RhinoCommon.Document.Files;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>]
[ValidationError<RhinoFault>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NamePart {
    internal static readonly SearchValues<char> Reserved = SearchValues.Create([.. """*/:<"?>\|""", .. Enumerable.Range(0, 32).Select(static code => (char)code)]);

    public static NamePart Ordinal(int position) => new(position.ToString(CultureInfo.InvariantCulture));

    static partial void ValidateFactoryArguments(ref RhinoFault? validationError, ref string value) {
        ReadOnlySpan<char> trimmed = value.AsSpan().Trim();
        value = string.Create(trimmed.Length, trimmed, static (target, source) => source.ReplaceAny(target, Reserved, '_'));
        validationError = trimmed.Trim('.').IsEmpty ? new RhinoFault($"value = '{value}'", RhinoReason.Unqualified) : null;
    }
}

[ValueObject<string>]
[ValidationError<RhinoFault>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FileExtension {
    internal const char Mark = '.';

    static partial void ValidateFactoryArguments(ref RhinoFault? validationError, ref string value) =>
        validationError = value is [Mark, _, ..] && !value.AsSpan(1).ContainsAny(NamePart.Reserved) && !value.AsSpan(1).Contains(Mark) ? null : new RhinoFault($"value = '{value}'", RhinoReason.Unqualified);
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<RhinoFault>]
public sealed partial class OutputVersion {
    public static OutputVersion MinValue { get; } = new(1);

    static partial void ValidateFactoryArguments(ref RhinoFault? validationError, ref int value) =>
        validationError = value >= 1 ? null : new RhinoFault(string.Create(CultureInfo.InvariantCulture, $"value = {value}"), RhinoReason.Outside);
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<RhinoFault>]
public sealed partial class SequenceNumber {
    static partial void ValidateFactoryArguments(ref RhinoFault? validationError, ref int value) =>
        validationError = value >= 0 ? null : new RhinoFault(string.Create(CultureInfo.InvariantCulture, $"value = {value}"), RhinoReason.Outside);
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

    extension<TCounter>(TCounter counter) where TCounter : class, IObjectFactory<TCounter, int, RhinoFault>, IConvertible<int> {
        public Option<TCounter> Next() =>
            counter.ToValue() is var held and < int.MaxValue && TCounter.Validate(held + 1, provider: null, out TCounter? next) is null ? Optional(next) : None;
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
        return int.TryParse(rest.IndexOfAny(Separator, FileExtension.Mark) is >= 0 and var stop ? rest[..stop] : rest, NumberStyles.None, CultureInfo.InvariantCulture, out int number)
            && OutputVersion.TryCreate(number, out OutputVersion? version)
            ? Some(version)
            : None;
    }
}
