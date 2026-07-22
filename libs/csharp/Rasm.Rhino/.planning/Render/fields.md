# [RASM_RHINO_RENDER_FIELDS]

`FieldValue` owns the typed RDK parameter algebra from admission through declaration, binding, mutation, recovery, and detached census. `FieldCarrier` declares the payload correspondence once, `FieldPresentation` and `FieldRange` carry declaration policy as values, `ParamScope` reaches every native parameter route, and `ChangeScope` brackets every write before any host carrier escapes.

## [01]-[INDEX]

- [02]-[VALUE]: `FieldValue` — the one polymorphic payload owner with write, recovery, and boxing dispatch.
- [03]-[DECLARATION]: `FieldPresentation` and `FieldSpec` — field declaration rows; `DynamicFieldSpec` — the dynamic-field bracket fold.
- [04]-[BINDING_AND_PARAMS]: `FieldBinding`, `ParamScope`, and the `FieldPortrait` census.
- [05]-[SURFACE_LEDGER]: page owner table.

## [02]-[VALUE]

- Owner: `FieldValue` closes the payload alternatives, and `FieldCarrier` rows bind each case to its host field type, raw payload type, boxing projection, range order, declaration delegates, write delegate, and recovery projection.
- Law: `FieldCarrier.Items` derives field-type and payload-type lookup from one correspondence; boxing and scalar-range admission dispatch through that same row, so ordinary carrier growth adds one value case and one behavior row.
- Law: `FieldCarrier.Declare` captures native declaration failures; `Bytes` uses the value-only `Add` overload and rejects textured or filename presentation.
- Law: `Null` recovers `NullField`, `DBNull.Value`, and `null` payloads — every shape its declared payload type routes to it — preserves a `NullField` census row, and boxes to `null` for object-typed parameter seams; payload-typed dictionary `Write` refuses because no `Set` overload exists for `NullField`.
- Boundary: `Color4f` rides the union as the host color seam value — field payloads are content-parameter truth, and a domain color composes the kernel `PerceptualColor` owner at the consumer that treats it as color, never inside the parameter carrier.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.Geometry;
using Rhino.Render;
using Rhino.Render.Fields;

namespace Rasm.Rhino.Render;

// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FieldValue : IDetachedDocumentResult {
    private FieldValue() { }
    public sealed record Toggle(bool Value) : FieldValue;
    public sealed record Whole(int Value) : FieldValue;
    public sealed record Single(float Value) : FieldValue;
    public sealed record Real(double Value) : FieldValue;
    public sealed record Colour(Color4f Value) : FieldValue;
    public sealed record Vec2(Vector2d Value) : FieldValue;
    public sealed record Vec3(Vector3d Value) : FieldValue;
    public sealed record Pt2(Point2d Value) : FieldValue;
    public sealed record Pt3(Point3d Value) : FieldValue;
    public sealed record Pt4(Point4d Value) : FieldValue;
    public sealed record Text(string Value) : FieldValue;
    public sealed record Stamp(DateTime Value) : FieldValue;
    public sealed record Key(Guid Value) : FieldValue;
    public sealed record Motion(Transform Value) : FieldValue;
    public sealed record Bytes(Arr<byte> Value) : FieldValue;
    public sealed record Null : FieldValue;

    internal FieldCarrier Carrier => FieldCarrier.Get(GetType());

    internal static Fin<FieldValue> Of(object? payload, Op key) =>
        FieldCarrier.Recover(payload: payload, key: key);

    internal Fin<Unit> Declare(FieldDeclaration declaration, Op key) =>
        Carrier.Declare(declaration: declaration, payload: Boxed(), key: key);

    internal Fin<Unit> Write(FieldDictionary fields, string name, Op key) =>
        Carrier.Write(fields: fields, name: name, payload: Boxed(), key: key);

    internal object? Boxed() => Carrier.Box(value: this);
}

public readonly record struct FieldDeclaration(
    FieldDictionary Fields,
    string Name,
    string Prompt,
    int Section,
    FieldPresentation Presentation);

