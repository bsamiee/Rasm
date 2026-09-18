// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, type Config, Console, Effect, FileSystem, Filter, identity, Match, Number, Option, Order, Path, type PlatformError, pipe, Record, Schema, String, Struct } from 'effect';
import type { ChildProcessSpawner } from 'effect/unstable/process';
import {
    type BinaryExpression,
    type CallExpression,
    ModuleDeclarationKind,
    Node,
    Project,
    ScriptKind,
    type SourceFile,
    type StatementStructures,
    StructureKind,
    SyntaxKind,
    ts,
    VariableDeclarationKind,
    Writers,
} from 'ts-morph';
import type { BridgeError } from '../errors.ts';
import { root } from '../jobs.ts';
import { absent, camel, dictionary, fourcc, MANIPULATION, pascal, SDEF_TYPES, type SdefClass, type SdefEnumeration, type SdefProperty, sorted } from '../sdef.ts';
import { AbsolutePath, TimeoutMs } from '../values.ts';
import { dispatch, type Site, site, Unavailable } from './channel.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Slot {
    readonly name: string;
    readonly access: Member['access'];
    readonly enumeration: Option.Option<string>;
    readonly literal: Option.Option<string>;
    readonly parameters: readonly { readonly name: string; readonly type: string }[];
}

interface DomClass {
    readonly name: string;
    readonly kind: 'creatable' | 'proxy' | 'collection';
    readonly slots: readonly Slot[];
}

interface Member {
    readonly name: string;
    readonly access: 'readonly' | 'readwrite' | 'method';
    readonly type: string;
    readonly parameters: Option.Option<Slot['parameters']>;
}

interface Delta {
    readonly kind: StructureKind.Interface | StructureKind.Class;
    readonly members: readonly Member[];
}

interface Table {
    readonly sdef: SdefClass;
    readonly members: Readonly<Record<string, SdefProperty>>;
}

interface Sources {
    readonly image: AbsolutePath;
    readonly sdefClasses: readonly SdefClass[];
    readonly sdefEnumerations: readonly SdefEnumeration[];
    readonly dom: Readonly<Record<string, DomClass>>;
    readonly domEnumerations: Readonly<Record<string, { readonly id: number; readonly members: Readonly<Record<string, number>> }>>;
    readonly known: Readonly<Record<string, { readonly derived: readonly string[]; readonly members: readonly string[]; readonly types: Readonly<Record<string, string>> }>>;
    readonly knownEnumerations: Readonly<Record<string, readonly string[]>>;
}

interface Names {
    readonly classes: Readonly<Record<string, string>>;
    readonly enumerations: Readonly<Record<string, string>>;
    readonly typings: (name: string) => string;
}

type Live = (typeof _RECONCILE)['response']['Type'];

// --- [CONSTANTS] -----------------------------------------------------------------------

const _RECONCILE_MS = 300_000;
const _LIVE_TYPES: Readonly<Record<string, string>> = {
    any: 'unknown',
    array: 'unknown[]',
    boolean: 'boolean',
    filespec: 'File',
    number: 'number',
    point: 'number[]',
    rect: 'number[]',
    string: 'string',
};

// --- [ERRORS] --------------------------------------------------------------------------

const GenerateError: Schema.TaggedUnion<{
    readonly resourceNotFound: Schema.TaggedStruct<'resourceNotFound', { readonly resource: Schema.Literals<readonly ['sdef', 'image', 'script', 'typings']>; readonly count: Schema.Number }>;
    readonly loweringHelper: Schema.TaggedStruct<'loweringHelper', { readonly helpers: Schema.$Record<Schema.String, Schema.$Array<Schema.String>> }>;
}> = Schema.TaggedUnion({
    resourceNotFound: { resource: Schema.Literals(['sdef', 'image', 'script', 'typings']), count: Schema.Number },
    loweringHelper: { helpers: Schema.Record(Schema.String, Schema.Array(Schema.String)) },
});

// --- [MODELS] --------------------------------------------------------------------------

const _strings = Schema.Array(Schema.String);

const _flags = Schema.Array(Schema.Boolean);

const _RECONCILE = {
    entry: 'reconcile',
    request: Schema.Struct({
        image: AbsolutePath,
        creatable: _strings,
        classes: Schema.Record(Schema.String, _strings),
        enumerations: Schema.Array(Schema.Struct({ name: Schema.String, members: _strings })),
    }),
    response: Schema.Struct({
        kind: Schema.Literal('reconciliation'),
        created: Schema.Json,
        classes: Schema.Record(
            Schema.String,
            Schema.Struct({
                properties: Schema.Array(
                    Schema.Struct({ name: Schema.String, type: Schema.Literals(['unknown', 'readonly', 'readwrite', 'createonly', 'method', 'parameter']), dataType: Schema.String }),
                ),
                methods: _strings,
                probed: _flags,
            }),
        ),
        enumerations: Schema.Array(Schema.Record(Schema.String, _flags)),
        globals: Schema.Array(Schema.Record(Schema.String, Schema.Boolean)),
        inherited: _strings,
        unavailable: Schema.Array(Unavailable),
    }),
};

