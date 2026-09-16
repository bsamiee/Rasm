// --- [IMPORTS] -------------------------------------------------------------------------

import { NodeRuntime, NodeServices, NodeSocketServer } from '@effect/platform-node';
import { Cause, Console, Effect, Exit, flow, Layer, Logger, Option, Record, Ref, Runtime } from 'effect';
import { McpProtocol, McpServer } from 'effect/unstable/ai';
import type { SocketServer } from 'effect/unstable/socket';
import { layer as health } from './health.ts';
import { HOSTS, Hosts, resolve } from './hosts.ts';
import { type Activity, Jobs } from './jobs.ts';
import manifest from './package.json' with { type: 'json' };
import { type Endpoint, type Link, Links, type SocketHost, serve } from './socket.ts';

// --- [ASSEMBLY] ------------------------------------------------------------------------

const _endpoint = (host: SocketHost): Effect.Effect<Endpoint> => Effect.map(Ref.make<Link>({ _tag: 'listening' }), (link) => ({ host, link }));

const _listener = (host: SocketHost): Layer.Layer<never, SocketServer.SocketServerError, Links> =>
    Layer.provide(serve(host), NodeSocketServer.layerWebSocket({ host: '127.0.0.1', port: HOSTS[host].port }));

// --- [ENTRY] ---------------------------------------------------------------------------

Layer.launch(
    Layer.provideMerge(
        McpServer.layerStdio({ name: manifest.name, version: manifest.version, protocols: [McpProtocol.v2025_11_25] }),
        Layer.mergeAll(health, _listener('photoshop'), _listener('indesign')),
    ).pipe(
        Layer.provide(
            Layer.mergeAll(
                Layer.effect(Hosts, resolve),
                Layer.effect(Links, Effect.all({ photoshop: _endpoint('photoshop'), indesign: _endpoint('indesign') })),
                Layer.effect(Jobs, Effect.all(Record.map(HOSTS, () => Ref.make<Activity>({ inFlight: Option.none(), lastSuccessAt: Option.none(), lastError: Option.none() })))),
            ),
        ),
    ),
).pipe(
    Effect.tapError((error) => Console.error(Cause.pretty(Cause.fail(error)))),
    Effect.tapDefect(flow(Cause.die, Cause.pretty, Console.error)),
    Effect.provide(Layer.mergeAll(NodeServices.layer, Layer.succeed(Logger.LogToStderr, true))),
    NodeRuntime.runMain({
        disableErrorReporting: true,
        teardown: (exit, onExit) => (Exit.isFailure(exit) && Cause.hasInterruptsOnly(exit.cause) ? onExit(0) : Runtime.defaultTeardown(exit, onExit)),
    }),
);
