import { execFileSync } from 'node:child_process';
import { createDependencies, createNodes } from '@rasm/nx-cargo';
import { Cargo, execute } from '@rasm/nx-cargo/executors/cargo';
import { it, targetOptions } from '@rasm/testing/nx';
import { Predicate } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const cargo = { createNodes, createDependencies };
const crates = {
    'project.json': '{ "name": "repo" }',
    'Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["crates/*"]\n',
    'crates/kit/Cargo.toml': '[package]\nname = "kit"\nversion = "0.1.0"\nedition = "2024"\n',
    'crates/kit/src/lib.rs': 'pub fn one() -> u32 {\n    1\n}\n',
    'crates/app/Cargo.toml': '[package]\nname = "app"\nversion = "0.1.0"\nedition = "2024"\n\n[dependencies]\nkit = { path = "../kit", version = "0.1.0" }\n',
    'crates/app/src/main.rs': 'fn main() {\n    println!("{}", kit::one());\n}\n',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

describe('check--clippy', { timeout: 120_000 }, () => {
    it('passes a crate holding only a binary, selecting --lib only for a crate with a library', async ({ workspace }) => {
        const graph = await workspace.write(crates).graph(cargo);
        execFileSync('cargo', ['generate-lockfile'], { cwd: workspace.root });

        const { success } = await execute(await targetOptions(Cargo, graph, 'app', 'check--clippy'), workspace.context(graph, 'app', 'check--clippy'));

        expect(success).toBe(true);
    });

    it('checks again under each profile whose panic strategy differs from that of dev', async ({ workspace }) => {
        const graph = await workspace.write({ ...crates, 'Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["crates/*"]\n\n[profile.release-abort]\ninherits = "release"\npanic = "abort"\n\n[profile.release-lean]\ninherits = "release"\nopt-level = "s"\n' }).graph(cargo);

        const { commands } = await targetOptions(Cargo, graph, 'kit', 'check--clippy');
        expect(commands.map(({ args }) => (args.includes('--profile') ? args[args.indexOf('--profile') + 1] : 'dev'))).toStrictEqual(['dev', 'release-abort']);
    });

    it.for([
        { case: 'the nearest clippy.toml above the crate', files: { 'clippy.toml': 'too-many-lines-threshold = 80\n' }, read: ['{workspaceRoot}/clippy.toml'] },
        {
            case: 'the clippy.toml of the folder CLIPPY_CONF_DIR names relative to its Cargo configuration',
            files: { 'clippy.toml': 'too-many-lines-threshold = 80\n', '.cargo/config.toml': '[env]\nCLIPPY_CONF_DIR = { value = "lints", relative = true }\n', 'lints/clippy.toml': 'too-many-lines-threshold = 40\n' },
            read: ['{workspaceRoot}/lints/clippy.toml'],
        },
    ])('hashes $case', async ({ files, read }, { workspace }) => {
        const graph = await workspace.write({ ...crates, ...files }).graph(cargo);

        const { inputs = [] } = graph.nodes['kit']?.data.targets?.['check--clippy'] ?? {};
        expect(inputs.filter(Predicate.isString).filter((input) => input.endsWith('clippy.toml'))).toStrictEqual(read);
    });
});

describe('check--rustdoc', { timeout: 120_000 }, () => {
    it.for([
        { case: 'fails a crate whose documentation breaks a denied rustdoc lint', source: '#![deny(rustdoc::broken_intra_doc_links)]\n\n/// Returns [`Missing`].\npub fn one() -> u32 {\n    1\n}\n', passed: false },
        { case: 'passes a crate whose documentation resolves every link', source: '#![deny(rustdoc::broken_intra_doc_links)]\n\n/// Returns [`u32::MAX`] minus a bound.\npub fn one() -> u32 {\n    1\n}\n', passed: true },
    ])('$case', async ({ source, passed }, { workspace }) => {
        const graph = await workspace.write({ ...crates, 'crates/kit/src/lib.rs': source }).graph(cargo);
        execFileSync('cargo', ['generate-lockfile'], { cwd: workspace.root });

        const { success } = await execute(await targetOptions(Cargo, graph, 'kit', 'check--rustdoc'), workspace.context(graph, 'kit', 'check--rustdoc'));

        expect(success).toBe(passed);
    });
});

describe('check--dylint', { timeout: 120_000 }, () => {
    it('exists only in a workspace whose metadata names dylint libraries, hashing each library folder', async ({ workspace }) => {
        const graph = await workspace
            .write({
                ...crates,
                'Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["crates/*"]\nexclude = ["lints"]\n\n[workspace.metadata.dylint]\nlibraries = [{ path = "lints" }]\n',
                'lints/Cargo.toml': '[package]\nname = "lints"\nversion = "0.1.0"\nedition = "2024"\n\n[lib]\ncrate-type = ["cdylib"]\n\n[workspace]\n',
                'lints/src/lib.rs': '',
                'other/Cargo.toml': '[package]\nname = "other"\nversion = "0.1.0"\nedition = "2024"\n\n[workspace]\n',
                'other/src/lib.rs': '',
            })
            .graph(cargo);

        expect(graph.nodes['kit']?.data.targets?.['check--dylint']?.inputs).toContain('{workspaceRoot}/lints/**/*');
        expect(graph.nodes['other']?.data.targets).not.toHaveProperty('check--dylint');
    });
});

describe('check--cargo-deny', { timeout: 120_000 }, () => {
    it.for([
        { case: 'the advisories table alone', files: { 'deny.toml': '[advisories]\nversion = 2\n' }, targets: ['check--cargo-deny-advisories'] },
        { case: 'the bans and licenses tables', files: { 'deny.toml': '[bans]\nmultiple-versions = "deny"\n\n[licenses]\nallow = ["MIT"]\n' }, targets: ['check--cargo-deny'] },
        { case: 'no deny.toml', files: {}, targets: [] },
    ])('gives the workspace root a check per cargo-deny run that $case selects', async ({ files, targets }, { workspace }) => {
        const graph = await workspace.write({ ...crates, ...files }).graph(cargo);

        expect(Object.keys(graph.nodes['repo']?.data.targets ?? {}).filter((target) => target.startsWith('check--cargo-deny'))).toStrictEqual(targets);
    });
});
