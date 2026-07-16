# [COMPUTE_EXTENSION_OPS]

Rasm.Compute model extension-ops: one `CustomOps` owner folds extension and custom-op registration into the `Model/sessions#SESSION_CAPSULE` admission AND reads the non-tensor model boundary the custom-op lane produces — `string`-tensor outputs and the structured `ZipMap` sequence/map outputs the numeric tensor egress cannot carry. ONNX Runtime owns the custom-op library lifetime through `RegisterCustomOpLibrary(path)`, freed when the `SessionOptions` and every session built from them release, so registration tracks no caller handle; the `out`-handle `RegisterCustomOpLibraryV2(path, out nint)` whose discarded handle leaks the library is the rejected form.

Registration extends the `ModelSessions` boundary capsule and rides `Microsoft.ML.OnnxRuntime.Extensions`/`Microsoft.ML.OnnxRuntime`; `SessionPolicy` arrives settled from `Model/sessions#SESSION_CAPSULE`, native-asset evidence rides the `Model/identity#MODEL_IDENTITY` `ModelLoad` receipt, and string INGRESS rides `Model/inference#INFERENCE_MODES` `RunInput.Strings`. Non-tensor `Egress` is the catalogued completion of that ingress: `RunInput.Strings` admits a `Tensor<string>` through the `Tensor/residency` `TensorBridge.Ingress` `OrtValue.CreateFromStringTensor` factory (the sole `OrtValue` C-data factory, never re-minted here), and the `OnnxType`-discriminated `Egress` reads the model's non-tensor outputs back — never a second string-input factory and never the interior `System.Numerics.Tensors` carrier, because a string tensor is a model-boundary `Microsoft.ML.OnnxRuntime.Tensors.Tensor<string>` only.

## [01]-[INDEX]

- [01]-[EXTENSION_OPS]: extension/custom-op registration with asset evidence and ORT-managed library lifetime; the polymorphic non-tensor `Egress` reader over `string`-tensor and `ZipMap` sequence/map outputs; the guarded bound string-output allocator.

## [02]-[EXTENSION_OPS]

- Owner: `CustomOps` — the registration fold over the extensions bundle and the custom-op library rows (ORT-managed lifetime, no caller handle), the guarded bound string-output allocator `StringSlots`, and the polymorphic non-tensor `Egress` projecting an output `OrtValue` onto the `OpOutput` `[Union]` by `OnnxValueType`; string INGRESS rides `RunInput.Strings` on the inference owner, never a second string-input factory here.
- Cases: registration arms `RegisterOrtExtensions` (the bundle, gated on `SessionPolicy.OrtExtensions`) and `RegisterCustomOpLibrary` per `SessionPolicy.CustomOpLibraries` path; `OpOutput` egress cases `Strings` (an `ONNX_TYPE_TENSOR` of `String` → shaped `Tensor<string>`), `Mapping` (one `ONNX_TYPE_MAP` label→score), `Batched` (an `ONNX_TYPE_SEQUENCE` of maps — the `ZipMap` classifier output).
- Entry: `public static Fin<SessionOptions> Register(SessionOptions options, SessionPolicy policy)` — `Fin` aborts with `ExtensionAssetMissing` naming every absent custom-op path before any registration runs, then converts the bundle's native `OnnxRuntimeException` to the same typed fault; `public Fin<OpOutput> Egress()` on `OrtValue` projects a non-tensor output and faults `ModelRejected` on an `OnnxType` the boundary does not model.
- Receipt: native-asset evidence rides the `Model/identity#MODEL_IDENTITY` `ModelLoad` receipt; the missing-path set (or the native fault message) is the `ExtensionAssetMissing` payload.
- Packages: Microsoft.ML.OnnxRuntime.Extensions, Microsoft.ML.OnnxRuntime, LanguageExt.Core, BCL inbox
- Growth: a new custom-op library is one path row on `SessionPolicy.CustomOpLibraries`; a new non-tensor output kind is one `OpOutput` case plus one `OnnxType` arm on `Egress`; zero new surface.
- Boundary: `CustomOps` extends the `Model/sessions#SESSION_CAPSULE` `ModelSessions` capsule and this fence carries the language-owned statement and native-disposal forms the capsule admits (per `boundaries.md` CAPSULE_OWNER) — asset guard precedes registration, the bundle fault converts at the seam, and every child `OrtValue` a map or sequence yields is read inside a `using`. `RegisterCustomOpLibrary(path)` hands the library to ONNX Runtime, freed when the `SessionOptions` and derived sessions release, so a partial-failure `options.Dispose()` on the `Lease` rail still frees every registered library and the `out`-handle `RegisterCustomOpLibraryV2(path, out _)` whose discarded handle leaks is the rejected spelling. `RegisterOrtExtensions()` faults `OnnxRuntimeException(ErrorCode.NoSuchFile)` when the `libortextensions` asset is absent, so the registration brackets both arms and lifts that native fault into `ExtensionAssetMissing`; path-supplied libraries are pre-guarded with `File.Exists` for a precise multi-path payload, and the bundle's absence — an OS-resolved native asset with no managed path to probe — is caught at the bracket. Tokenizer and pre/post operators stay session assets entered through this one registration call — a preprocessing or tokenizer service family is the rejected form, and there is no managed op-discovery surface because those operators are wholly native. `Egress` is the non-tensor reader: a `String`-typed `ONNX_TYPE_TENSOR` reconstructs a shaped `Microsoft.ML.OnnxRuntime.Tensors.DenseTensor<string>` from `GetStringTensorAsArray()` plus the `GetTensorTypeAndShape().Shape` extents (one bulk read, never an element-wise `GetStringElement(index)` loop, shape-preserving so the egress is symmetric with the `RunInput.Strings` ingress); an `ONNX_TYPE_MAP` reads its key (`Int64` or `String`, normalized to a label) and `Float` score child values into label→score pairs; an `ONNX_TYPE_SEQUENCE` folds one map per element into the `ZipMap` rows; a numeric tensor or an `ONNX_TYPE_OPTIONAL`/`ONNX_TYPE_SPARSETENSOR` is `ModelRejected`, because numeric egress is the inference owner's `GetTensorDataAsSpan<T>` and is never duplicated here. `StringSlots` allocates pre-bound `string`-output slots through `CreateTensorWithEmptyStrings` for the fixed-extent IO-binding case and faults `ModelRejected` on a symbolic or negative extent — a runtime-allocated dynamic string output routes straight through `Egress`, so `RegisterCustomOpLibrary(path)` is the canonical spelling and the legacy out-handle path is the only rejected registration form.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OpOutput {
    private OpOutput() { }

    public sealed record Strings(Microsoft.ML.OnnxRuntime.Tensors.Tensor<string> Text) : OpOutput;

    public sealed record Mapping(Seq<(string Label, float Score)> Pairs) : OpOutput;

    public sealed record Batched(Seq<Seq<(string Label, float Score)>> Rows) : OpOutput;
}

