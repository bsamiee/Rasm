// --- [IMPORTS] -------------------------------------------------------------------------

import {
    Array,
    Console,
    Effect,
    FileSystem,
    Filter,
    Function,
    flow,
    HashSet,
    identity,
    Match,
    Number,
    Option,
    Order,
    Path,
    type PlatformError,
    Predicate,
    pipe,
    Record,
    Result,
    Schema,
    String,
    Struct,
} from 'effect';
import { ChildProcess, type ChildProcessSpawner } from 'effect/unstable/process';
import {
    type ClassDeclaration,
    type ExpressionWithTypeArguments,
    type GetAccessorDeclarationStructure,
    type InterfaceDeclaration,
    type InterfaceDeclarationStructure,
    type MethodSignatureStructure,
    type OptionalKind,
    Project,
    type PropertySignatureStructure,
    type SetAccessorDeclarationStructure,
    StructureKind,
    VariableDeclarationKind,
    type WriterFunction,
} from 'ts-morph';
import { NonZeroExit } from '../errors.ts';
import { bundle } from '../hosts.ts';
import { reply } from '../osascript.ts';
import { camel, constant, dictionary, fourcc, MANIPULATION, pascal, SDEF_TYPES, type SdefClass, type SdefProperty, sorted } from '../sdef.ts';
import { HOSTS } from '../values.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Declaration extends InterfaceDeclarationStructure {
    readonly extends: string[];
    readonly properties: OptionalKind<PropertySignatureStructure>[];
    readonly getAccessors: OptionalKind<GetAccessorDeclarationStructure>[];
    readonly setAccessors: OptionalKind<SetAccessorDeclarationStructure>[];
    readonly methods: OptionalKind<MethodSignatureStructure>[];
}

interface Slot {
    readonly name: string;
    readonly type: string;
    readonly description: string;
}

interface Lookup {
    readonly tables: Readonly<Record<string, Readonly<Record<string, Slot>>>>;
    readonly parents: Readonly<Record<string, readonly string[]>>;
}

type GenerateError = (typeof GenerateError)['Type'];

// --- [CONSTANTS] -----------------------------------------------------------------------

const _TYPES: Readonly<Record<string, string>> = { ...SDEF_TYPES, any: 'any', file: 'File | string', number: 'number', record: 'object', specifier: 'any' };

// --- [ERRORS] --------------------------------------------------------------------------

const GenerateError: Schema.TaggedUnion<{
    readonly hostNotInstalled: Schema.TaggedStruct<'hostNotInstalled', { readonly cause: Schema.Defect }>;
    readonly sdefNotGenerated: Schema.TaggedStruct<'sdefNotGenerated', { readonly cause: typeof NonZeroExit }>;
    readonly sdefNotDecodable: Schema.TaggedStruct<'sdefNotDecodable', { readonly cause: Schema.Defect }>;
    readonly typingsNotDecodable: Schema.TaggedStruct<'typingsNotDecodable', { readonly cause: Schema.Defect }>;
}> = Schema.TaggedUnion({
    hostNotInstalled: { cause: Schema.Defect() },
    sdefNotGenerated: { cause: NonZeroExit },
    sdefNotDecodable: { cause: Schema.Defect() },
    typingsNotDecodable: { cause: Schema.Defect() },
});

// --- [NAMES] ---------------------------------------------------------------------------

const _constant = (name: string): string =>
    Match.value(name).pipe(
        Match.when('default', () => 'DEFAULT_VALUE'),
        Match.orElse(constant),
    );

const _mentioned = (description: string): readonly string[] =>
    Array.filterMap(
        Array.fromIterable(description.matchAll(/(?<name>[A-Z][A-Za-z0-9]*) enumerator/gu)),
        Filter.fromPredicateOption((found) => Option.fromNullishOr(found.groups?.['name'])),
    );

const _typeNames = (property: SdefProperty): readonly string[] => [...Option.toArray(property.attributes.type), ...Array.map(property.type, (row) => row.attributes.type)];

