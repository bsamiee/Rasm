import { execFileSync } from 'node:child_process';
import { existsSync, readFileSync } from 'node:fs';
import { join } from 'node:path';
import { createDependencies, createNodes } from '@rasm/nx-cargo';
import { Cargo, execute } from '@rasm/nx-cargo/executors/cargo';
import { it, outputs, targetOptions } from '@rasm/testing/nx';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const cargo = { createNodes, createDependencies };
const crates = {
    'rust/Cargo.toml': '[workspace]\nresolver = "3"\nmembers = ["crates/*"]\n',
    'rust/crates/kit/Cargo.toml': '[package]\nname = "kit"\nversion = "0.1.0"\nedition = "2024"\n',
    'rust/crates/kit/src/lib.rs': '/// ```\n/// assert_eq!(kit::double(2), 4);\n/// ```\npub fn double(value: u32) -> u32 {\n    value * 2\n}\n',
    'rust/crates/app/Cargo.toml': '[package]\nname = "app"\nversion = "0.1.0"\nedition = "2024"\n\n[dependencies]\nkit = { path = "../kit", version = "0.1.0" }\n',
    'rust/crates/app/src/main.rs': 'fn main() {\n    println!("{}", kit::double(1));\n}\n\n#[test]\nfn doubles() {\n    assert_eq!(kit::double(3), 6);\n}\n',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

describe('test tools', { timeout: 120_000 }, () => {
    it('runs only the documentation tests in test--cargo where nextest runs the suites', async ({ workspace }) => {
        const graph = await workspace
            .write({
                ...crates,
                'rust/.config/nextest.toml': '[profile.default]\nfail-fast = false\n',
                'rust/crates/kit/src/lib.rs':
                    '/// ```\n/// std::fs::write(concat!(env!("CARGO_MANIFEST_DIR"), "/doc-ran"), "").unwrap();\n/// assert_eq!(kit::double(2), 4);\n/// ```\npub fn double(value: u32) -> u32 {\n    value * 2\n}\n\n#[test]\nfn broken() {\n    panic!("a unit test, which no documentation run reaches");\n}\n',
            })
            .graph(cargo);
        execFileSync('cargo', ['generate-lockfile'], { cwd: join(workspace.root, 'rust') });

        const { success } = await execute(await targetOptions(Cargo, graph, 'kit', 'test--cargo'), workspace.context(graph, 'kit', 'test--cargo'));

        expect([success, existsSync(join(workspace.root, 'rust/crates/kit/doc-ran'))]).toStrictEqual([true, true]);
    });
});

describe('coverage', { timeout: 120_000 }, () => {
    it('writes the lcov report of the crate and its direct workspace dependencies to <coveragePath>/lcov.info', async ({ workspace }) => {
        const graph = await workspace.write(crates).graph(cargo, { coveragePath: '.artifacts/coverage/{projectName}' });
        execFileSync('cargo', ['generate-lockfile'], { cwd: join(workspace.root, 'rust') });

        const { success } = await execute(await targetOptions(Cargo, graph, 'app', 'test--cargo', 'coverage'), workspace.context(graph, 'app', 'test--cargo', 'coverage'));

        const report = readFileSync(join(workspace.root, '.artifacts/coverage/app/lcov.info'), 'utf-8');
        expect(outputs(graph, 'app', 'test--cargo', 'coverage')).toStrictEqual(['.artifacts/coverage/app/lcov.info']);
        expect(success).toBe(true);
        expect([report.includes(`SF:${join(workspace.root, 'rust/crates/app/src/main.rs')}\n`), report.includes(`SF:${join(workspace.root, 'rust/crates/kit/src/lib.rs')}\n`)]).toStrictEqual([true, true]);
    });
});
