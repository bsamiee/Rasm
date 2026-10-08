using System.Reflection;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rhino.Collections;
using Rhino.Runtime;

namespace Rasm.Rhino.Persistence;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record DictionaryOwner(IO<ArchivableDictionary> Read, Func<Func<ArchivableDictionary, Fin<Unit>>, IO<Unit>> Edit) {
    public static DictionaryOwner Live(CommonObject owner) =>
        Staged(IO.pure(owner), static _ => IO.pure(unit));

    public static DictionaryOwner Staged<TOwner>(IO<TOwner> copy, Func<TOwner, IO<Unit>> commit) where TOwner : CommonObject =>
        new(
            copy.Bind(static staged => ArchivableDictionaries.UserDictionary(staged)),
            edit =>
                from staged in copy
                from target in ArchivableDictionaries.UserDictionary(staged)
                from edited in IO.lift(() => edit(target))
                from committed in commit(staged)
                select committed);
}

public sealed record DictionaryCodec<T>(string Name, int Version, Func<T, Fin<ArchivableDictionary>> Encode, Func<ArchivableDictionary, Fin<T>> Decode) {
    public Fin<ArchivableDictionary> ToDictionary(T value) =>
        Encode(value).Map(encoded => {
            encoded.Version = Version;
            encoded.Name = Name;
            return encoded;
        });

    public Fin<T> FromDictionary(ArchivableDictionary stored) =>
        stored.Version <= Version ? Decode(stored) : new UnreadableRecord(Name, stored.Version, Version);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ArchivableDictionaries {
    // --- [VALUES]
    public static Fin<Option<T>> Find<T>(ArchivableDictionary source, string key) where T : notnull =>
        source.TryGetValue(key, out object held)
            ? held is T value ? Some(value) : new TypeMismatch(key, held.GetType(), typeof(T))
            : Option<T>.None;

    public static Fin<Option<T>> FindEnum<T>(ArchivableDictionary source, string key) where T : struct, Enum =>
        Find<string>(source, key).Bind(static text => text.Traverse(static held =>
            Enum.TryParse(held, ignoreCase: true, out T value) ? Fin.Succ(value) : new UnreadText(held, typeof(T))).As());

    public static Fin<T> Required<T>(ArchivableDictionary source, string key) where T : notnull =>
        Find<T>(source, key).Bind(held => held.ToFin(new Missing(key)));

    public static Fin<Seq<(string Key, ArchivableDictionary Value)>> Children(ArchivableDictionary source) =>
        Callbacks.Each(toSeq(source.Keys), (key, _) => Required<ArchivableDictionary>(source, key).Map(held => (key, held)));

    public static Fin<Unit> Set<T>(ArchivableDictionary target, string key, T value) where T : notnull =>
        Setter<T>.Set.Match(
            Some: set => Refused.Unless(set(target, key, value), nameof(ArchivableDictionary.Set)),
            None: () => Fin.Fail<Unit>(new UnstorableKind(key, typeof(T))));

    public static Fin<Unit> SetEnum<T>(ArchivableDictionary target, string key, T value) where T : struct, Enum =>
        Refused.Unless(target.SetEnumValue(key, value), nameof(ArchivableDictionary.SetEnumValue));

    public static Fin<ArchivableDictionary> Nested(Seq<(string Key, Fin<ArchivableDictionary> Value)> rows) =>
        from target in Fin.Succ(new ArchivableDictionary())
        from stored in Callbacks.Each(rows, (row, _) => row.Value.Bind(child => Set(target, row.Key, child)))
        select target;

    private static class Setter<T> {
        public static readonly Option<Func<ArchivableDictionary, string, T, bool>> Set =
            Optional(typeof(ArchivableDictionary).GetMethod(nameof(ArchivableDictionary.Set), BindingFlags.Public | BindingFlags.Instance | BindingFlags.ExactBinding, [typeof(string), typeof(T)]))
                .Map(static method => method.CreateDelegate<Func<ArchivableDictionary, string, T, bool>>());
    }

    // --- [USER_DICTIONARY]
    public static IO<ArchivableDictionary> UserDictionary(CommonObject owner) =>
        IO.lift(() => Missing.Unless(owner.UserDictionary, nameof(CommonObject.UserDictionary)));

    public static IO<Unit> Replace(DictionaryOwner owner, ArchivableDictionary source) =>
        owner.Edit(target => Fin.Succ(ignore(target.ReplaceContentsWith(source.Clone()))));

    // --- [RECORDS]
    public static IO<Option<T>> Read<T>(DictionaryOwner owner, DictionaryCodec<T> codec) =>
        owner.Read.Bind(source => IO.lift(() => Find<ArchivableDictionary>(source, codec.Name).Bind(stored => stored.Traverse(codec.FromDictionary).As())));

    public static IO<Unit> Write<T>(DictionaryOwner owner, DictionaryCodec<T> codec, T value) =>
        owner.Edit(target => codec.ToDictionary(value).Bind(record => Set(target, codec.Name, record)));

    public static IO<Unit> Remove<T>(DictionaryOwner owner, DictionaryCodec<T> codec) =>
        owner.Edit(target => Fin.Succ(ignore(target.Remove(codec.Name))));

    // --- [STORES]
    public static ValueStore<TValue> Store<TValue, TRaw, TError>(DictionaryOwner owner, ValueKey<TValue, TRaw, TError> key, Option<HostEvent<Unit>> changed)
        where TValue : notnull
        where TRaw : notnull =>
        ValueStore.Of(
            owner.Read.Bind(source => IO.lift(() => Find<TRaw>(source, key.Name).Bind(raw => raw.Traverse(key.From).As()))),
            value => owner.Edit(target => value.Match(
                Some: held => Set(target, key.Name, key.ToRaw(held)),
                None: () => Fin.Succ(ignore(target.Remove(key.Name))))),
            Applied.Live,
            changed);
}
