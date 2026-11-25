import { readdirSync, readFileSync, statSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { getTouchingPropertyName, isParameterDeclaration, isPropertyAccessExpression, isSetAccessorDeclaration, isTypeAliasDeclaration } from '@typescript/native/ast';
import { createFileSystemLayer, createFileSystemWithLib, type FileSystemCallbacks, serverFS } from '@typescript/native/fs';
import { API, type Checker, type Snapshot, type Diagnostic, DiagnosticCategory, type Program, type Signature, SymbolFlags, type Symbol as TypeScriptSymbol, type Type, TypeFlags, TypeFormatFlags } from '@typescript/native/sync';
import { Array, Effect, Equal, Filter, Function, flow, Option, Predicate, Record, Result, Schema, SchemaTransformation, Stream, String, Struct } from 'effect';
import { Ndjson } from 'effect/encoding';
import { XMLParser } from 'fast-xml-parser';
import { Declaration } from '#/model';

// --- [TYPES] ---------------------------------------------------------------------------

interface DeclarationSource {
    readonly root: string;
    readonly types: readonly string[];
    readonly lib: readonly string[];
    readonly preamble: string;
    readonly enumeration: Option.Option<string>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const SCRIPT = '/script.ts';
const VERSIONS = ['minVersion', 'since'] as const;
const OPTIONS = ['--module', 'esnext', '--moduleResolution', 'bundler', '--strict', 'false', '--skipLibCheck', '--noEmit'] as const;
const ARRAY_TAGS = new Set(['classdef', 'elements', 'property', 'method', 'parameter', 'datatype']);
const PARSER = new XMLParser({ ignoreAttributes: false, attributeNamePrefix: '', textNodeName: 'text', parseTagValue: false, parseAttributeValue: false, isArray: (name) => ARRAY_TAGS.has(name) });

// --- [MODELS] --------------------------------------------------------------------------

const Flag = <const T extends string, const F extends string>(truthy: T, falsy: F) => Schema.Union([Schema.Literal(truthy).transform(true), Schema.Literal(falsy).transform(false)]);
const Range = Schema.TemplateLiteralParser([Schema.String, '(Range: ', Schema.Finite, ' to ', Schema.Finite, ')', Schema.String]);
const Text = Schema.Union([Schema.String, Schema.Struct({ text: Schema.String }).pipe(Schema.decodeTo(Schema.String, SchemaTransformation.transform({ decode: Struct.get('text'), encode: (text) => ({ text }) })))]);
const Datatype = Schema.Struct({ type: Text, array: Schema.optionalKey(Schema.Union([Schema.String, Schema.Struct({ size: Schema.String })])), value: Schema.optionalKey(Text), ...Struct.Record(['min', 'max'], Schema.optionalKey(Schema.FiniteFromString)) });
const DescriptionFields = { name: Schema.String, shortdesc: Schema.optionalKey(Text), datatype: Schema.optionalKey(Schema.NonEmptyArray(Datatype).annotate({ description: 'Alternatives where a value takes more than one type' })) };
class Argument extends Schema.Opaque<Argument, { readonly brand: unique symbol }>()(Schema.Struct({ ...DescriptionFields, optional: Schema.optionalKey(Schema.Literal('true').transform(true)) })) {}
const Property = Schema.Struct({ ...DescriptionFields, rwaccess: Schema.optionalKey(Schema.Literal('readonly').transform(true)) });
const MethodElement = Schema.Struct({ ...DescriptionFields, parameters: Schema.optionalKey(Schema.Struct({ parameter: Schema.Array(Argument) })) });
const Elements = Schema.Struct({ type: Schema.Literals(['instance', 'class', 'constructor', 'event']), property: Schema.optionalKey(Schema.Array(Property)), method: Schema.optionalKey(Schema.Array(MethodElement)) });
class Classdef extends Schema.Opaque<Classdef, { readonly brand: unique symbol }>()(
    Schema.Struct({ ...DescriptionFields, enumeration: Schema.optionalKey(Schema.Literal('true').transform(true)), superclass: Schema.optionalKey(Text), elements: Schema.Array(Elements).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) }),
) {}
class Omv extends Schema.Opaque<Omv, { readonly brand: unique symbol }>()(Schema.Struct({ dictionary: Schema.Struct({ package: Schema.Struct({ classdef: Schema.Array(Classdef) }) }) })) {}
const BooleanText = Flag('true', 'false');
const Kind = Flag('class', 'instance');
const Callable = Schema.Struct({ name: Schema.String, dataType: Schema.String, propKind: Kind, async: BooleanText, params: Schema.Array(Schema.Struct({ name: Schema.optionalKey(Schema.String), dataType: Schema.String, optional: BooleanText })) });
const Constructor = Schema.Struct({ propKind: Schema.Literal('constructor') });
class Entry extends Schema.Opaque<Entry, { readonly brand: unique symbol }>()(
    Schema.Struct({
        name: Schema.String,
        properties: Schema.NullOr(Schema.Array(Schema.Struct({ name: Schema.String, dataType: Schema.String, propKind: Kind, propType: Flag('readOnly', 'readWrite'), async: BooleanText }))),
        methods: Schema.NullOr(Schema.Array(Schema.Union([Callable, Constructor]))),
    }),
) {}
const Manifest = Schema.fromJsonString(Schema.Array(Entry));
class DocumentationRow extends Schema.Opaque<DocumentationRow, { readonly brand: unique symbol }>()(
    Schema.Struct({
        name: Schema.String,
        description: Schema.NullOr(Schema.String),
        properties: Schema.Array(Schema.Struct({ Name: Schema.String, Type: Schema.String, Access: Flag('R', 'RW'), 'Min Version': Schema.String, Description: Schema.String })),
        methods: Schema.Array(Schema.Struct({ name: Schema.String, returns: Schema.NullOr(Schema.String), since: Schema.NullOr(Schema.String), description: Schema.NullOr(Schema.String), parameters: Schema.Array(Schema.Struct({ Name: Schema.String, Type: Schema.String, Description: Schema.String })) })),
    }),
) {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const statPath = (path: string) => statSync(path, { throwIfNoEntry: false });
const HOST: FileSystemCallbacks = {
    directoryExists: (path) => statPath(path)?.isDirectory() ?? serverFS.useOS,
    fileExists: (path) => statPath(path)?.isFile() ?? serverFS.useOS,
    getAccessibleEntries: (path) =>
        Option.filter(Option.fromUndefinedOr(statPath(path)), (stats) => stats.isDirectory()).pipe(
            Option.map(() => Array.partition(readdirSync(path), (name) => (statPath(join(path, name))?.isDirectory() === true ? Result.fail(name) : Result.succeed(name)))),
            Option.match({ onNone: () => serverFS.useOS, onSome: ([files, directories]) => ({ files, directories, symlinks: undefined }) }),
        ),
    readFile: (path) => (statPath(path)?.isFile() === true ? readFileSync(path, 'utf-8') : serverFS.useOS),
    realpath: serverFS.identity,
    stat: serverFS.fakeStat,
    writeFile: serverFS.error,
    removeFile: serverFS.error,
};
const formatScript = (preamble: string, program: string) => `${preamble}\n${program}\nexport {};\n`;
const hasFlag = (symbol: TypeScriptSymbol, flags: SymbolFlags) => (symbol.flags & flags) !== 0;
const formatType = (checker: Checker, type: Type) => checker.typeToString(type, undefined, TypeFormatFlags.NoTruncation);
const isPromise = (checker: Checker, type: Type) => Option.exists(Option.fromUndefinedOr(checker.getAwaitedType(type)), (awaited) => awaited.id !== type.id);
const readBounds = (description: Option.Option<string>) => {
    const range = Option.flatMap(description, Schema.decodeUnknownOption(Range));
    return { minimum: Option.map(range, (parsed) => parsed[2]), maximum: Option.map(range, (parsed) => parsed[4]) };
};
const readTags = (checker: Checker, symbol: TypeScriptSymbol) => ({
    name: symbol.name,
    description: Option.liftPredicate(checker.getDocumentationCommentOfSymbol(symbol), String.isNonEmpty),
    version: Array.findFirst(checker.getJsDocTagsOfSymbol(symbol), ({ name }) => Array.contains(VERSIONS, name)).pipe(Option.flatMapNullishOr(({ text }) => text)),
});
const isOptional = (parameter: TypeScriptSymbol) => Option.exists(Option.fromUndefinedOr(parameter.valueDeclaration?.resolve()), (node) => isParameterDeclaration(node) && (node.questionToken ?? node.initializer ?? node.dotDotDotToken) !== undefined);
const method = (checker: Checker, tags: ReturnType<typeof readTags>, statics: boolean) => (signature: Signature) =>
    Declaration.Method.make({
        ...tags,
        type: Option.some(formatType(checker, signature.getReturnType())),
        static: statics,
        async: isPromise(checker, signature.getReturnType()),
        parameters: Array.map(signature.getParameters(), (parameter) => {
            const { description } = readTags(checker, parameter);
            return Declaration.Parameter.make({ name: parameter.name, description, ...readBounds(description), type: Option.some(formatType(checker, checker.getTypeOfSymbol(parameter))), optional: isOptional(parameter) });
        }),
    });
const members = (checker: Checker, type: Type, statics: boolean) => {
    const [fields, methods] = Array.partition(
        Array.filter(type.getProperties(), (symbol) => !(statics && hasFlag(symbol, SymbolFlags.Prototype))),
        (symbol) => {
            const value = checker.getTypeOfSymbol(symbol);
            const tags = readTags(checker, symbol);
            const signatures = hasFlag(symbol, SymbolFlags.Method) ? value.getCallSignatures() : [];
            return Array.isReadonlyArrayNonEmpty(signatures)
                ? Result.fail(Array.map(signatures, method(checker, tags, statics)))
                : Result.succeed(Declaration.Field.make({ ...tags, ...readBounds(tags.description), type: Option.some(formatType(checker, value)), static: statics, async: isPromise(checker, value), readonly: checker.isReadonlySymbol(symbol), optional: hasFlag(symbol, SymbolFlags.Optional) }));
        },
    );
    return { fields, methods: Array.flatten(methods) };
};
const listExports = (checker: Checker, symbol: TypeScriptSymbol): readonly TypeScriptSymbol[] =>
    Array.flatMap(checker.getExportsOfModule(symbol), (each) => [each, ...(hasFlag(each, SymbolFlags.Module) && !hasFlag(each, SymbolFlags.Variable | SymbolFlags.Class | SymbolFlags.Function | SymbolFlags.Enum) ? listExports(checker, each) : [])]);
const classify = (checker: Checker, base: Option.Option<Type>) => (symbol: TypeScriptSymbol) => {
    const enumerations = hasFlag(symbol, SymbolFlags.Enum) || (hasFlag(symbol, SymbolFlags.Variable) && Option.exists(base, (enumeration) => checker.isTypeAssignableTo(checker.getTypeOfSymbol(symbol), enumeration)));
    const isDeclaration = enumerations || (hasFlag(symbol, SymbolFlags.Class | SymbolFlags.Interface | SymbolFlags.TypeAlias) && Array.isReadonlyArrayNonEmpty(checker.getDeclaredTypeOfSymbol(symbol).getProperties()));
    return isDeclaration ? Result.succeed({ symbol, enumerations }) : Result.failVoid;
};
const readEnumeration = (checker: Checker, symbol: TypeScriptSymbol, base: Option.Option<Type>) => {
    const baseNames = new Set(Option.match(base, { onNone: Array.empty<string>, onSome: (type) => Array.map(type.getProperties(), Struct.get('name')) }));
    const entries = hasFlag(symbol, SymbolFlags.Enum) ? checker.getExportsOfModule(symbol) : Array.filter(checker.getTypeOfSymbol(symbol).getProperties(), ({ name }) => !baseNames.has(name));
    return Declaration.cases.Enumeration.make({
        ...readTags(checker, symbol),
        members: Array.map(entries, (each) => ({ name: each.name, description: readTags(checker, each).description, value: Option.fromUndefinedOr(each.valueDeclaration?.resolve()).pipe(Option.flatMapNullishOr((node) => checker.getConstantValue(node))) })),
    });
};
const readClass = (checker: Checker, symbol: TypeScriptSymbol) => {
    const declaredType = checker.getDeclaredTypeOfSymbol(symbol);
    const constructor = hasFlag(symbol, SymbolFlags.Class) ? Option.some(checker.getTypeOfSymbol(symbol)) : Option.none();
    const own = members(checker, declaredType, false);
    const statics = Option.match(constructor, { onNone: () => ({ fields: [], methods: [] }), onSome: (type) => members(checker, type, true) });
    return Declaration.cases.Class.make({
        ...readTags(checker, symbol),
        parents: Array.map(declaredType.getBaseTypes() ?? [], (parent) => formatType(checker, parent)),
        constructible: Option.exists(constructor, (type) => Array.isReadonlyArrayNonEmpty(type.getConstructSignatures())),
        fields: [...own.fields, ...statics.fields],
        methods: [...own.methods, ...statics.methods],
    });
};
const readModel = (checker: Checker, kept: Record.ReadonlyRecord<string, { readonly symbol: TypeScriptSymbol; readonly enumerations: boolean }>) => {
    const findOwner = (type: Type) => Array.findFirst([type, ...(type.getBaseTypes() ?? [])], (each) => Option.filter(Option.fromUndefinedOr(each.getSymbol()?.name), (name) => Record.has(kept, name)));
    const listClasses = (type: Type) => {
        const constituents = type.isUnionType() ? type.getTypes() : [type];
        return constituents.some(({ flags }) => (flags & TypeFlags.StringLike) !== 0) ? [] : Array.dedupe(Array.filterMap(constituents, flow(findOwner, Result.fromOption(Function.constVoid))));
    };
    const readSetterType = (symbol: TypeScriptSymbol) =>
        Array.findFirst(symbol.declarations, (handle) => Option.filter(Option.fromUndefinedOr(handle.resolve()), isSetAccessorDeclaration).pipe(Option.flatMapNullishOr(({ parameters }) => parameters[0]?.type))).pipe(Option.map((node) => checker.getTypeFromTypeNode(node)));
    const field = (symbol: TypeScriptSymbol) => {
        const read = checker.getTypeOfSymbol(symbol);
        return {
            settable: !checker.isReadonlySymbol(symbol),
            collection: Option.fromUndefinedOr(read.getSymbol()).pipe(
                Option.flatMap((type) => Array.findFirst(checker.getJsDocTagsOfSymbol(type), ({ name }) => name === 'collection')),
                Option.flatMapNullishOr(({ text }) => text),
            ),
            array: read.isTypeReference() && checker.isArrayType(read) ? Option.flatMap(Array.head(checker.getTypeArguments(read)), findOwner) : Option.none(),
            classes: Option.match(readSetterType(symbol), { onNone: Array.empty<string>, onSome: listClasses }),
            ...readBounds(Option.liftPredicate(checker.getDocumentationCommentOfSymbol(symbol), String.isNonEmpty)),
        };
    };
    const parameters = (symbol: TypeScriptSymbol) => Option.match(Array.head(checker.getTypeOfSymbol(symbol).getCallSignatures()), { onNone: Array.empty<readonly string[]>, onSome: (signature) => Array.map(signature.getParameters(), (parameter) => listClasses(checker.getTypeOfSymbol(parameter))) });
    return {
        classes: Record.filterMap(kept, ({ symbol, enumerations }) => {
            const [fields, methods] = Array.partition(checker.getDeclaredTypeOfSymbol(symbol).getProperties(), (each) => (hasFlag(each, SymbolFlags.Method) ? Result.fail(each) : Result.succeed(each)));
            return enumerations ? Result.failVoid : Result.succeed({ fields: Record.fromIterableWith(fields, (each) => [each.name, field(each)] as const), methods: Record.fromIterableWith(methods, (each) => [each.name, parameters(each)] as const) });
        }),
        enumerations: Record.keys(Record.filter(kept, Struct.get('enumerations'))),
    };
};
const readName = (symbol: TypeScriptSymbol) => (hasFlag(symbol, SymbolFlags.TypeLiteral) ? Option.filter(Option.fromUndefinedOr(symbol.declarations[0]?.resolve()?.parent), isTypeAliasDeclaration).pipe(Option.map((node) => node.name.text)) : Option.some(symbol.name));
const toFinding = (program: Program, checker: Checker, lines: number) => (diagnostic: Diagnostic) => ({
    line: Option.map(Option.fromUndefinedOr(diagnostic.startPosition), ({ line }) => line - lines + 1),
    code: diagnostic.code,
    text: diagnostic.text,
    declared: Option.fromUndefinedOr(program.getSourceFile(SCRIPT)).pipe(
        Option.flatMap((source) => Option.liftPredicate(getTouchingPropertyName(source, diagnostic.pos).parent, isPropertyAccessExpression)),
        Option.flatMapNullishOr(({ expression }) => {
            const type = checker.getTypeAtLocation(expression);
            return type.getAliasSymbol() ?? type.getSymbol() ?? checker.getSymbolAtLocation(expression);
        }),
        Option.flatMap(readName),
    ),
});
const readDeclaration =
    (checker: Checker, base: Option.Option<Type>) =>
    ({ symbol, enumerations }: { readonly symbol: TypeScriptSymbol; readonly enumerations: boolean }) =>
        enumerations ? readEnumeration(checker, symbol, base) : readClass(checker, symbol);
const makeChecker = (base: Snapshot, id: Parameters<Snapshot['getProject']>[0], preamble: string) =>
    Effect.fnUntraced(function* (code: string) {
        const next = yield* Effect.acquireDisposable(Effect.sync(() => base.update({ fileSystem: createFileSystemLayer([[SCRIPT, formatScript(preamble, code)]]), fileNotifications: { changed: [SCRIPT] }, ensurePrograms: true })));
        const project = yield* Effect.orDie(Effect.fromNullishOr(next.getProject(id)));
        return [...project.program.getSyntacticDiagnostics(SCRIPT), ...project.program.getSemanticDiagnostics(SCRIPT)].filter(({ category }) => Equal.equals(category, DiagnosticCategory.Error)).map(toFinding(project.program, project.checker, preamble.split('\n').length));
    }, Effect.scoped);
const listDeclarations = (checker: Checker, files: readonly TypeScriptSymbol[], base: Option.Option<string>) => {
    const table = Record.fromIterableBy(
        Array.flatMap(files, (symbol) => listExports(checker, symbol)),
        Struct.get('name'),
    );
    const enumeration = Option.flatMap(base, (name) => Record.get(table, name)).pipe(Option.map((symbol) => checker.getTypeOfSymbol(symbol)));
    const candidates = Record.filterMap(table, classify(checker, enumeration));
    const names = new Set(Record.keys(Record.filter(candidates, Struct.get('enumerations'))));
    return { enumeration, kept: Record.filter(candidates, ({ symbol, enumerations }) => enumerations || !(checker.getDeclaredTypeOfSymbol(symbol).getBaseTypes() ?? []).some((parent) => Option.exists(Option.fromUndefinedOr(parent.getSymbol()?.name), (name) => names.has(name)))) };
};
const readTypeScript = Effect.fnUntraced(function* (compiler: Option.Option<{ readonly executable: string; readonly library: string }>, modules: string) {
    const api = yield* Effect.acquireDisposable(Effect.sync(() => new API({ cwd: '/', fs: HOST, ...Record.getSomes({ tsserverPath: Option.map(compiler, Struct.get('executable')) }) })));
    const mounts = { '/node_modules': { target: modules, host: true } };
    const libraries = Option.match(compiler, { onNone: () => ({ symlinks: mounts }), onSome: ({ executable, library }) => ({ defaultLibraryPath: dirname(executable), symlinks: { ...mounts, [dirname(executable)]: { target: library, host: true } } }) });
    return Effect.fnUntraced(function* (source: DeclarationSource) {
        const packageRoot = join(modules, source.root);
        const { options } = api.parseCommandLine([...OPTIONS, '--lib', source.lib.join(','), ...Array.flatMap(source.types, (type) => ['--types', type])]);
        const base = api.createSnapshot({
            fileSystem: createFileSystemWithLib([[SCRIPT, formatScript(source.preamble, '')]], libraries),
            createPrograms: [{ rootFiles: [SCRIPT], compilerOptions: options }],
        });
        const [{ id }] = base.operation.createdPrograms;
        const { checker, program } = yield* Effect.orDie(Effect.fromNullishOr(base.getProject(id)));
        const files = program.getSourceFileNames().filter((path) => path.startsWith(packageRoot));
        const declarations = yield* Effect.cached(Effect.sync(() => listDeclarations(checker, Array.filter(checker.getSymbolOfSourceFile(files), Predicate.isNotUndefined), source.enumeration)));
        const check = makeChecker(base, id, source.preamble);
        return {
            check,
            catalog: yield* Effect.cached(Effect.map(declarations, ({ enumeration, kept }) => Declaration.catalog(kept, readDeclaration(checker, enumeration), Struct.get('enumerations')))),
            model: Effect.map(declarations, ({ kept }) => readModel(checker, kept)),
        };
    });
});

const nonEmpty = (text: string | undefined) => Option.filter(Option.map(Option.fromUndefinedOr(text), String.trim), String.isNonEmpty);
const readDatatype = (datatypes: Classdef['datatype']) => ({
    type: Option.map(Option.fromUndefinedOr(datatypes), (alternatives) => Array.map(alternatives, ({ type, array }) => (array === undefined ? type : `${type}[]`)).join(' | ')),
    minimum: Array.findFirst(datatypes ?? [], ({ min }) => Option.fromUndefinedOr(min)),
    maximum: Array.findFirst(datatypes ?? [], ({ max }) => Option.fromUndefinedOr(max)),
});
const parameter = (each: Argument) => Declaration.Parameter.make({ name: each.name, description: nonEmpty(each.shortdesc), ...readDatatype(each.datatype), optional: each.optional ?? false });
const hasGroup =
    (type: Classdef['elements'][number]['type']) =>
    ({ elements }: Classdef) =>
        elements.some((group) => group.type === type);
const classdef = (constructible: Predicate.Predicate<Classdef>) => (definition: Classdef) => {
    const { name, shortdesc, enumeration, superclass, elements } = definition;
    const description = nonEmpty(shortdesc);
    const properties = Array.flatMap(elements, (group) => group.property ?? []);
    const methods = Array.flatMap(elements, (group) => (group.type === 'constructor' ? [] : (group.method ?? [])));
    const page =
        enumeration === true
            ? Declaration.cases.Enumeration.make({ name, description, members: Array.map(properties, (each) => ({ name: each.name, description: nonEmpty(each.shortdesc), value: Array.findFirst(each.datatype ?? [], ({ value }) => Option.fromUndefinedOr(value)) })) })
            : Declaration.cases.Class.make({
                  name,
                  description,
                  parents: Array.fromNullishOr(superclass),
                  constructible: constructible(definition),
                  fields: Array.map(properties, (each) => Declaration.Field.make({ name: each.name, description: nonEmpty(each.shortdesc), ...readDatatype(each.datatype), readonly: each.rwaccess ?? false })),
                  methods: Array.map(methods, (each) => Declaration.Method.make({ name: each.name, description: nonEmpty(each.shortdesc), ...readDatatype(each.datatype), parameters: Array.map(each.parameters?.parameter ?? [], parameter) })),
              });
    return [name, page] as const;
};
const classdefs = ({ dictionary }: Omv) => Array.map(dictionary.package.classdef, classdef(dictionary.package.classdef.some(hasGroup('constructor')) ? hasGroup('constructor') : Predicate.not(hasGroup('instance'))));
const catalogPages = <P>(pages: Record.ReadonlyRecord<string, P>) => Declaration.catalog(pages, Function.identity, Declaration.guards.Enumeration);
const readOmv = (texts: Array.NonEmptyReadonlyArray<string>) => Effect.forEach(texts, (text) => Schema.decodeUnknownEffect(Omv)(PARSER.parse(text))).pipe(Effect.map((dictionaries) => catalogPages(Record.fromEntries(Array.flatMap(dictionaries, classdefs)))));
const entry = ({ name, properties, methods }: Entry) => {
    const [callable, constructors] = Array.partition(methods ?? [], Filter.fromPredicate(Schema.is(Callable)));
    const fields = Array.map(properties ?? [], (each) => Declaration.Field.make({ name: each.name, type: nonEmpty(each.dataType), static: each.propKind, async: each.async, readonly: each.propType }));
    const methodDeclarations = Array.map(callable, (each) => {
        const parameters = Array.map(each.params, (argument, index) => Declaration.Parameter.make({ name: argument.name ?? `arguments[${index}]`, type: nonEmpty(argument.dataType), optional: argument.optional }));
        return Declaration.Method.make({ name: each.name, type: nonEmpty(each.dataType), static: each.propKind, async: each.async, parameters });
    });
    return [name, Declaration.cases.Class.make({ name, parents: [], constructible: Array.isReadonlyArrayNonEmpty(constructors), fields, methods: methodDeclarations })] as const;
};
const readManifest = (text: string) => Effect.map(Schema.decodeEffect(Manifest)(text), (entries) => catalogPages(Record.fromIterableWith(entries, entry)));
const readMethod = (each: DocumentationRow['methods'][number]) =>
    Declaration.Method.make({
        name: each.name,
        description: Option.fromNullOr(each.description),
        type: Option.fromNullOr(each.returns),
        version: Option.fromNullOr(each.since),
        parameters: Array.map(each.parameters, (argument) => Declaration.Parameter.make({ name: argument.Name, description: nonEmpty(argument.Description), type: nonEmpty(argument.Type) })),
    });
const readDocuments = <E>(rows: Stream.Stream<Uint8Array, E>) =>
    rows.pipe(
        Stream.pipeThroughChannel(Ndjson.decodeSchema(DocumentationRow)()),
        Stream.filter(({ properties, methods }) => Array.isReadonlyArrayNonEmpty(properties) || Array.isReadonlyArrayNonEmpty(methods)),
        Stream.map(({ name, description, properties, methods }) => {
            const fields = Array.map(properties, (each) => Declaration.Field.make({ name: each.Name, description: nonEmpty(each.Description), type: nonEmpty(each.Type), version: nonEmpty(each['Min Version']), readonly: each.Access }));
            return [name, Declaration.cases.Class.make({ name, description: Option.fromNullOr(description), parents: [], constructible: false, fields, methods: Array.map(methods, readMethod) })] as const;
        }),
        Stream.runCollect,
        Effect.map((entries) => catalogPages(Record.fromEntries(entries))),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { type DeclarationSource, readDocuments, readManifest, readOmv, readTypeScript };
