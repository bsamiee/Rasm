using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects.Tables;

namespace Rasm.Rhino.Persistence;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DocumentTextKey {
    public sealed record Flat(string Value) : DocumentTextKey;

    public sealed record Section(string Name, string Entry) : DocumentTextKey;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class UserTexts {
    public static IO<HashMap<string, string>> DocumentText(RhinoDoc doc) =>
        IO.lift(() => toHashMap(toSeq(Range(0, doc.Strings.Count)).Map(index => (doc.Strings.GetKey(index), doc.Strings.GetValue(index)))));

    public static IO<HashMap<string, string>> DocumentSection(RhinoDoc doc, string section) =>
        IO.lift(() => toHashMap(toSeq(doc.Strings.GetEntryNames(section)).Map(entry => (entry, doc.Strings.GetValue(section, entry)))));

    public static IO<Option<string>> DocumentValue(RhinoDoc doc, DocumentTextKey key) =>
        IO.lift(() => Answers.Present(key.Switch(
            doc.Strings,
            flat: static (strings, flat) => strings.GetValue(flat.Value),
            section: static (strings, section) => strings.GetValue(section.Name, section.Entry))));

    public static IO<Unit> WriteDocumentText(RhinoDoc doc, HashMap<DocumentTextKey, Option<string>> desired) =>
        from stored in IO.lift(() => Invalid.Unless(desired.Values.ForAll(static value => value.ForAll(static text => text.Length > 0)), nameof(StringTable.SetString)))
        from written in IO.lift(() => desired.Iter((key, value) => key.Switch(
            (doc.Strings, Text: Answers.Unset(value)),
            flat: static (scope, flat) => scope.Strings.SetString(flat.Value, scope.Text),
            section: static (scope, section) => scope.Strings.SetString(section.Name, section.Entry, scope.Text))))
        select written;
}
