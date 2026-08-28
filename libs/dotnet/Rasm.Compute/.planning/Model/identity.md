# [COMPUTE_IDENTITY]

Rasm.Compute model identity owns ONNX provenance and the content address every downstream cache key, result, and claim derives from. `ModelIdentity` carries checksum, `SlotShape` schema trees, `Provenance`, and the `CustomMetadata`/`Initializers` self-description channels; `ModelSource` folds five acquisition cases to one byte admission; `RosterFingerprint` owns the execution-provider-composed fingerprint; `GraduationEnvelope` and `GraduationEvidence` own the graduation boundary's forward drift admission and reverse descriptor bundle. Admission settles the model, input, initializer, drift, and descriptor contracts once.

Identity derives from the model bytes through the kernel seed-zero `XxHash128` entry `Rasm.Domain.ContentHash.Of`, the workspace's one hasher, and `RosterFingerprint` projects that same digest's LOW lane through the kernel `CanonicalWriter`, so this package carries ONE hash family; slot schema reads `Microsoft.ML.OnnxRuntime` `InferenceSession` metadata; the descriptor bundle rides `System.Text.Json` under the injected `JsonTypeInfo<GraduationEvidence>` contract. `NodaTime` `Instant`, the kernel `CorrelationId` (`Rasm/Domain/frame#SOURCE`), the `Runtime/admission#SUBSTRATE_AXIS` `Substrate` axis, and the Persistence `ArtifactIndexRow` arrive settled. `ModelIdentity`/`RosterFingerprint`/`Slot` cross to `Model/sessions#SESSION_CAPSULE`, `Model/providers#EP_AXIS`, `Model/run#RUN_MODES`, and `Model/generative#GENERATIVE_RUN` as settled vocabulary, `Checksum` is the deterministic cache and result-key seed `Model/run#RESULT_CACHE` consumes, and a `DriftVerdict` whose `Severity.Invalidates` answers true is the reuse-invalidation signal that same cache consumes as an `EquivalenceMiss` fault.

## [01]-[INDEX]

- [02]-[MODEL_IDENTITY]: checksum identity; five-case acquisition union with the byte-resolution fold; kind-discriminated schema snapshot with provenance; admission over input slots and overridable initializers; custom-metadata self-description; shared ordinal-keyvalue fingerprint over the kernel canonical writer; the graduation drift sentinel over its numeric and categorical band cases.
- [03]-[GRADUATION_EVIDENCE]: scalar-kind vocabulary; recursive `FieldNode` descriptor union with locked kind literals; owner-descriptor roster; bundle admission proving well-formedness, reference resolution, and an acyclic owner graph before its content-keyed bytes leave.

## [02]-[MODEL_IDENTITY]

