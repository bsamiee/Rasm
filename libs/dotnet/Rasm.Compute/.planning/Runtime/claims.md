# [BENCHMARK_CLAIMS]

Rasm.Compute binds every performance-motivated route to a MEASURED claim: an admitted input class, a measured distribution, a typed profile artifact, and the host fingerprint the measurement ran under. A route binds only behind a winning claim whose full fingerprint and input class match, so a SIMD lane, a compression posture, a partition count, a DATAS value, or a numeric-provider rank is a measurement rather than an intuition.

Recency and fingerprint admission are the settled `Rasm.Persistence` `Query/cache#BENCHMARK_INDEX` owner's — its horizon and clock close inside the index — and `HostFingerprint` is the `Rasm.AppHost` `Runtime/determinism#DETERMINISM_KERNEL` declaration this package composes through its legal reference.

## [01]-[INDEX]

- [02]-[CLAIM_INPUT]: `PayloadBand`, `BenchmarkPolarity`, `BenchDistribution`, `BenchmarkInput` — the closed band and direction vocabularies, the measured distribution product, and the admitted tensor input class.
- [03]-[PROFILE_EVIDENCE]: `ProfileArtifact` — the one content-addressed profile-evidence vocabulary.
- [04]-[CLAIM_ROW]: `BenchmarkClaim` — measured evidence bound to family, case token, and host fingerprint, with its durable mint and staleness read.
- [05]-[HOST_FORECAST]: `HostClaims` — the two host-fingerprint extensions only this domain can decide, and the ONE duration-forecast query the substrate axis binds.

## [02]-[CLAIM_INPUT]

