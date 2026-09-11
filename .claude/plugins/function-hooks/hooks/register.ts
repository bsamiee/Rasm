// --- [IMPORTS] -------------------------------------------------------------------------

import type {
    ClassicHookEvent,
    ClassicHookInputs,
    ClassicResult,
    EngineInterface,
    Frozen,
    Next,
    ProcessRunInit,
    Register,
    ToolCallInput,
    ToolCallResult,
} from 'claude-code';
import { fault, ok, type Result } from './composition/result.ts';
import { decide } from './events/tool-call.ts';
import {
    awaiting,
    type Candidate,
    categoryPrompt,
    context,
    DELIVER,
    due,
    dueCategories,
    type Lineage,
    lineageOf,
    listed,
    type Range,
    REPORT,
    REVOKE,
    rangePrompt,
    resolved,
    type Settings,
    STATE,
    type State,
    settings,
    state,
    status,
    unbuilt,
} from './observation/delivery.ts';
import { CALL, type Cell, CLASSIC, type Columns, type Event, type Row, row, session, TURN } from './observation/row.ts';
import { type Argv, database, keep, LOCATE, open, script, sqlite3 } from './observation/sql.ts';
import { SCAN } from './text/command.ts';
import { basename } from './text/path.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Sink {
    readonly argv: Argv;
    readonly root: string;
}

type Classic = Frozen<ClassicHookInputs[Exclude<ClassicHookEvent, 'PreToolUse'>]>;

type Boundary = Frozen<ClassicHookInputs['Stop']> | Frozen<ClassicHookInputs['SubagentStop']>;

// One text held across calls, `set` answers whether it changed
interface Memo {
    readonly text: () => string;
    readonly set: (text: string) => boolean;
}

