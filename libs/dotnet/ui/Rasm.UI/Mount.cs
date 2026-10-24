using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Host(SKSize Size, Theme Theme, bool Cycles, bool Shown, bool Active);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Notice {
    public sealed record Announced(string Text, Live Live) : Notice;

    public sealed record ValueChanged(Seq<Key> Element) : Notice;

    public sealed record FocusChanged(Seq<Key> Element) : Notice;

    public sealed record LayoutChanged : Notice;

    public sealed record Detent : Notice;
}

public sealed record Output(Seq<SKRect> Damage, Seq<Notice> Notices, Option<IO<Unit>> Answer);

public sealed record Mount {
    // --- [STATE]
    private readonly HashMap<Seq<Key>, Element> elements;

    private Mount(Host host, Scene scene) => (Host, Scene, Routing, Tree) = (host, scene, Routing.Idle, Access.Project(scene));

    public Host Host { get; private init; }
    public Scene Scene { get; private init; }
    public Routing Routing { get; private init; }
    public HashMap<Seq<Key>, Grip> Grips { get; private init; }
    public HashMap<(Seq<Key> Path, Interaction Interaction), Animated> Fades { get; private init; }
    public Option<(Seq<Key> Path, Editor Editor)> Editing { get; private init; }

    public Seq<Element> Tree {
        get;
        private init {
            field = value;
            elements = Access.Indexed(value);
        }
    }

    // --- [DERIVED]
    public Cursor Cursor =>
        (Routing.Captured | Routing.Hover).Bind(path => Scene.Adjustable(path)
                ? Scene.Index.Find(path.Init).Bind(static laid => laid.Node is Node.Split split ? Some(split.Axis) : None).Map(static axis => axis.Resize)
                : Owner(path).Map(owner => owner.Part.Cursor(owner.Laid, path, Grips.Find(owner.Path))))
            .IfNone(Cursor.Default);

    public bool Animating => Host.Shown && (!Fades.IsEmpty || (!Host.Theme.Display.ReduceMotion && Scene.Order.Exists(static laid => laid.Timed)));

    public Option<string> Help => Routing.Hover.Bind(path => Scene.Above(path).Choose(elements.Find).Choose(static element => element.Accessible.Help).Head);

    public Option<Seq<Key>> Focused =>
        Routing.Focus.Bind(focus => Scene.Above(Scene.Index.Find(focus).Bind(Lead).IfNone(focus)).Find(elements.ContainsKey));

    public HashMap<(Seq<Key> Path, Interaction Interaction), float> Washes(Duration now) =>
        Active(Routing) switch {
            var active => toHashMap(active.ToSeq().Map(key => (Key: key, Weight: Fades.Find(key).Map(fade => fade.At(now).Value).IfNone(1f)))
                .Concat(toSeq(Fades.Filter((key, _) => !active.Contains(key)).AsIterable()).Map(entry => (entry.Key, Weight: entry.Value.At(now).Value)))
                .Filter(static entry => entry.Weight > 0f)),
        };

    // --- [TRANSITIONS]
    public static Fin<(Mount Mount, Output Output)> Create(Host host, Shipped shipped, Node view) =>
        Scene.Of(host.Theme, shipped, view, host.Size, []).Map(static scene => Revealed(None, scene).Scene).Map(scene =>
            (new Mount(host, scene), new Output(Seq(SKRect.Create(host.Size)), [], Watched(None, scene, Routing.Idle))));

    public Fin<(Mount Mount, Output Output)> Show(Node view, Duration now) =>
        Scene.Reconcile(view).Map(moved => Settled((this with { Scene = moved.Scene }).Unanswered with { Damage = moved.Damage }, now));

    public (Mount Mount, Output Output) Rehost(Host host, Duration now) =>
        (Host.Theme == host.Theme ? (Scene, Seq<SKRect>()) : Scene.Restyle(host.Theme)) switch {
            var (styled, restyled) => (styled.Layout.Frame.Size == host.Size ? (styled, Seq<SKRect>()) : styled.Resize(host.Size)) switch {
                var (sized, resized) => Settled((this with { Host = host, Scene = sized }).Unanswered with {
                    Damage = restyled.Concat(resized).Concat(Host.Active == host.Active ? [] : Routing.Focus.Bind(sized.Bounds).ToSeq()),
                }, now),
            },
        };

