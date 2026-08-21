# [APPUI_COMMANDS_AVAILABILITY]

Rasm.AppUi runs one command rail: a single `CommandRow` table is the UI's one intent-row table, and menus, toolbars, access keys, hotkeys, tray items, palette entries, deep links, and remote verbs are derivation folds over it. Command IDENTITY is not this package's — it is `Rasm.AppHost/Agent/runtime#DISPATCH_FRONT_DOOR` `CommandIntent`, and every row executes by minting one through `CommandRow.ToIntent` and crossing that page's `Run` door. The page owns the intent row shape with its payload union, the keyed arrow table and the family-row data every cross-page verb reaches the deck through, the typed availability algebra over the degradation vocabulary, the execution receipt family sealed through the receipt sink, the deck-owned search and invocation spine, and the command wire contract. The federated palette, its surface frames, and the binding editor are `Shell/palette.md`'s — presentation over this deck.

## [01]-[INDEX]

- [02]-[INTENT_TABLE]: One frozen row table, payload shapes, binding overlays, per-surface deck freeze.
- [03]-[DECK_FAMILIES]: The keyed arrow table, the row-shape policy, and the family-row data every owner roster projects through.
- [04]-[AVAILABILITY_ALGEBRA]: Typed availability inputs fold into one `CanExecute` stream.
- [05]-[EXECUTION_RECEIPTS]: Total outcome rail; receipts sealed through the sink message envelope; the one raise and the one remote entry.
- [06]-[TS_PROJECTION]: Intent, availability, invocation, and receipt wire shapes.
- [07]-[RESEARCH]

## [02]-[INTENT_TABLE]

- Owner: `CommandRow` the UI derivation row over one `Rasm.AppHost` command, carrying its nested `Availability` input struct and the `ToIntent` mint that hands identity back to its owner; `CommandPayload` `[Union]` argument shapes; `DeckFault` the direct generated `[Union]` with one `[FaultCase]` leaf per command failure; `BindingOverlay` the per-user gesture-and-alias overlay folded ahead of the freeze; `CommandDeck` per-surface frozen result carrying the row table, the normalized palette index, the chord-claimant oracle, and the gesture-contest fold.
- Cases: `CommandPayload` = None | Single | Many | Text | Fields under the locked kind literals none, single, many, text, fields — parameterized intents discriminate on payload shape, never on name suffixes; each row's `Accepts` set names its admitted kind domain, and `Admit` seals `DeckFault.PayloadRejected` before the crossing on every invocation modality, so a payload the row never admitted opens no suite transaction.
- Entry: `public static Fin<CommandDeck> Freeze(CommandComposition composition, params ReadOnlySpan<CommandRow> rows)` — identity admission accumulates every duplicate-key and duplicate-text defect through `Validation` before the first `GestureContest` refuses; one freeze per mounted surface, and the composition-time services travel as one carrier.
- Auto: the `Surfaces` predicate filters rows exactly once at freeze against the supplied `ConsumptionProfile` and the resolved `SurfaceMount`; the composition's `BindingOverlay` rebinds each surviving row's gesture and widens the index with its aliases BEFORE the contest fold runs, so a user chord collides on the same oracle a default one does; `Claimants` is the one chord-ownership read and `Contests` is that read filtered to contested chords, so `Freeze` refuses the first deterministic contest before any command materializes and the binding editor asks the identical question at assignment time.
- Receipt: `Composition.Conflict` seals the deterministic `GestureContest` through the composition-bound evidence sink immediately before `Freeze` returns `DeckFault.GestureConflict` carrying the same value; execution receipts begin only after a contest-free deck exists.
- Packages: Thinktecture.Runtime.Extensions, Avalonia, LanguageExt.Core, Rasm (kernel fault floor, `CapabilitySet`, `MonotonicTimeline`), Rasm.AppHost (project — `ConsumptionProfile`, `CommandIntent`, `CommandArguments`, `CallerModality`, `CommandTxn`), BCL inbox
- Growth: one `CommandRow` absorbs a new verb across every derived surface, one `CommandPayload` case absorbs a new argument shape, and one `BindingOverlay` row absorbs a whole named keymap; zero new surface.
- Boundary: every column here is a UI DERIVATION column — what the row presents, where it mounts, which chord claims it, which palette kinds it acts on, which schema collects its arguments — and the columns naming WHAT command are read off `Rasm.AppHost/Agent/runtime#DISPATCH_FRONT_DOOR` `CommandIntent` at `ToIntent` rather than re-declared here, because a second command identity in a package that references its owner is a strata twin whose two spellings dispatch resolves by whichever page a call site happened to cite; the locked row shape — intent key, capability requirement, availability delegate over the two-plane `Availability` input, `Option<KeyGesture>`, surface predicate, palette-kind target set, argument schema — deletes menu registries, toolbar registries, palette registries, hotkey tables, keymap files, and deep-link maps in one stroke; `CommandPayload.Many` and `CommandPayload.Fields` decode through the suite mint's `LanguageExtJsonConverterFactory` — `Seq<A>` and `HashMap<K, V>` carry no serializer-visible population hook and LanguageExt ships no converter of its own, so the `Rasm.AppHost/Runtime/ports#WIRE_LAW` factory registered before the options freeze is the one decode path for both carriers and a member-level `[JsonConverter]` beside it is the second spelling this row deletes; the intent key is simultaneously the localization string key the `Label` resolver consumes and the icon catalog key, so a label column and an icon column are the deleted forms; `Chord` is the host-agnostic Cmd/Ctrl column transform, so per-platform gesture rows are the rejected form; `Execute` delegates bind host work at composition and no case body names a host API outside its own row; `Targets` names the `PaletteKind` keys a verb acts on as a CONTEXTUAL action — keys rather than rows, because the set crosses the intent wire; `Arguments` carries the `Editing/forms#FORM_SCHEMA` schema a parameterized verb collects its own arguments through, the schema's `SubmitIntent` and this row's key being one value by construction; the kind literals discriminating `CommandPayload` and `CommandOutcome` live ONCE on their `[JsonDerivedType]` rows — `CommandKinds` projects the wire-context metadata and both `Kind` reads take that projection, so a case-to-literal `Switch` beside the annotations is the deleted form; a surface verb absent from this table is not a dead button but a screen that FAILS TO MATERIALIZE, because a tree resolves its expansion command and a strip its jump command against this frozen deck and both abort the materialize on a miss.

