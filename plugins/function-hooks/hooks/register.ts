import type { ClassicHookInputs, EngineInterface, Frozen, PluginOptions, ProcessRunInit, Register, ToolCallInput, ToolCallResult } from 'claude-code';
import { atom, memberOf, read, update } from 'claude-code';
import { bind, both, decoded, type Fault, fault, fromUndefined, map, none, type Option, ok, type Result, rendered, some } from '../composition.ts';
import { fromAstGrep, lines, summary } from '../context/diagnostics.ts';
import { pointer, touched } from '../context/plan.ts';
import { type Boundary, delivered, outcome, request, type Spawn } from '../observation/delivery.ts';
import { CALL, CLASSIC, type Columns, type Event, type Payload, row, TURN, USAGE } from '../observation/row.ts';
import { BOUNDARY, bound, DELTA, INSERT, JUDGE, OPEN, REPORT, sqlite } from '../observation/sql.ts';
import { SCAN } from '../policies/command.ts';
import type { Invocation } from '../policies/invocation.ts';
import { commandDecision, type Decision, type Host, pathRefusal, worktreeRefusal } from '../policies/policies.ts';
import { reformatted } from '../repository/format.ts';
import { caption, capturePath, recorded } from '../ui/capture.ts';
import { down, kickstart, LAUNCHD_AGENTS, LISTENERS, remaining, services, UID } from '../ui/health.ts';
import { band, bandRows, captureRow, resultRow } from '../ui/render.tsx';
import type { Capture, Diagnostic, Notice, Service } from './state.d.ts';

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
const _EDITED = atom({ plugin: 'function-hooks', key: 'edited' } as const, []);
const _DIAGNOSTICS = atom({ plugin: 'function-hooks', key: 'diagnostics' } as const, []);
const _CAPTURE = atom({ plugin: 'function-hooks', key: 'capture' } as const, none);
const _PLAN = atom({ plugin: 'function-hooks', key: 'plan' } as const, { path: none, taskFile: none });
const _SAID = atom({ plugin: 'function-hooks', key: 'said' } as const, []);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [HOST]

