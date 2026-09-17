// --- [IMPORTS] -------------------------------------------------------------------------

import { Schema } from 'effect';
import { Rpc, RpcGroup, type RpcSchema } from 'effect/unstable/rpc';
import { HostRejection } from './errors.ts';
import { Autocorrections, JobId } from './values.ts';

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

const HistoryState: Schema.Struct<{ readonly documentId: Schema.Int; readonly name: Schema.String }> = Schema.Struct({ documentId: Schema.Int, name: Schema.String });

const Job: Schema.Struct<{
    readonly jobId: Schema.Codec<JobId, string>;
    readonly kind: Schema.Literals<
        readonly ['execute', 'listEnums', 'snapshot', 'getLayout', 'findKeyStrings', 'getPreferences', 'setPreferences', 'setTextDefaults', 'batchPlay', 'getDocument', 'listPresets', 'runAction']
    >;
    readonly body: Schema.Codec<Schema.Json>;
    readonly suspendHistory: Schema.OptionFromOptionalKey<typeof HistoryState>;
    readonly commandName: Schema.OptionFromOptionalKey<Schema.String>;
}> = Schema.Struct({
    jobId: JobId,
    kind: Schema.Literals([
        'execute',
        'listEnums',
        'snapshot',
        'getLayout',
        'findKeyStrings',
        'getPreferences',
        'setPreferences',
        'setTextDefaults',
        'batchPlay',
        'getDocument',
        'listPresets',
        'runAction',
    ]),
    body: Schema.Json,
    suspendHistory: Schema.OptionFromOptionalKey(HistoryState),
    commandName: _optionalString,
});

const Settle: Schema.Struct<{
    readonly jobId: Schema.Codec<JobId, string>;
    readonly autocorrections: typeof Autocorrections;
    readonly result: Schema.Result<Schema.Codec<Schema.Json>, typeof HostRejection>;
}> = Schema.Struct({ jobId: JobId, autocorrections: Autocorrections, result: Schema.Result(Schema.Json, HostRejection) });

const Execute: Schema.Struct<{ readonly code: Schema.String; readonly undoName: Schema.OptionFromOptionalKey<Schema.String> }> = Schema.Struct({ code: Schema.String, undoName: _optionalString });

const AlreadyAttached: Schema.TaggedStruct<'alreadyAttached', Record<never, never>> = Schema.TaggedStruct('alreadyAttached', {});

const Link: Schema.Union<
    readonly [Schema.TaggedStruct<'listening', Record<never, never>>, Schema.TaggedStruct<'attached', { readonly identity: typeof Identity; readonly state: Schema.OptionFromNullOr<typeof State> }>]
> = Schema.Union([Schema.TaggedStruct('listening', {}), Schema.TaggedStruct('attached', { identity: Identity, state: Schema.OptionFromNullOr(State) })]);

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

interface Done {
    readonly value: Schema.Json;
    readonly autocorrections: Autocorrections;
}

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Done, Identity, Job, Settle, State };
export { AlreadyAttached, Execute, Frames, HistoryState, Link };
