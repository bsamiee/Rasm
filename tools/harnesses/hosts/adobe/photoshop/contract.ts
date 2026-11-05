import { Schema, Struct } from 'effect';
import { contract, hostCase } from '#/cases/contract';
import { Assignments, added, additions, Copy, documents, Enumerated, Steps } from '#/cases/precondition';
import { Outcome, recorded } from '#/cases/results';
import { answers, EXTENDSCRIPT, Expression } from '#/cases/schema';
import { JsonObject } from '#/cases/values';

// --- [MODELS] --------------------------------------------------------------------------

const Created = Schema.Struct({
    width: Schema.Finite.annotate({ description: 'Width in the case ruler units' }),
    height: Schema.Finite.annotate({ description: 'Height in the case ruler units' }),
    resolution: Schema.Finite.annotate({ description: 'Pixels per inch, the print or web intent' }),
    mode: Enumerated.annotate({ description: 'NewDocumentMode member' }),
    initialFill: Enumerated.annotate({ description: 'DocumentFill member' }),
    pixelAspectRatio: Schema.Finite,
    bitsPerChannel: Enumerated.annotate({ description: 'BitsPerChannelType member' }),
    colorProfileName: Schema.NonEmptyString.annotate({ description: 'Color profile the document embeds' }),
}).annotate({ description: 'app.documents.add arguments, each one the document states read back after the add, a mismatch failing the case' });
const Document = Schema.TaggedUnion({
    Made: { arguments: Created },
    Opened: { open: Copy },
});
const Placed = Schema.Struct({
    documents: documents(Document),
    units: Schema.Struct({ ruler: Enumerated, type: Enumerated }).annotate({ description: 'app.preferences rulerUnits and typeUnits, restored after the case' }),
    colorSettings: Schema.optionalKey(Schema.String.annotate({ description: 'app.colorSettings the case runs under, restored after it' })),
    features: Schema.optionalKey(Schema.Record(Schema.String, Schema.Boolean).annotate({ description: 'app.featureEnabled answer each flag must give, the case failing on another' })),
    fonts: Schema.optionalKey(Schema.Array(Schema.NonEmptyString).annotate({ description: 'PostScript names app.fonts must hold, the case failing on a missing one' })),
    add: additions(added('artLayers', {})),
    set: Assignments,
    steps: Steps,
    select: Schema.optionalKey(
        Schema.Struct({
            layers: Schema.optionalKey(Schema.Array(Expression).annotate({ description: 'Layers one select event targets by identifier in the first document' })),
            region: Schema.optionalKey(Schema.Array(Schema.Tuple([Schema.Finite, Schema.Finite])).annotate({ description: 'Polygon points in ruler units doc.selection.select selects' })),
        }),
    ),
}).check(Schema.makeFilter((placed) => placed.documents.length > 0 || (placed.add === undefined && placed.select === undefined) || 'add and select need a case document'));
const Contract = contract({
    engines: { extendscript: { extensions: EXTENDSCRIPT } },
    Case: hostCase(Placed, {
        folder: Schema.String.annotate({ description: 'Case folder holding document copies and every file the case writes, outside/<path> for a write the original aimed elsewhere' }),
        ...Struct.pick(answers, ['windows', 'confirms', 'prompts', 'files']),
    }),
    outcome: {
        ...Outcome.common,
        writes: recorded(
            Schema.Struct({
                target: Schema.optionalKey(Schema.String.annotate({ description: 'object:/document[@id=N] with /layer[@id=M], /<part>[n], or /json under it, or customOptions, the application when absent' })),
                member: Schema.String,
                before: Schema.optionalKey(Schema.Json),
                after: Schema.Json,
            }).annotate({ description: 'Descriptor key, custom options key, or held application member the original changed, Action JSON' }),
        ),
        history: recorded(Schema.String.annotate({ description: 'History state a case document added during the run, newest first' })),
        opened: recorded(
            Schema.Struct({
                document: Schema.Json,
                layers: Schema.Array(Schema.Json),
                parts: Schema.Record(Schema.String, Schema.Json).annotate({ description: 'Guide, channel, and path descriptors by <part>[n], and the json tree with path data' }),
            }).annotate({ description: 'Case document state the original started from, Action JSON' }),
        ),
        errors: recorded(
            Schema.Struct({
                version: Schema.String,
                id: Schema.String,
                code: Schema.String,
                message: Schema.String,
                frames: Schema.Array(Schema.String.annotate({ description: '<module> +0x<offset>' })),
            }).annotate({ description: 'PSErrorLog.txt entry the instance wrote while the case ran' }),
        ),
    },
    survey: {
        question: 'Unit enumerations, locale, feature access level, and color settings the installation runs under',
        host: Schema.Struct({
            locale: Schema.String.annotate({ description: '$.locale' }),
            featureAccessLevel: Schema.String.annotate({ description: 'Application descriptor featureAccessLevel deciding which prerelease flags exist' }),
            colorSettings: Schema.String.annotate({ description: 'app.colorSettings deciding the working space a created or opened document takes' }),
        }),
        fields: { units: Schema.Struct({ ruler: Schema.String, type: Schema.String }).annotate({ description: 'Enumeration of the app.preferences rulerUnits and typeUnits values' }) },
    },
    reply: {
        state: Schema.Struct({
            dialogs: Schema.String.annotate({ description: 'app.displayDialogs the batch restores' }),
            shifting: Schema.Array(Schema.String).annotate({ description: 'Application descriptor keys a label document opened and closed at batch start moves, left out of writes and leaked' }),
            saving: Schema.Array(Schema.String).annotate({ description: 'Document descriptor keys saving the label document moves, left out of document writes' }),
            unlisted: Schema.Array(Schema.String).annotate({ description: 'Application member the engine shadows that app.reflect declares no method for' }),
            defaults: JsonObject.annotate({ description: 'Application descriptor read whole at batch start, Action JSON' }),
        }),
    },
    batch: { label: Schema.String.annotate({ description: 'Name prefix of every created case document' }) },
});
const Quit = Schema.fromJsonString(Schema.Struct({ quit: Schema.Literal(true) })).annotate({ description: 'Batch end: the engine closes every document without saving and sends the quit event with DialogModes.NO' });

// --- [EXPORTS] -------------------------------------------------------------------------

export { Contract, Created, Document, Placed, Quit };
