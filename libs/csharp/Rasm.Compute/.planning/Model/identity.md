# [COMPUTE_IDENTITY]

Rasm.Compute model identity: ONNX model identity and provenance — the checksum-derived `ModelIdentity` record with nested `Slot` schema rows and the `CustomMetadata`/`Initializers` self-description channels, the `ModelSource` four-case acquisition union collapsing to one byte admission, and the one shared `ModelFingerprint` ordinal-keyvalue projection both this owner and the execution-provider axis ride. The page owns the `ModelSource`/`ModelIdentity`/`ModelFingerprint` vocabulary and the `Snapshot`/`Accepts`/`Initializer` admission fold; identity derives from the model bytes through `System.IO.Hashing` `XxHash128`/`XxHash3`, the slot schema reads `Microsoft.ML.OnnxRuntime` `InferenceSession` metadata, the `ModelLoad` receipt rides the `ComputeReceipt` rail, and `NodaTime` `Instant` and the Persistence `ArtifactIndexRow` arrive settled. The `ModelIdentity`/`ModelFingerprint`/`Slot` shapes cross to `Model/sessions#SESSION_CAPSULE`, `Model/providers#EP_AXIS`, `Model/inference#INFERENCE_MODES`, and `Model/generative#GENERATIVE_RUN` as settled vocabulary, and the `Checksum` is the deterministic cache and result-key seed `Model/inference#RESULT_CACHE` consumes.

## [1]-[INDEX]

- [1]-[MODEL_IDENTITY]: checksum identity; acquisition union; schema snapshot; admission law; custom-metadata and initializer admission; shared ordinal-keyvalue fingerprint.

## [2]-[MODEL_IDENTITY]

- Owner: `ModelIdentity` identity record with nested `Slot` schema rows and the `CustomMetadata`/`Initializers` self-description channels; `ModelSource` `[Union]` four acquisition cases collapsing to one byte admission; `ModelFingerprint` the one shared ordinal-keyvalue projection both this owner and `ExecutionProvider` ride.
- Cases: `LocalFile`, `EmbeddedResource`, `PersistenceBlob`, `RemoteFetch`.
- Entry: `public static ModelIdentity Snapshot(ModelSource source, ReadOnlySpan<byte> bytes, InferenceSession session, Instant at)` — pure value; identity derives from the bytes, never from the caller.
- Auto: `Snapshot` stamps the XxHash128 identity checksum, graph version, the input/output slot rows, the `ModelMetadata.CustomMetadataMap` self-description channel, and the `OverridableInitializerMetadata` deployment-constant slots in one call; `Accepts` runs once at load over the input slot rows and `Initializer` admits the deployment-constant `OrtValue` against the overridable-initializer slot it targets, faulting `ModelRejected` with mismatch evidence; per-call re-validation is the deleted form because admission settles the contract once; the custom-metadata fingerprint rides `ModelFingerprint.Of` — the single ordinal-keyvalue `XxHash3` projection the EP option-hash also rides, so the checksum body is declared once.
- Receipt: the ModelLoad receipt case carries checksum, source case, slot counts, the custom-metadata fingerprint, and elapsed; emission rides the sink port at the composition edge.
- Packages: Microsoft.ML.OnnxRuntime, System.IO.Hashing, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Persistence (project)
- Growth: a new acquisition route is one case on `ModelSource`; a new deployment-constant binding is one `Initializer` admission against the overridable-initializer slot; zero new surface.
- Boundary: every downstream cache key, receipt, and claim derives from `Checksum` — path-keyed or filename-keyed model identity is the deleted form; `Slot.FreeDims` rows drive the free-dimension overrides at session build, with symbolic-dim values arriving from the geometry-encoding rows as settled vocabulary; the `CustomMetadata` channel is the artifact's self-description read once at snapshot (and its fingerprint joins the session fingerprint so a re-trained model with identical bytes-prefix but different metadata keys distinctly), the `Initializers` slot table carries the `OverridableInitializerMetadata` deployment constants admitted through `AddInitializer(string, OrtValue)` at session build — never per-run inputs — and schema admission happens exactly once at load; the ordinal-keyvalue fingerprint is `ModelFingerprint.Of`, the one projection `ExecutionProvider.Hash` and `MetadataFingerprint` both compose — a re-derived `XxHash3.HashToUInt64(UTF8(string.Join(';', ...)))` body in either owner is the named defect.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ModelSource {
    private ModelSource() { }

    public sealed record LocalFile(string Path) : ModelSource;

    public sealed record EmbeddedResource(Assembly Assembly, string Name) : ModelSource;

    public sealed record PersistenceBlob(ArtifactIndexRow Row) : ModelSource;

    public sealed record RemoteFetch(string ArtifactId) : ModelSource;
}

public static class ModelFingerprint {
    public static ulong Of(IEnumerable<KeyValuePair<string, string>> rows) =>
        XxHash3.HashToUInt64(Encoding.UTF8.GetBytes(string.Join(';',
            rows.OrderBy(static row => row.Key, StringComparer.Ordinal).Select(static row => $"{row.Key}={row.Value}"))));
}

public sealed record ModelIdentity(
    UInt128 Checksum,
    long GraphVersion,
    Seq<ModelIdentity.Slot> Inputs,
    Seq<ModelIdentity.Slot> Outputs,
    Seq<ModelIdentity.Slot> Initializers,
    FrozenDictionary<string, string> CustomMetadata,
    ModelSource Source,
    Instant AcquiredAt) {
    public sealed record Slot(string Name, TensorElementType Dtype, Seq<int> Dims, Seq<string> FreeDims);

    public string Key => $"{Checksum:x32}";

    public ulong MetadataFingerprint => ModelFingerprint.Of(CustomMetadata);

    public static ModelIdentity Snapshot(ModelSource source, ReadOnlySpan<byte> bytes, InferenceSession session, Instant at) =>
        new(
            XxHash128.HashToUInt128(bytes),
            session.ModelMetadata.Version,
            Slots(session.InputMetadata),
            Slots(session.OutputMetadata),
            Slots(session.OverridableInitializerMetadata),
            session.ModelMetadata.CustomMetadataMap.ToFrozenDictionary(StringComparer.Ordinal),
            source,
            at);

    public Fin<Unit> Accepts(Seq<(string Name, TensorElementType Dtype, int Rank)> binding) =>
        binding.Filter(slot => !Inputs.Exists(own =>
            StringComparer.Ordinal.Equals(own.Name, slot.Name)
            && own.Dtype == slot.Dtype
            && own.Dims.Count == slot.Rank)).IsEmpty
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new ComputeFault.ModelRejected(Key));

    public Fin<(string Name, OrtValue Value)> Initializer(string name, OrtValue value) =>
        Initializers.Find(slot => StringComparer.Ordinal.Equals(slot.Name, name)).Case is Slot slot
        && slot.Dtype == value.GetTensorTypeAndShape().ElementDataType
            ? Fin.Succ((name, value))
            : Fin.Fail<(string, OrtValue)>(new ComputeFault.ModelRejected($"{Key}:initializer:{name}"));

    static Seq<Slot> Slots(IReadOnlyDictionary<string, NodeMetadata> nodes) =>
        toSeq(nodes).Map(static pair => new Slot(
            pair.Key,
            pair.Value.ElementDataType,
            toSeq(pair.Value.Dimensions),
            toSeq(pair.Value.SymbolicDimensions)));
}
```
