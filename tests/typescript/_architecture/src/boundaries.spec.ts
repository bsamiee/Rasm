import { NodeContext } from '@effect/platform-node';
import { expect, layer } from '@effect/vitest';
import { Audit, Imports } from '@rasm/ts-testkit/gauges';
import { Effect, Option } from 'effect';

// --- [CONSTANTS] -------------------------------------------------------------------------

const _LIBS = new URL('../../../../libs/typescript', import.meta.url).pathname;

// The branch-wide migrator ban plus the runtime-binding fence the exports map cannot express.
const _BANNED = [/^@effect\/sql\/Migrator/, /^@effect\/sql-pg\/PgMigrator/] as const;

// --- [OPERATIONS] ------------------------------------------------------------------------

// Source-walking gauges self-activate: while libs/typescript ships no source the verdict is UNSUPPORTED, never a vacuous green.
layer(NodeContext.layer)('branch boundaries', (it) => {
    it.effect('the migrator-import ban holds over every libs/typescript source module', () =>
        Effect.gen(function* () {
            const modules = yield* Imports.load(_LIBS);
            const verdict = Imports.verdict(modules, {
                banned: [..._BANNED],
                permitted: [],
                zone: () => Option.none(),
                zoneOf: () => Option.none(),
            });
            expect(
                Audit.$match(verdict, {
                    Unsupported: ({ reason }) => reason,
                    Audited: ({ violations }) => violations.map((violation) => `${violation.path} -> ${violation.specifier}`).join(', ') || 'clean',
                }),
            ).toMatch(/^(zero modules scanned|clean)$/);
        }),
    );

    it.effect('the edge-ledger audit demands its permitted-edge table the day source lands', () =>
        Effect.gen(function* () {
            const modules = yield* Imports.load(_LIBS);
            // Activation tripwire: source arriving without the composition-system permitted-edge table wired here is a loud failure.
            expect(
                Audit.$is('Unsupported')(
                    Imports.verdict(modules, { banned: [], permitted: [], zone: () => Option.none(), zoneOf: () => Option.none() }),
                ),
            ).toBe(true);
        }),
    );
});
