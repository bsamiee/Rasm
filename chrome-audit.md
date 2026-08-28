# 1. Delete the command-role mirror

From:
`libs/dotnet/Rasm/.planning/Interaction/chrome.md` `[02]-[INTENT]`, `CommandRole`
```csharp
[SmartEnum<int>]
public sealed partial class CommandRole {
    public static readonly CommandRole Push = new(key: 0, mint: static (_, _) => new Command());
    public static readonly CommandRole Check = new(key: 1, mint: static (seed, _) => new CheckCommand { Checked = seed.IfNone(false) });
    public static readonly CommandRole Radio = new(key: 2, mint: static (seed, head) => head.Match(
        Some: controller => new RadioCommand { Controller = controller, Checked = seed.IfNone(false) },
        None: () => new RadioCommand { Checked = seed.IfNone(false) }));
    [UseDelegateFromConstructor] internal partial Command Mint(Option<bool> seed, Option<RadioCommand> head);
}
```

To:
```csharp
// CommandRole DELETED
```

Why: The rows duplicate the `CommandKind` cases and publish a second discriminant for the same behavior.

Change: Mint the Eto command directly in `CommandKind`.

Delta: Net -10 nonblank code LOC, -1 top-level type, and -4 declared members.

# 2. Mint commands in the behavior owner

From:
`libs/dotnet/Rasm/.planning/Interaction/chrome.md` `[02]-[INTENT]`, `CommandKind.Role` through `CommandKind.Seed`
```csharp
    internal CommandRole Role => Switch(
        act: static _ => CommandRole.Push,
        toggle: static _ => CommandRole.Check,
        pick: static _ => CommandRole.Radio);
    internal Option<GroupKey> Group => Switch(
        act: static _ => Option<GroupKey>.None,
        toggle: static _ => Option<GroupKey>.None,
        pick: static kind => Some(kind.Group));
    internal Option<bool> Seed => Switch(
        act: static _ => Option<bool>.None,
        toggle: static kind => Some(kind.Read()),
        pick: static kind => Some(kind.Read()));
```

To:
```csharp
    internal (Command Host, Option<GroupKey> Group) Mint(Option<RadioCommand> controller) => Switch(
        state: controller,
        act: static (_, _) => ((Command)new Command(), Option<GroupKey>.None),
        toggle: static (_, kind) => (
            (Command)new CheckCommand { Checked = kind.Read() }, Option<GroupKey>.None),
        pick: static (head, kind) => ((Command)head.Match(
            Some: command => new RadioCommand { Controller = command, Checked = kind.Read() },
            None: () => new RadioCommand { Checked = kind.Read() }), Some(kind.Group)));
```

Why: `Role`, `Group`, and `Seed` are immediately recombined by `IntentTable.Bind`.

Change: Return the command and radio-group evidence from one exhaustive dispatch.

Delta: Net -4 nonblank code LOC and -2 declared members.

# 3. Remove invocation convenience projections

From:
`libs/dotnet/Rasm/.planning/Interaction/chrome.md` `[02]-[INTENT]`, `Invocation`
```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct Invocation(
    IntentKey Key,
    Fin<Unit> Outcome,
    Option<GaugedSpan<DispatchLane>> Span) : IUiFact, IValidityEvidence {
    public string Kind => "intent";
    public Option<Error> Fault => Outcome.Match(Succ: static _ => Option<Error>.None, Fail: Some);
    public Option<TimeSpan> Latency => Span.Map(static span => span.Elapsed);
    public bool IsValid => Outcome.IsSucc;
}
```

To:
```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct Invocation(
    IntentKey Key,
    Fin<Unit> Outcome,
    Option<GaugedSpan<DispatchLane>> Span) : IUiFact, IValidityEvidence {
    public string Kind => "intent";
    public bool IsValid => Outcome.IsSucc;
}
```

Why: `Fault` and `Latency` only rename public carrier projections.

Change: Match `Outcome` and map `Span` at their consumers.

Delta: Net -2 nonblank code LOC and -2 declared members.

# 4. Keep only menu-tree operations

