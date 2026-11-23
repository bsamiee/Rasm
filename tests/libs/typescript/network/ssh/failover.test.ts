import { assert, it } from '@effect/vitest';
import { Ssh } from '@rasm/ssh';
import { Array, Effect } from 'effect';
import { bounds, callers, reached, Sshd, within } from '#/sshd';

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Sshd.layer(), { timeout: bounds.started, excludeTestServices: true })('failover', (methods) => {
    methods.effect(
        'keeps a shared connection on its dial while it lives, dials again when that dial stops answering, and decides again for each next one',
        Effect.fnUntraced(function* () {
            const [ssh, { hosted, asked, far, near }] = yield* Effect.all([Ssh, Sshd]);
            const host = yield* hosted('flipped', { path: { _tag: 'Direct', preferred: [{ ...near.dial, within }] } });
            const every = Effect.map(
                Effect.forEach(Array.range(1, callers), () => reached(ssh, host), { concurrency: 'unbounded' }),
                Array.dedupe,
            );
            const decided = Effect.andThen(asked(host, 'exit'), every);
            const first = yield* every;
            const stopped = yield* near.paused(reached(ssh, host));
            const resumed = yield* every;
            const flips = yield* Effect.all([near.paused(decided), decided, near.paused(decided)]);
            assert.deepStrictEqual([first, stopped, resumed, flips], [[near.name], far.name, [far.name], [[far.name], [near.name], [far.name]]]);
        }),
        bounds.test,
    );
});
