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
        from parent in envIO
        from held in use(static () => Ref(Option<EnvIO>.None), static held => held.Value.Iter(static scope => scope.Dispose()))
        from followed in follow.IfNone(notify => Hooks.On(h => Written += h, h => Written -= h, notify))(
            from value in Read
            from scope in IO.lift(() => {
                EnvIO current = parent.Local;
                Option<EnvIO> prior = atomic(() => (Prior: held.Value, Next: held.Swap(_ => Some(current))).Prior);
                _ = prior.Iter(static stale => stale.Dispose());
                return current;
            })
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

    public static Preference<T> Property<T>(Func<T> read, Action<T> write, Option<(Action<EventHandler> Add, Action<EventHandler> Remove)> changed = default) =>
        new(
            IO.lift(read),
            value => IO.lift(() => write(value)),
            changed.Map<Func<IO<Unit>, IO<Unit>>>(static subscription => notify => Hooks.On(subscription.Add, subscription.Remove, notify)));

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

    private static Preference<T> Keyed<T>(Func<KeyedValues> values, GrasshopperIO.Name key, T fallback, Func<Action, IO<Unit>> commit) where T : notnull {
        string name = key.ToString();
        return new(
            IO.lift(() => Unpacked<T>(values().Get(name, NTuple.Empty)).IfNone(fallback)),
            value => commit(() => values().Set(name, Packed(value))),
            Some<Func<IO<Unit>, IO<Unit>>>(notify => IO.lift(values).Bind(held => Hooks.On<KeyValueEventArgs>(
                h => held.ValueChanged += h,
                h => held.ValueChanged -= h,
                args => when(string.Equals(args.Key, name, StringComparison.OrdinalIgnoreCase), notify).As()))));
    }

    // --- [CODEC]
    private const string Element = "Value";

    private static NTuple Packed<T>(T value) where T : notnull =>
        new((Element, typeof(T).IsEnum ? Garden.IPear(Convert.ToInt32(value, CultureInfo.InvariantCulture)) : Garden.Pear(value)));

    private static Option<T> Unpacked<T>(NTuple tuple) where T : notnull =>
        typeof(T).IsEnum
            ? tuple.Get(Element, StringComparison.Ordinal, out int number) ? (T)Enum.ToObject(typeof(T), number) : None
            : tuple.Get(Element, StringComparison.Ordinal, out T stored) ? stored : None;
}
