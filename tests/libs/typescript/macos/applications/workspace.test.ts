import { assert, describe, it } from '@effect/vitest';
import { Pressure } from '@rasm/applications/contract';
import { strains } from '@rasm/applications/workspace';
import { Effect, Schema } from 'effect';

describe('workspace', () => {
    it.effect('names an advisory the Swift server read as unread, apart from its levels', () =>
        Effect.gen(function* () {
            const unread = yield* Schema.decodeEffect(Pressure)({ memory: 'normal', available: 60, thermal: 'nominal', lowPower: false });
            const read = yield* Schema.decodeEffect(Pressure)({ memory: 'normal', available: 60, thermal: 'nominal', lowPower: false, advisory: { CombinedLevel: 'bad', UserLevel: 'great' } });
            const steady = [
                { normal: true, text: 'memory pressure normal with 60% available' },
                { normal: true, text: 'thermal state nominal' },
                { normal: true, text: 'low power mode off' },
            ];
            assert.deepStrictEqual(
                { unread: strains(unread), read: strains(read) },
                {
                    unread: [...steady, { normal: false, text: 'load advisory unread' }],
                    read: [...steady, { normal: false, text: 'load advisory CombinedLevel bad' }, { normal: true, text: 'load advisory UserLevel great' }],
                },
            );
        }),
    );
});