- Owner: `ModelIdentity` identity record with nested `Slot` rows over recursive `SlotShape` tensor, sparse-tensor, sequence, map, and optional cases; `Provenance` owns the producer/domain/graph/description block and `CustomMetadata`/`Initializers` the self-description channels; `ModelSource` `[Union]` owns five acquisition cases whose `Acquire` fold resolves each through injected `SourceResolver` ports and whose `Origin` projects the result source class; `RosterFingerprint` owns the ordinal-keyvalue projection over the kernel canonical writer composed by `ExecutionProvider.ResultKey`; `IdentityRefusal` names this owner's shared contract refusals without a string-key roster; `GraduationEnvelope` owns the admitted per-feature `Band` roster and its `Observe` projection onto one reference/observed mass pair, `DriftStatistic` owns the score over that pair, `DriftPolicy` owns the statistic row beside its ordered thresholds and sampling floors, `FeatureSample` carries one serving window per feature, and `DriftReport` carries the per-feature verdict set with its headline and uncovered roster.
- Cases: `ModelSource` cases `LocalFile`, `EmbeddedResource`, `PersistenceBlob`, `RemoteFetch`, `Buffer`; `SlotShape` cases `Tensor`, `SparseTensor`, `Sequence`, `Map`, `Optional`; `Band` and `FeatureSample` cases `Numeric`, `Categorical`; `DriftSeverity` rows `stable`, `drifting`, `breached`, each carrying the threshold predicate that elects it and its reuse-invalidation posture; `DriftStatistic` rows `psi`.
- Entry: `public static Fin<ModelIdentity> Snapshot(ModelSource source, ReadOnlySpan<byte> bytes, InferenceSession session, Instant at)` — metadata topology and model bytes admit together; identity derives from the bytes. `GraduationEnvelope.Admit(HdfHandle)` is the FORWARD graduation ingest — one `Runtime/archive#HDF_ARCHIVE` open per admission job reads the h5py-written `/bands/<feature>` roster (`kind` attribute selects the case, `edges`/`mass`/`categories` datasets read under declared selections, the evidence key an attribute, `LinkExists` proving the roster before any resolve) and re-enters the roster `Admit`, so every Wellformed gate reruns on read bands; the reverse JSON `GraduationEvidence` leg keeps its own container untouched.
- Auto: `Acquire` captures the source fold through `Try.lift`; typed resolver faults survive and throwing file or resource boundaries remain the original `Error`.
- Result: `ModelIdentity` carries the checksum, source, graph version, slots, provenance, metadata, and fingerprint obtained from the loaded model. Every drift verdict whose severity `Invalidates` faults `ComputeFault.EquivalenceMiss` at the consuming lane — correctness gates reuse exactly as it gates session admission, and a fast stale surrogate is the worst reused object.
- Packages: Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, System.IO.Hashing, System.Text.Json, PureHDF (`NativeGroup`, `IH5Object.Name`/`Attribute`/`Children`, `NativeDataset.Read<T>(H5DatasetAccess, Span<T>, …)`, `HyperslabSelection`), Generator.Equals (`[Equatable]`, `[OrderedEquality]`, `[UnorderedEquality]`, `[IgnoreEquality]`), QuikGraph (`AlgorithmExtensions.IsDirectedAcyclicGraph(IEnumerable<TEdge>)`, `SEquatableEdge<T>`), NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm (project, `Domain.ContentHash`/`CanonicalWriter`/`Lane`), Rasm.AppHost (project), Rasm.Persistence (project, `ContentBlobPort`, `ArtifactIndexRow`)
- Growth: a new acquisition route is one `ModelSource` case with its `Acquire` arm and `Origin` projection; a new ONNX value kind is one `SlotShape` case and one `ShapeOf` arm; a new deployment-constant binding composes `Initializer`; a new self-description axis is one `Provenance` field; a new drift statistic is one `DriftStatistic` row scoring the same reference/observed mass pair, with the policy already carrying the row; a new severity band is one `DriftSeverity` row carrying its own election predicate and invalidation posture, and no consumer switch moves; a new feature kind is one `Band` case beside its `FeatureSample` case and one `Observe` arm.
- Boundary: every downstream cache key, result, and claim derives from `Checksum`; `Checksum` composes the kernel seed-zero `ContentHash.Of(bytes)` owner shared with geometry, contract content addresses, and Persistence indexes. `Acquire` owns file and manifest-resource statement boundaries and brackets expected I/O faults into one named `IdentityRefusal`; the injected `SourceResolver` keeps blob storage and HTTP transport outside this owner, and its blob route is the Persistence `Store/blobstore#OBJECT_STORE` `ContentBlobPort` itself rather than a page-local key-to-bytes delegate — one interface, bound by the composition root, adapted from its `IO` effect onto this page's `Fin` result at its single use site. `SlotShape` preserves the full recursive ONNX value topology: `SequenceMetadata.ElementMeta`, `OptionalMetadata.ElementMeta`, and `MapMetadata.KeyDataType`/`ValueMetadata` recurse until a dense or sparse tensor leaf. Sequence and map slots admit by NAME here because a slot describes a shape while a value carries one, and the value's structural gate is `Model/extension#EXTENSION_OPS` `Egress` — whose arm set is CLOSED over every `SlotShape` leaf this owner can snapshot, so a schema this admitter accepts has a reader at run and neither end carries a kind the other cannot. `Accepts` treats negative model dims as free axes but rejects negative offered extents and requires the complete non-`Optional` input set; `Initializer` requires an exact dense-tensor shape instead of accepting a different rank with the same element count. `CustomMetadata` mints no second identity because its bytes already participate in `Checksum`; `Initializers` remain deployment constants bound through `AddInitializer(string, OrtValue)`. `GraduationEnvelope` admits evidence-keyed normalized bands, scores a PARTIAL serving window rather than refusing it, and returns an evidence-bearing typed verdict per covered feature only after that feature's window crosses the policy sample floor — an uncovered band names itself on `DriftReport.Uncovered` and never folds into the headline, because a feature nobody sampled is a hole in the evidence rather than a stable one.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

