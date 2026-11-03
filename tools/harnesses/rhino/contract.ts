import { Schema, Struct } from 'effect';
import { answers, Name } from '#/cases';
import { hostCase, Outcome } from '#/results';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REFUSED = ['dialogs', 'windows', 'confirms', 'prompts', 'files'] as const;

// --- [MODELS] --------------------------------------------------------------------------

// --- [CASES]
const Engine = Schema.Literal('csharp');
const Expression = Schema.NonEmptyString.annotate({ description: 'C# expression over doc, view, made, canvas, editor, and scope' });
const Member = Schema.NonEmptyString.annotate({ description: 'Enumeration member name the engine decodes against RhinoCommon' });
const Point = Schema.Tuple([Schema.Finite, Schema.Finite, Schema.Finite]);
const culture = <S extends Schema.Top>(names: S) => Schema.optionalKey(names.annotate({ description: 'Culture the instance runs or one of its parents' }));
const Tolerance = Schema.Struct({
    absolute: Schema.Finite.annotate({ description: 'ModelAbsoluteTolerance in the case units' }),
    angle: Schema.optionalKey(Schema.Finite.annotate({ description: 'ModelAngleToleranceDegrees' })),
    relative: Schema.optionalKey(Schema.Finite.annotate({ description: 'ModelRelativeTolerance' })),
});
const Template = { name: Schema.NonEmptyString.annotate({ description: 'File under FileSettings.TemplateFolder of the running language' }), units: Member.annotate({ description: 'UnitSystem member' }), tolerance: Tolerance };
const Opened = { path: Schema.NonEmptyString.annotate({ description: '.3dm path under the product folder, absolute once placed, its own units and tolerances kept unless stated' }), units: Schema.optionalKey(Member), tolerance: Schema.optionalKey(Tolerance) };
const Source = Schema.TaggedUnion({ Template, Open: Opened, Application: {} }).annotate({ description: 'Case document source, an application case opening none' });
const Placed = Schema.Struct({
    source: Source,
    language: culture(Schema.NonEmptyString),
    view: Schema.optionalKey(
        Schema.Struct({
            projection: Schema.optionalKey(Member.annotate({ description: 'DefinedViewportProjection member' })),
            display: Schema.optionalKey(Schema.NonEmptyString.annotate({ description: 'DisplayModeDescription English name' })),
            location: Schema.optionalKey(Point),
            target: Schema.optionalKey(Point),
            lens: Schema.optionalKey(Schema.Finite.annotate({ description: 'Camera35mmLensLength' })),
        }).annotate({ description: 'Active viewport of the case document' }),
    ),
    layout: Schema.optionalKey(Schema.Struct({ name: Schema.NonEmptyString, width: Schema.Finite, height: Schema.Finite, units: Schema.optionalKey(Member.annotate({ description: 'UnitSystem member of the page units' })) }).annotate({ description: 'Page view added in page units' })),
    layers: Schema.optionalKey(
        Schema.Array(Schema.Struct({ path: Schema.NonEmptyString.annotate({ description: 'Layer full path, parents joined by ::' }), color: Schema.optionalKey(Schema.NonEmptyString.annotate({ description: 'HTML color name or #rrggbb' })), locked: Schema.optionalKey(Schema.Boolean) })).annotate({
            description: 'Layers made or updated before add, each parent path created first',
        }),
    ),
    add: Schema.optionalKey(
        Schema.Array(Schema.Struct({ as: Schema.NonEmptyString, geometry: Expression.annotate({ description: 'GeometryBase expression' }), layer: Schema.optionalKey(Schema.NonEmptyString.annotate({ description: 'Layer full path' })) })).annotate({
            description: 'Objects added with default attributes, their ids read as made["<as>"]',
        }),
    ),
    select: Schema.optionalKey(Schema.Array(Schema.NonEmptyString).annotate({ description: 'made names selected, a locked, hidden, or absent one failing the case' })),
    canvas: Schema.optionalKey(Schema.Struct({ open: Schema.optionalKey(Schema.NonEmptyString.annotate({ description: '.ghz path under the product folder, absolute once placed' })) }).annotate({ description: 'Grasshopper 2 document the editor canvas shows, new without open' })),
    steps: Schema.optionalKey(Schema.Array(Schema.NonEmptyString).annotate({ description: 'C# statements run after add and select in one undo record' })),
    batch: Schema.optionalKey(Schema.Boolean.annotate({ description: 'BatchMode on for the case, command line input in place of dialogs' })),
});
const HostCase = hostCase({
    precondition: Placed,
    folder: Schema.String.annotate({ description: 'Case folder the engine writes the document copies and captures into' }),
    commands: Schema.optionalKey(Schema.Array(Schema.NonEmptyString).annotate({ description: 'Macros run one per RunScript on the case document, each answering its own prompts' })),
    plugin: Schema.optionalKey(Schema.String.annotate({ description: 'Plug-in binary the build references and publicizes and the engine loads before the cases' })),
    rebuilt: Schema.optionalKey(
        Schema.Struct({
            fn: Schema.NonEmptyString.annotate({ description: 'Rebuilt module function the body calls with the case args in place of the original' }),
            module: Schema.String.annotate({ description: 'Rebuilt module the build references' }),
        }),
    ),
    ...Struct.pick(answers, REFUSED),
});
const Row = Schema.Struct({
    name: Name,
    folder: Schema.String,
    precondition: Placed,
    reads: Schema.Array(Schema.String).annotate({ description: 'Read keys in the order of their compiled expressions' }),
    macros: Schema.Array(Schema.NonEmptyString),
    args: Schema.Array(Schema.Json),
    script: Schema.optionalKey(Schema.String.annotate({ description: 'Script RhinoCode runs by its extension or #! header on the case document' })),
});
const Rows = Schema.fromJsonString(Schema.Struct({ cases: Schema.Array(Row), plugin: Schema.optionalKey(Schema.String) })).annotate({ description: 'Batch file the engine reads, one row per compiled case' });
const Request = Schema.fromJsonString(Schema.Array(Schema.String)).annotate({ description: 'Loader request, assembly, batch, records, and reply paths for a batch, none for the quit' });