    public (Mount Mount, Output Output) Receive(Input input, Duration now) => Settled(Dispatched(input), now);

    public (Mount Mount, Output Output) Tick(Duration now) =>
        (this with { Fades = Fades.Filter(fade => fade.At(now).Continue) }, new Output(Fades.Keys.ToSeq().Map(static key => key.Path).Concat(Scene.Order.Filter(static laid => laid.Timed).Map(static laid => laid.Path)).Choose(Scene.Bounds), [], None));

    private (Mount Mount, Output Output) Settled(Routed routed, Duration now) {
        (Scene revealed, Seq<SKRect> damage) = ReferenceEquals(Scene, routed.Mount.Scene) ? (routed.Mount.Scene, routed.Damage) : Revealed(Some(Scene), routed.Mount.Scene) switch {
            var held => (held.Scene, routed.Damage.Concat(held.Damage)),
        };
        Mount next = routed.Mount with { Scene = revealed };
        Routing routing = next.Routing.Pruned(next.Scene);
        HashMap<Seq<Key>, Grip> dropped = next.Grips.Filter((path, grip) => !next.Scene.Index.Find(path).Exists(laid => laid.Node.Part.IsSome || grip is Grip.Adjust));
        Option<IO<Unit>> cancelled = Part.Joined(toSeq(dropped.AsIterable()).Map(entry => Owning(entry.Key).Bind(owner =>
            owner.Part.Receive(new Delivery(new Input.Perform(new Act.Cancel(), Modifiers.Plain, Some(entry.Key)), entry.Key, owner.Bounds, owner.Laid, Some(entry.Value), None)).Answer)));
        LanguageExt.HashSet<(Seq<Key> Path, Interaction Interaction)> was = Active(Routing);
        LanguageExt.HashSet<(Seq<Key> Path, Interaction Interaction)> active = Active(routing);
        Seq<(Seq<Key> Path, Interaction Interaction)> changed = was.Except(active).Union(active.Except(was)).ToSeq();
        HashMap<(Seq<Key> Path, Interaction Interaction), Animated> fades = changed.Fold(
            next.Fades.Filter((key, _) => next.Scene.Index.ContainsKey(key.Path)),
            (held, key) => next.Scene.Index.Find(key.Path).Map(static laid => laid.Style.Look.Motion).IfNone(new TimingCurve.Instant()) switch {
                var curve => (active.Contains(key) ? 1f : 0f) switch {
                    var target => held.Find(key).Map(fade => fade.Toward(target, now, curve)).IfNone(new Animated(1f - target, target, now, curve)) switch {
                        var fade when fade.At(now).Continue => held.AddOrUpdate(key, fade),
                        _ => held.Remove(key),
                    },
                },
            });
        Mount settled = (ReferenceEquals(Scene, next.Scene) ? next : next with { Tree = Access.Project(next.Scene) }) with {
            Routing = routing,
            Grips = next.Grips.RemoveRange(dropped.Keys),
            Fades = fades,
            Editing = next.Editing.Filter(held => routing.Focus == Some(held.Path) && next.Scene.Index.Find(held.Path).Exists(static laid => laid is Laid.Text && laid.Node.Part.Exists(static part => part.Edits))),
        };
        Option<IO<Unit>> watched = ReferenceEquals(Scene, settled.Scene) && Routing.Hover == routing.Hover ? None : Watched(Some((Scene, Routing)), settled.Scene, routing);
        return (settled, new Output(
            damage.Concat(changed.Choose(key => next.Scene.Bounds(key.Path))).Concat(routing.Focus != Routing.Focus ? Seq(Routing.Focus, routing.Focus).Somes().Choose(next.Scene.Bounds) : []),
            (ReferenceEquals(Scene, next.Scene) ? [] : Access.Changes(elements, settled.elements))
                .Concat(settled.Focused != Focused ? settled.Focused.Map(static path => (Notice)new Notice.FocusChanged(path)).ToSeq() : [])
                .Concat(settled.Grips.Exists((path, grip) => grip.Snapped.IsSome && Grips.Find(path).Bind(static prior => prior.Snapped) != grip.Snapped) ? Seq<Notice>(new Notice.Detent()) : []),
            Part.Joined(routed.Answer, cancelled, watched)));
    }

