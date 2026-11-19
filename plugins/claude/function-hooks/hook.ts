import { userInfo } from 'node:os';
import { basename } from 'node:path';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { layerNodeFromConfig, teardown } from '@rasm/telemetry/node';
import { Array, Cause, Console, Effect, Function, Layer, Match, Option, Record, Schema, Stdio, Stream, Struct } from 'effect';
import { Host } from '#/host';
import manifest from '#/manifest' with { type: 'json' };
import { decide, due, Engine } from '#/rules/decide';
import { Decision, type Request } from '#/rules/decision';
import { quote } from '#/rules/rule';
import { PROGRAMS } from '#/shell/programs';

// --- [CONSTANTS] -----------------------------------------------------------------------

const EVENT = 'PreToolUse';
const SHELL = Option.exists(Option.fromNullOr(userInfo().shell), (path) => basename(path) === 'zsh') ? 'zsh' : 'bash';
const ADDED = /^\*\*\* Add File: (?<file>.+)\n(?<body>(?:\+.*\n?)*)/gmu;
const ADDED_LINE = /^\+/gmu;
const SPAWNS = /\bchild_process\b/u;
const INLINE: ReadonlyMap<string, string> = new Map(Record.toEntries(PROGRAMS).flatMap(([name, { extensions = [], code = [], shell = [] }]) => Option.match(Array.head([...code, ...shell]), { onNone: () => [], onSome: (flag) => extensions.map((extension) => [extension, `${name} ${flag}`] as const) })));

// --- [MODELS] --------------------------------------------------------------------------

const Envelope = { session_id: Schema.String, cwd: Schema.String, tool_use_id: Schema.optionalKey(Schema.String) };
const Command = Schema.Struct({ command: Schema.String });
const Call = Schema.Union([
    Schema.Struct({ ...Envelope, tool_name: Schema.Literal('Bash'), tool_input: Command }),
    Schema.Struct({ ...Envelope, tool_name: Schema.Literal('apply_patch'), tool_input: Command }),
    Schema.Struct({ ...Envelope, tool_name: Schema.Literal('clocksleep') }),
    Schema.Struct({ ...Envelope, tool_name: Schema.Literal('mcp__node_repl__js'), tool_input: Schema.Struct({ code: Schema.String }) }),
]).pipe(Schema.toTaggedUnion('tool_name'));
const Antigravity = Schema.fromJsonString(Schema.Struct({ conversationId: Schema.String, cwd: Schema.String, toolCall: Schema.Struct({ args: Schema.Struct({ CommandLine: Schema.String }) }) }));
const Output = Schema.fromJsonString(Schema.Unknown);

// --- [OPERATIONS] ----------------------------------------------------------------------

const requested = ({ session_id, cwd, tool_use_id }: typeof Call.Type, command: string): typeof Request.Type => ({ command, cwd, kind: 'plain', shell: SHELL, session: session_id, ...(tool_use_id === undefined ? {} : { call: tool_use_id }) });

const judged = Call.match({
    Bash: (call) => decide(requested(call, call.tool_input.command)),
    apply_patch: Effect.fnUntraced(function* (call) {
        const { path, inside, root } = yield* Host.make(call.cwd);
        const scripts = Array.fromIterable(call.tool_input.command.matchAll(ADDED)).flatMap(({ groups: { file = '', body = '' } = {} }) =>
            Option.toArray(
                Option.map(
                    Option.filter(Option.fromUndefinedOr(INLINE.get(path.extname(file))), () => !inside(path.resolve(call.cwd, file), root)),
                    (runner) => ({ file, command: `sh -c ${quote(`${runner} ${quote(body.replaceAll(ADDED_LINE, ''))}`)}` }),
                ),
            ),
        );
        const decisions = yield* Effect.forEach(scripts, ({ command }) => decide(requested(call, command)), { concurrency: 'unbounded' });
        const refused = Array.findFirst(Array.zip(scripts, decisions), ([{ file }, decision]) =>
            Match.value(decision).pipe(
                Match.tag('Allow', () => Option.none()),
                Match.tag('Deny', ({ reason }) => Option.some(Decision.cases.Deny.make({ reason: `${file}: ${reason}` }))),
                Match.orElse(({ note }) => Option.some(Decision.cases.Deny.make({ reason: `${file}: ${note ?? 'its commands take another form, run them as separate calls'}` }))),
            ),
        );
        return Option.getOrElse(refused, () => Decision.cases.Allow.make({}));
    }),
    clocksleep: () => Effect.succeed(Decision.cases.Deny.make({ reason: "clocksleep waits on nothing and wakes you with nothing new: wait_agent returns with a spawned agent's result, and a turn with nothing to do ends until a notice arrives" })),
    mcp__node_repl__js: ({ tool_input }) => Effect.succeed(SPAWNS.test(tool_input.code) ? Decision.cases.Deny.make({ reason: 'child_process runs a command the shell rules never judge, run it through the shell tool' }) : Decision.cases.Allow.make({})),
});

