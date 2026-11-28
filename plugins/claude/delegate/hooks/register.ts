import { Engine } from '@rasm/claude-code/engine';
import { Plugin, reply, Tool } from '@rasm/claude-code/events';
import { host } from '@rasm/claude-code/host';
import { Session } from '@rasm/claude-code/session';
import { ProcessTree, Writer } from '@rasm/process/tree';
import { Format, Line, Report } from '@rasm/report';
import { Shell } from '@rasm/shell';
import type { AgentReportResult, Frozen, On, SessionMessage, ToolCallInput, ToolCallResult, TurnStepInput } from 'claude-code';
import { Array, ByteSize, Cache, type Cause, Channel, Config, Context, Data, DateTime, Duration, Effect, Equal, Exit, Fiber, FileSystem, Layer, Match, Option, Order, Path, Record, Result, Schedule, Schema, Stream, Struct } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Fault = Effect.Error<ReturnType<typeof execute>> | Effect.Error<ReturnType<typeof launch>> | Effect.Error<typeof table> | Cause.UnknownError;
type Ending = Data.TaggedEnum<{ Exited: { readonly code: number; readonly stderr: Option.Option<string> }; Failed: { readonly reason: string }; Limit: object; Quiet: { readonly sample: Sample } }>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const QUIET = { every: Duration.minutes(5), after: Duration.minutes(20) } as const;
const ANSWER = { bytes: ByteSize.mebibytes(4), token: /^\s*(?<word>\S{1,64})\s*$/u } as const;

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
const FieldsJson = Schema.fromJsonString(Fields);

class Command extends Schema.Class<Command, { readonly brand: unique symbol }>('Command')({ program: Schema.String, words: Schema.Array(Schema.String) }) {
    get label(): string {
        return [this.program, ...Array.take(this.words, 1)].join(' ');
    }

    get model(): Option.Option<string> {
        return Option.flatMap(
            Array.findFirstIndex(this.words, (word) => word === '-m' || word === '--model'),
            (at) => Array.get(this.words, at + 1),
        );
    }
}

class Started extends Schema.Class<Started, { readonly brand: unique symbol }>('Started')({
    agent: Schema.String,
    claude: Schema.Int,
    pid: Schema.Int,
    command: Command,
    description: Schema.String,
    start: Schema.DateTimeUtcFromMillis,
}) {
    static readonly json = Schema.fromJsonString(Started);
}

