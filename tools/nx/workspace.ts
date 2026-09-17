// --- [IMPORTS] -------------------------------------------------------------------------

import { NodeServices } from '@effect/platform-node';
import { type CreateNodes, type CreateNodesResultArray, createNodesFromFiles } from '@nx/devkit';
import { Array, Effect, FileSystem, Iterable, Option, Path, Record, Schema, SchemaGetter, Struct } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _NAME = /^\[project\][ \t]*(?:#.*)?$(?:\r?\n(?!\[).*)*?\r?\nname[ \t]*=[ \t]*(?<quote>["'])(?<name>[A-Za-z0-9](?:[A-Za-z0-9._-]*[A-Za-z0-9])?)\k<quote>/mu;
const _ROOT_PROJECT_NAME = /^rootProject\.name[ \t]*=[ \t]*"(?<name>[^"]+)"/mu;
const _HOST = /\bHOSTS\.(?<name>[a-z]+)\b/gu;
const _SUBCOMMAND = /Command\.make\(\s*'(?<name>[a-z][a-z-]*)'\s*,/gu;

// --- [PROGRAM] -------------------------------------------------------------------------

const _name = (pattern: RegExp): Schema.decodeTo<Schema.String, Schema.String> =>
    Schema.String.pipe(
        Schema.decodeTo(Schema.String, {
            decode: SchemaGetter.transformOptional(Option.flatMapNullishOr((text: string) => text.match(pattern)?.groups?.['name'])),
            encode: SchemaGetter.forbidden(() => 'decodes alone'),
        }),
    );

const _names = (pattern: RegExp): Schema.decodeTo<Schema.$Array<Schema.String>, Schema.String> =>
    Schema.String.pipe(
        Schema.decodeTo(Schema.Array(Schema.String), {
            decode: SchemaGetter.transform((text: string) => Array.getSomes(Iterable.map(text.matchAll(pattern), (match) => Option.fromNullishOr(match.groups?.['name'])))),
            encode: SchemaGetter.forbidden(() => 'decodes alone'),
        }),
    );

const _hosts = (text: string): Effect.Effect<string[], Schema.SchemaError> => Effect.map(Schema.decodeEffect(_names(_HOST))(text), (names) => Array.map(Array.dedupe(names), (name) => `host:${name}`));

const _project = Effect.fnUntraced(function* (file: string, root: string) {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const directory = path.dirname(file);
    const manifest = fs.readFileString(path.join(root, file));
    const configurations = {
        '.csproj': Effect.succeed({ root: directory, tags: ['language:dotnet'], targets: { typecheck: {}, check: {} } }),
        '.swcrc': Effect.succeed({ root: directory, tags: ['host:extendscript'], targets: { build: {} } }),
        'automation.ts': manifest.pipe(
            Effect.flatMap((text) => Effect.all({ hosts: _hosts(text), names: Schema.decodeEffect(_names(_SUBCOMMAND))(text) })),
            Effect.map(({ hosts, names }) => ({
                root: directory,
                tags: hosts,
                targets: Record.fromIterableWith(names, (name) => [name, { command: `node automation.ts ${name}`, options: { cwd: '{projectRoot}' } }]),
            })),
        ),
        'project.pbxproj': Effect.succeed({
            root: path.dirname(directory),
            name: path.basename(directory, '.xcodeproj'),
            tags: ['language:swift', 'host:macos'],
            targets: { build: {}, install: {}, lint: {}, format: {}, check: {} },
        }),
        'pyproject.toml': manifest.pipe(
            Effect.flatMap(Schema.decodeEffect(_name(_NAME))),
            Effect.map((name) => ({
                root: directory,
                name,
                tags: ['language:python'],
                targets: file.startsWith('tests/python/libs/') ? { typecheck: {}, test: {}, check: {} } : { typecheck: {}, check: {} },
            })),
        ),
        'settings.gradle.kts': manifest.pipe(
            Effect.flatMap(Schema.decodeEffect(_name(_ROOT_PROJECT_NAME))),
            Effect.map((name) => ({ root: directory, name, tags: ['language:java'], targets: { check: {} } })),
        ),
        'tsconfig.json': Effect.succeed({ root: directory, tags: ['language:typescript'], targets: { typecheck: {}, check: {} } }),
        'uxp.config.ts': manifest.pipe(
            Effect.flatMap(_hosts),
            Effect.filterOrFail(Array.isReadonlyArrayNonEmpty, () => new Error(`${file} names no row of the HOSTS table`)),
            Effect.map((hosts) => ({ root: directory, tags: ['host:uxp', ...hosts], targets: { build: {} } })),
        ),
    };
    const kind = path.extname(file) === '.csproj' ? '.csproj' : path.basename(file);
    const configuration = yield* configurations[yield* Schema.decodeUnknownEffect(Schema.Literals(Struct.keys(configurations)))(kind)];
    return { projects: { [configuration.root]: configuration } };
}, Effect.provide(NodeServices.layer));

// --- [REGISTRATION] --------------------------------------------------------------------

const createNodes: CreateNodes = [
    '{{apps,libs,tests,tools}/**/*.csproj,{apps,libs,tests,tools}/**/*.xcodeproj/project.pbxproj,{apps,libs,tests}/**/tsconfig.json,.claude/plugins/*/tsconfig.json,{libs/python,apps/*,tests/python,tests/python/libs}/*/pyproject.toml,{apps,libs,tests}/**/settings.gradle.kts,apps/**/uxp.config.ts,apps/**/.swcrc,{apps,libs}/**/automation.ts}',
    (files, options, context): Promise<CreateNodesResultArray> => createNodesFromFiles((file) => Effect.runPromise(_project(file, context.workspaceRoot)), files, options, context),
];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
