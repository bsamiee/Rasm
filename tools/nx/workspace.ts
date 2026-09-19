// --- [IMPORTS] -------------------------------------------------------------------------

import { Lang, parse, type SgNode } from '@ast-grep/napi';
import type TsxTypes from '@ast-grep/napi/lang/Tsx';
import { NodeServices } from '@effect/platform-node';
import { type CreateNodes, type CreateNodesResultArray, createNodesFromFiles } from '@nx/devkit';
import { Array, Data, Effect, FileSystem, Option, Path, Record, Schema, SchemaGetter, Struct } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _NAME = /^\[project\][ \t]*(?:#.*)?$(?:\r?\n(?!\[).*)*?\r?\nname[ \t]*=[ \t]*(?<quote>["'])(?<name>[A-Za-z0-9](?:[A-Za-z0-9._-]*[A-Za-z0-9])?)\k<quote>/mu;
const _ROOT_PROJECT_NAME = /^rootProject\.name[ \t]*=[ \t]*"(?<name>[^"]+)"/mu;
const _COMMAND_NAME = /^[\w-]+$/u;

// --- [ERRORS] --------------------------------------------------------------------------

class InferenceError extends Data.TaggedError('InferenceError')<{ readonly file: string; readonly reason: string }> {
    override get message(): string {
        return `${this.file}: ${this.reason}`;
    }
}

// --- [PROGRAM] -------------------------------------------------------------------------

const _name = (pattern: RegExp): Schema.decodeTo<Schema.String, Schema.String> =>
    Schema.String.pipe(
        Schema.decodeTo(Schema.String, {
            decode: SchemaGetter.transformOptional(Option.flatMapNullishOr((text: string) => text.match(pattern)?.groups?.['name'])),
            encode: SchemaGetter.forbidden(() => 'decodes alone'),
        }),
    );

const _hosts = (source: SgNode<TsxTypes, 'program'>): readonly string[] =>
    Array.dedupe(
        Array.flatMap(source.findAll<'import_specifier'>({ rule: { kind: 'import_specifier', has: { field: 'name', pattern: 'HOSTS' } } }), (specifier) => {
            const binding = Option.getOrElse(Option.fromNullishOr(specifier.field('alias')), () => specifier.field('name')).text();
            return Array.map(source.findAll<'member_expression'>({ rule: { pattern: `${binding}.$HOST` } }), (node) => `host:${node.field('property').text()}`);
        }),
    );

const _project = Effect.fnUntraced(function* (file: string, root: string) {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const directory = path.dirname(file);
    const manifest = fs.readFileString(path.join(root, file));
    const syntax = manifest.pipe(Effect.map((text) => parse<TsxTypes>(Lang.Tsx, text).root()));
    const configurations = {
        '.csproj': Effect.succeed({ root: directory, tags: ['language:dotnet'], targets: { typecheck: {}, check: {} } }),
        '.swcrc': Effect.succeed({ root: directory, tags: ['host:extendscript'], targets: { build: {} } }),
        'automation.ts': Effect.gen(function* () {
            const source = yield* syntax;
            const imports = source.findAll<'import_specifier'>({
                rule: { kind: 'import_specifier', has: { field: 'name', pattern: 'Command' } },
            });
            const imported = Array.filter(imports, (node) =>
                node.inside({ rule: { kind: 'import_statement', has: { field: 'source', has: { pattern: { context: "'effect/unstable/cli'", selector: 'string_fragment' } } } } }),
            );
            const specifier = yield* Effect.fromOption(
                Option.filter(Array.head(imported), () => imported.length === 1),
                () => new InferenceError({ file, reason: 'Expected one named Effect Command import' }),
            );
            const binding = Option.getOrElse(Option.fromNullishOr(specifier.field('alias')), () => specifier.field('name')).text();
            const registrations = source.findAll({ rule: { pattern: `${binding}.withSubcommands($COMMANDS)` } });
            const registration = yield* Effect.fromOption(
                Option.filter(Array.head(registrations), () => registrations.length === 1),
                () => new InferenceError({ file, reason: 'Expected one command registration' }),
            );
            const commands = yield* Effect.fromOption(
                Option.filter(Option.fromNullishOr(registration.getMatch('COMMANDS')), (node) => node.is('array')),
                () => new InferenceError({ file, reason: 'Subcommands must be registered in an array' }),
            );
            const names = yield* Effect.validate(
                Array.filter(commands.namedChildren(), (node) => !node.is('comment')),
                Effect.fnUntraced(function* (command: SgNode<TsxTypes>) {
                    const value = command.is('identifier')
                        ? Option.flatMap(Option.fromNullishOr(source.find({ rule: { pattern: `const ${command.text()} = $VALUE`, inside: { kind: 'program' } } })), (node) =>
                              Option.fromNullishOr(node.getMatch('VALUE')),
                          )
                        : Option.some(command);
                    const declared = yield* Effect.fromOption(
                        Option.filter(value, (node) => node.matches(`${binding}.make($NAME, $$$ARGUMENTS)`)),
                        () => new InferenceError({ file, reason: `Subcommand ${command.text()} must be a direct Command.make call or a top-level const containing one` }),
                    );
                    const literal = declared.find({ rule: { pattern: `${binding}.make($NAME, $$$ARGUMENTS)` } })?.getMatch('NAME');
                    const fragments = literal?.is('string') === true ? literal.namedChildren() : [];
                    const name = yield* Effect.fromOption(
                        Option.filter(Array.head(fragments), (node) => fragments.length === 1 && node.is('string_fragment') && _COMMAND_NAME.test(node.text())),
                        () => new InferenceError({ file, reason: `Subcommand ${command.text()} must have a literal name containing only letters, digits, underscores, or hyphens` }),
                    );
                    return name.text();
                }),
            );
            if (Array.dedupe(names).length !== names.length) {
                return yield* Effect.fail(new InferenceError({ file, reason: 'Registered subcommands must have distinct names' }));
            }
            return {
                root: directory,
                tags: [..._hosts(source)],
                targets: Record.fromIterableWith(names, (name) => [name, { command: `node automation.ts ${name}`, options: { cwd: '{projectRoot}' } }]),
            };
        }),
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
        'uxp.config.ts': syntax.pipe(
            Effect.map(_hosts),
            Effect.filterOrFail(Array.isReadonlyArrayNonEmpty, () => new InferenceError({ file, reason: 'No imported HOSTS member identifies the UXP host' })),
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
