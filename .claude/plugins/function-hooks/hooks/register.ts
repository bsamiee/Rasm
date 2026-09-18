// --- [IMPORTS] -------------------------------------------------------------------------

import type {
    ClassicHookEvent,
    ClassicHookInputs,
    ClassicResult,
    EngineInterface,
    Frozen,
    Next,
    ProcessRunInit,
    ProcessRunResult,
    Register,
    ToolCallInput,
    ToolCallResult,
    TurnCompleteInput,
} from 'claude-code';
import { SCAN } from './command.ts';
import { bind, fault, fromNullable, map, none, type Option, ok, type Result, some } from './composition.ts';
import { decide } from './events/tool-call.ts';
import {
    type Building,
    context,
    DELIVER,
    delivering,
    description,
    due,
    dueCategories,
    type Judging,
    LEDGER,
    type Lineage,
    lineageOf,
    listed,
    occupied,
    prompt,
    REPORT,
    resolved,
    type Settings,
    type Spawned,
    STATE,
    settings,
    state,
    status,
    subject,
} from './observation/delivery.ts';
import { CALL, CLASSIC, type Columns, type Event, row, session, TURN } from './observation/row.ts';
import { type Argv, database, keep, LOCATE, open, script, sqlite3 } from './observation/sql.ts';
import { basename } from './path.ts';
import type { Place } from './policies/walk.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Sink {
    readonly argv: Argv;
    readonly root: string;
}

interface Stamped {
    readonly sink: Sink;
    readonly ts: number;
}

type Classic = Frozen<ClassicHookInputs[Exclude<ClassicHookEvent, 'PreToolUse'>]>;

type Boundary = Frozen<ClassicHookInputs['Stop']> | Frozen<ClassicHookInputs['SubagentStop']>;

type Completed = Frozen<TurnCompleteInput>;

interface Memo {
    readonly text: () => string;
    readonly set: (text: string) => boolean;
}

interface Environment {
    readonly chosen: Settings;
    readonly claims: Set<string>;
    readonly spawned: Map<string, Spawned>;
    readonly footer: Memo;
}

type Awaited = readonly [string, Spawned, Map<string, Spawned>];

// --- [CONSTANTS] -----------------------------------------------------------------------

const _CTRL = /\p{Cc}+/gu;
const _PLACED: Argv = ['git', 'status', '--porcelain', 'tools/ast-grep'];
const _PORCELAIN_PATH = 3;

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

const _scan = ($: EngineInterface, text: string): Promise<ProcessRunResult> => $.session.repo().then((repo) => $.process.run(SCAN, repo === null ? { stdin: text } : { stdin: text, cwd: repo.root }));

const _homed = ($: EngineInterface, cwd: string): Promise<Place> => $.env.get('HOME').then((home) => ({ home: fromNullable(home), cwd }));

const _place = ($: EngineInterface): Promise<Place> => $.session.cwd().then((cwd) => _homed($, cwd));

// --- [OPEN] ----------------------------------------------------------------------------

const _switched = ($: EngineInterface, sqlite: Argv, root: string): Promise<Result<Sink>> =>
    _run($, sqlite, { stdin: 'pragma journal_mode=wal;', cwd: root }).then((switched) => {
        const mode = switched.kind === 'ok' ? switched.value.trim() : switched.reason;
        if (mode !== 'wal') {
            $.ui.log(`journal mode not switched, ${mode}`);
        }
        return ok({ argv: sqlite, root });
    });

const _applied = ($: EngineInterface, sqlite: Argv, root: string): Promise<Result<Sink>> =>
    _run($, sqlite, { stdin: open(root), cwd: root }).then((applied) => (applied.kind === 'fault' ? fault(`schema not applied, ${applied.reason}`) : _switched($, sqlite, root)));

const _prepared = ($: EngineInterface, sqlite: Argv, root: string): Promise<Result<Sink>> =>
    $.fs.write(keep(root), '').then(
        () => _applied($, sqlite, root),
        (cause: unknown) => fault(`${keep(root)} not written, ${String(cause)}`),
    );

