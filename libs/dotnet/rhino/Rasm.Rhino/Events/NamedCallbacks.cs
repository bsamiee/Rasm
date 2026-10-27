using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rhino.Runtime;

namespace Rasm.RhinoCommon.Events;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class NamedCallbacks {
    // --- [REGISTRATION]
    private const string HandledEntry = "handled";

    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "_namedCallbacks")]
    private static extern ref Dictionary<string, EventHandler<NamedParametersEventArgs>> Handlers([UnsafeAccessorType("Rhino.Runtime.HostUtils, RhinoCommon")] object? owner);

    public static IO<Unit> Register(string name, Func<NamedParametersEventArgs, IO<Unit>> reply, object owner) =>
        IO.lift(() => Handlers(owner: null).ContainsKey(name) ? new RhinoFault(name, RhinoReason.Taken) : Fin.Succ(unit))
        >> Subscriptions.Host<NamedParametersEventArgs>(handler => HostUtils.RegisterNamedCallback(name, handler), _ => HostUtils.RemoveNamedCallback(name))
            .Inline(args => reply(args) >> IO.lift(() => args.Set(HandledEntry, value: true)), owner);

    // --- [EXECUTION]
    public static IO<T> Execute<T>(string name, Func<NamedParametersEventArgs, Option<T>> read, params Seq<Action<NamedParametersEventArgs>> entries) =>
        (from args in use(static () => new NamedParametersEventArgs())
         from entered in IO.lift(() => entries.Iter(entry => entry(args)))
         from executed in IO.lift(() => RhinoFault.Unless(HostUtils.ExecuteNamedCallback(name, args)))
         from value in IO.lift(() => RhinoFault.Found(read(args)))
         select value).Bracket();

    public static Option<Unit> Handled(NamedParametersEventArgs args) =>
        Conversions.Found(args.TryGetBool(HandledEntry, out bool handled) && handled, unit);
}
