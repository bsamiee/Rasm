# [COMPUTE_IDENTITY]

Rasm.Compute model identity owns ONNX provenance and the content address every downstream cache key, receipt, and claim derives from. `ModelIdentity` carries the checksum, recursive `SlotShape` schema trees, the `Provenance` producer/domain/graph/description block, and the `CustomMetadata`/`Initializers` self-description channels; `ModelSource` folds five acquisition cases to one byte admission through `Acquire` and projects the receipt source string through `Origin`; `ModelFingerprint` owns the length-framed ordinal-keyvalue projection composed by the execution-provider axis; `GraduationEnvelope` owns serving-population drift admission and evaluation over the graduation evidence every offline-learned model crosses with; `GraduationEvidence` owns the reverse descriptor bundle that same seam carries back. Admission settles the model, input, initializer, drift, and descriptor contracts once.

Identity derives from the model bytes through the kernel seed-zero `XxHash128` entry `Rasm.Domain.ContentHash.Of`, the workspace's one hasher — shared with the geometry `GeometryHash`, the seam `ContentAddress`, and the Persistence `ArtifactIndexRow`/`ModelResultIndex` spine — while `ModelFingerprint` rides `System.IO.Hashing` `XxHash3`; slot schema reads `Microsoft.ML.OnnxRuntime` `InferenceSession` metadata; `ModelLoad` rides the `ComputeReceipt` rail; the descriptor bundle rides `System.Text.Json` under the injected `JsonTypeInfo<GraduationEvidence>` contract. `NodaTime` `Instant`, the kernel `CorrelationId` and `ReceiptSinkPort` (`Rasm/Domain/telemetry#CAUSAL_FRAME`), the `Runtime/receipts#RECEIPT_UNION` spine and its `ComputeWireContext` Strict resolver, the `Runtime/admission#SUBSTRATE_AXIS` `Substrate` axis beside the spine `WorkLane` roster, and the Persistence `ArtifactIndexRow` arrive settled. `ModelIdentity`/`ModelFingerprint`/`Slot` cross to `Model/sessions#SESSION_CAPSULE`, `Model/providers#EP_AXIS`, `Model/inference#INFERENCE_MODES`, and `Model/generative#GENERATIVE_RUN` as settled vocabulary, `Checksum` is the deterministic cache and result-key seed `Model/inference#RESULT_CACHE` consumes, and `DriftVerdict.Breached` is the reuse-invalidation signal that same cache consumes as an `EquivalenceMiss` fault.

## [01]-[INDEX]

- [02]-[MODEL_IDENTITY]: checksum identity; five-case acquisition union with the byte-resolution fold; kind-discriminated schema snapshot with provenance; admission over input slots and overridable initializers; custom-metadata self-description; shared length-framed ordinal-keyvalue fingerprint; the graduation drift sentinel over its numeric and categorical band cases; `ModelLoad` receipt mint.
- [03]-[GRADUATION_EVIDENCE]: the scalar-kind vocabulary and recursive `FieldNode` descriptor union with its locked kind literals, the owner-descriptor roster, and the bundle whose admission proves well-formedness, reference resolution, and an acyclic owner graph before its content-keyed bytes leave.

## [02]-[MODEL_IDENTITY]

