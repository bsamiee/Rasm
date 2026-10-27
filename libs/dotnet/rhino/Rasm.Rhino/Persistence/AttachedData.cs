using Rasm.RhinoCommon.PlugIns;
using Rasm.RhinoCommon.UI;
using Rhino.Collections;
using Rhino.DocObjects.Custom;
using Rhino.FileIO;
using Rhino.Runtime;

namespace Rasm.RhinoCommon.Persistence;

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class DefinedUserData<TState> : UserData where TState : notnull {
    // --- [STATE]
    private readonly uint typeCode;
    private readonly DictionaryCodec<TState> codec;
    private readonly Option<Func<TState, Transform, TState>> transformed;
    private readonly IPlugInViews owner;

    protected DefinedUserData(uint typeCode, DictionaryCodec<TState> codec, Option<Func<TState, Transform, TState>> transformed) =>
        (this.typeCode, this.codec, this.transformed, owner) = (typeCode, codec, transformed, PlugInOwners.Owner(GetType()));

    public Option<TState> State { get; private set; }

    internal Option<ArchivableDictionary> Newer { get; private set; }

    public sealed override string Description => GetType().Name;

    public sealed override bool ShouldWrite => State.IsSome || Newer.IsSome;

    internal IO<Unit> Assign(TState state) =>
        Newer.Match(Some: raw => IO.fail<Unit>(codec.Refusal(raw)), None: () => IO.lift(() => { State = Some(state); }));

    protected sealed override bool Write(BinaryArchiveWriter archive) =>
        owner.Answer(
            (Newer.Map(raw => AttachedData.Write(archive, typeCode, raw)) | State.Map(held => AttachedData.Write(archive, typeCode, codec, held)))
                .Map(static written => written.Map(static _ => true))
                .IfNone(IO.pure(value: false)),
            failed: false);

    protected sealed override bool Read(BinaryArchiveReader archive) =>
        owner.Answer(
            from read in AttachedData.Read(archive, typeCode, codec)
            from stored in IO.lift(() => {
                (State, Newer) = read.Switch(
                    held: static held => (Some(held.Value), Option<ArchivableDictionary>.None),
                    absent: static _ => (Option<TState>.None, Option<ArchivableDictionary>.None),
                    newer: static newer => (Option<TState>.None, Some(newer.Raw)));
            })
            select true,
            failed: false);

    protected sealed override void OnTransform(Transform transform) {
        base.OnTransform(transform);
        State = (transformed, State).Apply((move, held) => move(held, transform)).As() || State;
    }

    protected sealed override void OnDuplicate(UserData source) {
        DefinedUserData<TState> held = (DefinedUserData<TState>)source;
        (State, Newer) = (held.State, held.Newer);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class AttachedData {
    // --- [FRAMES]
    public static IO<Unit> Write<T>(BinaryArchiveWriter writer, uint typeCode, DictionaryCodec<T> codec, T payload) =>
        IO.lift(() => codec.ToDictionary(payload)).Bind(dictionary => Write(writer, typeCode, dictionary));

    public static IO<Unit> Write(BinaryArchiveWriter writer, uint typeCode, ArchivableDictionary dictionary) =>
        from begun in IO.lift(() => RhinoFault.Unless(writer.BeginWrite3dmChunk(typeCode, majorVersion: 1, minorVersion: 0)))
        from written in IO.lift(() => writer.WriteDictionary(dictionary)).Finally(IO.lift(() => RhinoFault.Unless(writer.EndWrite3dmChunk())))
        select written;

    public static IO<StoredRecord<T>> Read<T>(BinaryArchiveReader reader, uint typeCode, DictionaryCodec<T> codec) =>
        from begun in IO.lift(() => RhinoFault.Unless(reader.BeginRead3dmChunk(typeCode, out _, out _)))
        from dictionary in IO.lift(reader.ReadDictionary).Finally(IO.lift(() => RhinoFault.Unless(reader.EndRead3dmChunk(suppressPartiallyReadChunkWarning: true))))
        from payload in IO.lift(() => codec.FromDictionary(dictionary))
        select payload;

    public static IO<Option<T>> Read<T>(File3dm archive, Guid plugIn, Func<BinaryArchiveReader, IO<T>> read) =>
        from found in IO.lift(() => Conversions.Rows(archive.PlugInData).Find(data => data.PlugInId == plugIn))
        from payload in found.Traverse(data => Callbacks.Captured<T>(answer => IO.lift(() => ignore(archive.PlugInData.TryRead(data, (_, reader) => {
            Fin<T> outcome = Callbacks.Settled(read(reader), CancellationToken.None);
            answer(outcome);
            return outcome.IsSucc;
        }))))).As()
        select payload;

    // --- [USER_DATA]
    public static IO<Option<TData>> Find<TData>(CommonObject target) where TData : UserData =>
        IO.lift(() => Optional(target.UserData.Find(typeof(TData)) as TData));

    public static IO<Unit> Set<TData, TState>(CommonObject target, TState state)
        where TData : DefinedUserData<TState>, new()
        where TState : notnull =>
        from found in Find<TData>(target)
        from stored in found.Match(
            Some: held => held.Assign(state),
            None: Attached<TData, TState>(target, state))
        select stored;

    public static IO<Unit> Remove<TData>(CommonObject target) where TData : UserData =>
        from found in Find<TData>(target)
        from removed in found.Traverse(held =>
            (from data in use(() => held)
             from detached in IO.lift(() => RhinoFault.Unless(target.UserData.Remove(data)))
             select detached).Bracket()).As()
        select unit;

    public static IO<Unit> Move(CommonObject source, CommonObject destination, bool append) =>
        IO.lift(() => UserData.MoveUserDataFrom(source)).Bind(id =>
            IO.lift(() => UserData.MoveUserDataTo(destination, id, append))
                .Catch(error => IO.lift(() => UserData.MoveUserDataTo(source, id, append: true)) >> IO.fail<Unit>(error))).Uninterruptible();

    private static IO<Unit> Attached<TData, TState>(CommonObject target, TState state)
        where TData : DefinedUserData<TState>, new()
        where TState : notnull =>
        IO.lift(static () => new TData()).Bind(data =>
            (data.Assign(state) >> IO.lift(() => RhinoFault.Unless(target.UserData.Add(data))))
                .Catch(error => IO.lift(data.Dispose) >> IO.fail<Unit>(error))).Uninterruptible();
}
