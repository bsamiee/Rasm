import { matchesGlob } from 'node:path';
import { env } from 'node:process';
import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { Client } from '@modelcontextprotocol/client';
import { StdioClientTransport } from '@modelcontextprotocol/client/stdio';
import { type Implementation, SdkError, SdkErrorCode, Server, type ServerCapabilities } from '@modelcontextprotocol/server';
import { Deferred, Effect, identity, Logger, Predicate, Record, Schema } from 'effect';
import { Argument, Command, Flag } from 'effect/cli';
import { acquireListenSubscriptions, getUpstreamBridge, proxyServer, startHTTPServer } from 'mcp-proxy';

// --- [CONSTANTS] -----------------------------------------------------------------------

const BRIDGE = { name: 'bridge', version: '0' };

// --- [MODELS] --------------------------------------------------------------------------

const Port = Schema.Number.check(Schema.isBetween({ minimum: 1, maximum: 65_535 }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const OPTIONS = {
    port: Flag.Int('port').pipe(Flag.withSchema(Port), Flag.withDescription('Loopback port the Streamable HTTP endpoint listens on')),
    hide: Flag.String('hide').pipe(Flag.atLeast(0), Flag.withDescription('Glob of server tool names left out of the endpoint, repeated per glob')),
    command: Argument.String('command').pipe(Argument.withDescription('Stdio MCP server to bridge, written after --')),
    args: Argument.String('argument').pipe(Argument.variadic()),
};
const bridge = Effect.fn('bridge')(function* ({ args, command, hide, port }: Command.Command.Config.Infer<typeof OPTIONS>) {
    const requestTimeout = 300_000;
    const closed = yield* Deferred.make<never, SdkError>();
    const client = new (class extends Client {
        override listTools(...options: Parameters<Client['listTools']>): ReturnType<Client['listTools']> {
            return Effect.tryPromise({ try: () => super.listTools(...options), catch: identity }).pipe(
                Effect.map((result) => ({ ...result, tools: result.tools.filter(({ name }) => !hide.some((glob) => matchesGlob(name, glob))) })),
                Effect.runPromise,
            );
        }
    })(BRIDGE);
    const report = (error: Error): void => {
        Effect.logError(error).pipe(Effect.runFork);
    };
    client.onerror = report;
    client.onclose = (): void => {
        Deferred.doneUnsafe(closed, Effect.fail(new SdkError(SdkErrorCode.ConnectionClosed, 'Upstream MCP connection closed')));
    };
    yield* Effect.addFinalizer(() => Effect.promise(() => client.close()));
    yield* Effect.tryPromise(() => client.connect(new StdioClientTransport({ command, args: [...args], env: Record.filter(env, Predicate.isString) })));
    const serverInfo = client.getServerVersion() as Implementation;
    const serverCapabilities = client.getServerCapabilities() as ServerCapabilities;
    const instructions = client.getInstructions();
    const upstream = getUpstreamBridge({ client, requestTimeout });
    yield* Effect.addFinalizer(() => Effect.promise(() => upstream.close()));
    const http = yield* Effect.acquireRelease(
        Effect.tryPromise(() =>
            startHTTPServer({
                host: '127.0.0.1',
                port,
                streamEndpoint: '/',
                sseEndpoint: null,
                cors: false,
                onListenSubscriptions: (uris) => acquireListenSubscriptions({ client, requestTimeout, uris }),
                createServer: async () => {
                    const server = new Server(serverInfo, { capabilities: serverCapabilities, ...(instructions === undefined ? {} : { instructions }) });
                    server.onerror = report;
                    await proxyServer({ client, requestTimeout, server, serverCapabilities });
                    return server;
                },
            }),
        ),
        (server) => Effect.promise(() => server.close()),
    );
    const notifications = upstream.subscribe({
        toolsListChanged: http.notify.toolsChanged,
        resourcesListChanged: http.notify.resourcesChanged,
        promptsListChanged: http.notify.promptsChanged,
        resourceUpdated: ({ uri }) => http.notify.resourceUpdated(uri),
    });
    yield* Effect.addFinalizer(() => Effect.sync(notifications.release));
    return yield* Deferred.await(closed);
}, Effect.scoped);
Command.make(BRIDGE.name, OPTIONS, bridge).pipe(Command.run({ version: BRIDGE.version }), Effect.provide(NodeServices.layer), Effect.provideService(Logger.LogToStderr, true), NodeRuntime.runMain);