- Owner: `ModelIdentity` identity record with nested `Slot` rows over recursive `SlotShape` tensor, sparse-tensor, sequence, map, and optional cases; `Provenance` owns the producer/domain/graph/description block and `CustomMetadata`/`Initializers` the self-description channels; `ModelSource` `[Union]` owns five acquisition cases whose `Acquire` fold resolves each through injected `SourceResolver` ports and whose `Origin` projects the receipt source class; `ModelFingerprint` owns the length-framed ordinal-keyvalue projection composed by `ExecutionProvider.ResultKey`; `GraduationEnvelope` owns the admitted per-feature `Band` roster and its `Observe` projection onto one reference/observed mass pair, `DriftStatistic` owns the score over that pair, `DriftPolicy` owns the statistic row beside its ordered thresholds and sampling floors, `FeatureSample` carries one serving window per feature, and `DriftReport` carries the per-feature verdict set with its headline and uncovered roster.
- Cases: `ModelSource` cases `LocalFile`, `EmbeddedResource`, `PersistenceBlob`, `RemoteFetch`, `Buffer`; `SlotShape` cases `Tensor`, `SparseTensor`, `Sequence`, `Map`, `Optional`; `Band` and `FeatureSample` cases `Numeric`, `Categorical`; `DriftVerdict` cases `Stable`, `Drifting`, `Breached`; `DriftStatistic` rows `psi`.
- Entry: `public static Fin<ModelIdentity> Snapshot(ModelSource source, ReadOnlySpan<byte> bytes, InferenceSession session, Instant at)` — metadata topology and model bytes admit together; identity derives from the bytes.
- Auto: `Snapshot` traverses input, output, and overridable-initializer metadata into recursive `SlotShape` trees; the three independent slot sets lift to K-kinded `Validation<Error>` legs, accumulate through tuple `Apply`, and rejoin `Fin<ModelIdentity>` once, so simultaneous schema faults survive one admission. Unknown ONNX kinds fault their slot instead of entering a dtype/default ghost. `Acquire` folds five source cases through `Try.lift(...).Run().Bind(identity)`; typed resolver faults survive while throwing file/resource boundaries become `ModelRejected`. `Accepts` requires every non-`Optional` input exactly once — dense and sparse leaves gate dtype, rank, and fixed extents, sequence/map slots conform by name and their payload proves structurally at run against the closed `Model/extension#EXTENSION_OPS` coverage, `Optional` unwraps to its element rule when bound and may be absent — and rejects unknown names, negative offered extents, and duplicates; `Initializer` applies the dense-tensor discriminant and exact-shape gate. `ModelFingerprint.Of` length-frames each ordinal key and value through one disposed incremental `XxHash3`. `GraduationEnvelope.Admit` rejects a blank or duplicated feature, a non-normalized mass vector, and — per case — non-finite or non-monotonic bin edges, a mis-sized mass vector, and a blank or duplicated category. `Drift` pairs each band with the serving window naming it, scores the COVERED pairs alone and names the rest, and `Band.Observe` projects each pair onto one reference/observed mass vector — a numeric window bisects every value onto its bin over the sorted edges, a categorical window tallies every label onto its category and routes each unrostered label to the appended unseen bucket — which `DriftPolicy.Statistic` then grades. Per-feature verdicts ACCUMULATE, so one undersized window refuses alone rather than hiding every other feature's score.
- Receipt: `ModelLoad` — the `Runtime/receipts#RECEIPT_UNION` `ModelLoad(checksum, source, ep, version)` shape — is minted by `LoadReceipt` from this owner's `Key`, `Source.Origin`, and snapshotted `GraphVersion` with the loader's `ExecutionProvider`, correlation, work lane, substrate, and elapsed; emission rides the sink port at the composition edge. Every `Breached` drift verdict faults `ComputeFault.EquivalenceMiss` at the consuming lane — correctness gates reuse exactly as it gates session admission, and a fast stale surrogate is the worst reused object.
- Packages: Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, System.IO.Hashing, System.Text.Json, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm (project, `Domain.ContentHash`), Rasm.AppHost (project), Rasm.Persistence (project)
- Growth: a new acquisition route is one `ModelSource` case with its `Acquire` arm and `Origin` projection; a new ONNX value kind is one `SlotShape` case and one `ShapeOf` arm; a new deployment-constant binding composes `Initializer`; a new self-description axis is one `Provenance` field; a new drift statistic is one `DriftStatistic` row scoring the same reference/observed mass pair, with the policy already carrying the row; a new feature kind is one `Band` case beside its `FeatureSample` case and one `Observe` arm.
- Boundary: every downstream cache key, receipt, and claim derives from `Checksum`; `Checksum` composes the kernel seed-zero `ContentHash.Of(bytes)` owner shared with geometry, seam content addresses, and Persistence indexes. `Acquire` owns file and manifest-resource statement boundaries and brackets expected I/O faults into `ComputeFault.ModelRejected`; injected `SourceResolver` ports keep blob storage and HTTP transport outside this owner. `SlotShape` preserves the full recursive ONNX value topology: `SequenceMetadata.ElementMeta`, `OptionalMetadata.ElementMeta`, and `MapMetadata.KeyDataType`/`ValueMetadata` recurse until a dense or sparse tensor leaf. Sequence and map slots admit by NAME here because a slot describes a shape while a value carries one, and the value's structural gate is `Model/extension#EXTENSION_OPS` `Egress` — whose arm set is CLOSED over every `SlotShape` leaf this owner can snapshot, so a schema this admitter accepts has a reader at run and neither end carries a kind the other cannot. `Accepts` treats negative model dims as free axes but rejects negative offered extents and requires the complete non-`Optional` input set; `Initializer` requires an exact dense-tensor shape instead of accepting a different rank with the same element count. `CustomMetadata` mints no second identity because its bytes already participate in `Checksum`; `Initializers` remain deployment constants bound through `AddInitializer(string, OrtValue)`. `ModelLoad` carries derivable `ModelMetadata.Version` and takes its lane and substrate from the loader, because a background sweep's cold open and an interactive lease land on different lanes and the selection axis — not this owner — decides which substrate ran, so a hardwired pair publishes one lane's evidence for every load. `GraduationEnvelope` admits evidence-keyed normalized bands, scores a PARTIAL serving window rather than refusing it, and returns an evidence-bearing typed verdict per covered feature only after that feature's window crosses the policy sample floor — an uncovered band names itself on `DriftReport.Uncovered` and never folds into the headline, because a feature nobody sampled is a hole in the evidence rather than a stable one.

