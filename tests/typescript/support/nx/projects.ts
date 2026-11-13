import { Array, Effect, Option, Path, pipe, Record } from 'effect';
import { Consumer } from '#/nx/consumer';

// --- [TYPES] ---------------------------------------------------------------------------

interface Member {
    readonly root: string;
    readonly sources: readonly string[];
    readonly dependencies: readonly string[];
}

interface Build {
    readonly root: string;
    readonly read: readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const READERS = 6;

// --- [OPERATIONS] ----------------------------------------------------------------------

const placed = Effect.gen(function* () {
    const { read } = yield* Consumer;
    const { nodes, dependencies, failures } = yield* read;
    const projects = Object.values(nodes).filter(({ data }) => data.root !== '.');
    const roots = Record.fromIterableWith(projects, ({ name, data }) => [name, data.root]);
    const edges = projects.flatMap(({ name }) => (dependencies[name] ?? []).filter(({ target }) => target in roots));
    const targets = projects.flatMap(({ name, data }) => Record.keys(data.targets ?? {}).map((target) => ({ project: name, root: data.root, target, verb: target.split('--')[0] ?? target })));
    const held = (verb: string) => Array.dedupe(targets.filter(({ target }) => target === verb).map(({ root }) => root));
    return {
        projects,
        roots,
        names: projects.map(({ name }) => name),
        folders: Object.values(roots),
        targets,
        held,
        lacking: (verbs: readonly string[]) => verbs.flatMap((verb) => Array.difference(Object.values(roots), held(verb)).map((root) => `${root} lacks ${verb}`)),
        tagged: projects.filter(({ data }) => (data.tags ?? []).includes('release')).map(({ data }) => data.root),
        refused: failures.flatMap(({ files }) => files.map(({ file }) => file)),
        refusals: failures.flatMap(({ message, files }) => [message, ...files.map((file) => file.message)]).join('\n'),
        pairs: Array.dedupe(edges.map(({ source, target }) => `${roots[source]} -> ${roots[target]}`)),
        loose: edges.filter(({ type }) => type !== 'static').map(({ source, target, type }) => `${source} -> ${target} ${type}`),
    };
});

const named = Effect.fnUntraced(function* (root: string) {
    const { projects } = yield* placed;
    return yield* pipe(
        Array.findFirst(projects, ({ data }) => data.root === root),
        Option.map(({ name }) => name),
        Effect.fromOption,
        Effect.orDie,
    );
});

const ran = Effect.fnUntraced(function* (...args: readonly string[]) {
    const { nx } = yield* Consumer;
    const { code, stdout, stderr } = yield* nx(...args, '--output-style=static');
    return { code: Number(code), output: `${stdout}${stderr}` };
});

const released = Effect.fnUntraced(function* (...args: readonly string[]) {
    const { nx } = yield* Consumer;
    const { code, stdout, stderr } = yield* nx('release', ...args);
    return { code: Number(code), output: `${stdout}${stderr}` };
});

const tools = Effect.fnUntraced(function* (kinds: readonly string[]) {
    const [consumer, { targets }] = yield* Effect.all([Consumer, placed]);
    const chosen = targets.filter(({ target, verb }) => kinds.includes(verb) && target !== verb);
    const { tasks, dependencies } = yield* consumer.planned('run-many', `--targets=${Array.dedupe(chosen.flatMap(({ target, verb }) => [target, verb])).join(',')}`);
    return chosen.map(({ project, verb, target }) => ({
        id: `${project}:${target}`,
        cache: tasks[`${project}:${target}`]?.cache ?? false,
        waited: (dependencies[`${project}:${verb}`] ?? []).some(({ id }) => id === `${project}:${target}`),
    }));
});

const publishing = Effect.gen(function* () {
    const [consumer, { roots }] = yield* Effect.all([Consumer, placed]);
    const { tasks, dependencies } = yield* consumer.planned('run-many', '--targets=nx-release-publish');
    const rooted = (id: string) => roots[id.replace(':nx-release-publish', '')] ?? id;
    return {
        folders: Record.keys(tasks).map(rooted),
        cached: Record.keys(Record.filter(tasks, ({ cache = false }) => cache)),
        pairs: Record.collect(dependencies, (id, waited) => waited.map((dependency) => `${rooted(id)} -> ${rooted(dependency.id)}`)).flat(),
    };
});

const hashed = Effect.gen(function* () {
    const [{ inputs }, { projects, held }] = yield* Effect.all([Consumer, placed]);
    const builds = projects.filter(({ data }) => held('build').includes(data.root));
    return yield* Effect.forEach(builds, ({ name, data }) => Effect.map(inputs(`${name}:build`), ({ files }): Build => ({ root: data.root, read: files })));
});

const unread = Effect.gen(function* () {
    const [{ inputs }, cached] = yield* Effect.all([Consumer, tools(['build', 'test', 'check'])]);
    const hashes = yield* Effect.forEach(cached, ({ id }) => Effect.map(inputs(id), ({ files, runtime = [] }) => ({ id, files, runtime })));
    return hashes.flatMap(({ id, files, runtime }) => [...Array.intersection(files, ['.config/mise.lock']), ...runtime.filter((command) => command.includes('version'))].map((input) => `${id} hashes ${input}`));
});

const affected = Effect.fnUntraced(function* (file: string) {
    const [{ nx }, { roots }] = yield* Effect.all([Consumer, placed]);
    const { stdout } = yield* nx('show', 'projects', '--affected', `--files=${file}`, '--withTarget=build', '--json');
    return Object.values(Record.filter(roots, (_, name) => stdout.includes(`"${name}"`)));
});

const printed = Effect.gen(function* () {
    const { nx } = yield* Consumer;
    const { code, stdout, stderr } = yield* nx('graph', '--print');
    return { code: Number(code), graph: stdout, stderr };
});

const reprinted = Effect.gen(function* () {
    const { root, nx } = yield* Consumer;
    const first = yield* printed;
    yield* nx('reset');
    const second = yield* printed;
    return { root, first, second };
});

const seen = Effect.gen(function* () {
    const [{ graph }, inputs] = yield* Effect.all([printed, hashed]);
    return { graph, inputs };
});

const together = Effect.gen(function* () {
    const { nx } = yield* Consumer;
    yield* nx('reset');
    const shown = yield* Effect.all(
        Array.makeBy(READERS, () => nx('show', 'projects', '--json')),
        { concurrency: 'unbounded' },
    );
    return { failed: shown.filter(({ code }) => Number(code) !== 0).map(({ stderr }) => stderr), answers: Array.dedupe(shown.map(({ stdout }) => stdout.split(',').toSorted().join(','))) };
});

const spied = Effect.fnUntraced(function* (...commands: readonly string[]) {
    const [{ root, called, wrote, text }, path] = yield* Effect.all([Consumer, Path.Path]);
    const [log, spies] = [path.join(root, '.nx/spied.log'), path.join(root, '.nx/spies')];
    const real = yield* Effect.forEach(commands, (command) => Effect.map(called('which', [command]), ({ stdout }) => ({ command, at: stdout.trim() })));
    yield* wrote(Record.fromIterableWith(real, ({ command, at }) => [`.nx/spies/${command}`, `#!/bin/sh\necho ${command} >> '${log}'\nexec '${at}' "$@"\n`]));
    yield* called('chmod', ['-R', '+x', '.nx/spies']);
    const shown = Effect.fnUntraced(function* (reset: string) {
        yield* wrote({ '.nx/spied.log': '' });
        const read = yield* called('sh', ['-c', `${reset}PATH="${spies}:$PATH" nx show projects --json`]);
        const runs = yield* text('.nx/spied.log');
        return { code: Number(read.code), runs: runs.split('\n').filter((line) => line !== '').length };
    });
    const cold = yield* shown('nx reset >/dev/null 2>&1; ');
    const warm = yield* shown('');
    return { cold, warm };
});

const attaching = Effect.gen(function* () {
    const [consumer, { roots }] = yield* Effect.all([Consumer, placed]);
    const { tasks, dependencies } = yield* consumer.planned('run-many', '--targets=attach');
    const uploads = Record.keys(tasks).flatMap((id) => {
        const [project = id, tool] = id.split(':attach--');
        const packaged = (dependencies[id] ?? []).some((waited) => waited.id.startsWith(`${project}:pack--`) || waited.id.startsWith(`${project}:build--`));
        return tool === undefined ? [] : [{ id, root: roots[project] ?? project, packaged }];
    });
    return { folders: Array.dedupe(uploads.map(({ root }) => root)), loose: uploads.filter(({ packaged }) => !packaged).map(({ id }) => id) };
});

const swept = Effect.fnUntraced(function* (...verbs: readonly string[]) {
    const { nx, cleaned, written, absent } = yield* Consumer;
    yield* cleaned;
    yield* nx('show', 'projects');
    const [run, created] = yield* written(ran('run-many', `--targets=${verbs.join(',')}`, '--skip-nx-cache'));
    const clean = yield* ran('run-many', '--targets=clean');
    const gone = yield* absent(created);
    return { run, clean, created, left: Array.difference(created, gone) };
});

const resolved = (output: string) =>
    output
        .split('\n')
        .filter((line) => line.includes('Resolved the current version as'))
        .map((line) => line.slice(line.indexOf('Resolved')).trim());

const reach = (members: readonly Member[], root: string, held: ReadonlySet<string> = new Set()): ReadonlySet<string> =>
    held.has(root)
        ? held
        : Option.match(
              Array.findFirst(members, (member) => member.root === root),
              { onNone: () => held.union(new Set([root])), onSome: ({ dependencies }) => dependencies.reduce<ReadonlySet<string>>((reached, dependency) => reach(members, dependency, reached), held.union(new Set([root]))) },
          );

const pairs = (members: readonly Member[]) => members.flatMap(({ root, dependencies }) => dependencies.map((dependency) => `${root} -> ${dependency}`));

const read = (builds: readonly Build[], root: string) => builds.filter((build) => build.root === root).flatMap((build) => build.read);

const omitted = (builds: readonly Build[], members: readonly Member[], shared: readonly string[]) => members.flatMap(({ root, sources }) => Array.difference([...shared, ...sources], read(builds, root)).map((file) => `${root} build omits ${file}`));

const foreign = (builds: readonly Build[], members: readonly Member[]) =>
    builds.flatMap(({ root, read: files }) => {
        const near = reach(members, root);
        const outside = members.filter((member) => !near.has(member.root)).flatMap(({ sources }) => sources);
        return Array.intersection(outside, files).map((file) => `${root} build reads ${file}`);
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { affected, attaching, foreign, hashed, named, omitted, pairs, placed, printed, publishing, ran, reach, released, reprinted, resolved, seen, spied, swept, together, tools, unread };
