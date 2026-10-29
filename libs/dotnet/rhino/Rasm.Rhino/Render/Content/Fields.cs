using System.Collections.Generic;
using System.Drawing;
using Rhino.Display;
using Rhino.Render;
using Rhino.Render.Fields;

namespace Rasm.Rhino.Render.Content;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ContentField<TState> where TState : notnull {
    internal ContentField(string key, Func<FieldDictionary, string, TState, Field> add, Func<FieldDictionary, TState, TState> read, Action<FieldDictionary, TState> write, bool file) =>
        (Key, Add, Read, Write, File) = (key, add, read, write, file);

    public string Key { get; }
    public Option<string> Prompt { get; init; }
    public bool Hashed { get; init; } = true;

    internal Func<FieldDictionary, string, TState, Field> Add { get; }
    internal Func<FieldDictionary, TState, TState> Read { get; }
    internal Action<FieldDictionary, TState> Write { get; }
    internal bool File { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ContentFields {
    // --- [FIELDS]
    public static ContentField<TState> Of<TState, T>(string key, Lens<TState, T> value, bool textured = false, bool linear = false) where TState : notnull where T : notnull =>
        Kinds.GetValueOrDefault(typeof(T)) is (Func<FieldDictionary, string, T, string, bool, bool, Field> add, Action<FieldDictionary, string, T> set)
            ? new(key, (fields, prompt, state) => add(fields, key, value.Get(state), prompt, textured, linear), Recalled(key, value), (fields, state) => set(fields, key, value.Get(state)), file: false)
            : throw new ArgumentException($"{typeof(T).Name} is no RDK field type", nameof(value));

    public static ContentField<TState> File<TState>(string key, Lens<TState, string> path) where TState : notnull =>
        new(key, (fields, prompt, state) => fields.AddFilename(key, path.Get(state), prompt, sectionId: 0), Recalled(key, path), (fields, state) => fields.Set(key, path.Get(state)), file: true);

    private static readonly FrozenDictionary<Type, object> Kinds = new Dictionary<Type, object> {
        [typeof(string)] = Kind<string>(static (f, k, v, p, t, l) => t ? f.AddTextured(k, v, p, l) : f.Add(k, v, p), static (f, k, v) => f.Set(k, v)),
        [typeof(bool)] = Kind<bool>(static (f, k, v, p, t, l) => t ? f.AddTextured(k, v, p, l) : f.Add(k, v, p), static (f, k, v) => f.Set(k, v)),
        [typeof(int)] = Kind<int>(static (f, k, v, p, t, l) => t ? f.AddTextured(k, v, p, l) : f.Add(k, v, p), static (f, k, v) => f.Set(k, v)),
        [typeof(float)] = Kind<float>(static (f, k, v, p, t, l) => t ? f.AddTextured(k, v, p, l) : f.Add(k, v, p), static (f, k, v) => f.Set(k, v)),
        [typeof(double)] = Kind<double>(static (f, k, v, p, t, l) => t ? f.AddTextured(k, v, p, l) : f.Add(k, v, p), static (f, k, v) => f.Set(k, v)),
        [typeof(Color4f)] = Kind<Color4f>(static (f, k, v, p, t, l) => t ? f.AddTextured(k, v, p, l) : f.Add(k, v, p), static (f, k, v) => f.Set(k, v)),
        [typeof(Color)] = Kind<Color>(static (f, k, v, p, t, l) => t ? f.AddTextured(k, v, p, l) : f.Add(k, v, p), static (f, k, v) => f.Set(k, v)),
        [typeof(Vector2d)] = Kind<Vector2d>(static (f, k, v, p, t, l) => t ? f.AddTextured(k, v, p, l) : f.Add(k, v, p), static (f, k, v) => f.Set(k, v)),
        [typeof(Vector3d)] = Kind<Vector3d>(static (f, k, v, p, t, l) => t ? f.AddTextured(k, v, p, l) : f.Add(k, v, p), static (f, k, v) => f.Set(k, v)),
        [typeof(Point2d)] = Kind<Point2d>(static (f, k, v, p, t, l) => t ? f.AddTextured(k, v, p, l) : f.Add(k, v, p), static (f, k, v) => f.Set(k, v)),
        [typeof(Point3d)] = Kind<Point3d>(static (f, k, v, p, t, l) => t ? f.AddTextured(k, v, p, l) : f.Add(k, v, p), static (f, k, v) => f.Set(k, v)),
        [typeof(Point4d)] = Kind<Point4d>(static (f, k, v, p, t, l) => t ? f.AddTextured(k, v, p, l) : f.Add(k, v, p), static (f, k, v) => f.Set(k, v)),
        [typeof(Guid)] = Kind<Guid>(static (f, k, v, p, t, l) => t ? f.AddTextured(k, v, p, l) : f.Add(k, v, p), static (f, k, v) => f.Set(k, v)),
        [typeof(Transform)] = Kind<Transform>(static (f, k, v, p, t, l) => t ? f.AddTextured(k, v, p, l) : f.Add(k, v, p), static (f, k, v) => f.Set(k, v)),
        [typeof(DateTime)] = Kind<DateTime>(static (f, k, v, p, t, l) => t ? f.AddTextured(k, v, p, l) : f.Add(k, v, p), static (f, k, v) => f.Set(k, v)),
    }.ToFrozenDictionary();

    private static object Kind<T>(Func<FieldDictionary, string, T, string, bool, bool, Field> add, Action<FieldDictionary, string, T> set) => (add, set);

    private static Func<FieldDictionary, TState, TState> Recalled<TState, T>(string key, Lens<TState, T> value) =>
        (fields, state) => fields.TryGetValue(key, out T held) ? value.Set(held, state) : state;

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
