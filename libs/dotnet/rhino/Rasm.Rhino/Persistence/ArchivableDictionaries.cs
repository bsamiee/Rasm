using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using NodaTime.Serialization.SystemTextJson;
using Rhino;
using Rhino.Collections;
using Rhino.Runtime;
using UnitsNet;
using Wacton.Unicolour;

namespace Rasm.Rhino.Persistence;

// --- [MODELS] --------------------------------------------------------------------------

public sealed record DictionaryOwner(IO<ArchivableDictionary> Read, Func<Func<ArchivableDictionary, Fin<Unit>>, IO<Unit>> Edit) {
    public Option<RhinoDoc> Document { get; init; }

    public static DictionaryOwner Of<TOwner>(IO<TOwner> acquire, Option<Func<TOwner, IO<Unit>>> commit = default) where TOwner : CommonObject =>
        new(
            acquire.Map(static owner => owner.UserDictionary),
            edit =>
                from owner in acquire
                from edited in IO.lift(edit(owner.UserDictionary))
                from committed in commit.Traverse(write => write(owner)).As()
                select edited);
}

public sealed record RecordUpgrade<T>(Func<ArchivableDictionary, Fin<T>> Read, Option<string> Earlier = default);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record StoredRecord<T> {
    public Option<T> Present => Switch(decoded: static decoded => Some(decoded.Value), absent: static _ => Option<T>.None, newer: static _ => Option<T>.None);

    public sealed record Decoded(T Value) : StoredRecord<T>;
    public sealed record Absent : StoredRecord<T>;
    public sealed record Newer(ArchivableDictionary Raw) : StoredRecord<T>;
}

public sealed record DictionaryCodec<T>(string Name, int Version, Func<T, Fin<ArchivableDictionary>> Encode, Func<ArchivableDictionary, Fin<T>> Decode, Option<RecordUpgrade<T>> Upgrade = default) {
    public Fin<ArchivableDictionary> ToDictionary(T value) =>
        Encode(value).Map(encoded => {
            (encoded.Name, encoded.Version) = (Name, Version);
            return encoded;
        });

    public Fin<StoredRecord<T>> FromDictionary(ArchivableDictionary stored) =>
        stored.Version == Version ? Decode(stored).Map(Hold)
            : stored.Version > Version ? new StoredRecord<T>.Newer(stored)
            : Upgrade.Filter(_ => stored.Version == Version - 1).Match(
                Some: upgrade => upgrade.Read(stored).Map(Hold),
                None: static () => new StoredRecord<T>.Absent());

    public Error Refusal(ArchivableDictionary stored) =>
        new RhinoFault((Name, Stored: stored.Version, Current: Version), RhinoReason.Unreadable);

    private static StoredRecord<T> Hold(T value) => new StoredRecord<T>.Decoded(value);
}

// --- [SERVICES] ------------------------------------------------------------------------

internal sealed class TypeConverterFactory : JsonConverterFactory {
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsValueType && (typeToConvert == typeof(Color) || typeToConvert.IsAssignableTo(typeof(IQuantity)));

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
        (JsonConverter?)(typeToConvert.IsAssignableTo(typeof(IQuantity)) ? new Func<JsonConverter>(Quantity<Length>) : Described<Color>).Method
            .GetGenericMethodDefinition()
            .MakeGenericMethod(typeToConvert)
            .Invoke(obj: null, parameters: null);

    private static TypeConverterJson<T> Quantity<T>() where T : struct, IQuantity => new(new QuantityTypeConverter<T>());

    private static TypeConverterJson<T> Described<T>() => new(TypeDescriptor.GetConverter(typeof(T)));
}

internal sealed class TypeConverterJson<T>(TypeConverter converter) : JsonConverter<T> {
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.GetString() is { } text ? (T?)converter.ConvertFromInvariantString(text) : default;

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
        writer.WriteStringValue(converter.ConvertToInvariantString(value));
}

internal sealed class LanguageExtConverterFactory : JsonConverterFactory {
    private static readonly FrozenDictionary<Type, MethodInfo> Converters = new Dictionary<Type, MethodInfo> {
        [typeof(Option<>)] = new Func<JsonConverter>(OptionOf<Unit>).Method.GetGenericMethodDefinition(),
        [typeof(Seq<>)] = new Func<JsonConverter>(SeqOf<Unit>).Method.GetGenericMethodDefinition(),
        [typeof(Lst<>)] = new Func<JsonConverter>(LstOf<Unit>).Method.GetGenericMethodDefinition(),
        [typeof(HashMap<,>)] = new Func<JsonConverter>(HashMapOf<Unit, Unit>).Method.GetGenericMethodDefinition(),
    }.ToFrozenDictionary();

    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsGenericType && Converters.ContainsKey(typeToConvert.GetGenericTypeDefinition());

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
        (JsonConverter?)Converters[typeToConvert.GetGenericTypeDefinition()].MakeGenericMethod(typeToConvert.GetGenericArguments()).Invoke(obj: null, parameters: null);

