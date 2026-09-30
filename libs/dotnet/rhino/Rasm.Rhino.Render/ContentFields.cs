using System.Diagnostics;
using System.Globalization;
using Rasm.Rhino.Document;
using Rhino.Render;
using Rhino.Render.Fields;

namespace Rasm.Rhino.Render;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FieldValue {
    public sealed record Bool(bool Value) : FieldValue;

    public sealed record Int(int Value) : FieldValue;

    public sealed record Float(float Value) : FieldValue;

    public sealed record Double(double Value) : FieldValue;

    public sealed record Color4f(global::Rhino.Display.Color4f Value) : FieldValue;

    public sealed record Vector2d(global::Rhino.Geometry.Vector2d Value) : FieldValue;

    public sealed record Vector3d(global::Rhino.Geometry.Vector3d Value) : FieldValue;

    public sealed record Point2d(global::Rhino.Geometry.Point2d Value) : FieldValue;

    public sealed record Point3d(global::Rhino.Geometry.Point3d Value) : FieldValue;

    public sealed record Point4d(global::Rhino.Geometry.Point4d Value) : FieldValue;

    public sealed record String(string Value) : FieldValue;

    public sealed record DateTime(System.DateTime Value) : FieldValue;

    public sealed record Guid(System.Guid Value) : FieldValue;

    public sealed record Xform(Transform Value) : FieldValue;

    public sealed record ByteArray(Seq<byte> Value) : FieldValue;

    public sealed record Null() : FieldValue;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FieldPresentation {
    public sealed record DataOnly() : FieldPresentation;

    public sealed record Plain(string Prompt, int SectionId) : FieldPresentation;

    public sealed record Textured(string Prompt, int SectionId, bool TreatAsLinear) : FieldPresentation;

    public sealed record Filename(string Prompt, int SectionId) : FieldPresentation;
}

public sealed record DynamicFieldSpec(string InternalName, string LocalName, string EnglishName, FieldValue Value, Option<(FieldValue Min, FieldValue Max)> Bounds, int SectionId);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ParameterRef {
    public sealed record Named(string Parameter) : ParameterRef;

    public sealed record ExtraRequirement(string Parameter, string Requirement) : ParameterRef;
}

public sealed record FieldState(string Name, FieldValue Value, double TextureAmountMin, double TextureAmountMax, bool UseTextureOn, bool UseTextureAmount, bool IsHiddenInAutoUI);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ContentFields {
    // --- [DICTIONARY]
    public static IO<Field> Declare(FieldDictionary fields, string name, FieldValue value, FieldPresentation presentation) =>
        from absent in IO.lift(() => Invalid.Unless(!fields.ContainsField(name), nameof(FieldDictionary.ContainsField)))
        from field in IO.lift(() => presentation.Switch<(FieldDictionary Fields, string Name, FieldValue Value), Fin<Field>>(
            (fields, name, value),
            dataOnly: static (state, _) => state.Value is FieldValue.ByteArray bytes
                ? state.Fields.Add(state.Name, [.. bytes.Value])
                : Added(state.Fields, state.Name, state.Value, "", 0),
            plain: static (state, plain) => Added(state.Fields, state.Name, state.Value, plain.Prompt, plain.SectionId),
            textured: static (state, textured) => AddedTextured(state.Fields, state.Name, state.Value, textured.Prompt, textured.SectionId, textured.TreatAsLinear),
            filename: static (state, filename) => state.Value is FieldValue.String text
                ? state.Fields.AddFilename(state.Name, text.Value, filename.Prompt, filename.SectionId)
                : new Invalid(nameof(FieldDictionary.AddFilename))))
        select field;