// --- [RECORDS] -------------------------------------------------------------------------

const _byKey = <A>(self: Readonly<Record<string, A>>): readonly (readonly [string, A])[] => Array.sortWith(Record.toEntries(self), ([key]) => key, Order.String);

const _single = <A>(resource: (typeof GenerateError.cases.resourceNotFound.fields.resource)['Type'], found: readonly A[]): Effect.Effect<A, (typeof GenerateError)['Type']> =>
    Effect.fromOption(
        Option.flatMap(
            Option.liftPredicate(found, (rows) => rows.length === 1),
            Array.head,
        ),
        () => GenerateError.cases.resourceNotFound.make({ resource, count: found.length }),
    );

const _identifier = (name: string): boolean => Option.exists(Option.fromNullishOr(ts.parseIsolatedEntityName(name, ts.ScriptTarget.ES5)), ts.isIdentifier);

// --- [DOM_SCRIPT] ----------------------------------------------------------------------

const _stringOf = (node: Node | undefined): Option.Option<string> => Option.map(Option.liftPredicate(node, Node.isStringLiteral), (literal) => literal.getLiteralValue());

const _numberOf = (node: Node | undefined): Option.Option<number> => Option.map(Option.liftPredicate(node, Node.isNumericLiteral), (literal) => literal.getLiteralValue());

const _owner = (node: Node | undefined): Option.Option<string> =>
    pipe(
        Option.liftPredicate(node, Node.isPropertyAccessExpression),
        Option.filter((access) => access.getName() === 'prototype'),
        Option.map((access) => access.getExpression().getText()),
    );

const _enumSpec = (node: Node, parameter: Option.Option<string>): Option.Option<string> =>
    pipe(
        node.getDescendantsOfKind(SyntaxKind.CallExpression),
        Array.findFirst(
            (call) =>
                call.getExpression().getText() === '_enumSpec' &&
                Option.match(parameter, { onNone: () => true, onSome: (name) => Option.exists(Array.get(call.getArguments(), 1), (argument) => argument.getText() === name) }),
        ),
        Option.flatMap((call) => _stringOf(Option.getOrUndefined(Array.head(call.getArguments())))),
    );

const _assigned = <L extends Node>(node: BinaryExpression, target: (node: Node) => node is L): Option.Option<{ readonly left: L; readonly right: Node }> =>
    Option.all({
        left: Option.liftPredicate<Node, L>(node.getLeft(), target),
        right: Option.map(
            Option.liftPredicate(node, (binary) => binary.getOperatorToken().getKind() === SyntaxKind.EqualsToken),
            (binary) => binary.getRight(),
        ),
    });

const _field = (node: BinaryExpression): Option.Option<readonly [string, Slot]> =>
    pipe(
        _assigned(node, Node.isPropertyAccessExpression),
        Option.filter(({ left }) => Node.isThisExpression(left.getExpression())),
        Option.flatMap(({ left, right }) =>
            Option.map(Option.fromNullishOr(node.getFirstAncestorByKind(SyntaxKind.FunctionDeclaration)?.getName()), (owner) => [
                owner,
                {
                    name: left.getName(),
                    access: 'readwrite',
                    enumeration: Option.none(),
                    literal: Option.map(
                        Option.liftPredicate(
                            Node.isConditionalExpression(right) ? right.getWhenFalse() : right,
                            (value) => Node.isLiteralExpression(value) || Node.isTrueLiteral(value) || Node.isFalseLiteral(value),
                        ),
                        (value) => value.getType().getBaseTypeOfLiteralType().getText(),
                    ),
                    parameters: [],
                },
            ]),
        ),
    );

const _accessor = (call: CallExpression): Option.Option<readonly [string, Slot]> => {
    const [target, name, descriptor] = call.getArguments();
    const setter = Option.flatMap(Option.liftPredicate(descriptor, Node.isObjectLiteralExpression), (object) => Option.fromNullishOr(object.getProperty('set')));
    return Option.map(Option.all([Option.liftPredicate(call, (candidate) => candidate.getExpression().getText() === 'Object.defineProperty'), _owner(target), _stringOf(name)]), ([, owner, slot]) => [
        owner,
        { name: slot, access: Option.isSome(setter) ? 'readwrite' : 'readonly', enumeration: Option.flatMap(setter, (node) => _enumSpec(node, Option.none())), literal: Option.none(), parameters: [] },
    ]);
};

