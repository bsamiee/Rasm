import type { ClassicHookInputs, EngineInterface, Frozen, Next, PluginOptions, ProcessRunInit, Register, ToolCallInput, ToolCallResult } from 'claude-code';
import { atom, memberOf, read, update } from 'claude-code';
import { all, bind, decoded, type Fault, fault, fromUndefined, map, none, type Option, ok, type Result, rendered, some } from '../composition.ts';
import { planned, pointer, tasked } from '../context/plan.ts';
import { type Operation, type Ran, scanned, target, targets, written } from '../edits/targets.ts';
import { type Boundary, delivered, outcome, request, type Spawn } from '../observation/delivery.ts';
import { CALL, CLASSIC, type Columns, type Event, type Payload, row, TURN, USAGE } from '../observation/row.ts';
import { BOUNDARY, bound, INSERT, JUDGE, OPEN, OPENER, REPORT, sqlite } from '../observation/sql.ts';
import { scan } from '../policies/command.ts';
import type { Invocation } from '../policies/invocation.ts';
import { commandDecision, type Decision, type Host, pathRefusal, worktreeRefusal } from '../policies/policies.ts';
import { AGENTS, down, JOBS, kickstart, services, UID } from '../ui/health.ts';
import { band, bandRows } from '../ui/render.tsx';
import type { Notice, Queue, Row } from './state.d.ts';

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

// --- [STATE] ---------------------------------------------------------------------------

const _DATABASE = atom({ plugin: 'function-hooks', key: 'database' } as const, none);
const _ROWS = atom({ plugin: 'function-hooks', key: 'rows' } as const, []);
const _NOTICE = atom({ plugin: 'function-hooks', key: 'notice' } as const, none);
const _DOWN = atom({ plugin: 'function-hooks', key: 'down' } as const, []);
const _EDITS = atom({ plugin: 'function-hooks', key: 'edits' } as const, { format: [], diagnostics: [] });
const _PLAN = atom({ plugin: 'function-hooks', key: 'plan' } as const, { path: none, taskFile: none });
const _SAID = atom({ plugin: 'function-hooks', key: 'said' } as const, []);
const _LOGGED = atom({ plugin: 'function-hooks', key: 'logged' } as const, []);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [HOST]

const _fresh = (held: readonly string[], texts: readonly string[]): readonly string[] => [...new Set(texts)].filter((text) => !held.includes(text));

const _once = async ($: EngineInterface, loop: string, texts: readonly string[]): Promise<readonly string[]> => {
    if (texts.length === 0) {
        return [];
    }
    let fresh: readonly string[] = [];
    await update($, memberOf(_SAID, { requestId: loop }), (held) => {
        fresh = _fresh(held, texts);
        return [...held, ...fresh];
    });
    return fresh;
};

const _result = <T>(pending: Promise<T>, kind: 'unstarted' | 'unread' | 'unwritten', subject: string): Promise<Result<T>> => pending.then(ok, (cause: unknown) => fault<T>({ kind, subject, cause }));

const _faulted = async ($: EngineInterface, line: string, faults: readonly Fault[]): Promise<void> => {
    let fresh: readonly string[] = [];
    await update($, _LOGGED, (held) => {
        fresh = _fresh(held, [`${line}, ${rendered(faults)}`]);
        return [...held, ...fresh];
    });
    fresh.forEach((text) => {
        $.ui.log(text);
    });
};

const _loaded = <T>(held: Map<string, Promise<T>>, key: string, load: () => Promise<T>): Promise<T> => {
    const loading = held.get(key) ?? load();
    held.set(key, loading);
    return loading;
};

const _exited = async ($: EngineInterface, launcher: readonly string[], argv: Invocation, init: ProcessRunInit, exits: readonly number[]): Promise<Result<string>> =>
    bind(await _result($.process.run([...launcher, ...argv], init), 'unstarted', argv[0]), ({ exitCode, stdout, stderr }) => (exits.includes(exitCode) ? ok(stdout) : fault<string>({ kind: 'exited', subject: argv[0], code: exitCode, stderr: stderr.trim() })));

const _launcher = (
    (held: Map<string, Promise<Result<readonly string[]>>>) =>
    ($: EngineInterface, program: string): Promise<Result<readonly string[]>> =>
        _loaded(held, program, async () => {
            const exec: Invocation = ['mise', 'exec', '--'];
            const cwd = await $.session.root();
            const paths = await Promise.all([exec, ['mise', '--no-config', 'exec', '--']].map((launcher) => _exited($, launcher, ['which', program], { cwd }, [0, 1])));
            return map(all(paths), ([configured, unconfigured]) => (configured === unconfigured ? [] : exec));
        })
)(new Map());

