# [RASM_RHINO_PERSISTENCE_USERTEXT]

User-text custody (`Rasm.Rhino.Persistence`). `DocumentTextTarget` addresses `RhinoDoc.Strings` flat or sectioned entries. `ObjectTextTarget` addresses either the `ObjectAttributes` or `GeometryBase` store behind any `TableTarget`. `TextEdit` generates set, delete, and clear behavior for both object stores. `TextProgram` is the sole mutation rail: document edits own their `StringTable` undo window, attribute edits lower to `TableOp.Amend`, and geometry edits lower to detached duplicates plus `TableOp.Replace`. `TextRead` returns complete detached snapshots or host wildcard-search results. `DocumentStream` remains the sole `UserStringChanged` observation owner.

## [01]-[INDEX]

- [02]-[ADDRESS_EDIT]: document and object addresses plus generated edit behavior.
- [03]-[MUTATION_RAIL]: document, attribute, and geometry commit lowering.
- [04]-[READ_RAIL]: complete snapshots and both host search-filter overloads.
- [05]-[SURFACE_LEDGER]: ownership and entry points.

## [02]-[ADDRESS_EDIT]

- Address: `DocumentTextTarget` is the flat-versus-section choice. `ObjectTextTarget` combines one `TableTarget` with an attribute-or-geometry side.
- Host truth: public per-object user strings exist on `ObjectAttributes` and `GeometryBase`; `CommonObject` exposes only internal primitives and no public `SetUserString` family.
- Edit: `TextEdit` closes set, delete, and clear. Admission rejects null values but preserves empty strings, while delete remains the only null-writing host operation.
- Generation: `ApplyObject` receives the four host operators once and derives identical attribute and geometry behavior without parallel edit DTOs.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Collections.Specialized;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.Geometry;

namespace Rasm.Rhino.Persistence;

// --- [ADDRESSES] ----------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DocumentTextTarget {
    private DocumentTextTarget() { }
    public sealed record Flat : DocumentTextTarget;
    public sealed record Section(string Name) : DocumentTextTarget;

    internal Fin<DocumentTextTarget> Admit(Op op) =>
        Switch(
            flat: static target => Fin.Succ<DocumentTextTarget>(value: target),
            section: target => op.AcceptText(value: target.Name).Map(name => (DocumentTextTarget)new Section(Name: name)));
}

[SmartEnum<int>]
public sealed partial class ObjectTextSide {
    public static readonly ObjectTextSide Attributes = new(key: 0);
    public static readonly ObjectTextSide Geometry = new(key: 1);
}

public sealed record ObjectTextTarget(TableTarget Objects, ObjectTextSide Side) {
    internal Fin<ObjectTextTarget> Admit(Op op) =>
        from objects in Optional(Objects).ToFin(Fail: op.InvalidInput())
        from side in Optional(Side).ToFin(Fail: op.InvalidInput())
        select new ObjectTextTarget(Objects: objects, Side: side);
}

