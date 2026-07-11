# [RASM_RHINO_ETO_PLATFORM]

The platform-handler, native-host, and theme seam of `Rasm.Rhino.Eto`. The Rhino process resolves ONE ambient `Platform` for its loaded `Eto.dll`; this owner binds that instance and projects it as rows — backend identity as a classified vocabulary, feature discovery as `Option`-railed probes gating `Supports<T>` before `Create<T>`, worker-thread scoping as a lease over `ThreadStart` — so a missing handler is a typed capability fact, never a construction crash, and `Platform.Initialize` against the host thread is structurally absent from the sub-domain. `NativeMount` bridges a host-native control into the Eto tree through `NativeControlHost` eagerly or lazily, and `NativeAttachment` leases the inverse crossing (`AttachNative`/`DetachNative`) so an Eto control under a foreign native parent always detaches. The theme seam is two owners: `ThemeSeam` registers named style rows through the host `Style` registry, tracks live controls, and rebroadcasts `TriggerStyleChanged` on a host light/dark transition; `ThemeCatalog` is the frozen (role × variant) color grid over the kernel `PerceptualColor` — total at freeze, atomically swapped with a changed-key diff — that `canvas.md` fills and chrome accents resolve, so a literal color at any call site is the named defect.

## [01]-[INDEX]

- [02]-[BACKEND]: `Backend` + `HostPlatform` — backend identity as classified rows and the `Option`-railed feature-discovery, shared-handler, worker-scope, and platform-context surface.
- [03]-[NATIVE_HOST]: `NativeMount` + `NativeAttachment` — both directions of the native seam: foreign control into the Eto tree, Eto control under a foreign parent, each lifecycle-owned.
- [04]-[STYLE_SEAM]: `StyleKey` + `ThemeSeam` — named style-handler rows, the tracked restyle set, the `TriggerStyleChanged` rebroadcast, and the Rhino theme-notifier subscription seam.
- [05]-[THEME_TOKENS]: `ThemeVariant` + `PaletteRole` + `ThemeCatalog` — the frozen perceptual color grid, its freeze-time totality gate, and the atomic variant swap with diff evidence.

## [02]-[BACKEND]

- Owner: `Backend` `[SmartEnum<int>]` — the classified backend vocabulary (`Mac`, `WinForms`, `Wpf`, `Gtk`, `Other`) whose `Of()` reads the ambient `Platform.Instance` predicates once, so backend branching is a generated `Switch` over rows, never an `IsMac`/`IsWpf` predicate ladder — and `HostPlatform`, the discovery owner: `Probe<THandler>` gates `Supports<THandler>()` before `Create<THandler>()` and returns `Option`, `Shared<THandler>` rides `CreateShared`, `Locate<THandler>` wraps `Find`, `WorkerScope` leases `ThreadStart()` for a non-UI thread that must construct Eto objects, and `Under` runs a body inside `Platform.Invoke` context.
- Law: the ambient platform is the host's — this owner never calls `Platform.Initialize`; a worker thread that needs Eto scopes through `WorkerScope`, and the scope's disposal is the thread's platform teardown.
- Law: capability absence is data — a `Probe` miss is `None` folded into a `UiFault.Unavailable` only where the caller REQUIRES the handler; probing to branch behavior is legal composition.
- Packages: Eto (host — `Platform`, `Platform.Instance`, `Supports<T>`, `Create<T>`, `CreateShared<T>`, `Find<T>`, `ThreadStart`, `Invoke`, the identity predicates), LanguageExt.Core, Rasm.Domain (project — `Op`, `Op.Catch`).
- Growth: a new backend the host ships is one `Backend` row; a new discovery verb is one member on `HostPlatform`.

