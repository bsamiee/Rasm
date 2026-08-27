# [APPUI_COMMANDS_AVAILABILITY]

Rasm.AppUi runs one command deck: a single `CommandRow` table is the UI's one intent-row table, and menus, toolbars, access keys, hotkeys, tray items, palette entries, deep links, and remote verbs are derivation folds over it. Command IDENTITY is not this package's — it is `Rasm.AppHost/Agent/runtime#DISPATCH_FRONT_DOOR` `CommandIntent`, and every row executes by minting one through `CommandRow.ToIntent` and crossing that page's `Run` door. The page owns the intent row shape with its payload union, the keyed arrow table and the family-row data every cross-page verb reaches the deck through, the typed availability algebra over the degradation vocabulary, the total execution outcome, the deck-owned search and invocation spine, and the generated command wire families it lowers onto. The federated palette, its surface frames, and the binding editor are `Shell/palette.md`'s — presentation over this deck.

## [01]-[INDEX]

- [02]-[INTENT_TABLE]: One frozen row table, payload shapes, binding overlays, per-surface deck freeze.
- [03]-[DECK_FAMILIES]: The keyed arrow table, the row-shape policy, and the family-row data every owner roster projects through.
- [04]-[AVAILABILITY_ALGEBRA]: Typed availability inputs fold into one `CanExecute` stream.
- [05]-[EXECUTION_OUTCOME]: Total outcome union, one hook fact, one raise, and one remote entry.
- [06]-[TS_PROJECTION]: The generated command families the deck produces and admits.
- [07]-[RESEARCH]

## [02]-[INTENT_TABLE]

- Owner: `CommandRow` the UI derivation row over one `Rasm.AppHost` command, carrying its nested `Availability` input struct and the `ToIntent` mint that hands identity back to its owner; `CommandPayload` `[Union]` argument shapes; `DeckFault` the direct generated `[Union]` with one `[FaultCase]` leaf per command failure; `BindingOverlay` the per-user gesture-and-alias overlay folded ahead of the freeze; `CommandDeck` per-surface frozen result carrying the row table, the normalized palette index, the chord-claimant oracle, and the gesture-contest fold.
- Cases: `CommandPayload` = None | Single | Many | Text | Fields — one case per `Ui.CommandPayloadWire.kind` arm (`none`, `single`, `many`, `text`, `fields`), so the discriminant is the generated `KindOneofCase` and no literal restates it; parameterized intents discriminate on payload shape, never on name suffixes; each row's `Accepts` set names its admitted arm domain, and `Admit` seals `DeckFault.PayloadRejected` before the crossing on every invocation modality, so a payload the row never admitted opens no suite transaction.
- Entry: `public static Fin<CommandDeck> Freeze(CommandComposition composition, params ReadOnlySpan<CommandRow> rows)` — identity admission accumulates every duplicate-key and duplicate-text defect through `Validation` before the first `GestureContest` refuses; one freeze per mounted surface, and the composition-time services travel as one carrier.
- Auto: the `Surfaces` predicate filters rows exactly once at freeze against the supplied `ConsumptionProfile` and the resolved `SurfaceMount`; the composition's `BindingOverlay` rebinds each surviving row's gesture and widens the index with its aliases BEFORE the contest fold runs, so a user chord collides on the same oracle a default one does; `Claimants` is the one chord-ownership read and `Contests` is that read filtered to contested chords, so `Freeze` refuses the first deterministic contest before any command materializes and the binding editor asks the identical question at assignment time.
- Evidence: `Composition.Conflict` records the deterministic `GestureContest` immediately before `Freeze` returns `DeckFault.GestureConflict` carrying the same value; command execution begins only after a contest-free deck exists.
- Packages: Thinktecture.Runtime.Extensions, Avalonia, LanguageExt.Core, Rasm (kernel fault floor, `CapabilitySet`, `MonotonicTimeline`), Rasm.AppHost (project — `ConsumptionProfile`, `CommandIntent`, `CommandArguments`, `CallerModality`, `CommandResult`), BCL inbox
- Growth: one `CommandRow` absorbs a new verb across every derived surface, one `CommandPayload` case absorbs a new argument shape, and one `BindingOverlay` row absorbs a whole named keymap; zero new surface.
- Boundary: every column here is a UI DERIVATION column — what the row presents, where it mounts, which chord claims it, which palette kinds it acts on, which schema collects its arguments — and the columns naming WHAT command are read off `Rasm.AppHost/Agent/runtime#DISPATCH_FRONT_DOOR` `CommandIntent` at `ToIntent` rather than re-declared here, because a second command identity in a package that references its owner is a strata twin whose two spellings dispatch resolves by whichever page a call site happened to cite; the locked row shape — intent key, capability requirement, availability delegate over the two-plane `Availability` input, `Option<KeyGesture>`, surface predicate, palette-kind target set, argument schema — deletes menu registries, toolbar registries, palette registries, hotkey tables, keymap files, and deep-link maps in one stroke; `CommandPayload` crosses as the generated `CommandPayloadWire` alone — `Many` fills the arm's `ids` repeated field and `Fields` its `Struct`, each field value parsed through the AppHost `WireJson.Parser` as a well-known `Value`, so no carrier converter exists on this path and a `JsonSerializerOptions` column on the composition is the deleted form; the intent key is simultaneously the localization string key the `Label` resolver consumes and the icon catalog key, so a label column and an icon column are the deleted forms; `Chord` is the host-agnostic Cmd/Ctrl column transform, so per-platform gesture rows are the rejected form; `Execute` delegates bind host work at composition and no case body names a host API outside its own row; `Targets` names the `PaletteKind` keys a verb acts on as a CONTEXTUAL action — keys rather than rows, because the set crosses the intent wire; `Arguments` carries the `Editing/forms#FORM_SCHEMA` schema a parameterized verb collects its own arguments through, the schema's `SubmitIntent` and this row's key being one value by construction; the discriminants of `CommandPayload` and `CommandOutcome` are the generated `KindOneofCase` enums — `CommandWire.Kind` reads a payload's arm off its own lowering and `CommandWire.KindOf` renders an outcome's arm name off the descriptor, so a literal roster beside the corpus is the deleted form; a surface verb absent from this table is not a dead button but a screen that FAILS TO MATERIALIZE, because a tree resolves its expansion command and a strip its jump command against this frozen deck and both abort the materialize on a miss.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Text.Json;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using LanguageExt;
using LanguageExt.Common;
using NodaTime.Serialization.Protobuf;
using Rasm.AppHost.Runtime;
using Rasm.AppUi.Diagnostics;
using Rasm.Domain;
using Riok.Mapperly.Abstractions;
using Thinktecture;
// Contracts are retired from this logic.
using Duration = NodaTime.Duration;
// Contracts are retired from this logic.
using WkDuration = Google.Protobuf.WellKnownTypes.Duration;
using static LanguageExt.Prelude;

