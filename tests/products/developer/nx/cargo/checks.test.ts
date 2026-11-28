import { execFileSync } from 'node:child_process';
import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import { workspaceRoot } from '@nx/devkit';
import { createDependencies, createNodes } from '@rasm/nx-cargo';
import { Cargo, execute } from '@rasm/nx-cargo/executors/cargo';
import { it, targetOptions } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const cargo = { createNodes, createDependencies };
const crates = {
    'rust-toolchain.toml': readFileSync(join(workspaceRoot, 'rust-toolchain.toml'), 'utf-8'),
    'Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["crates/*"]\n',
    'crates/kit/Cargo.toml': '[package]\nname = "kit"\nversion = "0.1.0"\nedition = "2024"\n',
    'crates/kit/src/lib.rs': 'pub fn one() -> u32 {\n    1\n}\n',
    'crates/app/Cargo.toml': '[package]\nname = "app"\nversion = "0.1.0"\nedition = "2024"\n\n[dependencies]\nkit = { path = "../kit", version = "0.1.0" }\n',
    'crates/app/src/main.rs': 'fn main() {\n    println!("{}", kit::one());\n}\n',
};
const ABORTING = '[workspace]\nresolver = "3"\nmembers = ["crates/*"]\n\n[profile.release-abort]\ninherits = "release"\npanic = "abort"\n';

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('check--clippy', () => {
    it.for([
        { case: 'a crate holding only a binary', project: 'app', files: {}, passed: true },
        { case: 'a library beside an example target that cannot compile', project: 'kit', files: { 'crates/kit/examples/fixture.rs': 'compile_error!("a lint fixture");\n' }, passed: true },
        { case: 'an unwind-only library, no abort profile', project: 'kit', files: { 'crates/kit/src/lib.rs': '#[cfg(panic = "abort")]\ncompile_error!("unwinding required");\n\npub fn one() -> u32 {\n    1\n}\n' }, passed: true },
        { case: 'an unwind-only library, an abort profile', project: 'kit', files: { 'Cargo.toml': ABORTING, 'crates/kit/src/lib.rs': '#[cfg(panic = "abort")]\ncompile_error!("unwinding required");\n\npub fn one() -> u32 {\n    1\n}\n' }, passed: false },
    ])('reports ($passed) $case', async ({ project, files, passed }, { workspace }) => {
        const graph = await workspace.write({ ...crates, ...files }).graph(cargo);
        execFileSync(...workspace.spawned('cargo', ['generate-lockfile']));

        const { success } = await execute(await targetOptions(Cargo, graph, project, 'check--clippy'), workspace.context(graph, project, 'check--clippy'));

        expect(success).toBe(passed);
    });
});