// --- [DECLARATIONS] --------------------------------------------------------------------

const _text = (type: string | WriterFunction | undefined): string => Option.getOrElse(Option.liftPredicate(type, Predicate.isString), () => 'any');

const _description = (docs: InterfaceDeclarationStructure['docs']): string =>
    Array.join(
        Array.map(Array.flatten(Array.fromNullishOr(docs)), (doc) =>
            Match.value(doc).pipe(
                Match.when(Predicate.isString, String.trim),
                Match.orElse((structure) => String.trim(_text(structure.description))),
            ),
        ),
        ' ',
    );

const _union = (names: HashSet.HashSet<string>, type: string | WriterFunction | undefined): string =>
    pipe(
        String.split(_text(type), ' & '),
        Option.liftPredicate((parts) => parts.length > 1 && Array.every(parts, (part) => HashSet.has(names, part))),
        Option.map(Array.join(' | ')),
        Option.getOrElse(() => _text(type)),
    );

const _accepted = (names: HashSet.HashSet<string>, type: string | WriterFunction | undefined): string =>
    pipe(
        _union(names, type),
        Option.liftPredicate((text) => Array.contains(String.split(text, ' | '), 'File')),
        Option.match({ onNone: () => _union(names, type), onSome: (text) => `${text} | string` }),
    );

const _declaration =
    (names: HashSet.HashSet<string>) =>
    (name: string, parents: readonly ExpressionWithTypeArguments[], node: ClassDeclaration | InterfaceDeclaration): Declaration => ({
        kind: StructureKind.Interface,
        name,
        isExported: true,
        docs: Array.map(node.getJsDocs(), (doc) => doc.getStructure()),
        extends: Array.map(parents, (parent) => parent.getText()),
        properties: Array.map(node.getProperties(), (property) => {
            const structure = property.getStructure();
            return { ...Struct.pick(structure, ['name', 'isReadonly', 'hasQuestionToken', 'docs']), type: _union(names, structure.type) };
        }),
        getAccessors: Array.map(node.getGetAccessors(), (accessor) => {
            const structure = accessor.getStructure();
            return { ...structure, returnType: _union(names, structure.returnType) };
        }),
        setAccessors: Array.map(node.getSetAccessors(), (accessor) => {
            const structure = accessor.getStructure();
            return { ...structure, parameters: Array.map(Array.flatten(Array.fromNullishOr(structure.parameters)), (parameter) => ({ ...parameter, type: _accepted(names, parameter.type) })) };
        }),
        methods: Array.map(node.getMethods(), (method) => {
            const structure = method.getStructure();
            return {
                ...Struct.pick(structure, ['docs', 'hasQuestionToken']),
                name: method.getName(),
                parameters: Array.map(Array.flatten(Array.fromNullishOr(structure.parameters)), (parameter) => ({ ...parameter, type: _accepted(names, parameter.type) })),
                returnType: _union(names, structure.returnType),
            };
        }),
    });

const _members = (row: Declaration): readonly Slot[] => [
    ...Array.map(row.properties, (field) => ({ name: field.name, type: _text(field.type), description: _description(field.docs) })),
    ...Array.map(row.getAccessors, (getter) => ({ name: getter.name, type: _text(getter.returnType), description: _description(getter.docs) })),
];

const _fields = (named: (name: string) => string, row: SdefClass): OptionalKind<PropertySignatureStructure>[] =>
    Array.map(
        Array.filter(row.property, (property) => Option.isNone(property.attributes.hidden)),
        (property) => ({
            name: camel(property.attributes.name),
            type: Array.match(
                Array.dedupe(
                    Array.map(
                        [...Option.toArray(Option.map(property.attributes.type, (type) => ({ type, list: Option.none() }))), ...Array.map(property.type, Struct.get('attributes'))],
                        ({ type, list }) => `${named(type)}${Option.match(list, { onNone: () => '', onSome: () => '[]' })}`,
                    ),
                ),
                { onEmpty: Function.constant('any'), onNonEmpty: Array.join(' | ') },
            ),
            isReadonly: Option.isSome(property.attributes.access),
            docs: Array.filter(Option.toArray(property.attributes.description), String.isNonEmpty),
        }),
    );