```csharp signature
public readonly record struct SourceResolver(
    Func<ArtifactIndexRow, Fin<ReadOnlyMemory<byte>>> Blob,
    Func<string, Fin<ReadOnlyMemory<byte>>> Remote) {
    public static readonly SourceResolver Local = new(
        static _ => Fin.Fail<ReadOnlyMemory<byte>>(new ComputeFault.ModelRejected("<no-blob-resolver>")),
        static _ => Fin.Fail<ReadOnlyMemory<byte>>(new ComputeFault.ModelRejected("<no-remote-resolver>")));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ModelSource {
    private ModelSource() { }

    public sealed record LocalFile(string Path) : ModelSource;

    public sealed record EmbeddedResource(Assembly Assembly, string Name) : ModelSource;

    public sealed record PersistenceBlob(ArtifactIndexRow Row) : ModelSource;

    public sealed record RemoteFetch(string ArtifactId) : ModelSource;

    public sealed record Buffer(ReadOnlyMemory<byte> Bytes) : ModelSource;

    public string Origin => Switch(
        localFile:        static f => $"file:{f.Path}",
        embeddedResource: static e => $"resource:{e.Assembly.GetName().Name}/{e.Name}",
        persistenceBlob:  static _ => "blob",
        remoteFetch:      static r => $"remote:{r.ArtifactId}",
        buffer:           static b => $"buffer:{b.Bytes.Length}");

    public Fin<ReadOnlyMemory<byte>> Acquire(SourceResolver resolver) {
        return Try.lift(() => Switch(
                localFile: static f => File.Exists(f.Path)
                    ? Fin.Succ((ReadOnlyMemory<byte>)File.ReadAllBytes(f.Path))
                    : Fin.Fail<ReadOnlyMemory<byte>>(new ComputeFault.ModelRejected($"<model-source-missing:file:{f.Path}>")),
                embeddedResource: static e => e.Assembly.GetManifestResourceStream(e.Name) is Stream stream
                    ? Read(stream)
                    : Fin.Fail<ReadOnlyMemory<byte>>(new ComputeFault.ModelRejected($"<model-source-missing:resource:{e.Name}>")),
                persistenceBlob: b => resolver.Blob(b.Row),
                remoteFetch:     r => resolver.Remote(r.ArtifactId),
                buffer:          static b => Fin.Succ(b.Bytes)))
            .Run()
            .MapFail(error => new ComputeFault.ModelRejected($"<model-source-error:{Origin}:{error.Message}>"))
            .Bind(identity);
    }

    static Fin<ReadOnlyMemory<byte>> Read(Stream stream) {
        using (stream) {
            if (stream.Length > Array.MaxLength) {
                return Fin.Fail<ReadOnlyMemory<byte>>(new ComputeFault.ModelRejected($"<model-source-too-large:{stream.Length}>"));
            }
            byte[] bytes = GC.AllocateUninitializedArray<byte>(checked((int)stream.Length));
            stream.ReadExactly(bytes);
            return Fin.Succ((ReadOnlyMemory<byte>)bytes);
        }
    }
}

public static class ModelFingerprint {
    // Length framing makes the preimage self-delimiting: a separator character inside a value can never shift two distinct option tables onto one fingerprint.
    public static ulong Of(IEnumerable<KeyValuePair<string, string>> rows) {
        using XxHash3 hash = new();
        foreach (KeyValuePair<string, string> row in rows.OrderBy(static row => row.Key, StringComparer.Ordinal)) {
            Framed(hash, row.Key);
            Framed(hash, row.Value);
        }
        return hash.GetCurrentHashAsUInt64();
    }

    static void Framed(XxHash3 hash, string field) {
        Span<byte> frame = stackalloc byte[4];
        byte[] bytes = Encoding.UTF8.GetBytes(field);
        BinaryPrimitives.WriteInt32LittleEndian(frame, bytes.Length);
        hash.Append(frame);
        hash.Append(bytes);
    }
}

[Union]
public abstract partial record SlotShape {
    private SlotShape() { }

    public sealed record Tensor(TensorElementType Dtype, Seq<int> Dims, Seq<string> FreeDims) : SlotShape;

    public sealed record SparseTensor(TensorElementType Dtype, Seq<int> Dims, Seq<string> FreeDims) : SlotShape;

    public sealed record Sequence(SlotShape Element) : SlotShape;

    public sealed record Map(TensorElementType KeyDtype, SlotShape Value) : SlotShape;

    public sealed record Optional(SlotShape Element) : SlotShape;
}

public sealed record ModelIdentity(
    UInt128 Checksum,
    long GraphVersion,
    Seq<ModelIdentity.Slot> Inputs,
    Seq<ModelIdentity.Slot> Outputs,
    Seq<ModelIdentity.Slot> Initializers,
    FrozenDictionary<string, string> CustomMetadata,
    ModelIdentity.Provenance Provenance,
    ModelSource Source,
    Instant AcquiredAt) {
    public sealed record Slot(string Name, SlotShape Shape);

    public sealed record Provenance(string Producer, string Domain, string GraphName, string Description, string GraphDescription);

    public string Key => $"{Checksum:x32}";

    public static Fin<ModelIdentity> Snapshot(ModelSource source, ReadOnlySpan<byte> bytes, InferenceSession session, Instant at) {
        UInt128 checksum = ContentHash.Of(bytes);
        ModelMetadata metadata = session.ModelMetadata;
        return (SlotSet(session.InputMetadata), SlotSet(session.OutputMetadata), SlotSet(session.OverridableInitializerMetadata))
            .Apply((inputs, outputs, initializers) => new ModelIdentity(
                checksum,
                metadata.Version,
                inputs,
                outputs,
                initializers,
                metadata.CustomMetadataMap.ToFrozenDictionary(StringComparer.Ordinal),
                new Provenance(metadata.ProducerName, metadata.Domain, metadata.GraphName, metadata.Description, metadata.GraphDescription),
                source,
                at))
            .As()
            .ToFin();
    }

    public Fin<Unit> Accepts(Seq<(string Name, TensorElementType Dtype, Seq<long> Shape)> binding) {
        // Completeness runs over EVERY non-Optional input slot — a required sparse, sequence, or map input
        // missing from the binding rejects; only Optional slots may be absent. Dtype/dims evidence gates the
        // dense and sparse leaves; sequence/map payloads conform by name here and admit structurally at run.
        Seq<Slot> expected = Inputs.Filter(static slot => slot.Shape is not SlotShape.Optional);
        Seq<(string Name, TensorElementType Dtype, Seq<long> Shape)> rejected = binding.Filter(b => !Inputs.Exists(slot => Conforms(slot, b)));
        Seq<string> missing = expected
            .Filter(slot => !binding.Exists(candidate => StringComparer.Ordinal.Equals(candidate.Name, slot.Name)))
            .Map(static slot => slot.Name);
        bool unique = binding.Map(static candidate => candidate.Name).ToFrozenSet(StringComparer.Ordinal).Count == binding.Count;
        return unique && missing.IsEmpty && rejected.IsEmpty
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new ComputeFault.ModelRejected(
                $"{Key}:binding:unique={unique}:missing={string.Join(',', missing)}:reject={string.Join(',', rejected.Map(static b => $"{b.Name}[{string.Join('x', b.Shape)}]"))}"));
    }

    public Fin<(string Name, OrtValue Value)> Initializer(string name, OrtValue value) {
        return value.OnnxType == OnnxValueType.ONNX_TYPE_TENSOR
            ? Try.lift(value.GetTensorTypeAndShape).Run()
                .MapFail(error => new ComputeFault.ModelRejected($"{Key}:initializer:{name}:{error.Message}"))
                .Bind(info => Initializers.Find(slot => StringComparer.Ordinal.Equals(slot.Name, name)).Case is Slot slot
                    && slot.Shape is SlotShape.Tensor tensor
                    && tensor.Dtype == info.ElementDataType
                    && DimsConform(tensor.Dims, info.Shape)
                        ? Fin.Succ((name, value))
                        : Fin.Fail<(string, OrtValue)>(new ComputeFault.ModelRejected(
                            $"{Key}:initializer:{name}:dtype={info.ElementDataType}:shape={string.Join('x', info.Shape)}")))
            : Fin.Fail<(string, OrtValue)>(new ComputeFault.ModelRejected($"{Key}:initializer:{name}:kind={value.OnnxType}"));
    }

    // Lane and substrate are the LOADER's facts, not this owner's: a warm-sweep cold open and an interactive lease
    // run on different lanes, and the substrate axis answers which route the selection chose — hardwiring either
    // publishes one call site's context on every load. `AllocationClass.NativeOrt` stays fixed because the arena a
    // session allocates from is a property of the runtime doing the loading, which this owner does know.
    public ComputeReceipt.ModelLoad LoadReceipt(
        ExecutionProvider ep, CorrelationId correlation, WorkLane lane, Substrate substrate, Duration elapsed) =>
        new(Key, Source.Origin, ep, GraphVersion) {
            Scope = new ReceiptScope.Execution(correlation, lane, substrate, AllocationClass.NativeOrt, elapsed),
        };

    static bool Conforms(Slot slot, (string Name, TensorElementType Dtype, Seq<long> Shape) binding) =>
        StringComparer.Ordinal.Equals(slot.Name, binding.Name)
        && (Unwrap(slot.Shape) switch {
            SlotShape.Tensor tensor => tensor.Dtype == binding.Dtype && binding.Shape.ForAll(static dim => dim >= 0) && DimsConform(tensor.Dims, binding.Shape),
            SlotShape.SparseTensor sparse => sparse.Dtype == binding.Dtype && binding.Shape.ForAll(static dim => dim >= 0) && DimsConform(sparse.Dims, binding.Shape),
            SlotShape.Sequence or SlotShape.Map => true,
            _ => false,
        });

    static SlotShape Unwrap(SlotShape shape) => shape is SlotShape.Optional optional ? Unwrap(optional.Element) : shape;

    static bool DimsConform(Seq<int> expected, IReadOnlyList<long> offered) =>
        expected.Count == offered.Count
        && expected.Zip(toSeq(offered)).ForAll(static pair => pair.Item1 < 0 || pair.Item1 == pair.Item2);

    static K<Validation<Error>, Seq<Slot>> SlotSet(IReadOnlyDictionary<string, NodeMetadata> nodes) =>
        toSeq(nodes.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
            .Traverse(static pair => ShapeOf(pair.Value).Map(shape => new Slot(pair.Key, shape)).ToValidation())
            .As();

    static Fin<SlotShape> ShapeOf(NodeMetadata node) => node.OnnxValueType switch {
        OnnxValueType.ONNX_TYPE_TENSOR => Fin.Succ<SlotShape>(new SlotShape.Tensor(
            node.ElementDataType, toSeq(node.Dimensions), toSeq(node.SymbolicDimensions))),
        OnnxValueType.ONNX_TYPE_SPARSETENSOR => Fin.Succ<SlotShape>(new SlotShape.SparseTensor(
            node.ElementDataType, toSeq(node.Dimensions), toSeq(node.SymbolicDimensions))),
        OnnxValueType.ONNX_TYPE_SEQUENCE => ShapeOf(node.AsSequenceMetadata().ElementMeta)
            .Map<SlotShape>(static element => new SlotShape.Sequence(element)),
        OnnxValueType.ONNX_TYPE_MAP => MapOf(node.AsMapMetadata()),
        OnnxValueType.ONNX_TYPE_OPTIONAL => ShapeOf(node.AsOptionalMetadata().ElementMeta)
            .Map<SlotShape>(static element => new SlotShape.Optional(element)),
        _ => Fin.Fail<SlotShape>(new ComputeFault.ModelRejected($"<model-slot-kind:{node.OnnxValueType}>")),
    };

    static Fin<SlotShape> MapOf(MapMetadata metadata) => ShapeOf(metadata.ValueMetadata)
        .Map<SlotShape>(value => new SlotShape.Map(metadata.KeyDataType, value));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DriftStatistic {
    // Both vectors arrive normalized and equally sized off `Band.Observe`, so a row scores two distributions and
    // re-derives no binning — which is what keeps a second statistic ONE row rather than a second fold beside the
    // band cases. The scalar walk is the named span exemption: no `TensorPrimitives` member owns the floored
    // log-ratio product, and the floor is the statistic's own numerical guard rather than a band column, because a
    // divergence with no logarithm in it needs none.
    public static readonly DriftStatistic Psi = new("psi", static (reference, observed, probabilityFloor) => {
        double psi = 0d;
        for (int bin = 0; bin < reference.Length; bin++) {
            double expected = double.Max(reference[bin], probabilityFloor);
            double actual = double.Max(observed[bin], probabilityFloor);
            psi += (actual - expected) * Math.Log(actual / expected);
        }
        return psi;
    });

    [UseDelegateFromConstructor]
    public partial double Score(ReadOnlySpan<double> reference, ReadOnlySpan<double> observed, double probabilityFloor);
}

// One serving window per feature. AEC populations are mostly CATEGORICAL — system type, material class, code
// jurisdiction — and forcing a label roster onto a numeric axis invents ordinal distance the graduation fit never
// had, so the re-coding that actually drifts a model reads as noise on a fabricated scale.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FeatureSample(string Feature) {
    public sealed record Numeric(string Feature, ReadOnlyMemory<double> Values) : FeatureSample(Feature);

    public sealed record Categorical(string Feature, Seq<string> Labels) : FeatureSample(Feature);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DriftVerdict(
    UInt128 EvidenceKey, string Feature, DriftStatistic Statistic, double Score, int SampleCount) {
    public sealed record Stable(UInt128 EvidenceKey, string Feature, DriftStatistic Statistic, double Score, int SampleCount)
        : DriftVerdict(EvidenceKey, Feature, Statistic, Score, SampleCount);

    public sealed record Drifting(UInt128 EvidenceKey, string Feature, DriftStatistic Statistic, double Score, int SampleCount)
        : DriftVerdict(EvidenceKey, Feature, Statistic, Score, SampleCount);

    public sealed record Breached(UInt128 EvidenceKey, string Feature, DriftStatistic Statistic, double Score, int SampleCount)
        : DriftVerdict(EvidenceKey, Feature, Statistic, Score, SampleCount);
}

// Per-feature verdicts, the worst as the headline every reuse gate reads, and the bands no serving window covered.
// An uncovered feature never folds into the headline: it is a hole in the evidence, and reporting it as stable is
// exactly how a drifting model keeps serving on the strength of a column nobody sampled.
public sealed record DriftReport(DriftVerdict Worst, Seq<DriftVerdict> Features, Seq<string> Uncovered);

[ComplexValueObject]
public sealed partial class DriftPolicy {
    // Thresholds are STATISTIC-RELATIVE, so the row rides the policy: a PSI-calibrated 0.25 breach means nothing to
    // a Hellinger or Jensen-Shannon row, and a threshold pair carried without the statistic that produced it grades
    // one divergence against another's calibration.
    public DriftStatistic Statistic { get; }

    public double DriftingScore { get; }

    public double BreachScore { get; }

    public int MinimumSamples { get; }

    public double ProbabilityFloor { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref DriftStatistic statistic,
        ref double driftingScore,
        ref double breachScore,
        ref int minimumSamples,
        ref double probabilityFloor) =>
        validationError = statistic is not null
            && double.IsFinite(driftingScore)
            && double.IsFinite(breachScore)
            && driftingScore >= 0.0
            && breachScore > driftingScore
            && minimumSamples > 0
            && double.IsFinite(probabilityFloor)
            && probabilityFloor > 0.0
            && probabilityFloor < 1.0
            ? null
            : new ValidationError(
                message: $"<drift-policy:{statistic?.Key}:{driftingScore}:{breachScore}:{minimumSamples}:{probabilityFloor}>");
}

public sealed record GraduationEnvelope(UInt128 EvidenceKey, Seq<GraduationEnvelope.Band> Bands) {
    // One band's reference and observed mass vectors — equal in length by construction — beside the window size that
    // produced the observed half, so the statistic reads two spans and the verdict reports what it read.
    public readonly record struct Observation(double[] Reference, double[] Observed, int SampleCount);

    // Reference mass is fitted by the Python companion at graduation and never here: a numeric band carries the
    // feature's quantile cuts with the mass per bin, a categorical band the label roster with its mass. `Observe`
    // appends ONE unseen bucket at zero reference mass to every categorical read — a label the graduation
    // population never held IS the drift signal, so refusing on it discards exactly the evidence this sentinel
    // exists to report, and the statistic's probability floor already lifts the zero it scores against.
    [Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
    public abstract partial record Band(string Feature) {
        public sealed record Numeric(string Feature, Seq<double> BinEdges, Seq<double> BinMass) : Band(Feature);

        public sealed record Categorical(string Feature, Seq<string> Categories, Seq<double> Mass) : Band(Feature);

        public Fin<Observation> Observe(FeatureSample window, int minimumSamples) => Switch(
            state: (Window: window, Floor: minimumSamples),
            numeric: static (at, band) => at.Window is FeatureSample.Numeric values
                ? Binned(band, values, at.Floor)
                : Mismatched(band.Feature),
            categorical: static (at, band) => at.Window is FeatureSample.Categorical labels
                ? Tallied(band, labels, at.Floor)
                : Mismatched(band.Feature));

        public bool Wellformed(Seq<Band> roster) =>
            !string.IsNullOrWhiteSpace(Feature)
            && roster.Count(candidate => StringComparer.Ordinal.Equals(candidate.Feature, Feature)) == 1
            && Switch(
                numeric: static band => band.BinMass.Count == band.BinEdges.Count + 1
                    && band.BinEdges.ForAll(double.IsFinite)
                    && band.BinEdges.Zip(band.BinEdges.Tail).ForAll(static pair => pair.Item1 < pair.Item2)
                    && Normalized(band.BinMass),
                categorical: static band => !band.Categories.IsEmpty
                    && band.Categories.Count == band.Mass.Count
                    && band.Categories.ForAll(static category => !string.IsNullOrWhiteSpace(category))
                    && band.Categories.ToFrozenSet(StringComparer.Ordinal).Count == band.Categories.Count
                    && Normalized(band.Mass));

        static Fin<Observation> Binned(Numeric band, FeatureSample.Numeric window, int minimumSamples) {
            ReadOnlySpan<double> values = window.Values.Span;
            if (values.Length < minimumSamples || !TensorPrimitives.IsFiniteAll(values)) {
                return Fin.Fail<Observation>(new ComputeFault.ModelRejected($"<drift-sample:{band.Feature}:{values.Length}>"));
            }
            ReadOnlySpan<double> edges = band.BinEdges.AsSpan();
            double[] observed = new double[band.BinMass.Count];
            foreach (double value in values) { observed[Bin(edges, value)] += 1d; }
            return Fin.Succ(new Observation(band.BinMass.ToArray(), Normalize(observed, values.Length), values.Length));
        }

        static Fin<Observation> Tallied(Categorical band, FeatureSample.Categorical window, int minimumSamples) {
            if (window.Labels.Count < minimumSamples) {
                return Fin.Fail<Observation>(new ComputeFault.ModelRejected($"<drift-sample:{band.Feature}:{window.Labels.Count}>"));
            }
            FrozenDictionary<string, int> slots = band.Categories
                .Map(static (category, index) => KeyValuePair.Create(category, index))
                .ToFrozenDictionary(StringComparer.Ordinal);
            double[] observed = new double[band.Categories.Count + 1];
            foreach (string label in window.Labels) {
                observed[slots.TryGetValue(label, out int slot) ? slot : band.Categories.Count] += 1d;
            }
            return Fin.Succ(new Observation(
                [.. band.Mass, 0d], Normalize(observed, window.Labels.Count), window.Labels.Count));
        }

        // A band whose case disagrees with its window's is a CALLER defect, never a numeric one: scoring a label
        // roster against quantile cuts would answer a number for a comparison nothing performed.
        static Fin<Observation> Mismatched(string feature) =>
            Fin.Fail<Observation>(new ComputeFault.ModelRejected($"<drift-window-kind:{feature}>"));

        // Edges are SORTED by admission, so the bin is a bisection: a miss answers the complement of its insertion
        // point, and a hit sits in the bin ABOVE the edge because binning is half-open from the left. The linear
        // edge count this replaces re-walked every edge for every value in the window.
        static int Bin(ReadOnlySpan<double> edges, double value) {
            int probe = edges.BinarySearch(value);
            return probe >= 0 ? probe + 1 : ~probe;
        }

        static double[] Normalize(double[] counts, int total) {
            TensorPrimitives.Divide(counts, total, counts);
            return counts;
        }

        static bool Normalized(Seq<double> mass) =>
            mass.ForAll(static value => double.IsFinite(value) && value > 0.0)
            && Math.Abs(mass.Fold(0.0, static (sum, value) => sum + value) - 1.0) <= 1e-9;
    }

    public static Fin<GraduationEnvelope> Admit(UInt128 evidenceKey, Seq<Band> bands) =>
        guard(
            evidenceKey != UInt128.Zero && !bands.IsEmpty && bands.ForAll(band => band.Wellformed(bands)),
            new ComputeFault.ModelRejected($"<graduation-envelope:{evidenceKey:x32}>"))
        .ToFin()
        .Map(_ => new GraduationEnvelope(evidenceKey, bands));

    // Coverage is PARTIAL by design — a caller samples the features it observed — so the covered pairs score and the
    // rest are NAMED. Verdicts accumulate rather than short-circuit: a monadic fold stops at the first undersized
    // window and hides every other feature's score behind it, which is how a breach on the third feature goes
    // unreported for as long as the first stays thin.
    public Fin<DriftReport> Drift(Seq<FeatureSample> serving, DriftPolicy policy) {
        bool unique = serving.Map(static window => window.Feature).ToFrozenSet(StringComparer.Ordinal).Count == serving.Count;
        Seq<(Band Band, Option<FeatureSample> Window)> paired = Bands.Map(band =>
            (Band: band, Window: serving.Find(window => StringComparer.Ordinal.Equals(window.Feature, band.Feature))));
        Seq<string> uncovered = paired.Filter(static row => row.Window.IsNone).Map(static row => row.Band.Feature);
        Seq<(Band Band, FeatureSample Window)> covered =
            paired.Choose(static row => row.Window.Map(window => (row.Band, Window: window)));
        return guard(
                unique && !covered.IsEmpty,
                new ComputeFault.ModelRejected(
                    $"<drift-population:unique={unique}:covered={covered.Count}:bands={Bands.Count}>"))
            .ToFin()
            .Bind(_ => covered.Traverse(row => Verdict(row.Band, row.Window, policy).ToValidation()).As().ToFin())
            .Map(verdicts => new DriftReport(
                verdicts.Reduce(static (worst, verdict) => verdict.Score > worst.Score ? verdict : worst),
                verdicts,
                uncovered));
    }

    Fin<DriftVerdict> Verdict(Band band, FeatureSample window, DriftPolicy policy) =>
        band.Observe(window, policy.MinimumSamples)
            .Map(observed => Graded(
                band.Feature,
                policy,
                policy.Statistic.Score(observed.Reference, observed.Observed, policy.ProbabilityFloor),
                observed.SampleCount));

    // Thresholds are POLICY VALUES rather than patterns, so the joint discriminant is the pair of relational
    // answers: one tuple pattern, one dispatch level, and a fourth severity is one more arm instead of a deeper
    // conditional chain.
    DriftVerdict Graded(string feature, DriftPolicy policy, double score, int samples) =>
        (Breach: score >= policy.BreachScore, Drifting: score >= policy.DriftingScore) switch {
            (true, _) => new DriftVerdict.Breached(EvidenceKey, feature, policy.Statistic, score, samples),
            (_, true) => new DriftVerdict.Drifting(EvidenceKey, feature, policy.Statistic, score, samples),
            _ => new DriftVerdict.Stable(EvidenceKey, feature, policy.Statistic, score, samples),
        };
}
```