- Owner: `PayloadBand` owns the closed payload-size band and each row's own ceiling; `BenchmarkPolarity` owns the closed optimization direction; `BenchDistribution` owns the six columns one benchmark run measures together and their ordering invariants; `BenchmarkInput` owns the admitted tensor shape, stride, batch, density, and band.
- Entry: `BenchmarkInput.Validate` / `BenchmarkInput.Create` — the generated `[ComplexValueObject]` factory pair over the member-ordered arguments; `PayloadBand.Of(long)` classifies a payload size onto its row; `BenchDistribution.Validate` admits the measured distribution.
- Auto: admission validates payload size, dtype, shape, strides, batch, and density in ONE accumulating pass, so a malformed input reports every offending column in one refusal; `Rank`, `Contiguous`, and `Band` derive from the admitted members and never travel as caller-supplied columns a caller could contradict.
- Packages: Thinktecture.Runtime.Extensions, Generator.Equals (`[Equatable]` + `[IgnoreEquality]` — the input-class diff rail), LanguageExt.Core, NodaTime, Rasm.Element (project — the `Projection/fault#ADMISSION_SLOTS` accumulating slot algebra), BCL inbox
- Growth: a new input dimension is one `BenchmarkInput` member with its own slot; a new payload band is one `PayloadBand` row carrying its ceiling; duration admits `BenchmarkPolarity.Minimize` while throughput and scores admit `Maximize`.
- Law: admission ACCUMULATES. Every column here is an INDEPENDENT claim, so a malformed sweep row names all seven at once where the `+`-chained violation string it replaces reported them as an opaque comma list and the short-circuiting rail before that reported one. The slots are the seam's own `AdmissionSlots` algebra under its deferred-mint arity, so the refusal codes on Compute's `FaultBand.Core` row through Compute's own minter and this page declares no accumulator of its own.
- Law: a derived column is DERIVED, never stored. `Band` reads `PayloadBytes`, `Rank` reads `Shape.Count`, and `Contiguous` folds the shape against the strides — three columns a stored mirror would let a rehydration contradict. `Contiguous` folds unchecked because the extent slot already proved the shape's product fits, so the `Try`-to-`bool` collapse that discarded the overflow error and then re-reported it as an opaque `"extent"` token has nothing left to discard.
- Boundary: shape, strides, batch, density, route, and tolerance participate in identity, preventing a contiguous micro-vector claim from winning for a strided batched tensor. `Key()` is the identity spelling and the generated comparer never keys a store — `[Equatable]` is the input-class DIFF rail alone, so `Inequalities` names the axis that moved between two claim generations while the derived projections stay ignored and no member compares twice. NAMED LOSS: the two `[OrderedEquality]` attributes the `Seq<long>` members carried are retired — the carrier compares element-wise (E29), so the attribute restated a guarantee the member already held.
- Boundary: the terminal `PayloadBand` row's ceiling IS `long.MaxValue`, so the band scan is total by construction and the `"large"` magic default the tuple roster carried — a second spelling of the terminal row, drifting the moment the row moved — has nothing left to fall back to.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using static Rasm.Element.Projection.AdmissionSlots;

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PayloadBand {
    public static readonly PayloadBand Micro = new("micro", maxBytes: 4L << 10);
    public static readonly PayloadBand Small = new("small", maxBytes: 256L << 10);
    public static readonly PayloadBand Medium = new("medium", maxBytes: 16L << 20);
    public static readonly PayloadBand Large = new("large", maxBytes: long.MaxValue);

    public long MaxBytes { get; }

    public static PayloadBand Of(long payloadBytes) =>
        toSeq(Items).Find(row => payloadBytes <= row.MaxBytes).IfNone(Large);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BenchmarkPolarity {
    public static readonly BenchmarkPolarity Minimize = new("minimize");
    public static readonly BenchmarkPolarity Maximize = new("maximize");
}

[ComplexValueObject]
public sealed partial class BenchDistribution {
    public Duration Mean { get; }
    public Duration Median { get; }
    public Duration P95 { get; }
    public Duration StdDev { get; }
    public int Samples { get; }
    public int Warmups { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Duration mean, ref Duration median, ref Duration p95, ref Duration stdDev, ref int samples, ref int warmups) =>
        validationError = Accumulate(Seq(
            Gate(mean >= Duration.Zero, "mean", mean, Rejected),
            Gate(median >= Duration.Zero, "median", median, Rejected),
            Gate(p95 >= median, "p95-below-median", p95, Rejected),
            Gate(stdDev >= Duration.Zero, "std-dev", stdDev, Rejected),
            Gate(samples >= 2, "samples", samples, Rejected),
            Gate(warmups >= 0, "warmups", warmups, Rejected)))
        .Match(Succ: static _ => null, Fail: static errors => Rejection("benchmark-distribution", errors));

    static Error Rejected<T>(string slot, T value) =>
        new ComputeFault.EquivalenceMiss($"<{slot}:{value}>");

    internal static ComputeFault Rejection(string stem, Seq<Error> errors) =>
        new ComputeFault.EquivalenceMiss($"<{stem}-rejected:{Error.Many(errors)}>");
}

[ComplexValueObject(SkipEqualityComparison = true)]
[Equatable]
public sealed partial class BenchmarkInput {
    public long PayloadBytes { get; }
    public string Dtype { get; }
    public Seq<long> Shape { get; }
    public Seq<long> Strides { get; }
    public int Batch { get; }
    public double Density { get; }

    [IgnoreMember, IgnoreEquality]
    public PayloadBand Band => PayloadBand.Of(PayloadBytes);

    [IgnoreMember, IgnoreEquality]
    public int Rank => Shape.Count;

    [IgnoreMember, IgnoreEquality]
    public bool Contiguous =>
        Shape.Rev().Zip(Strides.Rev())
            .Fold((Expected: 1L, Valid: true), static (state, axis) => (state.Expected * axis.Item1, state.Valid && axis.Item2 == state.Expected))
            .Valid;

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref long payloadBytes, ref string dtype, ref Seq<long> shape, ref Seq<long> strides, ref int batch, ref double density) =>
        validationError = (
            Gate(payloadBytes >= 0L, "payload", payloadBytes, Rejected),
            Gate(!string.IsNullOrWhiteSpace(dtype), "dtype", dtype, Rejected),
            Gate(!shape.IsEmpty && shape.ForAll(static dimension => dimension > 0L), "shape", shape.Count, Rejected),
            Gate(shape.Count == strides.Count && strides.ForAll(static stride => stride > 0L), "strides", strides.Count, Rejected),
            Extent(shape),
            Gate(batch > 0, "batch", batch, Rejected),
            Gate(double.IsFinite(density) && density is > 0d and <= 1d, "density", density, Rejected))
            .Apply(static (_, _, _, _, _, _, _) => unit).As()
            .Match(Succ: static _ => null, Fail: static errors => BenchDistribution.Rejection("benchmark-input", errors));

    static Validation<Error, Unit> Extent(Seq<long> shape) =>
        Op.Of(name: "benchmark.extent").Catch(() => Fin.Succ(shape.Fold(1L, static (extent, dimension) => checked(extent * dimension))))
            .Match(
                Succ: static _ => Success<Error, Unit>(unit),
                Fail: static _ => Fail<Error, Unit>(Rejected("extent", "overflow")));

    static Error Rejected<T>(string slot, T value) =>
        new ComputeFault.PayloadOverBounds($"<{slot}:{value}>");

    public string Key() =>
        string.Create(CultureInfo.InvariantCulture,
            $"{Band.Key}|{Dtype}|{string.Join("x", Shape)}|{string.Join("x", Strides)}|{Batch}|{Density:R}");
}
```

## [03]-[PROFILE_EVIDENCE]

- Owner: `ProfileArtifact` — the ONE typed benchmark profile-evidence vocabulary, keyed by the content address the blob index mints.
- Cases: `ChromeTrace` from the inference `EndProfiling` run, carrying the `InferenceSession.ProfilingStartTimeNs` epoch so a trace viewer aligns run-relative timestamps without re-opening the session · `BenchmarkExport` from a BenchmarkDotNet exporter, carrying the exporter key · `EpContext` from the session fleet compile, carrying the execution-provider key.
- Auto: artifacts — chrome-trace profiles, BenchmarkDotNet exports, EP-context caches — admit as content-keyed `ArtifactIndexRow`s on the blob lane and ride the claim as typed cases, each carrying the same `ContentAddress` the index row holds so evidence joins its blob in one hop.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Element (project — `Projection/address#CONTENT_ADDRESS` `ContentAddress`), Rasm.Persistence (project), BCL inbox
- Growth: a new profile source is one case row and one oneof arm at `[06]-[BENCHMARK_WIRE]`; zero new surface.
- Boundary: identity is the `ContentAddress` the blob index mints, never the on-disk path, so a moved or re-materialized file cannot fork evidence. This vocabulary replaces loose path-string columns on per-run artifact lists.

