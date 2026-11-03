import { Record, Schema, Struct } from 'effect';
import { answers, Expression } from '#/cases';
import { Assignments, added, additions, Copy, documents, Enumerated, Steps } from '#/precondition';
import { hostCase, Outcome, Records, recorded, reply } from '#/results';
import { JsonObject, members } from '#/values';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PASTEBOARD = ['copy', 'cut'] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Engine = Schema.Literal('extendscript');
const Assignment = Schema.Tuple([Expression, Schema.Json]);
const Created = Schema.Struct({
    preset: Schema.String.annotate({ description: 'app.startupPresetsList entry whose settings documents.addDocument starts from' }),
    settings: JsonObject.annotate({ description: 'DocumentPreset members written over the preset settings' }),
    writes: Schema.Array(Assignment).annotate({ description: 'Document member path and value written after the open' }),
    expects: Schema.Array(Assignment).annotate({ description: 'Document read and the value the open must leave, each mismatch failing the case' }),
});
const Opened = Schema.Struct({
    open: Copy,
    colorSpace: Schema.optionalKey(Enumerated.annotate({ description: 'DocumentColorSpace app.open converts the document to' })),
    options: JsonObject.annotate({ description: 'OpenOptions members written over a new OpenOptions' }),
});
const Preference = Schema.Union(Record.collect({ Boolean: Schema.Boolean, Integer: Schema.Int, Real: Schema.Finite, String: Schema.String } as const, (kind, value) => Schema.Struct({ key: Schema.NonEmptyString, kind: Schema.Literal(kind), value }))).annotate({
    description: 'app.preferences key and the value its set<kind>Preference writes',
});
const Added = added('pathItems', {
    method: Schema.optionalKey(Schema.NonEmptyString.annotate({ description: 'Factory the collection reflects, as rectangle or areaText, add when absent' })),
    args: Schema.optionalKey(Schema.Array(Expression).annotate({ description: 'Factory arguments evaluated over doc and made' })),
});
const Placed = Schema.Struct({
    documents: documents(Schema.Union([Created, Opened])),
    coordinates: Enumerated.annotate({ description: 'CoordinateSystem every position and bounds read and write takes, the earlier value restored after the case' }),
    interaction: Enumerated.annotate({ description: 'UserInteractionLevel the case runs under, its own writes reaching the host and restored after it' }),
    colorSettings: Schema.optionalKey(Schema.NonEmptyString.annotate({ description: 'app.colorSettingsList file loaded before the documents open, the batch-start settings loaded back after the case' })),
    fonts: Schema.optionalKey(Schema.Array(Schema.NonEmptyString).annotate({ description: 'PostScript names app.textFonts must hold' })),
    preferences: Schema.optionalKey(Schema.Array(Preference).annotate({ description: 'app.preferences keys written before the case and restored after it' })),
    sends: Schema.optionalKey(Schema.Array(Schema.NonEmptyString).annotate({ description: 'Application methods the case sends to the host, every other one recorded as an activation, each paired state restored after it' })),
    add: additions(Added),
    select: Schema.optionalKey(Schema.Array(Expression).annotate({ description: 'Items the first document selects, a text range through its select member' })),
    set: Assignments,
    steps: Steps,
});
const HostCase = hostCase({
    precondition: Placed,
    folder: Schema.String.annotate({ description: 'Case folder holding document copies and every file the case writes' }),
    ...Struct.omit(answers, ['presses', 'dialogs']),
});
const Enumerations = Schema.Record(Schema.String, Schema.Array(Schema.String)).annotate({ description: 'Enumeration members of the scripting dictionary the embedded UXP DOM maps' });
const Sent = Schema.TaggedUnion({
    Survey: { enumerations: Enumerations.annotate({ description: 'Enumerations whose DocumentPresetType members the engine resolves to startup presets' }) },
    Cases: {
        records: Records,
        label: Schema.String.annotate({ description: 'Title prefix of every created case document, a document holding it at batch start being a recovered one the engine closes' }),
        colorSettings: Schema.String.annotate({ description: 'Copy of the instance color settings taken before the batch, loaded back after a case loads its own' }),
        logs: Schema.Array(Schema.String.annotate({ description: 'AIGeneral, AIHangMonitor or UXP log the instance alone holds open for writing, its lines written during a case recorded as the case logs' })),
        cases: Schema.Array(HostCase),
    },
    Closed: { folders: Schema.Array(Schema.String.annotate({ description: 'Folders whose open documents the engine closes unsaved before the quit' })) },
});
const Install = Schema.Struct({
    build: Schema.String.annotate({ description: 'Info.plist AdobeBuildNumber' }),
    locale: Schema.String.annotate({ description: 'Settings folder locale the instance holds open' }),
    features: JsonObject.annotate({ description: 'Server feature flags the settings folder .runConfig caches for the version' }),
    plugins: Schema.Array(Schema.String).annotate({ description: 'Plug-ins the instance maps from outside its application bundle' }),
}).annotate({ description: 'Install state deciding answers beside the version, read before any host call' });
const Preset = Schema.Struct({
    types: Schema.Array(Schema.String).annotate({ description: 'DocumentPresetType members app.getPresetFileOfType resolves to the preset' }),
    settings: JsonObject.annotate({ description: 'app.getPresetSettings read whole' }),
});
const Surveyed = {
    presets: Schema.Record(Schema.String, Preset).annotate({ description: 'app.startupPresetsList entry, localized, and its settings' }),
    application: members({ colorSettingsList: Schema.Array(Schema.String) }).annotate({ description: 'Readonly application members read whole' }),
    methods: Schema.Array(Schema.String).annotate({ description: 'Application methods app.reflect lists' }),
    options: JsonObject.annotate({ description: 'new OpenOptions read whole' }),
};
const Survey = Schema.Struct({ enumerations: Enumerations, ...Surveyed, host: Install }).annotate({ description: 'Installation facts the index holds per bundle identifier and version' });
const State = Schema.Struct({
    documents: Schema.Int.annotate({ description: 'Documents open at batch start' }),
    application: JsonObject.annotate({ description: 'Writable application and preference option members read whole at batch start, userInteractionLevel included' }),
    unread: Schema.Array(Schema.String).annotate({ description: 'Application members the snapshot and restore could not cover, each with its error' }),
}).annotate({ description: 'Instance state at batch start' });
const Reply = reply({ state: State, survey: Schema.optionalKey(Schema.Struct(Surveyed)) });
const Write = Schema.TaggedUnion({
    Application: { member: Schema.String.annotate({ description: 'Member path under app' }), before: Schema.optionalKey(Schema.Json), after: Schema.Json },
    Preference: { key: Schema.String, before: Schema.optionalKey(Schema.Json), after: Schema.Json },
    Pasteboard: { member: Schema.Literals(PASTEBOARD) },
}).annotate({ description: 'Application member, preference key, or pasteboard the original changed, restored after the case' });
const HostOutcome = Outcome.mapFields(
    Struct.assign({
        ...Outcome.common,
        logs: recorded(Schema.Struct({ file: Schema.String, line: Schema.String }).annotate({ description: 'Line written while the case ran to an AIGeneral, AIHangMonitor or UXP log no other process holds open' })),
        writes: recorded(Write),
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Added, Engine, HostCase, HostOutcome, Install, PASTEBOARD, Placed, Reply, Sent, Survey };