From:
`libs/dotnet/Rasm/.planning/Interaction/chrome.md` `[03]-[MENU]`, `MenuForge`
```csharp
public static class MenuForge {
    public static Fin<Lease<ContextMenu>> Context(
        Seq<MenuNode> nodes, IntentTable table, Option<Dimension> depth = default);
    public static Fin<Unit> Attach(Control host, Lease<ContextMenu> menu);
    public static Fin<Unit> Popup(Lease<ContextMenu> menu, Control anchor, EtoPointF at);
    public static Fin<Seq<MenuSlot>> Flatten(
        Seq<MenuNode> nodes, IntentTable table, Option<Dimension> depth = default);
    public static Fin<Option<IntentKey>> Choose(Seq<MenuSlot> slots, int index);
}
```

To:
```csharp
public static class MenuTree {
    public static Fin<Lease<ContextMenu>> Context(Seq<MenuNode> nodes, IntentTable table);
    public static Fin<Seq<MenuSlot>> Flatten(Seq<MenuNode> nodes, IntentTable table);
    public static Fin<Option<IntentKey>> Choose(Seq<MenuSlot> slots, int index);
}
```

Why: `Attach` and `Popup` rename host calls, while the optional depth re-describes the declared limit.

Change: Keep construction, flattening, and ordinal resolution; assign and show at the control boundary.

Delta: Net -4 nonblank code LOC and -2 declared members.

Ripples: Replace `MenuForge.Flatten` and `MenuForge.Choose` with `MenuTree.Flatten` and `MenuTree.Choose` in `libs/dotnet/Rasm.Rhino/.planning/HostUi/dialogs.md`.

# 5. Replace the menu-budget vocabulary with limits

From:
`libs/dotnet/Rasm/.planning/Interaction/chrome.md` `[03]-[MENU]`, `MenuBudget`
```csharp
[SmartEnum<int>]
public sealed partial class MenuBudget {
    public static readonly MenuBudget Depth = new(key: 0, bound: Dimension.Create(value: 16));
    public static readonly MenuBudget Roster = new(key: 1, bound: Dimension.Create(value: 512));
    public Dimension Bound { get; }
}
```

To:
```csharp
    private static readonly Dimension MaximumDepth = Dimension.Create(value: 16);
    private static readonly Dimension MaximumItems = Dimension.Create(value: 512);
```

Why: Two unrelated ceilings are not a keyed runtime vocabulary.

Change: Seat the limits privately on `MenuTree`.

Delta: Net -4 nonblank code LOC, -1 top-level type, and -1 declared member.

# 6. Use keyless window modality rows

From:
`libs/dotnet/Rasm/.planning/Interaction/chrome.md` `[04]-[WINDOW]`, `WindowRole`
```csharp
[SmartEnum<int>]
public sealed partial class WindowRole {
    public static readonly WindowRole Shell = new(key: 0, mint: static () => new Form());
    public static readonly WindowRole Float = new(key: 1, mint: static () => new FloatingForm());
    [UseDelegateFromConstructor] internal partial Form Mint();
}
```

To:
```csharp
[SmartEnum]
public sealed partial class WindowRole {
    public static readonly WindowRole Modeless = new(mint: static () => new Form());
    public static readonly WindowRole Floating = new(mint: static () => new FloatingForm());
    [UseDelegateFromConstructor] internal partial Form Mint();
}
```

Why: The process-local modality has no wire identity; the replacement names match Eto.

Change: Remove ordinal keys and rename the generated rows and dispatch arms.

Delta: Net 0 handwritten LOC and 0 declared members; generated key lookup and conversion members are removed.

# 7. Keep every lifecycle state without ordinal keys

From:
`libs/dotnet/Rasm/.planning/Interaction/chrome.md` `[04]-[WINDOW]`, `MountPhase`
```csharp
[SmartEnum<int>]
public sealed partial class MountPhase {
    public static readonly MountPhase Open = new(key: 0, closes: false);
    public static readonly MountPhase Closing = new(key: 1, closes: true);
    public static readonly MountPhase Released = new(key: 2, closes: true);
    public bool Closes { get; }
}
```

To:
```csharp
[SmartEnum]
public sealed partial class MountPhase {
    public static readonly MountPhase Open = new(closes: false);
    public static readonly MountPhase Closing = new(closes: true);
    public static readonly MountPhase Released = new(closes: true);
    public bool Closes { get; }
}
```

Why: `Released` is consumed by Rhino render and page lifecycles, but the states have no wire key.

Change: Retain the lifecycle and remove only generated key surface.

Delta: Net 0 handwritten LOC and 0 declared members; generated key lookup and conversion members are removed.

# 8. Return a typed refusal from custody entry

