# [RASM_RHINO_RENDER_FIELDS]

`ContentValue` owns the typed RDK parameter algebra from admission through declaration, binding, mutation, recovery, and detached census. `ContentCarrier` declares the payload correspondence once, `FieldPresentation` and `FieldRange` carry declaration policy as values, `ParamScope` reaches every native parameter route, and `ChangeScope` brackets every write before any host carrier escapes.

## [01]-[INDEX]

- [02]-[VALUE]: `ContentValue` — the one polymorphic payload owner with write, recovery, and boxing dispatch.
- [03]-[DECLARATION]: `FieldPresentation` and `FieldSpec` — field declaration rows; `DynamicFieldSpec` — the dynamic-field bracket fold.
- [04]-[BINDING_AND_PARAMS]: `FieldBinding`, the `PbrChannel`/`ContentParameter` name vocabularies, `ParamScope`, and the `FieldPortrait` census.
- [05]-[SURFACE_LEDGER]: page owner table.

## [02]-[VALUE]

- Owner: `ContentValue` closes the payload alternatives, and `ContentCarrier` rows bind each case to its host field type, raw payload type, boxing projection, range order, declaration delegates, write delegate, and recovery projection.
- Law: the name is `ContentValue`, never `FieldValue` — three separate owners in this branch claim the shorter spelling (the kernel `Rasm.Interaction` control payload, this content-parameter algebra, and the Annotation text-field grammar), and `Rasm.Rhino.Render` and `Rasm.Interaction` meet in one file the moment the registry's editor shell composes a control spec.
- Law: each case declares its own payload correspondence — `IContentPayload<TSelf, T>` carries the static `Of` mint and the `Value` read, so `ContentCarrier.Row` derives the row key, the pack, and the unpack from the case type itself. Per-row `key`/`pack`/`unpack` lambda triples are the deleted form and, with them, the chance of a row whose key names one case while its lambdas read another.
- Law: `ContentCarrier.Items` derives field-type and payload-type lookup from one correspondence; boxing and scalar-range admission dispatch through that same row, so ordinary carrier growth adds one value case and one behavior row.
- Law: recovery is keyed, never scanned — the generated `Type` key is the union CASE type `ContentValue.Carrier` reads, so the two recovery axes fold once off `Items` into their own field-type and payload-type indexes and a payload answers its carrier in one hit.
- Law: `ContentCarrier.Declare` captures native declaration failures; `Bytes` uses the value-only `Add` overload and rejects textured or filename presentation.
- Law: a host hole is a row VALUE — `WriteHole` names the payload-to-field pair the host publishes no route for, and `Write` refuses on it before any host call, so the roster stays total and the absence reads as a host limit rather than an omitted case.
- Law: `Null` recovers `NullField`, `DBNull.Value`, and `null` payloads — every shape its declared payload type routes to it — preserves a `NullField` census row, and boxes to `null` for object-typed parameter boundaries; it declares through `FieldDictionary.Add` yet carries the one `WriteHole` in the roster, because the host publishes no `Set` overload reaching a `NullField` — so a null field is declarable and readable but never writable, and a consumer replaces the value by re-declaring rather than setting.
- Boundary: `Color4f` rides the union as the host color boundary value — field payloads are content-parameter truth, and a domain color composes the kernel `PerceptualColor` owner at the consumer that treats it as color, never inside the parameter carrier.
- Packages: `api-rhinocommon-rendercontent.md` (`FieldDictionary.Add`/`AddTextured`/`AddFilename`/`Set`, `Field`, `BoolField`/`IntField`/`FloatField`/`DoubleField`/`Color4fField`/`Vector2dField`/`Vector3dField`/`Point2dField`/`Point3dField`/`Point4dField`/`StringField`/`DateTimeField`/`GuidField`/`TransformField`/`ByteArrayField`/`NullField`); `api-rhinocommon-display.md` (`Color4f`); `api-rhinocommon-geometry.md` (`Vector2d`, `Vector3d`, `Point2d`, `Point3d`, `Point4d`, `Transform`); kernel `Domain/results` (`Try.lift`, `KernelFault.InvalidInput`, `KernelFault.Unsupported`); LanguageExt.Core (`Fin`, `HashMap`, `Arr`, `Option`); Thinktecture.Runtime.Extensions (`[Union]`, `[SmartEnum]`, `[UseDelegateFromConstructor]`).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Rhino.Display;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Render;
using Rhino.Render.Fields;
using Thinktecture;

