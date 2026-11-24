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

// --- [MODELS] --------------------------------------------------------------------------

const Rows = <const K extends string>(keys: readonly K[]) => Schema.fromJsonString(Schema.StructWithRest(Schema.Struct(Struct.Record(keys, Schema.NonEmptyString)), [Schema.Record(Schema.String, Schema.String)]));
const LOCAL = ['AUTHELIA_OWNER_PASSWORD_HASH', 'AUTHELIA_SESSION_SECRET', 'AUTHELIA_STORAGE_ENCRYPTION_KEY', 'RUSTFS_ACCESS_KEY', 'RUSTFS_SECRET_KEY'] as const;
const Installed = Rows(LOCAL);
const Deployed = Rows([...LOCAL, 'ATUIN_PASSWORD', 'CLOUDFLARE_ACCOUNT_ID', 'DOPPLER_IAC_TOKEN', 'EPIC_GHCR_TOKEN', 'GITHUB_TOKEN', 'NX_SELF_HOSTED_REMOTE_CACHE_ACCESS_TOKEN', 'NX_SELF_HOSTED_REMOTE_CACHE_READ_TOKEN', 'TAILSCALE_ENROLMENT_OAUTH_SECRET']);
const Request = Schema.Struct({ positionals: Schema.Tuple([Schema.Literals(['install', 'deploy'])]), operation: Schema.Literals(['up', 'preview', 'destroy']), stack: Schema.optionalKey(Schema.String), production: Schema.Boolean, expectNoChanges: Schema.Boolean }).pipe(
    Schema.encodeKeys({ expectNoChanges: 'expect-no-changes' }),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = (config: (typeof configs)[number]) => Effect.map(run(['doppler', 'secrets', 'download', '--no-file', '--format', 'json', '--config', config]), Struct.get('stdout'));

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const { positionals, operation, stack, production, expectNoChanges } = yield* Effect.try({
        try: () => parseArgs({ args: process.argv.slice(2), options: { operation: { type: 'string', default: 'up' }, stack: { type: 'string' }, production: { type: 'boolean', default: false }, 'expect-no-changes': { type: 'boolean', default: false } }, allowPositionals: true }),
        catch: (cause) => new Cause.UnknownError(cause, `${manifest.name} reads <install|deploy> [--operation=up|preview|destroy] [--stack=<name>] [--production] [--expect-no-changes]`),
    }).pipe(Effect.flatMap((parsed) => Schema.decodeUnknownEffect(Request)({ ...parsed.values, positionals: parsed.positionals })));
    const [fs, services] = yield* Effect.all([FileSystem.FileSystem, Effect.context<FileSystem.FileSystem | Path.Path | ChildProcessSpawner.ChildProcessSpawner>()]);
    const deploy = positionals[0] === 'deploy' ? Option.some({ production, rows: yield* Effect.flatMap(read('deploy'), Schema.decodeEffect(Deployed)) }) : Option.none();
    const root = relative(workspaceRoot, process.cwd()) || '.';
    const programs = Option.match(deploy, {
        onNone: () => Effect.map(Effect.flatMap(read('dev'), Schema.decodeEffect(Installed)), (rows) => [[{ stackName: local.host, program: installed(rows) }]]),
        onSome: ({ rows }) => Effect.map(identified, (identity) => [[{ stackName: identity.project, program: repositoryStack(identity, rows) }], Record.collect(hosts, (host) => ({ stackName: host, program: provisioned(host, identity, rows) }))]),
    });
    const hosted = Effect.try({ try: () => Record.findFirst(readCachedProjectGraph().nodes, ({ data }) => data.root === root), catch: (cause) => new Cause.UnknownError(cause, `${root} has no cached Nx project graph, which an nx run writes`) }).pipe(
        Effect.flatMap((found) => Effect.fromOption(found, () => new Cause.NoSuchElementError(`No Nx project sits at ${root}`))),
        Effect.flatMap(([, node]) => hostingStacks(node, deploy)),
        Effect.map(Array.of),
    );
    const groups = yield* process.cwd() === import.meta.dirname ? programs : hosted;
    const selected = groups.map((group) => group.filter(({ stackName }) => stack === undefined || stack === stackName));
    yield* Effect.when(
        new Cause.NoSuchElementError(`${manifest.name} runs ${UndefinedOr.match(stack, { onUndefined: () => 'no stack', onDefined: (name) => `no stack named ${name}` })} for ${process.cwd()}, its stacks are ${groups.flat().map(Struct.get('stackName')).join(', ')}`),
        Effect.succeed(selected.flat().length === 0),
    );
    yield* Effect.forEach(
        operation === 'destroy' ? selected.toReversed() : selected,
        flow(
            Effect.forEach(
                Effect.fnUntraced(
                    function* ({ stackName, program }: (typeof groups)[number][number]) {
                        const workDir = yield* fs.makeTempDirectoryScoped();
                        const failed = (cause: unknown) => new Cause.UnknownError(cause, `${stackName} ${operation} failed`);
                        const handlers = { color: 'never', ...Struct.Record(['onOutput', 'onError'], (text: string) => process.stderr.write(text.replaceAll(/^(?=.)/gmu, `${stackName} | `))) } as const;
                        const converged = { ...handlers, refresh: true, runProgram: true, expectNoChanges };
                        const settings = {
                            program: () => Effect.runPromiseWith(services)<Readonly<Record<string, unknown>>, Effect.Error<typeof program>>(program),
                            workDir,
                            envVars: Option.match(deploy, { onNone: () => ({ PULUMI_BACKEND_URL: 'file://~', PULUMI_CONFIG_PASSPHRASE: '' }), onSome: ({ rows }) => ({ ...rows, DOPPLER_TOKEN: rows.DOPPLER_IAC_TOKEN }) }),
                            projectSettings: { name: manifest.name.replace('@', '').replace('/', '-'), runtime: 'nodejs', main: import.meta.dirname },
                        } satisfies automation.LocalWorkspaceOptions;
                        const workspace = yield* Effect.tryPromise({ try: () => automation.LocalWorkspace.create(settings), catch: failed });
                        const running = yield* Effect.tryPromise({ try: () => (operation === 'up' ? automation.Stack.createOrSelect : automation.Stack.select)(stackName, workspace), catch: failed });
                        yield* Effect.tryPromise({ try: (signal): Promise<unknown> => ({ up: () => running.up({ ...converged, signal }), preview: () => running.preview({ ...converged, signal, diff: true }), destroy: () => running.destroy({ ...handlers, signal, remove: true }) })[operation](), catch: failed });
                    },
                    Effect.scoped,
                    Effect.exit,
                ),
                { concurrency: 'unbounded' },
            ),
            Effect.flatMap(Exit.asVoidAll),
        ),
        { discard: true },
    );
}).pipe(Effect.provide(layer), runMain());