const _table = (row: Declaration): Readonly<Record<string, Slot>> =>
    Record.fromIterableBy(
        [..._members(row), ...Array.map(row.methods, (method) => ({ name: method.name, type: `(${JSON.stringify(method.parameters)}) => ${JSON.stringify(method.returnType)}`, description: '' }))],
        Struct.get('name'),
    );

const _lookup = (rows: Readonly<Record<string, Declaration>>): Lookup => ({ tables: Record.map(rows, _table), parents: Record.map(rows, Struct.get('extends')) });

const _ancestors = (lookup: Lookup, name: string): readonly string[] => {
    const parents = Option.getOrElse(Record.get(lookup.parents, name), () => []);
    return Array.dedupe([...parents, ...Array.flatMap(parents, (parent) => _ancestors(lookup, parent))]);
};

const _find = (lookup: Lookup, name: string, member: string): Option.Option<Slot> =>
    Option.orElse(Option.flatMap(Record.get(lookup.tables, name), Record.get(member)), () =>
        Array.head(
            Array.filterMap(
                Option.getOrElse(Record.get(lookup.parents, name), () => []),
                Filter.fromPredicateOption((parent) => _find(lookup, parent, member)),
            ),
        ),
    );

const _only = <A, B>(self: Readonly<Record<string, A>>, that: Readonly<Record<string, B>>): Record<string, A> => Record.filter(self, (_, key) => !Record.has(that, key));

// --- [GENERATE] ------------------------------------------------------------------------

