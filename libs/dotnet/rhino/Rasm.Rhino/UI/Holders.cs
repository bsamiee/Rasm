using System.Drawing;
using System.Runtime.CompilerServices;
using Eto;
using Eto.Forms;
using NodaTime.Text;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rhino;
using Rhino.UI;
using Rhino.UI.Controls;
using NativeDataSourceEventArgs = Rhino.UI.Controls.DataSource.EventArgs;

namespace Rasm.Rhino.UI;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HolderRow {
    public Seq<View.Section> Sections =>
        Switch(
            stacked: static stacked => stacked.Members,
            filled: static filled => filled.Above.Add(filled.Fill) + filled.Below);

    public sealed record Stacked(Seq<View.Section> Members) : HolderRow;

    public sealed record Filled(Seq<View.Section> Above, View.Section Fill, Seq<View.Section> Below) : HolderRow;
}

internal sealed record HeaderButton(GlyphRole Glyph, string ToolTip, IO<Unit> Click);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class ViewBody : IDisposable {
    // --- [STATE]
    private readonly View row;
    private readonly RowScope enclosing;
    private readonly Retained<IDisposable> owned;
    private readonly Atom<bool> hidden = Atom(value: false);
    private readonly ShownSubscription listeners;
    private RowRules Rules => new(row.Enabled, row.Visible);
    private bool showing;

    private ViewBody(
        View row, Control content, RowScope scope, RowScope enclosing, BindingGroup group, Seq<IO<Unit>> clicks,
        Option<Func<ViewBody, Action<IDisposable>, IO<Unit>>> listening, Retained<IDisposable> owned) {
        (this.row, Content, Scope, this.enclosing, Group, Clicks, this.owned) = (row, content, scope, enclosing, group, clicks, owned);
        listeners = new ShownSubscription(retain => Listening(listening, retain));
    }

    public Control Content { get; }
    public RowScope Scope { get; }
    public BindingGroup Group { get; }
    public Seq<IO<Unit>> Clicks { get; }
    public bool Hidden => hidden.Value;
    public int Height => hidden.Value ? 0 : (int)Math.Ceiling(Content.GetPreferredSize().Height);

    // --- [REALIZATION]
    public static IO<ViewBody> Realize(IPlugInViews owner, View.Section row, RowScope enclosing, Action<IDisposable> retain) =>
        DisposalOps.Acquire(Built(owner, row, enclosing, None), retain);

    public static IO<ViewBody> Open(IPlugInViews owner, View row, Option<RhinoDoc> document, CommitMode mode, Option<Func<ViewBody, Action<IDisposable>, IO<Unit>>> listening) =>
        from enclosing in RowScope.Open(owner, document, mode)
        from body in Built(owner, row, enclosing, listening)
        from opened in DisposalOps.OnFailure(body.Scope.Opened(body.Group), IO.lift(body.Dispose))
        select body;

    private static IO<ViewBody> Built(IPlugInViews owner, View row, RowScope enclosing, Option<Func<ViewBody, Action<IDisposable>, IO<Unit>>> listening) =>
        from owned in IO.lift(static () => new Retained<IDisposable>(DisposalOps.Release))
        from body in DisposalOps.OnFailure(
            from bindings in ViewOps.Bindings(owner, row, enclosing)
            from history in row.Store.Traverse(store => IO.lift(ViewOps.History(owner, row, store, bindings, enclosing.Document))).As()
            let scope = enclosing.Nested(ViewOps.Settings(owner, row), history, ViewOps.Locks(row, enclosing.Document))
            from hidden in scope.Shown(visible: false)
            from content in ViewOps.Realize(owner, row.Children, scope, owned.Retain)
            from clicks in (from section in Optional(row as View.Section) from held in history select Clicked(owner, section, held, scope, owned.Retain))
                .IfNone(IO.pure(Seq<IO<Unit>>()))
            from built in IO.lift(() => new ViewBody(row, content, scope, enclosing, bindings, clicks, listening, owned))
            from visibility in enclosing.OnVisibility(_ => built.Evaluated, owned.Retain)
            select built,
            IO.lift(owned.Dispose))
        select body;

    // --- [EDGES]
    public IO<Unit> Shown(bool visible) => IO.lift(() => showing = visible)
        .Bind(_ => Callbacks.Each(Seq(listeners.Shown(visible), Evaluated))).Map(unit);

