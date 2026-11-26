import { hash } from 'node:crypto';
import { homedir, tmpdir } from 'node:os';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Handles } from '@rasm/mcp/handles';
import { Array, Console, Effect, FiberMap, FileSystem, Filter, Layer, Option, Path, Record, Schema, Stdio, Stream } from 'effect';
import { FetchHttpClient } from 'effect/http';
import { HttpApiClient } from 'effect/http-api';
import { Host } from 'effect/net';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';

// --- [MODELS] --------------------------------------------------------------------------

const Arguments = Schema.Record(Schema.String, Schema.Json);
const Servers = Schema.fromJsonString(Schema.Record(Schema.String, Schema.String));
const CodexEvent = Schema.Struct({ session_id: Schema.String, transcript_path: Schema.NullOr(Schema.String) });
const CodexTurn = Schema.fromJsonString(CodexEvent);
const CodexTool = Schema.fromJsonString(Schema.Struct({ ...CodexEvent.fields, tool_name: Schema.String, tool_input: Arguments }));
const CodexStop = Schema.fromJsonString(Schema.Struct({ ...CodexEvent.fields, stop_hook_active: Schema.Boolean, agent_transcript_path: Schema.optionalKey(Schema.NullOr(Schema.String)) }));
const CodexServers = Schema.fromJsonString(Schema.Array(Schema.Struct({ name: Schema.String, enabled: Schema.Boolean, transport: Schema.Struct({ url: Schema.optionalKey(Schema.String) }) })));
const AntigravityEvent = Schema.Struct({ conversationId: Schema.String, workspacePaths: Schema.Array(Schema.String) });
const AntigravityTurn = Schema.fromJsonString(AntigravityEvent);
const AntigravityTool = Schema.fromJsonString(Schema.Struct({ ...AntigravityEvent.fields, toolCall: Schema.Struct({ name: Schema.String, args: Arguments }) }));
const AntigravityPlugins = Schema.fromJsonString(Schema.Array(Schema.Struct({ name: Schema.String, enabled: Schema.Boolean, path: Schema.String, mcpServers: Schema.optionalKey(Schema.Array(Schema.String)) })));
const AntigravityConfig = Schema.fromJsonString(Schema.Struct({ mcpServers: Schema.Record(Schema.String, Schema.Struct({ serverUrl: Schema.optionalKey(Schema.String), url: Schema.optionalKey(Schema.String) })) }));
const Invocation = Schema.Union([
    Schema.Tuple([Schema.Literal('codex'), Schema.Literals(['SessionStart', 'SessionEnd', 'PreToolUse', 'PostToolUse', 'UserPromptSubmit', 'Stop', 'SubagentStop'])]),
    Schema.Tuple([Schema.Literal('antigravity'), Schema.Literals(['PreToolUse', 'PreInvocation', 'Stop'])]),
    Schema.Tuple([Schema.Literal('antigravity'), Schema.Literal('follow')]),
]);

// --- [OPERATIONS] ----------------------------------------------------------------------