```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(ChromeTrace), "chrome-trace")]
[JsonDerivedType(typeof(BenchmarkExport), "benchmark-export")]
[JsonDerivedType(typeof(EpContext), "ep-context")]
public abstract partial record ProfileArtifact {
    private ProfileArtifact(ContentAddress content) => Content = content;

    public ContentAddress Content { get; }

    public sealed record ChromeTrace(ContentAddress Content, ulong StartNs) : ProfileArtifact(Content);

    public sealed record BenchmarkExport(ContentAddress Content, string Exporter) : ProfileArtifact(Content);

    public sealed record EpContext(ContentAddress Content, string Ep) : ProfileArtifact(Content);
}
```

## [04]-[CLAIM_ROW]

- Owner: `BenchmarkClaim` — measured evidence bound to `BenchmarkFamily`, `CacheToken`, and `HostFingerprint`, carrying its own durable mint and its staleness read.
- Entry: `BenchmarkClaim.Validate` / `BenchmarkClaim.Create` — the generated `[ComplexValueObject]` factory pair; `Persist()` delegates the durable mint to `BenchmarkFamily.Claim`, carrying operations, corpus, artifact key, timing, allocation, fingerprint, and timestamp without a parallel constructor; `Stale(HostFingerprint)` compares the effective fingerprint through the spine record's generated structural equality; `Sweep(Func<IO<Unit>>)` registers the equivalence cadence row on `WorkLane.Benchmark`.
- Auto: `Key` includes the family, admitted case token, full input class, route, provider, polarity, and tolerance class, so claim admission refuses a zero-init case token — the struct value object's admission-bypassing ghost — beside the family check before identity forms. `Stale` includes the container-limited processor count `HostFingerprint.Effective` substitutes for the spine mint's ambient host count.
- Packages: BenchmarkDotNet (the `Summary` graph the AppHost bench edge folds into the measured distribution and the `JsonExporter.Full` artifact this claim references by key), Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core, Rasm.AppHost (project — the declared `HostFingerprint` this claim composes), Rasm.Element (project — the accumulating slot algebra), Rasm.Persistence (project — `BenchmarkFamily.Claim`, `BenchmarkRow`), BCL inbox
- Growth: a new performance surface is one claim row; a new claim dimension is one `BenchmarkClaim` member with its own slot; a new host dimension lands on the AppHost declaration, never a Compute mirror.
- Law: `ArtifactKey` is a PATH a run wrote, never a minted address. It carries the BenchmarkDotNet `ExporterBase.GetArtifactFullName(Summary)` string the AppHost bench edge resolves, spelled identically at `Rasm.Persistence` `Query/cache#BENCHMARK_INDEX` `BenchmarkRow.ArtifactKey` — three packages, one `Option<string>` column. The Compute object-plane address grammar (`Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIdentity.Address`) and the `Rasm.Bim` `Energy/exchange#ENERGY_EXCHANGE` `ArtifactKey` value object both MINT a `<content-key:x32>:<kind>` address; this column mints nothing, which is the discriminant, and admitting it through either grammar would reject every export path a harness writes.
- Boundary: `Provider` carries the numeric-lane key while `Substrate` remains the execution discriminant. `Stamps` includes the provider determinism tag, admitted package versions, device identity, and runtime posture; every mint on this page goes through `HostFingerprint.Effective` so `Processors` carries `CpuBudget.Total`, never the ambient host count the spine's own `Current` reads under a container limit.
- Boundary: the measured distribution and its protocol counts persist as claim evidence while Persistence owns recency; the family owns the durable mint AND its refusals, so a claim admitted here can still fail the row invariants persistence holds and swallowing that would leave a forecast reading a row no store would accept.
- Boundary: `artifactKey` admission reads FORWARD — a present key that is blank refuses, an absent key passes — where the inverted `Map(...).IfNone(true)` fold it replaces spelled the passing case as a negation of a negation.