public static class CustomOps {
    public static Fin<SessionOptions> Register(SessionOptions options, SessionPolicy policy) {
        var missing = policy.CustomOpLibraries.Filter(static path => !File.Exists(path));
        if (!missing.IsEmpty) {
            return Fin.Fail<SessionOptions>(new ComputeFault.ExtensionAssetMissing(string.Join(';', missing)));
        }
        try {
            if (policy.OrtExtensions) { options.RegisterOrtExtensions(); }
            policy.CustomOpLibraries.Iter(options.RegisterCustomOpLibrary);
            return Fin.Succ(options);
        }
        catch (OnnxRuntimeException native) {
            return Fin.Fail<SessionOptions>(new ComputeFault.ExtensionAssetMissing(native.Message));
        }
    }

    public static Fin<OrtValue> StringSlots(OrtAllocator allocator, long[] shape) =>
        Array.Exists(shape, static extent => extent < 0)
            ? Fin.Fail<OrtValue>(new ComputeFault.ModelRejected($"string-slots-symbolic:{string.Join('x', shape)}"))
            : Fin.Succ(OrtValue.CreateTensorWithEmptyStrings(allocator, shape));

    extension(OrtValue value) {
        public Fin<OpOutput> Egress() =>
            value.OnnxType switch {
                OnnxValueType.ONNX_TYPE_TENSOR when value.GetTensorTypeAndShape().ElementDataType is TensorElementType.String =>
                    Fin.Succ<OpOutput>(new OpOutput.Strings(new DenseTensor<string>(
                        value.GetStringTensorAsArray(),
                        Array.ConvertAll(value.GetTensorTypeAndShape().Shape, static extent => (int)extent)))),
                OnnxValueType.ONNX_TYPE_MAP =>
                    Fin.Succ<OpOutput>(new OpOutput.Mapping(Pairs(value))),
                OnnxValueType.ONNX_TYPE_SEQUENCE =>
                    Fin.Succ<OpOutput>(new OpOutput.Batched(toSeq(Enumerable.Range(0, value.GetValueCount())).Map(index => {
                        using var map = value.GetValue(index, OrtAllocator.DefaultInstance);
                        return Pairs(map);
                    }))),
                _ => Fin.Fail<OpOutput>(new ComputeFault.ModelRejected($"non-tensor-egress:{value.OnnxType}")),
            };
    }

    static Seq<(string Label, float Score)> Pairs(OrtValue map) {
        using var keys = map.GetValue(0, OrtAllocator.DefaultInstance);
        using var values = map.GetValue(1, OrtAllocator.DefaultInstance);
        var labels = keys.GetTensorTypeAndShape().ElementDataType is TensorElementType.String
            ? toSeq(keys.GetStringTensorAsArray())
            : toSeq(keys.GetTensorDataAsSpan<long>().ToArray()).Map(static key => key.ToString(CultureInfo.InvariantCulture));
        return labels.Zip(toSeq(values.GetTensorDataAsSpan<float>().ToArray()), static (label, score) => (label, score));
    }
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