const _method = (node: BinaryExpression): Option.Option<readonly [string, Slot]> =>
    pipe(
        _assigned(node, Node.isPropertyAccessExpression),
        Option.filter(({ left }) => !left.getName().startsWith('_')),
        Option.flatMap(({ left, right }) => Option.all({ owner: _owner(left.getExpression()), body: Option.liftPredicate(right, Node.isFunctionExpression), name: Option.some(left.getName()) })),
        Option.map(({ owner, body, name }) => [
            owner,
            {
                name,
                access: 'method',
                enumeration: Option.none(),
                literal: Option.none(),
                parameters: Array.map(body.getParameters(), (parameter) => ({
                    name: parameter.getName(),
                    type: Option.getOrElse(_enumSpec(body, Option.some(parameter.getName())), () => 'unknown'),
                })),
            },
        ]),
    );

const _table = (assignments: readonly BinaryExpression[], table: string): readonly (readonly [string, Node])[] =>
    Array.filterMap(
        assignments,
        Filter.fromPredicateOption((node) =>
            pipe(
                _assigned(node, Node.isElementAccessExpression),
                Option.filter(({ left }) => left.getExpression().getText() === table),
                Option.flatMap(({ left, right }) => Option.map(_stringOf(left.getArgumentExpression()), (name) => [name, right] as const)),
            ),
        ),
    );

const _domScript = (source: SourceFile): Pick<Sources, 'dom' | 'domEnumerations'> => {
    const assignments = source.getDescendantsOfKind(SyntaxKind.BinaryExpression);
    const properties = source.getDescendantsOfKind(SyntaxKind.PropertyAssignment);
    const declarations = Record.fromIterableBy(source.getDescendantsOfKind(SyntaxKind.FunctionDeclaration), (declaration) => declaration.getNameOrThrow());
    const creatable = Array.filterMap(
        properties,
        Filter.fromPredicateOption((property) =>
            pipe(
                Option.liftPredicate(property, (row) => row.getName() === '_kind' && Option.contains(_stringOf(row.getInitializer()), 'creatable')),
                Option.flatMap((row) => Option.liftPredicate(row.getParent().getProperty('_className'), Node.isPropertyAssignment)),
                Option.flatMap((row) => _stringOf(row.getInitializer())),
            ),
        ),
    );
    const wrapped = Array.filterMap(
        properties,
        Filter.fromPredicateOption((property) =>
            pipe(
                Option.liftPredicate(property.getParent().getParent(), Node.isVariableDeclaration),
                Option.filter((declaration) => declaration.getName() === '_classMap'),
                Option.flatMap(() => _stringOf(property.getNameNode())),
                Option.flatMap((name) => Option.map(Record.get(declarations, name), (declaration) => [name, declaration] as const)),
            ),
        ),
    );
    const kinds: Readonly<Record<string, DomClass['kind']>> = Record.union(
        Record.fromIterableWith(creatable, (name): readonly [string, DomClass['kind']] => [name, 'creatable']),
        Record.fromIterableWith(wrapped, ([name, declaration]): readonly [string, DomClass['kind']] => [
            name,
            Array.some(declaration.getBodyOrThrow().getDescendantsOfKind(SyntaxKind.NewExpression), (node) => node.getExpression().getText() === 'Proxy') ? 'collection' : 'proxy',
        ]),
        identity,
    );
    const slots = Array.groupBy(
        [
            ...Array.filterMap(source.getDescendantsOfKind(SyntaxKind.CallExpression), Filter.fromPredicateOption(_accessor)),
            ...Array.filterMap(assignments, Filter.fromPredicateOption(_method)),
            ...Array.filterMap(assignments, Filter.fromPredicateOption(_field)),
        ],
        ([owner]) => owner,
    );
    const ids = Record.fromEntries(
        Array.filterMap(
            _table(assignments, '_enumTypeIds'),
            Filter.fromPredicateOption(([name, right]) => Option.map(_numberOf(right), (id) => [name, id] as const)),
        ),
    );
    return {
        dom: Record.map(kinds, (kind, name): DomClass => ({ name, kind, slots: Array.map(Array.flatten(Array.fromOption(Record.get(slots, name))), ([, slot]) => slot) })),
        domEnumerations: Record.fromEntries(
            Array.filterMap(
                _table(assignments, '_enumNameToId'),
                Filter.fromPredicateOption(([name, right]) =>
                    Option.map(Record.get(ids, name), (id) => [
                        name,
                        {
                            id,
                            members: Record.fromEntries(
                                Array.filterMap(
                                    right.getDescendantsOfKind(SyntaxKind.PropertyAssignment),
                                    Filter.fromPredicateOption((property) => Option.all([_stringOf(property.getNameNode()), _numberOf(property.getInitializer())] as const)),
                                ),
                            ),
                        },
                    ]),
                ),
            ),
        ),
    };
};