namespace Rasm.AppUi.Shell;

// --- [MODELS] --------------------------------------------------------------------------

public sealed partial record CommandRow(
    string Key,
    CommandScope Scope,
    CapabilitySet<Faculty> Requires,
    FrozenSet<Wire.CommandPayloadWire.KindOneofCase> Accepts,
    Func<CommandRow.Availability, bool> When,
    Option<KeyGesture> Gesture,
    Func<ConsumptionProfile, SurfaceMount, bool> Surfaces,
    FrozenSet<string> Targets,
    Option<FormSchema> Arguments,
    Func<CommandPayload, CancellationToken, IO<Unit>> Execute) {
    public CommandIntent ToIntent(CommandPayload payload, CommandComposition composition, CallerModality caller) =>
        CommandIntent.Of(
            Key,
            new CommandArguments(
                EvidenceOps.Element(CommandWire.Lower(payload)),
                composition.Tenant,
                composition.Correlation),
            caller);

    public readonly record struct Availability(
        DegradationLevel Level, CapabilitySet<Faculty> Reach, bool Valid, SelectionSnapshot Selection);

    public bool Admits(Availability input) =>
        input.Level.Retains.AdmitsAll(Requires) && input.Reach.AdmitsAll(Requires) && When(input);

    public Fin<CommandPayload> Admit(CommandPayload payload) =>
        Accepts.Contains(CommandWire.Kind(payload))
            ? Fin.Succ(payload)
            : Fin.Fail<CommandPayload>(new DeckFault.PayloadRejected($"{Key}: '{CommandWire.Kind(payload)}' outside the row's admitted domain"));

    public Fin<CommandPayload> Compose(FormState state) =>
        Arguments.Match(
            Some: schema => schema.Admit(state).ToFin().Map(static admitted => (CommandPayload)new CommandPayload.Fields(admitted.Values)),
            None: () => Fin.Fail<CommandPayload>(new DeckFault.PayloadRejected($"{Key}: carries no argument schema")));

    public bool Acts(string paletteKind) => Targets.Contains(paletteKind);
}

[SmartEnum<string>]
public sealed partial class CommandScope {
    public static readonly CommandScope Global = new("global");
    public static readonly CommandScope Screen = new("screen");
    public static readonly CommandScope Viewport = new("viewport");
    public static readonly CommandScope Dialog = new("dialog");
}

[ComplexValueObject]
public readonly partial struct SelectionSnapshot {
    public int Count { get; }
    public Seq<string> Kinds { get; }

    public static readonly SelectionSnapshot None = Create(0, Seq<string>());

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int count, ref Seq<string> kinds) {
        kinds = toSeq(kinds.Distinct().OrderBy(static kind => kind, StringComparer.Ordinal));
        validationError = count >= kinds.Count && (count > 0 || kinds.Count == 0)
            ? validationError
            : new ValidationError($"selection count {count} cannot carry {kinds.Count} kinds");
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CommandPayload {
    private CommandPayload() { }
    public sealed record None : CommandPayload;
    public sealed record Single(string Id) : CommandPayload;
    public sealed record Many(Seq<string> Ids) : CommandPayload;
    public sealed record Text(string Value) : CommandPayload;

    public sealed record Fields(HashMap<string, JsonElement> Values) : CommandPayload;

    public UInt128 Digest() => ContentHash.Of(this, static (payload, w) => payload.CanonicalBytes(w));

    public CanonicalWriter CanonicalBytes(CanonicalWriter w) => Switch(
        state: w,
        none: static (writer, _) => writer.Ordinal(0),
        single: static (writer, c) => writer.Ordinal(1).String(c.Id),
        many: static (writer, c) => writer.Ordinal(2).Sorted(c.Ids, static id => id, StringComparer.Ordinal, static (id, x) => x.String(id)),
        text: static (writer, c) => writer.Ordinal(3).String(c.Value),
        fields: static (writer, c) => writer.Ordinal(4).Sorted(
            toSeq(c.Values), static pair => pair.Key, StringComparer.Ordinal,
            static (pair, x) => x.String(pair.Key).String(pair.Value.GetRawText())));
}

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

    public Seq<string> Texts(CommandRow row, Func<string, string> label) =>
        Seq(label(row.Key)) + Aliases.Find(row.Key).IfNone(Seq<string>());

    public bool Rebound(string key) => Gestures.ContainsKey();

    public BindingOverlay With(string key, Option<KeyGesture> gesture) =>
        this with { Gestures = Gestures.AddOrUpdate(gesture) };

    public BindingOverlay Without(string key) => this with { Gestures = Gestures.Remove() };
}

