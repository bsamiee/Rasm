import { execFileSync } from 'node:child_process';
import { globSync, readdirSync, statSync } from 'node:fs';
import { join, matchesGlob } from 'node:path';
import { type ExecutorContext, getOutputsForTargetAndConfiguration, type ProjectGraphProjectNode } from '@nx/devkit';
import { createNodes } from '@rasm/nx-distribution';
import { execute } from '@rasm/nx-distribution/executors/package';
import { TempFs } from '@rasm/testing/temp-fs';
import { describe, expect, test } from 'vitest';

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = test
    .extend('tempFs', ({}, { onCleanup }) => {
        const tempFs = new TempFs('package');
        onCleanup(() => tempFs.cleanup());
        tempFs.createFilesSync({
            '.gitignore': '.artifacts\n',
            'panel/index.html': '<!doctype html><title>Panel</title>\n',
            'panel/scripts/main.js': 'export const main = 1;\n',
        });
        return tempFs;
    })
    .extend('context', ({ tempFs }): ExecutorContext => ({
        root: tempFs.tempDir,
        cwd: tempFs.tempDir,
        isVerbose: false,
        projectName: 'panel',
        targetName: 'build--uxp',
        nxJsonConfiguration: {},
        projectsConfigurations: { version: 2, projects: { panel: { root: 'panel' } } },
        projectGraph: {
            nodes: { panel: { name: 'panel', type: 'lib', data: { root: 'panel', metadata: { technologies: ['uxp'] }, targets: { 'build--uxp': { executor: 'nx:noop', inputs: ['default', { dependentTasksOutputFiles: '**/*' }] } } } } },
            dependencies: {},
        },
    }));

describe('package executor', () => {
    it('packs the tracked source of a UXP plugin into one .ccx with no host installed', async ({ tempFs, context }) => {
        await tempFs.createFile('panel/manifest.json', JSON.stringify({ manifestVersion: 5, id: 'com.example.panel', name: 'Panel', version: '1.2.3', host: { app: 'PS', minVersion: '24.0.0' } }));

        await expect(execute({ outputPath: '.artifacts/distribution/panel' }, context)).resolves.toStrictEqual({ success: true });

        expect(execFileSync('unzip', ['-Z1', '.artifacts/distribution/panel/com.example.panel.ccx'], { cwd: tempFs.tempDir, encoding: 'utf-8' }).trim().split('\n')).toStrictEqual(['index.html', 'manifest.json', 'scripts/main.js']);
    });

    it('removes every earlier archive of the packed kind, whatever name its host tool gave it', async ({ tempFs, context }) => {
        await tempFs.createFile('panel/manifest.json', JSON.stringify({ manifestVersion: 5, id: 'com.example.panel', name: 'Panel', version: '1.2.3', host: { app: 'PS', minVersion: '24.0.0' } }));
        await tempFs.createFile('.artifacts/distribution/panel/previous-0.0.0.ccx', 'stale');
        await tempFs.createFile('.artifacts/distribution/panel/notes.txt', 'kept');

        await expect(execute({ outputPath: '.artifacts/distribution/panel' }, context)).resolves.toStrictEqual({ success: true });

        expect(readdirSync(join(tempFs.tempDir, '.artifacts/distribution/panel')).toSorted()).toStrictEqual(['com.example.panel.ccx', 'notes.txt']);
    });

    it('writes every output build--uxp declares and no file outside them', { timeout: 30_000 }, async ({ tempFs, context }) => {
        await tempFs.createFile('panel/manifest.json', JSON.stringify({ manifestVersion: 5, id: 'com.example.panel', name: 'Panel', version: '1.2.3', host: { app: 'PS', minVersion: '24.0.0' } }));
        const [, createNodesFunction] = createNodes;
        const results = await createNodesFunction(['panel/manifest.json'], { outputPath: '.artifacts/distribution/panel' }, { nxJsonConfiguration: {}, workspaceRoot: tempFs.tempDir });
        const node: ProjectGraphProjectNode = { name: 'panel', type: 'lib', data: { ...results[0]?.[1].projects?.['panel'], root: 'panel' } };
        const outputs = getOutputsForTargetAndConfiguration({ project: 'panel', target: 'build--uxp' }, {}, node);
        const before = new Set(readdirSync(tempFs.tempDir, { recursive: true, encoding: 'utf-8' }));

        expect(node.data.targets?.['build--uxp']?.options).toStrictEqual({ outputPath: '.artifacts/distribution/panel' });
        await expect(execute({ outputPath: '.artifacts/distribution/panel' }, { ...context, projectGraph: { nodes: { panel: node }, dependencies: {} } })).resolves.toStrictEqual({ success: true });

        const written = readdirSync(tempFs.tempDir, { recursive: true, encoding: 'utf-8' })
            .filter((file) => !before.has(file))
            .filter((file) => !matchesGlob(file, '.nx/**'))
            .filter((file) => statSync(join(tempFs.tempDir, file)).isFile());
        expect(written.toSorted()).toStrictEqual(globSync(outputs, { cwd: tempFs.tempDir }).toSorted());
        expect(outputs.map((output) => globSync(output, { cwd: tempFs.tempDir }).length)).toStrictEqual([1]);
    });

    it('fails naming the manifests it looked for where the project holds no host manifest', async ({ context }) => {
        await expect(execute({ outputPath: '.artifacts/distribution/panel' }, context)).rejects.toThrow('panel finds no manifest.json or manifest.yml in what its builds wrote');
    });
});