    // --- [ROUTE]
    private sealed record Owned(Seq<Key> Path, Laid Laid, Part Part, SKRect Bounds);

    private sealed record Routed(Mount Mount, Seq<SKRect> Damage, Option<IO<Unit>> Answer, Option<Seq<Key>> By) {
        public Routed Then(Func<Mount, Routed> next) =>
            next(Mount) switch {
                var after => new(after.Mount, Damage.Concat(after.Damage), Part.Joined(Answer, after.Answer), By | after.By),
            };

        public Routed Routes(Func<Routing, Routing> change) => this with { Mount = Mount with { Routing = change(Mount.Routing) } };

        public Routed Handled() => this with { Answer = Some(Part.Handled) };
    }

    private Routed Unanswered => new(this, [], None, None);

    // --- [DISPATCH]
    private Routed Dispatched(Input input) =>
        input.Switch(
            this,
            pointer: static (mount, pointer) => mount.Pointed(pointer, mount.Hits(pointer.At)),
            menu: static (mount, menu) => mount.Aimed(mount.Hits(menu.At), over => new Input.Perform(new Act.Menu(), menu.Modifiers, Some(over))),
            scroll: static (mount, scroll) => mount.Scrolled(scroll, mount.Hits(scroll.At)),
            pan: static (mount, pan) => mount.Aimed(mount.Hits(pan.At), _ => pan),
            magnify: static (mount, magnify) => mount.Aimed(mount.Hits(magnify.At), _ => magnify),
            rotate: static (mount, rotate) => mount.Aimed(mount.Hits(rotate.At), _ => rotate),
            perform: static (mount, perform) => perform.Target.Match(Some: target => mount.Targeted(perform, target), None: () => mount.Bubbled([perform], Some(perform))),
            edit: static (mount, edit) => mount.Bubbled(Seq<Input>(edit) + edit.Otherwise.Map(static otherwise => (Input)otherwise).ToSeq(), edit.Otherwise),
            focus: static (mount, focus) => mount.Routing.Focus.Bind(mount.Owner).Match(Some: owner => mount.Delivered(focus, owner.Path, owner), None: () => mount.Unanswered),
            enter: static (mount, enter) => mount.Entered(enter.Toward));

    private Routed Pointed(Input.Pointer pointer, Seq<Seq<Key>> hits) =>
        Routing.Captured.Match(
            Some: captured => pointer.Phase is PointerPhase.Leave
                ? Unanswered.Routes(static routing => routing with { Hover = None })
                : Dragged(pointer, captured).IfNone(() => Aimed(hits, _ => pointer)) switch {
                    var carried when pointer.Phase is PointerPhase.Up => carried.Then(static mount => (mount with { Grips = mount.Grips.Filter(static grip => grip is not Grip.Adjust) }).Unanswered)
                        .Routes(routing => routing with { Captured = None, Hover = captured }).Then(mount => mount.Hovered(pointer, hits)),
                    var carried => carried.Routes(routing => routing with { Hover = hits.Find(hit => hit == captured) }),
                },
            None: () => Hovered(pointer, hits).Then(mount => pointer.Phase switch {
                PointerPhase.Down => mount.Pressed(pointer, hits),
                PointerPhase.Leave => mount.Unanswered,
                PointerPhase.Move or PointerPhase.Up => mount.Chained(hits, _ => pointer),
            }));

