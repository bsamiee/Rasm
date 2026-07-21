# [RASM_RHINO_ETO_ELEMENTS]

`Element.Realize` folds one typed screen tree into an `ElementReceipt` that owns the control, descendant receipts, binding receipts, native resources, style tracking, and deterministic teardown. Control modalities carry only the evidence their constructors consume, grid ingress stays generic until the host object-array edge, partial realization releases every completed child before returning a fault, and an off-thread realization refuses with a typed fault before any host leaf mints.

## [01]-[INDEX]

- [02]-[FAULT]: `UiFault` is the closed failure family shared by every Eto owner.
- [03]-[SPEC]: `ElementKey`, `ElementState`, `ElementSpec`, `ElementRuntime`, and `ElementReceipt` own uniform state and lifecycle.
- [04]-[CONTROL]: `ControlSpec` carries the complete exact-payload control family.
- [05]-[TREE]: `Element` owns recursive realization across controls, chrome, layout, canvas, native, web, inspector-skin, collection-editor, and custom cases behind one thread-identity gate.
- [06]-[GRID]: `GridPlan<TRow>` keeps rows typed through flat or tree realization and the complete cell family.
- [07]-[LAYOUT]: `LayoutPlan` folds stack, table, flow, and absolute arrangements from recursive elements.
- [08]-[NOTICE]: `ThemedNotice` mints the themed modal message box with result-typed choices.

## [02]-[FAULT]

- Owner: `UiFault` carries every expected UI failure as a typed case and derives message plus category through one total dispatch; `FaultRail` folds admission, cleanup, reporting, and inner-step failures without escaping its typed rail.
- Cases: dismissal, cancellation, capability absence, thread refusal, admission rejection, absent payload, host refusal, and released-resource access.
- Growth: another failure is one case and compiler-forced message/category arms; another cleanup fold is one `FaultRail` extension member, never a per-owner Match block.
- Boundary: host exceptions enter through the raising owner's capture seam; no UI owner mints a bare string error.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Eto.Drawing;
using Eto.Forms;
using Eto.Forms.ThemedControls;
using Rasm.Csp;
using Rasm.Domain;
using System.Collections.Frozen;
using EtoThread = Eto.Threading.Thread;

namespace Rasm.Rhino.Eto;

// --- [ERRORS] -------------------------------------------------------------------------------
[Union]
public abstract partial record UiFault : Expected {
    private UiFault() : base() { }
    public sealed record Dismissed(Op Key) : UiFault;
    public sealed record Cancelled(Op Key) : UiFault;
    public sealed record Unavailable(Op Key, string Capability) : UiFault;
    public sealed record OffThread(Op Key) : UiFault;
    public sealed record Rejected(Op Key, string Field, string Reason) : UiFault;
    public sealed record AbsentPayload(Op Key, string Payload) : UiFault;
    public sealed record HostRejected(Op Key, string Detail) : UiFault;
    public sealed record Released(Op Key, string Resource) : UiFault;

    public override string Category => Switch(
        dismissed: static _ => nameof(Dismissed),
        cancelled: static _ => nameof(Cancelled),
        unavailable: static _ => nameof(Unavailable),
        offThread: static _ => nameof(OffThread),
        rejected: static _ => nameof(Rejected),
        absentPayload: static _ => nameof(AbsentPayload),
        hostRejected: static _ => nameof(HostRejected),
        released: static _ => nameof(Released));

    public override string Message => Switch(
        dismissed: static fault => $"UI operation '{fault.Key}' was dismissed.",
        cancelled: static fault => $"UI operation '{fault.Key}' was cancelled.",
        unavailable: static fault => $"UI operation '{fault.Key}' requires '{fault.Capability}'.",
        offThread: static fault => $"UI operation '{fault.Key}' requires the Eto UI thread.",
        rejected: static fault => $"UI operation '{fault.Key}' rejected '{fault.Field}': {fault.Reason}",
        absentPayload: static fault => $"UI operation '{fault.Key}' requires transfer payload '{fault.Payload}'.",
        hostRejected: static fault => $"UI operation '{fault.Key}' was refused by the host: {fault.Detail}",
        released: static fault => $"UI operation '{fault.Key}' addressed released resource '{fault.Resource}'.");
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class FaultRail {
    public static Fin<TValue> Admit<TValue>(
        TValue value,
        Op key,
        params (bool Valid, string Field, string Reason)[] rules) =>
        toSeq(rules)
            .Traverse(rule => (rule.Valid
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new UiFault.Rejected(Key: key, Field: rule.Field, Reason: rule.Reason))).ToValidation())
            .As()
            .ToFin()
            .Map(_ => value);

    extension(Error fault) {
        public Error Also(Fin<Unit> cleanup) => cleanup.Match(Succ: _ => fault, Fail: extra => fault + extra);

        public Error Reported(Action<Error> report, Op key) =>
            key.Catch(() => Fin.Succ(Op.Side(() => report(fault))))
                .Match(Succ: _ => fault, Fail: extra => fault + extra);
    }

    extension<TValue>(Fin<TValue> outcome) {
        public Fin<TValue> Sealed(Fin<Unit> cleanup) => cleanup.Match(
            Succ: _ => outcome,
            Fail: fault => outcome.Match(
                Succ: _ => Fin.Fail<TValue>(fault),
                Fail: prior => Fin.Fail<TValue>(prior + fault)));
    }

    extension(Seq<Action> steps) {
        public Fin<Unit> Drained(Op key) => steps
            .Map(step => (Func<Fin<Unit>>)(() => key.Catch(() => Fin.Succ(Op.Side(step)))))
            .Drained(key);
    }

