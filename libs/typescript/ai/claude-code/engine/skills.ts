import type { Args, Frozen } from 'claude-code';
import { Context, Effect, FileSystem, Formatter, Glob, HashMap, HashSet, Layer, Option, Path, type PlatformError, Predicate, Record, Ref, Result, Schema, SchemaGetter, SchemaIssue } from 'effect';
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

// --- [CONSTANTS] -----------------------------------------------------------------------

const COMMAND_NAME = /^<command-name>\/(?<name>[^<]+)<\/command-name>$/gmu;
const FRONTMATTER = /^---\r?\n(?<yaml>[^]*?)\r?\n---(?:\r?\n|$)/u;
const PATTERN_SEPARATOR = /\s*,\s*(?![^{]*\})/u;

// --- [MODELS] --------------------------------------------------------------------------

const Frontmatter = Schema.Struct({
    paths: Schema.Union([Schema.String, Schema.String.pipe(Schema.Array)]).pipe(Schema.optional),
    skills: Schema.String.pipe(Schema.Array, Schema.optional),
});
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
const SkillCall = Schema.Struct({ tool: Schema.Literal('Skill'), skill: Schema.String, agentId: Schema.optional(Schema.String) });

// --- [SERVICES] ------------------------------------------------------------------------

class Skills extends Context.Service<
    Skills,
    {
        readonly state: Effect.Effect<State>;
        readonly text: (skill: string) => Effect.Effect<Option.Option<string>, KeyValueStore.KeyValueStoreError>;
    }
>()('@rasm/claude-code/engine/skills') {
    static readonly #loaded = Ref.makeUnsafe(HashMap.empty<Option.Option<string>, HashSet.HashSet<string>>());
    static readonly #skills = Ref.makeUnsafe<Record.ReadonlyRecord<string, Skill>>({});
    static readonly #agents = Ref.makeUnsafe<Record.ReadonlyRecord<string, readonly string[]>>({});
    static readonly #load = (agent: Option.Option<string>, names: Iterable<string>) => Ref.update(Skills.#loaded, (held) => HashMap.set(held, agent, HashSet.union(Option.getOrElse(HashMap.get(held, agent), HashSet.empty<string>), HashSet.fromIterable(names))));
    static readonly #read = (e: Frozen<Args<'plugin.register'>>) =>
        Effect.gen(function* () {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const decode = (kind: 'skills' | 'agents', names: readonly string[], file: (segments: readonly string[]) => string) =>
                Effect.filterMapEffect(
                    names,
                    (name) => {
                        const at = file([e.root, kind, ...name.split(':').slice(1)]);
                        return fs.readFileString(at).pipe(
                            Effect.flatMap(Schema.decodeEffect(Definition)),
                            Effect.map((read) => Result.succeed([name, at, read] as const)),
                            Effect.catchCause((cause) => Effect.as(Effect.logWarning('frontmatter unread', cause).pipe(Effect.annotateLogs({ plugin: e.name, file: at })), Result.failVoid)),
                        );
                    },
                    { concurrency: 'unbounded' },
                );
            const [skills, agents] = yield* Effect.all([decode('skills', e.provides.skills, (segments) => path.join(...segments, 'SKILL.md')), decode('agents', e.provides.agents, (segments) => `${path.join(...segments)}.md`)], { concurrency: 'unbounded' });
            yield* Ref.update(Skills.#skills, (held) => Record.union(held, Record.fromEntries(skills.map(([name, at, read]) => [name, skill(Option.some(at), read)])), (_, read) => read));
            yield* Ref.update(Skills.#agents, (held) => Record.union(held, Record.fromEntries(agents.map(([name, , { skills: preloaded = [] }]) => [name, preloaded])), (_, read) => read));
        }).pipe(Effect.when(Path.Path.useSync((path) => path.isAbsolute(e.root))));
    static readonly layer = Layer.effect(
        Skills,
        KeyValueStore.KeyValueStore.useSync((store) => ({
            state: Effect.all({ loaded: Ref.get(Skills.#loaded), skills: Ref.get(Skills.#skills) }),
            text: (name: string) => Effect.map(store.get(`skill:${name}`), Option.fromUndefinedOr),
        })),
    );
    static readonly hooks: Part<Skills, never, PlatformError.PlatformError>['hooks'] = {
        'session.end': (e, next) => Effect.andThen(Ref.set(Skills.#loaded, HashMap.empty()), next(e)),
        'plugin.register': (e, next) => Effect.tap(next(e), ({ refuse }) => Effect.when(Skills.#read(e), Effect.succeed(refuse === undefined))),
        'turn.start': (e, next) =>
            Effect.andThen(
                Skills.#load(
                    Option.fromUndefinedOr(e.agentId),
                    e.text.matchAll(COMMAND_NAME).flatMap(({ groups }) => Option.toArray(Option.fromUndefinedOr(groups?.['name']))),
                ),
                next(e),
            ),
        'agent.register': (e, next) => Effect.tap(next(e), ({ value }) => Effect.forEach(Option.toArray(Option.fromUndefinedOr(value)), ({ agent }) => Ref.update(Skills.#agents, Record.set(agent, e.skills ?? [])), { discard: true })),
        'agent.spawn': (e, next) =>
            Effect.tap(next(e), (spawned) =>
                Effect.gen(function* () {
                    const [held, types] = yield* Effect.all([Ref.get(Skills.#loaded), Ref.get(Skills.#agents)]);
                    const preloaded = e.fork ? Option.getOrElse(HashMap.get(held, Option.fromUndefinedOr(e.parentAgentId)), HashSet.empty<string>) : Option.getOrElse(Record.get(types, e.subagentType), () => []);
                    yield* Effect.forEach(Option.toArray(Option.fromUndefinedOr(spawned.agentId)), (agentId) => Skills.#load(Option.some(agentId), preloaded), { discard: true });
                }),
            ),
        'tool.call': (e, next) =>
            Option.match(Schema.decodeUnknownOption(SkillCall)(e), {
                onNone: () => next(e),
                onSome: ({ skill: name, agentId }) => Effect.tap(next(e), ({ deny, isError }) => Effect.when(Skills.#load(Option.fromUndefinedOr(agentId), [name]), Effect.succeed(deny === undefined && isError !== true))),
            }),
        'skill.prompt': (e, next) =>
            Effect.tap(next(e), ({ text }) =>
                Effect.all([Schema.decodeEffect(Frontmatter)(e.frontmatter).pipe(Effect.flatMap((read) => Ref.update(Skills.#skills, Record.set(e.skill, skill(Option.fromUndefinedOr(e.path), read))))), KeyValueStore.KeyValueStore.use((store) => store.set(`skill:${e.skill}`, text))], {
                    concurrency: 'unbounded',
                    discard: true,
                }).pipe(Effect.catchCause((cause) => Effect.logWarning('skill unrecorded', cause).pipe(Effect.annotateLogs({ skill: e.skill })))),
            ),
    };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const skill = (path: Option.Option<string>, { paths = [] }: typeof Frontmatter.Type): Skill => ({ path, paths: (Predicate.isString(paths) ? paths.split(PATTERN_SEPARATOR) : paths).map((pattern) => Glob.make(pattern)) });

// --- [EXPORTS] -------------------------------------------------------------------------

export { Skills };
