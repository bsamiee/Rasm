// --- [IMPORTS] -------------------------------------------------------------------------

import metadata from '@adobe-uxp-types/photoshop/package.json' with { type: 'json' };
import { Array, Console, Effect, FileSystem, Number, Path, type PlatformError, Record, Schema } from 'effect';
import { type EnumMember, Node, Project, StructureKind, SyntaxKind, ts, VariableDeclarationKind, Writers } from 'ts-morph';
import { MANIPULATION } from '../sdef.ts';

// --- [ERRORS] --------------------------------------------------------------------------

const GenerateError: Schema.TaggedStruct<'typingsNotDecodable', { readonly cause: Schema.Defect }> = Schema.TaggedStruct('typingsNotDecodable', { cause: Schema.Defect() });

// --- [GENERATE] ------------------------------------------------------------------------

const generate: Effect.Effect<void, typeof GenerateError.Type | PlatformError.PlatformError, FileSystem.FileSystem | Path.Path> = Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const entry = yield* Effect.mapError(path.fromFileUrl(new URL(metadata.types, import.meta.resolve('@adobe-uxp-types/photoshop/package.json'))), (cause) => GenerateError.make({ cause }));
    const parsed = yield* Effect.try({
        try: () => {
            const project = new Project({ skipAddingFilesFromTsConfig: true, compilerOptions: { module: ts.ModuleKind.Preserve, moduleResolution: ts.ModuleResolutionKind.Bundler } });
            const source = project
                .addSourceFileAtPath(entry)
                .getModuleOrThrow((declaration) => {
                    const name = declaration.getNameNode();
                    return Node.isStringLiteral(name) && name.getLiteralValue() === 'photoshop';
                })
                .getSymbolOrThrow()
                .getExportOrThrow('constants')
                .getAliasedSymbolOrThrow()
                .getValueDeclarationOrThrow();
            const declarations = Record.fromIterableWith(source.getType().getProperties(), (property) => [
                property.getName(),
                property.getTypeAtLocation(source).getSymbolOrThrow().getValueDeclarationOrThrow().asKindOrThrow(SyntaxKind.EnumDeclaration).getMembers(),
            ]);
            return Record.map(
                declarations,
                Record.fromIterableWith((member: EnumMember) => [member.getSymbolOrThrow().getName(), member.getValue()]),
            );
        },
        catch: (cause) => GenerateError.make({ cause }),
    });
    const enumerations = yield* Effect.mapError(
        Schema.decodeUnknownEffect(Schema.Record(Schema.String, Schema.Record(Schema.String, Schema.Union([Schema.String, Schema.Finite]))), { errors: 'all' })(parsed),
        (cause) => GenerateError.make({ cause }),
    );
    const output = new Project({ useInMemoryFileSystem: true, manipulationSettings: MANIPULATION }).createSourceFile('enumerations.ts', {
        statements: [
            {
                kind: StructureKind.VariableStatement,
                declarationKind: VariableDeclarationKind.Const,
                isExported: true,
                declarations: [
                    {
                        name: 'constants',
                        initializer: Writers.assertion(JSON.stringify(enumerations, null, 4), "typeof import('photoshop').constants"),
                    },
                ],
            },
        ],
    });
    yield* fs.writeFileString(path.join(import.meta.dirname, 'enumerations.ts'), output.getFullText());
    yield* Console.log(
        JSON.stringify({ package: metadata.name, version: metadata.version, enumerations: Record.size(enumerations), members: Number.sumAll(Array.map(Record.values(enumerations), Record.size)) }),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { GenerateError, generate };