// --- [TABLES] --------------------------------------------------------------------------

const HOSTS = {
    codex: {
        call: Function.flow(
            Schema.decodeEffect(Schema.fromJsonString(Call)),
            Effect.map((call) => ({ session: call.session_id, decision: judged(call) })),
        ),
        rendered: Match.type<typeof Decision.Type>().pipe(
            Match.tag('Allow', ({ note }) => Option.map(Option.fromNullishOr(note), (additionalContext): object => ({ hookSpecificOutput: { hookEventName: EVENT, additionalContext } }))),
            Match.tag('Deny', ({ reason }) => Option.some<object>({ hookSpecificOutput: { hookEventName: EVENT, permissionDecision: 'deny', permissionDecisionReason: reason } })),
            Match.tag('Rewrite', 'Monitor', ({ command, note }) => Option.some<object>({ systemMessage: note, hookSpecificOutput: { hookEventName: EVENT, permissionDecision: 'allow', updatedInput: { command }, additionalContext: note } })),
            Match.exhaustive,
        ),
        unjudged: (line: string): Option.Option<object> => Option.some({ hookSpecificOutput: { hookEventName: EVENT, additionalContext: line } }),
    },
    agy: {
        call: Function.flow(
            Schema.decodeEffect(Antigravity),
            Effect.map(({ conversationId, cwd, toolCall }) => ({ session: conversationId, decision: decide({ command: toolCall.args.CommandLine, cwd, kind: 'plain', shell: SHELL, session: conversationId }) })),
        ),
        rendered: Match.type<typeof Decision.Type>().pipe(
            Match.tag('Allow', () => Option.none<object>()),
            Match.tag('Deny', ({ reason }) => Option.some<object>({ decision: 'deny', reason })),
            Match.tag('Rewrite', 'Monitor', ({ command }) => Option.some<object>({ decision: 'allow', overwrite: { CommandLine: command } })),
            Match.exhaustive,
        ),
        unjudged: (_line: string): Option.Option<object> => Option.none(),
    },
} as const;
const Harness = Schema.Literals(Struct.keys(HOSTS));

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const { args, stdin } = yield* Stdio.Stdio;
    const named = HOSTS[yield* Effect.flatMap(args, Function.flow(Array.head, Effect.fromOption, Effect.flatMap(Schema.decodeUnknownEffect(Harness))))];
    const { session, decision } = yield* named.call(yield* Stream.mkString(Stream.decodeText(stdin)));
    const answer = yield* decision.pipe(
        Effect.timeout('25 seconds'),
        Effect.map(named.rendered),
        Effect.catchCause((cause) => {
            const line = `${manifest.name} did not judge this command: ${Cause.pretty(cause).split('\n', 1).join('')}`;
            return Effect.logError(cause).pipe(Effect.andThen(due({ session }, new Map([[line, line]]))), Effect.map(Function.flow(Array.head, Option.flatMap(named.unjudged))));
        }),
    );
    yield* Option.match(answer, { onNone: () => Effect.void, onSome: (output) => Effect.flatMap(Schema.encodeEffect(Output)(output), Console.log) });
}).pipe(Effect.provide(Layer.provideMerge(Layer.provideMerge(Engine, layer), layerNodeFromConfig({ resource: { serviceName: manifest.name, serviceVersion: manifest.version }, target: 'file' }))), runMain({ teardown }));