public static class IdentityRefusal {
    public static readonly ContractRefusal SourceUnresolved = new(ComputeArea.Model, ComputeContract.Complete);
    public static readonly ContractRefusal SourceOversized = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal SlotKindUnmodelled = new(ComputeArea.Model, ComputeContract.Compatible);
    public static readonly ContractRefusal InitializerUnconformable = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal DriftWindowUndersized = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal DriftWindowMiskinded = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal DriftPopulationRejected = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal BandMalformed = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal EnvelopeMalformed = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal ArchiveUnreadable = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal OwnerRosterMalformed = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal OwnerGraphUnresolvable = new(ComputeArea.Model, ComputeContract.Valid);

}

public readonly record struct SourceResolver(
    Option<ContentBlobPort> Blob,
    Func<string, Fin<ReadOnlyMemory<byte>>> Remote) {
    public static readonly SourceResolver Local = new(
        None,
        static _ => Fin.Fail<ReadOnlyMemory<byte>>(IdentityRefusal.SourceUnresolved.Fault()));
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
                    : Fin.Fail<ReadOnlyMemory<byte>>(IdentityRefusal.SourceUnresolved.Fault()),
                embeddedResource: static e => e.Assembly.GetManifestResourceStream(e.Name) is Stream stream
                    ? Read(stream)
                    : Fin.Fail<ReadOnlyMemory<byte>>(IdentityRefusal.SourceUnresolved.Fault()),
                persistenceBlob: b => resolver.Blob
                    .ToFin((Error)IdentityRefusal.SourceUnresolved.Fault())
                    .Bind(port => port.Get(b.Row.Content).Try().Run()),
                remoteFetch:     r => resolver.Remote(r.ArtifactId),
                buffer:          static b => Fin.Succ(b.Bytes))).Run().Bind(static inner => inner);
    }

    static Fin<ReadOnlyMemory<byte>> Read(Stream stream) {
        using (stream) {
            if (stream.Length > Array.MaxLength) {
                return Fin.Fail<ReadOnlyMemory<byte>>(IdentityRefusal.SourceOversized.Fault());
            }
            byte[] bytes = GC.AllocateUninitializedArray<byte>(checked((int)stream.Length));
            stream.ReadExactly(bytes);
            return Fin.Succ((ReadOnlyMemory<byte>)bytes);
        }
    }
}

public static class RosterFingerprint {
    public static ulong Of(IEnumerable<KeyValuePair<string, string>> rows) => ContentHash.Halves(
        ContentHash.Of(toSeq(rows), static (roster, writer) => writer.Sorted(
            roster,
            static row => row.Key,
            StringComparer.Ordinal,
            static (row, framed) => framed.String(row.Key).String(row.Value)))).Low;
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

[Equatable]
public sealed partial record ModelIdentity(
    UInt128 Checksum,
    long GraphVersion,
    [property: OrderedEquality] Seq<ModelIdentity.Slot> Inputs,
    [property: OrderedEquality] Seq<ModelIdentity.Slot> Outputs,
    [property: OrderedEquality] Seq<ModelIdentity.Slot> Initializers,
    [property: UnorderedEquality] FrozenDictionary<string, string> CustomMetadata,
    ModelIdentity.Provenance Provenance,
    [property: IgnoreEquality] ModelSource Source,
    [property: IgnoreEquality] Instant AcquiredAt) {
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
        HashMap<string, Slot> slots = toHashMap(Inputs.Map(static slot => (slot.Name, slot)));
        Seq<(string Name, TensorElementType Dtype, Seq<long> Shape)> rejected =
            binding.Filter(candidate => slots.Find(candidate.Name).Case is not Slot slot || !Conforms(slot, candidate));
        Seq<string> missing = Inputs
            .Filter(slot => slot.Shape is not SlotShape.Optional && !binding.Exists(candidate => StringComparer.Ordinal.Equals(candidate.Name, slot.Name)))
            .Map(static slot => slot.Name);
        int distinct = binding.Map(static candidate => candidate.Name).ToFrozenSet(StringComparer.Ordinal).Count;
        return (Refusal.Unless(distinct == binding.Count, ComputeArea.Model,
                    new ComputeViolation.Contract(ComputeContract.Unique, new ContractEvidence.Count(distinct, binding.Count))),
                Refusal.Unless(missing.IsEmpty, ComputeArea.Model,
                    new ComputeViolation.Contract(ComputeContract.Complete, new ContractEvidence.Count(missing.Count, 0L))),
                Refusal.Unless(rejected.IsEmpty, ComputeArea.Model,
                    new ComputeViolation.Contract(ComputeContract.Compatible, new ContractEvidence.Count(rejected.Count, 0L))))
            .Apply(static (_, _, _) => unit).As().ToFin();
    }