    public IO<Unit> Reread => Scope.Changed(ViewCatalog.Tree(row).Bind(Keys));

    public void Dispose() {
        using (owned)
        using (listeners)
            _ = Scope.Shown(visible: false).Uninterruptible().Run();
    }

    // --- [RULES]
    private IO<Unit> Evaluated =>
        from shows in Rules.Shows(Scope)
        from enables in Rules.Enables(Scope)
        from visible in enclosing.Visibility
        from swapped in hidden.SwapIO(_ => !shows)
        from set in IO.lift(() => { (Content.Visible, Content.Enabled) = (shows, enables); })
        from availability in Scope.Shown(showing && visible && shows && enables)
        from sections in SectionHolder.Shown(Content, showing && visible && shows && enables)
        select unit;

    private IO<Unit> Listening(Option<Func<ViewBody, Action<IDisposable>, IO<Unit>>> listening, Action<IDisposable> retain) =>
        from signals in toSeq(Rules.Each.Bind(static rule => rule.Sources).Except(row.Children.Bind(static child => child.Controls).Choose(static control => control.Bound)))
            .TraverseM(source => source.Signals(Scope)).As()
        let reads = toHashSet(Rules.Each.Bind(static rule => rule.Reads))
        from heard in Scope.Listen(keys => when(keys.Exists(reads.Contains), Evaluated).As(), retain)
        from attached in signals.Flatten().TraverseM(signal => signal.Choose(static _ => Some((unit, unit)))
            .Through(Subscriptions.Idle<Unit, Unit>(_ => Evaluated), Scope.Sink, retain)).As()
        from extra in listening.Traverse(listen => listen(this, retain)).As()
        from reread in Reread
        select unit;

    private static Seq<EntryKey> Keys(View view) =>
        view.Children.Bind(static child => child.Controls).Bind(static control => control.Keys + control.Rules.Each.Bind(static rule => rule.Reads))
        + new RowRules(view.Enabled, view.Visible).Each.Bind(static rule => rule.Reads);

    // --- [HEADER]
    public static Option<IHeaderButtonHandler> Header(View.Section row, IO<Option<ViewBody>> current, IPlugInSink sink) =>
        Faces(row) switch {
            { IsEmpty: true } => None,
            var faces => Some<IHeaderButtonHandler>(new HeaderButtons(faces.Map((face, at) => new HeaderButton(face.Glyph, face.ToolTip,
                current.Bind(held => IO.lift(held.Bind(body => body.Clicks.At(at)).ToFin(new Refused(nameof(Clicks), Refused.Absent)))).Bind(static click => click))), sink, row.Identity)),
        };

    private static Seq<(GlyphRole Glyph, string ToolTip)> Faces(View.Section row) =>
        row.Store.IsSome
            ? row.Presets.Map(static _ => (Glyph: GlyphRole.Presets, ToolTip: "Presets")).ToSeq() + Seq<(GlyphRole Glyph, string ToolTip)>((GlyphRole.Options, "Values"), (GlyphRole.Reset, "Reset to defaults"))
            : [];

    private static IO<Seq<IO<Unit>>> Clicked(IPlugInViews owner, View.Section row, HistoryBinding history, RowScope scope, Action<IDisposable> retain) =>
        row.Presets.Map(listing => Popped(listing(scope), owner, retain)).ToSeq()
            .Add(Popped(Listing(owner, row.Children.Bind(static child => child.Controls), history, scope), owner, retain))
            .Add(IO.pure(scope.Commit(history.Group, history.Group.Defaults).Map(unit)))
            .TraverseM(static click => click).As();

    private static IO<IO<Unit>> Popped(IO<Seq<MenuPick>> listing, IPlugInSink sink, Action<IDisposable> retain) =>
        ChoiceRows.Menu(Seq<Seq<MenuEntry>>(), Seq<IO<Unit>>(), Some(listing), sink, retain).Map(static menu => IO.lift(() => menu.Show()));