```csharp signature
// --- [MODELS] ---------------------------------------------------------------------------

public sealed partial record CommandRow(
    string Key,
    CommandScope Scope,
    CapabilitySet<Faculty> Requires,
    FrozenSet<string> Accepts,
    Func<CommandRow.Availability, bool> When,
    Option<KeyGesture> Gesture,
    Func<ConsumptionProfile, SurfaceMount, bool> Surfaces,
    FrozenSet<string> Targets,
    Option<FormSchema> Arguments,
    // The BODY the suite dispatch drives, never a door: the root binds this arrow into the AppHost
    // `CommandRuntime.Dispatch` fold keyed by the row's descriptor, so the work runs INSIDE the transaction
    // that vetoed, brokered, metered, and chained it rather than beside one. The token is an EXPLICIT column
    // rather than an ambient read: a synchronous kernel fold below the effect floor takes its governance as
    // parameters, so a bound body that reaches one seats it there and a long solve ends on the gesture that
    // cancelled it; that token is the transaction's own `CancelScope` spine, so the receipt and the work
    // still burning a core cannot disagree about when the command stopped.
    Func<CommandPayload, CancellationToken, IO<Unit>> Execute) {
    // Identity crosses UP, never down: the row's key IS the AppHost descriptor id, the admitted payload is
    // the argument blob, and the boot-bound tenant and correlation ride the same carrier — so this mint is
    // the one place UI vocabulary becomes suite vocabulary. It is a FACTORY rather than a stored member
    // because two of the three columns are invocation facts: a payload exists only once a modality supplied
    // one, and the caller modality is what distinguishes the same row pressed by an operator from the same
    // row replayed off the wire.
    public CommandIntent ToIntent(CommandPayload payload, CommandComposition composition, CallerModality caller) =>
        CommandIntent.Of(
            Key,
            new CommandArguments(
                JsonSerializer.SerializeToElement(payload, composition.Wire),
                composition.Tenant,
                composition.Correlation),
            caller);

    // TWO capability planes, both total over the roster: the level is what the process still SERVES and
    // Reach is what the mounted surface structurally TOUCHES (`Shell/hosts` `SurfaceSession.Reach`) —
    // `DegradationLevel.Full` retains `Faculty.HostDocument` on every healthy process, so a level-only gate
    // admitted every host-targeting verb against a standalone shell that owns no document.
    public readonly record struct Availability(
        DegradationLevel Level, CapabilitySet<Faculty> Reach, bool Valid, SelectionSnapshot Selection);

    public bool Admits(Availability input) =>
        input.Level.Retains.AdmitsAll(Requires) && input.Reach.AdmitsAll(Requires) && When(input);

    // The one payload-admission fold: every invocation modality routes through Run, so a syntactically
    // valid payload outside the row's admitted kind domain seals PayloadRejected before Execute.
    public Fin<CommandPayload> Admit(CommandPayload payload) =>
        Accepts.Contains(payload.Kind)
            ? Fin.Succ(payload)
            : Fin.Fail<CommandPayload>(new DeckFault.PayloadRejected($"{Key}: '{payload.Kind}' outside the row's admitted domain"));

    // The argument fold: the schema accumulates every visible field rule and the admitted state lowers onto
    // the one erased payload case, so a half-filled form refuses HERE with every failure at once.
    public Fin<CommandPayload> Compose(FormState state) =>
        Arguments.Match(
            Some: schema => schema.Admit(state).ToFin().Map(static admitted => (CommandPayload)new CommandPayload.Fields(admitted.Values)),
            None: () => Fin.Fail<CommandPayload>(new DeckFault.PayloadRejected($"{Key}: carries no argument schema")));

    // Keys rather than palette rows: the target set crosses the intent wire, and the deck never imports the
    // palette vocabulary that composes it.
    public bool Acts(string paletteKind) => Targets.Contains(paletteKind);
}

[SmartEnum<string>]
public sealed partial class CommandScope {
    public static readonly CommandScope Global = new("global");
    public static readonly CommandScope Screen = new("screen");
    public static readonly CommandScope Viewport = new("viewport");
    public static readonly CommandScope Dialog = new("dialog");
}

// `Kinds` canonicalizes to a distinct ordinal-sorted `Seq`, so structural sequence equality IS set equality
// and the gate's `DistinctUntilChanged` never re-fires on a reference-fresh identical selection.
[ComplexValueObject]
public readonly partial struct SelectionSnapshot {
    public int Count { get; }
    public Seq<string> Kinds { get; }

    public static readonly SelectionSnapshot None = Create(0, Seq<string>());

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int count, ref Seq<string> kinds) {
        kinds = kinds.Distinct().OrderBy(static kind => kind, StringComparer.Ordinal).ToSeq();
        validationError = count >= kinds.Count && (count > 0 || kinds.Count == 0)
            ? validationError
            : new ValidationError($"selection count {count} cannot carry {kinds.Count} kinds");
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(CommandPayload.None), "none")]
[JsonDerivedType(typeof(CommandPayload.Single), "single")]
[JsonDerivedType(typeof(CommandPayload.Many), "many")]
[JsonDerivedType(typeof(CommandPayload.Text), "text")]
[JsonDerivedType(typeof(CommandPayload.Fields), "fields")]
public abstract partial record CommandPayload {
    private CommandPayload() { }
    public sealed record None : CommandPayload;
    public sealed record Single(string Id) : CommandPayload;
    public sealed record Many(Seq<string> Ids) : CommandPayload;
    public sealed record Text(string Value) : CommandPayload;

    // Values stay ERASED `JsonElement` exactly as the form state stores them, so the payload is the schema's
    // own admitted output; the field key is the wire key and the row's schema is the only decoder.
    public sealed record Fields(HashMap<string, JsonElement> Values) : CommandPayload;

    public string Kind => CommandKinds.Payload[GetType()];
}

// Polymorphic metadata is the ONE kind roster for both command unions: the `[JsonDerivedType]` rows are the
// only place a case names its literal, so the wire discriminator, each `Kind` read, and the TS union arms
// share one vocabulary. `.Default` is legal here alone — attribute metadata is identical across context
// instances and type init precedes the composition binding of the runtime options.
internal static class CommandKinds {
    internal static readonly FrozenDictionary<Type, string> Payload = Rows(AppUiWireContext.Default.CommandPayload);
    internal static readonly FrozenDictionary<Type, string> Outcome = Rows(AppUiWireContext.Default.CommandOutcome);

    static FrozenDictionary<Type, string> Rows(JsonTypeInfo info) =>
        info.PolymorphismOptions is { } options
            ? toSeq(options.DerivedTypes)
                .Choose(static row => row.TypeDiscriminator is string kind ? Some((row.DerivedType, kind)) : None)
                .ToFrozenDictionary(static row => row.DerivedType, static row => row.kind)
            : FrozenDictionary<Type, string>.Empty;
}

// The per-user binding overlay: one named set of gesture rebinds and label aliases, folded over the authored
// rows AHEAD of the freeze so the authored table stays pure data and every downstream reader sees one deck.
// A present gesture entry mapping to `None` is an explicit UNBIND, an absent key keeps the row's own chord,
// so "no opinion" and "deliberately unbound" are different values rather than one missing entry.
public sealed record BindingOverlay(
    string SetKey,
    string LabelKey,
    HashMap<string, Option<KeyGesture>> Gestures,
    HashMap<string, Seq<string>> Aliases) {
    public static readonly BindingOverlay Defaults = new(
        "defaults", "shortcuts.set.defaults",
        HashMap<string, Option<KeyGesture>>(),
        HashMap<string, Seq<string>>());

    public CommandRow Rebind(CommandRow row) =>
        Gestures.Find(row.Key).Match(Some: user => row with { Gesture = user }, None: () => row);

    // Every text a query can reach this row through: the resolved label first, then the user's aliases,
    // sharing one normalization domain because the label resolver is the deck's own.
    public Seq<string> Texts(CommandRow row, Func<string, string> label) =>
        Seq(label(row.Key)) + Aliases.Find(row.Key).IfNone(Seq<string>());

    public bool Rebound(string key) => Gestures.ContainsKey(key);

    public BindingOverlay With(string key, Option<KeyGesture> gesture) =>
        this with { Gestures = Gestures.AddOrUpdate(key, gesture) };

    public BindingOverlay Without(string key) => this with { Gestures = Gestures.Remove(key) };
}

// The persisted shortcut section, shaped exactly as the theme section is: a named active set plus the rows
// the user authored, so a swap is one key write and a reload re-admits every row through the same freeze.
public sealed record ShortcutPolicy(string ActiveSet, Seq<BindingOverlay> Sets) {
    public const string Section = nameof(ShortcutPolicy);

    public static readonly ShortcutPolicy Default = new(BindingOverlay.Defaults.SetKey, Seq(BindingOverlay.Defaults));

    public BindingOverlay Active =>
        Sets.Find(row => string.Equals(row.SetKey, ActiveSet, StringComparison.Ordinal)).IfNone(BindingOverlay.Defaults);
}

// Contest evidence: one spelling serves the sealed receipt, the fault detail, and the editor's refusal.
public sealed record GestureContest(CommandScope Scope, string Gesture, Seq<string> Keys) {
    public string Spelled => $"{Scope.Key}:{Gesture}:{string.Join(',', Keys)}";
}

// --- [ERRORS] ---------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DeckFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.UiCommand;
    private DeckFault(string detail) { Detail = detail; }
    public string Detail { get; }
    public override string Message => $"command/{Band.Key}: {Detail}";
    [FaultCase(0)]
    public sealed partial record DuplicateRow(string Detail) : DeckFault(Detail);
    [FaultCase(1)]
    public sealed partial record UnknownIntent(string Key) : DeckFault(Key);
    [FaultCase(2)]
    public sealed partial record GestureConflict(GestureContest Contest) : DeckFault(Contest.Spelled);
    [FaultCase(3)]
    public sealed partial record PayloadRejected(string Detail) : DeckFault(Detail);
    [FaultCase(4)]
    public sealed partial record UnknownSet(string Key) : DeckFault(Key);
    [FaultCase(5)]
    public sealed partial record ProviderFailed(string Detail) : DeckFault(Detail);
}

// --- [SERVICES] -------------------------------------------------------------------------

public sealed record CommandComposition(
    ConsumptionProfile Profile,
    SurfaceMount Mount,
    string SurfaceKey,
    BindingOverlay Overlay,
    Func<KeyGesture, KeyGesture> Chord,
    Func<string, string> Label,
    IObservable<CommandRow.Availability> Inputs,
    Func<CommandRow.Availability> Snapshot,
    IScheduler Scheduler,
    MonotonicTimeline Line,
    CorrelationId Correlation,
    TenantContext Tenant,
    ReceiptSinkPort Sink,
    Func<GestureContest, Unit> Conflict,
    // The ONE execution door this package reaches: the root binds it to `Rasm.AppHost/Agent/runtime`
    // `CommandDispatch.Run` and reads the returned receipt's `Txn`, so the veto rail fires, the caller
    // modality records at the mediation, the meter charges, and the entry chains — none of which a
    // UI-local dispatcher could do. A composition-bound delegate rather than an imported static because
    // the mounted surface, not this page, owns which `DispatchRuntime` its commands cross.
    Func<CommandIntent, CancellationToken, IO<CommandTxn>> Cross,
    JsonSerializerOptions Wire);

public sealed record CommandDeck(
    FrozenDictionary<string, CommandRow> Rows,
    FrozenDictionary<string, string> Index,
    CommandComposition Composition) {
    public static Fin<CommandDeck> Freeze(
        CommandComposition composition,
        params ReadOnlySpan<CommandRow> rows) =>
        Admitted(
            toSeq(rows.ToArray())
                .Filter(row => row.Surfaces(composition.Profile, composition.Mount))
                .Map(composition.Overlay.Rebind),
            composition)
            .ToFin()
            .Map(admitted => new CommandDeck(
                admitted.Rows.Map(static row => KeyValuePair.Create(row.Key, row)).ToFrozenDictionary(StringComparer.Ordinal),
                admitted.Index.Map(static entry => KeyValuePair.Create(entry.Text, entry.Key)).ToFrozenDictionary(StringComparer.Ordinal),
                composition))
            .Bind(deck => deck.Contests().Head.Match(
                Some: contest => (deck.Composition.Conflict(contest),
                    Fin.Fail<CommandDeck>(new DeckFault.GestureConflict(contest))).Item2,
                None: () => Fin.Succ(deck)));

    // The ONE chord-ownership read: the freeze refuses a contested default and the binding editor a contested
    // assignment through it, so the two answers cannot drift; scope-local, because the freeze's own law lets a
    // Global row and a Screen row share one chord.
    public Seq<string> Claimants(CommandScope scope, KeyGesture gesture) =>
        toSeq(Rows.Values
            .Where(row => row.Scope == scope && row.Gesture.Map(Composition.Chord).Filter(bound => bound.Equals(gesture)).IsSome)
            .Select(static row => row.Key)
            .Order(StringComparer.Ordinal));

    // The one keyed read every fold takes: `FrozenDictionary` answers through an out-parameter probe, so the
    // lift onto `Option` lands once here rather than at each call site.
    public Option<CommandRow> Row(string key) =>
        Rows.TryGetValue(key, out CommandRow? row) ? Optional(row) : None;

    public Seq<GestureContest> Contests() =>
        toSeq(toSeq(Rows.Values)
            .Bind(row => row.Gesture.Map(Composition.Chord).ToSeq().Map(gesture => (row.Scope, Gesture: gesture)))
            .Distinct()
            .Map(bound => new GestureContest(bound.Scope, bound.Gesture.ToString(), Claimants(bound.Scope, bound.Gesture)))
            .Filter(static contest => contest.Keys.Length > 1)
            .OrderBy(static contest => contest.Scope.Key, StringComparer.Ordinal)
            .ThenBy(static contest => contest.Gesture, StringComparer.Ordinal));

    // Identity admission covers the WHOLE searchable text set: an alias colliding with another row's label
    // resolves one query to two verbs — the same defect a duplicate label is. Both axes accumulate, so a boot
    // refusal names every colliding key AND every colliding text rather than one merged sentence.
    private static Validation<Error, (Seq<CommandRow> Rows, Seq<(string Text, string Key)> Index)> Admitted(
        Seq<CommandRow> rows, CommandComposition composition) {
        Seq<(string Text, string Key)> index = rows.Bind(row => composition.Overlay.Texts(row, composition.Label)
            .Map(text => (Text: text.ToLowerInvariant(), row.Key)));
        Seq<string> keys = rows.Map(static row => row.Key).Collisions(static key => key);
        Seq<string> texts = index.Map(static entry => entry.Text).Collisions(static text => text);
        return (
            keys.IsEmpty
                ? Validation<Error, Unit>.Success(unit)
                : Validation<Error, Unit>.Fail(new DeckFault.DuplicateRow($"intent key: {string.Join(',', keys)}")),
            texts.IsEmpty
                ? Validation<Error, Unit>.Success(unit)
                : Validation<Error, Unit>.Fail(new DeckFault.DuplicateRow($"palette label or alias: {string.Join(',', texts)}")))
            .Apply((_, _) => (rows, index)).As();
    }
}
```

