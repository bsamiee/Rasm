# [RASM_RHINO_RENDER_FIELDS]

Typed parameter family (`Rasm.Rhino.Render`). Host field carriers collapse onto one `FieldValue` union with exactly sixteen cases: one total dispatch declares plain or textured fields through payload-typed overloads, one writes through payload-typed `Set`, one recovers a live carrier without a silent arm, and one boxes values for object-typed host seams. `FieldSpec` adds filename presentation without duplicating the payload family, `DynamicFieldSpec` admits type-aligned bounds before the runtime bracket opens, `FieldBinding` closes both binding overloads, `ParamScope` closes the three name-keyed parameter routes, and `FieldPortrait` detaches the census. Every write rides `ChangeScope`; no live `Field` or `FieldDictionary` crosses the demand window.

## [01]-[INDEX]

- [02]-[VALUE]: `FieldValue` — the one polymorphic payload owner with write, recovery, and boxing dispatch.
- [03]-[DECLARATION]: `FieldPresentation` and `FieldSpec` — field declaration rows; `DynamicFieldSpec` — the dynamic-field bracket fold.
- [04]-[BINDING_AND_PARAMS]: `FieldBinding`, `ParamScope`, and the `FieldPortrait` census.
- [05]-[SURFACE_LEDGER]: page owner table.

## [02]-[VALUE]

- Owner: `FieldValue` `[Union]` — sixteen cases, one per host field carrier: `Toggle`/`BoolField`, `Whole`/`IntField`, `Single`/`FloatField`, `Real`/`DoubleField`, `Colour`/`Color4fField`, `Vec2`/`Vector2dField`, `Vec3`/`Vector3dField`, `Pt2`/`Point2dField`, `Pt3`/`Point3dField`, `Pt4`/`Point4dField`, `Text`/`StringField`, `Stamp`/`DateTimeField`, `Key`/`GuidField`, `Motion`/`TransformField`, `Bytes`/`ByteArrayField`, `Null`/`NullField`.
- Law: four total dispatches close the family — `Declare` routes plain/textured presentation, `Write` routes payload-typed `Set`, `Of` recovers only an exact carrier, and `Boxed` projects object-typed host seams; a seventeenth payload breaks every dispatch at compile time.
- Law: `Null` is declaration-only — it exists so a `NullField` census row survives typed, and a `Write` against it refuses typed rather than minting a phantom set.
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
public abstract partial record FieldValue {
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

    internal static Fin<FieldValue> Of(Field field, Op key) =>
        Optional(field).ToFin(Fail: key.InvalidInput()).Bind(active => active switch {
            BoolField typed => Fin.Succ<FieldValue>(value: new Toggle(Value: typed.Value)),
            IntField typed => Fin.Succ<FieldValue>(value: new Whole(Value: typed.Value)),
            FloatField typed => Fin.Succ<FieldValue>(value: new Single(Value: typed.Value)),
            DoubleField typed => Fin.Succ<FieldValue>(value: new Real(Value: typed.Value)),
            Color4fField typed => Fin.Succ<FieldValue>(value: new Colour(Value: typed.Value)),
            Vector2dField typed => Fin.Succ<FieldValue>(value: new Vec2(Value: typed.Value)),
            Vector3dField typed => Fin.Succ<FieldValue>(value: new Vec3(Value: typed.Value)),
            Point2dField typed => Fin.Succ<FieldValue>(value: new Pt2(Value: typed.Value)),
            Point3dField typed => Fin.Succ<FieldValue>(value: new Pt3(Value: typed.Value)),
            Point4dField typed => Fin.Succ<FieldValue>(value: new Pt4(Value: typed.Value)),
            StringField typed => Fin.Succ<FieldValue>(value: new Text(Value: typed.Value)),
            DateTimeField typed => Fin.Succ<FieldValue>(value: new Stamp(Value: typed.Value)),
            GuidField typed => Fin.Succ<FieldValue>(value: new Key(Value: typed.Value)),
            TransformField typed => Fin.Succ<FieldValue>(value: new Motion(Value: typed.Value)),
            ByteArrayField typed => Fin.Succ<FieldValue>(value: new Bytes(Value: toArr(typed.Value))),
            NullField => Fin.Succ<FieldValue>(value: new Null()),
            _ => Fin.Fail<FieldValue>(error: key.InvalidResult(detail: active.GetType().Name)),
        });

