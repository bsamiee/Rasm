using System.Collections.Generic;
using System.Collections.Specialized;
using LanguageExt.ClassInstances;
using Rhino;
using Rhino.Display;
using Rhino.FileIO;

namespace Rasm.Rhino.Persistence;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>(EqualityComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads, ComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[ValidationError<RhinoFault>]
public sealed partial class KeyName {
    public static Fin<KeyName> Validated(string value) => TryCreate(value, out KeyName? name, out RhinoFault? fault) ? name : fault;

    static partial void ValidateFactoryArguments(ref RhinoFault? validationError, ref string value) =>
        validationError = value.Length > 0 && !value.Contains(DocumentKey.Separator, StringComparison.Ordinal) ? null : new RhinoFault(value, RhinoReason.Unqualified);
}

public sealed record DocumentKey(KeyName Name, Option<KeyName> Entry = default) {
    internal const char Separator = '\\';

    public string Text => string.Join(Separator, Name.Cons(Entry.ToSeq()));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class UserStrings {
    // --- [OWNERS]
    public static HashMap<EqStringOrdinalIgnoreCase, string, string> Of(NameValueCollection bag) =>
        toHashMap<EqStringOrdinalIgnoreCase, string, string>(toSeq(bag.AllKeys).Map(key => (key, bag[key])));

    public static HashMap<EqStringOrdinalIgnoreCase, string, Option<string>> Replacing(
        HashMap<EqStringOrdinalIgnoreCase, string, string> held, HashMap<EqStringOrdinalIgnoreCase, string, string> desired) =>
        held.Map(static _ => Option<string>.None).Union(desired.Map(static value => Some(value)), static (_, _, value) => value);

    public static IO<Unit> Write(
        HashMap<EqStringOrdinalIgnoreCase, string, string> held, Func<string, string, bool> set, HashMap<EqStringOrdinalIgnoreCase, string, Option<string>> edits) =>
        IO.lift(() => Products.Each(Changed(held, edits), (edit, index) => RhinoFault.Unless(set(edit.Key, Conversions.Unset(edit.Value)), RhinoReason.Declined, edit.Key, index))).Map(unit);

    private static Seq<(string Key, Option<string> Value)> Changed(
        HashMap<EqStringOrdinalIgnoreCase, string, string> held, HashMap<EqStringOrdinalIgnoreCase, string, Option<string>> edits) =>
        toSeq<(string Key, Option<string> Value)>(edits).Filter(edit => held.Find(edit.Key) != edit.Value.Bind(Conversions.Present));

    // --- [VIEWPORT]
    public static IO<Unit> Write(RhinoViewport viewport, HashMap<EqStringOrdinalIgnoreCase, string, Option<string>> edits) =>
        from changed in IO.lift(() => Changed(Of(viewport.GetUserStrings()), edits))
        from written in unless(changed.IsEmpty, Products.Accepted(() => viewport.SetUserStrings(changed.Map(static edit => KeyValuePair.Create(edit.Key, Conversions.Unset(edit.Value))), replace: false))).As()
        select written;

    // --- [DOCUMENT]
    public static IO<Option<string>> Value(RhinoDoc doc, DocumentKey key) =>
        IO.lift(() => Conversions.Present(doc.Strings.GetValue(key.Text)));

    public static IO<Option<string>> Write(RhinoDoc doc, DocumentKey key, Option<string> value) =>
        Replaced(() => doc.Strings.GetValue(key.Text), text => ignore(doc.Strings.SetString(key.Text, text)), () => doc.Strings.Delete(key.Text), value);

    public static IO<Option<string>> Write(File3dm archive, DocumentKey key, Option<string> value) =>
        Replaced(() => archive.Strings.GetValue(key.Text), text => ignore(archive.Strings.SetString(key.Text, text)), () => archive.Strings.Delete(key.Text), value);

    private static IO<Option<string>> Replaced(Func<string?> read, Action<string> set, Action delete, Option<string> value) =>
        from prior in IO.lift(() => Conversions.Present(read()))
        let next = value.Bind(Conversions.Present)
        from written in when(prior != next, next.Match(Some: text => IO.lift(() => set(text)), None: () => IO.lift(delete))).As()
        select prior;

    public static IO<HashMap<EqStringOrdinalIgnoreCase, string, string>> Entries(RhinoDoc doc, KeyName section) =>
        IO.lift(() => toHashMap<EqStringOrdinalIgnoreCase, string, string>(
            toSeq(doc.Strings.GetEntryNames(section)).Map(entry => (entry, doc.Strings.GetValue(section, entry)))));
}
