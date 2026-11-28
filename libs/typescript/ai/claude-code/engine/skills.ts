import type { Args, Frozen } from 'claude-code';
import { Array, Context, Effect, FileSystem, Formatter, Glob, HashMap, HashSet, Layer, Option, Path, type PlatformError, Predicate, Record, Ref, Result, Schema, SchemaGetter, SchemaIssue } from 'effect';
import { Yaml } from 'effect/encoding';
import { KeyValueStore } from 'effect/persistence';
import type { Part } from '#/engine/host';

// --- [TYPES] ---------------------------------------------------------------------------

interface Skill {
    readonly path: Option.Option<string>;
    readonly paths: readonly Glob.Glob[];
}
interface State {
    readonly loaded: HashMap.HashMap<Option.Option<string>, HashSet.HashSet<string>>;
    readonly skills: Record.ReadonlyRecord<string, Skill>;
}
interface Found {
    readonly file: string;
    readonly name: (read: typeof Frontmatter.Type) => string;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const COMMAND_NAME = /^<command-name>\/(?<name>[^<]+)<\/command-name>$/gmu;
const FRONTMATTER = /^---\r?\n(?<yaml>[^]*?)\r?\n---(?:\r?\n|$)/u;
const PATTERN_SEPARATOR = /\s*,\s*(?![^{]*\})/u;
const MANIFEST = '.claude-plugin/plugin.json';
const TEXT_PREFIX = 'skill:';

// --- [MODELS] --------------------------------------------------------------------------

const Paths = Schema.Union([Schema.String, Schema.String.pipe(Schema.Array)]);
const Manifest = Schema.fromJsonString(Schema.Struct({ skills: Paths.pipe(Schema.optional), agents: Paths.pipe(Schema.optional) }));
const Frontmatter = Schema.Struct({ name: Schema.String.pipe(Schema.optional), paths: Paths.pipe(Schema.optional), skills: Schema.String.pipe(Schema.Array, Schema.optional) });
const Definition = Schema.String.pipe(
    Schema.decodeTo(Schema.Unknown, {
        decode: SchemaGetter.transformEffect((text: string, options) =>
            Option.match(Option.fromNullishOr(FRONTMATTER.exec(text)?.groups?.['yaml']), {
                onNone: () => Effect.succeed({}),
                onSome: (yaml) => Effect.try({ try: (): unknown => Yaml.parse(yaml), catch: (cause) => new SchemaIssue.InvalidValue({ message: `frontmatter is no YAML: ${Formatter.format(cause)}` }, text, options) }),
            }),
        ),
        encode: SchemaGetter.forbiddenEncoding,
    }),
    Schema.decodeTo(Frontmatter),
);
const SkillCall = Schema.Struct({ tool: Schema.Literal('Skill'), agentId: Schema.String.pipe(Schema.optional), result: Schema.Struct({ success: Schema.Literal(true), commandName: Schema.String, agentId: Schema.String.pipe(Schema.optional) }) });

// --- [SERVICES] ------------------------------------------------------------------------

class Skills extends Context.Service<
    Skills,
    {
        readonly state: Effect.Effect<State>;
        readonly text: (skill: string) => Effect.Effect<Option.Option<string>, KeyValueStore.KeyValueStoreError>;
    }
>()('@rasm/claude-code/engine/skills') {
    static readonly #state = Ref.makeUnsafe<State>({ loaded: HashMap.empty(), skills: {} });
    static readonly #agents = Ref.makeUnsafe<Record.ReadonlyRecord<string, readonly string[]>>({});
    static readonly #load = (agent: Option.Option<string>, names: Iterable<string>) => Ref.update(Skills.#state, (held) => ({ ...held, loaded: HashMap.set(held.loaded, agent, HashSet.union(Option.getOrElse(HashMap.get(held.loaded, agent), HashSet.empty<string>), HashSet.fromIterable(names))) }));
    static readonly #read = (e: Frozen<Args<'plugin.register'>>) =>
        Effect.gen(function* () {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const { skills: folders = [], agents: files } = yield* fs.readFileString(path.join(e.root, MANIFEST)).pipe(
                Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed('{}')),
                Effect.flatMap(Schema.decodeEffect(Manifest)),
            );
            const agents = path.join(e.root, 'agents');
            const [globbed, nested] = yield* Effect.all(
                [
                    Effect.forEach(
                        [[e.root, 'SKILL.md'] as const, ...Array.dedupe(['skills', ...Array.ensure(folders)].map((folder) => path.resolve(e.root, folder))).map((folder) => [folder, '{SKILL.md,*/SKILL.md}'] as const)],
                        ([folder, pattern]) => Effect.map(fs.glob(pattern, { root: folder }), (found) => [folder, found] as const),
                        { concurrency: 'unbounded' },
                    ),
                    files === undefined ? fs.glob('**/*.md', { root: agents }) : Effect.succeed([]),
                ],
                { concurrency: 'unbounded' },
            );
            const decode = (kind: 'skills' | 'agents', found: readonly Found[]) => {
                const provided = HashSet.fromIterable(e.provides[kind]);
                return Effect.map(
                    Effect.filterMapEffect(
                        found,
                        ({ file, name }) =>
                            fs.readFileString(file).pipe(
                                Effect.flatMap(Schema.decodeEffect(Definition)),
                                Effect.map((read) => Result.succeed([name(read), file, read] as const)),
                                Effect.catchCause((cause) => Effect.as(Effect.logWarning('frontmatter unread', cause).pipe(Effect.annotateLogs({ plugin: e.name, file })), Result.failVoid)),
                            ),
                        { concurrency: 'unbounded' },
                    ),
                    Array.filter(([named]) => e.unread.includes(kind) || HashSet.has(provided, named)),
                );
            };
            const [skills, types] = yield* Effect.all(
                [
                    decode(
                        'skills',
                        globbed.flatMap(([folder, found]): readonly Found[] =>
                            found.includes('SKILL.md') ? [{ file: path.join(folder, 'SKILL.md'), name: ({ name = path.basename(folder) }) => (name.startsWith(`${e.name}:`) ? name : `${e.name}:${name}`) }] : found.map((file) => ({ file: path.join(folder, file), name: () => `${e.name}:${path.dirname(file)}` })),
                        ),
                    ),
                    decode('agents', [
                        ...nested.map((file): Found => ({ file: path.join(agents, file), name: ({ name = path.basename(file, '.md') }) => [e.name, ...file.split('/').slice(0, -1), name].join(':') })),
                        ...Array.ensure(files ?? []).map((file): Found => ({ file: path.resolve(e.root, file), name: ({ name = path.basename(file, '.md') }) => `${e.name}:${name}` })),
                    ]),
                ],
                { concurrency: 'unbounded' },
            );
            yield* Ref.update(Skills.#state, (held) => ({ ...held, skills: { ...held.skills, ...Record.fromEntries(skills.map(([name, at, read]) => [name, skill(Option.some(at), read)])) } }));
            yield* Ref.update(Skills.#agents, (held) => ({ ...held, ...Record.fromEntries(types.map(([name, , { skills: preloaded = [] }]) => [name, preloaded])) }));
        }).pipe(
            Effect.catchCause((cause) => Effect.logWarning('skills unread', cause).pipe(Effect.annotateLogs({ plugin: e.name, root: e.root }))),
            Effect.when(Path.Path.useSync((path) => path.isAbsolute(e.root))),
        );
    static readonly layer = Layer.effect(
        Skills,
        KeyValueStore.KeyValueStore.useSync((store) => ({
            state: Ref.get(Skills.#state),
            text: (name: string) => Effect.map(KeyValueStore.prefix(store, TEXT_PREFIX).get(name), Option.fromUndefinedOr),
        })),
    );
    static readonly hooks: Part<Skills, never, PlatformError.PlatformError>['hooks'] = {
        'session.end': (e, next) =>
            Effect.andThen(
                Ref.update(Skills.#state, (held) => ({ ...held, loaded: HashMap.empty() })),
                next(e),
            ),
        'plugin.register': (e, next) => Effect.tap(next(e), ({ refuse }) => Effect.when(Skills.#read(e), Effect.succeed(refuse === undefined))),
        'turn.start': (e, next) =>
            Effect.andThen(
                Skills.#load(
                    Option.fromUndefinedOr(e.agentId),
                    e.text.matchAll(COMMAND_NAME).flatMap(({ groups }) => Option.toArray(Option.fromUndefinedOr(groups?.['name']))),
                ),
                next(e),
            ),
        'agent.register': (e, next) => Effect.tap(next(e), ({ value }) => Effect.forEach(Option.toArray(Option.fromUndefinedOr(value)), ({ agent }) => Ref.update(Skills.#agents, (held) => ({ ...held, [agent]: e.skills ?? [] })), { discard: true })),
        'agent.spawn': (e, next) =>
            Effect.tap(next(e), (spawned) =>
                Effect.gen(function* () {
                    const [{ loaded }, types] = yield* Effect.all([Ref.get(Skills.#state), Ref.get(Skills.#agents)]);
                    const preloaded = e.fork ? Option.getOrElse(HashMap.get(loaded, Option.fromUndefinedOr(e.parentAgentId)), HashSet.empty<string>) : (types[e.subagentType] ?? []);
                    yield* Effect.forEach(Option.toArray(Option.fromUndefinedOr(spawned.agentId)), (agentId) => Skills.#load(Option.some(agentId), preloaded), { discard: true });
                }),
            ),
        'tool.call': (e, next) =>
            Effect.tap(next(e), (answer) =>
                Option.match(Schema.decodeUnknownOption(SkillCall)({ ...answer, tool: e.tool, agentId: e.agentId }), {
                    onNone: () => Effect.void,
                    onSome: ({ agentId, result }) => Skills.#load(Option.fromUndefinedOr(result.agentId ?? agentId), [result.commandName]),
                }),
            ),
        'skill.prompt': (e, next) =>
            Effect.tap(next(e), ({ text }) =>
                Effect.all(
                    [
                        Schema.decodeEffect(Frontmatter)(e.frontmatter).pipe(Effect.flatMap((read) => Ref.update(Skills.#state, (held) => ({ ...held, skills: { ...held.skills, [e.skill]: skill(Option.fromUndefinedOr(e.path), read) } })))),
                        KeyValueStore.KeyValueStore.use((store) => KeyValueStore.prefix(store, TEXT_PREFIX).set(e.skill, text)),
                    ],
                    {
                        concurrency: 'unbounded',
                        discard: true,
                    },
                ).pipe(Effect.catchCause((cause) => Effect.logWarning('skill unrecorded', cause).pipe(Effect.annotateLogs({ skill: e.skill })))),
            ),
    };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const skill = (path: Option.Option<string>, { paths = [] }: typeof Frontmatter.Type): Skill => ({ path, paths: (Predicate.isString(paths) ? paths.split(PATTERN_SEPARATOR) : paths).map((pattern) => Glob.make(pattern)) });

// --- [EXPORTS] -------------------------------------------------------------------------

export { Skills };
