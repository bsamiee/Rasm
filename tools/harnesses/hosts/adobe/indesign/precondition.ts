import { Schema, SchemaGetter, Struct } from 'effect';
import { additions, enumerators, member, merged, Open, placings } from '#/cases/precondition';
import { Expression } from '#/cases/schema';
import { JsonObject } from '#/cases/values';
import { Added, type Contract, Placed } from '#/hosts/adobe/indesign/contract';

// --- [OPERATIONS] ----------------------------------------------------------------------

const precondition = (survey: typeof Contract.Survey.Type) => {
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
    return placings(rows, { document: Document, open: Open })(Placed, ({ documents, ...rest }) => ({
        ...rest,
        documents: documents.map((row) =>
            typeof row === 'string'
                ? { open: row }
                : {
                      properties: merged(row.properties ?? {}, {
                          documentPreferences: { pageWidth: row.width, pageHeight: row.height, intent: row.intent },
                          viewPreferences: { horizontalMeasurementUnits: rest.units.horizontal, verticalMeasurementUnits: rest.units.vertical },
                          cmykProfile: row.profiles.cmyk,
                          rgbProfile: row.profiles.rgb,
                          textDefaults: { appliedLanguage: row.language },
                      }),
                  },
        ),
    }));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { precondition };
