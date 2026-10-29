using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rasm.Rhino.Document.Files;
using Rhino;
using Rhino.Commands;
using Rhino.FileIO;
using SuperLinq;

namespace Rasm.Rhino.Document.Tables;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>(SkipIParsable = true)]
[ValidationError<RhinoFault>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StateName {
    public static Fin<StateName> Validated(string value) => TryCreate(value, out StateName? name, out RhinoFault? fault) ? name : fault;

    static partial void ValidateFactoryArguments(ref RhinoFault? validationError, ref string value) =>
        validationError = value.Length > 0 ? null : new RhinoFault($"value = '{value}'", RhinoReason.Invalid);
}

public sealed record NamedWrite(IO<Unit> Revert, IO<Unit> Release) {
    public static NamedWrite Undoable { get; } = new(IO.pure(unit), IO.pure(unit));

    internal static IO<Unit> Reverted(Seq<NamedWrite> writes) =>
        Products.Each(writes.Rev().Map(static write => write.Revert)).Map(static _ => unit);

    internal static IO<Unit> Released(Seq<NamedWrite> writes) =>
        Products.Each(writes.Map(static write => write.Release)).Map(static _ => unit);

    internal static IO<Unit> Undone(Seq<NamedWrite> writes) =>
        Products.Each(Seq(Reverted(writes), Released(writes))).Map(static _ => unit);
}

public sealed record NamedEntry(NamedTable Table, string Name, Option<Guid> Id);

public abstract record NamedState(NamedTable Table, StateName Name) {
    internal abstract IO<NamedWrite> Save(RhinoDoc doc);
}

public abstract record NamedRestore(NamedTable Table, StateName Name) {
    internal abstract IO<NamedWrite> Restore(RhinoDoc doc);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record NamedChange {
    private NamedChange(NamedTable table, string label) => (Table, Label) = (table, label);

    public NamedTable Table { get; }

    public string Label { get; }

    public sealed record Save(NamedState State) : NamedChange(State.Table, $"{nameof(Save)} {State.Name}");

    public sealed record Restore(NamedRestore Target) : NamedChange(Target.Table, $"{nameof(Restore)} {Target.Name}");

    public sealed record Rename(NamedEntry Entry, StateName Name) : NamedChange(Entry.Table, $"{nameof(Rename)} {Entry.Name}");

    public sealed record Delete(NamedEntry Entry) : NamedChange(Entry.Table, $"{nameof(Delete)} {Entry.Name}");

    public sealed record Xform(NamedEntry Entry, Transform Transform) : NamedChange(Entry.Table, $"{nameof(Transform)} {Entry.Name}");

    public sealed record Import(NamedTable Source, InputPath Path, Option<Seq<StateName>> Names) : NamedChange(Source, $"{nameof(Import)} {Source}");

    // --- [PLANNING]
    public static Fin<Seq<NamedChange>> Plan(Seq<NamedEntry> held, Seq<NamedState> desired, Seq<NamedTable> prune) =>
        (held.ToLookup(static entry => entry.Table), desired.ToLookup(static state => state.Table), toHashSet(prune)) switch {
            var (entries, states, pruned) => toSeq(states.Select(static group => group.Key).Concat(entries.Select(static group => group.Key)).Distinct())
                .Traverse(table => Rows.Plan(
                    toSeq(entries[table]), toSeq(states[table]),
                    static entry => entry.Name, static state => (string)state.Name, table.Comparer, pruned.Contains(table) ? Prune.Delete : Prune.Keep))
                .As()
                .Map(static plans => plans.Flatten().Choose(static step => step.Switch(
                    create: static create => Some<NamedChange>(new Save(create.Spec)),
                    change: static change => Some<NamedChange>(new Save(change.Spec)),
                    remove: static remove => Some<NamedChange>(new Delete(remove.Row)),
                    order: static _ => Option<NamedChange>.None))),
        };

    // --- [APPLY]
    public static IO<Seq<Committed<Seq<NamedEvent>>>> Apply(RhinoDoc doc, Seq<NamedTable> tables, Seq<NamedChange> changes) =>
        from held in NamedTable.Read(doc, tables)
        from valid in IO.lift(() => changes.Fold(Fin.Succ(held), static (entries, change) => entries.Bind(change.Landed)))
        from landed in IO.lift(static () => (Writes: Atom(Seq<NamedWrite>()), Records: Atom(Seq<Option<uint>>())))
        from committed in toSeq(changes.Segment(static (current, previous, _) => !current.Table.Batches || !previous.Table.Batches))
            .TraverseM(segment => Applied(doc, tables, toSeq(segment), landed.Writes, landed.Records)).As()
            .Catch(error => Products.Each(Seq(
                    IO.lift(() => landed.Writes.Value).Bind(NamedWrite.Reverted),
                    IO.lift(() => landed.Records.Value.Rev().Somes()).Bind(records => records.TraverseM(_ => Commits.Step(doc, doc.Undo)).As()).Map(static _ => unit)))
                >> IO.fail<Seq<Committed<Seq<NamedEvent>>>>(error))
            .Finally(IO.lift(() => landed.Writes.Value).Bind(NamedWrite.Released))
        select committed;

    private static IO<Committed<Seq<NamedEvent>>> Applied(RhinoDoc doc, Seq<NamedTable> tables, Seq<NamedChange> segment, Atom<Seq<NamedWrite>> writes, Atom<Seq<Option<uint>>> records) =>
        from before in NamedTable.Read(doc, tables)
        from record in segment.ForAll(static change => change.Table.Batches)
            ? Commits.Commit(
                    doc,
                    string.Join(", ", segment.Map(static change => change.Label)),
                    segment.Exists(static change => change is Restore or Xform) ? new RedrawPolicy.AllViews(Deferred: true) : new RedrawPolicy.Silent(),
                    segment.TraverseM(change => Ran(doc, change, writes)).As())
                .Map(static committed => committed.Record)
            : Commanded(doc, segment, writes)
        from after in NamedTable.Read(doc, tables)
        from noted in IO.lift(() => records.Swap(held => held.Add(record)))
        select new Committed<Seq<NamedEvent>>(NamedEvent.Between(before, after), record);

    private static IO<Option<uint>> Commanded(RhinoDoc doc, Seq<NamedChange> segment, Atom<Seq<NamedWrite>> writes) =>
        from prior in IO.lift(() => toHashSet(doc.GetUndoRecords().Select(static entry => entry.SerialNumber)))
        from ran in segment.TraverseM(change => Ran(doc, change, writes)).As()
        from record in IO.lift(() => toSeq(doc.GetUndoRecords()).Map(static entry => entry.SerialNumber).Filter(serial => !prior.Contains(serial)).Max<Seq, uint>())
        select record;

    private static IO<Unit> Ran(RhinoDoc doc, NamedChange change, Atom<Seq<NamedWrite>> writes) =>
        change.Run(doc).Bind(write => IO.lift(() => writes.Swap(held => held.Add(write)))).Map(static _ => unit);

    private IO<NamedWrite> Run(RhinoDoc doc) =>
        Switch(
            doc,
            save: static (document, save) => save.State.Save(document),
            restore: static (document, restore) => restore.Target.Restore(document),
            rename: static (document, rename) => rename.Entry.Table.Rename(document, rename.Entry, rename.Name),
            delete: static (document, delete) => delete.Entry.Table.Delete(document, delete.Entry),
            xform: static (document, xform) => IO.lift(() => xform.Entry.Table.Supported(xform.Entry.Table.Transform)).Bind(run => run(document, xform.Entry, xform.Transform)),
            import: static (document, import) => IO.lift(() => import.Source.Supported(import.Source.Import)).Bind(run => run(document, import.Path, import.Names)));

    private Fin<Seq<NamedEntry>> Landed(Seq<NamedEntry> entries) =>
        Switch(
            entries,
            save: static (held, save) =>
                from accepted in save.State.Table.Accepted(save.State.Name)
                select save.State.Table.Find(held, save.State.Name).IsSome ? held : held.Add(new NamedEntry(save.State.Table, save.State.Name, None)),
            restore: static (held, restore) => restore.Target.Table.Present(held, restore.Target.Name).Map(_ => held),
            rename: static (held, rename) =>
                from found in rename.Entry.Table.Present(held, rename.Entry.Name)
                from accepted in rename.Entry.Table.Accepted(rename.Name)
                from free in RhinoFault.Unless(rename.Entry.Table.Find(held, rename.Name).ForAll(taken => taken == found), RhinoReason.Taken, (string)rename.Name)
                select held.Map(entry => entry == found ? entry with { Name = rename.Name } : entry),
            delete: static (held, delete) => delete.Entry.Table.Present(held, delete.Entry.Name).Map(found => held.Filter(entry => entry != found)),
            xform: static (held, xform) =>
                from found in xform.Entry.Table.Present(held, xform.Entry.Name)
                from verb in xform.Entry.Table.Supported(xform.Entry.Table.Transform)
                select held,
            import: static (held, import) =>
                from verb in import.Source.Supported(import.Source.Import)
                select import.Names.Match(
                    Some: names => names.Fold(held, (entries, name) => import.Source.Find(entries, name).IsSome ? entries : entries.Add(new NamedEntry(import.Source, name, None))),
                    None: () => held));
}

public sealed record NamedEvent {
    private NamedEvent(Option<NamedEntry> before, Option<NamedEntry> after, NamedTable table) => (Before, After, Table) = (before, after, table);

    public Option<NamedEntry> Before { get; }

    public Option<NamedEntry> After { get; }

    public NamedTable Table { get; }

    public static Seq<NamedEvent> Between(Seq<NamedEntry> before, Seq<NamedEntry> after) =>
        before.Bind(prior => Paired(after, prior).Match(
                Some: next => string.Equals(next.Name, prior.Name, StringComparison.Ordinal) ? Seq<NamedEvent>() : Seq(new NamedEvent(prior, next, next.Table)),
                None: () => Seq(new NamedEvent(prior, None, prior.Table))))
            + after.Filter(next => Paired(before, next).IsNone).Map(static next => new NamedEvent(None, next, next.Table));

    public static HostEvent<Seq<NamedEvent>> Changed(ReadModel<Seq<NamedEntry>> catalog, RhinoDoc doc) =>
        catalog.Changed(doc).Choose(static pair => Some(Between(pair.Before, pair.After)).Filter(static events => !events.IsEmpty));

    private static Option<NamedEntry> Paired(Seq<NamedEntry> entries, NamedEntry entry) =>
        entry.Id.Match(
            Some: id => entries.Find(row => row.Table == entry.Table && row.Id == Some(id)),
            None: () => entry.Table.Find(entries, entry.Name));
}

// --- [TABLES] --------------------------------------------------------------------------
public sealed class NamedTable(string name, IEqualityComparer<string> comparer) {
    public static HostEvent<Option<uint>> DocumentProperties { get; } =
        ReadModels.Marks(Subscriptions.On<DocumentEventArgs>(static h => RhinoDoc.DocumentPropertiesChanged += h, static h => RhinoDoc.DocumentPropertiesChanged -= h).Documents());

    public IEqualityComparer<string> Comparer => comparer;

    public bool Batches { get; init; } = true;

    public Seq<HostEvent<Option<uint>>> Marks { get; init; } = [DocumentProperties];

    public required Func<RhinoDoc, IO<Seq<NamedEntry>>> Entries { get; init; }

    public required Func<RhinoDoc, NamedEntry, StateName, IO<NamedWrite>> Rename { get; init; }

    public required Func<RhinoDoc, NamedEntry, IO<NamedWrite>> Delete { get; init; }

    public required Func<RhinoDoc, Option<Guid>, IO<NamedWrite>> Hold { get; init; }

    public Option<Func<StateName, Fin<Unit>>> Accepts { get; init; }

    public Option<Func<RhinoDoc, NamedEntry, Transform, IO<NamedWrite>>> Transform { get; init; }

    public Option<Func<RhinoDoc, InputPath, Option<Seq<StateName>>, IO<NamedWrite>>> Import { get; init; }

    public Option<Func<RhinoDoc, Seq<NamedEntry>, File3dm, IO<Unit>>> Archive { get; init; }

    public Option<Func<RhinoDoc, NamedEntry, IO<Seq<Guid>>>> Members { get; init; }

    public Option<Func<RhinoDoc, Guid, IO<Option<NamedEntry>>>> Current { get; init; }

    public override string ToString() => name;

    // --- [ROWS]
    public Option<NamedEntry> Find(Seq<NamedEntry> entries, string entry) =>
        entries.Find(held => held.Table == this && Comparer.Equals(held.Name, entry));

    internal Fin<NamedEntry> Present(Seq<NamedEntry> entries, string entry) =>
        Find(entries, entry).ToFin(Missing(entry));

    internal Fin<Unit> Accepted(StateName entry) =>
        Accepts.Map(accepts => accepts(entry)).IfNone(unit);

    internal Fin<T> Supported<T>(Option<T> verb, [CallerArgumentExpression(nameof(verb))] string call = "") =>
        verb.ToFin(new RhinoFault($"{call} on {name}", RhinoReason.Unsupported));

    internal RhinoFault Missing(string entry) =>
        new($"{entry} in {name}", RhinoReason.Missing);

    public static IO<Seq<NamedEntry>> Read(RhinoDoc doc, Seq<NamedTable> tables) =>
        tables.TraverseM(table => table.Entries(doc)).As().Map(static entries => entries.Flatten());

    public static ReadModel<Seq<NamedEntry>> Catalog(Seq<NamedTable> tables) =>
        new(
            toSeq(tables.Bind(static table => table.Marks).Distinct())
            + Seq(
                ReadModels.Marks(Subscriptions.On<UndoRedoEventArgs>(static h => Command.UndoRedo += h, static h => Command.UndoRedo -= h).Documents(static args => args.DocumentSerialNumber)),
                ReadModels.Marks(Subscriptions.Watched<DocumentOpenEventArgs>(static (watcher, h) => watcher.EndOpenDocument += h).Documents())),
            doc => Read(doc, tables));

    // --- [HOLDS]
    public static IO<T> Within<T>(RhinoDoc doc, Seq<NamedTable> tables, Option<Guid> viewport, IO<T> body) =>
        tables.Fold(IO.pure(Seq<NamedWrite>()), (held, table) =>
                from writes in held
                from write in table.Hold(doc, viewport).Catch(error => NamedWrite.Undone(writes) >> IO.fail<NamedWrite>(error))
                select writes.Add(write))
            .Bind(writes => body.Finally(NamedWrite.Undone(writes)));

    // --- [EXCHANGE]
    public static IO<StepOutcome> Export(RhinoDoc doc, Seq<NamedEntry> entries, OutputPath path) =>
        from rows in IO.lift(() => toSeq(entries.GroupBy(static entry => entry.Table))
            .Traverse(static held => held.Key.Supported(held.Key.Archive).Map(archive => (Archive: archive, Entries: toSeq(held))))
            .As())
        from outcome in (from archive in use(static () => new File3dm())
                         from units in IO.lift(() => { archive.Settings.ModelUnits = doc.ModelUnits; })
                         from archived in rows.TraverseM(row => row.Archive(doc, row.Entries, archive)).As()
                         from written in StepOutcome.Write(path, target => IO.lift(() =>
                             archive.WriteWithLog(target, new File3dmWriteOptions(), out string log) ? Fin.Succ(unit) : new RhinoFault(target, RhinoReason.Unwritable, Inner: Error.New(log))))
                         select written).Bracket()
        select outcome;

    public IO<NamedWrite> Imported<T>(
        RhinoDoc doc, InputPath path, Option<Seq<StateName>> names, Func<File3dm, IEnumerable<T>> rows, Func<T, string> entry, Func<T, Transform, IO<NamedWrite>> write) =>
        (from model in Products.Of(() => File3dm.Read(path, File3dm.TableTypeFilter.Settings, File3dm.ObjectTypeFilter.None))
         from stored in IO.lift(() => Conversions.Rows(rows(model)))
         from chosen in IO.lift(() => names.Match(
             Some: wanted => wanted.Traverse(held => stored.Find(row => Comparer.Equals(entry(row), held)).ToFin(Missing(held))).As(),
             None: () => stored))
         from scaling in IO.lift(() => Conversions.Required(Conversions.Present(LengthUnit.Scale(model.Settings.ModelUnits, doc.ModelUnits)).Filter(static scale => scale > 0d))
             .Map(static scale => global::Rhino.Geometry.Transform.Scale(Point3d.Origin, scale)))
         from written in chosen.TraverseM(row => write(row, scaling)).As()
         select new NamedWrite(NamedWrite.Reverted(written), NamedWrite.Released(written))).Bracket();

    public static Fin<double> Dilation(Transform xform) =>
        RhinoFault.Unless(xform.DecomposeSimilarity(out _, out double dilation, out _, RhinoMath.SqrtEpsilon) is not TransformSimilarityType.NotSimilarity, RhinoReason.Invalid, xform)
            .Map(_ => Math.Abs(dilation));
}
