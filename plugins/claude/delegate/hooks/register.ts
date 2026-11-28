import { Engine } from '@rasm/claude-code/engine';
import { Plugin, reply, Tool } from '@rasm/claude-code/events';
import { host } from '@rasm/claude-code/host';
import { Session } from '@rasm/claude-code/session';
import { ProcessTree, Writer } from '@rasm/process/tree';
import { Format, Line, Report } from '@rasm/report';
import { Shell } from '@rasm/shell';
import type { AgentReportResult, Frozen, On, SessionMessage, ToolCallInput, ToolCallResult, TurnStepInput } from 'claude-code';
import { Array, Cache, type Cause, Channel, Config, Context, Data, DateTime, Duration, Effect, Equal, Exit, Fiber, FileSystem, Layer, Match, Option, Order, Path, Predicate, Record, Result, Schedule, Schema, Stream, Struct } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Fault = Effect.Error<ReturnType<typeof execute>> | Effect.Error<ReturnType<typeof launch>> | Effect.Error<typeof table> | Cause.UnknownError;
type Ending = Data.TaggedEnum<{ Exited: { readonly code: number }; Failed: { readonly reason: string }; Limit: object; Quiet: { readonly sample: Sample } }>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const QUIET = { every: Duration.minutes(5), after: Duration.minutes(20) } as const;
const ANSWER = { bytes: 4 * 1024 ** 2, token: /^\S{1,64}$/u } as const;
const MODEL = ['-m', '--model'] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Ending = Data.taggedEnum<Ending>();
const Run = Schema.Struct({
    command: Schema.NonEmptyString.annotate({ description: 'One simple command running an outside agent: a literal program, its words, redirects, and $(...) inside words, with no pipeline, list, &, or leading assignment' }),
    description: Schema.NonEmptyString.annotate({ description: 'A few words naming the run, shown on its agent row' }),
    limit: Schema.DurationFromString.annotate({ description: 'Longest the run may take before it ends, as "90 minutes"' }),
});
const Prompt = Schema.fromJsonString(Run);
const Launched = Schema.Struct({ result: Schema.Struct({ agentId: Schema.String }) });
const Fields = Schema.Record(Schema.String, Schema.Json);
const Answer = Schema.fromJsonString(Fields);

class Held extends Schema.Class<Held, { readonly brand: unique symbol }>('Held')({
    agent: Schema.String,
    claude: Schema.Int,
    pid: Schema.Int,
    label: Schema.String,
    model: Schema.optionalKey(Schema.String),
    description: Schema.String,
    start: Schema.DateTimeUtcFromMillis,
}) {
    static readonly json = Schema.fromJsonString(Held);
}

class Sample extends Data.Class<{ readonly cpu: Duration.Duration; readonly write: Option.Option<Writer> }> {}

class Command extends Data.Class<{ readonly program: string; readonly words: readonly string[]; readonly files: readonly string[] }> {
    get label(): string {
        return [this.program, ...Array.take(this.words, 1)].join(' ');
    }

