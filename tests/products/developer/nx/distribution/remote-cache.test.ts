import { env } from 'node:process';
import type { PreTasksExecutionContext } from '@nx/devkit';
import { preTasksExecution } from '@rasm/nx-distribution';
import { answered, remote } from '@rasm/testing/nx';
import { Effect, Exit, Function, Option, Scope } from 'effect';
import { describe, expect, test, vi } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const [server, token] = ['NX_SELF_HOSTED_REMOTE_CACHE_SERVER', 'NX_SELF_HOSTED_REMOTE_CACHE_ACCESS_TOKEN'] as const;
const context: PreTasksExecutionContext = { id: 'run-7f3a', workspaceRoot: '.', nxJsonConfiguration: {}, argv: ['node', 'nx', 'run-many', '--targets=build'] };

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('warned', ({}, { onCleanup }) => {
        const warn = vi.spyOn(console, 'warn').mockReturnValue(undefined);
        vi.stubEnv('NX_TASK_TARGET_PROJECT', undefined);
        vi.stubEnv('GITHUB_ACTIONS', undefined);
        vi.stubEnv(token, 'held-token');
        onCleanup(() => {
            warn.mockRestore();
            vi.unstubAllEnvs();
        });
        return warn;
    })
    .extend('cache', ({}, { onCleanup }) => {
        const scope = Scope.makeUnsafe();
        onCleanup(() => Effect.runPromise(Scope.close(scope, Exit.void)));
        return async (answer: Parameters<typeof remote>[1]) => {
            const held = await Effect.runPromise(Effect.provideService(remote('held-token', answer), Scope.Scope, scope));
            vi.stubEnv(server, held.address);
            return held;
        };
    });

describe('remote cache read before a run', () => {
    it.for([
        { case: '404 to the held token', answer: Function.flow(answered, Option.some) },
        { case: '200', answer: Function.constant(Option.some(200)) },
    ])('keeps the server and warns of nothing where its one read is answered $case', async ({ answer }, { cache, warned }) => {
        const { address, requests } = await cache(answer);

        await preTasksExecution({}, context);

        expect([env[server], requests, warned.mock.calls]).toStrictEqual([address, ['GET /v1/cache/run-7f3a'], []]);
    });

    it.for([
        { case: 'answered 401', answer: Function.constant(Option.some(401)), remoteCacheTimeout: '10 seconds', cause: String.raw`\b401\b` },
        { case: 'answered 500', answer: Function.constant(Option.some(500)), remoteCacheTimeout: '10 seconds', cause: String.raw`\b500\b` },
        { case: 'unanswered within remoteCacheTimeout', answer: Function.constant(Option.none()), remoteCacheTimeout: '50 millis', cause: String.raw`\S` },
    ])('empties the server and warns once where the read is $case', async ({ answer, remoteCacheTimeout, cause }, { cache, warned }) => {
        const { address } = await cache(answer);

        await preTasksExecution({ remoteCacheTimeout }, context);

        expect(env[server]).toBe('');
        expect(warned.mock.calls).toStrictEqual([[expect.stringMatching(new RegExp(`^${RegExp.escape(`Remote cache ${address} serves no read, so this run uses the local cache alone: `)}.*${cause}`, 'u'))]]);
    });

    it('empties the server and warns once where the connection is refused', async ({ warned }) => {
        const { address } = await Effect.runPromise(Effect.scoped(remote('held-token', Function.constant(Option.none()))));
        vi.stubEnv(server, address);

        await preTasksExecution({ remoteCacheTimeout: '10 seconds' }, context);

        expect([env[server], warned.mock.calls]).toStrictEqual(['', [[expect.stringMatching(new RegExp(`^${RegExp.escape(`Remote cache ${address} serves no read`)}`, 'u'))]]]);
    });

    it('warns as a workflow command under GITHUB_ACTIONS', async ({ cache, warned }) => {
        const { address } = await cache(Function.constant(Option.some(500)));
        vi.stubEnv('GITHUB_ACTIONS', 'true');

        await preTasksExecution({ remoteCacheTimeout: '10 seconds' }, context);

        expect(warned.mock.calls).toStrictEqual([[expect.stringMatching(new RegExp(`^${RegExp.escape(`::warning title=Nx remote cache::Remote cache ${address} serves no read`)}`, 'u'))]]);
    });

    it.for([undefined, ''])('sends nothing and warns of nothing where the server variable is %o', async (stated, { warned }) => {
        vi.stubEnv(server, stated);

        await preTasksExecution({ remoteCacheTimeout: '10 seconds' }, context);

        expect([env[server], warned.mock.calls]).toStrictEqual([stated, []]);
    });

    it('sends no read for a run a task started', async ({ cache, warned }) => {
        const { address, requests } = await cache(Function.constant(Option.some(500)));
        vi.stubEnv('NX_TASK_TARGET_PROJECT', 'app');

        await preTasksExecution({ remoteCacheTimeout: '10 seconds' }, context);

        expect([requests, env[server], warned.mock.calls]).toStrictEqual([[], address, []]);
    });
});