From:
`libs/dotnet/Rasm/.planning/Interaction/chrome.md` `[04]-[WINDOW]`, `MountCustody.Entered`
```csharp
    public Option<MountCustody> Entered() => Switch(
        live: static row => row.Phase.Closes
            ? Option<MountCustody>.None
            : Some<MountCustody>(row with { Active = Some(Dimension.Create(value: row.Active.Map(static held => held.Value).IfNone(0) + 1)) }),
        released: static _ => Option<MountCustody>.None);
```

To:
```csharp
    public Fin<MountCustody> Entered() => Switch(
        live: static row => row.Phase.Closes
            ? Fin.Fail<MountCustody>(new UiFault.Released())
            : Fin.Succ<MountCustody>(row with {
                Active = Some(Dimension.Create(value: row.Active.Map(static held => held.Value).IfNone(0) + 1)),
            }),
        released: static _ => Fin.Fail<MountCustody>(new UiFault.Released()));
```

Why: A closing or released mount is a reason-bearing failure, not absence.

Change: Bind the `Fin` before the protected operation.

Delta: Net +2 nonblank code LOC and 0 declared symbols.

# 9. Remove the unwriteable custody backlink

From:
`libs/dotnet/Rasm/.planning/Interaction/chrome.md` `[04]-[WINDOW]`, `MountCustody.Live`
```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MountCustody {
    private MountCustody() { }
    public sealed record Live(
        Option<Dimension> Active, Seq<IMount> Children, Option<IMount> Owner, MountPhase Phase) : MountCustody;
    public sealed record Released : MountCustody;
```

To:
```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MountCustody {
    private MountCustody() { }
    public sealed record Live(
        Option<Dimension> Active, Seq<IMount> Children, MountPhase Phase) : MountCustody;
    public sealed record Released : MountCustody;
```

Why: `IMount` exposes no operation that can write an owner into a child.

Change: Keep custody on the parent roster.

Delta: Net 0 LOC and -1 positional member.

Ripples: Remove `Owner: None` from the five `MountCustody.Live` initializers in this sheet.

# 10. Release children adopted after closure

From:
`libs/dotnet/Rasm/.planning/Interaction/chrome.md` `[04]-[WINDOW]`, `MountCustody.Adopted`
```csharp
    public MountCustody Adopted(IMount child) => Switch(
        state: child,
        live: static (held, row) => (MountCustody)(row with { Children = row.Children.Add(held) }),
        released: static (_, row) => row);
```

To:
```csharp
    public (MountCustody Next, Option<IMount> Release) Adopted(IMount child) => Switch(
        state: child,
        live: static (held, row) => row.Phase.Closes
            ? ((MountCustody)row, Some(held))
            : ((MountCustody)(row with { Children = row.Children.Add(held) }), Option<IMount>.None),
        released: static (held, row) => ((MountCustody)row, Some(held)));
```

Why: The released arm currently drops the child without owning or releasing it.

Change: Return late children for release after the pure custody swap.

Delta: Net +2 nonblank code LOC and 0 declared symbols.

# 11. Delete fictional reverse-custody operations

From:
`libs/dotnet/Rasm/.planning/Interaction/chrome.md` `[04]-[WINDOW]`, `MountCustody.Dropped` and `MountCustody.Adopter`
```csharp
    public MountCustody Dropped(IMount child) => Switch(
        state: child,
        live: static (held, row) => (MountCustody)(row with {
            Children = row.Children.Filter(seated => seated.Key != held.Key),
        }),
        released: static (_, row) => row);
    public Option<IMount> Adopter() => Switch(
        live: static row => row.Owner,
        released: static _ => Option<IMount>.None);
```

To:
```csharp
// MountCustody.Dropped and MountCustody.Adopter DELETED
```

Why: `IMount` has no `Key`, and the removed backlink leaves neither operation implementable.

Change: Rely on the parent roster and idempotent child release.

Delta: Net -9 nonblank code LOC and -2 declared members.

# 12. Delete the styling delegate wrapper

From:
`libs/dotnet/Rasm/.planning/Interaction/chrome.md` `[04]-[WINDOW]`, `ChromeStyler`
```csharp
public sealed record ChromeStyler(Func<Control, Fin<Unit>> Dress);
```

To:
```csharp
// ChromeStyler DELETED
```

Why: The record only renames one function and owns no invariant or lifecycle.

Change: Carry `Func<Control, Fin<Unit>>` directly in the presentation records.

Delta: Net -1 nonblank code LOC and -1 top-level type.