    private static OptionJson<T> OptionOf<T>() => new();

    private static SeqJson<T> SeqOf<T>() => new();

    private static LstJson<T> LstOf<T>() => new();

    private static HashMapJson<TKey, TValue> HashMapOf<TKey, TValue>() where TKey : notnull => new();
}

internal sealed class OptionJson<T> : JsonConverter<Option<T>> {
    public override bool HandleNull => true;

    public override Option<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType is JsonTokenType.Null ? Option<T>.None : Optional(JsonSerializer.Deserialize<T>(ref reader, options));

    public override void Write(Utf8JsonWriter writer, Option<T> value, JsonSerializerOptions options) =>
        value.Match(Some: held => JsonSerializer.Serialize(writer, held, options), None: writer.WriteNullValue);
}

internal sealed class SeqJson<T> : JsonConverter<Seq<T>> {
    public override bool HandleNull => false;

    public override Seq<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        toSeq(JsonSerializer.Deserialize<T[]>(ref reader, options));

    public override void Write(Utf8JsonWriter writer, Seq<T> value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value.AsEnumerable(), options);
}

internal sealed class LstJson<T> : JsonConverter<Lst<T>> {
    public override bool HandleNull => false;

    public override Lst<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        toList(JsonSerializer.Deserialize<T[]>(ref reader, options));

    public override void Write(Utf8JsonWriter writer, Lst<T> value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value.AsEnumerable(), options);
}

internal sealed class HashMapJson<TKey, TValue> : JsonConverter<HashMap<TKey, TValue>> where TKey : notnull {
    public override bool HandleNull => false;

    public override HashMap<TKey, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        toHashMap(toSeq(JsonSerializer.Deserialize<Dictionary<TKey, TValue>>(ref reader, options)).Map(static entry => (entry.Key, entry.Value)));

    public override void Write(Utf8JsonWriter writer, HashMap<TKey, TValue> value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value.AsIterable().ToDictionary(static entry => entry.Key, static entry => entry.Value), options);
}

internal sealed class UnicolourJson : JsonConverter<Unicolour> {
    public override Unicolour Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        JsonSerializer.Deserialize<double[]>(ref reader, options) is [var red, var green, var blue, var alpha]
            ? new Unicolour(ColourSpace.RgbLinear, red, green, blue, alpha)
            : throw new JsonException($"{nameof(Unicolour)} holds no linear red, green, blue and alpha");

