#!/usr/bin/env node
import { relative } from 'node:path';
import process from 'node:process';
import { parseArgs } from 'node:util';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { readCachedProjectGraph, workspaceRoot } from '@nx/devkit';
import { automation } from '@pulumi/pulumi';
import { run } from '@rasm/process';
import { Array, Cause, Effect, Exit, FileSystem, flow, Option, type Path, Record, Schema, Struct, UndefinedOr } from 'effect';
import type { ChildProcessSpawner } from 'effect/process';
import { hostingStacks } from '#/hosting-stack';
import manifest from '#/package.json' with { type: 'json' };
import { hosts, installed, local, provisioned } from '#/platform-stack';
import { type configs, identified, repositoryStack } from '#/repository-stack';

// --- [CONSTANTS] -----------------------------------------------------------------------

const OPERATIONS = { up: 'was not applied', preview: 'was not previewed', destroy: 'was not destroyed' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Deployed = Schema.fromJsonString(Schema.Struct(Struct.Record(['CLOUDFLARE_ACCOUNT_ID', 'CLOUDFLARE_API_TOKEN', 'DOPPLER_IAC_TOKEN', 'EPIC_GHCR_TOKEN', 'GITHUB_TOKEN', 'HOSTINGER_API_TOKEN', 'PULUMI_ACCESS_TOKEN', 'TAILSCALE_OAUTH_CLIENT_ID', 'TAILSCALE_OAUTH_CLIENT_SECRET'], Schema.NonEmptyString)));
const Request = Schema.Struct({
    verb: Schema.Literals(['install', 'deploy']).annotate({ description: 'install applies the stacks of this machine in a local backend, deploy the stacks of the repository, its machines, and the hosted apps' }),
    operation: Schema.Literals(Struct.keys(OPERATIONS)).annotate({ description: 'up applies each stack, preview prints the changes an apply makes and makes none, destroy destroys and removes it' }),
    stack: Schema.optionalKey(Schema.String).annotate({ description: 'Name of the one stack to run, absent runs every stack in dependency order' }),
    production: Schema.Boolean.annotate({ description: 'Deploys the stacks of a released version' }),
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const deployed = run(['doppler', 'secrets', 'download', '--no-file', '--format', 'json', '--config', 'deploy' satisfies (typeof configs)[number]]).pipe(Effect.flatMap(({ stdout }) => Schema.decodeEffect(Deployed)(stdout)));

const requested = Effect.flatMap(
    Effect.try({
        try: () => parseArgs({ args: process.argv.slice(2), options: { operation: { type: 'string', default: 'up' }, stack: { type: 'string' }, production: { type: 'boolean', default: false } }, allowPositionals: true }),
        catch: (cause) => new Cause.UnknownError(cause, `${manifest.name} reads <install|deploy> [--operation=up|preview|destroy] [--stack=<name>] [--production]`),
    }),
    ({ positionals: [verb], values }) => Schema.decodeUnknownEffect(Request)({ verb, ...values }),
);

const grouped = Effect.fnUntraced(function* ({ verb, production }: typeof Request.Type) {
    const deploy = verb === 'deploy' ? Option.some({ production, rows: yield* deployed }) : Option.none();
    const envVars = Option.match(deploy, { onNone: () => ({ PULUMI_BACKEND_URL: 'file://~', PULUMI_CONFIG_PASSPHRASE: '' }), onSome: ({ rows }) => ({ ...rows, DOPPLER_TOKEN: rows.DOPPLER_IAC_TOKEN }) });
    const root = relative(workspaceRoot, process.cwd()) || '.';
    const own = root === (relative(workspaceRoot, import.meta.dirname) || '.');
    const hosted = Effect.fnUntraced(function* () {
        const graph = yield* Effect.try({ try: readCachedProjectGraph, catch: (cause) => new Cause.UnknownError(cause, `${root} has no cached Nx project graph, which an nx run writes`) });
        const node = yield* Effect.fromOption(
            Record.findFirst(graph.nodes, ({ data }) => data.root === root),
            () => new Cause.NoSuchElementError(`No Nx project sits at ${root}`),
        );
        return [yield* hostingStacks(node[1], deploy)];
    });
    const groups = own
        ? yield* Option.match(deploy, {
              onNone: () => Effect.succeed([[{ stackName: local.host, program: installed }]]),
              onSome: Effect.fnUntraced(function* ({ rows }) {
                  const identity = yield* identified(workspaceRoot);
                  return [[{ stackName: identity.project, program: repositoryStack(workspaceRoot, identity, rows) }], Record.collect(hosts, (host) => ({ stackName: host, program: provisioned(host, { ...identity, registry: rows.EPIC_GHCR_TOKEN }) }))];
              }),
          })
        : yield* hosted();
    return { envVars, groups };
});

const applied = Effect.fnUntraced(function* (request: typeof Request.Type) {
    const [fs, services] = yield* Effect.all([FileSystem.FileSystem, Effect.context<FileSystem.FileSystem | Path.Path | ChildProcessSpawner.ChildProcessSpawner>()]);
    const { envVars, groups } = yield* grouped(request);
    const projectName = manifest.name.replace('@', '').replace('/', '-');
    const selected = UndefinedOr.match(request.stack, { onUndefined: () => groups, onDefined: (name) => groups.map((group) => group.filter(({ stackName }) => stackName === name)) });
    const absent = UndefinedOr.match(request.stack, { onUndefined: () => 'no stack', onDefined: (name) => `no stack named ${name}` });
    yield* Array.isReadonlyArrayNonEmpty(selected.flat())
        ? Effect.void
        : Effect.fail(
              new Cause.NoSuchElementError(
                  `${manifest.name} runs ${absent} for ${process.cwd()}, its stacks are ${groups
                      .flat()
                      .map(({ stackName }) => stackName)
                      .join(', ')}`,
              ),
          );
    const one = Effect.fnUntraced(function* ({ stackName, program }: (typeof groups)[number][number]) {
        const workDir = yield* fs.makeTempDirectoryScoped();
        const printed = (text: string) => process.stderr.write(text.replaceAll(/^(?=.)/gmu, `${stackName} | `));
        const failed = (cause: unknown) => new Cause.UnknownError(cause, `${stackName} ${OPERATIONS[request.operation]}`);
        const inline = () => Effect.runPromiseWith(services)<Readonly<Record<string, unknown>>, Effect.Error<typeof program>>(program);
        const stack = yield* Effect.tryPromise({ try: () => automation.LocalWorkspace.createOrSelectStack({ stackName, projectName, program: inline }, { workDir, envVars, projectSettings: { name: projectName, runtime: 'nodejs', main: import.meta.dirname } }), catch: failed });
        yield* Effect.tryPromise({
            try: (signal): Promise<unknown> => {
                const handlers = { signal, onOutput: printed, onError: printed, color: 'never', refresh: true, runProgram: true } as const;
                return { up: () => stack.up(handlers), preview: () => stack.preview(handlers), destroy: () => stack.destroy({ ...handlers, remove: true }) }[request.operation]();
            },
            catch: failed,
        });
    }, Effect.scoped);
    yield* Effect.forEach(selected, (group) => Effect.flatMap(Effect.forEach(group, flow(one, Effect.exit), { concurrency: 'unbounded' }), Exit.asVoidAll), { discard: true });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

if (import.meta.main) {
    runMain(Effect.provide(Effect.flatMap(requested, applied), layer));
}
