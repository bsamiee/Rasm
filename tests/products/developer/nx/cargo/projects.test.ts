import { createDependencies, createNodes } from '@rasm/nx-cargo';
import { it, unaggregated, unportable } from '@rasm/testing/nx';
import { Predicate, Record } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const cargo = { createNodes, createDependencies };
const crates = {
    'Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["crates/*"]\n',
    'crates/core-kit/Cargo.toml': '[package]\nname = "core-kit"\nversion = "0.1.0"\nedition = "2024"\n',
    'crates/core-kit/src/lib.rs': 'pub fn one() -> u32 {\n    1\n}\n',
    'crates/cli/Cargo.toml': '[package]\nname = "cli"\nversion = "0.1.0"\nedition = "2024"\n\n[dependencies]\ncore-kit = { path = "../core-kit", version = "0.1.0" }\n',
    'crates/cli/src/main.rs': 'fn main() {\n    println!("{}", core_kit::one());\n}\n',
};
const rich = {
    ...crates,
    'package.json': '{ "name": "repo", "private": true, "workspaces": ["crates/*"] }',
    'project.json': '{ "name": "repo" }',
    'Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["crates/*"]\n\n[profile.release-abort]\ninherits = "release"\npanic = "abort"\n',
    '.config/nextest.toml': '[profile.default]\nfail-fast = false\n',
    'deny.toml': '[advisories]\nversion = 2\n\n[bans]\nmultiple-versions = "deny"\n',
    'clippy.toml': 'too-many-arguments-threshold = 5\n',
    'crates/cli/package.json': '{ "name": "cli", "nx": { "targets": { "check--clippy": { "inputs": ["default", "{projectRoot}/lints/**/*"] } } } }',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

describe('inferred projects', { timeout: 120_000 }, () => {
    it('infers a project for each workspace member, named by its package in snake case and rooted at its manifest folder', async ({ workspace }) => {
        const graph = await workspace.write(crates).graph(cargo);

        expect(Record.map(graph.nodes, ({ data }) => data.root)).toStrictEqual({ core_kit: 'crates/core-kit', cli: 'crates/cli' });
    });

    it.for([
        { case: 'a package.json holding nx', files: { 'package.json': '{ "name": "repo", "nx": {} }' }, roots: { repo: '.', core: 'crates/core' } },
        { case: 'a project.json', files: { 'project.json': '{ "name": "repo" }' }, roots: { repo: '.', core: 'crates/core' } },
        { case: 'a package.json Nx reads no project from', files: { 'package.json': '{ "name": "repo" }' }, roots: { core: 'crates/core' } },
    ])('infers the virtual workspace root as a project only where Nx anchors one there, as with $case', async ({ files, roots }, { workspace }) => {
        const graph = await workspace.write({ ...files, 'Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["crates/*"]\n', 'crates/core/Cargo.toml': '[package]\nname = "core"\nedition = "2024"\n', 'crates/core/src/lib.rs': '' }).graph(cargo);

        expect(Record.map(graph.nodes, ({ data }) => data.root)).toStrictEqual(roots);
    });

    it('gives the workspace root project the upgrade and a clean of the whole workspace, and each member a clean of its own package', async ({ workspace }) => {
        const graph = await workspace.write({ ...crates, 'project.json': '{ "name": "repo" }' }).graph(cargo);

        const targets = Record.map(graph.nodes, ({ data }) => data.targets ?? {});
        expect(Object.keys(graph.nodes).filter((name) => 'upgrade--cargo' in (targets[name] ?? {}))).toStrictEqual(['repo']);
        expect(targets['repo']?.['clean--cargo']).toMatchObject({ options: { commands: [['cargo', 'clean']] } });
        expect(targets['cli']?.['clean--cargo']).toMatchObject({
            options: {
                commands: [
                    ['cargo', 'clean', '--package', 'cli', '--profile', 'dev', '--locked'],
                    ['cargo', 'clean', '--package', 'cli', '--profile', 'release', '--locked'],
                ],
            },
        });
    });
});

describe('hashing', { timeout: 120_000 }, () => {
    it('hashes the source folders of the crate targets, its manifest and readme, and the workspace lock, manifest, and cargo configuration', async ({ workspace }) => {
        const graph = await workspace
            .write({
                ...crates,
                '.cargo/config.toml': '[build]\nincremental = true\n',
                'crates/cli/README.md': '# cli\n',
                'crates/cli/Cargo.toml': '[package]\nname = "cli"\nversion = "0.1.0"\nedition = "2024"\nreadme = "README.md"\n',
                'crates/cli/tests/smoke.rs': '#[test]\nfn smoke() {}\n',
                'crates/cli/notes/plan.md': '# plan\n',
            })
            .graph(cargo);

        const hashed = graph.nodes['cli']?.data.namedInputs?.['default']?.filter(Predicate.isString) ?? [];
        expect(hashed).toStrictEqual(expect.arrayContaining(['{projectRoot}/Cargo.toml', '{projectRoot}/README.md', '{projectRoot}/src/**/*', '{projectRoot}/tests/**/*', '{workspaceRoot}/Cargo.lock', '{workspaceRoot}/Cargo.toml', '{workspaceRoot}/.cargo/config.toml']));
        expect(hashed.filter((input) => input.startsWith('{projectRoot}/notes') || input === '{projectRoot}/**/*')).toStrictEqual([]);
    });
});

describe('graph laws', { timeout: 120_000 }, () => {
    it('hashes every tool input in its verb and forwards the verb to each tool, package.json overrides included', async ({ workspace }) => {
        const graph = await workspace.write(rich).graph(cargo, { coveragePath: '.artifacts/coverage/{projectName}' });

        expect(graph.nodes['cli']?.data.targets?.['check--clippy']?.inputs).toContain('{projectRoot}/lints/**/*');
        expect(unaggregated(graph)).toStrictEqual([]);
    });

    it('states every root, input, output, and working folder as a workspace-relative posix path', async ({ workspace }) => {
        const graph = await workspace.write(rich).graph(cargo, { coveragePath: '.artifacts/coverage/{projectName}' });

        expect(Object.keys(graph.nodes['repo']?.data.targets ?? {})).toStrictEqual(expect.arrayContaining(['upgrade--cargo', 'check--cargo-deny', 'check--cargo-deny-advisories']));
        expect(unportable(graph)).toStrictEqual([]);
    });
});
