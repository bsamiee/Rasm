using System.Runtime.CompilerServices;
using Rhino.PlugIns;
using Rhino.Runtime;

namespace Rasm.Rhino.Events;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class NamedCallbacks {
    // --- [ANSWERS]
    private const string HandledEntry = "handled";

    public static Option<Unit> Handled(NamedParametersEventArgs args) =>
        Callbacks.Found(args.TryGetBool(HandledEntry, out bool handled) && handled, unit);

    // --- [REGISTRATION]
    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "_namedCallbacks")]
    private static extern ref Dictionary<string, EventHandler<NamedParametersEventArgs>> Handlers([UnsafeAccessorType("Rhino.Runtime.HostUtils, RhinoCommon")] object? owner);

    public static Func<PlugIn, IPlugInSink, IO<IDisposable>> Register(string name, Func<NamedParametersEventArgs, IO<Unit>> reply) =>
        (_, sink) =>
            from free in IO.lift(() => Taken.Unless(!Handlers(owner: null).ContainsKey(name), nameof(HostUtils.RegisterNamedCallback), name))
            let held = KeyValuePair.Create(name, Callbacks.Handler<NamedParametersEventArgs>(
                args => reply(args) >> IO.lift(() => args.Set(HandledEntry, value: true)),
                new CallbackSite(sink, typeof(HostUtils), name)))
            from registered in IO.lift(() => HostUtils.RegisterNamedCallback(held.Key, held.Value))
            select (IDisposable)new Disposal<KeyValuePair<string, EventHandler<NamedParametersEventArgs>>>(
                held,
                static pair => ((ICollection<KeyValuePair<string, EventHandler<NamedParametersEventArgs>>>)Handlers(owner: null)).Remove(pair));

    // --- [EXECUTION]
    public static IO<T> Execute<T>(string name, Func<NamedParametersEventArgs, Option<T>> read, params Seq<Action<NamedParametersEventArgs>> entries) =>
        (from args in use(static () => new NamedParametersEventArgs())
         from _ in IO.lift(() => entries.Iter(entry => entry(args)))
         from __ in IO.lift(() => Refused.Unless(HostUtils.ExecuteNamedCallback(name, args), name))
         from answer in IO.lift(() => read(args).ToFin(new Missing(name)))
         select answer).Bracket();
}