```csharp
[ComplexValueObject]
public sealed partial class BenchmarkClaim {
    public BenchmarkInput Input { get; }
    public Substrate Substrate { get; }
    public BenchmarkFamily Family { get; }
    public CacheToken Case { get; }
    public string Route { get; }
    public string Provider { get; }
    public BenchmarkPolarity Polarity { get; }
    public BenchDistribution Distribution { get; }
    public long AllocatedBytes { get; }
    public long Operations { get; }
    public Option<UInt128> Corpus { get; }
    public Option<string> ArtifactKey { get; }
    public double EquivalenceMaxDeviation { get; }
    public string ToleranceClass { get; }
    public HostFingerprint Fingerprint { get; }
    public Seq<ProfileArtifact> Artifacts { get; }
    public Instant At { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref BenchmarkInput input, ref Substrate substrate, ref BenchmarkFamily family, ref CacheToken @case,
        ref string route, ref string provider, ref BenchmarkPolarity polarity, ref BenchDistribution distribution,
        ref long allocatedBytes, ref long operations, ref Option<UInt128> corpus, ref Option<string> artifactKey,
        ref double equivalenceMaxDeviation, ref string toleranceClass, ref HostFingerprint fingerprint,
        ref Seq<ProfileArtifact> artifacts, ref Instant at) =>
        validationError = Accumulate(Seq(
            Gate(!string.IsNullOrWhiteSpace((string)@case), "case", "<default>", Rejected),
            Gate(!string.IsNullOrWhiteSpace(route), "route", route, Rejected),
            Gate(!string.IsNullOrWhiteSpace(provider), "provider", provider, Rejected),
            Gate(allocatedBytes >= 0L, "allocation", allocatedBytes, Rejected),
            Gate(operations >= 1L, "operations", operations, Rejected),
            Gate(!artifactKey.Exists(string.IsNullOrWhiteSpace), "artifact-key", "<blank>", Rejected),
            Gate(double.IsFinite(equivalenceMaxDeviation) && equivalenceMaxDeviation >= 0d, "equivalence", equivalenceMaxDeviation, Rejected),
            Gate(!string.IsNullOrWhiteSpace(toleranceClass), "tolerance", toleranceClass, Rejected),
            Gate(fingerprint.Processors > 0, "fingerprint", fingerprint.Processors, Rejected),
            Gate(!artifacts.Exists(static artifact => artifact.Switch(
                chromeTrace: static _ => false,
                benchmarkExport: static export => string.IsNullOrWhiteSpace(export.Exporter),
                epContext: static context => string.IsNullOrWhiteSpace(context.Ep))), "artifact", artifacts.Count, Rejected)))
        .Match(Succ: static _ => null, Fail: static errors => BenchDistribution.Rejection("benchmark-claim", errors));

    static Error Rejected<T>(string slot, T value) =>
        new ComputeFault.EquivalenceMiss($"<{slot}:{value}>");

    public string Key() => string.Create(CultureInfo.InvariantCulture,
        $"{Family.Key}|{(string)Case}|{Input.Key()}|{Substrate.Key}|{Route}|{Provider}|{Polarity.Key}|{ToleranceClass}");

    public Fin<BenchmarkRow> Persist() => Family.Claim(
        Case, Route, Distribution.Median, Distribution.P95, AllocatedBytes, Operations,
        Corpus, ArtifactKey, Fingerprint.ToString(), At);

    public bool Stale(HostFingerprint current) => !HostFingerprint.EqualityComparer.Default.Equals(Fingerprint, current);

    public static ScheduleEntry Sweep(Func<IO<Unit>> work) =>
        new("compute-equivalence-sweep", new OccurrenceSpec.Every(Duration.FromDays(7)), DeadlineClass.SupportWindow, None, RedrivePolicy.None, work);
}
```

