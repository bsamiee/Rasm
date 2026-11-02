import { Array, Match, Option, pipe, Record, Schema, SchemaGetter, Struct } from 'effect';
import { Expression } from '#/cases';
import { Placed, type Survey } from '#/illustrator/contract';
import { Application, enumerators, many, member, placings } from '#/precondition';
import { Enumerated } from '#/results';
import { JsonObject } from '#/values';

// --- [CONSTANTS] -----------------------------------------------------------------------

const QUIET = 'DONTDISPLAYALERTS';
const WRITES = { resolution: 'doc.rasterEffectSettings.resolution', language: 'doc.characterStyles[0].characterAttributes.language' } as const;
const NAMED = ['typename', 'title'];

// --- [OPERATIONS] ----------------------------------------------------------------------

const renamed = <A>(from: Schema.Codec<A, A>, to: Schema.Json) => from.pipe(Schema.decodeTo(Schema.toType(Schema.Json), { decode: SchemaGetter.transform(() => to), encode: SchemaGetter.forbiddenEncoding }));
const precondition = (survey: typeof Survey.Type) => {
    const checked = enumerators(survey.enumerations);
    const Properties = JsonObject.check(checked());
    const listed = (enumeration: string) => member(enumeration, survey.enumerations[enumeration] ?? []);
    const kind = (value: Schema.Json): Schema.Top & Schema.Decoder<Schema.Json> =>
        Match.value(value).pipe(
            Match.when(Schema.is(Enumerated), ({ $ }) => listed($.slice('enum:'.length, $.indexOf('.')))),
            Match.when(Schema.is(Schema.Array(Schema.Finite)), () => Schema.Array(Schema.Finite)),
            Match.when(Schema.is(Schema.Finite), () => Schema.Finite),
            Match.when(Schema.is(Schema.Boolean), () => Schema.Boolean),
            Match.when(Schema.is(Schema.String), () => Schema.String),
            Match.orElse(() => Schema.Json.check(checked())),
        );
    const derived = (settings: typeof JsonObject.Type) =>
        Record.map(
            Record.filter(settings, (_, key) => !NAMED.includes(key)),
            (value) => Schema.optionalKey(kind(value)),
        );
    const settingsOf = pipe(
        Object.values(survey.presets),
        Array.head,
        Option.map(({ settings }) => settings),
        Option.getOrElse(() => ({})),
    );
    const intents = [...Record.collect(survey.presets, (name) => renamed(Schema.Literal(name), name)), ...Record.collect(survey.presets, (name, { types }) => types.map((type) => renamed(Schema.Literal(type), name))).flat()];
    const colors = Option.getOrElse(Schema.decodeUnknownOption(Schema.Array(Schema.String))(survey.application.colorSettingsList), () => []);
    const Document = Schema.Struct({
        intent: Schema.Union(intents).annotate({ description: 'app.startupPresetsList entry, or a DocumentPresetType member the installation resolves to one' }),
        ...derived(settingsOf),
        resolution: Schema.optionalKey(Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Raster effects resolution in pixels per inch' })),
        language: Schema.optionalKey(listed('LanguageType').annotate({ description: 'LanguageType of the normal character style' })),
        profile: Schema.optionalKey(Schema.String.annotate({ description: 'colorProfileName the preset and color settings must give the document' })),
    });
    const Open = Schema.Struct({ file: Schema.NonEmptyString.annotate({ description: 'Document path under the product folder, its copy opened with a window' }), colorSpace: Schema.optionalKey(listed('DocumentColorSpace')), options: Schema.optionalKey(Schema.Struct(derived(survey.options))) });
    const rows = {
        ...Struct.omit(Placed.fields, ['documents']),
        coordinates: listed('CoordinateSystem'),
        interaction: Schema.optionalKey(listed('UserInteractionLevel').annotate({ description: `UserInteractionLevel the case runs under, ${QUIET} when absent` })),
        colorSettings: Schema.optionalKey(Schema.Union(colors.map((file) => renamed(Schema.Literal(decodeURI(file).replace(/^.*\/|\.csf$/gu, '')), file))).annotate({ description: 'Color settings file name app.colorSettingsList holds' })),
        sends: Schema.optionalKey(Schema.Array(Schema.Literals(survey.methods)).annotate({ description: 'Application methods app.reflect lists, sent to the host in place of recording them' })),
        plugins: Schema.optionalKey(Schema.Array(Schema.Literals(survey.host.plugins)).annotate({ description: 'Plug-ins the instance must map from outside its application bundle' })),
        set: Schema.optionalKey(Schema.Array(Schema.Tuple([Expression, Schema.Json.check(checked())]))),
        add: Schema.optionalKey(Schema.Array(Placed.fields.add.schema.value.mapFields((fields) => ({ ...fields, properties: Schema.optionalKey(Properties) })))),
    };
    const kept = <R extends { readonly plugins?: unknown; readonly interaction?: typeof Enumerated.Type; readonly colorSettings?: Schema.Json }>({ plugins: _, interaction, colorSettings, ...held }: R) => ({
        ...held,
        interaction: interaction ?? { $: `enum:UserInteractionLevel.${QUIET}` as const },
        ...(colorSettings === undefined ? {} : { colorSettings: String(colorSettings) }),
    });
    const created = ({ intent, resolution, language, profile, ...given }: typeof Document.Type) => {
        const reads: Record.ReadonlyRecord<string, string> = { width: 'doc.width', height: 'doc.height', units: 'doc.rulerUnits', colorMode: 'doc.documentColorSpace', numArtboards: 'doc.artboards.length' };
        const posted: { readonly [K in keyof typeof WRITES]: Schema.Json | undefined } = { resolution, language };
        const writes = Struct.keys(WRITES).flatMap((key) => Option.toArray(Option.map(Option.fromUndefinedOr(posted[key]), (value) => [WRITES[key], value] as const)));
        const expects = [...Array.getSomes(Record.collect(given, (key, value) => Option.map(Record.get(reads, key), (read) => [read, value] as const))), ...writes, ...Option.toArray(Option.map(Option.fromUndefinedOr(profile), (value) => ['doc.colorProfileName', value] as const))];
        return { preset: String(intent), settings: given, writes, expects };
    };
    return placings(
        Placed,
        { document: Schema.Struct({ document: many(Document), ...rows }), open: Schema.Struct({ open: many(Schema.Union([Schema.NonEmptyString, Open])), ...rows }), application: Schema.Struct({ application: Application, ...rows }) },
        {
            document: ({ document, ...rest }) => ({ ...kept(rest), documents: document.map(created) }),
            open: ({ open, ...rest }) => ({
                ...kept(rest),
                documents: open.map((row) => (typeof row === 'string' ? { open: row, options: {} } : { open: row.file, ...(row.colorSpace === undefined ? {} : { colorSpace: row.colorSpace }), options: Record.getSomes(Record.map(row.options ?? {}, Option.fromUndefinedOr)) })),
            }),
            application: ({ application: _, ...rest }) => ({ ...kept(rest), documents: [] }),
        },
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { precondition };
