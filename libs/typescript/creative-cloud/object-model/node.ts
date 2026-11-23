import { readdirSync, readFileSync, statSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { getTouchingPropertyName, isParameterDeclaration, isPropertyAccessExpression, isSetAccessorDeclaration, isTypeAliasDeclaration } from '@typescript/native/ast';
import { createFileSystemLayer, createFileSystemWithLib, type FileSystemCallbacks, serverFS } from '@typescript/native/fs';
import { API, type Checker, type Snapshot, type Diagnostic, DiagnosticCategory, type Program, type Signature, SymbolFlags, type Symbol as TypeScriptSymbol, type Type, TypeFlags, TypeFormatFlags } from '@typescript/native/sync';
import { Array, Effect, Equal, Function, flow, Match, Option, Predicate, Record, Result, Schema, SchemaTransformation, Stream, Struct } from 'effect';
import { Ndjson } from 'effect/encoding';
import { XMLParser } from 'fast-xml-parser';
import { Declaration } from '#/model';

// --- [TYPES] ---------------------------------------------------------------------------

interface Source {
    readonly root: string;
    readonly types: readonly string[];
    readonly preamble: string;
    readonly enumeration: Option.Option<string>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const SCRIPT = '/script.ts';
const VERSIONS = ['minVersion', 'since'] as const;
const OPTIONS = ['--target', 'es2022', '--module', 'esnext', '--moduleResolution', 'bundler', '--lib', 'es2022', '--strict', 'false', '--skipLibCheck', '--noEmit'] as const;
const RANGE = /\(Range: [^)]*\)/u;
const LISTED = new Set(['classdef', 'elements', 'property', 'method', 'parameter', 'datatype']);
const PARSER = new XMLParser({ ignoreAttributes: false, attributeNamePrefix: '', textNodeName: 'text', parseTagValue: false, parseAttributeValue: false, isArray: (name) => LISTED.has(name) });

// --- [MODELS] --------------------------------------------------------------------------

const Flag = <const T extends string, const F extends string>(truthy: T, falsy: F) => Schema.Union([Schema.Literal(truthy).transform(true), Schema.Literal(falsy).transform(false)]);
const Ranged = Schema.TemplateLiteralParser(['(Range: ', Schema.Finite, ' to ', Schema.Finite, ')']);
const Text = Schema.Union([Schema.String, Schema.Struct({ text: Schema.String }).pipe(Schema.decodeTo(Schema.String, SchemaTransformation.transform({ decode: Struct.get('text'), encode: (text) => ({ text }) })))]);
const Datatype = Schema.Struct({ type: Text, array: Schema.optionalKey(Schema.Union([Schema.String, Schema.Struct({ size: Schema.String })])), value: Schema.optionalKey(Text), ...Struct.Record(['min', 'max'], Schema.optionalKey(Schema.FiniteFromString)) });
const Described = { name: Schema.String, shortdesc: Schema.optionalKey(Text), datatype: Schema.optionalKey(Schema.NonEmptyArray(Datatype).annotate({ description: 'Alternatives where a value takes more than one type' })) };
const Argued = Schema.Struct({ ...Described, optional: Schema.optionalKey(Schema.Literal('true').transform(true)) });
const Property = Schema.Struct({ ...Described, rwaccess: Schema.optionalKey(Schema.Literal('readonly').transform(true)) });
const Called = Schema.Struct({ ...Described, parameters: Schema.optionalKey(Schema.Struct({ parameter: Schema.Array(Argued) })) });
const Elements = Schema.Struct({ type: Schema.Literals(['instance', 'class', 'constructor', 'event']), property: Schema.optionalKey(Schema.Array(Property)), method: Schema.optionalKey(Schema.Array(Called)) });
const Classdef = Schema.Struct({ ...Described, enumeration: Schema.optionalKey(Schema.Literal('true').transform(true)), superclass: Schema.optionalKey(Text), elements: Schema.Array(Elements).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) });
const Omv = Schema.Struct({ dictionary: Schema.Struct({ package: Schema.Struct({ classdef: Schema.Array(Classdef) }) }) });
const Stated = Flag('true', 'false');
const Kind = Flag('class', 'instance');
const Callable = Schema.Struct({ name: Schema.String, dataType: Schema.String, propKind: Kind, async: Stated, params: Schema.Array(Schema.Struct({ name: Schema.optionalKey(Schema.String), dataType: Schema.String, optional: Stated })) });
const Constructor = Schema.Struct({ propKind: Schema.Literal('constructor') });
const Entry = Schema.Struct({
    name: Schema.String,
    properties: Schema.NullOr(Schema.Array(Schema.Struct({ name: Schema.String, dataType: Schema.String, propKind: Kind, propType: Flag('readOnly', 'readWrite'), async: Stated }))),
    methods: Schema.NullOr(Schema.Array(Schema.Union([Callable, Constructor]))),
});
const Manifest = Schema.fromJsonString(Schema.Array(Entry));
const Documented = Schema.Struct({
    name: Schema.String,
    description: Schema.NullOr(Schema.String),
    properties: Schema.Array(Schema.Struct({ Name: Schema.String, Type: Schema.String, Access: Flag('R', 'RW'), 'Min Version': Schema.String, Description: Schema.String })),
    methods: Schema.Array(Schema.Struct({ name: Schema.String, returns: Schema.NullOr(Schema.String), since: Schema.NullOr(Schema.String), description: Schema.NullOr(Schema.String), parameters: Schema.Array(Schema.Struct({ Name: Schema.String, Type: Schema.String, Description: Schema.String })) })),
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const stated = (path: string) => Option.fromUndefinedOr(statSync(path, { throwIfNoEntry: false }));
const HOST: FileSystemCallbacks = {
    directoryExists: (path) => Option.match(stated(path), { onNone: () => serverFS.useOS, onSome: (stats) => stats.isDirectory() }),
    fileExists: (path) => Option.match(stated(path), { onNone: () => serverFS.useOS, onSome: (stats) => stats.isFile() }),
    getAccessibleEntries: (path) =>
        Option.match(
            Option.filter(stated(path), (stats) => stats.isDirectory()),
            {
                onNone: () => serverFS.useOS,
                onSome: () => {
                    const [files, directories] = Array.partition(
                        readdirSync(path),
                        Result.liftPredicate((name: string) => statSync(join(path, name), { throwIfNoEntry: false })?.isDirectory() !== true, Function.identity),
                    );
                    return { files, directories, symlinks: undefined };
                },
            },
        ),
    readFile: (path) =>
        Option.match(
            Option.filter(stated(path), (stats) => stats.isFile()),
            { onNone: () => serverFS.useOS, onSome: () => readFileSync(path, 'utf-8') },
        ),
    realpath: serverFS.identity,
    stat: serverFS.fakeStat,
    writeFile: serverFS.error,
    removeFile: serverFS.error,
};
const scripted = (preamble: string, program: string) => `${preamble}\n${program}\nexport {};\n`;
const flagged = (symbol: TypeScriptSymbol, ...flags: Array.NonEmptyReadonlyArray<SymbolFlags>) => flags.some((flag) => (symbol.flags & flag) !== 0);
const typed = (checker: Checker, type: Type) => checker.typeToString(type, undefined, TypeFormatFlags.NoTruncation);
const promised = (checker: Checker, type: Type) => Option.exists(Option.fromUndefinedOr(checker.getAwaitedType(type)), (awaited) => awaited.id !== type.id);
const bounded = (description: Option.Option<string>) => {
    const range = Option.flatMap(description, (text) => Option.flatMap(Option.fromNullishOr(RANGE.exec(text)), ([phrase]) => Schema.decodeUnknownOption(Ranged)(phrase)));
    return { minimum: Option.map(range, (parsed) => parsed[1]), maximum: Option.map(range, (parsed) => parsed[3]) };
};
const tagged = (checker: Checker, symbol: TypeScriptSymbol) => ({
    description: Option.liftPredicate(checker.getDocumentationCommentOfSymbol(symbol), (text) => text !== ''),
    version: Option.flatMap(
        Array.findFirst(checker.getJsDocTagsOfSymbol(symbol), ({ name }) => Array.contains(VERSIONS, name)),
        ({ text }) => Option.fromUndefinedOr(text),
    ),
});
const optional = (parameter: TypeScriptSymbol) => Option.exists(Option.fromUndefinedOr(parameter.valueDeclaration?.resolve()), (node) => isParameterDeclaration(node) && (node.questionToken ?? node.initializer ?? node.dotDotDotToken) !== undefined);
const method = (checker: Checker, symbol: TypeScriptSymbol, statics: boolean) => (signature: Signature) =>
    Declaration.Method.make({
        name: symbol.name,
        ...tagged(checker, symbol),
        type: Option.some(typed(checker, signature.getReturnType())),
        static: statics,
        async: promised(checker, signature.getReturnType()),
        parameters: Array.map(signature.getParameters(), (parameter) => {
            const { description } = tagged(checker, parameter);
            return Declaration.Parameter.make({ name: parameter.name, description, ...bounded(description), type: Option.some(typed(checker, checker.getTypeOfSymbol(parameter))), optional: optional(parameter) });
        }),
    });
const member = (checker: Checker, statics: boolean) => (symbol: TypeScriptSymbol) => {
    const value = checker.getTypeOfSymbol(symbol);
    const signatures = flagged(symbol, SymbolFlags.Method) ? value.getCallSignatures() : [];
    return Match.value({ prototype: statics && flagged(symbol, SymbolFlags.Prototype), signatures }).pipe(
        Match.when({ prototype: true }, () => ({ fields: [], methods: [] })),
        Match.when({ signatures: Array.isReadonlyArrayNonEmpty }, () => ({ fields: [], methods: Array.map(signatures, method(checker, symbol, statics)) })),
        Match.orElse(() => {
            const described = tagged(checker, symbol);
            return {
                fields: [Declaration.Field.make({ name: symbol.name, ...described, ...bounded(described.description), type: Option.some(typed(checker, value)), static: statics, async: promised(checker, value), readonly: checker.isReadonlySymbol(symbol), optional: flagged(symbol, SymbolFlags.Optional) })],
                methods: [],
            };
        }),
    );
};
const members = (checker: Checker, type: Type, statics: boolean) => {
    const listed = Array.map(type.getProperties(), member(checker, statics));
    return { fields: Array.flatMap(listed, Struct.get('fields')), methods: Array.flatMap(listed, Struct.get('methods')) };
};
const exported = (checker: Checker, symbol: TypeScriptSymbol): readonly TypeScriptSymbol[] =>
    Array.flatMap(checker.getExportsOfModule(symbol), (each) => [each, ...(flagged(each, SymbolFlags.Module) && !flagged(each, SymbolFlags.Variable, SymbolFlags.Class, SymbolFlags.Function, SymbolFlags.Enum) ? exported(checker, each) : [])]);
const classified = (checker: Checker, base: Option.Option<Type>) =>
    Match.type<TypeScriptSymbol>().pipe(
        Match.when(
            (symbol) => flagged(symbol, SymbolFlags.Enum) || (flagged(symbol, SymbolFlags.Variable) && Option.exists(base, (enumeration) => checker.isTypeAssignableTo(checker.getTypeOfSymbol(symbol), enumeration))),
            (symbol) => Result.succeed({ symbol, enumerations: true }),
        ),
        Match.when(
            (symbol) => flagged(symbol, SymbolFlags.Class, SymbolFlags.Interface, SymbolFlags.TypeAlias) && Array.isReadonlyArrayNonEmpty(checker.getDeclaredTypeOfSymbol(symbol).getProperties()),
            (symbol) => Result.succeed({ symbol, enumerations: false }),
        ),
        Match.orElse(() => Result.failVoid),
    );
const bases = (checker: Checker, symbol: TypeScriptSymbol) => Array.getSomes(Array.map(checker.getDeclaredTypeOfSymbol(symbol).getBaseTypes() ?? [], (parent) => Option.map(Option.fromUndefinedOr(parent.getSymbol()), Struct.get('name'))));
const enumerated = (checker: Checker, symbol: TypeScriptSymbol, base: Option.Option<Type>) => {
    const omitted = new Set(Option.match(base, { onNone: Array.empty<string>, onSome: (type) => Array.map(type.getProperties(), Struct.get('name')) }));
    const listed = flagged(symbol, SymbolFlags.Enum)
        ? checker.getExportsOfModule(symbol)
        : checker
              .getTypeOfSymbol(symbol)
              .getProperties()
              .filter(({ name }) => !omitted.has(name));
    return Declaration.cases.Enumeration.make({
        name: symbol.name,
        ...tagged(checker, symbol),
        members: Array.map(listed, (each) => ({ name: each.name, description: tagged(checker, each).description, value: Option.flatMap(Option.fromUndefinedOr(each.valueDeclaration?.resolve()), (node) => Option.fromUndefinedOr(checker.getConstantValue(node))) })),
    });
};
const classed = (checker: Checker, symbol: TypeScriptSymbol) => {
    const declared = checker.getDeclaredTypeOfSymbol(symbol);
    const constructor = flagged(symbol, SymbolFlags.Class) ? Option.some(checker.getTypeOfSymbol(symbol)) : Option.none();
    const [own, statics] = [members(checker, declared, false), Option.match(constructor, { onNone: () => ({ fields: [], methods: [] }), onSome: (type) => members(checker, type, true) })];
    return Declaration.cases.Class.make({
        name: symbol.name,
        ...tagged(checker, symbol),
        parents: Array.map(declared.getBaseTypes() ?? [], (parent) => typed(checker, parent)),
        constructible: Option.exists(constructor, (type) => Array.isReadonlyArrayNonEmpty(type.getConstructSignatures())),
        fields: [...own.fields, ...statics.fields],
        methods: [...own.methods, ...statics.methods],
    });
};
const shaped = (checker: Checker, kept: Record.ReadonlyRecord<string, { readonly symbol: TypeScriptSymbol; readonly enumerations: boolean }>) => {
    const owned = (type: Type) => Array.findFirst([type, ...(type.getBaseTypes() ?? [])], (each) => Option.filter(Option.map(Option.fromUndefinedOr(each.getSymbol()), Struct.get('name')), (name) => Record.has(kept, name)));
    const naming = (type: Type) => {
        const constituents = type.isUnionType() ? type.getTypes() : [type];
        return constituents.some(({ flags }) => (flags & TypeFlags.StringLike) !== 0) ? [] : Array.dedupe(Array.filterMap(constituents, flow(owned, Result.fromOption(Function.constVoid))));
    };
    const written = (symbol: TypeScriptSymbol) =>
        Array.findFirst(symbol.declarations, (handle) =>
            Option.fromUndefinedOr(handle.resolve()).pipe(
                Option.filter(isSetAccessorDeclaration),
                Option.flatMap(({ parameters }) => Array.head(parameters)),
                Option.flatMapNullishOr(Struct.get('type')),
                Option.map((node) => checker.getTypeFromTypeNode(node)),
            ),
        );
    const field = (symbol: TypeScriptSymbol) => {
        const read = checker.getTypeOfSymbol(symbol);
        return {
            settable: !checker.isReadonlySymbol(symbol),
            collection: Option.flatMap(Option.fromUndefinedOr(read.getSymbol()), (type) =>
                Option.flatMap(
                    Array.findFirst(checker.getJsDocTagsOfSymbol(type), ({ name }) => name === 'collection'),
                    ({ text }) => Option.fromUndefinedOr(text),
                ),
            ),
            array: read.isTypeReference() && checker.isArrayType(read) ? Option.flatMap(Array.head(checker.getTypeArguments(read)), owned) : Option.none(),
            classes: Option.match(written(symbol), { onNone: Array.empty<string>, onSome: naming }),
            ...bounded(Option.liftPredicate(checker.getDocumentationCommentOfSymbol(symbol), (text) => text !== '')),
        };
    };
    const parameters = (symbol: TypeScriptSymbol) => Option.match(Array.head(checker.getTypeOfSymbol(symbol).getCallSignatures()), { onNone: Array.empty<readonly string[]>, onSome: (signature) => Array.map(signature.getParameters(), (parameter) => naming(checker.getTypeOfSymbol(parameter))) });
    return {
        classes: Record.filterMap(kept, ({ symbol, enumerations }) => {
            const [fields, methods] = Array.partition(
                checker.getDeclaredTypeOfSymbol(symbol).getProperties(),
                Result.liftPredicate((each: TypeScriptSymbol) => !flagged(each, SymbolFlags.Method), Function.identity),
            );
            return enumerations ? Result.failVoid : Result.succeed({ fields: Record.fromIterableWith(fields, (each) => [each.name, field(each)] as const), methods: Record.fromIterableWith(methods, (each) => [each.name, parameters(each)] as const) });
        }),
        enumerations: Record.keys(Record.filter(kept, Struct.get('enumerations'))),
    };
};
const named = (symbol: TypeScriptSymbol) => (flagged(symbol, SymbolFlags.TypeLiteral) ? Option.flatMap(Option.fromUndefinedOr(Option.getOrUndefined(Array.head(symbol.declarations))?.resolve()?.parent), (node) => (isTypeAliasDeclaration(node) ? Option.some(node.name.text) : Option.none())) : Option.some(symbol.name));
const found = (program: Program, checker: Checker, lines: number) => (diagnostic: Diagnostic) => ({
    line: Option.map(Option.fromUndefinedOr(diagnostic.startPosition), ({ line }) => line - lines + 1),
    code: diagnostic.code,
    text: diagnostic.text,
    declared: Option.fromUndefinedOr(program.getSourceFile(SCRIPT)).pipe(
        Option.flatMap((source) => Option.liftPredicate(getTouchingPropertyName(source, diagnostic.pos).parent, isPropertyAccessExpression)),
        Option.flatMap((access) => {
            const type = checker.getTypeAtLocation(access.expression);
            return Option.firstSomeOf(Array.map([type.getAliasSymbol(), type.getSymbol(), checker.getSymbolAtLocation(access.expression)], Option.fromUndefinedOr));
        }),
        Option.flatMap(named),
    ),
});
const described =
    (checker: Checker, base: Option.Option<Type>) =>
    ({ symbol, enumerations }: { readonly symbol: TypeScriptSymbol; readonly enumerations: boolean }) =>
        enumerations ? enumerated(checker, symbol, base) : classed(checker, symbol);
const checking = (base: Snapshot, id: Parameters<Snapshot['getProject']>[0], preamble: string) =>
    Effect.fnUntraced(function* (code: string) {
        const next = yield* Effect.acquireRelease(
            Effect.sync(() => base.update({ fileSystem: createFileSystemLayer([[SCRIPT, scripted(preamble, code)]]), fileNotifications: { changed: [SCRIPT] }, ensurePrograms: true })),
            (held) => Effect.sync(() => held.dispose()),
        );
        const checked = yield* Effect.orDie(Effect.fromNullishOr(next.getProject(id)));
        return Array.map(
            [...checked.program.getSyntacticDiagnostics(SCRIPT), ...checked.program.getSemanticDiagnostics(SCRIPT)].filter(({ category }) => Equal.equals(category, DiagnosticCategory.Error)),
            found(checked.program, checked.checker, preamble.split('\n').length),
        );
    }, Effect.scoped);
const listing = (checker: Checker, files: readonly TypeScriptSymbol[], base: Option.Option<string>) => {
    const table = Record.fromIterableWith(
        Array.flatMap(files, (symbol) => exported(checker, symbol)),
        (symbol) => [symbol.name, symbol] as const,
    );
    const enumeration = Option.map(
        Option.flatMap(base, (name) => Record.get(table, name)),
        (symbol) => checker.getTypeOfSymbol(symbol),
    );
    const listed = Record.filterMap(table, classified(checker, enumeration));
    const names = new Set(Record.keys(Record.filter(listed, Struct.get('enumerations'))));
    return { enumeration, kept: Record.filter(listed, ({ symbol, enumerations }) => enumerations || !bases(checker, symbol).some((name) => names.has(name))) };
};
const declared = Effect.fnUntraced(function* (compiler: Option.Option<{ readonly executable: string; readonly library: string }>, modules: string) {
    const api = yield* Effect.acquireRelease(
        Effect.sync(() => new API({ cwd: '/', fs: HOST, ...Record.getSomes({ tsserverPath: Option.map(compiler, Struct.get('executable')) }) })),
        (held) => Effect.sync(() => held.close()),
    );
    const mounted = { '/node_modules': { target: modules, host: true } };
    const libraries = Option.match(compiler, { onNone: () => ({ symlinks: mounted }), onSome: ({ executable, library }) => ({ defaultLibraryPath: dirname(executable), symlinks: { ...mounted, [dirname(executable)]: { target: library, host: true } } }) });
    return Effect.fnUntraced(function* (source: Source) {
        const owned = join(modules, source.root);
        const { options } = api.parseCommandLine([...OPTIONS, ...Array.flatMap(source.types, (type) => ['--types', type])]);
        const base = api.createSnapshot({
            fileSystem: createFileSystemWithLib([[SCRIPT, scripted(source.preamble, '')]], libraries),
            createPrograms: [{ rootFiles: [SCRIPT], compilerOptions: options }],
        });
        const [{ id }] = base.operation.createdPrograms;
        const { checker, program } = yield* Effect.orDie(Effect.fromNullishOr(base.getProject(id)));
        const files = program.getSourceFileNames().filter((path) => path.startsWith(owned));
        const listed = yield* Effect.cached(Effect.sync(() => listing(checker, Array.getSomes(Array.map(checker.getSymbolOfSourceFile(files), Option.fromUndefinedOr)), source.enumeration)));
        const check = checking(base, id, source.preamble);
        return {
            check,
            catalog: yield* Effect.cached(Effect.map(listed, ({ enumeration, kept }) => Declaration.catalog(kept, described(checker, enumeration), Struct.get('enumerations')))),
            model: Effect.map(listed, ({ kept }) => shaped(checker, kept)),
        };
    });
});

const nonEmpty = (text: string) => Option.liftPredicate(text.trim(), (trimmed) => trimmed !== '');
const summary = (shortdesc: string | undefined) => Option.flatMap(Option.fromUndefinedOr(shortdesc), nonEmpty);
const datatyped = (datatypes: Array.NonEmptyReadonlyArray<typeof Datatype.Type> | undefined) => ({
    type: Option.map(Option.fromUndefinedOr(datatypes), (alternatives) => Array.map(alternatives, ({ type, array }) => (array === undefined ? type : `${type}[]`)).join(' | ')),
    minimum: Array.findFirst(datatypes ?? [], ({ min }) => Option.fromUndefinedOr(min)),
    maximum: Array.findFirst(datatypes ?? [], ({ max }) => Option.fromUndefinedOr(max)),
});
const parameter = (each: typeof Argued.Type) => Declaration.Parameter.make({ name: each.name, description: summary(each.shortdesc), ...datatyped(each.datatype), optional: each.optional ?? false });
const grouped =
    (type: (typeof Elements.Type)['type']) =>
    ({ elements }: typeof Classdef.Type) =>
        elements.some((group) => group.type === type);
const classdef =
    (constructible: Predicate.Predicate<typeof Classdef.Type>) =>
    (definition: typeof Classdef.Type): readonly [string, typeof Declaration.Page.Type] => {
        const { name, shortdesc, enumeration, superclass, elements } = definition;
        const description = summary(shortdesc);
        const properties = Array.flatMap(elements, (group) => group.property ?? []);
        return [
            name,
            (enumeration ?? false)
                ? Declaration.cases.Enumeration.make({ name, description, members: Array.map(properties, (each) => ({ name: each.name, description: summary(each.shortdesc), value: Array.findFirst(each.datatype ?? [], ({ value }) => Option.fromUndefinedOr(value)) })) })
                : Declaration.cases.Class.make({
                      name,
                      description,
                      parents: Option.toArray(Option.fromUndefinedOr(superclass)),
                      constructible: constructible(definition),
                      fields: Array.map(properties, (each) => Declaration.Field.make({ name: each.name, description: summary(each.shortdesc), ...datatyped(each.datatype), readonly: each.rwaccess ?? false })),
                      methods: Array.flatMap(elements, (group) =>
                          Array.map(group.type === 'constructor' ? [] : (group.method ?? []), (each) => Declaration.Method.make({ name: each.name, description: summary(each.shortdesc), ...datatyped(each.datatype), parameters: Array.map(each.parameters?.parameter ?? [], parameter) })),
                      ),
                  }),
        ];
    };
const classdefs = ({ dictionary }: typeof Omv.Type) => Array.map(dictionary.package.classdef, classdef(dictionary.package.classdef.some(grouped('constructor')) ? grouped('constructor') : Predicate.not(grouped('instance'))));
const paged = (pages: Record.ReadonlyRecord<string, typeof Declaration.Page.Type>) => Declaration.catalog(pages, Function.identity, Declaration.guards.Enumeration);
const omv = (texts: Array.NonEmptyReadonlyArray<string>) =>
    Effect.map(
        Effect.forEach(texts, (text) => Schema.decodeUnknownEffect(Omv)(PARSER.parse(text))),
        (decoded) => paged(Record.fromEntries(Array.flatMap(decoded, classdefs))),
    );
const entry = ({ name, properties, methods }: typeof Entry.Type): readonly [string, typeof Declaration.Page.Type] => {
    const [callable, constructors] = Array.partition(methods ?? [], Result.liftPredicate(Schema.is(Callable), Function.identity));
    return [
        name,
        Declaration.cases.Class.make({
            name,
            parents: [],
            constructible: Array.isReadonlyArrayNonEmpty(constructors),
            fields: Array.map(properties ?? [], (each) => Declaration.Field.make({ name: each.name, type: nonEmpty(each.dataType), static: each.propKind, async: each.async, readonly: each.propType })),
            methods: Array.map(callable, (each) =>
                Declaration.Method.make({
                    name: each.name,
                    type: nonEmpty(each.dataType),
                    static: each.propKind,
                    async: each.async,
                    parameters: Array.map(each.params, (argued, index) => Declaration.Parameter.make({ name: argued.name ?? `arguments[${index}]`, type: nonEmpty(argued.dataType), optional: argued.optional })),
                }),
            ),
        }),
    ];
};
const manifest = (text: string) => Effect.map(Schema.decodeEffect(Manifest)(text), (entries) => paged(Record.fromIterableWith(entries, entry)));
const documented = (each: (typeof Documented.Type)['methods'][number]) =>
    Declaration.Method.make({
        name: each.name,
        description: Option.fromNullOr(each.description),
        type: Option.fromNullOr(each.returns),
        version: Option.fromNullOr(each.since),
        parameters: Array.map(each.parameters, (argued) => Declaration.Parameter.make({ name: argued.Name, description: nonEmpty(argued.Description), type: nonEmpty(argued.Type) })),
    });
const documents = <E>(rows: Stream.Stream<Uint8Array, E>) =>
    Effect.map(Stream.runCollect(Stream.pipeThroughChannel(rows, Ndjson.decodeSchema(Documented)())), (pages) =>
        paged(
            Record.fromIterableWith(
                pages.filter(({ properties, methods }) => Array.isReadonlyArrayNonEmpty(properties) || Array.isReadonlyArrayNonEmpty(methods)),
                ({ name, description, properties, methods }): readonly [string, typeof Declaration.Page.Type] => [
                    name,
                    Declaration.cases.Class.make({
                        name,
                        description: Option.fromNullOr(description),
                        parents: [],
                        constructible: false,
                        fields: Array.map(properties, (each) => Declaration.Field.make({ name: each.Name, description: nonEmpty(each.Description), type: nonEmpty(each.Type), version: nonEmpty(each['Min Version']), readonly: each.Access })),
                        methods: Array.map(methods, documented),
                    }),
                ],
            ),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { declared, documents, manifest, omv, type Source };
