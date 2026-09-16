// --- [IMPORTS] -------------------------------------------------------------------------

import { NodeServices } from '@effect/platform-node';
import { type CreateNodes, type CreateNodesResultArray, createNodesFromFiles } from '@nx/devkit';
import { Effect, FileSystem, Option, Path, Schema, SchemaGetter, Struct } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _NAME = /^\[project\][ \t]*(?:#.*)?$(?:\r?\n(?!\[).*)*?\r?\nname[ \t]*=[ \t]*(?<quote>["'])(?<name>[A-Za-z0-9](?:[A-Za-z0-9._-]*[A-Za-z0-9])?)\k<quote>/mu;
const _ROOT_PROJECT_NAME = /^rootProject\.name[ \t]*=[ \t]*"(?<name>[^"]+)"/mu;

// --- [PROGRAM] -------------------------------------------------------------------------

const _name = (pattern: RegExp): Schema.decodeTo<Schema.String, Schema.String> =>
    Schema.String.pipe(
        Schema.decodeTo(Schema.String, {
            decode: SchemaGetter.transformOptional(Option.flatMapNullishOr((text: string) => text.match(pattern)?.groups?.['name'])),
            encode: SchemaGetter.forbidden(() => 'decodes alone'),
        }),
    );

const _project = Effect.fnUntraced(function* (file: string, root: string) {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const directory = path.dirname(file);
    const manifest = fs.readFileString(path.join(root, file));
    const configurations = {
        '.csproj': Effect.succeed({ root: directory, tags: ['language:dotnet'], targets: { typecheck: {}, check: {} } }),
        '.json': Effect.succeed({ root: directory, tags: ['language:typescript'], targets: { typecheck: {}, check: {} } }),
        '.kts': manifest.pipe(
            Effect.flatMap(Schema.decodeEffect(_name(_ROOT_PROJECT_NAME))),
            Effect.map((name) => ({ root: directory, name, tags: ['language:java'], targets: { check: {} } })),
        ),
        '.pbxproj': Effect.succeed({
            root: path.dirname(directory),
            name: path.basename(directory, '.xcodeproj'),
            tags: ['language:swift', 'host:macos'],
            targets: { build: {}, install: {}, lint: {}, format: {}, check: {} },
        }),
        '.toml': manifest.pipe(
            Effect.flatMap(Schema.decodeEffect(_name(_NAME))),
            Effect.map((name) => ({
                root: directory,
                name,
                tags: ['language:python'],
                targets: file.startsWith('tests/python/libs/') ? { typecheck: {}, test: {}, check: {} } : { typecheck: {}, check: {} },
            })),
        ),
    };
    const configuration = yield* configurations[yield* Schema.decodeUnknownEffect(Schema.Literals(Struct.keys(configurations)))(path.extname(file))];
    return { projects: { [configuration.root]: configuration } };
}, Effect.provide(NodeServices.layer));

// --- [REGISTRATION] --------------------------------------------------------------------

const createNodes: CreateNodes = [
    '{{apps,libs,tests,tools}/**/*.csproj,{apps,libs,tests,tools}/**/*.xcodeproj/project.pbxproj,{apps,libs,tests}/**/tsconfig.json,.claude/plugins/*/tsconfig.json,{libs/python,apps/*,tests/python,tests/python/libs}/*/pyproject.toml,{apps,libs,tests}/**/settings.gradle.kts}',
    (files, options, context): Promise<CreateNodesResultArray> => createNodesFromFiles((file) => Effect.runPromise(_project(file, context.workspaceRoot)), files, options, context),
];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