    public Fin<(string Name, OrtValue Value)> Initializer(string name, OrtValue value) {
        return value.OnnxType == OnnxValueType.ONNX_TYPE_TENSOR
            ? Try.lift(() => value.GetTensorTypeAndShape()).Run()
                .Bind(info => Initializers.Find(slot => StringComparer.Ordinal.Equals(slot.Name, name)).Case is Slot slot
                    && slot.Shape is SlotShape.Tensor tensor
                    && tensor.Dtype == info.ElementDataType
                    && DimsConform(tensor.Dims, info.Shape)
                        ? Fin.Succ((name, value))
                        : Fin.Fail<(string, OrtValue)>(IdentityRefusal.InitializerUnconformable.Fault()))
            : Fin.Fail<(string, OrtValue)>(IdentityRefusal.InitializerUnconformable.Fault());
    }

    static bool Conforms(Slot slot, (string Name, TensorElementType Dtype, Seq<long> Shape) binding) =>
        StringComparer.Ordinal.Equals(slot.Name, binding.Name)
        && Unwrap(slot.Shape).Switch(
            state: binding,
            tensor: static (offered, shape) => shape.Dtype == offered.Dtype && offered.Shape.ForAll(static dim => dim >= 0) && DimsConform(shape.Dims, offered.Shape),
            sparseTensor: static (offered, shape) => shape.Dtype == offered.Dtype && offered.Shape.ForAll(static dim => dim >= 0) && DimsConform(shape.Dims, offered.Shape),
            sequence: static (_, _) => true,
            map: static (_, _) => true,
            optional: static (_, _) => false);