## [03]-[DECK_FAMILIES]

- Owner: `DeckArrows` — the keyed arrow table the composition registers, one entry per intent constant, with the payload-shape adapters every registration composes; `RowShape` — the admitted-payload-and-gate policy row every family row shares; `FamilyRow` — the ONE registration shape a verb is, carrying `Mint`, the one `CommandRow` construction anywhere in the package; `DeckRows` — the coverage-proving fold plus the family data every owner roster projects through and the owner-state families that close over live owners.
- Entry: `DeckArrows.Of(params (string Key, Func<CommandPayload, CancellationToken, IO<Unit>> Run)[])` — duplicate registrations refuse; `DeckArrows.Bound(key)` — the one read, refusing `DeckFault.UnknownIntent`; `DeckRows.Mint(DeckArrows arrows, Seq<FamilyRow> rows)` — the coverage proof and the row mint in one traverse, so a family row whose key no arrow binds refuses AT COMPOSITION rather than at first dispatch; `FamilyRow.Mint(run)` — the one `CommandRow` construction, which the owner-state families and the editor verb take directly with the arrow closing over their live owner.
- Law: a verb is ONE `FamilyRow` beside ONE arrow registration — the reachability lands as data, the behaviour stays the owner's own arrow, and no signature anywhere grows a column; a family whose keys generate off a bounded vocabulary (`TransportVerb`, `SessionAction`, `BakeVerb`, `CellVerb`, `ConflictIntent`, `VisibilityAction`) projects its roster's own `Items`, so a new roster row reaches its chord, its palette entry, and its journal replay with no edit here.
- Auto: `RowShape` closes the admitted-payload-and-gate product — bare, open, keyed, named, addressed, marked, fielded — so a family row states its shape as one row read and a bespoke `accepts`/`when` pair survives only as an explicit override with its reason at the site; the `media.*` roster stays composition-bound by structure — no media control binds a `media.*` key as its `IntentBinding.Command`, the transport bar's own segments carrying the `TransportVerb` keys this table already freezes.
- Packages: Thinktecture.Runtime.Extensions, Avalonia, LanguageExt.Core, BCL inbox
- Growth: one `FamilyRow` plus one arrow registration absorbs a new verb; one roster row at its owner absorbs a new generated verb with zero edits here; one `RowShape` row absorbs a new payload-and-gate shape.
- Boundary: `FamilyRow` is the ONE registration shape and `FamilyRow.Mint` the ONE `CommandRow` construction — a positional `CommandRow` construction and a full-control mint taking a raw accepts array beside a hand predicate are the two deleted forms, because each re-declares the columns the policy row already decides and neither is reachable by a column the row later grows; `ViewportVerbs.Visibility` keys derive as `viewport.<action.Key>` from the `Render/pipeline` `VisibilityAction` roster because that roster predates intent keys — every OTHER family key is its owner's own declared constant, so one act keeps one key and a re-spelled stem drifts nothing; the reveal family stays three HAND rows with stated provenance — the Command kind self-invokes and the Issue kind's act is `BoardSurface.JumpIntent`, already a Collab row, so a derivation over `PaletteKind.Reveal` would mint the borrowed key twice and refuse at the freeze; the history and graph planes arrive WHOLE off their own row projections (`HistoryIntents.Rows`, `GraphVerbs.Rows`) and the activity and workspace families close over their live owners, so owner-state behaviour never rides the arrow table where nothing could prove its coverage.

