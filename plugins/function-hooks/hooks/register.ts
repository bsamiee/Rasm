import type { ClassicHookInputs, EngineInterface, Frozen, PluginOptions, ProcessRunInit, Register, ToolCallInput, ToolCallResult } from 'claude-code';
import { atom, memberOf, read, update } from 'claude-code';
import { all, bind, both, decoded, type Fault, fault, fromUndefined, map, none, type Option, ok, type Result, rendered, some } from '../composition.ts';
import { fromAstGrep, fromBinlog, fromCodelens, lines, summary, trimmed } from '../context/diagnostics.ts';
import { pointer, touched } from '../context/plan.ts';
import { type Boundary, delivered, outcome, request, type Spawn } from '../observation/delivery.ts';
import { CALL, CLASSIC, type Columns, type Event, type Payload, row, TURN, USAGE } from '../observation/row.ts';
import { BOUNDARY, bound, DELTA, INSERT, JUDGE, OPEN, REPORT, sqlite } from '../observation/sql.ts';
import { SCAN } from '../policies/command.ts';
import type { Invocation } from '../policies/invocation.ts';
import { commandDecision, type Decision, type Host, pathRefusal, worktreeRefusal } from '../policies/policies.ts';
import { reformatted } from '../repository/format.ts';
import { caption, capturePath, recorded } from '../ui/capture.ts';
import { down, kickstart, LAUNCHD_AGENTS, LISTENERS, services, UID } from '../ui/health.ts';
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

// --- [STATE] ---------------------------------------------------------------------------

const _DATABASE = atom({ plugin: 'function-hooks', key: 'database' } as const, none);
const _NOTICE = atom({ plugin: 'function-hooks', key: 'notice' } as const, none);
const _DOWN = atom({ plugin: 'function-hooks', key: 'down' } as const, []);
const _EDITED = atom({ plugin: 'function-hooks', key: 'edited' } as const, []);
const _DIAGNOSTICS = atom({ plugin: 'function-hooks', key: 'diagnostics' } as const, []);
const _CAPTURE = atom({ plugin: 'function-hooks', key: 'capture' } as const, none);
const _PLAN = atom({ plugin: 'function-hooks', key: 'plan' } as const, { path: none, taskFile: none });

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [HOST]

const _result = <T>(pending: Promise<T>, kind: 'unstarted' | 'unread' | 'unwritten', subject: string): Promise<Result<T>> => pending.then(ok, (cause: unknown) => fault<T>({ kind, subject, cause }));

const _faulted = ($: EngineInterface, line: string, faults: readonly Fault[]): readonly never[] => {
    $.ui.log(`${line}, ${rendered(faults)}`);
    return [];
};

const _logged = <T>($: EngineInterface, line: string, result: Result<readonly T[]>): readonly T[] => (result.kind === 'ok' ? result.value : _faulted($, line, result.faults));

const _run = async ($: EngineInterface, argv: Invocation, init: ProcessRunInit, exits: readonly number[]): Promise<Result<string>> =>
    bind(await _result($.process.run(argv, init), 'unstarted', argv[0]), ({ exitCode, stdout, stderr }) => (exits.includes(exitCode) ? ok(stdout) : fault<string>({ kind: 'exited', subject: argv[0], code: exitCode, stderr: stderr.trim() })));

const _read = ($: EngineInterface, path: string): Promise<Result<string>> => _result($.fs.read(path), 'unread', path);

const _mcp = async ($: EngineInterface, server: string, tool: string, args: Record<string, unknown>): Promise<Result<string>> => {
    const subject = `${server} ${tool}`;
    return bind(await _result($.mcp.call(server, tool, args), 'unstarted', subject), ({ content, isError }) => {
        const text = content.flatMap((block) => (block.text === undefined ? [] : [block.text])).join('');
        return isError ? fault<string>({ kind: 'refused', subject, text }) : ok(text);
    });
};