// --- [JOINS] ---------------------------------------------------------------------------

const _sdefMembers = (classes: readonly SdefClass[], row: SdefClass): Readonly<Record<string, SdefProperty>> =>
    Record.union(
        Record.fromIterableBy(row.property, (property) => camel(property.attributes.name)),
        Option.match(
            Option.flatMap(row.attributes.inherits, (parent) => Array.findFirst(classes, (candidate) => candidate.attributes.name === parent)),
            { onNone: () => ({}), onSome: (parent) => _sdefMembers(classes, parent) },
        ),
        identity,
    );

const _joined = (dom: Sources['dom'], tables: Readonly<Record<string, Table>>): Readonly<Record<string, Table>> => {
    const weights = Record.map(
        Array.groupBy(
            Array.flatMap(Record.values(tables), (table) => Record.keys(table.members)),
            identity,
        ),
        (rows) => 1 / rows.length,
    );
    return Record.filterMap(
        dom,
        Filter.fromPredicateOption((row) => {
            const names = Array.map(row.slots, Struct.get('name'));
            const ranked = pipe(
                Record.toEntries(tables),
                Array.map(([name, table]) => ({
                    table,
                    score:
                        Number.sumAll(
                            Array.filterMap(
                                Array.intersection(Record.keys(table.members), names),
                                Filter.fromPredicateOption((member) => Record.get(weights, member)),
                            ),
                        ) + (name === row.name ? 1 : 0),
                })),
                Array.sortWith(Struct.get('score'), Order.flip(Order.Number)),
            );
            return pipe(
                Array.head(ranked),
                Option.filter((best) => best.score > 0 && Option.exists(Array.get(ranked, 1), (next) => next.score < best.score)),
                Option.map(Struct.get('table')),
            );
        }),
    );
};

const _names = (sources: Sources, tables: Readonly<Record<string, Table>>, joined: Readonly<Record<string, Table>>, live: Live): Names => {
    const renamed = pipe(
        Record.toEntries(live.classes),
        Array.flatMap(([owner, record]) => Array.map(record.properties, (property) => ({ owner, property }))),
        Array.filterMap(
            Filter.fromPredicateOption(({ owner, property }) =>
                pipe(
                    Record.get(sources.known, owner),
                    Option.flatMap((row) => Record.get(row.types, property.name)),
                    Option.filter((text) => Record.has(sources.known, text)),
                    Option.map((text) => [property.dataType, text] as const),
                ),
            ),
        ),
        Record.fromEntries,
    );
    const aliases = Record.filter(renamed, (text, name) => Record.has(sources.dom, name) && !Record.has(sources.known, name) && text !== name);
    const typings = (name: string): string => Option.getOrElse(Record.get(aliases, name), () => name);
    return {
        classes: Record.union(
            Record.fromIterableWith(Record.toEntries(joined), ([name, table]) => [table.sdef.attributes.name, typings(name)]),
            Record.fromIterableWith(
                Array.filter(Record.toEntries(tables), ([name]) => Record.has(sources.known, name)),
                ([name, table]) => [table.sdef.attributes.name, name],
            ),
            identity,
        ),
        enumerations: pipe(
            Record.toEntries(sources.domEnumerations),
            Array.filterMap(
                Filter.fromPredicateOption(([name, row]) =>
                    Option.map(
                        Array.findFirst(sources.sdefEnumerations, (sdef) => fourcc(sdef.attributes.code) === row.id),
                        (sdef) => [sdef.attributes.code, name] as const,
                    ),
                ),
            ),
            Record.fromEntries,
        ),
        typings,
    };
};

// --- [DELTA] ---------------------------------------------------------------------------