    // --- [VALUES]
    private static IO<Seq<MenuPick>> Listing(IPlugInViews owner, Seq<ControlRow> controls, HistoryBinding history, RowScope scope) =>
        from held in history.History
        from pastes in IO.lift(static () => Clipboard.Instance.ContainsText)
        from zone in IO.lift(() => owner.Clock.ToZonedClock().Zone)
        let stamp = LocalDateTimePattern.Create("g", RowText.Culture)
        select Seq(
                held.Done.Head.Map<MenuPick>(entry => Pick(RowText.Localize("Undo {0}", arguments: [Described(entry, controls, owner)]).Local, Heard(history.Step(StepDirection.Undo), scope))),
                held.Undone.Head.Map<MenuPick>(entry => Pick(RowText.Localize("Redo {0}", arguments: [Described(entry, controls, owner)]).Local, Heard(history.Step(StepDirection.Redo), scope))),
                Some<MenuPick>(Pick(RowText.Localize("Copy values").Local, Transfer.Copy(history.Group, None, scope))),
                Some<MenuPick>(Pick(RowText.Localize("Paste values").Local, Transfer.Paste(history.Group, None, scope))).Filter(_ => pastes),
                Some<MenuPick>(Pick(RowText.Localize("Clear history").Local, history.Clear)).Filter(_ => !(held.Done.IsEmpty && held.Undone.IsEmpty)),
                Some<MenuPick>(new MenuPick.Group(RowText.Localize("Restore").Local, held.Done.Map<MenuPick>(entry => Pick(
                    RowText.Localize("{0}, {1}", arguments: [Described(entry, controls, owner), stamp.Format(entry.At.InZone(zone).LocalDateTime)]).Local,
                    Heard(history.Restore(entry.Serial), scope))))).Filter(_ => !held.Done.IsEmpty))
            .Somes();

    private static MenuPick.Item Pick<T>(string caption, IO<T> run) => new(caption, Marked: false, run.Map(unit));

    private static IO<ValueDiff> Heard(IO<ValueDiff> step, RowScope scope) =>
        from diff in step
        from changed in scope.Changed(toSeq(diff.Changes.Keys))
        select diff;

    private static string Described(HistoryEntry entry, Seq<ControlRow> controls, IPlugInSink sink) =>
        controls.Filter(control => control.Keys.Exists(entry.Diff.Changes.ContainsKey)) switch {
            [var only] => only.Wording.Shown(only.Caption, sink),
            _ => RowText.Localize("{0} values", arguments: [entry.Diff.Changes.Count]).Local,
        };
}

internal sealed class HeaderButtons(Seq<HeaderButton> buttons, IPlugInSink sink, Type owner) : IHeaderButtonHandler {
    internal IO<Seq<ButtonDrawable>> Realize(Action<IDisposable> retain) =>
        buttons.Map((button, index) =>
            from icon in Icons.Frames((button.Glyph, IconSlot.SectionHeader.Master), IconSlot.SectionHeader)
            from drawable in Create(this, index, icon.GetFrame(1f).Bitmap, RowText.Localize(button.ToolTip).Local, new Retained<IDisposable>(DisposalOps.Release), retain)
            from paint in Subscriptions.Attach(
                handler => drawable.Paint += handler, handler => drawable.Paint -= handler,
                Callbacks.Handler<PaintEventArgs>(_ => IO.lift(() => Image(drawable) = Icons.Frame(
                    button.Glyph, IconSlot.SectionHeader.Master, new(drawable.Width, drawable.Height))
                    .GetFrame(drawable.ParentWindow.LogicalPixelSize, new(drawable.Width, drawable.Height)).Bitmap).Map(unit),
                    new CallbackSite(sink, owner, nameof(Drawable.Paint))), retain)
            select drawable).TraverseM(static drawable => drawable).As();

