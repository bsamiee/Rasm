import { Effect, Schema } from 'effect';
import { Expression } from '#/cases';
import { JsonObject } from '#/values';

// --- [MODELS] --------------------------------------------------------------------------

const recorded = <S extends Schema.Top>(item: S) => Schema.Array(item).pipe(Schema.withConstructorDefault(Effect.succeed([])), Schema.withDecodingDefaultKey(Effect.succeed([])));
const Pid = Schema.Int.annotate({ description: 'Process ID of the instance, left running for the agent' });
const Answered = Schema.Struct({ title: Schema.optionalKey(Schema.String), text: Schema.Array(Schema.String), pressed: Schema.String }).annotate({ description: 'Host dialog the case expected and the harness answered with the press its case named' });
const Dialog = Schema.Struct({ title: Schema.optionalKey(Schema.String), text: Schema.Array(Schema.String), buttons: Schema.Array(Schema.String), pid: Pid }).annotate({
    description: 'Dialog, sheet, or system prompt no case press answers, shown by the instance left running',
});
const Answer = Schema.Union([Schema.Struct({ value: Schema.Json }), Schema.Struct({ thrown: Schema.String })]).annotate({ description: 'Value or error message the host answered for one call' });
const Completion = Schema.TaggedUnion({
    Ran: {},
    Exited: {},
    Thrown: {
        message: Schema.String,
        number: Schema.optionalKey(Schema.Finite),
        line: Schema.optionalKey(Schema.Int),
        file: Schema.optionalKey(Schema.String),
        stack: Schema.optionalKey(Schema.Array(Schema.Struct({ line: Schema.Int, file: Schema.optionalKey(Schema.String) })).annotate({ description: 'Call chain innermost first, each frame mapped to its source file and line' })),
    },
    Failed: { reason: Schema.String.annotate({ description: 'Precondition, harness, or host failure, original not run or not finished' }) },
});
const Crash = Schema.Struct({
    status: Schema.optionalKey(Schema.Int),
    signal: Schema.optionalKey(Schema.Int),
    report: Schema.optionalKey(Schema.String.annotate({ description: 'Crash report path ReportCrash wrote for the pid' })),
    type: Schema.optionalKey(Schema.String.annotate({ description: 'Mach exception type and signal the report names' })),
    frames: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'Faulting thread frames of the crash report' })),
}).annotate({ description: 'Exit of the instance while the case ran, remaining cases not run' });
const Hang = Schema.Struct({ cpu: Schema.Literals(['busy', 'blocked']), sample: Schema.String.annotate({ description: 'sample report path of the instance' }), pid: Pid }).annotate({
    description: 'Case past the limit with no record, the instance sampled and left running',
});
const Enumerated = Schema.Struct({ $: Schema.TemplateLiteral(['enum:', Schema.String, '.', Schema.String]) }).annotate({ description: 'Enumerator the surveyed enumerations declare' });
const Source = Schema.Struct({ script: Schema.String.annotate({ description: 'Original script path the case answers as the running script' }), code: Schema.String.annotate({ description: 'Prepared source the engine evaluates' }) });
const Written = Schema.Struct({ file: Schema.String, size: Schema.Int, sha256: Schema.String }).annotate({ description: 'File the case wrote into its folder' });
const Common = Schema.Struct({
    alerts: recorded(Schema.Struct({ text: Schema.String, title: Schema.optionalKey(Schema.String) })),
    confirms: recorded(Schema.String),
    prompts: recorded(Schema.String),
    windows: recorded(Schema.Json.annotate({ description: 'ScriptUI window tree shown and filled' })),
    files: recorded(Schema.String.annotate({ description: 'Picker kind each file or folder picker the original opened' })),
    activations: recorded(Schema.String.annotate({ description: 'Member reaching another app, the system, or the host lifecycle, recorded and never sent' })),
    console: recorded(Schema.String.annotate({ description: 'Line the original printed' })),
    writes: recorded(Schema.Struct({ member: Schema.String, before: Schema.optionalKey(Schema.Json), after: Schema.Json }).annotate({ description: 'Application member the original changed, restored after the case' })),
    opened: recorded(Schema.Json.annotate({ description: 'Case document starting state' })),
}).annotate({ description: 'Outcome fields every scripted host records' });
const Difference = Schema.Struct({ path: Schema.String, host: Schema.Json, rebuilt: Schema.Json });
const Compare = Schema.Struct({
    module: Schema.String.annotate({ description: 'Rebuilt module the host answers were compared against' }),
    agreed: Schema.Int,
    mismatches: Schema.Array(Schema.Struct({ name: Schema.String, fn: Schema.String, args: Schema.Json, differences: Schema.Array(Difference) })),
    unproven: Schema.Array(Schema.String.annotate({ description: 'Case with no host answer or no rebuilt answer to compare' })),
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const outcome = <F extends Schema.Struct.Fields>(fields: F) =>
    Schema.Struct({
        name: Schema.String,
        completion: Completion,
        value: Schema.optionalKey(Schema.Json),
        before: Schema.optionalKey(JsonObject),
        read: Schema.optionalKey(JsonObject),
        readFailed: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)),
        answered: recorded(Answered),
        leaked: recorded(Schema.String.annotate({ description: 'Host state the case changed that the engine could not restore' })),
        crashed: Schema.optionalKey(Crash),
        hung: Schema.optionalKey(Hang),
        dialog: Schema.optionalKey(Dialog),
        written: recorded(Written),
        ...fields,
    });
const hostCase = <S extends Schema.Struct.Fields>(shared: S) =>
    Schema.TaggedUnion({
        Run: { ...shared, source: Source, args: Schema.Array(Schema.Json) },
        Call: { ...shared, source: Schema.optionalKey(Source), call: Expression, args: Schema.Array(Schema.Json) },
        Probe: { ...shared, evaluate: Expression },
    });
const reply = <H extends Schema.Top>(header: H) => Schema.fromJsonString(Schema.Union([header, Schema.Struct({ failed: Schema.String.annotate({ description: 'Batch failure before or around the cases' }) })])).annotate({ description: 'Engine answer to one batch, case outcomes sitting in the records file' });
const record = <O extends Schema.Top>(held: O) => Schema.fromJsonString(Schema.TaggedUnion({ Started: { index: Schema.Int }, Finished: { outcome: held } })).annotate({ description: 'Records line the engine appends before each case and after it' });
const Outcome = Schema.StructWithRest(outcome({}), [Schema.Record(Schema.String, Schema.Unknown)]).annotate({ description: 'Outcome of any host, host-only fields kept as they were recorded' });
const results = <E extends Schema.Top, H extends Schema.Struct.Fields, O extends Schema.Top>(engine: E, header: H, held: O) =>
    Schema.fromJsonString(
        Schema.Struct({
            application: Schema.String.annotate({ description: 'Name the host reports' }),
            identifier: Schema.String.annotate({ description: 'Bundle identifier of the installation' }),
            version: Schema.String.annotate({ description: 'Version the host reports' }),
            engine,
            ...header,
            shared: recorded(Pid).annotate({ description: 'Instances other processes ran before the run, left as they were' }),
            failed: Schema.optionalKey(Schema.String.annotate({ description: 'Why the run ended before every case ran, naming the instance left running' })),
            answered: recorded(Answered).annotate({ description: 'Host dialogs the run expected and answered before the first case started' }),
            cases: Schema.Array(held),
            compare: Schema.optionalKey(Compare),
        }),
        { space: 2 },
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { Answer, Answered, Common, Compare, Completion, Crash, Dialog, Enumerated, Hang, hostCase, Outcome, outcome, Pid, record, recorded, reply, results, Source };