    internal Fin<Unit> Declare(FieldDictionary fields, string name, string prompt, int sectionId, Option<bool> textured, Op key) =>
        Switch(
            state: (Fields: fields, Name: name, Prompt: prompt, Section: sectionId, Textured: textured, Op: key),
            toggle: static (ctx, c) => Added(ctx, c.Value, static (f, n, v, p, s) => f.Add(n, v, p, s), static (f, n, v, p, t, s) => f.AddTextured(n, v, p, t, s)),
            whole: static (ctx, c) => Added(ctx, c.Value, static (f, n, v, p, s) => f.Add(n, v, p, s), static (f, n, v, p, t, s) => f.AddTextured(n, v, p, t, s)),
            single: static (ctx, c) => Added(ctx, c.Value, static (f, n, v, p, s) => f.Add(n, v, p, s), static (f, n, v, p, t, s) => f.AddTextured(n, v, p, t, s)),
            real: static (ctx, c) => Added(ctx, c.Value, static (f, n, v, p, s) => f.Add(n, v, p, s), static (f, n, v, p, t, s) => f.AddTextured(n, v, p, t, s)),
            colour: static (ctx, c) => Added(ctx, c.Value, static (f, n, v, p, s) => f.Add(n, v, p, s), static (f, n, v, p, t, s) => f.AddTextured(n, v, p, t, s)),
            vec2: static (ctx, c) => Added(ctx, c.Value, static (f, n, v, p, s) => f.Add(n, v, p, s), static (f, n, v, p, t, s) => f.AddTextured(n, v, p, t, s)),
            vec3: static (ctx, c) => Added(ctx, c.Value, static (f, n, v, p, s) => f.Add(n, v, p, s), static (f, n, v, p, t, s) => f.AddTextured(n, v, p, t, s)),
            pt2: static (ctx, c) => Added(ctx, c.Value, static (f, n, v, p, s) => f.Add(n, v, p, s), static (f, n, v, p, t, s) => f.AddTextured(n, v, p, t, s)),
            pt3: static (ctx, c) => Added(ctx, c.Value, static (f, n, v, p, s) => f.Add(n, v, p, s), static (f, n, v, p, t, s) => f.AddTextured(n, v, p, t, s)),
            pt4: static (ctx, c) => Added(ctx, c.Value, static (f, n, v, p, s) => f.Add(n, v, p, s), static (f, n, v, p, t, s) => f.AddTextured(n, v, p, t, s)),
            text: static (ctx, c) => Added(ctx, c.Value, static (f, n, v, p, s) => f.Add(n, v, p, s), static (f, n, v, p, t, s) => f.AddTextured(n, v, p, t, s)),
            stamp: static (ctx, c) => Added(ctx, c.Value, static (f, n, v, p, s) => f.Add(n, v, p, s), static (f, n, v, p, t, s) => f.AddTextured(n, v, p, t, s)),
            key: static (ctx, c) => Added(ctx, c.Value, static (f, n, v, p, s) => f.Add(n, v, p, s), static (f, n, v, p, t, s) => f.AddTextured(n, v, p, t, s)),
            motion: static (ctx, c) => Added(ctx, c.Value, static (f, n, v, p, s) => f.Add(n, v, p, s), static (f, n, v, p, t, s) => f.AddTextured(n, v, p, t, s)),
            bytes: static (ctx, c) => Added(ctx, c.Value.ToArray(), static (f, n, v, p, s) => f.Add(n, v, p, s), static (f, n, v, p, t, s) => f.AddTextured(n, v, p, t, s)),
            @null: static (ctx, _) => ctx.Op.Catch(() => {
                Field added = ctx.Textured.Match(
                    Some: treat => ctx.Fields.AddTextured(ctx.Name, ctx.Prompt, treat, ctx.Section),
                    None: () => ctx.Fields.Add(ctx.Name, ctx.Prompt, ctx.Section));
                return Optional(added).ToFin(Fail: ctx.Op.InvalidResult()).Map(static _ => unit);
            }));