// Held from register until reload: settings, claimed spawns, the footer text, the last log line
interface Environment {
    readonly chosen: Settings;
    readonly claims: Set<string>;
    readonly footer: Memo;
    readonly logged: Memo;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _CTRL = /\p{Cc}+/gu;
const _WORKTREE: Argv = ['git', 'rev-parse', '--show-toplevel'];
const _BRANCH: Argv = ['git', 'branch', '--show-current'];
const _NONE: readonly string[] = [];

// --- [MEMO] ----------------------------------------------------------------------------

const _memo = (): Memo => {
    let last = '';
    return {
        text: () => last,
        set: (text: string): boolean => {
            const changed = text !== last;
            last = text;
            return changed;
        },
    };
};

// --- [PROCESS] -------------------------------------------------------------------------

const _run = ($: EngineInterface, argv: Argv, init?: ProcessRunInit): Promise<Result<string>> =>
    $.process.run(argv, init).then(
        ({ exitCode, stdout, stderr }) => (exitCode === 0 ? ok(stdout) : fault(`${basename(argv[0])} exited ${exitCode}, ${stderr.trim()}`)),
        (cause: unknown) => fault(`${basename(argv[0])} did not run, ${String(cause)}`),
    );

// --- [OPEN] ----------------------------------------------------------------------------

// WAL switch runs as its own process after the schema committed, its exclusive lock waits on no busy handler
// Loss to a concurrent first open of a fresh sink is logged and leaves the sink open in its journal mode
const _switched = ($: EngineInterface, sqlite: Argv, root: string): Promise<Result<Sink>> =>
    _run($, sqlite, { stdin: 'pragma journal_mode=wal;' }).then((switched) => {
        if (switched.kind === 'fault') {
            $.ui.log(`journal mode not switched, ${switched.reason}`);
        }
        return ok({ argv: sqlite, root });
    });

const _applied = ($: EngineInterface, sqlite: Argv, root: string): Promise<Result<Sink>> =>
    _run($, sqlite, { stdin: open(root) }).then((applied) =>
        applied.kind === 'fault' ? fault(`schema not applied, ${applied.reason}`) : _switched($, sqlite, root),
    );

const _prepared = ($: EngineInterface, sqlite: Argv, root: string): Promise<Result<Sink>> =>
    $.fs.write(keep(root), '').then(
        () => _applied($, sqlite, root),
        (cause: unknown) => fault(`${keep(root)} not written, ${String(cause)}`),
    );

const _located = ($: EngineInterface, root: string): Promise<Result<Sink>> =>
    _run($, LOCATE).then((where) => (where.kind === 'fault' ? where : _prepared($, sqlite3(where.value.trim(), database(root)), root)));

const _open = ($: EngineInterface): Promise<Result<Sink>> =>
    $.session
        .repo()
        .then((repo) => (repo === null ? fault<Sink>('session runs outside a git repository') : _located($, repo.root)))
        .then((opened) => {
            if (opened.kind === 'fault') {
                $.ui.log(`rows are not recorded, ${opened.reason}`);
            }
            return opened;
        });

// --- [RECORD] --------------------------------------------------------------------------

const _write = ($: EngineInterface, sqlite: Argv, built: Row): Promise<void> =>
    _run($, sqlite, { stdin: script(built) }).then((ran) => {
        if (ran.kind === 'fault') {
            $.ui.log(`${built.event} row ${built.toolUseId.kind === 'text' ? built.toolUseId.text : built.sessionId} not written, ${ran.reason}`);
        }
    });

const record = (
    $: EngineInterface,
    sink: Sink,
    event: Event,
    value: Readonly<Record<string, unknown>>,
    columns: Columns,
    ts: number,
): Promise<void> => {
    const own: Cell = session(value, columns);
    return own.kind === 'text'
        ? _write($, sink.argv, row(event, value, columns, own.text, ts))
        : $.session.id().then((id) => _write($, sink.argv, row(event, value, columns, id, ts)));
};

const _classic = ($: EngineInterface, sink: Sink, e: Classic, ts: number): Promise<void> =>
    e.hook_event_name === 'Stop' || e.hook_event_name === 'SessionEnd'
        ? $.session.usage().then((usage) => record($, sink, e.hook_event_name, { ...e, usage }, CLASSIC, ts))
        : record($, sink, e.hook_event_name, e, CLASSIC, ts);

const _denied = (
    $: EngineInterface,
    sink: Sink,
    e: Frozen<ToolCallInput>,
    next: Next<'tool.call'>,
    answer: ToolCallResult,
): Promise<ToolCallResult> => record($, sink, 'tool.call', { ...e, deny: answer.deny, trace: next.trace }, CALL, $.clock.now()).then(() => answer);

// --- [DELIVERY] ------------------------------------------------------------------------

const _stopping = (e: Classic): e is Boundary =>
    (e.hook_event_name === 'Stop' && !e.stop_hook_active) || (e.hook_event_name === 'SubagentStop' && e.agent_type !== '' && !e.stop_hook_active);

const _spawn = ($: EngineInterface, agent: string, prompt: string, description: string, cwd: string, subject: string): Promise<boolean> =>
    $.agent.spawn({ prompt, subagentType: agent, description, cwd }).then(
        (result) => {
            $.ui.log(resolved(agent, subject, result));
            return result.deny === undefined;
        },
        (cause: unknown) => {
            $.ui.log(`${agent} did not spawn, ${String(cause)}`);
            return false;
        },
    );

const _skip = ($: EngineInterface, reason: string): readonly string[] => {
    $.ui.log(`boundary skipped, ${reason}`);
    return _NONE;
};

// Claim holds the agent definition from the spawn call until the spawn resolves and later boundary events list the subagent
const _judge = ($: EngineInterface, lineage: Lineage, to: number, range: Range, tasks: readonly string[], claims: Set<string>): void => {
    if (due(range, [...tasks, ...claims])) {
        claims.add(range.trigger.agent);
        _spawn(
            $,
            range.trigger.agent,
            rangePrompt(lineage, range.from, to),
            `judge ${range.trigger.kind}s`,
            lineage.worktree,
            `${range.from}..${to}`,
        ).then(() => claims.delete(range.trigger.agent));
    }
};

const _delivered = (
    $: EngineInterface,
    sink: Sink,
    e: Boundary,
    lineage: Lineage,
    found: number,
    now: number,
): readonly string[] | Promise<readonly string[]> =>
    e.hook_event_name === 'Stop' && found > 0
        ? _run($, sink.argv, { stdin: DELIVER(lineage, e.session_id, now), cwd: lineage.worktree }).then((rows) =>
              rows.kind === 'fault' ? _skip($, `delivery rows not written, ${rows.reason}`) : context(rows.value, lineage.branch),
          )
        : _NONE;

// Refused category spawns take their report rows back, so the category is due again at the next boundary event
const _revoked = ($: EngineInterface, sink: Sink, e: Boundary, lineage: Lineage, now: number, launched: boolean): void => {
    if (!launched) {
        _run($, sink.argv, { stdin: REVOKE(lineage, e.session_id, now), cwd: lineage.worktree }).then((taken) => {
            if (taken.kind === 'fault') {
                $.ui.log(`report rows not revoked, ${taken.reason}`);
            }
        });
    }
};

const _reported = (
    $: EngineInterface,
    sink: Sink,
    e: Boundary,
    lineage: Lineage,
    environment: Environment,
    now: number,
    candidate: Candidate,
    written: Result<string>,
): void => {
    if (written.kind === 'fault') {
        environment.claims.delete(environment.chosen.categoryAgent);
        $.ui.log(`report rows not written, ${written.reason}`);
        return;
    }
    _spawn(
        $,
        environment.chosen.categoryAgent,
        categoryPrompt(candidate.category, lineage),
        'judge category',
        lineage.worktree,
        candidate.category,
    ).then((launched) => {
        environment.claims.delete(environment.chosen.categoryAgent);
        _revoked($, sink, e, lineage, now, launched);
    });
};

const _handed = (
    $: EngineInterface,
    sink: Sink,
    e: Boundary,
    lineage: Lineage,
    environment: Environment,
    seen: State,
    now: number,
    candidate: Candidate | undefined,
): readonly string[] | Promise<readonly string[]> =>
    candidate === undefined
        ? _delivered($, sink, e, lineage, seen.undelivered, now)
        : _run($, sink.argv, { stdin: REPORT(lineage, e.session_id, candidate.category, now), cwd: lineage.worktree })
              .then((written) => _reported($, sink, e, lineage, environment, now, candidate, written))
              .then(() => _delivered($, sink, e, lineage, seen.undelivered, now));

const _shown = (
    $: EngineInterface,
    sink: Sink,
    e: Boundary,
    lineage: Lineage,
    to: number,
    environment: Environment,
    seen: State,
): readonly string[] | Promise<readonly string[]> => {
    if (environment.footer.set(status(seen))) {
        $.ui.invalidate('ui.render');
    }
    const tasks = listed(e.background_tasks);
    if (tasks.kind === 'fault') {
        return _skip($, tasks.reason);
    }
    _judge($, lineage, to, seen.edits, tasks.value, environment.claims);
    const waiting = awaiting(seen);
    if (
        e.hook_event_name === 'Stop' &&
        environment.chosen.categoryThreshold === 0 &&
        waiting.length > 0 &&
        environment.logged.set(unbuilt(waiting))
    ) {
        $.ui.log(environment.logged.text());
    }
    const [candidate] = dueCategories(seen, environment.chosen, [...tasks.value, ...environment.claims]);
    if (candidate !== undefined) {
        environment.claims.add(environment.chosen.categoryAgent);
    }
    return _handed($, sink, e, lineage, environment, seen, $.clock.now(), candidate);
};

const _counted = ($: EngineInterface, sink: Sink, e: Boundary, lineage: Lineage, to: number, environment: Environment): Promise<readonly string[]> =>
    _run($, sink.argv, { stdin: STATE(lineage, to, environment.chosen), cwd: lineage.worktree }).then((read) => {
        if (read.kind === 'fault') {
            return _skip($, `state not read, ${read.reason}`);
        }
        const seen = state(read.value, environment.chosen);
        return seen.kind === 'fault' ? _skip($, `state line not read, ${seen.reason}`) : _shown($, sink, e, lineage, to, environment, seen.value);
    });

// Branch empty on a detached head, so the lineage is the worktree's
const _branched = ($: EngineInterface, sink: Sink, e: Boundary, to: number, environment: Environment, worktree: string): Promise<readonly string[]> =>
    _run($, _BRANCH, { cwd: e.cwd }).then((branch) =>
        branch.kind === 'fault'
            ? _skip($, branch.reason)
            : _counted($, sink, e, lineageOf(sink.root, worktree, branch.value.trim().replace(_CTRL, ' ')), to, environment),
    );

const _boundary = ($: EngineInterface, sink: Sink, e: Boundary, to: number, environment: Environment): Promise<readonly string[]> =>
    _run($, _WORKTREE, { cwd: e.cwd }).then((worktree) =>
        worktree.kind === 'fault' ? _skip($, worktree.reason) : _branched($, sink, e, to, environment, worktree.value.trim()),
    );

const _observed = ($: EngineInterface, sink: Sink, e: Classic, environment: Environment, to: number): Promise<readonly string[]> =>
    _classic($, sink, e, to).then(() => {
        if (e.hook_event_name === 'SessionEnd' && environment.footer.set('')) {
            $.ui.invalidate('ui.render');
        }
        return _stopping(e) ? _boundary($, sink, e, to, environment) : _NONE;
    });

const _answered = (result: ClassicResult, entries: readonly string[]): ClassicResult =>
    entries.length === 0 ? result : { ...result, additionalContext: [...(result.additionalContext ?? _NONE), ...entries] };

// --- [REGISTRATION] --------------------------------------------------------------------

const register: Register = (on, options) => {
    const claims: Set<string> = new Set();
    // Footer empty at zero counts, after a reload, and after SessionEnd, written at boundary events alone
    const environment: Environment = { chosen: settings(options), claims, footer: _memo(), logged: _memo() };
    // Thunk keeps `$` inside the hook, since the validator follows `$` into top-level functions alone, and the memo out of a parameter
    let opening: Promise<Result<Sink>> | undefined;
    const once = (attempt: () => Promise<Result<Sink>>): Promise<Result<Sink>> => {
        opening ??= attempt();
        return opening;
    };

    on('tool.call', ($, e, next) =>
        decide(
            e,
            (text: string) => $.process.run(SCAN, { stdin: text }),
            (path: string) => $.fs.exists(path),
        )
            .then((decision) => (decision.kind === 'deny' ? { deny: decision.reason.replace(_CTRL, ' ') } : next(decision.e)))
            .then<ToolCallResult>((answer) =>
                answer.deny === undefined
                    ? answer
                    : once(() => _open($)).then((sink) => (sink.kind === 'ok' ? _denied($, sink.value, e, next, answer) : answer)),
            ),
    );

    on(
        'classic.*',
        {
            ['hook_event_name']: [
                'SessionStart',
                'PermissionDenied',
                'PostToolUse',
                'PostToolUseFailure',
                'PostToolBatch',
                'SubagentStart',
                'SubagentStop',
                'UserPromptSubmit',
                'Stop',
                'PreCompact',
                'PostCompact',
                'SessionEnd',
                'WorktreeCreate',
                'WorktreeRemove',
            ],
        },
        ($, e, next) =>
            next.is('!classic.PreToolUse', e)
                ? once(() => _open($))
                      .then((sink) => (sink.kind === 'ok' ? _observed($, sink.value, e, environment, $.clock.now()) : _NONE))
                      .then((entries) => next(e).then((result) => _answered(result, entries)))
                : next(e),
    );

    on('turn.*', ($, e, next) =>
        once(() => _open($))
            .then((sink) => (sink.kind === 'ok' ? record($, sink.value, next.event, e, TURN, $.clock.now()) : undefined))
            .then(() => next(e)),
    );

    on('ui.render', { component: 'SessionMode' }, (_$, e, next) =>
        environment.footer.text() === '' ? next(e) : next({ ...e, props: { modes: [...e.props.modes, environment.footer.text()] } }),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
