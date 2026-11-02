import { Schema } from 'effect';
import { answers, Expression, Name, Reads } from '#/cases';
import { common, hostCase, outcome, record, recorded, reply } from '#/results';
import { JsonObject } from '#/values';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SURVEY = 'Host state, bpy.data collections, object modes, area types, and region types the installation declares';

// --- [MODELS] --------------------------------------------------------------------------

const members = <F extends Schema.Struct.Fields>(required: F) => Schema.StructWithRest(Schema.Struct(required), [JsonObject]);
const Managed = members({
    unit_settings: members({ system: Name, length_unit: Name, scale_length: Schema.Finite }),
    display_settings: members({ display_device: Name }),
    view_settings: members({ view_transform: Name, look: Name }),
    render: members({ resolution_x: Schema.Int, resolution_y: Schema.Int, resolution_percentage: Schema.Int, fps: Schema.Int, fps_base: Schema.Finite }),
}).annotate({ description: 'Scene members written in RNA order, units, color management, output size, and frame rate required, a nested record for a pointer member' });
const Document = Schema.Union([
    Schema.Literal('none').annotate({ description: 'Factory home file, unsaved, for application state alone' }),
    Schema.Struct({
        open: Name.annotate({ description: 'Absolute path of the case folder copy the engine opens' }),
        options: Schema.optionalKey(JsonObject.annotate({ description: 'wm.open_mainfile properties, load_ui and use_scripts defaulting to the preferences' })),
        scene: Schema.optionalKey(JsonObject.annotate({ description: 'Scene members written over the file in RNA order' })),
    }),
    Schema.Struct({ working_space: Name.annotate({ description: 'wm.set_working_color_space working_space' }), scene: Managed }).annotate({ description: 'Empty file saved in the case folder' }),
]).annotate({ description: 'Case document, the factory home file when absent' });
const Area = Schema.Struct({ type: Name, region: Schema.optionalKey(Name.annotate({ description: 'Region type, WINDOW when absent' })) }).annotate({ description: 'Area type the case overrides, the largest area turned to it when none has it' });
const Added = Schema.Struct({ kind: Name.annotate({ description: 'bpy.data collection' }), properties: Schema.optionalKey(JsonObject) });
const Placed = Schema.Struct({
    extensions: Schema.optionalKey(Schema.Array(Name).annotate({ description: "Extension ids from the owner's repositories, linked into the batch repository and enabled before the document loads" })),
    document: Schema.optionalKey(Document),
    language: Schema.optionalKey(Name.annotate({ description: 'preferences.view.language with every PreferencesView translation on' })),
    window: Schema.optionalKey(Schema.Struct({ scene: Schema.optionalKey(Name), view_layer: Schema.optionalKey(Name), workspace: Schema.optionalKey(Name) }).annotate({ description: 'Window members by name' })),
    area: Schema.optionalKey(Area),
    add: Schema.optionalKey(Schema.Record(Name, Added).annotate({ description: 'IDs created under their key as made[key], objects linked into the scene collection' })),
    set: Schema.optionalKey(Schema.Array(Schema.Tuple([Expression, Schema.Json])).annotate({ description: 'Assignment target and value, written in order' })),
    select: Schema.optionalKey(Schema.Array(Expression).annotate({ description: 'Objects selected alone, the first active' })),
    mode: Schema.optionalKey(Name.annotate({ description: 'object.mode_set mode' })),
    frame: Schema.optionalKey(Schema.Int.annotate({ description: 'scene.frame_set frame' })),
    steps: Schema.optionalKey(Schema.Array(Name).annotate({ description: 'Statements run in one namespace with the reads' })),
    edited: Schema.optionalKey(Schema.Boolean.annotate({ description: 'Push an undo step so the document reads edited' })),
});
const HostCase = hostCase({
    name: Name,
    precondition: Placed,
    read: Reads,
    folder: Schema.String.annotate({ description: 'Case folder holding the document copy and every file the case writes' }),
    keywords: Schema.optionalKey(JsonObject.annotate({ description: 'Keyword arguments beside args, operator properties among them' })),
    confirms: Schema.optionalKey(Schema.Array(Schema.Boolean).annotate({ description: 'Return or Escape posted to the window for each popup in order' })),
    files: answers.files,
});
const Session = Schema.Struct({
    records: Schema.String.annotate({ description: 'Records file the engine appends a Started line to before each case and a Finished line after it' }),
    reply: Schema.String.annotate({ description: 'Reply file the engine writes once every case ran' }),
    survey: Schema.Boolean.annotate({ description: 'Reply carries the installation survey' }),
    cases: Schema.Array(HostCase),
});
const Encoded = Schema.fromJsonString(Session);
const HostState = Schema.Struct({
    gpu: Schema.Struct({ backend: Schema.String, vendor: Schema.String, renderer: Schema.String, version: Schema.String }),
    ocio: Schema.Array(Schema.Int),
    build: Schema.Array(Schema.String).annotate({ description: 'bpy.app.build_options members built in' }),
    locale: Schema.String.annotate({ description: 'Interface locale Blender resolved' }),
    python: Schema.String,
    flags: JsonObject.annotate({ description: 'Interpreter sys.flags' }),
    background: Schema.Boolean.annotate({ description: 'Instance runs with --background, no window and no event loop' }),
}).annotate({ description: 'Install state deciding answers beside the version' });
const Written = Schema.fromJsonString(HostState);
const Survey = Schema.Struct({
    host: HostState,
    collections: Schema.Array(Schema.String).annotate({ description: 'bpy.data collection identifiers an add row names as kind' }),
    modes: Schema.Array(Schema.String).annotate({ description: 'object.mode_set mode identifiers' }),
    areas: Schema.Array(Schema.String).annotate({ description: 'Area type identifiers' }),
    regions: Schema.Array(Schema.String).annotate({ description: 'Region type identifiers' }),
}).annotate({ description: 'Installation facts the index holds per bundle identifier and version' });
const Header = Schema.Struct({ application: Schema.String, version: Schema.String, survey: Schema.optionalKey(Survey) });
const Reply = reply(Header);
const fields = {
    console: common.console,
    files: common.files,
    opened: common.opened,
    windows: recorded(Schema.Array(Schema.String).annotate({ description: 'Area types of one window the case opened, closed after it' })),
    stderr: recorded(Schema.String.annotate({ description: 'Line the original wrote to the error stream' })),
    log: recorded(Schema.Struct({ category: Schema.String, message: Schema.String }).annotate({ description: 'Blender operator log line of the case body, each call with its properties' })),
    modal: recorded(Schema.String.annotate({ description: 'Modal operator running after the case settled' })),
    warnings: recorded(Schema.Struct({ category: Schema.String, message: Schema.String, file: Schema.String, line: Schema.Int }).annotate({ description: 'Python warning the case raised, each category shown' })),
    changes: Schema.optionalKey(
        Schema.Struct({
            created: Schema.Array(Schema.String),
            removed: Schema.Array(Schema.String),
            updated: Schema.Record(Schema.String, Schema.Array(Schema.String)),
            undo: Schema.Array(Schema.String),
            dirty: Schema.Boolean,
        }).annotate({ description: 'IDs created and removed by data path, IDs the depsgraph updated with each update kind, undo steps pushed, and the edited state after the case' }),
    ),
    beforeFailed: Schema.optionalKey(Schema.Record(Schema.String, Schema.String).annotate({ description: 'Reads that failed before the case ran' })),
};
const Outcome = outcome(fields);
const Recorded = record(Outcome);
const Engine = Schema.Literal('python');

// --- [EXPORTS] -------------------------------------------------------------------------

export { Encoded, Engine, HostCase, Outcome, Placed, Recorded, Reply, SURVEY, Survey, Written };
