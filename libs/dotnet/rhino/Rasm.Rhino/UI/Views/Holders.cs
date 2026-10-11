using System.Drawing;
using System.Runtime.CompilerServices;
using Eto;
using Eto.Forms;
using NodaTime.Text;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rhino.UI.Controls;
using Rhino.UI;

namespace Rasm.Rhino.UI.Views;

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
public sealed class SectionBody : IDisposable {
    // --- [STATE]
    private readonly View.Section row;
    private readonly RowScope scope;
    private Seq<ControlRow> Controls => row.Children.Bind(static child => child.Controls);
    private RowRules Rules => new(row.Enabled, row.Visible);
    private readonly Atom<bool> hidden = Atom(value: false);
    private readonly ShownSubscription listeners;
    private readonly RowScope enclosing;
    private bool showing;

    private SectionBody(View.Section row, Control content, RowScope scope, RowScope enclosing, Option<IHeaderButtonHandler> header) {
        (this.row, Content, this.scope, this.enclosing, Header) = (row, content, scope, enclosing, header);
        listeners = new ShownSubscription(Listening);
    }

    public Control Content { get; }
    public Option<IHeaderButtonHandler> Header { get; }
    public bool Hidden => hidden.Value;
    public int Height => hidden.Value ? 0 : (int)Math.Ceiling(Content.GetPreferredSize().Height);

    // --- [REALIZATION]
    public static IO<SectionBody> Realize(IPlugInViews owner, View.Section row, RowScope enclosing, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
            from history in row.Store.Traverse(store => IO.lift(ViewOps.Bindings(owner, row, enclosing).Bind(bound => ViewOps.History(owner, row, store, bound, enclosing.Document)))).As()
            let scope = enclosing.Nested(ViewOps.Settings(owner, row), history, ViewOps.Locks(row, enclosing.Document))
            from hidden in scope.Shown(visible: false)
            let controls = row.Children.Bind(static child => child.Controls)
            from content in ViewOps.Realize(owner, row.Children, scope, owned)
            from header in history.Traverse(held => Buttons(owner, row, controls, held, scope, owned)).As()
            from section in DisposalOps.Acquire(IO.lift(() => new SectionBody(
                row, content, scope, enclosing, header.Map(static held => (IHeaderButtonHandler)held))), static held => IO.lift(held.Dispose), owned)
            from visibility in enclosing.OnVisibility(_ => section.Evaluated, owned)
            from evaluated in section.Evaluated
            select section, (_, release) => retain(release));

    // --- [EDGES]
    public IO<Unit> Shown(bool visible) => IO.lift(() => showing = visible)
        .Bind(_ => Callbacks.Each(Seq(listeners.Shown(visible), Evaluated))).Map(static _ => unit);

    public IO<Unit> Reread => scope.Changed(RealizedView.Keys(row));

    public void Dispose() {
        try {
            _ = scope.Shown(visible: false).Uninterruptible().Run();
        } finally {
            listeners.Dispose();
        }
    }

    // --- [RULES]
    private IO<Unit> Evaluated =>
        from shows in Rules.Shows(scope)
        from enables in Rules.Enables(scope)
        from swapped in hidden.SwapIO(_ => !shows)
        from set in IO.lift(() => Content.Enabled = enables)
        from visible in enclosing.Visibility
        from availability in scope.Shown(showing && visible && shows && enables)
        from sections in SectionHolder.Shown(Content, showing && visible && shows && enables)
        select unit;

    private IO<Unit> Listening(Action<IDisposable> retain) =>
        from signals in toSeq(Rules.Each.Bind(static rule => rule.Sources).Except(Controls.Choose(static control => control.Bound)))
            .TraverseM(source => source.Signals(scope)).As()
        let reads = toHashSet(Rules.Each.Bind(static rule => rule.Reads))
        from heard in scope.Listen(keys => when(keys.Exists(reads.Contains), Evaluated).As(), retain)
        from attached in signals.Flatten().TraverseM(signal => signal.Choose(static _ => Some((unit, unit)))
            .Through(Subscriptions.Idle<Unit, Unit>(_ => Evaluated), scope.Sink, retain)).As()
        from reread in Reread
        select unit;

