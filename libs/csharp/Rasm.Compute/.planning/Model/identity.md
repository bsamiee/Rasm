# [COMPUTE_IDENTITY]

Rasm.Compute model identity owns ONNX provenance and the content address every downstream cache key, receipt, and claim derives from. `ModelIdentity` carries the checksum, recursive `SlotShape` schema trees, the `Provenance` producer/domain/graph/description block, and the `CustomMetadata`/`Initializers` self-description channels; `ModelSource` folds five acquisition cases to one byte admission through `Acquire` and projects the receipt source string through `Origin`; `ModelFingerprint` owns the length-framed ordinal-keyvalue projection composed by the execution-provider axis; `GraduationEnvelope` owns serving-population drift admission and evaluation over the graduation evidence every offline-learned model crosses with. Admission settles the model, input, initializer, and drift contracts once.

Identity derives from the model bytes through the kernel seed-zero `XxHash128` entry `Rasm.Domain.ContentHash.Of`, the workspace's one hasher — shared with the geometry `GeometryHash`, the seam `ContentAddress`, and the Persistence `ArtifactIndexRow`/`ModelResultIndex` spine — while `ModelFingerprint` rides `System.IO.Hashing` `XxHash3`; slot schema reads `Microsoft.ML.OnnxRuntime` `InferenceSession` metadata; `ModelLoad` rides the `ComputeReceipt` rail. `NodaTime` `Instant`, the AppHost `CorrelationId` and sink port, the `Runtime/receipts#RECEIPT_UNION` spine, and the Persistence `ArtifactIndexRow` arrive settled. `ModelIdentity`/`ModelFingerprint`/`Slot` cross to `Model/sessions#SESSION_CAPSULE`, `Model/providers#EP_AXIS`, `Model/inference#INFERENCE_MODES`, and `Model/generative#GENERATIVE_RUN` as settled vocabulary, `Checksum` is the deterministic cache and result-key seed `Model/inference#RESULT_CACHE` consumes, and `DriftVerdict.Breached` is the reuse-invalidation signal that same cache consumes as an `EquivalenceMiss` fault.

## [01]-[INDEX]

- [01]-[MODEL_IDENTITY]: checksum identity; five-case acquisition union with the byte-resolution fold; kind-discriminated schema snapshot with provenance; admission over input slots and overridable initializers; custom-metadata self-description; shared length-framed ordinal-keyvalue fingerprint; the graduation drift sentinel; `ModelLoad` receipt mint.

## [02]-[MODEL_IDENTITY]

