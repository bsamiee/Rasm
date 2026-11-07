using System.Drawing;
using Rhino.Render;
using Rhino.Render.Fields;

namespace Rasm.Rhino.Render.Content;

// --- [MODELS] --------------------------------------------------------------------------

public sealed record ContentField<TState> where TState : notnull {
    internal ContentField(string key, Func<FieldDictionary, string, TState, Field> add, Func<Field, TState, TState> read, Func<TState, object> value, bool file) =>
        (Key, Add, Read, Value, File) = (key, add, read, value, file);

    public string Key { get; }
    public Option<string> Prompt { get; init; }
    public bool Hashed { get; init; } = true;

    internal Func<FieldDictionary, string, TState, Field> Add { get; }
    internal Func<Field, TState, TState> Read { get; }
    internal Func<TState, object> Value { get; }
    internal bool File { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class ContentFields {

    // --- [FIELDS]

    public static Fin<ContentField<TState>> Of<TState, T>(string key, Lens<TState, T> value, bool textured = false, bool linear = false) where TState : notnull where T : notnull {
        Fin<Func<FieldDictionary, string, T, string, Field>> add = textured
            ? Adding<Func<FieldDictionary, string, T, string, bool, Field>>(nameof(FieldDictionary.AddTextured))
                .Map<Func<FieldDictionary, string, T, string, Field>>(make => (fields, name, initial, prompt) => make(fields, name, initial, prompt, linear))
            : Adding<Func<FieldDictionary, string, T, string, Field>>(nameof(FieldDictionary.Add));
        return add.Map(make => new ContentField<TState>(key, (fields, prompt, state) => make(fields, key, value.Get(state), prompt),
            (field, state) => value.Set(typeof(T) == typeof(Color) ? (T)(object)((Color4fField)field).SystemColorValue : field.GetValue<T>(), state),
            state => value.Get(state), file: false));
    }

    public static ContentField<TState> File<TState>(string key, Lens<TState, string> path) where TState : notnull =>
        new(key, (fields, prompt, state) => fields.AddFilename(key, path.Get(state), prompt, sectionId: 0),
            (field, state) => path.Set(field.GetValue<string>(), state), state => path.Get(state), file: true);

    private static Fin<TDelegate> Adding<TDelegate>(string member) where TDelegate : Delegate {
        RhinoFault Rejected(Option<Error> inner) => new((typeof(TDelegate), member), RhinoReason.Unsupported, Inner: inner);
        return Try.lift(() => typeof(FieldDictionary)
                .GetMethod(member, [.. typeof(TDelegate).GetMethod(nameof(Action.Invoke))!.GetParameters().Skip(1).Select(static parameter => parameter.ParameterType)])
                ?.CreateDelegate<TDelegate>()).Run()
            .MapFail(error => Rejected(error))
            .Bind(binding => Optional(binding).Match<Fin<TDelegate>>(Some: static held => held, None: () => Rejected(None)));
    }

    // --- [PARAMETERS]

    public static IO<Option<T>> GetParameter<T>(RenderContent content, string parameter, Option<string> requirement = default) where T : notnull =>
        Internals.Unwrapped<T>(() => requirement.Match(Some: extra => content.GetExtraRequirementParameter(parameter, extra), None: () => content.GetParameter(parameter)), parameter);

    public static IO<Unit> SetParameter(
        RenderContent content, string parameter, object value, Option<string> requirement = default,
        RenderContent.ExtraRequirementsSetContexts context = RenderContent.ExtraRequirementsSetContexts.Program) =>
        requirement.Match(
            Some: extra => Products.Of(() => Internals.NewVariant(value))
                .Bind(variant => Products.Accepted(() => content.SetExtraRequirementParameter(parameter, extra, variant, context)))
                .Bracket(),
            None: () => Products.Accepted(() => content.SetParameter(parameter, value)));
}
