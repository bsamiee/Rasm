import { Unmatched } from '@rasm/search';
import { Peer } from '@rasm/uxp/link';
import { ORIGIN } from '@rasm/uxp/runtime';
import { Duration, Effect, Option, Schedule, Schema, Stream } from 'effect';
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

// --- [MODELS] --------------------------------------------------------------------------

const Failure = Schema.Union([Unmatched, Schema.ErrorInstance()]);

// --- [SERVICES] ------------------------------------------------------------------------

class Hold extends RpcMiddleware.Service<Hold>()('@rasm/indesign/service/contract/Hold', { error: Schema.ErrorInstance(), requiredForClient: true }) {
    static readonly client = (report: (message: string) => Effect.Effect<void>) =>
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
                                Stream.runForEach(
                                    Option.match({
                                        onNone: () => report(`${request.tag} is waiting for InDesign; an open menu, dialog, or drag holds it`),
                                        onSome: Phase.match({ Idle: () => Effect.void, Entered: ({ label }) => report(label) }),
                                    }),
                                ),
                            ),
                        ),
            ),
        );
}

// --- [COMPOSITION] ---------------------------------------------------------------------

const WIRE = RpcSerialization.layerSchemaBinary({ fingerprintPayloads: true });
const LINK = { path: '/indesign', origin: ORIGIN, peer: 'InDesign panel', identity: Peer, serialization: WIRE } as const;
const Enter = Rpc.make('enter', { payload: {}, success: Phase });
const InDesign = RpcGroup.make(
    Rpc.make('inspect', { payload: Inspect, success: Inspect.Result, error: Failure }),
    Rpc.make('change', { payload: Change, success: Change.Result, error: Failure }),
    Rpc.make('create', { payload: Create, success: Create.Result, error: Failure }),
    Rpc.make('remove', { payload: Remove, success: Remove.Result, error: Failure }),
    Rpc.make('execute_script', { payload: Script, success: Script.Result, error: Failure }),
    Rpc.make('find_change', { payload: FindChange, success: FindChange.Result, error: Failure }),
    Rpc.make('font', { payload: Font, success: Font.Result, error: Failure }),
    Rpc.make('kind', { payload: Kind, success: Kind.Result, error: Failure }),
    Rpc.make('render', { payload: Render, success: Render.Result, error: Failure }),
    Rpc.make('menu', { payload: Menu, success: Menu.Result, error: Failure }),
    Rpc.make('invoke', { payload: Invoke, success: Invoke.Result, error: Failure }),
    Rpc.make('preflight', { payload: Preflight, success: Preflight.Result, error: Failure }),
    Rpc.make('queries', { payload: Queries, success: Queries.Result, error: Failure }),
)
    .middleware(Hold)
    .add(Enter);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Hold, InDesign, LINK };
