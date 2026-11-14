import type { Stacks } from '@rasm/nx-pulumi/stacks';
import { Cause, Effect, type FileSystem, Match, Option, type Path, Record, Schema, Stream, Struct } from 'effect';
import { ChildProcess, type ChildProcessSpawner } from 'effect/process';
import { hostingStacks } from '#/hosting-stack';
import manifest from '#/package.json' with { type: 'json' };
import { hosts, installed, local, provisioned } from '#/platform-stack';
import { type configs, identified, repositoryStack } from '#/repository-stack';

// --- [MODELS] --------------------------------------------------------------------------

const Deployed = Schema.fromJsonString(Schema.Struct(Struct.Record(['CLOUDFLARE_ACCOUNT_ID', 'CLOUDFLARE_API_TOKEN', 'DOPPLER_IAC_TOKEN', 'EPIC_GHCR_TOKEN', 'GITHUB_TOKEN', 'PULUMI_ACCESS_TOKEN', 'TAILSCALE_OAUTH_CLIENT_ID', 'TAILSCALE_OAUTH_CLIENT_SECRET'], Schema.NonEmptyString)));

// --- [OPERATIONS] ----------------------------------------------------------------------

const deployed = Effect.gen(function* () {
    const handle = yield* ChildProcess.make('doppler', ['secrets', 'download', '--no-file', '--format', 'json', '--config', 'deploy' satisfies (typeof configs)[number]], { stdin: 'ignore' });
    const [stdout, stderr, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
    return yield* code === 0 ? Schema.decodeEffect(Deployed)(stdout) : new Cause.UnknownError(stderr, `doppler exited ${code}: ${stderr}`);
}).pipe(Effect.scoped);

// --- [COMPOSITION] ---------------------------------------------------------------------

const stacks: Stacks = Effect.fnUntraced(function* (request) {
    const [{ context }, services] = [request, yield* Effect.context<FileSystem.FileSystem | Path.Path | ChildProcessSpawner.ChildProcessSpawner>()];
    const { envVars, deploy } = yield* Match.valueTags(request, {
        Install: () => Effect.succeed({ envVars: { PULUMI_BACKEND_URL: 'file://~', PULUMI_CONFIG_PASSPHRASE: '' }, deploy: Option.none() }),
        Deploy: ({ production }) => Effect.map(deployed, (rows) => ({ envVars: { ...rows, DOPPLER_TOKEN: rows.DOPPLER_IAC_TOKEN }, deploy: Option.some({ production, rows }) })),
    });
    const groups =
        context.projectName === manifest.name
            ? yield* Option.match(deploy, {
                  onNone: () => Effect.succeed([[{ stackName: local.host, program: installed }]]),
                  onSome: Effect.fnUntraced(function* ({ rows }) {
                      const identity = yield* identified(context.root);
                      return [[{ stackName: identity.project, program: repositoryStack(context.root, identity, rows) }], Record.collect(hosts, (host) => ({ stackName: host, program: provisioned(host, { ...identity, registry: rows.EPIC_GHCR_TOKEN }) }))];
                  }),
              })
            : [yield* hostingStacks(context, deploy)];
    return groups.map((group) => group.map(({ stackName, program }) => ({ stackName, envVars, projectName: manifest.name.replace('@', '').replace('/', '-'), program: () => Effect.runPromiseWith(services)<Readonly<Record<string, unknown>>, Effect.Error<typeof program>>(program) })));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { stacks };
