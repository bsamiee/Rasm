import { spawn } from 'node:child_process';
import { once } from 'node:events';
import { createServer } from 'node:net';
import { createNodes } from '@rasm/nx-storybook';
import { TempFs } from '@rasm/testing/temp-fs';
import { Cause, Effect, Option, Predicate, type Scope } from 'effect';
import { assert, describe, expect, test, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const [, createNodesFunction] = createNodes;

// --- [OPERATIONS] ----------------------------------------------------------------------

const bound = (port: number, host: string) =>
    Effect.acquireRelease(
        Effect.tryPromise({
            try: async () => {
                const server = createServer().listen(port, host);
                await once(server, 'listening');
                return server;
            },
            catch: (cause) => new Cause.UnknownError(cause, `listen on ${host} port ${port} refused`),
        }),
        (server) => Effect.sync(() => server.close()),
    );

const getFreePort = Effect.fn('getFreePort')(function* (): Effect.fn.Return<number, Cause.UnknownError, Scope.Scope> {
    const address = (yield* bound(0, '127.0.0.1')).address();
    const port = address === null || Predicate.isString(address) ? 0 : address.port;
    return yield* bound(port, '::1').pipe(
        Effect.as(port),
        Effect.catchIf(
            ({ cause }) => Predicate.hasProperty(cause, 'code') && cause.code === 'EADDRINUSE',
            () => getFreePort(),
        ),
        Effect.orElseSucceed(() => port),
    );
});

const settle = (wait: Promise<number | null>, ms: number) =>
    Effect.runPromise(
        Effect.timeoutOption(
            Effect.promise(() => wait),
            `${ms} millis`,
        ),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('command', { scope: 'file' }, async ({}, { onCleanup }) => {
        const cwd = process.cwd();
        const tempFs = new TempFs('storybook-ready');
        process.chdir(tempFs.tempDir);
        onCleanup(() => {
            process.chdir(cwd);
            tempFs.cleanup();
        });
        tempFs.createFilesSync({
            'package.json': JSON.stringify({ name: 'repo' }),
            'my-app/project.json': JSON.stringify({ name: 'my-app' }),
            'my-app/.storybook/main.ts': "export default { stories: ['../*.stories.tsx'] };\n",
        });
        const nodes = await createNodesFunction(['my-app/.storybook/main.ts'], {}, { nxJsonConfiguration: {}, workspaceRoot: tempFs.tempDir });
        const readyWhen = nodes[0]?.[1].projects?.['my-app']?.targets?.['serve--storybook']?.readyWhen;
        assert(Predicate.hasProperty(readyWhen, 'command') && Predicate.isString(readyWhen.command), 'serve--storybook waits on a readyWhen command');
        return readyWhen.command;
    })
    .extend('tempFs', ({}, { onCleanup }) => {
        const tempFs = new TempFs('nx-storybook-ready-');
        onCleanup(() => tempFs.cleanup());
        return tempFs;
    })
    .extend('ports', async () => {
        const [port, otherPort] = await Effect.runPromise(Effect.scoped(Effect.all([getFreePort(), getFreePort()])));
        return { port, otherPort };
    })
    .extend(
        'fill',
        ({ ports: { port, otherPort } }) =>
            (value: string) =>
                value.replace('$port', String(port)).replace('$other', String(otherPort)),
    )
    .extend('stubEnv', ({}, { onCleanup }) => {
        ['PORT', 'SBCONFIG_PORT', 'SBCONFIG_HOSTNAME', 'CLAUDE_AGENT_SDK_VERSION', 'AI_AGENT'].forEach((name) => {
            vi.stubEnv(name, undefined);
        });
        onCleanup(() => {
            vi.unstubAllEnvs();
        });
        return (name: string, value: string) => vi.stubEnv(name, value);
    })
    .extend('installStorybook', ({ tempFs }) => (version: string) => {
        tempFs.createFileSync('node_modules/storybook/package.json', JSON.stringify({ name: 'storybook', version }));
    })
    .extend(
        'stubTaskOptions',
        ({ tempFs, stubEnv }) =>
            (options: Record<string, unknown>) =>
                stubEnv('NX_TASK_OPTIONS', JSON.stringify({ cwd: tempFs.tempDir, ...options })),
    )
    .extend('waitForStorybookDevServer', ({ command }, { onCleanup }) => async () => {
        const check = spawn(command, { shell: true, stdio: 'inherit' });
        onCleanup(() => {
            check.kill();
        });
        await once(check, 'exit');
        return check.exitCode;
    })
    .extend('listen', ({}, { onCleanup }) => async (port: number) => {
        const server = createServer().listen(port, '127.0.0.1');
        onCleanup(() => {
            server.close();
        });
        await once(server, 'listening');
    });

describe('waitForStorybookDevServer', () => {
    it.for([
        ['10.5.0', ['-p', '$port'], { SBCONFIG_PORT: '$other', PORT: '$other' }],
        ['10.5.0', [], { SBCONFIG_PORT: '$port', PORT: '$other' }],
        ['10.5.0', [], { PORT: '$port' }],
        ['10.5.0', ['--port=$other'], { CLAUDE_AGENT_SDK_VERSION: '1', PORT: '$port' }],
        ['10.5.0', ['-p', '$port', '-h', '::1'], { SBCONFIG_HOSTNAME: '127.0.0.1' }],
        ['10.4.6', ['--port', '$other'], { SBCONFIG_PORT: '$port' }],
        ['8.6.19', ['-p$port'], {}],
    ] as const)('waits for the port storybook %s resolves from %j and %j', async ([version, args, env], { ports: { port }, fill, stubEnv, installStorybook, stubTaskOptions, waitForStorybookDevServer, listen }) => {
        installStorybook(version);
        Object.entries(env).forEach(([name, value]) => {
            stubEnv(name, fill(value));
        });
        stubTaskOptions({ args: args.map(fill) });

        const wait = waitForStorybookDevServer();
        await expect(settle(wait, 500)).resolves.toSatisfy(Option.isNone);
        await listen(port);
        await expect(settle(wait, 10_000)).resolves.toStrictEqual(Option.some(0));
    });

    it.for(['8.6.19', '10.5.0'])('probes nothing when storybook %s gets no port, since it picks a free one', async (version, { installStorybook, stubTaskOptions, waitForStorybookDevServer }) => {
        installStorybook(version);
        stubTaskOptions({});

        await expect(settle(waitForStorybookDevServer(), 10_000)).resolves.toStrictEqual(Option.some(0));
    });
});
