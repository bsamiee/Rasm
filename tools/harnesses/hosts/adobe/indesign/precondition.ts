import { Schema, SchemaGetter, Struct } from 'effect';
import { Application, additions, enumerators, many, member, merged, Open, placings } from '#/cases/precondition';
import { Expression } from '#/cases/schema';
import { JsonObject } from '#/cases/values';
import { Added, Placed, type Survey } from '#/hosts/adobe/indesign/contract';

// --- [OPERATIONS] ----------------------------------------------------------------------

const precondition = (survey: typeof Survey.Type) => {
    const checked = enumerators(survey.enumerations);
    const Properties = JsonObject.check(checked());
    const unit = member('MeasurementUnits', survey.enumerations.MeasurementUnits ?? []);
    const Units = Schema.Union([unit.pipe(Schema.decodeTo(Placed.fields.units, { decode: SchemaGetter.transform((held) => ({ script: held, horizontal: held, vertical: held })), encode: SchemaGetter.forbiddenEncoding })), Schema.Struct({ script: unit, horizontal: unit, vertical: unit })]).annotate({
        description: 'MeasurementUnits member names for script reads and the rulers, one name setting all three',
    });
    const Document = Schema.Struct({
        intent: member('DocumentIntentOptions', survey.enumerations.DocumentIntentOptions ?? []),
        width: Schema.Finite.annotate({ description: 'Page width in the script unit' }),
        height: Schema.Finite.annotate({ description: 'Page height in the script unit' }),
        profiles: Schema.Struct({ cmyk: Schema.Literals(survey.cmyk), rgb: Schema.Literals(survey.rgb) }).annotate({ description: 'Working space profile names the host lists' }),
        language: Schema.Literals(survey.languages).annotate({ description: 'Language name the host lists, applied to the text defaults' }),
        properties: Schema.optionalKey(Properties.annotate({ description: 'Further documents.add properties, {"$": "enum:<Enumeration>.<MEMBER>"} naming an enumerator' })),
    });
    const rows = {
        ...Struct.omit(Placed.fields, ['documents']),
        units: Units,
        set: Schema.optionalKey(Schema.Array(Schema.Tuple([Expression, Schema.Json.check(checked())]))),
        add: additions(Added.mapFields(Struct.assign({ properties: Schema.optionalKey(Properties) }))),
    };
    const documented = (row: typeof Document.Type, units: typeof Placed.Type.units) => ({
        properties: merged(row.properties ?? {}, {
            documentPreferences: { pageWidth: row.width, pageHeight: row.height, intent: row.intent },
            viewPreferences: { horizontalMeasurementUnits: units.horizontal, verticalMeasurementUnits: units.vertical },
            cmykProfile: row.profiles.cmyk,
            rgbProfile: row.profiles.rgb,
            textDefaults: { appliedLanguage: row.language },
        }),
    });
    return placings(
        Placed,
        { document: Schema.Struct({ document: many(Document), ...rows }), open: Schema.Struct({ open: many(Open), ...rows }), application: Schema.Struct({ application: Application, ...rows }) },
        {
            document: ({ document, ...rest }) => ({ ...rest, documents: document.map((row) => documented(row, rest.units)) }),
            open: ({ open, ...rest }) => ({ ...rest, documents: open.map((file) => ({ open: file })) }),
            application: ({ application: _, ...rest }) => ({ ...rest, documents: [] }),
        },
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { precondition };