```csharp signature
// --- [TABLES] ---------------------------------------------------------------------------

// The admitted-payload-and-gate policy every family row shares. `Marked` is the selection-gated shape;
// `Open` is the reveal-or-recall shape whose bare arm shows the roster and whose keyed arm recalls one row;
// `Keyed` addresses exactly one subject, so a bare press refuses where `Open` would list; `Named` is the
// shape whose payload IS the name, which is how a row over live user-minted state stays one frozen row.
// The roster is POLICY, not the payload union's power set: a subset no family names would be a row nothing
// reads, and a family whose product no row spells states its `Accepts`/`When` override at the site instead.
[SmartEnum<string>]
public sealed partial class RowShape {
    public static readonly RowShape Bare      = new("bare", K("none"), static _ => true);
    public static readonly RowShape Open      = new("open", K("none", "single"), static _ => true);
    public static readonly RowShape Keyed     = new("keyed", K("single"), static _ => true);
    public static readonly RowShape Named     = new("named", K("text"), static _ => true);
    public static readonly RowShape Addressed = new("addressed", K("single", "fields"), static _ => true);
    public static readonly RowShape Marked    = new("marked", K("single", "many"), static input => input.Selection.Count > 0);
    public static readonly RowShape Fielded   = new("fielded", K("fields"), static _ => true);

    public FrozenSet<string> Accepts { get; }

    [UseDelegateFromConstructor]
    public partial bool When(CommandRow.Availability input);

    private static FrozenSet<string> K(params string[] kinds) => kinds.ToFrozenSet(StringComparer.Ordinal);
}

// One reachability row: key, scope, shape, and the optional columns a family occasionally overrides. The
// override columns exist for the roster-generated families whose rows carry their own accepts and chord.
// This is the ONE registration shape and `Mint` the ONE place a `CommandRow` is constructed, so the two
// columns no family varies — the surface predicate and the palette targets — are spelled once, and a column
// landing on `CommandRow` reaches every family through this one body rather than through every mint site.
public sealed record FamilyRow(
    string Key,
    CommandScope Scope,
    RowShape Shape,
    Option<KeyGesture> Gesture = default,
    Option<FormSchema> Arguments = default,
    Option<CapabilitySet<Faculty>> Requires = default,
    Option<Func<CommandRow.Availability, bool>> When = default,
    Option<FrozenSet<string>> Accepts = default) {
    public CommandRow Mint(Func<CommandPayload, CancellationToken, IO<Unit>> run) =>
        new(Key, Scope, Requires.IfNone(CapabilitySet<Faculty>.None), Accepts.IfNone(() => Shape.Accepts),
            When.IfNone(() => Shape.When), Gesture, static (_, _) => true, FrozenSet<string>.Empty, Arguments, run);
}

// The keyed arrow table the composition registers: one entry per intent constant, so a verb stops editing a
// record signature and starts adding one registration. The adapters carry the two payload admissions every
// registration otherwise re-spells.
public sealed record DeckArrows(HashMap<string, Func<CommandPayload, CancellationToken, IO<Unit>>> Rows) {
    public static Fin<DeckArrows> Of(params ReadOnlySpan<(string Key, Func<CommandPayload, CancellationToken, IO<Unit>> Run)> rows) {
        Seq<(string Key, Func<CommandPayload, CancellationToken, IO<Unit>> Run)> declared = toSeq(rows.ToArray());
        Seq<string> collided = declared.Map(static row => row.Key).Collisions(static key => key);
        return collided.IsEmpty
            ? Fin.Succ(new DeckArrows(declared.ToHashMap(static row => row.Key, static row => row.Run)))
            : Fin.Fail<DeckArrows>(new DeckFault.DuplicateRow($"arrow: {string.Join(',', collided)}"));
    }

    public Fin<Func<CommandPayload, CancellationToken, IO<Unit>>> Bound(string key) =>
        Rows.Find(key).ToFin(new DeckFault.UnknownIntent(key));

    public static Func<CommandPayload, CancellationToken, IO<Unit>> Bare(Func<IO<Unit>> run) => (_, _) => run();

    public static Func<CommandPayload, CancellationToken, IO<Unit>> Addressed(string key, Func<string, IO<Unit>> run) =>
        (payload, _) => payload is CommandPayload.Single single
            ? run(single.Id)
            : IO.fail<Unit>(new DeckFault.PayloadRejected($"{key}: subject absent"));

    public static Func<CommandPayload, CancellationToken, IO<Unit>> Railed(Func<CommandPayload, IO<Fin<Unit>>> run) =>
        (payload, _) => run(payload).Bind(static outcome => outcome.Match(Succ: static _ => IO.pure(unit), Fail: IO.fail<Unit>));
}

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class DeckRows {
    // Coverage and mint in ONE traverse: a family row whose key no arrow binds refuses at composition on the
    // same UnknownIntent rail a bad deep link takes, so an unreachable verb cannot survive to its first press.
    public static Fin<Seq<CommandRow>> Mint(DeckArrows arrows, Seq<FamilyRow> rows) =>
        rows.Traverse(row => arrows.Bound(row.Key).Map(row.Mint)).As();

    // The named-view family: traversal, orientation, projection, bookmarks, and measure all move the ONE
    // `Render/viewpoint#VIEW_REGISTRY` registry; the bookmark row discriminates on its own payload.
    public static Seq<FamilyRow> View() => Seq(
        new FamilyRow(ViewChrome.BackKey, CommandScope.Viewport, RowShape.Bare),
        new FamilyRow(ViewChrome.ForwardKey, CommandScope.Viewport, RowShape.Bare),
        new FamilyRow(ViewChrome.OrientationKey, CommandScope.Viewport, RowShape.Bare),
        new FamilyRow(ViewChrome.ProjectionKey, CommandScope.Viewport, RowShape.Bare),
        new FamilyRow(ViewChrome.BookmarksKey, CommandScope.Viewport, RowShape.Open),
        new FamilyRow(ViewChrome.MeasureKey, CommandScope.Viewport, RowShape.Bare));

    // Presence is the gate rather than a capability row: a surface that never opened a session registers no
    // arrows for these keys, and `XrReviewVerb.Bound` refuses a key the frozen deck does not carry.
    public static Seq<FamilyRow> Immersive() => Seq(
        new FamilyRow(XrReviewVerb.CaptureIssueIntent, CommandScope.Viewport, RowShape.Bare),
        new FamilyRow(XrReviewVerb.PassthroughIntent, CommandScope.Viewport, RowShape.Bare),
        new FamilyRow(XrReviewVerb.RecenterIntent, CommandScope.Viewport, RowShape.Bare));

    public static Seq<FamilyRow> Transport() =>
        toSeq(TransportVerb.Items).Map(static verb => new FamilyRow(verb.IntentKey, CommandScope.Screen, RowShape.Bare));

    public static Seq<FamilyRow> Collab() => Seq(
        new FamilyRow(PresenceFollow.ReleaseIntent, CommandScope.Global, RowShape.Bare),
        // The connection verbs are a bounded roster and project as one — the two per-const rows were the
        // transcription this table's own law names the deleted form.
        .. toSeq(ConnectionVerb.Items).Map(static verb => new FamilyRow(verb.Key, CommandScope.Global, RowShape.Bare)),
        new FamilyRow(DiffSurface.LayoutIntent, CommandScope.Screen, RowShape.Bare),
        new FamilyRow(DiffSurface.NextIntent, CommandScope.Screen, RowShape.Bare),
        new FamilyRow(DiffSurface.PreviousIntent, CommandScope.Screen, RowShape.Bare),
        new FamilyRow(DiffSurface.RevealIntent, CommandScope.Screen, RowShape.Addressed),
        new FamilyRow(CompareSession.JumpIntent, CommandScope.Screen, RowShape.Addressed),
        new FamilyRow(CompareSession.LegendIntent, CommandScope.Screen, RowShape.Bare),
        new FamilyRow(BoardSurface.DropIntent, CommandScope.Screen, RowShape.Addressed),
        new FamilyRow(BoardSurface.AssignIntent, CommandScope.Screen, RowShape.Addressed),
        new FamilyRow(BoardSurface.LabelIntent, CommandScope.Screen, RowShape.Addressed),
        new FamilyRow(BoardSurface.AttachIntent, CommandScope.Screen, RowShape.Addressed),
        new FamilyRow(BoardSurface.JumpIntent, CommandScope.Global, RowShape.Addressed),
        new FamilyRow(PresenterStrip.PreviousIntent, CommandScope.Global, RowShape.Bare),
        new FamilyRow(PresenterStrip.NextIntent, CommandScope.Global, RowShape.Bare),
        new FamilyRow(PresenterStrip.PeekIntent, CommandScope.Global, RowShape.Bare),
        new FamilyRow(PresenterStrip.SessionKey, CommandScope.Global, RowShape.Bare),
        new FamilyRow(AudienceChrome.FollowIntent, CommandScope.Global, RowShape.Bare),
        new FamilyRow(AudienceChrome.UnfollowIntent, CommandScope.Global, RowShape.Bare),
        new FamilyRow(SeatCluster.RevealIntent, CommandScope.Global, RowShape.Addressed))
        + toSeq(SessionAction.Items).Map(static action => new FamilyRow(action.Key, CommandScope.Global, RowShape.Addressed));

    public static Seq<FamilyRow> Analysis() => Seq(
        new FamilyRow(AnalysisLayers.ToggleIntent, CommandScope.Screen, RowShape.Addressed),
        new FamilyRow(AnalysisLayers.DropIntent, CommandScope.Screen, RowShape.Addressed),
        new FamilyRow(AnalysisLayers.RaiseIntent, CommandScope.Screen, RowShape.Addressed),
        new FamilyRow(AnalysisLayers.ExpandIntent, CommandScope.Screen, RowShape.Addressed),
        new FamilyRow(AnalysisLayers.DimIntent, CommandScope.Screen, RowShape.Addressed),
        new FamilyRow(RunQueueSurface.AdoptIntent, CommandScope.Global, RowShape.Addressed),
        new FamilyRow(ProbeChannel.PinIntent, CommandScope.Screen, RowShape.Addressed),
        new FamilyRow(ProbeChannel.ClearIntent, CommandScope.Screen, RowShape.Bare),
        new FamilyRow(ProbeChannel.ExportIntent, CommandScope.Screen, RowShape.Bare),
        new FamilyRow(CompareBoard.SwapIntent, CommandScope.Screen, RowShape.Addressed),
        new FamilyRow(CompareBoard.PinIntent, CommandScope.Screen, RowShape.Addressed),
        new FamilyRow(CompareBoard.SheetIntent, CommandScope.Screen, RowShape.Bare),
        new FamilyRow(ContextChannel.ScrubIntent, CommandScope.Screen, RowShape.Addressed),
        new FamilyRow(ContextChannel.GrainIntent, CommandScope.Screen, RowShape.Addressed),
        new FamilyRow(ContextChannel.ScenarioIntent, CommandScope.Screen, RowShape.Addressed))
        + toSeq(BakeVerb.Items).Map(static verb => new FamilyRow(verb.Intent, CommandScope.Screen, RowShape.Addressed));

    // `Editing/forms#SELECTION_MODEL` owns the sets and states their recall verbs are table intents.
    public static Seq<FamilyRow> SelectionSets() => Seq(
        new FamilyRow(SelectionSet.ListIntent, CommandScope.Global, RowShape.Bare),
        new FamilyRow(SelectionSet.ApplyIntent, CommandScope.Global, RowShape.Addressed),
        new FamilyRow(SelectionSet.RenameIntent, CommandScope.Global, RowShape.Addressed),
        new FamilyRow(SelectionSet.DropIntent, CommandScope.Global, RowShape.Addressed),
        new FamilyRow(SelectionSet.SimilarIntent, CommandScope.Global, RowShape.Marked));

    // HAND rows with stated provenance: Command self-invokes and Issue's act is `BoardSurface.JumpIntent`
    // (already a Collab row), so deriving over `PaletteKind.Reveal` would mint the borrowed key twice.
    public static Seq<FamilyRow> Reveal() => Seq(
        new FamilyRow(PaletteKind.DocumentRevealIntent, CommandScope.Global, RowShape.Addressed),
        new FamilyRow(PaletteKind.ElementRevealIntent, CommandScope.Global, RowShape.Addressed),
        new FamilyRow(PaletteKind.RouteRevealIntent, CommandScope.Global, RowShape.Addressed));

    // The three document verbs name `Faculty.HostDocument`, so they fold unavailable both on a degraded
    // process and on a mount whose surface never reached a document — the pair the two-plane gate answers.
    public static Seq<FamilyRow> Surfaces() => Seq(
        new FamilyRow(RunQueueSurface.ExpandIntent, CommandScope.Screen, RowShape.Addressed),
        new FamilyRow(RunQueueSurface.CancelIntent, CommandScope.Screen, RowShape.Addressed),
        new FamilyRow(RunQueueSurface.RetryIntent, CommandScope.Screen, RowShape.Addressed),
        new FamilyRow(CodePane.JumpVerb, CommandScope.Screen, RowShape.Addressed),
        new FamilyRow(CoachMarks.DismissVerb, CommandScope.Global, RowShape.Addressed),
        new FamilyRow(FaultReport.SubmitVerb, CommandScope.Screen, RowShape.Bare),
        new FamilyRow(FaultReport.RestoreVerb, CommandScope.Screen, RowShape.Bare),
        new FamilyRow(FaultReport.DiscardVerb, CommandScope.Screen, RowShape.Bare),
        new FamilyRow(ProductPrograms.OpenVerb, CommandScope.Global, RowShape.Addressed, Requires: Some(CapabilitySet<Faculty>.Of(Faculty.HostDocument))),
        new FamilyRow(ProductPrograms.SaveVerb, CommandScope.Global, RowShape.Addressed, Requires: Some(CapabilitySet<Faculty>.Of(Faculty.HostDocument))),
        new FamilyRow(ProductPrograms.RecoverVerb, CommandScope.Global, RowShape.Addressed, Requires: Some(CapabilitySet<Faculty>.Of(Faculty.HostDocument))),
        new FamilyRow(SettingsSurface.ResetRowVerb, CommandScope.Screen, RowShape.Addressed),
        new FamilyRow(SettingsSurface.ResetSectionVerb, CommandScope.Screen, RowShape.Addressed));

    // The seven cell rows generate off `CellVerb`; the cell rides the PAYLOAD because the deck freezes
    // before any cell exists, which is why every one of them is addressed.
    public static Seq<FamilyRow> Documents() => Seq(
        new FamilyRow(ResultsPresentation.ExpandIntent, CommandScope.Screen, RowShape.Addressed),
        new FamilyRow(ExportForm.RunIntent, CommandScope.Screen, RowShape.Addressed),
        new FamilyRow(ExportForm.OpenIntent, CommandScope.Global, RowShape.Addressed),
        new FamilyRow(ExportForm.RevealIntent, CommandScope.Global, RowShape.Addressed))
        + toSeq(CellVerb.Items).Map(static verb => new FamilyRow(verb.Intent, CommandScope.Screen, RowShape.Addressed));

    // The conflict family generates off `Editing/inspector#CONFLICT_RESOLUTION`'s non-generic roster; each
    // row carries its own admitted payload domain and default chord as override columns.
    public static Seq<FamilyRow> Conflict() =>
        toSeq(ConflictIntent.Items).Map(static row => new FamilyRow(
            row.Key, CommandScope.Dialog, RowShape.Addressed,
            Gesture: row.Chord, Accepts: Some(row.Accepts.ToFrozenSet(StringComparer.Ordinal))));

    // Keys derive as `viewport.<action.Key>` because the `VisibilityAction` roster predates intent keys —
    // the one derived-key family; reset admits none and stays always available.
    public static Seq<FamilyRow> Visibility() =>
        toSeq(VisibilityAction.Items).Map(static action => new FamilyRow(
            $"viewport.{action.Key}", CommandScope.Viewport,
            action == VisibilityAction.Reset ? RowShape.Bare : RowShape.Marked));

    // --- [OWNER_STATE_FAMILIES] — the arrow closes over a live owner, so these bind their body AT the mint
    // instead of registering it in the composition-time arrow table. The registration shape is still the one
    // `FamilyRow`, so an owner-state row carries the same declared shape, gate, and chord columns every
    // arrow-table row does and the coverage a keyed table would prove is the closure itself.

    public static Seq<CommandRow> Graph(
        IDrawingNode drawing, GraphCamera camera, GraphFind find, Func<Seq<GraphNodeRow>> selected) =>
        GraphVerbs.Rows(drawing, camera, find, selected).Add(GraphVerbs.Jump(camera));

    public static Seq<CommandRow> History(
        EditHistory history,
        Func<RevertDirection, CancellationToken, IO<Unit>> turn,
        Func<RevertOrdinal, CancellationToken, IO<Unit>> jump,
        Func<double, Fin<RevertOrdinal>> ordinalOf,
        Func<CommandPayload, CancellationToken, IO<Unit>> timelineExpand) =>
        HistoryIntents.Rows(history, turn, jump, ordinalOf)
            .Add(new FamilyRow(TimelineSurface.ExpandVerb, CommandScope.Screen, RowShape.Addressed).Mint(timelineExpand));

    // `activity.clear` stays available under a running entry because the fold itself SKIPS running rows.
    public static Seq<CommandRow> Activity(ActivityCenter centre, Func<IO<Unit>> open) => Seq(
        new FamilyRow(ActivityCenter.OpenKey, CommandScope.Global, RowShape.Bare,
            Gesture: Some(new KeyGesture(Key.I, KeyModifiers.Control | KeyModifiers.Shift)))
            .Mint(DeckArrows.Bare(open)),
        new FamilyRow(ActivityCenter.ClearKey, CommandScope.Global, RowShape.Bare,
            When: Some<Func<CommandRow.Availability, bool>>(_ => centre.Unread > 0 || centre.Rows.Length > 0))
            .Mint(DeckArrows.Bare(() => IO.lift(() => ignore(centre.Clear())))));

    // `Enter` and `Reset` address a workspace by key so a palette hit, a chord, and a remote caller name one
    // row and an unknown key refuses on the nav band; `Save` captures whatever the live surface holds.
    public static Seq<CommandRow> Workspace(WorkspaceCell cell, Func<IO<Unit>> save) => Seq(
        Entered(Workspaces.EnterVerb, cell, static (workspace, row) => workspace.Enter(row)),
        new FamilyRow(Workspaces.SaveVerb, CommandScope.Global, RowShape.Bare,
            Gesture: Some(new KeyGesture(Key.S, KeyModifiers.Control | KeyModifiers.Alt)))
            .Mint(DeckArrows.Bare(save)),
        Entered(Workspaces.ResetVerb, cell, static (workspace, row) => workspace.Reset(row)));

    static CommandRow Entered(
        string key, WorkspaceCell cell, Func<WorkspaceCell, WorkspaceRow, IO<Fin<Seq<RouteRestoreFact>>>> run) =>
        new FamilyRow(key, CommandScope.Global, RowShape.Keyed).Mint(
            DeckArrows.Railed(payload => payload is CommandPayload.Single addressed
                ? Workspaces.Find(addressed.Id).Match(
                    Succ: row => run(cell, row).Map(static outcome => outcome.Map(static _ => unit)),
                    Fail: static error => IO.pure(Fin.Fail<Unit>(error)))
                : IO.pure(Fin.Fail<Unit>(new DeckFault.PayloadRejected($"{key}: workspace key absent")))));
}
```

Every family binds keys its own owner declares, so the deck states reachability and the owner states behaviour:

| [INDEX] | [FAMILY]                   | [KEY_OWNER]                                                           | [DECK_PROJECTION]          |
| :-----: | :------------------------- | :-------------------------------------------------------------------- | :------------------------- |
|  [01]   | viewport.*                 | `Render/pipeline` `VisibilityAction`                                  | `DeckRows.Visibility`      |
|  [02]   | view.*                     | `Render/pipeline` `ViewChrome`                                        | `DeckRows.View`            |
|  [03]   | xr.* review.*              | `Render/immersive` `XrReviewVerb`                                     | `DeckRows.Immersive`       |
|  [04]   | transport.*                | `Render/animation` `TransportVerb.IntentKey`                          | `DeckRows.Transport`       |
|  [05]   | graph.*                    | `Editing/graph` `GraphVerbs`                                          | `DeckRows.Graph`           |
|  [06]   | collab.*                   | `Collab/presence` `PresenceFollow`; `Collab/session` `ConnectionVerb` | `DeckRows.Collab`          |
|  [07]   | compare.*                  | `Collab/compare` `DiffSurface`, `CompareSession`                      | `DeckRows.Collab`          |
|  [08]   | issue.*                    | `Collab/issues` `BoardSurface`                                        | `DeckRows.Collab`          |
|  [09]   | tour.*                     | `Collab/tour` `PresenterStrip`                                        | `DeckRows.Collab`          |
|  [10]   | session.*                  | `Collab/session` `SessionAction`, `SeatCluster`                       | `DeckRows.Collab`          |
|  [11]   | analysis.*                 | `Analysis/layers`, `compare`, `context`                               | `DeckRows.Analysis`        |
|  [12]   | selection.*                | `Editing/forms` `SelectionSet`                                        | `DeckRows.SelectionSets`   |
|  [13]   | activity.*                 | `Shell/dialogs` `ActivityCenter`                                      | `DeckRows.Activity`        |
|  [14]   | workspace.*                | `Shell/navigation` `Workspaces`                                       | `DeckRows.Workspace`       |
|  [15]   | media.*                    | `Document/media` `MediaCommand`                                       | composition-bound payloads |
|  [16]   | history.*                  | `Editing/history` `HistoryIntents`, `TimelineSurface`                 | `DeckRows.History`         |
|  [17]   | run.queue.*                | `Shell/screens` `RunQueueSurface`                                     | `DeckRows.Surfaces`        |
|  [18]   | product.* report.*         | `Shell/screens` `ProductPrograms`, `FaultReport`, `CoachMarks`        | `DeckRows.Surfaces`        |
|  [19]   | settings.reset.*           | `Shell/screens` `SettingsSurface`                                     | `DeckRows.Surfaces`        |
|  [20]   | inspector.code.*           | `Editing/inspector` `CodePane`                                        | `DeckRows.Surfaces`        |
|  [21]   | document.* element.* nav.* | `Shell/palette` `PaletteKind`                                         | `DeckRows.Reveal`          |
|  [22]   | search.band.*              | `Document/search` `ResultsPresentation`                               | `DeckRows.Documents`       |
|  [23]   | notebook.cell.*            | `Document/notebook` `CellVerb`                                        | `DeckRows.Documents`       |
|  [24]   | export.*                   | `Document/export` `ExportForm`                                        | `DeckRows.Documents`       |
|  [25]   | conflict.*                 | `Editing/inspector` `ConflictIntent`                                  | `DeckRows.Conflict`        |

## [04]-[AVAILABILITY_ALGEBRA]

- Owner: `CommandGate` — the one availability fold from typed input streams to the `CanExecute` stream every materialized command consumes.
- Entry: `public IObservable<bool> CanExecute(IObservable<CommandRow.Availability> inputs)` — one gate stream per row, derived, never hand-written at call sites.
- Auto: the level stream attaches through `UiSchedulerPort.Degradation`, the valid stream is the screen validation fold, and the selection stream rides selection state — all as delegate-supplied streams, no sibling type re-modeled; the mount reach enters once as the frozen `SurfaceSession.Reach` capability set beside them; `Observe` seeds every stream so the gate is total before the first emission.
- Packages: System.Reactive, LanguageExt.Core, Rasm (kernel `CapabilitySet`), BCL inbox
- Growth: one `Availability` field row plus one `Observe` source row absorbs a new availability driver; zero new surface.
- Boundary: capability admission is TWO total planes folded by `AdmitsAll` — the health-derived level and the mount reach — so a `Requires` set naming `Faculty.HostDocument` folds unavailable both when the host degrades and when the surface never reached one; per-call-site CanExecute lambdas and availability policy enums are the deleted forms; `IsExecuting` on the materialized command drives progress presentation and suppresses re-entrancy, so a busy column on `Availability` was a manual busy flag no row ever read and it is deleted; a batch verb materialized through `CommandExecution.Combine` derives its availability as the all-true fold `CreateCombined` computes over child `CanExecute` streams.

```csharp signature
public static class CommandGate {
    // Reach is a MOUNT fact, so it enters as a frozen value rather than a stream: the mounted surface cannot
    // change shape under a live deck; it rides the fault fallback unchanged for the same reason.
    public static IObservable<CommandRow.Availability> Observe(
        IObservable<DegradationLevel> level,
        CapabilitySet<Faculty> reach,
        IObservable<bool> valid,
        IObservable<SelectionSnapshot> selected) =>
        Observable.CombineLatest(
            level.StartWith(DegradationLevel.Full),
            valid.StartWith(false),
            selected.StartWith(SelectionSnapshot.None),
            (current, admit, count) => new CommandRow.Availability(current, reach, admit, count))
        .DistinctUntilChanged()
        .Catch(Observable.Return(new CommandRow.Availability(DegradationLevel.Full, reach, false, SelectionSnapshot.None)))
        .Replay(1)
        .RefCount();

