import { dried, executor, spawned } from '@rasm/nx';
import { Array, Cause, DateTime, Effect, FileSystem, Option, Order, Path, Result, Schema, SemVer, Struct } from 'effect';
import { commands } from '#/executors/run/executor';
import { MANIFEST } from '#/swift-package';

// --- [MODELS] --------------------------------------------------------------------------

const Upgrade = Schema.Struct({
    package: Schema.String.annotate({ description: 'Package folder the plugin infers, passed as --package-path' }),
    dryRun: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Prints each move and makes none, passing --dry-run to swift package update, as NX_DRY_RUN=true does' }),
}).annotate({
    title: 'Upgrade',
    description:
        "Moves the lower bound of each remote requirement of Package.swift whose range ends at the next major, the range from: states, to the highest semver tag git ls-remote lists for its repository, prereleases included, among the tags published at or after the bound's own tag, as SwiftPM admits a prerelease only into a range with a prerelease bound, then moves Package.resolved with swift package update",
});
const Dumped = Schema.fromJsonString(
    Schema.Struct({
        dependencies: Schema.Array(
            Schema.Struct({
                sourceControl: Schema.Array(
                    Schema.Struct({
                        location: Schema.Struct({ remote: Schema.Array(Schema.Struct({ urlString: Schema.String })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) }),
                        requirement: Schema.Struct({ range: Schema.Array(Schema.Struct({ lowerBound: Schema.SemVerFromString, upperBound: Schema.SemVerFromString })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) }),
                    }),
                ).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
            }),
        ),
    }),
);
const Published = Schema.Array(Schema.Tuple([Schema.String, Schema.DateTimeUtcFromString]));

// --- [OPERATIONS] ----------------------------------------------------------------------

const dated = Effect.fnUntraced(function* (url: string, tags: Array.NonEmptyReadonlyArray<string>, cwd: string) {
    const bare = yield* (yield* FileSystem.FileSystem).makeTempDirectoryScoped();
    yield* spawned(['git', 'init', '--quiet', '--bare', bare], { cwd });
    yield* spawned(['git', '-C', bare, 'fetch', '--quiet', '--depth=1', '--filter=tree:0', '--no-tags', url, ...tags.map((tag) => `refs/tags/${tag}:refs/tags/${tag}`)], { cwd });
    const listed = yield* spawned(['git', '-C', bare, 'for-each-ref', '--format=%(refname:strip=2)%09%(creatordate:iso-strict)', 'refs/tags'], { cwd });
    return new Map(
        yield* Schema.decodeUnknownEffect(Published)(
            listed
                .trim()
                .split('\n')
                .map((line) => line.split('\t')),
        ),
    );
}, Effect.scoped);

const newest = Effect.fnUntraced(function* (url: string, lowerBound: SemVer.SemVer, cwd: string) {
    const listed = yield* spawned(['git', 'ls-remote', '--tags', '--refs', url], { cwd });
    const tags = Array.getSomes(Array.fromIterable(listed.matchAll(/refs\/tags\/(?<tag>\S+)$/gmu)).map(({ groups: { tag = '' } = {} }) => Option.map(Result.getSuccess(SemVer.parse(tag.replace(/^v/u, ''))), (version) => ({ tag, version }))));
    const current = tags.filter(({ version }) => SemVer.equals(version, lowerBound));
    const later = tags.filter(({ version }) => SemVer.isGreaterThan(version, lowerBound));
    const published = yield* Array.isReadonlyArrayNonEmpty(later) ? dated(url, Array.map(Array.appendAll(later, current), Struct.get('tag')), cwd) : Effect.succeed(new Map<string, DateTime.Utc>());
    const since = Option.flatMap(Array.head(current), ({ tag }) => Option.fromUndefinedOr(published.get(tag)));
    const eligible = Array.getSomes(later.map((held) => Option.map(Option.fromUndefinedOr(published.get(held.tag)), (at) => ({ ...held, at })))).filter(({ at }) => Option.match(since, { onNone: () => true, onSome: (held) => DateTime.isGreaterThanOrEqualTo(at, held) }));
    return Option.map(Option.liftPredicate(eligible, Array.isReadonlyArrayNonEmpty<(typeof eligible)[number]>), (held) => Array.max(held, Order.mapInput(SemVer.Order, Struct.get('version'))));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(Upgrade, function* ({ package: root, dryRun }, context) {
    const [fs, path, dry] = yield* Effect.all([FileSystem.FileSystem, Path.Path, dried(dryRun)]);
    const file = path.join(context.root, root, MANIFEST);
    const { dependencies } = yield* Effect.flatMap(spawned(['swift', 'package', '--package-path', root, 'dump-package'], { cwd: context.root }), Schema.decodeEffect(Dumped));
    const requirements = dependencies
        .flatMap(Struct.get('sourceControl'))
        .flatMap(({ location, requirement }) => Array.cartesianWith(location.remote, requirement.range, ({ urlString }, range) => ({ url: urlString, ...range })))
        .filter(({ lowerBound, upperBound }) => SemVer.equals(upperBound, SemVer.bumpMajor(SemVer.release(lowerBound))));
    const moves = Array.getSomes(
        yield* Effect.forEach(
            requirements,
            (requirement) =>
                Effect.map(
                    newest(requirement.url, requirement.lowerBound, context.root),
                    Option.map((head) => ({ ...requirement, head })),
                ),
            { concurrency: 'unbounded' },
        ),
    );
    const source = yield* fs.readFileString(file);
    const rewritten = yield* Effect.reduce(
        moves,
        () => source,
        (text, { url, lowerBound, head }) => {
            const bound = new RegExp(`(?<call>${RegExp.escape(JSON.stringify(url))}[^)]*?)${RegExp.escape(JSON.stringify(SemVer.format(lowerBound)))}`, 'u');
            return bound.test(text) ? Effect.succeed(text.replace(bound, (_, call: string) => `${call}${JSON.stringify(SemVer.format(head.version))}`)) : Effect.fail(new Cause.UnknownError(url, `${file} states no "${SemVer.format(lowerBound)}" lower bound inside the .package call naming "${url}"`));
        },
    );
    yield* Effect.forEach(moves, ({ url, lowerBound, head }) => Effect.logInfo(`${url} ${SemVer.format(lowerBound)} -> ${SemVer.format(head.version)} (tag ${head.tag}, published ${DateTime.formatIso(head.at)}) in ${file}`), { discard: true });
    yield* dry || moves.length === 0 ? Effect.void : fs.writeFileString(file, rewritten);
    return yield* commands({ runs: [['swift', 'package', '--package-path', root, 'update', ...(dry ? ['--dry-run'] : [])]] }, context.root);
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Upgrade };