const generate: Effect.Effect<void, GenerateError | PlatformError.PlatformError, FileSystem.FileSystem | Path.Path | ChildProcessSpawner.ChildProcessSpawner> = Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const bundlePath = yield* Effect.mapError(bundle(HOSTS.indesign), (cause) => GenerateError.cases.hostNotInstalled.make({ cause }));
    const xml = yield* Effect.mapError(reply(ChildProcess.make('sdef', [bundlePath])), (cause) => GenerateError.cases.sdefNotGenerated.make({ cause }));
    const parsed = yield* Effect.mapError(dictionary(xml), (cause) => GenerateError.cases.sdefNotDecodable.make({ cause }));
    const project = new Project({ useInMemoryFileSystem: true, manipulationSettings: MANIPULATION });
    const adobe = project.createSourceFile(
        'adobe.d.ts',
        yield* fs.readFileString(path.join(bundlePath, 'Contents', 'Resources', 'UXP', 'com.adobe.indesign.creative-assistant', 'tsValidation', 'indesign.d.ts')),
    );
    const classes = Array.filter(Array.flatMap(parsed.dictionary.suite, Struct.get('class')), (row) => Option.isNone(row.attributes.hidden));
    const sdefEnumerations = Record.values(Record.fromIterableBy(Array.flatMap(parsed.dictionary.suite, Struct.get('enumeration')), (row) => row.attributes.code));
    const adobeEnumerations = Record.fromIterableWith(
        yield* Effect.mapError(
            Schema.decodeUnknownEffect(Schema.Array(Schema.Struct({ name: Schema.String, members: Schema.Array(Schema.Struct({ name: Schema.String, initializer: Schema.NumberFromString })) })))(
                Array.map(adobe.getEnums(), (node) => node.getStructure()),
            ),
            (cause) => GenerateError.cases.typingsNotDecodable.make({ cause }),
        ),
        (row) => [row.name, Record.fromIterableWith(row.members, (member) => [member.name, member.initializer])],
    );
    const adobeNames = Record.keys(adobeEnumerations);
    const declaration = _declaration(HashSet.fromIterable([...Array.map(adobe.getClasses(), (node) => node.getNameOrThrow()), ...Array.map(adobe.getInterfaces(), (node) => node.getName())]));
    const typed: Readonly<Record<string, Declaration>> = Record.fromIterableBy(
        [
            ...Array.map(adobe.getClasses(), (node) => declaration(node.getNameOrThrow(), Array.fromNullishOr(node.getExtends()), node)),
            ...Array.map(adobe.getInterfaces(), (node) => declaration(node.getName(), node.getExtends(), node)),
        ],
        Struct.get('name'),
    );
    const lookup = _lookup(typed);
    const pairs = Array.flatMap(classes, (klass) => Array.map(klass.property, (property) => ({ klass, property })));
    const candidates = (pair: (typeof pairs)[number]): readonly string[] =>
        Option.match(_find(lookup, pascal(pair.klass.attributes.name), camel(pair.property.attributes.name)), {
            onNone: () => [],
            onSome: (slot) => Array.intersection([...Array.map(String.split(slot.type, '|'), String.trim), ..._mentioned(slot.description)], adobeNames),
        });
    const referenced = HashSet.fromIterable(Array.flatMap(pairs, (pair) => _typeNames(pair.property)));
    const [unidentified, identified] = Array.separate(
        Array.map(sdefEnumerations, (row) => {
            const values = Array.map(row.enumerator, (member) => fourcc(member.attributes.code));
            const names = Array.match(
                sorted(
                    Array.flatMap(
                        Array.filter(pairs, (pair) => Array.contains(_typeNames(pair.property), row.attributes.code)),
                        candidates,
                    ),
                ),
                {
                    onEmpty: () => adobeNames,
                    onNonEmpty: identity,
                },
            );
            const ranked = Array.sort(
                Array.map(Record.toEntries(Record.filter(adobeEnumerations, (_, name) => Array.contains(names, name))), ([name, constants]) => ({
                    name,
                    count: Array.intersection(Record.values(constants), values).length,
                    size: Record.size(constants),
                })),
                Order.Struct({ count: Order.flip(Order.Number), size: Order.Number }),
            );
            const runner = Array.get(ranked, 1);
            const top = Option.filter(Array.head(ranked), (best) => best.count > 0 && (Option.isNone(runner) || runner.value.count < best.count || runner.value.size > best.size));
            const fallback = Option.as(
                Option.liftPredicate(row.attributes.code, (code) => HashSet.has(referenced, code)),
                pascal(row.attributes.name),
            );
            return Result.fromOption(
                Option.map(Option.orElse(Option.map(top, Struct.get('name')), Function.constant(fallback)), (name) => ({ name, row })),
                Function.constant(row.attributes.code),
            );
        }),
    );
    const enumerationNames = Record.fromIterableWith(identified, ({ name, row }) => [row.attributes.code, name]);
    const classNames = Record.fromIterableWith(classes, (row) => [row.attributes.name, pascal(row.attributes.name)]);
    const sdefTable = Record.fromIterableWith(identified, ({ name, row }) => [
        name,
        Record.fromIterableWith(row.enumerator, (member) => [_constant(member.attributes.name), fourcc(member.attributes.code)]),
    ]);
    const phrases = Record.fromIterableWith(identified, ({ name, row }) => [name, Record.fromIterableWith(row.enumerator, (member) => [_constant(member.attributes.name), member.attributes.name])]);
    const shared = Record.toEntries(
        Record.intersection(sdefTable, adobeEnumerations, (sdef, theirs) => ({
            added: Record.keys(_only(sdef, theirs)),
            mismatched: Record.keys(
                Record.filter(
                    Record.intersection(sdef, theirs, (value, known) => value !== known),
                    identity,
                ),
            ),
        })),
    );
    const completed = Record.union(adobeEnumerations, sdefTable, (theirs, sdef) => ({ ...theirs, ..._only(sdef, theirs) }));
    const named = (name: string): string =>
        pipe(
            Record.get(enumerationNames, name),
            Option.orElse(() => Record.get(classNames, name)),
            Option.orElse(() => Record.get(_TYPES, name)),
            Option.getOrElse(() => 'any'),
        );
    const additions = pipe(
        Array.flatMap(classes, (row) => Array.map(_fields(named, row), (field) => ({ owner: pascal(row.attributes.name), field }))),
        Array.filter(({ owner, field }) => Record.has(typed, owner) && Option.isNone(_find(lookup, owner, field.name))),
        Array.dedupeWith((left, right) => left.owner === right.owner && left.field.name === right.field.name),
        Array.groupBy(Struct.get('owner')),
    );
    const missing = Array.filter(classes, (row) => !Record.has(typed, pascal(row.attributes.name)));
    const widened: Readonly<Record<string, Declaration>> = {
        ...Record.map(typed, (row) => ({ ...row, properties: [...row.properties, ...Array.map(Array.flatten(Option.toArray(Record.get(additions, row.name))), Struct.get('field'))] })),
        ...Record.fromIterableBy(
            Array.map(
                missing,
                (row): Declaration => ({
                    kind: StructureKind.Interface,
                    name: pascal(row.attributes.name),
                    isExported: true,
                    docs: Array.filter(Option.toArray(row.attributes.description), String.isNonEmpty),
                    extends: Array.map(Option.toArray(row.attributes.inherits), pascal),
                    properties: _fields(named, row),
                    getAccessors: [],
                    setAccessors: [],
                    methods: [],
                }),
            ),
            Struct.get('name'),
        ),
    };
    const widenedLookup = _lookup(widened);
    const extended = Array.map(
        Array.flatMap(Record.toEntries(widened), ([name, row]) => Array.map(row.extends, (parent) => ({ name, row, parent }))),
        ({ name, row, parent }) => {
            const overridden = Record.filter(_table(row), (own, member) => {
                const inherited = Option.map(_find(widenedLookup, parent, member), Struct.get('type'));
                return Option.isSome(inherited) && !Option.contains(inherited, own.type);
            });
            return { name, parent: Array.match(sorted(Record.keys(overridden)), { onEmpty: Function.constant(parent), onNonEmpty: (names) => `Omit<${parent}, '${Array.join(names, "' | '")}'>` }) };
        },
    );
    const parents = Record.map(Array.groupBy(extended, Struct.get('name')), Array.map(Struct.get('parent')));
    const properties = Record.map(
        Array.groupBy(
            Array.flatMap(Record.values(widened), (row) => [
                ..._members(row),
                ...Array.map(row.setAccessors, (setter) => ({
                    name: setter.name,
                    type: Option.match(Array.head(Array.flatten(Array.fromNullishOr(setter.parameters))), { onNone: Function.constant('any'), onSome: flow(Struct.get('type'), _text) }),
                    description: _description(setter.docs),
                })),
            ]),
            Struct.get('name'),
        ),
        (group) => {
            const types = sorted(Array.flatMap(group, (slot) => Array.map(String.split(slot.type, '|'), String.trim)));
            return {
                types,
                list: Array.some(types, String.endsWith('[]')),
                enumerations: sorted(Array.intersection([...types, ..._mentioned(Array.join(Array.map(group, Struct.get('description')), ' '))], Record.keys(completed))),
            };
        },
    );
    const collections = Record.fromIterableWith(
        Array.filterMap(
            classes,
            Filter.fromPredicateOption((row) => row.attributes.plural),
        ),
        (plural) => [camel(plural), pascal(plural)],
    );
    const ancestors = Record.map(widened, (_, name) => _ancestors(widenedLookup, name));
    const writable = Record.map(widened, (row) =>
        Record.fromEntries([
            ...Array.map(row.properties, (field) => [field.name, field.isReadonly !== true] as const),
            ...Array.map(row.getAccessors, (getter) => [getter.name, false] as const),
            ...Array.map(row.setAccessors, (setter) => [setter.name, true] as const),
        ]),
    );
    const members = Record.map(ancestors, (chain, name) =>
        Record.fromEntries(Array.flatMap(Array.getSomes(Array.map([...Array.reverse(chain), name], (ancestor) => Record.get(writable, ancestor))), Record.toEntries)),
    );
    const preferences = pipe(
        Record.get(widenedLookup.tables, 'Application'),
        Option.getOrElse((): Readonly<Record<string, Slot>> => ({})),
        Record.filter((slot) => slot.type === 'Preference' || Option.exists(Record.get(ancestors, slot.type), Array.contains('Preference'))),
        Record.map(Struct.get('type')),
    );
    const out = project.createSourceFile('indesign.ts', {
        statements: [
            'declare const enumerationName: unique symbol;',
            'export interface Enumerator<Name extends string> { readonly [enumerationName]?: Name; equals(other: Enumerator<Name>): boolean; toString(): string; }',
            ...Array.map(Record.keys(completed), (name): InterfaceDeclarationStructure => ({ kind: StructureKind.Interface, name, isExported: true, extends: [`Enumerator<'${name}'>`] })),
            ...Array.map(
                Record.toEntries(completed),
                ([name, constants]) =>
                    `export declare const ${name}: { ${Array.join(
                        Array.map(Record.keys(constants), (member) => `readonly ${member}: ${name};`),
                        ' ',
                    )} };`,
            ),
            { kind: StructureKind.TypeAlias, name: 'Real', type: 'number' },
            { kind: StructureKind.TypeAlias, name: 'Strings', type: 'readonly string[]' },
            ...Array.map(adobe.getTypeAliases(), (node) => node.getStructure()),
            ...Array.map(Record.toEntries(widened), ([name, row]) => ({ ...row, extends: Option.getOrElse(Record.get(parents, name), () => row.extends) })),
            { kind: StructureKind.VariableStatement, declarationKind: VariableDeclarationKind.Const, hasDeclareKeyword: true, isExported: true, declarations: [{ name: 'app', type: 'Application' }] },
            `export const enumerations: Readonly<Record<string, Readonly<Record<string, number>>>> = ${JSON.stringify(completed, null, 4)};`,
            `export const phrases: Readonly<Record<string, Readonly<Record<string, string>>>> = ${JSON.stringify(phrases, null, 4)};`,
            `export const properties: Readonly<Record<string, { readonly types: readonly string[]; readonly list: boolean; readonly enumerations: readonly string[] }>> = ${JSON.stringify(properties, null, 4)};`,
            `export const collections: Readonly<Record<string, string>> = ${JSON.stringify(collections, null, 4)};`,
            `export const members = ${JSON.stringify(members, null, 4)};`,
            `export const ancestors: Readonly<Record<string, readonly string[]>> = ${JSON.stringify(ancestors, null, 4)};`,
            `export const preferences: Readonly<Record<${Array.join(
                Array.map(Record.keys(preferences), (name) => `'${name}'`),
                ' | ',
            )}, keyof typeof members>> = ${JSON.stringify(preferences, null, 4)};`,
        ],
    });
    yield* fs.writeFileString(path.join(import.meta.dirname, 'indesign.ts'), out.getFullText());
    yield* Console.log(
        JSON.stringify(
            {
                dictionary: Option.map(parsed.dictionary.attributes, Struct.get('title')),
                classes: {
                    sdef: classes.length,
                    adobe: Record.size(typed),
                    added: Array.map(missing, (row) => pascal(row.attributes.name)),
                    addedProperties: Number.sumAll(Array.map(Record.values(additions), Array.length)),
                },
                enumerations: {
                    sdef: Record.size(sdefTable),
                    adobe: Record.size(adobeEnumerations),
                    added: Record.keys(_only(sdefTable, adobeEnumerations)),
                    addedConstants: Array.flatMap(shared, ([name, row]) => Array.map(row.added, (member) => `${name}.${member}`)),
                    valueMismatches: Array.flatMap(shared, ([name, row]) => Array.map(row.mismatched, (member) => `${name}.${member}`)),
                    adobeOnly: Array.difference(adobeNames, Record.keys(sdefTable)),
                    unidentified,
                },
                properties: Record.size(properties),
                collections: Record.size(collections),
            },
            null,
            4,
        ),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { GenerateError, generate };
