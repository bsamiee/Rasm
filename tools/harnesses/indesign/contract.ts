import { Schema, Struct } from 'effect';
import { answers, Expression, Name, Reads } from '#/cases';
import { common, Enumerated, hostCase, outcome, record, recorded, reply } from '#/results';
import { JsonObject } from '#/values';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ENGINES = {
    extendscript: { extensions: ['jsx', 'jsxbin', 'js'], language: 'javascript' },
    uxpscript: { extensions: ['idjs'], language: 'uxpscript' },
} as const;

// --- [MODELS] --------------------------------------------------------------------------

const Engine = Schema.Literals(Struct.keys(ENGINES));
const Units = Schema.Struct({ script: Enumerated, horizontal: Enumerated, vertical: Enumerated }).annotate({ description: 'MeasurementUnits enumerators for script reads and the document rulers' });
const Opened = Schema.Union([
    Schema.Struct({ properties: JsonObject.annotate({ description: 'documents.add properties, {"$": "enum:<Enumeration>.<MEMBER>"} naming an enumerator, each read back after the open' }) }),
    Schema.Struct({ open: Schema.NonEmptyString.annotate({ description: 'Absolute path of the case copy the engine opens' }) }),
]);
const Placed = Schema.Struct({
    documents: Schema.Array(Opened).annotate({ description: 'Case documents opened with a window in order, the first one active, none for an application case' }),
    units: Units,
    fonts: Schema.optionalKey(Schema.Array(Schema.NonEmptyString).annotate({ description: 'PostScript names app.fonts must report installed, the case failing on each other status' })),
    idml: Schema.optionalKey(Schema.Boolean.annotate({ description: 'Export each case document as <folder>/<index>.before.idml and .after.idml inside the rolled-back step' })),
    labels: Schema.optionalKey(Schema.Record(Schema.String, Schema.String).annotate({ description: 'insertLabel rows of the first document' })),
    add: Schema.optionalKey(
        Schema.Array(Schema.Struct({ as: Schema.NonEmptyString, to: Expression, kind: Schema.NonEmptyString.annotate({ description: 'Collection member of the target, as rectangles' }), properties: Schema.optionalKey(JsonObject) })).annotate({
            description: 'Items added and named as made.<as>',
        }),
    ),
    select: Schema.optionalKey(Schema.Array(Expression).annotate({ description: 'Items app.select selects in the case window, an insertion point included' })),
    set: Schema.optionalKey(Schema.Array(Schema.Tuple([Expression, Schema.Json])).annotate({ description: 'Member path and value written in order, [*] spreading over a collection' })),
    steps: Schema.optionalKey(Schema.Array(Schema.NonEmptyString).annotate({ description: 'Statements run in order after set and add' })),
    menus: Schema.optionalKey(Schema.Boolean.annotate({ description: 'Capture script menu actions and invoke each onInvoke after the run' })),
});
const HostCase = hostCase({
    name: Name,
    precondition: Placed,
    read: Reads,
    folder: Schema.String.annotate({ description: 'Case folder holding document copies and every file the case writes' }),
    interact: Schema.Boolean.annotate({ description: 'Body runs under INTERACT_WITH_ALL so the host dialogs its presses answer show, every other step under NEVER_INTERACT' }),
    ...Struct.omit(answers, ['presses']),
});
const Batch = Schema.Struct({
    records: Schema.String.annotate({ description: 'Records file the engine appends a Started line to before each case and a Finished line after it' }),
    label: Schema.String.annotate({ description: 'Document label key every case document holds, a document holding it at batch start being a recovered one the engine closes' }),
    enumerations: Schema.Record(Schema.String, Schema.Array(Schema.String)).annotate({ description: 'Surveyed enumeration members the codec qualifies enumerators against' }),
    survey: Schema.Boolean.annotate({ description: 'Reply carries the installation survey' }),
    cases: Schema.Array(HostCase),
});
const Survey = Schema.Struct({
    enumerations: Batch.fields.enumerations,
    cmyk: Schema.Array(Schema.String).annotate({ description: 'CMYK working space profile names' }),
    rgb: Schema.Array(Schema.String).annotate({ description: 'RGB working space profile names' }),
    languages: Schema.Array(Schema.String).annotate({ description: 'Language names with vendors' }),
    host: Schema.Struct({ locale: Schema.String, featureSet: Schema.String, dom: Schema.String.annotate({ description: 'scriptPreferences.version' }) }).annotate({ description: 'Install state deciding answers beside the version' }),
}).annotate({ description: 'Installation facts the index holds per bundle identifier and version' });
const State = Schema.Struct({
    modal: Schema.Boolean.annotate({ description: 'app.modalState, under which case documents neither close nor take recovered ones away' }),
    cmyk: Schema.String,
    rgb: Schema.String,
    defaults: JsonObject.annotate({ description: 'Application preference objects read whole at batch start' }),
    unread: Schema.Array(Schema.String).annotate({ description: 'Application members the snapshot and restore could not cover, each with its error' }),
}).annotate({ description: 'Shared instance state at batch start' });
const Header = Schema.Struct({ application: Schema.String, version: Schema.String, state: State, survey: Schema.optionalKey(Survey) });
const Reply = reply(Header);
const fields = {
    ...common,
    dialogs: recorded(Schema.Json.annotate({ description: 'app.dialogs controls with their prefilled values' })),
    menus: recorded(Schema.Json),
    undo: recorded(Schema.String.annotate({ description: 'Undo step a case document recorded during the run, newest first' })),
};
const Outcome = outcome(fields);
const Recorded = record(Outcome);
const Encoded = Schema.fromJsonString(Batch);

// --- [EXPORTS] -------------------------------------------------------------------------

export { ENGINES, Encoded, Engine, fields, Header, HostCase, Outcome, Placed, Recorded, Reply, Survey };