    get model(): Option.Option<string> {
        return Option.flatMap(
            Array.findFirstIndex(this.words, (word) => MODEL.some((flag) => word === flag)),
            (at) => Array.get(this.words, at + 1),
        );
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

class Runner extends Context.Service<
    Runner,
    {
        readonly runs: string;
        readonly programs: string;
        readonly claude: number;
        readonly tool: string;
        readonly agent: string;
        readonly runner: (agent: Option.Option<string>) => Effect.Effect<Option.Option<string>>;
    }
>()('@rasm/delegate/hooks/register/Runner') {
    static readonly layer = Layer.effect(
        Runner,
        Effect.gen(function* () {
            const [engine, path, plugin, { data, claude }] = yield* Effect.all([Engine, Path.Path, Plugin, Config.all({ data: Config.String('CLAUDE_PLUGIN_DATA'), claude: Config.Int('CLAUDE_PID') })]);
            const { tool, agent } = yield* Option.all({ tool: Record.get(plugin.tools, 'run'), agent: Record.get(plugin.agents, 'run') }).pipe(Effect.fromOption, Effect.orDie);
            const folders = { runs: path.join(data, 'runs'), programs: path.join(data, 'programs') };
            const types = yield* Cache.makeWith((id: string) => Effect.map(Effect.orElseSucceed(engine.agents, Record.empty), Record.get(id)), {
                capacity: Number.POSITIVE_INFINITY,
                timeToLive: (exit) => (Exit.isSuccess(exit) && Option.isSome(exit.value) ? Duration.infinity : Duration.zero),
            });
            const runner = (loop: Option.Option<string>) =>
                Option.match(loop, {
                    onNone: () => Effect.succeedNone,
                    onSome: (id) =>
                        Effect.map(Cache.get(types, id), (found) =>
                            Option.as(
                                Option.filter(found, ({ type }) => type === agent),
                                id,
                            ),
                        ),
                });
            return { ...folders, claude, tool, agent, runner };
        }),
    );
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const refusal = (reason: string, rows: readonly Line[] = []): ToolCallResult => ({ deny: Report.text(new Report({ lines: [new Line({ head: ['refused'], value: 'run', facts: [reason], rows })] })) });
const read = Effect.fnUntraced(function* (command: string, agent: Option.Option<string>) {
    const [session, shell] = yield* Effect.all([Session, Shell]);
    const opened = yield* session.shell(agent);
    const { dialect, cwd } = yield* opened.environment;
    const script = yield* shell.read(command, { dialect, cwd });
    const top = (yield* script.invocations).filter(({ within }) => Array.isReadonlyArrayEmpty(within));
    const refused = [
        { reason: 'no command', holds: Array.isReadonlyArrayEmpty(top) },
        { reason: 'more than one command', holds: top.length > 1 && top.some(({ stage }) => Option.isNone(stage)) },
        { reason: 'a pipeline', holds: top.some(({ stage }) => Option.isSome(stage)) },
        { reason: 'a background command', holds: top.some(({ background }) => background) },
        { reason: 'a leading assignment, which env states', holds: top.some(({ assigns }) => Array.isReadonlyArrayNonEmpty(assigns)) },
        { reason: 'a program named at run time', holds: top.some(({ name }) => Option.isNone(name)) },
    ].filter(({ holds }) => holds);
    const files = top.flatMap((invocation) => [...invocation.writes, ...invocation.words.map(({ field }) => field)].flatMap(({ text }) => Option.toArray(text)));
    const resolved = Array.dedupe(files.map((file) => opened.path.resolve(file)));
    const invoked = Option.flatMap(Array.head(top), (invocation) => Option.map(invocation.name, (program) => new Command({ program, words: Array.getSomes(Array.drop(invocation.words, 1).map(Struct.get('text'))), files: resolved })));
    return { script, exec: opened.exec, cwd, refused: refused.map(Struct.get('reason')), command: invoked };
});
const launch = Effect.fnUntraced(function* (run: typeof Run.Type, e: Frozen<ToolCallInput>) {
    const [engine, { agent: runner }] = yield* Effect.all([Engine, Runner]);
    const agent = Option.fromUndefinedOr(e.agentId);
    const parsed = yield* read(run.command, agent);
    const called = yield* Array.match(parsed.refused, {
        onEmpty: () => Effect.map(Schema.encodeEffect(Prompt)(run), (prompt) => ({ tool: 'Agent', input: { subagent_type: runner, prompt, description: run.description }, ...Option.match(agent, { onNone: () => ({}), onSome: (agentId) => ({ agentId }) }) })).pipe(Effect.flatMap(engine.callTool), Effect.asSome),
        onNonEmpty: () => Effect.succeedNone,
    });
    const [limits, agents] = yield* Effect.all([engine.limits, engine.agents]);
    const count = new Line({ head: ['agents'], value: `${Record.size(Record.filter(agents, ({ status }) => status === 'running'))} running`, facts: [`limit ${limits.concurrent}`] });
    return Option.match(called, {
        onNone: () => refusal(parsed.refused.join(', ')),
        onSome: (answer) =>
            Option.match(Schema.decodeUnknownOption(Launched)(answer), {
                onNone: () => refusal(answer.deny ?? answer.text ?? 'the Agent tool started no agent', [count]),
                onSome: ({ result }) => ({ result: Report.text(new Report({ lines: [new Line({ head: ['started'], value: Option.match(parsed.command, { onNone: () => run.command, onSome: Struct.get('label') }), facts: [`agent ${result.agentId}`], rows: [count] })] })) }),
            }),
    });
});
const sample = Effect.fnUntraced(function* (pid: number) {
    const tree = yield* ProcessTree.read;
    const own = yield* Effect.fromOption(Option.flatMap(tree.subtree(pid), ({ processes }) => Array.head(processes)));
    const written = yield* Writer.of(new ProcessTree({ processes: [own] }));
    return new Sample({ cpu: own.cpu, write: Array.head(written) });
});
const quiet = (pid: number) =>
    Stream.fromEffectSchedule(sample(pid), Schedule.spaced(QUIET.every)).pipe(
        Stream.sliding(Math.round(Duration.toMillis(QUIET.after) / Duration.toMillis(QUIET.every)) + 1),
        Stream.filter((window) => window.every((each) => Equal.equals(each, Array.headNonEmpty(window)))),
        Stream.runHead,
        Effect.flatMap(Effect.fromOption),
        Effect.map((window) => Ending.Quiet({ sample: Array.lastNonEmpty(window) })),
    );
const fields = (held: Readonly<Record<string, Schema.Json>>, outer: boolean): readonly string[] =>
    Record.collect(held, (key, value) =>
        Match.value(value).pipe(
            Match.when(Match.string, (text) => [ANSWER.token.test(text) ? `${key} ${text}` : `${key} ${Format.count(text.length, { one: 'char', other: 'chars' })}`]),
            Match.when(globalThis.Array.isArray, (items) => [`${key} ${Format.count(items.length, { one: 'item', other: 'items' })}`]),
            Match.when(Schema.is(Fields), (inner) => (outer ? fields(inner, false).map((field) => `${key}.${field}`) : [`${key} ${Format.count(Record.size(inner), { one: 'key', other: 'keys' })}`])),
            Match.orElse((scalar) => [`${key} ${JSON.stringify(scalar)}`]),
        ),
    ).flat();
const summary = (text: string): readonly Line[] => {
    const lines = text.split('\n').filter((line) => line.trim() !== '');
    return Match.value({ held: Option.getOrUndefined(Schema.decodeOption(Answer)(text)), lines }).pipe(
        Match.when({ held: Match.defined }, ({ held }) => Array.match(fields(held, true), { onEmpty: () => [], onNonEmpty: (all) => [new Line({ head: ['fields'], value: Array.headNonEmpty(all), facts: Array.tailNonEmpty(all) })] })),
        Match.when({ lines: (all: readonly string[]) => Predicate.isTupleOf(all, 1) && ANSWER.token.test(all[0]) }, () => [new Line({ head: ['text'], value: lines.join('') })]),
        Match.orElse(() => (Array.isReadonlyArrayEmpty(lines) ? [] : [new Line({ head: ['lines'], value: Format.count(lines.length) })])),
    );
};
const answers = Effect.fnUntraced(function* (files: readonly string[], start: DateTime.Utc, cwd: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const since = DateTime.toDate(DateTime.startOf(start, 'second'));
    const stated = yield* Effect.filterMapEffect(files, (file) =>
        fs.stat(file).pipe(
            Effect.map((info) => Result.succeed({ file, ...info })),
            Effect.orElseSucceed(() => Result.failVoid),
        ),
    );
    const fresh = stated.filter(({ type, mtime }) => type === 'File' && Option.exists(mtime, (date) => date >= since));
    return yield* Effect.forEach(fresh, ({ file, size }) => {
        const relative = path.relative(cwd, file);
        const shown = Option.getOrElse(Option.as(path.resolveWithin(cwd, relative), relative), () => file);
        return Effect.map(Number(size) <= ANSWER.bytes ? Effect.asSome(fs.readFileString(file)) : Effect.succeedNone, (text) => new Line({ head: ['answer'], value: shown, facts: [Format.bytes(size)], rows: Option.match(text, { onNone: Array.empty, onSome: summary }) }));
    });
});
const execute = Effect.fnUntraced(function* (run: typeof Run.Type, agent: string) {
    const [fs, path, runner] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Runner]);
    const parsed = yield* read(run.command, Option.some(agent));
    const command = yield* Effect.fromOption(Option.filter(parsed.command, () => Array.isReadonlyArrayEmpty(parsed.refused)));
    const start = yield* DateTime.now;
    const record = path.join(runner.runs, `${agent}.json`);
    const { ending, stderr } = yield* Effect.scoped(
        Effect.gen(function* () {
            const child = yield* parsed.exec(parsed.script);
            const held = Held.make({ agent, claude: runner.claude, pid: child.pid, label: command.label, ...Option.match(command.model, { onNone: () => ({}), onSome: (model) => ({ model }) }), description: run.description, start });
            yield* Effect.all([Effect.flatMap(Schema.encodeEffect(Held.json)(held), (text) => fs.writeFileString(record, text)), fs.writeFileString(path.join(runner.programs, encodeURIComponent(command.label)), '')], { discard: true });
            const output = yield* Effect.forkScoped(
                Effect.all(
                    [
                        Stream.runDrain(child.stdout),
                        child.stderr.pipe(
                            Stream.decodeText,
                            Stream.splitLines,
                            Stream.filter((line) => line.trim() !== ''),
                            Stream.runLast,
                        ),
                    ],
                    { concurrency: 'unbounded' },
                ),
            );
            const ended = yield* Effect.raceAll([Effect.match(child.exitCode, { onFailure: ({ message }) => Ending.Failed({ reason: message }), onSuccess: (code) => Ending.Exited({ code }) }), Effect.as(Effect.sleep(run.limit), Ending.Limit()), quiet(child.pid)]);
            return { ending: ended, stderr: Ending.$is('Exited')(ended) && ended.code !== 0 ? Effect.map(Fiber.await(output), Exit.match({ onFailure: () => Option.none<string>(), onSuccess: ([, last]) => last })) : Effect.succeedNone };
        }),
    );
    const [end, last, written] = yield* Effect.all([DateTime.now, stderr, Ending.$is('Exited')(ending) ? answers(command.files, start, parsed.cwd) : Effect.succeed<readonly Line[]>([])]);
    const said = Ending.$match(ending, {
        Exited: ({ code }) => ({ state: `exit ${code}`, rows: [] }),
        Failed: ({ reason }) => ({ state: 'failed', rows: [new Line({ head: ['reason'], value: reason })] }),
        Limit: () => ({ state: 'ended at limit', rows: [] }),
        Quiet: ({ sample: { cpu, write } }) => ({
            state: 'ended quiet',
            rows: [new Line({ head: ['cpu'], value: Format.duration(cpu), facts: [`unchanged over ${Format.duration(QUIET.after)}`] }), ...Option.toArray(Option.map(write, ({ path: file, modified }) => new Line({ head: ['last write'], value: Format.instant(modified), facts: [file] })))],
        }),
    });
    return Report.text(
        new Report({
            lines: [
                new Line({
                    head: ['run'],
                    value: command.label,
                    facts: [said.state, Format.duration(DateTime.distance(start, end))],
                    rows: [...Option.toArray(Option.map(command.model, (model) => new Line({ head: ['model'], value: model }))), ...written, ...said.rows, ...Option.toArray(Option.map(last, (line) => new Line({ head: ['stderr'], value: line })))],
                }),
            ],
        }),
    );
});
const answer = Effect.fnUntraced(function* (e: Frozen<TurnStepInput>, agent: string) {
    const [engine, { tool }] = yield* Effect.all([Engine, Runner]);
    const messages: readonly SessionMessage[] = yield* engine.messages(Option.some(agent));
    return yield* e.index === 0
        ? Effect.fromOption(Array.findFirst(messages, ({ role }) => role === 'user')).pipe(
              Effect.flatMap(({ text }) => Schema.decodeEffect(Prompt)(text)),
              Effect.flatMap(Schema.encodeEffect(Run)),
              Effect.map((input) => ({ answer: '', toolUses: [{ name: tool, input }] })),
          )
        : Effect.succeed({
              answer: Option.getOrElse(
                  Option.flatMapNullishOr(
                      Array.findLast(
                          messages.flatMap(({ toolUses }) => toolUses),
                          (use) => use.tool === tool,
                      ),
                      Struct.get('text'),
                  ),
                  () => 'run: no verdict',
              ),
              toolUses: [],
          });
});
const records = Effect.gen(function* () {
    const [fs, path, { runs }] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Runner]);
    const names = yield* fs.readDirectory(runs).pipe(Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed(Array.empty<string>())));
    return yield* Effect.filterMapEffect(names, (name) => fs.readFileString(path.join(runs, name)).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Held.json)), Effect.result));
});
const stopped = Effect.fnUntraced(function* (agent: string) {
    const [fs, path, { runs }, now] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Runner, DateTime.now]);
    const record = path.join(runs, `${agent}.json`);
    const held = yield* fs.readFileString(record).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Held.json)), Effect.option);
    yield* Effect.ignore(fs.remove(record, { force: true }));
    return Option.map(held, ({ label, pid, start }) => Report.text(new Report({ lines: [new Line({ head: ['run'], value: label, facts: ['stopped', Format.duration(DateTime.distance(start, now))], rows: [new Line({ head: ['pid'], value: `${pid}` })] })] })));
});
const table = Effect.gen(function* () {
    const [fs, path, { runs, programs, claude }, now] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Runner, DateTime.now]);
    const [tree, held, known] = yield* Effect.all(
        [
            ProcessTree.read,
            records,
            fs.readDirectory(programs).pipe(
                Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed(Array.empty<string>())),
                Effect.map((names) => new Set(names.map(decodeURIComponent))),
            ),
        ],
        { concurrency: 'unbounded' },
    );
    const [live, dead] = Array.partition(held, (each) => (Option.isSome(tree.subtree(each.pid)) ? Result.succeed(each) : Result.fail(each)));
    yield* Effect.forEach(dead, ({ agent }) => Effect.ignore(fs.remove(path.join(runs, `${agent}.json`), { force: true })), { discard: true });
    const pids = new Set(live.map(Struct.get('pid')));
    const program = (command: string) => path.basename(command.split(' ', 1).join(''));
    const session = Option.map(
        Option.flatMap(tree.subtree(claude), ({ processes }) => Array.head(processes)),
        ({ command }) => program(command),
    );
    const where = (pid: number) =>
        Option.match(
            tree.ancestor(pid, ({ command }) => Option.contains(session, program(command))),
            { onNone: () => 'outside Claude', onSome: ({ pid: found }) => (found === claude ? 'this session' : `claude ${found}`) },
        );
    const found = tree.processes.flatMap(({ pid, cpu, command }) => {
        const words = command.split(' ');
        const at = words.findIndex((word, index) => !pids.has(pid) && known.has([path.basename(word), ...Array.fromNullishOr(words[index + 1])].join(' ')));
        return at === -1 ? [] : [{ pid, cpu, run: new Command({ program: path.basename(words[at] ?? ''), words: words.slice(at + 1), files: [] }) }];
    });
    const rows = [
        ...Array.sort(
            live,
            Order.mapInput(Order.Boolean, ({ claude: owner }: Held) => owner !== claude),
        ).map(({ claude: owner, label, model, description, start, pid }) => new Line({ head: [owner === claude ? 'this session' : `claude ${owner}`], facts: [label, ...Array.fromNullishOr(model), description, Format.duration(DateTime.distance(start, now)), `pid ${pid}`] })),
        ...found.map(({ pid, cpu, run }) => new Line({ head: [where(pid)], facts: [run.label, ...Option.toArray(run.model), `cpu ${Format.duration(cpu)}`, `pid ${pid}`] })),
    ];
    return Report.text(new Report({ lines: [new Line({ head: ['runs on this Mac'], value: Format.count(rows.length), rows })] }));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const layer = Runner.layer.pipe(Layer.provideMerge(Session.layer));