    extension(Seq<Func<Fin<Unit>>> steps) {
        public Fin<Unit> Drained(Op key) => steps
            .Traverse(step => Try.lift(step).Run()
                .Bind(static result => result)
                .MapFail(error => new UiFault.HostRejected(Key: key, Detail: error.Message))
                .ToValidation())
            .As()
            .Map(static _ => unit)
            .ToFin();
    }
}
```

## [03]-[SPEC]

- Owner: `ElementSpec` applies identity, state, tooltip, style, and binding plans once; `ElementReceipt` owns the realized subtree; `UiLease` is the one release gate — every Eto capsule derives it, so the interlocked one-shot release, the `Released` probe, and the throw-on-fault `Dispose` terminal exist exactly once.
- Entry: `Element.Realize` admits its shared `ElementSpec` and runtime before dispatch; `ElementReceipt.Mint` brackets construction, and `ElementReceipt.Create` admits the host without returning a live control outside its owner.
- Auto: binding plans fold fail-fast and release earlier receipts on failure; styles enroll through the injected `ThemeSeam`.
- Receipt: `ElementReceipt` exposes the host control and accumulates every binding, child, resource, and host release fault before terminating.
- Growth: another uniform control axis is one `ElementSpec` field consumed in `Create`.
- Exemption: control mutation, provider construction, and the all-attempted `IDisposable` terminal are the host-control statement seam.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct ElementKey {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        if (string.IsNullOrWhiteSpace(value)) {
            validationError = new ValidationError(message: "Element identity requires text.");
            return;
        }
        value = value.Trim();
        validationError = null;
    }
}

[SmartEnum<int>]
public sealed partial class ElementState {
    public static readonly ElementState Active = new(key: 0, visible: true, enabled: true);
    public static readonly ElementState Disabled = new(key: 1, visible: true, enabled: false);
    public static readonly ElementState Hidden = new(key: 2, visible: false, enabled: false);
    internal bool Visible { get; }
    internal bool Enabled { get; }
}

[SmartEnum<int>]
public sealed partial class HostCustody {
    public static readonly HostCustody Owned = new(key: 0, release: true);
    public static readonly HostCustody Borrowed = new(key: 1, release: false);
    internal bool Release { get; }
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record ElementSpec(
    ElementKey Key,
    ElementState State,
    Option<string> ToolTip,
    Option<StyleKey> Style,
    Seq<IBindingPlan> Bindings) {
    internal Fin<ElementSpec> Admit(Op key) => FaultRail.Admit(
        this,
        key,
        (!string.IsNullOrWhiteSpace(Key.Value), nameof(Key), "element identity is uninitialized"),
        (State is not null, nameof(State), "element state is absent"),
        (Bindings.Fold(true, static (valid, plan) => valid && plan is not null), nameof(Bindings), "binding plans cannot contain absent entries"));
}

public sealed record ElementRuntime(ThemeSeam Themes, IntentTable Intents);

// --- [SERVICES] -----------------------------------------------------------------------------
public abstract class UiLease : IDisposable {
    private int released;

    protected bool Released => Volatile.Read(ref released) != 0;

    protected abstract Fin<Unit> Free();

    public Fin<Unit> Release() => Interlocked.Exchange(ref released, 1) == 0 ? Free() : Fin.Succ(unit);

    public virtual void Dispose() => _ = Release().Match(
        Succ: static released => released,
        Fail: static fault => throw fault.ToException());
}

public sealed class ElementReceipt : UiLease {
    private readonly Seq<BindReceipt> bindings;
    private readonly Seq<ElementReceipt> children;
    private readonly Seq<IDisposable> resources;
    private readonly HostCustody custody;
    private readonly Op key;

    private ElementReceipt(
        Control host,
        Seq<BindReceipt> bindings,
        Seq<ElementReceipt> children,
        Seq<IDisposable> resources,
        HostCustody custody,
        Op key) {
        Host = host;
        this.bindings = bindings;
        this.children = children;
        this.resources = resources;
        this.custody = custody;
        this.key = key;
    }

    public Control Host { get; }

    internal static Fin<ElementReceipt> Create(
        Control host,
        ElementSpec spec,
        ElementRuntime runtime,
        Seq<ElementReceipt> children,
        Op key,
        HostCustody custody,
        Seq<IDisposable> resources) =>
        key.Catch(() => Apply(host, spec, runtime).Map(receipts => new ElementReceipt(host, receipts, children, resources, custody, key)))
            .MapFail(fault => fault.Also((
                children.Rev().Map(static child => (Func<Fin<Unit>>)child.Release)
                + resources.Rev().Map(resource => Step(resource.Dispose, key))
                + (custody.Release ? Seq(Step(host.Dispose, key)) : Seq<Func<Fin<Unit>>>())).Drained(key)));

    protected override Fin<Unit> Free() =>
        (bindings.Rev().Map(static binding => (Func<Fin<Unit>>)binding.Release)
        + children.Rev().Map(static child => (Func<Fin<Unit>>)child.Release)
        + resources.Rev().Map(resource => Step(resource.Dispose, key))
        + (custody.Release ? Seq(Step(Host.Dispose, key)) : Seq<Func<Fin<Unit>>>())).Drained(key);

    internal static Fin<Seq<ElementReceipt>> Gather(Seq<Element> elements, ElementRuntime runtime, Op key) =>
        elements.Fold(Fin.Succ(Seq<ElementReceipt>()), (rail, element) =>
            rail.Bind(held => element.Realize(runtime, key)
                .Map(held.Add)
                .MapFail(fault => fault.Also(held.Rev().Map(static receipt => (Func<Fin<Unit>>)receipt.Release).Drained(key)))));

    internal static Fin<ElementReceipt> Mint(
        Func<Control> mint,
        ElementSpec spec,
        ElementRuntime runtime,
        Seq<ElementReceipt> children,
        Op key) {
        Control? host = null;
        return key.Catch(() => {
                Control built = mint();
                host = built;
                return Fin.Succ(built);
            })
            .MapFail(fault => fault.Also((
                children.Rev().Map(static child => (Func<Fin<Unit>>)child.Release)
                + Optional(host).Map(control => Seq(Step(control.Dispose, key))).IfNone(Seq<Func<Fin<Unit>>>())).Drained(key)))
            .Bind(control => Create(control, spec, runtime, children, key, HostCustody.Owned, Seq<IDisposable>()));
    }

    private static Func<Fin<Unit>> Step(Action release, Op key) =>
        () => key.Catch(() => {
            release();
            return Fin.Succ(unit);
        });

    private static Fin<Seq<BindReceipt>> Apply(Control host, ElementSpec spec, ElementRuntime runtime) {
        host.ID = spec.Key.Value;
        host.Visible = spec.State.Visible;
        host.Enabled = spec.State.Enabled;
        _ = spec.ToolTip.Iter(value => host.ToolTip = value);
        _ = spec.Style.Iter(value => host.Style = value.Value);
        _ = runtime.Themes.Track(host);
        return spec.Bindings.Fold(Fin.Succ(Seq<BindReceipt>()), (rail, plan) =>
            rail.Bind(held => plan.Rig(host, Op.Of(spec.Key.Value))
                .Map(held.Add)
                .MapFail(fault => fault.Also(
                    held.Rev().Map(static receipt => (Func<Fin<Unit>>)receipt.Release).Drained(Op.Of(spec.Key.Value))))));
    }
}
```

## [04]-[CONTROL]

- Owner: `ControlSpec` carries every leaf-control modality, and each case owns only the payload its constructor consumes.
- Cases: text, choice, scalar, picker, content, and indicator cases share one admission path and one realization consumer.
- Entry: `ControlSpec.Admit` accumulates every referenced payload, collection element, enum, range, and selection defect through `FaultRail` before `ControlSpec.Mint`; both entries remain internal to `Element.Realize`.
- Growth: another host control is one exact-payload case and one total mint arm.
- Boundary: host enums and images remain at this Eto seam; paint colors enter through canvas `PerceptualColor` projection.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class TextAccess {
    public static readonly TextAccess Editable = new(key: 0, hostReadOnly: false);
    public static readonly TextAccess ReadOnly = new(key: 1, hostReadOnly: true);
    internal bool HostReadOnly { get; }
}

[SmartEnum<int>]
public sealed partial class TextWrap {
    public static readonly TextWrap Unwrapped = new(key: 0, enabled: false);
    public static readonly TextWrap Wrapped = new(key: 1, enabled: true);
    internal bool Enabled { get; }
}

