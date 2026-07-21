# [USER_TEXT]

`TextOperation` closes document text, attribute text, geometry text, detached reads, and wildcard search as one concern. `Texts.Commit` resolves one document session, derives undo and table needs from the active case, and returns a case-matched detached answer.

## [01]-[VOCABULARY]

`TextAddress` makes document and object stores disjoint. `TextSearchPolicy` crosses a non-empty store set with one comparison row and projects Rhino booleans only at the host call, so search growth adds an axis value instead of another named product case.

```csharp signature
namespace Rasm.Rhino.Persistence;

using LanguageExt;
using Rhino.DocObjects;
using Thinktecture;
using static LanguageExt.Prelude;

[ValueObject<string>]
public readonly partial struct TextKey
{
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            validationError = new ValidationError("Text key is empty.");
            return;
        }

        value = value.Trim();
        validationError = null;
    }
}

[ValueObject<string>]
public readonly partial struct TextValue
{
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = value is null
            ? new ValidationError("Text value is null.")
            : null;
}

[ComplexValueObject]
public sealed partial record TextSection(string Section, string Entry)
{
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string section,
        ref string entry)
    {
        if (string.IsNullOrWhiteSpace(section) || string.IsNullOrWhiteSpace(entry))
        {
            validationError = new ValidationError("Text section and entry are required.");
            return;
        }

        section = section.Trim();
        entry = entry.Trim();
        validationError = null;
    }
}

[SmartEnum<string>]
public sealed partial class ObjectTextStore
{
    public static readonly ObjectTextStore Attributes = new(
        "attributes",
        searchRoute: (Geometry: false, Attributes: true));
    public static readonly ObjectTextStore Geometry = new(
        "geometry",
        searchRoute: (Geometry: true, Attributes: false));

    internal (bool Geometry, bool Attributes) SearchRoute { get; }
}

[Union]
public abstract partial record TextAddress
{
    public sealed record DocumentKeyCase(TextKey Key) : TextAddress;
    public sealed record DocumentSectionCase(TextSection Address) : TextAddress;
    public sealed record ObjectCase(Guid ObjectId, ObjectTextStore Store, TextKey Key) : TextAddress;
}

[Union]
public abstract partial record TextEdit
{
    public sealed record SetCase(TextValue Value) : TextEdit;
    public sealed record DeleteCase : TextEdit;

    internal Option<TextValue> Result => Switch<Option<TextValue>>(
        setCase: static write => Some(write.Value),
        deleteCase: static _ => None);
}

public sealed record TextMutation(TextAddress Address, TextEdit Edit);

[SmartEnum<string>]
public sealed partial class TextComparison
{
    public static readonly TextComparison Exact = new("exact", true);
    public static readonly TextComparison Folded = new("folded", false);
    internal bool CaseSensitive { get; }
}

[ComplexValueObject]
public sealed partial record TextSearchPolicy(
    LanguageExt.HashSet<ObjectTextStore> Stores,
    TextComparison Comparison)
{
    internal (bool Geometry, bool Attributes) SearchRoute => Stores.Fold(
        (Geometry: false, Attributes: false),
        static (route, store) => (
            route.Geometry || store.SearchRoute.Geometry,
            route.Attributes || store.SearchRoute.Attributes));
    internal bool CaseSensitive => Comparison.CaseSensitive;

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref LanguageExt.HashSet<ObjectTextStore> stores,
        ref TextComparison comparison) =>
        validationError = stores.IsEmpty || !stores.ForAll(static store => store is not null) || comparison is null
            ? new ValidationError("Text search policy requires a store and comparison.")
            : null;
}

[Union]
public abstract partial record TextObjectFilter
{
    public sealed record KindsCase(ObjectType Kinds) : TextObjectFilter;
    public sealed record EnumeratorCase(ObjectEnumeratorSettings Settings) : TextObjectFilter;
}

public sealed record TextSearch(
    TextKey Key,
    TextValue Pattern,
    TextSearchPolicy Policy,
    TextObjectFilter Filter);

[Union]
public abstract partial record TextMutationBatch
{
    public sealed record DocumentCase(Seq<TextMutation> Mutations) : TextMutationBatch;
    public sealed record ObjectsCase(Seq<TextMutation> Mutations) : TextMutationBatch;
}

[Union]
public abstract partial record TextOperation
{
    public sealed record MutateCase(TextMutationBatch Batch) : TextOperation;
    public sealed record ReadDocumentCase : TextOperation;
    public sealed record ReadObjectsCase(Seq<Guid> ObjectIds) : TextOperation;
    public sealed record SearchCase(TextSearch Search) : TextOperation;
}
```