    internal Fin<Unit> Write(FieldDictionary fields, string name, Op key) =>
        Switch(
            state: (Fields: fields, Name: name, Op: key),
            toggle: static (ctx, c) => ctx.Op.Catch(() => { ctx.Fields.Set(key: ctx.Name, value: c.Value); return Fin.Succ(value: unit); }),
            whole: static (ctx, c) => ctx.Op.Catch(() => { ctx.Fields.Set(key: ctx.Name, value: c.Value); return Fin.Succ(value: unit); }),
            single: static (ctx, c) => ctx.Op.Catch(() => { ctx.Fields.Set(key: ctx.Name, value: c.Value); return Fin.Succ(value: unit); }),
            real: static (ctx, c) => ctx.Op.Catch(() => { ctx.Fields.Set(key: ctx.Name, value: c.Value); return Fin.Succ(value: unit); }),
            colour: static (ctx, c) => ctx.Op.Catch(() => { ctx.Fields.Set(key: ctx.Name, value: c.Value); return Fin.Succ(value: unit); }),
            vec2: static (ctx, c) => ctx.Op.Catch(() => { ctx.Fields.Set(key: ctx.Name, value: c.Value); return Fin.Succ(value: unit); }),
            vec3: static (ctx, c) => ctx.Op.Catch(() => { ctx.Fields.Set(key: ctx.Name, value: c.Value); return Fin.Succ(value: unit); }),
            pt2: static (ctx, c) => ctx.Op.Catch(() => { ctx.Fields.Set(key: ctx.Name, value: c.Value); return Fin.Succ(value: unit); }),
            pt3: static (ctx, c) => ctx.Op.Catch(() => { ctx.Fields.Set(key: ctx.Name, value: c.Value); return Fin.Succ(value: unit); }),
            pt4: static (ctx, c) => ctx.Op.Catch(() => { ctx.Fields.Set(key: ctx.Name, value: c.Value); return Fin.Succ(value: unit); }),
            text: static (ctx, c) => ctx.Op.Catch(() => { ctx.Fields.Set(key: ctx.Name, value: c.Value); return Fin.Succ(value: unit); }),
            stamp: static (ctx, c) => ctx.Op.Catch(() => { ctx.Fields.Set(key: ctx.Name, value: c.Value); return Fin.Succ(value: unit); }),
            key: static (ctx, c) => ctx.Op.Catch(() => { ctx.Fields.Set(key: ctx.Name, value: c.Value); return Fin.Succ(value: unit); }),
            motion: static (ctx, c) => ctx.Op.Catch(() => { ctx.Fields.Set(key: ctx.Name, value: c.Value); return Fin.Succ(value: unit); }),
            bytes: static (ctx, c) => ctx.Op.Catch(() => { ctx.Fields.Set(key: ctx.Name, value: c.Value.ToArray()); return Fin.Succ(value: unit); }),
            @null: static (ctx, _) => Fin.Fail<Unit>(error: ctx.Op.Unsupported(geometryType: typeof(FieldValue), outputType: typeof(Unit))));

    internal object? Boxed() =>
        Switch<object?>(
            toggle: static c => c.Value,
            whole: static c => c.Value,
            single: static c => c.Value,
            real: static c => c.Value,
            colour: static c => c.Value,
            vec2: static c => c.Value,
            vec3: static c => c.Value,
            pt2: static c => c.Value,
            pt3: static c => c.Value,
            pt4: static c => c.Value,
            text: static c => c.Value,
            stamp: static c => c.Value,
            key: static c => c.Value,
            motion: static c => c.Value,
            bytes: static c => c.Value.ToArray(),
            @null: static _ => null);

