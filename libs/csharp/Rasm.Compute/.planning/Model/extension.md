# [COMPUTE_EXTENSION_OPS]

Rasm.Compute model extension-ops: one `CustomOps` registration fold over the extensions bundle and the custom-op library rows plus the string-tensor output boundary (empty-slot allocator AND egress reader). The page owns the `CustomOps` registration-and-string-boundary fold; registration extends the `Model/sessions#SESSION_CAPSULE` `ModelSessions` boundary capsule and rides `Microsoft.ML.OnnxRuntime.Extensions`/`Microsoft.ML.OnnxRuntime`, the `SessionPolicy` lifecycle record arrives settled from `Model/sessions#SESSION_CAPSULE`, the native-asset evidence rides the `Model/identity#MODEL_IDENTITY` `ModelLoad` receipt, and the string INGRESS rides `Model/inference#INFERENCE_MODES` `RunInput.Strings`. The `CustomOps.Register`/`StringSlots`/`StringEgress` fold crosses to `Model/sessions#SESSION_CAPSULE` as the one registration step the open fold composes.

## [01]-[INDEX]

- [01]-[EXTENSION_OPS]: extension and custom-op registration with asset evidence; bidirectional string-tensor boundary (ingress and egress).

## [02]-[EXTENSION_OPS]

- Owner: `CustomOps` — one registration fold over the extensions bundle and the custom-op library rows, plus the string-tensor output boundary (empty-slot allocator AND egress reader); string INGRESS rides `RunInput.Strings` on the inference owner, never a second string-input factory here.
- Cases: `RegisterOrtExtensions` bundle row; `RegisterCustomOpLibraryV2` per-path rows; `StringSlots` empty-output allocator, `StringEgress` element reader.
- Entry: `public static Fin<SessionOptions> Register(SessionOptions options, SessionPolicy policy)` — `Fin` aborts with `ExtensionAssetMissing` naming every absent native asset before any registration runs.
- Receipt: native-asset evidence rides the ModelLoad receipt; the missing-path set is the fault payload.
- Packages: Microsoft.ML.OnnxRuntime.Extensions, Microsoft.ML.OnnxRuntime, LanguageExt.Core, BCL inbox
- Growth: a new custom-op library is one path row on `SessionPolicy.CustomOpLibraries`; zero new surface.
- Boundary: registration extends the `Model/sessions#SESSION_CAPSULE` `ModelSessions` boundary capsule and this fence carries language-owned statement forms — guard admission before registration and the out-parameter custom-op handle; `RegisterOrtExtensions()` faults `OnnxRuntimeException(ErrorCode.NoSuchFile)` if the `libortextensions` native asset is absent, so the asset-presence guard precedes registration; tokenizer and pre/post operators stay session assets — a preprocessing or tokenizer service family is the rejected form; the `String` dtype is a model-boundary-only row entering through `Tensor<string>` via the `Model/inference#INFERENCE_MODES` `RunInput.Strings` admission case (`OrtValue.CreateFromStringTensor(Tensor<string>)`) on the inference owner, the empty-string output slots allocated here through `CreateTensorWithEmptyStrings`, and leaving here through the element-wise `GetStringElement(index)` reader projected over the flat element count on egress — a string-tensor model (tokenizer, postproc, detokenizer) needs the full round-trip so the string egress is the catalogued completion of the `RunInput` ingress, never a duplicate string-input factory and never the interior tensor vocabulary; `RegisterCustomOpLibrary(path)` (no handle) is the deleted spelling because `RegisterCustomOpLibraryV2(path, out nint)` carries the unload handle.

```csharp signature
public static class CustomOps {
    public static Fin<SessionOptions> Register(SessionOptions options, SessionPolicy policy) {
        var missing = policy.CustomOpLibraries.Filter(static path => !File.Exists(path));
        if (!missing.IsEmpty) {
            return Fin.Fail<SessionOptions>(new ComputeFault.ExtensionAssetMissing(string.Join(';', missing)));
        }
        if (policy.OrtExtensions) {
            options.RegisterOrtExtensions();
        }
        policy.CustomOpLibraries.Iter(path => options.RegisterCustomOpLibraryV2(path, out _));
        return Fin.Succ(options);
    }

    public static OrtValue StringSlots(OrtAllocator allocator, long[] shape) =>
        OrtValue.CreateTensorWithEmptyStrings(allocator, shape);

    public static Seq<string> StringEgress(OrtValue value) {
        var count = checked((int)value.GetTensorTypeAndShape().ElementCount);
        return toSeq(Enumerable.Range(0, count)).Map(value.GetStringElement);
    }
}
```
