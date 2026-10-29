using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rhino.Runtime;

namespace Rasm.Rhino;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class NamedCallbacks {
    // --- [REGISTRATION]
    private const string HandledEntry = "handled";

    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "_namedCallbacks")]
    private static extern ref Dictionary<string, EventHandler<NamedParametersEventArgs>> Handlers([UnsafeAccessorType("Rhino.Runtime.HostUtils, RhinoCommon")] object? owner);

    public static IO<Unit> Register(string name, Func<NamedParametersEventArgs, IO<Unit>> reply) =>
        IO.lift(() => RhinoFault.Unless(!Handlers(owner: null).ContainsKey(name), RhinoReason.Taken, name))
        >> Subscriptions.Inline<NamedParametersEventArgs>(handler => HostUtils.RegisterNamedCallback(name, handler), _ => HostUtils.RemoveNamedCallback(name))
            .Attach(args => reply(args) >> IO.lift(() => args.Set(HandledEntry, value: true)));

    // --- [EXECUTION]
    public static IO<T> Execute<T>(string name, Func<NamedParametersEventArgs, Option<T>> read, params Seq<Action<NamedParametersEventArgs>> entries) =>
        (from args in use(static () => new NamedParametersEventArgs())
         from entered in IO.lift(() => entries.Iter(entry => entry(args)))
         from executed in Products.Accepted(() => HostUtils.ExecuteNamedCallback(name, args))
         from value in IO.lift(() => Conversions.Required(read(args)))
         select value).Bracket();

    public static Option<Unit> Handled(NamedParametersEventArgs args) =>
        Conversions.Present(args.TryGetBool(HandledEntry, out bool handled) && handled, unit);
}