## [03]-[GRADUATION_EVIDENCE]

- Owner: `FieldScalar` `[SmartEnum<string>]` is the closed wire-primitive vocabulary; `FieldNode` `[Union]` is the recursive descriptor tree whose six cases carry every composite shape a C# owner projects, each nesting the root so depth growth stays case-owned; `OwnerDescriptor` names one owner and its ordered field roster; `GraduationEvidence` is the versioned bundle carrying the roster with the content key its own canonical projection mints.
- Cases: `FieldNode` cases `Scalar` (one `FieldScalar` leaf), `Array` (one element node), `Nested` (one owner-name reference), `Mapping` (key and value nodes), `Optional` (one element node), `UnionCase` (a non-empty member roster); `FieldScalar` rows `i32`, `i64`, `f64`, `bool`, `string`, `key`, `bytes`, `decimal`.
- Law: kind literals are the DECODE contract. The companion projector selects its leaf case on the `kind` discriminator alone, and the union generator emits no JSON support of any kind — no converter, no derived-type roster — so a case crossing without its `[JsonDerivedType]` row serializes as the abstract base, one empty object per case, with no decode refusal on either end. The roster is hand-declared on the union declaration and the literals are frozen: renaming a case is free, renaming a literal is a wire break.
- Law: the bundle is OFFLINE at rest and reaches no gRPC leg. It crosses as bytes the app root writes through the Persistence object lane exactly as a warm artifact does, so the `Runtime/wire#PROTO_VOCABULARY` descriptor set never grows a message for it and the `Runtime/wire#CONTRACT_EVOLUTION` additive-only guard has nothing to police here — a wire the channel never carries cannot drift a channel contract.
- Law: admission proves what the far end can only fail on. `Nested.Ref` names a declared owner and the owner graph is ACYCLIC, because the projector builds each struct against already-registered siblings — an unresolved reference is an unbound name at class creation there and a back edge is a topological refusal, both after the bytes already shipped. `UnionCase.Members` is non-empty for the same reason: the projector's member fold reduces from its first element.
- Entry: `public static Fin<GraduationEvidence> Admit(Seq<OwnerDescriptor> owners)` — the caller supplies the roster and the bundle mints its own `SchemaVersion` and `BundleKey`, so neither is a claim a caller can spell wrong. `public Fin<ReadOnlyMemory<byte>> Bundle(JsonTypeInfo<GraduationEvidence> contract)` writes the canonical UTF-8 payload under an injected contract.
- Auto: `Admit` refuses an empty roster, a blank or duplicated owner name, a blank or duplicated field name within one owner, an unsound node anywhere in a tree, an unresolvable `Nested.Ref`, and a cyclic owner graph proved by peeling every reference-free owner until either the graph empties or a pass settles nothing. `Render` is the one catamorphism over the tree: it feeds the length-framed preimage `ContentHash.Of` keys, so bundle identity and the shape it describes cannot disagree, and a field-order or scalar-row change re-keys the bundle the companion pins its round-trip against.
- Receipt: none — the bundle is an artifact, not a measured run, and its content key is the identity the writing composition indexes it under.
- Packages: System.Text.Json, System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm (project, `Domain.ContentHash`), BCL inbox
- Growth: a new wire primitive is one `FieldScalar` row the projector absorbs with one table row of its own; a new composite shape is one `FieldNode` case with its `[JsonDerivedType]` literal, one `Render` arm, one `Sound` arm, and one `Refs` arm; a new bundle column is one record field beside a `SchemaVersion` bump at both ends in one change.
- Boundary: this owner mints descriptors and never decodes them — the projection back into typed stubs is the companion's, and nothing here imports a peer-runtime shape. Serialization rides the `Runtime/receipts#RECEIPT_UNION` `ComputeWireContext` Strict resolver through an INJECTED `JsonTypeInfo<GraduationEvidence>`, so the LanguageExt carrier factory that populates every `Seq<T>` column registers once at that owner and this page holds no serializer, no options handle, and no second context. `SchemaVersion` is a gate rather than a column a caller fills: a bundle outside the carried set rails at the companion's decode band, so minting one outside it here would ship bytes guaranteed to refuse. The seam is the branch `[GRADUATION]` edge and it is REVERSE-ONLY from this end — the forward leg is the companion's handoff axis, this leg is the evidence answering it, and neither end references the other's types.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FieldScalar {
    public static readonly FieldScalar I32 = new("i32");
    public static readonly FieldScalar I64 = new("i64");
    public static readonly FieldScalar F64 = new("f64");
    public static readonly FieldScalar Bool = new("bool");
    // `Text`, not `String`: a row field spelled `String` shadows the `System.String` simple name for every member of
    // this class, the same interior-versus-wire split the stage record takes on its `Pad` column.
    public static readonly FieldScalar Text = new("string");
    public static readonly FieldScalar Key = new("key");
    public static readonly FieldScalar Bytes = new("bytes");
    public static readonly FieldScalar Decimal = new("decimal");
}

