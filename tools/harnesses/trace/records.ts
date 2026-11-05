import { Pressure } from '@rasm/applications';
import { Effect, type LogLevel, Match, Option, type Path, Predicate, type Record, Result, Schema, Struct } from 'effect';
import { Halted } from '#/cases/results';
import { HarnessError } from '#/harness-error';

// --- [CONSTANTS] -----------------------------------------------------------------------

const STATES = { launching: 'Info', responding: 'Info', unresponsive: 'Info', failed: 'Warn', exited: 'Info' } as const satisfies Record.ReadonlyRecord<string, LogLevel.Severity>;
const RECORDS = 'records.jsonl';

// --- [MODELS] --------------------------------------------------------------------------

const Kind = Schema.Literals(['crash', 'hang', 'spin', 'shutdown', 'recovery', 'host']).annotate({
    description: "System report or host log kind an instance leaves behind: ReportCrash and vendor crash reports, hang and resource reports, spindump and force-quit log lines, protective-shutdown entries, recovery files, and the host's own session logs",
});
const Field = {
    pid: Schema.optionalKey(Schema.Int),
    started: Schema.optionalKey(Schema.String),
    version: Schema.optionalKey(Schema.String),
    proof: Schema.optionalKey(Schema.String),
    case: Schema.optionalKey(Schema.String),
    tag: Schema.optionalKey(Schema.String),
    completion: Schema.optionalKey(Schema.String),
    cases: Schema.optionalKey(Schema.Array(Schema.String)),
    kind: Schema.optionalKey(Schema.String),
    state: Schema.optionalKey(Schema.String),
    file: Schema.optionalKey(Schema.String),
    title: Schema.optionalKey(Schema.String),
    frames: Schema.optionalKey(Schema.Array(Schema.String)),
};
const scoped = Struct.pick(Field, ['pid', 'started', 'case']);
const grouped = Struct.pick(Field, ['pid', 'started', 'version', 'proof', 'case']);
const sighting = Schema.Struct({
    ...scoped,
    bundle: Schema.optionalKey(Schema.String),
    number: Schema.optionalKey(Schema.Int),
    kind: Schema.optionalKey(Schema.String),
    role: Schema.optionalKey(Schema.String),
    subrole: Schema.optionalKey(Schema.String),
    title: Field.title,
    document: Schema.optionalKey(Schema.String),
    edited: Schema.optionalKey(Schema.Boolean),
    modal: Schema.optionalKey(Schema.Boolean),
    identifier: Schema.optionalKey(Schema.String),
    asking: Schema.optionalKey(Schema.Boolean),
    text: Schema.optionalKey(Schema.Array(Schema.String)),
    controls: Schema.optionalKey(Schema.Array(Schema.Json)),
});
const Messages = {
    'run started': Schema.Struct({ commit: Schema.String, args: Schema.Array(Schema.String), session: Schema.NullOr(Schema.String) }),
    'instance launched': Schema.Struct({ ...scoped, app: Schema.String, created: Schema.Boolean, state: Schema.Literals(Struct.keys(STATES)), shared: Schema.optionalKey(Schema.NonEmptyArray(Schema.Int)) }),
    instance: Schema.Struct({
        ...scoped,
        bundle: Schema.optionalKey(Schema.String),
        state: Schema.Literals(Struct.keys(STATES)),
        ...Struct.pick(Halted.cases.Hung.fields, ['process', 'cpu', 'footprint', 'threads']),
        duration: Schema.optionalKey(Schema.Finite.annotate({ description: 'Seconds from the unresponsive state opening the spell to the state closing it, the stall having begun at least one heartbeat earlier' })),
        reason: Schema.optionalKey(Schema.String),
        status: Schema.optionalKey(Schema.Int),
        signal: Schema.optionalKey(Schema.Int),
        detail: Schema.optionalKey(Schema.Array(Schema.String)),
        statusFailure: Schema.optionalKey(Schema.String),
        report: Schema.optionalKey(Schema.String),
        exception: Schema.optionalKey(Schema.String),
        reportFailure: Schema.optionalKey(Schema.String),
    }),
    'window opened': sighting,
    'window changed': sighting,
    'window closed': sighting,
    'window busy': Schema.Struct({ ...scoped, number: Schema.Int, sample: Halted.cases.Hung.fields.sample, frames: Halted.cases.Hung.fields.frames }),
    'plugin loaded': Schema.Struct({ plugin: Schema.String, folder: Schema.String, port: Schema.Int }),
    'plugin linked': Schema.Struct({ plugin: Schema.String }),
    'dialog answered': Schema.Struct({ ...scoped, title: Field.title, text: Schema.Array(Schema.String), pressed: Schema.String }),
    'system pressure': Pressure,
    'case started': Schema.Struct({ ...grouped, case: Schema.String, call: Schema.Struct({ _tag: Schema.String, fn: Schema.optionalKey(Schema.String), script: Schema.optionalKey(Schema.String), args: Schema.optionalKey(Schema.Array(Schema.Json)) }) }),
    'case ended': Schema.StructWithRest(Schema.Struct({ ...grouped, case: Schema.String, completion: Schema.String, tag: Field.tag, error: Schema.optionalKey(Schema.String) }), [Schema.Record(Schema.String, Schema.Unknown)]),
    'cases not run': Schema.Struct({ ...grouped, cases: Schema.Array(Schema.String), error: Schema.String }),
    'report linked': Schema.Struct({
        ...scoped,
        kind: Kind,
        file: Field.file,
        event: Schema.optionalKey(Schema.String.annotate({ description: 'Event the spindump report names: hang, cpu resource, or disk writes' })),
        exception: Schema.optionalKey(Schema.String.annotate({ description: 'Mach exception type and signal the crash report names' })),
        reason: Schema.optionalKey(Schema.String.annotate({ description: 'Jetsam reason the memory report names for the pid' })),
        frames: Field.frames,
        duration: Schema.optionalKey(Schema.Finite.annotate({ description: 'Seconds the hang report states the process was unresponsive' })),
        lines: Schema.optionalKey(Schema.NonEmptyArray(Schema.String).annotate({ description: 'Lines of the file or log the kind keeps for the instance' })),
    }),
    'quit sent': Schema.Struct(scoped),
    failed: Schema.StructWithRest(Schema.Struct({ ...grouped, tag: Schema.String, error: Schema.String }), [Schema.Record(Schema.String, Schema.Unknown)]),
    compared: Schema.Struct({ ...grouped, module: Schema.String, agreed: Schema.Int, mismatched: Schema.Array(Schema.String), unproven: Schema.Array(Schema.String), tag: Field.tag }),
    'results written': Schema.Struct({ ...grouped, file: Schema.String }),
    'run ended': Schema.Struct({}),
};
const Message = Schema.Literals(Struct.keys(Messages));
const Signed = Schema.Struct({ ...Struct.pick(Field, ['version', 'proof', 'case']), phase: Schema.String, tag: Schema.String }).annotate({ description: 'Failure signature: where a run failed and what tag it failed with' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const record = <M extends keyof typeof Messages>(level: LogLevel.Severity, message: M, fields: (typeof Messages)[M]['Encoded']) => Effect.annotateLogs(Effect.logWithLevel(level)(message), fields);
const declared = (message: string, fields: Record.ReadonlyRecord<string, unknown>): Record.ReadonlyRecord<string, unknown> =>
    Option.match(Schema.decodeUnknownOption(Message)(message), {
        onNone: () => fields,
        onSome: (held) => {
            const schema: Schema.Decoder<Record.ReadonlyRecord<string, unknown>> = Messages[held];
            return Result.getOrThrow(Schema.decodeResult(schema)(fields));
        },
    });
const tagOf = Match.type<unknown>().pipe(
    Match.when(Schema.is(HarnessError), ({ reason }) => reason._tag),
    Match.when(Predicate.isError, ({ name }) => name),
    Match.orElse(() => 'Defect'),
);
const messageOf = Match.type<unknown>().pipe(
    Match.when(Predicate.isError, ({ message }) => message),
    Match.orElse(String),
);
const partition = (path: Path.Path, logs: string, host: string, run: string) => path.join(logs, `host=${host}`, `run=${run}`);

// --- [EXPORTS] -------------------------------------------------------------------------

export { declared, Field, Kind, Messages, messageOf, partition, RECORDS, record, Signed, STATES, tagOf };
