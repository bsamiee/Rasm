using Rasm.TestKit;

namespace Rasm.Benchmarks;

// --- [OPERATIONS] ----------------------------------------------------------------------
// The committed corpus manifest: folder BenchClaim rosters DECLARE corpus slugs and stay the
// authority; this manifest realizes each slug as the fixture row the corpus gate admits.
// CorpusEntry.RelativePath transcribes the declared slug verbatim and CorpusEntry.Key measures
// from fixture bytes at run, so fixture identity is discovered, never asserted here.
internal static class BenchCorpus {
    public const string Root = "tests/csharp/_benchmarks/corpus";
    public const string Pattern = "corpus-*";

    // Declared slugs, ordinal-sorted; every row transcribes a folder roster's Corpus column.
    public static readonly Seq<string> Declared = Seq(
        "corpus-drawing-dwg",
        "corpus-geo-cog",
        "corpus-geo-gpkg",
        "corpus-graph-100k",
        "corpus-graph-1m",
        "corpus-mesh-ply",
        "corpus-model-bim",
        "corpus-model-ifc",
        "corpus-scene-fbx",
        "corpus-scene-glb",
        "corpus-stage-usd");

    // Bijective admission: every declared slug resolves one committed fixture and every discovered
    // fixture realizes one declared slug; a gap in either direction is a typed refusal, so a
    // declaration cannot float free of the corpus and a stray fixture cannot ride ungoverned.
    public static Fin<Seq<CorpusEntry>> Admit() {
        Seq<CorpusEntry> discovered = Manifests.Corpus(relativeRoot: Root, pattern: Pattern);
        Seq<Error> gaps =
            Declared.Bind(slug => discovered.Exists(predicate: entry => string.Equals(a: entry.RelativePath, b: slug, comparisonType: StringComparison.Ordinal))
                ? Seq<Error>()
                : Seq(Error.New($"missing fixture: declared corpus slug '{slug}' resolves no committed file under {Root}")))
            + discovered.Bind(entry => Declared.Exists(predicate: slug => string.Equals(a: slug, b: entry.RelativePath, comparisonType: StringComparison.Ordinal))
                ? Seq<Error>()
                : Seq(Error.New($"undeclared fixture: '{entry.RelativePath}' realizes no declared corpus slug")));
        return gaps.IsEmpty
            ? Fin.Succ(value: discovered)
            : Fin.Fail<Seq<CorpusEntry>>(error: Error.Many(errors: gaps));
    }
}