const folders = (harness: (typeof Invocation.Type)[0]) => Effect.map(Path.Path, (path) => ({ bound: path.join(tmpdir(), 'risman', harness, 'bound'), idle: path.join(tmpdir(), 'risman', harness, 'idle'), servers: path.join(tmpdir(), 'risman', harness, 'servers') }));
const payload = Effect.flatMap(Stdio.Stdio, ({ stdin }) => Stream.mkString(Stream.decodeText(stdin)));
const local = Effect.flatMap(FileSystem.FileSystem, (fs) =>
    Effect.map(
        Effect.orElseSucceed(fs.readFileString('/etc/hosts'), () => ''),
        (text) => Handles.local(Host.parseHostsFile(text)),
    ),
);
const resident = (rows: readonly (readonly [string, string | undefined])[]) => Effect.map(local, (isLocal) => Record.fromEntries(rows.flatMap(([name, url]) => (url !== undefined && isLocal(url) ? [[name, url] as const] : []))));
const client = (origin: string) => HttpApiClient.make(Handles, { baseUrl: origin });
const bind = Effect.fnUntraced(function* (bound: string, agent: string, [server, origin]: readonly [string, string]) {
    const [fs, path, handles] = yield* Effect.all([FileSystem.FileSystem, Path.Path, client(origin)]);
    const { handle } = yield* handles.bind({ query: { agent } });
    yield* fs.makeDirectory(path.join(bound, agent), { recursive: true });
    yield* fs.writeFileString(path.join(bound, agent, server), origin);
    return handle;
});
const report = Effect.fnUntraced(function* (bound: string, agent: string, server: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const handles = yield* Effect.flatMap(fs.readFileString(path.join(bound, agent, server)), client);
    const sent = yield* handles.events({ params: { handle: agent }, query: { follow: false } });
    const events = yield* Effect.fromOption(Option.liftPredicate(Stream.isStream(sent) ? [] : sent, Array.isReadonlyArrayNonEmpty));
    return { text: Handles.report(server, events), ack: Effect.asVoid(handles.ack({ params: { handle: agent }, query: { through: Array.lastNonEmpty(events).sequence } })) };
});
const pending = Effect.fnUntraced(function* (bound: string, agent: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const servers = yield* Effect.orElseSucceed(fs.readDirectory(path.join(bound, agent)), () => []);
    const reports = yield* Effect.filterMapEffect(servers, (server) => Effect.result(report(bound, agent, server)), { concurrency: 'unbounded' });
    return Option.map(Option.liftPredicate(reports, Array.isReadonlyArrayNonEmpty), (each) => ({ text: Array.map(each, ({ text }) => text).join('\n\n'), ack: Effect.forEach(each, ({ ack }) => Effect.ignore(ack), { discard: true }) }));
});
const delivered = (bound: string, agent: string) => Effect.tap(pending(bound, agent), (found) => Effect.transposeOption(Option.map(found, ({ ack }) => ack)));
const codexAgent = ({ session_id, transcript_path }: typeof CodexEvent.Type) => hash('sha256', transcript_path ?? session_id);
const codexServers = Effect.fnUntraced(function* (session: string) {
    const [fs, path, { servers }, spawner] = yield* Effect.all([FileSystem.FileSystem, Path.Path, folders('codex'), ChildProcessSpawner.ChildProcessSpawner]);
    const file = path.join(servers, `${session}.json`);
    const listed = spawner.string(ChildProcess.make`codex mcp list --json`).pipe(
        Effect.flatMap(Schema.decodeEffect(CodexServers)),
        Effect.flatMap((rows) => resident(rows.map(({ name, enabled, transport }) => [name, enabled ? transport.url : undefined] as const))),
        Effect.tap((found) =>
            Effect.andThen(
                fs.makeDirectory(servers, { recursive: true }),
                Effect.flatMap(Schema.encodeEffect(Servers)(found), (text) => fs.writeFileString(file, text)),
            ),
        ),
    );
    return yield* Effect.catch(Effect.flatMap(fs.readFileString(file), Schema.decodeEffect(Servers)), () => listed);
});
const antigravityServers = Effect.fnUntraced(function* (workspaces: readonly string[]) {
    const [fs, path, spawner] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner]);
    const config = (file: string, prefix: string) =>
        fs.readFileString(file).pipe(
            Effect.flatMap(Schema.decodeEffect(AntigravityConfig)),
            Effect.map(({ mcpServers }) => Record.collect(mcpServers, (name, { serverUrl, url }) => [`${prefix}${name}`, serverUrl ?? url] as const)),
            Effect.orElseSucceed(() => []),
        );
    const plugins = yield* spawner.string(ChildProcess.make`agy plugin list --json`).pipe(
        Effect.flatMap(Schema.decodeEffect(AntigravityPlugins)),
        Effect.orElseSucceed(() => []),
    );
    const rows = yield* Effect.all(
        [
            config(path.join(homedir(), '.gemini', 'config', 'mcp_config.json'), ''),
            ...workspaces.map((workspace) => config(path.join(workspace, '.agents', 'mcp_config.json'), '')),
            ...plugins.filter(({ enabled, mcpServers = [] }) => enabled && mcpServers.length > 0).map(({ name, path: root }) => config(path.join(root, 'mcp_config.json'), `${name}_`)),
        ],
        { concurrency: 'unbounded' },
    );
    return yield* resident(rows.flat());
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const codex = {
    SessionStart: Effect.fnUntraced(function* (text: string) {
        const { session_id } = yield* Schema.decodeEffect(CodexTurn)(text);
        yield* codexServers(session_id);
        return Option.none();
    }),
    SessionEnd: Effect.fnUntraced(function* (text: string) {
        const [fs, path, { servers }, { session_id }] = yield* Effect.all([FileSystem.FileSystem, Path.Path, folders('codex'), Schema.decodeEffect(CodexTurn)(text)]);
        yield* fs.remove(path.join(servers, `${session_id}.json`), { force: true });
        return Option.none();
    }),
    PreToolUse: Effect.fnUntraced(function* (text: string) {
        const [event, { bound }] = yield* Effect.all([Schema.decodeEffect(CodexTool)(text), folders('codex')]);
        const servers = yield* codexServers(event.session_id);
        const named = Option.filter(
            Array.findFirst(Record.toEntries(servers), ([name]) => event.tool_name.startsWith(`mcp__${name}__`)),
            () => !Record.has(event.tool_input, 'handle'),
        );
        const handle = yield* Effect.orElseSucceed(Effect.transposeOption(Option.map(named, (server) => bind(bound, codexAgent(event), server))), () => Option.none());
        return Option.map(handle, (given) => ({ hookSpecificOutput: { hookEventName: 'PreToolUse', permissionDecision: 'allow', updatedInput: { ...event.tool_input, handle: given } } }));
    }),
    PostToolUse: Effect.fnUntraced(function* (text: string) {
        const [event, { bound }] = yield* Effect.all([Schema.decodeEffect(CodexTurn)(text), folders('codex')]);
        return Option.map(yield* delivered(bound, codexAgent(event)), ({ text: additionalContext }) => ({ hookSpecificOutput: { hookEventName: 'PostToolUse', additionalContext } }));
    }),
    UserPromptSubmit: Effect.fnUntraced(function* (text: string) {
        const [event, { bound }] = yield* Effect.all([Schema.decodeEffect(CodexTurn)(text), folders('codex')]);
        return Option.map(yield* delivered(bound, codexAgent(event)), ({ text: additionalContext }) => ({ hookSpecificOutput: { hookEventName: 'UserPromptSubmit', additionalContext } }));
    }),
    Stop: Effect.fnUntraced(function* (text: string) {
        const [event, { bound }] = yield* Effect.all([Schema.decodeEffect(CodexStop)(text), folders('codex')]);
        const found = event.stop_hook_active ? Option.none() : yield* delivered(bound, codexAgent(event));
        return Option.map(found, ({ text: reason }) => ({ decision: 'block', reason }));
    }),
    SubagentStop: Effect.fnUntraced(function* (text: string) {
        const [event, { bound }] = yield* Effect.all([Schema.decodeEffect(CodexStop)(text), folders('codex')]);
        const agent = Option.filter(Option.fromNullishOr(event.agent_transcript_path), () => !event.stop_hook_active);
        const found = yield* Effect.map(Effect.transposeOption(Option.map(agent, (path) => delivered(bound, hash('sha256', path)))), Option.flatten);
        return Option.map(found, ({ text: reason }) => ({ decision: 'block', reason }));
    }),
} as const;

