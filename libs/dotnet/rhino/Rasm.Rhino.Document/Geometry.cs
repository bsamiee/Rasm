using System.Collections.Specialized;
using Rhino.DocObjects;

namespace Rasm.Rhino.Document;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record GeometryPair(GeometryBase Geometry, Option<ObjectAttributes> Attributes);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record UserStringEdit {
    public sealed record Set : UserStringEdit {
        private Set(string key, string value) => (Key, Value) = (key, value);

        public string Key { get; }

        public string Value { get; }

        public static Fin<UserStringEdit> From(string key, string value) => Invalid.Unless<UserStringEdit>(value.Length > 0, new Set(key, value), nameof(Set));
    }

    public sealed record Delete(string Key) : UserStringEdit;

    public sealed record DeleteAll() : UserStringEdit;
}

public sealed record UserStringAccessors(Func<string, string, bool> Set, Func<string, bool> Delete, Action DeleteAll, Func<NameValueCollection> GetAll);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class GeometryOps {
    // --- [COPIES]
    public static IO<T> Duplicated<T>(T source) where T : GeometryBase =>
        IO.lift(() => Missing.Unless(source.Duplicate() as T, nameof(GeometryBase.Duplicate)));

    public static IO<TValue> WithGeometry<T, TValue>(T source, Func<T, IO<TValue>> body) where T : GeometryBase =>
        DisposalOps.Using(Duplicated(source), body);

    // --- [USER_STRINGS]
    public static IO<HashMap<string, string>> ReadUserStrings(UserStringAccessors accessors) =>
        from strings in IO.lift(accessors.GetAll)
        select toHashMap(toSeq(strings.AllKeys).Choose(key =>
            from present in Optional(key)
            from value in Optional(strings[present])
            select (present, value)));

    public static IO<Unit> EditUserStrings(UserStringAccessors accessors, UserStringEdit edit) =>
        edit.Switch(
            accessors,
            set: static (target, set) => IO.lift(() => Refused.Unless(target.Set(set.Key, set.Value), nameof(GeometryBase.SetUserString))),
            delete: static (target, delete) => IO.lift(() => { _ = target.Delete(delete.Key); }),
            deleteAll: static (target, _) => IO.lift(target.DeleteAll));

    public static UserStringAccessors UserStrings(ObjectAttributes attributes) =>
        new(attributes.SetUserString, attributes.DeleteUserString, attributes.DeleteAllUserStrings, attributes.GetUserStrings);

    public static UserStringAccessors UserStrings(DimensionStyle style) =>
        new(style.SetUserString, style.DeleteUserString, style.DeleteAllUserStrings, style.GetUserStrings);

    public static UserStringAccessors UserStrings(HatchPattern pattern) =>
        new(pattern.SetUserString, pattern.DeleteUserString, pattern.DeleteAllUserStrings, pattern.GetUserStrings);

    public static UserStringAccessors UserStrings(Linetype linetype) =>
        new(linetype.SetUserString, linetype.DeleteUserString, linetype.DeleteAllUserStrings, linetype.GetUserStrings);

    public static UserStringAccessors UserStrings(InstanceDefinitionGeometry definition) =>
        new(definition.SetUserString, definition.DeleteUserString, definition.DeleteAllUserStrings, definition.GetUserStrings);
}