public sealed record ShortcutPolicy(string ActiveSet, Seq<BindingOverlay> Sets) {
    public const string Section = nameof(ShortcutPolicy);

    public static readonly ShortcutPolicy Default = new(BindingOverlay.Defaults.SetKey, Seq(BindingOverlay.Defaults));

    public BindingOverlay Active =>
        Sets.Find(row => string.Equals(row.SetKey, ActiveSet, StringComparison.Ordinal)).IfNone(BindingOverlay.Defaults);
}

public sealed record GestureContest(CommandScope Scope, string Gesture, Seq<string> Keys) {
    public string Spelled => $"{Scope.Key}:{Gesture}:{string.Join(',', Keys)}";
}

// --- [ERRORS] --------------------------------------------------------------------------

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

// --- [SERVICES] ------------------------------------------------------------------------

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
    HookSet<AppUiPoint, AppUiFact, TelemetrySource> Hooks,
    Func<GestureContest, Unit> Conflict,
    Func<CommandIntent, CancellationToken, IO<CommandResult>> Cross);

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

    public Seq<string> Claimants(CommandScope scope, KeyGesture gesture) =>
        toSeq(Rows.Values
            .Where(row => row.Scope == scope && row.Gesture.Map(Composition.Chord).Filter(bound => bound.Equals(gesture)).IsSome)
            .Select(static row => row.Key)
            .Order(StringComparer.Ordinal));

    public Option<CommandRow> Row(string key) =>
        Rows.TryGetValue(out CommandRow? row) ? Optional(row) : None;

    public Seq<GestureContest> Contests() =>
        toSeq(toSeq(Rows.Values)
            .Bind(row => row.Gesture.Map(Composition.Chord).ToSeq().Map(gesture => (row.Scope, Gesture: gesture)))
            .Distinct()
            .Map(bound => new GestureContest(bound.Scope, bound.Gesture.ToString(), Claimants(bound.Scope, bound.Gesture)))
            .Filter(static contest => contest.Keys.Length > 1)
            .OrderBy(static contest => contest.Scope.Key, StringComparer.Ordinal)
            .ThenBy(static contest => contest.Gesture, StringComparer.Ordinal));

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
- Entry: `DeckArrows.Of(params (string Key, Func<CommandPayload, CancellationToken, IO<Unit>> Run)[])` — duplicate registrations refuse; `DeckArrows.Bound()` — the one read, refusing `DeckFault.UnknownIntent`; `DeckRows.Mint(DeckArrows arrows, Seq<FamilyRow> rows)` — the coverage proof and the row mint in one traverse, so a family row whose key no arrow binds refuses AT COMPOSITION rather than at first dispatch; `FamilyRow.Mint(run)` — the one `CommandRow` construction, which the owner-state families and the editor verb take directly with the arrow closing over their live owner.
- Law: a verb is ONE `FamilyRow` beside ONE arrow registration — the reachability lands as data, the behaviour stays the owner's own arrow, and no signature anywhere grows a column; a family whose keys generate off a bounded vocabulary (`TransportVerb`, `SessionAction`, `BakeVerb`, `CellVerb`, `ConflictIntent`, `VisibilityAction`) projects its roster's own `Items`, so a new roster row reaches its chord, its palette entry, and its journal replay with no edit here.
- Auto: `RowShape` closes the admitted-payload-and-gate product — bare, open, keyed, named, addressed, marked, fielded — so a family row states its shape as one row read and a bespoke `accepts`/`when` pair survives only as an explicit override with its reason at the site; the `media.*` roster stays composition-bound by structure — no media control binds a `media.*` key as its `IntentBinding.Command`, the transport bar's own segments carrying the `TransportVerb` keys this table already freezes.
- Packages: Thinktecture.Runtime.Extensions, Avalonia, LanguageExt.Core, BCL inbox
- Growth: one `FamilyRow` plus one arrow registration absorbs a new verb; one roster row at its owner absorbs a new generated verb with zero edits here; one `RowShape` row absorbs a new payload-and-gate shape.
- Boundary: `FamilyRow` is the ONE registration shape and `FamilyRow.Mint` the ONE `CommandRow` construction — a positional `CommandRow` construction and a full-control mint taking a raw accepts array beside a hand predicate are the two deleted forms, because each re-declares the columns the policy row already decides and neither is reachable by a column the row later grows; `ViewportVerbs.Visibility` keys derive as `viewport.<action.Key>` from the `Render/pipeline` `VisibilityAction` roster because that roster predates intent keys — every OTHER family key is its owner's own declared constant, so one act keeps one key and a re-spelled stem drifts nothing; the reveal family stays three HAND rows with stated provenance — the Command kind self-invokes and the Issue kind's act is `BoardSurface.JumpIntent`, already a Collab row, so a derivation over `PaletteKind.Reveal` would mint the borrowed key twice and refuse at the freeze; the history and graph planes arrive WHOLE off their own row projections (`HistoryIntents.Rows`, `GraphVerbs.Rows`) and the activity and workspace families close over their live owners, so owner-state behaviour never rides the arrow table where nothing could prove its coverage.