    public static IO<Unit> Write(FieldDictionary fields, string name, FieldValue value) =>
        value.Switch<(FieldDictionary Fields, string Name), IO<Unit>>(
            (fields, name),
            @bool: static (state, flag) => IO.lift(() => state.Fields.Set(state.Name, flag.Value)),
            @int: static (state, whole) => IO.lift(() => state.Fields.Set(state.Name, whole.Value)),
            @float: static (state, single) => IO.lift(() => state.Fields.Set(state.Name, single.Value)),
            @double: static (state, wide) => IO.lift(() => state.Fields.Set(state.Name, wide.Value)),
            color4f: static (state, color) => IO.lift(() => state.Fields.Set(state.Name, color.Value)),
            vector2d: static (state, vector) => IO.lift(() => state.Fields.Set(state.Name, vector.Value)),
            vector3d: static (state, vector) => IO.lift(() => state.Fields.Set(state.Name, vector.Value)),
            point2d: static (state, point) => IO.lift(() => state.Fields.Set(state.Name, point.Value)),
            point3d: static (state, point) => IO.lift(() => state.Fields.Set(state.Name, point.Value)),
            point4d: static (state, point) => IO.lift(() => state.Fields.Set(state.Name, point.Value)),
            @string: static (state, text) => IO.lift(() => state.Fields.Set(state.Name, text.Value)),
            dateTime: static (state, date) => IO.lift(() => state.Fields.Set(state.Name, date.Value)),
            guid: static (state, id) => IO.lift(() => state.Fields.Set(state.Name, id.Value)),
            xform: static (state, xform) => IO.lift(() => state.Fields.Set(state.Name, xform.Value)),
            byteArray: static (state, bytes) => IO.lift(() => state.Fields.Set(state.Name, [.. bytes.Value])),
            @null: static (_, _) => IO.fail<Unit>(new Invalid(nameof(FieldValue.Null))))
        | @catch(static error => error.HasException<InvalidOperationException>(), static _ => IO.fail<Unit>(new Refused(nameof(FieldDictionary.Set))));

    public static IO<Option<FieldValue>> Read(FieldDictionary fields, string name) =>
        IO.lift(() => Optional(fields.GetField(name)).Map(ValueOf));

    public static IO<Seq<FieldState>> Fields(FieldDictionary fields) =>
        IO.lift(() => toSeq(fields.Cast<Field>())
            .Map(static field => new FieldState(field.Name, ValueOf(field), field.TextureAmountMin, field.TextureAmountMax, field.UseTextureOn, field.UseTextureAmount, field.IsHiddenInAutoUI))
            .Strict());

    private static Fin<Field> Added(FieldDictionary fields, string name, FieldValue value, string prompt, int sectionId) =>
        value.Switch<(FieldDictionary Fields, string Name, string Prompt, int Section), Fin<Field>>(
            (fields, name, prompt, sectionId),
            @bool: static (state, flag) => state.Fields.Add(state.Name, flag.Value, state.Prompt, state.Section),
            @int: static (state, whole) => state.Fields.Add(state.Name, whole.Value, state.Prompt, state.Section),
            @float: static (state, single) => state.Fields.Add(state.Name, single.Value, state.Prompt, state.Section),
            @double: static (state, wide) => state.Fields.Add(state.Name, wide.Value, state.Prompt, state.Section),
            color4f: static (state, color) => state.Fields.Add(state.Name, color.Value, state.Prompt, state.Section),
            vector2d: static (state, vector) => state.Fields.Add(state.Name, vector.Value, state.Prompt, state.Section),
            vector3d: static (state, vector) => state.Fields.Add(state.Name, vector.Value, state.Prompt, state.Section),
            point2d: static (state, point) => state.Fields.Add(state.Name, point.Value, state.Prompt, state.Section),
            point3d: static (state, point) => state.Fields.Add(state.Name, point.Value, state.Prompt, state.Section),
            point4d: static (state, point) => state.Fields.Add(state.Name, point.Value, state.Prompt, state.Section),
            @string: static (state, text) => state.Fields.Add(state.Name, text.Value, state.Prompt, state.Section),
            dateTime: static (state, date) => state.Fields.Add(state.Name, date.Value, state.Prompt, state.Section),
            guid: static (state, id) => state.Fields.Add(state.Name, id.Value, state.Prompt, state.Section),
            xform: static (state, xform) => state.Fields.Add(state.Name, xform.Value, state.Prompt, state.Section),
            byteArray: static (_, _) => new Invalid(nameof(FieldDictionary.Add)),
            @null: static (state, _) => state.Fields.Add(state.Name, state.Prompt, state.Section));

