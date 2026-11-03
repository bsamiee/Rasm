import { Schema, Struct } from 'effect';
import { answers, Expression } from '#/cases';
import { Assignments, added, Copy, documents, Enumerated, Steps } from '#/precondition';
import { hostCase, Outcome, Records, recorded, reply, Surveying } from '#/results';
import { EXTENDSCRIPT } from '#/source';
import { JsonObject } from '#/values';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ENGINES = {
    extendscript: { extensions: EXTENDSCRIPT, language: 'javascript' },
    uxpscript: { extensions: ['idjs'], language: 'uxpscript' },
} as const;

// --- [MODELS] --------------------------------------------------------------------------

const Engine = Schema.Literals(Struct.keys(ENGINES));
const Units = Schema.Struct({ script: Enumerated, horizontal: Enumerated, vertical: Enumerated }).annotate({ description: 'MeasurementUnits enumerators for script reads and the document rulers' });
const Opened = Schema.Union([Schema.Struct({ properties: JsonObject.annotate({ description: 'documents.add properties, {"$": "enum:<Enumeration>.<MEMBER>"} naming an enumerator, each read back after the open' }) }), Schema.Struct({ open: Copy })]);
const Placed = Schema.Struct({
    documents: documents(Opened),
    units: Units,
    fonts: Schema.optionalKey(Schema.Array(Schema.NonEmptyString).annotate({ description: 'PostScript names app.fonts must report installed, the case failing on each other status' })),
    idml: Schema.optionalKey(Schema.Boolean.annotate({ description: 'Export each case document as <folder>/<index>.before.idml and .after.idml inside the rolled-back step' })),
    labels: Schema.optionalKey(Schema.Record(Schema.String, Schema.String).annotate({ description: 'insertLabel rows of the first document' })),
    add: added('rectangles', {}),
    select: Schema.optionalKey(Schema.Array(Expression).annotate({ description: 'Items app.select selects in the case window, an insertion point included' })),
    set: Assignments,
    steps: Steps,
    menus: Schema.optionalKey(Schema.Boolean.annotate({ description: 'Capture script menu actions and invoke each onInvoke after the run' })),
});
const HostCase = hostCase({
    precondition: Placed,
    folder: Schema.String.annotate({ description: 'Case folder holding document copies and every file the case writes' }),
    interact: Schema.Boolean.annotate({ description: 'Body runs under INTERACT_WITH_ALL, every other step under NEVER_INTERACT' }),
    ...Struct.omit(answers, ['presses']),
});
const Batch = Schema.Struct({
    records: Records,
    label: Schema.String.annotate({ description: 'Document label key every case document holds, a document holding it at batch start being a recovered one the engine closes' }),
    enumerations: Schema.Record(Schema.String, Schema.Array(Schema.String)).annotate({ description: 'Surveyed enumeration members the codec qualifies enumerators against' }),
    survey: Surveying,
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
const Reply = reply({ state: State, survey: Schema.optionalKey(Survey) });
const fields = {
    ...Outcome.common,
    dialogs: recorded(Schema.Json.annotate({ description: 'app.dialogs controls with their prefilled values' })),
    menus: recorded(Schema.Json),
    undo: recorded(Schema.String.annotate({ description: 'Undo step a case document recorded during the run, newest first' })),
};
const HostOutcome = Schema.Struct({ ...Outcome.fields, ...fields });
const Recorded = Records.line(HostOutcome);
const Encoded = Schema.fromJsonString(Batch);

// --- [EXPORTS] -------------------------------------------------------------------------

export { ENGINES, Encoded, Engine, fields, HostCase, HostOutcome, Placed, Recorded, Reply, Survey };
