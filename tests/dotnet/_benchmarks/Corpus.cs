using System.Globalization;
using Rasm.TestKit;

namespace Rasm.Benchmarks;

// --- [ERRORS] --------------------------------------------------------------------------
// One closed refusal family for corpus admission: each case convicts the slug or path it names, so the gate prints a row an operator acts on
// and the count separates the four defects instead of folding them into one string bag. Expected-derived, so Error.Many carries the case
// whole and a consumer reading the rail still dispatches on the type.
internal abstract record CorpusGap : Expected {
    private CorpusGap(string message) : base(Message: message, Code: 0, Inner: default) { }

    public sealed record Missing(string Slug) : CorpusGap($"missing fixture: declared slug '{Slug}' resolves no committed file under {BenchCorpus.Root}");
    public sealed record Undeclared(string Path) : CorpusGap($"undeclared fixture: '{Path}' realizes no declared slug");
    public sealed record Unprefixed(string Slug) : CorpusGap($"unrealizable slug: '{Slug}' carries no '{BenchCorpus.Prefix}' prefix, so no committed file can realize it");
    public sealed record Oversize(string Path, long Bytes) : CorpusGap(string.Create(provider: CultureInfo.InvariantCulture, $"oversize fixture: '{Path}' is {Bytes} bytes over the {BenchCorpus.Ceiling}-byte ceiling"));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
// The committed corpus manifest: folder BenchClaim rosters DECLARE corpus slugs and stay the authority; this manifest realizes each slug as
// the fixture row the corpus gate admits. CorpusEntry.RelativePath transcribes the declared slug verbatim and CorpusEntry.Key measures from
// fixture bytes at run, so fixture identity is discovered, never asserted here.
internal static class BenchCorpus {
    public const string Root = "tests/dotnet/_benchmarks/corpus";

    // The realization discriminant on a roster's Corpus column: a corpus- slug names a committed fixture this manifest discovers, a forge-
    // slug an Element CorpusGrade the bench mints in memory. The discovery glob IS this prefix, so a forge- column can neither be declared
    // here nor land under the root, and Admit refuses a declared slug the prefix cannot realize.
    public const string Prefix = "corpus-";
    public const string Pattern = $"{Prefix}*";

    // A fixture is a benchmark INPUT, sized to decode inside a BenchmarkDotNet warmup and iteration budget — a specimen archive over the
    // ceiling refuses at admission rather than inflating every session it enters.
    public const long Ceiling = 32L << 20;

    // Declared slugs, ordinal-sorted; each transcribes a folder roster's corpus- Corpus column, spelled corpus-<Key> off the roster row the
    // claim's lane decodes (InterchangeFormat.Key, GeoVectorSource.Key), so a row-key rename lands here as a missing-fixture row and never as
    // a fixture silently detached from the claim that measures it.
    public static readonly Seq<string> Declared = Seq(
        "corpus-bim",
        "corpus-dwg",
        "corpus-fbx",
        "corpus-geopackage",
        "corpus-geotiff",
        "corpus-glb",
        "corpus-ifc",
        "corpus-ply",
        "corpus-usd");

    // Bijective admission under the prefix and ceiling policies: every declared slug resolves one committed fixture and every discovered
    // fixture realizes one declared slug; a gap in either direction, a slug the prefix cannot realize, or a fixture over the ceiling is a
    // typed refusal, so a declaration cannot float free of the corpus and a stray or oversize fixture cannot ride ungoverned.
    public static Fin<Seq<CorpusEntry>> Admit() {
        Seq<CorpusEntry> discovered = Manifests.Corpus(relativeRoot: Root, pattern: Pattern);
        Seq<CorpusGap> gaps =
            Declared.Filter(static slug => !slug.StartsWith(value: Prefix, comparisonType: StringComparison.Ordinal)).Map(CorpusGap (slug) => new CorpusGap.Unprefixed(Slug: slug))
            + Declared.Filter(slug => !discovered.Exists(predicate: entry => Same(a: entry.RelativePath, b: slug))).Map(CorpusGap (slug) => new CorpusGap.Missing(Slug: slug))
            + discovered.Filter(static entry => !Declared.Exists(predicate: slug => Same(a: slug, b: entry.RelativePath))).Map(CorpusGap (entry) => new CorpusGap.Undeclared(Path: entry.RelativePath))
            + discovered.Filter(static entry => entry.Source.Length > Ceiling).Map(CorpusGap (entry) => new CorpusGap.Oversize(Path: entry.RelativePath, Bytes: entry.Source.Length));
        return gaps.IsEmpty
            ? Fin.Succ(value: discovered)
            : Fin.Fail<Seq<CorpusEntry>>(error: Error.Many(errors: gaps.Map(static gap => (Error)gap)));
    }

    private static bool Same(string a, string b) => string.Equals(a: a, b: b, comparisonType: StringComparison.Ordinal);
}
