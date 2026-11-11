import type { ExecutorContext } from '@nx/devkit';
import { interpolate } from '@nx/devkit/internal';
import { executor, Released, released } from '@rasm/nx-devkit/executor';
import { printed, ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect, FileSystem, Match, Option, Path, Schema, Struct } from 'effect';
import { CommandLine } from 'effect/process';
import { VersionActionsOptions, versioned } from '#/targets';

// --- [MODELS] --------------------------------------------------------------------------

const Publish = Released.mapFields(
    Struct.assign({
        folders: Schema.Array(Schema.String).annotate({ description: 'Folders the release build zips into, relative to the workspace root' }),
        notarized: Schema.Array(Schema.String).annotate({ description: 'Folders holding a notarized, stapled app, whose SUFeedURL names its Sparkle feed', default: [] }),
    }),
).annotate({ title: 'Release publish', description: "Attaches each folder's zip at the version nx release set to the project's GitHub release, then publishes each notarized app naming SUFeedURL to its Sparkle feed" });
const Release = Schema.fromJsonString(Schema.Struct({ isImmutable: Schema.Boolean, assets: Schema.Array(Schema.Struct({ name: Schema.String })) }));
const Feed = Schema.Tuple([Schema.Literal(''), Schema.String, Schema.String, Schema.Literal('releases'), Schema.Literal('download'), Schema.String, Schema.String]);

// --- [OPERATIONS] ----------------------------------------------------------------------

const viewed = (tag: string, cwd: string, repository: readonly string[]) => Effect.option(Effect.flatMap(printed('gh', ['release', 'view', tag, ...repository, '--json', 'isImmutable,assets'], { cwd }), Schema.decodeEffect(Release)));

const appcast = Effect.fnUntraced(
    function* (zip: string, feed: string, tag: string, cwd: string, dry: boolean) {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const [, owner, repo, , , channel, asset] = yield* Effect.mapError(Schema.decodeUnknownEffect(Feed)(new URL(feed).pathname.split('/')), () =>
            ToolError.make({ subject: feed, detail: 'is the SUFeedURL, and the Sparkle feed publishes to a GitHub release asset (https://github.com/<owner>/<repo>/releases/download/<tag>/<file>)' }),
        );
        const repository = ['--repo', `${owner}/${repo}`];
        const existing = yield* viewed(channel, cwd, repository);
        const staging = yield* fs.makeTempDirectoryScoped();
        const steps: readonly (readonly [string, ...string[]])[] = [
            ...(Option.isNone(existing) ? [['gh', 'release', 'create', channel, ...repository, '--prerelease', '--title', channel, '--notes', `Sparkle feed ${asset}`] as const] : []),
            ...(Option.exists(existing, ({ assets }) => assets.some(({ name }) => name === asset)) ? [['gh', 'release', 'download', channel, ...repository, '--pattern', asset, '--dir', staging] as const] : []),
            ['generate_appcast', '--download-url-prefix', `https://github.com/${owner}/${repo}/releases/download/${tag}/`, staging],
            ['gh', 'release', 'upload', channel, ...repository, '--clobber', path.join(staging, asset)],
        ];
        return yield* dry
            ? Effect.forEach(steps, (step) => Effect.logInfo(`Would run ${CommandLine.display(step)}`), { discard: true })
            : Effect.andThen(
                  fs.copyFile(path.join(cwd, zip), path.join(staging, path.basename(zip))),
                  Effect.forEach(steps, ([command, ...args]) => printed(command, args, { cwd }), { discard: true }),
              );
    },
    Effect.scoped,
    Effect.catchTag('PlatformError', ToolError.of('Sparkle feed', 'does not publish')),
);