Ripples: Change `WindowPolicy.Styler` and its rows in `libs/dotnet/Rasm.Rhino/.planning/HostUi/shell.md` to the direct function type.

# 13. Keep one window construction entry

From:
`libs/dotnet/Rasm/.planning/Interaction/chrome.md` `[04]-[WINDOW]`, `WindowSpec`
```csharp
public sealed record WindowSpec(
    string Title,
    ControlSpec Content,
    WindowRole Role,
    WindowChrome Chrome,
    Option<ChromeStyler> Styler,
    Option<PlacementKey> Menu,
    Option<PlacementKey> Bar,
    bool Activated) {
    public Fin<Lease<WindowMount>> Realize(ElementRuntime runtime);
    public Fin<Lease<WindowMount>> Present(ElementRuntime runtime);
}
```

To:
```csharp
public sealed record WindowSpec(
    string Title,
    ControlSpec Content,
    WindowRole Role,
    WindowChrome Chrome,
    Option<Func<Control, Fin<Unit>>> Style,
    Option<PlacementKey> Menu,
    Option<PlacementKey> Bar,
    bool ShowActivated) {
    public Fin<Lease<WindowMount>> Realize(ElementRuntime runtime);
}
```

Why: `Present` is `Realize` followed by `Form.Show`; retained names should match their carried function and Eto property.

Change: Traverse `Style` during realization and show at the application boundary.

Delta: Net -1 nonblank code LOC and -1 declared member.

# 14. Store prompt refusal policy as a boolean

From:
`libs/dotnet/Rasm/.planning/Interaction/chrome.md` `[05]-[PROMPT]`, `RefusalPosture`
```csharp
[SmartEnum<int>]
public sealed partial class RefusalPosture {
    public static readonly RefusalPosture Close = new(key: 0, holds: false);
    public static readonly RefusalPosture KeepOpen = new(key: 1, holds: true);
    internal bool Holds { get; }
}
```

To:
```csharp
// RefusalPosture DELETED
```

Why: The two rows derive one boolean and have no distinct behavior or boundary identity.

Change: Store `KeepOpenOnRefusal` directly on `Prompt<TResult>`.

Delta: Net -6 nonblank code LOC, -1 top-level type, and -3 declared members.

# 15. Delete the custom prompt outcome carrier

From:
`libs/dotnet/Rasm/.planning/Interaction/chrome.md` `[05]-[PROMPT]`, `PromptSettle<TResult>`
```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PromptSettle<TResult> {
    private PromptSettle() { }
    public sealed record Chose(TResult Value) : PromptSettle<TResult>;
    public sealed record Refused(Error Cause) : PromptSettle<TResult>;
    public sealed record Dismissed : PromptSettle<TResult>;
}
```

To:
```csharp
// PromptSettle<TResult> DELETED
```

Why: `Chose` and `Refused` duplicate success and failure, while dismissal is ordinary absence.

Change: Represent the crossing as `IO<Option<TResult>>`.

Delta: Net -7 nonblank code LOC, -1 top-level type, and -3 nested case types.

Ripples: Replace `PromptSettle<TResult>` with `Option<TResult>` inside the `IO` flow in `libs/dotnet/Rasm.AppUi/.planning/Shell/dialogs.md`.

# 16. Keep one deferred prompt entry

From:
`libs/dotnet/Rasm/.planning/Interaction/chrome.md` `[05]-[PROMPT]`, `Prompt<TResult>`
```csharp
public sealed record Prompt<TResult>(
    string Title, ControlSpec Content, Seq<PromptChoice<TResult>> Choices,
    Option<string> CancelCaption, Option<EtoSize> ClientSize,
    DialogDisplayMode DisplayMode, Option<ChromeStyler> Styler,
    RefusalPosture Posture, FaultCell Faults) {
    public Fin<TResult> Ask(
        ElementRuntime runtime,
        Func<Dialog<PromptSettle<TResult>>, Fin<PromptSettle<TResult>>> present);
    public ValueTask<Fin<TResult>> Ask(
        ElementRuntime runtime,
        Func<Dialog<PromptSettle<TResult>>, ValueTask<Fin<PromptSettle<TResult>>>> present,
        CancellationToken cancellation);
    private Fin<Unit> Admit();
}
```