## [05]-[HOST_FORECAST]

- Owner: `HostClaims` — the two `HostFingerprint` members only this domain can decide, and the ONE duration-forecast query.
- Entry: `public Option<BenchmarkRow> Claim(ModelResultIndex index, Seq<BenchmarkRow> rows)` delegates fingerprint and recency admission to the Persistence `ModelResultIndex.Claim` owner (its horizon and clock close inside the index; no call shape can omit or replace them), `None` being the fall-through to the static cost rank on the substrate row. `public Option<Duration> Forecast(ModelResultIndex index, Seq<BenchmarkClaim> claims, Substrate substrate, long payloadBytes)` is the ONE duration-forecast query — it narrows the claims to the substrate row and the payload band, hands the survivors' minted rows to that same `Claim` gate, and answers the winner's median; `Runtime/admission#SUBSTRATE_AXIS` `SelectionContext.Forecast` binds it and re-derives no half of it.
- Auto: narrowing lands here because substrate and payload band live on the CLAIM — the durable row key carries family, case, and route alone — while fingerprint match and recency stay closed inside `ModelResultIndex.Claim`, so neither gate is re-implemented on the selection side. Claims whose mint refuses drop out rather than forecasting off a row persistence would never hold.
- Packages: LanguageExt.Core, NodaTime, Rasm.AppHost (project — `HostFingerprint`, `CpuBudget`), Rasm.Persistence (project — `ModelResultIndex`, `BenchmarkRow`), BCL inbox
- Growth: a further host-derived read that only this domain can decide is one extension member here; a further host DIMENSION lands on the AppHost declaration.