// --- [EDIT] ---------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextEdit {
    private TextEdit() { }
    private sealed record SetCase(string Name, string Value) : TextEdit;
    private sealed record DeleteCase(string Name) : TextEdit;
    private sealed record ClearCase : TextEdit;

    public static Fin<TextEdit> Set(string name, string value) {
        Op op = Op.Of();
        return from key in op.AcceptText(value: name)
               from text in Optional(value).ToFin(Fail: op.InvalidInput())
               select (TextEdit)new SetCase(Name: key, Value: text);
    }

    public static Fin<TextEdit> Delete(string name) =>
        Op.Of().AcceptText(value: name).Map(key => (TextEdit)new DeleteCase(Name: key));

    public static TextEdit Clear { get; } = new ClearCase();

    internal Fin<Unit> Apply(StringTable table, DocumentTextTarget target, Op op) =>
        target.Switch(
            flat: _ => Switch(
                (Table: table, Op: op),
                setCase: static (context, edit) => context.Op.Catch(() => Fin.Succ(value: Op.Side(() => context.Table.SetString(edit.Name, edit.Value)))),
                deleteCase: static (context, edit) => context.Op.Catch(() => Fin.Succ(value: Op.Side(() => context.Table.Delete(edit.Name)))),
                clearCase: static (context, _) => context.Op.Catch(() => {
                    Seq<string> keys = toSeq(Enumerable.Range(0, context.Table.Count))
                        .Map(index => context.Table.GetKey(i: index))
                        .Filter(static key => !key.Contains(value: '\\', comparisonType: StringComparison.Ordinal));
                    return Fin.Succ(value: keys.Iter(key => context.Table.Delete(key: key)));
                })),
            section: section => Switch(
                (Table: table, Op: op, Section: section.Name),
                setCase: static (context, edit) => context.Op.Catch(() => Fin.Succ(value: Op.Side(() => context.Table.SetString(context.Section, edit.Name, edit.Value)))),
                deleteCase: static (context, edit) => context.Op.Catch(() => Fin.Succ(value: Op.Side(() => context.Table.Delete(context.Section, edit.Name)))),
                clearCase: static (context, _) => context.Op.Catch(() => Fin.Succ(value: Op.Side(() => context.Table.Delete(context.Section, entry: null))))));

    internal Fin<Unit> Apply(ObjectAttributes target, Op op) => ApplyObject(
        target,
        set: static (store, name, value) => store.SetUserString(name, value),
        delete: static (store, name) => store.DeleteUserString(name),
        clear: static store => store.DeleteAllUserStrings(),
        op: op);

    internal Fin<Unit> Apply(GeometryBase target, Op op) => ApplyObject(
        target,
        set: static (store, name, value) => store.SetUserString(name, value),
        delete: static (store, name) => store.DeleteUserString(name),
        clear: static store => store.DeleteAllUserStrings(),
        op: op);

    private Fin<Unit> ApplyObject<T>(
        T target,
        Func<T, string, string, bool> set,
        Func<T, string, bool> delete,
        Action<T> clear,
        Op op) =>
        Switch(
            (Target: target, Set: set, Delete: delete, Clear: clear, Op: op),
            setCase: static (context, edit) => context.Op.Catch(() => context.Op.Confirm(
                success: context.Set(context.Target, edit.Name, edit.Value))),
            deleteCase: static (context, edit) => context.Op.Catch(() => Fin.Succ(
                value: Op.Side(() => _ = context.Delete(context.Target, edit.Name)))),
            clearCase: static (context, _) => context.Op.Catch(() => Fin.Succ(
                value: Op.Side(() => context.Clear(context.Target)))));
}
```

## [03]-[MUTATION_RAIL]

- Owner: `TextProgram` distinguishes document and object programs because their admission and commit owners differ. Factories admit a non-empty edit sequence before host access.
- Document: one `DocumentSession.Demand` mutation window and shared `UndoBracket` apply every edit to `StringTable`.
- Attributes: one `TableOp.Amend` applies the edit fold to each duplicated `ObjectAttributes`; `TableTransaction.Recorded` and `Tables.Commit` own modify, undo, and session consequences.
- Geometry: one read window duplicates each selected geometry and applies its edit fold. Each detached duplicate becomes a single-target `TableOp.Replace`, then `Tables.Commit` owns replacement. `GeometryTextPlan` disposes every duplicate after commit or any plan-construction failure.
- Receipt: document commits report applied edit count and target; object commits retain the canonical `TableReceipt` instead of inventing parallel object facts.

```csharp signature
// --- [PROGRAM] ------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextProgram {
    private TextProgram() { }
    private sealed record DocumentCase(string Name, DocumentTextTarget Target, Seq<TextEdit> Edits) : TextProgram;
    private sealed record ObjectsCase(string Name, ObjectTextTarget Target, Seq<TextEdit> Edits) : TextProgram;

    public static Fin<TextProgram> Document(string name, DocumentTextTarget target, params ReadOnlySpan<TextEdit> edits) {
        Op op = Op.Of();
        return from label in op.AcceptText(value: name)
               from address in Optional(target).ToFin(Fail: op.InvalidInput()).Bind(active => active.Admit(op: op))
               from program in Admit(edits: edits, op: op)
               select (TextProgram)new DocumentCase(Name: label, Target: address, Edits: program);
    }

    public static Fin<TextProgram> Objects(string name, ObjectTextTarget target, params ReadOnlySpan<TextEdit> edits) {
        Op op = Op.Of();
        return from label in op.AcceptText(value: name)
               from address in Optional(target).ToFin(Fail: op.InvalidInput()).Bind(active => active.Admit(op: op))
               from program in Admit(edits: edits, op: op)
               select (TextProgram)new ObjectsCase(Name: label, Target: address, Edits: program);
    }

    internal Fin<TextReceipt> Commit(DocumentSession session, Op op) =>
        Switch(
            (Session: session, Op: op),
            documentCase: static (context, program) => context.Session.Demand(
                use: document => context.Op.Catch(() => {
                    using UndoBracket undo = UndoBracket.Begin(document: document, name: program.Name, recordsUndo: true);
                    Fin<TextReceipt> outcome = guard(undo.Admitted, context.Op.InvalidResult()).ToFin()
                        .Bind(_ => program.Edits.TraverseM(edit => edit.Apply(document.Strings, program.Target, context.Op)).As())
                        .Map(_ => (TextReceipt)new TextReceipt.Document(
                            Applied: program.Edits.Count,
                            Target: program.Target is DocumentTextTarget.Section section ? Some(section.Name) : None));
                    return undo.Seal(outcome: outcome, stamp: static (receipt, _) => receipt, key: context.Op);
                }),
                key: context.Op,
                needs: [SessionNeed.Mutate, SessionNeed.Undo]),
            objectsCase: static (context, program) => program.Target.Side == ObjectTextSide.Attributes
                ? CommitAttributes(context.Session, program, context.Op)
                : CommitGeometry(context.Session, program, context.Op));

    private static Fin<Seq<TextEdit>> Admit(ReadOnlySpan<TextEdit> edits, Op op) =>
        from program in toSeq(edits.ToArray()).TraverseM(edit => Optional(edit).ToFin(Fail: op.InvalidInput())).As()
        from _ in guard(!program.IsEmpty, op.InvalidInput()).ToFin()
        select program;

    private static Fin<TextReceipt> CommitAttributes(DocumentSession session, ObjectsCase program, Op op) =>
        from edit in TableOp.Amend(
            target: program.Target.Objects,
            change: attributes => program.Edits.TraverseM(item => item.Apply(target: attributes, op: op)).As().Map(static _ => unit),
            notice: Notice.Quiet)
        from transaction in TableTransaction.Recorded(
            name: program.Name,
            redraw: RedrawPolicy.None,
            customUndo: Seq<TableCustomUndo>(),
            operations: [edit])
        from receipt in Tables.Commit(session: session, transaction: transaction, key: op)
        select new TextReceipt.Objects(Receipt: receipt);

    private static Fin<TextReceipt> CommitGeometry(DocumentSession session, ObjectsCase program, Op op) =>
        from plan in PrepareGeometry(session: session, program: program, op: op)
        from receipt in op.Catch(() => {
            using (plan) {
                return from transaction in TableTransaction.Recorded(
                           name: program.Name,
                           redraw: RedrawPolicy.None,
                           customUndo: Seq<TableCustomUndo>(),
                           operations: plan.Operations.ToArray())
                       from committed in Tables.Commit(session: session, transaction: transaction, key: op)
                       select (TextReceipt)new TextReceipt.Objects(Receipt: committed);
            }
        })
        select receipt;

    private static Fin<GeometryTextPlan> PrepareGeometry(DocumentSession session, ObjectsCase program, Op op) =>
        session.Demand(
            use: document =>
                from ids in program.Target.Objects.Resolve(document: document, key: op)
                from _ in guard(!ids.IsEmpty, op.MissingContext()).ToFin()
                from plan in ids.Fold(
                    Fin.Succ(value: GeometryTextPlan.Empty),
                    (state, id) => state.Bind(held => BuildGeometry(document, id, program.Edits, op).Match(
                        Succ: item => Fin.Succ(value: held.Add(item.Operation, item.Lease)),
                        Fail: error => {
                            held.Dispose();
                            return Fin.Fail<GeometryTextPlan>(error: error);
                        })))
                select plan,
            key: op,
            needs: [SessionNeed.Read]);

    private static Fin<(TableOp Operation, GeometryBase Lease)> BuildGeometry(
        RhinoDoc document,
        Guid id,
        Seq<TextEdit> edits,
        Op op) =>
        from native in Optional(document.Objects.FindId(id)).ToFin(Fail: op.MissingContext())
        from source in Optional(native.Geometry).ToFin(Fail: op.MissingContext())
        from lease in op.Catch(() => Optional(source.Duplicate()).ToFin(Fail: op.InvalidResult()))
        from result in edits.TraverseM(edit => edit.Apply(target: lease, op: op)).As()
            .Bind(_ => TableTarget.Of(id))
            .Bind(target => TableOp.Replace(target: target, replacement: lease, modes: ObjectMode.Respect))
            .Match(
                Succ: operation => Fin.Succ(value: (operation, lease)),
                Fail: error => {
                    lease.Dispose();
                    return Fin.Fail<(TableOp, GeometryBase)>(error: error);
                })
        select result;
}