To:
```csharp
public sealed record Prompt<TResult>(
    string Title, ControlSpec Content, Seq<PromptChoice<TResult>> Choices,
    Option<string> CancelCaption, Option<EtoSize> ClientSize,
    DialogDisplayMode DisplayMode, Option<Func<Control, Fin<Unit>>> Style,
    bool KeepOpenOnRefusal, FaultCell Faults) {
    public IO<Option<TResult>> Ask(
        ElementRuntime runtime,
        Func<Dialog<Option<TResult>>, IO<Option<TResult>>> present);
    private Fin<Unit> Admit();
}
```

Why: Prompt work is deferred boundary work; the two entries duplicate one flow, and cancellation belongs to the effect.

Change: Keep one `IO<Option<TResult>>` entry and direct policy values.

Delta: Net -4 nonblank code LOC and -1 declared member.

Ripples: Adapt the presenter in `libs/dotnet/Rasm.Rhino/.planning/HostUi/shell.md` to `IO<Option<TResult>>`.

# 17. Remove prompt-mount outcome forwarding

From:
`libs/dotnet/Rasm/.planning/Interaction/chrome.md` `[05]-[PROMPT]`, `PromptMount<TResult>` result members
```csharp
    internal Dialog<PromptSettle<TResult>> Dialog { get; }
    internal Unit Cancel();
    internal static Fin<TResult> Settle(PromptSettle<TResult> verdict);
    public Fin<Unit> Release();
    public void Dispose() => _ = Release();
```

To:
```csharp
    internal Dialog<Option<TResult>> Dialog { get; }
    public Fin<Unit> Release();
    public void Dispose() => _ = Release();
```

Why: `Cancel` only closes with dismissal and `Settle` only re-encodes the deleted carrier.

Change: Close with `None` and return the presenter effect directly.

Delta: Net -2 nonblank code LOC and -2 declared members.

# 18. Remove process-local keys from message policies

From:
`libs/dotnet/Rasm/.planning/Interaction/chrome.md` `[05]-[PROMPT]`, `AskDelivery` and `AskModality`
```csharp
[SmartEnum<int>]
public sealed partial class AskDelivery {
    public static readonly AskDelivery Application = new(key: 0, universal: true);
    public static readonly AskDelivery Desktop = new(key: 1, universal: false);
    public static readonly AskDelivery Service = new(key: 2, universal: false);
    internal bool Universal { get; }
}
[SmartEnum<int>]
public sealed partial class AskModality {
    public static readonly AskModality Application = new(key: 0);
    public static readonly AskModality System = new(key: 1);
    public static readonly AskModality Task = new(key: 2);
}
```

To:
```csharp
[SmartEnum]
public sealed partial class AskDelivery {
    public static readonly AskDelivery Application = new();
    public static readonly AskDelivery Desktop = new();
    public static readonly AskDelivery Service = new();
}
[SmartEnum]
public sealed partial class AskModality {
    public static readonly AskModality Application = new();
    public static readonly AskModality System = new();
    public static readonly AskModality Task = new();
}
```

Why: Neither policy crosses a wire, and `Universal` duplicates the `Application` identity.

Change: Use keyless dispatch and compare the delivery row directly.

Delta: Net -1 handwritten LOC and -1 declared member; generated key surface is removed from both types.

# 19. Re-enter the sheet owner through its factory

From:
`libs/dotnet/Rasm/.planning/Interaction/chrome.md` `[06]-[PRINT]`, `PageFrame.Sheet.Inset`
```csharp
            from surface in SheetSize.Custom(
                width: laid.Width, height: laid.Height, standard: Size.Standard)
            from extent in surface.In(unit: points)
```

To:
```csharp
            from surface in SheetSize.Of(
                width: laid.Width, height: laid.Height, standard: Size.Standard)
            from extent in surface.In(unit: points)
```

Why: `SheetSize.Custom` is an internal case constructor; `SheetSize.Of` is the public admission surface.

Change: Admit the oriented extent before projecting printer points.

Delta: Net 0 LOC and 0 symbols.

# 20. Call the sheet inset with its declared arity

From:
`libs/dotnet/Rasm/.planning/Interaction/chrome.md` `[06]-[PRINT]`, `PageFrame.Resolve`
```csharp
    internal Fin<EtoRectangleF> Resolve(PrintPageEventArgs args) => Switch(
        state: args,
        host: static (held, _) => Fin.Succ(new EtoRectangleF(
            x: 0f, y: 0f, width: held.PageSize.Width, height: held.PageSize.Height)),
        printer: static (_, frame) => Fin.Succ(frame.Settings.PrintableArea),
        bounded: static (_, frame) => Fin.Succ(frame.Bounds),
        sheet: static (held, frame) => frame.Inset(held.Key));
```

