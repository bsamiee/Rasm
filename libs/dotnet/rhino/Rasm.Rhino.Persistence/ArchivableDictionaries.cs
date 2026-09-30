using Rasm.Rhino.Document;
using Rhino.Collections;
using Rhino.Runtime;

namespace Rasm.Rhino.Persistence;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IArchivedRecord<TSelf> where TSelf : IArchivedRecord<TSelf> {
    public static abstract string DictionaryName { get; }

    public static abstract int DictionaryVersion { get; }

    public static abstract IO<Unit> Encode(TSelf value, ArchivableDictionary target);

    public static abstract Fin<TSelf> Decode(ArchivableDictionary source);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ArchivableDictionaries {
    // --- [READS]
    public static IO<Option<T>> Find<T>(ArchivableDictionary source, string key) where T : notnull =>
        IO.lift(() => source.TryGetValue(key, out object? data)
            ? data is T value ? Fin.Succ(Some(value)) : Fin.Fail<Option<T>>(new TypeMismatch(key, data, typeof(T)))
            : Option<T>.None);

    public static IO<Option<T>> FindEnum<T>(ArchivableDictionary source, string key) where T : struct, Enum =>
        IO.lift(() => Answers.Found(source.TryGetEnumValue(key, out T value), value));

    // --- [WRITES]
    public static IO<Unit> SetEnum<T>(ArchivableDictionary target, string key, T value) where T : struct, Enum =>
        IO.lift(() => Refused.Unless(target.SetEnumValue(key, value), nameof(ArchivableDictionary.SetEnumValue)));

    // --- [RECORDS]
    public static IO<Option<T>> Read<T>(CommonObject owner) where T : IArchivedRecord<T> =>
        from dictionary in AttachedData.UserDictionary(owner)
        from stored in Find<ArchivableDictionary>(dictionary, T.DictionaryName)
        from record in IO.lift(stored.Traverse(static held => T.Decode(held)).As())
        select record;

    public static IO<Unit> Write<T>(CommonObject owner, T value) where T : IArchivedRecord<T> =>
        from dictionary in AttachedData.UserDictionary(owner)
        from record in IO.lift(static () => new ArchivableDictionary(T.DictionaryVersion, T.DictionaryName))
        from encoded in T.Encode(value, record)
        from stored in IO.lift(() => Refused.Unless(dictionary.Set(T.DictionaryName, record), nameof(ArchivableDictionary.Set)))
        select stored;

    public static IO<bool> Remove<T>(CommonObject owner) where T : IArchivedRecord<T> =>
        AttachedData.UserDictionary(owner).Bind(static dictionary => IO.lift(() => dictionary.Remove(T.DictionaryName)));
}
