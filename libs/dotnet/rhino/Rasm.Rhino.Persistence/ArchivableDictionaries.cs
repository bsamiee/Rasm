using Rasm.Rhino.Document;
using Rhino.Collections;
using Rhino.DocObjects;

namespace Rasm.Rhino.Persistence;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ArchivableDictionaries {
    // --- [READS]
    public static IO<Option<T>> Find<T>(ArchivableDictionary source, string key) where T : notnull =>
        IO.lift(() => source.TryGetValue(key, out object? data)
            ? data is T value ? Fin.Succ(Some(value)) : Fin.Fail<Option<T>>(new TypeMismatch(key, data.GetType(), typeof(T)))
            : Option<T>.None);

    public static IO<Option<T>> FindEnum<T>(ArchivableDictionary source, string key) where T : struct, Enum =>
        IO.lift(() => Answers.Found(source.TryGetEnumValue(key, out T value), value));

    // --- [WRITES]
    public static IO<Unit> SetEnum<T>(ArchivableDictionary target, string key, T value) where T : struct, Enum =>
        from keyed in IO.lift(() => Invalid.Unless(key.Length > 0, nameof(key)))
        from stored in IO.lift(() => Refused.Unless(target.SetEnumValue(key, value), nameof(ArchivableDictionary.SetEnumValue)))
        select stored;

    // --- [SNAPSHOTS]
    public static IO<TValue> WithSnapshot<TValue>(ArchivableDictionary source, Func<ArchivableDictionary, IO<TValue>> body) =>
        from snapshot in IO.lift(source.Clone)
        from value in Disposal.Using(CopyReferences(snapshot), _ => body(snapshot))
        select value;

    private static IO<Seq<IDisposable>> CopyReferences(ArchivableDictionary snapshot) =>
        from keys in IO.lift(() => toSeq(snapshot.Keys))
        from held in Disposal.AcquireAll(keys.Map(key =>
            from data in Data(snapshot, key)
            from copied in data switch {
                GeometryBase geometry => Stored(Duplicated(geometry), copy => snapshot.Set(key, copy), static copy => Seq<IDisposable>(copy)),
                MeshingParameters parameters => Stored(IO.lift(() => new MeshingParameters(parameters)), copy => snapshot.Set(key, copy), static copy => Seq<IDisposable>(copy)),
                ObjRef reference => Stored(IO.lift(() => new ObjRef(reference)), copy => snapshot.Set(key, copy), static copy => Seq<IDisposable>(copy)),
                System.Drawing.Font font => IO.pure(Seq<IDisposable>(font)),
                IEnumerable<GeometryBase> rows =>
                    Stored(Disposal.AcquireAll(toSeq(rows).Map(Duplicated)), copies => snapshot.Set(key, copies), static copies => copies.Map<IDisposable>(static copy => copy)),
                IEnumerable<ObjRef> rows =>
                    Stored(Disposal.AcquireAll(toSeq(rows).Map(static row => IO.lift(() => new ObjRef(row)))), copies => snapshot.Set(key, copies), static copies => copies.Map<IDisposable>(static copy => copy)),
                ArchivableDictionary nested => CopyReferences(nested),
                _ => IO.pure(Seq<IDisposable>()),
            }
            select copied))
        select held;

    private static IO<Seq<IDisposable>> Stored<T>(IO<T> acquire, Func<T, bool> store, Func<T, Seq<IDisposable>> owned) =>
        from held in acquire
        from stored in GeometryOps.OnFailure(IO.lift(() => Refused.Unless(store(held), nameof(ArchivableDictionary.Set))), Disposal.Release(owned(held)))
        select owned(held);

    private static IO<GeometryBase> Duplicated(GeometryBase geometry) =>
        IO.lift(() => Missing.Unless(geometry.Duplicate(), nameof(GeometryBase.Duplicate)));

    private static IO<object> Data(ArchivableDictionary dictionary, string key) =>
        IO.lift(() => Missing.Unless(dictionary.TryGetValue(key, out object? data) ? data : null, nameof(ArchivableDictionary.TryGetValue)));
}