To:
```csharp
    internal Fin<EtoRectangleF> Resolve(PrintPageEventArgs args) => Switch(
        state: args,
        host: static (held, _) => Fin.Succ(new EtoRectangleF(
            x: 0f, y: 0f, width: held.PageSize.Width, height: held.PageSize.Height)),
        printer: static (_, frame) => Fin.Succ(frame.Settings.PrintableArea),
        bounded: static (_, frame) => Fin.Succ(frame.Bounds),
        sheet: static (_, frame) => frame.Inset());
```

Why: `Sheet.Inset` is parameterless and `PrintPageEventArgs` has no `Key`.

Change: Discard the threaded state in the sheet arm.

Delta: Net 0 LOC and 0 symbols.

# 21. Delete the second page-span factory

From:
`libs/dotnet/Rasm/.planning/Interaction/chrome.md` `[06]-[PRINT]`, `PageSpan`
```csharp
[ComplexValueObject]
public sealed partial class PageSpan {
    public Dimension First { get; }
    public Dimension Last { get; }
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Dimension first, ref Dimension last) =>
        validationError = last.Value >= first.Value
            ? null
            : new ValidationError(message: "PageSpan requires a last page at or after its first.");
    public static Fin<PageSpan> Of(Dimension first, Dimension last, Dimension pageCount);
}
```

To:
```csharp
[ComplexValueObject]
public sealed partial class PageSpan {
    public Dimension First { get; }
    public Dimension Last { get; }
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Dimension first, ref Dimension last) =>
        validationError = last.Value >= first.Value
            ? null
            : new ValidationError(message: "PageSpan requires a last page at or after its first.");
}
```

Why: Generated admission owns ordering; the job ceiling is contextual.

Change: Compare the admitted span with the live page count in `PrintScope.Admit`.

Delta: Net -1 nonblank code LOC and -1 declared member.

# 22. Admit print scope without reconstruction

From:
`libs/dotnet/Rasm/.planning/Interaction/chrome.md` `[06]-[PRINT]`, `PrintScope.Admit`
```csharp
    internal Fin<Unit> Admit(Dimension pageCount) => Switch(
        state: pageCount,
        all: static (_, _) => Fin.Succ(unit),
        selected: static (held, scope) =>
            PageSpan.Of(first: scope.Span.First, last: scope.Span.Last, pageCount: held)
                .Map(static _ => unit),
        hostSelection: static (held, _) => Fin.Fail<Unit>(new UiFault.Rejected(Field: FieldTag.Create(value: nameof(PrintScope)), Reason: RejectReason.HostSelection)));
```

To:
```csharp
    internal Fin<Unit> Admit(Dimension pageCount) => Switch(
        state: pageCount,
        all: static (_, _) => Fin.Succ(unit),
        selected: static (held, scope) => scope.Span.Last.Value <= held.Value
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new UiFault.Rejected(
                Field: FieldTag.Create(value: nameof(PageSpan)), Reason: RejectReason.PageSpan)),
        hostSelection: static (_, _) => Fin.Succ(unit));
```

Why: `PageSpan` already proves ordering, and Eto declares host selection as a valid scope.

Change: Check only the contextual upper bound and configure host selection normally.

Delta: Net +1 nonblank code LOC and 0 declared symbols.

# 23. Make print policy rows keyless

From:
`libs/dotnet/Rasm/.planning/Interaction/chrome.md` `[06]-[PRINT]`, `CollatePosture` and `PageOrder`
```csharp
[SmartEnum<int>]
public sealed partial class CollatePosture {
    public static readonly CollatePosture Host = new(key: 0, apply: static _ => unit);
    public static readonly CollatePosture Collated = new(key: 1, apply: static settings => HostEdge.Side(() => settings.Collate = true));
    public static readonly CollatePosture Uncollated = new(key: 2, apply: static settings => HostEdge.Side(() => settings.Collate = false));
    [UseDelegateFromConstructor] internal partial Unit Apply(PrintSettings settings);
}
[SmartEnum<int>]
public sealed partial class PageOrder {
    public static readonly PageOrder Host = new(key: 0, apply: static _ => unit);
    public static readonly PageOrder Forward = new(key: 1, apply: static settings => HostEdge.Side(() => settings.Reverse = false));
    public static readonly PageOrder Reverse = new(key: 2, apply: static settings => HostEdge.Side(() => settings.Reverse = true));
    [UseDelegateFromConstructor] internal partial Unit Apply(PrintSettings settings);
}
```

