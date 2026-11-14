import { platform } from 'node:process';
import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import type { ProjectGraph } from '@nx/devkit';
import plugin from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Consumer, Packed } from '@rasm/testing/nx';
import { Array, Duration, Effect, Formatter, Layer, Path, Predicate, Record } from 'effect';
import nxPackage from 'nx/package.json' with { type: 'json' };

// --- [CONSTANTS] -----------------------------------------------------------------------

const UNREAD = /(?:^|[\s"'])-{0,2}version\b|mise\.lock|node_modules|@rasm\/nx-|TOKEN|SECRET|PASSWORD|CREDENTIAL/u;
const bounds = { built: Duration.toMillis('5 minutes'), test: Duration.toMillis('3 minutes') };
const hosts = {
    blender: {
        folder: 'hosts/blender',
        manifest: 'blender_manifest.toml',
        format: 'docs.blender.org/manual/en/latest/advanced/extensions/getting_started.html#manifest: version',
        kept: '# Marks points',
        versioned: (version: string) => `version = "${version}"`,
        text: '# Marks points\nschema_version = "1.0.0"\nid = "marker_tools"\nversion = "1.2.3"\nname = "Marker Tools"\ntagline = "Marks points"\nmaintainer = "Suite <suite@example.com>"\ntype = "add-on"\nblender_version_min = "4.2.0"\nlicense = ["SPDX:MIT"]\n',
    },
    yak: {
        folder: 'hosts/yak',
        manifest: 'manifest.yml',
        format: 'developer.rhino3d.com/guides/yak/the-package-manifest: version',
        kept: '# Marks points',
        versioned: (version: string) => `version: ${version}`,
        text: '# Marks points\nname: marker\nversion: 1.2.3\nauthors:\n    - Suite\ndescription: Marks points\nurl: https://example.com\n',
    },
    uxp: {
        folder: 'hosts/uxp',
        manifest: 'manifest.json',
        format: 'developer.adobe.com/photoshop/uxp/2022/guides/uxp_guide/uxp-misc/manifest-v5: version',
        kept: '"id": "com.example.panel"',
        versioned: (version: string) => `"version": "${version}"`,
        text: '{\n    "manifestVersion": 5,\n    "id": "com.example.panel",\n    "name": "Panel",\n    "version": "1.2.3",\n    "main": "index.html",\n    "host": { "app": "PS", "minVersion": "24.0.0" },\n    "entrypoints": [{ "type": "panel", "id": "panel", "label": { "default": "Panel" } }]\n}\n',
    },
    cep: {
        folder: 'hosts/cep',
        manifest: 'CSXS/manifest.xml',
        format: 'github.com/Adobe-CEP/CEP-Resources CEP_11.x/Documentation/CEP 11.1 HTML Extension Cookbook.md: ExtensionBundleVersion',
        kept: '<!-- Marks points -->',
        versioned: (version: string) => `ExtensionBundleVersion="${version}"`,
        text: '<?xml version="1.0" encoding="UTF-8"?>\n<!-- Marks points -->\n<ExtensionManifest Version="11.0" ExtensionBundleId="com.example.bridge" ExtensionBundleVersion="1.2.3" ExtensionBundleName="Bridge">\n    <ExtensionList>\n        <Extension Id="com.example.bridge.panel" Version="1.0" />\n    </ExtensionList>\n    <ExecutionEnvironment>\n        <HostList>\n            <Host Name="PHXS" Version="[24.0,99.9]" />\n        </HostList>\n        <LocaleList>\n            <Locale Code="All" />\n        </LocaleList>\n        <RequiredRuntimeList>\n            <RequiredRuntime Name="CSXS" Version="11.0" />\n        </RequiredRuntimeList>\n    </ExecutionEnvironment>\n</ExtensionManifest>\n',
    },
    unreal: {
        folder: 'apps/unreal/Marker',
        manifest: 'Marker.uplugin',
        format: 'dev.epicgames.com/documentation/unreal-engine/plugins-in-unreal-engine#plugindescriptorfiles: VersionName',
        kept: '"FriendlyName": "Marker"',
        versioned: (version: string) => `"VersionName": "${version}"`,
        text: '{\n    "FileVersion": 3,\n    "Version": 1,\n    "VersionName": "1.2.3",\n    "FriendlyName": "Marker",\n    "Modules": [{ "Name": "Marker", "Type": "Runtime", "LoadingPhase": "Default" }]\n}\n',
    },
} as const;
const packaged = ['blender', 'yak', 'uxp', 'cep'] as const;
const files = {
    ...Record.mapEntries(hosts, ({ folder, manifest, text }) => [`${folder}/${manifest}`, text]),
    'package.json': JSON.stringify({ name: 'consumer', private: true, type: 'module', workspaces: ['hosts/uxp', 'web/site'], devDependencies: { nx: nxPackage.version, [plugin.name]: '*' } }),
    'hosts/blender/__init__.py': 'def register():\n    pass\n\n\ndef unregister():\n    pass\n',
    'hosts/uxp/package.json': '{ "name": "panel", "version": "1.2.3", "private": true, "type": "module" }\n',
    'hosts/uxp/index.html': '<!doctype html><title>Panel</title>\n',
    'hosts/cep/index.html': '<!doctype html><title>Bridge</title>\n',
    'apps/unreal/Game/Game.uproject': '{ "FileVersion": 3, "EngineAssociation": "5.6", "Plugins": [{ "Name": "Marker", "Enabled": true }] }\n',
    'web/site/package.json': '{ "name": "site", "private": true, "type": "module" }\n',
    'web/site/manifest.json': '{ "name": "Site", "short_name": "Site", "start_url": "/", "display": "standalone", "icons": [] }\n',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = Effect.flatMap(Consumer, (consumer) => consumer.read);

const hosted = ({ nodes }: Pick<ProjectGraph, 'nodes'>, folder: string) => Object.values(nodes).filter(({ data }) => data.root === folder);

const packing = (targets: NonNullable<ProjectGraph['nodes'][string]['data']['targets']>) => Record.keys(targets).filter((name) => name.startsWith('build--'));

const needed = (targets: NonNullable<ProjectGraph['nodes'][string]['data']['targets']>, name: string) => (targets[name]?.dependsOn ?? []).map((row) => (Predicate.isString(row) ? row : row.target));

const named = (graph: Pick<ProjectGraph, 'nodes'>, folder: string) => hosted(graph, folder).flatMap(({ data }) => Record.keys(data.targets ?? {}));

// --- [COMPOSITION] ---------------------------------------------------------------------

const workspace = (stated: Readonly<Record<string, string>>, version: object = {}) =>
    Layer.provideMerge(Consumer.layer({ resolved: import.meta.url, settings: { plugins: [plugin.name], release: { projects: ['tag:release'], version } }, ignored: ['dist', '*.ccx', '*.zxp', '*.yak', '*.zip'], files: stated }), Layer.provideMerge(Packed.layer(plugin.name), layer));

it.layer(workspace(files), { timeout: bounds.built, excludeTestServices: true })((methods) => {
    const tagged = 'README.md PRINCIPLES "Plugins tag a project they give release configuration release, the one selector of nx.json release.projects"';
    const api = 'nrwl/nx packages/nx/src/project-graph/plugins/public-api.ts CreateNodesV2';

    methods.effect(
        'reading the graph writes no file',
        Effect.fnUntraced(function* () {
            const [, written] = yield* Effect.flatMap(Consumer, ({ written: tracked }) => tracked(read));
            assert.deepStrictEqual(written, [], `${api}: createNodes reads manifests and packages nothing`);
        }),
        bounds.test,
    );

    methods.effect.each(Record.keys(hosts))(
        'the folder of a %s manifest is one project',
        Effect.fnUntraced(function* (host) {
            const graph = yield* read;
            assert.deepStrictEqual(graph.failures, [], 'a valid workspace reads without failure');
            assert.lengthOf(hosted(graph, hosts[host].folder), 1, `${hosts[host].format}; ${api}: ${hosts[host].manifest} names its folder a project`);
        }),
        bounds.test,
    );

    methods.effect.each(packaged)(
        'a %s project is tagged release and builds its host package under the build verb',
        Effect.fnUntraced(function* (host) {
            const verbs = 'README.md PRINCIPLES "Verbs aggregate <verb>--<tool> targets"';
            const [project] = hosted(yield* read, hosts[host].folder);
            const targets = project?.data.targets ?? {};
            const { build } = targets;
            const waited = new Set(build?.dependsOn ?? []);
            assert.include(project?.data.tags ?? [], 'release', tagged);
            assert.isNotEmpty(packing(targets), `README.md TASKS "build compiles or packages a project into its declared outputs", "pack writes the files a release holds where they differ from the build"; ${verbs}: a build--<tool> target writes the host package`);
            assert.isTrue(waited.has('build--*') || new Set(packing(targets)).isSubsetOf(waited), `${verbs}: build depends on each build--<tool> target`);
        }),
        bounds.test,
    );

    methods.effect.each(packaged)(
        'each attach--<tool> target of a %s project depends on its packaging target',
        Effect.fnUntraced(function* (host) {
            const [project] = hosted(yield* read, hosts[host].folder);
            const targets = project?.data.targets ?? {};
            const attached = Record.keys(targets).filter((name) => name.startsWith('attach--'));
            assert.isNotEmpty(attached, 'README.md DELIVERY "the GitHub release holds the packaged files each attach target uploads"');
            assert.deepStrictEqual(
                attached.filter((name) => Array.intersection(needed(targets, name), ['build', ...packing(targets)]).length === 0),
                [],
                'README.md TASKS "Attach and publish targets depend on the targets that package their files"',
            );
        }),
        bounds.test,
    );

    methods.effect(
        'a Blender or Yak project publishes through nx release publish, and a UXP, CEP, or Unreal project holds no publish target',
        Effect.fnUntraced(function* () {
            const graph = yield* read;
            assert.deepStrictEqual(
                Record.map(hosts, ({ folder }) => named(graph, folder).includes('nx-release-publish')),
                { blender: true, yak: true, uxp: false, cep: false, unreal: false },
                'README.md DELIVERY "Publishing delivers those files by each destination\'s own command (pnpm publish, dotnet nuget push, uv publish, yak push)", "Plugin technology decides a host\'s package and install form (UXP .ccx, CEP .zxp), never where it publishes", "Fab takes no upload from a tool"; extensions.blender.org/api/v1/extensions/<id>/versions/upload/ takes a version upload',
            );
        }),
        bounds.test,
    );

    methods.effect.each(['yak', 'uxp', 'cep'] as const)(
        'a %s project holds one install--<tool> target on a platform its host runs on, and none elsewhere',
        Effect.fnUntraced(function* (host) {
            assert.lengthOf(
                named(yield* read, hosts[host].folder).filter((name) => name.startsWith('install--')),
                ['darwin', 'win32'].includes(platform) ? 1 : 0,
                'README.md DELIVERY "Installs serve development alone, replacing a previous build where its host loads it"; README.md PRINCIPLES "Plugins present a target that runs a host or needs a platform only on a machine holding it": Rhino and the Adobe hosts run on macOS and Windows alone',
            );
        }),
        bounds.test,
    );

    methods.effect(
        'a folder holding a package manifest and a host manifest is one project named by its package manifest',
        Effect.fnUntraced(function* () {
            assert.deepStrictEqual(
                hosted(yield* read, hosts.uxp.folder).map(({ name }) => name),
                ['panel'],
                'README.md PRINCIPLES "a host manifest names only a folder no ecosystem manifest names"',
            );
        }),
        bounds.test,
    );

    methods.effect(
        'a web app manifest.json makes no host package',
        Effect.fnUntraced(function* () {
            const [site] = hosted(yield* read, 'web/site');
            assert.isEmpty(packing(site?.data.targets ?? {}), 'developer.mozilla.org/docs/Web/Progressive_web_apps/Manifest: a manifest.json with no manifestVersion and host is no UXP manifest');
            assert.notInclude(site?.data.tags ?? [], 'release', tagged);
        }),
        bounds.test,
    );

    methods.effect(
        'no input of an inferred target is a version command (--version, -version, <tool> version), plugin source, mise.lock, or a credential variable',
        Effect.fnUntraced(function* () {
            const rows = Object.values((yield* read).nodes)
                .flatMap(({ data }) => Object.values(data.targets ?? {}))
                .flatMap(({ inputs = [] }) => inputs)
                .map((row) => Formatter.format(row));
            assert.deepStrictEqual(
                rows.filter((row) => UNREAD.test(row)),
                [],
                'README.md PRINCIPLES "Task inputs hold the files a task\'s tool reads: plugin and executor source, .config/mise.lock, tool --version output, and credentials are no input"',
            );
        }),
        bounds.test,
    );
    methods.effect(
        'two reads of one workspace give equal projects, with plugin isolation on and off',
        Effect.fnUntraced(function* () {
            const { called } = yield* Consumer;
            const printed = (isolate: boolean) => called('env', ['NX_CACHE_PROJECT_GRAPH=false', `NX_ISOLATE_PLUGINS=${isolate}`, 'nx', 'show', 'project', 'panel', '--json']);
            const [first, second, shared] = [yield* printed(true), yield* printed(true), yield* printed(false)];
            assert.deepStrictEqual([second.code, second.stdout], [first.code, first.stdout], `${api}: one set of files gives one set of nodes`);
            assert.deepStrictEqual([shared.code, shared.stdout], [first.code, first.stdout], 'nrwl/nx packages/nx/src/project-graph/plugins/isolation/isolated-plugin.spec.ts: a plugin returns the same project from a worker and in process');
        }),
        bounds.test,
    );
});

it.layer(workspace(files, { currentVersionResolver: 'registry', fallbackCurrentVersionResolver: 'disk' }), { timeout: bounds.built, excludeTestServices: true })((methods) => {
    methods.effect(
        'building a UXP plugin with no host installed writes one .ccx under the declared outputs',
        Effect.fnUntraced(function* () {
            const [{ nx, outputs, written }, path, graph] = yield* Effect.all([Consumer, Path.Path, read]);
            const [project] = hosted(graph, hosts.uxp.folder);
            const [{ code, stdout, stderr }, packed] = yield* written(nx('run', `${project?.name}:build`, '--output-style=static'));
            const stored = yield* Effect.forEach(packing(project?.data.targets ?? {}), (name) => outputs(`${project?.name}:${name}`));
            const folders = stored.flatMap(({ expandedOutputs }) => expandedOutputs.map(RegExp.escape)).join('|');
            const declared = new RegExp(`^(?:${folders})(?:${RegExp.escape(path.sep)}|$)`, 'u');
            assert.strictEqual(code, 0, `README.md PRINCIPLES "building, checking, and packaging a host project needs no host installed": nx run ${project?.name}:build exits 0: ${stdout}${stderr}`);
            assert.lengthOf(
                packed.filter((file) => path.extname(file) === '.ccx'),
                1,
                'README.md DELIVERY "Plugin technology decides a host\'s package and install form (UXP .ccx, CEP .zxp)"',
            );
            assert.deepStrictEqual(
                packed.filter((file) => !declared.test(file)),
                [],
                'nx/schemas/project-schema.json outputs: every file the build writes lies under a declared output',
            );
        }),
        bounds.test,
    );

    methods.effect(
        'clean removes the package the build wrote',
        Effect.fnUntraced(function* () {
            const [{ absent, nx, outputs }, graph] = yield* Effect.all([Consumer, read]);
            const [project] = hosted(graph, hosts.uxp.folder);
            const stored = yield* Effect.forEach(packing(project?.data.targets ?? {}), (name) => outputs(`${project?.name}:${name}`));
            const written = stored.flatMap(({ expandedOutputs }) => expandedOutputs);
            const { code, stdout, stderr } = yield* nx('run', `${project?.name}:clean`, '--output-style=static');
            assert.isNotEmpty(written, 'the build of the test before wrote the package');
            assert.strictEqual(code, 0, `nx run ${project?.name}:clean exits 0: ${stdout}${stderr}`);
            assert.deepStrictEqual(yield* absent(written), written, 'README.md TASKS "clean removes the outputs a project\'s targets declare and runs its tool\'s clean"');
        }),
        bounds.test,
    );

    methods.effect(
        'nx release version reads the current version of a host project from its manifest where the registry resolver does not apply',
        Effect.fnUntraced(function* () {
            const { nx } = yield* Consumer;
            const { code, stdout, stderr } = yield* nx('release', 'version', '2.0.0', '--dry-run');
            assert.strictEqual(code, 0, `nrwl/nx packages/nx/src/command-line/release/version/resolve-current-version.ts resolveCurrentVersionFromRegistry: a registry read giving null falls back to fallbackCurrentVersionResolver disk: ${stdout}${stderr}`);
            assert.include(stdout, '1.2.3', 'nrwl/nx packages/nx/src/command-line/release/version/resolve-current-version.spec.ts: the disk resolver reads the current version from the source manifest');
        }),
        bounds.test,
    );

    methods.effect(
        'nx release version writes the new version into the version field of each host manifest and keeps the rest of the file',
        Effect.fnUntraced(function* () {
            const { nx, text } = yield* Consumer;
            const { code, stdout, stderr } = yield* nx('release', 'version', '2.0.0');
            const texts = yield* Effect.forEach(Object.values(hosts), ({ folder, manifest }) => text(`${folder}/${manifest}`));
            assert.strictEqual(code, 0, `nx release version 2.0.0 exits 0: ${stdout}${stderr}`);
            assert.deepStrictEqual(
                Array.zipWith(Object.values(hosts), texts, ({ kept, versioned }, written) => [written.includes(versioned('2.0.0')), written.includes(versioned('1.2.3')), written.includes(kept)]),
                Object.values(hosts).map(() => [true, false, true]),
                `README.md DELIVERY "Releases give every project one version and tag through nx release"; nrwl/nx packages/js/src/release/version-actions.spec.ts "updateProjectVersion": ${Object.values(hosts)
                    .map(({ format }) => format)
                    .join('; ')}`,
            );
        }),
        bounds.test,
    );
});

it.layer(workspace({ ...files, 'hosts/torn/blender_manifest.toml': 'id = "torn\nversion = \n' }), { timeout: bounds.built, excludeTestServices: true })((methods) => {
    methods.effect(
        'a blender_manifest.toml that fails to parse is named in the failure, and every other project keeps its targets',
        Effect.fnUntraced(function* () {
            const errors = 'nrwl/nx packages/nx/src/project-graph/error-types.ts AggregateCreateNodesError';
            const graph = yield* read;
            const [yak] = hosted(graph, hosts.yak.folder);
            assert.include(
                graph.failures.flatMap(({ files: failed }) => failed).map(({ file }) => file),
                'hosts/torn/blender_manifest.toml',
                `${errors}: errors hold a [file, error] tuple for the file that failed`,
            );
            assert.isNotEmpty(packing(yak?.data.targets ?? {}), `${errors}: "It allows Nx to recieve partial results and continue processing"`);
        }),
        bounds.test,
    );
});
