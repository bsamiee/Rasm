namespace System.Text.Json.Serialization;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal sealed class OptionJsonConverter<T> : JsonConverter<Option<T>> {
    public override bool HandleNull => true;

    public override Option<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType is JsonTokenType.Null ? None : Optional(JsonSerializer.Deserialize<T>(ref reader, options));

    public override void Write(Utf8JsonWriter writer, Option<T> value, JsonSerializerOptions options) =>
        _ = value.Match(Some: held => JsonSerializer.Serialize(writer, held, options), None: writer.WriteNullValue);
}

internal sealed class SeqJsonConverter<T> : JsonConverter<Seq<T>> {
    public override Seq<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        toSeq(LanguageJsonConverterFactory.Present(JsonSerializer.Deserialize<T[]>(ref reader, options)));

    public override void Write(Utf8JsonWriter writer, Seq<T> value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize<IEnumerable<T>>(writer, value, options);
}

internal sealed class HashSetJsonConverter<T> : JsonConverter<LanguageExt.HashSet<T>> {
    public override LanguageExt.HashSet<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        toHashSet(LanguageJsonConverterFactory.Present(JsonSerializer.Deserialize<T[]>(ref reader, options)));

    public override void Write(Utf8JsonWriter writer, LanguageExt.HashSet<T> value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, LanguageJsonConverterFactory.Ordered(value, static item => item), options);
}

internal sealed class HashMapJsonConverter<TKey, TValue> : JsonConverter<HashMap<TKey, TValue>> where TKey : notnull {
    public override HashMap<TKey, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        toHashMap(LanguageJsonConverterFactory.Present(JsonSerializer.Deserialize<Dictionary<TKey, TValue>>(ref reader, options)).Select(static pair => (pair.Key, pair.Value)));

    public override void Write(Utf8JsonWriter writer, HashMap<TKey, TValue> value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, LanguageJsonConverterFactory.Ordered(value.AsIterable(), static pair => pair.Key).ToDictionary(static pair => pair.Key, static pair => pair.Value), options);
}

internal sealed class MapJsonConverter<TKey, TValue> : JsonConverter<Map<TKey, TValue>> where TKey : notnull {
    public override Map<TKey, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        toMap(LanguageJsonConverterFactory.Present(JsonSerializer.Deserialize<Dictionary<TKey, TValue>>(ref reader, options)).Select(static pair => (pair.Key, pair.Value)));

    public override void Write(Utf8JsonWriter writer, Map<TKey, TValue> value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value.AsIterable().ToDictionary(static pair => pair.Key, static pair => pair.Value), options);
}

internal sealed class LanguageJsonConverterFactory : JsonConverterFactory {
    private static readonly HashMap<Type, Delegate> Converters = HashMap<Type, Delegate>(
        (typeof(Option<>), OptionConverter<object>),
        (typeof(Seq<>), SeqConverter<object>),
        (typeof(LanguageExt.HashSet<>), HashSetConverter<object>),
        (typeof(HashMap<,>), HashMapConverter<object, object>),
        (typeof(Map<,>), MapConverter<object, object>));

    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsGenericType && Converters.ContainsKey(typeToConvert.GetGenericTypeDefinition());

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
        (JsonConverter)Converters[typeToConvert.GetGenericTypeDefinition()].Method.GetGenericMethodDefinition().MakeGenericMethod(typeToConvert.GetGenericArguments()).Invoke(obj: null, parameters: null)!;

    private static OptionJsonConverter<T> OptionConverter<T>() => new();

    private static SeqJsonConverter<T> SeqConverter<T>() => new();

    private static HashSetJsonConverter<T> HashSetConverter<T>() => new();

    private static HashMapJsonConverter<TKey, TValue> HashMapConverter<TKey, TValue>() where TKey : notnull => new();

    private static MapJsonConverter<TKey, TValue> MapConverter<TKey, TValue>() where TKey : notnull => new();

    internal static IEnumerable<T> Ordered<T, TKey>(IEnumerable<T> items, Func<T, TKey> key) =>
        typeof(IComparable<TKey>).IsAssignableFrom(typeof(TKey)) ? items.OrderBy(key) : items;

    internal static T Present<T>(T? value) where T : class => value ?? throw new JsonException($"null in place of {typeof(T).Name}");
}