[SmartEnum<Type>]
public sealed partial class FieldCarrier {
    public static readonly FieldCarrier Toggle = Row<FieldValue.Toggle, bool, BoolField>(
        key: typeof(FieldValue.Toggle), pack: static value => new FieldValue.Toggle(Value: value), unpack: static value => value.Value,
        read: static field => field.Value,
        plain: static (f, n, v, p, s) => f.Add(n, v, p, s), write: static (f, n, v) => f.Set(n, v),
        textured: static (f, n, v, p, l, s) => f.AddTextured(n, v, p, l, s));
    public static readonly FieldCarrier Whole = Row<FieldValue.Whole, int, IntField>(
        key: typeof(FieldValue.Whole), pack: static value => new FieldValue.Whole(Value: value), unpack: static value => value.Value,
        read: static field => field.Value,
        plain: static (f, n, v, p, s) => f.Add(n, v, p, s), write: static (f, n, v) => f.Set(n, v),
        textured: static (f, n, v, p, l, s) => f.AddTextured(n, v, p, l, s),
        ordered: static (lo, hi) => lo <= hi);
    public static readonly FieldCarrier Single = Row<FieldValue.Single, float, FloatField>(
        key: typeof(FieldValue.Single), pack: static value => new FieldValue.Single(Value: value), unpack: static value => value.Value,
        read: static field => field.Value,
        plain: static (f, n, v, p, s) => f.Add(n, v, p, s), write: static (f, n, v) => f.Set(n, v),
        textured: static (f, n, v, p, l, s) => f.AddTextured(n, v, p, l, s),
        ordered: static (lo, hi) => float.IsFinite(lo) && float.IsFinite(hi) && lo <= hi);
    public static readonly FieldCarrier Real = Row<FieldValue.Real, double, DoubleField>(
        key: typeof(FieldValue.Real), pack: static value => new FieldValue.Real(Value: value), unpack: static value => value.Value,
        read: static field => field.Value,
        plain: static (f, n, v, p, s) => f.Add(n, v, p, s), write: static (f, n, v) => f.Set(n, v),
        textured: static (f, n, v, p, l, s) => f.AddTextured(n, v, p, l, s),
        ordered: static (lo, hi) => double.IsFinite(lo) && double.IsFinite(hi) && lo <= hi);
    public static readonly FieldCarrier Colour = Row<FieldValue.Colour, Color4f, Color4fField>(
        key: typeof(FieldValue.Colour), pack: static value => new FieldValue.Colour(Value: value), unpack: static value => value.Value,
        read: static field => field.Value,
        plain: static (f, n, v, p, s) => f.Add(n, v, p, s), write: static (f, n, v) => f.Set(n, v),
        textured: static (f, n, v, p, l, s) => f.AddTextured(n, v, p, l, s));
    public static readonly FieldCarrier Vec2 = Row<FieldValue.Vec2, Vector2d, Vector2dField>(
        key: typeof(FieldValue.Vec2), pack: static value => new FieldValue.Vec2(Value: value), unpack: static value => value.Value,
        read: static field => field.Value,
        plain: static (f, n, v, p, s) => f.Add(n, v, p, s), write: static (f, n, v) => f.Set(n, v),
        textured: static (f, n, v, p, l, s) => f.AddTextured(n, v, p, l, s));
    public static readonly FieldCarrier Vec3 = Row<FieldValue.Vec3, Vector3d, Vector3dField>(
        key: typeof(FieldValue.Vec3), pack: static value => new FieldValue.Vec3(Value: value), unpack: static value => value.Value,
        read: static field => field.Value,
        plain: static (f, n, v, p, s) => f.Add(n, v, p, s), write: static (f, n, v) => f.Set(n, v),
        textured: static (f, n, v, p, l, s) => f.AddTextured(n, v, p, l, s));
    public static readonly FieldCarrier Pt2 = Row<FieldValue.Pt2, Point2d, Point2dField>(
        key: typeof(FieldValue.Pt2), pack: static value => new FieldValue.Pt2(Value: value), unpack: static value => value.Value,
        read: static field => field.Value,
        plain: static (f, n, v, p, s) => f.Add(n, v, p, s), write: static (f, n, v) => f.Set(n, v),
        textured: static (f, n, v, p, l, s) => f.AddTextured(n, v, p, l, s));
    public static readonly FieldCarrier Pt3 = Row<FieldValue.Pt3, Point3d, Point3dField>(
        key: typeof(FieldValue.Pt3), pack: static value => new FieldValue.Pt3(Value: value), unpack: static value => value.Value,
        read: static field => field.Value,
        plain: static (f, n, v, p, s) => f.Add(n, v, p, s), write: static (f, n, v) => f.Set(n, v),
        textured: static (f, n, v, p, l, s) => f.AddTextured(n, v, p, l, s));
    public static readonly FieldCarrier Pt4 = Row<FieldValue.Pt4, Point4d, Point4dField>(
        key: typeof(FieldValue.Pt4), pack: static value => new FieldValue.Pt4(Value: value), unpack: static value => value.Value,
        read: static field => field.Value,
        plain: static (f, n, v, p, s) => f.Add(n, v, p, s), write: static (f, n, v) => f.Set(n, v),
        textured: static (f, n, v, p, l, s) => f.AddTextured(n, v, p, l, s));
    public static readonly FieldCarrier Text = Row<FieldValue.Text, string, StringField>(
        key: typeof(FieldValue.Text), pack: static value => new FieldValue.Text(Value: value), unpack: static value => value.Value,
        read: static field => field.Value,
        plain: static (f, n, v, p, s) => f.Add(n, v, p, s), write: static (f, n, v) => f.Set(n, v),
        textured: static (f, n, v, p, l, s) => f.AddTextured(n, v, p, l, s));
    public static readonly FieldCarrier Stamp = Row<FieldValue.Stamp, DateTime, DateTimeField>(
        key: typeof(FieldValue.Stamp), pack: static value => new FieldValue.Stamp(Value: value), unpack: static value => value.Value,
        read: static field => field.Value,
        plain: static (f, n, v, p, s) => f.Add(n, v, p, s), write: static (f, n, v) => f.Set(n, v),
        textured: static (f, n, v, p, l, s) => f.AddTextured(n, v, p, l, s),
        ordered: static (lo, hi) => lo <= hi);
    public static readonly FieldCarrier Key = Row<FieldValue.Key, Guid, GuidField>(
        key: typeof(FieldValue.Key), pack: static value => new FieldValue.Key(Value: value), unpack: static value => value.Value,
        read: static field => field.Value,
        plain: static (f, n, v, p, s) => f.Add(n, v, p, s), write: static (f, n, v) => f.Set(n, v),
        textured: static (f, n, v, p, l, s) => f.AddTextured(n, v, p, l, s));
    public static readonly FieldCarrier Motion = Row<FieldValue.Motion, Transform, TransformField>(
        key: typeof(FieldValue.Motion), pack: static value => new FieldValue.Motion(Value: value), unpack: static value => value.Value,
        read: static field => field.Value,
        plain: static (f, n, v, p, s) => f.Add(n, v, p, s), write: static (f, n, v) => f.Set(n, v),
        textured: static (f, n, v, p, l, s) => f.AddTextured(n, v, p, l, s));
    public static readonly FieldCarrier Bytes = new(
        key: typeof(FieldValue.Bytes), fieldType: typeof(ByteArrayField), payloadType: typeof(byte[]),
        box: static value => value is FieldValue.Bytes bytes ? bytes.Value.ToArray() : null,
        acceptsRange: static (_, _) => false,
        read: static (payload, key) => payload switch {
            ByteArrayField field => Fin.Succ<FieldValue>(value: new FieldValue.Bytes(Value: toArr(field.Value))),
            byte[] value => Fin.Succ<FieldValue>(value: new FieldValue.Bytes(Value: toArr(value))),
            _ => Fin.Fail<FieldValue>(error: key.InvalidResult()),
        },
        declare: static (declaration, payload, key) => declaration.Presentation is FieldPresentation.Plain && payload is byte[] bytes
            ? key.Catch(() => { _ = declaration.Fields.Add(declaration.Name, bytes); return Fin.Succ(value: unit); })
            : Fin.Fail<Unit>(error: key.InvalidInput()),
        write: static (fields, name, payload, key) => payload is byte[] bytes
            ? key.Catch(() => { fields.Set(name, bytes); return Fin.Succ(value: unit); })
            : Fin.Fail<Unit>(error: key.InvalidInput()));
    public static readonly FieldCarrier Null = new(
        key: typeof(FieldValue.Null), fieldType: typeof(NullField), payloadType: typeof(DBNull),
        box: static _ => null,
        acceptsRange: static (_, _) => false,
        read: static (payload, key) => payload is NullField or DBNull or null
            ? Fin.Succ<FieldValue>(value: new FieldValue.Null())
            : Fin.Fail<FieldValue>(error: key.InvalidResult()),
        declare: static (declaration, _, key) => declaration.Presentation.Switch(
            state: (Declaration: declaration, Op: key),
            plain: static (ctx, _) => ctx.Op.Catch(() => { _ = ctx.Declaration.Fields.Add(ctx.Declaration.Name, ctx.Declaration.Prompt, ctx.Declaration.Section); return Fin.Succ(value: unit); }),
            textured: static (ctx, row) => ctx.Op.Catch(() => { _ = ctx.Declaration.Fields.AddTextured(ctx.Declaration.Name, ctx.Declaration.Prompt, row.TreatAsLinear, ctx.Declaration.Section); return Fin.Succ(value: unit); }),
            filename: static (ctx, _) => Fin.Fail<Unit>(error: ctx.Op.InvalidInput())),
        write: static (_, _, _, key) => Fin.Fail<Unit>(error: key.Unsupported(geometryType: typeof(FieldValue), outputType: typeof(Unit))));

