import { Array, Match, Option, Record, Schema, SchemaGetter, Struct } from 'effect';
import { Expression } from '#/cases';
import { Enumerated, Placed, type Survey } from '#/illustrator/contract';
import { JsonObject } from '#/values';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ENUMERATOR = /^enum:(?<enumeration>[^.]+)\.(?<name>.+)$/u;
const QUIET = 'DONTDISPLAYALERTS';
const READS: Record.ReadonlyRecord<string, string> = { width: 'doc.width', height: 'doc.height', units: 'doc.rulerUnits', colorMode: 'doc.documentColorSpace', numArtboards: 'doc.artboards.length' };
const WRITES = { resolution: 'doc.rasterEffectSettings.resolution', language: 'doc.characterStyles[0].characterAttributes.language' } as const;
const NAMED = ['typename', 'title'];

// --- [OPERATIONS] ----------------------------------------------------------------------

const many = <S extends Schema.Top>(item: S) => Schema.Union([Schema.NonEmptyArray(item), item.pipe(Schema.decodeTo(Schema.NonEmptyArray(Schema.toType(item)), { decode: SchemaGetter.transform((one) => [one] as const), encode: SchemaGetter.forbiddenEncoding }))]);
const placing = <S extends Schema.Top & Schema.Decoder<unknown>>(row: S, decode: (held: S['Type']) => typeof Placed.Type) => row.pipe(Schema.decodeTo(Schema.toType(Placed), { decode: SchemaGetter.transform(decode), encode: SchemaGetter.forbiddenEncoding }));
const renamed = <A>(from: Schema.Codec<A, A>, to: Schema.Json) => from.pipe(Schema.decodeTo(Schema.toType(Schema.Json), { decode: SchemaGetter.transform(() => to), encode: SchemaGetter.forbiddenEncoding }));
const tags = (value: Schema.Json): readonly string[] =>
    Match.value(value).pipe(
        Match.when(Schema.is(Schema.Array(Schema.Json)), (items) => items.flatMap(tags)),
        Match.when(Schema.is(Schema.Struct({ $: Schema.String })), ({ $ }) => [$]),
        Match.when(Schema.is(JsonObject), (held) => Object.values(held).flatMap(tags)),
        Match.orElse(() => []),
    );
