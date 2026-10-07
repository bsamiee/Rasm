import type { ClassicHookInputs, EngineInterface, Frozen, On, PluginOptions, ProcessRunInit, Register, ToolCallInput, ToolCallResult, TurnCompleteInput } from 'claude-code';
import { atom, read, update } from 'claude-code';
import { type Command, parse, SCAN, type Scanner } from './command.ts';
import { bind, decoded, fault, map, none, type Option, ok, type Result, rendered, some } from './composition.ts';
import { basename, type Invocation } from './invocation.ts';
import { context, type Delivered, decided, outcome, request, type Settings, type State, settings, status, subject } from './observation/delivery.ts';
import { CALL, CLASSIC, type Columns, type Event, type Payload, row, session, TURN } from './observation/row.ts';
import { bound, DATABASE, DELIVER, DELTA, INSERT, JUDGE, OPEN, REPORT, STATE } from './observation/sql.ts';
import type { Facts, Rewrite, Walk } from './policies.ts';
import { CLOUD, commandRefusal, commandRewrite, gitPaths, locks, WORKTREE, walkStarts } from './policies.ts';
import type { Building, Judging, Spawned } from './state.d.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Once<T> = (start: () => Promise<T>) => Promise<T>;
type Decision = { readonly kind: 'deny'; readonly reason: string } | ({ readonly kind: 'rewrite'; readonly input: ToolCallInput } & Omit<Rewrite, 'command'>) | { readonly kind: 'pass' };