const _run = async ($: EngineInterface, argv: Invocation, init: ProcessRunInit, exits: readonly number[]): Promise<Result<string>> => bind(await _launcher($, argv[0]), (launcher) => _exited($, launcher, argv, init, exits));

const _streamed = async ($: EngineInterface, argv: Invocation, cwd: string): Promise<Result<Ran>> =>
    bind(await _launcher($, argv[0]), async (launcher): Promise<Result<Ran>> => {
        const stream = $.process.spawn({ argv: [...launcher, ...argv], cwd });
        const pieces = { stdout: '', stderr: '' };
        for await (const { stream: pipe, text } of stream) {
            pieces[pipe] += text;
        }
        const { code, signal } = await stream.result;
        return code === null ? fault({ kind: 'unstarted', subject: argv[0], cause: signal }) : ok({ code, ...pieces });
    });

const _real = ($: EngineInterface, path: string): Promise<Option<string>> =>
    $.fs.stat(path, { resolve: true }).then(
        ({ realPath }) => fromUndefined(realPath),
        () => none,
    );

const _scanner = (
    (held: Map<string, Promise<Result<Invocation>>>) =>
    ($: EngineInterface): Promise<Result<Invocation>> => {
        const folder = `${$.plugin.root}/policies`;
        const config = `${folder}/scan.yml`;
        return _loaded(held, config, async () =>
            bind(decoded<{ readonly ruleDirs: readonly string[] }>(config, await _result($.fs.read(config), 'unread', config)), async ({ ruleDirs }) =>
                map(await _result(Promise.all(ruleDirs.map(async (dir) => Promise.all((await $.fs.list(`${folder}/${dir}`)).map(({ name }) => $.fs.read(`${folder}/${dir}/${name}`))))), 'unread', folder), (rules): Invocation => scan(rules.flat())),
            ),
        );
    }
)(new Map());

const _host = ($: EngineInterface): Host => ({
    scan: async (text) => bind(await _scanner($), async (argv) => _run($, argv, { stdin: text, cwd: await $.session.root() }, [0])),
    root: () => $.session.repo().then((found) => (found === null ? none : some(found.root))),
    exists: (path) => $.fs.exists(path),
    cwd: () => $.session.cwd(),
    home: () => $.env.get('HOME').then(fromUndefined),
});

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

const _health = async ($: EngineInterface): Promise<void> => {
    const repo = await $.session.repo();
    if (repo === null) {
        return;
    }
    const checked = await bind(services(await _run($, AGENTS, { cwd: repo.root }, [0])), async (known) => (known.length === 0 ? ok([]) : map(await _run($, JOBS, {}, [0]), (jobs) => down(known, jobs))));
    if (checked.kind === 'fault') {
        await _faulted($, 'services not checked', checked.faults);
        return;
    }
    const held = await read($, _DOWN);
    await (checked.value.length === held.length && checked.value.every(({ label }) => held.some((service) => service.label === label)) ? undefined : update($, _DOWN, () => checked.value));
};

const _restart = async ($: EngineInterface): Promise<void> => {
    const [held, uid] = await Promise.all([read($, _DOWN), _run($, UID, {}, [0])]);
    if (uid.kind === 'fault') {
        await _faulted($, 'services not restarted', uid.faults);
        return;
    }
    const ran = await Promise.all(held.map(async (service) => ({ service, result: await _run($, kickstart(service, uid.value), {}, [0]) })));
    const restarted = new Set(ran.flatMap(({ service, result }) => (result.kind === 'ok' ? [service.label] : [])));
    const failed = ran.flatMap(({ result }) => (result.kind === 'fault' ? result.faults : []));
    await (failed.length === 0 ? undefined : _faulted($, 'services not restarted', failed));
    await (restarted.size === 0 ? undefined : Promise.all([_noticed($, `${held.flatMap(({ name, label }) => (restarted.has(label) ? [name] : [])).join(' and ')} restarted · reconnect with /mcp`), update($, _DOWN, (current) => current.filter(({ label }) => !restarted.has(label)))]));
};

// --- [TOOL_CALL]