```csharp
public static class HostClaims {
    extension(HostFingerprint host) {
        public Option<BenchmarkRow> Claim(ModelResultIndex index, Seq<BenchmarkRow> rows) => index.Claim(rows, host.ToString());

        public Option<Duration> Forecast(ModelResultIndex index, Seq<BenchmarkClaim> claims, Substrate substrate, long payloadBytes) =>
            host.Claim(index, Banded(claims, substrate, PayloadBand.Of(payloadBytes))).Map(static row => row.Median);
    }

    extension(HostFingerprint) {
        public static HostFingerprint Effective(FrozenDictionary<string, string> stamps, CpuBudget budget) =>
            HostFingerprint.Current(stamps) with { Processors = budget.Total };
    }

    static Seq<BenchmarkRow> Banded(Seq<BenchmarkClaim> claims, Substrate substrate, PayloadBand band) =>
        claims.Filter(claim => claim.Substrate == substrate && claim.Input.Band == band)
            .Choose(static claim => claim.Persist().ToOption());
}
```

## [06]-[BENCHMARK_WIRE]

- Law: Compute is the MINTER of the benchmark claim document, so the generated messages are its wire vocabulary and no STJ record, TS interface, or MessagePack twin carries the claim's name — `Rasm.Persistence` and the Rhino host decode the same generated family and import no Compute type. The document's `host` column binds AppHost `Runtime/determinism#DETERMINISM_KERNEL` `HostFingerprintMap.Wire(EnvFingerprint)` by IMPORT — a second host projection beside the claim forks the frozen column set the moment either side gains a column. Producers format the document through AppHost `WireJson.Formatter` and parse through `WireJson.Parser`; Compute formats nothing itself.
- Law: the correspondence is GENERATED where it is reader-free and hand-written exactly where protobuf forbids generation — proto3 `optional` scalars (`corpus`, `artifact_key`, `warmups`, `allocated_bytes`, `operations`, `ticks`) sit behind null-rejecting setters, so they land as one `IfSome` tail after the generated body, and the `subject`/`kind` oneofs assign ONE arm through the domain union's generated total `Switch`. `counters` stays the generated `MapField<string,double>` the executing harness fills at the AppHost bench edge; the claim carries no counter column. NAMED LOSS: the seven hand TS interfaces and the `BenchmarkRungWire` literal roster are retired. Witness: `BenchRung` is the generated enum, and the rung cells the band carries are `RungCell` rows — `Avg = Mean`, `P50 = Median`, `P95`, `StdDev` — in NANOSECONDS off `Duration.TotalNanoseconds`, the one unit every consumer performs arithmetic on.
- Growth: a new claim dimension lands as one generated field and one `[MapProperty]` row, the build breaking until both agree; a new payload band, polarity, or rung lands as one enum value at the corpus and one `[MapEnumValue]` row only where the domain spelling diverges; a new profile source is one `ProfileArtifact` case and one oneof arm, the total `Switch` breaking until the arm lands.
- Boundary: `BenchmarkPolarity` and `PayloadBand` are `[SmartEnum<string>]` rows whose keys spell the generated enum names under `ByName` mapping, so `minimize`/`maximize` and `micro`/`small`/`medium`/`large` cross with no table and an unrostered key has no arm to land on; `corpus` crosses as the kernel's sixteen big-endian bytes through `ContentHash.Wire`; `minted` crosses as the NodaTime `Instant` through `ToTimestamp`; the band's `samples` column is the measured sample vector the AppHost bench edge supplies and stays empty where a claim carries only its distribution.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
// Contracts are retired from this logic.

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target, EnumMappingStrategy = EnumMappingStrategy.ByName)]
[UseStaticMapper(typeof(NodaExtensions))]
public static partial class ClaimWireMap {
    public static BenchmarkClaimWire Document(string suite, EnvFingerprint env, Instant minted, Seq<BenchmarkClaim> claims) {
        BenchmarkClaimWire document = new() { Suite = suite, Host = HostFingerprintMap.Wire(env), Minted = minted.ToTimestamp() };
        document.Metrics.AddRange(claims.Map(Metric));
        return document;
    }