[SmartEnum<int>]
public sealed partial class AlphaMode {
    public static readonly AlphaMode Opaque = new(key: 0, enabled: false);
    public static readonly AlphaMode Alpha = new(key: 1, enabled: true);
    internal bool Enabled { get; }
}

[SmartEnum<int>]
public sealed partial class InspectorMode {
    public static readonly InspectorMode Flat = new(key: 0, showCategories: false);
    public static readonly InspectorMode Categorized = new(key: 1, showCategories: true);
    internal bool ShowCategories { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ControlSpec {
    private ControlSpec() { }
    public sealed record Line(Option<string> Placeholder, TextAccess Access, Option<int> MaxLength, TextAlignment Alignment) : ControlSpec;
    public sealed record Area(TextAccess Access, TextWrap Wrap) : ControlSpec;
    public sealed record Rich(TextAccess Access) : ControlSpec;
    public sealed record Secret(TextAccess Access, Option<int> MaxLength) : ControlSpec;
    public sealed record Search(Option<string> Placeholder) : ControlSpec;
    public sealed record Drop(Seq<string> Items, Option<int> Selected) : ControlSpec;
    public sealed record Combo(Seq<string> Items, Option<int> Selected) : ControlSpec;
    public sealed record List(Seq<string> Items, Option<int> Selected) : ControlSpec;
    public sealed record Checks(Seq<string> Items, Orientation Axis) : ControlSpec;
    public sealed record Segments(Seq<string> Items, SegmentedSelectionMode Mode, Option<int> Selected) : ControlSpec;
    public sealed record Radios(Seq<string> Items, Orientation Axis, Option<int> Selected) : ControlSpec;
    public sealed record Stepper(double Minimum, double Maximum, double Increment, int Decimals) : ControlSpec;
    public sealed record Slider(int Minimum, int Maximum, Orientation Axis, Option<int> Tick) : ControlSpec;
    public sealed record Progress(int Minimum, int Maximum) : ControlSpec;
    public sealed record Busy : ControlSpec;
    public sealed record Spinner : ControlSpec;
    public sealed record Moment(DateTimePickerMode Mode, Option<DateTime> Minimum, Option<DateTime> Maximum) : ControlSpec;
    public sealed record Calendar(CalendarMode Mode, Option<DateTime> Minimum, Option<DateTime> Maximum) : ControlSpec;
    public sealed record Colour(AlphaMode Alpha) : ControlSpec;
    public sealed record Font : ControlSpec;
    public sealed record File(FileAction Action, Seq<FileFilter> Filters) : ControlSpec;
    public sealed record Label(string Value) : ControlSpec;
    public sealed record Picture(Image Value) : ControlSpec;
    public sealed record Separator(Orientation Axis, int Thickness) : ControlSpec;

    internal Fin<ControlSpec> Admit(Op key) => Switch(
        state: key,
        line: static (op, spec) => FaultRail.Admit(
            (ControlSpec)spec,
            op,
            Required(spec.Access, nameof(spec.Access)),
            Defined(spec.Alignment, nameof(spec.Alignment)),
            (spec.MaxLength.Map(value => value > 0).IfNone(true), nameof(spec.MaxLength), "maximum length must be positive")),
        area: static (op, spec) => FaultRail.Admit(
            (ControlSpec)spec,
            op,
            Required(spec.Access, nameof(spec.Access)),
            Required(spec.Wrap, nameof(spec.Wrap))),
        rich: static (op, spec) => FaultRail.Admit((ControlSpec)spec, op, Required(spec.Access, nameof(spec.Access))),
        secret: static (op, spec) => FaultRail.Admit(
            (ControlSpec)spec,
            op,
            Required(spec.Access, nameof(spec.Access)),
            (spec.MaxLength.Map(value => value > 0).IfNone(true), nameof(spec.MaxLength), "maximum length must be positive")),
        search: static (_, spec) => Fin.Succ<ControlSpec>(spec),
        drop: static (op, spec) => Selection(spec, spec.Items, spec.Selected, op),
        combo: static (op, spec) => Selection(spec, spec.Items, spec.Selected, op),
        list: static (op, spec) => Selection(spec, spec.Items, spec.Selected, op),
        checks: static (op, spec) => FaultRail.Admit(
            (ControlSpec)spec,
            op,
            Elements(spec.Items, nameof(spec.Items)),
            Defined(spec.Axis, nameof(spec.Axis))),
        segments: static (op, spec) => Selection(spec, spec.Items, spec.Selected, op, Defined(spec.Mode, nameof(spec.Mode))),
        radios: static (op, spec) => Selection(spec, spec.Items, spec.Selected, op, Defined(spec.Axis, nameof(spec.Axis))),
        stepper: static (op, spec) => FaultRail.Admit(
            (ControlSpec)spec,
            op,
            (double.IsFinite(spec.Minimum), nameof(spec.Minimum), "minimum must be finite"),
            (double.IsFinite(spec.Maximum), nameof(spec.Maximum), "maximum must be finite"),
            (double.IsFinite(spec.Increment), nameof(spec.Increment), "increment must be finite"),
            (spec.Maximum > spec.Minimum, nameof(spec.Maximum), "maximum must exceed minimum"),
            (spec.Increment > 0d && spec.Increment <= spec.Maximum - spec.Minimum, nameof(spec.Increment), "increment must fit the positive range"),
            (spec.Decimals >= 0, nameof(spec.Decimals), "decimal places cannot be negative")),
        slider: static (op, spec) => FaultRail.Admit(
            (ControlSpec)spec,
            op,
            Defined(spec.Axis, nameof(spec.Axis)),
            (spec.Maximum > spec.Minimum, nameof(spec.Maximum), "maximum must exceed minimum"),
            (spec.Tick.Map(value => value > 0 && value <= spec.Maximum - spec.Minimum).IfNone(true), nameof(spec.Tick), "tick frequency must fit the positive range")),
        progress: static (op, spec) => FaultRail.Admit((ControlSpec)spec, op, (spec.Maximum > spec.Minimum, nameof(spec.Maximum), "maximum must exceed minimum")),
        busy: static (_, spec) => Fin.Succ<ControlSpec>(spec),
        spinner: static (_, spec) => Fin.Succ<ControlSpec>(spec),
        moment: static (op, spec) => DateRange(spec, spec.Mode, spec.Minimum, spec.Maximum, op),
        calendar: static (op, spec) => DateRange(spec, spec.Mode, spec.Minimum, spec.Maximum, op),
        colour: static (op, spec) => FaultRail.Admit((ControlSpec)spec, op, Required(spec.Alpha, nameof(spec.Alpha))),
        font: static (_, spec) => Fin.Succ<ControlSpec>(spec),
        file: static (op, spec) => FaultRail.Admit(
            (ControlSpec)spec,
            op,
            Defined(spec.Action, nameof(spec.Action)),
            Elements(spec.Filters, nameof(spec.Filters))),
        label: static (op, spec) => FaultRail.Admit((ControlSpec)spec, op, (spec.Value is not null, nameof(spec.Value), "label payload is absent")),
        picture: static (op, spec) => FaultRail.Admit((ControlSpec)spec, op, (spec.Value is not null, nameof(spec.Value), "image payload is absent")),
        separator: static (op, spec) => FaultRail.Admit(
            (ControlSpec)spec,
            op,
            Defined(spec.Axis, nameof(spec.Axis)),
            (spec.Thickness > 0, nameof(spec.Thickness), "separator thickness must be positive")));

    internal Control Mint() => Switch(
        line: static spec => Configure(new TextBox { ReadOnly = spec.Access.HostReadOnly, TextAlignment = spec.Alignment }, spec.Placeholder, spec.MaxLength),
        area: static spec => new TextArea { ReadOnly = spec.Access.HostReadOnly, Wrap = spec.Wrap.Enabled },
        rich: static spec => new RichTextArea { ReadOnly = spec.Access.HostReadOnly },
        secret: static spec => Configure(new PasswordBox { ReadOnly = spec.Access.HostReadOnly }, None, spec.MaxLength),
        search: static spec => Configure(new SearchBox(), spec.Placeholder, None),
        drop: static spec => Select(new DropDown { DataStore = spec.Items.Cast<object>() }, spec.Selected),
        combo: static spec => Select(new ComboBox { DataStore = spec.Items.Cast<object>() }, spec.Selected),
        list: static spec => Select(new ListBox { DataStore = spec.Items.Cast<object>() }, spec.Selected),
        checks: static spec => new CheckBoxList { DataStore = spec.Items.Cast<object>(), Orientation = spec.Axis },
        segments: static spec => Segmented(spec),
        radios: static spec => Radio(spec),
        stepper: static spec => new NumericStepper { MinValue = spec.Minimum, MaxValue = spec.Maximum, Increment = spec.Increment, DecimalPlaces = spec.Decimals },
        slider: static spec => spec.Tick.Match(
            Some: tick => new global::Eto.Forms.Slider { MinValue = spec.Minimum, MaxValue = spec.Maximum, Orientation = spec.Axis, TickFrequency = tick, SnapToTick = true },
            None: () => new global::Eto.Forms.Slider { MinValue = spec.Minimum, MaxValue = spec.Maximum, Orientation = spec.Axis }),
        progress: static spec => new ProgressBar { MinValue = spec.Minimum, MaxValue = spec.Maximum },
        busy: static _ => new ProgressBar { Indeterminate = true },
        spinner: static _ => new global::Eto.Forms.Spinner(),
        moment: static spec => MomentRange(spec),
        calendar: static spec => CalendarRange(spec),
        colour: static spec => new ColorPicker { AllowAlpha = spec.Alpha.Enabled },
        font: static _ => new FontPicker(),
        file: static spec => FileControl(spec),
        label: static content => new global::Eto.Forms.Label { Text = content.Value },
        picture: static content => new ImageView { Image = content.Value },
        separator: static content => Divider(content));

    private static TText Configure<TText>(TText control, Option<string> placeholder, Option<int> maxLength) where TText : TextControl {
        _ = placeholder.Iter(value => control.PlaceholderText = value);
        _ = maxLength.Iter(value => control.MaxLength = value);
        return control;
    }

    private static TControl Select<TControl>(TControl control, Option<int> selected) where TControl : ListControl {
        _ = selected.Iter(value => control.SelectedIndex = value);
        return control;
    }

    private static Fin<ControlSpec> Selection(
        ControlSpec spec,
        Seq<string> items,
        Option<int> selected,
        Op key,
        params (bool Valid, string Field, string Reason)[] rules) =>
        FaultRail.Admit(
            spec,
            key,
            [
                Elements(items, nameof(Drop.Items)),
                (selected.Map(value => value >= 0 && value < items.Count).IfNone(true), nameof(Drop.Selected), "selection must address an item"),
                .. rules,
            ]);

    private static Fin<ControlSpec> DateRange<TMode>(
        ControlSpec spec,
        TMode mode,
        Option<DateTime> minimum,
        Option<DateTime> maximum,
        Op key) where TMode : struct, Enum =>
        FaultRail.Admit(
            spec,
            key,
            Defined(mode, nameof(Moment.Mode)),
            (minimum.Map(lower => maximum.Map(upper => lower <= upper).IfNone(true)).IfNone(true), nameof(Moment.Maximum), "maximum cannot precede minimum"));

    private static (bool Valid, string Field, string Reason) Required<TValue>(TValue? value, string field) where TValue : class =>
        (value is not null, field, "required payload is absent");

    private static (bool Valid, string Field, string Reason) Elements<TValue>(Seq<TValue> values, string field) where TValue : class =>
        (values.Fold(true, static (valid, value) => valid && value is not null), field, "collection cannot contain absent payloads");

    private static (bool Valid, string Field, string Reason) Defined<TValue>(TValue value, string field) where TValue : struct, Enum =>
        (Enum.IsDefined(value), field, "host enum value is undefined");

    private static SegmentedButton Segmented(Segments spec) {
        SegmentedButton control = new() { SelectionMode = spec.Mode };
        _ = spec.Items.Iter(value => control.Items.Add(new ButtonSegmentedItem { Text = value }));
        _ = spec.Selected.Iter(value => control.SelectedIndex = value);
        return control;
    }

    private static StackLayout Radio(Radios spec) {
        StackLayout layout = new() { Orientation = spec.Axis };
        _ = spec.Items.Fold((Index: 0, Controller: (RadioButton?)null), (held, text) => {
            RadioButton button = new(held.Controller) { Text = text, Checked = spec.Selected.Map(value => value == held.Index).IfNone(false) };
            layout.Items.Add(new StackLayoutItem(button));
            return (held.Index + 1, held.Controller ?? button);
        });
        return layout;
    }

    private static DateTimePicker MomentRange(Moment spec) {
        DateTimePicker picker = new() { Mode = spec.Mode };
        _ = spec.Minimum.Iter(value => picker.MinDate = value);
        _ = spec.Maximum.Iter(value => picker.MaxDate = value);
        return picker;
    }

    private static global::Eto.Forms.Calendar CalendarRange(Calendar spec) {
        global::Eto.Forms.Calendar picker = new() { Mode = spec.Mode };
        _ = spec.Minimum.Iter(value => picker.MinDate = value);
        _ = spec.Maximum.Iter(value => picker.MaxDate = value);
        return picker;
    }

    private static global::Eto.Forms.Panel Divider(Separator spec) => spec.Axis == Orientation.Horizontal
        ? new global::Eto.Forms.Panel { Height = spec.Thickness }
        : new global::Eto.Forms.Panel { Width = spec.Thickness };

    private static FilePicker FileControl(File spec) {
        FilePicker picker = new() { FileAction = spec.Action };
        _ = spec.Filters.Iter(picker.Filters.Add);
        return picker;
    }
}
```

## [05]-[TREE]

- Owner: `Element` is the recursive screen union; every case realizes through one total `Realize` dispatch and returns an owned subtree.
- Cases: typed controls, commands, containers, fixed tabs, closable document tabs, fixed-panel splitters, grid, layout, canvas, native, web, skinned inspector, collection editor, and custom host controls.
- Entry: `Element.Realize` accepts `ElementRuntime` and an operation key; callers enter through `UiThread.Run` before invoking it, and the dispatch prologue re-proves Eto thread identity — an off-thread call refuses with `UiFault.OffThread` before any host leaf mints.
- Law: `Inspector` owns both property-grid skins on one case — the native skin binds one subject, the themed skin binds one or many with a description pane — and a multi-subject native request is a typed rejection, never a silent first-subject pick.
- Receipt: every arm returns `ElementReceipt`; child receipts survive only inside the parent receipt, and the collection editor's extra content nests as an owned child.
- Growth: another element modality is one case and one dispatch arm.
- Boundary: `Custom` carries the foreign extension seam and still passes through `ElementSpec` admission; `EtoThread` answers only the Eto-level main-thread test and never replaces the host marshal seam.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record InspectorSkin {
    private InspectorSkin() { }
    public sealed record NativeCase : InspectorSkin;
    public sealed record ThemedCase(bool Description) : InspectorSkin;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Element {
    private Element(ElementSpec specification) => Specification = specification;
    private ElementSpec Specification { get; }
    public sealed record Widget(ElementSpec Spec, ControlSpec Definition) : Element(Spec);
    public sealed record Button(ElementSpec Spec, IntentKey Intent) : Element(Spec);
    public sealed record Link(ElementSpec Spec, IntentKey Intent) : Element(Spec);
    public sealed record BinaryToggle(ElementSpec Spec, string Caption, bool Value) : Element(Spec);
    public sealed record TriToggle(ElementSpec Spec, string Caption, Option<bool> Value) : Element(Spec);
    public sealed record Panel(ElementSpec Spec, Element Child) : Element(Spec);
    public sealed record Group(ElementSpec Spec, string Header, Element Child) : Element(Spec);
    public sealed record Expander(ElementSpec Spec, string Header, bool Expanded, Element Child) : Element(Spec);
    public sealed record Scroll(ElementSpec Spec, Element Child) : Element(Spec);
    public sealed record Tabs(ElementSpec Spec, Seq<(string Title, Element Body)> Pages, int Selected) : Element(Spec);
    public sealed record Documents(ElementSpec Spec, Seq<(string Title, Element Body, bool Closable)> Pages, int Selected, bool Reorderable) : Element(Spec);
    public sealed record Split(ElementSpec Spec, Orientation Axis, Element First, Element Second, Option<double> Relative, SplitterFixedPanel FixedPanel) : Element(Spec);
    public sealed record Grid(ElementSpec Spec, IGridPlan Plan) : Element(Spec);
    public sealed record Layout(ElementSpec Spec, LayoutPlan Plan) : Element(Spec);
    public sealed record Painted(ElementSpec Spec, SurfaceSpec Surface) : Element(Spec);
    public sealed record Embedded(ElementSpec Spec, NativeMount Mount) : Element(Spec);
    public sealed record Web(ElementSpec Spec, Uri Address) : Element(Spec);
    public sealed record Inspector(ElementSpec Spec, Seq<object> Subjects, InspectorMode Mode, InspectorSkin Skin) : Element(Spec);
    public sealed record Collection(ElementSpec Spec, Seq<object> Items, Type ItemType, Option<Element> Extra) : Element(Spec);
    public sealed record Custom(ElementSpec Spec, Func<Fin<Control>> Mint) : Element(Spec);

    public Fin<ElementReceipt> Realize(ElementRuntime runtime, Op? key = null) {
        Op op = key.OrDefault();
        if (!EtoThread.IsMainThread) return Fin.Fail<ElementReceipt>(error: new UiFault.OffThread(Key: op));
        return FaultRail.Admit(
            (Specification, Runtime: runtime),
            op,
            (Specification is not null, nameof(Specification), "element specification is absent"),
            (runtime is not null, nameof(runtime), "element runtime is absent"),
            (runtime?.Themes is not null, nameof(ElementRuntime.Themes), "theme seam is absent"),
            (runtime?.Intents is not null, nameof(ElementRuntime.Intents), "intent table is absent"))
        .Bind(admitted => admitted.Specification.Admit(op).Bind(_ => Switch(
            state: (admitted.Runtime, Key: op),
            widget: static (held, node) => node.Definition.Admit(held.Key).Bind(definition => Leaf(node.Spec, definition.Mint, held)),
            button: static (held, node) => Command(node.Spec, node.Intent, held, static command => new global::Eto.Forms.Button { Command = command }),
            link: static (held, node) => Command(node.Spec, node.Intent, held, static command => new LinkButton { Command = command }),
            binaryToggle: static (held, node) => Leaf(node.Spec, () => new CheckBox { Text = node.Caption, Checked = node.Value }, held),
            triToggle: static (held, node) => Leaf(node.Spec, () => new CheckBox { Text = node.Caption, Checked = node.Value.Match<bool?>(Some: static value => value, None: static () => null), ThreeState = true }, held),
            panel: static (held, node) => One(node.Spec, node.Child, held, static child => new global::Eto.Forms.Panel { Content = child }),
            group: static (held, node) => One(node.Spec, node.Child, held, child => new GroupBox { Text = node.Header, Content = child }),
            expander: static (held, node) => One(node.Spec, node.Child, held, child => new global::Eto.Forms.Expander { Header = node.Header, Expanded = node.Expanded, Content = child }),
            scroll: static (held, node) => One(node.Spec, node.Child, held, static child => new Scrollable { Content = child }),
            tabs: static (held, node) => Pages(node, held),
            documents: static (held, node) => Docks(node, held),
            split: static (held, node) => Pair(node, held),
            grid: static (held, node) => node.Plan.Realize(held.Key).Bind(control => Leaf(node.Spec, () => control, held)),
            layout: static (held, node) => node.Plan.Realize(node.Spec, held.Runtime, held.Key),
            painted: static (held, node) => node.Surface.Mount(held.Key).Bind(surface =>
                ElementReceipt.Create(
                    surface.Host,
                    node.Spec,
                    held.Runtime,
                    Seq<ElementReceipt>(),
                    held.Key,
                    HostCustody.Borrowed,
                    Seq<IDisposable>(surface))),
            embedded: static (held, node) => node.Mount.Realize(held.Key).Bind(control => Leaf(node.Spec, () => control, held)),
            web: static (held, node) => Leaf(node.Spec, () => new WebView { Url = node.Address }, held),
            inspector: static (held, node) => node.Skin.Switch(
                (Held: held, Node: node),
                nativeCase: static seat => seat.Node.Subjects is [var only]
                    ? Leaf(seat.Node.Spec, () => new PropertyGrid { SelectedObject = only, ShowCategories = seat.Node.Mode.ShowCategories }, seat.Held)
                    : Fin.Fail<ElementReceipt>(new UiFault.Rejected(
                        Key: seat.Held.Key,
                        Field: nameof(Inspector.Subjects),
                        Reason: "the native inspector binds one subject")),
                themedCase: static (seat, skin) => Leaf(seat.Node.Spec, () => Themed(seat.Node, skin), seat.Held)),
            collection: static (held, node) => node.Extra.Match(
                Some: extra => extra.Realize(held.Runtime, held.Key).Bind(child =>
                    ElementReceipt.Mint(() => Editor(node, Some(child.Host)), node.Spec, held.Runtime, Seq(child), held.Key)),
                None: () => ElementReceipt.Mint(() => Editor(node, None), node.Spec, held.Runtime, Seq<ElementReceipt>(), held.Key)),
            custom: static (held, node) => Optional(node.Mint)
                .ToFin(new UiFault.Rejected(Key: held.Key, Field: nameof(node.Mint), Reason: "custom control mint is absent"))
                .Bind(mint => mint().Bind(control => Leaf(node.Spec, () => control, held))))));
    }

    private static Control Themed(Inspector node, InspectorSkin.ThemedCase skin) =>
        node.Subjects is [var single]
            ? new ThemedPropertyGrid { SelectedObject = single, ShowCategories = node.Mode.ShowCategories, ShowDescription = skin.Description }
            : new ThemedPropertyGrid { SelectedObjects = node.Subjects, ShowCategories = node.Mode.ShowCategories, ShowDescription = skin.Description };

    private static Control Editor(Collection node, Option<Control> extra) {
        ThemedCollectionEditor editor = new() { DataStore = node.Items, ElementType = node.ItemType };
        _ = extra.Iter(control => editor.ExtraContent = control);
        return editor;
    }

    private static Fin<ElementReceipt> Leaf(
        ElementSpec spec,
        Func<Control> mint,
        (ElementRuntime Runtime, Op Key) held) =>
        ElementReceipt.Mint(mint, spec, held.Runtime, Seq<ElementReceipt>(), held.Key);

    private static Fin<ElementReceipt> One(
        ElementSpec spec,
        Element child,
        (ElementRuntime Runtime, Op Key) held,
        Func<Control, Control> mint) =>
        child.Realize(held.Runtime, held.Key).Bind(receipt =>
            ElementReceipt.Mint(() => mint(receipt.Host), spec, held.Runtime, Seq(receipt), held.Key));

    private static Fin<ElementReceipt> Command(
        ElementSpec spec,
        IntentKey intent,
        (ElementRuntime Runtime, Op Key) held,
        Func<Command, Control> mint) =>
        held.Runtime.Intents.Command(intent).Bind(command =>
            Leaf(spec, () => mint(command), held));

    private static Fin<ElementReceipt> Pages(Tabs node, (ElementRuntime Runtime, Op Key) held) =>
        node.Pages.IsEmpty
            ? Fin.Fail<ElementReceipt>(new UiFault.Rejected(Key: held.Key, Field: nameof(node.Pages), Reason: "tabs require a page"))
            : from selected in Selected(node.Selected, node.Pages.Count, nameof(node.Selected), held.Key)
              from children in ElementReceipt.Gather(node.Pages.Map(static page => page.Body), held.Runtime, held.Key)
              from receipt in ElementReceipt.Mint(() => {
                    TabControl tabs = new();
                    _ = node.Pages.Zip(children).Iter(pair => tabs.Pages.Add(new TabPage { Text = pair.First.Title, Content = pair.Second.Host }));
                    tabs.SelectedIndex = selected;
                    return tabs;
                }, node.Spec, held.Runtime, children, held.Key)
              select receipt;

    private static Fin<ElementReceipt> Docks(Documents node, (ElementRuntime Runtime, Op Key) held) =>
        node.Pages.IsEmpty
            ? Fin.Fail<ElementReceipt>(new UiFault.Rejected(Key: held.Key, Field: nameof(node.Pages), Reason: "a document host requires a page"))
            : from selected in Selected(node.Selected, node.Pages.Count, nameof(node.Selected), held.Key)
              from children in ElementReceipt.Gather(node.Pages.Map(static page => page.Body), held.Runtime, held.Key)
              from receipt in ElementReceipt.Mint(() => {
                    DocumentControl docks = new() { AllowReordering = node.Reorderable };
                    _ = node.Pages.Zip(children).Iter(pair => docks.Pages.Add(new DocumentPage { Content = pair.Second.Host, Text = pair.First.Title, Closable = pair.First.Closable }));
                    docks.SelectedIndex = selected;
                    return docks;
                }, node.Spec, held.Runtime, children, held.Key)
              select receipt;

    private static Fin<int> Selected(int selected, int count, string field, Op key) =>
        selected >= 0 && selected < count
            ? Fin.Succ(selected)
            : Fin.Fail<int>(new UiFault.Rejected(Key: key, Field: field, Reason: $"selection {selected} is outside {count} pages"));

    private static Fin<ElementReceipt> Pair(Split node, (ElementRuntime Runtime, Op Key) held) =>
        ElementReceipt.Gather(Seq(node.First, node.Second), held.Runtime, held.Key).Bind(children =>
            ElementReceipt.Mint(() => {
                Splitter split = new() { Orientation = node.Axis, Panel1 = children[0].Host, Panel2 = children[1].Host, FixedPanel = node.FixedPanel };
                _ = node.Relative.Iter(value => split.RelativePosition = value);
                return split;
            }, node.Spec, held.Runtime, children, held.Key));
}
```

## [06]-[GRID]

- Owner: `GridPlan<TRow>` retains typed rows through flat or tree shape, lowers to host `object[]` only at `GridItem` construction, and routes callback faults through its injected error sink.
- Cases: `GridRows<TRow>` distinguishes flat rows from a tree carrying child projection, expansion policy, node budget, and identity equality; `CellSpec` covers host cell modalities through one guarded callback sink.
- Entry: `IGridPlan.Realize` rejects default expansion limits, absent equality, repeated identities, and budget exhaustion before returning a host tree.
- Egress: grid realization returns one unadmitted host control; `ElementReceipt.Create` applies the caller's `ElementSpec` exactly once, and `GridPlan.Failures` retains callback plus reporter faults.
- Growth: another cell is one `CellSpec` case, another topology is one `GridRows<TRow>` case, and another host column capability is one `GridColumnFeature` row folded through `Features`.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CellSpec {
    private CellSpec() { }
    public sealed record Text(TextAlignment Alignment) : CellSpec;
    public sealed record Check : CellSpec;
    public sealed record Choice(Seq<object> Items) : CellSpec;
    public sealed record Image : CellSpec;
    public sealed record ImageText(int TextColumn) : CellSpec;
    public sealed record Progress : CellSpec;
    public sealed record Custom(Func<CellEventArgs, Control> Create, Option<Action<CellEventArgs, Control>> Configure) : CellSpec;
    public sealed record Draw(Action<DrawableCellPaintEventArgs> Paint) : CellSpec;

    internal Cell Mint(int column, Action<Error> retain, Op key) => Switch(
        state: (Column: column, Retain: retain, Key: key),
        text: static (held, cell) => new TextBoxCell(held.Column) { TextAlignment = cell.Alignment },
        check: static (held, _) => new CheckBoxCell(held.Column),
        choice: static (held, cell) => new ComboBoxCell(held.Column) { DataStore = cell.Items },
        image: static (held, _) => new ImageViewCell(held.Column),
        imageText: static (held, cell) => new ImageTextCell(held.Column, cell.TextColumn),
        progress: static (held, _) => new ProgressCell(held.Column),
        custom: static (held, cell) => Configure(cell, held.Retain, held.Key),
        draw: static (held, cell) => Drawn(cell, held.Retain, held.Key));

    private static CustomCell Configure(Custom cell, Action<Error> retain, Op key) {
        CustomCell host = new() {
            CreateCell = args => key.Catch(() => Fin.Succ(cell.Create(args))).Match(
                Succ: static control => control,
                Fail: fault => {
                    retain(fault);
                    return new Panel();
                }),
        };
        _ = cell.Configure.Iter(configure => host.ConfigureCell = (args, control) =>
            _ = key.Catch(() => Fin.Succ(Op.Side(() => configure(args, control)))).Match(
                Succ: static configured => configured,
                Fail: fault => Op.Side(() => retain(fault))));
        return host;
    }

    private static DrawableCell Drawn(Draw cell, Action<Error> retain, Op key) {
        DrawableCell host = new();
        host.Paint += (_, args) =>
            _ = key.Catch(() => Fin.Succ(Op.Side(() => cell.Paint(args)))).Match(
                Succ: static painted => painted,
                Fail: fault => Op.Side(() => retain(fault)));
        return host;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GridRows<TRow> {
    private GridRows() { }
    public sealed record Flat(Seq<TRow> Rows) : GridRows<TRow>;
    public sealed record Tree(
        Seq<TRow> Roots,
        Func<TRow, Seq<TRow>> Children,
        Func<TRow, bool> Expanded,
        GridExpansionLimit Limit,
        System.Collections.Generic.IEqualityComparer<TRow> Identity) : GridRows<TRow>;
}

[ValueObject<int>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct GridExpansionLimit {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value <= 0 ? new ValidationError(message: "Grid expansion limit must be positive.") : null;
}

[SmartEnum<int>]
public sealed partial class GridColumnFeature {
    public static readonly GridColumnFeature Editable = new(key: 0);
    public static readonly GridColumnFeature Sortable = new(key: 1);
    public static readonly GridColumnFeature Resizable = new(key: 2);
    public static readonly GridColumnFeature AutoSized = new(key: 3);
    public static readonly GridColumnFeature Hidden = new(key: 4);
}

[SmartEnum<int>]
public sealed partial class GridChrome {
    public static readonly GridChrome Headered = new(key: 0, showHeader: true);
    public static readonly GridChrome Bare = new(key: 1, showHeader: false);
    internal bool ShowHeader { get; }
}

[SmartEnum<int>]
public sealed partial class GridSelection {
    public static readonly GridSelection SingleRequired = new(key: 0, multiple: false, allowEmpty: false);
    public static readonly GridSelection SingleOptional = new(key: 1, multiple: false, allowEmpty: true);
    public static readonly GridSelection MultipleRequired = new(key: 2, multiple: true, allowEmpty: false);
    public static readonly GridSelection MultipleOptional = new(key: 3, multiple: true, allowEmpty: true);
    internal bool Multiple { get; }
    internal bool AllowEmpty { get; }
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record GridColumnPlan<TRow>(
    string Header,
    Func<TRow, object?> Read,
    CellSpec Cell,
    FrozenSet<GridColumnFeature> Features,
    Option<int> Width);

public interface IGridPlan {
    Fin<Control> Realize(Op key);
}

public sealed record GridPlan<TRow>(
    Seq<GridColumnPlan<TRow>> Columns,
    GridRows<TRow> Rows,
    GridChrome Chrome,
    GridLines Lines,
    BorderType Border,
    GridSelection Selection,
    Action<Error> Report) : IGridPlan {
    private readonly Atom<Seq<Error>> failures = Atom(Seq<Error>());

    public Seq<Error> Failures => failures.Value;

    public Fin<Control> Realize(Op key) => key.Catch(() => Rows.Switch(
        flat: rows => Fin.Succ<Grid>(new GridView { DataStore = rows.Rows.Map(row => (object)Item(row)) }),
        tree: rows => Tree(rows, key).Map(items => (Grid)new TreeGridView { DataStore = items })))
        .Bind(grid => key.Catch(() => {
            grid.ShowHeader = Chrome.ShowHeader;
            grid.GridLines = Lines;
            grid.Border = Border;
            grid.AllowMultipleSelection = Selection.Multiple;
            grid.AllowEmptySelection = Selection.AllowEmpty;
            _ = Columns.Map((column, index) => {
                GridColumn hosted = new() {
                    HeaderText = column.Header,
                    DataCell = column.Cell.Mint(index, fault => Retain(fault, key), key),
                    Editable = column.Features.Contains(GridColumnFeature.Editable),
                    Sortable = column.Features.Contains(GridColumnFeature.Sortable),
                    Resizable = column.Features.Contains(GridColumnFeature.Resizable),
                    AutoSize = column.Features.Contains(GridColumnFeature.AutoSized),
                    Visible = !column.Features.Contains(GridColumnFeature.Hidden),
                };
                _ = column.Width.Iter(value => hosted.Width = value);
                return hosted;
            }).Iter(grid.Columns.Add);
            return Fin.Succ<Control>(grid);
        }));

    private Unit Retain(Error fault, Op key) =>
        ignore(failures.Swap(held => held.Add(fault.Reported(Report, key))));

    private GridItem Item(TRow row) => new([.. Columns.Map(column => column.Read(row))]) { Tag = row };

    private Fin<TreeGridItemCollection> Tree(GridRows<TRow>.Tree tree, Op key) =>
        tree.Limit.Value <= 0
            ? Fin.Fail<TreeGridItemCollection>(new UiFault.Rejected(
                Key: key, Field: nameof(tree.Limit), Reason: "tree expansion requires an admitted positive limit"))
            : Optional(tree.Identity)
                .ToFin(new UiFault.Rejected(Key: key, Field: nameof(tree.Identity), Reason: "tree expansion requires identity equality"))
                .Bind(identity => tree.Roots.Fold(
                    Fin.Succ((
                        Items: Seq<ITreeGridItem>(),
                        Seen: System.Collections.Immutable.ImmutableHashSet.Create(identity),
                        Remaining: tree.Limit.Value)),
                    (rail, root) => rail.Bind(state => Branch(root, tree, state.Remaining, state.Seen, key)
                        .Map(branch => (
                            Items: state.Items.Add(branch.Item),
                            branch.Seen,
                            branch.Remaining))))
                .Map(state => new TreeGridItemCollection(state.Items)));

    private Fin<(ITreeGridItem Item, System.Collections.Immutable.ImmutableHashSet<TRow> Seen, int Remaining)> Branch(
        TRow row,
        GridRows<TRow>.Tree tree,
        int remaining,
        System.Collections.Immutable.ImmutableHashSet<TRow> seen,
        Op key) {
        if (remaining <= 0)
            return Fin.Fail<(ITreeGridItem, System.Collections.Immutable.ImmutableHashSet<TRow>, int)>(
                new UiFault.Rejected(Key: key, Field: nameof(tree.Limit), Reason: "tree expansion exhausted its node limit"));
        if (seen.Contains(row))
            return Fin.Fail<(ITreeGridItem, System.Collections.Immutable.ImmutableHashSet<TRow>, int)>(
                new UiFault.Rejected(Key: key, Field: nameof(tree.Identity), Reason: "tree expansion revisited a node identity"));

        System.Collections.Immutable.ImmutableHashSet<TRow> admitted = seen.Add(row);
        return key.Catch(() => Fin.Succ(tree.Children(row)))
            .Bind(children => children.Fold(
                Fin.Succ((Items: Seq<ITreeGridItem>(), Seen: admitted, Remaining: remaining - 1)),
                (rail, child) => rail.Bind(state => Branch(child, tree, state.Remaining, state.Seen, key)
                    .Map(branch => (
                        Items: state.Items.Add(branch.Item),
                        branch.Seen,
                        branch.Remaining)))))
            .Bind(state => key.Catch(() => Fin.Succ((
                Item: (ITreeGridItem)new TreeGridItem(state.Items, [.. Columns.Map(column => column.Read(row))]) {
                    Expanded = tree.Expanded(row),
                    Tag = row,
                },
                state.Seen,
                state.Remaining))));
    }

}
```

## [07]-[LAYOUT]

- Owner: `LayoutPlan` is one recursive placement union over flow, stack, table, and absolute host layouts.
- Entry: `LayoutPlan.Realize` realizes children through `ElementReceipt.Gather`, constructs the selected host layout, then applies the caller's `ElementSpec`.
- Receipt: child receipts stay nested under the layout receipt and release on any parent failure.
- Growth: another host placement strategy is one case and one total dispatch arm.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class Stretch {
    public static readonly Stretch Fixed = new(key: 0, hostExpand: false);
    public static readonly Stretch Fill = new(key: 1, hostExpand: true);
    internal bool HostExpand { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LayoutPlan {
    private LayoutPlan() { }
    public sealed record Flow(Padding Padding, Size Spacing, Seq<Element> Items) : LayoutPlan;
    public sealed record Stack(Orientation Axis, int Spacing, Seq<(Element Item, Stretch Stretch)> Items) : LayoutPlan;
    public sealed record Table(Padding Padding, Size Spacing, Seq<Seq<(Element Item, Stretch Stretch)>> Rows) : LayoutPlan;
    public sealed record Absolute(Seq<(Element Item, Point At)> Items) : LayoutPlan;

    public Fin<ElementReceipt> Realize(ElementSpec spec, ElementRuntime runtime, Op key) => Switch(
        state: (Spec: spec, Runtime: runtime, Key: key),
        flow: static (held, plan) => ElementReceipt.Gather(plan.Items, held.Runtime, held.Key).Bind(children => ElementReceipt.Mint(() => {
            DynamicLayout layout = new() { Padding = plan.Padding, Spacing = plan.Spacing };
            _ = children.Iter(child => layout.Add(child.Host));
            return layout;
        }, held.Spec, held.Runtime, children, held.Key)),
        stack: static (held, plan) => ElementReceipt.Gather(plan.Items.Map(static item => item.Item), held.Runtime, held.Key).Bind(children => ElementReceipt.Mint(() => {
            StackLayout layout = new() { Orientation = plan.Axis, Spacing = plan.Spacing };
            _ = plan.Items.Zip(children).Iter(pair => layout.Items.Add(new StackLayoutItem(pair.Second.Host, pair.First.Stretch.HostExpand)));
            return layout;
        }, held.Spec, held.Runtime, children, held.Key)),
        table: static (held, plan) => ElementReceipt.Gather(plan.Rows.Bind(static row => row.Map(static cell => cell.Item)), held.Runtime, held.Key).Bind(children => ElementReceipt.Mint(() => {
            int cursor = 0;
            TableLayout layout = new() { Padding = plan.Padding, Spacing = plan.Spacing };
            _ = plan.Rows.Iter(row => layout.Rows.Add(new TableRow([.. row.Map(cell => new TableCell(children[cursor++].Host, cell.Stretch.HostExpand))])));
            return layout;
        }, held.Spec, held.Runtime, children, held.Key)),
        absolute: static (held, plan) => ElementReceipt.Gather(plan.Items.Map(static item => item.Item), held.Runtime, held.Key).Bind(children => ElementReceipt.Mint(() => {
            PixelLayout layout = new();
            _ = plan.Items.Zip(children).Iter(pair => layout.Add(pair.Second.Host, pair.First.At.X, pair.First.At.Y));
            return layout;
        }, held.Spec, held.Runtime, children, held.Key)));
}
```

## [08]-[NOTICE]

- Owner: `ThemedNotice` closes the themed modal message box — text, alignment, badge, and result-typed choices — and `NoticeMount` is its `UiLease` dialog capsule with typed reply and idempotent release rails.
- Entry: `ThemedNotice.Mint` validates the choice set and mints the dialog; presentation rides the HostUi window owner's untyped modal entry, and `NoticeMount.Reply` reads the boxed choice after dismissal.
- Law: a notice requires at least one choice and at most one default; a reply is `Option<int>` — dismissal without a choice is `None`, never a sentinel.
- Boundary: the dialog carries its result on the instance, so the mount is the one reply seam; spinner glyph policy stays with the themed handler registration the platform page owns.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
public sealed record NoticeChoice(string Caption, int Result, bool Default, bool Abort);

public sealed record ThemedNotice(string Text, TextAlignment Alignment, Option<Image> Badge, Seq<NoticeChoice> Choices) {
    public Fin<NoticeMount> Mint(Op? key = null) {
        Op op = key.OrDefault();
        ThemedNotice spec = this;
        return from _ in FaultRail.Admit(
                   unit,
                   op,
                   (Text is not null, nameof(Text), "notice text is absent"),
                   (Enum.IsDefined(Alignment), nameof(Alignment), "text alignment is undefined"),
                   (!Choices.IsEmpty, nameof(Choices), "a notice requires a choice"),
                   (Choices.Fold(true, static (valid, choice) => valid && choice is not null), nameof(Choices), "choices cannot contain absent entries"),
                   (Choices.Fold(0, static (count, choice) => choice is { Default: true } ? count + 1 : count) <= 1, nameof(Choices), "a notice permits at most one default"),
                   (Choices.Fold(true, static (valid, choice) => valid && choice is not null && choice.Caption is not null), nameof(Choices), "choice captions cannot be absent"))
               from host in op.Catch(() => {
                   ThemedMessageBox notice = new() { Text = spec.Text, TextAlignment = spec.Alignment };
                   _ = spec.Badge.Iter(image => notice.Image = image);
                   _ = spec.Choices.Iter(choice => notice.AddButton(
                       text: choice.Caption,
                       result: choice.Result,
                       isDefault: choice.Default,
                       isAbort: choice.Abort));
                   return Fin.Succ(value: new NoticeMount(host: notice, key: op));
               })
               select host;
    }
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed class NoticeMount(ThemedMessageBox host, Op key) : UiLease {
    public ThemedMessageBox Host { get; } = host;

    public Dialog Dialog => Host;

    public Option<int> Reply => Host.Result is int chosen ? Some(chosen) : None;

    protected override Fin<Unit> Free() =>
        key.Catch(() => Fin.Succ(Op.Side(Host.Dispose)));
}
```

## [09]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