    public Type FieldType { get; }
    public Type PayloadType { get; }

    [UseDelegateFromConstructor]
    internal partial object? Box(FieldValue value);

    [UseDelegateFromConstructor]
    internal partial bool AcceptsRange(FieldValue min, FieldValue max);

    [UseDelegateFromConstructor]
    internal partial Fin<FieldValue> Read(object? payload, Op key);

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> Declare(FieldDeclaration declaration, object? payload, Op key);

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> Write(FieldDictionary fields, string name, object? payload, Op key);

    internal static Fin<FieldValue> Recover(object? payload, Op key) =>
        payload is null
            ? Null.Read(payload: null, key: key)
            : toSeq(Items)
                .Filter(row => payload is Field ? row.FieldType == payload.GetType() : row.PayloadType == payload.GetType())
                .Head
                .ToFin(Fail: key.InvalidResult(detail: payload.GetType().Name))
                .Bind(row => row.Read(payload: payload, key: key));

    private static FieldCarrier Row<TCase, T, TField>(
        Type key,
        Func<T, TCase> pack,
        Func<TCase, T> unpack,
        Func<TField, T> read,
        Func<FieldDictionary, string, T, string, int, Field> plain,
        Func<FieldDictionary, string, T, string, bool, int, Field> textured,
        Action<FieldDictionary, string, T> write,
        Func<T, T, bool>? ordered = null)
        where TCase : FieldValue
        where TField : Field =>
        new(
            key: key,
            fieldType: typeof(TField),
            payloadType: typeof(T),
            box: value => value is TCase typed ? unpack(typed) : null,
            acceptsRange: (min, max) => ordered is not null
                && min is TCase lower
                && max is TCase upper
                && ordered(unpack(lower), unpack(upper)),
            read: (payload, op) => payload switch {
                TField field => Fin.Succ<FieldValue>(value: pack(read(field))),
                T value => Fin.Succ<FieldValue>(value: pack(value)),
                _ => Fin.Fail<FieldValue>(error: op.InvalidResult()),
            },
            declare: (declaration, payload, op) => payload is T value
                ? declaration.Presentation.Switch(
                    state: (Declaration: declaration, Value: value, Op: op, Plain: plain, Textured: textured),
                    plain: static (ctx, _) => ctx.Op.Catch(() => { _ = ctx.Plain(ctx.Declaration.Fields, ctx.Declaration.Name, ctx.Value, ctx.Declaration.Prompt, ctx.Declaration.Section); return Fin.Succ(value: unit); }),
                    textured: static (ctx, row) => ctx.Op.Catch(() => { _ = ctx.Textured(ctx.Declaration.Fields, ctx.Declaration.Name, ctx.Value, ctx.Declaration.Prompt, row.TreatAsLinear, ctx.Declaration.Section); return Fin.Succ(value: unit); }),
                    filename: static (ctx, _) => ctx.Value is string path
                        ? ctx.Op.Catch(() => { _ = ctx.Declaration.Fields.AddFilename(ctx.Declaration.Name, path, ctx.Declaration.Prompt, ctx.Declaration.Section); return Fin.Succ(value: unit); })
                        : Fin.Fail<Unit>(error: ctx.Op.InvalidInput()))
                : Fin.Fail<Unit>(error: op.InvalidInput()),
            write: (fields, name, payload, op) => payload is T value
                ? op.Catch(() => { write(fields, name, value); return Fin.Succ(value: unit); })
                : Fin.Fail<Unit>(error: op.InvalidInput()));
}
```

## [03]-[DECLARATION]

- Owner: `FieldPresentation` `[Union]` — the declaration posture: `Plain` the ordinary field, `Textured` with its treat-as-linear grant through `AddTextured`, `Filename` the file-path string through `AddFilename`; `FieldSpec` — one declaration row: name, initial `FieldValue`, prompt, section, presentation; `DynamicFieldSpec` — one admitted runtime row whose optional bounds ride one ordered scalar carrier — finite, `Min <= Max`, carrier-equal to the value — declared inside the host begin/end bracket as one fold.
- Law: declaration is data — a content class's field roster is a `Seq<FieldSpec>` declared in one pass, so the roster is diffable and a new field is one row; a hand-spelled `Add` chain beside the spec fold is the deleted form.
- Law: `FieldRange` delegates scalar ordering to the same `FieldCarrier` row that boxes and declares the case; non-scalar rows refuse bounds without a parallel case roster.
- Law: `DynamicFields.Declare` opens `BeginCreateDynamicFields`, admits every row, and closes `EndCreateDynamicFields` on every exit.
- Law: a textured or filename declaration is a presentation row, never a sibling spec type — the presentation discriminates the host `Add` overload, and the returned typed field stays inside the fold.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FieldPresentation {
    private FieldPresentation() { }
    public sealed record Plain : FieldPresentation;
    public sealed record Textured(bool TreatAsLinear) : FieldPresentation;
    public sealed record Filename : FieldPresentation;
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record FieldSpec(
    string Name,
    FieldValue Value,
    FieldPresentation Presentation,
    Option<string> Prompt = default,
    int SectionId = 0) {
    internal Fin<Unit> Declare(FieldDictionary fields, Op key) {
        FieldSpec self = this;
        return from name in key.AcceptText(value: self.Name)
               from value in Optional(self.Value).ToFin(Fail: key.InvalidInput(detail: "<field-value>"))
               from presentation in Optional(self.Presentation).ToFin(Fail: key.InvalidInput(detail: "<field-presentation>"))
               from _ in value.Declare(
                   declaration: new FieldDeclaration(
                       Fields: fields,
                       Name: name,
                       Prompt: self.Prompt.IfNone(name),
                       Section: self.SectionId,
                       Presentation: presentation),
                   key: key)
               select unit;
    }
}

[ComplexValueObject]
public sealed partial class FieldRange {
    public FieldValue Min { get; }
    public FieldValue Max { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref FieldValue min,
        ref FieldValue max) =>
        validationError = min is not null && max is not null && min.Carrier.AcceptsRange(min: min, max: max)
            ? null
            : new ValidationError(message: "<field-range-bounds>");

    internal static Fin<FieldRange> Of(FieldValue min, FieldValue max, Op key) =>
        key.AcceptValidated(Validate(min, max, out FieldRange? range), range);
}

[ComplexValueObject]
public sealed partial class DynamicFieldSpec {
    public string InternalName { get; }
    public string LocalName { get; }
    public string EnglishName { get; }
    public FieldValue Value { get; }
    public Option<FieldRange> Bounds { get; }
    public int SectionId { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string internalName,
        ref string localName,
        ref string englishName,
        ref FieldValue value,
        ref Option<FieldRange> bounds,
        ref int sectionId) =>
        validationError = string.IsNullOrWhiteSpace(internalName)
            || string.IsNullOrWhiteSpace(localName)
            || string.IsNullOrWhiteSpace(englishName)
            || value is null
            || sectionId < 0
            || bounds.Case is FieldRange range && range.Min.Carrier != value.Carrier
                ? new ValidationError(message: "<dynamic-field-shape>")
                : null;

    public static Fin<DynamicFieldSpec> Of(
        string internalName, string localName, string englishName, FieldValue value,
        Option<(FieldValue Min, FieldValue Max)> bounds, int sectionId, Op key) =>
        from range in bounds.Traverse(row => FieldRange.Of(min: row.Min, max: row.Max, key: key)).As()
        from admitted in key.AcceptValidated(
            Validate(internalName, localName, englishName, value, range, sectionId, out DynamicFieldSpec? created), created)
        select admitted;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class DynamicFields {
    internal static Fin<Unit> Declare(RenderContent content, bool automatic, Seq<DynamicFieldSpec> rows, Op key) =>
        key.Catch(() => {
            content.BeginCreateDynamicFields(automatic: automatic);
            try {
                return rows.TraverseM(row => Optional(row)
                    .ToFin(Fail: key.InvalidInput(detail: "<dynamic-field-row>"))
                    .Bind(admitted => key.Catch(() => key.Confirm(success: content.CreateDynamicField(
                        internalName: admitted.InternalName,
                        localName: admitted.LocalName,
                        englishName: admitted.EnglishName,
                        value: admitted.Value.Boxed(),
                        minValue: admitted.Bounds.Case is FieldRange range ? range.Min.Boxed() : null,
                        maxValue: admitted.Bounds.Case is FieldRange range ? range.Max.Boxed() : null,
                        sectionId: admitted.SectionId))))).As().Map(static _ => unit);
            } finally {
                content.EndCreateDynamicFields();
            }
        });
}
```

