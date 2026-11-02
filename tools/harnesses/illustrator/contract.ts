import { Record, Schema, Struct } from 'effect';
import { answers, Expression, Name, Reads } from '#/cases';
import { outcome, recorded } from '#/results';
import { JsonObject } from '#/values';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PASTEBOARD = ['copy', 'cut'] as const;
const PREFERENCE = { Boolean: Schema.Boolean, Integer: Schema.Int, Real: Schema.Finite, String: Schema.String } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Engine = Schema.Literal('extendscript');
const Enumerated = Schema.Struct({ $: Schema.TemplateLiteral(['enum:', Schema.String, '.', Schema.String]) }).annotate({ description: 'Enumerator the surveyed enumerations declare' });
const Assignment = Schema.Tuple([Expression, Schema.Json]);
const Created = Schema.Struct({
    preset: Schema.String.annotate({ description: 'app.startupPresetsList entry whose settings documents.addDocument starts from' }),
    settings: JsonObject.annotate({ description: 'DocumentPreset members written over the preset settings' }),
    writes: Schema.Array(Assignment).annotate({ description: 'Document member path and value written after the open' }),
    expects: Schema.Array(Assignment).annotate({ description: 'Document read and the value the open must leave, each mismatch failing the case' }),
});
const Opened = Schema.Struct({
    open: Schema.NonEmptyString.annotate({ description: 'Absolute path of the case copy the engine opens' }),
    colorSpace: Schema.optionalKey(Enumerated.annotate({ description: 'DocumentColorSpace app.open converts the document to' })),
    options: JsonObject.annotate({ description: 'OpenOptions members written over a new OpenOptions' }),
});
const Preference = Schema.Union(Record.collect(PREFERENCE, (kind, value) => Schema.Struct({ key: Schema.NonEmptyString, kind: Schema.Literal(kind), value }))).annotate({ description: 'app.preferences key and the value its set<kind>Preference writes' });
const Placed = Schema.Struct({
    documents: Schema.Array(Schema.Union([Created, Opened])).annotate({ description: 'Case documents opened with a window in order, the first one active, none for an application case' }),
    coordinates: Enumerated.annotate({ description: 'CoordinateSystem every position and bounds read and write takes, the earlier value restored after the case' }),
    interaction: Enumerated.annotate({ description: 'UserInteractionLevel the case runs under, its own writes reaching the host and restored after it' }),
    colorSettings: Schema.optionalKey(Schema.NonEmptyString.annotate({ description: 'app.colorSettingsList file loaded before the documents open, the batch-start settings loaded back after the case' })),
    fonts: Schema.optionalKey(Schema.Array(Schema.NonEmptyString).annotate({ description: 'PostScript names app.textFonts must hold' })),
    preferences: Schema.optionalKey(Schema.Array(Preference).annotate({ description: 'app.preferences keys written before the case and restored after it' })),
    sends: Schema.optionalKey(Schema.Array(Schema.NonEmptyString).annotate({ description: 'Application methods the case sends to the host, every other one recorded as an activation, each paired state restored after it' })),
    add: Schema.optionalKey(
        Schema.Array(
            Schema.Struct({
                as: Schema.NonEmptyString,
                to: Expression,
                kind: Schema.NonEmptyString.annotate({ description: 'Collection member of the target, as pathItems' }),
                method: Schema.optionalKey(Schema.NonEmptyString.annotate({ description: 'Factory the collection reflects, as rectangle or areaText, add when absent' })),
                args: Schema.optionalKey(Schema.Array(Expression).annotate({ description: 'Factory arguments evaluated over doc and made' })),
                properties: Schema.optionalKey(JsonObject),
            }),
        ).annotate({ description: 'Items added and named as made.<as>' }),
    ),
    select: Schema.optionalKey(Schema.Array(Expression).annotate({ description: 'Items the first document selects, a text range through its select member' })),
    set: Schema.optionalKey(Schema.Array(Assignment).annotate({ description: 'Member path and value written in order, [*] spreading over a collection' })),
    steps: Schema.optionalKey(Schema.Array(Schema.NonEmptyString).annotate({ description: 'Statements run in order after set and add' })),
    events: Schema.optionalKey(
        Schema.Array(
            Schema.Struct({
                type: Schema.NonEmptyString,
                data: Schema.optionalKey(Schema.String),
                scope: Schema.optionalKey(Schema.Literals(['APPLICATION', 'GLOBAL'])),
                appId: Schema.optionalKey(Schema.NonEmptyString),
                extensionId: Schema.optionalKey(Schema.NonEmptyString),
            }),
        ).annotate({ description: 'CSXSEvent dispatches ending the call after the steps, the case resuming in the next call once the instance answers nothing' }),
    ),
});
const Source = Schema.Struct({ script: Schema.String.annotate({ description: 'Original script path, the $.fileName the case answers' }), code: Schema.String.annotate({ description: 'Prepared source the engine evaluates' }) });
const shared = {
    name: Name,
    precondition: Placed,
    read: Reads,
    folder: Schema.String.annotate({ description: 'Case folder holding document copies and every file the case writes' }),
    ...Struct.omit(answers, ['presses', 'dialogs', 'files']),
    files: Schema.optionalKey(Schema.Array(Schema.NullOr(Schema.String)).annotate({ description: 'Absolute path each file or folder picker returns in order' })),
};
const HostCase = Schema.TaggedUnion({
    Run: { ...shared, source: Source, args: Schema.Array(Schema.Json) },
    Call: { ...shared, source: Schema.optionalKey(Source), call: Expression, args: Schema.Array(Schema.Json) },
    Probe: { ...shared, evaluate: Expression },
});
const label = Schema.String.annotate({ description: 'Title prefix of every created case document, a document holding it being one the engine closes' });
const Cases = {
    records: Schema.String.annotate({ description: 'Folder the engine writes <index>.started into before each case and <index>.json after it' }),
    label,
    colorSettings: Schema.String.annotate({ description: 'Copy of the instance color settings taken before the batch, loaded back after a case loads its own' }),
    from: Schema.Int.annotate({ description: 'Index of the first case the call runs, records numbered from the batch start' }),
    cases: Schema.Array(HostCase),
};
const Batch = Schema.TaggedUnion({
    Survey: { types: Schema.Array(Schema.String).annotate({ description: 'DocumentPresetType members the engine resolves to startup presets' }) },
    Cases,
    Resumed: { ...Cases, cases: Cases.cases.annotate({ description: 'Cases from the one whose events ended the previous call, it continuing from its held file' }) },
    Closed: { label, folders: Schema.Array(Schema.String).annotate({ description: 'Case folders whose opened copies the engine closes unsaved' }) },
});
const Host = Schema.Struct({
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
    application: JsonObject.annotate({ description: 'Readonly application members read whole, build, locale, preset, color settings, and printer lists among them' }),
    methods: Schema.Array(Schema.String).annotate({ description: 'Application methods app.reflect lists' }),
    options: JsonObject.annotate({ description: 'new OpenOptions read whole' }),
};
const Survey = Schema.Struct({
    enumerations: Schema.Record(Schema.String, Schema.Array(Schema.String)).annotate({ description: 'Enumeration members of the scripting dictionary the embedded UXP DOM maps' }),
    ...Surveyed,
    host: Host,
}).annotate({ description: 'Installation facts the index holds per bundle identifier and version' });
const Header = Schema.Struct({
    application: Schema.String,
    version: Schema.String,
    state: Schema.Struct({
        documents: Schema.Int.annotate({ description: 'Documents open at batch start' }),
        application: JsonObject.annotate({ description: 'Writable application and preference option members read whole at batch start, userInteractionLevel included' }),
        unread: Schema.Array(Schema.String).annotate({ description: 'Application members the snapshot and restore could not cover, each with its error' }),
    }),
    survey: Schema.optionalKey(Schema.Struct(Surveyed)),
    held: Schema.optionalKey(Schema.Int.annotate({ description: 'Case whose events ended the call, its documents left open' })),
});
const Reply = Schema.fromJsonString(Schema.Union([Header, Schema.Struct({ failed: Schema.String.annotate({ description: 'Batch failure before or around the cases' }) })])).annotate({ description: 'Engine answer to one call, case outcomes sitting in the records folder' });
const Write = Schema.TaggedUnion({
    Application: { member: Schema.String.annotate({ description: 'Member path under app' }), before: Schema.optionalKey(Schema.Json), after: Schema.Json },
    Preference: { key: Schema.String, before: Schema.optionalKey(Schema.Json), after: Schema.Json },
    Pasteboard: { member: Schema.Literals(PASTEBOARD) },
}).annotate({ description: 'Application member, preference key, or pasteboard the original changed, restored after the case' });
const fields = {
    alerts: recorded(Schema.Struct({ text: Schema.String, title: Schema.optionalKey(Schema.String) })),
    confirms: recorded(Schema.String),
    prompts: recorded(Schema.String),
    windows: recorded(Schema.Json.annotate({ description: 'ScriptUI window tree shown and filled' })),
    files: recorded(Schema.Literals(['open', 'save', 'folder']).annotate({ description: 'Picker kind each file or folder picker the original opened' })),
    activations: recorded(Schema.String.annotate({ description: 'Application method the case left out of sends, or a member reaching another app or the host lifecycle, recorded and never sent' })),
    console: recorded(Schema.String.annotate({ description: 'Line the original printed' })),
    logs: recorded(Schema.Struct({ file: Schema.String, line: Schema.String }).annotate({ description: 'Line written while the case ran to a log file no other instance holds open' })),
    writes: recorded(Write),
    opened: recorded(JsonObject.annotate({ description: 'Case document starting state, every reflected member read whole' })),
    written: recorded(Schema.Struct({ file: Schema.String, size: Schema.Int, sha256: Schema.String }).annotate({ description: 'File the case wrote into its folder, digested without the per-save identifiers and dates' })),
};
const Outcome = outcome(fields);
const Recorded = Schema.fromJsonString(Outcome);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Batch, Engine, Enumerated, fields, Header, HostCase, Outcome, PASTEBOARD, Placed, Recorded, Reply, Survey };
