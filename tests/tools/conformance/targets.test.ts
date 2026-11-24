import type { ProjectNodesWithFiles } from '@nx/conformance/testing';
import type { TargetConfiguration } from '@nx/devkit';
import rule from '@rasm/conformance/targets';
import { describe, expect, it } from 'vitest';
import { project, reported, violations } from '#/workspace';

// --- [OPERATIONS] ----------------------------------------------------------------------

const run = (target: Omit<TargetConfiguration, 'executor' | 'options'> = {}): TargetConfiguration => ({ executor: 'nx:run-commands', options: { command: 'true' }, ...target });

const aggregate = (name: string, inputs?: TargetConfiguration['inputs']): TargetConfiguration => ({ executor: 'nx:noop', dependsOn: [{ target: `${name}--*`, params: 'forward' }], ...(inputs === undefined ? {} : { inputs }) });

const checked = (...projects: readonly ProjectNodesWithFiles[]) => violations(rule, { separator: '--' }, { projects });

const app = project('app', { targets: { codegen: aggregate('codegen'), 'codegen--cf': run(), clean: aggregate('clean'), 'clean--colima': run(), test: aggregate('test'), 'test--vitest': run() } });

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('targets', () => {
    it('accepts valid options, dependsOn entries Nx resolves, a ^ entry naming a target no project of this graph holds, and nx:noop aggregates stating exactly the inputs of their tool targets', async () => {
        const lib = project('lib', {
            targets: {
                build: aggregate('build', ['default', { externalDependencies: ['typescript'] }, { dependentTasksOutputFiles: '**/*.d.ts', transitive: true }]),
                'build--tsc': run({
                    inputs: ['default', { externalDependencies: ['typescript'] }, { dependentTasksOutputFiles: '**/*.d.ts' }],
                    dependsOn: ['^build--*', 'codegen--*', { projects: '*', target: 'clean--colima' }, 'app:codegen--cf', '^install--colima', { dependencies: true, target: 'install--colima' }],
                    configurations: { watch: { command: 'tsc --watch' } },
                }),
                check: { executor: 'nx:noop', dependsOn: [{ target: 'check--*', params: 'forward', projects: ['lib'] }] },
                'check--tsc': run({ dependsOn: ['build--tsc'] }),
            },
        });
        await expect(checked(app, lib)).resolves.toStrictEqual([]);
    });

    it('reports a project outside the project schema and options outside their executor schema under each configuration', async () => {
        const rows = ['lib | lib data/targets/build/dependsOn', 'lib | lib:build:broken ', 'lib | lib:missing '];
        const lib = project('lib', { targets: { build: run({ dependsOn: [{ target: 'build', projects: ['app'], dependencies: true }], configurations: { broken: { parallel: 'yes' } } }), missing: { executor: '@rasm/absent:run' } } });
        expect(reported(await checked(lib), rows)).toStrictEqual(rows);
    });

    it('reports dependsOn entries naming a target their own project, their named projects, or every project lacks', async () => {
        const rows = ['lib | lib:test--vitest dependsOn "install--colima" names no target', 'lib | lib:test--vitest dependsOn "missing--*" names no target', 'lib | lib:test--vitest dependsOn {"projects":["app"],"target":"build--tsc"} names no target'];
        const lib = project('lib', {
            targets: {
                test: aggregate('test'),
                'test--vitest': run({ dependsOn: ['install--colima', 'missing--*', { projects: ['app'], target: 'build--tsc' }, 'build--tsc', '^test--vitest'] }),
                build: aggregate('build'),
                'build--tsc': run(),
                check: aggregate('check'),
                'check--tsc': run(),
            },
        });
        expect(reported(await checked(app, lib), rows)).toStrictEqual(rows);
    });

    it('reports a tool target without its aggregate, an aggregate without tools or running a command, an input one side states alone, and a target depending on an aggregate', async () => {
        const rows = [
            'lib | lib holds pack--pnpm and no pack target',
            'lib | lib test runs no test--<tool> target',
            'lib | lib build runs nx:run-commands after ["^build"]',
            'lib | lib build lacks input "^production" of build--tsc',
            'lib | lib build states input "{workspaceRoot}/README.md" no build--<tool> target reads',
            'lib | lib build depends on build,',
            'lib | lib build--tsc depends on codegen,',
        ];
        const lib = project('lib', {
            targets: {
                'pack--pnpm': run(),
                test: aggregate('test'),
                build: run({ dependsOn: ['^build'], inputs: ['default', '{workspaceRoot}/README.md'] }),
                'build--tsc': run({ inputs: ['default', '^production'], dependsOn: ['app:codegen'] }),
            },
        });
        expect(reported(await checked(app, lib), rows)).toStrictEqual(rows);
    });
});
