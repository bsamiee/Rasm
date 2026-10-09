import type { ClassicHookInputs, EngineInterface, Frozen, Next, PluginOptions, ProcessRunInit, Register, ToolCallInput, ToolCallResult } from 'claude-code';
import { atom, memberOf, read, update } from 'claude-code';
import { bind, both, decoded, type Fault, fault, fromUndefined, map, none, type Option, ok, type Result, rendered, some } from '../composition.ts';
import { pointer, touched } from '../context/plan.ts';
import { type Boundary, delivered, outcome, request, type Spawn } from '../observation/delivery.ts';
import { CALL, CLASSIC, type Columns, type Event, type Payload, row, TURN, USAGE } from '../observation/row.ts';
import { BOUNDARY, bound, DELTA, INSERT, JUDGE, OPEN, REPORT, sqlite } from '../observation/sql.ts';
import { SCAN } from '../policies/command.ts';
import type { Invocation } from '../policies/invocation.ts';
import { commandDecision, type Decision, type Host, pathRefusal, worktreeRefusal } from '../policies/policies.ts';
import { caption, capturePath, recorded } from '../ui/capture.ts';
import { down, kickstart, LAUNCHD_AGENTS, LISTENERS, remaining, services, UID } from '../ui/health.ts';
import { band, bandRows, captureRow, resultRow } from '../ui/render.tsx';
import type { Capture, Notice, Service } from './state.d.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Stamped {
    readonly root: string;
    readonly ts: number;
}
interface Bindings {
    readonly main: string;
    readonly worktree: string;
    readonly branch: string;
    readonly key: string;
    readonly session: string;
    readonly at: number;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _MAIN = 'main';
const _LOG = 'log';

// --- [STATE] ---------------------------------------------------------------------------

const _DATABASE = atom({ plugin: 'function-hooks', key: 'database' } as const, none);
const _NOTICE = atom({ plugin: 'function-hooks', key: 'notice' } as const, none);
const _DOWN = atom({ plugin: 'function-hooks', key: 'down' } as const, []);
const _EDITS = atom({ plugin: 'function-hooks', key: 'edits' } as const, { format: [], diagnostics: [] });
const _CAPTURE = atom({ plugin: 'function-hooks', key: 'capture' } as const, none);
const _PLAN = atom({ plugin: 'function-hooks', key: 'plan' } as const, { path: none, taskFile: none });
const _SAID = atom({ plugin: 'function-hooks', key: 'said' } as const, []);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [HOST]

const _once = async ($: EngineInterface, loop: string, texts: readonly string[]): Promise<readonly string[]> => {
    if (texts.length === 0) {
        return [];
    }
    let fresh: readonly string[] = [];
    await update($, memberOf(_SAID, { requestId: loop }), (held) => {
        fresh = [...new Set(texts)].filter((text) => !held.includes(text));
        return [...held, ...fresh];
    });
    return fresh;
};

const _result = <T>(pending: Promise<T>, kind: 'unstarted' | 'unread' | 'unwritten', subject: string): Promise<Result<T>> => pending.then(ok, (cause: unknown) => fault<T>({ kind, subject, cause }));

const _faulted = async ($: EngineInterface, line: string, faults: readonly Fault[]): Promise<void> => {
    (await _once($, _LOG, [`${line}, ${rendered(faults)}`])).forEach((text) => {
        $.ui.log(text);
    });
};

const _run = async ($: EngineInterface, argv: Invocation, init: ProcessRunInit, exits: readonly number[]): Promise<Result<string>> =>
    bind(await _result($.process.run(argv, init), 'unstarted', argv[0]), ({ exitCode, stdout, stderr }) => (exits.includes(exitCode) ? ok(stdout) : fault<string>({ kind: 'exited', subject: argv[0], code: exitCode, stderr: stderr.trim() })));

const _host = ($: EngineInterface): Host => ({
    scan: (text) => _run($, SCAN, { stdin: text }, [0]),
    repo: () => $.session.repo().then((found) => (found === null ? none : some(found.root))),
    exists: (path) => $.fs.exists(path),
    real: (path) =>
        $.fs.stat(path, { resolve: true }).then(
            ({ realPath }) => fromUndefined(realPath),
            () => none,
        ),
    home: () => $.env.get('HOME').then(fromUndefined),
});

const _toplevel = async ($: EngineInterface): Promise<Result<string>> => map(await _run($, ['git', 'rev-parse', '--show-toplevel'], { cwd: await $.session.root() }, [0]), (printed) => printed.trim());

// --- [NOTICE]

const _cleared = async ($: EngineInterface, at: Option<number>): Promise<void> => {
    const clears = (held: Option<Notice>): boolean => held.kind === 'some' && (at.kind === 'none' || at.value === held.value.at);
    await (clears(await read($, _NOTICE)) ? update($, _NOTICE, (held) => (clears(held) ? none : held)) : undefined);
};

const _noticed = async ($: EngineInterface, text: string): Promise<void> => {
    const shownMs = 8000;
    const at = await $.clock.now();
    await update($, _NOTICE, () => some({ text, at }));
    $.clock.after(shownMs, () => _cleared($, some(at)));
};

// --- [SERVICES]

const _health = async ($: EngineInterface, known: readonly Service[]): Promise<void> => {
    const [listened, held] = await Promise.all([_run($, LISTENERS, {}, [0, 1]), read($, _DOWN)]);
    const found = listened.kind === 'ok' ? down(known, listened.value) : held;
    await (found.length === held.length && found.every(({ name }) => held.some((service) => service.name === name)) ? undefined : update($, _DOWN, () => found));
};

const _watched = async ($: EngineInterface): Promise<void> => {
    const healthMs = 60_000;
    const repo = await $.session.repo();
    const known = repo === null ? ok<readonly Service[]>([]) : services(await _run($, LAUNCHD_AGENTS, { cwd: repo.root }, [0]));
    if (known.kind === 'ok' && known.value.length > 0) {
        $.clock.every(healthMs, () => _health($, known.value));
        await _health($, known.value);
    }
};

const _restart = async ($: EngineInterface): Promise<void> => {
    const [held, uid] = await Promise.all([read($, _DOWN), _run($, UID, {}, [0])]);
    if (uid.kind === 'fault') {
        await _faulted($, 'services not restarted', uid.faults);
        return;
    }
    const ran = await Promise.all(held.map(async ({ name }) => ({ name, result: await _run($, kickstart(name, uid.value), {}, [0]) })));
    const restarted = new Set(ran.flatMap(({ name, result }) => (result.kind === 'ok' ? [name] : [])));
    const failed = ran.flatMap(({ result }) => (result.kind === 'fault' ? result.faults : []));
    await (failed.length === 0 ? undefined : _faulted($, 'services not restarted', failed));
    await (restarted.size === 0 ? undefined : Promise.all([_noticed($, `${[...restarted].join(' and ')} restarted · reconnect with /mcp`), update($, _DOWN, (current) => remaining(current, restarted))]));
};

// --- [TOOL_CALL]

const _decision = ($: EngineInterface, e: ToolCallInput, walkPolicy: boolean): Promise<Decision> => {
    if ((e.tool === 'Bash' || e.tool === 'Monitor') && e.command !== undefined) {
        return commandDecision(_host($), e.tool, e.command, e.tool_use_id, walkPolicy);
    }
    if (e.tool === 'EnterWorktree' || (e.tool === 'Agent' && e.isolation === 'worktree')) {
        return Promise.resolve({ kind: 'deny', reason: worktreeRefusal(e.tool) });
    }
    const refusal = e.tool === 'Write' ? pathRefusal([e.file_path]) : none;
    return Promise.resolve(refusal.kind === 'some' ? { kind: 'deny', reason: refusal.value } : { kind: 'allow', rewrite: none });
};

const _called = async (e: Frozen<ToolCallInput>, next: Next<'tool.call'>): Promise<{ readonly answer: ToolCallResult; readonly paths: readonly string[] }> => {
    if (e.tool === 'Edit' || e.tool === 'Write') {
        const answer = await next(e);
        return { answer, paths: answer.deny === undefined && answer.isError !== true && answer.result.staged !== true ? [answer.result.filePath] : [] };
    }
    if (e.tool === 'NotebookEdit') {
        const answer = await next(e);
        return { answer, paths: answer.deny === undefined && answer.isError !== true && answer.result.error === undefined ? [answer.result.notebook_path] : [] };
    }
    return { answer: await next(e), paths: [] };
};

const _queued = async ($: EngineInterface, loop: string, paths: readonly string[]): Promise<void> => {
    if (paths.length === 0) {
        return;
    }
    const merge = (held: readonly string[]): readonly string[] => [...new Set([...held, ...paths])];
    await update($, memberOf(_EDITS, { requestId: loop }), (held) => ({ format: merge(held.format), diagnostics: merge(held.diagnostics) }));
};

const _drained = async ($: EngineInterface, loop: string, operation: 'format' | 'diagnostics'): Promise<readonly string[]> => {
    let paths: readonly string[] = [];
    await update($, memberOf(_EDITS, { requestId: loop }), (held) => {
        paths = held[operation];
        return { ...held, [operation]: [] };
    });
    return paths;
};

const _node = async ($: EngineInterface, operation: 'format' | 'diagnostics', root: string, paths: readonly string[]): Promise<Result<string>> => {
    const stream = $.process.spawn({ argv: ['node', `${$.plugin.root}/repository/${operation}-cli.ts`], cwd: root, input: JSON.stringify({ root, paths }) });
    let stdout = '';
    let stderr = '';
    for await (const chunk of stream) {
        if (chunk.stream === 'stdout') {
            stdout += chunk.text;
        } else {
            stderr += chunk.text;
        }
    }
    const { code, signal } = await stream.result;
    if (code === null) {
        return fault({ kind: 'unstarted', subject: operation, cause: signal });
    }
    return code === 0 ? ok(stdout) : fault({ kind: 'exited', subject: operation, code, stderr });
};

const _processed = async ($: EngineInterface, loop: string, operation: 'format' | 'diagnostics'): Promise<readonly string[]> => {
    const paths = await _drained($, loop, operation);
    if (paths.length === 0) {
        return [];
    }
    const ran = await bind(await _toplevel($), (root) => _result(_node($, operation, root, paths), 'unstarted', operation));
    const output = bind(ran, (printed) => decoded<readonly string[]>(operation, printed));
    if (output.kind === 'fault') {
        await update($, memberOf(_EDITS, { requestId: loop }), (held) => ({ ...held, [operation]: [...new Set([...paths, ...held[operation]])] }));
        return [rendered(output.faults)];
    }
    if (operation === 'format') {
        await update($, memberOf(_EDITS, { requestId: loop }), (held) => ({ ...held, diagnostics: [...new Set([...held.diagnostics, ...paths])] }));
    }
    return output.value;
};

const _captured = async ($: EngineInterface, e: ToolCallInput, text: string): Promise<void> => {
    const found = capturePath(e.tool, text);
    if (found.kind === 'none') {
        return;
    }
    const capture = await bind(await _toplevel($), async (root) => {
        const path = `${root}/${found.value}`;
        const [printed, stat] = await Promise.all([_run($, ['exiftool', '-j', '-Capture', path], {}, [0]), _result($.fs.stat(path), 'unread', path)]);
        return map(both(recorded(printed), stat), ([record, { mtimeMs }]): Capture => ({ path, record, generation: mtimeMs }));
    });
    await (capture.kind === 'ok' ? update($, memberOf(_CAPTURE, { requestId: e.tool_use_id }), () => some(capture.value)) : undefined);
};

const _planned = async ($: EngineInterface, e: ToolCallInput): Promise<void> => {
    if (e.agentId !== undefined || (e.tool !== 'Read' && e.tool !== 'Write' && e.tool !== 'Edit')) {
        return;
    }
    const [home, plan] = await Promise.all([$.env.get('HOME'), read($, _PLAN)]);
    const moved = home === undefined ? none : touched(plan, e.file_path, e.tool === 'Read' ? none : some(e.tool === 'Write' ? e.content : e.new_string), home);
    await (moved.kind === 'some' ? update($, _PLAN, () => moved.value) : undefined);
};

// --- [RECORDING]

const _opened = async ($: EngineInterface): Promise<Option<string>> => {
    const repo = await $.session.repo();
    if (repo === null) {
        return none;
    }
    const { root } = repo;
    const delta = `${root}/${DELTA}`;
    const applied = await bind(await _result($.fs.write(delta, ''), 'unwritten', delta), () => _run($, sqlite(root), { stdin: OPEN, cwd: root }, [0]));
    if (applied.kind === 'fault') {
        await _faulted($, 'rows not recorded', applied.faults);
        return none;
    }
    return some(root);
};

const _database = async ($: EngineInterface, held: Option<Option<string>>): Promise<Option<string>> => {
    const opened = held.kind === 'some' ? held.value : await _opened($);
    await (held.kind === 'none' ? update($, _DATABASE, () => some(opened)) : undefined);
    return opened;
};

const _record = async ($: EngineInterface, event: Event, value: Payload, columns: Columns): Promise<Option<Stamped>> => {
    const [root, ts, session, payload] = await Promise.all([read($, _DATABASE).then((held) => _database($, held)), $.clock.now(), $.session.id(), USAGE.includes(event) ? $.session.usage().then((usage): Payload => ({ ...value, usage })) : value]);
    if (root.kind === 'none') {
        return none;
    }
    const inserted = await _run($, sqlite(root.value), { stdin: bound(row(event, payload, columns, session, ts), INSERT), cwd: root.value }, [0]);
    await (inserted.kind === 'ok' ? undefined : _faulted($, 'observation row not written', inserted.faults));
    return some({ root: root.value, ts });
};

const _denied = ($: EngineInterface, observation: Option<PluginOptions>, e: ToolCallInput, reason: string, trace: unknown): Promise<Option<Stamped>> => (observation.kind === 'some' ? _record($, 'tool.call', { ...e, deny: reason, trace }, CALL) : Promise.resolve(none));

const _spawn = async ($: EngineInterface, spawn: Spawn, bindings: Bindings): Promise<void> => {
    const answer = await _result($.agent.spawn(request(spawn, bindings.key, bindings.worktree)), 'unstarted', spawn.agent);
    if (answer.kind === 'fault') {
        await _faulted($, 'agent not spawned', answer.faults);
        return;
    }
    if (answer.value.deny !== undefined) {
        $.ui.log(outcome(spawn, answer.value));
        return;
    }
    const { agentId } = answer.value;
    const [written] = await Promise.all([agentId === undefined ? undefined : _run($, sqlite(bindings.main), { stdin: bound({ ...spawn, ...bindings, id: agentId }, spawn.kind === 'range' ? JUDGE : REPORT), cwd: bindings.worktree }, [0]), _noticed($, outcome(spawn, answer.value))]);
    await (written?.kind === 'fault' ? _faulted($, 'observation row not written', written.faults) : undefined);
};

const _boundary = async ($: EngineInterface, options: PluginOptions, session: string, { root: main, ts }: Stamped): Promise<readonly string[]> => {
    const [toplevel, branch] = await Promise.all([_toplevel($), $.session.root().then((cwd) => _run($, ['git', 'branch', '--show-current'], { cwd }, [0]))]);
    const decided = await bind(both(toplevel, branch), async ([worktree, name]) => {
        const lineage = { main, worktree, branch: name.trim() };
        return map(decoded<Boundary>('sqlite3', await _run($, sqlite(main), { stdin: bound({ ...options, ...lineage, to: ts, session }, BOUNDARY), cwd: worktree }, [0])), (boundary) => ({ ...boundary, bindings: { ...lineage, key: boundary.key, session, at: ts } }));
    });
    if (decided.kind === 'fault') {
        await _faulted($, 'boundary skipped', decided.faults);
        return [];
    }
    const { bindings, spawns, findings } = decided.value;
    await Promise.all(spawns.map((spawn) => _spawn($, spawn, bindings)));
    return delivered(findings, bindings.branch);
};

// --- [WRITERS]

const _stopped = async <E extends Frozen<ClassicHookInputs['Stop' | 'SubagentStop']>, R extends { readonly additionalContext?: readonly string[] }>($: EngineInterface, e: E, next: (input: E) => Promise<R>, observation: Option<PluginOptions>): Promise<R> => {
    const loop = e.hook_event_name === 'SubagentStop' ? e.agent_id : _MAIN;
    const stamped = observation.kind === 'some' ? await _record($, e.hook_event_name, e, CLASSIC) : none;
    const result = await next(e);
    const boundary = observation.kind === 'some' && stamped.kind === 'some' && e.hook_event_name === 'Stop' ? _boundary($, observation.value, e.session_id, stamped.value) : [];
    const written = await _processed($, loop, 'format');
    const checked = await _processed($, loop, 'diagnostics');
    const context = e.stop_hook_active ? [] : await _once($, loop, [...(await boundary), ...written, ...checked]);
    return context.length === 0 ? result : { ...result, additionalContext: [...(result.additionalContext ?? []), ...context] };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const register: Register = (on, options) => {
    const walkPolicy = options.walkPolicy === true;
    const observation = options.observation === true ? some(options) : none;

    on('session.start', async ($, e, next) => {
        await Promise.all([_watched($), observation.kind === 'some' ? _database($, none) : undefined]);
        return next(e);
    });

    on('tool.call', async ($, e, next) => {
        const decision = await _decision($, e, walkPolicy);
        if (decision.kind === 'deny') {
            await _denied($, observation, e, decision.reason, next.trace);
            return { deny: decision.reason };
        }
        const { rewrite } = decision;
        const loop = e.agentId ?? _MAIN;
        const [{ answer, paths }] = await Promise.all([_called((e.tool === 'Bash' || e.tool === 'Monitor') && rewrite.kind === 'some' ? { ...e, command: rewrite.value.command } : e, next), rewrite.kind === 'some' ? _noticed($, rewrite.value.notice) : undefined]);
        if (answer.deny !== undefined) {
            await _denied($, observation, e, answer.deny, next.trace);
            return answer;
        }
        const failed = answer.isError === true;
        await Promise.all([_queued($, loop, paths), failed || answer.text === undefined ? undefined : _captured($, e, answer.text), failed ? undefined : _planned($, e)]);
        const context = await _once($, loop, [...(rewrite.kind === 'some' ? [rewrite.value.context] : []), ...(await _processed($, loop, 'diagnostics'))]);
        return context.length === 0 ? answer : ({ ...answer, context: [...(answer.context ?? []), ...context] } satisfies ToolCallResult);
    }).catch((_$, e, next) => (next.called ? next(e) : { deny: 'function-hooks policy did not run' }));

    on('turn.complete', async ($, e, next) => {
        const loop = e.agentId ?? _MAIN;
        const said = memberOf(_SAID, { requestId: loop });
        const logged = memberOf(_SAID, { requestId: _LOG });
        const [notes, faults] = await Promise.all([read($, said), loop === _MAIN ? read($, logged) : [], observation.kind === 'some' ? _record($, 'turn.complete', e, TURN) : undefined]);
        const [result] = await Promise.all([next(e), notes.length === 0 ? undefined : update($, said, () => []), faults.length === 0 ? undefined : update($, logged, () => [])]);
        return result;
    });

    on('classic.Stop', ($, e, next) => _stopped($, e, next, observation));

    on('classic.SubagentStop', ($, e, next) => _stopped($, e, next, observation));

    on('prompt.submit', async ($, e, next) => {
        await _cleared($, none);
        return next(e);
    });

    on('prompt.compose', async ($, e, next) => {
        const [composed, plan] = await Promise.all([next(e), read($, _PLAN)]);
        const shown = pointer(plan);
        return shown.kind === 'none' ? composed : { ...composed, sections: [...composed.sections, { id: 'function-hooks:plan', text: shown.value, scope: 'session' }] };
    });

    on('session.compact', async ($, e, next) => {
        const shown = pointer(await read($, _PLAN));
        return next(shown.kind === 'none' || e.agentId !== undefined ? e : { ...e, instructions: [...(e.instructions === undefined ? [] : [e.instructions]), shown.value].join('\n') });
    });

    on('ui.render', { component: 'AbovePrompt' }, async ($, e, next) => {
        const [below, notice, held] = await Promise.all([next(e), read($, _NOTICE), read($, _DOWN)]);
        const shown = e.props.hasSurvey ? [] : bandRows(notice, held, () => _restart($));
        return shown.length === 0 ? below : band($.ui.resolve(e), shown, below);
    });

    on('ui.render', { component: 'ToolResult' }, async ($, e, next) => {
        const [below, capture] = await Promise.all([next(e), read($, memberOf(_CAPTURE, e))]);
        if (capture.kind === 'some') {
            return e.surface === 'terminal' && e.viewport !== undefined ? captureRow($.ui.resolve(e), below, capture.value, e.viewport.columns) : resultRow($.ui.resolve(e), below, caption(capture.value.path, capture.value.record));
        }
        return below;
    });

    if (observation.kind === 'some') {
        on('classic.*', { hook_event_name: ['SessionStart', 'PermissionDenied', 'PostToolUse', 'PostToolUseFailure', 'PostToolBatch', 'SubagentStart', 'UserPromptSubmit', 'StopFailure', 'PreCompact', 'PostCompact', 'SessionEnd', 'WorktreeCreate', 'WorktreeRemove'] }, async ($, e, next) => {
            await (next.is('!classic.PreToolUse', e) ? _record($, e.hook_event_name, e, CLASSIC) : undefined);
            return next(e);
        });

        on('turn.start', async ($, e, next) => {
            await _record($, 'turn.start', e, TURN);
            return next(e);
        });

        on('turn.step', async function* ($, e, next) {
            await _record($, 'turn.step', e, TURN);
            return yield* next(e);
        });
    }
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