const _host = ($: EngineInterface): Host => ({
    scan: (text) => _run($, SCAN, { stdin: text }, [0]),
    read: (path) => _read($, path),
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
    if (listened.kind === 'fault') {
        _faulted($, 'services not read', listened.faults);
        return;
    }
    const found = down(known, listened.value);
    await (found.length === held.length && found.every(({ name }) => held.some((service) => service.name === name)) ? undefined : update($, _DOWN, () => found));
};

const _watched = async ($: EngineInterface): Promise<void> => {
    const healthMs = 60_000;
    const repo = await $.session.repo();
    const known = repo === null ? [] : _logged($, 'launchd agents not read', services(await _run($, LAUNCHD_AGENTS, { cwd: repo.root }, [0])));
    if (known.length > 0) {
        $.clock.every(healthMs, () => _health($, known));
        await _health($, known);
    }
};

const _restart = async ($: EngineInterface): Promise<void> => {
    const [held, uid] = await Promise.all([read($, _DOWN), _run($, UID, {}, [0])]);
    const ran = await Promise.all(held.map(async ({ name }) => map(await bind(uid, (id) => _run($, kickstart(name, id), {}, [0])), () => name)));
    const restarted = ran.flatMap((result) => (result.kind === 'ok' ? [result.value] : []));
    _logged(
        $,
        'services not restarted',
        bind(uid, () => all(ran)),
    );
    await (restarted.length === 0 ? undefined : Promise.all([_noticed($, `${restarted.join(' and ')} restarted · reconnect with /mcp`), update($, _DOWN, (current) => current.filter(({ name }) => !restarted.includes(name)))]));
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

const _project = async ($: EngineInterface, root: string, folder: string): Promise<Result<Option<string>>> =>
    folder.startsWith(root)
        ? bind(await _result($.fs.list(folder), 'unread', folder), (entries) => {
              const found = entries.find((entry) => entry.kind === 'file' && entry.name.endsWith('.csproj'));
              return found === undefined ? _project($, root, folder.slice(0, folder.lastIndexOf('/'))) : Promise.resolve(ok(some(found.name.slice(0, -'.csproj'.length))));
          })
        : ok(none);

const _codelens = async ($: EngineInterface, root: string, file: string): Promise<Result<readonly Diagnostic[]>> =>
    bind(await _project($, root, file.slice(0, file.lastIndexOf('/'))), async (project) => (project.kind === 'none' ? ok([]) : bind(await _mcp($, 'roslyn-codelens', 'get_diagnostics', { project: project.value, includeAnalyzers: true, severity: 'warning' }), (json) => fromCodelens(json, file))));

const _edited = async ($: EngineInterface, e: ToolCallInput, loop: string): Promise<readonly string[]> => {
    if (e.tool !== 'Edit' && e.tool !== 'Write' && e.tool !== 'NotebookEdit') {
        return [];
    }
    const path = e.tool === 'NotebookEdit' ? e.notebook_path : e.file_path;
    const scannable = e.tool !== 'NotebookEdit';
    const checked = await bind(await _toplevel($), async (root) => {
        const [codelens, scanned] = path.startsWith(`${root}/`)
            ? await Promise.all([
                  scannable && path.endsWith('.cs') ? _codelens($, root, path) : ok([]),
                  scannable ? _run($, ['ast-grep', 'scan', '--json=compact', path], { cwd: root }, [0, 1]).then((printed) => bind(printed, fromAstGrep)) : ok([]),
                  update($, memberOf(_EDITED, { requestId: loop }), (held) => (held.includes(path) ? held : [...held, path])),
              ])
            : [ok([]), ok([])];
        return ok([..._logged($, 'roslyn-codelens diagnostics not read', codelens), ..._logged($, 'ast-grep diagnostics not read', scanned)]);
    });
    const rows = _logged($, 'edit not checked', checked);
    await (rows.length === 0 ? undefined : update($, memberOf(_DIAGNOSTICS, { requestId: e.tool_use_id }), () => rows));
    return lines(rows);
};

const _built = async ($: EngineInterface, binlogs: readonly string[]): Promise<readonly string[]> => {
    const errors = await Promise.all(binlogs.map(async (path) => ((await $.fs.exists(path)) ? [await _mcp($, 'binlog', 'binlog_errors', { binlog_file: path })] : [])));
    return lines(_logged($, 'binlog not read', all(errors.flat())).flatMap(fromBinlog));
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
    await (capture.kind === 'ok' ? update($, memberOf(_CAPTURE, { requestId: e.tool_use_id }), () => some(capture.value)) : _faulted($, 'capture not read', capture.faults));
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
        _faulted($, 'rows not recorded', applied.faults);
        return none;
    }
    const mode = map(await _run($, sqlite(root), { stdin: 'pragma journal_mode=wal;', cwd: root }, [0]), (text) => text.trim());
    if (mode.kind === 'fault') {
        _faulted($, 'journal mode not switched', mode.faults);
    } else if (mode.value !== 'wal') {
        $.ui.log(`journal mode not switched, sqlite3 kept ${mode.value}`);
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
    const built = row(event, payload, columns, session, ts);
    const inserted = await _run($, sqlite(root.value), { stdin: bound(built, INSERT), cwd: root.value }, [0]);
    if (inserted.kind === 'fault') {
        _faulted($, `${built.event} row ${built.tool_use_id ?? built.session_id} not written`, inserted.faults);
    }
    return some({ root: root.value, ts });
};

const _denied = ($: EngineInterface, observation: Option<PluginOptions>, e: ToolCallInput, reason: string, trace: unknown): Promise<Option<Stamped>> => (observation.kind === 'some' ? _record($, 'tool.call', { ...e, deny: reason, trace }, CALL) : Promise.resolve(none));

const _spawn = async ($: EngineInterface, spawn: Spawn, bindings: Bindings): Promise<void> => {
    const answer = await _result($.agent.spawn(request(spawn, bindings.key, bindings.worktree)), 'unstarted', spawn.agent);
    if (answer.kind === 'fault') {
        _faulted($, 'agent not spawned', answer.faults);
        return;
    }
    if (answer.value.deny !== undefined) {
        $.ui.log(outcome(spawn, answer.value));
        return;
    }
    const { agentId } = answer.value;
    const [written] = await Promise.all([agentId === undefined ? undefined : _run($, sqlite(bindings.main), { stdin: bound({ ...spawn, ...bindings, id: agentId }, spawn.kind === 'range' ? JUDGE : REPORT), cwd: bindings.worktree }, [0]), _noticed($, outcome(spawn, answer.value))]);
    if (written?.kind === 'fault') {
        _faulted($, `${spawn.kind === 'range' ? 'judged_range row' : 'report rows'} not written`, written.faults);
    }
};

const _boundary = async ($: EngineInterface, options: PluginOptions, session: string, { root: main, ts }: Stamped): Promise<readonly string[]> => {
    const [toplevel, branch] = await Promise.all([_toplevel($), $.session.root().then((cwd) => _run($, ['git', 'branch', '--show-current'], { cwd }, [0]))]);
    const decided = await bind(both(toplevel, branch), async ([worktree, name]) => {
        const lineage = { main, worktree, branch: name.trim() };
        return map(decoded<Boundary>('sqlite3', await _run($, sqlite(main), { stdin: bound({ ...options, ...lineage, to: ts, session }, BOUNDARY), cwd: worktree }, [0])), (boundary) => ({ ...boundary, bindings: { ...lineage, key: boundary.key, session, at: ts } }));
    });
    if (decided.kind === 'fault') {
        return _faulted($, 'boundary skipped', decided.faults);
    }
    const { bindings, spawns, findings } = decided.value;
    await Promise.all(spawns.map((spawn) => _spawn($, spawn, bindings)));
    return delivered(findings, bindings.branch);
};

// --- [WRITERS]

const _formatted = async ($: EngineInterface, loop: string): Promise<readonly string[]> => {
    const [files, toplevel] = await Promise.all([read($, memberOf(_EDITED, { requestId: loop })), _toplevel($)]);
    const formatted = await bind(toplevel, async (root) => {
        const { context, failed } = await reformatted(
            {
                exec: (argv, exits) => _run($, argv, { cwd: root, timeoutMs: 120_000 }, exits),
                read: (path) => _read($, path),
                mtime: (path) =>
                    $.fs.stat(path).then(
                        ({ mtimeMs }) => some(mtimeMs),
                        () => none,
                    ),
            },
            root,
            files,
        );
        if (failed.length > 0) {
            _faulted($, 'writers failed', failed);
        }
        return ok(context.kind === 'some' ? [context.value] : []);
    });
    return _logged($, 'writers not run', formatted);
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
        const [answer] = await Promise.all([(e.tool === 'Bash' || e.tool === 'Monitor') && rewrite.kind === 'some' ? next({ ...e, command: rewrite.value.command }) : next(e), rewrite.kind === 'some' ? _noticed($, rewrite.value.notice) : undefined]);
        if (answer.deny !== undefined) {
            await _denied($, observation, e, answer.deny, next.trace);
            return answer;
        }
        const failed = answer.isError === true;
        const [edited, built] = await Promise.all([failed ? [] : _edited($, e, e.agentId ?? _MAIN), failed && rewrite.kind === 'some' ? _built($, rewrite.value.binlogs) : [], failed || answer.text === undefined ? undefined : _captured($, e, answer.text), failed ? undefined : _planned($, e)]);
        const context = [...(rewrite.kind === 'some' ? [rewrite.value.context] : []), ...edited, ...built];
        return context.length === 0 ? answer : ({ ...answer, context: [...(answer.context ?? []), ...context] } satisfies ToolCallResult);
    }).catch((_$, e, next) => (next.called ? next(e) : { deny: 'function-hooks policy did not run' }));

    on('turn.complete', async ($, e, next) => {
        const edited = memberOf(_EDITED, { requestId: e.agentId ?? _MAIN });
        const [held] = await Promise.all([read($, edited), observation.kind === 'some' ? _record($, 'turn.complete', e, TURN) : undefined]);
        const [result] = await Promise.all([next(e), held.length === 0 ? undefined : update($, edited, () => [])]);
        return result;
    });

    on('classic.Stop', ($, e, next) => _stopped($, e, next, observation));

    on('classic.SubagentStop', ($, e, next) => _stopped($, e, next, observation));

    on('prompt.submit', async ($, e, next) => {
        await _cleared($, none);
        return next(e);
    });

    on('prompt.attachment', { type: 'diagnostics' }, async ($, e, next) => {
        const [given, edited] = await Promise.all([next(e), read($, memberOf(_EDITED, { requestId: e.agentId ?? _MAIN }))]);
        return given.text === null ? given : trimmed(given.text, edited);
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