// Recursion is CASE-OWNED — every composite case holds a root-typed child — so a deeper shape costs no consumer a
// dispatch edit and the generated `Switch` stays total at every depth. `Name` threads through the base positional
// column each case passes, because a base member computed over same-named case payloads suppresses the case's own
// property synthesis and then recurses at first read.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(Scalar), "scalar")]
[JsonDerivedType(typeof(Array), "array")]
[JsonDerivedType(typeof(Nested), "nested")]
[JsonDerivedType(typeof(Mapping), "mapping")]
[JsonDerivedType(typeof(Optional), "optional")]
[JsonDerivedType(typeof(UnionCase), "union")]
public abstract partial record FieldNode(string Name) {
    public sealed record Scalar(string Name, FieldScalar Kind) : FieldNode(Name);

    public sealed record Array(string Name, FieldNode Element) : FieldNode(Name);

    public sealed record Nested(string Name, string Ref) : FieldNode(Name);

    public sealed record Mapping(string Name, FieldNode Key, FieldNode Value) : FieldNode(Name);

    public sealed record Optional(string Name, FieldNode Element) : FieldNode(Name);

    public sealed record UnionCase(string Name, Seq<FieldNode> Members) : FieldNode(Name);

    // ONE catamorphism over the tree. The content-key preimage reads it, so the identity a bundle publishes and the
    // shape it describes are one derivation and a scalar-row or member-order change re-keys the bundle.
    public string Render() => Switch(
        scalar:    static node => $"{node.Name}:{node.Kind.Key}",
        array:     static node => $"{node.Name}:array<{node.Element.Render()}>",
        nested:    static node => $"{node.Name}:nested<{node.Ref}>",
        mapping:   static node => $"{node.Name}:mapping<{node.Key.Render()},{node.Value.Render()}>",
        optional:  static node => $"{node.Name}:optional<{node.Element.Render()}>",
        unionCase: static node => $"{node.Name}:union<{string.Join('|', node.Members.Map(static member => member.Render()))}>");