    private static Fin<Unit> Added<T>(
        (FieldDictionary Fields, string Name, string Prompt, int Section, Option<bool> Textured, Op Op) ctx,
        T value,
        Func<FieldDictionary, string, T, string, int, Field> plain,
        Func<FieldDictionary, string, T, string, bool, int, Field> withTexture) =>
        ctx.Op.Catch(() => {
            Field added = ctx.Textured.Match(
                Some: treat => withTexture(ctx.Fields, ctx.Name, value, ctx.Prompt, treat, ctx.Section),
                None: () => plain(ctx.Fields, ctx.Name, value, ctx.Prompt, ctx.Section));
            return Optional(added).ToFin(Fail: ctx.Op.InvalidResult()).Map(static _ => unit);
        });
}
```

## [03]-[DECLARATION]

- Owner: `FieldPresentation` `[Union]` — the declaration posture: `Plain` the ordinary field, `Textured` with its treat-as-linear grant through `AddTextured`, `Filename` the file-path string through `AddFilename`; `FieldSpec` — one declaration row: name, initial `FieldValue`, prompt, section, presentation; `DynamicFieldSpec` — one admitted runtime row whose optional bounds share the value carrier, declared inside the host begin/end bracket as one fold.
- Law: declaration is data — a content class's field roster is a `Seq<FieldSpec>` declared in one pass, so the roster is diffable and a new field is one row; a hand-spelled `Add` chain beside the spec fold is the deleted form.
- Law: the dynamic bracket is owned here — `DynamicFields.Declare` opens `BeginCreateDynamicFields`, folds every row through `CreateDynamicField` with `Boxed` payloads, and closes `EndCreateDynamicFields` on every exit, so a throwing row never leaves the bracket open.
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
        return key.AcceptText(value: self.Name).Bind(name => self.Presentation.Switch(
            state: (Spec: self, Fields: fields, Name: name, Prompt: self.Prompt.IfNone(name), Op: key),
            plain: static (ctx, _) => ctx.Spec.Value.Declare(
                fields: ctx.Fields, name: ctx.Name, prompt: ctx.Prompt, sectionId: ctx.Spec.SectionId, textured: Option<bool>.None, key: ctx.Op),
            textured: static (ctx, row) => ctx.Spec.Value.Declare(
                fields: ctx.Fields, name: ctx.Name, prompt: ctx.Prompt, sectionId: ctx.Spec.SectionId, textured: Some(row.TreatAsLinear), key: ctx.Op),
            filename: static (ctx, _) => ctx.Spec.Value is FieldValue.Text path
                ? ctx.Op.Catch(() => {
                    Field added = ctx.Fields.AddFilename(
                        key: ctx.Name, value: path.Value, prompt: ctx.Prompt, sectionId: ctx.Spec.SectionId);
                    return Optional(added).ToFin(Fail: ctx.Op.InvalidResult()).Map(static _ => unit);
                })
                : Fin.Fail<Unit>(error: ctx.Op.InvalidInput())));
    }
}

public sealed record DynamicFieldSpec {
    private DynamicFieldSpec(
        string internalName, string localName, string englishName, FieldValue value,
        Option<(FieldValue Min, FieldValue Max)> bounds, int sectionId) =>
        (InternalName, LocalName, EnglishName, Value, Bounds, SectionId) =
        (internalName, localName, englishName, value, bounds, sectionId);

    public string InternalName { get; }
    public string LocalName { get; }
    public string EnglishName { get; }
    public FieldValue Value { get; }
    public Option<(FieldValue Min, FieldValue Max)> Bounds { get; }
    public int SectionId { get; }

    public static Fin<DynamicFieldSpec> Of(
        string internalName, string localName, string englishName, FieldValue value,
        Option<(FieldValue Min, FieldValue Max)> bounds, int sectionId, Op key) =>
        from admittedInternal in key.AcceptText(value: internalName)
        from admittedLocal in key.AcceptText(value: localName)
        from admittedEnglish in key.AcceptText(value: englishName)
        from _ in bounds.Match(
            Some: range => value is not FieldValue.Null && range.Min.GetType() == value.GetType() && range.Max.GetType() == value.GetType()
                ? Fin.Succ(value: unit)
                : Fin.Fail<Unit>(error: key.InvalidInput()),
            None: static () => Fin.Succ(value: unit))
        select new DynamicFieldSpec(
            internalName: admittedInternal, localName: admittedLocal, englishName: admittedEnglish,
            value: value, bounds: bounds, sectionId: sectionId);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class DynamicFields {
    internal static Fin<Unit> Declare(RenderContent content, bool automatic, Seq<DynamicFieldSpec> rows, Op key) =>
        key.Catch(() => {
            content.BeginCreateDynamicFields(automatic: automatic);
            try {
                return rows.TraverseM(row => key.Catch(() => key.Confirm(success: content.CreateDynamicField(
                    internalName: row.InternalName,
                    localName: row.LocalName,
                    englishName: row.EnglishName,
                    value: row.Value.Boxed(),
                    minValue: row.Bounds.Map(static bounds => bounds.Min.Boxed()).IfNoneUnsafe((object?)null),
                    maxValue: row.Bounds.Map(static bounds => bounds.Max.Boxed()).IfNoneUnsafe((object?)null),
                    sectionId: row.SectionId)))).As().Map(static _ => unit);
            } finally {
                content.EndCreateDynamicFields();
            }
        });
}
```

