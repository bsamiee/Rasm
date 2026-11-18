import { assert, describe, it } from '@effect/vitest';
import { RunningInstance } from '@rasm/applications';
import { bound, token } from '@rasm/atelier/native';
import { Effect, Option, Schema } from 'effect';

// --- [OPERATIONS] ----------------------------------------------------------------------

const instance = (url: string, state: object, listening: readonly number[]) => Schema.decodeUnknownEffect(Schema.toCodecJson(RunningInstance))({ pid: 7, instance: '7@1', identifier: 'com.adobe.illustrator', url, activationPolicy: 'regular', state, listening });

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('native MCP server', () => {
    const row = { url: new URL('file:///Applications/Adobe%20Illustrator.app/') };
    const live = { failed: { reason: 'timedOut', subject: 'windows' } };

    it.effect.each([
        { named: 'takes the first candidate port the running application listens on', running: instance(row.url.href, live, [80, 18_414, 18_413]), expected: Option.some(18_413) },
        { named: 'finds no port while the application listens on none of the candidates', running: instance(row.url.href, live, [80]), expected: Option.none() },
        { named: 'ignores a candidate port another application listens on', running: instance('file:///Applications/Other.app/', live, [18_413]), expected: Option.none() },
        { named: 'ignores the ports of an instance that exited', running: instance(row.url.href, { exited: { status: 0 } }, [18_413]), expected: Option.none() },
    ])('$named', ({ running, expected }) => Effect.map(running, (each) => assert.deepStrictEqual(bound(row, { ports: [18_413, 18_414] })([each]), expected)));

    it.each([
        { named: 'reads the bearer token from the hex bytes of the AIMCPServer AuthToken', preferences: '/AIMCPServer {\n\t/Port 18412\n\t/AuthToken [ 5\n\t\t746f6b\n\t\t656e\n\t]\n}\n', expected: Option.some('token') },
        { named: 'reads no token from the AuthToken of another section', preferences: '/Other {\n\t/AuthToken [ 5\n\t\t746f6b656e\n\t]\n}\n/AIMCPServer {\n\t/Port 18412\n}\n', expected: Option.none() },
        { named: 'reads no token where preferences hold no AIMCPServer section', preferences: '/Plugin {\n\t/Port 1\n}\n', expected: Option.none() },
    ])('$named', ({ preferences, expected }) => {
        assert.deepStrictEqual(token(preferences), expected);
    });
});
