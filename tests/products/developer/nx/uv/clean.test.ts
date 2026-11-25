import { execFileSync } from 'node:child_process';
import { createNodes } from '@rasm/nx-uv';
import { execute } from '@rasm/nx/executors/clean';
import { it, targetOptions } from '@rasm/testing/nx';
import { Schema } from 'effect';
import { describe, expect, vi } from 'vitest';

vi.mock(import('@nx/devkit/internal'), async (actual) => ({ ...(await actual()), workspaceDataDirectory: 'tmp/project-graph-cache' }));

// --- [CONSTANTS] -----------------------------------------------------------------------

const TESTED = {
    'pyproject.toml': '[project]\nname = "root"\nversion = "0.1.0"\nrequires-python = ">=3.13"\n\n[dependency-groups]\ndev = ["mypy"]\ntest = ["pytest>=8"]\n\n[tool.uv.workspace]\nmembers = ["proj/*"]\n',
    'proj/library/pyproject.toml': '[project]\nname = "library"\nversion = "0.1.0"\nrequires-python = ">=3.13"\n\n[build-system]\nrequires = ["uv_build>=0.13,<0.14"]\nbuild-backend = "uv_build"\n',
    'proj/library/src/library/__init__.py': 'def one() -> int:\n    return 1\n',
    'proj/library/tests/test_library.py': 'from library import one\n\n\ndef test_one() -> None:\n    assert one() == 1\n',
};

const UNSET = { commands: [], cwd: '.', folders: [] };

// --- [MODELS] --------------------------------------------------------------------------

const Stated = Schema.Record(Schema.String, Schema.Unknown);

// --- [OPERATIONS] ----------------------------------------------------------------------

const kept = (file: string) => file.startsWith('tmp/environments/library/') || (file.startsWith('proj/library/') && file.includes('/__pycache__/'));

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('clean--uv', { timeout: 120_000 }, () => {
    it('keeps what the checkers and tests of a member wrote under plain clean, and removes its environments, tool caches, and __pycache__ folders under configuration cache', async ({ workspace, onTestFinished }) => {
        execFileSync('uv', ['lock', '--quiet'], { cwd: workspace.write(TESTED).root });
        vi.stubEnv('NX_WORKSPACE_ROOT', workspace.root);
        vi.stubEnv('PYTHONPYCACHEPREFIX', undefined);
        onTestFinished(() => {
            vi.unstubAllEnvs();
        });
        const graph = await workspace.graph({ createNodes }, { environmentPath: 'tmp/environments/{projectName}' });
        const before = workspace.files();
        const runs = [await workspace.run(graph, 'library', 'check--mypy'), await workspace.run(graph, 'library', 'test--pytest')];
        const written = [...workspace.files().difference(before)].filter(kept);

        await execute({ ...UNSET, ...(await targetOptions(Stated, graph, 'library', 'clean--uv')) }, workspace.context(graph, 'library', 'clean--uv'));
        const plain = [...workspace.files()].filter(kept);
        await execute({ ...UNSET, ...(await targetOptions(Stated, graph, 'library', 'clean--uv', 'cache')) }, workspace.context(graph, 'library', 'clean--uv', 'cache'));
        const emptied = [...workspace.files()].filter(kept);

        expect(runs.map(({ success }) => success)).toStrictEqual([true, true]);
        expect(['tmp/environments/library/dev/pyvenv.cfg', 'tmp/environments/library/.mypy_cache/CACHEDIR.TAG', 'tmp/environments/library/.pytest_cache/CACHEDIR.TAG'].filter((file) => !written.includes(file))).toStrictEqual([]);
        expect(written.some((file) => file.startsWith('proj/library/'))).toBe(true);
        expect([plain.toSorted(), emptied]).toStrictEqual([written.toSorted(), []]);
    });
});