const stated = Effect.fnUntraced(function* (context: ExecutorContext, project: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const node = yield* Effect.fromNullishOr(context.projectsConfigurations.projects[project]);
    const { manifest } = yield* Schema.decodeUnknownEffect(VersionActionsOptions)(node.release?.version?.versionActionsOptions);
    const at = path.join(node.root, manifest);
    const { version } = yield* Effect.flatMap(fs.readFileString(path.join(context.root, at)), (text) => versioned(context.root, at, text));
    return yield* Effect.mapError(Effect.fromNullishOr(version), () => ToolError.make({ subject: at, detail: 'states no version' }));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(
    Publish,
    Effect.fnUntraced(function* ({ folders, notarized, ...options }, context, project) {
        const [{ dry, version, unversioned }, fs, path] = yield* Effect.all([released(options, project), FileSystem.FileSystem, Path.Path]);
        if (unversioned) {
            return yield* Effect.as(Effect.logInfo(`Skipped ${project}, nx release left it unversioned`), true);
        }
        const known = yield* Option.match(version, { onSome: Effect.succeed, onNone: () => stated(context, project) });
        const pattern = yield* Effect.mapError(Effect.fromNullishOr(context.nxJsonConfiguration.release?.releaseTag?.pattern), () => ToolError.make({ subject: 'nx.json release.releaseTag.pattern', detail: 'is unset, and names the GitHub release each zip attaches to' }));
        const tag = interpolate(pattern, { projectName: project, version: known });
        const listed = yield* Effect.forEach(folders, (folder) => Effect.orElseSucceed(fs.readDirectory(path.join(context.root, folder)), Array.empty<string>));
        const zips = yield* Effect.forEach(Array.zip(folders, listed), ([folder, names]) =>
            Effect.mapBoth(Effect.fromOption(Array.findFirst(names, (name) => name.endsWith(`-${known}.zip`))), {
                onFailure: () => ToolError.make({ subject: folder, detail: `holds no zip at ${known}; run nx build ${project} -c release after nx release versions it` }),
                onSuccess: (name) => ({ folder, zip: path.join(folder, name) }),
            }),
        );
        const release = dry ? Option.none() : yield* viewed(tag, context.root, []);
        const attached = new Set(Option.toArray(release).flatMap(({ assets }) => assets.map(({ name }) => name)));
        yield* Effect.forEach(
            zips,
            ({ zip }) =>
                Match.value({ dry, attached: attached.has(path.basename(zip)), immutable: Option.exists(release, ({ isImmutable }) => isImmutable) }).pipe(
                    Match.when({ dry: true }, () => Effect.logInfo(`Would run gh release upload ${tag} ${zip}`)),
                    Match.when({ attached: true }, () => Effect.logInfo(`Skipped ${zip}, GitHub release ${tag} already holds it`)),
                    Match.when({ immutable: true }, () => ToolError.make({ subject: tag, detail: `is an immutable GitHub release, which takes no asset after nx release publishes it, so ${zip} stays unattached` })),
                    Match.orElse(() => Effect.andThen(printed('gh', ['release', 'upload', tag, zip], { cwd: context.root }), Effect.logInfo(`Attached ${zip} to GitHub release ${tag}`))),
                ),
            { discard: true },
        );
        const exported = zips.filter(({ folder }) => notarized.includes(folder));
        const contents = yield* Effect.forEach(exported, ({ folder }) => fs.readDirectory(path.join(context.root, folder)));
        const apps = Array.zip(exported, contents).flatMap(([{ folder, zip }, names]) => names.filter((name) => path.extname(name) === '.app').map((app) => ({ zip, plist: path.join(folder, app, 'Contents', 'Info.plist') })));
        const feeds = yield* Effect.forEach(apps, ({ zip, plist }) =>
            Effect.map(
                Effect.option(printed('plutil', ['-extract', 'SUFeedURL', 'raw', '-o', '-', plist], { cwd: context.root })),
                Option.map((feed) => ({ zip, feed: feed.trim() })),
            ),
        );
        yield* Effect.forEach(Array.getSomes(feeds), ({ zip, feed }) => appcast(zip, feed, tag, context.root, dry), { discard: true });
        return true;
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