    extension(CommandRow row) {
        public IObservable<bool> CanExecute(IObservable<CommandRow.Availability> inputs) =>
            inputs.Select(row.Admits)
                .Catch(Observable.Return(false))
                .StartWith(false)
                .DistinctUntilChanged()
                .Replay(1)
                .RefCount();
    }
}
```

## [05]-[EXECUTION_RECEIPTS]

- Owner: `CommandOutcome` `[Union]` total result vocabulary; `DeckReceipt` execution evidence record; `CommandExecution` — the materialize-cross-seal fold, the `CommandTxn` translation, the batch-combine projection, the deck-owned span-ranked search, the one raise, the one remote entry, and the telemetry contribution.
- Cases: `CommandOutcome` = Completed | Cancelled | Rejected | RolledBack | Compensated under the locked kind literals completed, cancelled, rejected, rolled-back, compensated; `Kind` reads the same `CommandKinds` metadata projection the payload union takes, so the evidence fan's outcome dimension and the wire discriminator are one literal.
- Entry: `public ReactiveCommand<CommandPayload, DeckReceipt> Materialize(CommandDeck deck)` — one generated command per admitted row; `public IO<DeckReceipt> Run(CommandPayload payload, CommandDeck deck, CallerModality caller, CancellationToken cancel = default)` — the one admit-mint-cross fold every modality ends at, minting the row's `CommandIntent` and handing it to the AppHost `Run` door; `Raise(key, payload, cancel)` — the one non-wire raise the palette activation, the action panel, and the argument submit all end at, seating `CallerModality.Operator` for all three; `Invoke(key, JsonElement payload, caller, cancel)` — the single remote, deep-link, and journal-replay route, carrying its caller's declared modality; `Search(query)` — the deck-owned span-ranked lookup the palette's command provider and the binding editor's text probe both read.
- Auto: `Settled` folds the returned `CommandTxn` through the union's generated total `Switch`, so the outcome is total by CONSTRUCTION rather than by a catch-all rail and a fifth transaction case breaks this page at compile time; the only residual catch is cancellation, which the suite reports as a rolled-back transaction the UI must still tell apart from a fault; residual throws ride `ThrownExceptions` into the one screen fault state and the error dialog intent row; elapsed derives from the kernel `MonotonicTimeline` stamp pair the composition binds, so a broken gauge refuses on the rail rather than fabricating a duration; `Combine` resolves each batch key through `Row` and a fail-closed `Traverse` into `Fin`, so an unknown intent key aborts the macro rather than silently dropping.
- Receipt: `DeckReceipt` — intent key, surface key, elapsed `Duration`, outcome, payload digest, `CorrelationId` — sealed through `ReceiptSinkPort.Send` as kind `command` under `TelemetrySource.AppUi` with the boot-bound tenant threaded; `TelemetryRow` contributes the command-outcome and command-elapsed instrument rows inward through the contributor port; outcome counts ride the evidence fan's command arm and elapsed records direct off the sealed receipt through `Observe`, so the fan never parses duration text.
- Packages: ReactiveUI, LanguageExt.Core, NodaTime, Rasm (kernel `ContentHash`, `MonotonicTimeline`, sink port, instrument rows), Rasm.AppHost (project — `CommandIntent`, `CallerModality`, `CommandTxn`), BCL inbox
- Growth: one `CommandOutcome` case absorbs a new result class and breaks every dispatch site at compile time; one command instrument is one `InstrumentSpec` row here; zero new surface.
- Boundary: this fold is a DERIVATION over the AppHost door, never a second dispatcher — the row mints one `CommandIntent`, crosses `Composition.Cross`, and reads the disposition back, so the veto rail, the caller mediation, the meter, and the hash-chained event log all see a UI command exactly as they see an MCP tool call, and a UI-local invocation of `row.Execute` is the deleted form; the sealed `DeckReceipt` here is PRESENTATION evidence — surface key, elapsed, digest, correlation — over the suite receipt the crossing already sealed, never a rival record of the transaction; cancellation crosses as ONE token on two planes — `EnvIO` cuts the effect chain and the transaction's own `CancelScope` spine passes explicitly to the row's bound body, so the work stops where the receipt says it stopped; the receipt record lands as one `[JsonSerializable]` row on the package wire context merged at app roots; ICommand wrapper classes are the deleted form and a generic receipt or ledger abstraction the rejected form; the digest is the kernel `ContentHash.Of` hex of the serialized payload (the federation one-hasher; seed zero), so receipt payloads stay fixed-size on the hot path; `Combine` is the only batch-verb spelling and each child execution still seals its own receipt, so batch evidence never collapses into one opaque receipt; `Search` and its `Score` kernel are the page's one language-owned boundary capsule carrying statement forms for the alternate-lookup probe and the span walk; intent keys cross every boundary as ordinal strings.

```csharp signature
// --- [MODELS] ---------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(CommandOutcome.Completed), "completed")]
[JsonDerivedType(typeof(CommandOutcome.Cancelled), "cancelled")]
[JsonDerivedType(typeof(CommandOutcome.Rejected), "rejected")]
[JsonDerivedType(typeof(CommandOutcome.RolledBack), "rolled-back")]
[JsonDerivedType(typeof(CommandOutcome.Compensated), "compensated")]
public abstract partial record CommandOutcome {
    private CommandOutcome() { }
    public sealed record Completed : CommandOutcome;
    public sealed record Cancelled : CommandOutcome;
    public sealed record Rejected(FaultObservationWire Fault) : CommandOutcome;
    public sealed record RolledBack(string Reason) : CommandOutcome;
    public sealed record Compensated(string Reason) : CommandOutcome;

