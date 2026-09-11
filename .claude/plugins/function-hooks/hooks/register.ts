// --- [IMPORTS] -------------------------------------------------------------------------

import type { EngineInterface, Frozen, Next, Register, ToolCallInput, ToolCallResult } from 'claude-code';
import { decide } from './events/tool-call.ts';
import { CALL, CLASSIC, type Columns, type Event, type Row, row, TURN } from './observation/row.ts';
import { argv, database, keep, LOCATE, script, WAL } from './observation/sql.ts';
import { SCAN } from './text/argv.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Opened =
    | { readonly kind: 'open'; readonly argv: readonly string[]; readonly sessionId: string }
    | { readonly kind: 'closed'; readonly reason: string };

// --- [CONSTANTS] -----------------------------------------------------------------------

const _CTRL = /\p{Cc}+/gu;

// --- [OPEN] ----------------------------------------------------------------------------

const _closed = (reason: string): Opened => ({ kind: 'closed', reason });

const _pragma = ($: EngineInterface, sqlite: readonly string[], sessionId: string): Promise<Opened> =>
    $.process.run(sqlite, { stdin: WAL }).then(
        ({ exitCode, stdout, stderr }) => {
            if (stdout.trim() !== 'wal') {
                $.ui.log(`observation: journal mode is ${stdout.trim()}, sqlite3 exited ${exitCode}, ${stderr.trim()}`);
            }
            return { kind: 'open', argv: sqlite, sessionId };
        },
        (cause: unknown) => _closed(`sqlite3 did not run, ${String(cause)}`),
    );

const _prepared = ($: EngineInterface, sqlite: readonly string[], sessionId: string, root: string): Promise<Opened> =>
    $.fs.write(keep(root), '').then(
        () => _pragma($, sqlite, sessionId),
        (cause: unknown) => _closed(`${keep(root)} not written, ${String(cause)}`),
    );

const _located = ($: EngineInterface, root: string, sessionId: string): Promise<Opened> =>
    $.process.run(LOCATE).then(
        ({ exitCode, stdout, stderr }) =>
            exitCode === 0
                ? _prepared($, argv(stdout.trim(), database(root)), sessionId, root)
                : _closed(`mise where sqlite exited ${exitCode}, ${stderr.trim()}`),
        (cause: unknown) => _closed(`mise where sqlite did not run, ${String(cause)}`),
    );

const _open = ($: EngineInterface): Promise<Opened> =>
    $.session
        .repo()
        .then((repo) =>
            repo === null ? _closed('session runs outside a git repository') : $.session.id().then((sessionId) => _located($, repo.root, sessionId)),
        );

const _noted = ($: EngineInterface, opened: Opened): void => {
    if (opened.kind === 'closed') {
        $.ui.log(`observation: rows are not recorded, ${opened.reason}`);
    }
};

// --- [RECORD] --------------------------------------------------------------------------

const _lost = ($: EngineInterface, built: Row, fault: string): void => {
    $.ui.log(`observation: ${built.event} row ${built.toolUseId.kind === 'text' ? built.toolUseId.text : built.sessionId} not written, ${fault}`);
};

const _write = ($: EngineInterface, sqlite: readonly string[], built: Row): Promise<void> =>
    $.process.run(sqlite, { stdin: script(built) }).then(
        ({ exitCode, stderr }) => {
            if (exitCode !== 0) {
                _lost($, built, `sqlite3 exited ${exitCode}, ${stderr.trim()}`);
            }
        },
        (cause: unknown) => _lost($, built, `sqlite3 did not run, ${String(cause)}`),
    );

const record = (
    $: EngineInterface,
    opened: Extract<Opened, { readonly kind: 'open' }>,
    event: Event,
    value: Readonly<Record<string, unknown>>,
    columns: Columns,
): Promise<void> => _write($, opened.argv, row(event, value, columns, opened.sessionId, $.clock.now()));

const _denied = (
    $: EngineInterface,
    opened: Opened,
    e: Frozen<ToolCallInput>,
    next: Next<'tool.call'>,
    answer: ToolCallResult,
): ToolCallResult | Promise<ToolCallResult> =>
    answer.deny === undefined || opened.kind === 'closed'
        ? answer
        : record($, opened, 'tool.call', { ...e, deny: answer.deny, trace: next.trace }, CALL).then(() => answer);

// --- [REGISTRATION] --------------------------------------------------------------------

const register: Register = (on) => {
    let opened: Opened = _closed('session.start has not run');

    on('session.start', ($, e, next) =>
        _open($).then((result) => {
            opened = result;
            _noted($, result);
            return next(e);
        }),
    );

    on('tool.call', ($, e, next) =>
        decide(
            e,
            (text: string) => $.process.run(SCAN, { stdin: text }),
            (path: string) => $.fs.exists(path),
        )
            .then((decision) => (decision.kind === 'deny' ? { deny: decision.reason.replace(_CTRL, ' ') } : next(decision.e)))
            .then<ToolCallResult>((answer) => _denied($, opened, e, next, answer)),
    );

    on(
        'classic.*',
        {
            ['hook_event_name']: [
                'PostToolUse',
                'PostToolUseFailure',
                'PostToolBatch',
                'SubagentStart',
                'SubagentStop',
                'UserPromptSubmit',
                'Stop',
                'PostCompact',
                'SessionEnd',
            ],
        },
        ($, e, next) =>
            next.is('!classic.PreToolUse', e) && opened.kind === 'open'
                ? record($, opened, e.hook_event_name, e, CLASSIC).then(() => next(e))
                : next(e),
    );

    on('turn.*', ($, e, next) => (opened.kind === 'open' ? record($, opened, next.event, e, TURN).then(() => next(e)) : next(e)));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