    internal static IO<Seq<ButtonDrawable>> Realize(IHeaderButtonHandler header, CallbackSite site, Action<IDisposable> retain) {
        IO<Option<(Eto.Drawing.Bitmap Image, string ToolTip)>> read(int index, Eto.Drawing.Size pixels, Action<IDisposable> owned) =>
            DisposalOps.Within(() => new Bitmap(pixels.Width, pixels.Height), seed =>
                IO.lift(() => {
                    Bitmap bitmap = seed;
                    string toolTip = "";
                    return header.ButtonDetails(index, ref bitmap, ref toolTip) ? Some((Bitmap: bitmap, ToolTip: toolTip)) : None;
                }).Bind(found => found.Traverse(value => DisposalOps.Acquire(IO.lift(value.Bitmap.ToEto), owned)
                    .Map(bitmap => (Image: bitmap, value.ToolTip))).As()));
        return header is HeaderButtons authored ? authored.Realize(retain) : Monad.recur(Seq<ButtonDrawable>(), controls =>
            from images in IO.lift(static () => new Retained<IDisposable>(DisposalOps.Release))
            from initial in DisposalOps.Acquire(
                owned => IconSlot.SectionHeader.Size().Bind(size => read(controls.Count, new(size, size), owned)),
                (found, release) => found.Match(Some: _ => images.Retain(release), None: release.Dispose))
            from next in initial.Match(
                Some: value =>
                    from drawable in Create(header, controls.Count, value.Image, value.ToolTip, images, retain)
                    from painted in Subscriptions.Attach(handler => drawable.Paint += handler, handler => drawable.Paint -= handler,
                        Callbacks.Handler<PaintEventArgs>(_ => DisposalOps.Replace(
                            owned => read(controls.Count, Eto.Drawing.Size.Ceiling((Eto.Drawing.SizeF)drawable.Size * drawable.ParentWindow.LogicalPixelSize), owned),
                            (found, release) => found.Match(
                                Some: shown => DisposalOps.Published(images, shown, release, () => (Image: Image(drawable), drawable.ToolTip), current => (Image(drawable), drawable.ToolTip) = current),
                                None: () => DisposalOps.Release(release))).Map(unit), site), retain)
                    select Next.Loop<Seq<ButtonDrawable>, Seq<ButtonDrawable>>(controls.Add(drawable)),
                None: () => IO.pure(Next.Done<Seq<ButtonDrawable>, Seq<ButtonDrawable>>(controls)))
            select next).As();
    }

    private static IO<ButtonDrawable> Create(IHeaderButtonHandler header, int index, Eto.Drawing.Bitmap image, string toolTip, Retained<IDisposable> images, Action<IDisposable> retain) =>
        DisposalOps.Within<IDisposable, ButtonDrawable>(owned => {
            Dictionary<ButtonDrawable, System.Collections.Generic.HashSet<Action>> prior = [];
            EventHandler<WidgetCreatedEventArgs> created = (_, args) => {
                if (args.Instance is not ButtonDrawable drawable) return;

                drawable.Tag = images;
                prior.Add(drawable, [.. ThemeActions(ThemeNotifier(owner: null, drawable))]);
            };
            return from attached in Subscriptions.Attach(
                static handler => Platform.Instance.WidgetCreated += handler, static handler => Platform.Instance.WidgetCreated -= handler, created, owned)
                   from drawable in DisposalOps.Acquire(IO.lift<ButtonDrawable>(() => new HeaderDrawable(header, index, image, toolTip, images)), retain)
                   from configured in IO.lift(() => {
                       List<Action> actions = ThemeActions(ThemeNotifier(owner: null, drawable));
                       _ = actions.RemoveAll(action => !prior[drawable].Contains(action));
                       actions.Add(drawable.Invalidate);
                   })
                   select drawable;
        }, DisposalOps.Release);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "Get")]
    [return: UnsafeAccessorType("Rhino.UI.EtoExtensions+ThemeNotifier, Rhino.UI")]
    private static extern object ThemeNotifier([UnsafeAccessorType("Rhino.UI.EtoExtensions+ThemeNotifier, Rhino.UI")] object? owner, Control control);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Actions")]
    private static extern List<Action> ThemeActions([UnsafeAccessorType("Rhino.UI.EtoExtensions+ThemeNotifier, Rhino.UI")] object notifier);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "m_image")]
    private static extern ref Eto.Drawing.Bitmap Image(ButtonDrawable drawable);

    public bool ButtonDetails(int index, ref Bitmap iconOut, ref string sToolTipOut) {
        (bool Shown, Bitmap Icon, string ToolTip) held = (false, iconOut, sToolTipOut);
        (bool shown, iconOut, sToolTipOut) = Callbacks.Answer(
            buttons.At(index)
                .Traverse(static button => Icons.Raster((button.Glyph, IconSlot.SectionHeader.Master), IconSlot.SectionHeader).Map(icon => (Shown: true, Icon: icon, ToolTip: RowText.Localize(button.ToolTip).Local)))
                .As()
                .Map(found => found.IfNone(held)),
            () => held,
            new CallbackSite(sink, owner, nameof(ButtonDetails)));
        return shown;
    }

    public bool OnButtonClicked(int index) =>
        Callbacks.Succeeded(buttons.At(index).Map(static button => button.Click), static () => false, new CallbackSite(sink, owner, nameof(OnButtonClicked)));

    public Rectangle ButtonRect(int index, Rectangle rectHeader) =>
        Callbacks.Answer(IconSlot.SectionHeader.Size().Map(static size => new Rectangle(0, 0, size, size)), static () => Rectangle.Empty, new CallbackSite(sink, owner, nameof(ButtonRect)));

    public void DeleteThis() { }

    private sealed class HeaderDrawable(IHeaderButtonHandler header, int index, Eto.Drawing.Bitmap image, string toolTip, Retained<IDisposable> images)
        : ButtonDrawable(header, index, image, toolTip) {
        protected override void Dispose(bool disposing) {
            if (disposing)
                _ = IO.lift(() => base.Dispose(disposing)).Bind(_ => images.Close).Uninterruptible().Run();
            else
                base.Dispose(disposing);
        }
    }
}

