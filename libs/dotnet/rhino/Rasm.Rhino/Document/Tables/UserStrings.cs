using System.Collections.Specialized;
using LanguageExt.ClassInstances;
using Rhino;

namespace Rasm.Rhino.Document.Tables;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>(EqualityComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads, ComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads)]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class KeyName {
    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref string value) =>
        validationError = value.Length > 0 && !value.Contains(DocumentKey.Separator, StringComparison.Ordinal) ? null : new InvalidRhinoValue();
}

public sealed record DocumentKey(KeyName Name, Option<KeyName> Entry = default) {
    internal const char Separator = '\\';

    public string Text => string.Join(Separator, Name.Cons(Entry.ToSeq()));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class UserStrings {
    // --- [OWNERS]
    public static HashMap<EqStringOrdinalIgnoreCase, string, string> Held(NameValueCollection bag) =>
        toHashMap<EqStringOrdinalIgnoreCase, string, string>(toSeq(bag.AllKeys).Map(key => (key!, bag[key]!)));

    public static HashMap<EqStringOrdinalIgnoreCase, string, Option<string>> Replacing(
        HashMap<EqStringOrdinalIgnoreCase, string, string> held, HashMap<EqStringOrdinalIgnoreCase, string, string> desired) =>
        held.Map(static _ => Option<string>.None).Union(desired.Map(static value => Some(value)), static (_, _, value) => value);

    public static IO<Unit> Write(
        HashMap<EqStringOrdinalIgnoreCase, string, string> held, Func<string, string, bool> set, HashMap<EqStringOrdinalIgnoreCase, string, Option<string>> edits) =>
        IO.lift(() => Callbacks.Each(toSeq<(string Key, Option<string> Value)>(edits),
            edit => held.Find(edit.Key) == edit.Value.Bind(Conversions.Present) || set(edit.Key, Conversions.Unset(edit.Value)), nameof(GeometryBase.SetUserString)));

    // --- [DOCUMENT]
    public static IO<Option<string>> Value(RhinoDoc doc, DocumentKey key) =>
        IO.lift(() => Conversions.Present(doc.Strings.GetValue(key.Text)));

    public static IO<Option<string>> Write(RhinoDoc doc, DocumentKey key, Option<string> value) =>
        Value(doc, key).Bind(prior => IO.lift(() => prior == value.Bind(Conversions.Present) ? prior : Conversions.Present(doc.Strings.SetString(key.Text, Conversions.Unset(value)))));

    public static IO<HashMap<EqStringOrdinalIgnoreCase, string, string>> Entries(RhinoDoc doc, KeyName section) =>
        IO.lift(() => toHashMap<EqStringOrdinalIgnoreCase, string, string>(
            from index in toSeq(Range(0, doc.Strings.Count))
            let path = doc.Strings.GetKey(index).Split(DocumentKey.Separator, 2)
            where path is [var name, _] && section == name
            select (path[1], doc.Strings.GetValue(index))));

    public static IO<Unit> Clear(RhinoDoc doc, KeyName section) =>
        Entries(doc, section).Bind(entries => IO.lift(() => entries.Keys.Iter(entry => doc.Strings.Delete(section, entry))));
}