    private Routed Hovered(Input.Pointer pointer, Seq<Seq<Key>> hits) =>
        (pointer.Phase is PointerPhase.Leave ? None : hits.Find(hit => Scene.Adjustable(hit) || Owner(hit).IsSome)) switch {
            var hover => Routing.Hover.Bind(Owner).Filter(prior => hover.Bind(Owner).ForAll(next => next.Path != prior.Path))
                .Match(Some: owner => Delivered(pointer with { Phase = PointerPhase.Leave }, owner.Path, owner), None: () => Unanswered)
                .Routes(routing => routing with { Hover = hover }),
        };

    private Routed Pressed(Input.Pointer pointer, Seq<Seq<Key>> hits) =>
        (from handle in hits.Head.Filter(Scene.Adjustable) from start in Scene.Adjustment(handle) select (Handle: handle, Start: start)).Match(
            Some: held => (this with { Grips = Grips.AddOrUpdate(held.Handle, new Grip.Adjust(pointer.At, held.Start)) }).Unanswered.Handled()
                .Routes(routing => routing with { Captured = held.Handle }),
            None: () => Chained(hits, _ => pointer) switch {
                var routed => routed.By.Match(
                    Some: by => routed.Routes(routing => routing with { Captured = by }).Then(pressed => pressed.Refocused(pressed.Owner(by).Map(static owner => owner.Path))),
                    None: () => routed),
            });

    private Option<Routed> Dragged(Input.Pointer pointer, Seq<Key> captured) =>
        from adjust in Grips.Find(captured).Bind(static grip => Optional(grip as Grip.Adjust))
        from axis in Scene.Index.Find(captured.Init).Bind(Axis)
        let travel = axis.Of(pointer.At - adjust.From)
        select Changed(Scene.Adjust(captured, adjust.Start + travel - Scene.Adjustment(captured).IfNone(adjust.Start))).Handled();

    private Routed Aimed(Seq<Seq<Key>> hits, Func<Seq<Key>, Input> input) =>
        Routing.Captured.Match(
            Some: captured => Owner(captured).Match(
                Some: owner => Over(hits, owner) switch {
                    var over => Delivered(input(over), over, owner),
                },
                None: () => Unanswered),
            None: () => Chained(hits, input));

    private Routed Scrolled(Input.Scroll scroll, Seq<Seq<Key>> hits) =>
        (scroll.Phase, Routing.Captured.IsSome, Routing.Latched) switch {
            (GesturePhase.Begins, false, { IsSome: true }) => Unanswered.Routes(static routing => routing with { Latched = None }).Then(mount => mount.Scrolled(scroll, hits)),
            (_, true, _) => Aimed(hits, _ => scroll),
            (_, _, var latched) => latched.Match(
                Some: path => Scene.Index.Find(path).Bind(Scrolling).Match(
                    Some: axis => Changed(Wheel(path, axis, scroll)).Handled(),
                    None: () => Owner(path).Match(Some: owner => Delivered(scroll, Over(hits, owner), owner), None: () => Unanswered)),
                None: () => Chained(hits, _ => scroll) switch {
                    var routed when routed.By.IsSome => routed.Routes(routing => routing with { Latched = routed.By.Bind(routed.Mount.Owner).Map(static owner => owner.Path) }),
                    var routed => routed.Then(mount => mount.Wheeled(hits, scroll)),
                }),
        };

    private Routed Wheeled(Seq<Seq<Key>> hits, Input.Scroll scroll) =>
        (from path in hits.Head.Map(Scene.Above).IfNone([])
         from axis in Scene.Index.Find(path).Bind(Scrolling).ToSeq()
         let moved = Changed(Wheel(path, axis, scroll))
         where moved.Answer.IsSome
         select moved.Routes(routing => routing with { Latched = path })).Head.IfNone(Unanswered);

