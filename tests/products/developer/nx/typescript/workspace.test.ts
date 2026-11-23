import { execFileSync } from 'node:child_process';
import { existsSync } from 'node:fs';
import { join } from 'node:path';
import { createNodes } from '@rasm/nx-typescript';
import { refused } from '@rasm/testing/errors';
import { it, pathed, refusing, typescript } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROOT = { 'package.json': '{"name":"root","type":"module","workspaces":["libs/*"]}', 'package-lock.json': '' };
const KNIP = { 'node_modules/knip/package.json': '{"name":"knip","type":"module","exports":"./index.js"}', 'node_modules/knip/index.js': 'export {};' };
const STYLELINT = import.meta.resolve('stylelint/package.json');
const STYLED = { stylelintConfig: '.config/stylelint.config.ts' };
const RULED = {
    '.config/stylelint.config.ts': "import type { Config } from 'stylelint';\n\nexport default { rules: { 'no-unknown-custom-properties': true }, overrides: [{ files: ['../site/**/*.css'], referenceFiles: ['../site/theme.css'] }] } satisfies Config;\n",
};
const TOKENS = { ...RULED, 'site/theme.css': ':root { --ink: black; }\n', 'site/page.css': 'a { color: var(--ink); }\n' };
const LAYERED = {
    'node_modules/order-groups/package.json': '{"name":"order-groups","type":"module","exports":"./index.js"}',
    'node_modules/order-groups/index.js': 'export default [];\n',
    'node_modules/unit-table/package.json': '{"name":"unit-table","type":"module","exports":"./index.js"}',
    'node_modules/unit-table/index.js': 'export {};\n',
    '.config/stylelint.config.ts': [
        "import groups from 'order-groups';",
        "import type { Config } from 'stylelint';",
        "import { bans } from './lint/bans.ts';",
        '',
        'export default {',
        "    extends: 'preset-standard',",
        "    plugins: ['@scope/plugin-order/strict', './lint/rule.mjs', groups],",
        "    processors: ['./lint/processor.mjs'],",
        "    customSyntax: 'syntax-top',",
        "    referenceFiles: '../tokens/*.css',",
        "    overrides: [{ files: ['../site/**/*.css'], extends: ['preset-site'], plugins: 'plugin-site', referenceFiles: [{ files: ['../site/theme.css'], customSyntax: 'syntax-reference', loader: './lint/loader.mjs' }] }],",
        "    rules: { 'order/properties-order': groups, 'unit-disallowed-list': bans },",
        '} satisfies Config;',
        '',
    ].join('\n'),
    '.config/lint/bans.ts': "export const bans: readonly string[] = ['pt'];\n",
    '.config/lint/rule.mjs': "import 'value-parser';\n\nexport default { ruleName: 'local/rule', rule: () => () => {} };\n",
    '.config/lint/processor.mjs': "export default () => ({ name: 'local-processor', postprocess: () => {} });\n",
    '.config/lint/loader.mjs': 'export default (content) => content;\n',
};

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('workspace root targets', () => {
    it.for([
        { manager: 'npm', files: ROOT, catalogs: [] },
        { manager: 'pnpm', files: { 'package.json': '{"name":"root"}', 'pnpm-lock.yaml': '', 'pnpm-workspace.yaml': 'packages: []\n' }, catalogs: ['pnpm-workspace.yaml'] },
    ])('upgrades through $manager, moving the catalogs it reads: $catalogs', async ({ manager, files, catalogs }, { workspace }) => {
        const graph = await workspace.write(files).graph({ createNodes });

        expect(graph.nodes['root']?.data.targets?.[`upgrade--${manager}`]?.options).toStrictEqual({ catalogs });
    });

    it.for([
        { case: 'the pattern of .config/jscpd.json', files: { '.config/jscpd.json': '{"pattern":"**/*.ts"}' }, inputs: ['{workspaceRoot}/**/*.ts', '{workspaceRoot}/.config/jscpd.json'] },
        { case: 'every file, as .jscpd.json, which jscpd reads before .config/jscpd.json, states no pattern', files: { '.jscpd.json': '{}', '.config/jscpd.json': '{"pattern":"**/*.ts"}' }, inputs: ['{workspaceRoot}/**/*', '{workspaceRoot}/.jscpd.json'] },
        { case: 'every file, as the jscpd row of package.json states no pattern', files: { 'package.json': '{"name":"root","workspaces":["libs/*"],"jscpd":{}}' }, inputs: ['{workspaceRoot}/**/*', '{workspaceRoot}/package.json'] },
        { case: 'nothing, as no jscpd configuration exists and no check--jscpd runs', files: {}, inputs: undefined },
    ])('hashes $case for check--jscpd', async ({ files, inputs }, { workspace }) => {
        const graph = await workspace.write({ ...ROOT, ...files }).graph({ createNodes });

        expect(graph.nodes['root']?.data.targets?.['check--jscpd']?.inputs?.filter((input) => typeof input === 'string')).toStrictEqual(inputs);
    });

    it.for(refusing('jscpd'))('refuses the graph with the command and $case of jscpd', async ({ scripts, refusal }, { workspace }) => {
        pathed(join(workspace.root, '.bin'), scripts);

        await expect(workspace.write({ ...ROOT, '.jscpd.json': '{}' }).graph({ createNodes })).rejects.toSatisfy(refused(refusal));
    });

    it.for([
        { case: 'knip does not resolve', files: {}, options: undefined, run: undefined },
        { case: 'knip resolves', files: KNIP, options: undefined, run: { command: 'knip', cwd: '.' } },
        { case: 'knip resolves and knipConfig names its configuration', files: KNIP, options: { knipConfig: '.config/knip.json' }, run: { command: 'knip --config .config/knip.json', cwd: '.' } },
    ])('runs check--knip as $run.command when $case', async ({ files, options, run }, { workspace }) => {
        const graph = await workspace.write({ ...ROOT, ...files }).graph({ createNodes }, options);

        expect(graph.nodes['root']?.data.targets?.['check--knip']?.options).toStrictEqual(run);
    });

    it('refuses the graph with the command and exit of git ls-files when stylelintConfig names a config outside a Git repository', async ({ workspace }) => {
        await expect(
            workspace
                .install(typescript, STYLELINT)
                .write({ ...ROOT, ...TOKENS })
                .graph({ createNodes }, STYLED),
        ).rejects.toSatisfy(refused(/^git ls-files .* exits 128: /u));
    });
});

