import { Unmatched } from '@rasm/search';
import { Duration, Effect, type Option, Schedule, Schema, SchemaGetter, Stream } from 'effect';
import { Rpc, RpcClient, RpcGroup, RpcMiddleware, RpcSerialization } from 'effect/rpc';
import { FindChange, Queries } from '../find.ts';
import { Font } from '../fonts.ts';
import { Inspect, Kind } from '../inspection.ts';
import { Change, Create, Remove } from '../mutations.ts';
import { Preflight } from '../preflight.ts';
import { Render } from '../render.ts';
import { Script } from '../scripts.ts';
import { Invoke, Menu } from '../session.ts';
import { Phase } from '../transactions.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PATH = '/indesign';

// --- [MODELS] --------------------------------------------------------------------------

const Peer = Schema.Struct({
    application: Schema.URLFromString.pipe(Schema.decodeTo(Schema.URL, { decode: SchemaGetter.transform((url) => (url.pathname.endsWith('/') ? url : new URL(`${url.href}/`))), encode: SchemaGetter.passthrough() })).annotate({
        description: 'Bundle url of the application the panel serves, in the directory form Launch Services gives a bundle',
    }),
    extension: Schema.String.annotate({ description: 'Plugin name its manifest declares' }),
});
const Failure = Schema.Union([Unmatched, Schema.ErrorInstance()]);

// --- [SERVICES] ------------------------------------------------------------------------

class Hold extends RpcMiddleware.Service<Hold>()('@rasm/indesign/service/contract/Hold', { error: Schema.ErrorInstance(), requiredForClient: true }) {
    static readonly client = (report: (phase: Option.Option<typeof Phase.Type>) => Effect.Effect<void>) =>
        RpcMiddleware.layerClient(
            Hold,
            Effect.map(
                RpcClient.make(RpcGroup.make(Enter), { disableTracing: true, flatten: true }),
                (enter) =>
                    ({ next, request }) =>
                        Effect.tap(next(request), () =>
                            Stream.fromSchedule(Schedule.min([Schedule.exponential(Duration.millis(2)), Schedule.spaced(Duration.seconds(1))])).pipe(
                                Stream.mapEffect(() => Effect.timeoutOption(enter('enter', {}), Duration.seconds(2)), { concurrency: 'unbounded', unordered: true }),
                                Stream.changes,
                                Stream.runForEach(report),
                            ),
                        ),
            ),
        );
}

// --- [COMPOSITION] ---------------------------------------------------------------------

const WIRE = RpcSerialization.layerSchemaBinary({ fingerprintPayloads: true });
const Enter = Rpc.make('enter', { payload: {}, success: Phase });
const InDesign = RpcGroup.make(
    Rpc.make('inspect', { payload: Inspect, success: Inspect.Result, error: Failure }),
    Rpc.make('change', { payload: Change, success: Change.Result, error: Failure }),
    Rpc.make('create', { payload: Create, success: Create.Result, error: Failure }),
    Rpc.make('remove', { payload: Remove, success: Remove.Result, error: Failure }),
    Rpc.make('execute_script', { payload: Script, success: Script.Result, error: Failure }),
    Rpc.make('find_change', { payload: FindChange, success: FindChange.Result, error: Failure }),
    Rpc.make('font', { payload: Font, success: Font.Result, error: Failure }),
    Rpc.make('kinds', { payload: Schema.Struct({}).annotate({ description: 'Every native class and enumeration name in the host dictionary' }), success: Schema.Array(Schema.String), error: Failure }),
    Rpc.make('kind', { payload: Kind, success: Kind.Result, error: Failure }),
    Rpc.make('render', { payload: Render, success: Render.Result, error: Failure }),
    Rpc.make('menu', { payload: Menu, success: Menu.Result, error: Failure }),
    Rpc.make('invoke', { payload: Invoke, success: Invoke.Result, error: Failure }),
    Rpc.make('preflight', { payload: Preflight, success: Preflight.Result, error: Failure }),
    Rpc.make('queries', { payload: Queries, success: Queries.Result, error: Failure }),
)
    .middleware(Hold)
    .add(Enter, Rpc.make('changed', { payload: {}, success: Preflight.Summary.value, stream: true }));

// --- [EXPORTS] -------------------------------------------------------------------------

export { Hold, InDesign, PATH, Peer, WIRE };
