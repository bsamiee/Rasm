import { assert } from '@effect/vitest';
import type { ProjectGraph } from '@nx/devkit';
import { createDependencies, createNodes } from '@rasm/nx-uv';
import manifest from '@rasm/nx-uv/package.json' with { type: 'json' };
import { Array, Effect, Order, Record } from 'effect';
import { cases, edges, inferred, roots, type Subject } from '#/inference/cases';

// --- [CONSTANTS] -----------------------------------------------------------------------

const subject: Subject = {
    plugin: { name: manifest.name, createNodes, createDependencies },
    tools: ['uv'],
    state: ['.venv'],
    files: {
        'package.json': '{ "name": "root" }\n',
        'pyproject.toml': '[project]\nname = "sheets"\nversion = "0.1.0"\nrequires-python = ">=3.13"\ndependencies = ["sheets-core"]\n\n[tool.uv.workspace]\nmembers = ["packages/*"]\n\n[tool.uv.sources]\nsheets-core = { workspace = true }\n',
        'uv.lock':
            'version = 1\nrevision = 5\nrequires-python = ">=3.13"\n\n[manifest]\nmembers = [\n    "sheets",\n    "sheets-core",\n]\n\n[[package]]\nname = "sheets"\nversion = "0.1.0"\nsource = { virtual = "." }\ndependencies = [\n    { name = "sheets-core" },\n]\n\n[package.metadata]\nrequires-dist = [{ name = "sheets-core", editable = "packages/core" }]\n\n[[package]]\nname = "sheets-core"\nversion = "0.1.0"\nsource = { editable = "packages/core" }\n',
        'packages/core/pyproject.toml': '[project]\nname = "sheets-core"\nversion = "0.1.0"\nrequires-python = ">=3.13"\n\n[build-system]\nrequires = ["uv_build>=0.9,<1"]\nbuild-backend = "uv_build"\n\n[tool.nx]\nimplicitDependencies = ["sheet-tools"]\n',
        'packages/core/sheets_core/__init__.py': '',
        'tools/pyproject.toml': '[project]\nname = "sheet-tools"\nversion = "0.1.0"\nrequires-python = ">=3.13"\n',
        'tools/uv.lock': 'version = 1\nrevision = 5\nrequires-python = ">=3.13"\n\n[[package]]\nname = "sheet-tools"\nversion = "0.1.0"\nsource = { virtual = "." }\n',
    },
    broken: ['packages/core/pyproject.toml', '[project\nname = "sheets-core"\n'],
};