```csharp
// --- [TABLES] --------------------------------------------------------------------------

[SmartEnum<string>]
public sealed partial class RowShape {
    public static readonly RowShape Bare      = new("bare", K(Arm.None_), static _ => true);
    public static readonly RowShape Open      = new("open", K(Arm.None_, Arm.Single), static _ => true);
    public static readonly RowShape Keyed     = new("keyed", K(Arm.Single), static _ => true);
    public static readonly RowShape Named     = new("named", K(Arm.Text), static _ => true);
    public static readonly RowShape Addressed = new("addressed", K(Arm.Single, Arm.Fields), static _ => true);
    public static readonly RowShape Marked    = new("marked", K(Arm.Single, Arm.Many), static input => input.Selection.Count > 0);
    public static readonly RowShape Fielded   = new("fielded", K(Arm.Fields), static _ => true);

    public FrozenSet<Wire.CommandPayloadWire.KindOneofCase> Accepts { get; }

    [UseDelegateFromConstructor]
    public partial bool When(CommandRow.Availability input);

    private static FrozenSet<Wire.CommandPayloadWire.KindOneofCase> K(params Wire.CommandPayloadWire.KindOneofCase[] arms) => arms.ToFrozenSet();
}

public sealed record FamilyRow(
    string Key,
    CommandScope Scope,
    RowShape Shape,
    Option<KeyGesture> Gesture = default,
    Option<FormSchema> Arguments = default,
    Option<CapabilitySet<Faculty>> Requires = default,
    Option<Func<CommandRow.Availability, bool>> When = default,
    Option<FrozenSet<Wire.CommandPayloadWire.KindOneofCase>> Accepts = default) {
    public CommandRow Mint(Func<CommandPayload, CancellationToken, IO<Unit>> run) =>
        new(Key, Scope, Requires.IfNone(CapabilitySet<Faculty>.None), Accepts.IfNone(() => Shape.Accepts),
            When.IfNone(() => Shape.When), Gesture, static (_, _) => true, FrozenSet<string>.Empty, Arguments, run);
}

public sealed record DeckArrows(HashMap<string, Func<CommandPayload, CancellationToken, IO<Unit>>> Rows) {
    public static Fin<DeckArrows> Of(params ReadOnlySpan<(string Key, Func<CommandPayload, CancellationToken, IO<Unit>> Run)> rows) {
        Seq<(string Key, Func<CommandPayload, CancellationToken, IO<Unit>> Run)> declared = toSeq(rows.ToArray());
        Seq<string> collided = declared.Map(static row => row.Key).Collisions(static key => key);
        return collided.IsEmpty
            ? Fin.Succ(new DeckArrows(declared.ToHashMap(static row => row.Key, static row => row.Run)))
            : Fin.Fail<DeckArrows>(new DeckFault.DuplicateRow($"arrow: {string.Join(',', collided)}"));
    }

    public Fin<Func<CommandPayload, CancellationToken, IO<Unit>>> Bound(string key) =>
        Rows.Find().ToFin(new DeckFault.UnknownIntent());

    public static Func<CommandPayload, CancellationToken, IO<Unit>> Bare(Func<IO<Unit>> run) => (_, _) => run();

    public static Func<CommandPayload, CancellationToken, IO<Unit>> Addressed(string key, Func<string, IO<Unit>> run) =>
        (payload, _) => payload is CommandPayload.Single single
            ? run(single.Id)
            : IO.fail<Unit>(new DeckFault.PayloadRejected($"{key}: subject absent"));

    public static Func<CommandPayload, CancellationToken, IO<Unit>> Lifted(Func<CommandPayload, IO<Fin<Unit>>> run) =>
        (payload, _) => run(payload).Bind(static outcome => outcome.Match(Succ: static _ => IO.pure(unit), Fail: IO.fail<Unit>));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class DeckRows {
    public static Fin<Seq<CommandRow>> Mint(DeckArrows arrows, Seq<FamilyRow> rows) =>
        rows.Traverse(row => arrows.Bound(row.Key).Map(row.Mint)).As();

    public static Seq<FamilyRow> View() => Seq(
        new FamilyRow(ViewChrome.BackKey, CommandScope.Viewport, RowShape.Bare),
        new FamilyRow(ViewChrome.ForwardKey, CommandScope.Viewport, RowShape.Bare),
        new FamilyRow(ViewChrome.OrientationKey, CommandScope.Viewport, RowShape.Bare),
        new FamilyRow(ViewChrome.ProjectionKey, CommandScope.Viewport, RowShape.Bare),
        new FamilyRow(ViewChrome.BookmarksKey, CommandScope.Viewport, RowShape.Open),
        new FamilyRow(ViewChrome.MeasureKey, CommandScope.Viewport, RowShape.Bare));

    public static Seq<FamilyRow> Immersive() => Seq(
        new FamilyRow(XrReviewVerb.CaptureIssueIntent, CommandScope.Viewport, RowShape.Bare),
        new FamilyRow(XrReviewVerb.PassthroughIntent, CommandScope.Viewport, RowShape.Bare),
        new FamilyRow(XrReviewVerb.RecenterIntent, CommandScope.Viewport, RowShape.Bare));

    public static Seq<FamilyRow> Transport() =>
        toSeq(TransportVerb.Items).Map(static verb => new FamilyRow(verb.IntentKey, CommandScope.Screen, RowShape.Bare));

    public static Seq<FamilyRow> Collab() => Seq(
        new FamilyRow(PresenceFollow.ReleaseIntent, CommandScope.Global, RowShape.Bare),
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

    public static Seq<FamilyRow> SelectionSets() => Seq(
        new FamilyRow(SelectionSet.ListIntent, CommandScope.Global, RowShape.Bare),
        new FamilyRow(SelectionSet.ApplyIntent, CommandScope.Global, RowShape.Addressed),
        new FamilyRow(SelectionSet.RenameIntent, CommandScope.Global, RowShape.Addressed),
        new FamilyRow(SelectionSet.DropIntent, CommandScope.Global, RowShape.Addressed),
        new FamilyRow(SelectionSet.SimilarIntent, CommandScope.Global, RowShape.Marked));

    public static Seq<FamilyRow> Reveal() => Seq(
        new FamilyRow(PaletteKind.DocumentRevealIntent, CommandScope.Global, RowShape.Addressed),
        new FamilyRow(PaletteKind.ElementRevealIntent, CommandScope.Global, RowShape.Addressed),
        new FamilyRow(PaletteKind.RouteRevealIntent, CommandScope.Global, RowShape.Addressed));

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

    public static Seq<FamilyRow> Documents() => Seq(
        new FamilyRow(ResultsPresentation.ExpandIntent, CommandScope.Screen, RowShape.Addressed),
        new FamilyRow(ExportForm.RunIntent, CommandScope.Screen, RowShape.Addressed),
        new FamilyRow(ExportForm.OpenIntent, CommandScope.Global, RowShape.Addressed),
        new FamilyRow(ExportForm.RevealIntent, CommandScope.Global, RowShape.Addressed))
        + toSeq(CellVerb.Items).Map(static verb => new FamilyRow(verb.Intent, CommandScope.Screen, RowShape.Addressed));

    public static Seq<FamilyRow> Conflict() =>
        toSeq(ConflictIntent.Items).Map(static row => new FamilyRow(
            row.Key, CommandScope.Dialog, RowShape.Addressed,
            Gesture: row.Chord, Accepts: Some(row.Accepts.ToFrozenSet())));

    public static Seq<FamilyRow> Visibility() =>
        toSeq(VisibilityAction.Items).Map(static action => new FamilyRow(
            $"viewport.{action.Key}", CommandScope.Viewport,
            action == VisibilityAction.Reset ? RowShape.Bare : RowShape.Marked));

    // --- [OWNER_STATE_FAMILIES]

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

    public static Seq<CommandRow> Activity(ActivityCenter centre, Func<IO<Unit>> open) => Seq(
        new FamilyRow(ActivityCenter.OpenKey, CommandScope.Global, RowShape.Bare,
            Gesture: Some(new KeyGesture(Key.I, KeyModifiers.Control | KeyModifiers.Shift)))
            .Mint(DeckArrows.Bare(open)),
        new FamilyRow(ActivityCenter.ClearKey, CommandScope.Global, RowShape.Bare,
            When: Some<Func<CommandRow.Availability, bool>>(_ => centre.Unread > 0 || centre.Rows.Length > 0))
            .Mint(DeckArrows.Bare(() => IO.lift(() => ignore(centre.Clear())))));

    public static Seq<CommandRow> Workspace(WorkspaceCell cell, Func<IO<Unit>> save) => Seq(
        Entered(Workspaces.EnterVerb, cell, static (workspace, row) => workspace.Enter(row)),
        new FamilyRow(Workspaces.SaveVerb, CommandScope.Global, RowShape.Bare,
            Gesture: Some(new KeyGesture(Key.S, KeyModifiers.Control | KeyModifiers.Alt)))
            .Mint(DeckArrows.Bare(save)),
        Entered(Workspaces.ResetVerb, cell, static (workspace, row) => workspace.Reset(row)));

    static CommandRow Entered(
        string key, WorkspaceCell cell, Func<WorkspaceCell, WorkspaceRow, IO<Fin<Seq<RouteRestoreFact>>>> run) =>
        new FamilyRow(CommandScope.Global, RowShape.Keyed).Mint(
            DeckArrows.Lifted(payload => payload is CommandPayload.Single addressed
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

- Owner: `CommandGate` — the one availability fold from typed input streams to the `CanExecute` stream every materialized command consumes and the generated `CommandGateWire` feed remote presentation reads.
- Entry: `CommandGate.CanExecute(row, inputs)` serves the local reactive command; `CommandGate.Wire(row, inputs)` projects the same row/input pair onto the generated keyed verdict without re-running policy in TypeScript. Both remain callable through their `CommandRow` extension form.
- Auto: the level stream attaches through `UiSchedulerPort.Degradation`, the valid stream is the screen validation fold, and the selection stream rides selection state — all as delegate-supplied streams, no sibling type re-modeled; the mount reach enters once as the frozen `SurfaceSession.Reach` capability set beside them; `Observe` seeds every stream so the gate is total before the first emission.
- Packages: System.Reactive, LanguageExt.Core, Rasm (kernel `CapabilitySet`), BCL inbox
- Growth: one `Availability` field row plus one `Observe` source row absorbs a new availability driver; zero new surface.
- Boundary: capability admission is TWO total planes folded by `AdmitsAll` — the health-derived level and the mount reach — so a `Requires` set naming `Faculty.HostDocument` folds unavailable both when the host degrades and when the surface never reached one; per-call-site CanExecute lambdas and availability policy enums are the deleted forms; `IsExecuting` on the materialized command drives progress presentation and suppresses re-entrancy, so a busy column on `Availability` was a manual busy flag no row ever read and it is deleted; a batch verb materialized through `CommandExecution.Combine` derives its availability as the all-true fold `CreateCombined` computes over child `CanExecute` streams.

```csharp
public static class CommandGate {
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