    static Fin<SlotShape> MapOf(MapMetadata metadata) => ShapeOf(metadata.ValueMetadata)
        .Map<SlotShape>(value => new SlotShape.Map(metadata.KeyDataType, value));

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
        _ => Fin.Fail<SlotShape>(IdentityRefusal.SlotKindUnmodelled.Fault()),
    };

}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DriftStatistic {
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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FeatureSample(string Feature) {
    public sealed record Numeric(string Feature, ReadOnlyMemory<double> Values) : FeatureSample(Feature);

    public sealed record Categorical(string Feature, Seq<string> Labels) : FeatureSample(Feature);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DriftSeverity {
    public static readonly DriftSeverity Stable = new("stable", invalidates: false, static (score, policy) => score < policy.DriftingScore);
    public static readonly DriftSeverity Drifting = new("drifting", invalidates: false, static (score, policy) => score >= policy.DriftingScore && score < policy.BreachScore);
    public static readonly DriftSeverity Breached = new("breached", invalidates: true, static (score, policy) => score >= policy.BreachScore);

    public bool Invalidates { get; }

    [UseDelegateFromConstructor]
    public partial bool Elects(double score, DriftPolicy policy);

    public static DriftSeverity Of(double score, DriftPolicy policy) =>
        toSeq(Items).Find(row => row.Elects(score, policy)).IfNone(Stable);
}

public sealed record DriftVerdict(
    UInt128 EvidenceKey, string Feature, DriftStatistic Statistic, DriftSeverity Severity, double Score, int SampleCount);

public sealed record DriftReport(DriftVerdict Worst, Seq<DriftVerdict> Features, Seq<string> Uncovered);

[ComplexValueObject]
public sealed partial class DriftPolicy {
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
                message: $"<{nameof(DriftPolicy)}:{statistic?.Key}:{driftingScore}:{breachScore}:{minimumSamples}:{probabilityFloor}>");
}

public sealed record GraduationEnvelope(UInt128 EvidenceKey, Seq<GraduationEnvelope.Band> Bands) {
    public readonly record struct Observation(double[] Reference, double[] Observed, int SampleCount);

    [Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
    public abstract partial record Band(string Feature) {
        public sealed record Numeric(string Feature, Seq<double> BinEdges, Seq<double> BinMass) : Band(Feature);

        public sealed record Categorical(string Feature, Seq<string> Categories, Seq<double> Mass) : Band(Feature);

        public Fin<Observation> Observe(FeatureSample window, int minimumSamples) => (Band: this, Window: window) switch {
            (Numeric band, FeatureSample.Numeric values) => Binned(band, values, minimumSamples),
            (Categorical band, FeatureSample.Categorical labels) => Tallied(band, labels, minimumSamples),
            var mismatched => Mismatched(mismatched.Band.Feature),
        };

        public bool Wellformed() =>
            !string.IsNullOrWhiteSpace(Feature)
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
                return Fin.Fail<Observation>(IdentityRefusal.DriftWindowUndersized.Fault());
            }
            ReadOnlySpan<double> edges = band.BinEdges.AsSpan();
            double[] observed = new double[band.BinMass.Count];
            foreach (double value in values) { observed[Bin(edges, value)] += 1d; }
            return Fin.Succ(new Observation(band.BinMass.ToArray(), Normalize(observed, values.Length), values.Length));
        }

        static Fin<Observation> Tallied(Categorical band, FeatureSample.Categorical window, int minimumSamples) {
            if (window.Labels.Count < minimumSamples) {
                return Fin.Fail<Observation>(IdentityRefusal.DriftWindowUndersized.Fault());
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

        static Fin<Observation> Mismatched(string feature) =>
            Fin.Fail<Observation>(IdentityRefusal.DriftWindowMiskinded.Fault());

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
            && Math.Abs(TensorPrimitives.Sum<double>(mass.AsSpan()) - 1.0) <= 1e-9;
    }

    public static Fin<GraduationEnvelope> Admit(UInt128 evidenceKey, Seq<Band> bands) =>
        (guard(evidenceKey != UInt128.Zero, (Error)IdentityRefusal.EnvelopeMalformed.Fault()),
         guard(!bands.IsEmpty, (Error)IdentityRefusal.EnvelopeMalformed.Fault()),
         guard(
             bands.Map(static band => band.Feature).ToFrozenSet(StringComparer.Ordinal).Count == bands.Count,
             (Error)IdentityRefusal.EnvelopeMalformed.Fault()),
         bands.Traverse(static band => guard(band.Wellformed(), (Error)IdentityRefusal.BandMalformed.Fault())).As())
        .Apply(static (_, _, _, _) => unit).As().ToFin()
        .Map(_ => new GraduationEnvelope(evidenceKey, bands));

    public static Fin<GraduationEnvelope> Admit(HdfHandle archive) =>
        guard(archive.Exists("bands"), (Error)IdentityRefusal.ArchiveUnreadable.Fault())
        .ToFin()
        .Bind(_ =>
            from root in archive.Group("bands")
            from header in Try.lift(() => (
                EvidenceKey: UInt128.Parse(root.Attribute("evidence-key").Read<string>(), NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                Children: toSeq(root.Children()))).Run()
            from bands in header.Children.Traverse(child =>
                from row in Try.lift(() => (
                    Feature: child.Name,
                    Kind: child.Attribute("kind").Read<string>())).Run()
                from massSet in archive.Dataset($"bands/{row.Feature}/mass")
                from mass in ReadDoubles(archive, massSet)
                from band in StringComparer.Ordinal.Equals(row.Kind, "numeric")
                    ? from edgeSet in archive.Dataset($"bands/{row.Feature}/edges")
                      from edges in ReadDoubles(archive, edgeSet)
                      select (Band)new Band.Numeric(row.Feature, toSeq(edges), toSeq(mass))
                    : StringComparer.Ordinal.Equals(row.Kind, "categorical")
                    ? from categorySet in archive.Dataset($"bands/{row.Feature}/categories")
                      from categories in ReadStrings(categorySet)
                      select (Band)new Band.Categorical(row.Feature, toSeq(categories), toSeq(mass))
                    : Fin.Fail<Band>((Error)IdentityRefusal.BandMalformed.Fault())
                select band)
            from admitted in Admit(header.EvidenceKey, bands)
            select admitted);

    static Fin<double[]> ReadDoubles(HdfHandle archive, NativeDataset dataset) =>
        Try.lift(() => {
            ulong[] extent = dataset.Space.Dimensions;
            if (extent.Length != 1 || dataset.Type.Class != H5DataTypeClass.FloatingPoint || dataset.Type.Size != sizeof(double)) {
                return Fin.Fail<double[]>((Error)IdentityRefusal.BandMalformed.Fault());
            }
            double[] values = new double[checked((int)extent[0])];
            dataset.Read<double>(archive.Access, values.AsSpan(), new HyperslabSelection(0, (ulong)values.Length));
            return Fin.Succ(values);
        }).Run().Bind(static inner => inner);

    static Fin<string[]> ReadStrings(NativeDataset dataset) =>
        Try.lift(() =>
            dataset.Space.Dimensions.Length == 1 && dataset.Type.Class == H5DataTypeClass.String
                ? Fin.Succ(dataset.Read<string[]>())
                : Fin.Fail<string[]>((Error)IdentityRefusal.BandMalformed.Fault())).Run().Bind(static inner => inner);

    public Fin<DriftReport> Drift(Seq<FeatureSample> serving, DriftPolicy policy) {
        bool unique = serving.Map(static window => window.Feature).ToFrozenSet(StringComparer.Ordinal).Count == serving.Count;
        Seq<(Band Band, Option<FeatureSample> Window)> paired = Bands.Map(band =>
            (Band: band, Window: serving.Find(window => StringComparer.Ordinal.Equals(window.Feature, band.Feature))));
        Seq<string> uncovered = paired.Filter(static row => row.Window.IsNone).Map(static row => row.Band.Feature);
        Seq<(Band Band, FeatureSample Window)> covered =
            paired.Choose(static row => row.Window.Map(window => (row.Band, Window: window)));
        return (guard(unique, (Error)IdentityRefusal.DriftPopulationRejected.Fault()),
                guard(!covered.IsEmpty, (Error)IdentityRefusal.DriftPopulationRejected.Fault()))
            .Apply(static (_, _) => unit).As().ToFin()
            .Bind(_ => covered.Traverse(row => Verdict(row.Band, row.Window, policy).ToValidation()).As().ToFin())
            .Map(verdicts => new DriftReport(
                verdicts.Reduce(static (worst, verdict) => verdict.Score > worst.Score ? verdict : worst),
                verdicts,
                uncovered));
    }

    Fin<DriftVerdict> Verdict(Band band, FeatureSample window, DriftPolicy policy) =>
        from observed in band.Observe(window, policy.MinimumSamples)
        let score = policy.Statistic.Score(observed.Reference, observed.Observed, policy.ProbabilityFloor)
        select new DriftVerdict(
            EvidenceKey, band.Feature, policy.Statistic, DriftSeverity.Of(score, policy), score, observed.SampleCount);
}
```

## [03]-[GRADUATION_EVIDENCE]

- Owner: `FieldScalar` `[SmartEnum<string>]` is the closed wire-primitive vocabulary; `FieldNode` `[Union]` is the recursive descriptor tree whose six cases carry every composite shape a .NET owner projects, each nesting the root so depth growth stays case-owned; `OwnerDescriptor` names one owner and its ordered field roster; `GraduationEvidence` is the versioned bundle carrying the roster with the content key its own canonical projection mints.
- Cases: `FieldNode` cases `Scalar` (one `FieldScalar` leaf), `Array` (one element node), `Nested` (one owner-name reference), `Mapping` (key and value nodes), `Optional` (one element node), `UnionCase` (a non-empty member roster); `FieldScalar` rows `i32`, `i64`, `f64`, `bool`, `string`, `key`, `bytes`, `decimal`.
- Law: kind literals are the DECODE contract. Its companion projector selects each leaf case on the `kind` discriminator alone, and the union generator emits no JSON support of any kind — no converter, no derived-type roster — so a case crossing without its `[JsonDerivedType]` row serializes as the abstract base, one empty object per case, with no decode refusal on either end. Hand-declaration on the union declaration freezes the literals: renaming a case is free, renaming a literal is a wire break.
- Law: the bundle is OFFLINE at rest and reaches no gRPC leg. It crosses as bytes the app root writes through the Persistence object lane exactly as a warm artifact does, so the `Runtime/wire#PROTO_VOCABULARY` roster never grows a message for it and the corpus gate has nothing to police here — a wire the channel never carries cannot drift a channel contract.
- Law: admission proves what the far end can only fail on. `Nested.Ref` names a declared owner and the owner graph is ACYCLIC, because the projector builds each struct against already-registered siblings — an unresolved reference is an unbound name at class creation there and a back edge is a topological refusal, both after the bytes already shipped. `UnionCase.Members` is non-empty for the same reason: the projector's member fold reduces from its first element.
- Entry: `public static Fin<GraduationEvidence> Admit(Seq<OwnerDescriptor> owners)` — the caller supplies the roster and the bundle mints its own `SchemaVersion` and `BundleKey`, so neither is a claim a caller can spell wrong. `public Fin<ReadOnlyMemory<byte>> Bundle(JsonTypeInfo<GraduationEvidence> contract)` writes the canonical UTF-8 payload under an injected contract; on the wire `BundleKey` IS its bare 32-hex text — the solution content-key text law, minted once at admission where the roster is in hand, so the numeric `Key` stays an in-process read and no decode re-admits what `Admit` already derived (a raw `UInt128` JSON number breaks double-precision consumers), and the scalar leaf's payload property pins `"scalar"` because CamelCase would seat it on the `"kind"` discriminator STJ refuses.
- Auto: `Admit` refuses an empty roster, a blank or duplicated owner name, a blank or duplicated field name within one owner, an unsound node anywhere in a tree, an unresolvable `Nested.Ref`, and a cyclic owner graph proved by peeling every reference-free owner until either the graph empties or a pass settles nothing. `Render` is the one catamorphism over the tree: it feeds the length-framed preimage `ContentHash.Of` keys, so bundle identity and the shape it describes cannot disagree, and a field-order or scalar-row change re-keys the bundle the companion pins its round-trip against.
- Result: none — the bundle is an artifact, not a measured run, and its content key is the identity the writing composition indexes it under.
- Packages: System.Text.Json, System.IO.Hashing, Generator.Equals (`[Equatable]` diff surface — `BundleKey` ignored as derived, the gate stays the content key), Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm (project, `Domain.ContentHash`), BCL inbox
- Growth: a new wire primitive is one `FieldScalar` row the projector absorbs with one table row of its own; a new composite shape is one `FieldNode` case with its `[JsonDerivedType]` literal, one `Render` arm, one `Sound` arm, and one `Refs` arm; a new bundle column is one record field beside a `SchemaVersion` bump at both ends in one change.
- Boundary: this owner mints descriptors and never decodes them — the projection back into typed stubs is the companion's, and nothing here imports a peer-runtime shape. Serialization uses an injected `JsonTypeInfo<GraduationEvidence>`, so this page holds no serializer, options handle, or second context. `SchemaVersion` is a gate rather than a column a caller fills: a bundle outside the carried set fails at the companion's decode band, so minting one outside it here ships bytes guaranteed to refuse. Boundary law: the branch `[GRADUATION]` edge runs REVERSE-ONLY from this end — the forward leg is the companion's handoff axis, this leg is the evidence answering it, and neither end references the other's types.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FieldScalar {
    public static readonly FieldScalar I32 = new("i32");
    public static readonly FieldScalar I64 = new("i64");
    public static readonly FieldScalar F64 = new("f64");
    public static readonly FieldScalar Bool = new("bool");
    public static readonly FieldScalar Text = new("string");
    public static readonly FieldScalar Key = new("key");
    public static readonly FieldScalar Bytes = new("bytes");
    public static readonly FieldScalar Decimal = new("decimal");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(Scalar), "scalar")]
[JsonDerivedType(typeof(Array), "array")]
[JsonDerivedType(typeof(Nested), "nested")]
[JsonDerivedType(typeof(Mapping), "mapping")]
[JsonDerivedType(typeof(Optional), "optional")]
[JsonDerivedType(typeof(UnionCase), "union")]
public abstract partial record FieldNode(string Name) {
    public sealed record Scalar(string Name, [property: JsonPropertyName("scalar")] FieldScalar Kind) : FieldNode(Name);

    public sealed record Array(string Name, FieldNode Element) : FieldNode(Name);

    public sealed record Nested(string Name, string Ref) : FieldNode(Name);

    public sealed record Mapping(string Name, FieldNode Key, FieldNode Value) : FieldNode(Name);

    public sealed record Optional(string Name, FieldNode Element) : FieldNode(Name);

    public sealed record UnionCase(string Name, Seq<FieldNode> Members) : FieldNode(Name);

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

[Equatable]
public sealed partial record GraduationEvidence(string SchemaVersion, [property: OrderedEquality] Seq<OwnerDescriptor> Owners, [property: IgnoreEquality] string BundleKey) {
    public const string Schema = "1";

    [JsonIgnore] public UInt128 Key => KeyOf(Owners);

    public static Fin<GraduationEvidence> Admit(Seq<OwnerDescriptor> owners) =>
        (guard(!owners.IsEmpty, (Error)IdentityRefusal.OwnerRosterMalformed.Fault()),
         guard(
             owners.Map(static owner => owner.Name).ToFrozenSet(StringComparer.Ordinal).Count == owners.Count,
             (Error)IdentityRefusal.OwnerRosterMalformed.Fault()),
         owners.Traverse(static owner => Wellformed(owner)).As())
        .Apply(static (_, _, _) => unit).As().ToFin()
        .Bind(_ => Resolvable(owners))
        .Map(_ => new GraduationEvidence(Schema, owners, $"{KeyOf(owners):x32}"));

    static Validation<Error, Unit> Wellformed(OwnerDescriptor owner) =>
        (guard(!string.IsNullOrWhiteSpace(owner.Name), (Error)IdentityRefusal.OwnerRosterMalformed.Fault()),
         guard(
             owner.Fields.Map(static field => field.Name).ToFrozenSet(StringComparer.Ordinal).Count == owner.Fields.Count,
             (Error)IdentityRefusal.OwnerRosterMalformed.Fault()),
         owner.Fields.Traverse(field => guard(
             field.Sound(), (Error)IdentityRefusal.OwnerRosterMalformed.Fault())).As())
        .Apply(static (_, _, _) => unit).As();

    public Fin<ReadOnlyMemory<byte>> Bundle(JsonTypeInfo<GraduationEvidence> contract) =>
        Try.lift(() => (ReadOnlyMemory<byte>)JsonSerializer.SerializeToUtf8Bytes(this, contract)).Run();

    static Fin<Unit> Resolvable(Seq<OwnerDescriptor> owners) {
        FrozenSet<string> declared = owners.Map(static owner => owner.Name).ToFrozenSet(StringComparer.Ordinal);
        Seq<string> unbound = owners
            .Bind(static owner => owner.Fields.Bind(static field => field.Refs()))
            .Distinct()
            .Filter(reference => !declared.Contains(reference));
        Seq<SEquatableEdge<string>> edges = owners.Bind(owner => owner.Fields
            .Bind(static field => field.Refs())
            .Distinct()
            .Filter(declared.Contains)
            .Map(reference => new SEquatableEdge<string>(owner.Name, reference)));
        return (guard(
                    unbound.IsEmpty,
                    (Error)IdentityRefusal.OwnerGraphUnresolvable.Fault()),
                guard(
                    edges.ToArray().IsDirectedAcyclicGraph(),
                    (Error)IdentityRefusal.OwnerGraphUnresolvable.Fault()))
            .Apply(static (_, _) => unit).As().ToFin();
    }

    static UInt128 KeyOf(Seq<OwnerDescriptor> owners) => ContentHash.Of(owners, static (roster, writer) => writer
        .String(Schema)
        .Rows(roster, static (owner, rows) => rows
            .String(owner.Name)
            .Rows(owner.Fields, static (field, fields) => fields.String(field.Render()))));
}
```