class Sample extends Data.Class<{ readonly cpu: Duration.Duration; readonly write: Option.Option<Writer> }> {}

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
            const types = yield* Cache.makeWith((id: string) => Effect.map(Effect.orElseSucceed(engine.agents, Record.empty), Record.get(id)), {
                capacity: Number.POSITIVE_INFINITY,
                timeToLive: (exit) => (Exit.isSuccess(exit) && Option.isSome(exit.value) ? Duration.infinity : Duration.zero),
            });
            return {
                runs: path.join(data, 'runs'),
                programs: path.join(data, 'programs'),
                claude,
                tool,
                agent,
                runner: (loop: Option.Option<string>) =>
                    Effect.map(Effect.transposeOption(Option.map(loop, (id) => Cache.get(types, id))), (found) =>
                        found.pipe(
                            Option.flatten,
                            Option.filter(({ type }) => type === agent),
                            Option.map(Struct.get('id')),
                        ),
                    ),
            };
        }),
    );
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const report = (line: Line) => Report.text(new Report({ lines: [line] }));
const refuse = (reason: string, rows: readonly Line[]): ToolCallResult => ({ deny: report(new Line({ head: ['refused'], value: 'run', facts: [reason], rows })) });
const read = Effect.fnUntraced(function* (command: string, agent: Option.Option<string>) {
    const opened = yield* Session.use((session) => session.shell(agent));
    const { dialect, cwd } = yield* opened.environment;
    const script = yield* Shell.use((shell) => shell.read(command, { dialect, cwd }));
    const top = (yield* script.invocations).filter(({ within }) => Array.isReadonlyArrayEmpty(within));
    const refused = [
        { reason: 'no command', holds: Array.isReadonlyArrayEmpty(top) },
        { reason: 'more than one command', holds: top.length > 1 && top.some(({ stage }) => Option.isNone(stage)) },
        { reason: 'a pipeline', holds: top.some(({ stage }) => Option.isSome(stage)) },
        { reason: 'a background command', holds: top.some(({ background }) => background) },
        { reason: 'a leading assignment, which env states', holds: top.some(({ assigns }) => Array.isReadonlyArrayNonEmpty(assigns)) },
        { reason: 'a program named at run time', holds: top.some(({ name }) => Option.isNone(name)) },
    ].flatMap(({ reason, holds }) => (holds ? [reason] : []));
    const files = Array.dedupe(top.flatMap((invocation) => [...invocation.writes, ...invocation.words.map(Struct.get('field'))].flatMap(({ text }) => Option.toArray(Option.map(text, opened.path.resolve)))));
    const invoked = Option.flatMap(Array.head(top), (invocation) => Option.map(invocation.name, (program) => Command.make({ program, words: Array.getSomes(Array.drop(invocation.words, 1).map(Struct.get('text'))) })));
    return { script, exec: opened.exec, cwd, refused, files, command: Option.filter(invoked, () => Array.isReadonlyArrayEmpty(refused)) };
});
const launch = Effect.fnUntraced(function* (run: typeof Run.Type, e: Frozen<ToolCallInput>) {
    const [engine, { agent: runner }] = yield* Effect.all([Engine, Runner]);
    const [{ command, refused }, prompt] = yield* Effect.all([read(run.command, Option.fromUndefinedOr(e.agentId)), Schema.encodeEffect(Prompt)(run)]);
    const started = Effect.all([engine.callTool({ tool: 'Agent', input: { subagent_type: runner, prompt, description: run.description }, ...Struct.pick(e, ['agentId']) }), engine.limits, engine.agents]);
    return yield* Option.match(command, {
        onNone: () => Effect.succeed(refuse(refused.join(', '), [])),
        onSome: ({ label }) =>
            Effect.map(started, ([answer, limits, agents]) => {
                const count = new Line({ head: ['agents'], value: `${Record.size(Record.filter(agents, ({ status }) => status === 'running'))} running`, facts: [`limit ${limits.concurrent}`] });
                return Option.match(Schema.decodeUnknownOption(Launched)(answer), {
                    onNone: () => refuse(answer.deny ?? answer.text ?? 'the Agent tool started no agent', [count]),
                    onSome: ({ result }) => ({ result: report(new Line({ head: ['started'], value: label, facts: [`agent ${result.agentId}`], rows: [count] })) }),
                });
            }),
    });
});
const sample = Effect.fnUntraced(function* (pid: number) {
    const tree = yield* ProcessTree.read;
    const own = yield* Effect.fromOption(Option.flatMap(tree.subtree(pid), ({ processes }) => Array.head(processes)));
    const written = yield* Writer.of(new ProcessTree({ processes: [own] }));
    return new Sample({ cpu: own.cpu, write: Array.head(written) });
});
const watchQuiet = (pid: number) =>
    Stream.fromEffectSchedule(sample(pid), Schedule.spaced(QUIET.every)).pipe(
        Stream.sliding(Math.round(Duration.toMillis(QUIET.after) / Duration.toMillis(QUIET.every)) + 1),
        Stream.filter((window) => window.every((each) => Equal.equals(each, Array.headNonEmpty(window)))),
        Stream.runHead,
        Effect.flatMap(Effect.fromOption),
        Effect.map((window) => Ending.Quiet({ sample: Array.lastNonEmpty(window) })),
    );
const describe = (held: Readonly<Record<string, Schema.Json>>, outer: boolean): readonly string[] =>
    Record.collect(held, (key, value) =>
        Match.value(value).pipe(
            Match.when(Match.string, (text) => [`${key} ${ANSWER.token.exec(text)?.groups?.['word'] ?? Format.count(text.length, { one: 'char', other: 'chars' })}`]),
            Match.when(globalThis.Array.isArray, (items) => [`${key} ${Format.count(items.length, { one: 'item', other: 'items' })}`]),
            Match.when(Schema.is(Fields), (inner) => (outer ? describe(inner, false).map((field) => `${key}.${field}`) : [`${key} ${Format.count(Record.size(inner), { one: 'key', other: 'keys' })}`])),
            Match.orElse((scalar) => [`${key} ${JSON.stringify(scalar)}`]),
        ),
    ).flat();
const summarize = (text: string): readonly Line[] =>
    Match.value({ held: Option.getOrUndefined(Schema.decodeOption(FieldsJson)(text)), word: ANSWER.token.exec(text)?.groups?.['word'] }).pipe(
        Match.when({ held: Match.defined }, ({ held }) => Array.match(describe(held, true), { onEmpty: Array.empty, onNonEmpty: (all) => [new Line({ head: ['fields'], value: Array.headNonEmpty(all), facts: Array.tailNonEmpty(all) })] })),
        Match.when({ word: Match.string }, ({ word }) => [new Line({ head: ['text'], value: word })]),
        Match.orElse(() =>
            Array.match(
                text.split('\n').filter((line) => line.trim() !== ''),
                { onEmpty: Array.empty, onNonEmpty: (lines) => [new Line({ head: ['lines'], value: Format.count(lines.length) })] },
            ),
        ),
    );
