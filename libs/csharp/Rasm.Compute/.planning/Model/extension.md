# [COMPUTE_EXTENSION_OPS]

Rasm.Compute model extension-ops: one `CustomOps` owner folds extension and custom-op registration into the `Model/sessions#SESSION_CAPSULE` admission AND reads the non-tensor model boundary the custom-op lane produces — `string`-tensor outputs and the structured `ZipMap` sequence/map outputs the numeric tensor egress cannot carry. ONNX Runtime owns the custom-op library lifetime through `RegisterCustomOpLibrary(path)`, freed when the `SessionOptions` and every session built from them release, so registration tracks no caller handle; the `out`-handle `RegisterCustomOpLibraryV2(path, out nint)` whose discarded handle leaks the library is the rejected form.

Registration extends the `ModelSessions` boundary capsule and rides `Microsoft.ML.OnnxRuntime.Extensions`/`Microsoft.ML.OnnxRuntime`; `SessionPolicy` arrives settled from `Model/sessions#SESSION_CAPSULE`, native-asset evidence rides the `Model/identity#MODEL_IDENTITY` `ModelLoad` receipt, and string INGRESS rides `Model/run#RUN_MODES` `RunInput.Strings`. Non-tensor `Egress` is the catalogued completion of that ingress: `RunInput.Strings` admits a `Tensor<string>` through the `Tensor/residency` `TensorBridge.Ingress` `OrtValue.CreateFromStringTensor` factory (the sole `OrtValue` C-data factory, never re-minted here), and the `OnnxType`-discriminated `Egress` reads the model's non-tensor outputs back — never a second string-input factory and never the interior `System.Numerics.Tensors` carrier, because a string tensor is a model-boundary `Microsoft.ML.OnnxRuntime.Tensors.Tensor<string>` only.

## [01]-[INDEX]

- [02]-[EXTENSION_OPS]: extension/custom-op registration with asset evidence and ORT-managed library lifetime; the recursive non-tensor `Egress` reader closing coverage over every container and leaf the identity snapshot can carry; the guarded bound string-output allocator.

## [02]-[EXTENSION_OPS]

- Owner: `CustomOps` — the registration fold over the `CustomOpLibrary` roster (ORT-managed lifetime, no caller handle), the overflow-safe `Extent` product both extent gates read, the guarded bound string-output allocator `StringSlots`, and the recursive non-tensor `Egress` projecting an output `OrtValue` onto the `OpOutput` `[Union]` by `OnnxValueType` and proving it against the declared `SlotShape` through `Covers`; `EgressRefusal` names this owner's shared contract refusals without a string-key roster; string INGRESS rides `RunInput.Strings` on the inference owner, never a second string-input factory here.
- Cases: registration targets are `SessionPolicy.CustomOpLibrary` rows — `Bundled` reaching `RegisterOrtExtensions` and `Asset` reaching `RegisterCustomOpLibrary(path)` — so the bundle rides the same roster, the same probe, and the same resident fingerprint every other library does; `OpOutput` egress cases `Strings` (an `ONNX_TYPE_TENSOR` of `String` → shaped `Tensor<string>`), `Numeric` (every other tensor leaf, carried as its own dtype, shape, and owned byte run), `Mapping` (one `ONNX_TYPE_MAP` typed-key→typed-value roster), recursive `Sequence` over `ONNX_TYPE_SEQUENCE` elements, and recursive `Optional` over zero-or-one value; nested `MapKey` cases retain `String` and `Int64` identity without text coercion, and nested `MapValue` cases `Real` and `Whole` keep the value domain the map declared.
- Law: coverage is CLOSED against the identity snapshot, and `Egress` PROVES it. `Model/identity#MODEL_IDENTITY` `SlotShape` admits sequence and map slots by NAME because a slot describes a shape while a value carries one, and this grammar is where that value proves — the declared slot rides into `Egress` and `Covers` is the joint gate, because a coverage law stated in prose closes nothing. Every `SlotShape` case reachable from a snapshot therefore has a disposition here: a tensor leaf reads through `Strings` or `Numeric`, a sequence recurses through `Sequence`, a map reads through `Mapping`, an optional recurses through `Optional`, and a sparse tensor NESTED in a container refuses by name. Anything short of that leaves a schema the admitter accepts with no reader at run, which is the asymmetry that made a legal `seq(tensor(float))` output an unexplained refusal.
- Law: the sparse carve is OUTER-LEVEL routing, not a gap. A sparse output at the top is the caller's own undisposed value and crosses whole to `Tensor/residency#ORT_BRIDGE`; a sparse child dies with the container walk, and a sparse value's three-buffer residency is that owner's shape rather than a byte run this page could copy out. Reopens only on the residency owner publishing an owned sparse snapshot.
- Entry: `Register` accumulates asset probes before `Op.Catch` admits native registration; `Egress` captures native metadata reads and proves the projected value against its declared slot.
- Receipt: native-asset evidence rides the `Model/identity#MODEL_IDENTITY` `ModelLoad` receipt; the missing-path set (or the native fault message) is the `ExtensionAssetMissing` payload.
- Packages: Microsoft.ML.OnnxRuntime.Extensions, Microsoft.ML.OnnxRuntime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new custom-op library is one `CustomOpLibrary` row on `SessionPolicy.CustomOpLibraries` and a new registration MECHANISM is one case on that union, carrying its own probe, identity, and fingerprint column; a new ONNX value kind is one `OpOutput` case with one `OnnxType` arm on `Egress`, landing beside the `SlotShape` case the identity snapshot grows in the same change; a new map-value domain is one `MapValue` case and one `Valued` arm. Container growth costs nothing: `Sequence` and `Optional` recurse through the one reader, so a sequence of sequences of maps reads today.
- Boundary: `CustomOps` extends the session capsule; `RegisterCustomOpLibrary` transfers lifetime to its `SessionOptions`, and each `OrtValue` child is read within its native lease.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------

