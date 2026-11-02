import { Effect, Schema } from 'effect';
import { JsonObject } from '#/values';

// --- [MODELS] --------------------------------------------------------------------------

const recorded = <S extends Schema.Top>(item: S) => Schema.Array(item).pipe(Schema.withConstructorDefault(Effect.succeed([])));
const Answered = Schema.Struct({ title: Schema.optionalKey(Schema.String), text: Schema.Array(Schema.String), pressed: Schema.optionalKey(Schema.String) }).annotate({ description: 'Host dialog the watcher answered or recorded while the case ran' });
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
}).annotate({ description: 'Exit of the instance while the case ran' });
const Hang = Schema.Struct({ cpu: Schema.Literals(['busy', 'blocked']), sample: Schema.String.annotate({ description: 'sample report path of the instance' }) }).annotate({ description: 'Case past the limit with no record, the instance sampled then ended' });

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
        ...fields,
    });
const results = <E extends Schema.Top, H extends Schema.Struct.Fields, F extends Schema.Struct.Fields>(engine: E, header: H, fields: F) =>
    Schema.fromJsonString(
        Schema.Struct({
            application: Schema.String.annotate({ description: 'Name the host reports' }),
            identifier: Schema.String.annotate({ description: 'Bundle identifier of the installation' }),
            version: Schema.String.annotate({ description: 'Version the host reports' }),
            engine,
            ...header,
            answered: recorded(Answered).annotate({ description: 'Host dialogs shown before the first case started' }),
            cases: Schema.Array(outcome(fields)),
        }),
        { space: 2 },
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { Answer, Answered, Completion, Crash, Hang, outcome, recorded, results };
