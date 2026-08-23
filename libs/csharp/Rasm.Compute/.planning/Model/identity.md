# [COMPUTE_IDENTITY]

Rasm.Compute model identity owns ONNX provenance and the content address every downstream cache key, receipt, and claim derives from. `ModelIdentity` carries checksum, `SlotShape` schema trees, `Provenance`, and the `CustomMetadata`/`Initializers` self-description channels; `ModelSource` folds five acquisition cases to one byte admission; `RosterFingerprint` owns the execution-provider-composed fingerprint; `GraduationEnvelope` and `GraduationEvidence` own the graduation seam's forward drift admission and reverse descriptor bundle. Admission settles the model, input, initializer, drift, and descriptor contracts once.

Identity derives from the model bytes through the kernel seed-zero `XxHash128` entry `Rasm.Domain.ContentHash.Of`, the workspace's one hasher, and `RosterFingerprint` projects that same digest's LOW lane through the kernel `CanonicalWriter`, so this package carries ONE hash family; slot schema reads `Microsoft.ML.OnnxRuntime` `InferenceSession` metadata; `ModelLoad` rides the `ComputeReceipt` rail; the descriptor bundle rides `System.Text.Json` under the injected `JsonTypeInfo<GraduationEvidence>` contract. `NodaTime` `Instant`, the kernel `CorrelationId` (`Rasm/Domain/frame#SOURCE`) and `ReceiptSinkPort` (`Rasm/Domain/frame#RECEIPT_PORT`), the `Runtime/receipts#RECEIPT_UNION` spine and its `ComputeWireContext` Strict resolver, the `Runtime/admission#SUBSTRATE_AXIS` `Substrate` axis beside the spine `WorkLane` roster, and the Persistence `ArtifactIndexRow` arrive settled. `ModelIdentity`/`RosterFingerprint`/`Slot` cross to `Model/sessions#SESSION_CAPSULE`, `Model/providers#EP_AXIS`, `Model/run#RUN_MODES`, and `Model/generative#GENERATIVE_RUN` as settled vocabulary, `Checksum` is the deterministic cache and result-key seed `Model/run#RESULT_CACHE` consumes, and a `DriftVerdict` whose `Severity.Invalidates` answers true is the reuse-invalidation signal that same cache consumes as an `EquivalenceMiss` fault.

## [01]-[INDEX]

- [02]-[MODEL_IDENTITY]: checksum identity; five-case acquisition union with the byte-resolution fold; kind-discriminated schema snapshot with provenance; admission over input slots and overridable initializers; custom-metadata self-description; shared ordinal-keyvalue fingerprint over the kernel canonical writer; the graduation drift sentinel over its numeric and categorical band cases; `ModelLoad` receipt mint.
- [03]-[GRADUATION_EVIDENCE]: scalar-kind vocabulary; recursive `FieldNode` descriptor union with locked kind literals; owner-descriptor roster; bundle admission proving well-formedness, reference resolution, and an acyclic owner graph before its content-keyed bytes leave.

## [02]-[MODEL_IDENTITY]

