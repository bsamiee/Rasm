# [RASM_RHINO_ETO_PLATFORM]

`HostPlatform` binds Rhino's ambient Eto backend, and the same owner admits handler discovery, native-control crossings, and theme transitions without reinitializing the host's `Platform`. Native attachments carry deterministic release, deferred native controls mint through `OnCreateNativeControl`, and theme cells derive from one parameterized generator before any control resolves a role.

## [01]-[INDEX]

- [02]-[PLATFORM]: `Backend` and `HostPlatform` own ambient identity, handler discovery, platform context, and worker scope.
- [03]-[NATIVE]: `NativeMount` and `NativeAttachment` own both native-control crossings and their lifetimes.
- [04]-[THEME]: `StyleKey`, `ThemeVariant`, `PaletteRole`, `ThemeCatalog`, and `ThemeSeam` own generated theme state and tracked rebroadcast.

## [02]-[PLATFORM]

- Owner: `HostPlatform` resolves one `HandlerDemand<THandler>` against `Platform.Instance` and exposes the host platform context without admitting `Platform.Initialize`.
- Cases: `HandlerDemand<THandler>` carries per-instance creation, shared creation, or registered lookup as distinct evidence shapes.
- Entry: `HostPlatform.Current`, `Resolve`, `Under`, and `Worker` share one captured ambient-platform funnel; handler discovery, context, and worker lifetime stay on `Fin`.
- Receipt: `BackendFact` detaches backend identity, desktop posture, and supported feature flags from the ambient provider before policy code consumes them.
- Growth: a handler modality is one `HandlerDemand<THandler>` case; a backend identity is one `Backend` row.
- Boundary: `UiThread` owns `Application` affinity; this owner scopes `Platform` only.
- Exemption: `ThreadStart`'s `using` scope is the worker-boundary statement seam that guarantees platform-context release.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Eto;
using Eto.Forms;
using Rasm.Csp;
using Rasm.Domain;

namespace Rasm.Rhino.Eto;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class Backend {
    public static readonly Backend Mac = new(key: 0, matches: static platform => platform.IsMac);
    public static readonly Backend WinForms = new(key: 1, matches: static platform => platform.IsWinForms);
    public static readonly Backend Wpf = new(key: 2, matches: static platform => platform.IsWpf);
    public static readonly Backend Gtk = new(key: 3, matches: static platform => platform.IsGtk);
    public static readonly Backend Other = new(key: 4, matches: static platform =>
        !(platform.IsMac || platform.IsWinForms || platform.IsWpf || platform.IsGtk));

    [UseDelegateFromConstructor]
    internal partial bool Matches(Platform platform);