internal sealed record GeometryTextPlan(Seq<TableOp> Operations, Seq<GeometryBase> Leases) : IDetachedDocumentResult, IDisposable {
    internal static GeometryTextPlan Empty { get; } = new(Operations: Seq<TableOp>(), Leases: Seq<GeometryBase>());
    internal GeometryTextPlan Add(TableOp operation, GeometryBase lease) => new(Operations: Operations.Add(operation), Leases: Leases.Add(lease));
    public void Dispose() => Leases.Iter(static lease => lease.Dispose());
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextReceipt : IDetachedDocumentResult {
    private TextReceipt() { }
    public sealed record Document(int Applied, Option<string> Target) : TextReceipt;
    public sealed record Objects(TableReceipt Receipt) : TextReceipt;
}

public static partial class Texts {
    public static Fin<TextReceipt> Commit(DocumentSession session, TextProgram program) {
        Op op = Op.Of();
        return from active in Optional(program).ToFin(Fail: op.InvalidInput())
               from context in Optional(session).ToFin(Fail: op.InvalidInput())
               from receipt in active.Commit(session: context, op: op)
               select receipt;
    }
}
```

## [04]-[READ_RAIL]

- Document snapshot: `DocumentTextSnapshot` retains every flat key, every section entry, empty stored strings, and the host count triple.
- Object snapshot: `ObjectTextSnapshot` retains independent attribute and geometry maps for every resolved object; no single-target narrowing or merged-store read occurs.
- Search: `TextSearchFilter` composes both `FindByUserString` filter overloads—`ObjectType` and the sibling `ObjectQuery` projection—while preserving independent geometry and attribute flags.
- Observation: `DocumentStream` owns `EventFamily.UserStringChanged` and its detached key payload. No subscription or event delegate appears here.

```csharp signature
// --- [READ_MODELS] --------------------------------------------------------------------------
public readonly record struct TextTally(int Total, int Sectioned, int Flat) : IDetachedDocumentResult {
    public bool Consistent => Total == Sectioned + Flat;
}

public sealed record DocumentTextSnapshot(
    HashMap<string, string> Flat,
    HashMap<string, HashMap<string, string>> Sections,
    TextTally Tally) : IDetachedDocumentResult;

public sealed record ObjectTextSnapshot(
    ObjectRuntime Runtime,
    HashMap<string, string> Attributes,
    Option<HashMap<string, string>> Geometry) : IDetachedDocumentResult;

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextSearchFilter {
    private TextSearchFilter() { }
    private sealed record TypesCase(ObjectType Types) : TextSearchFilter;
    private sealed record QueryCase(ObjectQuery Query) : TextSearchFilter;

    public static TextSearchFilter Types(ObjectType types) => new TypesCase(Types: types);

    public static Fin<TextSearchFilter> Query(ObjectQuery query) {
        Op op = Op.Of();
        return Optional(query).ToFin(Fail: op.InvalidInput()).Map(value => (TextSearchFilter)new QueryCase(Query: value));
    }

    internal Fin<Seq<RhinoObject>> Find(RhinoDoc document, TextSearch search, Op op) =>
        Switch(
            (Document: document, Search: search, Op: op),
            typesCase: static (context, filter) => context.Op.Catch(() => Fin.Succ(value:
                Optional(context.Document.Objects.FindByUserString(
                    context.Search.KeyPattern,
                    context.Search.ValuePattern,
                    context.Search.CaseSensitive,
                    context.Search.SearchGeometry,
                    context.Search.SearchAttributes,
                    filter.Types))
                .Map(static rows => toSeq(rows))
                .IfNone(Seq<RhinoObject>()))),
            queryCase: static (context, filter) => filter.Query.Build(document: context.Document, key: context.Op)
                .Bind(settings => context.Op.Catch(() => Fin.Succ(value:
                    Optional(context.Document.Objects.FindByUserString(
                        context.Search.KeyPattern,
                        context.Search.ValuePattern,
                        context.Search.CaseSensitive,
                        context.Search.SearchGeometry,
                        context.Search.SearchAttributes,
                        settings))
                    .Map(static rows => toSeq(rows))
                    .IfNone(Seq<RhinoObject>())))));
}

public sealed record TextSearch {
    private TextSearch(
        string keyPattern,
        string valuePattern,
        bool caseSensitive,
        bool searchGeometry,
        bool searchAttributes,
        TextSearchFilter filter) =>
        (KeyPattern, ValuePattern, CaseSensitive, SearchGeometry, SearchAttributes, Filter) =
        (keyPattern, valuePattern, caseSensitive, searchGeometry, searchAttributes, filter);