const _decision = ($: EngineInterface, e: ToolCallInput, project: string): Promise<Decision> => {
    if ((e.tool === 'Bash' || e.tool === 'Monitor') && e.command !== undefined) {
        return commandDecision(_host($), e.tool, e.command, e.tool_use_id, project);
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

const _queued = async ($: EngineInterface, loop: string, operations: readonly Operation[], paths: readonly string[]): Promise<void> => {
    const merge = (held: Queue): Queue => ({ ...held, ...Object.fromEntries(operations.map((operation) => [operation, [...new Set([...held[operation], ...paths])]])) });
    await (paths.length === 0 ? undefined : update($, memberOf(_EDITS, { requestId: loop }), merge));
};

const _processed = async ($: EngineInterface, loop: string, operation: Operation, project: string): Promise<readonly string[]> => {
    if ((await read($, memberOf(_EDITS, { requestId: loop })))[operation].length === 0) {
        return [];
    }
    let drained: readonly string[] = [];
    await update($, memberOf(_EDITS, { requestId: loop }), (held) => {
        drained = held[operation];
        return { ...held, [operation]: [] };
    });
    const root = await $.session.root();
    const existing = (await Promise.all(drained.filter((path) => path.startsWith(`${root}/`)).map(async (path) => ((await $.fs.exists(path)) ? [path] : [])))).flat();
    const ignored = await _run($, ['git', 'check-ignore', '--stdin'], { cwd: root, stdin: existing.join('\n') }, [0, 1]);
    if (ignored.kind === 'fault') {
        await _faulted($, `${operation} not run`, ignored.faults);
        return [];
    }
    const paths = existing.filter((path) => !ignored.value.split('\n').includes(path));
    if (paths.length === 0) {
        return [];
    }
    const named = target(operation, project);
    const ran = await _streamed($, targets(operation, project, paths), root);
    const reported = operation === 'format' ? map(ran, (value) => written(named, value)) : bind(ran, (value) => scanned(named, value));
    if (reported.kind === 'fault') {
        await _faulted($, `${operation} not run`, reported.faults);
        return [];
    }
    await (operation === 'format' ? _queued($, loop, ['diagnostics'], paths) : undefined);
    return reported.value;
};

const _tasked = async ($: EngineInterface, e: ToolCallInput): Promise<void> => {
    if (e.agentId !== undefined || e.tool !== 'Write') {
        return;
    }
    const [file, temp, plan] = await Promise.all([_real($, e.file_path), _real($, '/tmp'), read($, _PLAN)]);
    const moved = file.kind === 'some' && temp.kind === 'some' ? tasked(plan, file.value, temp.value) : none;
    await (moved.kind === 'some' ? update($, _PLAN, () => moved.value) : undefined);
};

// --- [RECORDING]

const _opened = async ($: EngineInterface): Promise<Option<string>> => {
    const repo = await $.session.repo();
    if (repo === null) {
        return none;
    }
    const applied = await _run($, OPENER, { stdin: OPEN, cwd: repo.root }, [0]);
    if (applied.kind === 'fault') {
        await _faulted($, 'rows not recorded', applied.faults);
        return none;
    }
    return some(repo.root);
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
    await update($, _ROWS, (held) => [...held, row(event, payload, columns, session, ts)]);
    return some({ root: root.value, ts });
};

const _flushed = async ($: EngineInterface): Promise<void> => {
    const [held, root] = await Promise.all([read($, _ROWS), read($, _DATABASE)]);
    if (held.length === 0 || root.kind === 'none' || root.value.kind === 'none') {
        return;
    }
    let rows: readonly Row[] = [];
    await update($, _ROWS, (current) => {
        rows = current;
        return [];
    });
    const inserted = await _run($, sqlite(root.value.value), { stdin: bound({ rows }, INSERT), cwd: root.value.value }, [0]);
    await (inserted.kind === 'ok' ? undefined : _faulted($, 'observation rows not written', inserted.faults));
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
    const [stored] = await Promise.all([agentId === undefined ? undefined : _run($, sqlite(bindings.main), { stdin: bound({ ...spawn, ...bindings, id: agentId }, spawn.kind === 'range' ? JUDGE : REPORT), cwd: bindings.worktree }, [0]), _noticed($, outcome(spawn, answer.value))]);
    await (stored?.kind === 'fault' ? _faulted($, 'observation row not written', stored.faults) : undefined);
};

const _boundary = async ($: EngineInterface, options: PluginOptions, session: string, { root: main, ts }: Stamped): Promise<readonly string[]> => {
    const worktree = await $.session.root();
    const decided = await bind(await _run($, ['git', 'branch', '--show-current'], { cwd: worktree }, [0]), async (name) => {
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

const _stopped = async <E extends Frozen<ClassicHookInputs['Stop' | 'SubagentStop']>, R extends { readonly additionalContext?: readonly string[] }>($: EngineInterface, e: E, next: (input: E) => Promise<R>, observation: Option<PluginOptions>, project: string): Promise<R> => {
    const loop = e.hook_event_name === 'SubagentStop' ? e.agent_id : e.session_id;
    const stamped = observation.kind === 'some' ? await _record($, e.hook_event_name, e, CLASSIC) : none;
    await (observation.kind === 'some' ? _flushed($) : undefined);
    const result = await next(e);
    const boundary = observation.kind === 'some' && stamped.kind === 'some' && e.hook_event_name === 'Stop' ? _boundary($, observation.value, e.session_id, stamped.value) : [];
    const formatted = await _processed($, loop, 'format', project);
    const checked = await _processed($, loop, 'diagnostics', project);
    const context = e.stop_hook_active ? [] : await _once($, loop, [...(await boundary), ...formatted, ...checked]);
    return context.length === 0 ? result : { ...result, additionalContext: [...(result.additionalContext ?? []), ...context] };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const register: Register = (on, options) => {
    const project = String(options.project);
    const observation = options.observation === true ? some(options) : none;

    on('session.start', async ($, e, next) => {
        await Promise.all([_health($), observation.kind === 'some' ? _database($, none) : undefined]);
        return next(e);
    });

    on('tool.call', async ($, e, next) => {
        const decision = await _decision($, e, project);
        if (decision.kind === 'deny') {
            await _denied($, observation, e, decision.reason, next.trace);
            return { deny: decision.reason };
        }
        const { rewrite } = decision;
        const loop = e.agentId ?? (await $.session.id());
        const [{ answer, paths }] = await Promise.all([_called((e.tool === 'Bash' || e.tool === 'Monitor') && rewrite.kind === 'some' ? { ...e, command: rewrite.value.command } : e, next), rewrite.kind === 'some' ? _noticed($, rewrite.value.notice) : undefined]);
        if (answer.deny !== undefined) {
            await _denied($, observation, e, answer.deny, next.trace);
            return answer;
        }
        const failed = answer.isError === true;
        await Promise.all([_queued($, loop, ['format', 'diagnostics'], paths), failed ? undefined : _tasked($, e), failed && String(e.tool).startsWith('mcp__') ? _health($) : undefined]);
        const context = await _once($, loop, [...(rewrite.kind === 'some' ? [rewrite.value.context] : []), ...(await _processed($, loop, 'diagnostics', project))]);
        return context.length === 0 ? answer : ({ ...answer, context: [...(answer.context ?? []), ...context] } satisfies ToolCallResult);
    }).catch((_$, e, next) => (next.called ? next(e) : { deny: 'function-hooks policy did not run' }));

    on('turn.complete', async ($, e, next) => {
        const loop = e.agentId ?? (await $.session.id());
        const [notes, faults] = await Promise.all([read($, memberOf(_SAID, { requestId: loop })), e.agentId === undefined ? read($, _LOGGED) : [], observation.kind === 'some' ? _record($, 'turn.complete', e, TURN).then(() => _flushed($)) : undefined]);
        const [result] = await Promise.all([next(e), notes.length === 0 ? undefined : update($, memberOf(_SAID, { requestId: loop }), () => []), faults.length === 0 ? undefined : update($, _LOGGED, () => [])]);
        return result;
    });

    on('classic.Stop', ($, e, next) => _stopped($, e, next, observation, project));

    on('classic.SubagentStop', ($, e, next) => _stopped($, e, next, observation, project));

    on('prompt.submit', async ($, e, next) => {
        await _cleared($, none);
        return next(e);
    });

    on('prompt.attachment', { type: ['plan_mode', 'plan_mode_reentry', 'plan_mode_exit'] }, async ($, e, next) => {
        const { detail } = e;
        const moved = e.agentId === undefined && detail !== undefined && (!('hasPlan' in detail) || detail.hasPlan) ? planned(await read($, _PLAN), detail.planFilePath) : none;
        await (moved.kind === 'some' ? update($, _PLAN, () => moved.value) : undefined);
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

    if (observation.kind === 'some') {
        on('classic.*', { hook_event_name: ['SessionStart', 'PermissionDenied', 'PostToolUse', 'PostToolUseFailure', 'PostToolBatch', 'SubagentStart', 'UserPromptSubmit', 'StopFailure', 'PreCompact', 'PostCompact', 'SessionEnd', 'WorktreeCreate', 'WorktreeRemove'] }, async ($, e, next) => {
            await (next.is('!classic.PreToolUse', e) ? _record($, e.hook_event_name, e, CLASSIC).then(() => (e.hook_event_name === 'SessionEnd' ? _flushed($) : undefined)) : undefined);
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