## [04]-[BINDING_AND_PARAMS]

- Owner: `FieldBinding` `[Union]` — the two `BindParameterToField` overloads as cases: `Direct` binds a named parameter, `AtSlot` binds through a child-slot name; `ParamScope` `[Union]` — the three name-keyed parameter routes: `Named` the direct `GetParameter`/`SetParameter` pair, `Extra` the auto-UI extra-requirement pair, `ChildExtra` the child-slot extra-requirement pair; `FieldPortrait` — the detached census row: name, current value, textured-amount bounds, texture-usage grants, auto-UI visibility.
- Law: name resolution stays host-owned — `ChildSlotNameFromParamName`/`ParamNameFromChildSlotName` answer the correspondence at the consulting site, and no local table mirrors it.
- Law: reads recover typed — a `ParamScope` read boxes through the host and immediately classifies into `FieldValue` by runtime payload type, so `object` dies at this seam.
- Law: the census walks the dictionary once — `FieldPortrait.CensusOf` enumerates `FieldDictionary`, projecting value, bounds, and visibility per field; a consumer probing fields one key at a time re-derives what one pass carries.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FieldBinding {
    private FieldBinding() { }
    public sealed record Direct(string Parameter) : FieldBinding;
    public sealed record AtSlot(string Parameter, string ChildSlot) : FieldBinding;

    internal Fin<Unit> Bind(RenderContent content, Field field, ChangeReason reason, Op key) =>
        ChangeScope.Write(content: content, reason: reason, key: key, body: live => Switch(
            state: (Content: live, Field: field, Reason: reason, Op: key),
            direct: static (ctx, binding) => ctx.Op.Catch(() => {
                ctx.Content.BindParameterToField(parameterName: binding.Parameter, field: ctx.Field, setEvent: ctx.Reason.Native);
                return Fin.Succ(value: unit);
            }),
            atSlot: static (ctx, binding) => ctx.Op.Catch(() => {
                ctx.Content.BindParameterToField(
                    parameterName: binding.Parameter, childSlotName: binding.ChildSlot, field: ctx.Field, setEvent: ctx.Reason.Native);
                return Fin.Succ(value: unit);
            })));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ParamScope {
    private ParamScope() { }
    public sealed record Named(string Parameter) : ParamScope;
    public sealed record Extra(string Parameter, string Requirement) : ParamScope;
    public sealed record ChildExtra(string ChildSlot, string Parameter) : ParamScope;

    internal Fin<FieldValue> Read(RenderContent content, Op key) =>
        Switch(
            state: (Content: content, Op: key),
            named: static (ctx, scope) => ctx.Op.Catch(() => Classified(ctx.Content.GetParameter(parameterName: scope.Parameter), ctx.Op)),
            extra: static (ctx, scope) => ctx.Op.Catch(() => Classified(
                ctx.Content.GetExtraRequirementParameter(contentParameterName: scope.Parameter, extraRequirementParameter: scope.Requirement), ctx.Op)),
            childExtra: static (ctx, scope) => ctx.Op.Catch(() => Classified(
                ctx.Content.GetChildSlotParameter(childSlotName: scope.ChildSlot, parameterName: scope.Parameter), ctx.Op)));

    internal Fin<Unit> Write(
        RenderContent content, FieldValue value, ChangeReason reason,
        RenderContent.ExtraRequirementsSetContexts context, Op key) =>
        ChangeScope.Write(content: content, reason: reason, key: key, body: live => Switch(
            state: (Content: live, Value: value, Context: context, Op: key),
            named: static (ctx, scope) => ctx.Op.Catch(() => ctx.Op.Confirm(success: ctx.Content.SetParameter(
                parameterName: scope.Parameter, value: ctx.Value.Boxed()))),
            extra: static (ctx, scope) => ctx.Op.Catch(() => ctx.Op.Confirm(success: ctx.Content.SetExtraRequirementParameter(
                contentParameterName: scope.Parameter, extraRequirementParameter: scope.Requirement, value: ctx.Value.Boxed(), sc: ctx.Context))),
            childExtra: static (ctx, scope) => ctx.Op.Catch(() => ctx.Op.Confirm(success: ctx.Content.SetChildSlotParameter(
                childSlotName: scope.ChildSlot, parameterName: scope.Parameter, value: ctx.Value.Boxed(), sc: ctx.Context))));

    private static Fin<FieldValue> Classified(object? payload, Op key) =>
        payload switch {
            bool value => Fin.Succ<FieldValue>(value: new FieldValue.Toggle(Value: value)),
            int value => Fin.Succ<FieldValue>(value: new FieldValue.Whole(Value: value)),
            float value => Fin.Succ<FieldValue>(value: new FieldValue.Single(Value: value)),
            double value => Fin.Succ<FieldValue>(value: new FieldValue.Real(Value: value)),
            Color4f value => Fin.Succ<FieldValue>(value: new FieldValue.Colour(Value: value)),
            Vector2d value => Fin.Succ<FieldValue>(value: new FieldValue.Vec2(Value: value)),
            Vector3d value => Fin.Succ<FieldValue>(value: new FieldValue.Vec3(Value: value)),
            Point2d value => Fin.Succ<FieldValue>(value: new FieldValue.Pt2(Value: value)),
            Point3d value => Fin.Succ<FieldValue>(value: new FieldValue.Pt3(Value: value)),
            Point4d value => Fin.Succ<FieldValue>(value: new FieldValue.Pt4(Value: value)),
            string value => Fin.Succ<FieldValue>(value: new FieldValue.Text(Value: value)),
            DateTime value => Fin.Succ<FieldValue>(value: new FieldValue.Stamp(Value: value)),
            Guid value => Fin.Succ<FieldValue>(value: new FieldValue.Key(Value: value)),
            Transform value => Fin.Succ<FieldValue>(value: new FieldValue.Motion(Value: value)),
            byte[] value => Fin.Succ<FieldValue>(value: new FieldValue.Bytes(Value: toArr(value))),
            null => Fin.Succ<FieldValue>(value: new FieldValue.Null()),
            var foreign => Fin.Fail<FieldValue>(error: key.InvalidResult(detail: foreign.GetType().Name)),
        };
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record FieldPortrait(
    string Name,
    FieldValue Value,
    double TextureAmountMin,
    double TextureAmountMax,
    bool UseTextureOn,
    bool UseTextureAmount,
    bool HiddenInAutoUi) : IDetachedDocumentResult {
    internal static Fin<Arr<FieldPortrait>> CensusOf(FieldDictionary fields, Op key) =>
        key.Catch(() => toSeq(fields)
            .TraverseM(field => FieldValue.Of(field: field, key: key).Map(value => new FieldPortrait(
                Name: field.Name,
                Value: value,
                TextureAmountMin: field.TextureAmountMin,
                TextureAmountMax: field.TextureAmountMax,
                UseTextureOn: field.UseTextureOn,
                UseTextureAmount: field.UseTextureAmount,
                HiddenInAutoUi: field.IsHiddenInAutoUI)))
            .As()
            .Map(static rows => toArr(rows)));
}
```

## [05]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]         | [OWNER]         | [FORM]                                                    | [ENTRY]                             |
| :-----: | :---------------- | :-------------- | :-------------------------------------------------------- | :---------------------------------- |
|  [01]   | payload family    | `FieldValue`    | one union, sixteen cases, write/recover/box dispatch      | `Write` / `Of` / `Boxed`            |
|  [02]   | field declaration | `FieldSpec`     | name + value + prompt + section + presentation row        | `Declare(fields, key)`              |
|  [03]   | dynamic fields    | `DynamicFields` | begin/rows/end bracket as one fold                        | `Declare(content, automatic, rows)` |
|  [04]   | parameter binding | `FieldBinding`  | the two `BindParameterToField` overloads as cases         | `Bind(content, field, reason)`      |
|  [05]   | name-keyed params | `ParamScope`    | direct, extra-requirement, child-slot routes on one owner | `Read` / `Write`                    |
|  [06]   | field census      | `FieldPortrait` | one-pass dictionary walk to detached rows                 | `CensusOf(fields, key)`             |