- Owner: `ModelIdentity` identity record with nested `Slot` rows over recursive `SlotShape` tensor, sparse-tensor, sequence, map, and optional cases; `Provenance` owns the producer/domain/graph/description block and `CustomMetadata`/`Initializers` the self-description channels; `ModelSource` `[Union]` owns five acquisition cases whose `Acquire` fold resolves each through injected `SourceResolver` ports and whose `Origin` projects the receipt source class; `ModelFingerprint` owns the length-framed ordinal-keyvalue projection composed by `ExecutionProvider.ResultKey`; `GraduationEnvelope` owns admitted per-feature PSI bands and `DriftPolicy` owns ordered thresholds.
- Cases: `ModelSource` cases `LocalFile`, `EmbeddedResource`, `PersistenceBlob`, `RemoteFetch`, `Buffer`; `DriftVerdict` cases `Stable`, `Drifting`, `Breached`.
- Entry: `public static Fin<ModelIdentity> Snapshot(ModelSource source, ReadOnlySpan<byte> bytes, InferenceSession session, Instant at)` — metadata topology and model bytes admit together; identity derives from the bytes.
- Auto: `Snapshot` traverses input, output, and overridable-initializer metadata into recursive `SlotShape` trees; the three independent slot sets lift to K-kinded `Validation<Error>` legs, accumulate through tuple `Apply`, and rejoin `Fin<ModelIdentity>` once, so simultaneous schema faults survive one admission. An unknown ONNX kind faults its slot instead of entering a dtype/default ghost. `Acquire` folds five source cases through `Try.lift(...).Run().Bind(identity)`; typed resolver faults survive while throwing file/resource boundaries become `ModelRejected`. `Accepts` requires each dense-tensor input exactly once and rejects unknown names, negative offered extents, dtype drift, rank drift, and fixed-extent drift, while non-tensor payloads route through `Model/extension#EXTENSION_OPS`; `Initializer` applies the same tensor discriminant and exact-shape gate. `ModelFingerprint.Of` length-frames each ordinal key and value through one disposed incremental `XxHash3`. `GraduationEnvelope.Admit` rejects empty, duplicated, non-finite, non-monotonic, mis-sized, or non-normalized bands; `DriftPolicy` carries the minimum serving population and probability floor beside its ordered thresholds; `Drift` rejects duplicate, missing, undersized, and non-finite feature samples before returning the worst PSI verdict with its evidence and sample count.
- Receipt: `ModelLoad` — the `Runtime/receipts#RECEIPT_UNION` `ModelLoad(checksum, source, ep, version)` shape — is minted by `LoadReceipt` from this owner's `Key`, `Source.Origin`, and snapshotted `GraphVersion` plus the loader's `ExecutionProvider`, correlation, and elapsed; emission rides the sink port at the composition edge. A `Breached` drift verdict faults `ComputeFault.EquivalenceMiss` at the consuming lane — correctness gates reuse exactly as it gates session admission, and a fast stale surrogate is the worst reused object.
- Packages: Microsoft.ML.OnnxRuntime, System.IO.Hashing, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm (project, `Domain.ContentHash`), Rasm.AppHost (project), Rasm.Persistence (project)
- Growth: a new acquisition route is one `ModelSource` case plus its `Acquire` arm and `Origin` projection; a new ONNX value kind is one `SlotShape` case and one `ShapeOf` arm; a new deployment-constant binding composes `Initializer`; a new self-description axis is one `Provenance` field; a new drift statistic is one `Band` column folded into `Drift`.
- Boundary: every downstream cache key, receipt, and claim derives from `Checksum`; `Checksum` composes the kernel seed-zero `ContentHash.Of(bytes)` owner shared with geometry, seam content addresses, and Persistence indexes. `Acquire` owns file and manifest-resource statement boundaries and brackets expected I/O faults into `ComputeFault.ModelRejected`; injected `SourceResolver` ports keep blob storage and HTTP transport outside this owner. `SlotShape` preserves the full recursive ONNX value topology: `SequenceMetadata.ElementMeta`, `OptionalMetadata.ElementMeta`, and `MapMetadata.KeyDataType`/`ValueMetadata` recurse until a dense or sparse tensor leaf. `Accepts` treats negative model dims as free axes but rejects negative offered extents and requires the complete dense-input set; `Initializer` requires an exact dense-tensor shape instead of accepting a different rank with the same element count. `CustomMetadata` mints no second identity because its bytes already participate in `Checksum`; `Initializers` remain deployment constants bound through `AddInitializer(string, OrtValue)`. `ModelLoad` carries derivable `ModelMetadata.Version`, while `GraduationEnvelope` admits evidence-keyed normalized bands and returns an evidence-bearing typed verdict only after every serving population crosses the policy sample floor.

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
        Seq<Slot> expected = Inputs.Filter(static slot => slot.Shape is SlotShape.Tensor);
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

    public ComputeReceipt.ModelLoad LoadReceipt(ExecutionProvider ep, CorrelationId correlation, Duration elapsed) =>
        new(Key, Source.Origin, ep, GraphVersion) {
            Scope = new ReceiptScope.Execution(correlation, WorkLane.Background, Substrate.Onnx, AllocationClass.NativeOrt, elapsed),
        };

    static bool Conforms(Slot slot, (string Name, TensorElementType Dtype, Seq<long> Shape) binding) =>
        slot.Shape is SlotShape.Tensor tensor
        && StringComparer.Ordinal.Equals(slot.Name, binding.Name)
        && tensor.Dtype == binding.Dtype
        && binding.Shape.ForAll(static dim => dim >= 0)
        && DimsConform(tensor.Dims, binding.Shape);

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

[Union]
public abstract partial record DriftVerdict {
    private DriftVerdict() { }

    public sealed record Stable(UInt128 EvidenceKey, string Feature, double Psi, int SampleCount) : DriftVerdict;

    public sealed record Drifting(UInt128 EvidenceKey, string Feature, double Psi, int SampleCount) : DriftVerdict;

    public sealed record Breached(UInt128 EvidenceKey, string Feature, double Psi, int SampleCount) : DriftVerdict;
}

[ComplexValueObject]
public sealed partial class DriftPolicy {
    public double DriftingPsi { get; }

    public double BreachPsi { get; }

    public int MinimumSamples { get; }

