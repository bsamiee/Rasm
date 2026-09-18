// --- [IMPORTS] -------------------------------------------------------------------------

import { Schema } from 'effect';
import { Rpc, RpcGroup, type RpcSchema } from 'effect/unstable/rpc';
import type { SocketServer } from 'effect/unstable/socket';
import { HostRejection } from './errors.ts';
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
export { AlreadyAttached, Execute, Frames, HistoryState, Job, Link };
