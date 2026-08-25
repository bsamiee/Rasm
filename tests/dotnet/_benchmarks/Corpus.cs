using System.Globalization;
using Rasm.TestKit;

namespace Rasm.Benchmarks;

// --- [ERRORS] --------------------------------------------------------------------------
internal abstract record CorpusGap : Expected {
    private CorpusGap(string message) : base(Message: message, Code: 0, Inner: default) { }

    public sealed record Missing(string Slug) : CorpusGap($"missing fixture: declared slug '{Slug}' resolves no committed file under {BenchCorpus.Root}");
    public sealed record Undeclared(string Path) : CorpusGap($"undeclared fixture: '{Path}' realizes no declared slug");
    public sealed record Unprefixed(string Slug) : CorpusGap($"unrealizable slug: '{Slug}' carries no '{BenchCorpus.Prefix}' prefix, so no committed file can realize it");
    public sealed record Oversize(string Path, long Bytes) : CorpusGap(string.Create(provider: CultureInfo.InvariantCulture, $"oversize fixture: '{Path}' is {Bytes} bytes over the {BenchCorpus.Ceiling}-byte ceiling"));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class BenchCorpus {
    public const string Root = "tests/dotnet/_benchmarks/corpus";

    public const string Prefix = "corpus-";
    public const string Pattern = $"{Prefix}*";

    public const long Ceiling = 32L << 20;

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
