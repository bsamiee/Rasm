import { type } from 'node:os';
import { basename, delimiter, join, posix, relative, resolve } from 'node:path';
import process from 'node:process';
import { fileURLToPath } from 'node:url';
import { normalizePath, readJsonFile } from '@nx/devkit';
import type { PackageJson } from '@nx/devkit/internal';
import { runner } from '@rasm/nx-distribution/inference';
import { Array, Cause, Context, Effect, FileSystem, Function, Layer, Match, Option, Order, pipe, Record, Result, Schema, SemVer } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Tree = Effect.Success<ReturnType<typeof resolved>>;
type Workflow = Tree['workflows'][number];

// --- [CONSTANTS] -----------------------------------------------------------------------

const PLUGIN = readJsonFile<PackageJson>(fileURLToPath(import.meta.resolve('#/package.json'))).name;
const EXPORTER = fileURLToPath(import.meta.resolve('#/coverage.cmake'));
const MANIFEST = 'CMakeLists.txt';
const PRESETS = 'CMakePresets.json';
const PACKAGING = 'CPackConfig.cmake';
const REGISTRY = { manifest: 'vcpkg.json', configuration: 'vcpkg-configuration.json' } as const;
const DEFINING = new Set(['add_library', 'add_executable', 'add_custom_target']);
const TIDY = /^CMAKE_[A-Z]+_CLANG_TIDY$/u;
const WORKING = '$env{PWD}';
const MACRO = /\$(?<space>env|penv)?\{(?<name>\w+)\}/gu;
const ENVIRONMENT = /\$env\{(?<name>\w+)\}/gu;
const TOKEN = /#\[(?<comment>=*)\[[\s\S]*?\]\k<comment>\]|#[^\n]*|\[(?<bracket>=*)\[[\s\S]*?\]\k<bracket>\]|"(?<quoted>(?:\\[\s\S]|[^"\\])*)"|(?<head>[A-Za-z_]\w*)[ \t]*\(|[^\s()#"]+|[()]/gu;

// --- [MODELS] --------------------------------------------------------------------------