public sealed class SectionLifetime : IDisposable {
    // --- [STATE]
    private readonly EtoCollapsibleSection section;
    private readonly IPlugInSink sink;
    private readonly Option<Guid> data;
    private readonly Func<bool> excluded;
    private readonly ShownSubscription realized;
    private Option<IDisposable> edges;
    private Option<ViewBody> body;
    private bool visible;

    public SectionLifetime(EtoCollapsibleSection section, IPlugInSink sink, Option<Guid> data, Func<bool> excluded, bool visible, Func<Action<IDisposable>, IO<ViewBody>> realize) {
        (this.section, this.sink, this.data, this.excluded, this.visible) = (section, sink, data, excluded, visible);
        realized = new ShownSubscription(retain =>
            from held in realize(retain)
            from placed in DisposalOps.Change(() => (body, section.Content), next => (body, section.Content) = next, (Some(held), held.Content), retain)
            from shown in IO.lift(() => this.visible).Bind(held.Shown)
            select unit);
        _ = ViewOps.WhileLoaded(section, Listening, new CallbackSite(sink, section.GetType(), nameof(Control.LoadComplete)), lease => edges = Some(lease)).Run();
    }

    public int Height => body.Match(Some: static held => held.Height, None: static () => 0);

    public bool Hidden => body.Exists(static held => held.Hidden);

    // --- [EDGES]
    public Option<IHeaderButtonHandler> Header(View.Section row) =>
        ViewBody.Header(row, realized.Shown(shown: true).Bind(_ => IO.lift(() => body)), sink);

    public IO<Unit> Reread => IO.lift(() => body).Bind(static held => held.Traverse(static current => current.Reread).As()).Map(unit);

    public IO<Unit> Visible(bool shown) =>
        IO.lift(() => {
            visible = shown;
            return body;
        }).Bind(held => held.Traverse(current => current.Shown(shown)).As()).Map(unit);

    private IO<Unit> Listening(Action<IDisposable> retain) =>
        Optional(section.Parent as Expander) switch {
            var expander => IO.lift(() => expander.ForAll(static held => held.Expanded) && !excluded()).Bind(realized.Shown) switch {
                var evaluated =>
                    from released in IO.lift(() => retain(realized))
                    from expansions in expander.Traverse(held => Subscriptions.Attach(
                        handler => held.ExpandedChanged += handler, handler => held.ExpandedChanged -= handler,
                        Callbacks.Handler<EventArgs>(_ => evaluated, new CallbackSite(sink, typeof(Expander), nameof(Expander.ExpandedChanged))), retain)).As()
                    from changes in data.Traverse(type => Subscriptions.Attach(
                        handler => section.DataChanged += handler, handler => section.DataChanged -= handler,
                        Callbacks.Handler<NativeDataSourceEventArgs>(args => when(args.DataType == type, evaluated).As(),
                            new CallbackSite(sink, typeof(EtoCollapsibleSection), nameof(EtoCollapsibleSection.DataChanged))), retain)).As()
                    from shows in Subscriptions.Attach(
                        handler => section.Shown += handler, handler => section.Shown -= handler,
                        Callbacks.Handler<EventArgs>(_ => evaluated, new CallbackSite(sink, typeof(Control), nameof(Control.Shown))), retain)
                    from current in evaluated
                    select unit,
            },
        };

    // --- [RELEASE]
    public void Dispose() {
        try {
            _ = edges.Iter(static lease => lease.Dispose());
        } finally {
            realized.Dispose();
        }
    }
}