const readAnswers = Effect.fnUntraced(function* (files: readonly string[], start: DateTime.Utc, cwd: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const since = DateTime.toDate(DateTime.startOf(start, 'second'));
    const fresh = yield* Effect.filterMapEffect(files, (file) =>
        Effect.match(fs.stat(file), {
            onFailure: () => Result.failVoid,
            onSuccess: ({ type, mtime, size }) => (type === 'File' && Option.exists(mtime, (date) => date >= since) ? Result.succeed({ file, size }) : Result.failVoid),
        }),
    );
    return yield* Effect.forEach(fresh, ({ file, size }) => {
        const relative = path.relative(cwd, file);
        const shown = Option.getOrElse(Option.as(path.resolveWithin(cwd, relative), relative), () => file);
        return Effect.map(ByteSize.isLessThanOrEqualTo(size, ANSWER.bytes) ? Effect.asSome(fs.readFileString(file)) : Effect.succeedNone, (text) => new Line({ head: ['answer'], value: shown, facts: [Format.bytes(size)], rows: Option.match(text, { onNone: Array.empty, onSome: summarize }) }));
    });
});
const execute = Effect.fnUntraced(function* (run: typeof Run.Type, agent: string) {
    const [fs, path, runner] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Runner]);
    const parsed = yield* read(run.command, Option.some(agent));
    const command = yield* Effect.fromOption(parsed.command);
    const start = yield* DateTime.now;
    const child = yield* parsed.exec(parsed.script);
    yield* Effect.all(
        [
            Effect.flatMap(Schema.encodeEffect(Started.json)(Started.make({ agent, claude: runner.claude, pid: child.pid, command, description: run.description, start })), (text) => fs.writeFileString(path.join(runner.runs, `${agent}.json`), text)),
            fs.writeFileString(path.join(runner.programs, encodeURIComponent(command.label)), ''),
        ],
        { discard: true },
    );
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
    const exited = child.exitCode.pipe(
        Effect.flatMap((code) => Effect.map(code === 0 ? Effect.succeedNone : Effect.map(Fiber.join(output), ([, last]) => last), (stderr) => Ending.Exited({ code, stderr }))),
        Effect.catch(({ message }) => Effect.succeed(Ending.Failed({ reason: message }))),
    );
    const ending = yield* Effect.raceAll([exited, Effect.as(Effect.sleep(run.limit), Ending.Limit()), watchQuiet(child.pid)]);
    const end = yield* DateTime.now;
    const { state, rows } = yield* Ending.$match(ending, {
        Exited: ({ code, stderr }) => Effect.map(readAnswers(parsed.files, start, parsed.cwd), (written) => ({ state: `exit ${code}`, rows: [...written, ...Option.toArray(Option.map(stderr, (line) => new Line({ head: ['stderr'], value: line })))] })),
        Failed: ({ reason }) => Effect.succeed({ state: 'failed', rows: [new Line({ head: ['reason'], value: reason })] }),
        Limit: () => Effect.succeed({ state: 'ended at limit', rows: [] }),
        Quiet: ({ sample: { cpu, write } }) =>
            Effect.succeed({
                state: 'ended quiet',
                rows: [new Line({ head: ['cpu'], value: Format.duration(cpu), facts: [`unchanged over ${Format.duration(QUIET.after)}`] }), ...Option.toArray(Option.map(write, ({ path: file, modified }) => new Line({ head: ['last write'], value: Format.instant(modified), facts: [file] })))],
            }),
    });
    return report(new Line({ head: ['run'], value: command.label, facts: [state, Format.duration(DateTime.distance(start, end))], rows: [...Option.toArray(Option.map(command.model, (model) => new Line({ head: ['model'], value: model }))), ...rows] }));
}, Effect.scoped);
const answer = Effect.fnUntraced(function* (e: Frozen<TurnStepInput>, agent: string) {
    const [engine, { tool }] = yield* Effect.all([Engine, Runner]);
    const messages: readonly SessionMessage[] = yield* engine.messages(Option.some(agent));
    return yield* e.index === 0
        ? Effect.fromOption(Array.findFirst(messages, ({ role }) => role === 'user')).pipe(
              Effect.flatMap(({ text }) => Schema.decodeEffect(FieldsJson)(text)),
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
                  () => report(new Line({ head: ['run'], value: 'no verdict' })),
              ),
              toolUses: [],
          });
});
const closeRecord = Effect.fnUntraced(function* (agent: string) {
    const [fs, path, { runs }, now] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Runner, DateTime.now]);
    const record = path.join(runs, `${agent}.json`);
    const held = yield* fs.readFileString(record).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Started.json)), Effect.option);
    yield* fs.remove(record, { force: true });
    return Option.map(held, ({ command, pid, start }) => report(new Line({ head: ['run'], value: command.label, facts: ['stopped', Format.duration(DateTime.distance(start, now))], rows: [new Line({ head: ['pid'], value: `${pid}` })] })));
});
const table = Effect.gen(function* () {
    const [fs, path, { runs, programs, claude }, now] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Runner, DateTime.now]);
    const list = (folder: string) => fs.readDirectory(folder).pipe(Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed(Array.empty<string>())));
    const [tree, names, known] = yield* Effect.all([ProcessTree.read, list(runs), Effect.map(list(programs), (labels) => new Set(labels.map(decodeURIComponent)))], { concurrency: 'unbounded' });
    const held = yield* Effect.filterMapEffect(names, (name) => fs.readFileString(path.join(runs, name)).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Started.json)), Effect.result));
    const [live, dead] = Array.partition(held, (each) => (Option.isSome(tree.subtree(each.pid)) ? Result.succeed(each) : Result.fail(each)));
    yield* Effect.forEach(dead, ({ agent }) => fs.remove(path.join(runs, `${agent}.json`), { force: true }), { discard: true });
    const pids = new Set(live.map(Struct.get('pid')));
    const program = (command: string) => path.basename(command.split(' ', 1).join(''));
    const session = Option.map(
        Option.flatMap(tree.subtree(claude), ({ processes }) => Array.head(processes)),
        ({ command }) => program(command),
    );
    const owner = (pid: number) => (pid === claude ? 'this session' : `claude ${pid}`);
    const found = tree.processes
        .filter(({ pid }) => !pids.has(pid))
        .flatMap(({ pid, cpu, command }) => {
            const words = command.split(' ');
            const at = words.findIndex((word, index) => known.has([path.basename(word), ...Array.fromNullishOr(words[index + 1])].join(' ')));
            return at === -1 ? [] : [{ pid, cpu, run: Command.make({ program: path.basename(words[at] ?? ''), words: words.slice(at + 1) }) }];
        });
    const rows = [
        ...Array.sort(
            live,
            Order.mapInput(Order.Boolean, ({ claude: pid }: Started) => pid !== claude),
        ).map(({ claude: pid, command, description, start, pid: run }) => new Line({ head: [owner(pid)], facts: [command.label, ...Option.toArray(command.model), description, Format.duration(DateTime.distance(start, now)), `pid ${run}`] })),
        ...found.map(
            ({ pid, cpu, run }) =>
                new Line({
                    head: [
                        Option.match(
                            tree.ancestor(pid, ({ command }) => Option.contains(session, program(command))),
                            { onNone: () => 'outside Claude', onSome: ({ pid: at }) => owner(at) },
                        ),
                    ],
                    facts: [run.label, ...Option.toArray(run.model), `cpu ${Format.duration(cpu)}`, `pid ${pid}`],
                }),
        ),
    ];
    return report(new Line({ head: ['runs on this Mac'], value: Format.count(rows.length), rows }));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const layer = Runner.layer.pipe(Layer.provideMerge(Session.layer));