## [04]-[BINDING_AND_PARAMS]

- Owner: `FieldBinding` admits direct and child-slot field bindings through one optional-slot factory. `ParamScope` admits named, child-slot extra-requirement, and direct extra-requirement routes. `FieldPortrait` and `FieldCensus` detach the dictionary in one pass.
- Law: each `ParamScope` case reaches its corresponding host endpoint; child-slot and direct extra-requirement semantics remain distinct cases.
- Law: name resolution stays host-owned — `ChildSlotNameFromParamName`/`ParamNameFromChildSlotName` answer the correspondence at the consulting site, and no local table mirrors it.
- Law: reads recover typed — a `ParamScope` read boxes through the host and immediately classifies into `FieldValue` by runtime payload type, so `object` dies at this seam.
- Law: `FieldCensus.Of` traverses `FieldDictionary` once and projects value, texture bounds, usage grants, and visibility per field.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FieldBinding {
    private FieldBinding() { }
    private sealed record DirectCase(string Parameter) : FieldBinding;
    private sealed record AtSlotCase(string Parameter, string ChildSlot) : FieldBinding;

    public static Fin<FieldBinding> Of(string parameter, Option<string> childSlot = default) {
        Op op = Op.Of(name: nameof(FieldBinding));
        return from admittedParameter in op.AcceptText(value: parameter)
               from admittedSlot in childSlot.Traverse(slot => op.AcceptText(value: slot)).As()
               select admittedSlot.Match(
                   Some: slot => (FieldBinding)new AtSlotCase(Parameter: admittedParameter, ChildSlot: slot),
                   None: () => new DirectCase(Parameter: admittedParameter));
    }

    internal Fin<Unit> Bind(RenderContent content, Field field, ChangeReason reason, Op key) =>
        ChangeScope.Write(content: content, reason: reason, key: key, body: live => Switch(
            state: (Content: live, Field: field, Reason: reason, Op: key),
            directCase: static (ctx, binding) => ctx.Op.Catch(() => {
                ctx.Content.BindParameterToField(parameterName: binding.Parameter, field: ctx.Field, setEvent: ctx.Reason.Native);
                return Fin.Succ(value: unit);
            }),
            atSlotCase: static (ctx, binding) => ctx.Op.Catch(() => {
                ctx.Content.BindParameterToField(
                    parameterName: binding.Parameter, childSlotName: binding.ChildSlot, field: ctx.Field, setEvent: ctx.Reason.Native);
                return Fin.Succ(value: unit);
            })));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ParamScope {
    private ParamScope() { }
    private sealed record NamedCase(string Parameter) : ParamScope;
    private sealed record ChildCase(string ChildSlot, string Requirement) : ParamScope;
    private sealed record ExtraCase(string Parameter, string Requirement) : ParamScope;

    public static Fin<ParamScope> Named(string parameter) =>
        Op.Of(name: nameof(ParamScope)).AcceptText(value: parameter)
            .Map(static admitted => (ParamScope)new NamedCase(Parameter: admitted));

    public static Fin<ParamScope> Child(string childSlot, string requirement) {
        Op op = Op.Of(name: nameof(ParamScope));
        return from admittedSlot in op.AcceptText(value: childSlot)
               from admittedRequirement in op.AcceptText(value: requirement)
               select (ParamScope)new ChildCase(ChildSlot: admittedSlot, Requirement: admittedRequirement);
    }

    public static Fin<ParamScope> Extra(string parameter, string requirement) {
        Op op = Op.Of(name: nameof(ParamScope));
        return from admittedParameter in op.AcceptText(value: parameter)
               from admittedRequirement in op.AcceptText(value: requirement)
               select (ParamScope)new ExtraCase(Parameter: admittedParameter, Requirement: admittedRequirement);
    }

    internal Fin<FieldValue> Read(RenderContent content, Op key) =>
        Switch(
            state: (Content: content, Op: key),
            namedCase: static (ctx, scope) => ctx.Op.Catch(() => FieldValue.Of(
                payload: ctx.Content.GetParameter(parameterName: scope.Parameter), key: ctx.Op)),
            childCase: static (ctx, scope) => ctx.Op.Catch(() => FieldValue.Of(
                payload: ctx.Content.GetChildSlotParameter(scope.ChildSlot, scope.Requirement), key: ctx.Op)),
            extraCase: static (ctx, scope) => ctx.Op.Catch(() => FieldValue.Of(
                payload: ctx.Content.GetExtraRequirementParameter(
                    contentParameterName: scope.Parameter,
                    extraRequirementParameter: scope.Requirement),
                key: ctx.Op)));

    internal Fin<Unit> Write(
        RenderContent content, FieldValue value, ChangeReason reason,
        RenderContent.ExtraRequirementsSetContexts context, Op key) =>
        ChangeScope.Write(content: content, reason: reason, key: key, body: live => Switch(
            state: (Content: live, Value: value, Context: context, Op: key),
            namedCase: static (ctx, scope) => ctx.Op.Catch(() => ctx.Op.Confirm(success: ctx.Content.SetParameter(
                parameterName: scope.Parameter, value: ctx.Value.Boxed()))),
            childCase: static (ctx, scope) => ctx.Op.Catch(() => ctx.Op.Confirm(success: ctx.Content.SetChildSlotParameter(
                scope.ChildSlot, scope.Requirement, ctx.Value.Boxed(), ctx.Context))),
            extraCase: static (ctx, scope) => ctx.Op.Catch(() => ctx.Op.Confirm(success: ctx.Content.SetExtraRequirementParameter(
                contentParameterName: scope.Parameter,
                extraRequirementParameter: scope.Requirement,
                value: ctx.Value.Boxed(),
                sc: ctx.Context)))));
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record FieldPortrait(
    string Name,
    FieldValue Value,
    double TextureAmountMin,
    double TextureAmountMax,
    bool UseTextureOn,
    bool UseTextureAmount,
    bool HiddenInAutoUi) : IDetachedDocumentResult;

public sealed record FieldCensus(Arr<FieldPortrait> Rows) : IDetachedDocumentResult {
    internal static Fin<FieldCensus> Of(FieldDictionary fields, Op key) =>
        key.Catch(() => toSeq(fields)
            .TraverseM(field => FieldValue.Of(payload: field, key: key).Map(value => new FieldPortrait(
                Name: field.Name,
                Value: value,
                TextureAmountMin: field.TextureAmountMin,
                TextureAmountMax: field.TextureAmountMax,
                UseTextureOn: field.UseTextureOn,
                UseTextureAmount: field.UseTextureAmount,
                HiddenInAutoUi: field.IsHiddenInAutoUI)))
            .As()
            .Map(static rows => new FieldCensus(Rows: toArr(rows))));
}
```

## [05]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]         | [OWNER]            | [FORM]                                                    | [ENTRY]                        |
| :-----: | :---------------- | :----------------- | :-------------------------------------------------------- | :----------------------------- |
|  [01]   | payload family    | `FieldValue`       | union cases derived through `FieldCarrier` rows           | `Declare` / `Write` / `Of`     |
|  [02]   | field declaration | `FieldSpec`        | name + value + prompt + section + presentation row        | `Declare(fields, key)`         |
|  [03]   | dynamic fields    | `DynamicFieldSpec` | generated admission plus bracketed traversal              | `Of` / `DynamicFields.Declare` |
|  [04]   | parameter binding | `FieldBinding`     | admitted direct and child-slot cases                      | `Of` / `Bind`                  |
|  [05]   | parameter routes  | `ParamScope`       | named, child-slot, and direct-extra cases                 | `Named` / `Child` / `Extra`    |
|  [06]   | field census      | `FieldCensus`      | one-pass dictionary walk to detached `FieldPortrait` rows | `Of(fields, key)`              |

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