    internal static Backend Detect(Platform platform) =>
        toSeq(Items).Find(row => row.Matches(platform)).IfNone(Other);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HandlerDemand<THandler> where THandler : class {
    private HandlerDemand() { }
    public sealed record Create : HandlerDemand<THandler>;
    public sealed record Shared : HandlerDemand<THandler>;
    public sealed record Registered : HandlerDemand<THandler>;
}

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct BackendFact(string Identity, Backend Kind, bool Desktop, PlatformFeatures Features);

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class HostPlatform {
    public static Fin<BackendFact> Current(Op? key = null) =>
        Within(key.OrDefault(), static platform => Fin.Succ(new BackendFact(
            Identity: platform.ID,
            Kind: Backend.Detect(platform),
            Desktop: platform.IsDesktop,
            Features: platform.SupportedFeatures)));

    public static Fin<Option<THandler>> Resolve<THandler>(HandlerDemand<THandler> demand, Op? key = null) where THandler : class {
        Op op = key.OrDefault();
        return Optional(demand)
            .ToFin(new UiFault.Rejected(Key: op, Field: nameof(demand), Reason: "handler demand is absent"))
            .Bind(admitted => Within(op, platform => Fin.Succ(admitted.Switch(
                state: platform,
                create: static (host, _) => host.Supports<THandler>() ? Optional(host.Create<THandler>()) : None,
                shared: static (host, _) => host.Supports<THandler>() ? Optional(host.CreateShared<THandler>()) : None,
                registered: static (host, _) => Optional(host.Find<THandler>())))));
    }

    public static Fin<TResult> Under<TResult>(Func<Fin<TResult>> body, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(body)
            .ToFin(new UiFault.Rejected(Key: op, Field: nameof(body), Reason: "platform body is absent"))
            .Bind(admitted => Within(op, platform => platform.Invoke(admitted)));
    }

    public static Fin<TResult> Worker<TResult>(Func<Fin<TResult>> body, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(body)
            .ToFin(new UiFault.Rejected(Key: op, Field: nameof(body), Reason: "worker body is absent"))
            .Bind(admitted => Within(op, platform => {
                using IDisposable scope = platform.ThreadStart();
                return admitted();
            }));
    }

    private static Fin<TResult> Within<TResult>(Op op, Func<Platform, Fin<TResult>> body) =>
        op.Catch(() => body(Platform.Instance));
}
```

## [03]-[NATIVE]

- Owner: `NativeMount` folds eager and deferred native sources into one `Control`; `NativeAttachment` owns the inverse attach/detach capsule.
- Cases: `NativeMount.Eager` carries an existing native object; `NativeMount.Deferred` carries the supplier and typed error sink invoked by `OnCreateNativeControl`.
- Entry: `NativeMount.Realize` and `NativeAttachment.Attach` are the two direction-specific admissions.
- Receipt: deferred supply and reporter failures stay retained on the callback owner; `NativeAttachment.Release` returns detach failure on the operation rail.
- Growth: native lifecycle evidence extends `NativeAttachment`; another supply timing is one `NativeMount` case.
- Boundary: native focus, keyboard, and event routing stay on the mounted controls; this seam owns custody only.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record NativeMount {
    private NativeMount() { }
    public sealed record Eager(object Native) : NativeMount;
    public sealed record Deferred(Func<object> Supply, Action<Error> Report) : NativeMount;

    public Fin<Control> Realize(Op? key = null) =>
        key.OrDefault().Catch(() => Fin.Succ(Switch(
            state: key.OrDefault(),
            eager: static (_, source) => (Control)new NativeControlHost(nativeControl: source.Native),
            deferred: static (op, source) => new DeferredHost(source.Supply, source.Report, op))));

    private sealed class DeferredHost(Func<object> supply, Action<Error> report, Op key) : NativeControlHost {
        private readonly Atom<Seq<Error>> failures = Atom(Seq<Error>());

        protected override void OnCreateNativeControl(CreateNativeControlArgs e) =>
            _ = key.Catch(() => {
                    e.NativeControl = supply();
                    return Fin.Succ(unit);
                })
                .BindFail(Retain);

        private Fin<Unit> Retain(Error fault) => key.Catch(() => {
            _ = failures.Swap(held => held.Add(fault.Reported(report, key)));
            return Fin.Succ(unit);
        });
    }
}

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class NativeAttachment : UiLease {
    private readonly Op key;

    private NativeAttachment(Control subject, Op key) {
        Subject = subject;
        this.key = key;
    }

    public Control Subject { get; }

    public static Fin<NativeAttachment> Attach(Control subject, Op? key = null) =>
        Optional(subject)
            .ToFin(new UiFault.Rejected(Key: key.OrDefault(), Field: nameof(subject), Reason: "native subject is absent"))
            .Bind(admitted => key.OrDefault().Catch(() => {
                admitted.AttachNative();
                return Fin.Succ(new NativeAttachment(admitted, key.OrDefault()));
            }));

    protected override Fin<Unit> Free() => key.Catch(() => Fin.Succ(Op.Side(Subject.DetachNative)));
}
```

## [04]-[THEME]

- Owner: `ThemeProgram` generates the complete role-by-variant grid; `ThemeCatalog` retains the contrast rules and publishes one immutable `ThemeSnapshot`; `ThemeSeam` registers styles and rebroadcasts the accepted snapshot to tracked controls.
- Cases: `ThemeVariant` carries light, dark, and high-contrast axes; `PaletteRole` carries semantic paint roles without embedding colors; `ThemeShift` closes the transition family — `Generated` selects a variant from the frozen grid, `Hosted` merges live host-detached `PerceptualColor` cells over that variant's row, so the generator palette and the host theme meet in one owner.
- Entry: `ThemeCatalog.Freeze` admits the initial variant, every contrast endpoint, and every finite ratio floor; `ThemeSeam.Register` and `ThemeSeam.Change` rail registration, callback failure, and rebroadcast over one polymorphic `ThemeShift`.
- Auto: `ThemeProgram.Generate` derives every cell from the cross-product of generated vocabulary items, so missing-cell fallbacks cannot exist; a `Hosted` merge re-enters the same contrast gate `Freeze` runs, so an ingested cell breaching a floor rejects without touching the grid.
- Receipt: `ThemeChange` carries the accepted generation, variant, changed-role set, and rebroadcast failures; a content-identical shift emits an empty changed set and holds the generation.
- Growth: a role or variant is one generated row plus generator support; another transition modality is one `ThemeShift` case; every consumer remains unchanged.
- Boundary: `ThemeSeam.Change` accepts the HostUi-selected shift — the variant polarity and any live host swatches arrive injected; Eto code never reads Rhino theme globals.
- Exemption: style registration, weak-control compaction, and `TriggerStyleChanged` rebroadcast are the handler-registry statement seam.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct StyleKey {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        if (string.IsNullOrWhiteSpace(value)) {
            validationError = new ValidationError(message: "Style identity requires text.");
            return;
        }
        value = value.Trim();
        validationError = null;
    }
}