    public bool Sound() =>
        !string.IsNullOrWhiteSpace(Name)
        && Switch(
            scalar:    static _ => true,
            array:     static node => node.Element.Sound(),
            nested:    static node => !string.IsNullOrWhiteSpace(node.Ref),
            mapping:   static node => node.Key.Sound() && node.Value.Sound(),
            optional:  static node => node.Element.Sound(),
            unionCase: static node => !node.Members.IsEmpty && node.Members.ForAll(static member => member.Sound()));

    public Seq<string> Refs() => Switch(
        scalar:    static _ => Seq<string>(),
        array:     static node => node.Element.Refs(),
        nested:    static node => Seq(node.Ref),
        mapping:   static node => node.Key.Refs() + node.Value.Refs(),
        optional:  static node => node.Element.Refs(),
        unionCase: static node => node.Members.Bind(static member => member.Refs()));
}

// --- [MODELS] --------------------------------------------------------------------------

public sealed record OwnerDescriptor(string Name, Seq<FieldNode> Fields);

public sealed record GraduationEvidence(string SchemaVersion, Seq<OwnerDescriptor> Owners, UInt128 BundleKey) {
    // The projector CARRIES exactly this version and rails on anything else rather than best-effort decoding a
    // drifted shape, so a bundle minted outside it would ship bytes guaranteed to refuse.
    public const string Schema = "1";

