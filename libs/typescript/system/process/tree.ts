import { Array, Data, DateTime, Duration, Effect, FileSystem, Graph, Option, Order, Result } from 'effect';
import { run } from '#/child-process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const row = /^\s*(?<pid>\d+)\s+(?<parent>\d+)\s+(?:(?<days>\d+)-)?(?:(?<hours>\d+):)?(?<minutes>\d+):(?<seconds>\d+(?:\.\d+)?)\s(?<command>.*)$/u;

// --- [MODELS] --------------------------------------------------------------------------

class Process extends Data.Class<{ readonly pid: number; readonly parent: number; readonly cpu: Duration.Duration; readonly command: string }> {}

class ProcessTree extends Data.Class<{ readonly processes: readonly Process[] }> {
    static readonly read = Effect.map(run(['ps', '-A', '-o', 'pid=,ppid=,time=,command=']), ({ stdout }) => {
        const listed = stdout.split('\n').map((line) => Option.fromNullishOr(row.exec(line)?.groups));
        return new ProcessTree({
            processes: Array.getSomes(listed).map(
                ({ pid = '', parent = '', days = '0', hours = '0', minutes = '', seconds = '', command = '' }) => new Process({ pid: Number(pid), parent: Number(parent), cpu: Duration.fromInputUnsafe({ days: Number(days), hours: Number(hours), minutes: Number(minutes), seconds: Number(seconds) }), command }),
            ),
        });
    });
    readonly #pids = new Set(this.processes.map(({ pid }) => pid));
    readonly #graph = Graph.fromSnapshot({
        type: 'directed',
        nodes: Array.sort(
            this.processes,
            Order.mapInput(Order.Number, ({ pid }: Process) => pid),
        ).map((process) => ({ index: process.pid, data: process })),
        edges: this.processes.filter(({ pid, parent }) => parent !== pid && this.#pids.has(parent)).map(({ pid, parent }, index) => ({ index, source: parent, target: pid, data: undefined })),
    });

    get cpu() {
        return this.processes.reduce((total, { cpu }) => Duration.sum(total, cpu), Duration.zero);
    }
    subtree(pid: number) {
        return Option.map(Graph.getNode(this.#graph, pid), () => new ProcessTree({ processes: this.#graph.pipe(Graph.dfs({ start: [pid] }), Graph.values, Array.fromIterable) }));
    }
    ancestor(pid: number, test: (process: Process) => boolean) {
        return Option.flatMap(Graph.getNode(this.#graph, pid), () =>
            this.#graph.pipe(
                Graph.dfs({ start: [pid], direction: 'incoming' }),
                Graph.values,
                Array.findFirst((process) => process.pid !== pid && test(process)),
            ),
        );
    }
}

class Writer extends Data.Class<{ readonly pid: number; readonly parent: number; readonly path: string; readonly modified: DateTime.Utc }> {
    static readonly of = Effect.fn('Writer.of')(function* (over: ProcessTree | readonly string[]) {
        const fs = yield* FileSystem.FileSystem;
        const named =
            over instanceof ProcessTree
                ? Option.map(Option.liftPredicate(over.processes, Array.isReadonlyArrayNonEmpty), (processes) => ['-b', '-p', processes.map(({ pid }) => pid).join(',')])
                : Option.map(Option.liftPredicate(yield* Effect.filter(over, (file) => fs.exists(file)), Array.isReadonlyArrayNonEmpty), (files) => ['--', ...files]);
        const stdout = yield* Option.match(named, {
            onNone: () => Effect.succeed(''),
            onSome: (words) =>
                run(['lsof', '-w', '-n', '-P', '-F', 'pRftan', ...words]).pipe(
                    Effect.catchTag('ExitError', (exited) => (exited.code === 1 && exited.stderr === '' ? Effect.succeed(exited) : exited)),
                    Effect.map((ran) => ran.stdout),
                ),
        });
        const listed = stdout
            .split(/^p/mu)
            .slice(1)
            .flatMap((listing) => {
                const [head = '', ...files] = listing.split(/^f/mu);
                const owner = fields(`p${head}`);
                return files
                    .map(fields)
                    .filter((file) => /^[uw]$/u.test(file.get('a') ?? '') && file.get('t') === 'REG' && file.has('n'))
                    .map((file) => ({ pid: Number(owner.get('p') ?? ''), parent: Number(owner.get('R') ?? ''), path: file.get('n') ?? '' }));
            });
        const held = Array.dedupeWith(listed, (left, right) => left.pid === right.pid && left.path === right.path);
        const writers = yield* Effect.filterMapEffect(held, (file) =>
            fs.stat(file.path).pipe(
                Effect.map(({ mtime }) => Option.match(mtime, { onNone: () => Result.failVoid, onSome: (date) => Result.succeed(new Writer({ ...file, modified: DateTime.fromDateUnsafe(date) })) })),
                Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed(Result.failVoid)),
            ),
        );
        return Array.sort(writers, Order.flip(Order.mapInput(DateTime.Order, ({ modified }: Writer) => modified)));
    });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const fields = (block: string) => new Map(block.split('\n').map((line) => [line.slice(0, 1), line.slice(1)] as const));

// --- [EXPORTS] -------------------------------------------------------------------------

export { Process, ProcessTree, Writer };
