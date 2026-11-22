import type { ProjectConfiguration } from '@nx/devkit';
import rule from '@rasm/conformance/nx-schemas';
import { Effect } from 'effect';
import { describe, expect, it } from 'vitest';
import { project, violations } from '#/workspace.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const labelled = (data: Omit<ProjectConfiguration, 'root'>, nxJson: Readonly<Record<string, unknown>> = {}) =>
    violations(rule, undefined, { nxJson, projects: [project('lib', data)] }).pipe(
        Effect.map((found) => found.map(({ file, sourceProject, message }) => ({ at: file ?? sourceProject, label: message.split(' ', 1)[0] }))),
        Effect.runPromise,
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('nx-schemas', () => {
    it('accepts settings, a project, and target options under the schemas Nx states', async () => {
        await expect(labelled({ tags: ['os:all'], targets: { build: { executor: 'nx:run-commands', options: { command: 'tsc' }, configurations: { watch: { command: 'tsc --watch' } } } } }, { parallel: 4 })).resolves.toStrictEqual([]);
    });

    it('reports nx.json settings outside the settings schema against nx.json', async () => {
        await expect(labelled({}, { parallel: 'many' })).resolves.toStrictEqual([{ at: 'nx.json', label: 'nx.json' }]);
    });

    it('reports a project outside the project schema against that project', async () => {
        await expect(labelled({ targets: { build: { executor: 'nx:noop', dependsOn: [{ target: 'build', projects: ['app'], dependencies: true }] } } })).resolves.toStrictEqual([{ at: 'lib', label: 'lib' }]);
    });

    it('reports options of a configuration and of an unresolvable executor under their target labels', async () => {
        await expect(labelled({ targets: { build: { executor: 'nx:run-commands', options: { command: 'tsc' }, configurations: { broken: { parallel: 'yes' } } }, missing: { executor: '@rasm/absent:run' } } })).resolves.toStrictEqual([
            { at: 'lib', label: 'lib:build:broken' },
            { at: 'lib', label: 'lib:missing' },
        ]);
    });
});
