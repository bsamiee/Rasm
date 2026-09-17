// --- [IMPORTS] -------------------------------------------------------------------------

import { NodeRuntime, NodeServices, NodeSocketServer } from '@effect/platform-node';
import { Cause, Effect, Exit, Layer, Logger, Record, Ref, Runtime, Struct } from 'effect';
import { McpProtocol, McpServer } from 'effect/unstable/ai';
import type { SocketServer } from 'effect/unstable/socket';
import { layer as acrobat } from './acrobat/tools.ts';
import { layer as health } from './health.ts';
import { Hosts, resolve } from './hosts.ts';
import { layer as indesign } from './indesign/tools.ts';
import { layer as jobs } from './jobs.ts';
import manifest from './package.json' with { type: 'json' };
import { type Endpoint, type Link, Links, type SocketHost, serve } from './socket.ts';
import { HOSTS } from './values.ts';

// --- [ASSEMBLY] ------------------------------------------------------------------------

const _server = { name: manifest.name, version: manifest.version };

const _SOCKETS = Struct.pick(HOSTS, ['photoshop', 'indesign']);

const _hosts = Layer.effect(Hosts, resolve);

const _endpoint = (host: SocketHost): Effect.Effect<Endpoint> => Effect.map(Ref.make<Link>({ _tag: 'listening' }), (link) => ({ host, link }));

const _listener = (row: (typeof _SOCKETS)[SocketHost]): Layer.Layer<never, SocketServer.SocketServerError, Links> =>
    Layer.provide(serve(row.id), NodeSocketServer.layerWebSocket({ host: '127.0.0.1', port: row.port }));

// --- [ENTRY] ---------------------------------------------------------------------------

Layer.launch(
    Layer.provideMerge(
        McpServer.layerStdio({ ..._server, protocols: [McpProtocol.v2025_11_25] }),
        Layer.mergeAll(health(_server), acrobat, indesign, ...Record.values(Record.map(_SOCKETS, _listener))),
    ).pipe(Layer.provide(Layer.mergeAll(_hosts, Layer.effect(Links, Effect.all(Record.map(_SOCKETS, (row) => _endpoint(row.id)))), Layer.provide(jobs, _hosts)))),
).pipe(
    Effect.tapCause((cause) => (Cause.hasInterruptsOnly(cause) ? Effect.void : Effect.logError(cause))),
    Effect.provide(Layer.mergeAll(NodeServices.layer, Logger.layer([Logger.withConsoleError(Logger.formatJson)]))),
    NodeRuntime.runMain({
        disableErrorReporting: true,
        teardown: (exit, onExit) => (Exit.isFailure(exit) && Cause.hasInterruptsOnly(exit.cause) ? onExit(0) : Runtime.defaultTeardown(exit, onExit)),
    }),
);
