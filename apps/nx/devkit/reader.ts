import { execPath } from 'node:process';
import { fileURLToPath } from 'node:url';
import { detectPackageManager, hashArray, normalizePath } from '@nx/devkit';
import { getLockFileName, globWithWorkspaceContext, hashObject, hashWithWorkspaceContext, killProcessTreeGraceful } from '@nx/devkit/internal';
import { type RawTsconfigJsonCache, walkTsconfigExtendsChain } from '@nx/js/internal';
import { Array, Duration, Effect, Fiber, Option, Path, Queue, Result, Schema, Stream } from 'effect';
import { ancestors, CONFIG, cached } from '#/inference';
import { spawned, ToolError } from '#/process';
import { type Job, Jobs, REPLIES, Reply } from '#/reply';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MANIFEST = 'package.json';

// --- [MODELS] --------------------------------------------------------------------------

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
    return roots.map((root) => {
        const files = Option.match(
            Array.findFirst(governing(root), (file) => present.has(file)),
            { onNone: () => [], onSome: (config) => [...reached(path.join(workspaceRoot, config), new Set())] },
        );
        return [root, files.map((at) => normalizePath(path.relative(workspaceRoot, at))).filter((at) => !(at.startsWith('..') || at.split('/').includes('node_modules')))] as const;
    });
});

const consulted = Effect.fnUntraced(function* <J extends Job, A, I, T extends { readonly each: J['Type'] }>(entry: string, jobs: ReturnType<typeof Jobs<J>>, reply: ReturnType<typeof Reply<A, I>>, workspaceRoot: string, launch: Readonly<Record<string, string>>, deadline: Duration.Duration, asked: readonly T[]) {
    const input = yield* Effect.orDie(Schema.encodeEffect(jobs)(asked.map(({ each }) => each)));
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
    const answered = read.map(
        ([request, text]) =>
            [
                request,
                Result.gen(function* () {
                    const { loaded, outcome } = yield* Result.mapError(decoded(text), ToolError.of(request.each.file, 'got a reply that does not decode'));
                    return { report: yield* outcome, loaded };
                }),
            ] as const,
    );
    const ended = Effect.map(Effect.all([Effect.result(handle.exitCode), Effect.orElseSucceed(Fiber.join(stderr), () => '')]), ([code, output]) => [['ended the reader', ...Result.match(code, { onFailure: () => [], onSuccess: (exited) => [`with exit ${exited}`] })].join(' '), output.trim()].join(': '));
    const unanswered = yield* Array.match(unread, {
        onEmpty: () => Effect.succeed([]),
        onNonEmpty: Effect.fnUntraced(function* ([[first, silent], ...rest]) {
            const reasons = [[first, silent ? `sent no reply within ${Duration.format(deadline)}` : yield* ended] as const, ...rest.map(([request]) => [request, `not read, the reader stopped at ${first.each.file}`] as const)];
            return reasons.map(([request, detail]) => [request, Result.fail(ToolError.make({ subject: request.each.file, detail }))] as const);
        }),
    });
    return [...answered, ...unanswered];
}, Effect.scoped);