const _located = ($: EngineInterface, root: string): Promise<Result<Sink>> =>
    _run($, LOCATE, { cwd: root }).then((where) => bind(where, (install) => _prepared($, sqlite3(install.trim(), database(root)), root)));

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

const _stamped = ($: EngineInterface, once: (attempt: () => Promise<Result<Sink>>) => Promise<Result<Sink>>): Promise<Result<Stamped>> =>
    $.clock.now().then((ts) => once(() => _open($)).then((sink) => map(sink, (value) => ({ sink: value, ts }))));

// --- [RECORD] --------------------------------------------------------------------------

const record = ($: EngineInterface, sink: Sink, event: Event, value: Readonly<Record<string, unknown>>, columns: Columns, ts: number): Promise<void> => {
    const write = (id: string): Promise<void> => {
        const built = row(event, value, columns, id, ts);
        return _run($, sink.argv, { stdin: script(built), cwd: sink.root }).then((ran) => {
            if (ran.kind === 'fault') {
                $.ui.log(`${built.event} row ${built.toolUseId.kind === 'some' ? built.toolUseId.value : built.sessionId} not written, ${ran.reason}`);
            }
        });
    };
    const own = session(value, columns);
    return own.kind === 'some' ? write(own.value) : $.session.id().then(write);
};

const _classic = ($: EngineInterface, sink: Sink, e: Classic, ts: number): Promise<void> =>
    e.hook_event_name === 'Stop' || e.hook_event_name === 'SessionEnd'
        ? $.session.usage().then((usage) => record($, sink, e.hook_event_name, { ...e, usage }, CLASSIC, ts))
        : record($, sink, e.hook_event_name, e, CLASSIC, ts);

const _denied = ($: EngineInterface, sink: Sink, e: Frozen<ToolCallInput>, next: Next<'tool.call'>, answer: ToolCallResult, ts: number): Promise<ToolCallResult> =>
    record($, sink, 'tool.call', { ...e, deny: answer.deny, trace: next.trace }, CALL, ts).then(() => answer);

// --- [SPAWN] ---------------------------------------------------------------------------

const _spawn = ($: EngineInterface, spawned: Spawned): Promise<Option<string>> =>
    $.agent.spawn({ prompt: prompt(spawned), subagentType: spawned.agent, description: description(spawned), cwd: spawned.lineage.worktree }).then(
        (result) => {
            $.ui.log(resolved(spawned, result));
            return fromNullable(result.agentId);
        },
        (cause: unknown) => {
            $.ui.log(`${spawned.agent} did not spawn, ${String(cause)}`);
            return none;
        },
    );

const _launched = ($: EngineInterface, environment: Environment, spawned: Spawned): void => {
    environment.claims.add(spawned.agent);
    _spawn($, spawned).then((id) => {
        environment.claims.delete(spawned.agent);
        if (id.kind === 'some') {
            environment.spawned.set(id.value, spawned);
        }
    });
};

// --- [SETTLEMENT] ----------------------------------------------------------------------

const _awaited = (environment: Result<Environment>, agentId: string | undefined): Option<Awaited> => {
    if (environment.kind === 'fault' || agentId === undefined) {
        return none;
    }
    const found = fromNullable(environment.value.spawned.get(agentId));
    return found.kind === 'some' ? some([agentId, found.value, environment.value.spawned]) : none;
};

const _judged = ($: EngineInterface, sink: Sink, e: Completed, [id, judging, spawned]: readonly [string, Judging, Map<string, Spawned>], ts: number): Promise<void> =>
    record($, sink, 'turn.complete', e, TURN, ts)
        .then(() => _run($, sink.argv, { stdin: LEDGER(judging, id, ts), cwd: judging.lineage.worktree }))
        .then((written) => {
            if (written.kind === 'ok' && written.value.trim() === '') {
                $.ui.log(`${judging.agent} ${id} answered with no transition over ${subject(judging)}, range stays unjudged`);
                return;
            }
            spawned.delete(id);
            $.ui.log(written.kind === 'fault' ? `ledger row not written, ${written.reason}` : `${judging.agent} ${id} judged ${subject(judging)}`);
        });

