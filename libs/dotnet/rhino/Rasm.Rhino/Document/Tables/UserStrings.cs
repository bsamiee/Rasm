using System.Collections.Specialized;
using LanguageExt.ClassInstances;
using Rhino;

namespace Rasm.Rhino.Document.Tables;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class KeyName {
    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref string value) =>
        validationError = value.Length > 0 && !value.Contains(DocumentKey.Separator, StringComparison.Ordinal) ? null : new InvalidRhinoValue();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DocumentKey {
    internal const char Separator = '\\';

    public abstract string Text { get; }

    public sealed record Flat(KeyName Name) : DocumentKey {
        public override string Text => Name;
    }

    public sealed record Section(KeyName Name, KeyName Entry) : DocumentKey {
        public override string Text => string.Join(Separator, Name, Entry);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class UserStrings {
    // --- [OWNERS]
    public static HashMap<EqStringOrdinalIgnoreCase, string, string> Held(NameValueCollection bag) =>
        toHashMap<EqStringOrdinalIgnoreCase, string, string>(toSeq(bag.AllKeys).Map(key => (key!, bag[key]!)));

    public static HashMap<EqStringOrdinalIgnoreCase, string, Option<string>> Replacing(
        HashMap<EqStringOrdinalIgnoreCase, string, string> held, HashMap<EqStringOrdinalIgnoreCase, string, string> desired) =>
        held.Union<string, Option<string>>(
            desired,
            MapLeft: static (_, _) => Option<string>.None,
            MapRight: static (_, value) => Some(value),
            Merge: static (_, _, value) => Some(value));

    public static IO<Unit> Write(
        HashMap<EqStringOrdinalIgnoreCase, string, string> held, Func<string, string, bool> set, HashMap<EqStringOrdinalIgnoreCase, string, Option<string>> edits) =>
        IO.lift(() => Callbacks.Each(
            toSeq<(string Key, Option<string> Value)>(edits),
            edit => held.Find(edit.Key) == edit.Value.Bind(Conversions.Present) || set(edit.Key, Conversions.Unset(edit.Value)),
            nameof(GeometryBase.SetUserString)));

    // --- [DOCUMENT]
    public static IO<Option<string>> Value(RhinoDoc doc, DocumentKey key) =>
        IO.lift(() => Conversions.Present(doc.Strings.GetValue(key.Text)));

    public static IO<Option<string>> Write(RhinoDoc doc, DocumentKey key, Option<string> value) =>
        IO.lift(() => Conversions.Present(doc.Strings.SetString(key.Text, Conversions.Unset(value))));

    public static IO<HashMap<EqStringOrdinalIgnoreCase, string, string>> Entries(RhinoDoc doc, KeyName section) =>
        IO.lift(() => {
            string prefix = $"{section}{DocumentKey.Separator}";
            return toHashMap<EqStringOrdinalIgnoreCase, string, string>(toSeq(Range(0, doc.Strings.Count)).Map(doc.Strings.GetKey)
                .Filter(key => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Map(key => (key[prefix.Length..], doc.Strings.GetValue(key))));
        });

    public static IO<Unit> Clear(RhinoDoc doc, KeyName section) =>
        Entries(doc, section).Bind(entries => IO.lift(() => {
            foreach (string entry in entries.Keys)
                doc.Strings.Delete(section, entry);
            doc.Strings.Delete(section);
        }));
}
