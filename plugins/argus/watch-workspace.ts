import { dirname, resolve } from 'node:path';
import { type Event, getEventsSince, subscribe, writeSnapshot } from '@parcel/watcher';
import { Array, Effect, FileSystem, flow, Option, Order, PlatformError, Queue, Stream, String, Trie } from 'effect';
import { ChildProcess } from 'effect/process';
import { escape } from 'minimatch';
import { Exited } from '#/exited';

// --- [OPERATIONS] ----------------------------------------------------------------------

const git = Effect.fnUntraced(function* (root: string, args: readonly string[]) {
    const child = yield* ChildProcess.make('git', args, { cwd: root });
    const [text, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(child.stdout)), child.exitCode], { concurrency: 'unbounded' });
    return code === 0 ? text : yield* Exited.make({ command: 'git', args, code });
}, Effect.scoped);

const watchWorkspace = Effect.fn('watchWorkspace')(function* (root: string) {
    const [listing, repository] = yield* Effect.all([git(root, ['ls-files', '--others', '--ignored', '--exclude-standard', '--directory', '-z']), git(root, ['rev-parse', '--git-dir'])], { concurrency: 'unbounded' });
    const ignored = [...listing.split('\0').filter(String.isNonEmpty), `${String.trimEnd(repository)}/`];
    const parents = Trie.fromIterable(ignored.filter(String.endsWith('/')).map((path) => [path, undefined] as const));
    const outermost = ignored.filter(flow(dirname, String.concat('/'), (parent) => Trie.longestPrefixOf(parents, parent), Option.isNone));
    const fseventsExclusionLimit = 8;
    const [excluded, matched] = Array.splitAt(
        Array.sortWith(outermost, (path) => [!path.endsWith('/'), path.split('/').length] as const, Order.Tuple([Order.Boolean, Order.Number])),
        fseventsExclusionLimit,
    );
    const snapshot = resolve(yield* FileSystem.FileSystem.use((fs) => fs.makeTempDirectoryScoped()), 'snapshot');
    const ignore = [...excluded.map((path) => resolve(root, path)), ...matched.map((path) => `${escape(path.endsWith('/') ? path.slice(0, -1) : path, { magicalBraces: true })}{,/**}`)];
    const scanned = { ignore, backend: 'brute-force' as const };
    const watched = <A>(run: () => Promise<A>) => Effect.tryPromise({ try: run, catch: (cause) => PlatformError.systemError({ _tag: 'Unknown', module: 'FileSystem', method: 'watch', pathOrDescriptor: root, cause }) });
    const record = watched(() => writeSnapshot(root, snapshot, scanned));
    const missed = Effect.tap(
        watched(() => getEventsSince(root, snapshot, scanned)),
        record,
    );
    const listen = (queue: Queue.Enqueue<readonly [Error | null, readonly Event[]]>) => Effect.acquireRelease(watched(subscribe.bind(undefined, root, (error, events) => Queue.offerUnsafe(queue, [error, events]), { ignore })), (subscription) => Effect.promise(subscription.unsubscribe.bind(subscription)));
    return Stream.unwrap(Effect.as(record, Stream.callback<readonly [Error | null, readonly Event[]], PlatformError.PlatformError>(listen))).pipe(Stream.mapEffect(([error, events]) => (error === null ? Effect.succeed(events) : Effect.map(missed, Array.prependAll(events)))));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { watchWorkspace };