const _placed = ($: EngineInterface, e: Completed, building: Building): Promise<Readonly<Record<string, unknown>>> =>
    _run($, _PLACED, { cwd: building.lineage.worktree }).then((printed) => {
        if (printed.kind === 'fault') {
            $.ui.log(`placed rules not read, ${printed.reason}`);
            return e;
        }
        return { ...e, placed: printed.value.split('\n').flatMap((line) => (line === '' ? [] : [line.slice(_PORCELAIN_PATH)])) };
    });

const _built = ($: EngineInterface, sink: Sink, e: Completed, [id, building, spawned]: readonly [string, Building, Map<string, Spawned>], ts: number): Promise<void> => {
    spawned.delete(id);
    return _placed($, e, building)
        .then((value) => record($, sink, 'turn.complete', value, TURN, ts))
        .then(() => _run($, sink.argv, { stdin: REPORT(building, id, ts), cwd: building.lineage.worktree }))
        .then((written) => {
            $.ui.log(written.kind === 'fault' ? `report rows not written, ${written.reason}` : `${building.agent} ${id} reported ${subject(building)}`);
        });
};

const _settled = ($: EngineInterface, sink: Sink, e: Completed, [id, pending, spawned]: Awaited, ts: number): Promise<void> => {
    if (e.reason !== 'answer') {
        spawned.delete(id);
        $.ui.log(`${pending.agent} ${id} ended on ${e.reason} over ${subject(pending)}, nothing recorded`);
        return record($, sink, 'turn.complete', e, TURN, ts);
    }
    return pending.kind === 'range' ? _judged($, sink, e, [id, pending, spawned], ts) : _built($, sink, e, [id, pending, spawned], ts);
};

const _completed = ($: EngineInterface, sink: Sink, e: Completed, ts: number, environment: Result<Environment>): Promise<void> => {
    const awaited = _awaited(environment, e.agentId);
    return awaited.kind === 'some' ? _settled($, sink, e, awaited.value, ts) : record($, sink, 'turn.complete', e, TURN, ts);
};

// --- [DELIVERY] ------------------------------------------------------------------------

const _stopping = (e: Classic): e is Boundary => (e.hook_event_name === 'Stop' && !e.stop_hook_active) || (e.hook_event_name === 'SubagentStop' && e.agent_type !== '' && !e.stop_hook_active);

const _skip = ($: EngineInterface, reason: string): readonly string[] => {
    $.ui.log(`boundary skipped, ${reason}`);
    return [];
};

const _delivered = ($: EngineInterface, sink: Sink, e: Boundary, lineage: Lineage, to: number): Promise<readonly string[]> =>
    _run($, sink.argv, { stdin: DELIVER(lineage, e.session_id, to), cwd: lineage.worktree }).then((rows) =>
        rows.kind === 'fault' ? _skip($, `delivery rows not written, ${rows.reason}`) : context(rows.value, lineage.branch),
    );

const _counted = ($: EngineInterface, sink: Sink, e: Boundary, lineage: Lineage, to: number, environment: Environment): Promise<readonly string[]> =>
    _run($, sink.argv, { stdin: STATE(lineage, to, environment.chosen), cwd: lineage.worktree }).then((read) => {
        const seen = bind(read, (text) => state(text, environment.chosen));
        if (seen.kind === 'fault') {
            return _skip($, `state not read, ${seen.reason}`);
        }
        if (environment.footer.set(status(seen.value))) {
            $.ui.invalidate('ui.render');
        }
        const tasks = listed(e.background_tasks);
        if (tasks.kind === 'fault') {
            return _skip($, tasks.reason);
        }
        const quiet = seen.value.edits.holding === 0;
        const busy = occupied(tasks.value, environment.claims);
        if (due(seen.value.edits, busy, quiet)) {
            _launched($, environment, { kind: 'range', agent: environment.chosen.edits.agent, lineage, range: seen.value.edits, to });
        }
        const [candidate] = dueCategories(seen.value, environment.chosen, busy, quiet);
        if (candidate !== undefined) {
            _launched($, environment, { kind: 'category', agent: environment.chosen.categoryAgent, lineage, session: e.session_id, category: candidate.category });
        }
        return e.hook_event_name === 'Stop' && delivering(seen.value, environment.chosen, busy, quiet) ? _delivered($, sink, e, lineage, to) : [];
    });

