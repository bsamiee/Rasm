using System.Reflection;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rhino.Collections;
using Rhino.Runtime;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Persistence;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record DictionaryOwner(IO<ArchivableDictionary> Read, Func<Func<ArchivableDictionary, Fin<Unit>>, IO<Unit>> Edit) {
    public static DictionaryOwner Of<TOwner>(IO<TOwner> acquire, Option<Func<TOwner, IO<Unit>>> commit = default) where TOwner : CommonObject =>
        new(
            acquire.Bind(ArchivableDictionaries.UserDictionary),
            (from edit in Eff.runtime<Func<ArchivableDictionary, Fin<Unit>>>()
             from owner in acquire
             from dictionary in ArchivableDictionaries.UserDictionary(owner)
             from edited in IO.lift(() => edit(dictionary))
             from committed in commit.Traverse(write => write(owner)).As()
             select edited).RunIO);
}

public sealed record DictionaryCodec<T>(string Name, int Version, Func<T, Fin<ArchivableDictionary>> Encode, Func<ArchivableDictionary, Fin<T>> Decode) {
    public Fin<ArchivableDictionary> ToDictionary(T value) =>
        Encode(value).Map(encoded => {
            DictionaryMapper.Stamp((Name, Version), encoded);
            return encoded;
        });

    public Fin<T> FromDictionary(ArchivableDictionary stored) =>
        stored.Version <= Version ? Decode(stored) : new UnreadableRecord(Name, stored.Version, Version);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
internal static partial class DictionaryMapper {
    internal static partial void Stamp((string Name, int Version) header, ArchivableDictionary target);
}

public static class ArchivableDictionaries {
    // --- [VALUES]
    public static Fin<Option<T>> Find<T>(ArchivableDictionary source, string key) where T : notnull =>
        source.TryGetValue(key, out object held) ? TypeMismatch.Unless<T>(held, key).Map(Some) : Option<T>.None;

    public static Fin<Option<T>> FindEnum<T>(ArchivableDictionary source, string key) where T : struct, Enum =>
        Find<string>(source, key).Bind(static text => text.Traverse(static held =>
            Enum.TryParse(held, ignoreCase: true, out T value) ? Fin.Succ(value) : new UnreadText(held, typeof(T))).As());

    public static Fin<T> Required<T>(ArchivableDictionary source, string key) where T : notnull =>
        from found in Find<T>(source, key)
        from value in found.ToFin(new Missing(key))
        select value;

    public static Fin<Seq<(string Key, ArchivableDictionary Value)>> Children(ArchivableDictionary source) =>
        Callbacks.Each(Conversions.Rows(source), static (row, _) => TypeMismatch.Unless<ArchivableDictionary>(row.Value, row.Key).Map(value => (row.Key, value)));

    public static Fin<Unit> Set<T>(ArchivableDictionary target, string key, T value) where T : notnull =>
        Setter<T>.Set.Match(
            Some: set => Refused.Unless(set(target, key, value), nameof(ArchivableDictionary.Set)),
            None: () => Fin.Fail<Unit>(new UnstorableKind(key, typeof(T))));

    public static Fin<Unit> SetEnum<T>(ArchivableDictionary target, string key, T value) where T : struct, Enum =>
        Refused.Unless(target.SetEnumValue(key, value), nameof(ArchivableDictionary.SetEnumValue));

    public static Fin<ArchivableDictionary> Nested(Seq<(string Key, Fin<ArchivableDictionary> Value)> rows) =>
        from target in Fin.Succ(new ArchivableDictionary())
        from stored in rows.Traverse(row =>
            (from child in row.Value
             from written in Set(target, row.Key, child)
             select written).ToValidation()).As().ToFin()
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
        from source in owner.Read
        from stored in IO.lift(() => Find<ArchivableDictionary>(source, codec.Name))
        from value in IO.lift(() => stored.Traverse(codec.FromDictionary).As())
        select value;

    public static IO<Unit> Write<T>(DictionaryOwner owner, DictionaryCodec<T> codec, T value) =>
        from record in IO.lift(() => codec.ToDictionary(value))
        from written in owner.Edit(target => Set(target, codec.Name, record))
        select written;

    public static IO<Unit> Remove<T>(DictionaryOwner owner, DictionaryCodec<T> codec) =>
        owner.Edit(target => Fin.Succ(ignore(target.Remove(codec.Name))));

    // --- [STORES]
    public static ValueStore<TValue> Store<TValue, TRaw, TError>(DictionaryOwner owner, ValueKey<TValue, TRaw, TError> key, Option<HostEvent<Unit>> changed)
        where TValue : notnull
        where TRaw : notnull =>
        ValueStore.Of(
            from source in owner.Read
            from raw in IO.lift(() => Find<TRaw>(source, key.Name))
            from value in IO.lift(() => raw.Traverse(key.From).As())
            select value,
            (from value in Eff.runtime<Option<TValue>>()
             from written in owner.Edit(value.Match(
                 Some: held => fun((ArchivableDictionary target) => Set(target, key.Name, key.ToRaw(held))),
                 None: fun((ArchivableDictionary target) => Fin.Succ(ignore(target.Remove(key.Name))))))
             select written).RunIO,
            Applied.Live,
            changed);
}
