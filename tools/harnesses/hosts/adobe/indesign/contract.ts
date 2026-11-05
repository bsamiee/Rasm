import { Schema, Struct } from 'effect';
import { contract, hostCase } from '#/cases/contract';
import { Assignments, added, additions, Copy, documents, Enumerated, Steps } from '#/cases/precondition';
import { Outcome, recorded } from '#/cases/results';
import { answers, EXTENDSCRIPT, Expression } from '#/cases/schema';
import { JsonObject } from '#/cases/values';

// --- [MODELS] --------------------------------------------------------------------------

const Units = Schema.Struct({ script: Enumerated, horizontal: Enumerated, vertical: Enumerated }).annotate({ description: 'MeasurementUnits enumerators for script reads and the document rulers' });
const Opened = Schema.Union([Schema.Struct({ properties: JsonObject.annotate({ description: 'documents.add properties, {"$": "enum:<Enumeration>.<MEMBER>"} naming an enumerator, each read back after the open' }) }), Schema.Struct({ open: Copy })]);
const Added = added('rectangles', {});
const Placed = Schema.Struct({
    documents: documents(Opened),
    units: Units,
    fonts: Schema.optionalKey(Schema.Array(Schema.NonEmptyString).annotate({ description: 'PostScript names app.fonts must report installed, the case failing on each other status' })),
    idml: Schema.optionalKey(Schema.Boolean.annotate({ description: 'Export each case document as <folder>/<index>.before.idml and .after.idml inside the rolled-back step' })),
    labels: Schema.optionalKey(Schema.Record(Schema.String, Schema.String).annotate({ description: 'insertLabel rows of the first document' })),
    add: additions(Added),
    select: Schema.optionalKey(Schema.Array(Expression).annotate({ description: 'Items app.select selects in the case window, an insertion point included' })),
    set: Assignments,
    steps: Steps,
    menus: Schema.optionalKey(Schema.Boolean.annotate({ description: 'Capture script menu actions and invoke each onInvoke after the run' })),
});
const Enumerations = Schema.Record(Schema.String, Schema.Array(Schema.String)).annotate({ description: 'Surveyed enumeration members the codec qualifies enumerators against' });
const fields = {
    ...Outcome.common,
    dialogs: recorded(Schema.Json.annotate({ description: 'app.dialogs controls with their prefilled values' })),
    menus: recorded(Schema.Json),
    undo: recorded(Schema.String.annotate({ description: 'Undo step a case document recorded during the run, newest first' })),
};
const Held = Schema.Union([Schema.Struct({ index: Schema.Int, failed: Schema.String }), Schema.Struct({ index: Schema.Int, opened: Schema.Array(Schema.Json) })]).annotate({
    description: 'Panel case whose Started line the call wrote, its precondition failed or applied and left open, its Finished line left to the link',
});
const Contract = contract({
    engines: { uxpscript: { extensions: ['idjs'], language: 'uxpscript' }, extendscript: { extensions: EXTENDSCRIPT, language: 'javascript' } },
    Case: hostCase.linked(Placed, { interact: Schema.Boolean.annotate({ description: 'Body runs under INTERACT_WITH_ALL, every other step under NEVER_INTERACT' }), ...Struct.omit(answers, ['presses']) }),
    outcome: fields,
    survey: {
        question: 'Enumerations, working space profiles, languages, locale, and feature set the installation declares',
        host: Schema.Struct({ locale: Schema.String, featureSet: Schema.String, dom: Schema.String.annotate({ description: 'scriptPreferences.version' }) }),
        fields: {
            enumerations: Enumerations,
            cmyk: Schema.Array(Schema.String).annotate({ description: 'CMYK working space profile names' }),
            rgb: Schema.Array(Schema.String).annotate({ description: 'RGB working space profile names' }),
            languages: Schema.Array(Schema.String).annotate({ description: 'Language names with vendors' }),
        },
    },
    reply: {
        state: Schema.Struct({
            modal: Schema.Boolean.annotate({ description: 'app.modalState, under which case documents neither close nor take recovered ones away' }),
            cmyk: Schema.String,
            rgb: Schema.String,
            defaults: JsonObject.annotate({ description: 'Application preference objects read whole at batch start' }),
            unread: Schema.Array(Schema.String).annotate({ description: 'Application members the snapshot and restore could not cover, each with its error' }),
        }),
        held: Schema.optionalKey(Held),
    },
    batch: {
        label: Schema.String.annotate({ description: 'Document label key every case document holds, a document holding it at batch start being a recovered one the engine closes' }),
        enumerations: Enumerations,
        from: Schema.Int.annotate({ description: 'Index of the first case this call runs, earlier cases recorded by an earlier call' }),
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Added, Contract, fields, Held, Placed };