## [02]-[DETACHED_RESULTS]

Every mutation emits its prior and current value. Receipt counts derive from address identity, document counts preserve Rhino's flat-versus-section partition, and object snapshots derive geometry count from the detached store. `TextMatch` carries a non-empty store set, so adding a store row never adds product cases.

```csharp signature
namespace Rasm.Rhino.Persistence;

using LanguageExt;
using Thinktecture;

public sealed record TextDelta(
    TextAddress Address,
    Option<TextValue> Prior,
    Option<TextValue> Current);

public sealed record TextMutationReceipt(Seq<TextDelta> Changes)
{
    public int DocumentChanges => Changes.Filter(static change =>
        change.Address is not TextAddress.ObjectCase && change.Prior != change.Current).Count;
    public int ObjectChanges => Changes.Filter(static change =>
        change.Address is TextAddress.ObjectCase && change.Prior != change.Current).Count;
}

public sealed record DocumentTextSnapshot(
    HashMap<TextKey, TextValue> Flat,
    HashMap<TextSection, TextValue> Sections,
    int FlatCount,
    int SectionCount)
{
    public int Count => FlatCount + SectionCount;
    public bool Consistent => Flat.Count == FlatCount && Sections.Count == SectionCount;
}

public sealed record ObjectTextSnapshot(
    Guid ObjectId,
    HashMap<TextKey, TextValue> Attributes,
    HashMap<TextKey, TextValue> Geometry,
    int AttributeCount)
{
    public int GeometryCount => Geometry.Count;
    public bool Consistent => Attributes.Count == AttributeCount;
}

[ComplexValueObject]
public sealed partial record TextMatch(
    Guid ObjectId,
    LanguageExt.HashSet<ObjectTextStore> Stores)
{
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Guid objectId,
        ref LanguageExt.HashSet<ObjectTextStore> stores) =>
        validationError = objectId == Guid.Empty || stores.IsEmpty || !stores.ForAll(static store => store is not null)
            ? new ValidationError("Text match requires an object and at least one store.")
            : null;
}

[Union]
public abstract partial record TextAnswer
{
    public sealed record MutationCase(TextMutationReceipt Receipt) : TextAnswer;
    public sealed record DocumentCase(DocumentTextSnapshot Snapshot) : TextAnswer;
    public sealed record ObjectsCase(Seq<ObjectTextSnapshot> Snapshots) : TextAnswer;
    public sealed record MatchesCase(Seq<TextMatch> Matches) : TextAnswer;
}
```

## [03]-[INTERPRETER]

Document mutations run inside the document undo bracket. Object mutations group each store by object: attribute groups fold through one detached map and one `TableOp.Amend`, while geometry groups fold through one staged clone and one `TableOp.Replace`; no live `RhinoObject` or mutable string collection escapes.

Rhino's mutable text, clone, undo, and object-table calls form the platform-forced statement seam. Generated `Switch` dispatch retains operation, address, store, edit, and filter identity until each host call; search matches derive from set membership, so an unmatched object is structurally unspellable.

