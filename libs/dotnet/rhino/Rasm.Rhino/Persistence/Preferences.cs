using System.Collections.Generic;
using System.Drawing;
using System.Text.Json;
using LanguageExt.ClassInstances;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.DocObjects;
using Rhino.PlugIns;

namespace Rasm.Rhino.Persistence;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class Preference<T> {
    private static readonly Func<T, T, bool> Equal =
        typeof(T) == typeof(Color) ? (Func<T, T, bool>)(object)new Func<Color, Color, bool>(Conversions.Same) : EqualityComparer<T>.Default.Equals;

    private readonly Func<T, IO<Unit>> write;
    private readonly Option<HostEvent<Unit>> signal;
    private readonly Func<T, T, bool> same;

    internal Preference(IO<T> read, Func<T, IO<Unit>> write, Option<HostEvent<Unit>> signal, Option<Func<T, T, bool>> same = default) {
        Read = read;
        this.write = write;
        this.signal = signal;
        this.same = same.IfNone(Equal);
    }

    private event EventHandler? Written;

    public IO<T> Read { get; }

    public IO<Unit> Write(T value) =>
        from held in Read
        from written in unless(same(held, value), write(value) >> when(signal.IsNone, IO.lift(() => Written?.Invoke(this, EventArgs.Empty))).As()).As()
        select unit;

    public IO<IO<Unit>> Swap(T value) =>
        from prior in Read
        from written in Write(value)
        select Write(prior);

