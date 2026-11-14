import { assert, it } from '@effect/vitest';
import { workspaceRoot } from '@nx/devkit';
import { Array, Duration, Effect, Function, type Layer, Option, Record, Result, Schema } from 'effect';
import { answered, Consumer, fromCache, remote } from '#/nx';

// --- [TYPES] ---------------------------------------------------------------------------

interface Member {
    readonly root: string;
    readonly name: string;
    readonly manifest: string;
    readonly version: string;
    readonly released: boolean;
    readonly verbs: readonly string[];
    readonly sources: readonly string[];
    readonly dependencies: readonly string[];
}

interface Ecosystem<R, M extends Member> {
    readonly workspace: (files?: Record.ReadonlyRecord<string, string>) => Layer.Layer<Consumer | R>;
    readonly members: Effect.Effect<readonly M[], never, Consumer | R>;
    readonly built: (member: M) => Effect.Effect<readonly string[], never, Consumer | R>;
    readonly report: string;
    readonly tools: readonly string[];
    readonly verbs: readonly string[];
    readonly shared: readonly string[];
    readonly unlocked: readonly string[];
    readonly scratch: readonly string[];
    readonly added: Effect.Effect<void, never, Consumer | R>;
    readonly refused: Record.ReadonlyRecord<string, string>;
    readonly manifests: (next: string) => Record.ReadonlyRecord<string, string>;
    readonly published: boolean;
    readonly edits: {
        readonly verbs: readonly string[];
        readonly dependent: string;
        readonly upstream: (mark: string) => Record.ReadonlyRecord<string, string>;
        readonly outside: (mark: string) => Record.ReadonlyRecord<string, string>;
        readonly tested: string;
        readonly failing: Record.ReadonlyRecord<string, string>;
    };
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const REPLAYED = /:(?:build|test|check)--/u;
const EFFECTFUL = /:(?:fix|clean|install|upgrade)(?:--|$)|:nx-release-publish$/u;
const PUBLISH = ':nx-release-publish';
const NOOP = 'nx:noop';

// --- [MODELS] --------------------------------------------------------------------------

const Names = Schema.fromJsonString(Schema.Array(Schema.String));

// --- [OPERATIONS] ----------------------------------------------------------------------

const placed = Effect.gen(function* () {
    const { read } = yield* Consumer;
    const { nodes, dependencies, failures } = yield* read;
    const projects = Object.values(nodes).filter(({ data }) => data.root !== '.');
    const roots = Record.fromIterableWith(projects, ({ name, data }) => [name, data.root]);
    const edges = projects.flatMap(({ name }) => (dependencies[name] ?? []).filter(({ target }) => target in roots));
    const idle = Object.values(nodes).flatMap(({ name, data }) => Record.keys(Record.filter(data.targets ?? {}, ({ executor }) => executor === NOOP)).map((target) => `${name}:${target}`));
    return { projects, roots, edges, failures, idle: new Set(idle), tasks: projects.flatMap(({ name, data }) => Record.keys(data.targets ?? {}).map((target) => ({ name, target, id: `${name}:${target}` }))) };
});

const reach = (members: readonly Member[], root: string, held: ReadonlySet<string> = new Set()): ReadonlySet<string> =>
    held.has(root)
        ? held
        : members
              .filter((member) => member.root === root)
              .flatMap(({ dependencies }) => dependencies)
              .reduce<ReadonlySet<string>>((reached, dependency) => reach(members, dependency, reached), held.union(new Set([root])));

const misread = (members: readonly Member[], shared: readonly string[], { root, sources }: Member, files: readonly string[]) => {
    const near = reach(members, root);
    const [inside, outside] = Array.partition(members, (member) => (near.has(member.root) ? Result.succeed(member.sources) : Result.fail(member.sources)));
    return [...Array.difference([...shared, ...sources], files).map((file) => `${root} build omits ${file}`), ...Array.intersection(Array.difference(outside.flat(), inside.flat()), files).map((file) => `${root} build reads ${file}`)];
};

const under = (files: readonly string[], folders: readonly string[]) => files.filter((file) => folders.some((folder) => file === folder || file.startsWith(`${folder}/`)));

const tasked = (members: readonly Member[]) => members.flatMap(({ name, verbs }) => verbs.map((verb) => `${name}:${verb}`));

const pairs = (members: readonly Member[]) => members.flatMap(({ root, dependencies }) => dependencies.map((dependency) => `${root} -> ${dependency}`));

const awaited = (dependencies: Record.ReadonlyRecord<string, readonly { readonly id: string }[]>) =>
    Object.entries(dependencies)
        .flatMap(([id, tasks]) => tasks.map((task) => [id, task.id] as const))
        .filter((ends) => ends.every((end) => end.endsWith(PUBLISH)))
        .map((ends) => ends.join(' -> '));

const waits = (open: readonly Member[]) => open.flatMap(({ name, dependencies }) => open.filter(({ root }) => dependencies.includes(root)).map((needed) => `${name}${PUBLISH} -> ${needed.name}${PUBLISH}`));

const tooled = Effect.fnUntraced(function* (name: string, verb: string) {
    const [{ planned }, { idle }] = yield* Effect.all([Consumer, placed]);
    const { dependencies } = yield* planned('run', `${name}:${verb}`);
    const run = (id: string): readonly string[] => (dependencies[id] ?? []).flatMap((task) => (idle.has(task.id) ? run(task.id) : [task.id]));
    const hits = run(`${name}:${verb}`).map(fromCache);
    assert.isNotEmpty(hits, 'README.md PRINCIPLES "Verbs aggregate <verb>--<tool> targets"; nrwl/nx packages/nx/src/project-graph/utils/project-configuration/target-normalization.ts: a target with no executor and no command is nx:noop, which runs no tool');
    return (printed: string) => hits.filter((hit) => !hit.test(printed)).map(String);
});

const counted = Effect.fnUntraced(function* (tools: readonly string[], reset: string) {
    const { root, called, wrote, text } = yield* Consumer;
    const real = yield* Effect.forEach(tools, (tool) => Effect.map(called('which', [tool]), ({ stdout }) => ({ tool, at: stdout.trim() })));
    yield* wrote({ '.nx/spied.log': '', ...Record.fromIterableWith(real, ({ tool, at }) => [`.nx/spies/${tool}`, `#!/bin/sh\necho ${tool} >> '${root}/.nx/spied.log'\nexec '${at}' "$@"\n`]) });
    yield* called('chmod', ['-R', '+x', '.nx/spies']);
    const { code, stderr } = yield* called('sh', ['-c', `${reset}PATH="${root}/.nx/spies:$PATH" nx show projects --json`]);
    const runs = yield* text('.nx/spied.log');
    return { code, stderr, runs: runs.split('\n').filter((line) => line !== '').length };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const conforms = <R, M extends Member>({ workspace, members, built, report, tools, verbs, shared, unlocked, scratch, added, refused, manifests, published, edits }: Ecosystem<R, M>) => {
    const [bound, next, token] = [Duration.toMillis('5 minutes'), '0.2.0', 'suite-token'];
    const options = { timeout: bound, excludeTestServices: true } as const;
    const member = (root: string) => Effect.flatMap(members, (stated) => Effect.orDie(Effect.fromOption(Array.findFirst(stated, (candidate) => candidate.root === root))));
    const seen = Effect.gen(function* () {
        const [{ root, nx, inputs }, stated] = yield* Effect.all([Consumer, members]);
        const { stdout } = yield* nx('graph', '--print');
        const hashed = yield* Effect.forEach(
            stated.filter((candidate) => candidate.verbs.includes('build')),
            ({ name }) => inputs(`${name}:build`),
        );
        return { root, read: { graph: stdout, hashed } };
    });

    it.layer(
        workspace(),
        options,
    )((methods) => {
        methods.effect(
            'gives every member the tool reports one project, under the name and at the folder the tool gives it',
            Effect.fnUntraced(function* () {
                const [stated, { roots }] = yield* Effect.all([members, placed]);
                assert.deepStrictEqual(
                    roots,
                    Record.fromIterableWith(stated, ({ name, root }) => [name, root]),
                    `${report}; nrwl/nx e2e/dotnet/src/dotnet.test.ts "should detect .NET projects"; packages/nx/src/project-graph/plugins/public-api.ts CreateNodesResult "projects" keyed by root`,
                );
            }),
            bound,
        );

        methods.effect(
            `gives a member each of ${verbs.join(', ')} exactly where the tool report gives it`,
            Effect.fnUntraced(function* () {
                const [stated, { tasks }] = yield* Effect.all([members, placed]);
                assert.sameMembers(
                    tasks.filter(({ target }) => verbs.includes(target)).map(({ id }) => id),
                    tasked(stated),
                    `${report}; README.md TASKS; nrwl/nx e2e/dotnet/src/dotnet.test.ts "should show different targets for library projects", "should show test targets for test projects"`,
                );
            }),
            bound,
        );

        methods.effect(
            'caches every build, test, and check tool target, and no target that edits sources, removes outputs, installs, upgrades, or publishes',
            Effect.fnUntraced(function* () {
                const [{ planned }, { tasks, idle }] = yield* Effect.all([Consumer, placed]);
                const plan = yield* planned('run-many', `--targets=${Array.dedupe(tasks.map(({ target }) => target)).join(',')}`);
                const tooling = Record.filter(plan.tasks, ({ target }) => !idle.has(`${target.project}:${target.target}`));
                const cached = Record.keys(Record.filter(tooling, ({ cache = false }) => cache));
                const replayed = Record.keys(tooling).filter((id) => REPLAYED.test(id));
                assert.isNotEmpty(replayed, 'README.md PRINCIPLES "Verbs aggregate <verb>--<tool> targets"');
                assert.deepStrictEqual(Array.difference(replayed, cached), [], 'nrwl/nx astro-docs/src/content/docs/features/cache-task-results.mdoc "Define cacheable tasks"; e2e/dotnet/src/dotnet-cache.test.ts "should store and use local cache for builds"');
                assert.deepStrictEqual(
                    cached.filter((id) => EFFECTFUL.test(id)),
                    [],
                    'nrwl/nx astro-docs/src/content/docs/features/cache-task-results.mdoc: a task with side effects is not cacheable',
                );
            }),
            bound,
        );

        methods.effect(
            'hashes for the build of each member every shared file and every source the tool lists for it, and no source of a member outside its dependencies',
            Effect.fnUntraced(function* () {
                const [{ inputs }, stated] = yield* Effect.all([Consumer, members]);
                const builds = stated.filter((candidate) => candidate.verbs.includes('build'));
                const hashed = yield* Effect.forEach(builds, ({ name }) => inputs(`${name}:build`));
                assert.deepStrictEqual(
                    Array.zipWith(builds, hashed, (candidate, { files }) => misread(stated, shared, candidate, files)).flat(),
                    [],
                    `${report}; README.md PRINCIPLES "Task inputs hold the files a task's tool reads"; nrwl/nx e2e/dotnet/src/dotnet-advanced-msbuild.test.ts; e2e/gradle/src/gradle-task-dependencies.test.ts "should still hit cache when an unrelated file changes"`,
                );
            }),
            bound,
        );

        methods.effect.each(shared)(
            'marks every member with a build affected by a change to %s',
            Effect.fnUntraced(function* (file) {
                const [{ nx }, stated] = yield* Effect.all([Consumer, members]);
                const { stdout } = yield* nx('show', 'projects', '--affected', `--files=${file}`, '--withTarget=build', '--json');
                assert.includeMembers(
                    [...(yield* Effect.orDie(Schema.decodeEffect(Names)(stdout)))],
                    stated.filter(({ verbs: given }) => given.includes('build')).map(({ name }) => name),
                    `${report}; README.md TASKS "--files=<path> limits it to one change's reach"; nrwl/nx astro-docs/src/content/docs/features/CI Features/affected.mdoc`,
                );
            }),
            bound,
        );

        methods.effect(
            'draws one edge per dependency the tool reports between members and none for a transitive one, static where the member holds its manifest and implicit elsewhere',
            Effect.fnUntraced(function* () {
                const [stated, { roots, edges }] = yield* Effect.all([members, placed]);
                assert.sameMembers(
                    Array.dedupe(edges.map(({ source, target }) => `${roots[source]} -> ${roots[target]}`)),
                    pairs(stated),
                    `${report}; nrwl/nx e2e/dotnet/src/dotnet.test.ts "should detect project dependencies"; e2e/dotnet/src/dotnet-dependency-graph.test.ts "should only show direct dependencies, not transitive ones"`,
                );
                const holders = stated.filter(({ root, manifest }) => manifest.startsWith(`${root}/`)).map(({ name }) => name);
                assert.deepStrictEqual(
                    edges.filter(({ source, type }) => (type === 'static') !== holders.includes(source)).map(({ source, target, type }) => `${source} -> ${target} ${type}`),
                    [],
                    'nrwl/nx packages/gradle/src/plugin/dependencies.spec.ts "records an edge as static when the source project owns the build file", "records an ancestor-configured project as implicit, since Nx rejects a sourceFile outside the source project"',
                );
            }),
            bound,
        );

        methods.effect(
            'gives a second checkout at another path the same graph and the same build inputs, with no checkout path in either',
            Effect.fnUntraced(function* () {
                const [here, there] = yield* Effect.all([seen, Effect.provide(seen, workspace())]);
                assert.notInclude(here.read.graph, here.root, 'README.md PRINCIPLES "one remote cache serves every runner"');
                assert.notInclude(here.read.graph, workspaceRoot, 'README.md PRINCIPLES "Libraries take paths, names, and policies from their caller and hold no repository convention": a packed plugin names no path of the repository that packed it');
                assert.deepStrictEqual(there.read, here.read, 'README.md PRINCIPLES "one remote cache serves every runner"; nrwl/nx astro-docs/src/content/docs/kb/performant-project-graph-plugins.mdoc');
            }),
            bound,
        );

        methods.effect(
            'reads the same graph with plugin isolation off as with it on',
            Effect.fnUntraced(function* () {
                const { called } = yield* Consumer;
                const [isolated, joined] = yield* Effect.forEach(['true', 'false'], (flag) => called('env', [`NX_ISOLATE_PLUGINS=${flag}`, 'NX_CACHE_PROJECT_GRAPH=false', 'nx', 'graph', '--print']));
                assert.strictEqual(joined?.code, 0, joined?.stderr);
                assert.strictEqual(joined?.stdout, isolated?.stdout, 'nrwl/nx packages/nx/src/project-graph/plugins/isolation/isolated-plugin.spec.ts: a plugin gives one result in the Nx process and in a plugin worker');
            }),
            bound,
        );

        methods.effect(
            `runs ${tools.join(' and ')} for a cold graph read and not once for a warm read of unchanged files`,
            Effect.fnUntraced(function* () {
                const cold = yield* counted(tools, 'nx reset >/dev/null 2>&1; ');
                const warm = yield* counted(tools, '');
                assert.strictEqual(warm.code, 0, warm.stderr);
                assert.isAbove(cold.runs, 0, `${report}: the graph comes from the tool report`);
                assert.strictEqual(warm.runs, 0, 'nrwl/nx packages/dotnet/src/analyzer/analyzer-client.spec.ts "should reuse a cached result whose evaluation inputs are unchanged"');
            }),
            bound,
        );

        methods.effect(
            'keeps every node as it was when a file no member reads appears',
            Effect.fnUntraced(function* () {
                const { nx, wrote } = yield* Consumer;
                const first = yield* nx('graph', '--print');
                yield* wrote({ 'docs/notes.md': 'No member reads this file\n' });
                const second = yield* nx('graph', '--print');
                assert.strictEqual(second.stdout, first.stdout, 'nrwl/nx packages/dotnet/src/analyzer/analyzer-client.spec.ts "should reuse a cached result whose evaluation inputs are unchanged"');
            }),
            bound,
        );

        methods.effect(
            `writes no file into the workspace while reading the graph with ${['every ignored file', ...unlocked].join(', ')} absent`,
            Effect.fnUntraced(function* () {
                const { nx, called, cleaned, written } = yield* Consumer;
                yield* cleaned;
                yield* called('git', ['rm', '--quiet', '--cached', '-r', '--ignore-unmatch', '--', '.nx/spies', ...unlocked]);
                yield* called('rm', ['-rf', ...unlocked]);
                yield* nx('reset');
                const [shown, created] = yield* written(nx('show', 'projects'));
                yield* called('git', ['reset', '--quiet', '--hard']);
                assert.strictEqual(shown.code, 0, shown.stderr);
                assert.deepStrictEqual(Array.difference(created, under(created, scratch)), [], 'nrwl/nx astro-docs/src/content/docs/kb/performant-project-graph-plugins.mdoc; README.md PRINCIPLES "Caches sit under .cache/<tool> and generated output under .artifacts/<tool>"');
            }),
            bound,
        );

        methods.effect(
            'gives a warm graph the member and the edge a manifest edit adds',
            Effect.fnUntraced(function* () {
                yield* placed;
                yield* added;
                const [stated, { roots, edges }] = yield* Effect.all([members, placed]);
                const source = `${report}; nrwl/nx packages/dotnet/src/analyzer/analyzer-client.spec.ts "should rerun the analyzer when an evaluation input changed"`;
                assert.sameMembers(
                    Object.values(roots),
                    stated.map(({ root }) => root),
                    source,
                );
                assert.sameMembers(Array.dedupe(edges.map(({ source: from, target }) => `${roots[from]} -> ${roots[target]}`)), pairs(stated), source);
            }),
            bound,
        );
    });

    it.layer(
        workspace(refused),
        options,
    )((methods) => {
        methods.effect(
            `names ${Record.keys(refused).join(' and ')} in the graph failure once the tool refuses it, and keeps every member the tool still reports`,
            Effect.fnUntraced(function* () {
                const [stated, { roots, failures }] = yield* Effect.all([members, placed]);
                const source = 'nrwl/nx packages/nx/src/project-graph/plugins/utils.spec.ts "should handle partial errors"; packages/nx/src/project-graph/error-types.ts AggregateCreateNodesError';
                assert.includeMembers(
                    failures.flatMap(({ files }) => files).map(({ file }) => file),
                    Record.keys(refused).slice(0, 1),
                    source,
                );
                assert.sameMembers(
                    Object.values(roots),
                    stated.map(({ root }) => root),
                    `${report}; ${source}`,
                );
            }),
            bound,
        );

        methods.effect(
            'reads a whole graph again once the refused manifest is gone',
            Effect.fnUntraced(function* () {
                const { called } = yield* Consumer;
                yield* called('rm', ['-rf', ...Record.keys(refused)]);
                const { failures } = yield* placed;
                assert.deepStrictEqual(failures, [], 'nrwl/nx packages/dotnet/src/analyzer/analyzer-client.spec.ts "should not cache a cancelled run": no failed read is stored');
            }),
            bound,
        );
    });

    it.layer(
        workspace(),
        options,
    )((methods) => {
        methods.effect(
            'removes with clean every file the tool reports for a build, and restores each from the cache on the next build',
            Effect.fnUntraced(function* () {
                const [{ nx, absent }, stated] = yield* Effect.all([Consumer, member(edits.dependent)]);
                const missed = yield* tooled(stated.name, 'build');
                const run = (verb: string) => nx(verb, stated.name, '--output-style=static');
                const first = yield* run('build');
                assert.strictEqual(first.code, 0, `${first.stdout}${first.stderr}`);
                const files = yield* built(stated);
                assert.isNotEmpty(files, report);
                assert.deepStrictEqual(yield* absent(files), [], report);
                const cleaned = yield* run('clean');
                assert.strictEqual(cleaned.code, 0, `${cleaned.stdout}${cleaned.stderr}`);
                assert.sameMembers([...(yield* absent(files))], [...files], 'README.md TASKS "clean removes the outputs a project\'s targets declare and runs its tool\'s clean"');
                const again = yield* run('build');
                assert.deepStrictEqual(missed(again.stdout), [], 'nrwl/nx e2e/dotnet/src/dotnet-cache.test.ts "should rebuild after clean even with cache available"');
                assert.deepStrictEqual(yield* absent(files), [], 'nrwl/nx e2e/dotnet/src/dotnet-cache.test.ts "should store and use local cache for builds"');
            }),
            bound,
        );

        methods.effect(
            'keeps every file a build wrote through a graph read holding no stored plugin result',
            Effect.fnUntraced(function* () {
                const [{ nx, absent }, stated] = yield* Effect.all([Consumer, member(edits.dependent)]);
                const run = yield* nx('build', stated.name, '--output-style=static');
                assert.strictEqual(run.code, 0, `${run.stdout}${run.stderr}`);
                const files = yield* built(stated);
                assert.isNotEmpty(files, report);
                yield* nx('reset');
                const shown = yield* nx('show', 'projects');
                assert.strictEqual(shown.code, 0, shown.stderr);
                assert.deepStrictEqual(yield* absent(files), [], 'nrwl/nx packages/nx/src/project-graph/plugins/public-api.ts CreateNodesFunction: a plugin reads configuration files and returns project configuration, and graph creation removes no file a task wrote');
            }),
            bound,
        );

        methods.effect(
            'declares as a build output no path a build of every member leaves unwritten',
            Effect.fnUntraced(function* () {
                const [{ nx, cleaned, written, outputs }, { tasks }] = yield* Effect.all([Consumer, placed]);
                yield* cleaned;
                const [run, created] = yield* written(nx('run-many', '--targets=build', '--skip-nx-cache', '--output-style=static'));
                assert.strictEqual(run.code, 0, `${run.stdout}${run.stderr}`);
                const declared = yield* Effect.forEach(
                    tasks.filter(({ target }) => target.startsWith('build--')),
                    ({ id }) => outputs(id),
                );
                const stated = declared.flatMap(({ outputPaths }) => outputPaths);
                assert.deepStrictEqual(
                    stated.filter((output) => Array.isReadonlyArrayEmpty(under(created, [output]))),
                    [],
                    'nrwl/nx astro-docs/src/content/docs/features/cache-task-results.mdoc: outputs are the files a task writes',
                );
            }),
            bound,
        );

        methods.effect.each(edits.verbs)(
            'misses the %s cache of a member after a source edit in a member it depends on, and hits it after an edit to files the tool reads for neither',
            Effect.fnUntraced(function* (verb) {
                const [{ nx, wrote }, { name }] = yield* Effect.all([Consumer, member(edits.dependent)]);
                const [run, missed] = [nx(verb, name, '--output-style=static'), yield* tooled(name, verb)];
                yield* run;
                assert.deepStrictEqual(missed((yield* run).stdout), [], 'nrwl/nx e2e/dotnet/src/dotnet-cache.test.ts "should store and use local cache for builds"');
                yield* wrote(edits.upstream(verb));
                const edited = yield* run;
                assert.strictEqual(edited.code, 0, `${edited.stdout}${edited.stderr}`);
                assert.isNotEmpty(missed(edited.stdout), 'nrwl/nx e2e/gradle/src/gradle-task-dependencies.test.ts "should not serve a cache hit after the dependency changes"');
                yield* wrote(edits.outside(verb));
                assert.deepStrictEqual(missed((yield* run).stdout), [], `${report}; nrwl/nx e2e/gradle/src/gradle-task-dependencies.test.ts "should still hit cache when an unrelated file changes"; e2e/gradle/src/gradle-cache.test.ts "should rebuild on source and resource changes and hit cache otherwise"`);
            }),
            bound,
        );

        methods.effect(
            'passes the tests of a member, replays a rerun from the cache, and fails once a test fails',
            Effect.fnUntraced(function* () {
                const [{ nx, called, wrote }, { name }] = yield* Effect.all([Consumer, member(edits.tested)]);
                const [run, missed] = [nx('test', name, '--output-style=static'), yield* tooled(name, 'test')];
                const first = yield* run;
                assert.strictEqual(first.code, 0, `${first.stdout}${first.stderr}`);
                assert.deepStrictEqual(missed((yield* run).stdout), [], 'nrwl/nx e2e/dotnet/src/dotnet-cache.test.ts "should handle test execution with fully cached dependency chain"');
                yield* wrote(edits.failing);
                const failing = yield* run;
                yield* called('git', ['checkout', '--', ...Record.keys(edits.failing)]);
                assert.notStrictEqual(failing.code, 0, 'nrwl/nx e2e/dotnet/src/dotnet.test.ts "should run tests"');
            }),
            bound,
        );

        methods.effect(
            'runs the build, test, and check of every member in one parallel run',
            Effect.fnUntraced(function* () {
                const { nx } = yield* Consumer;
                const run = yield* nx('run-many', '--targets=build,test,check', '--skip-nx-cache', '--parallel=8', '--output-style=static');
                assert.strictEqual(run.code, 0, `nrwl/nx e2e/dotnet/src/dotnet-cache.test.ts "should use cache when running multiple builds": ${run.stdout}${run.stderr}`);
            }),
            bound,
        );

        methods.effect(
            'stores the build of a member in a remote cache server and replays it from there into an empty local cache',
            Effect.fnUntraced(function* () {
                const [{ root, called }, { name }, { address, requests }] = yield* Effect.all([Consumer, member(edits.dependent), remote(token, Function.flow(answered, Option.some))]);
                const missed = yield* tooled(name, 'build');
                const run = (folder: string) => called('env', [`NX_SELF_HOSTED_REMOTE_CACHE_SERVER=${address}`, `NX_SELF_HOSTED_REMOTE_CACHE_ACCESS_TOKEN=${token}`, `NX_CACHE_DIRECTORY=${root}/.nx/${folder}`, 'nx', 'build', name, '--output-style=static']);
                const first = yield* run('stored');
                const stored = requests.filter((request) => request.startsWith('PUT '));
                const second = yield* run('emptied');
                const source = 'nrwl/nx e2e/nx/src/cache-no-daemon.test.ts http remote cache "should PUT and GET cache from remote cache"; packages/nx/src/native/cache/http_remote_cache.rs: store sends PUT /v1/cache/<hash>, retrieve sends GET';
                assert.strictEqual(first.code, 0, `${first.stdout}${first.stderr}`);
                assert.isNotEmpty(stored, source);
                assert.deepStrictEqual(
                    Array.difference(
                        stored.map((request) => request.replace('PUT ', 'GET ')),
                        requests.slice(requests.lastIndexOf(stored.at(-1) ?? 'PUT ')),
                    ),
                    [],
                    source,
                );
                assert.deepStrictEqual(missed(second.stdout), [], `${source}: ${second.stdout}${second.stderr}`);
            }, Effect.scoped),
            bound,
        );
    });

    it.layer(
        workspace(),
        options,
    )((methods) => {
        methods.effect(
            'tags release every member the tool packages',
            Effect.fnUntraced(function* () {
                const [stated, { projects }] = yield* Effect.all([members, placed]);
                assert.includeMembers(
                    projects.filter(({ data }) => (data.tags ?? []).includes('release')).map(({ name }) => name),
                    stated.filter(({ released }) => released).map(({ name }) => name),
                    `${report}; README.md PRINCIPLES "Plugins tag a project they give release configuration release"`,
                );
            }),
            bound,
        );

        methods.effect(
            'resolves the current version of each released member from the manifest stating it, and changes no file under --dry-run',
            Effect.fnUntraced(function* () {
                const [{ nx, called, text }, stated] = yield* Effect.all([Consumer, members]);
                const shown = yield* nx('release', 'version', next, '--dry-run');
                assert.strictEqual(shown.code, 0, `${shown.stdout}${shown.stderr}`);
                const open = stated.filter(({ released }) => released);
                const texts = yield* Effect.forEach(open, ({ manifest }) => text(manifest));
                const read = Array.zipWith(open, texts, ({ manifest, version }, stating) => (stating.includes(version) ? [`Resolved the current version as ${version} from manifest: ${manifest}`] : []));
                assert.deepStrictEqual(
                    read.flat().filter((line) => !`${shown.stdout}${shown.stderr}`.includes(line)),
                    [],
                    `${report}; nrwl/nx packages/nx/src/command-line/release/version/resolve-current-version.ts "disk"; version/test-utils.ts ExampleRustVersionActions readCurrentVersionFromSourceManifest`,
                );
                const changed = yield* called('git', ['status', '--porcelain']);
                assert.strictEqual(changed.stdout, '', 'nrwl/nx packages/js/src/release/utils/update-lock-file.ts: a dry run writes no manifest and no lock');
            }),
            bound,
        );

        methods.effect(
            published ? 'gives every released member a publish target that waits on the publish of each released member it depends on, and no other member one' : 'gives no member a publish target',
            Effect.fnUntraced(function* () {
                const [{ planned }, stated] = yield* Effect.all([Consumer, members]);
                const { tasks, dependencies } = yield* planned('run-many', `--targets=${PUBLISH.slice(1)}`);
                const open = stated.filter(({ released }) => released && published);
                const source = `${report}; README.md DELIVERY "Publishing delivers those files by each destination's own command"; TASKS "Attach and publish targets depend on the targets that package their files"; nrwl/nx e2e/release/src/private-js-packages.test.ts "should skip private packages"`;
                assert.sameMembers(
                    Record.keys(tasks).filter((id) => id.endsWith(PUBLISH)),
                    open.map(({ name }) => `${name}${PUBLISH}`),
                    source,
                );
                assert.sameMembers(awaited(dependencies), waits(open), source);
            }),
            bound,
        );

        methods.effect(
            'writes the new version where the tool reads it, and changes nothing else in a manifest',
            Effect.fnUntraced(function* () {
                const { nx, text } = yield* Consumer;
                const versioned = yield* nx('release', 'version', next);
                assert.strictEqual(versioned.code, 0, `${versioned.stdout}${versioned.stderr}`);
                const stated = yield* members;
                assert.deepStrictEqual(
                    stated.filter(({ released, version }) => released && version !== next).map(({ name, version }) => `${name} ${version}`),
                    [],
                    `${report}; nrwl/nx packages/nx/src/command-line/release/version/test-utils.ts ExampleRustVersionActions updateProjectVersion`,
                );
                const kept = manifests(next);
                assert.deepStrictEqual(yield* Effect.forEach(Record.keys(kept), text), Object.values(kept), 'nrwl/nx packages/js/src/release/version-actions.spec.ts "updateProjectVersion": formatting and other fields kept');
            }),
            bound,
        );
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { conforms };