```csharp signature
namespace Rasm.Rhino.Persistence;

using System.Collections.Specialized;
using LanguageExt;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using static LanguageExt.Prelude;

public static class Texts
{
    public static Fin<TextAnswer> Commit(
        DocumentSession session,
        TextOperation operation,
        Op? key = null)
    {
        Op op = key.OrDefault();
        return from owner in Optional(session).ToFin(Fail: op.MissingContext())
               from request in Optional(operation).ToFin(Fail: op.InvalidInput())
               from active in Admit(request, op)
               from answer in active.Switch<(DocumentSession Session, Op Op), Fin<TextAnswer>>(
                state: (owner, op),
                mutateCase: static (state, mutate) => Mutate(state.Session, mutate.Batch, state.Op),
                readDocumentCase: static (state, _) => state.Session.Demand(
                    use: document => ReadDocument(document, state.Op).Map<TextAnswer>(static value => new TextAnswer.DocumentCase(value)),
                    key: state.Op,
                    needs: SessionNeed.Read),
                readObjectsCase: static (state, read) => state.Session.Demand(
                    use: document => read.ObjectIds
                        .Map(id => ReadObject(document, id, state.Op))
                        .Traverse(static value => value)
                        .Map<TextAnswer>(static values => new TextAnswer.ObjectsCase(values)),
                    key: state.Op,
                    needs: SessionNeed.Read),
                searchCase: static (state, search) => state.Session.Demand(
                    use: document => Search(document, search.Search, state.Op)
                        .Map<TextAnswer>(static values => new TextAnswer.MatchesCase(values)),
                    key: state.Op,
                    needs: SessionNeed.Read))
               select answer;
    }

    private static Fin<TextOperation> Admit(TextOperation operation, Op op) => operation.Switch<Op, Fin<TextOperation>>(
        state: op,
        mutateCase: static (op, mutate) => Admit(mutate.Batch, op)
            .Map<TextOperation>(static batch => new TextOperation.MutateCase(batch)),
        readDocumentCase: static (_, _) => Fin.Succ<TextOperation>(new TextOperation.ReadDocumentCase()),
        readObjectsCase: static (op, read) => guard(
                read.ObjectIds.ForAll(static id => id != Guid.Empty),
                op.InvalidInput())
            .ToFin()
            .Map<TextOperation>(_ => new TextOperation.ReadObjectsCase(read.ObjectIds)),
        searchCase: static (op, search) => Admit(search.Search, op)
            .Map<TextOperation>(static admitted => new TextOperation.SearchCase(admitted)));

    private static Fin<TextMutationBatch> Admit(TextMutationBatch batch, Op op) =>
        Optional(batch).ToFin(Fail: op.InvalidInput())
            .Bind(active => active.Switch<Op, Fin<TextMutationBatch>>(
                state: op,
                documentCase: static (op, document) => Admit(
                        document.Mutations,
                        static address => address is not TextAddress.ObjectCase,
                        op)
                    .Map<TextMutationBatch>(static mutations => new TextMutationBatch.DocumentCase(mutations)),
                objectsCase: static (op, objects) => Admit(
                        objects.Mutations,
                        static address => address is TextAddress.ObjectCase,
                        op)
                    .Map<TextMutationBatch>(static mutations => new TextMutationBatch.ObjectsCase(mutations))));

    private static Fin<TextSearch> Admit(TextSearch search, Op op) =>
        from source in Optional(search).ToFin(Fail: op.InvalidInput())
        from key in op.AcceptValidated<TextKey>(source.Key.Value)
        from pattern in op.AcceptValidated<TextValue>(source.Pattern.Value)
        from policy in Admit(source.Policy, op)
        from filter in Admit(source.Filter, op)
        select new TextSearch(key, pattern, policy, filter);

    private static Fin<TextSearchPolicy> Admit(TextSearchPolicy policy, Op op) =>
        from source in Optional(policy).ToFin(Fail: op.InvalidInput())
        from _stores in guard(source.Stores.ForAll(static store => store is not null), op.InvalidInput()).ToFin()
        from admitted in op.AcceptValidated<TextSearchPolicy>(
            TextSearchPolicy.Validate(source.Stores, source.Comparison, out TextSearchPolicy? value),
            value)
        select admitted;

    private static Fin<TextObjectFilter> Admit(TextObjectFilter filter, Op op) =>
        Optional(filter).ToFin(Fail: op.InvalidInput())
            .Bind(active => active.Switch<Op, Fin<TextObjectFilter>>(
                state: op,
                kindsCase: static (_, kinds) => Fin.Succ<TextObjectFilter>(new TextObjectFilter.KindsCase(kinds.Kinds)),
                enumeratorCase: static (op, enumerator) => Optional(enumerator.Settings)
                    .ToFin(Fail: op.InvalidInput())
                    .Map<TextObjectFilter>(static settings => new TextObjectFilter.EnumeratorCase(settings))));

    private static Fin<TextMutation> Admit(TextMutation mutation, Op op) =>
        from source in Optional(mutation).ToFin(Fail: op.InvalidInput())
        from address in Admit(source.Address, op)
        from edit in Admit(source.Edit, op)
        select new TextMutation(address, edit);

    private static Fin<TextAddress> Admit(TextAddress address, Op op) =>
        Optional(address).ToFin(Fail: op.InvalidInput())
            .Bind(active => active.Switch<Op, Fin<TextAddress>>(
                state: op,
                documentKeyCase: static (op, document) => op.AcceptValidated<TextKey>(document.Key.Value)
                    .Map<TextAddress>(static key => new TextAddress.DocumentKeyCase(key)),
                documentSectionCase: static (op, document) =>
                    from source in Optional(document.Address).ToFin(Fail: op.InvalidInput())
                    from section in op.AcceptValidated<TextSection>(
                        TextSection.Validate(source.Section, source.Entry, out TextSection? value),
                        value)
                    select (TextAddress)new TextAddress.DocumentSectionCase(section),
                objectCase: static (op, value) =>
                    from _id in guard(value.ObjectId != Guid.Empty, op.InvalidInput()).ToFin()
                    from store in Optional(value.Store).ToFin(Fail: op.InvalidInput())
                    from key in op.AcceptValidated<TextKey>(value.Key.Value)
                    select (TextAddress)new TextAddress.ObjectCase(value.ObjectId, store, key)));

    private static Fin<TextEdit> Admit(TextEdit edit, Op op) =>
        Optional(edit).ToFin(Fail: op.InvalidInput())
            .Bind(active => active.Switch<Op, Fin<TextEdit>>(
                state: op,
                setCase: static (op, write) => op.AcceptValidated<TextValue>(write.Value.Value)
                    .Map<TextEdit>(static value => new TextEdit.SetCase(value)),
                deleteCase: static (_, _) => Fin.Succ<TextEdit>(new TextEdit.DeleteCase())));

    private static Fin<TextAnswer> Mutate(DocumentSession session, TextMutationBatch batch, Op op) =>
        batch.Switch<(DocumentSession Session, Op Op), Fin<TextAnswer>>(
        state: (session, op),
        documentCase: static (state, document) =>
            from changes in state.Session.Demand(
                use: owner => MutateDocuments(owner, document.Mutations, state.Op),
                key: state.Op,
                needs: [SessionNeed.Read, SessionNeed.Mutate, SessionNeed.Undo])
            select (TextAnswer)new TextAnswer.MutationCase(new TextMutationReceipt(changes)),
        objectsCase: static (state, objects) =>
            from plans in state.Session.Demand(
                use: owner => PlanObjects(owner, objects.Mutations, state.Op),
                key: state.Op,
                needs: SessionNeed.Read)
            from changes in CommitObjectPlans(state.Session, plans, state.Op)
            select (TextAnswer)new TextAnswer.MutationCase(new TextMutationReceipt(changes)));

    private static Fin<Seq<TextDelta>> MutateDocuments(
        RhinoDoc document,
        Seq<TextMutation> mutations,
        Op op) =>
        DocumentCommit.Sealed(
            document: document,
            name: nameof(Commit),
            recordsUndo: true,
            redraw: RedrawPolicy.None,
            run: () => mutations
                .Map(mutation => MutateDocument(document, mutation, op))
                .Traverse(static change => change),
            stamp: static (changes, _) => changes,
            op: op);

    private static Fin<Seq<(TableOp Operation, Seq<TextDelta> Evidence)>> PlanObjects(
        RhinoDoc document,
        Seq<TextMutation> mutations,
        Op op) =>
        from attributeGroups in ObjectGroups(mutations, static store => store.SearchRoute.Attributes, op)
        from attributes in attributeGroups.OrderBy(static group => group.Key)
            .Map(group => op.Catch(() => Optional(document.Objects.FindId(group.Key))
                .ToFin(Fail: op.InvalidResult(detail: $"Object '{group.Key}' does not exist.")))
                .Bind(value => PlanAttributes(value, group.Value, op)))
            .Traverse(static plan => plan)
        from geometryGroups in ObjectGroups(mutations, static store => store.SearchRoute.Geometry, op)
        from geometry in geometryGroups.OrderBy(static group => group.Key)
            .Map(group => op.Catch(() => Optional(document.Objects.FindId(group.Key))
                .ToFin(Fail: op.InvalidResult(detail: $"Object '{group.Key}' does not exist.")))
                .Bind(value => PlanGeometry(value, group.Value, op)))
            .Traverse(static plan => plan)
        select attributes.Concat(geometry).ToSeq();

    private static Fin<HashMap<Guid, Seq<TextMutation>>> ObjectGroups(
        Seq<TextMutation> mutations,
        Func<ObjectTextStore, bool> includes,
        Op op) =>
        mutations.Fold(
            Succ(HashMap<Guid, Seq<TextMutation>>()),
            (state, mutation) => state.Bind(groups => mutation.Address switch
            {
                TextAddress.ObjectCase address when includes(address.Store) =>
                    Succ(groups.Find(address.ObjectId).Match(
                        Some: grouped => groups.SetItem(address.ObjectId, grouped.Add(mutation)),
                        None: () => groups.Add(address.ObjectId, Seq(mutation)))),
                TextAddress.ObjectCase => Succ(groups),
                _ => Fail<HashMap<Guid, Seq<TextMutation>>>(op.InvalidInput()),
            }));

    private static Fin<Seq<TextMutation>> Admit(
        Seq<TextMutation> mutations,
        Func<TextAddress, bool> belongs,
        Op op) =>
        from _nonempty in guard(!mutations.IsEmpty, op.InvalidInput()).ToFin()
        from admitted in mutations
            .Map(mutation => Admit(mutation, op).ToValidation())
            .Traverse(static value => value)
            .As()
            .ToFin()
        from _owner in guard(admitted.ForAll(mutation => belongs(mutation.Address)), op.InvalidInput()).ToFin()
        select admitted;

    private static Fin<TextDelta> MutateDocument(RhinoDoc document, TextMutation mutation, Op op) =>
        mutation.Address.Switch<(RhinoDoc Document, TextMutation Mutation, Op Op), Fin<TextDelta>>(
            state: (document, mutation, op),
            documentKeyCase: static (state, key) => ApplyDocument(
                state.Mutation,
                () => ReadFlat(state.Document, key.Key, state.Op),
                value => state.Document.Strings.SetString(key.Key.Value, value),
                () => state.Document.Strings.Delete(key.Key.Value),
                state.Op),
            documentSectionCase: static (state, section) => ApplyDocument(
                state.Mutation,
                () => ReadSection(state.Document, section.Address, state.Op),
                value => state.Document.Strings.SetString(section.Address.Section, section.Address.Entry, value),
                () => state.Document.Strings.Delete(section.Address.Section, section.Address.Entry),
                state.Op),
            objectCase: static (state, _) => Fail<TextDelta>(state.Op.InvalidInput()));

    private static Fin<TextDelta> ApplyDocument(
        TextMutation mutation,
        Func<Fin<Option<TextValue>>> read,
        Func<string, string> set,
        Action delete,
        Op op) =>
        from prior in op.Catch(read)
        let expected = mutation.Edit.Result
        from applied in op.Catch(() => mutation.Edit.Switch<Unit>(
            setCase: write =>
            {
                _ = set(write.Value.Value);
                return unit;
            },
            deleteCase: _ =>
            {
                delete();
                return unit;
            }))
        from current in op.Catch(read)
        from delta in current == expected
            ? Succ(new TextDelta(mutation.Address, prior, current))
            : Fail<TextDelta>(op.InvalidResult(detail: "Document text postcondition failed."))
        select delta;

    private static Fin<(TableOp Operation, Seq<TextDelta> Evidence)> PlanAttributes(
        RhinoObject source,
        Seq<TextMutation> mutations,
        Op op) =>
        from detached in Freeze(() => source.Attributes.GetUserStrings(), op)
        from planned in mutations.Fold(
            Succ((Values: detached, Evidence: Seq<TextDelta>())),
            (state, mutation) => state.Bind(plan =>
                from address in ObjectAddress(mutation.Address, op)
                let prior = plan.Values.Find(address.Key)
                let current = mutation.Edit.Result
                let values = current.Match(
                    Some: value => plan.Values.AddOrUpdate(address.Key, value),
                    None: () => plan.Values.Remove(address.Key))
                select (
                    Values: values,
                    Evidence: plan.Evidence.Add(new TextDelta(mutation.Address, prior, current)))))
        from target in TableTarget.Of(source.Id)
        from operation in TableOp.Amend(
            target,
            attributes => mutations.Fold(
                Succ(Seq<TextDelta>()),
                (state, mutation) => state.Bind(evidence => ApplyObject(
                    attributes.GetUserString,
                    attributes.SetUserString,
                    attributes.DeleteUserString,
                    mutation,
                    op).Map(evidence.Add)))
                .Bind(actual => actual.SequenceEqual(planned.Evidence)
                    ? Succ(unit)
                    : Fail<Unit>(op.InvalidResult(detail: "Attribute text plan diverged before commit."))),
            Notice.Quiet)
        select (operation, planned.Evidence);

    private static Fin<(TableOp Operation, Seq<TextDelta> Evidence)> PlanGeometry(
        RhinoObject source,
        Seq<TextMutation> mutations,
        Op op) =>
        from edited in op.Catch(() => Optional(source.Geometry.Duplicate())
            .ToFin(Fail: op.InvalidResult(detail: "Geometry duplication returned no custody.")))
        from planned in (
            from deltas in mutations.Fold(
                Succ(Seq<TextDelta>()),
                (state, mutation) => state.Bind(changes => ApplyObject(
                    edited.GetUserString,
                    edited.SetUserString,
                    edited.DeleteUserString,
                    mutation,
                    op).Map(delta => changes.Add(delta))))
            from target in TableTarget.Of(source.Id)
            from operation in TableOp.Replace(target, edited, ObjectMode.Respect)
            select (operation, deltas))
            .BindFail(error => op.Catch(edited.Dispose).Match(
                Succ: _ => Fail<(TableOp, Seq<TextDelta>)>(error),
                Fail: release => Fail<(TableOp, Seq<TextDelta>)>(error + release)))
        select planned;

    private static Fin<Seq<TextDelta>> CommitObjectPlans(
        DocumentSession session,
        Seq<(TableOp Operation, Seq<TextDelta> Evidence)> plans,
        Op op) =>
        plans.IsEmpty
            ? Succ(Seq<TextDelta>())
            : (from evidence in plans
                    .Map((plan, index) => (!plan.Evidence.IsEmpty
                        ? Succ(plan.Evidence)
                        : Fail<Seq<TextDelta>>(op.InvalidResult(
                            detail: $"Object text plan '{index}' produced no evidence."))).ToValidation())
                    .Traverse(static value => value)
                    .As()
                    .ToFin()
               from transaction in TableTransaction.Recorded(
                    nameof(Commit),
                    RedrawPolicy.None,
                    Seq<TableCustomUndo>(),
                    plans.Map(static plan => plan.Operation).ToArray())
               from _committed in Tables.Commit(session, transaction, op)
               select evidence.Bind(static changes => changes).ToSeq());

    private static Fin<TextDelta> ApplyObject(
        Func<string, string?> get,
        Func<string, string, bool> set,
        Func<string, bool> delete,
        TextMutation mutation,
        Op op) =>
        from address in ObjectAddress(mutation.Address, op)
        from prior in op.Catch(() => Value(get(address.Key.Value), op))
        from accepted in op.Catch(() => Fin.Succ(value: mutation.Edit.Switch<bool>(
            setCase: write => set(address.Key.Value, write.Value.Value),
            deleteCase: _ => delete(address.Key.Value))))
        from current in op.Catch(() => Value(get(address.Key.Value), op))
        from delta in accepted || prior == current
            ? Succ(new TextDelta(mutation.Address, prior, current))
            : Fail<TextDelta>(op.InvalidResult(detail: $"Object text mutation '{address.Key.Value}' was rejected."))
        select delta;

    private static Fin<TextAddress.ObjectCase> ObjectAddress(TextAddress address, Op op) =>
        address.Switch<Op, Fin<TextAddress.ObjectCase>>(
            state: op,
            documentKeyCase: static (op, _) => Fail<TextAddress.ObjectCase>(op.InvalidInput()),
            documentSectionCase: static (op, _) => Fail<TextAddress.ObjectCase>(op.InvalidInput()),
            objectCase: static (_, value) => Succ(value));

    private static Fin<DocumentTextSnapshot> ReadDocument(RhinoDoc document, Op op) =>
        op.Catch(() => Enumerable.Range(0, document.Strings.Count)
            .Map(index =>
            {
                string raw = document.Strings.GetKey(index);
                int separator = raw.IndexOf('\\', StringComparison.Ordinal);
                return from value in op.AcceptValidated<TextValue>(document.Strings.GetValue(index))
                       from row in separator < 0
                           ? op.AcceptValidated<TextKey>(raw).Map(key => (
                               Raw: raw,
                               Flat: Some(key),
                               Section: Option<TextSection>.None,
                               Value: value))
                           : op.Catch(() => Fin.Succ(value: TextSection.Create(raw[..separator], raw[(separator + 1)..])))
                               .Map(section => (
                                   Raw: raw,
                                   Flat: Option<TextKey>.None,
                                   Section: Some(section),
                                   Value: value))
                       select row;
            })
            .Traverse(static row => row))
            .Bind(rows => rows.Fold(
                Fin.Succ(value: (Flat: HashMap<TextKey, TextValue>(), Sections: HashMap<TextSection, TextValue>())),
                (state, row) => state.Bind(maps => row.Flat.Match(
                    Some: key => maps.Flat.ContainsKey(key)
                        ? Fail<(HashMap<TextKey, TextValue> Flat, HashMap<TextSection, TextValue> Sections)>(
                            Collision(nameof(TextKey), row.Raw, key.Value, op))
                        : Succ(maps with { Flat = maps.Flat.Add(key, row.Value) }),
                    None: () => row.Section.Match(
                        Some: section => maps.Sections.ContainsKey(section)
                            ? Fail<(HashMap<TextKey, TextValue> Flat, HashMap<TextSection, TextValue> Sections)>(
                                Collision(nameof(TextSection), row.Raw, $"{section.Section}\\{section.Entry}", op))
                            : Succ(maps with { Sections = maps.Sections.Add(section, row.Value) }),
                        None: () => Fail<(HashMap<TextKey, TextValue> Flat, HashMap<TextSection, TextValue> Sections)>(
                            op.InvalidResult()))))))
            .Bind(maps => op.Catch(() => Fin.Succ(value: new DocumentTextSnapshot(
                maps.Flat,
                maps.Sections,
                document.Strings.DocumentUserTextCount,
                document.Strings.DocumentDataCount))));

    private static Fin<ObjectTextSnapshot> ReadObject(RhinoDoc document, Guid objectId, Op op) => op.Catch(() =>
        Optional(document.Objects.FindId(objectId))
            .ToFin(Fail: op.InvalidResult(detail: $"Object '{objectId}' does not exist."))
            .Bind(value =>
                from attributes in Freeze(() => value.Attributes.GetUserStrings(), op)
                from geometry in Freeze(() => value.Geometry.GetUserStrings(), op)
                select new ObjectTextSnapshot(
                    objectId,
                    attributes,
                    geometry,
                    value.Attributes.UserStringCount)));

    private static Fin<Seq<TextMatch>> Search(RhinoDoc document, TextSearch search, Op op) =>
        from attributes in search.Policy.SearchRoute.Attributes
            ? Find(document, search, ObjectTextStore.Attributes, op)
            : Succ(HashSet<Guid>())
        from geometry in search.Policy.SearchRoute.Geometry
            ? Find(document, search, ObjectTextStore.Geometry, op)
            : Succ(HashSet<Guid>())
        from matches in op.Catch(() => Fin.Succ(value: attributes.Union(geometry)
            .OrderBy(static id => id)
            .Map(id => TextMatch.Create(
                id,
                toSeq(ObjectTextStore.Items)
                    .Filter(store => (store.SearchRoute.Attributes && attributes.Contains(id))
                        || (store.SearchRoute.Geometry && geometry.Contains(id)))
                    .ToHashSet()))
            .ToSeq()))
        select matches;

    private static Fin<HashSet<Guid>> Find(
        RhinoDoc document,
        TextSearch search,
        ObjectTextStore store,
        Op op) =>
        from route in Optional(store).ToFin(Fail: op.InvalidInput()).Map(static active => active.SearchRoute)
        from values in op.Catch(() => Fin.Succ(value: search.Filter.Switch<(
                RhinoDoc Document,
                TextSearch Search,
                (bool Geometry, bool Attributes) Route), RhinoObject[]?>(
                state: (document, search, route),
                kindsCase: static (state, kinds) => state.Document.Objects.FindByUserString(
                    state.Search.Key.Value, state.Search.Pattern.Value, state.Search.Policy.CaseSensitive,
                    state.Route.Geometry, state.Route.Attributes, kinds.Kinds),
                enumeratorCase: static (state, enumerator) => state.Document.Objects.FindByUserString(
                    state.Search.Key.Value, state.Search.Pattern.Value, state.Search.Policy.CaseSensitive,
                    state.Route.Geometry, state.Route.Attributes, enumerator.Settings))))
        select Optional(values).IfNone(Array.Empty<RhinoObject>())
            .Map(static value => value.Id)
            .ToHashSet();

    private static Fin<Option<TextValue>> ReadFlat(RhinoDoc document, TextKey key, Op op) =>
        Enumerable.Range(0, document.Strings.Count)
            .Exists(index => string.Equals(document.Strings.GetKey(index), key.Value, StringComparison.Ordinal))
            ? Value(document.Strings.GetValue(key.Value), op)
            : Fin.Succ(value: Option<TextValue>.None);

    private static Fin<Option<TextValue>> ReadSection(RhinoDoc document, TextSection address, Op op) =>
        document.Strings.GetEntryNames(address.Section)
            .Exists(entry => string.Equals(entry, address.Entry, StringComparison.Ordinal))
            ? Value(document.Strings.GetValue(address.Section, address.Entry), op)
            : Fin.Succ(value: Option<TextValue>.None);

    private static Fin<HashMap<TextKey, TextValue>> Freeze(Func<NameValueCollection> read, Op op) =>
        op.Catch(() => Fin.Succ(value: read())).Bind(source =>
            toSeq(source.AllKeys.OfType<string>())
                .Map(raw => from admitted in op.AcceptValidated<TextKey>(raw)
                            from value in op.AcceptValidated<TextValue>(source[raw])
                            select (Raw: raw, Key: admitted, Value: value))
                .Traverse(static row => row)
                .Bind(rows => rows.Fold(
                    Succ(HashMap<TextKey, TextValue>()),
                    (state, row) => state.Bind(map => map.ContainsKey(row.Key)
                        ? Fail<HashMap<TextKey, TextValue>>(Collision(nameof(TextKey), row.Raw, row.Key.Value, op))
                        : Succ(map.Add(row.Key, row.Value))))));

    private static Error Collision(string label, string raw, string canonical, Op op) => new Fault.InvalidValue(
        Label: label,
        Requirement: $"Distinct host key '{raw}' collapses onto canonical key '{canonical}'.",
        Key: Some(op));

    private static Fin<Option<TextValue>> Value(string? source, Op op) => source is null
        ? Fin.Succ(value: Option<TextValue>.None)
        : op.AcceptValidated<TextValue>(source).Map(Some);
}
```

## [04]-[LIFECYCLE]

`TextOperation` admits once, `Texts.Commit` resolves one session, and the active case selects pure read, wildcard search, document undo, or object-table commit. Host-key folds reject canonical collisions before map insertion. Object plans admit complete evidence before `Tables.Commit`, and each attribute callback must reproduce its planned delta before host mutation. Mutation receipts derive from the same clones and native values sent to the host, and their counts include only prior/current transitions.

Search policy remains one value across admission and projects `SearchRoute` and `CaseSensitive` only inside `FindByUserString`. Store rows retain independent host routing, and each match carries the exact contributing row set.

## [05]-[SEAMS]

`DocumentStream` alone observes `RhinoDoc.UserStringChanged`; `Texts.Commit` never creates a parallel event surface. Object mutation terminates at `Tables.Commit`, while document text remains under the session's undo bracket.

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