    public IO<Unit> Changed(Func<T, IO<Unit>> react) =>
        signal.IfNone(() => Subscriptions.Inline<EventHandler, EventArgs>(handler => Written += handler, handler => Written -= handler, static handler => handler.Invoke).Choose(static _ => Some(unit)))
            .Attach(_ => Read.Bind(react));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Preferences {
    // --- [SIGNALS]
    public static readonly HostEvent<Unit> AppSettingsChanged =
        Subscriptions.On<EventHandler, EventArgs>(static h => RhinoApp.AppSettingsChanged += h, static h => RhinoApp.AppSettingsChanged -= h, static handler => handler.Invoke).Choose(static _ => Some(unit));

    // --- [HOST]
    public static Preference<T> Of<T>(IO<T> read, Func<T, IO<Unit>> write, Option<HostEvent<Unit>> changed = default) => new(read, write, changed);

    public static Preference<T> Property<T>(Func<T> read, Action<T> write, Option<HostEvent<Unit>> changed = default) =>
        new(IO.lift(read), value => IO.lift(() => write(value)), changed);

    public static Preference<T> State<TState, T>(Func<TState> current, Action<TState> update, Func<TState, T> read, Action<TState, T> write) where TState : class =>
        new(IO.lift(() => read(current())), value => IO.lift(() => {
            TState state = current();
            write(state, value);
            update(state);
        }), Some(AppSettingsChanged));

    // --- [SETTINGS]
    public static Preference<T> Setting<T>(SettingsNode node, string key, T fallback, Option<Func<T, bool>> rule = default, bool hidden = false, Seq<string> legacy = default) where T : notnull =>
        new(
            from settings in Registered(node, key, fallback, rule, hidden)
            select SettingOverloads<T>.Read(settings, key, legacy).IfNone(fallback),
            value =>
                from settings in Registered(node, key, fallback, rule, hidden)
                from writable in IO.lift(() => RhinoFault.Unless(!(settings.TryGetSettingIsReadOnly(key, out bool locked) && locked), RhinoReason.Unwritable, key))
                from written in IO.lift(() => SettingOverloads<T>.Write(settings, key, value))
                from flushed in when(node.Root is SettingsRoot.ApplicationNode, IO.lift(static () => PlugIn.FlushSettingsSavedQueue())).As()
                select unit,
            Some(node.Saved));

    private static IO<PersistentSettings> Registered<T>(SettingsNode node, string key, T fallback, Option<Func<T, bool>> rule, bool hidden) where T : notnull =>
        from supported in IO.lift(SettingOverloads<T>.Supported)
        from settings in node.Created
        from registered in IO.lift(() => SettingOverloads<T>.Registered(settings, key, fallback, rule))
        from concealed in when(hidden, IO.lift(() => settings.HideSettingFromUserInterface(key))).As()
        select settings;

    // --- [DOCUMENTS]
    public static Preference<T> Dictionary<T>(DictionaryOwner owner, string key, T fallback, Option<HostEvent<Unit>> changed = default) where T : notnull =>
        new(
            from source in owner.Read
            from held in IO.lift(() => ArchivableDictionaries.Find<T>(source, key))
            select held.IfNone(fallback),
            value => owner.Edit(target => ArchivableDictionaries.Set(target, key, value)),
            changed);

    public static Preference<T> Document<T>(RhinoDoc doc, DocumentKey key, T fallback) where T : notnull =>
        new(
            UserStrings.Value(doc, key).Map(text => Decoded(key.Text, text, fallback)),
            value => UserStrings.Write(doc, key, Encoded(value, fallback)).Map(static _ => unit),
            Some(Subscriptions.On<RhinoDoc.UserStringChangedArgs>(static h => RhinoDoc.UserStringChanged += h, static h => RhinoDoc.UserStringChanged -= h)
                .Documents(static args => args.Document).In(doc.RuntimeSerialNumber).Choose(static _ => Some(unit))));

    public static Preference<T> Viewport<T>(RhinoDoc doc, ViewportTarget target, string key, T fallback) where T : notnull =>
        Viewports.Member(
            doc, target,
            port => IO.lift(() => Decoded(key, Conversions.Present(port.GetUserString(key)), fallback)),
            (port, value) => UserStrings.Write(port, toHashMap<EqStringOrdinalIgnoreCase, string, Option<string>>(Seq((key, Encoded(value, fallback))))) >> IO.lift(() => { doc.Modified = true; }));

    public static Preference<T> Object<T>(RhinoObject owner, string key, T fallback) where T : notnull =>
        (Doc: owner.Document, owner.Id) switch {
            var (doc, id) => new(
                IO.lift(() => Conversions.Required(doc.Objects.FindId(id))).Map(found => Decoded(key, Conversions.Present(found.Attributes.GetUserString(key)), fallback)),
                value =>
                    from found in IO.lift(() => Conversions.Required(doc.Objects.FindId(id)))
                    from staged in IO.lift(() => found.Attributes.Duplicate())
                    from strung in UserStrings.Write(UserStrings.Of(staged.GetUserStrings()), staged.SetUserString, toHashMap<EqStringOrdinalIgnoreCase, string, Option<string>>(Seq((key, Encoded(value, fallback)))))
                    from modified in Products.Accepted(() => doc.Objects.ModifyAttributes(found, staged, quiet: true))
                    select unit,
                Some(Subscriptions.On<RhinoModifyObjectAttributesEventArgs>(static h => RhinoDoc.ModifyObjectAttributes += h, static h => RhinoDoc.ModifyObjectAttributes -= h)
                    .Choose(args => Conversions.Present(args.RhinoObject.Id == id && args.Document.RuntimeSerialNumber == doc.RuntimeSerialNumber, unit)))),
        };

    // --- [LIFETIME]
    public static IO<IO<Unit>> Swap(params Seq<IO<IO<Unit>>> swaps) =>
        swaps.Fold(
                IO.pure(Seq<IO<Unit>>()),
                static (applied, swap) => applied.Bind(done => swap.Map(restore => restore.Cons(done))
                    .Catch(error => Restored(done).Map(_ => error).Catch(restoring => IO.pure(error + restoring)).Bind(static failure => IO.fail<Seq<IO<Unit>>>(failure)))))
            .Map(Restored);

    public static IO<Unit> Hold(params Seq<IO<IO<Unit>>> swaps) =>
        Swap(swaps).Bind(static restore => Products.Released(() => Session.Run(restore))).Scoped(Session.Scope);

    private static IO<Unit> Restored(Seq<IO<Unit>> restores) => Products.Each(restores).Map(static _ => unit);

    // --- [CODEC]
    private static T Decoded<T>(string key, Option<string> text, T fallback) where T : notnull =>
        text.Bind(held => ArchivableDictionaries.Decoded(key, () => JsonSerializer.Deserialize<T>(held, ArchivableDictionaries.Options)).ToOption()).IfNone(fallback);

    private static Option<string> Encoded<T>(T value, T fallback) where T : notnull =>
        (JsonSerializer.Serialize(value, ArchivableDictionaries.Options), JsonSerializer.Serialize(fallback, ArchivableDictionaries.Options)) switch {
            var (text, absent) => Conversions.Present(!string.Equals(text, absent, StringComparison.Ordinal), text),
        };
}
