import { execPath } from 'node:process';
import { fileURLToPath } from 'node:url';
import { detectPackageManager, normalizePath } from '@nx/devkit';
import { getLockFileName, globWithWorkspaceContext, hashObject, isCI, killProcessTreeGraceful } from '@nx/devkit/internal';
import { type RawTsconfigJsonCache, walkTsconfigExtendsChain } from '@nx/js/internal';
import { Array, Duration, Effect, Fiber, Option, Path, Queue, Result, Schema, Stream } from 'effect';
import { ancestors, listed, MANIFESTS, type Reading, type Request, type Unit } from '#/plugin';
import { spawned, ToolError } from '#/process';
import { type Job, Jobs, REPLIES, Reply } from '#/reply';

// --- [TYPES] ---------------------------------------------------------------------------

interface Asked<J extends Job> extends Unit {
    readonly job: J['Type'];
    readonly chain: readonly string[];
    readonly governed: readonly string[];
}

interface Answer<A> {
    readonly report: A;
    readonly loaded: readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const CONFIG = 'tsconfig.json';

// --- [MODELS] --------------------------------------------------------------------------

const Timed = Schema.Struct({ deadline: Schema.DurationFromString.pipe(Schema.withDecodingDefaultKey(Effect.succeed('1 minute'))) }).annotate({ description: 'deadline: longest wait for one config to answer before its reader child is stopped' });
const Referencing = Schema.Struct({ references: Schema.Array(Schema.Struct({ path: Schema.String })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const chained = Effect.fnUntraced(function* (workspaceRoot: string, roots: readonly string[]) {
    const path = yield* Path.Path;
    const governing = (root: string) => ancestors(path, root).map((at) => path.join(at, CONFIG));
    const present = new Set(yield* Effect.promise(() => globWithWorkspaceContext(workspaceRoot, Array.dedupe(roots.flatMap(governing)))));
    const parsed: RawTsconfigJsonCache = new Map();
    const referenced = (at: string, raw: unknown) =>
        Option.match(Schema.decodeUnknownOption(Referencing)(raw), {
            onNone: () => [],
            onSome: ({ references }) => references.map((to) => path.resolve(path.dirname(at), to.path)).map((to) => (path.extname(to) === '.json' ? to : path.join(to, CONFIG))),
        });
    const reached = (entry: string, seen: Set<string>): Set<string> => {
        const linked: string[] = [];
        walkTsconfigExtendsChain(
            entry,
            (at, raw) => {
                seen.add(at);
                linked.push(...(at === entry ? referenced(at, raw) : []));
                return 'continue';
            },
            { jsonCache: parsed },
        );
        return linked.filter((to) => !seen.has(to)).reduce((found, to) => reached(to, found), seen);
    };
    return new Map(
        roots.map((root) => {
            const files = Option.match(
                Array.findFirst(governing(root), (file) => present.has(file)),
                { onNone: () => [], onSome: (config) => [...reached(path.join(workspaceRoot, config), new Set())] },
            );
            return [root, files.map((at) => normalizePath(path.relative(workspaceRoot, at))).filter((at) => !(at.startsWith('..') || at.split('/').includes('node_modules')))] as const;
        }),
    );
});

const answered = Effect.fnUntraced(function* <J extends Job, A>(entry: string, jobs: ReturnType<typeof Jobs<J>>, reply: ReturnType<typeof Reply<A, A>>, workspaceRoot: string, launch: Readonly<Record<string, string>>, deadline: Duration.Duration, asked: readonly Asked<J>[]) {
    const input = yield* Effect.orDie(Schema.encodeEffect(jobs)(asked.map(({ job }) => job)));
    const handle = yield* spawned(execPath, [fileURLToPath(entry)], { cwd: workspaceRoot, env: launch, stdin: Stream.encodeText(Stream.succeed(input)), stdout: 'ignore', killSignal: 'SIGKILL', additionalFds: { [`fd${REPLIES}`]: { type: 'output' } } });
    const [stderr, replies] = yield* Effect.all([Effect.forkScoped(Stream.mkString(Stream.decodeText(handle.stderr))), Stream.toQueue(Stream.splitLines(Stream.decodeText(handle.getOutputFd(REPLIES))), { capacity: 'unbounded' })]);
    const stopped = Effect.promise(() => killProcessTreeGraceful(handle.pid, 'SIGKILL'));
    const next = Effect.timeoutOrElse(Effect.asSome(Effect.option(Queue.take(replies))), { duration: deadline, orElse: () => Effect.as(stopped, Option.none<Option.Option<string>>()) });
    const taken = yield* Effect.forEach(asked, (request) => Effect.map(next, (line) => [request, line] as const));
    const [read, unread] = Array.partition(taken, ([request, line]) =>
        Result.fromOption(
            Option.map(Option.flatten(line), (text) => [request, text] as const),
            () => [request, Option.isNone(line)] as const,
        ),
    );
    const decoded = Schema.decodeResult(reply);
    const settled = read.map(
        ([request, text]) =>
            [
                request,
                Result.gen(function* () {
                    const { loaded, outcome } = yield* Result.mapError(decoded(text), ToolError.of(request.file, 'got a reply that does not decode'));
                    return { report: yield* outcome, loaded };
                }),
            ] as const,
    );
    const ended = Effect.map(Effect.all([Effect.result(handle.exitCode), Effect.orElseSucceed(Fiber.join(stderr), () => '')]), ([code, output]) => [['ended the reader', ...Result.match(code, { onFailure: () => [], onSuccess: (exited) => [`with exit ${exited}`] })].join(' '), output.trim()].join(': '));
    const unanswered = yield* Array.match(unread, {
        onEmpty: () => Effect.succeed([]),
        onNonEmpty: Effect.fnUntraced(function* ([[first, silent], ...rest]) {
            const reasons = [[first, silent ? `sent no reply within ${Duration.format(deadline)}` : yield* ended] as const, ...rest.map(([request]) => [request, `not read, the reader stopped at ${first.file}`] as const)];
            return reasons.map(([request, detail]) => [request, Result.fail(ToolError.make({ subject: request.file, detail }))] as const);
        }),
    });
    return [...settled, ...unanswered];
}, Effect.scoped);

const consulted = <J extends Job, A, O extends typeof Timed.Type>({
    entry,
    job,
    report,
    launch,
    jobs,
}: {
    readonly entry: string;
    readonly job: J;
    readonly report: Schema.Codec<A, A>;
    readonly launch: (job: J['Type']) => Readonly<Record<string, string>>;
    readonly jobs: (request: Request<O>) => readonly J['Type'][];
}) => {
    const [protocol, reply] = [Jobs(job), Reply(report)];
    return {
        units: Effect.fnUntraced(function* (request: Request<O>) {
            const { workspaceRoot } = request.context;
            const stated = jobs(request);
            const names = yield* listed(workspaceRoot, Array.dedupe(stated.map(({ root }) => root)));
            const chains = yield* chained(workspaceRoot, [...names.keys()]);
            const [ci, lock] = [isCI(), getLockFileName(detectPackageManager(workspaceRoot))];
            const kept = stated.filter(({ root }) => names.has(root));
            return kept.map(
                (each): Asked<J> => ({
                    key: each.file,
                    file: each.file,
                    job: each,
                    governed: [request.path.join(each.root, MANIFESTS), lock, ...(chains.get(each.root) ?? [])],
                    chain: (chains.get(each.root) ?? []).filter((at) => !at.startsWith(`${each.root}/`)),
                    salt: hashObject({ each, launch: launch(each), ci, names: names.get(each.root) }),
                }),
            );
        }),
        read: Effect.fnUntraced(function* (missing: Array.NonEmptyReadonlyArray<Asked<J>>, request: Request<O>) {
            const { workspaceRoot } = request.context;
            const runs = Object.values(Array.groupBy(missing, ({ job: each }) => hashObject(launch(each))));
            const replies = yield* Effect.forEach(runs, (run) => answered(entry, protocol, reply, workspaceRoot, launch(Array.headNonEmpty(run).job), request.options.deadline, run), { concurrency: 'unbounded' });
            const read = new Map(replies.flat().map(([asked, outcome]) => [asked.key, outcome] as const));
            return yield* Effect.forEach(
                missing,
                (asked): Effect.Effect<Result.Result<Reading<Answer<A>>, Error>> =>
                    Result.match(read.get(asked.key) ?? Result.fail(ToolError.make({ subject: asked.file, detail: 'was not read' })), {
                        onFailure: (error) => Effect.succeed(Result.fail<Error>(error)),
                        onSuccess: ({ report: value, loaded: named }) =>
                            Effect.map(
                                Effect.promise(() => globWithWorkspaceContext(workspaceRoot, [asked.file, ...named])),
                                (loaded) => Result.succeed<Reading<Answer<A>>>({ report: { report: value, loaded } }),
                            ),
                    }),
                { concurrency: 'unbounded' },
            );
        }),
        sources: ({ loaded }: Answer<A>, { file, governed }: Asked<J>) => ({ tracked: [file, ...governed, ...loaded] }),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Answer, type Asked, consulted, Timed };