    public static Fin<GraduationEvidence> Admit(Seq<OwnerDescriptor> owners) =>
        guard(
            !owners.IsEmpty
            && owners.ForAll(static owner => !string.IsNullOrWhiteSpace(owner.Name))
            && owners.Map(static owner => owner.Name).ToFrozenSet(StringComparer.Ordinal).Count == owners.Count
            && owners.ForAll(static owner =>
                owner.Fields.Map(static field => field.Name).ToFrozenSet(StringComparer.Ordinal).Count == owner.Fields.Count
                && owner.Fields.ForAll(static field => field.Sound())),
            new ComputeFault.ModelRejected($"<graduation-owners:{owners.Count}>"))
        .ToFin()
        .Bind(_ => Resolvable(owners))
        .Map(_ => new GraduationEvidence(Schema, owners, KeyOf(owners)));

    public Fin<ReadOnlyMemory<byte>> Bundle(JsonTypeInfo<GraduationEvidence> contract) =>
        Try.lift(() => (ReadOnlyMemory<byte>)JsonSerializer.SerializeToUtf8Bytes(this, contract))
            .Run()
            .MapFail(static error => new ComputeFault.ModelRejected($"<graduation-bundle:{error.Message}>"));

    // Owner graph is a DAG by contract: the projector registers each struct against already-built siblings, so a
    // reference naming no owner is an unbound name and a back edge is a topological refusal — both AFTER the bytes
    // shipped. Peeling settles every reference-free owner per pass, and a pass settling nothing over a non-empty
    // remainder IS the cycle, with no visitor state and no recursion depth to bound.
    static Fin<Unit> Resolvable(Seq<OwnerDescriptor> owners) {
        FrozenSet<string> declared = owners.Map(static owner => owner.Name).ToFrozenSet(StringComparer.Ordinal);
        Seq<string> unbound = owners
            .Bind(static owner => owner.Fields.Bind(static field => field.Refs()))
            .Distinct()
            .Filter(reference => !declared.Contains(reference));
        Seq<string> cyclic = Peel(owners.Map(owner => (
            Owner: owner.Name,
            Refs: owner.Fields.Bind(static field => field.Refs()).Distinct().Filter(declared.Contains))));
        return unbound.IsEmpty && cyclic.IsEmpty
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new ComputeFault.ModelRejected(
                $"<graduation-owner-graph:unbound={string.Join(',', unbound)}:cyclic={string.Join(',', cyclic)}>"));
    }

    static Seq<string> Peel(Seq<(string Owner, Seq<string> Refs)> graph) =>
        graph.Filter(static row => row.Refs.IsEmpty) switch {
            var settled when settled.IsEmpty => graph.Map(static row => row.Owner),
            var settled => Peel(graph
                .Filter(row => !settled.Exists(seated => StringComparer.Ordinal.Equals(seated.Owner, row.Owner)))
                .Map(row => (
                    row.Owner,
                    Refs: row.Refs.Filter(reference =>
                        !settled.Exists(seated => StringComparer.Ordinal.Equals(seated.Owner, reference)))))),
        };

    // Length framing makes the preimage self-delimiting for the same reason the option fingerprint frames: a
    // separator inside an owner or field name can never shift two distinct rosters onto one key.
    static UInt128 KeyOf(Seq<OwnerDescriptor> owners) {
        ArrayBufferWriter<byte> preimage = new();
        Framed(preimage, Schema);
        owners.Iter(owner => {
            Framed(preimage, owner.Name);
            owner.Fields.Iter(field => Framed(preimage, field.Render()));
        });
        return ContentHash.Of(preimage.WrittenSpan);
    }

    static void Framed(ArrayBufferWriter<byte> preimage, string value) {
        int bytes = Encoding.UTF8.GetByteCount(value);
        BinaryPrimitives.WriteInt32LittleEndian(preimage.GetSpan(4), bytes);
        preimage.Advance(4);
        preimage.Advance(Encoding.UTF8.GetBytes(value, preimage.GetSpan(bytes)));
    }
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