```csharp
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Collections.Frozen;
using Eto;
using Eto.Forms;
using Rasm.Csp;
using Rasm.Domain;
using Rasm.Numerics;

namespace Rasm.Rhino.Eto;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class Backend {
    public static readonly Backend Mac = new(key: 0);
    public static readonly Backend WinForms = new(key: 1);
    public static readonly Backend Wpf = new(key: 2);
    public static readonly Backend Gtk = new(key: 3);
    public static readonly Backend Other = new(key: 4);

    public static Backend Of() =>
        (Platform.Instance.IsMac, Platform.Instance.IsWinForms, Platform.Instance.IsWpf, Platform.Instance.IsGtk) switch {
            (true, _, _, _) => Mac,
            (_, true, _, _) => WinForms,
            (_, _, true, _) => Wpf,
            (_, _, _, true) => Gtk,
            _ => Other,
        };
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class HostPlatform {
    public static string Identity => Platform.Instance.ID;

    public static Option<THandler> Probe<THandler>() where THandler : class =>
        Platform.Instance.Supports<THandler>() ? Optional(Platform.Instance.Create<THandler>()) : None;

    public static Option<THandler> Shared<THandler>() where THandler : class =>
        Platform.Instance.Supports<THandler>() ? Optional(Platform.Instance.CreateShared<THandler>()) : None;

    public static Option<THandler> Locate<THandler>() where THandler : class =>
        Optional(Platform.Instance.Find<THandler>());

    public static Fin<THandler> Require<THandler>(Op? key = null) where THandler : class =>
        Probe<THandler>().ToFin(Fail: new UiFault.Unavailable(Key: key.OrDefault(), Capability: typeof(THandler).Name));

    public static Fin<Lease<IDisposable>> WorkerScope(Op? key = null) =>
        key.OrDefault().Catch(() => Fin.Succ(value: (Lease<IDisposable>)new Lease<IDisposable>.Owned(Value: Platform.Instance.ThreadStart())));

    public static Fin<Unit> Under(Action body, Op? key = null) =>
        key.OrDefault().Catch(() => { Platform.Instance.Invoke(action: body); return Fin.Succ(value: unit); });
}
```

## [03]-[NATIVE_HOST]

