import { createServer } from 'node:http';
import { NodeHttpServer, NodeRuntime, NodeServices } from '@effect/platform-node';
import { Cause, Effect, Exit, flow, Layer, Logger, Runtime, Schema } from 'effect';
import { McpServer } from 'effect/ai';
import { Command, Flag } from 'effect/cli';
import { FetchHttpClient, HttpRouter } from 'effect/http';
import { Otlp, OtlpSerialization } from 'effect/observability';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';

// --- [COMPOSITION] ---------------------------------------------------------------------

const observability = ({ name, version }: { readonly name: string; readonly version: string }): Layer.Layer<never> =>
    Otlp.layerFromConfig({ resource: { serviceName: name, serviceVersion: version } }).pipe(Layer.provide([FetchHttpClient.layer, OtlpSerialization.layerProtobuf]), Layer.provideMerge(Logger.layer([Logger.consoleJson])));
const main = (server: Omit<Parameters<typeof McpServer.layerHttp>[0], 'path'>, endpoint: Layer.Layer<never, unknown, McpServer.McpServer | HttpRouter.HttpRouter | HttpRouter.Request<'Error', unknown> | NodeServices.NodeServices>, observer: Layer.Layer<never>): void => {
    const owned = (command: ChildProcess.Command): ChildProcess.Command => (command._tag === 'StandardCommand' ? ChildProcess.make(command.command, command.args, { ...command.options, detached: false, killSignal: 'SIGKILL' }) : ChildProcess.pipeTo(owned(command.left), owned(command.right), command.options));
    Command.make(server.name, { port: Flag.Int('port').pipe(Flag.withSchema(Schema.Int.check(Schema.isBetween({ minimum: 1, maximum: 65_535 }))), Flag.withDescription('Loopback port the Streamable HTTP endpoint listens on')) }, ({ port }) =>
        HttpRouter.serve(endpoint.pipe(Layer.provide(McpServer.layerHttp({ ...server, path: '/' }))), { disableLogger: true }).pipe(Layer.provide(NodeHttpServer.layerServer(createServer, { host: '127.0.0.1', port, disablePreemptiveShutdown: true })), Layer.launch),
    ).pipe(
        Command.run({ version: server.version }),
        Effect.updateService(ChildProcessSpawner.ChildProcessSpawner, ({ spawn }) => ChildProcessSpawner.make(flow(owned, spawn))),
        Effect.provide([NodeServices.layer, observer]),
        NodeRuntime.runMain({ teardown: (exit, onExit) => (Exit.isFailure(exit) && Cause.hasInterruptsOnly(exit.cause) ? onExit(0) : Runtime.defaultTeardown(exit, onExit)) }),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { main, observability };