        public IObservable<Wire.CommandGateWire> Wire(IObservable<CommandRow.Availability> inputs) =>
            inputs.Select(input => new Wire.CommandGateWire {
                    Key = row.Key,
                    Available = row.Admits(input),
                    Level = input.Level.Wire,
                })
                .DistinctUntilChanged(static gate => (gate.Key, gate.Available, gate.Level))
                .Replay(1)
                .RefCount();
    }
}
```

## [05]-[EXECUTION_OUTCOME]

- Owner: `CommandOutcome` `[Union]` total result vocabulary; `DeckOutcome` execution result; `CommandExecution` — the materialize-cross-observe fold, the `CommandResult` translation, the batch-combine projection, the deck-owned span-ranked search, the one raise, the one remote entry, and the telemetry contribution.
- Cases: `CommandOutcome` = Completed | Cancelled | Rejected | RolledBack | Compensated — one case per `Ui.CommandOutcomeWire.kind` arm; `CommandExecution.Observe` writes the outcome dimension from `CommandWire.KindOf`, the arm name the descriptor publishes, so the dimension value and the wire discriminator are one spelling.
- Entry: `public ReactiveCommand<CommandPayload, DeckOutcome> Materialize(CommandDeck deck)` — one generated command per admitted row; `public IO<DeckOutcome> Run(CommandPayload payload, CommandDeck deck, CallerModality caller, CancellationToken cancel = default)` — the one admit-mint-cross fold every modality ends at, minting the row's `CommandIntent`, handing it to the AppHost `Run` door, and firing the returned outcome on the package hook dispatch; `Raise(payload, cancel)` — the one non-wire raise the palette activation, the action panel, and the argument submit all end at, seating `CallerModality.Operator` for all three; `Invoke(CommandPayloadWire payload, caller, cancel)` — the single remote, deep-link, and journal-replay route, taking the generated message its caller parsed through the AppHost `WireJson.Parser` and carrying the caller's declared modality; `Search(query)` — the deck-owned span-ranked lookup the palette's command provider and the binding editor's text probe both read.
- Auto: `Settled` folds the returned `CommandResult.Txn` through the union's generated total `Switch`, so the outcome is total by CONSTRUCTION rather than by a catch-all arm and a fifth transaction case breaks this page at compile time; the only residual catch is cancellation, which the suite reports as a rolled-back transaction the UI must still tell apart from a fault; residual throws ride `ThrownExceptions` into the one screen fault state and the error dialog intent row; elapsed derives from the kernel `MonotonicTimeline` stamp pair the composition binds, so a broken gauge refuses on the result rather than fabricating a duration; `Combine` resolves each batch key through `Row` and a fail-closed `Traverse` into `Fin`, so an unknown intent key aborts the macro rather than silently dropping.
- Evidence: `DeckOutcome` carries the intent key, surface key, elapsed `Duration`, command outcome, and payload digest; `AppUiFact.Command` fires it at `AppUiPoint.Command` on the composition's `HookSet`, whose observe tap lowers it to the generated `Ui.EvidenceWire` command arm and publishes that data through the CloudEvent envelope; `TelemetryRow` contributes the command-outcome and command-elapsed instrument rows inward through the contributor port, and `Observe` records elapsed directly from the outcome.
- Packages: ReactiveUI, LanguageExt.Core, NodaTime, Rasm (kernel `ContentHash`, `MonotonicTimeline`, `HookSet`, instrument rows), Rasm.AppHost (project — `CommandIntent`, `CallerModality`, `CommandResult`), BCL inbox
- Growth: one `CommandOutcome` case absorbs a new result class and breaks every dispatch site at compile time; one command instrument is one `InstrumentSpec` row here; zero new surface.
- Boundary: this fold is a DERIVATION over the AppHost door, never a second dispatcher — the row mints one `CommandIntent`, crosses `Composition.Cross`, and reads the disposition back, so the veto path, the caller mediation, the meter, and the hash-chained event log all see a UI command exactly as they see an MCP tool call, and a UI-local invocation of `row.Execute` is the deleted form; cancellation crosses as ONE token on two planes — `EnvIO` cuts the effect chain and the transaction's own `CancelScope` spine passes explicitly to the row's bound body, so the work stops where the outcome says it stopped; `DeckOutcome` crosses as the generated `DeckOutcomeWire` through `DeckWire.Lower` and re-admits through `DeckWire.Admit`, so no package context row names it; ICommand wrapper classes are the deleted form; the digest is the kernel `ContentHash.Of` over the payload's own canonical bytes (the federation one-hasher; seed zero) and crosses as its 16 big-endian bytes through `ContentHash.Wire`, so command outcomes stay fixed-size on the hot path; `Combine` is the only batch-verb spelling and each child execution fires its own outcome; `Search` and its `Score` kernel are the page's one language-owned boundary capsule carrying statement forms for the alternate-lookup probe and the span walk; intent keys cross every boundary as ordinal strings.

```csharp
// --- [MODELS] --------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CommandOutcome {
    private CommandOutcome() { }
    public sealed record Completed : CommandOutcome;
    public sealed record Cancelled : CommandOutcome;
    public sealed record Rejected(FaultV1.FaultObservation Fault) : CommandOutcome;
    public sealed record RolledBack(string Reason) : CommandOutcome;
    public sealed record Compensated(string Reason) : CommandOutcome;
}

