import type { ExecutorContext } from '@nx/devkit';
import { executor, Released, released } from '@rasm/nx-devkit/executor';
import { printed, ToolError } from '@rasm/nx-devkit/process';
import { Array, Config, Effect, FileSystem, Option, Path, Redacted, Schema, String, Struct } from 'effect';
import { CommandLine } from 'effect/process';
import { VersionActionsOptions, versioned } from '#/targets';

// --- [MODELS] --------------------------------------------------------------------------

const Publish = Released.mapFields(Struct.assign({ folders: Schema.Array(Schema.String).annotate({ description: 'Folders the release build packages into, relative to the workspace root' }) })).annotate({
    title: 'Release publish',
    description: "Attaches each folder's disk image, else its zip, at the version nx release set to the project's GitHub release; where SPARKLE_ED_PRIVATE_KEY is set, adds each disk image holding a stapled notarization ticket to the Sparkle feed its app names in SUFeedURL",
});
const Channel = Schema.fromJsonString(Schema.Struct({ assets: Schema.Array(Schema.Struct({ name: Schema.String })) }));
const Feed = Schema.Tuple([Schema.Literal(''), Schema.String, Schema.String, Schema.Literal('releases'), Schema.Literal('download'), Schema.String, Schema.String]);

// --- [OPERATIONS] ----------------------------------------------------------------------

const appcast = Effect.fnUntraced(
    function* (image: string, feed: string, key: Redacted.Redacted, tag: string, cwd: string, dry: boolean) {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const [, owner, repo, , , channel, asset] = yield* Effect.mapError(Schema.decodeUnknownEffect(Feed)(new URL(feed).pathname.split('/')), () =>
            ToolError.make({ subject: feed, detail: 'is the SUFeedURL, and the Sparkle feed publishes to a GitHub release asset (https://github.com/<owner>/<repo>/releases/download/<tag>/<file>)' }),
        );
        const repository = ['--repo', `${owner}/${repo}`];
        const existing = yield* Effect.option(Effect.flatMap(printed('gh', ['release', 'view', channel, ...repository, '--json', 'assets'], { cwd }), Schema.decodeEffect(Channel)));
        const staging = yield* fs.makeTempDirectoryScoped();
        const steps: readonly { readonly run: readonly [string, ...string[]]; readonly input?: string }[] = [
            ...(Option.isNone(existing) ? [{ run: ['gh', 'release', 'create', channel, ...repository, '--prerelease', '--title', channel, '--notes', `Sparkle feed ${asset}`] as const }] : []),
            ...(Option.exists(existing, ({ assets }) => assets.some(({ name }) => name === asset)) ? [{ run: ['gh', 'release', 'download', channel, ...repository, '--pattern', asset, '--dir', staging] as const }] : []),
            { run: ['generate_appcast', '--ed-key-file', '-', '--download-url-prefix', `https://github.com/${owner}/${repo}/releases/download/${tag}/`, staging], input: Redacted.value(key) },
            { run: ['gh', 'release', 'upload', channel, ...repository, '--clobber', path.join(staging, asset)] },
        ];
        return yield* dry
            ? Effect.forEach(steps, ({ run }) => Effect.logInfo(`Would run ${CommandLine.display(run)}`), { discard: true })
            : Effect.andThen(
                  fs.copyFile(path.join(cwd, image), path.join(staging, path.basename(image))),
                  Effect.forEach(steps, ({ run: [command, ...args], ...piped }) => printed(command, args, { cwd, ...piped }), { discard: true }),
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
    Effect.fnUntraced(function* ({ folders, ...options }, context, project) {
        const [{ dry, version, unversioned, tagged, attached }, fs, path] = yield* Effect.all([released(options, context, project), FileSystem.FileSystem, Path.Path]);
        if (unversioned) {
            return yield* Effect.as(Effect.logInfo(`Skipped ${project}, nx release left it unversioned`), true);
        }
        const known = yield* Option.match(version, { onSome: Effect.succeed, onNone: () => stated(context, project) });
        if (!(yield* tagged(known))) {
            return yield* Effect.as(Effect.logInfo(`Skipped ${project}, no release tag holds ${known}`), true);
        }
        const listed = yield* Effect.forEach(folders, (folder) => Effect.orElseSucceed(fs.readDirectory(path.join(context.root, folder)), Array.empty<string>));
        const packages = yield* Effect.forEach(Array.zip(folders, listed), ([folder, names]) =>
            Effect.mapBoth(Effect.fromOption(Array.findFirst(['.dmg', '.zip'], (kind) => Array.findFirst(names, String.endsWith(`-${known}${kind}`)))), {
                onFailure: () => ToolError.make({ subject: folder, detail: `holds no disk image or zip at ${known}; run nx build ${project} -c release after nx release versions it` }),
                onSuccess: (name) => ({ file: path.join(folder, name), plists: names.filter((app) => path.extname(app) === '.app').map((app) => path.join(folder, app, 'Contents', 'Info.plist')) }),
            }),
        );
        const tag = yield* attached(
            known,
            packages.map(({ file }) => file),
        );
        const key = yield* Config.option(Config.Redacted('SPARKLE_ED_PRIVATE_KEY'));
        const feeds = yield* Effect.forEach(
            packages.flatMap(({ file, plists }) => plists.map((plist) => ({ file, plist }))),
            ({ file, plist }) =>
                Effect.all({ file: Effect.succeed(file), feed: printed('plutil', ['-extract', 'SUFeedURL', 'raw', '-o', '-', plist], { cwd: context.root }), stapled: printed('xcrun', ['stapler', 'validate', file], { cwd: context.root }) }).pipe(
                    Effect.tapCause((cause) => Effect.logWarning(`Skipped the Sparkle feed for ${file}`, cause)),
                    Effect.option,
                ),
        );
        yield* Effect.transposeOption(Option.map(key, (secret) => Effect.forEach(Array.getSomes(feeds), ({ file, feed }) => appcast(file, feed.trim(), secret, tag, context.root, dry), { discard: true })));
        return true;
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
