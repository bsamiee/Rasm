// --- [IMPORTS] -------------------------------------------------------------------------

import { FileSystem, Path } from '@effect/platform';
import { NodeContext } from '@effect/platform-node';
import { type CreateNodes, type CreateNodesResultArray, createNodesFromFiles } from '@nx/devkit';
import { Effect, Option, ParseResult, Schema, Struct } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _NAME = /^\[project\][ \t]*(?:#.*)?$(?:\r?\n(?!\[).*)*?\r?\nname[ \t]*=[ \t]*(?<quote>["'])(?<name>[A-Za-z0-9](?:[A-Za-z0-9._-]*[A-Za-z0-9])?)\k<quote>/mu;

// --- [MODELS] --------------------------------------------------------------------------

const _ProjectName = Schema.transformOrFail(Schema.String, Schema.String.pipe(Schema.brand('ProjectName')), {
    decode: (text, _, ast) => ParseResult.fromOption(Option.fromNullable(_NAME.exec(text)?.groups?.['name']), () => new ParseResult.Type(ast, text, 'declares no name under [project]')),
    encode: (name, _, ast) => ParseResult.fail(new ParseResult.Forbidden(ast, name, 'decodes alone')),
});

// --- [PROGRAM] -------------------------------------------------------------------------

const _project = Effect.fnUntraced(function* (file: string, root: string) {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const directory = path.dirname(file);
    const configurations = {
        '.csproj': Effect.succeed({ root: directory, tags: ['language:dotnet'], targets: { typecheck: {}, check: {} } }),
        '.json': Effect.succeed({ root: directory, tags: ['language:typescript'], targets: { typecheck: {}, check: {} } }),
        '.pbxproj': Effect.succeed({
            root: path.dirname(directory),
            name: path.basename(directory, '.xcodeproj'),
            tags: ['language:swift', 'host:macos'],
            targets: { build: {}, install: {}, lint: {}, format: {}, check: {} },
        }),
        '.toml': fs.readFileString(path.join(root, file)).pipe(
            Effect.flatMap(Schema.decode(_ProjectName)),
            Effect.map((name) => ({
                root: directory,
                name,
                tags: ['language:python'],
                targets: file.startsWith('tests/python/libs/') ? { typecheck: {}, test: {}, check: {} } : { typecheck: {}, check: {} },
            })),
        ),
    };
    const configuration = yield* configurations[yield* Schema.decodeUnknown(Schema.Literal(...Struct.keys(configurations)))(path.extname(file))];
    return { projects: { [configuration.root]: configuration } };
}, Effect.provide(NodeContext.layer));

// --- [REGISTRATION] --------------------------------------------------------------------

const createNodes: CreateNodes = [
    '{{apps,libs,tests,tools}/**/*.csproj,{apps,libs,tests,tools}/**/*.xcodeproj/project.pbxproj,{apps,libs,tests}/**/tsconfig.json,.claude/plugins/*/tsconfig.json,{libs/python,apps/*,tests/python,tests/python/libs}/*/pyproject.toml}',
    (files, options, context): Promise<CreateNodesResultArray> => createNodesFromFiles((file) => Effect.runPromise(_project(file, context.workspaceRoot)), files, options, context),
];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
