import { enforce } from '@rasm/claude-code/enforcement';
import { Engine, Notice } from '@rasm/claude-code/engine';
import { host } from '@rasm/claude-code/host';
import { Policy } from '@rasm/policy/policy';
import { Call } from '@rasm/report';
import type { On } from 'claude-code';
import { Array, Config, Duration, Effect, Equal, FileSystem, Glob, HashSet, Layer, Match, Option, Path, Predicate, Record, Ref, Schema, Semaphore, Stream } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
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

const real = (path: string) => FileSystem.FileSystem.use((fs) => fs.realPath(path)).pipe(Effect.orElseSucceed(() => path));
const spawned = (command: ChildProcess.Command) =>
    ChildProcessSpawner.ChildProcessSpawner.use((processes) =>
        Effect.scoped(Effect.flatMap(processes.spawn(command), (handle) => Effect.all({ stdout: handle.stdout.pipe(Stream.decodeText, Stream.mkString), stderr: handle.stderr.pipe(Stream.decodeText, Stream.mkString), exitCode: handle.exitCode }, { concurrency: 'unbounded' }))),
    );
const dirty = (root: string) => Effect.map(spawned(ChildProcess.make('git', ['-C', root, 'status', '--porcelain'])), ({ exitCode, stdout }) => exitCode !== 0 || stdout.trim() !== '');
const regenerate = Effect.fnUntraced(function* (root: string, memories: Ref.Ref<readonly Memory[]>) {
    const [engine, path, plugin, session] = yield* Effect.all([Engine, Path.Path, Config.String('CLAUDE_PLUGIN_ROOT'), Config.String('CLAUDE_CODE_SESSION_ID')]);
    const project = yield* engine.root;
    const { exitCode, stdout, stderr } = yield* spawned(ChildProcess.make('uv', ['run', '--locked', '--script', path.join(plugin, 'skills', 'memory', 'scripts', 'index.py'), root, project, '--table'], { env: { CLAUDE_CODE_SESSION_ID: session }, extendEnv: true })).pipe(Effect.timeout(Duration.minutes(2)));
    yield* Option.match(Schema.decodeOption(Memories)(stdout), { onNone: () => Effect.void, onSome: (rows) => Ref.set(memories, rows.map(compiled)) });
    return { failed: exitCode !== 0, report: stderr.trim() };
});

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
    const refreshed = (root: string) => Effect.tap(queued(regenerate(root, memories)), () => Engine.use((engine) => engine.invalidate('prompt.context')));
    host(
        on,
        {
            layer: Layer.empty,
            hooks: {
                'prompt.section': (e, next) =>
                    e.name === 'memory'
                        ? Effect.map(next(e), (answer) => {
                              const text = answer.text === null ? null : POINTER.reduce((swapped, [pattern, replacement]) => swapped.replace(pattern, replacement), answer.text);
                              return { ...answer, text: text !== null && text === answer.text ? `${text}\n\nThis Claude Code release words the memory section above in a form the dreamweave plugin does not recognize, so its steps may still name \`${INDEX}\`. ${GENERATED}` : text };
                          })
                        : next(e),
                'prompt.context': Effect.fnUntraced(function* (e, next) {
                    const answer = yield* next(e);
                    const files = answer.instructionFiles ?? e.instructionFiles ?? [];
                    const file = files.find(({ kind }) => kind === 'memory');
                    if (file === undefined) {
                        return answer;
                    }
                    const root = (yield* real(file.path)).slice(0, -INDEX.length);
                    state.root = root;
                    const [{ failed, report }, content] = yield* Effect.all([queued(regenerate(root, memories)), FileSystem.FileSystem.use((fs) => fs.readFileString(file.path))]);
                    return { blocks: failed ? [...answer.blocks, { name: 'memoryIndex', text: report }] : answer.blocks, instructionFiles: files.map((row) => (row === file ? { ...row, content } : row)) };
                }),
                'tool.call': Effect.fnUntraced(function* (e, next) {
                    const path = e['file_path'];
                    const target = (e.tool === 'Write' || e.tool === 'Edit') && Predicate.isString(path) ? Option.some(yield* real(path)) : Option.none();
                    const { root } = state;
                    if (root !== undefined && Option.contains(target, `${root}${INDEX}`)) {
                        return { deny: `\`${INDEX}\` is generated from each memory file's \`description\` after every write: edit the \`description\` of the memory file instead, and its index line follows.` };
                    }
                    const answer = yield* next(e);
                    if (answer.deny !== undefined || root === undefined || !Option.exists(target, (written) => written.endsWith('.md') && written.startsWith(root))) {
                        return answer;
                    }
                    const { report } = yield* refreshed(root);
                    return { ...answer, context: [...(answer.context ?? []), report] };
                }),
                'turn.complete': Effect.fnUntraced(function* (e, next) {
                    const [answer, { root }] = [yield* next(e), state];
                    if (root === undefined || !(yield* queued(dirty(root)))) {
                        return answer;
                    }
                    const { report } = yield* refreshed(root);
                    yield* Engine.use((engine) => engine.deliver(Notice.Note({ text: report, agent: Option.none() })));
                    return answer;
                }),
            },
        },
        skills,
        enforce(new Policy(triggers(told(memories, read)))),
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
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
