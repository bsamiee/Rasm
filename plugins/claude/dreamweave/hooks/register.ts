import { enforce } from '@rasm/claude-code/enforcement';
import { Policy } from '@rasm/policy/policy';
import { Call } from '@rasm/report';
import type { EngineInterface, On } from 'claude-code';
import { Array, Effect, Equal, Glob, HashSet, Match, Option, Predicate, Record, Ref, Schema, Semaphore } from 'effect';
import { skills } from '#/hooks/skills';

// --- [TYPES] ---------------------------------------------------------------------------

type Table = ConstructorParameters<typeof Policy>[number];
type Rows = Table[string];
type Script = Extract<Rows, { readonly action: 'Shell'; readonly decide: unknown }>;
type Invocation = Parameters<Script['decide']>[1]['invocations'][number];
type Mcp = Parameters<Extract<Rows, { readonly action: 'Mcp'; readonly decide: unknown }>['decide']>[0];
type Agent = Mcp['agent'];
type Memory = ReturnType<typeof compiled>;
type Opened = HashSet.HashSet<{ readonly agent: Agent; readonly memory: string }>;
type Said = Extract<ReturnType<Extract<Rows, { readonly action: 'Mcp'; readonly decide: unknown }>['decide']>, readonly unknown[]>[number];
type Note = Extract<Said, { readonly note: string }>;
interface Form {
    readonly program: string;
    readonly words: readonly string[];
    readonly flags: readonly string[];
}
interface Reach {
    readonly paths: readonly string[];
    readonly invocations: readonly Invocation[];
    readonly tool: Option.Option<Pick<Mcp, 'server' | 'tool' | 'input'>>;
    readonly output: Option.Option<string>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const INDEX = 'MEMORY.md';
const MARKDOWN = /\.md$/u;
const GENERATED = `After writing the file, stop: \`${INDEX}\` is generated from each memory file's \`description\` after every write, one line per file, and loaded into context each session, so never write or edit \`${INDEX}\` itself. Keep each \`description\` to one specific line of at most 150 characters, as it is that file's line in the index.`;
const POINTER: readonly (readonly [RegExp, string])[] = [
    [new RegExp(`^(?:After writing the file, add a one-line pointer in \`${RegExp.escape(INDEX)}\`|\\*\\*Step 2\\*\\* — add a pointer to that file in ).*$`, 'mu'), GENERATED],
    [/^Saving a memory is a two-step process:\n\n\*\*Step 1\*\* — write /mu, 'Write '],
    [new RegExp(`^- \`${RegExp.escape(INDEX)}\` is always loaded into your conversation context.*\\n`, 'mu'), ''],
];

// --- [MODELS] --------------------------------------------------------------------------

const Texts = Schema.Array(Schema.String);
const Tools = Schema.Array(Schema.Struct({ name: Schema.String, input: Schema.Record(Schema.String, Texts) }));
const form = (written: string): Form => {
    const [program = written, ...rest] = written.split(' ');
    return { program, words: rest.filter((word) => !word.startsWith('-')), flags: rest.filter((word) => word.startsWith('-')) };
};
const named = (tools: typeof Tools.Type) => tools.map(({ name, input }) => ({ name: Glob.make(name, { dot: true }), input }));
const Row = Schema.Struct({
    memory: Schema.String,
    description: Schema.String,
    file: Schema.String,
    triggers: Schema.Struct({ paths: Texts, programs: Texts, tools: Tools, output: Texts, refuse: Schema.Struct({ programs: Texts, tools: Tools }) }),
});
const compiled = ({ memory, description, file, triggers: { paths, programs, tools, output, refuse } }: typeof Row.Type) => ({
    memory,
    description,
    file,
    paths: paths.map((path) => Glob.make(path, { dot: true })),
    programs: programs.map(form),
    tools: named(tools),
    output,
    refuse: { programs: refuse.programs.map(form), tools: named(refuse.tools) },
});
const Memories = Row.pipe(Schema.Array, Schema.fromJsonString);

// --- [OPERATIONS] ----------------------------------------------------------------------

function real($: EngineInterface, path: string) {
    return Effect.promise(() =>
        $.fs
            .stat(path, { resolve: true })
            .then(({ realPath }) => realPath ?? path)
            .catch(() => path),
    );
}

function dirty($: EngineInterface, root: string) {
    return Effect.map(
        Effect.promise(() => $.process.run(['git', '-C', root, 'status', '--porcelain'])),
        ({ exitCode, stdout }) => exitCode !== 0 || stdout.trim() !== '',
    );
}

function regenerate($: EngineInterface, root: string, memories: Ref.Ref<readonly Memory[]>) {
    return Effect.promise(() => Promise.all([$.session.root(), $.process.hookEnv()])).pipe(
        Effect.flatMap(([project, env]) => Effect.promise(() => $.process.run(['uv', 'run', '--locked', '--script', `${$.plugin.root}/skills/memory/scripts/index.py`, root, project, '--table'], { env, timeoutMs: 120_000 }))),
        Effect.tap(({ stdout }) => Option.match(Schema.decodeOption(Memories)(stdout), { onNone: () => Effect.void, onSome: (rows) => Ref.set(memories, rows.map(compiled)) })),
        Effect.map(({ exitCode, stderr }) => ({ failed: exitCode !== 0, report: stderr.trim() })),
    );
}

const runs = (invocation: Invocation, { program, words, flags }: Form) => {
    const stated = [...invocation.path, ...Array.getSomes(invocation.operands.map(({ word }) => word.text))];
    return invocation.runs(program) && words.every((word, at) => stated[at] === word) && flags.every((flag) => invocation.has(flag));
};
const ran = (forms: readonly Form[], { invocations }: Reach) => invocations.some((invocation) => forms.some((written) => runs(invocation, written)));
const touches = (globs: readonly Glob.Glob[], { paths }: Reach) => paths.some((path) => globs.some((glob) => Glob.match(glob, path)));
const holds = (texts: readonly string[], value: unknown) => Predicate.isString(value) && texts.some((text) => value.toLowerCase().includes(text.toLowerCase()));
const calls = (tools: ReturnType<typeof named>, reach: Reach) => Option.exists(reach.tool, ({ server, tool, input }) => tools.some(({ name, input: wanted }) => Glob.match(name, `mcp__${server}__${tool}`) && Record.every(wanted, (texts, field) => holds(texts, input[field]))));
const touched = (invocations: readonly Invocation[]) => invocations.flatMap(({ reads, writes }) => [...reads, ...writes].map((field) => Option.getOrElse(field.text, () => field.prefix)));
const told = (memories: Ref.Ref<readonly Memory[]>, read: Ref.Ref<Opened>) =>
    Effect.fnUntraced(function* (agent: Agent, reach: Reach): Effect.fn.Return<readonly Said[]> {
        const [held, opened] = yield* Effect.all([Ref.get(memories), Ref.get(read)]);
        yield* Ref.update(read, HashSet.union(HashSet.fromIterable(held.filter(({ file }) => reach.paths.includes(file)).map(({ memory }) => ({ agent, memory })))));
        return held.flatMap((each): readonly Said[] => {
            const said = { subject: `memory ${each.memory}`, next: new Call({ tool: 'Read', given: { file_path: each.file } }) };
            return Match.value({
                refused: !HashSet.has(opened, { agent, memory: each.memory }) && (ran(each.refuse.programs, reach) || calls(each.refuse.tools, reach)),
                pointed: touches(each.paths, reach) || ran(each.programs, reach) || calls(each.tools, reach) || Option.exists(reach.output, (text) => holds(each.output, text)),
            }).pipe(
                Match.when({ refused: true }, () => [{ ...said, deny: 'guards this call, and this agent has not read it', values: { held: each.description } }]),
                Match.when({ pointed: true }, () => [{ ...said, note: each.description, once: 'agent' as const }]),
                Match.orElse(() => []),
            );
        });
    });
const triggers = (heard: ReturnType<typeof told>): Table => {
    const none: Reach = { paths: [], invocations: [], tool: Option.none(), output: Option.none() };
    return {
        'memory.read': { action: 'Read', decide: ({ agent, path }) => heard(agent, { ...none, paths: [path] }) },
        'memory.write': { action: 'Write', decide: ({ agent, path }) => heard(agent, { ...none, paths: [path] }) },
        'memory.edit': { action: 'Edit', decide: ({ agent, path }) => heard(agent, { ...none, paths: [path] }) },
        'memory.shell': { action: 'Shell', decide: (_, { action: { agent }, invocations }) => heard(agent, { ...none, invocations, paths: touched(invocations) }) },
        'memory.tool': { action: 'Mcp', decide: ({ agent, server, tool, input }) => heard(agent, { ...none, tool: Option.some({ server, tool, input }) }) },
        'memory.output': {
            action: 'Output',
            decide: ({ agent, text, readOnly }) =>
                readOnly
                    ? []
                    : Effect.map(
                          heard(agent, { ...none, output: Option.some(text) }),
                          Array.filter((said): said is Note => 'note' in said),
                      ),
        },
    };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

function register(on: On) {
    const state: { root?: string } = {};
    const [memories, read, lock] = [Ref.makeUnsafe<readonly Memory[]>([]), Ref.makeUnsafe<Opened>(HashSet.empty()), Semaphore.makeUnsafe(1)];
    const queued = Semaphore.withPermit(lock);
    enforce(on, new Policy(triggers(told(memories, read))));
    skills(on);
    on('prompt.section', { name: 'memory' }, (_, e, next) =>
        next(e).then((answer) => {
            const text = answer.text === null ? null : POINTER.reduce((swapped, [pattern, replacement]) => swapped.replace(pattern, replacement), answer.text);
            return { ...answer, text: text !== null && text === answer.text ? `${text}\n\nThis Claude Code release words the memory section above in a form the dreamweave plugin does not recognize, so its steps may still name \`${INDEX}\`. ${GENERATED}` : text };
        }),
    );
    on('prompt.context', ($, e, next) =>
        Effect.runPromise(
            Effect.gen(function* () {
                const answer = yield* Effect.promise(() => next(e));
                const files = answer.instructionFiles ?? e.instructionFiles ?? [];
                const file = files.find(({ kind }) => kind === 'memory');
                if (file === undefined) {
                    return answer;
                }
                const root = (yield* real($, file.path)).slice(0, -INDEX.length);
                state.root = root;
                const [{ failed, report }, content] = yield* Effect.all([queued(regenerate($, root, memories)), Effect.promise(() => $.fs.read(file.path))]);
                return { blocks: failed ? [...answer.blocks, { name: 'memoryIndex', text: report }] : answer.blocks, instructionFiles: files.map((row) => (row === file ? { ...row, content } : row)) };
            }),
        ),
    );
    on('classic.PostCompact', (_, e, next) => {
        const agent = Option.fromUndefinedOr(e.agent_id);
        Effect.runSync(
            Ref.update(
                read,
                HashSet.filter((entry) => !Equal.equals(entry.agent, agent)),
            ),
        );
        return next(e);
    });
    on('tool.call', { tool: ['Write', 'Edit'], file_path: new RegExp(`${RegExp.escape(INDEX)}$`, 'u') }, ($, e, next) =>
        Effect.runPromise(
            Effect.gen(function* () {
                const path = e['file_path'];
                const target = Predicate.isString(path) ? yield* real($, path) : undefined;
                return state.root !== undefined && target === `${state.root}${INDEX}` ? { deny: `\`${INDEX}\` is generated from each memory file's \`description\` after every write: edit the \`description\` of the memory file instead, and its index line follows.` } : yield* Effect.promise(() => next(e));
            }),
        ),
    );
    on('tool.run', { tool: ['Write', 'Edit'], file_path: MARKDOWN }, ($, e, next) =>
        Effect.runPromise(
            Effect.gen(function* () {
                const [{ root }, path] = [state, e['file_path']];
                const answer = yield* Effect.promise(() => next(e));
                const target = Predicate.isString(path) ? yield* real($, path) : undefined;
                if (answer.deny !== undefined || root === undefined || target?.startsWith(root) !== true) {
                    return answer;
                }
                const { report } = yield* queued(regenerate($, root, memories));
                $.ui.invalidate('prompt.context');
                return { ...answer, context: [...(answer.context ?? []), report] };
            }),
        ),
    );
    on('classic.Stop', ($, e, next) =>
        Effect.runPromise(
            Effect.gen(function* () {
                const [answer, { root }] = [yield* Effect.promise(() => next(e)), state];
                if (root === undefined || !(yield* queued(dirty($, root)))) {
                    return answer;
                }
                const { report } = yield* queued(regenerate($, root, memories));
                $.ui.invalidate('prompt.context');
                yield* Effect.promise(() => $.session.append({ message: { type: 'user', content: [{ type: 'text', text: report }] } }));
                return answer;
            }),
        ),
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
