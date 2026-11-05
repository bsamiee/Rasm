import { Array, Effect, Match, Option, pipe, Record, Schema, SchemaGetter, Struct } from 'effect';
import { additions, Enumerated, enumerators, member, Open, placings } from '#/cases/precondition';
import { Expression } from '#/cases/schema';
import { JsonObject } from '#/cases/values';
import { Added, type Contract, Placed } from '#/hosts/adobe/illustrator/contract';

// --- [TYPES] ---------------------------------------------------------------------------

type Kind = Schema.Top & Schema.Decoder<Schema.Json>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const WRITES = { resolution: 'doc.rasterEffectSettings.resolution', language: 'doc.characterStyles[0].characterAttributes.language' } as const;
const NAMED = ['typename', 'title'];

// --- [MODELS] --------------------------------------------------------------------------

const Parsed = Schema.Struct({ $: Enumerated.parts });
const Finites = Schema.Array(Schema.Finite);

// --- [OPERATIONS] ----------------------------------------------------------------------

const renamed = <A>(from: Schema.Codec<A, A>, to: Schema.Json) => from.pipe(Schema.decodeTo(Schema.toType(Schema.Json), { decode: SchemaGetter.transform(() => to), encode: SchemaGetter.forbiddenEncoding }));
const kind = (value: Schema.Json, listed: (enumeration: string) => Kind, open: Kind): Kind =>
    Option.match(Schema.decodeUnknownOption(Parsed)(value), {
        onSome: ({ $: [, enumeration] }) => listed(enumeration),
        onNone: () =>
            Match.value(value).pipe(
                Match.when(Schema.is(Finites), () => Finites),
                Match.when(Schema.is(Schema.Finite), () => Schema.Finite),
                Match.when(Schema.is(Schema.Boolean), () => Schema.Boolean),
                Match.when(Schema.is(Schema.String), () => Schema.String),
                Match.orElse(() => open),
            ),
    });
const precondition = (survey: typeof Contract.Survey.Type) => {
    const quiet = 'DONTDISPLAYALERTS';
    const checked = enumerators(survey.enumerations);
    const Properties = JsonObject.check(checked());
    const listed = (enumeration: string) => member(enumeration, survey.enumerations[enumeration] ?? []);
    const derived = (settings: typeof JsonObject.Type) =>
        Record.map(
            Record.filter(settings, (_, key) => !NAMED.includes(key)),
            (value) => Schema.optionalKey(kind(value, listed, Schema.Json.check(checked()))),
        );
    const settingsOf = pipe(
        Object.values(survey.presets),
        Array.head,
        Option.map(({ settings }) => settings),
        Option.getOrElse(() => ({})),
    );
    const intents = [...Record.collect(survey.presets, (name) => renamed(Schema.Literal(name), name)), ...Record.collect(survey.presets, (name, { types }) => types.map((type) => renamed(Schema.Literal(type), name))).flat()];
    const Document = Schema.Struct({
        intent: Schema.Union(intents).annotate({ description: 'app.startupPresetsList entry, or a DocumentPresetType member the installation resolves to one' }),
        ...derived(settingsOf),
        resolution: Schema.optionalKey(Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Raster effects resolution in pixels per inch' })),
        language: Schema.optionalKey(listed('LanguageType').annotate({ description: 'LanguageType of the normal character style' })),
        profile: Schema.optionalKey(Schema.String.annotate({ description: 'colorProfileName the preset and color settings must give the document' })),
    });
    const Opening = Schema.Struct({ file: Open, colorSpace: Schema.optionalKey(listed('DocumentColorSpace')), options: Schema.optionalKey(Schema.Struct(derived(survey.options))) });
    const rows = {
        ...Struct.omit(Placed.fields, ['documents']),
        coordinates: listed('CoordinateSystem'),
        interaction: listed('UserInteractionLevel')
            .annotate({ description: `UserInteractionLevel the case runs under, ${quiet} when absent` })
            .pipe(Schema.withDecodingDefaultKey(Effect.succeed(quiet))),
        colorSettings: Schema.optionalKey(Schema.Union(survey.application.colorSettingsList.map((file) => renamed(Schema.Literal(decodeURI(file).replace(/^.*\/|\.csf$/gu, '')), file))).annotate({ description: 'Color settings file name app.colorSettingsList holds' })),
        sends: Schema.optionalKey(Schema.Array(Schema.Literals(survey.methods)).annotate({ description: 'Application methods app.reflect lists, sent to the host in place of recording them' })),
        plugins: Schema.optionalKey(Schema.Array(Schema.Literals(survey.host.plugins)).annotate({ description: 'Plug-ins the instance must map from outside its application bundle' })),
        set: Schema.optionalKey(Schema.Array(Schema.Tuple([Expression, Schema.Json.check(checked())]))),
        add: additions(Added.mapFields(Struct.assign({ properties: Schema.optionalKey(Properties) }))),
    };
    const created = ({ intent, resolution, language, profile, ...given }: typeof Document.Type) => {
        const reads: Record.ReadonlyRecord<string, string> = { width: 'doc.width', height: 'doc.height', units: 'doc.rulerUnits', colorMode: 'doc.documentColorSpace', numArtboards: 'doc.artboards.length' };
        const posted: { readonly [K in keyof typeof WRITES]: Schema.Json | undefined } = { resolution, language };
        const writes = Array.getSomes(Struct.keys(WRITES).map((key) => Option.map(Option.fromUndefinedOr(posted[key]), (value) => [WRITES[key], value] as const)));
        const expects = [...Array.getSomes(Record.collect(given, (key, value) => Option.map(Record.get(reads, key), (read) => [read, value] as const))), ...writes, ...Option.toArray(Option.map(Option.fromUndefinedOr(profile), (value) => ['doc.colorProfileName', value] as const))];
        return { preset: String(intent), settings: given, writes, expects };
    };
    return placings(rows, { document: Document, open: Schema.Union([Open, Opening]) })(Placed, ({ documents, plugins: _, colorSettings, ...rest }) => ({
        ...rest,
        ...(colorSettings === undefined ? {} : { colorSettings: String(colorSettings) }),
        documents: documents.map((row) =>
            Match.value(row).pipe(
                Match.when(Match.string, (open) => ({ open, options: {} })),
                Match.when({ file: Match.string }, ({ file, colorSpace, options = {} }) => ({ open: file, ...(colorSpace === undefined ? {} : { colorSpace }), options: Record.getSomes(Record.map(options, Option.fromUndefinedOr)) })),
                Match.orElse(created),
            ),
        ),
    }));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { precondition };
