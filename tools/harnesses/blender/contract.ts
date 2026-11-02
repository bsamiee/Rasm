import { Effect, Schema, Struct } from 'effect';
import { answers, Call, Batch as Kit, Probe, Run } from '#/cases';
import { outcome, results } from '#/results';

// --- [MODELS] --------------------------------------------------------------------------

const Name = Schema.NonEmptyString;
const Expression = Schema.NonEmptyString.annotate({ description: 'Python expression reading bpy, context, scene, made, and after the case depsgraph' });
const Members = Schema.Record(Schema.String, Schema.Json);
const absent = <S extends Schema.Top>(schema: S, value: S['Encoded']) => schema.pipe(Schema.withDecodingDefaultKey(Effect.succeed(value)));
const empty = <S extends Schema.Top & Schema.WithoutConstructorDefault>(schema: S, value: S['Type'] & S['Encoded']) => absent(schema.pipe(Schema.withConstructorDefault(Effect.succeed(value))), value);
const members = <F extends Schema.Struct.Fields>(required: F) => Schema.StructWithRest(Schema.Struct(required), [Members]);
const Managed = members({
    unit_settings: members({ system: Name, length_unit: Name, scale_length: Schema.Finite }),
    display_settings: members({ display_device: Name }),
    view_settings: members({ view_transform: Name, look: Name }),
    render: members({ resolution_x: Schema.Int, resolution_y: Schema.Int, resolution_percentage: Schema.Int, fps: Schema.Int, fps_base: Schema.Finite }),
}).annotate({ description: 'Scene members written in RNA order, units, color management, output size, and frame rate required, a nested record for a pointer member' });
const Document = Schema.Union([
    Schema.Literal('none').annotate({ description: 'Factory home file, unsaved, for application state alone' }),
    Schema.Struct({
        open: Name.annotate({ description: '.blend path under the product folder the case folder copies' }),
        options: Schema.optionalKey(Members.annotate({ description: 'wm.open_mainfile properties, load_ui and use_scripts defaulting to the preferences' })),
        scene: Schema.optionalKey(Members.annotate({ description: 'Scene members written over the file in RNA order' })),
    }),
    Schema.Struct({ working_space: Name.annotate({ description: 'wm.set_working_color_space working_space' }), scene: Managed }).annotate({ description: 'Empty file saved in the case folder' }),
]).annotate({ description: 'Case document, every case stating one through its own or the batch precondition' });
const Precondition = Schema.Struct({
    extensions: Schema.optionalKey(Schema.Array(Name).annotate({ description: "Extension ids from the owner's repositories, linked into the batch repository and enabled before the document loads" })),
    document: Schema.optionalKey(Document),
    language: Schema.optionalKey(Name.annotate({ description: 'preferences.view.language with every PreferencesView translation on' })),
    window: Schema.optionalKey(Schema.Struct({ scene: Schema.optionalKey(Name), view_layer: Schema.optionalKey(Name), workspace: Schema.optionalKey(Name) }).annotate({ description: 'Window members by name' })),
    area: Schema.optionalKey(Schema.Struct({ type: Name, region: Schema.optionalKey(Name.annotate({ description: 'Region type, WINDOW when absent' })) }).annotate({ description: 'Area type the case overrides, the largest area turned to it when none has it' })),
    add: Schema.optionalKey(Schema.Record(Name, Schema.Struct({ kind: Name.annotate({ description: 'bpy.data collection' }), properties: Schema.optionalKey(Members) })).annotate({ description: 'IDs created under their key as made[key], objects linked into the scene collection' })),
    set: Schema.optionalKey(Schema.Array(Schema.Tuple([Expression, Schema.Json])).annotate({ description: 'Assignment target and value, written in order' })),
    select: Schema.optionalKey(Schema.Array(Expression).annotate({ description: 'Objects selected alone, the first active' })),
    mode: Schema.optionalKey(Name.annotate({ description: 'object.mode_set mode' })),
    frame: Schema.optionalKey(Schema.Int.annotate({ description: 'scene.frame_set frame' })),
    steps: Schema.optionalKey(Schema.Array(Name).annotate({ description: 'Statements run in one namespace with the reads' })),
    edited: Schema.optionalKey(Schema.Boolean.annotate({ description: 'Push an undo step so the document reads edited' })),
});
const omitted = [...Struct.keys(Struct.omit(answers, ['confirms', 'files', 'presses'])), 'precondition'] as const;
const keyed = { keywords: Schema.optionalKey(Members.annotate({ description: 'Keyword arguments, operator properties among them' })), precondition: Schema.optionalKey(Precondition) };
const Batch = Schema.Struct({
    script: Schema.optionalKey(Name.annotate({ description: 'Script, add-on, or extension folder under the product folder for cases naming none' })),
    module: Schema.optionalKey(Name.annotate({ description: 'Rebuilt Python module under the product folder compare calls in the host' })),
    precondition: Schema.optionalKey(Precondition),
    read: Probe.fields.read,
    expect: Probe.fields.expect,
    keys: Probe.fields.keys,
    vector: Schema.optionalKey(Schema.Struct(Struct.omit(Kit.fields.vector.schema.fields, ['lines']))),
    cases: Schema.Array(Schema.Union([Schema.Struct({ ...Struct.omit(Probe.fields, omitted), ...keyed }), Schema.Struct({ ...Struct.omit(Call.fields, [...omitted, 'lines']), ...keyed }), Schema.Struct({ ...Struct.omit(Run.fields, omitted), ...keyed })])),
});
const shared = {
    name: Schema.String,
    precondition: Schema.Struct({ ...Precondition.fields, document: Document }),
    read: Schema.Record(Schema.String, Schema.String),
    confirms: Schema.Array(Schema.Boolean).annotate({ description: 'Return or Escape posted to the window for each popup in order' }),
    files: Schema.Array(
        Schema.NullOr(
            Schema.Struct({
                path: Schema.String.annotate({ description: 'Path under the case folder the file browser executes' }),
                source: Schema.optionalKey(Schema.String.annotate({ description: 'Product file or folder copied to path before the case' })),
            }),
        ),
    ).annotate({ description: 'File browser answers in order, null cancelling one' }),
};
const HostCase = Schema.TaggedUnion({
    Probe: { ...shared, evaluate: Schema.String },
    Call: { ...shared, script: Schema.optionalKey(Schema.String), call: Schema.String, args: Schema.Array(Schema.Json), keywords: Members },
    Run: { ...shared, script: Schema.String },
});
const Session = Schema.Struct({
    limit: Schema.Finite.annotate({ description: 'Seconds a case settles before its outcome records the jobs still running' }),
    results: Schema.String,
    stdout: Schema.String,
    stderr: Schema.String,
    log: Schema.String,
    dictionary: Schema.optionalKey(Schema.String.annotate({ description: 'File the engine writes the RNA dictionary of every struct and operator to before the cases' })),
    cases: Schema.Array(HostCase),
});
const fields = {
    modal: empty(Schema.Array(Schema.String).annotate({ description: 'Modal operators running after the case settled' }), []),
    jobs: empty(Schema.Array(Schema.String).annotate({ description: 'Job types still running at the settle limit' }), []),
    stdout: empty(Schema.String.annotate({ description: 'Process output of the case body and its settle, C-level output and operator reports included' }), ''),
    stderr: empty(Schema.String, ''),
    log: empty(Schema.Array(Schema.Struct({ category: Schema.String, message: Schema.String })).annotate({ description: 'Blender operator log lines of the case body, each call with its properties' }), []),
    warnings: empty(Schema.Array(Schema.Struct({ category: Schema.String, message: Schema.String, file: Schema.String, line: Schema.Int })).annotate({ description: 'Python warnings the case raised, each category shown' }), []),
    windows: empty(Schema.Array(Schema.Struct({ areas: Schema.Array(Schema.String), picked: Schema.optionalKey(Schema.NullOr(Schema.String)) })).annotate({ description: 'Area types of each window the case opened, closed after it, a file browser with the path given' }), []),
    files: empty(Schema.Record(Schema.String, Schema.Struct({ size: Schema.Int, sha256: Schema.String, pixels: Schema.optionalKey(Schema.String) })).annotate({ description: 'Case folder files the case body wrote, image pixels digested apart from metadata' }), {}),
    document: Schema.optionalKey(Members.annotate({ description: 'Units, color management, working color space, output size and rate, frame, context members, version, path, and edited state the case ran on' })),
    writes: Schema.optionalKey(
        Schema.Struct({
            created: Schema.Array(Schema.String),
            removed: Schema.Array(Schema.String),
            updated: Schema.Record(Schema.String, Schema.Array(Schema.String)),
            undo: Schema.Array(Schema.String),
            dirty: Schema.Boolean,
        }).annotate({ description: 'IDs created and removed by data path, IDs the depsgraph updated with each update kind, undo steps pushed, and the edited state after the case' }),
    ),
    beforeFailed: Schema.optionalKey(Schema.Record(Schema.String, Schema.String).annotate({ description: 'Reads that failed before the case ran' })),
    backtraces: empty(Schema.Array(Schema.String).annotate({ description: 'Crash records Blender wrote beside a crash: C and Python backtraces' }), []),
    stacks: Schema.optionalKey(Schema.String.annotate({ description: 'Python stacks of every thread faulthandler wrote before a hung instance ended' })),
};
const Outcome = outcome(fields);
const Recorded = Schema.Struct({ ...Outcome.fields, answered: absent(Outcome.fields.answered, []), leaked: absent(Outcome.fields.leaked, []) });
const Host = Schema.Struct({
    gpu: Schema.Struct({ backend: Schema.String, vendor: Schema.String, renderer: Schema.String, version: Schema.String }),
    ocio: Schema.Array(Schema.Int),
    build: Schema.Array(Schema.String).annotate({ description: 'bpy.app.build_options members built in' }),
    locale: Schema.String.annotate({ description: 'Interface locale Blender resolved' }),
    python: Schema.String,
    flags: Members.annotate({ description: 'Interpreter sys.flags' }),
});
const Entry = Schema.TaggedUnion({ Host: { host: Host }, Started: { name: Schema.String }, Finished: { outcome: Recorded }, Failed: { reason: Schema.String } });
const Engine = Schema.Literal('python');
const Results = results(Engine, { host: Schema.optionalKey(Host) }, fields);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Batch, Engine, Entry, HostCase, Outcome, Results, Session };