To:
```csharp
[SmartEnum]
public sealed partial class CollatePosture {
    public static readonly CollatePosture Host = new(apply: static _ => unit);
    public static readonly CollatePosture Collated = new(apply: static settings => HostEdge.Side(() => settings.Collate = true));
    public static readonly CollatePosture Uncollated = new(apply: static settings => HostEdge.Side(() => settings.Collate = false));
    [UseDelegateFromConstructor] internal partial Unit Apply(PrintSettings settings);
}
[SmartEnum]
public sealed partial class PageOrder {
    public static readonly PageOrder Host = new(apply: static _ => unit);
    public static readonly PageOrder Forward = new(apply: static settings => HostEdge.Side(() => settings.Reverse = false));
    public static readonly PageOrder Reverse = new(apply: static settings => HostEdge.Side(() => settings.Reverse = true));
    [UseDelegateFromConstructor] internal partial Unit Apply(PrintSettings settings);
}
```

Why: Both are process-local behavior rows with no persisted key.

Change: Retain behavior columns and remove generated key surface.

Delta: Net 0 handwritten LOC and 0 declared members; generated key lookup and conversions are removed from both types.

# 24. Remove print-outcome convenience projections

From:
`libs/dotnet/Rasm/.planning/Interaction/chrome.md` `[06]-[PRINT]`, `PrintOutcome.Failed` and `PrintOutcome.Actual`
```csharp
    public Seq<PrintPageFact.Failed> Failed => Pages.Choose(static fact => fact.Map(
        @default: static _ => Option<PrintPageFact.Failed>.None,
        failed: Some));
    public int Actual => Pages.Count;
```

To:
```csharp
// PrintOutcome.Failed and PrintOutcome.Actual DELETED
```

Why: Both members are direct projections of public `Pages`.

Change: Project failures and count pages at their consumers.

Delta: Net -4 nonblank code LOC and -2 declared members.

# 25. State print completion on the page facts

From:
`libs/dotnet/Rasm/.planning/Interaction/chrome.md` `[06]-[PRINT]`, `PrintOutcome.Measured` and `PrintOutcome.Completed`
```csharp
    private Tally Measured => Pages.Fold(state: Tally.Of(expected: Expected), f: static (tally, fact) => tally.With(fact));
    public bool Completed => HostCompleted && Measured.Complete;
```

To:
```csharp
    public bool Completed =>
        HostCompleted
        && Pages.Count == Expected.Value
        && Pages.ForAll(fact => fact.IsValid && fact.Index >= 0 && fact.Index < Expected.Value)
        && Pages.Map(static fact => fact.Index).Distinct().Count == Pages.Count;
```

Why: Count, validity, range, and uniqueness are already carrier operations.

Change: Express completion directly over `Pages`.

Delta: Net +3 nonblank code LOC and -1 declared member.

# 26. Delete the print tally type

From:
`libs/dotnet/Rasm/.planning/Interaction/chrome.md` `[06]-[PRINT]`, `PrintOutcome.Tally`
```csharp
    private readonly record struct Tally(Dimension Expected, int Count, Set<int> Indices, bool InRange, bool Failed) {
        internal static Tally Of(Dimension expected) =>
            new(Expected: expected, Count: 0, Indices: Set<int>(), InRange: true, Failed: false);
        internal Tally With(PrintPageFact fact) => this with {
            Count = Count + 1,
            Indices = Indices.TryAdd(fact.Index),
            InRange = InRange && fact.Index >= 0 && fact.Index < Expected.Value,
            Failed = Failed || !fact.IsValid,
        };
        internal bool Complete => Count == Expected.Value && Indices.Count == Count && InRange && !Failed;
    }
```

To:
```csharp
// PrintOutcome.Tally DELETED
```

Why: After completion is direct, the nested accumulator owns no behavior.

Change: Remove the type and its three members.

Delta: Net -11 nonblank code LOC, -1 nested type, and -3 declared members.

# 27. Put presence acquisition on the operation owner

