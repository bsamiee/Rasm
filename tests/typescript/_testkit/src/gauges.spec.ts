import { FileSystem, Path } from '@effect/platform';
import { NodeContext } from '@effect/platform-node';
import { describe, expect, it, layer } from '@effect/vitest';
import { Array, Effect, Option } from 'effect';
import { Audit, Imports, Snapshots } from './gauges.ts';

// --- [CONSTANTS] -------------------------------------------------------------------------

const _RULES = {
    banned: [/^@effect\/sql\/Migrator/, /^@effect\/sql-pg\/PgMigrator/, /^node:/],
    zone: (path: string): Option.Option<string> => Array.head(path.split('/')),
    zoneOf: (specifier: string): Option.Option<string> =>
        specifier.startsWith('./') || specifier.startsWith('../') ? Option.none() : Array.head(specifier.split('/')),
    permitted: [['app', 'kernel']] as ReadonlyArray<readonly [string, string]>,
} as const;

// --- [OPERATIONS] ------------------------------------------------------------------------

describe('import gauge engine', () => {
    it('zero modules is UNSUPPORTED, never a vacuous green', () => {
        expect(Audit.$is('Unsupported')(Imports.verdict([], _RULES))).toBe(true);
    });

    it('a banned specifier is flagged with its type-only provenance', () => {
        const verdict = Imports.verdict(
            Imports.scan([
                { path: 'app/live.ts', text: "import { readFileSync } from 'node:fs';" },
                { path: 'app/types.ts', text: "import type { Stats } from 'node:fs';" },
            ]),
            _RULES,
        );
        expect(
            Audit.$match(verdict, {
                Unsupported: () => [],
                Audited: ({ violations }) => Array.map(violations, (violation) => [violation.kind, violation.typeOnly] as const),
            }),
        ).toEqual([
            ['banned', false],
            ['banned', true],
        ]);
    });

    it('an unpermitted cross-zone edge is flagged; the permitted row passes', () => {
        const verdict = Imports.verdict(
            Imports.scan([
                { path: 'app/main.ts', text: "import { seal } from 'kernel/seal.ts';" },
                { path: 'kernel/seal.ts', text: "import { paint } from 'ui/paint.ts';" },
            ]),
            _RULES,
        );
        expect(
            Audit.$match(verdict, {
                Unsupported: () => [],
                Audited: ({ violations }) => Array.map(violations, (violation) => violation.path),
            }),
        ).toEqual(['kernel/seal.ts']);
    });
});

layer(NodeContext.layer)('gauges over the real tree', (it) => {
    it.effect('the kit audits its own import graph clean of banned rails', () =>
        Effect.gen(function* () {
            const modules = yield* Imports.load(new URL('.', import.meta.url).pathname);
            const verdict = Imports.verdict(modules, {
                ..._RULES,
                banned: [/^@effect\/sql\/Migrator/, /^@effect\/sql-pg\/PgMigrator/],
                permitted: [],
                zone: () => Option.none(),
                zoneOf: () => Option.none(),
            });
            expect(Audit.$is('Audited')(verdict)).toBe(true);
            expect(Audit.$match(verdict, { Unsupported: () => -1, Audited: ({ violations }) => violations.length })).toBe(0);
        }),
    );

    it.effect('an orphaned snapshot is flagged; an owned one is not', () =>
        Effect.scoped(
            Effect.gen(function* () {
                const fs = yield* FileSystem.FileSystem;
                const path = yield* Path.Path;
                const scratch = yield* fs.makeTempDirectoryScoped();
                yield* fs.makeDirectory(path.join(scratch, '__snapshots__'), { recursive: true });
                yield* fs.writeFileString(path.join(scratch, 'owned.spec.ts'), 'export {};');
                yield* fs.writeFileString(path.join(scratch, '__snapshots__', 'owned.spec.ts.snap'), '');
                yield* fs.writeFileString(path.join(scratch, '__snapshots__', 'ghost.spec.ts.snap'), '');
                const report = yield* Snapshots.audit(scratch);
                expect(report.scanned).toBe(2);
                expect(report.orphans).toEqual([path.join('__snapshots__', 'ghost.spec.ts.snap')]);
            }),
        ),
    );
});