- Owner: `ModelIdentity` identity record with nested `Slot` rows over recursive `SlotShape` tensor, sparse-tensor, sequence, map, and optional cases; `Provenance` owns the producer/domain/graph/description block and `CustomMetadata`/`Initializers` the self-description channels; `ModelSource` `[Union]` owns five acquisition cases whose `Acquire` fold resolves each through injected `SourceResolver` ports and whose `Origin` projects the receipt source class; `RosterFingerprint` owns the ordinal-keyvalue projection over the kernel canonical writer composed by `ExecutionProvider.ResultKey`; `IdentityRefusal` names this owner's shared contract refusals without a string-key roster; `GraduationEnvelope` owns the admitted per-feature `Band` roster and its `Observe` projection onto one reference/observed mass pair, `DriftStatistic` owns the score over that pair, `DriftPolicy` owns the statistic row beside its ordered thresholds and sampling floors, `FeatureSample` carries one serving window per feature, and `DriftReport` carries the per-feature verdict set with its headline and uncovered roster.
- Cases: `ModelSource` cases `LocalFile`, `EmbeddedResource`, `PersistenceBlob`, `RemoteFetch`, `Buffer`; `SlotShape` cases `Tensor`, `SparseTensor`, `Sequence`, `Map`, `Optional`; `Band` and `FeatureSample` cases `Numeric`, `Categorical`; `DriftSeverity` rows `stable`, `drifting`, `breached`, each carrying the threshold predicate that elects it and its reuse-invalidation posture; `DriftStatistic` rows `psi`.
- Entry: `public static Fin<ModelIdentity> Snapshot(ModelSource source, ReadOnlySpan<byte> bytes, InferenceSession session, Instant at)` — metadata topology and model bytes admit together; identity derives from the bytes. `GraduationEnvelope.Admit(HdfHandle)` is the FORWARD graduation ingest — one `Runtime/archive#HDF_ARCHIVE` open per admission job reads the h5py-written `/bands/<feature>` roster (`kind` attribute selects the case, `edges`/`mass`/`categories` datasets read under declared selections, the evidence key an attribute, `LinkExists` proving the roster before any resolve) and re-enters the roster `Admit`, so every Wellformed gate reruns on read bands; the reverse JSON `GraduationEvidence` leg keeps its own container untouched.
- Auto: `Acquire` captures the source fold through `Op.Catch`; typed resolver faults survive and throwing file or resource boundaries remain the original `Error`.
- Receipt: `ModelLoad` — the `Runtime/receipts#RECEIPT_UNION` `ModelLoad(checksum, source, ep, version)` shape — is minted by `LoadReceipt` from this owner's `Key`, `Source.Origin`, and snapshotted `GraphVersion` with the loader's `ExecutionProvider`, correlation, work lane, substrate, and elapsed; emission rides the sink port at the composition edge. Every drift verdict whose severity `Invalidates` faults `ComputeFault.EquivalenceMiss` at the consuming lane — correctness gates reuse exactly as it gates session admission, and a fast stale surrogate is the worst reused object.
- Packages: Microsoft.ML.OnnxRuntime, System.Numerics.Tensors, System.IO.Hashing, System.Text.Json, PureHDF (`NativeGroup`, `IH5Object.Name`/`Attribute`/`Children`, `NativeDataset.Read<T>(H5DatasetAccess, Span<T>, …)`, `HyperslabSelection`), Generator.Equals (`[Equatable]`, `[OrderedEquality]`, `[UnorderedEquality]`, `[IgnoreEquality]`), QuikGraph (`AlgorithmExtensions.IsDirectedAcyclicGraph(IEnumerable<TEdge>)`, `SEquatableEdge<T>`), NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm (project, `Domain.ContentHash`/`CanonicalWriter`/`Lane`), Rasm.AppHost (project), Rasm.Persistence (project, `ContentBlobPort`, `ArtifactIndexRow`)
- Growth: a new acquisition route is one `ModelSource` case with its `Acquire` arm and `Origin` projection; a new ONNX value kind is one `SlotShape` case and one `ShapeOf` arm; a new deployment-constant binding composes `Initializer`; a new self-description axis is one `Provenance` field; a new drift statistic is one `DriftStatistic` row scoring the same reference/observed mass pair, with the policy already carrying the row; a new severity band is one `DriftSeverity` row carrying its own election predicate and invalidation posture, and no consumer switch moves; a new feature kind is one `Band` case beside its `FeatureSample` case and one `Observe` arm.
- Boundary: every downstream cache key, receipt, and claim derives from `Checksum`; `Checksum` composes the kernel seed-zero `ContentHash.Of(bytes)` owner shared with geometry, seam content addresses, and Persistence indexes. `Acquire` owns file and manifest-resource statement boundaries and brackets expected I/O faults into one named `IdentityRefusal`; the injected `SourceResolver` keeps blob storage and HTTP transport outside this owner, and its blob route is the Persistence `Store/blobstore#OBJECT_STORE` `ContentBlobPort` itself rather than a page-local key-to-bytes delegate — one seam, bound by the composition root, adapted from its `IO` rail onto this page's `Fin` rail at its single use site. `SlotShape` preserves the full recursive ONNX value topology: `SequenceMetadata.ElementMeta`, `OptionalMetadata.ElementMeta`, and `MapMetadata.KeyDataType`/`ValueMetadata` recurse until a dense or sparse tensor leaf. Sequence and map slots admit by NAME here because a slot describes a shape while a value carries one, and the value's structural gate is `Model/extension#EXTENSION_OPS` `Egress` — whose arm set is CLOSED over every `SlotShape` leaf this owner can snapshot, so a schema this admitter accepts has a reader at run and neither end carries a kind the other cannot. `Accepts` treats negative model dims as free axes but rejects negative offered extents and requires the complete non-`Optional` input set; `Initializer` requires an exact dense-tensor shape instead of accepting a different rank with the same element count. `CustomMetadata` mints no second identity because its bytes already participate in `Checksum`; `Initializers` remain deployment constants bound through `AddInitializer(string, OrtValue)`. `ModelLoad` carries derivable `ModelMetadata.Version` and takes its lane and substrate from the loader, because a background sweep's cold open and an interactive lease's cold open land on different lanes and the selection axis — not this owner — decides which substrate ran, so a hardwired pair publishes one lane's evidence for every load. `GraduationEnvelope` admits evidence-keyed normalized bands, scores a PARTIAL serving window rather than refusing it, and returns an evidence-bearing typed verdict per covered feature only after that feature's window crosses the policy sample floor — an uncovered band names itself on `DriftReport.Uncovered` and never folds into the headline, because a feature nobody sampled is a hole in the evidence rather than a stable one.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------

