using System.Runtime.CompilerServices;
using Rhino.PlugIns;
using Rhino.Runtime;

namespace Rasm.Rhino.Events;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class NamedCallbacks {
    // --- [REGISTRATION]
    private const string HandledEntry = "handled";

    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "_namedCallbacks")]
    private static extern ref Dictionary<string, EventHandler<NamedParametersEventArgs>> Handlers([UnsafeAccessorType("Rhino.Runtime.HostUtils, RhinoCommon")] object? owner);

    public static Func<PlugIn, IPlugInSink, IO<IDisposable>> Register(string name, Func<NamedParametersEventArgs, IO<Unit>> reply) =>
        (_, sink) => IO.lift(() => Taken.Unless(!Handlers(owner: null).ContainsKey(name), nameof(HostUtils.RegisterNamedCallback), name))
            >> Subscriptions.Host<NamedParametersEventArgs>(typeof(HostUtils), handler => HostUtils.RegisterNamedCallback(name, handler),
                handler => ((ICollection<KeyValuePair<string, EventHandler<NamedParametersEventArgs>>>)Handlers(owner: null)).Remove(new(name, handler)), name)
                .Inline(args => reply(args) >> IO.lift(() => args.Set(HandledEntry, value: true)), sink);

    // --- [EXECUTION]
    public static IO<T> Execute<T>(string name, Func<NamedParametersEventArgs, Option<T>> read, params Seq<Action<NamedParametersEventArgs>> entries) =>
        use(static () => new NamedParametersEventArgs())
            .Bind(args => IO.lift(() => entries.Iter(entry => entry(args))) >> IO.lift(() => Refused.Unless(HostUtils.ExecuteNamedCallback(name, args), name)) >> IO.lift(() => read(args).ToFin(new Missing(name))))
            .Bracket();

    public static Option<Unit> Handled(NamedParametersEventArgs args) =>
        Callbacks.Found(args.TryGetBool(HandledEntry, out bool handled) && handled, unit);
}