const _members = (sources: Sources, names: Names, joined: Readonly<Record<string, Table>>, inherited: readonly string[], row: DomClass, live: Live['classes'][string]): readonly Member[] => {
    const sdefMembers = Option.map(Record.get(joined, row.name), Struct.get('members'));
    const sdefType = (table: Readonly<Record<string, string>>, property: SdefProperty): Option.Option<string> =>
        pipe(
            Option.map(property.attributes.type, (name) => ({ name, list: false })),
            Option.orElse(() => Option.map(Array.head(property.type), (type) => ({ name: type.attributes.type, list: Option.isSome(type.attributes.list) }))),
            Option.flatMap(({ name, list }) => Option.map(Record.get(table, name), (text) => (list ? `${text}[]` : text))),
        );
    const resolve = (slot: Option.Option<Slot>, sdef: Option.Option<SdefProperty>, dataType: Option.Option<string>): string =>
        pipe(
            Option.flatMap(slot, Struct.get('enumeration')),
            Option.orElse(() => Option.flatMap(sdef, (property) => sdefType(names.enumerations, property))),
            Option.orElse(() => Option.flatMap(sdef, (property) => sdefType(names.classes, property))),
            Option.orElse(() => Option.flatMap(dataType, (name) => Record.get(_LIVE_TYPES, name))),
            Option.orElse(() =>
                Option.map(
                    Option.filter(dataType, (name) => Record.has(sources.dom, name) || Record.has(sources.known, name)),
                    names.typings,
                ),
            ),
            Option.orElse(() => Option.flatMap(sdef, (property) => sdefType(SDEF_TYPES, property))),
            Option.orElse(() => Option.flatMap(slot, Struct.get('literal'))),
            Option.getOrElse(() => 'unknown'),
        );
    const described = Array.map(live.properties, (property) => ({
        name: property.name,
        access: Match.value(property.type).pipe(
            Match.withReturnType<Member['access']>(),
            Match.whenOr('readonly', 'createonly', () => 'readonly'),
            Match.whenOr('readwrite', 'unknown', 'parameter', () => 'readwrite'),
            Match.when('method', () => 'method'),
            Match.exhaustive,
        ),
        dataType: Option.some(property.dataType),
    }));
    const listed = Array.match(live.properties, {
        onEmpty: () =>
            Array.map(
                Array.filter(Array.zip(row.slots, live.probed), ([, present]) => present),
                ([slot]) => ({ name: slot.name, access: slot.access, dataType: Option.none<string>() }),
            ),
        onNonEmpty: () => [...described, ...Array.map(live.methods, (name) => ({ name, access: 'method' as const, dataType: Option.none<string>() }))],
    });
    return pipe(
        listed,
        Array.filter((member) => _identifier(member.name) && !Array.contains(inherited, member.name)),
        Array.dedupeWith((left, right) => left.name === right.name),
        Array.map((member): Member => {
            const slot = Array.findFirst(row.slots, (candidate) => candidate.name === member.name);
            return {
                name: member.name,
                access: member.access,
                ...Match.value(member.access).pipe(
                    Match.withReturnType<Pick<Member, 'type' | 'parameters'>>(),
                    Match.when('method', () => ({
                        type: 'unknown',
                        parameters: Option.map(
                            Option.filter(slot, (found) => found.access === 'method'),
                            Struct.get('parameters'),
                        ),
                    })),
                    Match.whenOr('readonly', 'readwrite', () => ({ type: resolve(slot, Option.flatMap(sdefMembers, Record.get(member.name)), member.dataType), parameters: Option.none() })),
                    Match.exhaustive,
                ),
            };
        }),
    );
};

const _lifted = (
    sources: Sources,
    tables: Readonly<Record<string, Table>>,
    joined: Readonly<Record<string, Table>>,
    names: Names,
    reached: Readonly<Record<string, readonly Member[]>>,
): Readonly<Record<string, readonly Member[]>> =>
    pipe(
        Record.toEntries(sources.known),
        Array.filterMap(
            Filter.fromPredicateOption(([name, base]) => {
                const children = Record.values(Record.filter(reached, (_, child) => Array.contains(base.derived, names.typings(child))));
                const sdefBase = Option.map(
                    Option.orElse(Record.get(joined, name), () => Record.get(tables, name)),
                    Struct.get('members'),
                );
                return pipe(
                    Option.liftPredicate(children, Array.isReadonlyArrayNonEmpty<readonly Member[]>),
                    Option.filter((rows) => rows.length > 1),
                    Option.map((rows) =>
                        Array.reduce(
                            rows,
                            Array.headNonEmpty(rows),
                            Array.intersectionWith<Member>((left, right) => left.name === right.name && left.type === right.type),
                        ),
                    ),
                    Option.map(Array.filter((row) => !Array.contains(base.members, row.name) && Option.exists(sdefBase, Record.has(row.name)))),
                    Option.flatMap(Option.liftPredicate(Array.isReadonlyArrayNonEmpty)),
                    Option.map((rows) => [name, rows] as const),
                );
            }),
        ),
        Record.fromEntries,
    );

// --- [GENERATE] ------------------------------------------------------------------------