function register(on: On) {
    host<Layer.Success<typeof layer>, Layer.Error<typeof layer>, Fault>(on, {
        layer,
        hooks: {
            'turn.step': (e, next) =>
                Channel.unwrap(
                    Effect.flatMap(
                        Runner.use(({ runner }) => runner(Option.fromUndefinedOr(e.agentId))),
                        Option.match({ onNone: () => Effect.succeed(next(e)), onSome: (agent) => Effect.map(answer(e, agent), (said) => reply(e, said)) }),
                    ),
                ),
            'agent.report': (e, next) =>
                Effect.flatMap(Runner, ({ agent: runner }) =>
                    e.subagentType === runner
                        ? Effect.flatMap(closeRecord(e.agentId), (verdict) =>
                              Option.match(
                                  Option.filter(verdict, () => e.status === 'stopped' && e.text === ''),
                                  { onNone: () => next(e), onSome: (text) => Effect.succeed<AgentReportResult>({ text }) },
                              ),
                          )
                        : next(e),
                ),
            'tool.check': (e, next) => Effect.flatMap(Runner, ({ runner, tool }) => (e.tool === tool ? Effect.flatMap(runner(Option.fromUndefinedOr(e.agentId)), (held) => (Option.isSome(held) ? Effect.succeed({ decision: 'allow' as const }) : next(e))) : next(e))),
            'command.run': (e, next) => Effect.flatMap(Plugin, ({ name }) => (e.command === `${name}:runs` ? Effect.map(table, (text) => ({ text, isTextDisplayOnly: true }) as const) : next(e))),
        },
        tools: {
            run: Tool(Run, {
                description: `Runs one outside agent command (codex exec, agy -p, any one-command CLI agent) as a background agent of the calling loop and answers at once with its agent id. The agent reports the exit, time, model, files the run wrote with their fields, and the last stderr line of a failed run. A run ends at its limit, or once its program uses no CPU and writes none of its files for ${Format.duration(QUIET.after)}, and TaskStop with the agent id ends it and its program`,
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