const NX_PYTHON = 'lucasvieirasilva/nx-plugins@1c05d36 packages/nx-python';
const REGISTRY = /(?<=\[\[package\]\]\nname = ")[^"]+(?="\nversion = "[^"]+"\nsource = \{ registry)/gu;
const CHECKERS = ['check--mypy', 'check--refurb', 'check--ty', 'test--pytest'];
const WORKSPACE = '[tool.uv.workspace]\nmembers = ["apps/*", "libs/*"]\n';
const SOURCED = '\n[tool.uv.sources]\ndep1 = { workspace = true }\n';
const member = (name: string, tables = '') => `[project]\nname = "${name}"\nversion = "0.1.0"\nrequires-python = ">=3.13"\n${tables}`;

const INSTALLED_LOCK = `version = 1
revision = 5
requires-python = ">=3.13"
resolution-markers = [
    "python_full_version >= '3.15'",
    "python_full_version < '3.15'",
]

[manifest]
members = [
    "app1",
    "app2",
    "dep1",
]

[[package]]
name = "anyio"
version = "4.16.0"
source = { registry = "https://pypi.org/simple" }
dependencies = [
    { name = "idna" },
    { name = "typing-extensions", marker = "python_full_version < '3.15'" },
]

[[package]]
name = "app1"
version = "0.1.0"
source = { virtual = "apps/app1" }
dependencies = [
    { name = "httpx" },
]

[package.dev-dependencies]
dev = [
    { name = "mypy" },
    { name = "pytest" },
    { name = "refurb" },
    { name = "ty" },
]

[package.metadata]
requires-dist = [{ name = "httpx" }]

[package.metadata.requires-dev]
dev = [
    { name = "mypy" },
    { name = "pytest" },
    { name = "refurb" },
    { name = "ty" },
]

[[package]]
name = "app2"
version = "0.1.0"
source = { virtual = "apps/app2" }
dependencies = [
    { name = "idna" },
]

[package.metadata]
requires-dist = [{ name = "idna" }]

[[package]]
name = "ast-serialize"
version = "0.13.1"
source = { registry = "https://pypi.org/simple" }

[[package]]
name = "certifi"
version = "2026.10.23"
source = { registry = "https://pypi.org/simple" }

[[package]]
name = "colorama"
version = "0.4.6"
source = { registry = "https://pypi.org/simple" }

[[package]]
name = "dep1"
version = "0.1.0"
source = { virtual = "libs/dep1" }

[[package]]
name = "h11"
version = "0.16.0"
source = { registry = "https://pypi.org/simple" }

[[package]]
name = "httpcore"
version = "1.0.9"
source = { registry = "https://pypi.org/simple" }
dependencies = [
    { name = "certifi" },
    { name = "h11" },
]

[[package]]
name = "httpx"
version = "0.28.1"
source = { registry = "https://pypi.org/simple" }
dependencies = [
    { name = "anyio" },
    { name = "certifi" },
    { name = "httpcore" },
    { name = "idna" },
]

[[package]]
name = "idna"
version = "3.20"
source = { registry = "https://pypi.org/simple" }

[[package]]
name = "iniconfig"
version = "2.3.1"
source = { registry = "https://pypi.org/simple" }

[[package]]
name = "librt"
version = "0.16.0"
source = { registry = "https://pypi.org/simple" }

[[package]]
name = "mypy"
version = "2.4.1"
source = { registry = "https://pypi.org/simple" }
dependencies = [
    { name = "ast-serialize" },
    { name = "librt", marker = "platform_python_implementation != 'PyPy'" },
    { name = "mypy-extensions" },
    { name = "pathspec" },
    { name = "typing-extensions" },
]

[[package]]
name = "mypy-extensions"
version = "1.2.0"
source = { registry = "https://pypi.org/simple" }

[[package]]
name = "packaging"
version = "26.3"
source = { registry = "https://pypi.org/simple" }

[[package]]
name = "pathspec"
version = "1.1.1"
source = { registry = "https://pypi.org/simple" }

[[package]]
name = "pluggy"
version = "1.7.0"
source = { registry = "https://pypi.org/simple" }

[[package]]
name = "pygments"
version = "2.21.0"
source = { registry = "https://pypi.org/simple" }

[[package]]
name = "pytest"
version = "9.2.0"
source = { registry = "https://pypi.org/simple" }
dependencies = [
    { name = "colorama", marker = "sys_platform == 'win32'" },
    { name = "iniconfig" },
    { name = "packaging" },
    { name = "pluggy" },
    { name = "pygments" },
]

[[package]]
name = "refurb"
version = "2.3.1"
source = { registry = "https://pypi.org/simple" }
dependencies = [
    { name = "mypy" },
]

[[package]]
name = "ty"
version = "0.0.100"
source = { registry = "https://pypi.org/simple" }

[[package]]
name = "typing-extensions"
version = "4.16.0"
source = { registry = "https://pypi.org/simple" }
`;

const linked: Subject = {
    ...subject,
    files: {
        'package.json': '{ "name": "root" }\n',
        'pyproject.toml': `${WORKSPACE}\n[tool.uv.sources]\ndep2 = { workspace = true }\n`,
        'uv.lock': `version = 1
revision = 5
requires-python = ">=3.13"

[manifest]
members = [
    "acme-my-app",
    "acme-shared-lib",
    "dep1",
    "dep2",
    "extra",
    "grouped",
    "other",
    "rooted",
]

[[package]]
name = "acme-my-app"
version = "0.1.0"
source = { virtual = "apps/my-app" }
dependencies = [
    { name = "acme-shared-lib" },
]

[package.metadata]
requires-dist = [{ name = "acme-shared-lib", editable = "libs/shared-lib" }]

[[package]]
name = "acme-shared-lib"
version = "0.1.0"
source = { editable = "libs/shared-lib" }

[[package]]
name = "dep1"
version = "0.1.0"
source = { editable = "libs/dep1" }

[[package]]
name = "dep2"
version = "0.1.0"
source = { editable = "libs/dep2" }

[[package]]
name = "extra"
version = "0.1.0"
source = { virtual = "apps/extra" }

[package.optional-dependencies]
extra1 = [
    { name = "dep1" },
]

[package.metadata]
requires-dist = [{ name = "dep1", marker = "extra == 'extra1'", editable = "libs/dep1" }]
provides-extras = ["extra1"]

[[package]]
name = "grouped"
version = "0.1.0"
source = { virtual = "apps/grouped" }

[package.dev-dependencies]
dev = [
    { name = "dep1" },
]

[package.metadata]

[package.metadata.requires-dev]
dev = [{ name = "dep1", editable = "libs/dep1" }]

[[package]]
name = "other"
version = "0.1.0"
source = { virtual = "apps/other" }

[package.dev-dependencies]
other = [
    { name = "dep1" },
]

[package.metadata]

[package.metadata.requires-dev]
other = [{ name = "dep1", editable = "libs/dep1" }]

[[package]]
name = "rooted"
version = "0.1.0"
source = { virtual = "apps/rooted" }
dependencies = [
    { name = "dep2" },
]

[package.metadata]
requires-dist = [{ name = "dep2", editable = "libs/dep2" }]
`,
        'apps/grouped/pyproject.toml': member('grouped', `\n[dependency-groups]\ndev = ["dep1"]\n${SOURCED}`),
        'apps/extra/pyproject.toml': member('extra', `\n[project.optional-dependencies]\nextra1 = ["dep1"]\n${SOURCED}`),
        'apps/other/pyproject.toml': member('other', `\n[dependency-groups]\nother = ["dep1"]\n${SOURCED}`),
        'apps/rooted/pyproject.toml': member('rooted', 'dependencies = ["dep2"]\n'),
        'apps/my-app/pyproject.toml': member('acme-my-app', 'dependencies = ["acme-shared-lib"]\n\n[tool.uv.sources]\nacme-shared-lib = { workspace = true }\n'),
        'apps/my-app/project.json': '{ "name": "my-app" }\n',
        'libs/dep1/pyproject.toml': member('dep1'),
        'libs/dep2/pyproject.toml': member('dep2'),
        'libs/shared-lib/pyproject.toml': member('acme-shared-lib'),
        'libs/shared-lib/project.json': '{ "name": "shared-lib" }\n',
        'scripts/pyproject.toml': '[tool.ruff]\nline-length = 100\n',
    },
};

const LINKED = [
    { source: 'grouped', target: 'dep1', stated: 'the dev dependency group', title: '"should link dev dependencies in the graph"' },
    { source: 'extra', target: 'dep1', stated: 'an optional-dependencies extra', title: '"should link optional-dependencies (extras) in the graph"' },
    { source: 'other', target: 'dep1', stated: 'a dependency group of another name', title: '"should link arbitrary groups dependencies in the graph"' },
    { source: 'rooted', target: 'dep2', stated: 'project dependencies with its source in the root pyproject.toml alone', title: '"should read sources from root pyproject.toml"' },
    { source: 'my-app', target: 'shared-lib', stated: 'project dependencies where project.json names both members apart from their distributions', title: '"should detect dependencies when Nx project name differs from Python package name"' },
] as const;

const installing: Subject = {
    ...subject,
    options: { externalNodes: true },
    files: {
        'package.json': '{ "name": "root" }\n',
        'pyproject.toml': WORKSPACE,
        'uv.lock': INSTALLED_LOCK,
        'apps/app1/pyproject.toml': member('app1', 'dependencies = ["httpx"]\n\n[dependency-groups]\ndev = ["pytest", "ty", "mypy", "refurb"]\n'),
        'apps/app1/app1/__init__.py': '',
        'apps/app1/tests/test_app1.py': 'def test_app1() -> None:\n    assert True\n',
        'apps/app2/pyproject.toml': member('app2', 'dependencies = ["idna"]\n'),
        'apps/app2/app2/__init__.py': '',
        'libs/dep1/pyproject.toml': member('dep1'),
        'libs/dep1/dep1/__init__.py': '',
    },
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const locked = Array.sort(
    INSTALLED_LOCK.matchAll(REGISTRY).map(([name]) => `pypi:${name}`),
    Order.String,
);

const joined = ({ source, target }: { readonly source: string; readonly target: string }) => `${source} ${target}`;

const rooted = (graph: ProjectGraph) => Record.keys(Record.filter(roots(graph), (folder) => folder === '.'));

const external = (graph: ProjectGraph) =>
    Record.keys(graph.externalNodes ?? {})
        .filter((name) => name.startsWith('pypi:'))
        .toSorted();

const installed = (graph: ProjectGraph) =>
    Record.map(graph.nodes, (_, project) =>
        (graph.dependencies[project] ?? [])
            .map(({ target }) => target)
            .filter((target) => target.startsWith('pypi:'))
            .toSorted(),
    );

const checked = (graph: ProjectGraph) => Record.map(graph.nodes, (node) => Array.intersection(CHECKERS, Record.keys(node.data.targets ?? {})));

// --- [COMPOSITION] ---------------------------------------------------------------------

cases(subject, (it) => {
    it.effect('names each project by its project name, links a workspace source from the pyproject.toml that states it, and links an implicit dependency its tool.nx table states', () =>
        Effect.map(inferred(subject), (graph) =>
            assert.deepOwnInclude(
                { ...roots(graph), edges: edges(graph) },
                {
                    sheets: '.',
                    'sheets-core': 'packages/core',
                    'sheet-tools': 'tools',
                    edges: [
                        { source: 'sheets', target: 'sheets-core', type: 'static' },
                        { source: 'sheets-core', target: 'sheet-tools', type: 'implicit' },
                    ],
                },
            ),
        ),
    );

    it.effect.each(LINKED)('links $source to $target, a workspace source stated in $stated', ({ source, target, title }) =>
        inferred(linked).pipe(
            Effect.map(edges),
            Effect.map((stated) => assert.deepInclude(stated, { source, target, type: 'static' }, `${NX_PYTHON}/src/plugins/plugin.spec.ts uv ${title}`)),
        ),
    );

    it.effect('links only the workspace sources its members state where a folder outside the workspace holds a pyproject.toml without a project table', () =>
        Effect.map(inferred(linked), (graph) => assert.deepStrictEqual(edges(graph).map(joined), LINKED.map(joined).toSorted(Order.String), `${NX_PYTHON}/src/plugins/plugin.spec.ts uv "should progress the dependency graph when there is an app with an empty pyproject.toml"`)),
    );

    it.effect('adds each registry package the lock holds as a pypi external node where its options set externalNodes, and none by default', () =>
        Effect.map(Effect.all([inferred(installing), inferred({ ...installing, options: undefined })]), ([enabled, plain]) =>
            assert.deepStrictEqual([external(enabled), external(plain)], [locked, []], `${NX_PYTHON}/src/plugins/plugin.spec.ts "should add the locked packages as external nodes when enabled", "should add no external nodes by default"`),
        ),
    );

    it.effect('links each member to every package its lock entry installs through dependencies and groups, to a package two members install from both, and to none another member alone installs', () =>
        Effect.map(inferred(installing), (graph) =>
            assert.deepStrictEqual(
                installed(graph),
                { app1: locked, app2: ['pypi:idna'], dep1: [] },
                `${NX_PYTHON}/src/plugins/plugin.spec.ts "should make a member depend on every package it installs", "should give every member its edges when they share the root lock file", ${NX_PYTHON}-e2e/src/external-nodes.spec.ts "makes each project depend only on the packages it installs"`,
            ),
        ),
    );

    it.effect('gives a member the ty, mypy, refurb, and pytest targets where its lock entry installs each tool, and none of them to a member whose entry installs none', () =>
        Effect.map(inferred({ ...installing, options: undefined }), (graph) =>
            assert.deepStrictEqual(checked(graph), { app1: CHECKERS, app2: [], dep1: [] }, `README [03] verbs aggregate <verb>--<tool> targets; ${NX_PYTHON}-e2e/src/uv.spec.ts "lints the clean scaffold with ruff", "runs unit tests (pytest) successfully"`),
        ),
    );

    it.effect('names a workspace root that holds no project table by no file outside the workspace', () =>
        Effect.map(inferred(installing), (graph) =>
            assert.deepStrictEqual(Array.difference(rooted(graph), ['root']), [], 'nrwl/nx@2a424d3 packages/nx/src/plugins/package-json/create-nodes.ts reads each manifest through join(context.workspaceRoot, file); README [03] "Libraries ... take paths, names, and policies from their caller"'),
        ),
    );
});