describe('check--stylelint of the workspace root', { timeout: 60_000 }, () => {
    const UNKNOWN = 'a { color: var(--absent); }\n';

    it.for([
        { case: 'stylelintConfig names a config, stylelint resolves, and Git lists a style file', packages: [typescript, STYLELINT], files: TOKENS, options: STYLED, inferred: true },
        { case: 'no stylelintConfig names a config', packages: [typescript, STYLELINT], files: TOKENS, options: undefined, inferred: false },
        { case: 'stylelint does not resolve', packages: [typescript], files: TOKENS, options: STYLED, inferred: false },
        { case: 'Git lists no style file, leaving a config that throws unread', packages: [typescript, STYLELINT], files: { '.config/stylelint.config.ts': "throw new Error('read');\n" }, options: STYLED, inferred: false },
    ])('infers the target: $inferred when $case', async ({ packages, files, options, inferred }, { workspace }) => {
        execFileSync('git', ['init', '--quiet'], { cwd: workspace.root });
        const graph = await workspace
            .install(...packages)
            .write({ ...ROOT, ...files })
            .graph({ createNodes }, options);

        expect('check--stylelint' in (graph.nodes['root']?.data.targets ?? {})).toBe(inferred);
    });

    it('hashes the style files Git lists, the config, each module and reference file it names, and each package it imports or names as a config, plugin, processor, syntax, or loader', async ({ workspace }) => {
        execFileSync('git', ['init', '--quiet'], { cwd: workspace.root });
        const graph = await workspace
            .install(typescript, STYLELINT)
            .write({ ...ROOT, ...TOKENS, ...LAYERED, '.gitignore': 'dist/\n', 'dist/page.css': UNKNOWN })
            .graph({ createNodes }, STYLED);

        const inputs = graph.nodes['root']?.data.targets?.['check--stylelint']?.inputs ?? [];

        expect({ files: inputs.filter((input) => typeof input === 'string').toSorted(), packages: inputs.flatMap((input) => (typeof input === 'object' && 'externalDependencies' in input ? input.externalDependencies : [])).toSorted() }).toStrictEqual({
            files: [
                '{workspaceRoot}/.config/lint/bans.ts',
                '{workspaceRoot}/.config/lint/loader.mjs',
                '{workspaceRoot}/.config/lint/processor.mjs',
                '{workspaceRoot}/.config/lint/rule.mjs',
                '{workspaceRoot}/.config/stylelint.config.ts',
                '{workspaceRoot}/site/page.css',
                '{workspaceRoot}/site/theme.css',
                '{workspaceRoot}/tokens/*.css',
            ],
            packages: ['@scope/plugin-order', 'order-groups', 'plugin-site', 'preset-site', 'preset-standard', 'stylelint', 'syntax-reference', 'syntax-top', 'value-parser'],
        });
    });

    it('reads the config again after a module it imports changes', async ({ workspace }) => {
        execFileSync('git', ['init', '--quiet'], { cwd: workspace.root });
        workspace.install(typescript, STYLELINT).write({ ...ROOT, ...TOKENS, ...LAYERED });
        await workspace.graph({ createNodes }, STYLED);

        const graph = await workspace.write({ '.config/lint/bans.ts': "import 'unit-table';\n\nexport const bans: readonly string[] = ['pt'];\n" }).graph({ createNodes }, STYLED);

        expect((graph.nodes['root']?.data.targets?.['check--stylelint']?.inputs ?? []).flatMap((input) => (typeof input === 'object' && 'externalDependencies' in input ? input.externalDependencies : []))).toContain('unit-table');
    });

    it('keeps what it read of a config that stops loading while the config and the modules it imports are unchanged', async ({ workspace }) => {
        execFileSync('git', ['init', '--quiet'], { cwd: workspace.root });
        workspace.install(typescript, STYLELINT).write({ ...ROOT, ...TOKENS, ...LAYERED });
        const fresh = await workspace.graph({ createNodes }, STYLED);

        const cached = await workspace.remove('node_modules/order-groups/index.js').graph({ createNodes }, STYLED);

        expect(cached.nodes['root']?.data.targets?.['check--stylelint']).toStrictEqual(fresh.nodes['root']?.data.targets?.['check--stylelint']);
    });

    it('reads the config again once a plugin option changes', async ({ workspace }) => {
        execFileSync('git', ['init', '--quiet'], { cwd: workspace.root });
        workspace.install(typescript, STYLELINT).write({ ...ROOT, ...TOKENS, ...LAYERED });
        await workspace.graph({ createNodes }, STYLED);

        await expect(workspace.remove('node_modules/order-groups/index.js').graph({ createNodes }, { ...STYLED, knipConfig: '.config/knip.json' })).rejects.toSatisfy(refused(/stylelint\.config\.ts exits 1: /u));
    });

    it('reads the config again after the lockfile changes, as a package the config imports states a row', async ({ workspace }) => {
        execFileSync('git', ['init', '--quiet'], { cwd: workspace.root });
        workspace.install(typescript, STYLELINT).write({
            ...ROOT,
            ...TOKENS,
            'node_modules/shared-rows/package.json': '{"name":"shared-rows","type":"module","exports":"./index.js"}',
            'node_modules/shared-rows/index.js': "export default ['plugin-early'];\n",
            '.config/stylelint.config.ts': "import plugins from 'shared-rows';\n\nexport default { plugins, rules: {} };\n",
        });
        await workspace.graph({ createNodes }, STYLED);

        const graph = await workspace.write({ 'node_modules/shared-rows/index.js': "export default ['plugin-late'];\n", 'package-lock.json': '{}' }).graph({ createNodes }, STYLED);

        expect((graph.nodes['root']?.data.targets?.['check--stylelint']?.inputs ?? []).flatMap((input) => (typeof input === 'object' && 'externalDependencies' in input ? input.externalDependencies : [])).toSorted()).toStrictEqual(['plugin-late', 'shared-rows', 'stylelint']);
    });

    it.for([
        { case: 'the config leaves the cache Stylelint keeps of each style file off', rows: '' },
        { case: 'the config turns that cache on', rows: 'cache: true, ' },
    ])('fails naming the style file once a reference stylesheet drops a token that unchanged file reads, when $case', async ({ rows }, { workspace }) => {
        execFileSync('git', ['init', '--quiet'], { cwd: workspace.root });
        const graph = await workspace
            .install(typescript, STYLELINT)
            .write({ ...ROOT, ...TOKENS, '.config/stylelint.config.ts': `export default { ${rows}rules: { 'no-unknown-custom-properties': true }, overrides: [{ files: ['../site/**/*.css'], referenceFiles: ['../site/theme.css'] }] };\n` })
            .graph({ createNodes }, STYLED);
        const before = await workspace.run(graph, 'root', 'check--stylelint');

        const after = await workspace.write({ 'site/theme.css': ':root { --paper: white; }\n' }).run(graph, 'root', 'check--stylelint');

        expect([before.success, after.success, after.terminalOutput]).toStrictEqual([true, false, expect.stringMatching(/site\/page\.css[\s\S]*no-unknown-custom-properties/u)]);
    });

    it('fails naming a style file added after it read the config', async ({ workspace }) => {
        execFileSync('git', ['init', '--quiet'], { cwd: workspace.root });
        workspace.install(typescript, STYLELINT).write({ ...ROOT, ...TOKENS });
        await workspace.graph({ createNodes }, STYLED);
        const graph = await workspace.write({ 'site/added.css': UNKNOWN }).graph({ createNodes }, STYLED);

        const result = await workspace.run(graph, 'root', 'check--stylelint');

        expect([result.success, result.terminalOutput]).toStrictEqual([false, expect.stringMatching(/site\/added\.css[\s\S]*no-unknown-custom-properties/u)]);
    });

    it('infers one target whether Git tracks a style file or lists it untracked', async ({ workspace }) => {
        execFileSync('git', ['init', '--quiet'], { cwd: workspace.root });
        workspace.install(typescript, STYLELINT).write({ ...ROOT, ...TOKENS });
        execFileSync('git', ['add', '--all'], { cwd: workspace.root });
        const untracked = await workspace.write({ 'site/zeta.css': 'b { color: var(--ink); }\n' }).graph({ createNodes }, STYLED);
        execFileSync('git', ['add', '--all'], { cwd: workspace.root });

        const tracked = await workspace.graph({ createNodes }, STYLED);

        expect(untracked.nodes['root']?.data.targets?.['check--stylelint']?.inputs).toContain('{workspaceRoot}/site/zeta.css');
        expect(tracked.nodes['root']?.data.targets?.['check--stylelint']).toStrictEqual(untracked.nodes['root']?.data.targets?.['check--stylelint']);
    });

    it('fails on a finding its rule reports at warning severity', async ({ workspace }) => {
        execFileSync('git', ['init', '--quiet'], { cwd: workspace.root });
        const graph = await workspace
            .install(typescript, STYLELINT)
            .write({ ...ROOT, 'site/page.css': UNKNOWN, '.config/stylelint.config.ts': "export default { rules: { 'no-unknown-custom-properties': [true, { severity: 'warning' }] } };\n" })
            .graph({ createNodes }, STYLED);

        const result = await workspace.run(graph, 'root', 'check--stylelint');

        expect([result.success, result.terminalOutput.includes('no-unknown-custom-properties')]).toStrictEqual([false, true]);
    });

    it('fails when the config enables a rule its plugin marks deprecated', async ({ workspace }) => {
        execFileSync('git', ['init', '--quiet'], { cwd: workspace.root });
        const graph = await workspace
            .install(typescript, STYLELINT)
            .write({
                ...ROOT,
                'site/page.css': 'a { display: block; }\n',
                '.config/stylelint.config.ts': "export default { plugins: ['./lint/old.mjs'], rules: { 'local/old': true } };\n",
                '.config/lint/old.mjs': "const ruleName = 'local/old';\n\nexport default { ruleName, rule: Object.assign(() => () => {}, { ruleName, messages: {}, meta: { url: 'https://example.invalid/old', deprecated: true } }) };\n",
            })
            .graph({ createNodes }, STYLED);

        const result = await workspace.run(graph, 'root', 'check--stylelint');

        expect([result.success, result.terminalOutput.includes('local/old')]).toStrictEqual([false, true]);
    });

    it('lints a style file whose name holds shell syntax as that file', async ({ workspace }) => {
        execFileSync('git', ['init', '--quiet'], { cwd: workspace.root });
        const graph = await workspace
            .install(typescript, STYLELINT)
            .write({ ...ROOT, ...TOKENS, 'site/page $(touch pwned).css': UNKNOWN })
            .graph({ createNodes }, STYLED);

        const result = await workspace.run(graph, 'root', 'check--stylelint');

        expect([result.success, result.terminalOutput.includes('no-unknown-custom-properties'), existsSync(join(workspace.root, 'pwned'))]).toStrictEqual([false, true, false]);
    });

    it.for([
        { case: 'a workspace folder Git lists', nested: [], files: { 'site/extra.css': UNKNOWN }, success: false, reported: true },
        { case: 'a folder .gitignore names', nested: [], files: { '.gitignore': 'dist/\n', 'dist/page.css': UNKNOWN }, success: true, reported: false },
        { case: 'a folder .git/info/exclude names', nested: [], files: { '.git/info/exclude': 'research/\n', 'research/page.css': UNKNOWN }, success: true, reported: false },
        { case: 'a repository nested in the workspace', nested: ['vendor'], files: { 'vendor/page.css': UNKNOWN }, success: true, reported: false },
    ])('ends with success $success, reporting the token: $reported, when a style file reading an unknown token sits in $case', async ({ nested, files, success, reported }, { workspace }) => {
        ['.', ...nested].forEach((at) => {
            execFileSync('git', ['init', '--quiet', at], { cwd: workspace.root });
        });
        const graph = await workspace
            .install(typescript, STYLELINT)
            .write({ ...ROOT, ...TOKENS, ...files })
            .graph({ createNodes }, STYLED);

        const result = await workspace.run(graph, 'root', 'check--stylelint');

        expect([result.success, result.terminalOutput.includes('no-unknown-custom-properties')]).toStrictEqual([success, reported]);
    });

    it.for([
        { case: 'a style file deleted from the working tree beside one present', removed: ['site/page.css'] },
        { case: 'only style files deleted from the working tree', removed: ['site/theme.css', 'site/page.css'] },
    ])('passes when the Git index lists $case', async ({ removed }, { workspace }) => {
        execFileSync('git', ['init', '--quiet'], { cwd: workspace.root });
        workspace.install(typescript, STYLELINT).write({ ...ROOT, ...TOKENS });
        execFileSync('git', ['add', '--all'], { cwd: workspace.root });
        const graph = await workspace.remove(...removed).graph({ createNodes }, STYLED);

        const result = await workspace.run(graph, 'root', 'check--stylelint');

        expect(result.success).toBe(true);
    });
});