const _branched = ($: EngineInterface, sink: Sink, e: Boundary, to: number, environment: Environment, worktree: string): Promise<readonly string[]> =>
    _run($, ['git', 'branch', '--show-current'], { cwd: e.cwd }).then((branch) =>
        branch.kind === 'fault' ? _skip($, branch.reason) : _counted($, sink, e, lineageOf(sink.root, worktree, branch.value.trim().replace(_CTRL, ' ')), to, environment),
    );

const _boundary = ($: EngineInterface, sink: Sink, e: Boundary, to: number, environment: Environment): Promise<readonly string[]> =>
    _run($, ['git', 'rev-parse', '--show-toplevel'], { cwd: e.cwd }).then((worktree) =>
        worktree.kind === 'fault' ? _skip($, worktree.reason) : _branched($, sink, e, to, environment, worktree.value.trim()),
    );

const _observed = ($: EngineInterface, sink: Sink, e: Classic, environment: Result<Environment>, footer: Memo, to: number): Promise<readonly string[]> =>
    _classic($, sink, e, to).then(() => {
        if (e.hook_event_name === 'SessionEnd' && footer.set('')) {
            $.ui.invalidate('ui.render');
        }
        if (!_stopping(e)) {
            return [];
        }
        return environment.kind === 'ok' ? _boundary($, sink, e, to, environment.value) : _skip($, `options not read, ${environment.reason}`);
    });

const _answered = (result: ClassicResult, entries: readonly string[]): ClassicResult => {
    const earlier = fromNullable(result.additionalContext);
    return entries.length === 0 ? result : { ...result, additionalContext: earlier.kind === 'some' ? [...earlier.value, ...entries] : [...entries] };
};

// --- [REGISTRATION] --------------------------------------------------------------------

const register: Register = (on, options) => {
    const footer = _memo();
    const claims: Set<string> = new Set();
    const spawned: Map<string, Spawned> = new Map();
    const environment = map(settings(options), (chosen): Environment => ({ chosen, claims, spawned, footer }));
    const walking = options['walkPolicy'] === true;
    let opening: Promise<Result<Sink>> | undefined;
    const once = (attempt: () => Promise<Result<Sink>>): Promise<Result<Sink>> => {
        opening ??= attempt();
        return opening;
    };

    on('tool.call', ($, e, next) =>
        decide(
            e,
            (text: string) => _scan($, text),
            (path: string) => $.fs.exists(path),
            () => _place($),
            walking,
        )
            .then((decision) => (decision.kind === 'deny' ? { deny: decision.reason.replace(_CTRL, ' ') } : next(decision.e)))
            .then<ToolCallResult>((answer) =>
                answer.deny === undefined ? answer : _stamped($, once).then((found) => (found.kind === 'ok' ? _denied($, found.value.sink, e, next, answer, found.value.ts) : answer)),
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
                'StopFailure',
                'PreCompact',
                'PostCompact',
                'SessionEnd',
                'WorktreeCreate',
                'WorktreeRemove',
            ],
        },
        ($, e, next) =>
            next.is('!classic.PreToolUse', e)
                ? _stamped($, once)
                      .then((found) => (found.kind === 'ok' ? _observed($, found.value.sink, e, environment, footer, found.value.ts) : []))
                      .then((entries) => next(e).then((result) => _answered(result, entries)))
                : next(e),
    );

    on('turn.*', ($, e, next) =>
        _stamped($, once)
            .then((found) => {
                if (found.kind === 'fault') {
                    return;
                }
                return next.is('turn.complete', e) ? _completed($, found.value.sink, e, found.value.ts, environment) : record($, found.value.sink, next.event, e, TURN, found.value.ts);
            })
            .then(() => next(e)),
    );

    on('ui.render', { component: 'SessionMode' }, (_$, e, next) => (footer.text() === '' ? next(e) : next({ ...e, props: { modes: [...e.props.modes, footer.text()] } })));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
