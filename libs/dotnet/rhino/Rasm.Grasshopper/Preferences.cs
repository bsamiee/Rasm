using System.Collections.Generic;
using System.Globalization;
using Grasshopper2;
using Grasshopper2.Data;
using Grasshopper2.Doc;
using Grasshopper2.Types;
using Grasshopper2.UI;
using Grasshopper2.Undo.Actions;
using Rasm.Grasshopper.Doc;

namespace Rasm.Grasshopper;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class Preference<T> {
    private readonly Func<T, IO<Unit>> write;
    private readonly Option<Func<IO<Unit>, IO<Unit>>> follow;

    internal Preference(IO<T> read, Func<T, IO<Unit>> write, Option<Func<IO<Unit>, IO<Unit>>> follow) {
        Read = read;
        this.write = write;
        this.follow = follow;
    }

    private event EventHandler? Written;

    public IO<T> Read { get; }

    public IO<Unit> Write(T value) => write(value) >> when(follow.IsNone, IO.lift(() => Written?.Invoke(this, EventArgs.Empty))).As();

    public IO<IO<Unit>> Swap(T value) =>
        from prior in Read
        from written in Write(value)
        select Write(prior);

    public IO<Unit> Changed(Func<T, IO<Unit>> react) =>
        from held in use(static () => Atom(Option<EnvIO>.None), static held => held.Value.Iter(static scope => scope.Dispose()))
        from followed in follow.IfNone(notify => Hooks.On(h => Written += h, h => Written -= h, notify))(
            from value in Read
            from scope in IO.lift(static () => EnvIO.New(token: CancellationToken.None))
            from prior in IO.lift(() => held.Value)
            from swapped in IO.lift(() => held.Swap(_ => Some(scope)))
            from released in IO.lift(() => prior.Iter(static stale => stale.Dispose()))
            from reacted in IO.liftVAsync(() => react(value).RunAsync(scope))
            select unit)
        select unit;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Preferences {
    // --- [HOST]
    public static Preference<T> Of<T>(Settings.Setting<T> setting) =>
        new(
            IO.lift(() => setting.Value),
            value => IO.lift(() => { setting.Value = value; }),
            Some<Func<IO<Unit>, IO<Unit>>>(notify => Hooks.On<EventArgs>(h => setting.Changed += h, h => setting.Changed -= h, _ => notify)));

    public static Preference<T> Property<T>(Func<T> read, Action<T> write, Action<EventHandler>? add = null, Action<EventHandler>? remove = null) =>
        new(
            IO.lift(read),
            value => IO.lift(() => write(value)),
            add is { } adding && remove is { } removing ? Some<Func<IO<Unit>, IO<Unit>>>(notify => Hooks.On(adding, removing, notify)) : None);

    // --- [STORES]
    public static Preference<T> Plugin<T>(GrasshopperIO.Name key, T fallback) where T : notnull =>
        Keyed(static () => Hooks.Session.Values, key, fallback, static set => IO.lift(set) >> Hooks.Saved);

    public static Preference<T> Object<T>(IDocumentObject owner, GrasshopperIO.Name key, T fallback, Nomen face) where T : notnull =>
        Keyed(() => owner.CustomValues, key, fallback, set =>
            IO.lift(() => Optional(owner.Document)).Bind(document => document.Match(
                Some: placed => Edits.Commit(placed, ("Set", face.Name), actions => IO.lift(() => {
                    actions.Add(new CustomValueAction(owner, key.ToString()));
                    set();
                })),
                None: () => IO.lift(set))));

    private static Preference<T> Keyed<T>(Func<KeyedValues> values, GrasshopperIO.Name key, T fallback, Func<Action, IO<Unit>> commit) where T : notnull =>
        new(
            IO.lift(() => Unpacked<T>(values().Get(key.ToString(), NTuple.Empty)).IfNone(fallback)),
            value => IO.lift(() => Packed(value)).Bind(tuple => commit(() => values().Set(key.ToString(), tuple))),
            Some<Func<IO<Unit>, IO<Unit>>>(notify => IO.lift(values).Bind(held => Hooks.On<KeyValueEventArgs>(
                h => held.ValueChanged += h,
                h => held.ValueChanged -= h,
                args => when(string.Equals(args.Key, key.ToString(), StringComparison.OrdinalIgnoreCase), notify).As()))));

    // --- [CODEC]
    private const string Element = "Value";

    private static Fin<NTuple> Packed<T>(T value) where T : notnull =>
        new NTuple((Element, typeof(T).IsEnum ? Garden.IPear(Convert.ToInt32(value, CultureInfo.InvariantCulture)) : Garden.Pear(value))) switch {
            var tuple => GrasshopperFault.Unless(
                    Unpacked<T>(GrasshopperIO.IO.ReadStorableFromByteArray<NTuple>(GrasshopperIO.IO.WriteStorableToByteArray(tuple))).Exists(stored => EqualityComparer<T>.Default.Equals(stored, value)),
                    GrasshopperReason.Unreadable,
                    value)
                .Map(_ => tuple),
        };

    private static Option<T> Unpacked<T>(NTuple tuple) where T : notnull =>
        typeof(T).IsEnum
            ? tuple.Get(Element, StringComparison.Ordinal, out int number) ? (T)Enum.ToObject(typeof(T), number) : None
            : tuple.Get(Element, StringComparison.Ordinal, out T stored) ? stored : None;
}