function register(on: On) {
    host<Layer.Success<typeof layer>, Layer.Error<typeof layer>, Fault>(on, {
        layer,
        hooks: {
            'turn.step': (e, next) =>
                Option.match(Option.fromUndefinedOr(e.agentId), {
                    onNone: () => next(e),
                    onSome: (agent) =>
                        Channel.unwrap(
                            Effect.flatMap(
                                Runner.use(({ runner }) => runner(Option.some(agent))),
                                Option.match({ onNone: () => Effect.succeed(next(e)), onSome: (id) => Effect.map(answer(e, id), (said) => reply(e, said)) }),
                            ),
                        ),
                }),
            'agent.report': (e, next) =>
                Effect.flatMap(Runner, ({ agent: runner }) =>
                    e.subagentType === runner
                        ? Effect.flatMap(stopped(e.agentId), (verdict) =>
                              Option.match(
                                  Option.filter(verdict, () => e.status === 'stopped' && e.text === ''),
                                  { onNone: () => next(e), onSome: (text) => Effect.succeed<AgentReportResult>({ text }) },
                              ),
                          )
                        : next(e),
                ),
            'tool.check': (e, next) => Effect.flatMap(Runner, ({ runner, tool }) => Effect.flatMap(runner(Option.fromUndefinedOr(e.agentId)), (held) => (Option.isSome(held) && e.tool === tool ? Effect.succeed({ decision: 'allow' as const }) : next(e)))),
            'command.run': (e, next) => Effect.flatMap(Plugin, ({ name }) => (e.command === `${name}:runs` ? Effect.map(table, (text) => ({ text, isTextDisplayOnly: true }) as const) : next(e))),
        },
        tools: {
            run: Tool(Run, {
                description: 'Runs one outside agent command (codex exec, agy -p, any one-command CLI agent) as a background agent of the calling loop: answers at once with its agent id, and that agent reports the exit, time, model, files the run wrote with their fields, and the last stderr line of a failed run',
                isDeferred: true,
                run: Effect.fnUntraced(function* (input: typeof Run.Type, e: Frozen<ToolCallInput>) {
                    const held = yield* Runner.use(({ runner }) => runner(Option.fromUndefinedOr(e.agentId)));
                    return yield* Option.match(held, { onNone: () => launch(input, e), onSome: (id) => Effect.map(execute(input, id), (result): ToolCallResult => ({ result })) });
                }),
            }),
        },
        agents: {
            run: {
                description: 'Runs one outside agent command through the delegate run tool and reports its verdict',
                prompt: 'Run the command of your first message through the run tool, then report its verdict.',
                maxTurns: 2,
                background: true,
                omitClaudeMd: true,
                isOffered: false,
            },
        },
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
