using System.Runtime.CompilerServices;
using Rhino.Render;
using Rhino.Render.Fields;

namespace Rasm.Rhino.Render.Content;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ContentFields {
    // --- [FIELDS]
    public static IO<Option<T>> Read<T>(FieldDictionary fields, string name, Func<Field, Fin<T>> read) =>
        IO.lift(() => Optional(fields.GetField(name)).Traverse(read).As());

    public static IO<Seq<T>> Rows<T>(FieldDictionary fields, Func<Field, Fin<T>> read) =>
        IO.lift(() => toSeq(fields.Cast<Field>()).Map(field => IO.lift(() => read(field))).Strict()).Bind(Callbacks.Each);

    public static IO<T> WithinDynamicFields<T>(RenderContent content, bool automatic, IO<T> body) =>
        DisposalOps.Within(fun(() => content.BeginCreateDynamicFields(automatic)), _ => body, _ => IO.lift(content.EndCreateDynamicFields));

    public static IO<Unit> CreateDynamicField<T>(
        RenderContent content, string internalName, string localName, string englishName, T value, Option<(T Min, T Max)> bounds, int sectionId) =>
        IO.lift(() => Refused.Unless(bounds.Match(
                Some: range => content.CreateDynamicField(internalName, localName, englishName, value, range.Min, range.Max, sectionId),
                None: () => content.CreateDynamicField(internalName, localName, englishName, value, minValue: null, maxValue: null, sectionId)),
            nameof(RenderContent.CreateDynamicField)));

    // --- [PARAMETERS]
    public static IO<Option<T>> GetParameter<T>(RenderContent content, string parameter, Func<object, Fin<T>> read, Option<string> requirement = default) =>
        DisposalOps.Within(() => Optional(requirement.Match(
                Some: extra => content.GetExtraRequirementParameter(parameter, extra),
                None: () => content.GetParameter(parameter))),
            answer => IO.lift(() => answer.Match(
                Some: held => Optional(VariantType.IsInstanceOfType(held) ? AsObject(held) : held).Traverse(read).As(),
                None: static () => Fin.Succ(Option<T>.None))),
            static answer => IO.lift(() => answer.Bind(static held => Optional(held as IDisposable)).Iter(static held => held.Dispose())));

    public static IO<Unit> SetParameter(
        RenderContent content, string parameter, object? value, Option<string> requirement = default,
        RenderContent.ExtraRequirementsSetContexts context = RenderContent.ExtraRequirementsSetContexts.Program) =>
        requirement.Match(
            Some: extra => DisposalOps.Within(
                () => NewVariant(value),
                variant => IO.lift(() => Refused.Unless(content.SetExtraRequirementParameter(parameter, extra, variant, context), nameof(RenderContent.SetExtraRequirementParameter))),
                static variant => IO.lift(((IDisposable)variant).Dispose)),
            None: () => IO.lift(() => Refused.Unless(content.SetParameter(parameter, value), nameof(RenderContent.SetParameter))));

    // --- [NATIVE_VARIANT]
    private const string VariantTypeName = "Rhino.Render.Variant, RhinoCommon";
    private static readonly Type VariantType = Type.GetType(VariantTypeName, throwOnError: true)!;

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    [return: UnsafeAccessorType(VariantTypeName)]
    private static extern object NewVariant(object? value);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "AsObject")]
    private static extern object? AsObject([UnsafeAccessorType(VariantTypeName)] object variant);
}
