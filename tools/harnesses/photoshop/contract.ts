import { Schema, Struct } from 'effect';
import { answers, Expression, Name, Reads } from '#/cases';
import { outcome, recorded, results } from '#/results';
import { JsonObject } from '#/values';

// --- [MODELS] --------------------------------------------------------------------------

const Engine = Schema.Literal('extendscript').annotate({ description: 'do javascript, the one dictionary command that runs code' });
const Enumerated = Schema.Struct({ $: Schema.TemplateLiteral(['enum:', Schema.String, '.', Schema.String]) }).annotate({ description: 'Enumerator the engine reads through $.global, a missing one failing the case' });
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
    Opened: { open: Schema.NonEmptyString.annotate({ description: 'Absolute path of the case copy the engine opens' }) },
});
const Placed = Schema.Struct({
    documents: Schema.Array(Document).annotate({ description: 'Case documents opened with a window in order, the first one active, none for an application case' }),
    units: Schema.Struct({ ruler: Enumerated, type: Enumerated }).annotate({ description: 'app.preferences rulerUnits and typeUnits, restored after the case since both persist across a relaunch' }),
    colorSettings: Schema.optionalKey(Schema.String.annotate({ description: 'app.colorSettings the case runs under, restored after it' })),
    features: Schema.optionalKey(Schema.Record(Schema.String, Schema.Boolean).annotate({ description: 'app.featureEnabled answer each flag must give, the case failing on another' })),
    fonts: Schema.optionalKey(Schema.Array(Schema.NonEmptyString).annotate({ description: 'PostScript names app.fonts must hold, the case failing on a missing one' })),
    add: Schema.optionalKey(Schema.Array(Schema.Struct({ as: Schema.NonEmptyString, to: Expression, kind: Schema.NonEmptyString.annotate({ description: 'Collection member of the target, as artLayers' }), properties: Schema.optionalKey(JsonObject) })).annotate({ description: 'Items added and named as made.<as>' })),
    set: Schema.optionalKey(Schema.Array(Schema.Tuple([Expression, Schema.Json])).annotate({ description: 'Member path and value written in order, [*] spreading over a collection' })),
    steps: Schema.optionalKey(Schema.Array(Schema.NonEmptyString).annotate({ description: 'Statements run in order after set and add' })),
    select: Schema.optionalKey(
        Schema.Struct({
            layers: Schema.optionalKey(Schema.Array(Expression).annotate({ description: 'Layers one select event targets by identifier in the first document' })),
            region: Schema.optionalKey(Schema.Array(Schema.Tuple([Schema.Finite, Schema.Finite])).annotate({ description: 'Polygon points in ruler units doc.selection.select selects' })),
        }),
    ),
}).check(Schema.makeFilter((placed) => placed.documents.length > 0 || (placed.add === undefined && placed.select === undefined) || 'add and select need a case document'));
const shared = { name: Name, precondition: Placed, read: Reads, folder: Schema.String.annotate({ description: 'Case folder holding document copies and every file the case writes, outside/<path> for a write the original aimed elsewhere' }), ...Struct.pick(answers, ['windows', 'confirms', 'prompts', 'files']) };
const Source = Schema.Struct({ script: Schema.String.annotate({ description: 'Original script path, $.fileName in the case scope and its folder current' }), code: Schema.String.annotate({ description: 'Script text with includes expanded and kept lines alone, evaluated in the case scope' }) });
const HostCase = Schema.TaggedUnion({
    Run: { ...shared, source: Source },
    Call: { ...shared, source: Schema.optionalKey(Source), call: Expression, args: Schema.Array(Schema.Json) },
    Probe: { ...shared, evaluate: Expression },
});
const Request = Schema.Struct({
    records: Schema.String.annotate({ description: 'Folder the engine writes <index>.started into before each case and <index>.json after it' }),
    label: Schema.String.annotate({ description: 'Name prefix of every created case document' }),
    survey: Schema.optionalKey(Schema.Boolean),
    cases: Schema.Array(HostCase),
});
const Survey = Schema.Struct({
    units: Schema.Struct({ ruler: Schema.String, type: Schema.String }).annotate({ description: 'Enumeration of the app.preferences rulerUnits and typeUnits values' }),
    host: Schema.Struct({
        locale: Schema.String.annotate({ description: '$.locale' }),
        featureAccessLevel: Schema.String.annotate({ description: 'Application descriptor featureAccessLevel deciding which prerelease flags exist' }),
        colorSettings: Schema.String.annotate({ description: 'app.colorSettings deciding the working space a created or opened document takes' }),
    }),
}).annotate({ description: 'Unit enumerations, locale, feature access level, and color settings the installation runs under' });
const State = Schema.Struct({
    dialogs: Schema.String.annotate({ description: 'app.displayDialogs the batch restores' }),
    shifting: Schema.Array(Schema.String).annotate({ description: 'Application descriptor keys a label document opened and closed at batch start moves, left out of writes and leaked' }),
    saving: Schema.Array(Schema.String).annotate({ description: 'Document descriptor keys saving the label document moves, left out of document writes' }),
    unlisted: Schema.Array(Schema.String).annotate({ description: 'Application member the engine shadows that app.reflect declares no method for' }),
    defaults: JsonObject.annotate({ description: 'Application descriptor read whole at batch start, Action JSON' }),
}).annotate({ description: 'Instance state at batch start' });
const Answer = Schema.TaggedUnion({
    Answered: { application: Schema.String, version: Schema.String, state: State, survey: Schema.optionalKey(Survey) },
    Failed: { reason: Schema.String.annotate({ description: 'Batch failure before or around the cases' }) },
});
const Reply = Schema.fromJsonString(Answer).annotate({ description: 'Engine answer to one batch, case outcomes sitting in the records folder' });
const fields = {
    alerts: recorded(Schema.Struct({ text: Schema.String, title: Schema.optionalKey(Schema.String) })),
    confirms: recorded(Schema.String),
    prompts: recorded(Schema.String),
    windows: recorded(Schema.Json.annotate({ description: 'ScriptUI window tree shown and filled' })),
    files: recorded(Schema.String.annotate({ description: 'Picker kind each file or folder picker the original opened' })),
    activations: recorded(Schema.String.annotate({ description: 'Member taking focus, reaching another app or the system, opening a windowless document, or raising a dialog, recorded and never sent' })),
    console: recorded(Schema.String.annotate({ description: 'Text the original wrote through $.write and $.writeln' })),
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
    written: recorded(Schema.Struct({ file: Schema.String, size: Schema.Int, sha256: Schema.String.annotate({ description: 'Content hash with XMP and EXIF metadata exiftool writes stripped' }) }).annotate({ description: 'File the case wrote into its folder' })),
};
const Outcome = outcome(fields);
const Recorded = Schema.fromJsonString(Outcome);
const Results = results(Engine, { state: Schema.optionalKey(State), host: Survey.fields.host }, fields);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Answer, Created, Document, Engine, Enumerated, HostCase, Outcome, Placed, Recorded, Reply, Request, Results, Survey };