- Owner: `NativeMount` — the closed `[Union]` over the two host construction shapes: `Eager(object)` wraps a live native control through the `NativeControlHost(object)` constructor, `Deferred(Func<object>)` defers the native mint to realization so the supplier pays only when the tree is actually built — and `NativeAttachment`, the inverse crossing: `Attach` moves an Eto control under an external native parent through `AttachNative()` and returns the lease whose disposal is the `DetachNative()` release, so an attached control cannot orphan its native parent relationship.
- Law: the deferred case is the default for expensive natives — an AppKit view, a WebKit process — because eager construction pays before any tree demands it; the supplier runs inside `Realize`'s `Op.Catch` bracket, so a throwing mint is a typed fault on the caller's rail.
- Law: this seam moves controls, never messages — event bridging, key forwarding, and focus negotiation between the native and Eto sides ride the host's own members on the realized objects; a message-pump shim here is the deleted form.
- Boundary: the macOS display-link, AppKit pacing, and Objective-C ownership are the Viewport unit's motion adapter over the macOS catalog; Rhino panel docking of Eto content is the HostUi unit's panel seam — both compose this mount, neither re-derives it.
- Growth: a lifecycle evidence axis (mounted-at ordinal, native type name) is one field on the attachment record.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record NativeMount {
    private NativeMount() { }
    public sealed record Eager(object Native) : NativeMount;
    public sealed record Deferred(Func<object> Supply) : NativeMount;

    public Fin<Control> Realize(Op? key = null) {
        Op op = key.OrDefault();
        return Switch(
            state: op,
            eager: static (op, mount) => op.Catch(() => Fin.Succ(value: (Control)new NativeControlHost(nativeControl: mount.Native))),
            deferred: static (op, mount) => op.Catch(() => Fin.Succ(value: (Control)new NativeControlHost(nativeControl: mount.Supply()))));
    }
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record NativeAttachment(Control Subject, Func<Unit> Detach) {
    public static Fin<NativeAttachment> Attach(Control subject, Op? key = null) =>
        key.OrDefault().Catch(() => {
            subject.AttachNative();
            return Fin.Succ(value: new NativeAttachment(Subject: subject, Detach: () => Op.Side(subject.DetachNative)));
        });
}
```

## [04]-[STYLE_SEAM]

- Owner: `StyleKey` `[ValueObject<string>]` — the named style identity `ElementSpec` carries and `Widget.Style` receives — and `ThemeSeam`, the restyle owner: `Register<TWidget>` lands a style-handler row in the host `Style` registry through the verified `Style.Add<TWidget>(string, StyleWidgetHandler<TWidget>)`, `Track` enrolls a live control in the weak restyle set, and `Rebroadcast` walks the survivors calling `TriggerStyleChanged()` so every registered handler re-applies under the new theme. Style handlers read the `[05]` catalog's CURRENT resolved palette, so a variant swap plus one rebroadcast restyles the whole tracked tree with zero per-control code.
- Law: registration precedes realization — a `StyleKey` referenced by an `ElementSpec` before its row lands is a silent no-op by host contract, so the seam's composition root registers rows first and the freeze-ordered boot is the guarantee, not a runtime check.
- Law: the tracked set is weak — realization enrolls, collection evicts, and `Rebroadcast` snapshots the survivors, prunes dead references through a pure `Swap`, and fires `TriggerStyleChanged` OUTSIDE the swap body (a `Swap` re-runs under contention, so a side-effecting fold would double-restyle); an unbounded strong set of controls is the leak this shape forecloses.
- Boundary: the Rhino transition edge is `Rhino.UI.ThemeSettings.ThemeChanged` — a public static `EventHandler` field subscribed with `+=` (the notifier behind `EtoExtensions` is a private nested type and never a seam); the HostUi shell owns that subscription and pushes the resolved variant into `OnHostThemeChanged(ThemeVariant)`, so this owner receives a variant and never reads the host.
- Growth: a new styled widget family is one `Register<TWidget>` row; a second restyle mechanism is the deleted form.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct StyleKey {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = string.IsNullOrWhiteSpace(value: value) ? new ValidationError(message: "StyleKey requires a non-whitespace name.") : null;
}

// --- [SERVICES] -----------------------------------------------------------------------------
public static class ThemeSeam {
    private static readonly Atom<Seq<WeakReference<Control>>> Tracked = Atom(Seq<WeakReference<Control>>());

    public static Unit Register<TWidget>(StyleKey key, Action<TWidget> apply) where TWidget : Widget =>
        Op.Side(() => Style.Add<TWidget>(style: key.Value, handler: widget => apply(widget)));

    public static Unit Track(Control control) =>
        ignore(Tracked.Swap(held => held.Add(new WeakReference<Control>(control))));

    public static Unit Rebroadcast() {
        Seq<Control> live = Tracked.Value.Choose(static reference => reference.TryGetTarget(out Control? held) ? Some(held) : None).Strict();
        _ = Tracked.Swap(held => held.Filter(static reference => reference.TryGetTarget(out _)));
        return ignore(live.Iter(static control => Op.Side(control.TriggerStyleChanged)));
    }

    public static Unit OnHostThemeChanged(ThemeVariant next) {
        _ = ThemeCatalog.Shared.Swap(variant: next);
        return Rebroadcast();
    }
}
```

## [05]-[THEME_TOKENS]

- Owner: `ThemeVariant` `[SmartEnum<int>]` (`Light`, `Dark`) — the variant axis the host light/dark transition selects — `PaletteRole` `[SmartEnum<int>]`, the closed role vocabulary every painted or chromed surface resolves against (canvas, panel, accent, stroke, primary and muted glyphs, focus, selection, hover, success, warning, failure), and `ThemeCatalog`, the frozen (role × variant) grid of kernel `PerceptualColor` cells: `Freeze` rejects any missing cell at construction, `Resolve` reads the current variant's full palette, `Swap` publishes the next variant atomically with a changed-role diff (value-identical cells emit nothing), and `Shared` is the one process instance — a second catalog forks the generation stamp and desynchronizes every derived cache. Ramps, hover derivations, and contrast checks are kernel math over the resolved cells — `Mix`, `Ramp`, `Contrast` on `PerceptualColor` — so no derived color is ever hand-lerped at a consumer.
- Law: the grid is total at freeze — exactly `roles × variants` cells, with a missing or duplicate cell a typed construction failure; a draw-time fallback chain is structurally absent because `Resolve` indexes a frozen dictionary the freeze proved complete.
- Law: text-bearing role pairs assert their WCAG floor at freeze — `GlyphPrimary` over `Canvas` and `GlyphPrimary` over `Panel` gate `Contrast >= 4.5` per variant, so an unreadable palette is a freeze rejection, never a shipped screen.
- Law: the swap is the one theme mutation — `ThemeSeam.OnHostThemeChanged` swaps then rebroadcasts; a consumer holding a resolved `PerceptualColor` across swaps holds stale paint, so style handlers and scene builders resolve at application time.
- Growth: a new role is one row plus its two seed cells — the freeze gate breaks construction until both land; a third variant (high-contrast) is one `ThemeVariant` row widening the same grid.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ThemeVariant {
    public static readonly ThemeVariant Light = new(key: 0);
    public static readonly ThemeVariant Dark = new(key: 1);
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
public sealed record ResolvedTheme(int Generation, ThemeVariant Variant, HashMap<PaletteRole, PerceptualColor> Cells) {
    public PerceptualColor this[PaletteRole role] => Cells[role];
}

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class ThemeCatalog {
    public static readonly ThemeCatalog Shared = Boot();
    private readonly FrozenDictionary<(int Role, int Variant), PerceptualColor> grid;
    private readonly Atom<ResolvedTheme> current;
    private ThemeCatalog(FrozenDictionary<(int, int), PerceptualColor> grid, ResolvedTheme seed) { this.grid = grid; current = Atom(seed); }

    public ResolvedTheme Current => current.Value;

    public static Fin<ThemeCatalog> Freeze(Seq<(PaletteRole Role, ThemeVariant Variant, PerceptualColor Cell)> cells, ThemeVariant boot, Op? key = null) {
        Op op = key.OrDefault();
        if (cells.Map(static cell => (cell.Role.Key, cell.Variant.Key)).GroupBy(static slot => slot).Exists(static group => group.Count() > 1)) {
            return Fin.Fail<ThemeCatalog>(error: new UiFault.Rejected(Key: op, Field: nameof(cells), Reason: "duplicate role x variant cell"));
        }
        FrozenDictionary<(int, int), PerceptualColor> grid = cells.AsEnumerable().ToFrozenDictionary(
            keySelector: static cell => (cell.Role.Key, cell.Variant.Key),
            elementSelector: static cell => cell.Cell);
        return toSeq(PaletteRole.Items).Bind(role => toSeq(ThemeVariant.Items).Map(variant => (Role: role, Variant: variant)))
            .TraverseM(slot => grid.ContainsKey((slot.Role.Key, slot.Variant.Key))
                ? Fin.Succ(value: slot)
                : Fin.Fail<(PaletteRole, ThemeVariant)>(error: new UiFault.Rejected(Key: op, Field: slot.Role.ToString(), Reason: $"missing cell for variant {slot.Variant}"))).As()
            .Bind(_ => Legible(grid: grid, op: op))
            .Map(_ => new ThemeCatalog(grid: grid, seed: new ResolvedTheme(Generation: 0, Variant: boot, Cells: Palette(grid: grid, variant: boot))));
    }

    public (int Generation, Seq<PaletteRole> Changed) Swap(ThemeVariant variant) {
        HashMap<PaletteRole, PerceptualColor> next = Palette(grid: grid, variant: variant);
        ResolvedTheme prior = current.Value;
        ResolvedTheme published = current.Swap(held => new ResolvedTheme(Generation: held.Generation + 1, Variant: variant, Cells: next));
        Seq<PaletteRole> changed = toSeq(PaletteRole.Items).Filter(role =>
            prior.Cells.Find(role).Map(was => !was.Equals(next.Find(role).IfNone(was))).IfNone(true));
        return (published.Generation, changed);
    }

    private static HashMap<PaletteRole, PerceptualColor> Palette(FrozenDictionary<(int, int), PerceptualColor> grid, ThemeVariant variant) =>
        toHashMap(toSeq(PaletteRole.Items).Map(role => (role, grid[(role.Key, variant.Key)])));

    private static Fin<Unit> Legible(FrozenDictionary<(int, int), PerceptualColor> grid, Op op) =>
        toSeq(ThemeVariant.Items)
            .TraverseM(variant =>
                grid[(PaletteRole.GlyphPrimary.Key, variant.Key)].Contrast(other: grid[(PaletteRole.Canvas.Key, variant.Key)]) >= 4.5
             && grid[(PaletteRole.GlyphPrimary.Key, variant.Key)].Contrast(other: grid[(PaletteRole.Panel.Key, variant.Key)]) >= 4.5
                    ? Fin.Succ(value: variant)
                    : Fin.Fail<ThemeVariant>(error: new UiFault.Rejected(Key: op, Field: nameof(PaletteRole.GlyphPrimary), Reason: $"contrast floor breached under {variant}"))).As()
            .Map(static _ => unit);

    private static ThemeCatalog Boot() =>
        Freeze(cells: StandardCells(), boot: ThemeVariant.Light).ThrowIfFail();

    private static Seq<(PaletteRole, ThemeVariant, PerceptualColor)> StandardCells() =>
        toSeq(new[] {
            Cell(PaletteRole.Canvas, ThemeVariant.Light, 250, 250, 250), Cell(PaletteRole.Canvas, ThemeVariant.Dark, 30, 30, 32),
            Cell(PaletteRole.Panel, ThemeVariant.Light, 240, 240, 241), Cell(PaletteRole.Panel, ThemeVariant.Dark, 44, 44, 47),
            Cell(PaletteRole.Accent, ThemeVariant.Light, 0, 102, 204), Cell(PaletteRole.Accent, ThemeVariant.Dark, 82, 156, 255),
            Cell(PaletteRole.Stroke, ThemeVariant.Light, 200, 200, 203), Cell(PaletteRole.Stroke, ThemeVariant.Dark, 70, 70, 74),
            Cell(PaletteRole.GlyphPrimary, ThemeVariant.Light, 24, 24, 26), Cell(PaletteRole.GlyphPrimary, ThemeVariant.Dark, 235, 235, 238),
            Cell(PaletteRole.GlyphMuted, ThemeVariant.Light, 110, 110, 115), Cell(PaletteRole.GlyphMuted, ThemeVariant.Dark, 155, 155, 160),
            Cell(PaletteRole.Focus, ThemeVariant.Light, 0, 122, 255), Cell(PaletteRole.Focus, ThemeVariant.Dark, 100, 170, 255),
            Cell(PaletteRole.Selection, ThemeVariant.Light, 205, 228, 255), Cell(PaletteRole.Selection, ThemeVariant.Dark, 42, 74, 115),
            Cell(PaletteRole.Hover, ThemeVariant.Light, 232, 240, 250), Cell(PaletteRole.Hover, ThemeVariant.Dark, 56, 60, 66),
            Cell(PaletteRole.Success, ThemeVariant.Light, 32, 140, 60), Cell(PaletteRole.Success, ThemeVariant.Dark, 88, 190, 118),
            Cell(PaletteRole.Warning, ThemeVariant.Light, 196, 138, 12), Cell(PaletteRole.Warning, ThemeVariant.Dark, 235, 186, 76),
            Cell(PaletteRole.Failure, ThemeVariant.Light, 200, 48, 48), Cell(PaletteRole.Failure, ThemeVariant.Dark, 240, 106, 106),
        });

    private static (PaletteRole, ThemeVariant, PerceptualColor) Cell(PaletteRole role, ThemeVariant variant, byte red, byte green, byte blue) =>
        (role, variant, PerceptualColor.OfRgb(red: red, green: green, blue: blue).ThrowIfFail());
}
```