const precondition = (survey: typeof Survey.Type) => {
    const declared = (tag: string) => Option.exists(Option.fromNullishOr(ENUMERATOR.exec(tag)?.groups), ({ enumeration = '', name = '' }) => Option.exists(Record.get(survey.enumerations, enumeration), (listed) => listed.includes(name)));
    const enumerators = <T extends Schema.Json>() =>
        Schema.makeFilter(
            (value: T) =>
                tags(value)
                    .filter((tag) => tag.startsWith('enum:') && !declared(tag))
                    .map((tag) => `${tag} names no enumerator the host declares, enumerators read enum:<Enumeration>.<MEMBER>`)[0] ?? true,
        );
    const Properties = JsonObject.check(enumerators());
    const member = (enumeration: string) =>
        Schema.Literals(survey.enumerations[enumeration] ?? [])
            .pipe(Schema.decodeTo(Enumerated, { decode: SchemaGetter.transform((name) => ({ $: `enum:${enumeration}.${name}` as const })), encode: SchemaGetter.forbiddenEncoding }))
            .annotate({ description: `${enumeration} member the host declares` });
    const kind = (value: Schema.Json): Schema.Top & Schema.Decoder<Schema.Json> =>
        Match.value(value).pipe(
            Match.when(Schema.is(Enumerated), ({ $ }) => member($.slice('enum:'.length, $.indexOf('.')))),
            Match.when(Schema.is(Schema.Array(Schema.Finite)), () => Schema.Array(Schema.Finite)),
            Match.when(Schema.is(Schema.Finite), () => Schema.Finite),
            Match.when(Schema.is(Schema.Boolean), () => Schema.Boolean),
            Match.when(Schema.is(Schema.String), () => Schema.String),
            Match.orElse(() => Schema.Json.check(enumerators())),
        );
    const derived = (settings: typeof JsonObject.Type) =>
        Record.map(
            Record.filter(settings, (_, key) => !NAMED.includes(key)),
            (value) => Schema.optionalKey(kind(value)),
        );
    const settingsOf = Option.getOrElse(
        Option.map(Array.head(Record.values(survey.presets)), ({ settings }) => settings),
        () => ({}),
    );
    const intents = [...Record.collect(survey.presets, (name) => renamed(Schema.Literal(name), name)), ...Record.collect(survey.presets, (name, { types }) => types.map((type) => renamed(Schema.Literal(type), name))).flat()];
    const colors = Option.getOrElse(Schema.decodeUnknownOption(Schema.Array(Schema.String))(survey.application.colorSettingsList), () => []);
    const Document = Schema.Struct({
        intent: Schema.Union(intents).annotate({ description: 'app.startupPresetsList entry, or a DocumentPresetType member the installation resolves to one' }),
        ...derived(settingsOf),
        resolution: Schema.optionalKey(Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Raster effects resolution in pixels per inch' })),
        language: Schema.optionalKey(member('LanguageType').annotate({ description: 'LanguageType of the normal character style' })),
        profile: Schema.optionalKey(Schema.String.annotate({ description: 'colorProfileName the preset and color settings must give the document' })),
    });
    const Open = Schema.Struct({ file: Schema.NonEmptyString.annotate({ description: 'Document path under the product folder, its copy opened with a window' }), colorSpace: Schema.optionalKey(member('DocumentColorSpace')), options: Schema.optionalKey(Schema.Struct(derived(survey.options))) });
    const rows = {
        ...Struct.omit(Placed.fields, ['documents']),
        coordinates: member('CoordinateSystem'),
        interaction: Schema.optionalKey(member('UserInteractionLevel').annotate({ description: `UserInteractionLevel the case runs under, ${QUIET} when absent` })),
        colorSettings: Schema.optionalKey(Schema.Union(colors.map((file) => renamed(Schema.Literal(decodeURI(file).replace(/^.*\/|\.csf$/gu, '')), file))).annotate({ description: 'Color settings file name app.colorSettingsList holds' })),
        sends: Schema.optionalKey(Schema.Array(Schema.Literals(survey.methods)).annotate({ description: 'Application methods app.reflect lists, sent to the host in place of recording them' })),
        plugins: Schema.optionalKey(Schema.Array(Schema.Literals(survey.host.plugins)).annotate({ description: 'Plug-ins the instance must map from outside its application bundle' })),
        set: Schema.optionalKey(Schema.Array(Schema.Tuple([Expression, Schema.Json.check(enumerators())]))),
        add: Schema.optionalKey(Schema.Array(Placed.fields.add.schema.value.mapFields((fields) => ({ ...fields, properties: Schema.optionalKey(Properties) })))),
    };
    const kept = <R extends { readonly plugins?: unknown; readonly interaction?: typeof Enumerated.Type; readonly colorSettings?: Schema.Json }>({ plugins: _, interaction, colorSettings, ...held }: R) => ({
        ...held,
        interaction: interaction ?? { $: `enum:UserInteractionLevel.${QUIET}` as const },
        ...(colorSettings === undefined ? {} : { colorSettings: String(colorSettings) }),
    });
    const created = ({ intent, resolution, language, profile, ...given }: typeof Document.Type) => {
        const posted: { readonly [K in keyof typeof WRITES]: Schema.Json | undefined } = { resolution, language };
        const writes = Struct.keys(WRITES).flatMap((key) => Option.toArray(Option.map(Option.fromUndefinedOr(posted[key]), (value) => [WRITES[key], value] as const)));
        const expects = [...Array.getSomes(Record.collect(given, (key, value) => Option.map(Record.get(READS, key), (read) => [read, value] as const))), ...writes, ...Option.toArray(Option.map(Option.fromUndefinedOr(profile), (value) => ['doc.colorProfileName', value] as const))];
        return { preset: String(intent), settings: given, writes, expects };
    };
    return Schema.Union([
        placing(Schema.Struct({ document: many(Document), ...rows }), ({ document, ...rest }) => ({ ...kept(rest), documents: document.map(created) })),
        placing(Schema.Struct({ open: many(Schema.Union([Schema.NonEmptyString, Open])), ...rows }), ({ open, ...rest }) => ({
            ...kept(rest),
            documents: open.map((row) => (typeof row === 'string' ? { open: row, options: {} } : { open: row.file, ...(row.colorSpace === undefined ? {} : { colorSpace: row.colorSpace }), options: Record.getSomes(Record.map(row.options ?? {}, Option.fromUndefinedOr)) })),
        })),
        placing(Schema.Struct({ application: Schema.Literal(true).annotate({ description: 'Application-level case reading defaults, opening no document' }), ...rows }), ({ application: __, ...rest }) => ({ ...kept(rest), documents: [] })),
    ]);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { precondition };
