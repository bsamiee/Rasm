import { Match, Option, Record, Schema, SchemaGetter } from 'effect';
import { Expression } from '#/cases';
import { Engine, Placed, type Survey } from '#/indesign/contract';
import { JsonObject } from '#/values';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ENUMERATOR = /^enum:(?<enumeration>[^.]+)\.(?<name>.+)$/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const merged = (left: typeof JsonObject.Type, right: typeof JsonObject.Type): typeof JsonObject.Type => Record.union(left, right, (inner, outer) => (Schema.is(JsonObject)(inner) && Schema.is(JsonObject)(outer) ? merged(inner, outer) : outer));
const many = <S extends Schema.Top>(item: S) => Schema.Union([Schema.NonEmptyArray(item), item.pipe(Schema.decodeTo(Schema.NonEmptyArray(Schema.toType(item)), { decode: SchemaGetter.transform((one) => [one] as const), encode: SchemaGetter.forbiddenEncoding }))]);
const placing = <S extends Schema.Top & Schema.Decoder<unknown>>(row: S, decode: (held: S['Type']) => typeof Placed.Type) => row.pipe(Schema.decodeTo(Schema.toType(Placed), { decode: SchemaGetter.transform(decode), encode: SchemaGetter.forbiddenEncoding }));
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
    const member = (enumeration: string) => Schema.Literals(survey.enumerations[enumeration] ?? []).pipe(Schema.decodeTo(Placed.fields.units.fields.script, { decode: SchemaGetter.transform((name) => ({ $: `enum:${enumeration}.${name}` as const })), encode: SchemaGetter.forbiddenEncoding }));
    const Unit = member('MeasurementUnits');
    const Units = Schema.Union([Unit.pipe(Schema.decodeTo(Placed.fields.units, { decode: SchemaGetter.transform((unit) => ({ script: unit, horizontal: unit, vertical: unit })), encode: SchemaGetter.forbiddenEncoding })), Schema.Struct({ script: Unit, horizontal: Unit, vertical: Unit })]).annotate({
        description: 'MeasurementUnits member names for script reads and the rulers, one name setting all three',
    });
    const Document = Schema.Struct({
        intent: member('DocumentIntentOptions').annotate({ description: 'DocumentIntentOptions member the host declares' }),
        width: Schema.Finite.annotate({ description: 'Page width in the script unit' }),
        height: Schema.Finite.annotate({ description: 'Page height in the script unit' }),
        profiles: Schema.Struct({ cmyk: Schema.Literals(survey.cmyk), rgb: Schema.Literals(survey.rgb) }).annotate({ description: 'Working space profile names the host lists' }),
        language: Schema.Literals(survey.languages).annotate({ description: 'Language name the host lists, applied to the text defaults' }),
        properties: Schema.optionalKey(Properties.annotate({ description: 'Further documents.add properties, {"$": "enum:<Enumeration>.<MEMBER>"} naming an enumerator' })),
    });
    const { documents: _, ...rows } = {
        ...Placed.fields,
        units: Units,
        set: Schema.optionalKey(Schema.Array(Schema.Tuple([Expression, Schema.Json.check(enumerators())]))),
        add: Schema.optionalKey(Schema.Array(Placed.fields.add.schema.value.mapFields((fields) => ({ ...fields, properties: Schema.optionalKey(Properties) })))),
        engine: Schema.optionalKey(Engine.annotate({ description: 'Engine a batch of evaluate cases runs in, the script extension deciding otherwise' })),
    };
    const kept = ({ engine: __, ...held }: { readonly engine?: unknown } & Omit<typeof Placed.Type, 'documents'>) => held;
    return Schema.Union([
        placing(Schema.Struct({ document: many(Document), ...rows }), ({ document, ...rest }) => ({
            ...kept(rest),
            documents: document.map((row) => ({
                properties: merged(row.properties ?? {}, {
                    documentPreferences: { pageWidth: row.width, pageHeight: row.height, intent: row.intent },
                    viewPreferences: { horizontalMeasurementUnits: rest.units.horizontal, verticalMeasurementUnits: rest.units.vertical },
                    cmykProfile: row.profiles.cmyk,
                    rgbProfile: row.profiles.rgb,
                    textDefaults: { appliedLanguage: row.language },
                }),
            })),
        })),
        placing(Schema.Struct({ open: many(Schema.NonEmptyString.annotate({ description: 'Document path under the product folder, its copy opened with a window' })), ...rows }), ({ open, ...rest }) => ({ ...kept(rest), documents: open.map((file) => ({ open: file })) })),
        placing(Schema.Struct({ application: Schema.Literal(true).annotate({ description: 'Application-level case reading defaults, opening no document' }), ...rows }), ({ application: __, ...rest }) => ({ ...kept(rest), documents: [] })),
    ]);
};
const engineOf = (condition: typeof JsonObject.Type) => Schema.decodeUnknownOption(Engine)(condition.engine);

// --- [EXPORTS] -------------------------------------------------------------------------

export { engineOf, merged, precondition };