const resolved = Effect.fn('resolved')(function* <J extends Job, A, I>({
    entry,
    job,
    report,
    launch,
    plugin,
    options,
    workspaceRoot,
    jobs,
    deadline,
}: {
    readonly entry: string;
    readonly job: J;
    readonly report: Schema.Codec<A, I>;
    readonly launch: (job: J['Type']) => Readonly<Record<string, string>>;
    readonly plugin: { readonly name: string; readonly digest: string };
    readonly options: unknown;
    readonly workspaceRoot: string;
    readonly jobs: readonly J['Type'][];
    readonly deadline: Duration.Duration;
}) {
    const path = yield* Path.Path;
    const [protocol, reply, stored] = [Jobs(job), Reply(report), Schema.toCodecJson(report)];
    const [cache, lock, chains] = yield* Effect.all([
        cached<{ readonly report: unknown; readonly loaded: readonly string[] }>(plugin, options),
        Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [getLockFileName(detectPackageManager(workspaceRoot))])),
        chained(workspaceRoot, Array.dedupe(jobs.map(({ root }) => root))),
    ]);
    const rooted = new Map(
        yield* Effect.forEach(
            chains,
            Effect.fnUntraced(function* ([root, chain]) {
                const files = yield* Effect.promise(() => globWithWorkspaceContext(workspaceRoot, [path.join(root, '**')]));
                const nested = files.filter((file) => path.basename(file) === MANIFEST && path.dirname(file) !== root).map((file) => `${path.dirname(file)}/`);
                const own = (file: string) => !nested.some((folder) => file.startsWith(folder));
                return [root, { chain, names: hashArray(files.filter(own)) }] as const;
            }),
            { concurrency: 'unbounded' },
        ),
    );
    const restored = ({ report: encoded, loaded }: { readonly report: unknown; readonly loaded: readonly string[] }) => Option.map(Schema.decodeUnknownOption(stored)(encoded), (value) => ({ report: value, loaded }));
    const held = yield* Effect.forEach(
        jobs,
        Effect.fnUntraced(function* (each) {
            const { chain, names } = yield* Effect.orDie(Effect.fromNullishOr(rooted.get(each.root)));
            const digest = (loaded: readonly string[]) =>
                Effect.map(
                    Effect.promise(() => hashWithWorkspaceContext(workspaceRoot, [each.file, path.join(each.root, MANIFEST), ...chain, ...loaded])),
                    (content) => hashArray([plugin.digest, lock, names, content]),
                );
            const key = hashObject({ each, launch: launch(each) });
            return { each, key, digest, chain: chain.filter((at) => !at.startsWith(`${each.root}/`)), kept: Option.flatMap(yield* cache.get(key, ({ loaded }) => digest(loaded)), restored) };
        }),
        { concurrency: 'unbounded' },
    );
    const kept = Effect.fnUntraced(
        function* ([request, result]: readonly [(typeof held)[number], Result.Result<{ readonly report: A; readonly loaded: readonly string[] }, ToolError>]) {
            const { report: value, loaded: named } = yield* Effect.fromResult(result);
            const loaded = yield* Effect.promise(() => globWithWorkspaceContext(workspaceRoot, [request.each.file, ...named]));
            const [encoded, now] = yield* Effect.all([Effect.orDie(Schema.encodeEffect(stored)(value)), request.digest(loaded)]);
            yield* cache.set(request.key, { report: encoded, loaded }, now);
            return { report: value, loaded };
        },
        (effect, [request]) => Effect.map(Effect.result(effect), (outcome) => [request, outcome] as const),
    );
    const runs = Object.values(
        Array.groupBy(
            held.filter((request) => Option.isNone(request.kept)),
            ({ each }) => hashObject(launch(each)),
        ),
    );
    const fresh = yield* Effect.forEach(
        runs,
        (run) =>
            Effect.flatMap(
                consulted(entry, protocol, reply, workspaceRoot, launch(Array.headNonEmpty(run).each), deadline, run),
                Effect.forEach((pair) => kept(pair)),
            ),
        { concurrency: 'unbounded' },
    );
    const read = new Map(fresh.flat());
    const settled = Array.getSomes(
        held.map((request) =>
            Option.map(
                Option.orElse(Option.map(request.kept, Result.succeed), () => Option.fromUndefinedOr(read.get(request))),
                (outcome) => [request, outcome] as const,
            ),
        ),
    );
    const [answers, failures] = Array.partition(settled, ([{ each, chain }, outcome]) => Result.mapBoth(outcome, { onSuccess: (answer) => ({ job: each, chain, ...answer }), onFailure: (error) => [each.file, error] as const }));
    return { answers, failures };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { resolved };