interface Database {
    readonly argv: Invocation;
    readonly root: string;
}
interface Observer {
    readonly chosen: Settings;
    readonly spawning: Set<string>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _SECOND_FILES: readonly RegExp[] = [
    /^(?:project\.json|\.nxignore)$/u,
    /^(?:\.mise(?:\..+)?\.toml|mise\..+\.toml|\.miserc\.toml|\.rtx\.toml|\.tool-versions|\.nvmrc|\.(?:node|python)-version)$/u,
    /^tsconfig\.(?!base\.json$).+\.json$/u,
    /^(?:\.?ruff\.toml|\.?mypy\.ini|pytest\.ini|tox\.ini|setup\.cfg)$/u,
    /^biome\.jsonc$/u,
    /^\.yamllint(?:\.yml)?$/u,
];
const _SPAWNED = atom({ plugin: 'function-hooks', key: 'spawned' } as const, {});
const _RECORDED = ['SessionStart', 'PermissionDenied', 'PostToolUse', 'PostToolUseFailure', 'PostToolBatch', 'SubagentStart', 'SubagentStop', 'UserPromptSubmit', 'StopFailure', 'PreCompact', 'PostCompact', 'SessionEnd', 'WorktreeCreate', 'WorktreeRemove'] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [PROCESS]

const _run = ($: EngineInterface, argv: Invocation, init: ProcessRunInit): Promise<Result<string>> =>
    $.process.run(argv, init).then(
        ({ exitCode, stdout, stderr }) => (exitCode === 0 ? ok(stdout) : fault<string>({ kind: 'exited', subject: argv[0], code: exitCode, stderr: stderr.trim() })),
        (cause: unknown) => fault<string>({ kind: 'unstarted', subject: argv[0], cause }),
    );

// --- [TOOL_CALL]

const _located = async ($: EngineInterface, path: string): Promise<Option<string>> => {
    const real = (spelled: string): Promise<Option<string>> =>
        $.fs.stat(spelled, { resolve: true }).then(
            ({ realPath }) => (realPath === undefined ? none : some(realPath.endsWith('/') ? realPath : `${realPath}/`)),
            () => none,
        );
    const own = await real(path);
    if (own.kind === 'some') {
        return own;
    }
    const cut = path.lastIndexOf('/');
    const folder = await real(cut < 0 ? '.' : path.slice(0, cut + 1));
    return folder.kind === 'some' ? some(`${folder.value}${path.slice(cut + 1)}/`) : none;
};

const _walk = async ($: EngineInterface, commands: readonly Command[]): Promise<Option<Walk>> => {
    const home = await $.env.get('HOME');
    if (home === undefined) {
        return none;
    }
    const [cloud, starts] = await Promise.all([_located($, `${home}/${CLOUD}`), Promise.all(walkStarts(commands, home).map(([word, path]) => _located($, path).then((place) => (place.kind === 'some' ? [[word, place.value] as const] : []))))]);
    return cloud.kind === 'some' ? some({ cloud: cloud.value, places: new Map(starts.flat()) }) : none;
};

const _facts = async ($: EngineInterface, commands: readonly Command[], walking: boolean): Promise<Facts> => {
    const [existing, walk] = await Promise.all([Promise.all(gitPaths(commands).map((path) => $.fs.exists(path).then((found) => (found ? [path] : [])))), walking ? _walk($, commands) : none]);
    return { existing: existing.flat(), walk };
};

const _queue = ($: EngineInterface, scan: Scanner, commands: readonly Command[]): Promise<Result<readonly string[]>> =>
    locks(
        scan,
        (path) => $.fs.read(path).then(ok, (cause: unknown) => fault<string>({ kind: 'unread', subject: path, cause })),
        () => $.session.repo().then((found) => (found === null ? none : some(found.root))),
        (folders) => _run($, ['mkdir', '-p', ...folders], {}),
        commands,
    );

const _callRefusal = (e: ToolCallInput): Option<string> => {
    if (e.tool === 'Write') {
        const name = basename(e.file_path);
        return _SECOND_FILES.some((pattern) => pattern.test(name)) ? some(`${name} is a second file beside its owner`) : none;
    }
    if (e.tool === 'EnterWorktree') {
        return some(`EnterWorktree ${WORKTREE}`);
    }
    return e.tool === 'Agent' && e.isolation === 'worktree' ? some(`Agent isolation worktree ${WORKTREE}`) : none;
};

const _decision = async ($: EngineInterface, e: ToolCallInput, walking: boolean): Promise<Decision> => {
    if (!((e.tool === 'Bash' || e.tool === 'Monitor') && e.command !== undefined)) {
        const refusal = _callRefusal(e);
        return refusal.kind === 'some' ? { kind: 'deny', reason: refusal.value } : { kind: 'pass' };
    }
    const { command, tool } = e;
    const scan = (text: string): Promise<Result<string>> => _run($, SCAN, { stdin: text });
    const parsed = await parse(scan, command);
    if (parsed.kind === 'fault') {
        return { kind: 'deny', reason: `command not parsed, ${rendered(parsed.fault)}` };
    }
    const { commands } = parsed.value;
    const refusal = commandRefusal(tool, parsed.value, await _facts($, commands, walking && tool === 'Bash'));
    if (refusal.kind === 'some') {
        return { kind: 'deny', reason: refusal.value };
    }
    const queue = tool === 'Bash' ? await _queue($, scan, commands) : ok<readonly string[]>([]);
    if (queue.kind === 'fault') {
        return { kind: 'deny', reason: `command not queued, ${rendered(queue.fault)}` };
    }
    const rewrite = commandRewrite(commands, command, queue.value);
    if (rewrite.kind === 'none') {
        return { kind: 'pass' };
    }
    const { command: rewritten, ...change } = rewrite.value;
    return { ...change, kind: 'rewrite', input: { ...e, command: rewritten } };
};

const _answer = async ($: EngineInterface, decision: Decision, e: ToolCallInput, next: (input: ToolCallInput) => Promise<ToolCallResult>): Promise<ToolCallResult> => {
    if (decision.kind === 'deny') {
        return { deny: decision.reason };
    }
    if (decision.kind === 'pass') {
        return next(e);
    }
    $.ui.log(decision.note);
    const ran = await next(decision.input);
    return ran.deny === undefined ? { ...ran, context: [...(ran.context ?? []), decision.context] } : ran;
};

// --- [RECORD]

const _once = <T>(): Once<T> => {
    let held: Promise<T> | undefined;
    return (start) => {
        held ??= start();
        return held;
    };
};

const _open = async ($: EngineInterface): Promise<Option<Database>> => {
    const repo = await $.session.repo();
    if (repo === null) {
        $.ui.log('rows not recorded, session runs outside a git repository');
        return none;
    }
    const { root } = repo;
    const located = await $.fs.write(`${root}/${DELTA}`, '').then(
        () => ok<Invocation>(['sqlite3', '-bail', '-cmd', '.timeout 10000', `${root}/${DATABASE}`]),
        (cause: unknown) => fault<Invocation>({ kind: 'unwritten', subject: `${root}/${DELTA}`, cause }),
    );
    const opened = await bind(located, (argv) => _run($, argv, { stdin: OPEN, cwd: root }).then((applied) => map(applied, () => argv)));
    if (opened.kind === 'fault') {
        $.ui.log(`rows not recorded, ${rendered(opened.fault)}`);
        return none;
    }
    const mode = map(await _run($, opened.value, { stdin: 'pragma journal_mode=wal;', cwd: root }), (text) => text.trim());
    if (mode.kind === 'fault' || mode.value !== 'wal') {
        $.ui.log(`journal mode not switched, ${mode.kind === 'fault' ? rendered(mode.fault) : mode.value}`);
    }
    return some({ argv: opened.value, root });
};

const _record = async ($: EngineInterface, db: Database, event: Event, value: Payload, columns: Columns, ts: number): Promise<void> => {
    const own = session(value, columns);
    const built = row(event, value, columns, own.kind === 'some' ? own.value : await $.session.id(), ts);
    const written = await _run($, db.argv, { stdin: bound(built, INSERT), cwd: db.root });
    if (written.kind === 'fault') {
        $.ui.log(`${built.event} row ${built.tool_use_id ?? built.session_id} not written, ${rendered(written.fault)}`);
    }
};

const _stamped = ($: EngineInterface, database: Once<Option<Database>>): Promise<readonly [Option<Database>, number]> => Promise.all([database(() => _open($)), $.clock.now()]);

// --- [SPAWN]

const _launch = async ($: EngineInterface, observer: Observer, spawned: Spawned, key: string): Promise<void> => {
    observer.spawning.add(spawned.agent);
    const { prompt, description } = request(spawned, key);
    const id = await $.agent.spawn({ prompt, subagentType: spawned.agent, description, cwd: spawned.lineage.worktree }).then(
        (answer) => {
            $.ui.log(outcome(spawned, answer));
            return answer.agentId;
        },
        (cause: unknown) => {
            $.ui.log(`${spawned.agent} did not spawn, ${String(cause)}`);
        },
    );
    observer.spawning.delete(spawned.agent);
    if (id !== undefined) {
        await update($, _SPAWNED, (held) => ({ ...held, [id]: spawned }));
    }
};

const _removed = ($: EngineInterface, id: string): Promise<unknown> => update($, _SPAWNED, ({ [id]: _ended, ...kept }) => kept);

const _judged = async ($: EngineInterface, db: Database, id: string, pending: Judging, ts: number): Promise<void> => {
    const over = subject(pending);
    const judged = await _run($, db.argv, { stdin: bound({ ...pending, ...pending.lineage, id, at: ts }, JUDGE), cwd: pending.lineage.worktree });
    if (judged.kind === 'ok' && judged.value.trim() === '') {
        $.ui.log(`${pending.agent} ${id} answered with no transition over ${over}, range stays unjudged`);
        return;
    }
    await _removed($, id);
    $.ui.log(judged.kind === 'fault' ? `judged_range row not written, ${rendered(judged.fault)}` : `${pending.agent} ${id} judged ${over}`);
};

const _reported = async ($: EngineInterface, db: Database, id: string, pending: Building, ts: number): Promise<void> => {
    await _removed($, id);
    const reported = await _run($, db.argv, { stdin: bound({ ...pending, ...pending.lineage, id, at: ts }, REPORT), cwd: pending.lineage.worktree });
    $.ui.log(reported.kind === 'fault' ? `report rows not written, ${rendered(reported.fault)}` : `${pending.agent} ${id} reported ${subject(pending)}`);
};

const _placed = async ($: EngineInterface, e: Frozen<TurnCompleteInput>, cwd: string): Promise<Payload> => {
    const lines = (text: string): readonly string[] => text.split('\n').filter((line) => line !== '');
    const directories = map(await _run($, ['yq', '-r', '.ruleDirs[], .utilDirs[]', 'sgconfig.yml'], { cwd }), lines);
    const placed = await bind(directories, async (paths) => {
        const [changed, untracked] = await Promise.all([_run($, ['git', 'diff', '--name-only', '--diff-filter=d', 'HEAD', '--', ...paths], { cwd }), _run($, ['git', 'ls-files', '--others', '--exclude-standard', '--', ...paths], { cwd })]);
        return bind(changed, (tracked) => map(untracked, (fresh) => [...lines(tracked), ...lines(fresh)]));
    });
    if (placed.kind === 'fault') {
        $.ui.log(`placed rules not read, ${rendered(placed.fault)}`);
        return e;
    }
    return { ...e, placed: placed.value };
};

const _completed = async ($: EngineInterface, db: Database, e: Frozen<TurnCompleteInput>, ts: number): Promise<void> => {
    const id = e.agentId;
    const pending = id === undefined ? undefined : (await read($, _SPAWNED))[id];
    await _record($, db, 'turn.complete', pending?.kind === 'category' && e.reason === 'answer' ? await _placed($, e, pending.lineage.worktree) : e, TURN, ts);
    if (id === undefined || pending === undefined) {
        return;
    }
    if (e.reason !== 'answer') {
        await _removed($, id);
        $.ui.log(`${pending.agent} ${id} ended on ${e.reason} over ${subject(pending)}, nothing recorded`);
        return;
    }
    await (pending.kind === 'range' ? _judged($, db, id, pending, ts) : _reported($, db, id, pending, ts));
};

// --- [DELIVERY]

const _boundary = async ($: EngineInterface, db: Database, e: Frozen<ClassicHookInputs['Stop' | 'SubagentStop']>, to: number, observer: Observer): Promise<readonly string[]> => {
    const [toplevel, branch, agents] = await Promise.all([_run($, ['git', 'rev-parse', '--show-toplevel'], { cwd: e.cwd }), _run($, ['git', 'branch', '--show-current'], { cwd: e.cwd }), $.agent.list()]);
    const { chosen } = observer;
    const located = bind(toplevel, (worktree) => map(branch, (name) => ({ main: db.root, worktree: worktree.trim(), branch: name.trim() })));
    if (located.kind === 'fault') {
        $.ui.log(`boundary skipped, ${rendered(located.fault)}`);
        return [];
    }
    const lineage = located.value;
    const state = decoded<State>('sqlite3', await _run($, db.argv, { stdin: bound({ ...lineage, ...chosen, to }, STATE), cwd: lineage.worktree }));
    if (state.kind === 'fault') {
        $.ui.log(`boundary skipped, ${rendered(state.fault)}`);
        return [];
    }
    const seen = state.value;
    const text = status(seen);
    $.ui.status(text.kind === 'some' ? text.value : undefined);
    const plan = decided(seen, chosen, [...agents.flatMap((agent) => (agent.status === 'running' ? [agent.type] : [])), ...observer.spawning]);
    if (plan.range) {
        _launch($, observer, { kind: 'range', agent: chosen.editAgent, lineage, from: seen.from, to }, seen.key);
    }
    if (plan.category.kind === 'some') {
        _launch($, observer, { kind: 'category', agent: chosen.categoryAgent, lineage, session: e.session_id, category: plan.category.value }, seen.key);
    }
    if (e.hook_event_name !== 'Stop' || !plan.deliver) {
        return [];
    }
    const delivered = decoded<Delivered>('sqlite3', await _run($, db.argv, { stdin: bound({ key: seen.key, session: e.session_id, at: to }, DELIVER), cwd: lineage.worktree }));
    if (delivered.kind === 'fault') {
        $.ui.log(`boundary skipped, delivery rows not written, ${rendered(delivered.fault)}`);
        return [];
    }
    return context(delivered.value, lineage.branch);
};

const _observed = async ($: EngineInterface, database: Once<Option<Database>>, e: Frozen<ClassicHookInputs[(typeof _RECORDED)[number] | 'Stop']>, observer: Observer): Promise<readonly string[]> => {
    const [db, ts] = await _stamped($, database);
    if (db.kind === 'none') {
        return [];
    }
    await _record($, db.value, e.hook_event_name, e.hook_event_name === 'Stop' || e.hook_event_name === 'SessionEnd' ? { ...e, usage: await $.session.usage() } : e, CLASSIC, ts);
    if (e.hook_event_name === 'SessionEnd') {
        $.ui.status(undefined);
    }
    const stopping = (e.hook_event_name === 'Stop' || (e.hook_event_name === 'SubagentStop' && e.agent_type !== '')) && !e.stop_hook_active;
    return stopping ? _boundary($, db.value, e, ts, observer) : [];
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const _observation = (on: On, options: PluginOptions, database: Once<Option<Database>>): void => {
    const observer: Observer = { chosen: settings(options), spawning: new Set() };

    on('classic.*', { hook_event_name: _RECORDED }, async ($, e, next) => {
        if (next.is('!classic.PreToolUse', e)) {
            await _observed($, database, e, observer);
        }
        return next(e);
    });

    on('classic.Stop', async ($, e, next) => {
        const entries = await _observed($, database, e, observer);
        const result = await next(e);
        return entries.length === 0 ? result : { ...result, additionalContext: [...(result.additionalContext ?? []), ...entries] };
    });

    on('turn.*', async ($, e, next) => {
        const [db, ts] = await _stamped($, database);
        if (db.kind === 'some') {
            await (next.is('turn.complete', e) ? _completed($, db.value, e, ts) : _record($, db.value, next.event, e, TURN, ts));
        }
        return next(e);
    });
};

const register: Register = (on, options) => {
    const walking = options.walkPolicy === true;
    const observing = options.observation === true;
    const database = _once<Option<Database>>();

    on('tool.call', async ($, e, next) => {
        const answer = await _answer($, await _decision($, e, walking), e, next);
        if (answer.deny !== undefined && observing) {
            const [db, ts] = await _stamped($, database);
            if (db.kind === 'some') {
                await _record($, db.value, 'tool.call', { ...e, deny: answer.deny, trace: next.trace }, CALL, ts);
            }
        }
        return answer;
    });

    if (observing) {
        _observation(on, options, database);
    }
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
