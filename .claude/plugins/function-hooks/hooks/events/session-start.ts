// Session facts written once, the status line, and under the dispatch option the close tool and the editor timer

// --- [IMPORTS] -------------------------------------------------------------------------

import type { On, RenderSurface, SessionRepo } from 'claude-code';
import { absurd } from '../composition/decision.ts';
import {
    flatMap,
    forEach,
    fromBoolean,
    fromNullable,
    fromPredicate,
    getOrElse,
    isRecord,
    liftPredicate,
    map,
    none,
    type Option,
    some,
    toArray,
} from '../composition/option.ts';
import { type Options, whenEnabled } from '../host/options.ts';
import {
    cleanedOf,
    decodeDispatch,
    decodeEnvironment,
    decodeFindings,
    decodeJson,
    decodeKeyedFindings,
    decodeSession,
    type Entry,
    type Environment,
    ids,
    isKindDispatch,
    isString,
    key,
    keys,
    type Namespace,
    type Session,
    suffix,
} from '../host/store.ts';
import { type Batch, EDITOR, type Spawn, spawnRule } from '../policies/agents.ts';
import {
    batch,
    CLOSE,
    type CloseInput,
    type Closing,
    close,
    decodeClose,
    due,
    expiredFindings,
    FINDING_VIEWS,
    MS_PER_DAY,
    MS_PER_HOUR,
    open,
    status,
} from '../policies/findings.ts';
import { expiredScans, type Run } from '../policies/scan.ts';

// --- [TYPES] ---------------------------------------------------------------------------