const Packaging = Schema.Struct({ name: Schema.String, configurePreset: Schema.String });
const Launcher = Schema.Union([Schema.String, Schema.Struct({ value: Schema.String })]);
class Presets extends Schema.Opaque<Presets, unknown>()(
    Schema.fromJsonString(
        Schema.Struct({
            configurePresets: Schema.Array(
                Schema.Struct({
                    name: Schema.String,
                    inherits: Schema.optionalKey(Schema.Union([Schema.String, Schema.Array(Schema.String)])),
                    generator: Schema.optionalKey(Schema.String),
                    binaryDir: Schema.optionalKey(Schema.String),
                    cacheVariables: Schema.optionalKey(Schema.Record(Schema.String, Schema.Unknown)),
                    environment: Schema.optionalKey(Schema.Record(Schema.String, Schema.NullOr(Schema.String))),
                }),
            ).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
            packagePresets: Schema.Array(Packaging).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
            workflowPresets: Schema.Array(Schema.Struct({ name: Schema.String, steps: Schema.Array(Schema.Struct({ type: Schema.String, name: Schema.String })) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
        }),
    ),
) {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const inside = (folder: string) => (file: string) => !posix.relative(folder, file).startsWith('..');

const commands = (text: string) => {
    const [, tokens] = Array.mapAccum(
        [...text.matchAll(TOKEN)].filter(([token]) => !token.startsWith('#')),
        { depth: 0, call: -1 },
        ({ depth, call }, { 0: token, index, groups = {} }) => {
            const opened = depth === 0 && groups['head'] !== undefined;
            const next = { depth: Math.max(0, depth + Number(token.endsWith('(')) - Number(token === ')')), call: opened ? call + 1 : call };
            const value = groups['head'] ?? groups['quoted'] ?? token;
            const start = index + (groups['quoted'] === undefined ? 0 : 1);
            return [next, { call: next.call, name: opened ? Option.some(value.toLowerCase()) : Option.none(), argument: depth > 0 && token !== '(' && token !== ')' ? Option.some({ value, start, end: start + value.length }) : Option.none() }] as const;
        },
    );
    const argued = Array.groupBy(
        tokens.flatMap(({ call, argument }) => Option.toArray(Option.map(argument, (held) => ({ call, ...held })))),
        ({ call }) => String(call),
    );
    return tokens.flatMap(({ call, name }) => Option.toArray(Option.map(name, (named) => ({ name: named, args: argued[String(call)] ?? [] }))));
};

const versioned = (text: string, at: string) =>
    Effect.mapError(
        Effect.fromOption(
            pipe(
                Array.findFirst(commands(text), ({ name }) => name === 'project'),
                Option.flatMap(({ args }) =>
                    Option.flatMap(
                        Array.findFirstIndex(args, ({ value }) => value === 'VERSION'),
                        (keyword) => Array.get(args, keyword + 1),
                    ),
                ),
            ),
        ),
        (cause) => new Cause.UnknownError(cause, `${at} states no VERSION in a project() call, which nx release reads and writes`),
    );

const restated = Effect.fnUntraced(function* (text: string, version: string, at: string) {
    yield* Effect.fromResult(SemVer.parse(version)).pipe(
        Effect.filterOrFail((parsed) => !(SemVer.isPrerelease(parsed) || SemVer.hasBuild(parsed))),
        Effect.mapError((cause) => new Cause.UnknownError(cause, `${at} takes a major.minor.patch version alone in project(VERSION), which ${version} is not`)),
    );
    const { start, end } = yield* versioned(text, at);
    return `${text.slice(0, start)}${version}${text.slice(end)}`;
});

const expanded = (named: Readonly<Record<string, string>>, environment: Readonly<Record<string, string>>) => (text: string) =>
    text.replaceAll(MACRO, (macro, space: string | undefined, name: string): string =>
        Match.value(space).pipe(
            Match.when(Match.undefined, () => named[name] ?? macro),
            Match.when('env', () => Option.match(Record.get(environment, name), { onNone: () => macro, onSome: expanded(named, Record.remove(environment, name)) })),
            Match.orElse(() => `$env{${name}}`),
        ),
    );

const configured = (configure: string, defines: Readonly<Record<string, string>> = {}): Array.NonEmptyReadonlyArray<string> => ['cmake', '--preset', configure, '-S', '.', ...Record.collect(defines, (name, value) => `-D${name}=${value}`)];

const resolved = Effect.fnUntraced(function* (workspaceRoot: string, presets: string, matched: readonly string[]) {
    const fs = yield* FileSystem.FileSystem;
    const root = posix.dirname(presets);
    const source = join(workspaceRoot, root);
    const { configurePresets, packagePresets: packages, workflowPresets } = yield* Effect.flatMap(fs.readFileString(join(workspaceRoot, presets)), Schema.decodeEffect(Presets));
    const chain = (name: string): readonly Presets['configurePresets'][number][] => configurePresets.flatMap((preset) => (preset.name === name ? [preset, ...Array.ensure(preset.inherits ?? []).flatMap(chain)] : []));
    const macros = (configure: string, at: string) =>
        expanded(
            {
                sourceDir: at,
                sourceParentDir: `${at}/..`,
                sourceDirName: basename(source),
                presetName: configure,
                configurePresetName: configure,
                hostSystemName: process.platform === 'win32' ? 'Windows' : type(),
                fileDir: at,
                dollar: '$',
                pathListSep: delimiter,
                ...Record.getSomes({ generator: Array.findFirst(chain(configure), ({ generator }) => Option.fromUndefinedOr(generator)) }),
            },
            Record.getSomes(
                Record.map(
                    Record.fromEntries(
                        chain(configure)
                            .toReversed()
                            .flatMap(({ environment = {} }) => Record.toEntries(environment)),
                    ),
                    Option.fromNullOr,
                ),
            ),
        );
    const stepped = (listed: Presets['workflowPresets'][number]['steps'], kind: string) =>
        Option.map(
            Array.findFirst(listed, (each) => each.type === kind),
            (each) => each.name,
        );
    const followed = (listed: Presets['workflowPresets'][number]['steps']) =>
        Option.flatMap(
            Array.findFirstIndex(listed, (each) => each.type === 'test'),
            (at) => stepped(listed.slice(at + 1), 'build'),
        );
    const rooted = (configure: string) =>
        normalizePath(
            relative(
                workspaceRoot,
                resolve(
                    source,
                    Option.match(
                        Array.findFirst(chain(configure), ({ binaryDir }) => Option.fromUndefinedOr(binaryDir)),
                        { onNone: () => '.', onSome: macros(configure, source) },
                    ),
                ),
            ),
        ) || '.';
    const launchers = (configure: string) =>
        Record.collect(
            Record.filterMap(
                Record.fromEntries(
                    chain(configure)
                        .toReversed()
                        .flatMap(({ cacheVariables = {} }) => Record.toEntries(cacheVariables)),
                ),
                (stated, variable) => (TIDY.test(variable) ? Result.fromOption(Schema.decodeUnknownOption(Launcher)(stated), Function.constVoid) : Result.failVoid),
            ),
            (variable, named) => ({ variable, launcher: macros(configure, WORKING)(typeof named === 'string' ? named : named.value) }),
        );
    const workflows = Array.getSomes(
        workflowPresets.map(({ name, steps }) =>
            Option.map(stepped(steps, 'configure'), (configure) => ({
                build: Option.getOrUndefined(stepped(steps, 'build')),
                test: Option.getOrUndefined(stepped(steps, 'test')),
                after: Option.getOrUndefined(followed(steps)),
                tree: rooted(configure),
                name,
                configure,
                tidy: launchers(configure),
            })),
        ),
    );
    const trees = matched.filter((file) => posix.basename(file) === PRESETS && file !== presets && inside(root)(file)).map(posix.dirname);
    const inputs = matched.filter((file) => inside(root)(file) && !trees.some((nested) => inside(nested)(file)));
    const parsed = yield* Effect.forEach(
        inputs.filter((file) => posix.basename(file) === MANIFEST),
        (file) => Effect.map(fs.readFileString(join(workspaceRoot, file)), (text) => ({ file, folder: posix.dirname(file), calls: commands(text) })),
        { concurrency: 'unbounded' },
    );
    const invoked = parsed.flatMap(({ file, folder, calls }) => calls.map((call) => ({ file, folder, ...call })));
    const declared = Array.dedupeWith(
        invoked.flatMap(({ name, folder, args }) => (name === 'project' ? Array.take(args, 1).map(({ value }) => ({ name: value, folder, args })) : [])),
        (one, other) => one.folder === other.folder,
    );
    const placed = declared.filter(({ folder }) => folder === root || !declared.some((outer) => outer.folder !== root && outer.folder !== folder && inside(outer.folder)(folder)));
    const owner = (folder: string) =>
        Array.min(
            Array.prepend(
                placed.map((held) => held.folder).filter((at) => inside(at)(folder)),
                root,
            ),
            Order.mapInput(Order.Number, (at: string) => posix.relative(at, folder).length),
        );
    const defined = Record.fromEntries(invoked.filter(({ name }) => DEFINING.has(name)).flatMap(({ folder, args }) => Array.take(args, 1).map(({ value }) => [value, owner(folder)] as const)));
    const linked = invoked
        .map(({ file, folder, args }) => ({
            file,
            args,
            from: pipe(
                Array.head(args),
                Option.flatMap(({ value }) => Record.get(defined, value)),
                Option.getOrElse(() => owner(folder)),
            ),
        }))
        .flatMap(({ file, args, from }) => Array.drop(args, 1).map(({ value }) => ({ file, from, to: Record.get(defined, value) })))
        .flatMap(({ file, from, to }) => Option.toArray(Option.filter(to, (held) => held !== from)).map((target) => ({ from, root: target, file })));
    const members = placed.map(({ name, folder, args }) => ({
        name,
        root: folder,
        build: posix.relative(root, folder),
        compiled: folder !== root && !args.some(({ value }) => value === 'NONE'),
        packaged: args.some(({ value }) => value === 'VERSION'),
        links: Array.dedupe(linked.filter(({ from }) => from === folder).map(({ root: to, file }) => ({ root: to, file }))),
        registries: inputs.filter((file) => posix.basename(file) === REGISTRY.manifest && owner(posix.dirname(file)) === folder).map((manifest) => ({ folder: posix.dirname(manifest), inputs: Array.intersection([manifest, posix.join(posix.dirname(manifest), REGISTRY.configuration)], matched) })),
    }));
    return { root, workflows, packages, members, inputs, exporter: normalizePath(relative(source, EXPORTER)) };
});

// --- [SERVICES] ------------------------------------------------------------------------

class Linked extends Context.Service<Linked, Map<string, Tree['members']>>()(`${PLUGIN}/workspace/Linked`) {
    static readonly layer = Layer.sync(Linked, () => new Map());
}

// --- [COMPOSITION] ---------------------------------------------------------------------

const run = runner(Linked.layer);

// --- [EXPORTS] -------------------------------------------------------------------------

export { configured, ENVIRONMENT, Linked, MANIFEST, PACKAGING, PLUGIN, PRESETS, REGISTRY, resolved, restated, run, type Tree, versioned, WORKING, type Workflow };