const _once = async ($: EngineInterface, loop: string, texts: readonly string[]): Promise<readonly string[]> => {
    const said = memberOf(_SAID, { requestId: loop });
    const held = await read($, said);
    const fresh = [...new Set(texts)].filter((text) => !held.includes(text));
    await (fresh.length === 0 ? undefined : update($, said, (current) => [...current, ...fresh]));
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
    read: (path) => _result($.fs.read(path), 'unread', path),
    repo: () => $.session.repo().then((found) => (found === null ? none : some(found.root))),
    make: (folders) => _run($, ['mkdir', '-p', ...folders], {}, [0]),
    exists: (path) => $.fs.exists(path),
    real: (path) =>
        $.fs.stat(path, { resolve: true }).then(
            ({ realPath }) => fromUndefined(realPath),
            () => none,
        ),
    home: () => $.env.get('HOME').then(fromUndefined),
    workspaceData: () => $.env.get('NX_WORKSPACE_DATA_DIRECTORY').then(fromUndefined),
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

const _edited = async ($: EngineInterface, e: ToolCallInput, loop: string): Promise<readonly string[]> => {
    if (e.tool !== 'Edit' && e.tool !== 'Write' && e.tool !== 'NotebookEdit') {
        return [];
    }
    const path = e.tool === 'NotebookEdit' ? e.notebook_path : e.file_path;
    const root = await _toplevel($);
    if (root.kind === 'fault' || !path.startsWith(`${root.value}/`)) {
        return [];
    }
    const [scanned] = await Promise.all([
        e.tool === 'NotebookEdit' ? ok<readonly Diagnostic[]>([]) : _run($, ['ast-grep', 'scan', '--json=compact', path], { cwd: root.value }, [0, 1]).then((printed) => fromAstGrep(printed, path)),
        update($, memberOf(_EDITED, { requestId: loop }), (held) => (held.includes(path) ? held : [...held, path])),
    ]);
    if (scanned.kind === 'fault') {
        return [`${rendered(scanned.faults)}. Run ast-grep scan ${path} for the full cause`];
    }
    await (scanned.value.length === 0 ? undefined : update($, memberOf(_DIAGNOSTICS, { requestId: e.tool_use_id }), () => scanned.value));
    return lines(scanned.value);
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

const _formatted = async ($: EngineInterface, loop: string): Promise<readonly string[]> => {
    const [files, toplevel] = await Promise.all([read($, memberOf(_EDITED, { requestId: loop })), _toplevel($)]);
    return toplevel.kind === 'fault'
        ? []
        : reformatted(
              {
                  exec: (argv, exits) => _run($, argv, { cwd: toplevel.value, timeoutMs: 120_000 }, exits),
                  read: (path) => _result($.fs.read(path), 'unread', path),
                  mtime: (path) =>
                      $.fs.stat(path).then(
                          ({ mtimeMs }) => some(mtimeMs),
                          () => none,
                      ),
              },
              toplevel.value,
              files,
          );
};

const _stopped = async <E extends Frozen<ClassicHookInputs['Stop' | 'SubagentStop']>, R extends { readonly additionalContext?: readonly string[] }>($: EngineInterface, e: E, next: (input: E) => Promise<R>, observation: Option<PluginOptions>): Promise<R> => {
    const stamped = observation.kind === 'some' ? await _record($, e.hook_event_name, e, CLASSIC) : none;
    const result = await next(e);
    const boundary = observation.kind === 'some' && stamped.kind === 'some' && e.hook_event_name === 'Stop' ? _boundary($, observation.value, e.session_id, stamped.value) : [];
    const context = e.stop_hook_active ? [] : (await Promise.all([boundary, _formatted($, e.hook_event_name === 'SubagentStop' ? e.agent_id : _MAIN)])).flat();
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
        const [answer] = await Promise.all([(e.tool === 'Bash' || e.tool === 'Monitor') && rewrite.kind === 'some' ? next({ ...e, command: rewrite.value.command }) : next(e), rewrite.kind === 'some' ? _noticed($, rewrite.value.notice) : undefined]);
        if (answer.deny !== undefined) {
            await _denied($, observation, e, answer.deny, next.trace);
            return answer;
        }
        const failed = answer.isError === true;
        const [edited] = await Promise.all([failed ? [] : _edited($, e, loop), failed || answer.text === undefined ? undefined : _captured($, e, answer.text), failed ? undefined : _planned($, e)]);
        const context = [...(await _once($, loop, rewrite.kind === 'some' ? [rewrite.value.context] : [])), ...edited];
        return context.length === 0 ? answer : ({ ...answer, context: [...(answer.context ?? []), ...context] } satisfies ToolCallResult);
    }).catch((_$, e, next) => (next.called ? next(e) : { deny: 'function-hooks policy did not run' }));

    on('turn.complete', async ($, e, next) => {
        const loop = e.agentId ?? _MAIN;
        const edited = memberOf(_EDITED, { requestId: loop });
        const said = memberOf(_SAID, { requestId: loop });
        const logged = memberOf(_SAID, { requestId: _LOG });
        const [files, notes, faults] = await Promise.all([read($, edited), read($, said), loop === _MAIN ? read($, logged) : [], observation.kind === 'some' ? _record($, 'turn.complete', e, TURN) : undefined]);
        const [result] = await Promise.all([next(e), files.length === 0 ? undefined : update($, edited, () => []), notes.length === 0 ? undefined : update($, said, () => []), faults.length === 0 ? undefined : update($, logged, () => [])]);
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
        const [below, rows, capture] = await Promise.all([next(e), read($, memberOf(_DIAGNOSTICS, e)), read($, memberOf(_CAPTURE, e))]);
        if (capture.kind === 'some') {
            return e.surface === 'terminal' && e.viewport !== undefined ? captureRow($.ui.resolve(e), below, capture.value, e.viewport.columns) : resultRow($.ui.resolve(e), below, caption(capture.value.path, capture.value.record));
        }
        return rows.length === 0 ? below : resultRow($.ui.resolve(e), below, summary(rows));
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
