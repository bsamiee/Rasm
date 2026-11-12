import { assert, it } from '@effect/vitest';
import { Effect } from 'effect';
import { fetched, type Located, matched, Workflows } from '#/delivery/workflows';

// --- [CONSTANTS] -----------------------------------------------------------------------

const RELEASE = /\bnx release\b/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const releasing = ({ step }: Located) => matched(RELEASE, step.run);

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Workflows.live, { excludeTestServices: true })('registry and store publishing', (methods) => {
    methods.effect(
        'starts a release by workflow_dispatch, never by pushed tags',
        Effect.fnUntraced(function* () {
            const { steps } = yield* Workflows;
            assert.isNotEmpty(steps.filter(releasing), 'release-assets/nrwl-nx/astro-docs/src/content/docs/features/manage-releases.mdoc:33 `nx release`, publish-in-ci-cd.mdoc:20 `nx release --skip-publish`');
            assert.deepStrictEqual(
                steps.filter((step) => releasing(step) && !(step.triggers.includes('workflow_dispatch') && !step.triggers.includes('push'))).map(({ at }) => at),
                [],
                'delivery/nx/astro-docs/src/content/docs/guides/Nx Release/publish-in-ci-cd.mdoc:194-211 "workflows triggered by the push event will not run if more than 3 tags are created at once", Option 1 `workflow_dispatch`',
            );
        }),
    );
    methods.effect(
        'checks out the full history before a release',
        Effect.fnUntraced(function* () {
            const { steps } = yield* Workflows;
            assert.deepStrictEqual(
                steps.filter((step) => releasing(step) && !fetched(step)).map(({ at }) => at),
                [],
                'delivery/nx/astro-docs/src/content/docs/guides/Nx Release/publish-in-ci-cd.mdoc:151,230 `fetch-depth: 0` on the checkout of the release job',
            );
        }),
    );
});
