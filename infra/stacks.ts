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

const Deployed = Schema.fromJsonString(Schema.StructWithRest(Schema.Struct(Struct.Record(['CLOUDFLARE_ACCOUNT_ID', 'DOPPLER_IAC_TOKEN', 'EPIC_GHCR_TOKEN', 'GITHUB_TOKEN'], Schema.NonEmptyString)), [Schema.Record(Schema.String, Schema.String)]));
const Request = Schema.Struct({ positionals: Schema.Tuple([Schema.Literals(['install', 'deploy'])]), operation: Schema.Literals(['up', 'preview', 'destroy']), stack: Schema.optionalKey(Schema.String), production: Schema.Boolean, expectNoChanges: Schema.Boolean }).pipe(
    Schema.encodeKeys({ expectNoChanges: 'expect-no-changes' }),
);

// --- [COMPOSITION] ---------------------------------------------------------------------

runMain(
    Effect.gen(function* () {
        const { positionals, operation, stack, production, expectNoChanges } = yield* Effect.flatMap(
            Effect.try({
                try: () => parseArgs({ args: process.argv.slice(2), options: { operation: { type: 'string', default: 'up' }, stack: { type: 'string' }, production: { type: 'boolean', default: false }, 'expect-no-changes': { type: 'boolean', default: false } }, allowPositionals: true }),
                catch: (cause) => new Cause.UnknownError(cause, `${manifest.name} reads <install|deploy> [--operation=up|preview|destroy] [--stack=<name>] [--production] [--expect-no-changes]`),
            }),
            (parsed) => Schema.decodeUnknownEffect(Request)({ ...parsed.values, positionals: parsed.positionals }),
        );
        const [fs, services] = yield* Effect.all([FileSystem.FileSystem, Effect.context<FileSystem.FileSystem | Path.Path | ChildProcessSpawner.ChildProcessSpawner>()]);
        const deploy = positionals[0] === 'deploy' ? Option.some({ production, rows: yield* Effect.flatMap(run(['doppler', 'secrets', 'download', '--no-file', '--format', 'json', '--config', 'deploy' satisfies (typeof configs)[number]]), ({ stdout }) => Schema.decodeEffect(Deployed)(stdout)) }) : Option.none();
        const root = relative(workspaceRoot, process.cwd()) || '.';
        const groups =
            process.cwd() === import.meta.dirname
                ? yield* Option.match(deploy, {
                      onNone: () => Effect.succeed([[{ stackName: local.host, program: installed }]]),
                      onSome: ({ rows }) => Effect.map(identified, (identity) => [[{ stackName: identity.project, program: repositoryStack(identity, rows) }], Record.collect(hosts, (host) => ({ stackName: host, program: provisioned(host, identity, rows) }))]),
                  })
                : [
                      yield* Effect.try({ try: readCachedProjectGraph, catch: (cause) => new Cause.UnknownError(cause, `${root} has no cached Nx project graph, which an nx run writes`) }).pipe(
                          Effect.flatMap(({ nodes }) =>
                              Effect.fromOption(
                                  Record.findFirst(nodes, ({ data }) => data.root === root),
                                  () => new Cause.NoSuchElementError(`No Nx project sits at ${root}`),
                              ),
                          ),
                          Effect.flatMap(([, node]) => hostingStacks(node, deploy)),
                      ),
                  ];
        const selected = groups.map((group) => group.filter(({ stackName }) => stack === undefined || stack === stackName));
        yield* Effect.filterOrFail(
            Effect.succeed(selected.flat()),
            Array.isReadonlyArrayNonEmpty,
            () => new Cause.NoSuchElementError(`${manifest.name} runs ${UndefinedOr.match(stack, { onUndefined: () => 'no stack', onDefined: (name) => `no stack named ${name}` })} for ${process.cwd()}, its stacks are ${groups.flat().map(Struct.get('stackName')).join(', ')}`),
        );
        yield* Effect.forEach(
            operation === 'destroy' ? selected.toReversed() : selected,
            flow(
                Effect.forEach(
                    Effect.fnUntraced(
                        function* ({ stackName, program }: (typeof groups)[number][number]) {
                            const workDir = yield* fs.makeTempDirectoryScoped();
                            yield* Effect.tryPromise({
                                try: (signal) => {
                                    const handlers = { signal, color: 'never', ...Struct.Record(['onOutput', 'onError'], (text: string) => process.stderr.write(text.replaceAll(/^(?=.)/gmu, `${stackName} | `))) } as const;
                                    const converged = { ...handlers, refresh: true, runProgram: true, expectNoChanges };
                                    return automation.LocalWorkspace.create({
                                        program: () => Effect.runPromiseWith(services)<Readonly<Record<string, unknown>>, Effect.Error<typeof program>>(program),
                                        workDir,
                                        envVars: Option.match(deploy, { onNone: () => ({ PULUMI_BACKEND_URL: 'file://~', PULUMI_CONFIG_PASSPHRASE: '' }), onSome: ({ rows }) => ({ ...rows, DOPPLER_TOKEN: rows.DOPPLER_IAC_TOKEN }) }),
                                        projectSettings: { name: manifest.name.replace('@', '').replace('/', '-'), runtime: 'nodejs', main: import.meta.dirname },
                                    })
                                        .then((workspace) => (operation === 'up' ? automation.Stack.createOrSelect : automation.Stack.select)(stackName, workspace))
                                        .then((running): Promise<unknown> => ({ up: () => running.up(converged), preview: () => running.preview({ ...converged, diff: true }), destroy: () => running.destroy({ ...handlers, remove: true }) })[operation]());
                                },
                                catch: (cause) => new Cause.UnknownError(cause, `${stackName} ${operation} failed`),
                            });
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
    }).pipe(Effect.provide(layer)),
);
