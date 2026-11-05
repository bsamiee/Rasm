import { Effect, Schema } from 'effect';
import { JsonObject } from '#/cases/values';
import type { Harness } from '#/session/harness';

// --- [CONSTANTS] -----------------------------------------------------------------------

const RESULTS = '.results.json';

// --- [MODELS] --------------------------------------------------------------------------

const recorded = <S extends Schema.Top>(item: S) => Schema.Array(item).pipe(Schema.withConstructorDefault(Effect.succeed([])), Schema.withDecodingDefaultKey(Effect.succeed([])));
const Answer = Schema.Union([Schema.Struct({ value: Schema.Json }), Schema.Struct({ thrown: Schema.String })]).annotate({ description: 'Value or error message the host answered for one call' });
const Halted = Schema.TaggedUnion({
    Crashed: {
        status: Schema.optionalKey(Schema.Int),
        signal: Schema.optionalKey(Schema.Int),
        report: Schema.optionalKey(Schema.String.annotate({ description: 'Crash report path ReportCrash wrote for the pid' })),
        exception: Schema.optionalKey(Schema.String.annotate({ description: 'Mach exception type and signal the report names' })),
        frames: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'Faulting thread frames of the crash report' })),
    },
    Hung: {
        process: Schema.optionalKey(Schema.Literals(['running', 'sleeping', 'stopped']).annotate({ description: 'Main thread state when the instance stopped answering' })),
        cpu: Schema.optionalKey(Schema.Finite.annotate({ description: 'CPU seconds when the instance stopped answering' })),
        footprint: Schema.optionalKey(Schema.Finite.annotate({ description: 'Physical footprint bytes when the instance stopped answering' })),
        threads: Schema.optionalKey(Schema.Int),
        sample: Schema.String.annotate({ description: 'sample report path of the instance' }),
        frames: recorded(Schema.String.annotate({ description: 'Main thread frame along the heaviest sample path, leaf first' })),
    },
    Dialog: { kind: Schema.optionalKey(Schema.String.annotate({ description: 'Window kind of the state row' })), number: Schema.optionalKey(Schema.Int), title: Schema.optionalKey(Schema.String), text: Schema.Array(Schema.String), buttons: Schema.Array(Schema.String) },
}).annotate({ description: 'Instance exit while or after the case ran, case past the limit with no record, or dialog no case press answers, a hung or dialog instance left running for the agent' });
const Outcome = Object.assign(
    Schema.Struct({
        name: Schema.String,
        completion: Schema.TaggedUnion({
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
        }),
        value: Schema.optionalKey(Schema.Json),
        before: Schema.optionalKey(JsonObject),
        beforeFailed: Schema.optionalKey(Schema.Record(Schema.String, Schema.String).annotate({ description: 'Reads that failed before the case ran' })),
        read: Schema.optionalKey(JsonObject),
        readFailed: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)),
        answered: recorded(Schema.Struct({ title: Schema.optionalKey(Schema.String), text: Schema.Array(Schema.String), pressed: Schema.String }).annotate({ description: 'Host dialog the case expected and the harness answered with the press its case named' })),
        leaked: recorded(Schema.String.annotate({ description: 'Host state the case changed that the engine could not restore' })),
        halted: Schema.optionalKey(Halted),
        written: recorded(Schema.Struct({ file: Schema.String, size: Schema.Int, sha256: Schema.String }).annotate({ description: 'File the case wrote into its folder' })),
    }).annotate({ description: 'Outcome of any host, each host contract assigning its own recorded fields beside these' }),
    {
        common: {
            alerts: recorded(Schema.Struct({ text: Schema.String, title: Schema.optionalKey(Schema.String) })),
            confirms: recorded(Schema.String),
            prompts: recorded(Schema.String),
            windows: recorded(Schema.Json.annotate({ description: 'ScriptUI window tree shown and filled' })),
            files: recorded(Schema.String.annotate({ description: 'Picker kind each file or folder picker the original opened' })),
            activations: recorded(Schema.String.annotate({ description: 'Member reaching another app, the system, or the host lifecycle, recorded and never sent' })),
            console: recorded(Schema.String.annotate({ description: 'Line the original printed' })),
            writes: recorded(Schema.Struct({ member: Schema.String, before: Schema.optionalKey(Schema.Json), after: Schema.Json }).annotate({ description: 'Application member the original changed, restored after the case' })),
            opened: recorded(Schema.Json.annotate({ description: 'Case document starting state' })),
        },
    },
);
const Records = Object.assign(Schema.String.annotate({ description: 'Records file the engine appends a Started line to before each case and a Finished line after it' }), {
    line: <O extends Schema.Top>(outcome: O) => Schema.fromJsonString(Schema.TaggedUnion({ Started: { index: Schema.Int }, Finished: { outcome } })).annotate({ description: 'Records line the engine appends before each case and after it' }),
});
const Environment = Schema.Record(Schema.String, Schema.String).annotate({ description: 'Environment supplied by the caller that decides startup behavior' });
const Compare = Schema.Struct({
    module: Schema.String.annotate({ description: 'Rebuilt module the host answers were compared against' }),
    agreed: Schema.Int,
    mismatches: Schema.Array(Schema.Struct({ name: Schema.String, fn: Schema.String, args: Schema.Json, differences: Schema.Array(Schema.Struct({ path: Schema.String, host: Schema.Json, rebuilt: Schema.Json })) })),
    unproven: Schema.Array(Schema.String.annotate({ description: 'Case with no host answer or no rebuilt answer to compare' })),
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const deciding = <E extends Schema.Top>(engine: E) => ({
    identifier: Schema.NonEmptyString.annotate({ description: 'Bundle identifier of the installation' }),
    version: Schema.NonEmptyString.annotate({ description: 'Version the installation reports' }),
    engine,
    environment: Schema.optionalKey(Environment),
    host: Schema.optionalKey(Schema.Json.annotate({ description: 'Install state deciding answers beside the version' })),
});
const results = (engine: Harness['Engine'], outcome: Schema.Codec<typeof Outcome.Type, unknown>) =>
    Schema.fromJsonString(
        Schema.Struct({
            application: Schema.String.annotate({ description: 'Name the host reports' }),
            ...deciding(engine),
            state: Schema.optionalKey(Schema.Json.annotate({ description: 'Instance state at batch start' })),
            shared: recorded(Schema.Int).annotate({ description: 'Instances other processes ran before the run, left as they were' }),
            failed: Schema.optionalKey(Schema.String.annotate({ description: 'Why the run ended before every case ran, naming the instance left running' })),
            answered: Outcome.fields.answered.annotate({ description: 'Host dialogs the run expected and answered before the first case started' }),
            cases: Schema.Array(outcome),
            compare: Schema.optionalKey(Compare),
        }),
        { space: 2 },
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { Answer, Compare, deciding, Halted, Outcome, RESULTS, Records, recorded, results };
