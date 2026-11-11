import { assert, it } from '@effect/vitest';
import { Effect } from 'effect';
import { fetched, type Located, matched, Workflows } from '#/delivery/workflows';

// --- [CONSTANTS] -----------------------------------------------------------------------

const AFFECTED = /\bnx (?:affected|show projects --affected)\b/u;
const EVERY = /\bnx run-many\b/u;
const SHAS = /^nrwl\/nx-set-shas@/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const selecting = ({ step }: Located) => matched(AFFECTED, step.run);

const based = ({ earlier, step }: Located) => step.run?.includes('--base') === true || earlier.some(({ uses }) => matched(SHAS, uses));

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Workflows.live, { excludeTestServices: true })('CI affected selection', (methods) => {
    methods.effect(
        'runs the project tasks of a pull request workflow through nx affected, never over every project',
        Effect.fnUntraced(function* () {
            const { steps } = yield* Workflows;
            const requested = steps.filter(({ triggers }) => triggers.includes('pull_request'));
            assert.isNotEmpty(requested.filter(selecting), 'delivery/nx/astro-docs/src/content/docs/features/CI Features/affected.mdoc:110 a pull request job runs `nx affected -t lint test build`');
            assert.deepStrictEqual(
                requested.filter(({ step }) => matched(EVERY, step.run)).map(({ at }) => at),
                [],
                'delivery/nx/astro-docs/src/content/docs/features/CI Features/affected.mdoc:28 `nx affected -t <targets>`, delivery/supabase-js/.github/workflows/ci-core.yml, delivery/typescript-eslint/.github/workflows/ci.yml',
            );
        }),
    );
    methods.effect(
        'checks out the full history before each affected query',
        Effect.fnUntraced(function* () {
            const { steps } = yield* Workflows;
            assert.deepStrictEqual(
                steps.filter((step) => selecting(step) && !fetched(step)).map(({ at }) => at),
                [],
                'delivery/nx/astro-docs/src/content/docs/features/CI Features/affected.mdoc:113 "fetch-depth: 0 is required. Without the full Git history, there is no base commit to compare against"',
            );
        }),
    );
    methods.effect(
        'gives each affected query the last successful commit as its base',
        Effect.fnUntraced(function* () {
            const { steps } = yield* Workflows;
            assert.deepStrictEqual(
                steps.filter((step) => selecting(step) && !based(step)).map(({ at }) => at),
                [],
                'delivery/nx/astro-docs/src/content/docs/features/CI Features/affected.mdoc:95-99 "set the base SHA to the latest successful commit", `nrwl/nx-set-shas` sets NX_BASE and NX_HEAD',
            );
        }),
    );
});