const _sources = Effect.fnUntraced(function* (at: Site, project: Project, scripts: string) {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const resources = path.join(at.host.bundlePath, 'Contents', 'Resources');
    const listing = yield* fs.readDirectory(resources);
    const sdef = yield* Effect.flatMap(_single('sdef', Array.filter(listing, String.endsWith('.sdef'))), (name) => Effect.flatMap(fs.readFileString(path.join(resources, name)), dictionary));
    const images = sorted(Array.filter(listing, String.endsWith('.png')));
    const image = yield* Effect.fromOption(Array.head(images), () => GenerateError.cases.resourceNotFound.make({ resource: 'image', count: images.length }));
    const framework = path.join(at.host.bundlePath, 'Contents', 'Frameworks', 'AIUXPExtensionHostAPI.framework', 'Versions', 'A', 'AIUXPExtensionHostAPI');
    const script = yield* Effect.flatMap(fs.readFile(framework), (bytes) =>
        _single('script', Array.filter(String.split(new TextDecoder().decode(bytes), '\0'), String.includes('Generated from HostModel.xml'))),
    );
    const prelude = yield* Effect.map(fs.readFileString(path.join(scripts, 'prelude.ts')), (text) => project.createSourceFile('prelude.ts', text));
    const directive = yield* _single('typings', prelude.getTypeReferenceDirectives());
    const typings = yield* Effect.map(
        Effect.flatMap(path.fromFileUrl(new URL(import.meta.resolve(`${directive.getFileName()}/index.d.ts`))), (file) => fs.readFileString(file)),
        (text) => project.createSourceFile('adobe.d.ts', text),
    );
    return {
        image: AbsolutePath.make(path.join(resources, image)),
        sdefClasses: Array.filter(Array.flatMap(sdef.dictionary.suite, Struct.get('class')), (row) => Option.isNone(row.attributes.hidden)),
        sdefEnumerations: Array.filter(Array.flatMap(sdef.dictionary.suite, Struct.get('enumeration')), (row) => Option.isNone(row.attributes.hidden)),
        ..._domScript(project.createSourceFile('illustrator-dom.js', script)),
        known: Record.fromIterableWith(typings.getClasses(), (node) => [
            node.getNameOrThrow(),
            {
                derived: Array.map(node.getDerivedClasses(), (child) => child.getNameOrThrow()),
                members: Array.map(node.getType().getProperties(), (symbol) => symbol.getName()),
                types: Record.fromEntries(
                    Array.filterMap(
                        node.getProperties(),
                        Filter.fromPredicateOption((property) => Option.map(Option.fromNullishOr(property.getTypeNode()), (type) => [property.getName(), type.getText()] as const)),
                    ),
                ),
            },
        ]),
        knownEnumerations: Record.fromIterableWith(typings.getEnums(), (node) => [node.getName(), Array.map(node.getMembers(), (member) => member.getName())]),
    } satisfies Sources;
});

const generate: () => Effect.Effect<
    void,
    (typeof GenerateError)['Type'] | BridgeError | PlatformError.BadArgument | PlatformError.PlatformError | Schema.SchemaError | Config.ConfigError,
    FileSystem.FileSystem | Path.Path | ChildProcessSpawner.ChildProcessSpawner
