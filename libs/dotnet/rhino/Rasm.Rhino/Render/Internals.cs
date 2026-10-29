using System.Runtime.CompilerServices;
using Rhino.Render;
using Rhino.Render.PostEffects;

namespace Rasm.Rhino.Render;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Internals {
    // --- [VARIANTS]
    private const string VariantName = "Rhino.Render.Variant, RhinoCommon";
    private static readonly Type VariantType = Type.GetType(VariantName, throwOnError: true)!;

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    [return: UnsafeAccessorType(VariantName)]
    internal static extern object NewVariant(object? value);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "AsObject")]
    private static extern object? AsObject([UnsafeAccessorType(VariantName)] object variant);

    internal static IO<Option<T>> Unwrapped<T>(Func<object?> read, string name) where T : notnull =>
        Products.Maybe(read, name).Bind(held => IO.lift(() => held.Bind(static value => VariantType.IsInstanceOfType(value) ? Optional(AsObject(value)) : Some(value))
                .Traverse(value => Conversions.Cast<T>(value, name)).As()))
            .Bracket();

    // --- [CONTENT]
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_AutoDelete")]
    internal static extern void AutoDelete(RenderContent content, bool autoDelete);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "RuntimeSerialNumber")]
    internal static extern ref int RuntimeSerialNumber(RenderContent content);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "Destroy")]
    internal static extern void Destroy(RenderContentSerializer serializer);

    // --- [RENDERING]
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "RhinoDocumentId")]
    internal static extern uint RhinoDocumentId([UnsafeAccessorType("Rhino.Render.Rendering, RhinoCommon")] object rendering);

    internal const PostEffectType ToneMappingStage = PostEffectType.ToneMapping + 1;
}