public sealed record DeckOutcome(
    string Key,
    string Surface,
    Duration Elapsed,
    CommandOutcome Outcome,
    UInt128 PayloadDigest);

// --- [COMPOSITION] ---------------------------------------------------------------------

[Mapper(
    RequiredMappingStrategy = RequiredMappingStrategy.Both,
    EnabledConversions = MappingConversionType.All & ~MappingConversionType.ExplicitCast)]
public static partial class CommandWire {
    public static Wire.CommandPayloadWire.KindOneofCase Kind(CommandPayload payload) => payload.Switch(
        none: static _ => Wire.CommandPayloadWire.KindOneofCase.None_,
        single: static _ => Wire.CommandPayloadWire.KindOneofCase.Single,
        many: static _ => Wire.CommandPayloadWire.KindOneofCase.Many,
        text: static _ => Wire.CommandPayloadWire.KindOneofCase.Text,
        fields: static _ => Wire.CommandPayloadWire.KindOneofCase.Fields);

    public static string KindOf(CommandOutcome outcome) =>
        Wire.CommandOutcomeWire.Descriptor.FindFieldByNumber((int)Lower(outcome).KindCase).Name;

    public static Wire.CommandPayloadWire Lower(CommandPayload payload) => payload.Switch(
        none: static _ => new Wire.CommandPayloadWire { None_ = new Empty() },
        single: static c => new Wire.CommandPayloadWire { Single = c.Id },
        many: static c => new Wire.CommandPayloadWire { Many = new Wire.CommandPayloadWire.Types.Many { Ids = { c.Ids } } },
        text: static c => new Wire.CommandPayloadWire { Text = c.Value },
        fields: static c => new Wire.CommandPayloadWire { Fields = Bag(c.Values) });