    private Routed Targeted(Input.Perform perform, Seq<Key> target) =>
        perform.Act is Act.Reveal
            ? Changed(Scene.Reveal(target))
            : (from laid in Scene.Index.Find(target)
               from axis in Axis(laid)
               from delta in perform.Act.SwitchPartially(
                   (Laid: laid, Axis: axis),
                   @default: static (_, _) => Option<float>.None,
                   step: static (held, step) => step.Along == held.Axis ? Some(step.Count * held.Laid.Style.Metrics[Metric.Spacing]) : None,
                   page: static (held, page) => page.Along == held.Axis && held.Laid.Node is Node.Scroll ? Some(page.Count * held.Axis.Of(held.Laid.Frame.Size)) : None)
               select delta).Match(
                Some: delta => Changed(Scene.Adjust(target, delta)),
                None: () => Owner(target).Match(Some: owner => Delivered(perform, target, owner), None: () => Unanswered));

    private Routed Bubbled(Seq<Input> forms, Option<Input.Perform> otherwise) =>
        (from focus in Routing.Focus.ToSeq() from owner in Owners(focus) from form in forms select (Focus: focus, Owner: owner, Form: form))
            .Fold(Unanswered, static (routed, held) => routed.Answer.IsSome ? routed : routed.Mount.Delivered(held.Form, held.Focus, held.Owner)) switch {
                var routed when routed.Answer.IsNone => otherwise.Bind(perform => perform.Act.SwitchPartially(
                    (Mount: this, routed.Mount.Routing),
                    @default: static (_, _) => Option<Seq<Key>>.None,
                    step: static (held, step) => held.Routing.Toward(held.Mount.Scene, step.Along, int.Sign(step.Count)),
                    tab: static (held, tab) => held.Routing.Next(held.Mount.Scene, tab.Toward, held.Mount.Host.Cycles))).Match(Some: stop => routed.Then(mount => mount.Refocused(Some(stop))).Handled(), None: () => routed),
                var routed => routed,
            };

    private Routed Entered(Heading toward) =>
        (this with { Routing = Routing with { Focus = None } }) switch {
            var entered => entered.Refocused(entered.Routing.Next(Scene, toward, cycles: true)),
        };

    private Routed Refocused(Option<Seq<Key>> stop) =>
        stop == Routing.Focus
            ? Unanswered
            : Routing.Focus.Bind(Owner).Match(Some: owner => Delivered(new Input.Focus(Gained: false), owner.Path, owner), None: () => Unanswered)
                .Routes(routing => routing with { Focus = stop })
                .Then(mount => stop.Bind(mount.Owner).Match(Some: owner => mount.Delivered(new Input.Focus(Gained: true), owner.Path, owner), None: () => mount.Unanswered));

    private Routed Chained(Seq<Seq<Key>> hits, Func<Seq<Key>, Input> input) =>
        toSeq(hits.Choose(hit => Owner(hit).Map(owner => (Hit: hit, Owner: owner))).DistinctBy(static held => held.Owner.Path))
            .Fold(Unanswered, (routed, held) => routed.Answer.IsSome
                ? routed
                : routed.Mount.Delivered(input(held.Hit), held.Hit, held.Owner) switch {
                    var delivered => delivered with { By = delivered.Answer.IsSome ? Some(held.Hit) : None },
                });

    private Routed Delivered(Input input, Seq<Key> over, Owned owner) =>
        Opening(input, owner).Match(
            Some: opened => Editing.Bind(held => Owner(held.Path)).Match(Some: open => Received(new Input.Focus(Gained: false), open.Path, open), None: () => Unanswered)
                .Then(mount => (mount with { Editing = Some((owner.Path, new Editor(opened.Node, opened.Laid))) }).Received(input, over, owner)),
            None: () => Received(input, over, owner));

    private Option<(Node.Text Node, Laid.Text Laid)> Opening(Input input, Owned owner) =>
        owner.Part.Edits && input is Input.Pointer { Phase: PointerPhase.Down } or Input.Focus { Gained: true } && !Editing.Exists(held => held.Path == owner.Path)
            && owner.Laid is Laid.Text { Node: Node.Text node } laid
            ? Some((node, laid))
            : None;

