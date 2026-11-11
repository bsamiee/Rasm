import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { Effect } from 'effect';
import { getReportData } from 'nx/src/command-line/report/report';
import { SLOW } from '#/workspace/graph';

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(layer, { excludeTestServices: true })('package group', (methods) => {
    methods.effect(
        'installs one nx version and every package of its package group at that version',
        Effect.fnUntraced(function* () {
            const { outOfSyncPackageGroup, mismatchedNxVersions } = yield* Effect.promise(() => getReportData());
            assert.deepStrictEqual([outOfSyncPackageGroup, mismatchedNxVersions ?? []], [undefined, []], 'nrwl/nx@2a424d3 packages/nx/src/command-line/report/report.ts getReportData, report.spec.ts');
        }),
        SLOW,
    );
});