    public static Fin<CommandPayload> Admit(Wire.CommandPayloadWire wire) => wire.KindCase switch {
        Wire.CommandPayloadWire.KindOneofCase.None_ => Fin.Succ<CommandPayload>(new CommandPayload.None()),
        Wire.CommandPayloadWire.KindOneofCase.Single => Fin.Succ<CommandPayload>(new CommandPayload.Single(wire.Single)),
        Wire.CommandPayloadWire.KindOneofCase.Many => toSeq(wire.Many.Ids) switch {
            { IsEmpty: false } ids when ids.Distinct().Count == ids.Count => Fin.Succ<CommandPayload>(new CommandPayload.Many(ids)),
            _ => Fin.Fail<CommandPayload>(new KernelFault.InvalidInput()),
        },
        Wire.CommandPayloadWire.KindOneofCase.Text => Fin.Succ<CommandPayload>(new CommandPayload.Text(wire.Text)),
        Wire.CommandPayloadWire.KindOneofCase.Fields => Fin.Succ<CommandPayload>(new CommandPayload.Fields(
            toHashMap(toSeq(wire.Fields.Fields).Map(static pair => (pair.Key, EvidenceOps.Element(pair.Value)))))),
        Wire.CommandPayloadWire.KindOneofCase.None or _ => Fin.Fail<CommandPayload>(new KernelFault.InvalidInput()),
    };

    public static Wire.CommandOutcomeWire Lower(CommandOutcome outcome) => outcome.Switch(
        completed: static _ => new Wire.CommandOutcomeWire { Completed = new Empty() },
        cancelled: static _ => new Wire.CommandOutcomeWire { Cancelled = new Empty() },
        rejected: static c => new Wire.CommandOutcomeWire { Rejected = c.Fault },
        rolledBack: static c => new Wire.CommandOutcomeWire { RolledBack = c.Reason },
        compensated: static c => new Wire.CommandOutcomeWire { Compensated = c.Reason });

    public static Fin<CommandOutcome> Admit(Wire.CommandOutcomeWire wire) => wire.KindCase switch {
        Wire.CommandOutcomeWire.KindOneofCase.Completed => Fin.Succ<CommandOutcome>(new CommandOutcome.Completed()),
        Wire.CommandOutcomeWire.KindOneofCase.Cancelled => Fin.Succ<CommandOutcome>(new CommandOutcome.Cancelled()),
        Wire.CommandOutcomeWire.KindOneofCase.Rejected => Fin.Succ<CommandOutcome>(new CommandOutcome.Rejected(wire.Rejected)),
        Wire.CommandOutcomeWire.KindOneofCase.RolledBack => Fin.Succ<CommandOutcome>(new CommandOutcome.RolledBack(wire.RolledBack)),
        Wire.CommandOutcomeWire.KindOneofCase.Compensated => Fin.Succ<CommandOutcome>(new CommandOutcome.Compensated(wire.Compensated)),
        Wire.CommandOutcomeWire.KindOneofCase.None or _ => Fin.Fail<CommandOutcome>(new KernelFault.InvalidInput()),
    };

    static Struct Bag(HashMap<string, JsonElement> values) {
        Struct bag = new();
        values.Iter((field, value) => bag.Fields[field] = WireJson.Parser.Parse<Value>(value.GetRawText()));
        return bag;
    }
}

[Mapper(
    RequiredMappingStrategy = RequiredMappingStrategy.Both,
    EnabledConversions = MappingConversionType.All & ~MappingConversionType.ExplicitCast)]
public static partial class DeckWire {
    public static partial Wire.DeckOutcomeWire Lower(DeckOutcome outcome);

    public static Fin<DeckOutcome> Admit(Wire.DeckOutcomeWire wire) =>
        (CommandWire.Admit(wire.Outcome).ToValidation(),
         ContentHash.Admit(wire.PayloadDigest.Span).ToValidation())
            .Apply((outcome, digest) => new DeckOutcome(wire.Key, wire.Surface, wire.Elapsed.ToNodaDuration(), outcome, digest))
            .As().ToFin();