    public string Kind => CommandKinds.Outcome[GetType()];
}

public sealed record DeckReceipt(
    string Key,
    string Surface,
    Duration Elapsed,
    CommandOutcome Outcome,
    string PayloadDigest,
    CorrelationId Correlation) {
    public const string Kind = "command";
}

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class CommandExecution {
    static readonly Op RunKey = Op.Of(name: "appui.command.run");

    public static readonly InstrumentSpec Outcome = InstrumentSpec.Create(
        "rasm.appui.command.outcome", InstrumentKind.Count, MeasureForm.Whole, "{command}",
        "command executions by outcome", Seq(AppUiTelemetry.OutcomeSlot), None, None, None);

    public static readonly InstrumentSpec Elapsed = InstrumentSpec.Create(
        "rasm.appui.command.elapsed", InstrumentKind.Distribution, MeasureForm.Real, "s",
        "command execution wall duration", Seq(AppUiTelemetry.CommandSlot), Some(Buckets.InteractionSeconds), None, None);

    public static TelemetryContributorPort TelemetryRow(string version) =>
        AppUiTelemetry.Contribute(version, Outcome, Elapsed);

    public static Fin<Unit> Observe(InstrumentSet set, DeckReceipt receipt) =>
        set.Write(Elapsed, receipt.Elapsed.TotalSeconds, InstrumentSet.Tags((AppUiTelemetry.CommandSlot, receipt.Key)));

    extension(CommandRow row) {
        // A press IS an operator act, so the modality is seated HERE rather than knobbed onto the signature:
        // ReactiveUI's own token reaches the crossing, and the transaction's spine carries it to the body.
        public ReactiveCommand<CommandPayload, DeckReceipt> Materialize(CommandDeck deck) =>
            ReactiveCommand.CreateFromTask<CommandPayload, DeckReceipt>(
                (payload, token) => row.Run(payload, deck, CallerModality.Operator, token).RunAsync(EnvIO.New(token: token)).AsTask(),
                row.CanExecute(deck.Composition.Inputs),
                deck.Composition.Scheduler);

        // Payload admission precedes the crossing on EVERY modality, and the crossing is the ONLY way a row
        // reaches its work: the row mints its `CommandIntent` and hands it to the AppHost front door, which
        // fires the veto rail, records the caller at the mediation, prices the command, drives the row's own
        // bound body through its dispatch, and chains the entry. Running `row.Execute` from here is the
        // DELETED form — it stranded the veto rail with no firing site, left the modality unrecorded, and
        // landed a dispatched command outside the hash chain, so the suite's command log disagreed with the
        // UI about what the user ran. This page therefore catches nothing but cancellation: a suite fault is
        // a `CommandTxn` case, not an exception this fold has to guess a code for.
        public IO<DeckReceipt> Run(CommandPayload payload, CommandDeck deck, CallerModality caller, CancellationToken cancel = default) =>
            from start in Stamp(deck)
            from outcome in row.Admit(payload).Match(
                Succ: admitted => deck.Composition
                    .Cross(row.ToIntent(admitted, deck.Composition, caller), cancel)
                    .Map(Settled)
                    .Catch(static error => error is KernelFault.Cancelled, static _ => IO.pure((CommandOutcome)new CommandOutcome.Cancelled())),
                Fail: static fault => IO.pure((CommandOutcome)new CommandOutcome.Rejected(Observed(fault))))
            from elapsed in Gauge(deck, start)
            from receipt in deck.Seal(row.Key, outcome, elapsed, payload.Digest(deck.Composition.Wire))
            select receipt;
    }

    // The suite disposition IS the UI outcome, folded TOTALLY through the union's generated `Switch` so a
    // fifth transaction case breaks this arm at compile time. A refusal carries its bounded structured fault
    // observation; a rolled-back or compensated transaction retains its reason with no fabricated fault identity.
    static CommandOutcome Settled(CommandTxn txn) =>
        txn.Switch(
            committed: static _ => (CommandOutcome)new CommandOutcome.Completed(),
            refused: static refused => new CommandOutcome.Rejected(Observed(refused.Fault)),
            rolledBack: static back => new CommandOutcome.RolledBack(back.Reason),
            compensated: static done => new CommandOutcome.Compensated(done.Reason));

    static FaultObservationWire Observed(Error error) => AppHostFaultMap.Wire(FaultObservation.Of(error));

    // Elapsed is MEASURED or the rail refuses: a fabricated zero on a broken gauge would bill and grade a
    // duration nothing measured.
    static IO<MonotonicStamp> Stamp(CommandDeck deck) =>
        IO.lift(() => deck.Composition.Line.Capture(RunKey))
            .Bind(static stamp => stamp.Match(Succ: IO.pure, Fail: IO.fail<MonotonicStamp>));

    static IO<Duration> Gauge(CommandDeck deck, MonotonicStamp start) =>
        Stamp(deck).Bind(end => deck.Composition.Line.Elapsed(start, end, RunKey)
            .Match(Succ: static span => IO.pure(Duration.FromTimeSpan(span)), Fail: IO.fail<Duration>));

    extension(CommandDeck deck) {
        public IO<DeckReceipt> Seal(string key, CommandOutcome outcome, Duration elapsed, string digest) =>
            IO.pure(new DeckReceipt(key, deck.Composition.SurfaceKey, elapsed, outcome, digest, deck.Composition.Correlation))
                .Bind(receipt => deck.Composition.Sink
                    .Send(deck.Composition.Correlation, deck.Composition.Tenant, AppUiTelemetry.Source, DeckReceipt.Kind,
                        JsonSerializer.SerializeToElement(receipt, deck.Composition.Wire))
                    .Map(_ => receipt));

        public Fin<CombinedReactiveCommand<CommandPayload, DeckReceipt>> Combine(params ReadOnlySpan<string> keys) =>
            toSeq(keys.ToArray())
                .Traverse(key => deck.Row(key)
                    .Map(row => row.Materialize(deck))
                    .ToFin(Fail: new DeckFault.UnknownIntent(key)))
                .As()
                .Map(children => ReactiveCommand.CreateCombined(children, outputScheduler: deck.Composition.Scheduler));

        // The one raise every non-wire modality ends at: no caller constructs a receipt, none skips the
        // availability read, and an unavailable or unknown key seals a rejected receipt with zero elapsed.
        // The palette activation, the action panel, and the argument submit are all operator gestures, so
        // the modality is a stated fact at this seat rather than a parameter every call site re-answers.
        public IO<DeckReceipt> Raise(string key, CommandPayload payload, CancellationToken cancel = default) =>
            deck.Row(key).Filter(row => row.Admits(deck.Composition.Snapshot())).Match(
                Some: row => row.Run(payload, deck, CallerModality.Operator, cancel),
                None: () => Unavailable(deck, key));
    }

    // Refusal evidence still seals: the receipt is the one record of a raise nothing ran, and the exact error
    // projects through the suite's bounded structured observation.
    static IO<DeckReceipt> Unavailable(CommandDeck deck, string key) {
        DeckFault fault = new DeckFault.UnknownIntent(key);
        return deck.Seal(key, new CommandOutcome.Rejected(Observed(fault)), Duration.Zero, string.Empty);
    }

    extension(CommandDeck deck) {

        // The wire route carries its caller as EVIDENCE: a deep link an operator followed, a journal replay
        // an agent drove, and a plugin's remote verb are one entry discriminated by the modality the caller
        // declared, which is exactly the fact the AppHost mediation records and the event log chains.
        public IO<DeckReceipt> Invoke(string key, JsonElement payload, CallerModality caller, CancellationToken cancel = default) =>
            deck.Row(key).Filter(row => row.Admits(deck.Composition.Snapshot())).Match(
                Some: row => RunKey.Catch(() => Fin.Succ(payload.Deserialize<CommandPayload>(deck.Composition.Wire)), token: cancel)
                    .Bind(decoded => Optional(decoded).ToFin(Fail: new DeckFault.PayloadRejected(key)))
                    .Match(
                        Succ: decoded => row.Run(decoded, deck, caller, cancel),
                        Fail: failure => deck.Seal(
                            key, new CommandOutcome.Rejected(Observed(failure)), Duration.Zero, string.Empty)),
                None: () => deck.Raise(key, new CommandPayload.None(), cancel));

        // The span-ranked lookup over the freeze-built index: the query folds to lowercase ONCE, so the exact
        // probe and the fuzzy walk read the casing the index admitted; one key reachable through both its
        // label and an alias ranks once at its best spread.
        public Seq<(string Key, int Rank)> Search(ReadOnlySpan<char> query) {
            Span<char> folded = query.Length <= 128 ? stackalloc char[query.Length] : new char[query.Length];
            ignore(query.ToLowerInvariant(folded));
            FrozenDictionary<string, string>.AlternateLookup<ReadOnlySpan<char>> lookup = deck.Index.GetAlternateLookup<ReadOnlySpan<char>>();
            if (lookup.TryGetValue(folded, out string? exact)) { return [(exact, 0)]; }
            List<(string Key, int Rank)> ranked = [];
            foreach (KeyValuePair<string, string> entry in deck.Index) {
                Option<int> rank = Score(entry.Key.AsSpan(), folded);
                if (rank is { IsSome: true, Case: int hit }) { ranked.Add((entry.Value, hit)); }
            }
            return toSeq(ranked
                .GroupBy(static found => found.Key, StringComparer.Ordinal)
                .Select(static group => (Key: group.Key, Rank: group.Min(static found => found.Rank)))
                .OrderBy(static found => found.Rank)
                .ThenBy(static found => found.Key, StringComparer.Ordinal));
        }
    }

    extension(CommandPayload payload) {
        // One-hasher law: the digest mints through the kernel ContentHash.Of seed-zero entry; lowercase hex is
        // this boundary's wire projection of the UInt128.
        public string Digest(JsonSerializerOptions wire) =>
            $"{ContentHash.Of(JsonSerializer.SerializeToUtf8Bytes(payload, wire)):x32}";
    }

    // Both spans arrive pre-normalized, so the walk is a pure ordinal subsequence rank (EXPRESSION_SPINE
    // exemption: a span walk no fold operator expresses without per-char allocation).
    private static Option<int> Score(ReadOnlySpan<char> label, ReadOnlySpan<char> query) {
        int cursor = 0;
        int spread = 0;
        for (int at = 0; at < label.Length && cursor < query.Length; at++) {
            bool match = label[at] == query[cursor];
            spread += match ? at - cursor : 0;
            cursor += match ? 1 : 0;
        }
        return cursor == query.Length ? Some(spread) : None;
    }
}
```

The ControlService operational verbs surface as ordinary table rows on companion-control surfaces; each `Execute` binding lands on the settled AppHost rail at composition:

| [INDEX] | [INTENT_KEY]            | [EXECUTE_BINDING]                                    |
| :-----: | :---------------------- | :--------------------------------------------------- |
|  [01]   | control.capture-support | SupportTrigger.ExternalCommand admission             |
|  [02]   | control.set-degradation | OperatorOverride force input to the degradation fold |
|  [03]   | control.reload-options  | ReloadOutcome transition on the options rail         |

## [06]-[TS_PROJECTION]

- Owner: `CommandPayloadWire` and `CommandGateWire` the two census families this deck mints outward; `CommandRowWire`, `CommandInvocationWire`, `CommandOutcomeWire`, and `DeckReceiptWire` the sibling records riding inside them — `tests/contracts/MANIFEST.md` `[02.22]` seats family members inside their family's registration.
- Packages: BCL inbox
- Growth: one wire member row per new receipt field and one kind literal per new payload or outcome case; zero new surface. Arms transcribe the owning `[Union]`'s `[JsonDerivedType]` literals, so a case landing here without its arm is the drift this block exists to foreclose.
- Boundary: wire fields follow the suite camelCase law; duration, correlation, gesture, level, payload, and outcome keep owner spellings; `many.ids` is a JSON string array and `fields.values` an opaque JSON object owned by the addressed form schema; the suite converter carries `Seq` and `HashMap` without changing their JSON shapes; `requires` carries the capability set's ordered key projection, `targets` palette-kind keys, and `arguments` the argument-schema key; every `Option<T>` column OMITS under the `csharp:Rasm.AppHost/Runtime/ports#WIRE_LAW` posture the suite mint binds, so `gesture` and `arguments` spell `field?: T` and a `| null` union there names a token no producer writes; the `rejected` outcome arm carries the settled `FaultObservationWire` and the `rolled-back` and `compensated` arms their transaction's own reason, so no arm fabricates a code; palette frames and binding-editor rows stay host-local — remote callers cross through `CommandInvocationWire`; `CommandGateWire` carries the row gate while AppHost `CommandAvailabilityWire` remains a separate health snapshot; TypeScript interchange decodes the same invocation and adds no command-side message envelope.

