using System.Runtime.CompilerServices;
using Rhino.Runtime;

namespace Rasm.Rhino.Events;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class NamedCallbacks {
    // --- [REGISTRATION]
    private const string HandledEntry = "handled";

    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "_namedCallbacks")]
    private static extern ref Dictionary<string, EventHandler<NamedParametersEventArgs>> Handlers([UnsafeAccessorType("Rhino.Runtime.HostUtils, RhinoCommon")] object? owner);

    public static Func<IPlugInSink, Action<IDisposable>, IO<Unit>> Register(string name, Func<NamedParametersEventArgs, IO<Unit>> reply) =>
        (sink, retain) => IO.lift(() => Handlers(owner: null).ContainsKey(name) ? Error.New($"{nameof(HostUtils.RegisterNamedCallback)} already holds {name}") : Fin.Succ(unit))
            >> Subscriptions.Host<NamedParametersEventArgs>(typeof(HostUtils), handler => HostUtils.RegisterNamedCallback(name, handler),
                _ => HostUtils.RemoveNamedCallback(name), name)
                .Inline(args => reply(args) >> IO.lift(() => args.Set(HandledEntry, value: true)), sink, retain);

    // --- [EXECUTION]
    public static IO<T> Execute<T>(string name, Func<NamedParametersEventArgs, Option<T>> read, params Seq<Action<NamedParametersEventArgs>> entries) =>
        DisposalOps.Within(static () => new NamedParametersEventArgs(),
            args => IO.lift(() => entries.Iter(entry => entry(args))) >> IO.lift(() => Refused.Unless(HostUtils.ExecuteNamedCallback(name, args), name)) >> IO.lift(() => read(args).ToFin(new Refused(name, Refused.Absent))));

    public static Option<Unit> Handled(NamedParametersEventArgs args) =>
        Callbacks.Found(args.TryGetBool(HandledEntry, out bool handled) && handled, unit);
}