    private static Fin<Field> AddedTextured(FieldDictionary fields, string name, FieldValue value, string prompt, int sectionId, bool treatAsLinear) =>
        value.Switch<(FieldDictionary Fields, string Name, string Prompt, int Section, bool Linear), Fin<Field>>(
            (fields, name, prompt, sectionId, treatAsLinear),
            @bool: static (state, flag) => state.Fields.AddTextured(state.Name, flag.Value, state.Prompt, state.Linear, state.Section),
            @int: static (state, whole) => state.Fields.AddTextured(state.Name, whole.Value, state.Prompt, state.Linear, state.Section),
            @float: static (state, single) => state.Fields.AddTextured(state.Name, single.Value, state.Prompt, state.Linear, state.Section),
            @double: static (state, wide) => state.Fields.AddTextured(state.Name, wide.Value, state.Prompt, state.Linear, state.Section),
            color4f: static (state, color) => state.Fields.AddTextured(state.Name, color.Value, state.Prompt, state.Linear, state.Section),
            vector2d: static (state, vector) => state.Fields.AddTextured(state.Name, vector.Value, state.Prompt, state.Linear, state.Section),
            vector3d: static (state, vector) => state.Fields.AddTextured(state.Name, vector.Value, state.Prompt, state.Linear, state.Section),
            point2d: static (state, point) => state.Fields.AddTextured(state.Name, point.Value, state.Prompt, state.Linear, state.Section),
            point3d: static (state, point) => state.Fields.AddTextured(state.Name, point.Value, state.Prompt, state.Linear, state.Section),
            point4d: static (state, point) => state.Fields.AddTextured(state.Name, point.Value, state.Prompt, state.Linear, state.Section),
            @string: static (state, text) => state.Fields.AddTextured(state.Name, text.Value, state.Prompt, state.Linear, state.Section),
            dateTime: static (state, date) => state.Fields.AddTextured(state.Name, date.Value, state.Prompt, state.Linear, state.Section),
            guid: static (state, id) => state.Fields.AddTextured(state.Name, id.Value, state.Prompt, state.Linear, state.Section),
            xform: static (state, xform) => state.Fields.AddTextured(state.Name, xform.Value, state.Prompt, state.Linear, state.Section),
            byteArray: static (_, _) => new Invalid(nameof(FieldDictionary.AddTextured)),
            @null: static (state, _) => state.Fields.AddTextured(state.Name, state.Prompt, state.Linear, state.Section));

    private static FieldValue ValueOf(Field field) =>
        field switch {
            BoolField typed => new FieldValue.Bool(typed.Value),
            IntField typed => new FieldValue.Int(typed.Value),
            FloatField typed => new FieldValue.Float(typed.Value),
            DoubleField typed => new FieldValue.Double(typed.Value),
            Color4fField typed => new FieldValue.Color4f(typed.Value),
            Vector2dField typed => new FieldValue.Vector2d(typed.Value),
            Vector3dField typed => new FieldValue.Vector3d(typed.Value),
            Point2dField typed => new FieldValue.Point2d(typed.Value),
            Point3dField typed => new FieldValue.Point3d(typed.Value),
            Point4dField typed => new FieldValue.Point4d(typed.Value),
            StringField typed => new FieldValue.String(typed.Value),
            DateTimeField typed => new FieldValue.DateTime(typed.Value),
            GuidField typed => new FieldValue.Guid(typed.Value),
            TransformField typed => new FieldValue.Xform(typed.Value),
            ByteArrayField typed => new FieldValue.ByteArray(toSeq(typed.Value)),
            NullField => new FieldValue.Null(),
            _ => throw new UnreachableException(),
        };

