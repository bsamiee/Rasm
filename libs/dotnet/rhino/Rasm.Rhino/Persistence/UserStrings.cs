using System.Collections.Specialized;
using LanguageExt.ClassInstances;
using Rhino;
using Rhino.Display;

namespace Rasm.Rhino.Persistence;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>(EqualityComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads, ComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class KeyName {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = value.Length > 0 && !value.Contains(DocumentKey.Separator, StringComparison.Ordinal) ? null : new ValidationError($"key name is empty or holds {DocumentKey.Separator}");
}

public sealed record DocumentKey(KeyName Name, Option<KeyName> Entry = default) {
    internal const char Separator = '\\';

    public string Text => string.Join(Separator, Name.Cons(Entry.ToSeq()));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class UserStrings {
    // --- [OWNERS]
    public static HashMap<EqStringOrdinalIgnoreCase, string, string> Held(NameValueCollection bag) =>
        toHashMap<EqStringOrdinalIgnoreCase, string, string>(toSeq(bag.AllKeys).Map(key => (key, bag[key])));

    public static HashMap<EqStringOrdinalIgnoreCase, string, Option<string>> Replacing(
        HashMap<EqStringOrdinalIgnoreCase, string, string> held, HashMap<EqStringOrdinalIgnoreCase, string, string> desired) =>
        held.Map(static _ => Option<string>.None).Union(desired.Map(static value => Some(value)), static (_, _, value) => value);

    public static IO<Unit> Write(
        HashMap<EqStringOrdinalIgnoreCase, string, string> held, Func<string, string, bool> set, HashMap<EqStringOrdinalIgnoreCase, string, Option<string>> edits) =>
        IO.lift(() => Callbacks.Each(Changed(held, edits), edit => set(edit.Key, Conversions.Unset(edit.Value)), nameof(GeometryBase.SetUserString)));

    private static Seq<(string Key, Option<string> Value)> Changed(
        HashMap<EqStringOrdinalIgnoreCase, string, string> held, HashMap<EqStringOrdinalIgnoreCase, string, Option<string>> edits) =>
        toSeq<(string Key, Option<string> Value)>(edits).Filter(edit => held.Find(edit.Key) != edit.Value.Bind(Conversions.Present));

    // --- [VIEWPORT]
    public static IO<Unit> Write(RhinoViewport viewport, HashMap<EqStringOrdinalIgnoreCase, string, Option<string>> edits) =>
        IO.lift(() => Changed(Held(viewport.GetUserStrings()), edits) switch {
            { IsEmpty: true } => unit,
            var changed => Refused.Unless(viewport.SetUserStrings(changed.Map(static edit => KeyValuePair.Create(edit.Key, Conversions.Unset(edit.Value))), replace: false), nameof(RhinoViewport.SetUserStrings)),
        });

    // --- [DOCUMENT]
    public static IO<Option<string>> Value(RhinoDoc doc, DocumentKey key) =>
        IO.lift(() => Conversions.Present(doc.Strings.GetValue(key.Text)));

    public static IO<Option<string>> Write(RhinoDoc doc, DocumentKey key, Option<string> value) =>
        from prior in Value(doc, key)
        let next = value.Bind(Conversions.Present)
        from written in when(prior != next, next.Match(
            Some: text => IO.lift(() => ignore(doc.Strings.SetString(key.Text, text))),
            None: () => IO.lift(() => doc.Strings.Delete(key.Text)))).As()
        select prior;

    public static IO<HashMap<EqStringOrdinalIgnoreCase, string, string>> Entries(RhinoDoc doc, KeyName section) =>
        IO.lift(() => toHashMap<EqStringOrdinalIgnoreCase, string, string>(
            toSeq(doc.Strings.GetEntryNames(section)).Map(entry => (entry, doc.Strings.GetValue(section, entry)))));

    public static IO<Unit> Clear(RhinoDoc doc, KeyName section) =>
        IO.lift(() => doc.Strings.Delete(section, entry: null));
}