> = Effect.fnUntraced(function* () {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const at = yield* site;
    const scripts = path.join(yield* root, 'apps', 'creative-cloud', 'illustrator-scripts');
    const project = new Project({ useInMemoryFileSystem: true, compilerOptions: { lib: ['lib.es5.d.ts'], types: [] }, manipulationSettings: MANIPULATION });
    yield* Effect.forEach(
        yield* fs.readDirectory(at.scripts),
        (name) =>
            Effect.map(
                fs.readFileString(path.join(at.scripts, name)),
                (text) => [name, Array.map(project.createSourceFile(name, text, { scriptKind: ScriptKind.JS }).getFunctions(), (declaration) => declaration.getNameOrThrow())] as const,
            ),
        { concurrency: 'unbounded' },
    ).pipe(
        Effect.map((rows) => Record.filter(Record.fromEntries(rows), Array.isReadonlyArrayNonEmpty)),
        Effect.filterOrFail(Record.isEmptyReadonlyRecord, (helpers) => GenerateError.cases.loweringHelper.make({ helpers })),
    );
    const sources = yield* _sources(at, project, scripts);
    const tables: Readonly<Record<string, Table>> = Record.fromIterableWith(sources.sdefClasses, (sdef) => [pascal(sdef.attributes.name), { sdef, members: _sdefMembers(sources.sdefClasses, sdef) }]);
    const candidates = pipe(
        Record.map(sources.domEnumerations, (row) => Record.keys(row.members)),
        Record.union(sources.knownEnumerations, (mine, theirs) => [...mine, ...theirs]),
        Record.map(sorted),
    );
    const live = yield* dispatch(at, TimeoutMs.make(_RECONCILE_MS), _RECONCILE, {
        image: sources.image,
        creatable: Array.map(
            Array.filter(Record.values(sources.dom), (row) => row.kind === 'creatable'),
            Struct.get('name'),
        ),
        classes: Record.map(sources.dom, (row) => Array.map(row.slots, Struct.get('name'))),
        enumerations: Array.map(Record.toEntries(candidates), ([name, rows]) => ({ name, members: rows })),
    });
    const joined = _joined(sources.dom, tables);
    const names = _names(sources, tables, joined, live);
    const reached = Record.filterMap(
        sources.dom,
        Filter.fromPredicateOption((row) => Option.map(Record.get(live.classes, row.name), (record) => _members(sources, names, joined, live.inherited, row, record))),
    );
    const lifted = _lifted(sources, tables, joined, names, reached);
    const liftedNames = pipe(Record.values(lifted), Array.flatten, Array.map(Struct.get('name')), Array.dedupe);
    const own = pipe(
        Record.toEntries(reached),
        Array.filterMap(
            Filter.fromPredicateOption(([name, rows]) => {
                const typed = names.typings(name);
                const known = Record.get(sources.known, typed);
                const excluded = Option.match(known, { onNone: () => liftedNames, onSome: (row) => [...row.members, ...liftedNames] });
                return Option.map(
                    Option.liftPredicate(
                        Array.filter(rows, (member) => !Array.contains(excluded, member.name)),
                        Array.isReadonlyArrayNonEmpty,
                    ),
                    (kept): readonly [string, Delta] => [typed, { kind: Option.match(known, { onNone: () => StructureKind.Class, onSome: () => StructureKind.Interface }), members: kept }],
                );
            }),
        ),
        Record.fromEntries,
    );
    const deltas: Readonly<Record<string, Delta>> = Record.union(
        Record.map(lifted, (rows): Delta => ({ kind: StructureKind.Interface, members: rows })),
        own,
        (lift, mine) => ({ kind: StructureKind.Interface, members: Array.appendAll(lift.members, mine.members) }),
    );
    const confirmedEnumerations = pipe(
        Array.flatMap(live.enumerations, Record.toEntries),
        Array.filterMap(
            Filter.fromPredicateOption(([name, flags]) =>
                Option.map(
                    Record.get(candidates, name),
                    (rows) =>
                        [
                            name,
                            Array.map(
                                Array.filter(Array.zip(rows, flags), ([, confirmed]) => confirmed),
                                ([member]) => member,
                            ),
                        ] as const,
                ),
            ),
        ),
        Record.fromEntries,
    );
    const enumerationDeltas = absent(confirmedEnumerations, sources.knownEnumerations);
    const delta = project.createSourceFile('illustrator.ts', {
        statements: [
            {
                kind: StructureKind.Module,
                name: 'global',
                declarationKind: ModuleDeclarationKind.Global,
                hasDeclareKeyword: true,
                statements: [
                    ...Array.map(_byKey(enumerationDeltas), ([name, rows]): StatementStructures => ({ kind: StructureKind.Enum, name, members: Array.map(rows, (member) => ({ name: member })) })),
                    ...Array.map(
                        _byKey(deltas),
                        ([typed, entry]): StatementStructures => ({
                            kind: entry.kind,
                            name: typed,
                            properties: Array.map(entry.members, (member) => {
                                const parameters = Array.join(Option.match(member.parameters, { onNone: () => ['...args: unknown[]'], onSome: Array.map((row) => `${row.name}?: ${row.type}`) }), ', ');
                                return {
                                    name: member.name,
                                    type: Match.value(member.access).pipe(
                                        Match.when('method', () => `(${parameters}) => ${member.type}`),
                                        Match.whenOr('readonly', 'readwrite', () => member.type),
                                        Match.exhaustive,
                                    ),
                                    isReadonly: member.access !== 'readwrite',
                                };
                            }),
                        }),
                    ),
                ],
            },
        ],
    });
    const dictionaries = {
        classes: Record.fromIterableWith(_byKey(reached), ([name, rows]) => [
            name,
            Record.fromIterableWith(Array.sortWith(rows, Struct.get('name'), Order.String), (member) => [member.name, { access: member.access, type: member.type }]),
        ]),
        enumerations: Record.fromEntries(_byKey(confirmedEnumerations)),
    };
    const table = project.createSourceFile('dictionary.ts', {
        statements: [
            ...Array.map(
                Record.toEntries(dictionaries),
                ([name, value]): StatementStructures => ({
                    kind: StructureKind.VariableStatement,
                    declarationKind: VariableDeclarationKind.Const,
                    declarations: [{ name, initializer: Writers.assertion(JSON.stringify(value, null, 4), 'const') }],
                }),
            ),
            { kind: StructureKind.ExportDeclaration, namedExports: Record.keys(dictionaries) },
        ],
    });
    yield* fs.writeFileString(path.join(scripts, 'illustrator.ts'), delta.getFullText());
    yield* fs.writeFileString(path.join(import.meta.dirname, 'dictionary.ts'), table.getFullText());
    const domNames = Record.keys(sources.dom);
    const domClasses = Record.values(sources.dom);
    const domEnumerationNames = Record.keys(sources.domEnumerations);
    const liveNames = Record.keys(live.classes);
    const reachedNames = Record.keys(reached);
    const dataTypes = Array.dedupe(Array.flatMap(Record.values(live.classes), (record) => Array.map(record.properties, Struct.get('dataType'))));
    const refuted = pipe(
        Record.toEntries(live.classes),
        Array.filterMap(
            Filter.fromPredicateOption(([name, record]) =>
                pipe(
                    Option.andThen(Option.liftPredicate(record.properties, Array.isReadonlyArrayNonEmpty), Record.get(sources.known, names.typings(name))),
                    Option.flatMap((row) =>
                        Option.liftPredicate(Array.difference(row.members, [...Array.map(record.properties, Struct.get('name')), ...record.methods, ...live.inherited]), Array.isReadonlyArrayNonEmpty),
                    ),
                    Option.map((missing) => [name, missing] as const),
                ),
            ),
        ),
        Record.fromEntries,
    );
    const rejectedMembers = Record.filterMap(
        sources.dom,
        Filter.fromPredicateOption((row) =>
            Option.flatMap(Record.get(reached, row.name), (rows) =>
                Option.liftPredicate(Array.difference(Array.map(row.slots, Struct.get('name')), Array.map(rows, Struct.get('name'))), Array.isReadonlyArrayNonEmpty),
            ),
        ),
    );
    const report = {
        sdef: {
            classes: sources.sdefClasses.length,
            enumerations: sources.sdefEnumerations.length,
            properties: Array.flatMap(sources.sdefClasses, Struct.get('property')).length,
            enumerators: Array.flatMap(sources.sdefEnumerations, Struct.get('enumerator')).length,
        },
        script: {
            classes: domNames.length,
            kinds: Record.map(Array.groupBy<DomClass, string>(domClasses, Struct.get('kind')), Array.length),
            members: Array.flatMap(domClasses, Struct.get('slots')).length,
            enumerations: domEnumerationNames.length,
            joined: Record.size(joined),
            unjoined: sorted(Array.difference(domNames, Record.keys(joined))),
            enumerationsUnjoined: sorted(Array.difference(domEnumerationNames, Record.values(names.enumerations))),
        },
        typings: {
            classes: Record.size(sources.known),
            enumerations: Record.size(sources.knownEnumerations),
            renamed: Record.fromIterableWith(
                Array.filter(reachedNames, (name) => names.typings(name) !== name),
                (name) => [name, names.typings(name)],
            ),
            outsideTypings: sorted(Array.filter(reachedNames, (name) => !Record.has(sources.known, names.typings(name)))),
        },
        live: {
            reached: reachedNames.length,
            unreached: sorted(Array.difference(domNames, liveNames)),
            outsideScript: sorted(Array.filter(Array.difference(liveNames, domNames), _identifier)),
            members: Array.flatten(Record.values(reached)).length,
            rejectedMembers,
            refutedTypings: refuted,
            dataTypesUnmapped: sorted(Array.filter(dataTypes, (name) => !(Record.has(_LIVE_TYPES, name) || Record.has(sources.dom, name) || Record.has(sources.known, name)))),
            enumerations: Record.size(confirmedEnumerations),
            enumerationsAbsent: sorted(Array.difference(Array.union(domEnumerationNames, Record.keys(sources.knownEnumerations)), Record.keys(confirmedEnumerations))),
            enumerators: Array.flatten(Record.values(confirmedEnumerations)).length,
            enumeratorsRefuted: absent(
                Record.filter(sources.knownEnumerations, (_, name) => Record.has(confirmedEnumerations, name)),
                confirmedEnumerations,
            ),
            globalsOutsideScript: sorted(
                Array.difference(
                    Array.flatMap(live.globals, (row) => Record.keys(Record.filter(row, identity))),
                    domEnumerationNames,
                ),
            ),
            unavailable: live.unavailable,
            created: live.created,
        },
        delta: {
            interfaces: Record.size(Record.filter(deltas, (row) => row.kind === StructureKind.Interface)),
            classes: Record.size(Record.filter(deltas, (row) => row.kind === StructureKind.Class)),
            lifted: Record.map(lifted, Array.map(Struct.get('name'))),
            members: Array.flatMap(Record.values(deltas), Struct.get('members')).length,
            enumerations: Record.size(enumerationDeltas),
            enumerators: Array.flatten(Record.values(enumerationDeltas)).length,
        },
    };
    yield* Console.log(JSON.stringify(report, null, 4));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { generate };
