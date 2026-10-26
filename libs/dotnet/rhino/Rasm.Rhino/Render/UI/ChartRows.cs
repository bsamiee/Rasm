using Eto.Forms;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rasm.UI;
using Rasm.UI.Imaging;
using Rhino.Resources;

namespace Rasm.Rhino.Render.UI;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ScopeSource(string Caption, Func<RowScope, IO<Option<ScopeFrames>>> Frames);

internal sealed record ScopeShown(Option<ScopeFrames> Frames, Option<ScopeRequest> Request, Content<ScopeTraces> Traces) {
    public static ScopeShown Initial { get; } = new(None, None, new Content<ScopeTraces>.Pending(None, None));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ChartRows {
    // --- [SCOPE]
    private static readonly Sizing Grown = new Sizing.Grow(Factor.Create(1f));

    public static (Func<IterableNE<ScopeSource>, ControlRow> Row, RowSource<ScopeState> Source) Scope(
        string caption, string help, string owner, Func<RowScope, ValueStore<ScopeState>> store, RowRules rules, Option<ScopeState> factory = default) =>
        IterableNE.create(ScopeParameter.Items[0], ScopeParameter.Items.Skip(1)).Map(static item => new RowField<ScopeState>(item)) switch {
            var fields => RowSource.Fields(owner, [.. ScopeParameter.Items], scope => IO.pure(Seq(store(scope))), factory) switch {
                var source => (sources => ControlRow.Of(source, fields, caption, help, RowShape.Block, (scope, retain) => Realized(caption, sources, source, fields, scope, retain), rules), source),
            },
        };

    private sealed record ScopeHeld(ScopeSource Source, ScopeMarks Marks, ScopeShown Shown) {
        public static Lens<ScopeHeld, ScopeShown> Charted { get; } = Lens<ScopeHeld, ScopeShown>.New(static held => held.Shown, static shown => held => held with { Shown = shown });
    }

    private static IO<RowCells> Realized(
        string caption, IterableNE<ScopeSource> sources, RowSource<ScopeState> source, IterableNE<RowField<ScopeState>> fields, RowScope scope, Action<IDisposable> retain) =>
        from named in IO.pure(RowText.Localize(caption, scope.Sink).Local)
        let empty = RowText.Localize("Stage input appears here", scope.Sink).Local
        let site = new CallbackSite(scope.Sink, typeof(ChartRows), nameof(Scope))
        from label in DisposalOps.Acquire(IO.lift(static () => new Label { Wrap = Eto.Forms.WrapMode.None, Font = HostTheme.Digits(EtoFonts.SmallFont) }), retain)
        from scened in SceneRow<ScopeState, ScopeHeld>.Bind(
            source, fields, new ScopeHeld(sources.Head, new ScopeMarks(None, []), ScopeShown.Initial),
            (row, state, held) => Charted(row, state, held, named, empty),
            (_, _, held) => Reading(held) switch {
                var text => when(!string.Equals(label.Text, text, StringComparison.Ordinal), IO.lift(() => { label.Text = text; })).As(),
            },
            scope, site, retain)
        let row = scened.Row
        from requested in Traced(row, ScopeHeld.Charted, held => held.Source.Frames(scope), frames => scope.Read(source).Map(state => Scopes.Request(state, frames)), scope, site, retain)
        from popup in ChoiceRows.Popup(
            ChoiceRows.Items<ScopeKind>(None, scope.Sink), kind => row.Edited<ScopeKind>(static (picked, state) => state with { Kind = picked })(new Edit<ScopeKind>.Commit(kind)), scope.Sink, retain)
        from options in CommandRows.Button(
            new Command<RowRule, Invoke>.Run(
                new CommandFace("scope-options", "Scope Options", None, Some<GlyphSource>(Icons.Embedded(typeof(ChartRows).Assembly, "options.svg"))), None,
                new Invoke.Listed(listed => Listing(toSeq(sources), source, row, requested, listed))),
            IconSlot.PanelButton, scope, retain)
        select new RowCells(
            row.View, None, None, None,
            Seq(new RowLine(None, popup.Control, Some(options.Control), None)),
            Seq(new RowLine(None, label, None, None)),
            new RowHelp(None, None),
            from shown in scened.Edit.Shown
            from state in row.Shown
            from picked in popup.Show(state.Map(static held => held.Kind))
            select unit,
            Some<RowEdit>(scened.Edit), None);

    private static Node Charted(SceneRow<ScopeState, ScopeHeld> row, ScopeState state, ScopeHeld held, string named, string empty) =>
        held.Shown.Frames.Match(
            Some: frames => Scopes.Chart(
                state, frames, held.Shown.Traces, held.Marks, None, row.Edited<ScopeState>(static (next, _) => next),
                edit => row.Change(local => local with { Marks = edit.Carried.IfNone(local.Marks) })) switch {
                var drawn => drawn with { Accessible = drawn.Accessible.Combine(new Accessible { Label = named }) },
            },
            None: () => new Node.Text([empty]) { Role = ImagingRoles.Well, Width = Grown, Align = UnitPoint.Center, Accessible = new Accessible { Label = named } });

    private static string Reading(ScopeHeld held) =>
        (from range in held.Marks.Selection
         from shown in held.Shown.Traces.Held
         from histogram in Optional(shown.Reading as ScopeReading.Histogram)
         from upper in histogram.Below(range.Last)
         from lower in range.First is 0 ? Some(0d) : histogram.Below(range.First - 1)
         select RowText.Localize(
             "{0:F3} – {1:F3} · {2:F3} %", arguments: [HistogramAxis.LowerBound(range.First), HistogramAxis.LowerBound(range.Last), 100d * (upper - lower)]).Local).IfNone("");

    // --- [OPTIONS]
    private static IO<Seq<Seq<Command<RowRule, Invoke>>>> Listing(
        Seq<ScopeSource> sources, RowSource<ScopeState> source, SceneRow<ScopeState, ScopeHeld> row, IO<Unit> requested, RowScope scope) =>
        from state in scope.Read(source)
        select Seq(
            toSeq(ScopeParameter.Items).Filter(parameter => parameter.Listed.Exists(kind => kind == state.Kind)).Choose(parameter => Listed(parameter, source, row)),
            Seq(Submenu(new CommandFace("scope-source", "Source", None, None), new Command<RowRule, Invoke>.Choice(
                    new CommandFace("scope-source", "Source", None, None), None, sources.Map(entry =>
                        new Command<RowRule, Invoke>.Run(new CommandFace($"scope-source-{entry.Caption}", entry.Caption, None, None), None,
                            new Invoke.Effect(_ => row.Change(held => held with { Source = entry }).Bind(_ => requested))) {
                            Marked = new RowRule([], [], _ => IO.lift(() => row.Held.Source == entry)),
                        }))))
                .Filter(_ => sources.Count > 1));

    private static Option<Command<RowRule, Invoke>> Listed(ScopeParameter parameter, RowSource<ScopeState> source, SceneRow<ScopeState, ScopeHeld> row) =>
        new CommandFace(parameter.Key, parameter.Text.Caption, Some(parameter.Text.Help), None) switch {
            var face => parameter.Kind switch {
                StateParameter<ScopeState>.Toggle toggle => Some<Command<RowRule, Invoke>>(CommandRows.Toggle(
                    face, None, RowRule.When(source, state => toggle.Lens.Get(state), parameter), (_, on) => Written(row, toggle.Lens, on))),
                StateParameter<ScopeState>.Choice<TraceMix> choice => Some(Chosen(face, parameter, choice.Lens, source, row)),
                StateParameter<ScopeState>.Choice<Brightness> choice => Some(Chosen(face, parameter, choice.Lens, source, row)),
                StateParameter<ScopeState>.Keyed<SampleSquare, int> keyed => Some(Submenu(face, new Command<RowRule, Invoke>.Choice(face, None, toSeq(SampleSquare.Items).Map(square =>
                    new Command<RowRule, Invoke>.Run(
                        new CommandFace($"{parameter.Key}-{square.Key}", RowText.Localize("{0} × {0}", arguments: [square.Key]).Local, None, None), None,
                        new Invoke.Effect(_ => Written(row, keyed.Lens, square))) {
                        Marked = RowRule.When(source, state => keyed.Lens.Get(state) == square, parameter),
                    })))),
                _ => None,
            },
        };

    private static Command<RowRule, Invoke> Chosen<TValue>(
        CommandFace face, ScopeParameter parameter, Lens<ScopeState, TValue> lens, RowSource<ScopeState> source, SceneRow<ScopeState, ScopeHeld> row)
        where TValue : class, ISmartEnum<string, TValue, ValidationError>, ICaptioned =>
        Submenu(face, CommandRows.Choices<TValue>(
            face, None,
            item => RowRule.When(source, state => lens.Get(state) == item, parameter),
            (_, item) => Written(row, lens, item),
            item => new CommandFace($"{parameter.Key}-{item.Key}", item.Caption, None, None)));

    private static Command<RowRule, Invoke> Submenu(CommandFace face, Command<RowRule, Invoke> choice) =>
        new Command<RowRule, Invoke>.Menu(face, None, [[choice]]);

    private static IO<Unit> Written<TValue>(SceneRow<ScopeState, ScopeHeld> row, Lens<ScopeState, TValue> lens, TValue value) where TValue : notnull =>
        row.Edited<TValue>((next, state) => lens.Set(next, state))(new Edit<TValue>.Commit(value));

    // --- [READINGS]
    internal static IO<IO<Unit>> Traced<TRecord, TLocal>(
        SceneRow<TRecord, TLocal> row, Lens<TLocal, ScopeShown> shown, Func<TLocal, IO<Option<ScopeFrames>>> frames, Func<ScopeFrames, IO<ScopeRequest>> request,
        RowScope scope, CallbackSite site, Action<IDisposable> retain)
        where TRecord : notnull
        where TLocal : notnull =>
        from reading in DisposalOps.Acquire(IO.lift(static () => new Retained<IDisposable>(DisposalOps.Release)), retain)
        from kept in DisposalOps.Acquire(IO.lift(static () => new Retained<ScopeTraces>(static traces => IO.lift(() => traces.Iter(static held => held.Dispose())))), retain)
        from landed in Subscriptions.Idle<ScopeRequest, Content<ScopeTraces>>(arrived => Landed(row, shown, kept, arrived))(site, retain)
        let asked = Asked(row, shown, frames, request, reading, landed)
        from posted in Subscriptions.Idle<Unit, Unit>(_ => asked)(site, retain)
        let ask = posted((unit, unit))
        from loaded in ViewOps.WhileLoaded(row.View, keep =>
            from changed in RenderRuns.Changed.Inline(_ => ask, scope.Sink, keep)
            from listened in scope.Listen(_ => ask, keep)
            from initial in asked
            select unit, site, retain)
        select ask;

    private static IO<Unit> Asked<TRecord, TLocal>(
        SceneRow<TRecord, TLocal> row, Lens<TLocal, ScopeShown> shown, Func<TLocal, IO<Option<ScopeFrames>>> frames, Func<ScopeFrames, IO<ScopeRequest>> request,
        Retained<IDisposable> reading, Func<(ScopeRequest Key, Content<ScopeTraces> Value), IO<Unit>> landed)
        where TRecord : notnull
        where TLocal : notnull =>
        from framed in frames(row.Held)
        from asked in framed.Traverse(request).As()
        from rendering in IO.lift(static () => RenderRuns.Rendering)
        let target = asked.Filter(_ => !rendering)
        let held = shown.Get(row.Held)
        from changed in when(held.Frames != framed || held.Request != target,
            from drained in reading.Drain
            from stored in row.Change(local => shown.Set(shown.Get(local) with { Frames = framed, Request = target, Traces = shown.Get(local).Traces.Requested(None) }, local))
            from forked in (from wanted in target from formed in framed select (Request: wanted, formed.Frame)).Traverse(work => DisposalOps.Fork(
                Scopes.Read(work.Request, work.Frame).Map(static traces => (Content<ScopeTraces>)new Content<ScopeTraces>.Ready(traces))
                    .Catch(static error => !error.Is(Errors.Cancelled), static error => IO.pure<Content<ScopeTraces>>(new Content<ScopeTraces>.Failed(error)))
                    .Bind(content => landed((work.Request, content))),
                reading.Retain)).As()
            select unit).As()
        select unit;

    private static IO<Unit> Landed<TRecord, TLocal>(
        SceneRow<TRecord, TLocal> row, Lens<TLocal, ScopeShown> shown, Retained<ScopeTraces> kept, HashMap<ScopeRequest, Content<ScopeTraces>> arrived)
        where TRecord : notnull
        where TLocal : notnull =>
        toSeq(arrived.AsIterable()).TraverseM(entry => shown.Get(row.Held).Request.Exists(asked => asked == entry.Key)
            ? row.Change(held => shown.Set(shown.Get(held) with { Traces = entry.Value }, held)).Bind(_ => entry.Value.Held.Traverse(kept.Replace).As().Map(static _ => unit))
            : IO.lift(() => entry.Value.Held.Iter(static traces => traces.Dispose()))).As().Map(static _ => unit);

    // --- [RAMP_KEY]
    public static ControlRow RampKey(string caption, string help, Func<RowScope, IO<Option<AgXLook>>> look, RowRules rules) =>
        new ControlRow.Readout(RowShape.Block, (scope, retain) =>
            from shown in look(scope)
            from ramp in DisposalOps.Acquire(IO.lift(() => new GradientRamp(scope.Sink, RowText.Localize(caption, scope.Sink).Local, new GradientRampState.Key(shown))), retain)
            select new RowCells(
                ramp, None, None, None, [], [], new RowHelp(None, None),
                look(scope).Bind(current => ramp.Advance(_ => new(new GradientRampState.Key(current), None))),
                None, None),
            caption, help, rules);
}
