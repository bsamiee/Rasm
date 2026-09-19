// --- [IMPORTS] -------------------------------------------------------------------------

import { Schema } from 'effect';
import { Rpc, RpcGroup, type RpcSchema } from 'effect/unstable/rpc';
import type { SocketServer } from 'effect/unstable/socket';
import { BridgeError, HostRejection } from './errors.ts';
import { Autocorrections, type Closed, JobId, OptionalString } from './values.ts';

// --- [TABLES] --------------------------------------------------------------------------

const _UNBOUND = ['SocketServerOpenError', 'SocketServerUnknownError'] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Identity: Schema.Struct<{
    readonly plugin: Schema.String;
    readonly version: Schema.String;
    readonly host: Schema.Struct<{ readonly name: Schema.String; readonly version: Schema.String }>;
    readonly uxp: Schema.String;
    readonly app: Schema.OptionFromOptionalKey<Schema.String>;
    readonly dom: Schema.OptionFromOptionalKey<Schema.String>;
}> = Schema.Struct({
    plugin: Schema.String,
    version: Schema.String,
    host: Schema.Struct({ name: Schema.String, version: Schema.String }),
    uxp: Schema.String,
    app: OptionalString,
    dom: OptionalString,
});

const State: Schema.Struct<{ readonly modalState: Schema.Boolean; readonly activeDocumentId: Schema.OptionFromNullOr<Schema.Int>; readonly modified: Schema.Boolean }> = Schema.Struct({
    modalState: Schema.Boolean,
    activeDocumentId: Schema.OptionFromNullOr(Schema.Int),
    modified: Schema.Boolean,
});

const HistoryState: Schema.Struct<{ readonly documentId: Schema.Int; readonly name: Schema.String }> = Schema.Struct({ documentId: Schema.Int, name: Schema.String });

const Job: Schema.Struct<{
    readonly jobId: Schema.Codec<JobId, string>;
    readonly kind: Schema.String;
    readonly body: Schema.Codec<Schema.Json>;
    readonly suspendHistory: Schema.OptionFromOptionalKey<typeof HistoryState>;
    readonly commandName: Schema.OptionFromOptionalKey<Schema.String>;
}> = Schema.Struct({
    jobId: JobId,
    kind: Schema.String,
    body: Schema.Json,
    suspendHistory: Schema.OptionFromOptionalKey(HistoryState),
    commandName: OptionalString,
});

const Settle: Schema.Struct<{
    readonly jobId: Schema.Codec<JobId, string>;
    readonly autocorrections: typeof Autocorrections;
    readonly result: Schema.Result<Schema.Codec<Schema.Json>, typeof HostRejection>;
}> = Schema.Struct({ jobId: JobId, autocorrections: Autocorrections, result: Schema.Result(Schema.Json, HostRejection) });

const Execute: Schema.Struct<{ readonly code: Schema.String; readonly undoName: Schema.OptionFromOptionalKey<Schema.String> }> = Schema.Struct({
    code: Schema.String,
    undoName: OptionalString,
});

const AlreadyAttached: Schema.TaggedStruct<'alreadyAttached', Record<never, never>> = Schema.TaggedStruct('alreadyAttached', {});

const Link: Schema.TaggedUnion<{
    readonly unbound: Schema.TaggedStruct<
        'unbound',
        { readonly port: Schema.Int; readonly reason: Schema.Literals<Closed<SocketServer.SocketServerErrorReason['_tag'], typeof _UNBOUND>>; readonly cause: Schema.Defect }
    >;
    readonly listening: Schema.TaggedStruct<'listening', Record<never, never>>;
    readonly attached: Schema.TaggedStruct<'attached', { readonly identity: typeof Identity; readonly state: Schema.OptionFromNullOr<typeof State> }>;
}> = Schema.TaggedUnion({
    unbound: { port: Schema.Int, reason: Schema.Literals(_UNBOUND), cause: Schema.Defect() },
    listening: {},
    attached: { identity: Identity, state: Schema.OptionFromNullOr(State) },
});

// --- [CONTRACT] ------------------------------------------------------------------------

const Frames: RpcGroup.RpcGroup<
    Rpc.Rpc<'attach', typeof Identity, RpcSchema.Stream<typeof Job, typeof AlreadyAttached>, Schema.Never> | Rpc.Rpc<'settle', typeof Settle> | Rpc.Rpc<'state', typeof State>