    public static BenchMetric Metric(BenchmarkClaim claim) {
        BenchMetric metric = new() {
            Label = claim.Case, Unit = "ns", Modality = BenchModality.Fn, Polarity = Polarity(claim.Polarity),
            Kernel = Kernel(claim), Band = Band(claim.Distribution),
            Warmups = checked((uint)claim.Distribution.Warmups), AllocatedBytes = checked((ulong)claim.AllocatedBytes), Operations = checked((ulong)claim.Operations),
        };
        return metric;
    }

    [MapProperty(nameof(BenchmarkClaim.Case), nameof(BenchKernelWire.Case), Use = nameof(CaseText))]
    [MapProperty(nameof(BenchmarkClaim.Substrate), nameof(BenchKernelWire.Substrate), Use = nameof(SubstrateKey))]
    [MapProperty(nameof(BenchmarkClaim.Family), nameof(BenchKernelWire.Family), Use = nameof(FamilyKey))]
    [MapperIgnoreTarget(nameof(BenchKernelWire.Corpus))]
    [MapperIgnoreTarget(nameof(BenchKernelWire.ArtifactKey))]
    private static partial BenchKernelWire KernelCore(BenchmarkClaim claim);

    private static BenchKernelWire Kernel(BenchmarkClaim claim) {
        BenchKernelWire kernel = KernelCore(claim);
        claim.Corpus.Iter(corpus => kernel.Corpus = ContentHash.Wire(corpus));
        claim.ArtifactKey.Iter(key => kernel.ArtifactKey = key);
        return kernel;
    }

    [MapProperty(nameof(BenchmarkInput.Band), nameof(BenchInputWire.Band), Use = nameof(BandKey))]
    [MapperIgnoreSource(nameof(BenchmarkInput.Rank))]
    [MapperIgnoreSource(nameof(BenchmarkInput.Contiguous))]
    public static partial BenchInputWire Input(BenchmarkInput input);

    public static BenchBandWire Band(BenchDistribution distribution) {
        BenchBandWire band = new() { SampleCount = checked((uint)distribution.Samples) };
        band.Rungs.AddRange(Seq(
            new RungCell { Rung = BenchRung.Avg, Value = distribution.Mean.TotalNanoseconds },
            new RungCell { Rung = BenchRung.P50, Value = distribution.Median.TotalNanoseconds },
            new RungCell { Rung = BenchRung.P95, Value = distribution.P95.TotalNanoseconds },
            new RungCell { Rung = BenchRung.StdDev, Value = distribution.StdDev.TotalNanoseconds }));
        return band;
    }

    public static ProfileArtifactWire Artifact(ProfileArtifact artifact) => artifact.Switch(
        chromeTrace: static trace => new ProfileArtifactWire { ChromeTrace = new ChromeTraceWire { Content = ContentHash.Wire(trace.Content.Value), StartNs = trace.StartNs } },
        benchmarkExport: static export => new ProfileArtifactWire { BenchmarkExport = new BenchmarkExportWire { Content = ContentHash.Wire(export.Content.Value), Exporter = export.Exporter } },
        epContext: static context => new ProfileArtifactWire { EpContext = new EpContextWire { Content = ContentHash.Wire(context.Content.Value), Ep = context.Ep } });

    [UserMapping] private static string CaseText(CacheToken token) => (string)token;
    [UserMapping] private static string SubstrateKey(Substrate substrate) => substrate.Key;
    [UserMapping] private static string FamilyKey(BenchmarkFamily family) => family.Key;
    [UserMapping] private static WireBand BandKey(PayloadBand band) => Enum.Parse<WireBand>(band.Key, ignoreCase: true);
    [UserMapping] private static BenchPolarity Polarity(BenchmarkPolarity polarity) => Enum.Parse<BenchPolarity>(polarity.Key, ignoreCase: true);
}
```