// --- [INSTANCE]
const Language = Schema.Struct({ lcid: Schema.Int, culture: Schema.String }).annotate({ description: 'RhCore localization the instance runs' });
const Environment = Schema.Struct({ version: Schema.String, runtime: Schema.String, system: Schema.String }).annotate({ description: 'RhinoApp.Version, .NET runtime, and OS' });
const Survey = Schema.Struct({ host: Schema.Struct({ language: Language, environment: Environment }) }).annotate({ description: 'Instance facts the loader reports at launch, deciding answers beside the version' });
const Session = Schema.fromJsonString(Schema.Struct({ application: Schema.String, version: Schema.String, survey: Survey })).annotate({ description: 'File the loader writes once the instance runs it' });
const Loaded = Schema.Struct({ name: Schema.String, version: Schema.String });
const PlugInLoad = Schema.Struct({ file: Schema.String, id: Schema.String, result: Schema.String, current: Schema.Boolean.annotate({ description: 'Loaded module is the file build' }) });
const Grasshopper = Schema.Struct({ version: Schema.String, loaded: Schema.Array(Schema.String), failed: Schema.Array(Schema.String), incomplete: Schema.optionalKey(Schema.String) });
const State = Schema.Struct({
    plugins: Schema.Array(Loaded).annotate({ description: 'Plug-ins loaded when the batch ended' }),
    plugIn: Schema.optionalKey(PlugInLoad.annotate({ description: 'Batch plug-in load' })),
    grasshopper: Schema.optionalKey(Grasshopper),
});
const Closed = Schema.fromJsonString(Schema.Struct({ ...State.fields, failed: Schema.optionalKey(Schema.String.annotate({ description: 'Batch failure before or around the cases' })) })).annotate({ description: 'Reply file the engine writes when the batch ends' });
const Info = Schema.Struct({ CFBundleIdentifier: Schema.String, CFBundleVersion: Schema.String, CFBundleName: Schema.String });
const Sarif = Schema.fromJsonString(
    Schema.Struct({
        runs: Schema.Array(
            Schema.Struct({
                results: Schema.optionalKey(
                    Schema.Array(
                        Schema.Struct({
                            ruleId: Schema.optionalKey(Schema.String),
                            level: Schema.optionalKey(Schema.String),
                            message: Schema.Struct({ text: Schema.String }),
                            suppressions: Schema.optionalKey(Schema.Array(Schema.Unknown)),
                            locations: Schema.optionalKey(Schema.Array(Schema.Struct({ physicalLocation: Schema.optionalKey(Schema.Struct({ artifactLocation: Schema.Struct({ uri: Schema.String }), region: Schema.optionalKey(Schema.Struct({ startLine: Schema.Int })) })) }))),
                        }),
                    ),
                ),
            }),
        ),
    }),
);

// --- [OUTCOMES]
const fields = {
    ...Struct.pick(Outcome.common, ['console']),
    document: Schema.optionalKey(
        Schema.Struct({
            serial: Schema.Int,
            path: Schema.optionalKey(Schema.String),
            template: Schema.optionalKey(Schema.String),
            units: Schema.Struct({ model: Schema.String, page: Schema.String }),
            tolerance: Schema.Struct({ absolute: Schema.Finite, angle: Schema.Finite, relative: Schema.Finite }),
            active: Schema.Boolean,
            modified: Schema.Boolean,
            selected: Schema.Int,
            view: Schema.optionalKey(Schema.Struct({ name: Schema.String, display: Schema.String, parallel: Schema.Boolean })),
            undo: Schema.Array(Schema.String),
        }).annotate({ description: 'Case document as the engine reports it after the case' }),
    ),
    saved: Schema.optionalKey(Schema.Struct({ before: Schema.String, after: Schema.String }).annotate({ description: '.3dm files of the case document before and after the case' })),
    macros: Schema.optionalKey(Schema.Array(Schema.Struct({ text: Schema.String, completed: Schema.Boolean })).annotate({ description: 'RunScript answer per command macro' })),
    commands: Schema.optionalKey(Schema.Array(Schema.Struct({ name: Schema.String, plugin: Schema.optionalKey(Schema.String), result: Schema.String, document: Schema.Int })).annotate({ description: 'Every command that ended while the case ran' })),
    prompts: Schema.optionalKey(Schema.Array(Schema.Struct({ text: Schema.String, default: Schema.optionalKey(Schema.String), options: Schema.Array(Schema.String) })).annotate({ description: 'Command prompts shown while the case ran' })),
    diagnostics: Schema.optionalKey(Schema.Array(Schema.Struct({ source: Schema.String, kind: Schema.String, message: Schema.String })).annotate({ description: 'Exception reports, RhinoCode logger warnings, and Grasshopper 2 logger entries' })),
    unhandled: Schema.optionalKey(Schema.String.annotate({ description: 'Exception a host thread left unhandled while the case ran, in full, the runtime ending the instance after it' })),
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Closed, culture, Engine, fields, HostCase, Info, Member, Opened, Placed, REFUSED, Request, Rows, Sarif, Session, Source, Survey, Template, Tolerance };