const antigravity = {
    PreToolUse: Effect.fnUntraced(function* (text: string) {
        const [fs, path, { conversationId, workspacePaths, toolCall }, { bound }] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Schema.decodeEffect(AntigravityTool)(text), folders('antigravity')]);
        const nested = toolCall.name === 'call_mcp_tool';
        const given: typeof Arguments.Type = nested ? Schema.decodeUnknownOption(Arguments)(toolCall.args['Arguments']).pipe(Option.getOrElse(() => ({}))) : toolCall.args;
        const wanted = (name: string) => (nested ? toolCall.args['ServerName'] === name : toolCall.name.startsWith(`mcp_${name}_`));
        const names = yield* Effect.orElseSucceed(fs.readDirectory(path.join(bound, conversationId)), () => []);
        const held = yield* Effect.forEach(names, (name) => Effect.map(fs.readFileString(path.join(bound, conversationId, name)), (origin) => [name, origin] as const));
        const resolved = Effect.map(antigravityServers(workspacePaths), (servers) => Array.findFirst(Record.toEntries(servers), ([name]) => wanted(name)));
        const named = Record.has(given, 'handle')
            ? Option.none()
            : yield* Option.match(
                  Array.findFirst(held, ([name]) => wanted(name)),
                  { onNone: () => resolved, onSome: Effect.succeedSome },
              );
        const handle = yield* Effect.orElseSucceed(Effect.transposeOption(Option.map(named, (server) => bind(bound, conversationId, server))), () => Option.none());
        return Option.some({ decision: 'allow', ...Option.match(handle, { onNone: () => ({}), onSome: (key) => ({ overwrite: nested ? { Arguments: { ...given, handle: key } } : { handle: key } }) }) });
    }),
    PreInvocation: Effect.fnUntraced(function* (text: string) {
        const [fs, path, { conversationId }, { bound, idle }] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Schema.decodeEffect(AntigravityTurn)(text), folders('antigravity')]);
        yield* fs.remove(path.join(idle, conversationId), { force: true });
        return Option.some(Option.match(yield* delivered(bound, conversationId), { onNone: () => ({}), onSome: ({ text: systemMessage }) => ({ injectSteps: [{ systemMessage: { systemMessage } }] }) }));
    }),
    Stop: Effect.fnUntraced(function* (text: string) {
        const [fs, path, { conversationId }, { idle }] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Schema.decodeEffect(AntigravityTurn)(text), folders('antigravity')]);
        yield* Effect.andThen(fs.makeDirectory(idle, { recursive: true }), fs.writeFileString(path.join(idle, conversationId), ''));
        return Option.some({});
    }),
} as const;