> = RpcGroup.make(Rpc.make('attach', { payload: Identity, success: Job, error: AlreadyAttached, stream: true }), Rpc.make('settle', { payload: Settle }), Rpc.make('state', { payload: State }));

const Completion: Schema.Struct<{ readonly value: Schema.Codec<Schema.Json>; readonly autocorrections: typeof Autocorrections }> = Schema.Struct({
    value: Schema.Json,
    autocorrections: Autocorrections,
});

const Outcome: Schema.TaggedUnion<{
    readonly unknown: Schema.TaggedStruct<'unknown', Record<never, never>>;
    readonly pending: Schema.TaggedStruct<'pending', Record<never, never>>;
    readonly settled: Schema.TaggedStruct<'settled', { readonly result: Schema.Result<typeof Completion, typeof BridgeError> }>;
}> = Schema.TaggedUnion({ unknown: {}, pending: {}, settled: { result: Schema.Result(Completion, BridgeError) } });

const InFlight: Schema.Struct<{ readonly jobId: typeof JobId; readonly startedAt: Schema.Number }> = Schema.Struct({ jobId: JobId, startedAt: Schema.Number });

const _inFlight = Schema.OptionFromNullOr(InFlight);

const Activity: Schema.Struct<{
    readonly queueDepth: Schema.Int;
    readonly inFlight: Schema.OptionFromNullOr<typeof InFlight>;
    readonly wedged: Schema.OptionFromNullOr<typeof InFlight>;
    readonly lastSuccessAt: Schema.OptionFromNullOr<Schema.Number>;
    readonly lastError: Schema.OptionFromNullOr<Schema.Struct<{ readonly error: typeof BridgeError; readonly count: Schema.Int; readonly at: Schema.Number }>>;
}> = Schema.Struct({
    queueDepth: Schema.Int,
    inFlight: _inFlight,
    wedged: _inFlight,
    lastSuccessAt: Schema.OptionFromNullOr(Schema.Number),
    lastError: Schema.OptionFromNullOr(Schema.Struct({ error: BridgeError, count: Schema.Int, at: Schema.Number })),
});

const Observation: Schema.Struct<{ readonly link: typeof Link; readonly activity: typeof Activity }> = Schema.Struct({ link: Link, activity: Activity });

const Request: Schema.Struct<{ readonly jobId: typeof JobId; readonly deadlineAt: Schema.Finite }> = Schema.Struct({ jobId: JobId, deadlineAt: Schema.Finite });

const Submission: Schema.Struct<{ readonly job: typeof Job; readonly deadlineAt: Schema.Finite }> = Schema.Struct({ job: Job, deadlineAt: Request.fields.deadlineAt });

const Probe: Schema.Struct<typeof Request.fields & { readonly statement: typeof OptionalString }> = Schema.Struct({ ...Request.fields, statement: OptionalString });

const Broker: RpcGroup.RpcGroup<
    | Rpc.Rpc<'submit', typeof Submission, typeof Completion, typeof BridgeError>
    | Rpc.Rpc<'probe', typeof Probe, Schema.Result<Schema.Codec<Schema.Json>, typeof BridgeError>, Schema.Never>
    | Rpc.Rpc<'outcome', typeof JobId, typeof Outcome, typeof BridgeError>
    | Rpc.Rpc<'observe', Schema.Void, RpcSchema.Stream<typeof Observation, Schema.Never>, Schema.Never>
> = RpcGroup.make(
    Rpc.make('submit', { payload: Submission, success: Completion, error: BridgeError }),
    Rpc.make('probe', { payload: Probe, success: Schema.Result(Schema.Json, BridgeError) }),
    Rpc.make('outcome', { payload: JobId, success: Outcome, error: BridgeError }),
    Rpc.make('observe', { success: Observation, stream: true }),
);

// --- [TYPES] ---------------------------------------------------------------------------

type Identity = (typeof Identity)['Type'];
type State = (typeof State)['Type'];
type Job = (typeof Job)['Type'];
type Execute = (typeof Execute)['Type'];
type Settle = (typeof Settle)['Type'];
type AlreadyAttached = (typeof AlreadyAttached)['Type'];
type Link = (typeof Link)['Type'];

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Identity, Settle, State };
export { Activity, AlreadyAttached, Broker, Completion, Execute, Frames, HistoryState, InFlight, Job, Link, Outcome, Request };