From:
`libs/dotnet/Rasm/.planning/Interaction/chrome.md` `[07]-[PRESENCE]`, `PresenceOp` operation members
```csharp
    public sealed record Alert(Toast Card, Action<ToastKey> Activated, Option<PresenceMount> Anchor) : PresenceOp;
    public sealed record Tray(string Title, Lease<EtoImage> Icon, Option<Lease<ContextMenu>> Menu, Action Activated) : PresenceOp;
    public sealed record Pulse(PulseState State) : PresenceOp;
    public sealed record Badge(Option<string> Label) : PresenceOp;
    internal Fin<Unit> Precondition() => Switch(
```

To:
```csharp
    public sealed record Alert(Toast Card, Action<ToastKey> Activated, Option<PresenceMount> Anchor) : PresenceOp;
    public sealed record Tray(string Title, Lease<EtoImage> Icon, Option<Lease<ContextMenu>> Menu, Action Activated) : PresenceOp;
    public sealed record Pulse(PulseState State) : PresenceOp;
    public sealed record Badge(Option<string> Label) : PresenceOp;
    public Fin<Lease<PresenceMount>> Apply(FaultCell faults);
    internal Fin<Unit> Precondition() => Switch(
```

Why: Acquisition exhaustively dispatches on `PresenceOp`; a separate one-method owner is a forwarding hop.

Change: Move the acquisition body to `PresenceOp.Apply`.

Delta: Net +1 nonblank code LOC and +1 declared member before deleting the shell.

Ripples: Call `.Apply(cell)` on the constructed `PresenceOp.Pulse` in `libs/dotnet/Rasm.Rhino/.planning/HostUi/shell.md`.

# 28. Resolve the actual tray anchor

From:
`libs/dotnet/Rasm/.planning/Interaction/chrome.md` `[07]-[PRESENCE]`, `PresenceOp.Anchored`
```csharp
    internal static Fin<Unit> Anchored(Alert alert, Notification card) =>
        alert.Anchor.IsSome || !card.RequiresTrayIndicator
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new UiFault.Rejected(Field: FieldTag.Create(value: nameof(Alert)), Reason: RejectReason.TrayAnchor));
```

To:
```csharp
    internal static Fin<Unit> Anchored(Alert alert, Notification card) =>
        !card.RequiresTrayIndicator || alert.Anchor.Bind(static mount => mount.Indicator).IsSome
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new UiFault.Rejected(Field: FieldTag.Create(value: nameof(Alert)), Reason: RejectReason.TrayAnchor));
```

Why: Pulse, badge, and alert mounts satisfy `IsSome` but contain no tray indicator.

Change: Admit only an anchor that projects the required host object.

Delta: Net 0 LOC and 0 symbols.

# 29. Retain alert image ownership

From:
`libs/dotnet/Rasm/.planning/Interaction/chrome.md` `[07]-[PRESENCE]`, `PresenceHold` cases
```csharp
    internal sealed record AlertHold(Lease<Notification> Card) : PresenceHold;
    internal sealed record TrayHold(
        Lease<TrayIndicator> Indicator, Option<Lease<EtoImage>> Icon, Option<Lease<ContextMenu>> Menu) : PresenceHold;
    internal sealed record PulseHold(PulseState Prior) : PresenceHold;
    internal sealed record BadgeHold(Option<string> Prior) : PresenceHold;
```

To:
```csharp
    internal sealed record AlertHold(Lease<Notification> Card, Option<Lease<EtoImage>> Content) : PresenceHold;
    internal sealed record TrayHold(
        Lease<TrayIndicator> Indicator, Option<Lease<EtoImage>> Icon, Option<Lease<ContextMenu>> Menu) : PresenceHold;
    internal sealed record PulseHold(PulseState Prior) : PresenceHold;
    internal sealed record BadgeHold(Option<string> Prior) : PresenceHold;
```

Why: The notification retains the image reference but the current hold drops its lease.

Change: Transfer the content lease into the alert hold and release it after notification detachment.

Delta: Net 0 LOC, +1 positional member, and 0 type-count change.

# 30. Delete the presence forwarding shell

From:
`libs/dotnet/Rasm/.planning/Interaction/chrome.md` `[07]-[PRESENCE]`, `Presence`
```csharp
public static class Presence {
    public static Fin<Lease<PresenceMount>> Apply(PresenceOp operation, FaultCell faults);
}
```

To:
```csharp
// Presence DELETED
```

Why: After acquisition moves to `PresenceOp`, the class contains only one forwarding member.

Change: Remove the shell and call the operation owner directly.

Delta: Net -3 nonblank code LOC, -1 top-level type, and -1 declared member.