const follow = Effect.gen(function* () {
    const [fs, path, spawner, { bound, idle }, followers] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner, folders('antigravity'), FiberMap.make<string>()]);
    const wake = Effect.fnUntraced(
        function* (conversation: string) {
            const found = (yield* fs.exists(path.join(idle, conversation))) ? yield* pending(bound, conversation) : Option.none();
            yield* Effect.transposeOption(Option.map(found, ({ text, ack }) => Effect.andThen(spawner.exitCode(ChildProcess.make('agentapi', ['send-message', conversation, text])), ack)));
        },
        (effect) => Effect.ignore(effect),
    );
    const track = (file: string) =>
        Option.match(
            Option.liftPredicate(file.split(path.sep), (parts): parts is [string, string] => parts.length === 2),
            {
                onNone: () => Effect.void,
                onSome: ([conversation]) =>
                    FiberMap.run(
                        followers,
                        file,
                        Effect.flatMap(Effect.flatMap(fs.readFileString(path.join(bound, file)), client), (handles) => handles.events({ params: { handle: conversation }, query: { follow: true } })).pipe(
                            Effect.flatMap((sent) => (Stream.isStream(sent) ? Stream.runForEach(sent, () => wake(conversation)) : Effect.void)),
                            Effect.ignore,
                        ),
                        { onlyIfMissing: true },
                    ).pipe(Effect.asVoid),
            },
        );
    yield* Effect.all([fs.makeDirectory(bound, { recursive: true }), fs.makeDirectory(idle, { recursive: true })]);
    yield* Effect.forEach(yield* fs.readDirectory(bound, { recursive: true }), track, { discard: true });
    yield* Stream.merge(
        Stream.mapEffect(fs.watch(bound, { recursive: true }), ({ path: file }) => track(file)),
        Stream.mapEffect(Stream.filterMap(fs.watch(idle), Filter.tagged('Create')), ({ path: conversation }) => wake(conversation)),
    ).pipe(Stream.runDrain);
    return Option.none();
}).pipe(Effect.scoped);

runMain(
    Effect.gen(function* () {
        const invocation = yield* Effect.flatMap(
            Effect.flatMap(Stdio.Stdio, ({ args }) => args),
            Schema.decodeUnknownEffect(Invocation),
        );
        if (invocation[1] === 'follow') {
            return yield* follow;
        }
        const text = yield* payload;
        const output = invocation[0] === 'codex' ? yield* codex[invocation[1]](text) : yield* antigravity[invocation[1]](text);
        return yield* Effect.transposeOption(Option.map(output, (value) => Console.log(JSON.stringify(value))));
    }).pipe(Effect.provide(Layer.merge(layer, FetchHttpClient.layer))),
);