    [UserMapping] private static Wire.CommandOutcomeWire Outcome(CommandOutcome outcome) => CommandWire.Lower(outcome);
    [UserMapping] private static WkDuration Lapse(Duration elapsed) => elapsed.ToProtobufDuration();
    [UserMapping] private static ByteString Key(UInt128 digest) => ContentHash.Wire(digest);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class CommandExecution {

    public static readonly InstrumentSpec Outcome = InstrumentSpec.Create(
        "rasm.appui.command.outcome", InstrumentKind.Count, MeasureForm.Whole, "{command}",
        "command executions by outcome", Seq(AppUiTelemetry.OutcomeSlot), None, None, None);

    public static readonly InstrumentSpec Elapsed = InstrumentSpec.Create(
        "rasm.appui.command.elapsed", InstrumentKind.Distribution, MeasureForm.Real, "s",
        "command execution wall duration", Seq(AppUiTelemetry.CommandSlot), Some(Buckets.InteractionSeconds), None, None);

    public static TelemetryContributorPort TelemetryRow(string version) =>
        AppUiTelemetry.Contribute(version, Outcome, Elapsed);

    public static Fin<Unit> Observe(InstrumentSet set, DeckOutcome outcome) =>
        (set.Write(Outcome, 1L, InstrumentSet.Tags((AppUiTelemetry.OutcomeSlot, CommandWire.KindOf(outcome.Outcome)))).ToValidation(),
         set.Write(Elapsed, outcome.Elapsed.TotalSeconds, InstrumentSet.Tags((AppUiTelemetry.CommandSlot, outcome.Key))).ToValidation())
            .Apply(static (_, _) => unit).As().ToFin();

    extension(CommandRow row) {
        public ReactiveCommand<CommandPayload, DeckOutcome> Materialize(CommandDeck deck) =>
            ReactiveCommand.CreateFromTask<CommandPayload, DeckOutcome>(
                (payload, token) => row.Run(payload, deck, CallerModality.Operator, token).RunAsync(EnvIO.New(token: token)).AsTask(),
                row.CanExecute(deck.Composition.Inputs),
                deck.Composition.Scheduler);

        public IO<DeckOutcome> Run(CommandPayload payload, CommandDeck deck, CallerModality caller, CancellationToken cancel = default) =>
            from start in Stamp(deck)
            from outcome in row.Admit(payload).Match(
                Succ: admitted => deck.Composition
                    .Cross(row.ToIntent(admitted, deck.Composition, caller), cancel)
                    .Map(Settled)
                    .Catch(static error => error is KernelFault.Cancelled, static _ => IO.pure((CommandOutcome)new CommandOutcome.Cancelled())),
                Fail: static fault => IO.pure((CommandOutcome)new CommandOutcome.Rejected(Observed(fault))))
            from elapsed in Gauge(deck, start)
            let result = new DeckOutcome(row.Key, deck.Composition.SurfaceKey, elapsed, outcome, payload.Digest())
            from fired in IO.lift(() => deck.Composition.Hooks.Fire(
                AppUiPoint.Command,
                new AppUiFact.Command(result),
                RunKey,
                body: _ => Fin.Succ(result)))
            select fired;
    }

    static CommandOutcome Settled(CommandResult result) =>
        result.Txn.Switch(
            committed: static _ => (CommandOutcome)new CommandOutcome.Completed(),
            refused: static refused => new CommandOutcome.Rejected(Observed(refused.Fault)),
            rolledBack: static back => new CommandOutcome.RolledBack(back.Reason),
            compensated: static done => new CommandOutcome.Compensated(done.Reason));

    static FaultV1.FaultObservation Observed(Error error) => FaultWire.Observe(error);

    static IO<MonotonicStamp> Stamp(CommandDeck deck) =>
        IO.lift(() => Error.New(RunKey.Message, RunKey))
            .Bind(static stamp => stamp.Match(Succ: IO.pure, Fail: IO.fail<MonotonicStamp>));

    static IO<Duration> Gauge(CommandDeck deck, MonotonicStamp start) =>
        Stamp(deck).Bind(end => deck.Composition.Line.Elapsed(start, end, RunKey)
            .Match(Succ: static span => IO.pure(Duration.FromTimeSpan(span)), Fail: IO.fail<Duration>));

    extension(CommandDeck deck) {
        public Fin<CombinedReactiveCommand<CommandPayload, DeckOutcome>> Combine(params ReadOnlySpan<string> keys) =>
            toSeq(keys.ToArray())
                .Traverse(key => deck.Row()
                    .Map(row => row.Materialize(deck))
                    .ToFin(Fail: new DeckFault.UnknownIntent()))
                .As()
                .Map(children => ReactiveCommand.CreateCombined(children, outputScheduler: deck.Composition.Scheduler));

        public IO<DeckOutcome> Raise(string key, CommandPayload payload, CancellationToken cancel = default) =>
            deck.Row().Filter(row => row.Admits(deck.Composition.Snapshot())).Match(
                Some: row => row.Run(payload, deck, CallerModality.Operator, cancel),
                None: () => IO.fail<DeckOutcome>(new DeckFault.UnknownIntent()));
    }

    extension(CommandDeck deck) {

        public IO<DeckOutcome> Invoke(string key, Wire.CommandPayloadWire payload, CallerModality caller, CancellationToken cancel = default) =>
            deck.Row().Filter(row => row.Admits(deck.Composition.Snapshot())).Match(
                Some: row => CommandWire.Admit(payload, RunKey).Match(
                    Succ: decoded => row.Run(decoded, deck, caller, cancel),
                    Fail: IO.fail<DeckOutcome>),
                None: () => deck.Raise(new CommandPayload.None(), cancel));

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

The ControlService operational verbs surface as ordinary table rows on companion-control surfaces; each `Execute` binding lands on the settled AppHost entry at composition:

| [INDEX] | [INTENT_KEY]            | [EXECUTE_BINDING]                                    |
| :-----: | :---------------------- | :--------------------------------------------------- |
|  [01]   | control.capture-support | SupportTrigger.ExternalCommand admission             |
|  [02]   | control.set-degradation | OperatorOverride force input to the degradation fold |
|  [03]   | control.reload-options  | ReloadOutcome transition on the options monitor      |

## [06]-[TS_PROJECTION]

- Growth: one payload or outcome arm is one `kind` arm at the corpus, one domain case here, and one arm on each of `CommandWire`'s two total `Switch` lowerings and two exhaustive admissions; zero new surface.
- Boundary: TypeScript peers bind `@rasm\/contracts/rasm/contracts/ui/commands_pb` and re-author nothing, so no hand interface mirrors a family on either side. `many.ids` is the arm's repeated field and `fields` its `Struct`, owned by the addressed form schema. The `rejected` outcome arm carries generated `Fault.FaultObservation`; `rolled_back` and `compensated` carry their transaction's own reason, so no arm fabricates a code. Palette frames and binding-editor rows stay host-local; remote callers cross through `CommandInvocation`. `CommandGateWire` carries the row gate while the AppHost availability snapshot stays in its own `availability` family. Every family's JSON leaves through AppHost `WireJson.Formatter` and enters through `WireJson.Parser`.

## [07]-[RESEARCH]

(none)