```ts signature
type CommandPayloadWire =
  | { readonly kind: "none" }
  | { readonly kind: "single"; readonly id: string }
  | { readonly kind: "many"; readonly ids: readonly string[] }
  | { readonly kind: "text"; readonly value: string }
  | { readonly kind: "fields"; readonly values: Readonly<Record<string, unknown>> };

type CommandOutcomeWire =
  | { readonly kind: "completed" }
  | { readonly kind: "cancelled" }
  | { readonly kind: "rejected"; readonly fault: FaultObservationWire }
  | { readonly kind: "rolled-back"; readonly reason: string }
  | { readonly kind: "compensated"; readonly reason: string };

interface CommandRowWire { readonly key: string; readonly scope: "global" | "screen" | "viewport" | "dialog"; readonly requires: readonly string[]; readonly gesture?: string; readonly targets: readonly string[]; readonly arguments?: string; }
interface CommandGateWire { readonly key: string; readonly available: boolean; readonly level: string; }
interface CommandInvocationWire { readonly key: string; readonly payload: CommandPayloadWire; }
interface DeckReceiptWire { readonly key: string; readonly surface: string; readonly elapsed: string; readonly outcome: CommandOutcomeWire; readonly payloadDigest: string; readonly correlation: string; }
```

## [07]-[RESEARCH]

(none)