public sealed class SectionHolder : EtoCollapsibleSectionHolder2, ICollapsibleSectionHolder2 {
    // --- [STATE]
    private readonly OrderedDictionary<IDisposable, Control> owned;
    private readonly EventHandler<EventArgs> tick;
    private Option<ICollapsibleSection> fullHeight;

    private SectionHolder(OrderedDictionary<IDisposable, Control> owned) {
        this.owned = owned;
        tick = TimerElapsed(m_timer)!;
        m_timer.Elapsed -= tick;
        _ = Subscriptions.Attach<EventHandler<EventArgs>>(
            handler => m_timer.Elapsed += handler, handler => m_timer.Elapsed -= handler,
            OnTick, lease => owned.TryAdd(lease, this)).Run();
    }

    private void OnTick(object? sender, EventArgs args) =>
        Callbacks.Started(DisposalOps.OnFailure(Native(IO.lift(() => {
            FullHeightIndex(this) = fullHeight.Match(
                Some: section => toSeq(m_sectionContainers).Filter(static container => !container.Interface.Hidden)
                    .TakeWhile(container => !ReferenceEquals(container.Interface, section)).Count,
                None: static () => -1);
            tick.Invoke(sender, args);
        }), held => owned.TryAdd(held, held)), Retired).Bind(_ => Retired), CallbackSite.Of(this));

    public new void SetFullHeightSection(ICollapsibleSection sec) =>
        _ = toSeq(m_sectionContainers).Find(container => ReferenceEquals(container.Interface, sec)).Iter(_ => fullHeight = Some(sec));

    // --- [NATIVE_SECTIONS]
    protected override void AddSectionToContainer(ICollapsibleSection section) =>
        _ = Native(
            from content in IO.lift(() => Refused.UnlessFound(InternalAdd(section), nameof(InternalAdd)))
            from retained in IO.lift(() => owned.TryAdd(section, content))
            from container in DisposalOps.Acquire(IO.lift(() => new HeldSectionContainer(section, content)), held => owned.TryAdd(held, held))
            from buttons in IO.lift(() => Optional(section as ICollapsibleSection2).Bind(static held => Optional(held.NewHeaderButtonHandler())))
            from images in buttons.Traverse(held => HeaderButtons.Realize(held, CallbackSite.Of(this), container.Owned.Retain)).As()
            from expanded in Subscriptions.Attach<EventHandler<EventArgs>>(
                handler => container.ExpandedChanged += handler, handler => container.ExpandedChanged -= handler,
                (sender, args) => ExpandedChanged(this, sender!, args), container.Owned.Retain)
            from loaded in Subscriptions.Attach<EventHandler<EventArgs>>(
                handler => container.Load += handler, handler => container.Load -= handler,
                (_, args) => ExpandedChanged(this, container, args), container.Owned.Retain)
            from configured in IO.lift(() => {
                _ = images.Iter(held => {
                    List<ButtonDrawable> controls = [.. held];
                    ((CollapsibleSectionContainerHandler)container.Handler).AddHeaderButtons(controls);
                    m_button_array.Add(section, controls);
                });
                NativeSections(this).Add(section);
                m_sectionContainers.Add(container);
                container.Expanded = IsExpanded(owner: null, section);
                if (UseCheckBoxes) {
                    StackLayout layout = new() {
                        Orientation = Orientation.Horizontal,
                        HorizontalContentAlignment = HorizontalAlignment.Left,
                        VerticalContentAlignment = VerticalAlignment.Top,
                    };
                    CheckBox checkBox = new();
                    layout.Items.Add(checkBox);
                    layout.Items.Add(new StackLayoutItem(container, expand: true));
                    if (!section.Hidden)
                        m_stack.Items.Add(new StackLayoutItem(layout));
                    object nativeBox = NewCheckBox(section.Caption.Local, checkBox);
                    _ = ((System.Collections.IList)CheckBoxes(this)).Add(nativeBox);
                    object listener = NewCheckBoxListener(this, new OnCheckboxCheckedHandler(OnCheckBoxClicked).Method.MethodHandle.GetFunctionPointer());
                    AttachCheckBoxListener(nativeBox, listener);
                    container.Owned.Retain(new Disposal<object>(listener, held => DetachCheckBoxListener(nativeBox, held)));
                } else if (!section.Hidden)
                    m_stack.Items.Add(new StackLayoutItem(container));
                _ = Optional(section as ICollapsibleSection2).Iter(held => held.OnAttachedToHolder(this));
            })
            select container, held => owned.TryAdd(held, held)).Run();