// Named sites select bounded contracts directly; no string-key roster survives beneath the shared violation.
public static class EgressRefusal {
    public static readonly ContractRefusal SlotsSymbolic = new(ComputeArea.Model, ComputeContract.Compatible);
    public static readonly ContractRefusal ValueUnmodelled = new(ComputeArea.Model, ComputeContract.Supported);
    public static readonly ContractRefusal ShapeSymbolic = new(ComputeArea.Model, ComputeContract.Compatible);
    public static readonly ContractRefusal CardinalityMismatched = new(ComputeArea.Model, ComputeContract.Compatible);
    public static readonly ContractRefusal OptionalCardinality = new(ComputeArea.Model, ComputeContract.Compatible);
    public static readonly ContractRefusal MapShapeRejected = new(ComputeArea.Model, ComputeContract.Compatible);
    public static readonly ContractRefusal MapKeyUnmodelled = new(ComputeArea.Model, ComputeContract.Supported);
    public static readonly ContractRefusal MapValueUnmodelled = new(ComputeArea.Model, ComputeContract.Supported);
    public static readonly ContractRefusal MapValueDegenerate = new(ComputeArea.Model, ComputeContract.Valid);
    public static readonly ContractRefusal SlotUnadmitted = new(ComputeArea.Model, ComputeContract.Compatible);

}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OpOutput {
    private OpOutput() { }

    [Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
    public abstract partial record MapKey {
        private MapKey() { }
        public sealed record String(string Value) : MapKey;
        public sealed record Int64(long Value) : MapKey;
    }

    // `Real` carries `Float` and `Double` alike because a float widens onto a double EXACTLY; `Whole` keeps `Int64`
    // in its own case because that widening does not — a 64-bit label past 2^53 rounds and reads back as another.
    [Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
    public abstract partial record MapValue {
        private MapValue() { }
        public sealed record Real(double Value) : MapValue;
        public sealed record Whole(long Value) : MapValue;
    }

    public sealed record Strings(Microsoft.ML.OnnxRuntime.Tensors.Tensor<string> Text) : OpOutput;

    public sealed record Numeric(TensorElementType Dtype, Seq<long> Shape, ReadOnlyMemory<byte> Raw) : OpOutput;

    public sealed record Mapping(Seq<(MapKey Key, MapValue Value)> Pairs) : OpOutput;

    // ONE recursive container case covers every sequence shape ONNX admits — of maps (the `ZipMap` classifier
    // output), of tensors, of sequences, of optionals — where a maps-only case refused a legal `seq(tensor(float))`
    // output as an unmodelled element and forced a second case per element kind.
    public sealed record Sequence(Seq<OpOutput> Elements) : OpOutput;

    public sealed record Optional(Option<OpOutput> Value) : OpOutput;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class CustomOps {
    // Assets accumulate: `.Traverse` runs EVERY probe and `Error.Combine` unions the whole absent set, so a
    // deployment missing three libraries learns all three — a monadic traverse aborted at the first and made the
    // card's promise to name every absent or replaced asset false for every case after it.
    public static Fin<SessionOptions> Register(SessionOptions options, SessionPolicy policy) =>
        policy.CustomOpLibraries.Traverse(static library => library.Verify().ToValidation()).As().ToFin()
            .Bind(_ => Op.Of(name: "model.custom-op-register").Catch(() => {
                policy.CustomOpLibraries.Iter(library => library.Switch(
                    bundled: _ => options.RegisterOrtExtensions(),
                    asset: asset => options.RegisterCustomOpLibrary(asset.Path)));
                return Fin.Succ(options);
            }));

    // The overflow fold is the SHAPE's own arithmetic and answers `Option`: a `-1L` sentinel threaded through an
    // `Aggregate` had to be re-tested at every read, and the two call sites spelled the test differently — one
    // re-checking `extent < 0L` the other had already pre-guarded.
    static Option<long> Extent(ReadOnlySpan<long> shape) {
        long elements = 1L;
        foreach (long extent in shape) {
            if (extent < 0L || (extent is not 0L && elements > long.MaxValue / extent)) { return None; }
            elements *= extent;
        }
        return Some(elements);
    }

    public static Fin<OrtValue> StringSlots(OrtAllocator allocator, long[] shape) =>
        Extent(shape)
            .ToFin(EgressRefusal.SlotsSymbolic.Fault())
            .Bind(_ => Op.Of(name: "model.string-slots").Catch(() => Fin.Succ(OrtValue.CreateTensorWithEmptyStrings(allocator, shape))));

    extension(OrtValue value) {
        // The declared SLOT is the coverage law made executable: `Model/identity#MODEL_IDENTITY` admits sequence
        // and map slots BY NAME because a slot describes a shape while a value carries one, and this is where the
        // value proves. Every `SlotShape` case the snapshot can carry has a disposition here, so a schema the
        // admitter accepted cannot reach a run with no reader — the arm set was closed by prose alone before, and
        // prose closes nothing.
        public Fin<OpOutput> Egress(SlotShape declared) =>
            Op.Of(name: "model.egress").Catch(() => EgressAdmitted(value))
                .Bind(output => Covers(declared, output)
                    ? Fin.Succ(output)
                    : Fin.Fail<OpOutput>(EgressRefusal.SlotUnadmitted.Fault()));
    }

    // The slot and the value are ONE joint discriminant. A nested sparse refuses BY NAME here rather than at the
    // byte copy, and a slot that admitted a sequence cannot read back a map.
    static bool Covers(SlotShape declared, OpOutput output) => (Slot: declared, Value: output) switch {
        (SlotShape.Tensor tensor, OpOutput.Strings) => tensor.Dtype is TensorElementType.String,
        (SlotShape.Tensor tensor, OpOutput.Numeric numeric) => tensor.Dtype == numeric.Dtype,
        (SlotShape.Map, OpOutput.Mapping) => true,
        (SlotShape.Sequence sequence, OpOutput.Sequence elements) => elements.Elements.ForAll(element => Covers(sequence.Element, element)),
        (SlotShape.Optional optional, OpOutput.Optional held) => held.Value.Match(Some: value => Covers(optional.Element, value), None: static () => true),
        _ => false,
    };

    // Child values die with the walk, so every recursion re-enters `EgressAdmitted` rather than the bracketed
    // `Egress`: one outer `Try` already owns every native read beneath it, and a per-level bracket would classify
    // one native fault at whichever depth it happened to surface.
    static Fin<OpOutput> EgressAdmitted(OrtValue value) => value.OnnxType switch {
        OnnxValueType.ONNX_TYPE_TENSOR => Dense(value, value.GetTensorTypeAndShape()),
        OnnxValueType.ONNX_TYPE_MAP => Pairs(value).Map(static pairs => (OpOutput)new OpOutput.Mapping(pairs)),
        OnnxValueType.ONNX_TYPE_SEQUENCE => Elements(value),
        OnnxValueType.ONNX_TYPE_OPTIONAL => Optional(value),
        // Outer-level sparse is the caller's own undisposed value and crosses whole to the residency owner; a sparse
        // CHILD has no such life, and its three-buffer layout is not a byte run this page can copy out.
        OnnxValueType.ONNX_TYPE_SPARSETENSOR => Fin.Fail<OpOutput>(new ComputeFault.Violation(ComputeArea.Model, new ComputeViolation.Unsupported(ComputeCapability.SparseTensor))),
        OnnxValueType unmodeled => Fin.Fail<OpOutput>(EgressRefusal.ValueUnmodelled.Fault()),
    };

    // Payload leaves as an OWNED byte copy under its declared dtype and shape: the copy is forced by the child's
    // lifetime, and carrying the bytes uninterpreted is what keeps every dtype dispatch at the residency owner
    // instead of minting a second numeric extraction on a page that exists for the non-tensor boundary. The dense
    // split is one ternary the string branch owns, not a hop.
    static Fin<OpOutput> Dense(OrtValue value, OrtTensorTypeAndShapeInfo info) =>
        info.ElementDataType is TensorElementType.String
            ? Strings(value, info)
            : toSeq(info.Shape).Exists(static extent => extent < 0L)
                ? Fin.Fail<OpOutput>(EgressRefusal.ShapeSymbolic.Fault())
                : Fin.Succ<OpOutput>(new OpOutput.Numeric(
                    info.ElementDataType, toSeq(info.Shape), value.GetTensorMutableRawData().ToArray()));

    // Elements ACCUMULATE: a sequence with three unmodelled children names all three rather than the first, which
    // is the same law the asset probe takes and for the same reason.
    static Fin<OpOutput> Elements(OrtValue value) =>
        Range.fromMinMax(0, value.GetValueCount() - 1, 1)
            .AsIterable()
            .ToSeq()
            .Traverse(index => {
                using OrtValue element = value.GetValue(index, OrtAllocator.DefaultInstance);
                return EgressAdmitted(element).ToValidation();
            })
            .As()
            .ToFin()
            .Map(static elements => (OpOutput)new OpOutput.Sequence(elements));

    static Fin<OpOutput> Optional(OrtValue value) => value.GetValueCount() switch {
        0 => Fin.Succ<OpOutput>(new OpOutput.Optional(None)),
        1 => WithOptional(value),
        int count => Fin.Fail<OpOutput>(EgressRefusal.OptionalCardinality.Fault()),
    };

    // The `using` inside a switch arm is what the platform forbids in an expression body, so this hop exists to
    // hold the child's disposal bracket rather than to name a step.
    static Fin<OpOutput> WithOptional(OrtValue value) {
        using OrtValue element = value.GetValue(0, OrtAllocator.DefaultInstance);
        return EgressAdmitted(element).Map(static output => (OpOutput)new OpOutput.Optional(Some(output)));
    }

    static Fin<OpOutput> Strings(OrtValue value, OrtTensorTypeAndShapeInfo info) =>
        toSeq(info.Shape).Exists(static extent => extent is < 0L or > int.MaxValue)
            ? Fin.Fail<OpOutput>(EgressRefusal.ShapeSymbolic.Fault())
            : Extent(info.Shape)
                .ToFin(EgressRefusal.ShapeSymbolic.Fault())
                .Bind(elements => value.GetStringTensorAsArray() is string[] text && elements == text.LongLength
                    ? Fin.Succ<OpOutput>(new OpOutput.Strings(new DenseTensor<string>(text, Array.ConvertAll(info.Shape, static extent => (int)extent))))
                    : Fin.Fail<OpOutput>(EgressRefusal.CardinalityMismatched.Fault()));

    // Key and value dtypes are INDEPENDENT axes of one map, so each admits through its own arm and the shape gate
    // runs once over both — a value dtype pinned to `Float` inside the shape predicate refused a legal
    // `map(int64, double)` output with a shape message naming nothing that was wrong with its shape.
    // Key and value dtypes are INDEPENDENT axes of one map, so each admits through its own arm and both accumulate:
    // a `map(int32, complex64)` used to report an unmodelled key and hide the unmodelled value behind it, and a
    // value dtype pinned to `Float` inside the shape predicate refused a legal `map(int64, double)` with a shape
    // message naming nothing wrong with its shape. The three structural gates accumulate for the same reason.
    static Fin<Seq<(OpOutput.MapKey Key, OpOutput.MapValue Value)>> Pairs(OrtValue map) {
        if (map.GetValueCount() is not 2) {
            return Fin.Fail<Seq<(OpOutput.MapKey Key, OpOutput.MapValue Value)>>(
                EgressRefusal.MapShapeRejected.Fault());
        }
        using OrtValue keys = map.GetValue(0, OrtAllocator.DefaultInstance);
        using OrtValue values = map.GetValue(1, OrtAllocator.DefaultInstance);
        if (keys.OnnxType is not OnnxValueType.ONNX_TYPE_TENSOR || values.OnnxType is not OnnxValueType.ONNX_TYPE_TENSOR) {
            return Fin.Fail<Seq<(OpOutput.MapKey Key, OpOutput.MapValue Value)>>(
                EgressRefusal.MapShapeRejected.Fault());
        }
        OrtTensorTypeAndShapeInfo keyInfo = keys.GetTensorTypeAndShape();
        OrtTensorTypeAndShapeInfo valueInfo = values.GetTensorTypeAndShape();
        if (keyInfo.Shape is not [>= 0] || valueInfo.Shape is not [>= 0]
            || keyInfo.Shape[0] != valueInfo.Shape[0] || keyInfo.Shape[0] > int.MaxValue) {
            return Fin.Fail<Seq<(OpOutput.MapKey Key, OpOutput.MapValue Value)>>(EgressRefusal.MapShapeRejected.Fault());
        }
        int cardinality = (int)keyInfo.Shape[0];
        return (Keyed(keys, keyInfo.ElementDataType), Valued(values, valueInfo.ElementDataType))
            .Apply(static (admitted, carried) => (Keys: admitted, Values: carried)).As().ToFin()
            .Bind(read => Zip(read.Keys, read.Values, cardinality));
    }

    static Validation<Error, Seq<OpOutput.MapKey>> Keyed(OrtValue keys, TensorElementType dtype) => dtype switch {
        TensorElementType.String => Success<Error, Seq<OpOutput.MapKey>>(toSeq(keys.GetStringTensorAsArray())
            .Map(static key => (OpOutput.MapKey)new OpOutput.MapKey.String(key))),
        TensorElementType.Int64 => Success<Error, Seq<OpOutput.MapKey>>(toSeq(keys.GetTensorDataAsSpan<long>().ToArray())
            .Map(static key => (OpOutput.MapKey)new OpOutput.MapKey.Int64(key))),
        TensorElementType unmodeled =>
            Fail<Error, Seq<OpOutput.MapKey>>(EgressRefusal.MapKeyUnmodelled.Fault()),
    };

    static Validation<Error, Seq<OpOutput.MapValue>> Valued(OrtValue values, TensorElementType dtype) => dtype switch {
        TensorElementType.Float => Real(toSeq(values.GetTensorDataAsSpan<float>().ToArray()).Map(static value => (double)value)),
        TensorElementType.Double => Real(toSeq(values.GetTensorDataAsSpan<double>().ToArray())),
        TensorElementType.Int64 => Success<Error, Seq<OpOutput.MapValue>>(toSeq(values.GetTensorDataAsSpan<long>().ToArray())
            .Map(static value => (OpOutput.MapValue)new OpOutput.MapValue.Whole(value))),
        TensorElementType unmodeled =>
            Fail<Error, Seq<OpOutput.MapValue>>(EgressRefusal.MapValueUnmodelled.Fault()),
    };

    // Finiteness is a REAL-value law alone: an integral map value has no degenerate encoding to screen for, so
    // folding both through one guard would invent a check the integral domain cannot fail.
    static Validation<Error, Seq<OpOutput.MapValue>> Real(Seq<double> values) =>
        values.ForAll(double.IsFinite)
            ? Success<Error, Seq<OpOutput.MapValue>>(values.Map(static value => (OpOutput.MapValue)new OpOutput.MapValue.Real(value)))
            : Fail<Error, Seq<OpOutput.MapValue>>(EgressRefusal.MapValueDegenerate.Fault());

    static Fin<Seq<(OpOutput.MapKey Key, OpOutput.MapValue Value)>> Zip(
        Seq<OpOutput.MapKey> keys, Seq<OpOutput.MapValue> values, int cardinality) =>
        keys.Count == cardinality && values.Count == cardinality && keys.Distinct().Count == cardinality
            ? Fin.Succ(keys.Zip(values, static (key, value) => (key, value)))
            : Fin.Fail<Seq<(OpOutput.MapKey Key, OpOutput.MapValue Value)>>(
                EgressRefusal.CardinalityMismatched.Fault());
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
