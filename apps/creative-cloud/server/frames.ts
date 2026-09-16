// --- [IMPORTS] -------------------------------------------------------------------------

import { Schema } from 'effect';
import { Rpc, RpcGroup, type RpcSchema } from 'effect/unstable/rpc';
import { HostRejection } from './errors.ts';
import { JobId } from './values.ts';

// --- [MODELS] --------------------------------------------------------------------------

const _optionalString = Schema.OptionFromOptionalKey(Schema.String);

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
    app: _optionalString,
    dom: _optionalString,
});

const State: Schema.Struct<{ readonly modalState: Schema.Boolean; readonly activeDocumentId: Schema.OptionFromNullOr<Schema.Int>; readonly modified: Schema.Boolean }> = Schema.Struct({
    modalState: Schema.Boolean,
    activeDocumentId: Schema.OptionFromNullOr(Schema.Int),
    modified: Schema.Boolean,
});

const Job: Schema.Struct<{
    readonly jobId: Schema.Codec<JobId, string>;
    readonly kind: Schema.Literals<readonly ['execute']>;
    readonly body: Schema.Codec<Schema.Json>;
    readonly suspendHistory: Schema.OptionFromNullOr<Schema.Struct<{ readonly documentId: Schema.Int; readonly name: Schema.String }>>;
    readonly commandName: Schema.OptionFromNullOr<Schema.String>;
}> = Schema.Struct({
    jobId: JobId,
    kind: Schema.Literals(['execute']),
    body: Schema.Json,
    suspendHistory: Schema.OptionFromNullOr(Schema.Struct({ documentId: Schema.Int, name: Schema.String })),
    commandName: Schema.OptionFromNullOr(Schema.String),
});

const Done: Schema.Struct<{ readonly value: Schema.Codec<Schema.Json>; readonly autocorrections: Schema.OptionFromOptionalKey<Schema.$Array<Schema.String>> }> = Schema.Struct({
    value: Schema.Json,
    autocorrections: Schema.OptionFromOptionalKey(Schema.Array(Schema.String)),
});

const AlreadyAttached: Schema.TaggedStruct<'alreadyAttached', Record<never, never>> = Schema.TaggedStruct('alreadyAttached', {});

const Link: Schema.Union<
    readonly [Schema.TaggedStruct<'listening', Record<never, never>>, Schema.TaggedStruct<'attached', { readonly identity: typeof Identity; readonly state: Schema.OptionFromNullOr<typeof State> }>]
> = Schema.Union([Schema.TaggedStruct('listening', {}), Schema.TaggedStruct('attached', { identity: Identity, state: Schema.OptionFromNullOr(State) })]);

// --- [CONTRACT] ------------------------------------------------------------------------

const Frames: RpcGroup.RpcGroup<
    | Rpc.Rpc<'attach', typeof Identity, RpcSchema.Stream<typeof Job, typeof AlreadyAttached>, Schema.Never>
    | Rpc.Rpc<'settle', Schema.Struct<{ readonly jobId: Schema.Codec<JobId, string>; readonly exit: Schema.Result<typeof Done, Schema.Codec<HostRejection, unknown>> }>>
    | Rpc.Rpc<'state', typeof State>
> = RpcGroup.make(
    Rpc.make('attach', { payload: Identity, success: Job, error: AlreadyAttached, stream: true }),
    Rpc.make('settle', { payload: { jobId: JobId, exit: Schema.Result(Done, HostRejection) } }),
    Rpc.make('state', { payload: State }),
);

// --- [TYPES] ---------------------------------------------------------------------------

type Identity = (typeof Identity)['Type'];
type State = (typeof State)['Type'];
type Job = (typeof Job)['Type'];
type JobKind = Job['kind'];
type Done = (typeof Done)['Type'];
type AlreadyAttached = (typeof AlreadyAttached)['Type'];
type Link = (typeof Link)['Type'];

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Done, Identity, Job, JobKind, State };
export { AlreadyAttached, Frames, Link };