    public double ProbabilityFloor { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double driftingPsi,
        ref double breachPsi,
        ref int minimumSamples,
        ref double probabilityFloor) =>
        validationError = double.IsFinite(driftingPsi)
            && double.IsFinite(breachPsi)
            && driftingPsi >= 0.0
            && breachPsi > driftingPsi
            && minimumSamples > 0
            && double.IsFinite(probabilityFloor)
            && probabilityFloor > 0.0
            && probabilityFloor < 1.0
            ? null
            : new ValidationError(message: $"<drift-policy:{driftingPsi}:{breachPsi}:{minimumSamples}:{probabilityFloor}>");
}

public sealed record GraduationEnvelope(UInt128 EvidenceKey, Seq<GraduationEnvelope.Band> Bands) {
    // BinEdges are the graduation-time feature quantile cuts; BinMass the reference mass per bin, both fitted by the Python companion, never here.
    public sealed record Band(string Feature, Seq<double> BinEdges, Seq<double> BinMass);

    public static Fin<GraduationEnvelope> Admit(UInt128 evidenceKey, Seq<Band> bands) =>
        guard(
            evidenceKey != UInt128.Zero && !bands.IsEmpty && bands.ForAll(band => Valid(band, bands)),
            new ComputeFault.ModelRejected($"<graduation-envelope:{evidenceKey:x32}>"))
        .ToFin()
        .Map(_ => new GraduationEnvelope(evidenceKey, bands));

    public Fin<DriftVerdict> Drift(Seq<(string Feature, ReadOnlyMemory<double> Sample)> serving, DriftPolicy policy) {
        bool unique = serving.Map(static row => row.Feature).ToFrozenSet(StringComparer.Ordinal).Count == serving.Count;
        return guard(
                unique && serving.Count == Bands.Count,
                new ComputeFault.ModelRejected($"<drift-population:unique={unique}:expected={Bands.Count}:found={serving.Count}>"))
            .ToFin()
            .Bind(_ => Bands.TraverseM(band => serving.Find(row => StringComparer.Ordinal.Equals(row.Feature, band.Feature))
                    .ToFin(new ComputeFault.ModelRejected($"<drift-feature-missing:{band.Feature}>"))
                    .Bind(row => Score(band, row.Sample.Span, policy)))
                .As())
            .Map(scores => scores.Reduce(static (worst, score) => score.Psi > worst.Psi ? score : worst))
            .Map<DriftVerdict>(worst => worst.Psi >= policy.BreachPsi
                ? new DriftVerdict.Breached(EvidenceKey, worst.Feature, worst.Psi, worst.SampleCount)
                : worst.Psi >= policy.DriftingPsi
                    ? new DriftVerdict.Drifting(EvidenceKey, worst.Feature, worst.Psi, worst.SampleCount)
                    : new DriftVerdict.Stable(EvidenceKey, worst.Feature, worst.Psi, worst.SampleCount));
    }

    static Fin<(string Feature, double Psi, int SampleCount)> Score(Band band, ReadOnlySpan<double> sample, DriftPolicy policy) =>
        guard(
            sample.Length >= policy.MinimumSamples && TensorPrimitives.IsFiniteAll(sample),
            new ComputeFault.ModelRejected($"<drift-sample:{band.Feature}>"))
        .ToFin()
        .Map(_ => (band.Feature, Psi(band, sample, policy.ProbabilityFloor), sample.Length));

    static bool Valid(Band band, Seq<Band> all) =>
        !string.IsNullOrWhiteSpace(band.Feature)
        && all.Count(candidate => StringComparer.Ordinal.Equals(candidate.Feature, band.Feature)) == 1
        && band.BinMass.Count == band.BinEdges.Count + 1
        && band.BinEdges.ForAll(double.IsFinite)
        && band.BinEdges.Zip(band.BinEdges.Tail).ForAll(static pair => pair.Item1 < pair.Item2)
        && band.BinMass.ForAll(static mass => double.IsFinite(mass) && mass > 0.0)
        && Math.Abs(band.BinMass.Fold(0.0, static (sum, mass) => sum + mass) - 1.0) <= 1e-9;

    static double Psi(Band band, ReadOnlySpan<double> sample, double probabilityFloor) {
        double[] mass = new double[band.BinMass.Count];
        foreach (double value in sample) {
            mass[Math.Min(band.BinEdges.Count(edge => value >= edge), mass.Length - 1)] += 1.0;
        }
        double total = sample.Length;
        return band.BinMass.Zip(toSeq(mass)).Fold(0.0, (psi, pair) => {
            (double reference, double observed) = pair;
            double p = double.Max(reference, probabilityFloor);
            double q = double.Max(observed / total, probabilityFloor);
            return psi + (q - p) * Math.Log(q / p);
        });
    }
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
