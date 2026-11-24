import { symlinkSync } from 'node:fs';
import { join } from 'node:path';
import type { ProjectGraph } from '@nx/devkit';
import { createNodes } from '@rasm/nx-playwright';
import { it as nx, taskGraph, type Workspace } from '@rasm/testing/nx';
import { readNxJson } from 'nx/src/config/nx-json';
import { createTaskHasher } from 'nx/src/hasher/create-task-hasher';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const INSTALLED = { '@playwright/test': '1.66.0', unread: '1.0.0' } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const hashed = (workspace: Workspace, graph: ProjectGraph, { versions = INSTALLED, env = { CI: 'true' } }: { readonly versions?: Readonly<Record<string, string>>; readonly env?: Readonly<Record<string, string>> } = {}) => {
    const planned = taskGraph(graph, ['e2e'], ['test--playwright']);
    const installed: ProjectGraph = { ...graph, externalNodes: Object.fromEntries(Object.entries(versions).map(([name, version]) => [`npm:${name}`, { type: 'npm', name: `npm:${name}`, data: { packageName: name, version, hash: `${name}@${version}` } }])) };
    const task = planned.tasks['e2e:test--playwright'];
    return task === undefined
        ? Promise.resolve(undefined)
        : createTaskHasher(installed, readNxJson(workspace.root))
              .hashTask(task, planned, { ...env })
              .then(({ value }) => value);
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = nx.extend('modules', { auto: true }, ({ workspace }) => {
    workspace.install(import.meta.resolve('@playwright/test/package.json'));
    workspace.write({
        'package.json': '{"name":"repo","workspaces":["apps/*","libs/*"]}',
        'package-lock.json': '{}',
        'tsconfig.base.json': '{"compilerOptions":{"target":"es2022"}}',
        'tsconfig.shared.json': '{"extends":"./tsconfig.base.json","compilerOptions":{"strict":true}}',
        'docs/guide.md': '# Guide\n',
        'tools/pw/base.js': 'module.exports = {};\n',
        'tools/pw/setup.js': 'module.exports = async () => {};\n',
        'suites/e2e/smoke.spec.js': "const { test } = require('@playwright/test');\n\ntest('passes', () => {});\n",
        'libs/shared/package.json': '{"name":"shared","main":"index.js"}',
        'libs/shared/index.js': 'module.exports = {};\n',
        'apps/e2e/package.json': '{"name":"e2e"}',
        'apps/e2e/tsconfig.json': '{"extends":"../../tsconfig.shared.json"}',
        'apps/e2e/playwright.config.js': "require('../../tools/pw/base.js');\nrequire('shared');\nmodule.exports = { testDir: '../../suites/e2e', globalSetup: '../../tools/pw/setup.js' };\n",
    });
    symlinkSync(join(workspace.root, 'libs/shared'), join(workspace.root, 'node_modules/shared'), 'dir');
});

describe('what test--playwright hashes', { timeout: 60_000 }, () => {
    it.for([
        { case: 'a test file in a testDir outside the project', edit: { 'suites/e2e/smoke.spec.js': "const { test } = require('@playwright/test');\n\ntest('still passes', () => {});\n" }, changes: true },
        { case: 'the global setup script outside the project', edit: { 'tools/pw/setup.js': 'module.exports = async () => {\n    await Promise.resolve();\n};\n' }, changes: true },
        { case: 'a workspace file the config requires from outside the project', edit: { 'tools/pw/base.js': 'module.exports = { changed: true };\n' }, changes: true },
        { case: 'the source of a workspace package the config requires', edit: { 'libs/shared/index.js': 'module.exports = { changed: true };\n' }, changes: true },
        { case: 'a file of that workspace package the config never loads', edit: { 'libs/shared/README.md': '# shared\n' }, changes: false },
        { case: 'the compilerOptions of a tsconfig outside the project its extends chain reaches', edit: { 'tsconfig.shared.json': '{"extends":"./tsconfig.base.json","compilerOptions":{"strict":false}}' }, changes: true },
        { case: 'the include list of that tsconfig, which Playwright never reads', edit: { 'tsconfig.shared.json': '{"extends":"./tsconfig.base.json","compilerOptions":{"strict":true},"include":["src"]}' }, changes: false },
        { case: 'a docs file no project owns', edit: { 'docs/guide.md': '# Guide\n\nMore.\n' }, changes: false },
    ])('hashes again after an edit to $case: $changes', async ({ edit, changes }, { workspace }) => {
        const before = await hashed(workspace, await workspace.graph({ createNodes }));

        const after = await hashed(workspace, await workspace.write(edit).graph({ createNodes }));

        expect({ planned: before !== undefined && after !== undefined, changed: before !== after }).toStrictEqual({ planned: true, changed: changes });
    });

    it.for([
        { case: 'a new release of @playwright/test', moved: { versions: { ...INSTALLED, '@playwright/test': '1.67.0' } }, changes: true },
        { case: 'a new release of a package nothing reads', moved: { versions: { ...INSTALLED, unread: '2.0.0' } }, changes: false },
        { case: 'a run outside CI', moved: { env: {} }, changes: false },
    ])('hashes again after $case: $changes', async ({ moved, changes }, { workspace }) => {
        const graph = await workspace.graph({ createNodes });

        const [before, after] = await Promise.all([hashed(workspace, graph), hashed(workspace, graph, moved)]);

        expect({ planned: before !== undefined && after !== undefined, changed: before !== after }).toStrictEqual({ planned: true, changed: changes });
    });
});