// The store as the timer reads it at one tick, the findings entries, the cleaned stamps, the dispatch rows, and the session's memory directory
interface Snapshot {
    readonly entries: readonly Entry[];
    readonly stamps: unknown;
    readonly dispatches: readonly unknown[];
    readonly memoryDir: Option<string>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _OWNER = /github\.com[:/](?<owner>[^/\s]+)\//u;
const _NON_ALPHANUMERIC = /[^A-Za-z0-9]/gu;
const _SETTINGS: readonly string[] = ['.claude/settings.json', '.claude/settings.local.json'];

// --- [OPERATIONS] ----------------------------------------------------------------------

// The autoMemoryDirectory of one settings file, a malformed file reads as no setting
const _configured = (text: string): Option<string> =>
    flatMap((settings: Readonly<Record<string, unknown>>) => fromPredicate(isString)(settings.autoMemoryDirectory))(
        flatMap(fromPredicate(isRecord))(decodeJson(text)),
    );

// The later file overrides the earlier one, and the derived directory stands where no file names one
const _memoryDirectory = (texts: readonly string[], derived: Option<string>): Option<string> =>
    texts.reduce<Option<string>>((chosen, text) => _configured(text).match<Option<string>>({ some, none: () => chosen }), derived);

// The HOME value printenv answered, none when it exited 1 with no output because HOME is unset, or printed an empty value
const _home = (run: Run): Option<string> =>
    flatMap((answered: Run) => liftPredicate<string>((value) => value !== '')(answered.stdout.trim()))(
        liftPredicate<Run>((answered) => answered.exitCode === 0)(run),
    );

// The directory the engine derives from HOME and the working directory, none when HOME answered nothing
const _derived = (home: Option<string>, cwd: string): Option<string> =>
    map((dir: string) => `${dir}/.claude/projects/${cwd.replace(_NON_ALPHANUMERIC, '-')}/memory`)(home);

// The owner segment of the origin remote $.session.repo() reads, none without a repository, a remote, or a github path
const _owner = (repo: SessionRepo | null): Option<string> =>
    flatMap((remote: string) => fromNullable(_OWNER.exec(remote)?.groups?.owner))(fromNullable(repo?.remote));

// Session-scoped keys of another session, deleted at start, the store then holds one session's facts beside the durable rows
const _SESSION_SCOPED: readonly Namespace[] = ['session', 'injected', 'loaded', 'snapshot', 'dns', 'prompt', 'roslyn'];

const _stale = (all: readonly string[], session: string): readonly string[] =>
    _SESSION_SCOPED.flatMap((namespace) =>
        keys(namespace)(all).filter((name) => name !== key(namespace, session) && !name.startsWith(`${key(namespace, session)}/`)),
    );

// The keys the start deletes: other sessions' facts, the scan hits past the window, and the closed findings past it
const _prune = (all: readonly string[], session: string, entries: readonly Entry[], now: number): readonly string[] => [
    ..._stale(all, session),
    ...expiredScans(all, now),
    ...expiredFindings(entries, now),
];

// Batches younger than a day hold the tick, their editor is still at work
const _held = (dispatches: readonly unknown[], now: number): boolean =>
    dispatches.flatMap((value) => toArray(decodeDispatch(value))).some((dispatch) => now - dispatch.spawnedAt < MS_PER_DAY);

// The close-out of an editor's final message, none when the message is not the close input
const _closing = (text: string, entries: readonly Entry[], stamps: unknown, now: number): Option<Closing> =>
    map((input: CloseInput) => close(input, decodeKeyedFindings(entries), cleanedOf(stamps), now))(flatMap(decodeClose)(decodeJson(text)));

// One line per row of a kind batch, the timer's spawn cannot reach the served close tool, and a part batch has no rows
const _rowLines = (named: Batch, entries: readonly Entry[]): readonly string[] =>
    toArray(fromPredicate(isKindDispatch)(named.dispatch)).flatMap((dispatch) =>
        decodeKeyedFindings(entries)
            .filter((keyed) => dispatch.rows.includes(keyed.key))
            .map(
                (keyed) =>
                    `- ${suffix('findings')(keyed.key)} ${keyed.row.kind} ${keyed.row.file} ${keyed.row.section}: ${keyed.row.evidence} | ${keyed.row.change}`,
            ),
    );

// The plugin's own spawn skips its agent.spawn hook, the editor's brief, the batch lines, the memory directory, and the rows join the prompt here,
// and the batch id is the editor's name and its probe directory label
const _editorPrompt = (named: Batch, entries: readonly Entry[], memoryDir: Option<string>): string =>
    spawnRule<Spawn>(
        some(named),
        named.batchId,
    )({
        subagentType: EDITOR,
        prompt: [`Batch ${named.batchId}`, ...toArray(map((dir: string) => `memory: ${dir}`)(memoryDir)), ..._rowLines(named, entries)].join('\n'),
    }).match<string>({
        rewrite: (input) => input.prompt,
        deny: absurd,
        answer: absurd,
    });

// --- [REGISTRATION] --------------------------------------------------------------------

const _facts = (on: On): void => {
    on('session.start', async ($, e, next) => {
        // mise.toml owns the environment, mise env --json in the session's working directory answers it, and every child runs over it
        const answered = await $.process.run(['mise', 'env', '--json']);
        const env = getOrElse((): Environment => {
            throw new Error(`mise env --json answered exit ${answered.exitCode} in ${e.cwd}: ${answered.stderr.trim()}`);
        })(flatMap(decodeEnvironment)(decodeJson(answered.stdout)));
        const [session, ancestors, repo, home, ...settings] = await Promise.all([
            $.session.id(),
            $.fs.ancestors({ names: ['CLAUDE.md'] }),
            $.session.repo(),
            $.process.run(['printenv', 'HOME'], { env }),
            ..._SETTINGS.map((file) =>
                $.fs
                    .exists(file)
                    .then((present) => forEach(() => $.fs.readFile(file))(fromBoolean(present)))
                    .then(getOrElse(() => '{}')),
            ),
        ]);
        // The configured autoMemoryDirectory wins over the derived one, and the MEMORY.md gate reads the winner
        const directory = _memoryDirectory(settings, _derived(_home(home), e.cwd));
        // The fs noun rejects a path outside the working directory, the gate on MEMORY.md runs through the host
        const memoryDir = getOrElse<Option<string>>(none)(
            await forEach((dir: string) =>
                $.process.run(['test', '-e', `${dir}/MEMORY.md`], { env }).then((memory) => liftPredicate<string>(() => memory.exitCode === 0)(dir)),
            )(directory),
        );
        // The session row is the store's JSON boundary, absence leaves the module as null here alone
        await $.store.set(key('session', session), {
            startedAt: $.clock.now(),
            claudeChain: ancestors.map((ancestor) => ancestor.dir),
            memoryDir: getOrElse((): string | null => null)(memoryDir),
            remoteOwner: getOrElse((): string | null => null)(_owner(repo)),
            env,
        });
        const all = await $.store.keys();
        const entries = await Promise.all(keys('findings')(all).map(async (name): Promise<Entry> => ({ key: name, value: await $.store.get(name) })));
        await Promise.all(_prune(all, session, entries, $.clock.now()).map((name) => $.store.delete(name)));
        return next(e);
    });
};

// The open count and the due parts of one snapshot, the line the status band shows
const _statusLine = (surface: RenderSurface | null, current: Snapshot, now: number): Option<string> =>
    status(surface, open(decodeFindings(current.entries.map((entry) => entry.value))), due(cleanedOf(current.stamps), now));

// The served tool and the editor timer belong to a session a person keeps open, a -p run exits before either matters
const _dispatch = (on: On): void => {
    on('session.start', { interactive: true }, async ($, e, next) => {
        const session = await $.session.id();
        await $.tool.register(CLOSE);
        const snapshot = async (): Promise<Snapshot> => {
            const all = await $.store.keys();
            const [stamps, sessionRow, ...values] = await Promise.all([
                $.store.get(key('cleaned')),
                $.store.get(key('session', session)),
                ...keys('findings')(all).map((name) => $.store.get(name)),
            ]);
            const dispatches = await Promise.all(ids('dispatch')(all).map((batchId) => $.store.get(key('dispatch', batchId))));
            return {
                entries: keys('findings')(all).map((name, index): Entry => ({ key: name, value: values[index] })),
                stamps,
                dispatches,
                memoryDir: flatMap((row: Session) => fromNullable(row.memoryDir))(decodeSession(sessionRow)),
            };
        };
        // The status line belongs to the guidance lifecycle, a session with dispatch off shows none
        $.ui.status(getOrElse(() => undefined)(_statusLine(e.surface, await snapshot(), $.clock.now())));
        // The editor's final message is the close input, the plugin's own spawn never reaches its served tool, and the settled batch lifts the hold
        const settle = async (batchId: string, text: string): Promise<void> => {
            const current = await snapshot();
            await forEach(async (closing: Closing): Promise<void> => {
                await Promise.all(closing.writes.map((write) => $.store.set(write.key, write.value)));
                await $.store.delete(key('dispatch', batchId));
                FINDING_VIEWS.map((view) => $.ui.invalidate(view));
            })(_closing(text, current.entries, current.stamps, $.clock.now()));
        };
        const tick = async (): Promise<void> => {
            const current = await snapshot();
            const now = $.clock.now();
            await forEach(async (named: Batch): Promise<void> => {
                await $.store.set(key('dispatch', named.batchId), named.dispatch);
                const spawned = await $.agent.spawn({
                    subagentType: EDITOR,
                    name: named.batchId,
                    prompt: _editorPrompt(named, current.entries, current.memoryDir),
                    background: true,
                });
                // The result is a refusal or the editor's final message, the notice under the one and the settle under the other
                await forEach(async (reason: string): Promise<void> => {
                    await $.store.set(key('notice'), { session, text: `The editor spawn was refused: ${reason}` });
                    $.ui.invalidate('ui.render');
                })(fromNullable(spawned.deny));
                await forEach((text: string) => settle(named.batchId, text))(fromNullable(spawned.text));
            })(flatMap(() => batch(current.entries, current.stamps, now, crypto.randomUUID()))(fromBoolean(!_held(current.dispatches, now))));
        };
        $.clock.every(MS_PER_HOUR, () => {
            tick().catch(() => undefined);
        });
        return next(e);
    });
};

// The plain hook writes the session row, the matched one sets the status line, serves the close tool, and runs the timer while dispatch is on
const sessionStart = (on: On, options: Options): void => {
    _facts(on);
    whenEnabled(options.dispatch, () => _dispatch(on));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { sessionStart };
