import type { ProjectNodesWithFiles } from '@nx/conformance/testing';
import rule from '@rasm/conformance/release-projects';
import { Effect } from 'effect';
import { describe, expect, it } from 'vitest';
import { project, violations } from '#/workspace';

// --- [OPERATIONS] ----------------------------------------------------------------------

const checked = (release: Readonly<Record<string, unknown>>, ...projects: readonly ProjectNodesWithFiles[]) => Effect.runPromise(violations(rule, undefined, { nxJson: { release }, projects }));

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('release-projects', () => {
    it('accepts release configuration on exactly the projects release projects and groups select', async () => {
        await expect(checked({ groups: { libraries: { projects: ['tag:release'] }, tools: { projects: 'cli' } } }, project('lib', { tags: ['release'], release: {} }), project('cli', { release: {} }), project('app', { tags: ['os:all'] }))).resolves.toStrictEqual([]);
    });

    it('reports a selected project without release configuration and release configuration outside the selection', async () => {
        await expect(checked({ projects: ['tag:release'] }, project('lib', { tags: ['release'] }), project('app', { release: {} }))).resolves.toStrictEqual([
            { sourceProject: 'lib', message: 'lib is a release project without release configuration, give it both or neither' },
            { sourceProject: 'app', message: 'app holds release configuration outside nx.json release projects, give it both or neither' },
        ]);
    });

    it('selects every project when release names neither projects nor groups, as nx release does', async () => {
        await expect(checked({}, project('lib', { release: {} }), project('app'))).resolves.toStrictEqual([{ sourceProject: 'app', message: 'app is a release project without release configuration, give it both or neither' }]);
    });
});