// Named sites select bounded contracts directly; no string-key roster survives beneath the shared violation.
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

// The two acquisition ROUTES a host wires, and only one of them is a delegate. The blob route IS the Persistence
// `Store/blobstore#OBJECT_STORE` `ContentBlobPort` — the one key-minting byte seam over the object plane — so
// this page spells no key-to-bytes pair of its own, which is exactly the loose-delegate form that port owner
// names as deleted; the remote route stays a delegate because no admitted transport owner exists to compose.
// Absence rides the `Option` rather than a refusing delegate: a port that is a VALUE cannot carry a "not wired"
// arm without inventing one, and the `Option` says the same thing where a reader can see it.
public readonly record struct SourceResolver(
    Option<ContentBlobPort> Blob,
    Func<string, Fin<ReadOnlyMemory<byte>>> Remote) {
    // The REFUSING default is the composition's own evidence: a host that binds neither route still admits
    // `LocalFile`, `EmbeddedResource`, and `Buffer`, and the two routes it did not wire refuse by name instead of
    // reading as an unbound delegate at first call.
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
        return Op.Of(name: "model.source-acquire").Catch(() => Switch(
                localFile: static f => File.Exists(f.Path)
                    ? Fin.Succ((ReadOnlyMemory<byte>)File.ReadAllBytes(f.Path))
                    : Fin.Fail<ReadOnlyMemory<byte>>(IdentityRefusal.SourceUnresolved.Fault()),
                embeddedResource: static e => e.Assembly.GetManifestResourceStream(e.Name) is Stream stream
                    ? Read(stream)
                    : Fin.Fail<ReadOnlyMemory<byte>>(IdentityRefusal.SourceUnresolved.Fault()),
                // The index row already NAMES its content address, so the read is the port's own inverse against
                // that key — never a re-derivation from the row's other columns. The port speaks the object
                // plane's `IO` rail and this page's rail is `Fin`, so the one adaptation happens at the one use
                // site rather than by re-shaping a landed port.
                persistenceBlob: b => resolver.Blob
                    .ToFin((Error)IdentityRefusal.SourceUnresolved.Fault())
                    .Bind(port => port.Get(b.Row.Content).Try().Run()),
                remoteFetch:     r => resolver.Remote(r.ArtifactId),
                buffer:          static b => Fin.Succ(b.Bytes)));
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

// Named for what it folds — an ordinal keyvalue ROSTER — because `ModelFingerprint` collided with the
// Persistence EF-model digest of the same name inside Compute's own compile closure, and the two are distinct
// concepts that must not merge: this one keys provider behavior, that one keys a schema.
public static class RosterFingerprint {
    // The ordinal keyvalue roster folds through the KERNEL canonical writer: `Sorted` publishes the canonical
    // order for a hash-keyed roster and `String` frames every field with its own UTF-8 byte count, so a separator
    // inside a value can never shift two distinct option tables onto one fingerprint — and the framing law lives
    // at its ONE owner instead of a second length-prefix loop per hashing surface. The digest is the LOW lane of
    // the estate's `XxHash128` content key rather than a parallel `XxHash3` path, so this package carries one hash
    // family; the `ulong` width is what keeps a fingerprint a 64-bit column inside a composite key.
    public static ulong Of(IEnumerable<KeyValuePair<string, string>> rows) => ContentHash.Half(
        ContentHash.Of(toSeq(rows), static (roster, writer) => writer.Sorted(
            roster,
            static row => row.Key,
            StringComparer.Ordinal,
            static (row, framed) => framed.String(row.Key).String(row.Value))),
        Lane.Low);
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

// `[Equatable]` closes the latent trap: `CustomMetadata` is a FrozenDictionary record equality compares by
// REFERENCE, so two snapshots of one model read unequal the moment the map is rebuilt. `Source` and
// `AcquiredAt` are IGNORED so `Equals` agrees with `Key` — one model acquired twice by different routes IS one
// identity — and `Inequalities` localizes which slot or metadata entry moved between two schema snapshots.
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

    // Completeness runs over EVERY non-Optional input slot — a required sparse, sequence, or map input missing
    // from the binding rejects; only Optional slots may be absent. The three facts are INDEPENDENT and accumulate
    // through one `Apply`, so a binding that is both duplicated and short of a required slot names both rather
    // than costing one round trip per defect; one conjunction folded all three into a single string a caller had
    // to parse to learn which of them it had violated. Slots index ORDINALLY once, so the three passes over two
    // collections the conjunction made collapse to one lookup per candidate.
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
            ? Op.Of(name: "model.initializer-shape").Catch(() => Fin.Succ(value.GetTensorTypeAndShape()))
                .Bind(info => Initializers.Find(slot => StringComparer.Ordinal.Equals(slot.Name, name)).Case is Slot slot
                    && slot.Shape is SlotShape.Tensor tensor
                    && tensor.Dtype == info.ElementDataType
                    && DimsConform(tensor.Dims, info.Shape)
                        ? Fin.Succ((name, value))
                        : Fin.Fail<(string, OrtValue)>(IdentityRefusal.InitializerUnconformable.Fault()))
            : Fin.Fail<(string, OrtValue)>(IdentityRefusal.InitializerUnconformable.Fault());
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

    // The generated total `Switch` over an OWNED closed family, never an `is`-ladder with a catch-all: a sixth
    // `SlotShape` case breaks this dispatch at compile time where `_ => false` silently refused every binding
    // against it. `Unwrap` has already peeled every `Optional`, so that arm is unreachable and says so.
    static bool Conforms(Slot slot, (string Name, TensorElementType Dtype, Seq<long> Shape) binding) =>
        StringComparer.Ordinal.Equals(slot.Name, binding.Name)
        && Unwrap(slot.Shape).Switch(
            state: binding,
            tensor: static (offered, shape) => shape.Dtype == offered.Dtype && offered.Shape.ForAll(static dim => dim >= 0) && DimsConform(shape.Dims, offered.Shape),
            sparseTensor: static (offered, shape) => shape.Dtype == offered.Dtype && offered.Shape.ForAll(static dim => dim >= 0) && DimsConform(shape.Dims, offered.Shape),
            sequence: static (_, _) => true,
            map: static (_, _) => true,
            optional: static (_, _) => false);

    // The one hop earns its seat: it BINDS the map metadata so the key dtype and the value recursion read one
    // native metadata handle — inlining costs either a second `AsMapMetadata()` call or a `switch`-as-let.
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

// Severity is a ROW, and the row carries the threshold predicate that elects it beside the invalidation posture
// the reuse gate reads. Three cases carried a byte-identical five-column payload whose only discriminant was the
// name — a union bought exactly one boolean probe and cost three declarations plus three positional re-spellings,
// while the thresholds that decide the name lived in a grading expression outside the family. NAMED LOSS:
// compile-time exhaustiveness on a case switch; bought back because no consumer switched on the case — the sole
// probe asked whether reuse is invalid, which is now a column, and a fourth severity is one row rather than a
// case plus a grading arm plus every consumer's default.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DriftSeverity {
    public static readonly DriftSeverity Stable = new("stable", invalidates: false, static (score, policy) => score < policy.DriftingScore);
    public static readonly DriftSeverity Drifting = new("drifting", invalidates: false, static (score, policy) => score >= policy.DriftingScore && score < policy.BreachScore);
    public static readonly DriftSeverity Breached = new("breached", invalidates: true, static (score, policy) => score >= policy.BreachScore);

    // `Model/run#RESULT_CACHE` purges and faults `EquivalenceMiss` on THIS column, so a later severity that
    // must invalidate reuse says so as data rather than as a case every gate has to learn to probe.
    public bool Invalidates { get; }

    [UseDelegateFromConstructor]
    public partial bool Elects(double score, DriftPolicy policy);

    // The rows partition the score line — `DriftPolicy` admission already proves `BreachScore > DriftingScore >= 0`
    // — so exactly one elects and the fold is order-free; `Stable` is the total floor rather than a fallback.
    public static DriftSeverity Of(double score, DriftPolicy policy) =>
        toSeq(Items).Find(row => row.Elects(score, policy)).IfNone(Stable);
}

public sealed record DriftVerdict(
    UInt128 EvidenceKey, string Feature, DriftStatistic Statistic, DriftSeverity Severity, double Score, int SampleCount);

// Per-feature verdicts, the worst as the headline every reuse gate reads, and the bands no serving window covered.
// `DriftReport` never folds an uncovered feature into the headline: it is a hole in the evidence, and reporting
// it as stable is exactly how a drifting model keeps serving on the strength of a column nobody sampled.
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
                message: $"<{nameof(DriftPolicy)}:{statistic?.Key}:{driftingScore}:{breachScore}:{minimumSamples}:{probabilityFloor}>");
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

        // The band case and the window case are ONE joint discriminant, so the pair patterns in one level and the
        // mismatch arm is structural instead of spelled once per band arm. NAMED LOSS: the generated `Switch`'s
        // compile-time totality over `Band` here; bought back by `Wellformed` below, which stays a total `Switch`
        // over the same family — a new case cannot land without visiting the band's own admission, and it lands in
        // `Mismatched` here until it declares its window pairing.
        public Fin<Observation> Observe(FeatureSample window, int minimumSamples) => (Band: this, Window: window) switch {
            (Numeric band, FeatureSample.Numeric values) => Binned(band, values, minimumSamples),
            (Categorical band, FeatureSample.Categorical labels) => Tallied(band, labels, minimumSamples),
            var mismatched => Mismatched(mismatched.Band.Feature),
        };

        // Feature-name UNIQUENESS is the roster's fact and proves once at `Admit` with one `FrozenSet`; asking each
        // band to count its own siblings made admission quadratic and forced every caller to hand a band the roster
        // it belongs to.
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

        // Band-case disagreement with the window is a CALLER defect, never a numeric one: scoring a label
        // roster against quantile cuts answers a number for a comparison nothing performed.
        static Fin<Observation> Mismatched(string feature) =>
            Fin.Fail<Observation>(IdentityRefusal.DriftWindowMiskinded.Fault());

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
            && Math.Abs(TensorPrimitives.Sum<double>(mass.AsSpan()) - 1.0) <= 1e-9;
    }

    // Roster-level facts and per-band facts are different grains and BOTH accumulate: the envelope key, emptiness,
    // and feature uniqueness prove once here, and every malformed band names ITSELF through a traverse rather than
    // hiding behind a first-failure `ForAll` whose fault carried only the evidence key.
    public static Fin<GraduationEnvelope> Admit(UInt128 evidenceKey, Seq<Band> bands) =>
        (guard(evidenceKey != UInt128.Zero, (Error)IdentityRefusal.EnvelopeMalformed.Fault()),
         guard(!bands.IsEmpty, (Error)IdentityRefusal.EnvelopeMalformed.Fault()),
         guard(
             bands.Map(static band => band.Feature).ToFrozenSet(StringComparer.Ordinal).Count == bands.Count,
             (Error)IdentityRefusal.EnvelopeMalformed.Fault()),
         bands.Traverse(static band => guard(band.Wellformed(), (Error)IdentityRefusal.BandMalformed.Fault())).As())
        .Apply(static (_, _, _, _) => unit).As().ToFin()
        .Map(_ => new GraduationEnvelope(evidenceKey, bands));

    // HDF5 ingest — the FORWARD graduation seam: the python companion fits reference bands at graduation and
    // writes `/bands/<feature>` groups h5py-side (`kind` attribute selects the case; numeric carries `edges`
    // float64[k] + `mass` float64[k+1], categorical carries `categories` string[] + `mass` float64[]); this arm
    // reads them under declared selections into one `Admit` call, so every Wellformed gate reruns on the read
    // roster. The reverse JSON GraduationEvidence leg keeps its own container — this arm re-containers NOTHING.
    public static Fin<GraduationEnvelope> Admit(HdfHandle archive) =>
        // Probe-first, and the probe gates on the RAIL: `LinkExists` answers absence without faulting, so an
        // archive with no band roster refuses by name OUTSIDE the bracket rather than throwing into it to be
        // re-read out as a message. The bracket then owns native reads alone.
        guard(archive.Exists("bands"), (Error)IdentityRefusal.ArchiveUnreadable.Fault())
        .ToFin()
        .Bind(_ =>
            from root in archive.Group("bands")
            from header in Op.Of(name: "model.graduation-archive-header").Catch(() => Fin.Succ((
                EvidenceKey: UInt128.Parse(root.Attribute("evidence-key").Read<string>(), NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                Children: toSeq(root.Children()))))
            from bands in header.Children.Traverse(child =>
                from row in Op.Of(name: "model.graduation-archive-band").Catch(() => Fin.Succ((
                    Feature: child.Name,
                    Kind: child.Attribute("kind").Read<string>())))
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
        Op.Of(name: "model.graduation-archive-double").Catch(() => {
            ulong[] extent = dataset.Space.Dimensions;
            if (extent.Length != 1 || dataset.Type.Class != H5DataTypeClass.FloatingPoint || dataset.Type.Size != sizeof(double)) {
                return Fin.Fail<double[]>((Error)IdentityRefusal.BandMalformed.Fault());
            }
            double[] values = new double[checked((int)extent[0])];
            dataset.Read<double>(archive.Access, values.AsSpan(), new HyperslabSelection(0, (ulong)values.Length));
            return Fin.Succ(values);
        });

    // String elements never span-read; the allocating overload is the sanctioned path here.
    static Fin<string[]> ReadStrings(NativeDataset dataset) =>
        Op.Of(name: "model.graduation-archive-string").Catch(() =>
            dataset.Space.Dimensions.Length == 1 && dataset.Type.Class == H5DataTypeClass.String
                ? Fin.Succ(dataset.Read<string[]>())
                : Fin.Fail<string[]>((Error)IdentityRefusal.BandMalformed.Fault()));

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
        return (guard(unique, (Error)IdentityRefusal.DriftPopulationRejected.Fault()),
                guard(!covered.IsEmpty, (Error)IdentityRefusal.DriftPopulationRejected.Fault()))
            .Apply(static (_, _) => unit).As().ToFin()
            .Bind(_ => covered.Traverse(row => Verdict(row.Band, row.Window, policy).ToValidation()).As().ToFin())
            .Map(verdicts => new DriftReport(
                verdicts.Reduce(static (worst, verdict) => verdict.Score > worst.Score ? verdict : worst),
                verdicts,
                uncovered));
    }

    // The statistic that produced the score rides the verdict and reaches the fault text, because a threshold pair
    // carried without it grades one divergence against another's calibration.
    Fin<DriftVerdict> Verdict(Band band, FeatureSample window, DriftPolicy policy) =>
        from observed in band.Observe(window, policy.MinimumSamples)
        let score = policy.Statistic.Score(observed.Reference, observed.Observed, policy.ProbabilityFloor)
        select new DriftVerdict(
            EvidenceKey, band.Feature, policy.Statistic, DriftSeverity.Of(score, policy), score, observed.SampleCount);
}
```

## [03]-[GRADUATION_EVIDENCE]

- Owner: `FieldScalar` `[SmartEnum<string>]` is the closed wire-primitive vocabulary; `FieldNode` `[Union]` is the recursive descriptor tree whose six cases carry every composite shape a C# owner projects, each nesting the root so depth growth stays case-owned; `OwnerDescriptor` names one owner and its ordered field roster; `GraduationEvidence` is the versioned bundle carrying the roster with the content key its own canonical projection mints.
- Cases: `FieldNode` cases `Scalar` (one `FieldScalar` leaf), `Array` (one element node), `Nested` (one owner-name reference), `Mapping` (key and value nodes), `Optional` (one element node), `UnionCase` (a non-empty member roster); `FieldScalar` rows `i32`, `i64`, `f64`, `bool`, `string`, `key`, `bytes`, `decimal`.
- Law: kind literals are the DECODE contract. Its companion projector selects each leaf case on the `kind` discriminator alone, and the union generator emits no JSON support of any kind — no converter, no derived-type roster — so a case crossing without its `[JsonDerivedType]` row serializes as the abstract base, one empty object per case, with no decode refusal on either end. Hand-declaration on the union declaration freezes the literals: renaming a case is free, renaming a literal is a wire break.
- Law: the bundle is OFFLINE at rest and reaches no gRPC leg. It crosses as bytes the app root writes through the Persistence object lane exactly as a warm artifact does, so the `Runtime/wire#PROTO_VOCABULARY` roster never grows a message for it and the corpus gate has nothing to police here — a wire the channel never carries cannot drift a channel contract.
- Law: admission proves what the far end can only fail on. `Nested.Ref` names a declared owner and the owner graph is ACYCLIC, because the projector builds each struct against already-registered siblings — an unresolved reference is an unbound name at class creation there and a back edge is a topological refusal, both after the bytes already shipped. `UnionCase.Members` is non-empty for the same reason: the projector's member fold reduces from its first element.
- Entry: `public static Fin<GraduationEvidence> Admit(Seq<OwnerDescriptor> owners)` — the caller supplies the roster and the bundle mints its own `SchemaVersion` and `BundleKey`, so neither is a claim a caller can spell wrong. `public Fin<ReadOnlyMemory<byte>> Bundle(JsonTypeInfo<GraduationEvidence> contract)` writes the canonical UTF-8 payload under an injected contract; on the wire `BundleKey` crosses as its bare 32-hex render (`$"{BundleKey:x32}"`, decoded `NumberStyles.HexNumber` — the estate content-key text law; a raw `UInt128` JSON number breaks double-precision consumers), and the scalar leaf's payload property pins `"scalar"` because CamelCase would seat it on the `"kind"` discriminator STJ refuses.
- Auto: `Admit` refuses an empty roster, a blank or duplicated owner name, a blank or duplicated field name within one owner, an unsound node anywhere in a tree, an unresolvable `Nested.Ref`, and a cyclic owner graph proved by peeling every reference-free owner until either the graph empties or a pass settles nothing. `Render` is the one catamorphism over the tree: it feeds the length-framed preimage `ContentHash.Of` keys, so bundle identity and the shape it describes cannot disagree, and a field-order or scalar-row change re-keys the bundle the companion pins its round-trip against.
- Receipt: none — the bundle is an artifact, not a measured run, and its content key is the identity the writing composition indexes it under.
- Packages: System.Text.Json, System.IO.Hashing, Generator.Equals (`[Equatable]` diff rail — `BundleKey` ignored as derived, the gate stays the content key), Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm (project, `Domain.ContentHash`), BCL inbox
- Growth: a new wire primitive is one `FieldScalar` row the projector absorbs with one table row of its own; a new composite shape is one `FieldNode` case with its `[JsonDerivedType]` literal, one `Render` arm, one `Sound` arm, and one `Refs` arm; a new bundle column is one record field beside a `SchemaVersion` bump at both ends in one change.
- Boundary: this owner mints descriptors and never decodes them — the projection back into typed stubs is the companion's, and nothing here imports a peer-runtime shape. Serialization rides the `Runtime/receipts#RECEIPT_UNION` `ComputeWireContext` Strict resolver through an INJECTED `JsonTypeInfo<GraduationEvidence>`, so the LanguageExt carrier factory that populates every `Seq<T>` column registers once at that owner and this page holds no serializer, no options handle, and no second context. `SchemaVersion` is a gate rather than a column a caller fills: a bundle outside the carried set rails at the companion's decode band, so minting one outside it here ships bytes guaranteed to refuse. Seam law: the branch `[GRADUATION]` edge runs REVERSE-ONLY from this end — the forward leg is the companion's handoff axis, this leg is the evidence answering it, and neither end references the other's types.

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
    // "scalar" pins the payload property: CamelCase would seat `Kind` on the "kind" discriminator STJ refuses
    // to double-book, and the python companion decodes the leaf under "scalar" beside the case literal.
    public sealed record Scalar(string Name, [property: JsonPropertyName("scalar")] FieldScalar Kind) : FieldNode(Name);

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

// `[Equatable]` is the DIFF rail: `BundleKey` stays the gate (content addressing is the framed XxHash128
// projection, never GetHashCode) and is IGNORED as derived state, so `Inequalities` explains WHICH owner or
// field moved when two bundles' keys disagree — a diff, never a second identity.
[Equatable]
public sealed partial record GraduationEvidence(string SchemaVersion, [property: OrderedEquality] Seq<OwnerDescriptor> Owners, [property: IgnoreEquality] UInt128 BundleKey) {
    // Projectors CARRY exactly this version and rail on anything else rather than best-effort decoding a
    // drifted shape, so a bundle minted outside it ships bytes guaranteed to refuse.
    public const string Schema = "1";

    // The bundle is a CODEGEN INPUT, so every defect it carries must arrive in one answer: a companion that cannot
    // decode used to receive an owner COUNT for four distinct violations across N owners and M fields, and fixing
    // one defect only revealed the next. Roster-grain facts accumulate beside a per-owner traverse, so a malformed
    // roster names every owner and every field that is wrong.
    public static Fin<GraduationEvidence> Admit(Seq<OwnerDescriptor> owners) =>
        (guard(!owners.IsEmpty, (Error)IdentityRefusal.OwnerRosterMalformed.Fault()),
         guard(
             owners.Map(static owner => owner.Name).ToFrozenSet(StringComparer.Ordinal).Count == owners.Count,
             (Error)IdentityRefusal.OwnerRosterMalformed.Fault()),
         owners.Traverse(static owner => Wellformed(owner)).As())
        .Apply(static (_, _, _) => unit).As().ToFin()
        .Bind(_ => Resolvable(owners))
        .Map(_ => new GraduationEvidence(Schema, owners, KeyOf(owners)));

    static Validation<Error, Unit> Wellformed(OwnerDescriptor owner) =>
        (guard(!string.IsNullOrWhiteSpace(owner.Name), (Error)IdentityRefusal.OwnerRosterMalformed.Fault()),
         guard(
             owner.Fields.Map(static field => field.Name).ToFrozenSet(StringComparer.Ordinal).Count == owner.Fields.Count,
             (Error)IdentityRefusal.OwnerRosterMalformed.Fault()),
         owner.Fields.Traverse(field => guard(
             field.Sound(), (Error)IdentityRefusal.OwnerRosterMalformed.Fault())).As())
        .Apply(static (_, _, _) => unit).As();

    public Fin<ReadOnlyMemory<byte>> Bundle(JsonTypeInfo<GraduationEvidence> contract) =>
        Op.Of(name: "model.graduation-bundle-write").Catch(() => Fin.Succ((ReadOnlyMemory<byte>)JsonSerializer.SerializeToUtf8Bytes(this, contract)));

    // Owner graph is a DAG by contract: the projector registers each struct against already-built siblings, so a
    // reference naming no owner is an unbound name and a back edge is a topological refusal — both AFTER the bytes
    // shipped. Acyclicity is the admitted graph package's own predicate over BARE EDGES, needing no container at
    // all; the hand Kahn peel it replaces re-scanned its settled set per pass, lost the cycle members' order, and
    // recursed to the owner count while claiming no depth to bound. The two facts are independent and accumulate,
    // so a roster that is both unbound and cyclic names each.
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

    // Bundle identity folds through the KERNEL canonical writer: `Rows` count-frames the owner and field runs and
    // `String` frames every name with its own byte count, so a separator inside an owner or field name can never
    // shift two distinct rosters onto one key — the same framing law the option fingerprint reads, at one owner
    // rather than a second length-prefix loop per hashing surface.
    static UInt128 KeyOf(Seq<OwnerDescriptor> owners) => ContentHash.Of(owners, static (roster, writer) => writer
        .String(Schema)
        .Rows(roster, static (owner, rows) => rows
            .String(owner.Name)
            .Rows(owner.Fields, static (field, fields) => fields.String(field.Render()))));
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