    // --- [HOLD]
    public static IO<SectionHolder> Hold(IPlugInViews owner, HolderRow holder, RowScope scope, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
            from stack in DisposalOps.Acquire(IO.lift(static () => {
                OrderedDictionary<IDisposable, Control> native = new(ReferenceEqualityComparer.Instance);
                return DisposalOps.OnFailure(
                    Native(IO.lift(() => new SectionHolder(native)), held => native.TryAdd(held, held)),
                    IO.lift(() => toSeq(native.Keys.ToArray())).Bind(DisposalOps.Release)).Run();
            }), owned)
            from sections in holder.Sections.TraverseM(row => Section(owner, row, scope, held => stack.owned.TryAdd(held, held))).As()
            from configured in IO.lift(() => {
                _ = sections.Iter(stack.Add);
                _ = holder.Switch(sections, stacked: static (_, _) => Option<DefinedSection>.None, filled: static (held, filled) => Some(held[filled.Above.Count]))
                    .Iter(stack.SetFullHeightSection);
            })
            select stack, retain);

    public static IO<DefinedSection> Section(IPlugInViews owner, View.Section row, RowScope scope, Action<DefinedSection> retain) =>
        DisposalOps.Acquire(owned =>
            from section in DisposalOps.Acquire(IO.lift(() => (DefinedSection)Activator.CreateInstance(row.Identity)!), owned)
            from attached in section.Attach(acquired => ViewBody.Realize(owner, row, scope, acquired))
            select section, (section, _) => retain(section));

    // --- [EDGES]
    public static new IO<Unit> Shown(Control content, bool visible) =>
        content switch {
            SectionHolder stack => Callbacks.Each(toSeq(stack.Sections).Choose(static section => Optional(section as EtoCollapsibleSection2))
                .Map(section => IO.lift(() => section.HolderVisible(visible)))).Map(unit),
            Container container => Callbacks.Each(toSeq(container.Controls).Map(child => Shown(child, visible))).Map(unit),
            _ => IO.pure(unit),
        };

    protected override void Dispose(bool disposing) {
        if (disposing && !IsDisposed) {
            Seq<Control> roots = Optional(Content).ToSeq().Add(m_stack).Distinct();
            Seq<Control> layouts = roots.Filter(root => !ReferenceEquals(root, m_stack));
            Seq<SectionContainer> containers = toSeq(m_sectionContainers.ToArray());
            IDisposable handler = (IDisposable)Handler;
            try {
                _ = Callbacks.Each(containers.Map(static container => IO.lift(container.Detach)))
                    .Bind(_ => IO.lift(Unbind)).Bind(_ => IO.lift(Detach)).Bind(_ => IO.lift(DetachNative))
                    .Bind(_ => IO.lift(() => {
                        Seq<IDisposable> released = toSeq(owned.Keys.ToArray()).Filter(resource => !roots.Exists(root => ReferenceEquals(root, resource)));
                        owned.Clear();
                        m_sectionContainers.Clear();
                        NativeSections(this).Clear();
                        m_button_array.Clear();
                        return released;
                    }))
                    .Bind(DisposalOps.Release).Uninterruptible().Run();
            } finally {
                SetDisposed(this, value: true);
                try {
                    base.Dispose(disposing);
                } finally {
                    _ = Callbacks.Each(Seq(DisposalOps.Release(layouts), IO.lift(() => { handler.Dispose(); SetHandler(this, value: null); }))).Uninterruptible().Run();
                }
            }
        } else
            base.Dispose(disposing);
    }

    private sealed class HeldSectionContainer(ICollapsibleSection section, Control content) : SectionContainer(section, content) {
        internal Retained<IDisposable> Owned { get; } = new(DisposalOps.Release);

        protected override void Dispose(bool disposing) {
            if (disposing && !IsDisposed) {
                IDisposable handler = (IDisposable)Handler;
                try {
                    _ = IO.lift(() => { Section(this) = null; Content = null; })
                        .Bind(_ => IO.lift(Unbind)).Bind(_ => IO.lift(Detach)).Bind(_ => IO.lift(DetachNative))
                        .Bind(_ => Owned.Close).Uninterruptible().Run();
                } finally {
                    SetDisposed(this, value: true);
                    try {
                        base.Dispose(disposing);
                    } finally {
                        handler.Dispose();
                        SetHandler(this, value: null);
                    }
                }
            } else
                base.Dispose(disposing);
        }
    }