    // --- [HEADER]
    private static IO<HeaderButtons> Buttons(IPlugInViews owner, View.Section row, Seq<ControlRow> controls, HistoryBinding history, RowScope scope, Action<IDisposable> retain) =>
        row.Presets.Map(listing => MenuButton(GlyphRole.Presets, "Presets", listing(scope), owner, retain)).ToSeq()
            .Add(MenuButton(GlyphRole.Options, "Values", Listing(owner, controls, history, scope), owner, retain))
            .TraverseM(static menu => menu).As().Map(menus => new HeaderButtons(
                menus.Add(new HeaderButton(GlyphRole.Reset, "Reset to defaults", scope.Commit(history.Group, history.Group.Defaults).Map(static _ => unit))),
                owner, row.Identity));

    private static IO<HeaderButton> MenuButton(GlyphRole glyph, string toolTip, IO<Seq<MenuPick>> listing, IPlugInSink sink, Action<IDisposable> retain) =>
        ChoiceRows.Menu(Seq<Seq<MenuEntry>>(), Seq<IO<Unit>>(), Some(listing), sink, retain)
            .Map(menu => new HeaderButton(glyph, toolTip, IO.lift(() => menu.Show())));

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

    private static MenuPick.Item Pick<T>(string caption, IO<T> run) => new(caption, Marked: false, run.Map(static _ => unit));

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
            from icon in Icons.Frames(sink, (button.Glyph, IconSlot.SectionHeader.Master), IconSlot.SectionHeader)
            from drawable in Create(this, index, icon.GetFrame(1f).Bitmap, RowText.Localize(button.ToolTip).Local, Ref(Seq<IDisposable>()), retain)
            from paint in Subscriptions.Attach(
                handler => drawable.Paint += handler, handler => drawable.Paint -= handler,
                Callbacks.Handler<PaintEventArgs>(_ => IO.lift(() => Image(drawable) = Icons.Frame(
                    sink, button.Glyph, IconSlot.SectionHeader.Master, new(drawable.Width, drawable.Height))
                    .GetFrame(drawable.ParentWindow.LogicalPixelSize, new(drawable.Width, drawable.Height)).Bitmap).Map(static _ => unit),
                    new CallbackSite(sink, owner, nameof(Drawable.Paint))), retain)
            select drawable).TraverseM(static drawable => drawable).As();

    internal static IO<Seq<ButtonDrawable>> Realize(IHeaderButtonHandler header, CallbackSite site, Action<IDisposable> retain) =>
        header is HeaderButtons authored ? authored.Realize(retain) : IO.lift(environment => {
            List<ButtonDrawable> controls = [];
            int index = 0;
            while (true) {
                Ref<Seq<IDisposable>> images = Ref(Seq<IDisposable>());
                IO<Option<(Eto.Drawing.Bitmap Image, string ToolTip)>> read(int buttonIndex, Size pixels, Action<IDisposable> owned) =>
                    DisposalOps.Within(() => new Bitmap(pixels.Width, pixels.Height), seed =>
                        IO.lift(() => {
                            Bitmap bitmap = seed;
                            string toolTip = "";
                            return header.ButtonDetails(buttonIndex, ref bitmap, ref toolTip) ? Some((Bitmap: bitmap, ToolTip: toolTip)) : None;
                        }).Bind(found => found.Traverse(value => DisposalOps.Acquire(IO.lift(value.Bitmap.ToEto), static held => IO.lift(held.Dispose), owned)
                            .Map(bitmap => (Image: bitmap, value.ToolTip))).As()), static seed => IO.lift(seed.Dispose));
                Option<(Eto.Drawing.Bitmap Image, string ToolTip)> initial = DisposalOps.Acquire(owned =>
                    IconSlot.SectionHeader.Size().Bind(size => read(index, new(size, size), owned)),
                    (found, release) => {
                        if (found.IsSome)
                            _ = atomic(() => images.Value = Seq(release));
                        else
                            release.Dispose();
                    }).Run(environment);
                if (initial.Case is not ValueTuple<Eto.Drawing.Bitmap, string> value)
                    break;
                ButtonDrawable drawable = Create(header, index, value.Item1, value.Item2, images, retain).Run(environment);
                int buttonIndex = index;
                _ = Subscriptions.Attach(handler => drawable.Paint += handler, handler => drawable.Paint -= handler,
                    Callbacks.Handler<PaintEventArgs>(_ => DisposalOps.Replace(owned => {
                        Eto.Drawing.Size pixels = Eto.Drawing.Size.Ceiling((Eto.Drawing.SizeF)drawable.Size * drawable.ParentWindow.LogicalPixelSize);
                        return read(buttonIndex, new(pixels.Width, pixels.Height), owned);
                    }, (found, release) => found.Match(
                        Some: value => {
                            Eto.Drawing.Bitmap priorImage = Image(drawable);
                            string priorTip = drawable.ToolTip;
                            Seq<IDisposable> prior = atomic(() => { Seq<IDisposable> held = images.Value; images.Value = held.Add(release); return held; });
                            IO<Unit> rejected = IO.lift(() => atomic(() => images.Value = prior)).Bind(_ => IO.lift(release.Dispose));
                            IO<Unit> restore = IO.lift(() => { Image(drawable) = priorImage; drawable.ToolTip = priorTip; });
                            return DisposalOps.OnFailure(IO.lift(() => { Image(drawable) = value.Image; drawable.ToolTip = value.ToolTip; }), restore.Bind(_ => rejected))
                                .Bind(_ => IO.lift(() => atomic(() => images.Value = Seq(release))))
                                .Bind(_ => DisposalOps.Release(prior));
                        },
                        None: () => DisposalOps.Release(Seq(release)))).Map(static _ => unit), site), retain).Run(environment);
                controls.Add(drawable);
                index++;
            }
            return toSeq(controls);
        });

    private static IO<ButtonDrawable> Create(IHeaderButtonHandler header, int index, Eto.Drawing.Bitmap image, string toolTip, Ref<Seq<IDisposable>> images, Action<IDisposable> retain) =>
        DisposalOps.Within<IDisposable, ButtonDrawable>(owned => {
            Dictionary<ButtonDrawable, System.Collections.Generic.HashSet<Action>> prior = [];
            EventHandler<WidgetCreatedEventArgs> created = (_, args) => {
                if (args.Instance is not ButtonDrawable drawable) return;

                drawable.Tag = images;
                prior.Add(drawable, [.. ThemeActions(ThemeNotifier(owner: null, drawable))]);
            };
            return from attached in Subscriptions.Attach(
                static handler => Platform.Instance.WidgetCreated += handler, static handler => Platform.Instance.WidgetCreated -= handler, created, owned)
                   from drawable in DisposalOps.Acquire(IO.lift<ButtonDrawable>(() => new HeaderDrawable(header, index, image, toolTip, images)), static drawable => IO.lift(drawable.Dispose), retain)
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
                .Traverse(button => Icons.Raster(sink, (button.Glyph, IconSlot.SectionHeader.Master), IconSlot.SectionHeader).Map(icon => (Shown: true, Icon: icon, ToolTip: RowText.Localize(button.ToolTip).Local)))
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

    private sealed class HeaderDrawable(IHeaderButtonHandler header, int index, Eto.Drawing.Bitmap image, string toolTip, Ref<Seq<IDisposable>> images)
        : ButtonDrawable(header, index, image, toolTip) {
        protected override void Dispose(bool disposing) {
            if (disposing)
                _ = IO.lift(() => base.Dispose(disposing))
                    .Bind(_ => IO.lift(() => atomic(() => { Seq<IDisposable> released = images.Value; images.Value = []; return released; })))
                    .Bind(DisposalOps.Release).Uninterruptible().Run();
            else
                base.Dispose(disposing);
        }
    }
}

public sealed class SectionHolder : EtoCollapsibleSectionHolder2, ICollapsibleSectionHolder2 {
    // --- [STATE]
    private readonly OrderedDictionary<IDisposable, Control> owned;
    private Option<ICollapsibleSection> fullHeight;

    private SectionHolder(OrderedDictionary<IDisposable, Control> owned) {
        this.owned = owned;
        tick = TimerElapsed(m_timer)!;
        m_timer.Elapsed -= tick;
        _ = Subscriptions.Attach<EventHandler<EventArgs>>(
            handler => m_timer.Elapsed += handler, handler => m_timer.Elapsed -= handler,
            OnTick, lease => owned.TryAdd(lease, this)).Run();
    }

    private readonly EventHandler<EventArgs> tick;

    private void OnTick(object? sender, EventArgs args) =>
        Callbacks.Run(DisposalOps.OnFailure(Native(IO.lift(() => {
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
            from content in IO.lift(() => Missing.Unless(InternalAdd(section), nameof(InternalAdd)))
            from retained in IO.lift(() => owned.TryAdd(section, content))
            from container in DisposalOps.Acquire(IO.lift(() => new HeldSectionContainer(section, content)), static held => IO.lift(held.Dispose), held => owned.TryAdd(held, held))
            from buttons in IO.lift(() => Optional(section as ICollapsibleSection2).Bind(static held => Optional(held.NewHeaderButtonHandler())))
            from images in buttons.Traverse(held => HeaderButtons.Realize(held, CallbackSite.Of(this), container.Retain)).As()
            from expanded in Subscriptions.Attach<EventHandler<EventArgs>>(
                handler => container.ExpandedChanged += handler, handler => container.ExpandedChanged -= handler,
                (sender, args) => ExpandedChanged(this, sender!, args), container.Retain)
            from loaded in Subscriptions.Attach<EventHandler<EventArgs>>(
                handler => container.Load += handler, handler => container.Load -= handler,
                (_, args) => ExpandedChanged(this, container, args), container.Retain)
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
                    container.Retain(new Disposal<object>(listener, held => DetachCheckBoxListener(nativeBox, held)));
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
            }), static held => IO.lift(held.Dispose), owned)
            from sections in holder.Sections.TraverseM(row => Section(owner, row, scope, held => stack.owned.TryAdd(held, held))).As()
            from configured in IO.lift(() => {
                _ = sections.Iter(stack.Add);
                _ = holder.Switch(sections, stacked: static (_, _) => Option<DefinedSection>.None, filled: static (held, filled) => Some(held[filled.Above.Count]))
                    .Iter(stack.SetFullHeightSection);
            })
            select stack, (_, release) => retain(release));

    public static IO<DefinedSection> Section(IPlugInViews owner, View.Section row, RowScope scope, Action<DefinedSection> retain) =>
        DisposalOps.Acquire(owned =>
            from section in DisposalOps.Acquire(IO.lift(() => (DefinedSection)Activator.CreateInstance(row.Identity)!), static held => IO.lift(held.Dispose), owned)
            from attached in section.Attach(acquired => SectionBody.Realize(owner, row, scope, acquired))
            select section, (section, _) => retain(section));

    // --- [EDGES]
    public static new IO<Unit> Shown(Control content, bool visible) =>
        content switch {
            RealizedView view => view.Shown(visible),
            SectionHolder stack => Callbacks.Each(toSeq(stack.Sections).Choose(static section => Optional(section as EtoCollapsibleSection2))
                .Map(section => IO.lift(() => section.HolderVisible(visible)))).Map(static _ => unit),
            Container container => Callbacks.Each(toSeq(container.Controls).Map(child => Shown(child, visible))).Map(static _ => unit),
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
        private Seq<IDisposable> owned;

        internal void Retain(IDisposable resource) => owned = owned.Add(resource);

        protected override void Dispose(bool disposing) {
            if (disposing && !IsDisposed) {
                IDisposable handler = (IDisposable)Handler;
                try {
                    _ = IO.lift(() => { Section(this) = null; Content = null; })
                        .Bind(_ => IO.lift(Unbind)).Bind(_ => IO.lift(Detach)).Bind(_ => IO.lift(DetachNative))
                        .Bind(_ => IO.lift(() => {
                            Seq<IDisposable> released = owned;
                            owned = [];
                            return released;
                        })).Bind(DisposalOps.Release).Uninterruptible().Run();
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
                    _ = DisposalOps.Acquire(IO.lift(() => control), static held => IO.lift(held.Dispose), retain).Run();
            };
            return Subscriptions.Attach(
                static handler => Platform.Instance.WidgetCreated += handler, static handler => Platform.Instance.WidgetCreated -= handler, created, owned).Bind(_ => body);
        }, DisposalOps.Release);
}