[SmartEnum<int>]
public sealed partial class ThemeVariant {
    public static readonly ThemeVariant Light = new(key: 0);
    public static readonly ThemeVariant Dark = new(key: 1);
    public static readonly ThemeVariant Contrast = new(key: 2);
}

[SmartEnum<int>]
public sealed partial class PaletteRole {
    public static readonly PaletteRole Canvas = new(key: 0);
    public static readonly PaletteRole Panel = new(key: 1);
    public static readonly PaletteRole Accent = new(key: 2);
    public static readonly PaletteRole Stroke = new(key: 3);
    public static readonly PaletteRole GlyphPrimary = new(key: 4);
    public static readonly PaletteRole GlyphMuted = new(key: 5);
    public static readonly PaletteRole Focus = new(key: 6);
    public static readonly PaletteRole Selection = new(key: 7);
    public static readonly PaletteRole Hover = new(key: 8);
    public static readonly PaletteRole Success = new(key: 9);
    public static readonly PaletteRole Warning = new(key: 10);
    public static readonly PaletteRole Failure = new(key: 11);
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record ContrastRule(PaletteRole Foreground, PaletteRole Background, double Minimum);

public sealed record ThemeProgram(Func<PaletteRole, ThemeVariant, PerceptualColor> Generate) {
    public Validation<Error, Seq<(PaletteRole Role, ThemeVariant Variant, PerceptualColor Cell)>> Cells(Op key) =>
        toSeq(PaletteRole.Items).Bind(role =>
            toSeq(ThemeVariant.Items).Map(variant => (Role: role, Variant: variant)))
        .Traverse(row => Try.lift(() => Generate(row.Role, row.Variant)).Run()
            .Map(cell => (row.Role, row.Variant, Cell: cell))
            .MapFail(error => new UiFault.HostRejected(Key: key, Detail: error.Message))
            .ToValidation())
        .As()
        .Map(static cells => cells.Strict());
}

public sealed record ThemeSnapshot(
    long Generation,
    ThemeVariant Variant,
    HashMap<PaletteRole, PerceptualColor> Cells) {
    public PerceptualColor this[PaletteRole role] => Cells[role];
}

public readonly record struct ThemeChange(
    long Generation,
    ThemeVariant Variant,
    Seq<PaletteRole> Changed,
    Seq<Error> Failures) : IValidityEvidence {
    public bool IsValid => Failures.IsEmpty;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ThemeShift {
    private ThemeShift() { }
    public sealed record Generated(ThemeVariant Variant) : ThemeShift;
    public sealed record Hosted(ThemeVariant Variant, HashMap<PaletteRole, PerceptualColor> Cells) : ThemeShift;

    internal (ThemeVariant Variant, HashMap<PaletteRole, PerceptualColor> Overlay) Merge() => Switch(
        generated: static shift => (shift.Variant, HashMap<PaletteRole, PerceptualColor>()),
        hosted: static shift => (shift.Variant, shift.Cells));
}

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class ThemeCatalog {
    private readonly Seq<ContrastRule> contrast;
    private readonly Atom<ThemeState> current;

    private sealed record ThemeState(
        HashMap<ThemeVariant, HashMap<PaletteRole, PerceptualColor>> Grid,
        ThemeSnapshot Snapshot,
        Fin<ThemeChange> Outcome);

    private ThemeCatalog(
        HashMap<ThemeVariant, HashMap<PaletteRole, PerceptualColor>> grid,
        Seq<ContrastRule> contrast,
        ThemeSnapshot seed) {
        this.contrast = contrast;
        current = Atom(new ThemeState(
            grid,
            seed,
            Fin.Succ(new ThemeChange(seed.Generation, seed.Variant, [], []))));
    }

    public ThemeSnapshot Current => current.Value.Snapshot;

    public static Validation<Error, ThemeCatalog> Freeze(
        ThemeProgram program,
        ThemeVariant initial,
        Seq<ContrastRule> contrast,
        Op? key = null) {
        ArgumentNullException.ThrowIfNull(program);
        Op op = key.OrDefault();
        return (Initial(initial, op), contrast.Traverse(rule => Rule(rule, op)))
            .Apply(static (variant, rules) => (Variant: variant, Rules: rules.Strict()))
            .As()
            .Bind(admitted => program.Cells(op)
                .Map(cells => toHashMap(toSeq(ThemeVariant.Items).Map(variant =>
                    (variant, toHashMap(cells.Filter(cell => cell.Variant == variant).Map(cell => (cell.Role, cell.Cell)))))))
                .Bind(generated => toSeq(ThemeVariant.Items)
                    .Traverse(variant => Admitted(generated[variant], admitted.Rules, op))
                    .As()
                    .Map(_ => new ThemeCatalog(
                        generated, admitted.Rules,
                        new ThemeSnapshot(Generation: 0L, Variant: admitted.Variant, Cells: generated[admitted.Variant])))));
    }

    public Fin<ThemeChange> Swap(ThemeShift shift, Op? key = null) {
        ArgumentNullException.ThrowIfNull(shift);
        Op op = key.OrDefault();
        (ThemeVariant variant, HashMap<PaletteRole, PerceptualColor> overlay) = shift.Merge();
        return variant is null || !toSeq(ThemeVariant.Items).Contains(variant)
            ? Fin.Fail<ThemeChange>(new UiFault.Rejected(
                Key: op, Field: nameof(ThemeShift), Reason: "shift variant must belong to the declared theme vocabulary"))
            : op.Catch(() => current.Swap(held => {
                HashMap<PaletteRole, PerceptualColor> merged = toHashMap(toSeq(PaletteRole.Items)
                    .Map(role => (role, overlay.Find(role).IfNone(() => held.Grid[variant][role]))));
                return Admitted(merged, contrast, op).ToFin().Match(
                    Succ: admitted => {
                        Seq<PaletteRole> changed = toSeq(PaletteRole.Items)
                            .Filter(role => !held.Snapshot.Cells[role].Equals(admitted[role]))
                            .Strict();
                        ThemeSnapshot snapshot = held.Snapshot.Variant == variant && changed.IsEmpty
                            ? held.Snapshot
                            : new ThemeSnapshot(held.Snapshot.Generation + 1L, variant, admitted);
                        ThemeChange change = new(snapshot.Generation, snapshot.Variant, changed, []);
                        return new ThemeState(held.Grid, snapshot, Fin.Succ(change));
                    },
                    Fail: fault => held with { Outcome = Fin.Fail<ThemeChange>(fault) });
            }).Outcome);
    }

    private static Validation<Error, HashMap<PaletteRole, PerceptualColor>> Admitted(
        HashMap<PaletteRole, PerceptualColor> cells, Seq<ContrastRule> contrast, Op op) =>
        contrast.Traverse(rule => cells[rule.Foreground].Contrast(cells[rule.Background]) >= rule.Minimum
                ? Validation<Error, ContrastRule>.Success(rule)
                : Validation<Error, ContrastRule>.Fail(
                    new UiFault.Rejected(Key: op, Field: rule.Foreground.ToString(), Reason: "contrast floor breached")))
            .As()
            .Map(_ => cells);

    private static K<Validation<Error>, ThemeVariant> Initial(ThemeVariant initial, Op op) =>
        initial is not null && toSeq(ThemeVariant.Items).Contains(initial)
            ? Validation<Error, ThemeVariant>.Success(initial)
            : Validation<Error, ThemeVariant>.Fail(new UiFault.Rejected(
                Key: op, Field: nameof(initial), Reason: "initial variant must belong to the declared theme vocabulary"));

    private static K<Validation<Error>, ContrastRule> Rule(ContrastRule rule, Op op) =>
        rule is null
            ? Validation<Error, ContrastRule>.Fail(new UiFault.Rejected(
                Key: op, Field: nameof(ContrastRule), Reason: "contrast rule is absent"))
            : (Role(rule.Foreground, nameof(ContrastRule.Foreground), op),
               Role(rule.Background, nameof(ContrastRule.Background), op),
               Floor(rule.Minimum, op))
                .Apply((_, _, _) => rule);

    private static K<Validation<Error>, PaletteRole> Role(PaletteRole role, string field, Op op) =>
        role is not null && toSeq(PaletteRole.Items).Contains(role)
            ? Validation<Error, PaletteRole>.Success(role)
            : Validation<Error, PaletteRole>.Fail(new UiFault.Rejected(
                Key: op, Field: field, Reason: "role must belong to the declared palette vocabulary"));

    private static K<Validation<Error>, double> Floor(double minimum, Op op) =>
        double.IsFinite(minimum) && minimum is >= 1d and <= 21d
            ? Validation<Error, double>.Success(minimum)
            : Validation<Error, double>.Fail(new UiFault.Rejected(
                Key: op, Field: nameof(ContrastRule.Minimum), Reason: "contrast floors must be finite ratios from one through twenty-one"));
}

public sealed class ThemeSeam(ThemeCatalog catalog) {
    private readonly Atom<Seq<Error>> failures = Atom(Seq<Error>());
    private readonly Atom<Seq<WeakReference<Control>>> tracked = Atom(Seq<WeakReference<Control>>());

    public Seq<Error> Failures => failures.Value;

    public Fin<Unit> Register<TWidget>(
        StyleKey key,
        Action<TWidget, ThemeSnapshot> apply,
        Action<Error> report,
        Op? operation = null) where TWidget : Widget {
        Op op = operation.OrDefault();
        return string.IsNullOrWhiteSpace(key.Value)
            ? Fin.Fail<Unit>(new UiFault.Rejected(Key: op, Field: nameof(key), Reason: "style identity requires an admitted key"))
            : op.Catch(() => Fin.Succ(Op.Side(() => Style.Add<TWidget>(key.Value, widget => {
                if (widget is Control control) _ = Track(control);
                _ = op.Catch(() => Fin.Succ(Op.Side(() => apply(widget, catalog.Current)))).Match(
                    Succ: static applied => applied,
                    Fail: fault => Retain(fault, report, op));
            }))));
    }

    public Unit Track(Control control) => ignore(tracked.Swap(held => {
        Seq<WeakReference<Control>> live = held.Filter(static reference => reference.TryGetTarget(out _)).Strict();
        return control is null || live.Exists(reference =>
            reference.TryGetTarget(out Control? candidate) && ReferenceEquals(candidate, control))
            ? live
            : live.Add(new WeakReference<Control>(control));
    }));

    public Fin<ThemeChange> Change(ThemeShift shift, Op? key = null) {
        Op op = key.OrDefault();
        return catalog.Swap(shift, op).Bind(change =>
            op.Catch(() => {
                Seq<Control> live = tracked.Value.Choose(static reference =>
                    reference.TryGetTarget(out Control? control) ? Some(control) : None).Strict();
                _ = tracked.Swap(held => held.Filter(static reference => reference.TryGetTarget(out _)));
                return live.Map(static control => (Action)control.TriggerStyleChanged).Drained(op);
            }).Match(
                Succ: _ => Fin.Succ(change),
                Fail: fault => Fin.Succ(Retain(change, fault))));
    }

    private Unit Retain(Error fault, Action<Error> report, Op op) =>
        ignore(failures.Swap(held => held.Add(fault.Reported(report, op))));

    private ThemeChange Retain(ThemeChange change, Error fault) {
        _ = failures.Swap(held => held.Add(fault));
        return change with { Failures = Seq(fault) };
    }
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