    // --- [NATIVE_LIFECYCLE]
    private const string SectionCheckBoxType = "Rhino.UI.Controls.SectionCheckBox, Rhino.UI";
    private const string SectionCheckBoxListenerType = "Rhino.UI.Controls.SectionCheckBox+OnCheckboxCheckedHandler, Rhino.UI";

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "IsExpanded")]
    private static extern bool IsExpanded([UnsafeAccessorType("Rhino.UI.Controls.HolderUtil, Rhino.UI")] object? owner, ICollapsibleSection sec);

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    [return: UnsafeAccessorType(SectionCheckBoxType)]
    private static extern object NewCheckBox(string caption, CheckBox checkBox);

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    [return: UnsafeAccessorType(SectionCheckBoxListenerType)]
    private static extern object NewCheckBoxListener(object target, nint method);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "add_OnCheckBoxChecked")]
    private static extern void AttachCheckBoxListener([UnsafeAccessorType(SectionCheckBoxType)] object checkBox, [UnsafeAccessorType(SectionCheckBoxListenerType)] object listener);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "remove_OnCheckBoxChecked")]
    private static extern void DetachCheckBoxListener([UnsafeAccessorType(SectionCheckBoxType)] object checkBox, [UnsafeAccessorType(SectionCheckBoxListenerType)] object listener);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "m_checkboxes")]
    [return: UnsafeAccessorType("System.Collections.Generic.List`1[[" + SectionCheckBoxType + "]], System.Private.CoreLib")]
    private static extern ref object CheckBoxes(EtoCollapsibleSectionHolder holder);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "m_iFullHeightSection")]
    private static extern ref int FullHeightIndex(EtoCollapsibleSectionHolder2 holder);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "Elapsed")]
    private static extern ref EventHandler<EventArgs>? TimerElapsed(UITimer timer);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "m_section")]
    private static extern ref ICollapsibleSection? Section(SectionContainer container);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "m_sections")]
    private static extern ref List<ICollapsibleSection> NativeSections(EtoCollapsibleSectionHolder holder);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "OnExpandedChanged")]
    private static extern void ExpandedChanged(EtoCollapsibleSectionHolder holder, object sender, EventArgs args);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_IsDisposed")]
    private static extern void SetDisposed(Widget control, bool value);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_Handler")]
    private static extern void SetHandler(Widget control, object? value);

    // --- [LAYOUT]
    protected override void OnPreLoad(EventArgs e) {
        base.OnPreLoad(e);
        if (UseScrollbars && toSeq(Parents).Exists(static parent => parent is Scrollable))
            Callbacks.Run(DisposalOps.OnFailure(Native(IO.lift(() => { UseScrollbars = false; }), held => owned.TryAdd(held, held)), Retired).Bind(_ => Retired), CallbackSite.Of(this));
    }

    private IO<Unit> Retired =>
        from retired in IO.lift(() => {
            Seq<Control> roots = this.Cons(toSeq(m_sectionContainers).Map(static container => (Control)container));
            System.Collections.Generic.HashSet<Control> current = new(roots.Bind(static root => root.Cons(Optional(root as Container).ToSeq()
                .Bind(static container => toSeq(container.VisualChildren)))), ReferenceEqualityComparer.Instance);
            return toSeq(owned.ToArray()).Filter(held => !current.Contains(held.Value)).Map(static held => held.Key);
        })
        from released in DisposalOps.Release(retired)
        from removed in IO.lift(() => retired.Iter(held => owned.Remove(held)))
        select unit;

    private static IO<A> Native<A>(IO<A> body, Action<Control> retain) =>
        DisposalOps.Within<IDisposable, A>(owned => {
            EventHandler<WidgetCreatedEventArgs> created = (_, args) => {
                if (args.Instance is Control control and not SectionHolder and not SectionContainer and not ButtonDrawable)
                    _ = DisposalOps.Acquire(IO.lift(() => control), retain).Run();
            };
            return Subscriptions.Attach(
                static handler => Platform.Instance.WidgetCreated += handler, static handler => Platform.Instance.WidgetCreated -= handler, created, owned).Bind(_ => body);
        }, DisposalOps.Release);
}