    public string KeyPattern { get; }
    public string ValuePattern { get; }
    public bool CaseSensitive { get; }
    public bool SearchGeometry { get; }
    public bool SearchAttributes { get; }
    public TextSearchFilter Filter { get; }

    public static Fin<TextSearch> Create(
        string keyPattern,
        string valuePattern,
        bool caseSensitive,
        bool searchGeometry,
        bool searchAttributes,
        TextSearchFilter filter) {
        Op op = Op.Of();
        return from key in op.AcceptText(value: keyPattern)
               from value in Optional(valuePattern).ToFin(Fail: op.InvalidInput())
               from scope in Optional(filter).ToFin(Fail: op.InvalidInput())
               from _ in guard(searchGeometry || searchAttributes, op.InvalidInput()).ToFin()
               select new TextSearch(
                   keyPattern: key,
                   valuePattern: value,
                   caseSensitive: caseSensitive,
                   searchGeometry: searchGeometry,
                   searchAttributes: searchAttributes,
                   filter: scope);
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextRead {
    private TextRead() { }
    public sealed record Document : TextRead;
    public sealed record Objects(TableTarget Target) : TextRead;
    public sealed record Search(TextSearch Query) : TextRead;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextAnswer : IDetachedDocumentResult {
    private TextAnswer() { }
    public sealed record Document(DocumentTextSnapshot Snapshot) : TextAnswer;
    public sealed record Objects(Seq<ObjectTextSnapshot> Snapshots) : TextAnswer;
    public sealed record Found(Seq<ObjectRuntime> Runtime) : TextAnswer;
}

public static partial class Texts {
    public static Fin<TextAnswer> Read(DocumentSession session, TextRead request) {
        Op op = Op.Of();
        return from context in Optional(session).ToFin(Fail: op.InvalidInput())
               from active in Optional(request).ToFin(Fail: op.InvalidInput())
               from answer in context.Demand(
                   use: document => active.Switch(
                       (Document: document, Op: op),
                       document: static (state, _) => SnapshotDocument(document: state.Document, op: state.Op)
                           .Map(snapshot => (TextAnswer)new TextAnswer.Document(Snapshot: snapshot)),
                       objects: static (state, query) => Optional(query.Target).ToFin(Fail: state.Op.InvalidInput())
                           .Bind(target => SnapshotObjects(document: state.Document, target: target, op: state.Op))
                           .Map(snapshots => (TextAnswer)new TextAnswer.Objects(Snapshots: snapshots)),
                       search: static (state, query) => Optional(query.Query).ToFin(Fail: state.Op.InvalidInput())
                           .Bind(search => search.Filter.Find(document: state.Document, search: search, op: state.Op))
                           .Bind(hits => hits.TraverseM(hit => ObjectRuntime.Of(hit.Id, hit.RuntimeSerialNumber, key: state.Op)).As())
                           .Map(runtime => (TextAnswer)new TextAnswer.Found(Runtime: runtime))),
                   key: op,
                   needs: [SessionNeed.Read])
               select answer;
    }

    private static Fin<DocumentTextSnapshot> SnapshotDocument(RhinoDoc document, Op op) => op.Catch(() => {
        StringTable table = document.Strings;
        HashMap<string, string> flat = toSeq(Enumerable.Range(0, table.Count))
            .Map(index => (Key: table.GetKey(i: index), Value: table.GetValue(i: index)))
            .Filter(static pair => !pair.Key.Contains(value: '\\', comparisonType: StringComparison.Ordinal))
            .Fold(HashMap<string, string>(), static (state, pair) => state.AddOrUpdate(pair.Key, pair.Value));
        HashMap<string, HashMap<string, string>> sections = toSeq(table.GetSectionNames()).Fold(
            HashMap<string, HashMap<string, string>>(),
            (state, section) => state.AddOrUpdate(
                section,
                toSeq(table.GetEntryNames(section: section)).Fold(
                    HashMap<string, string>(),
                    (entries, entry) => entries.AddOrUpdate(entry, table.GetValue(section: section, entry: entry)))));
        return Fin.Succ(value: new DocumentTextSnapshot(
            Flat: flat,
            Sections: sections,
            Tally: new TextTally(
                Total: table.Count,
                Sectioned: table.DocumentDataCount,
                Flat: table.DocumentUserTextCount)));
    });

    private static Fin<Seq<ObjectTextSnapshot>> SnapshotObjects(RhinoDoc document, TableTarget target, Op op) =>
        from ids in target.Resolve(document: document, key: op)
        from snapshots in ids.TraverseM(id =>
                from native in Optional(document.Objects.FindId(id)).ToFin(Fail: op.MissingContext())
                from runtime in ObjectRuntime.Of(native.Id, native.RuntimeSerialNumber, key: op)
                select new ObjectTextSnapshot(
                    Runtime: runtime,
                    Attributes: Freeze(source: native.Attributes.GetUserStrings()),
                    Geometry: Optional(native.Geometry).Map(static geometry => Freeze(source: geometry.GetUserStrings()))))
            .As()
        select snapshots;

    private static HashMap<string, string> Freeze(NameValueCollection source) =>
        toSeq(source.AllKeys)
            .Choose(key => Optional(key).Map(name => (Name: name, Value: source[name] ?? string.Empty)))
            .Fold(HashMap<string, string>(), static (state, pair) => state.AddOrUpdate(pair.Name, pair.Value));
}
```

## [05]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]        | [OWNER]                | [FORM]                                             | [ENTRY]                    |
| :-----: | :--------------- | :--------------------- | :------------------------------------------------- | :------------------------- |
|  [01]   | document address | `DocumentTextTarget`   | flat or sectioned store                            | `TextProgram.Document`     |
|  [02]   | object address   | `ObjectTextTarget`     | table target plus attribute-or-geometry side       | `TextProgram.Objects`      |
|  [03]   | edit vocabulary  | `TextEdit`             | generated set, delete, and clear behavior          | `Set` / `Delete` / `Clear` |
|  [04]   | mutation rail    | `TextProgram`          | document undo or canonical table transaction       | `Texts.Commit`             |
|  [05]   | document read    | `DocumentTextSnapshot` | full flat, sectioned, and count state              | `Texts.Read`               |
|  [06]   | object read      | `ObjectTextSnapshot`   | independent attribute and geometry maps per object | `Texts.Read`               |
|  [07]   | wildcard search  | `TextSearch`           | host flags plus object-type or object-query filter | `Texts.Read`               |
|  [08]   | observation      | `DocumentStream`       | existing user-string event family                  | seam owner only            |