namespace Rasm.Rhino.Render;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IContentPayload<TSelf, T> where TSelf : ContentValue, IContentPayload<TSelf, T> {
    static abstract TSelf Of(T value);
    T Value { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContentValue : IDetachedDocumentResult {
    private ContentValue() { }
    public sealed record Toggle(bool Value) : ContentValue, IContentPayload<Toggle, bool> { public static Toggle Of(bool value) => new(Value: value); }
    public sealed record Whole(int Value) : ContentValue, IContentPayload<Whole, int> { public static Whole Of(int value) => new(Value: value); }
    public sealed record Single(float Value) : ContentValue, IContentPayload<Single, float> { public static Single Of(float value) => new(Value: value); }
    public sealed record Real(double Value) : ContentValue, IContentPayload<Real, double> { public static Real Of(double value) => new(Value: value); }
    public sealed record Colour(Color4f Value) : ContentValue, IContentPayload<Colour, Color4f> { public static Colour Of(Color4f value) => new(Value: value); }
    public sealed record Vec2(Vector2d Value) : ContentValue, IContentPayload<Vec2, Vector2d> { public static Vec2 Of(Vector2d value) => new(Value: value); }
    public sealed record Vec3(Vector3d Value) : ContentValue, IContentPayload<Vec3, Vector3d> { public static Vec3 Of(Vector3d value) => new(Value: value); }
    public sealed record Pt2(Point2d Value) : ContentValue, IContentPayload<Pt2, Point2d> { public static Pt2 Of(Point2d value) => new(Value: value); }
    public sealed record Pt3(Point3d Value) : ContentValue, IContentPayload<Pt3, Point3d> { public static Pt3 Of(Point3d value) => new(Value: value); }
    public sealed record Pt4(Point4d Value) : ContentValue, IContentPayload<Pt4, Point4d> { public static Pt4 Of(Point4d value) => new(Value: value); }
    public sealed record Text(string Value) : ContentValue, IContentPayload<Text, string> { public static Text Of(string value) => new(Value: value); }
    public sealed record Stamp(DateTime Value) : ContentValue, IContentPayload<Stamp, DateTime> { public static Stamp Of(DateTime value) => new(Value: value); }
    public sealed record Key(Guid Value) : ContentValue, IContentPayload<Key, Guid> { public static Key Of(Guid value) => new(Value: value); }
    public sealed record Motion(Transform Value) : ContentValue, IContentPayload<Motion, Transform> { public static Motion Of(Transform value) => new(Value: value); }
    public sealed record Bytes(Arr<byte> Value) : ContentValue;
    public sealed record Null : ContentValue;

    internal ContentCarrier Carrier => ContentCarrier.Get(GetType());

    internal static Fin<ContentValue> Of(object? payload) =>
        ContentCarrier.Recover(payload: payload);

    internal Fin<Unit> Declare(FieldDeclaration declaration) =>
        Carrier.Declare(declaration: declaration, payload: Boxed());

    internal Fin<Unit> Write(FieldDictionary fields, string name) =>
        Carrier.Write(fields: fields, name: name, payload: Boxed());

    internal object? Boxed() => Carrier.Box(value: this);
}

public readonly record struct FieldDeclaration(
    FieldDictionary Fields,
    string Name,
    string Prompt,
    int Section,
    FieldPresentation Presentation);

[SmartEnum<Type>]
public sealed partial class ContentCarrier {
    public static readonly ContentCarrier Toggle = Row<ContentValue.Toggle, bool, BoolField>(
        read: static field => field.Value,
        plain: static (f, n, v, p, s) => f.Add(n, v, p, s), write: static (f, n, v) => f.Set(n, v),
        textured: static (f, n, v, p, l, s) => f.AddTextured(n, v, p, l, s));
    public static readonly ContentCarrier Whole = Row<ContentValue.Whole, int, IntField>(
        read: static field => field.Value,
        plain: static (f, n, v, p, s) => f.Add(n, v, p, s), write: static (f, n, v) => f.Set(n, v),
        textured: static (f, n, v, p, l, s) => f.AddTextured(n, v, p, l, s),
        ordered: static (lo, hi) => lo <= hi);
    public static readonly ContentCarrier Single = Row<ContentValue.Single, float, FloatField>(
        read: static field => field.Value,
        plain: static (f, n, v, p, s) => f.Add(n, v, p, s), write: static (f, n, v) => f.Set(n, v),
        textured: static (f, n, v, p, l, s) => f.AddTextured(n, v, p, l, s),
        ordered: static (lo, hi) => float.IsFinite(lo) && float.IsFinite(hi) && lo <= hi);
    public static readonly ContentCarrier Real = Row<ContentValue.Real, double, DoubleField>(
        read: static field => field.Value,
        plain: static (f, n, v, p, s) => f.Add(n, v, p, s), write: static (f, n, v) => f.Set(n, v),
        textured: static (f, n, v, p, l, s) => f.AddTextured(n, v, p, l, s),
        ordered: static (lo, hi) => double.IsFinite(lo) && double.IsFinite(hi) && lo <= hi);
    public static readonly ContentCarrier Colour = Row<ContentValue.Colour, Color4f, Color4fField>(
        read: static field => field.Value,
        plain: static (f, n, v, p, s) => f.Add(n, v, p, s), write: static (f, n, v) => f.Set(n, v),
        textured: static (f, n, v, p, l, s) => f.AddTextured(n, v, p, l, s));
    public static readonly ContentCarrier Vec2 = Row<ContentValue.Vec2, Vector2d, Vector2dField>(
        read: static field => field.Value,
        plain: static (f, n, v, p, s) => f.Add(n, v, p, s), write: static (f, n, v) => f.Set(n, v),
        textured: static (f, n, v, p, l, s) => f.AddTextured(n, v, p, l, s));
    public static readonly ContentCarrier Vec3 = Row<ContentValue.Vec3, Vector3d, Vector3dField>(
        read: static field => field.Value,
        plain: static (f, n, v, p, s) => f.Add(n, v, p, s), write: static (f, n, v) => f.Set(n, v),
        textured: static (f, n, v, p, l, s) => f.AddTextured(n, v, p, l, s));
    public static readonly ContentCarrier Pt2 = Row<ContentValue.Pt2, Point2d, Point2dField>(
        read: static field => field.Value,
        plain: static (f, n, v, p, s) => f.Add(n, v, p, s), write: static (f, n, v) => f.Set(n, v),
        textured: static (f, n, v, p, l, s) => f.AddTextured(n, v, p, l, s));
    public static readonly ContentCarrier Pt3 = Row<ContentValue.Pt3, Point3d, Point3dField>(
        read: static field => field.Value,
        plain: static (f, n, v, p, s) => f.Add(n, v, p, s), write: static (f, n, v) => f.Set(n, v),
        textured: static (f, n, v, p, l, s) => f.AddTextured(n, v, p, l, s));
    public static readonly ContentCarrier Pt4 = Row<ContentValue.Pt4, Point4d, Point4dField>(
        read: static field => field.Value,
        plain: static (f, n, v, p, s) => f.Add(n, v, p, s), write: static (f, n, v) => f.Set(n, v),
        textured: static (f, n, v, p, l, s) => f.AddTextured(n, v, p, l, s));
    public static readonly ContentCarrier Text = Row<ContentValue.Text, string, StringField>(
        read: static field => field.Value,
        plain: static (f, n, v, p, s) => f.Add(n, v, p, s), write: static (f, n, v) => f.Set(n, v),
        textured: static (f, n, v, p, l, s) => f.AddTextured(n, v, p, l, s));
    public static readonly ContentCarrier Stamp = Row<ContentValue.Stamp, DateTime, DateTimeField>(
        read: static field => field.Value,
        plain: static (f, n, v, p, s) => f.Add(n, v, p, s), write: static (f, n, v) => f.Set(n, v),
        textured: static (f, n, v, p, l, s) => f.AddTextured(n, v, p, l, s),
        ordered: static (lo, hi) => lo <= hi);
    public static readonly ContentCarrier Key = Row<ContentValue.Key, Guid, GuidField>(
        read: static field => field.Value,
        plain: static (f, n, v, p, s) => f.Add(n, v, p, s), write: static (f, n, v) => f.Set(n, v),
        textured: static (f, n, v, p, l, s) => f.AddTextured(n, v, p, l, s));
    public static readonly ContentCarrier Motion = Row<ContentValue.Motion, Transform, TransformField>(
        read: static field => field.Value,
        plain: static (f, n, v, p, s) => f.Add(n, v, p, s), write: static (f, n, v) => f.Set(n, v),
        textured: static (f, n, v, p, l, s) => f.AddTextured(n, v, p, l, s));
    public static readonly ContentCarrier Bytes = new(
        key: typeof(ContentValue.Bytes), fieldType: typeof(ByteArrayField), payloadType: typeof(byte[]),
        box: static value => value is ContentValue.Bytes bytes ? bytes.Value.ToArray() : null,
        acceptsRange: static (_, _) => false,
        read: static (payload, key) => payload switch {
            ByteArrayField field => Fin.Succ<ContentValue>(value: new ContentValue.Bytes(Value: toArray(field.Value))),
            byte[] value => Fin.Succ<ContentValue>(value: new ContentValue.Bytes(Value: toArray(value))),
            _ => Fin.Fail<ContentValue>(error: new KernelFault.InvalidResult()),
        },
        declare: static (declaration, payload, key) => declaration.Presentation is FieldPresentation.Plain && payload is byte[] bytes
            ? Try.lift(() => { _ = declaration.Fields.Add(declaration.Name, bytes); return Fin.Succ(value: unit); }).Run().Bind(static inner => inner)
            : Fin.Fail<Unit>(error: new KernelFault.InvalidInput()),
        writeHole: None,
        store: static (fields, name, payload, key) => payload is byte[] bytes
            ? Try.lift(() => { fields.Set(name, bytes); return Fin.Succ(value: unit); }).Run().Bind(static inner => inner)
            : Fin.Fail<Unit>(error: new KernelFault.InvalidInput()));
    public static readonly ContentCarrier Null = new(
        key: typeof(ContentValue.Null), fieldType: typeof(NullField), payloadType: typeof(DBNull),
        box: static _ => null,
        acceptsRange: static (_, _) => false,
        read: static (payload, key) => payload is NullField or DBNull or null
            ? Fin.Succ<ContentValue>(value: new ContentValue.Null())
            : Fin.Fail<ContentValue>(error: new KernelFault.InvalidResult()),
        declare: static (declaration, _, key) => declaration.Presentation.Switch(
            state: declaration,
            plain: static (ctx, _) => Try.lift(() => { _ = ctx.Fields.Add(ctx.Name, ctx.Prompt, ctx.Section); return Fin.Succ(value: unit); }).Run().Bind(static inner => inner),
            textured: static (ctx, row) => Try.lift(() => { _ = ctx.Fields.AddTextured(ctx.Name, ctx.Prompt, row.TreatAsLinear, ctx.Section); return Fin.Succ(value: unit); }).Run().Bind(static inner => inner),
            filename: static (ctx, _) => Fin.Fail<Unit>(error: new KernelFault.InvalidInput())),
        writeHole: Some((From: typeof(ContentValue.Null), To: typeof(NullField))),
        store: static (_, _, _, key) => Fin.Fail<Unit>(
            error: new KernelFault.Unsupported(InputType: typeof(ContentValue.Null), OutputType: typeof(NullField))));

    public Type FieldType { get; }
    public Type PayloadType { get; }

    internal Option<(Type From, Type To)> WriteHole { get; }

    [UseDelegateFromConstructor]
    internal partial object? Box(ContentValue value);

    [UseDelegateFromConstructor]
    internal partial bool AcceptsRange(ContentValue min, ContentValue max);

    [UseDelegateFromConstructor]
    internal partial Fin<ContentValue> Read(object? payload);

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> Declare(FieldDeclaration declaration, object? payload);

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> Store(FieldDictionary fields, string name, object? payload);

    internal Fin<Unit> Write(FieldDictionary fields, string name, object? payload) =>
        WriteHole.Match(
            Some: hole => Fin.Fail<Unit>(error: new KernelFault.Unsupported(InputType: hole.From, OutputType: hole.To)),
            None: () => Store(fields: fields, name: name, payload: payload));

    private static readonly Lazy<(HashMap<Type, ContentCarrier> ByField, HashMap<Type, ContentCarrier> ByPayload)> Index = new(
        static () => toSeq(Items).Fold(
            (ByField: HashMap<Type, ContentCarrier>(), ByPayload: HashMap<Type, ContentCarrier>()),
            static (state, row) => (
                ByField: state.ByField.Add(row.FieldType, row),
                ByPayload: state.ByPayload.Add(row.PayloadType, row))),
        LazyThreadSafetyMode.ExecutionAndPublication);

    internal static Fin<ContentValue> Recover(object? payload) =>
        payload is null
            ? Null.Read(payload: null)
            : (payload is Field ? Index.Value.ByField : Index.Value.ByPayload)
                .Find(payload.GetType())
                .ToFin(Fail: new KernelFault.InvalidResult(Detail: Some(payload.GetType().Name)))
                .Bind(row => row.Read(payload: payload));

    private static ContentCarrier Row<TCase, T, TField>(
        Func<TField, T> read,
        Func<FieldDictionary, string, T, string, int, Field> plain,
        Func<FieldDictionary, string, T, string, bool, int, Field> textured,
        Action<FieldDictionary, string, T> write,
        Func<T, T, bool>? ordered = null)
        where TCase : ContentValue, IContentPayload<TCase, T>
        where TField : Field =>
        new(
            key: typeof(TCase),
            fieldType: typeof(TField),
            payloadType: typeof(T),
            box: static value => value is TCase typed ? (object?)typed.Value : null,
            acceptsRange: (min, max) => ordered is not null
                && min is TCase lower
                && max is TCase upper
                && ordered(lower.Value, upper.Value),
            read: (payload, op) => payload switch {
                TField field => Fin.Succ<ContentValue>(value: TCase.Of(read(field))),
                T value => Fin.Succ<ContentValue>(value: TCase.Of(value)),
                _ => Fin.Fail<ContentValue>(error: new KernelFault.InvalidResult()),
            },
            declare: (declaration, payload, op) => payload is T value
                ? declaration.Presentation.Switch(
                    state: (Declaration: declaration, Value: value, Plain: plain, Textured: textured),
                    plain: static (ctx, _) => Try.lift(() => { _ = ctx.Plain(ctx.Declaration.Fields, ctx.Declaration.Name, ctx.Value, ctx.Declaration.Prompt, ctx.Declaration.Section); return Fin.Succ(value: unit); }).Run().Bind(static inner => inner),
                    textured: static (ctx, row) => Try.lift(() => { _ = ctx.Textured(ctx.Declaration.Fields, ctx.Declaration.Name, ctx.Value, ctx.Declaration.Prompt, row.TreatAsLinear, ctx.Declaration.Section); return Fin.Succ(value: unit); }).Run().Bind(static inner => inner),
                    filename: static (ctx, _) => ctx.Value is string path
                        ? Try.lift(() => { _ = ctx.Declaration.Fields.AddFilename(ctx.Declaration.Name, path, ctx.Declaration.Prompt, ctx.Declaration.Section); return Fin.Succ(value: unit); }).Run().Bind(static inner => inner)
                        : Fin.Fail<Unit>(error: new KernelFault.InvalidInput()))
                : Fin.Fail<Unit>(error: new KernelFault.InvalidInput()),
            writeHole: None,
            store: (fields, name, payload, op) => payload is T value
                ? Try.lift(() => { write(fields, name, value); return Fin.Succ(value: unit); }).Run().Bind(static inner => inner)
                : Fin.Fail<Unit>(error: new KernelFault.InvalidInput()));
}
```

## [03]-[DECLARATION]

- Owner: `FieldPresentation` `[Union]` — the declaration posture: `Plain` the ordinary field, `Textured` with its treat-as-linear grant through `AddTextured`, `Filename` the file-path string through `AddFilename`; `FieldSpec` — one declaration row: name, initial `ContentValue`, prompt, section, presentation; `DynamicFieldSpec` — one admitted runtime row whose optional bounds ride one ordered scalar carrier — finite, `Min <= Max`, carrier-equal to the value — declared inside the host begin/end bracket as one fold.
- Law: declaration is data — a content class's field roster is a `Seq<FieldSpec>` declared in one pass, so the roster is diffable and a new field is one row; a hand-spelled `Add` chain beside the spec fold is the deleted form.
- Law: `FieldRange` delegates scalar ordering to the same `ContentCarrier` row that boxes and declares the case; non-scalar rows refuse bounds without a parallel case roster.
- Law: refusal is per clause — `FactoryValidation` names every failing constraint at once, so one repair round sees the whole invalid row.
- Law: `DynamicFields.Declare` opens `BeginCreateDynamicFields` and closes `EndCreateDynamicFields` on the same `Lease<T>` custody bracket the content page's `ChangeScope` takes, so an `EndCreateDynamicFields` refusal AGGREGATES into the row fold's own fault instead of replacing it the way a `finally` does.
- Law: a textured or filename declaration is a presentation row, never a sibling spec type — the presentation discriminates the host `Add` overload, and the returned typed field stays inside the fold.
- Boundary: `RenderFault` on `FaultBand.HostRender 4950/4` is this branch's render admission family, minted at `Display/render.md`; this page codes its value-object refusals on it and mints no second family.
- Boundary: `FieldSpec`, `DynamicFields`, and `FieldBinding` carry NO in-package caller today. `Render/registry.md`'s render-editor shell is the declared consumer; until that page seats them the obligation is open, and an unseated owner at the next pass deletes rather than persisting unreached.
- Packages: `api-rhinocommon-rendercontent.md` (`RenderContent.BeginCreateDynamicFields`/`CreateDynamicField`/`EndCreateDynamicFields`, `FieldDictionary.Add`/`AddTextured`/`AddFilename`); kernel `Domain/results` (`Lease<T>.Acquire`/`Use`, `Acceptance.Text`, `Admit.Need`, `Admit.Confirm`), `Domain/validation` (`FactoryBridge.Accept<TVO>`, `FactoryValidation`); `Display/render.md` (`RenderFault`); LanguageExt.Core (`Fin`, `Option`, `Seq`, `TraverseM`); Thinktecture.Runtime.Extensions (`[Union]`, `[ComplexValueObject]`, `[ValidationError]`).

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FieldPresentation {
    private FieldPresentation() { }
    public sealed record Plain : FieldPresentation;
    public sealed record Textured(bool TreatAsLinear) : FieldPresentation;
    public sealed record Filename : FieldPresentation;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FieldSpec(
    string Name,
    ContentValue Value,
    FieldPresentation Presentation,
    Option<string> Prompt = default,
    int SectionId = 0) {
    internal Fin<Unit> Declare(FieldDictionary fields) {
        FieldSpec self = this;
        return from name in Acceptance.Text(value: self.Name)
               from value in Admit.Need(self.Value)
               from presentation in Admit.Need(self.Presentation)
               from _ in value.Declare(
                   declaration: new FieldDeclaration(
                       Fields: fields,
                       Name: name,
                       Prompt: self.Prompt.IfNone(name),
                       Section: self.SectionId,
                       Presentation: presentation))
               select unit;
    }
}

[ComplexValueObject]
[ValidationError]
public sealed partial class FieldRange {
    public ContentValue Min { get; }
    public ContentValue Max { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ContentValue min,
        ref ContentValue max) =>
        validationError = min is not null && max is not null && min.Carrier.AcceptsRange(min: min, max: max)
            ? null
            : new ValidationError(string.Join(" | ", new object?[] { nameof(FieldRange), "an ordered carrier-equal scalar pair" }));

    internal static Fin<FieldRange> Of(ContentValue min, ContentValue max) =>
        FactoryBridge.Accept<FieldRange>(Validate(min, max, out FieldRange? range), range);
}

[ComplexValueObject]
[ValidationError]
public sealed partial class DynamicFieldSpec {
    public string InternalName { get; }
    public string LocalName { get; }
    public string EnglishName { get; }
    public ContentValue Value { get; }
    public Option<FieldRange> Bounds { get; }
    public int SectionId { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string internalName,
        ref string localName,
        ref string englishName,
        ref ContentValue value,
        ref Option<FieldRange> bounds,
        ref int sectionId) =>
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
            (string.IsNullOrWhiteSpace(internalName), () => new ValidationClause(nameof(internalName))),
            (string.IsNullOrWhiteSpace(localName), () => new ValidationClause(nameof(localName))),
            (string.IsNullOrWhiteSpace(englishName), () => new ValidationClause(nameof(englishName))),
            (value is null, () => new ValidationClause(nameof(value))),
            (sectionId < 0, () => new ValidationClause(string.Join(" | ", new object?[] {
                nameof(sectionId), "a non-negative section index" }))),
            (bounds.Case is FieldRange range && value is not null && range.Min.Carrier != value.Carrier,
                () => new ValidationClause(string.Join(" | ", new object?[] {
                    nameof(bounds), "bounds carried by the value's own carrier" }))));

    public static Fin<DynamicFieldSpec> Of(
        string internalName, string localName, string englishName, ContentValue value,
        Option<(ContentValue Min, ContentValue Max)> bounds, int sectionId) {
        return from range in bounds.Traverse(row => FieldRange.Of(min: row.Min, max: row.Max)).As()
               from admitted in FactoryBridge.Accept<DynamicFieldSpec>(
                   Validate(internalName, localName, englishName, value, range, sectionId, out DynamicFieldSpec? created), created)
               select admitted;
    }
}

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class DynamicFieldScope : IDisposable {
    private readonly RenderContent content;

    internal DynamicFieldScope(RenderContent content, bool automatic) {
        this.content = content;
        content.BeginCreateDynamicFields(automatic: automatic);
    }

    public void Dispose() => content.EndCreateDynamicFields();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DynamicFields {
    internal static Fin<Unit> Declare(RenderContent content, bool automatic, Seq<DynamicFieldSpec> rows) =>
        Lease<DynamicFieldScope>.Acquire(mint: () => new DynamicFieldScope(content: content, automatic: automatic))
            .Bind(scope => scope.Use(
                body: _ => rows.TraverseM(row =>
                    from admitted in Admit.Need(row)
                    let bounds = admitted.Bounds
                        .Map(static range => (Min: (object?)range.Min.Boxed(), Max: (object?)range.Max.Boxed()))
                        .IfNone((Min: null, Max: null))
                    from created in Try.lift(() => Admit.Confirm(success: content.CreateDynamicField(
                        internalName: admitted.InternalName,
                        localName: admitted.LocalName,
                        englishName: admitted.EnglishName,
                        value: admitted.Value.Boxed(),
                        minValue: bounds.Min,
                        maxValue: bounds.Max,
                        sectionId: admitted.SectionId))).Run().Bind(static inner => inner)
                    select created).As().Map(static _ => unit)));
}
```

## [04]-[BINDING_AND_PARAMS]

- Owner: `FieldBinding` admits direct and child-slot field bindings through one optional-slot factory. `ParamScope` admits named, child-slot extra-requirement, and direct extra-requirement routes. `FieldPortrait` and `FieldCensus` detach the dictionary in one pass.
- Law: each `ParamScope` case reaches its corresponding host endpoint; child-slot and direct extra-requirement semantics remain distinct cases.
- Law: name resolution stays host-owned — `ChildSlotNameFromParamName`/`ParamNameFromChildSlotName` answer the correspondence at the consulting site, and no local table mirrors it.
- Law: a parameter or child-slot name never enters as a literal — `PbrChannel` and `ContentParameter` are the two admitted name vocabularies and `ParamScope` takes one of them, so the only `string` a caller supplies is the extra-requirement key the host itself leaves open.
- Law: `PbrChannel` keys on the host texture type because the PBR name space is DERIVED, not rostered — every child-slot name resolves through `ChildSlotNames.PhysicallyBased.FromTextureType` and every PBR parameter name but `pbr-brdf` forwards to that same child-slot name, so one key column answers both axes and a second parallel roster is the deleted form. `pbr-brdf` is the one PBR property answering a literal instead of forwarding, so it is a `PbrChannel` row carrying that literal on the derived-key vocabulary it belongs to — `Name` answers it and `Slot` refuses it, because a parameter forwarding to no child slot cannot spell one. `ContentParameter` is therefore the basic-material `const` names alone, the only genuinely enumerated parameters the host declares, so no row hand-copies a name the host itself publishes.
- Law: `PbrChannel.SlotOf` is the ONE reading of the host resolver — the channel axis and the `StandardChildSlots` arity both call it, so the empty-string sentinel projects to a typed fault at one site and a second inline copy of the same three-step admission is the deleted form.
- Law: `Child(RenderMaterial.StandardChildSlots)` composes `TextureTypeFromSlot` then that same resolver, so the slot enum stays the vocabulary `Render/kinds.md` `[02]` rules it and this page mints no slot wrapper.
- Law: reads recover typed — a `ParamScope` read boxes through the host and immediately classifies into `ContentValue` by runtime payload type, so `object` dies at this boundary.
- Law: `FieldCensus.Of` traverses `FieldDictionary` once and projects value, texture bounds, usage grants, and visibility per field.
- Packages: `api-rhinocommon-rendercontent.md` (`RenderContent.BindParameterToField` both arities, `GetParameter`/`SetParameter`, `GetChildSlotParameter`/`SetChildSlotParameter`, `GetExtraRequirementParameter`/`SetExtraRequirementParameter`, `ExtraRequirementsSetContexts`, `ChildSlotNames.PhysicallyBased.FromTextureType`, `ParameterNames.PhysicallyBased.BRDF`, `RenderMaterial.BasicMaterialParameterNames`, `RenderMaterial.TextureTypeFromSlot`, `Field.TextureAmountMin`/`TextureAmountMax`/`UseTextureOn`/`UseTextureAmount`/`IsHiddenInAutoUI`); `api-rhinocommon-objects.md` (`TextureType`); kernel `Domain/results` (`Acceptance.Text`, `Try.lift`); LanguageExt.Core (`Fin`, `Option`, `Arr`, `TraverseM`); Thinktecture.Runtime.Extensions (`[Union]`, `[SmartEnum]`).

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FieldBinding {
    private FieldBinding() { }
    private sealed record DirectCase(string Parameter) : FieldBinding;
    private sealed record AtSlotCase(string Parameter, string ChildSlot) : FieldBinding;

    public static Fin<FieldBinding> Of(string parameter, Option<string> childSlot = default) {
        return from admittedParameter in Acceptance.Text(value: parameter)
               from admittedSlot in childSlot.Traverse(slot => Acceptance.Text(value: slot)).As()
               select admittedSlot.Match(
                   Some: slot => (FieldBinding)new AtSlotCase(Parameter: admittedParameter, ChildSlot: slot),
                   None: () => new DirectCase(Parameter: admittedParameter));
    }

    internal Fin<Unit> Bind(RenderContent content, Field field, ChangeReason reason) =>
        ChangeScope.Write(content: content, reason: reason, body: live => Switch(
            state: (Content: live, Field: field, Reason: reason),
            directCase: static (ctx, binding) => Try.lift(() => {
                ctx.Content.BindParameterToField(parameterName: binding.Parameter, field: ctx.Field, setEvent: ctx.Reason.Native);
                return Fin.Succ(value: unit);
            }).Run().Bind(static inner => inner),
            atSlotCase: static (ctx, binding) => Try.lift(() => {
                ctx.Content.BindParameterToField(
                    parameterName: binding.Parameter, childSlotName: binding.ChildSlot, field: ctx.Field, setEvent: ctx.Reason.Native);
                return Fin.Succ(value: unit);
            }).Run().Bind(static inner => inner)));
}

[SmartEnum<TextureType>]
public sealed partial class PbrChannel {
    public static readonly PbrChannel Brdf = new(
        key: TextureType.None, literal: Some(global::Rhino.Render.ParameterNames.PhysicallyBased.BRDF));
    public static readonly PbrChannel BaseColor = new(key: TextureType.Bitmap);
    public static readonly PbrChannel Subsurface = new(key: TextureType.PBR_Subsurface);
    public static readonly PbrChannel SubsurfaceScatteringColor = new(key: TextureType.PBR_SubsurfaceScattering);
    public static readonly PbrChannel SubsurfaceScatteringRadius = new(key: TextureType.PBR_SubsurfaceScatteringRadius);
    public static readonly PbrChannel Specular = new(key: TextureType.PBR_Specular);
    public static readonly PbrChannel SpecularTint = new(key: TextureType.PBR_SpecularTint);
    public static readonly PbrChannel Metallic = new(key: TextureType.PBR_Metallic);
    public static readonly PbrChannel Roughness = new(key: TextureType.PBR_Roughness);
    public static readonly PbrChannel Anisotropic = new(key: TextureType.PBR_Anisotropic);
    public static readonly PbrChannel AnisotropicRotation = new(key: TextureType.PBR_Anisotropic_Rotation);
    public static readonly PbrChannel Sheen = new(key: TextureType.PBR_Sheen);
    public static readonly PbrChannel SheenTint = new(key: TextureType.PBR_SheenTint);
    public static readonly PbrChannel Clearcoat = new(key: TextureType.PBR_Clearcoat);
    public static readonly PbrChannel ClearcoatRoughness = new(key: TextureType.PBR_ClearcoatRoughness);
    public static readonly PbrChannel ClearcoatBump = new(key: TextureType.PBR_ClearcoatBump);
    public static readonly PbrChannel OpacityIor = new(key: TextureType.PBR_OpacityIor);
    public static readonly PbrChannel Opacity = new(key: TextureType.Transparency);
    public static readonly PbrChannel OpacityRoughness = new(key: TextureType.PBR_OpacityRoughness);
    public static readonly PbrChannel Emission = new(key: TextureType.PBR_Emission);
    public static readonly PbrChannel Displacement = new(key: TextureType.PBR_Displacement);
    public static readonly PbrChannel Bump = new(key: TextureType.Bump);
    public static readonly PbrChannel AmbientOcclusion = new(key: TextureType.PBR_AmbientOcclusion);
    public static readonly PbrChannel Alpha = new(key: TextureType.PBR_Alpha);

    internal Option<string> Literal { get; }

    internal Fin<string> Name() => Literal.Match(Some: Fin.Succ, None: () => Slot());

    internal Fin<string> Slot() => Literal.IsSome
        ? Fin.Fail<string>(error: new KernelFault.Unsupported(InputType: typeof(PbrChannel), OutputType: typeof(TextureType)))
        : SlotOf(textureType: Key);

    internal static Fin<string> SlotOf(TextureType textureType) =>
        Try.lift(() =>
            Optional(global::Rhino.Render.ChildSlotNames.PhysicallyBased.FromTextureType(textureType: textureType))
                .Filter(static name => !string.IsNullOrWhiteSpace(name))
                .ToFin(Fail: new KernelFault.InvalidResult(Detail: Some(textureType.ToString())))).Run().Bind(static inner => inner);
}

[SmartEnum<string>]
public sealed partial class ContentParameter {
    public static readonly ContentParameter Ambient = new(RenderMaterial.BasicMaterialParameterNames.Ambient);
    public static readonly ContentParameter Emission = new(RenderMaterial.BasicMaterialParameterNames.Emission);
    public static readonly ContentParameter FlamingoLibrary = new(RenderMaterial.BasicMaterialParameterNames.FlamingoLibrary);
    public static readonly ContentParameter DisableLighting = new(RenderMaterial.BasicMaterialParameterNames.DisableLighting);
    public static readonly ContentParameter Diffuse = new(RenderMaterial.BasicMaterialParameterNames.Diffuse);
    public static readonly ContentParameter Specular = new(RenderMaterial.BasicMaterialParameterNames.Specular);
    public static readonly ContentParameter TransparencyColor = new(RenderMaterial.BasicMaterialParameterNames.TransparencyColor);
    public static readonly ContentParameter ReflectivityColor = new(RenderMaterial.BasicMaterialParameterNames.ReflectivityColor);
    public static readonly ContentParameter Shine = new(RenderMaterial.BasicMaterialParameterNames.Shine);
    public static readonly ContentParameter Transparency = new(RenderMaterial.BasicMaterialParameterNames.Transparency);
    public static readonly ContentParameter Reflectivity = new(RenderMaterial.BasicMaterialParameterNames.Reflectivity);
    public static readonly ContentParameter Ior = new(RenderMaterial.BasicMaterialParameterNames.Ior);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ParamScope {
    private ParamScope() { }
    private sealed record NamedCase(string Parameter) : ParamScope;
    private sealed record ChildCase(string ChildSlot, string Requirement) : ParamScope;
    private sealed record ExtraCase(string Parameter, string Requirement) : ParamScope;

    public static Fin<ParamScope> Named(ContentParameter parameter) {
        return Admit.Need(parameter)
            .Map(static admitted => (ParamScope)new NamedCase(Parameter: admitted.Key));
    }

    public static Fin<ParamScope> Named(PbrChannel channel) {
        return from active in Admit.Need(channel)
               from name in active.Name()
               select (ParamScope)new NamedCase(Parameter: name);
    }

    public static Fin<ParamScope> Child(PbrChannel channel, string requirement) {
        return from active in Admit.Need(channel)
               from slot in active.Slot()
               from admittedRequirement in Acceptance.Text(value: requirement)
               select (ParamScope)new ChildCase(ChildSlot: slot, Requirement: admittedRequirement);
    }

    public static Fin<ParamScope> Child(RenderMaterial.StandardChildSlots slot, string requirement) {
        return from textureType in Try.lift(() => Fin.Succ(value: RenderMaterial.TextureTypeFromSlot(slot: slot))).Run().Bind(static inner => inner)
               from name in PbrChannel.SlotOf(textureType: textureType)
               from admittedRequirement in Acceptance.Text(value: requirement)
               select (ParamScope)new ChildCase(ChildSlot: name, Requirement: admittedRequirement);
    }

    public static Fin<ParamScope> Extra(string parameter, string requirement) {
        return from admittedParameter in Acceptance.Text(value: parameter)
               from admittedRequirement in Acceptance.Text(value: requirement)
               select (ParamScope)new ExtraCase(Parameter: admittedParameter, Requirement: admittedRequirement);
    }

    internal Fin<ContentValue> Read(RenderContent content) =>
        Switch(
            state: content,
            namedCase: static (ctx, scope) => Try.lift(() => ContentValue.Of(
                payload: ctx.GetParameter(parameterName: scope.Parameter))).Run().Bind(static inner => inner),
            childCase: static (ctx, scope) => Try.lift(() => ContentValue.Of(
                payload: ctx.GetChildSlotParameter(scope.ChildSlot, scope.Requirement))).Run().Bind(static inner => inner),
            extraCase: static (ctx, scope) => Try.lift(() => ContentValue.Of(
                payload: ctx.GetExtraRequirementParameter(
                    contentParameterName: scope.Parameter,
                    extraRequirementParameter: scope.Requirement))).Run().Bind(static inner => inner));

    internal Fin<Unit> Write(
        RenderContent content, ContentValue value, ChangeReason reason,
        RenderContent.ExtraRequirementsSetContexts context) =>
        ChangeScope.Write(content: content, reason: reason, body: live => Switch(
            state: (Content: live, Value: value, Context: context),
            namedCase: static (ctx, scope) => Try.lift(() => Admit.Confirm(success: ctx.Content.SetParameter(
                parameterName: scope.Parameter, value: ctx.Value.Boxed()))).Run().Bind(static inner => inner),
            childCase: static (ctx, scope) => Try.lift(() => Admit.Confirm(success: ctx.Content.SetChildSlotParameter(
                scope.ChildSlot, scope.Requirement, ctx.Value.Boxed(), ctx.Context))).Run().Bind(static inner => inner),
            extraCase: static (ctx, scope) => Try.lift(() => Admit.Confirm(success: ctx.Content.SetExtraRequirementParameter(
                contentParameterName: scope.Parameter,
                extraRequirementParameter: scope.Requirement,
                value: ctx.Value.Boxed(),
                sc: ctx.Context))).Run().Bind(static inner => inner)));
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FieldPortrait(
    string Name,
    ContentValue Value,
    double TextureAmountMin,
    double TextureAmountMax,
    bool UseTextureOn,
    bool UseTextureAmount,
    bool HiddenInAutoUi) : IDetachedDocumentResult;

public sealed record FieldCensus(Arr<FieldPortrait> Rows) : IDetachedDocumentResult {
    internal static Fin<FieldCensus> Of(FieldDictionary fields) =>
        Try.lift(() => toSeq(fields)
            .TraverseM(field => ContentValue.Of(payload: field).Map(value => new FieldPortrait(
                Name: field.Name,
                Value: value,
                TextureAmountMin: field.TextureAmountMin,
                TextureAmountMax: field.TextureAmountMax,
                UseTextureOn: field.UseTextureOn,
                UseTextureAmount: field.UseTextureAmount,
                HiddenInAutoUi: field.IsHiddenInAutoUI)))
            .As()
            .Map(static rows => new FieldCensus(Rows: toArray(rows)))).Run().Bind(static inner => inner);
}
```

## [05]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]         | [OWNER]            | [FORM]                                                    | [ENTRY]                        |
| :-----: | :---------------- | :----------------- | :-------------------------------------------------------- | :----------------------------- |
|  [01]   | payload family    | `ContentValue`     | union cases derived through `ContentCarrier` rows         | `Declare` / `Write` / `Of`     |
|  [02]   | payload contract  | `IContentPayload`  | the case's own static mint and payload read               | `Of(value)` / `Value`          |
|  [03]   | field declaration | `FieldSpec`        | name + value + prompt + section + presentation row        | `Declare(fields)`         |
|  [04]   | dynamic fields    | `DynamicFieldSpec` | clause-accumulating admission plus bracketed traversal    | `Of` / `DynamicFields.Declare` |
|  [05]   | parameter binding | `FieldBinding`     | admitted direct and child-slot cases                      | `Of` / `Bind`                  |
|  [06]   | parameter routes  | `ParamScope`       | named, child-slot, and direct-extra cases                 | `Named` / `Child` / `Extra`    |
|  [07]   | field census      | `FieldCensus`      | one-pass dictionary walk to detached `FieldPortrait` rows | `Of(fields)`              |
|  [08]   | pbr name space    | `PbrChannel`       | texture-type keyed, one host resolver for both axes       | `Name` / `Slot` / `SlotOf`     |
|  [09]   | basic param names | `ContentParameter` | the basic-material name constants                         | `ParamScope.Named`             |

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