    private Routed Received(Input input, Seq<Key> over, Owned owner) {
        Option<Editor> editor = Editing.Filter(held => held.Path == owner.Path).Map(static held => held.Editor);
        (Option<Grip> grip, Option<IO<Unit>> answer) = owner.Part.Receive(new Delivery(input, over, owner.Bounds, owner.Laid, Grips.Find(owner.Path), editor));
        Mount next = this with {
            Grips = grip.Match(Some: held => Grips.AddOrUpdate(owner.Path, held), None: () => Grips.Remove(owner.Path)),
            Editing = input is Input.Focus { Gained: false } && editor.IsSome ? None : Editing,
        };
        return new Routed(next, owner.Part.Edits && answer.IsSome ? Scene.Bounds(owner.Path).ToSeq() : [], answer, None);
    }

    private Routed Changed((Scene Scene, Seq<SKRect> Damage) next) =>
        next switch {
            var (scene, _) when scene.Adjusted == Scene.Adjusted => Unanswered,
            var (scene, damage) => new Routed(this with { Scene = scene }, damage, Some(Part.Handled), None),
        };

    private (Scene Scene, Seq<SKRect> Damage) Wheel(Seq<Key> path, Orientation axis, Input.Scroll scroll) => Scene.Adjust(path, -axis.Of(scroll.Delta));

    // --- [WATCH]
    private static (Scene Scene, Seq<SKRect> Damage) Revealed(Option<Scene> prior, Scene scene) =>
        scene.Order.Choose(laid => Lead(laid).Filter(lead => prior.Bind(held => held.Index.Find(laid.Path)).Bind(Lead) != Some(lead)))
            .Fold((Scene: scene, Damage: Seq<SKRect>()), static (held, lead) => held.Scene.Reveal(lead) switch {
                var (shown, damage) => (shown, held.Damage.Concat(damage)),
            });

    private static Option<IO<Unit>> Watched(Option<(Scene Scene, Routing Routing)> prior, Scene scene, Routing routing) =>
        Part.Joined(scene.Order.Map(laid =>
            from part in laid.Node.Part
            let before = from held in prior
                         from was in held.Scene.Index.Find(laid.Path)
                         where was.Node.Part.IsSome
                         select new Sight(was, InView(held.Scene), held.Routing.Hover)
            from answer in part.Watched(before, new Sight(laid, InView(scene), routing.Hover))
            select answer));

    private static Option<Seq<Key>> Lead(Laid laid) =>
        from part in laid.Node.Part
        from lead in part.Revealed(laid)
        select lead;

    private static Func<Seq<Key>, bool> InView(Scene scene) => path => scene.Bounds(path).Exists(static bounds => !bounds.IsEmpty);

    // --- [LOOKUP]
    private Seq<Seq<Key>> Hits(SKPoint at) => Scene.Hit(at).Map(static hit => hit.Laid.Path);

    private Option<Owned> Owner(Seq<Key> path) => Owners(path).Head;

    private Seq<Owned> Owners(Seq<Key> path) => Scene.Above(path).Choose(Owning);

    private Option<Owned> Owning(Seq<Key> prefix) =>
        from laid in Scene.Index.Find(prefix)
        from part in laid.Node.Part
        from bounds in Scene.Frame(prefix)
        select new Owned(prefix, laid, part, bounds);

    private static Seq<Key> Over(Seq<Seq<Key>> hits, Owned owner) => hits.Find(hit => hit.Take(owner.Path.Count) == owner.Path).IfNone(owner.Path);

    private static Option<Orientation> Axis(Laid laid) => laid.Node is Node.Split split ? Some(split.Axis) : Scrolling(laid);

    private static Option<Orientation> Scrolling(Laid laid) => laid.Node is Node.Scroll scroll ? Some(scroll.Axis) : None;

    private static LanguageExt.HashSet<(Seq<Key> Path, Interaction Interaction)> Active(Routing routing) =>
        toHashSet(routing.Hover.Map(static path => (path, Interaction.Hover)).ToSeq().Concat(routing.Captured.Map(static path => (path, Interaction.Pressed)).ToSeq()));
}