    // --- [DYNAMIC]
    public static IO<Unit> DeclareDynamic(RenderContent content, bool automatic, Seq<DynamicFieldSpec> rows) =>
        IO.lift(() => content.BeginCreateDynamicFields(automatic)).Bracket(
            Use: _ => rows.TraverseM(row => IO.lift(() => Refused.Unless(
                    content.CreateDynamicField(
                        row.InternalName,
                        row.LocalName,
                        row.EnglishName,
                        Boxed(row.Value).ValueUnsafe(),
                        (from bounds in row.Bounds from min in Boxed(bounds.Min) select min).ValueUnsafe(),
                        (from bounds in row.Bounds from max in Boxed(bounds.Max) select max).ValueUnsafe(),
                        row.SectionId),
                    nameof(RenderContent.CreateDynamicField))))
                .As()
                .Map(static _ => unit),
            Fin: _ => IO.lift(content.EndCreateDynamicFields));

    private static Option<object> Boxed(FieldValue value) =>
        value.Switch(
            @bool: static flag => Some<object>(flag.Value),
            @int: static whole => Some<object>(whole.Value),
            @float: static single => Some<object>(single.Value),
            @double: static wide => Some<object>(wide.Value),
            color4f: static color => Some<object>(color.Value),
            vector2d: static vector => Some<object>(vector.Value),
            vector3d: static vector => Some<object>(vector.Value),
            point2d: static point => Some<object>(point.Value),
            point3d: static point => Some<object>(point.Value),
            point4d: static point => Some<object>(point.Value),
            @string: static text => Some<object>(text.Value),
            dateTime: static date => Some<object>(date.Value),
            guid: static id => Some<object>(id.Value),
            xform: static xform => Some<object>(xform.Value),
            byteArray: static bytes => Some<object>(bytes.Value.ToArray()),
            @null: static _ => Option<object>.None);

    // --- [PARAMETERS]
    public static IO<Option<TValue>> ReadParameter<TValue>(RenderContent content, ParameterRef parameter) =>
        IO.lift(() => parameter.Switch(
                content,
                named: static (target, named) => Optional(target.GetParameter(named.Parameter)),
                extraRequirement: static (target, extra) => Optional(target.GetExtraRequirementParameter(extra.Parameter, extra.Requirement))))
            .Bracket(
                Use: static answer => IO.lift(() => answer.Map(static data => (TValue)Convert.ChangeType(data, typeof(TValue), CultureInfo.InvariantCulture)))
                    .Catch(static error => error.HasException<NotImplementedException>(), static _ => IO.fail<Option<TValue>>(new Invalid(nameof(IConvertible.ToType)))),
                Fin: static held => DisposalOps.Release(held.Bind(static data => Optional(data as IDisposable)).ToSeq()));

    public static IO<Unit> WriteParameter(RenderContent content, string parameter, FieldValue value) =>
        from data in IO.lift(() => Boxed(value).ToFin(new Invalid(nameof(FieldValue.Null))))
        from written in IO.lift(() => Refused.Unless(content.SetParameter(parameter, data), nameof(RenderContent.SetParameter)))
        select written;

    public static IO<Unit> BindParameter(RenderContent content, ParameterRef parameter, Field field) =>
        from managed in IO.lift(() => Invalid.Unless(content.GetType().Assembly != typeof(RenderContent).Assembly, nameof(RenderContent.BindParameterToField)))
        from bound in IO.lift(() => parameter.Switch(
            (Content: content, Field: field),
            named: static (state, named) => state.Content.BindParameterToField(named.Parameter, state.Field, RenderContent.ChangeContexts.Program),
            extraRequirement: static (state, extra) => state.Content.BindParameterToField(extra.Parameter, extra.Requirement, state.Field, RenderContent.ChangeContexts.Program)))
        select bound;
}
