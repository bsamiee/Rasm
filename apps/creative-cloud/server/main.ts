// --- [IMPORTS] -------------------------------------------------------------------------

import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { Cause, Effect, Exit, Layer, Logger, Runtime, Struct } from 'effect';
import { McpProtocol, McpServer } from 'effect/unstable/ai';
import { layer as acrobat } from './acrobat/tools.ts';
import { faulted } from './errors.ts';
import { layer as health, SERVER } from './health.ts';
import { layer as indesign } from './indesign/tools.ts';
import { layer as photoshop } from './photoshop/tools.ts';
import { layer as bridge } from './socket.ts';
import { SOCKETS } from './values.ts';

// --- [ENTRY] ---------------------------------------------------------------------------

Layer.launch(
    Layer.provideMerge(McpServer.layerStdio({ ...SERVER, protocols: [McpProtocol.v2025_11_25] }), Layer.mergeAll(health, acrobat, indesign, photoshop)).pipe(
        Layer.provide(bridge(Struct.keys(SOCKETS))),
    ),
).pipe(
    Effect.tapCause(faulted),
    Effect.provide(Layer.mergeAll(NodeServices.layer, Logger.layer([Logger.withConsoleError(Logger.formatJson)]))),
    NodeRuntime.runMain({
        disableErrorReporting: true,
        teardown: (exit, onExit) => (Exit.isFailure(exit) && Cause.hasInterruptsOnly(exit.cause) ? onExit(0) : Runtime.defaultTeardown(exit, onExit)),
    }),
);