    public override void Write(Utf8JsonWriter writer, Unicolour value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, new[] { value.RgbLinear.R, value.RgbLinear.G, value.RgbLinear.B, value.Alpha.A }, options);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class ArchivableDictionaries {

    // --- [VALUES]

    public static Fin<Option<T>> Find<T>(ArchivableDictionary source, string key) where T : notnull =>
        source.TryGetValue(key, out object held) ? Conversions.Cast<T>(held, key).Map(Some) : Option<T>.None;

    public static Fin<T> Required<T>(ArchivableDictionary source, string key) where T : notnull =>
        from found in Find<T>(source, key)
        from value in found.ToFin(new RhinoFault(key, RhinoReason.Missing))
        select value;

    public static Fin<Seq<(string Key, ArchivableDictionary Value)>> Children(ArchivableDictionary source) =>
        Products.Each(Conversions.Rows(source), static (row, _) => Conversions.Cast<ArchivableDictionary>(row.Value, row.Key).Map(value => (row.Key, value)));

    public static Fin<Unit> Set<T>(ArchivableDictionary target, string key, T value) where T : notnull =>
        Setter<T>.Set.Match(
            Some: set => RhinoFault.Unless(set(target, key, value), RhinoReason.Declined, key),
            None: () => new RhinoFault(key, RhinoReason.Mismatched));

    public static Fin<ArchivableDictionary> Nested(Seq<(string Key, Fin<ArchivableDictionary> Value)> rows) =>
        from target in Fin.Succ(new ArchivableDictionary())
        from stored in rows.Traverse(row => row.Value.Bind(child => Set(target, row.Key, child))).As()
        select target;

    private static class Setter<T> {
        public static readonly Option<Func<ArchivableDictionary, string, T, bool>> Set =
            Optional(typeof(ArchivableDictionary).GetMethod(nameof(ArchivableDictionary.Set), BindingFlags.Public | BindingFlags.Instance | BindingFlags.ExactBinding, [typeof(string), typeof(T)]))
                .Map(static method => method.CreateDelegate<Func<ArchivableDictionary, string, T, bool>>());
    }

    // --- [USER_DICTIONARY]

    public static IO<Unit> Replace(DictionaryOwner owner, ArchivableDictionary source) =>
        owner.Edit(target => ignore(target.ReplaceContentsWith(source.Clone())));

    // --- [RECORDS]

    public static IO<Option<T>> Read<T>(DictionaryOwner owner, DictionaryCodec<T> codec) =>
        from source in owner.Read
        from stored in IO.lift(Stored(source, codec))
        from pending in owner.Document.Filter(_ => stored.Older).Traverse(doc => IO.lift(() => Rewrites(doc).AddOrUpdate((source, codec.Name), Rewritten(owner, codec)))).As()
        select stored.Record.Present;

    public static IO<Unit> Write<T>(DictionaryOwner owner, DictionaryCodec<T> codec, T value) =>
        from record in IO.lift(() => codec.ToDictionary(value))
        from written in owner.Edit(target =>
            from held in Find<ArchivableDictionary>(target, codec.Name)
            from current in held.Filter(stored => stored.Version > codec.Version).Match(Some: stored => Fin.Fail<Unit>(codec.Refusal(stored)), None: static () => unit)
            from set in Set(target, codec.Name, record)
            select Earlier(codec).Iter(name => ignore(target.Remove(name))))
        select written;

    public static IO<Unit> Remove<T>(DictionaryOwner owner, DictionaryCodec<T> codec) =>
        owner.Edit(target => (Seq(codec.Name) + Earlier(codec).ToSeq()).Iter(name => ignore(target.Remove(name))));

    public static IO<Unit> Rewrite(RhinoDoc doc) =>
        from pending in IO.lift(() => Rewrites(doc).ToHashMap())
        from written in Products.Each(toSeq(pending.AsIterable()).Map(row => row.Value.Bind(_ => IO.lift(() => Rewrites(doc).Remove(row.Key)))))
        select unit;

    private static Fin<(StoredRecord<T> Record, bool Older)> Stored<T>(ArchivableDictionary source, DictionaryCodec<T> codec) =>
        from found in (Find<ArchivableDictionary>(source, codec.Name),
                       Earlier(codec).Traverse(name => Find<ArchivableDictionary>(source, name)).As().Map(static earlier => earlier.Flatten()))
            .Apply(static (current, earlier) => (Current: current, Earlier: earlier)).As()
        from record in (found.Current | found.Earlier).Match(Some: codec.FromDictionary, None: static () => new StoredRecord<T>.Absent())
        select (record, found.Current.Match(Some: held => held.Version < codec.Version, None: () => found.Earlier.IsSome));

    private static Option<string> Earlier<T>(DictionaryCodec<T> codec) =>
        codec.Upgrade.Bind(static upgrade => upgrade.Earlier);

    private static IO<Unit> Rewritten<T>(DictionaryOwner owner, DictionaryCodec<T> codec) =>
        from source in owner.Read
        from stored in IO.lift(Stored(source, codec))
        from written in when(stored.Older, stored.Record.Switch(
            (Owner: owner, Codec: codec),
            decoded: static (state, decoded) => Write(state.Owner, state.Codec, decoded.Value),
            absent: static (state, _) => Remove(state.Owner, state.Codec),
            newer: static (_, _) => IO.pure(unit))).As()
        select written;

    private static AtomHashMap<(ArchivableDictionary Dictionary, string Name), IO<Unit>> Rewrites(RhinoDoc doc) =>
        doc.RuntimeData.GetValue(typeof(DictionaryOwner), static _ => AtomHashMap<(ArchivableDictionary Dictionary, string Name), IO<Unit>>());

    // --- [CODECS]

    private const string JsonKey = "Json";

    public static JsonSerializerOptions Options { get; } = new JsonSerializerOptions(JsonSerializerDefaults.Strict) {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { Stated } },
        IncludeFields = true,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        Converters = { new JsonStringEnumConverter(), new TypeConverterFactory(), new LanguageExtConverterFactory(), new UnicolourJson() },
    }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

    public static JsonTypeInfo<T> Info<T>() => (JsonTypeInfo<T>)Options.GetTypeInfo(typeof(T));

    private static void Stated(JsonTypeInfo info) =>
        toSeq(info.Properties).Filter(static property => property.Set is null && property.AssociatedParameter is null).Strict().Iter(property => info.Properties.Remove(property));

    public static DictionaryCodec<T> Json<T>(string name, int version, Option<RecordUpgrade<T>> upgrade = default) =>
        new(name, version,
            static value =>
                from bytes in Try.lift(() => JsonSerializer.SerializeToUtf8Bytes(value, Options)).Run()
                let target = new ArchivableDictionary()
                from written in Set<IEnumerable<byte>>(target, JsonKey, bytes)
                select target,
            stored =>
                from bytes in Required<byte[]>(stored, JsonKey)
                from value in Decoded(name, () => JsonSerializer.Deserialize<T>(bytes, Options))
                select value,
            upgrade);

    public static Fin<T> Decoded<T>(string name, Func<T?> read) =>
        Try.lift(read).Run()
            .MapFail(error => error.HasException<JsonException>() || error.HasException<NotSupportedException>() ? new RhinoFault(name, RhinoReason.Unreadable, Inner: error) : error)
            .Bind(value => Optional(value).ToFin(new RhinoFault(name, RhinoReason.Missing)));
}
