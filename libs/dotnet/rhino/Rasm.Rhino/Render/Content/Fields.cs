using System.Runtime.CompilerServices;
using Rhino.Render;
using Rhino.Render.Fields;

namespace Rasm.RhinoCommon.Render.Content;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FieldBounds<T>(T Min, T Max);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ContentFields {
    // --- [FIELDS]
    public static IO<Option<T>> Read<T>(FieldDictionary fields, string name, Func<Field, Fin<T>> read) =>
        IO.lift(() => Optional(fields.GetField(name)).Traverse(read).As());

    public static IO<Seq<T>> Rows<T>(FieldDictionary fields, Func<Field, Fin<T>> read) =>
        IO.lift(() => toSeq(fields.Cast<Field>()).Traverse(read).As());

    public static IO<T> WithinDynamicFields<T>(RenderContent content, bool automatic, IO<T> body) =>
        IO.lift(() => content.BeginCreateDynamicFields(automatic)) >> body.Finally(IO.lift(content.EndCreateDynamicFields));

    public static IO<Unit> CreateDynamicField<T>(
        RenderContent content, string internalName, string localName, string englishName, T value, Option<FieldBounds<T>> bounds, int sectionId) =>
        IO.lift(() => bounds.Match(
            Some: range => RhinoFault.Unless(content.CreateDynamicField(internalName, localName, englishName, value, range.Min, range.Max, sectionId)),
            None: () => RhinoFault.Unless(content.CreateDynamicField(internalName, localName, englishName, value, minValue: null, maxValue: null, sectionId))));

    // --- [PARAMETERS]
    public static IO<Option<T>> GetParameter<T>(RenderContent content, string parameter, Func<object, Fin<T>> read, Option<string> requirement = default) =>
        IO.lift(() => Optional(requirement.Match(
                Some: extra => content.GetExtraRequirementParameter(parameter, extra),
                None: () => content.GetParameter(parameter))))
            .Bind(answer => IO.lift(() => answer.Bind(static held => Optional(VariantType.IsInstanceOfType(held) ? AsObject(held) : held)).Traverse(read).As())
                .Finally(IO.lift(() => answer.Bind(static held => Optional(held as IDisposable)).Iter(static held => held.Dispose()))));

    public static IO<Unit> SetParameter(
        RenderContent content, string parameter, object? value, Option<string> requirement = default,
        RenderContent.ExtraRequirementsSetContexts context = RenderContent.ExtraRequirementsSetContexts.Program) =>
        requirement.Match(
            Some: extra => use(() => NewVariant(value), static variant => ((IDisposable)variant).Dispose())
                .Bind(variant => IO.lift(() => RhinoFault.Unless(content.SetExtraRequirementParameter(parameter, extra, variant, context))))
                .Bracket(),
            None: () => IO.lift(() => RhinoFault.Unless(content.SetParameter(parameter, value))));

    // --- [NATIVE_VARIANT]
    private const string VariantTypeName = "Rhino.Render.Variant, RhinoCommon";
    private static readonly Type VariantType = Type.GetType(VariantTypeName, throwOnError: true)!;

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    [return: UnsafeAccessorType(VariantTypeName)]
    private static extern object NewVariant(object? value);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "AsObject")]
    private static extern object? AsObject([UnsafeAccessorType(VariantTypeName)] object variant);
}
