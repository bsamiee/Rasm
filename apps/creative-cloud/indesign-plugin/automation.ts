// --- [IMPORTS] -------------------------------------------------------------------------

import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { BridgeError } from '@rasm/creative-cloud-server/errors';
import { Link } from '@rasm/creative-cloud-server/frames';
import { doScript, read, reply } from '@rasm/creative-cloud-server/osascript';
import { HOSTS } from '@rasm/creative-cloud-server/values';
import {
    Array,
    Cause,
    Clock,
    Config,
    Console,
    Effect,
    FileSystem,
    Filter,
    flow,
    HashSet,
    identity,
    Layer,
    Option,
    Order,
    Path,
    type PlatformError,
    Predicate,
    pipe,
    Queue,
    Record,
    Result,
    Schedule,
    Schema,
    Stream,
    String,
    Struct,
} from 'effect';
import { McpProtocol, McpSchema } from 'effect/unstable/ai';
import { Command } from 'effect/unstable/cli';
import { ChildProcess, ChildProcessSpawner } from 'effect/unstable/process';
import { RpcClient, RpcSerialization } from 'effect/unstable/rpc';
import { Socket } from 'effect/unstable/socket';
import { XMLParser } from 'fast-xml-parser';
import {
    type ClassDeclaration,
    type ExpressionWithTypeArguments,
    type GetAccessorDeclarationStructure,
    IndentationText,
    type InterfaceDeclaration,
    type InterfaceDeclarationStructure,
    type MethodSignatureStructure,
    NewLineKind,
    type OptionalKind,
    Project,
    type PropertySignatureStructure,
    QuoteKind,
    type SetAccessorDeclarationStructure,
    StructureKind,
    VariableDeclarationKind,
    type WriterFunction,
} from 'ts-morph';
import { reflected } from './enums.ts';
import { host, manifest, panel } from './uxp.config.ts';

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

type Enumerations = Readonly<Record<string, Readonly<Record<string, number>>>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const _HOST = HOSTS.indesign;
const _DECLARATIONS = ['Contents', 'Resources', 'UXP', 'com.adobe.indesign.creative-assistant', 'tsValidation', 'indesign.d.ts'] as const;
const _ACRONYM = /^[A-Z][A-Z0-9]*$/u;
const _ENUMERATOR_DOC = /(?<name>[A-Z][A-Za-z0-9]*) enumerator/gu;
const _PRIMITIVES: Readonly<Record<string, string>> = {
    any: 'any',
    boolean: 'boolean',
    date: 'Date',
    file: 'File | string',
    integer: 'number',
    number: 'number',
    real: 'number',
    record: 'object',
    specifier: 'any',
    text: 'string',
    type: 'string',
};
const _BYTE = 256;
const _QUIT_MS = 300_000;
const _RUNNING_MS = 5000;
const _POLL = Schedule.spaced('250 millis').pipe(Schedule.upTo({ duration: '120 seconds' }));
const _PROTOCOL = McpProtocol.v2025_11_25.protocolVersion;

// --- [ERRORS] --------------------------------------------------------------------------

const _strings = Schema.Array(Schema.String);

const AutomationError = Schema.TaggedUnion({
    bundleNotFound: { bundleId: Schema.String },
    notReady: { hosts: Schema.String },
    toolFailed: { tool: Schema.String, text: Schema.String },
    unreconciled: { undeclared: Schema.Struct({ enumerations: _strings, constants: _strings }) },
});

// --- [DICTIONARY] ----------------------------------------------------------------------

const _children = <S extends Schema.Top>(schema: S): Schema.withDecodingDefaultKey<Schema.$Array<S>> => Schema.Array(schema).pipe(Schema.withDecodingDefaultKey(Effect.succeed([])));
const _optionalString = Schema.OptionFromOptionalKey(Schema.String);
const _yes = Schema.OptionFromOptionalKey(Schema.Literal('yes'));
const _named = { name: Schema.String, code: Schema.String, description: Schema.String };
const _Property = Schema.Struct({
    attributes: Schema.Struct({ ..._named, type: _optionalString, access: Schema.OptionFromOptionalKey(Schema.Literal('r')), hidden: _yes }),
    type: _children(Schema.Struct({ attributes: Schema.Struct({ type: Schema.String, list: _yes }) })),
});
const _Class = Schema.Struct({ attributes: Schema.Struct({ ..._named, inherits: _optionalString, plural: _optionalString, hidden: _yes }), property: _children(_Property) });
const _Enumeration = Schema.Struct({ attributes: Schema.Struct({ name: Schema.String, code: Schema.String }), enumerator: Schema.Array(Schema.Struct({ attributes: Schema.Struct(_named) })) });
const _Dictionary = Schema.Struct({
    dictionary: Schema.Struct({ attributes: Schema.Struct({ title: Schema.String }), suite: Schema.Array(Schema.Struct({ class: _children(_Class), enumeration: _children(_Enumeration) })) }),
});
const _Enum = Schema.Struct({ name: Schema.String, members: Schema.Array(Schema.Struct({ name: Schema.String, initializer: Schema.NumberFromString })) });

type SdefProperty = (typeof _Property)['Type'];
type SdefClass = (typeof _Class)['Type'];
type SdefEnumeration = (typeof _Enumeration)['Type'];

const _parser = new XMLParser({
    ignoreAttributes: false,
    attributeNamePrefix: '',
    attributesGroupName: 'attributes',
    ignoreDeclaration: true,
    trimValues: false,
    parseTagValue: false,
    parseAttributeValue: false,
    isArray: (tag): boolean => Array.contains(['suite', 'class', 'property', 'type', 'enumeration', 'enumerator'], tag),
});

const _bundle = Effect.orDie(reply(ChildProcess.make('mdfind', [`kMDItemCFBundleIdentifier == '${_HOST.bundleId}'`]))).pipe(
    Effect.flatMap(Schema.decodeEffect(Schema.NonEmptyString)),
    Effect.mapError(() => AutomationError.cases.bundleNotFound.make({ bundleId: _HOST.bundleId })),
);

// --- [NAMES] ---------------------------------------------------------------------------

const _words = (name: string): Array.NonEmptyReadonlyArray<string> => String.split(Array.headNonEmpty(String.split(name, '.')), ' ');

const _capitalized = (word: string): string => (_ACRONYM.test(word) ? word : String.capitalize(word));

const _pascal = (name: string): string => Array.join(Array.map(_words(name), _capitalized), '');

const _camel = (name: string): string => {
    const [head, ...rest] = _words(name);
    return `${_ACRONYM.test(head) ? String.toLowerCase(head) : String.uncapitalize(head)}${Array.join(Array.map(rest, _capitalized), '')}`;
};

const _constant = (name: string): string => (name === 'default' ? 'DEFAULT_VALUE' : Array.join(Array.map(_words(name), String.toUpperCase), '_'));

const _fourcc = (code: string): number => Array.reduce(code.split(''), 0, (total, character) => total * _BYTE + character.charCodeAt(0));

const _sorted = (names: Iterable<string>): readonly string[] => Array.sort(Array.dedupe(Array.fromIterable(names)), Order.String);

const _mentioned = (description: string): readonly string[] =>
    Array.filterMap(
        Array.fromIterable(description.matchAll(_ENUMERATOR_DOC)),
        Filter.fromPredicateOption((found) => Option.fromNullishOr(found.groups?.['name'])),
    );

const _typeNames = (property: SdefProperty): readonly string[] => [...Option.toArray(property.attributes.type), ...Array.map(property.type, (row) => row.attributes.type)];

// --- [DECLARATIONS] --------------------------------------------------------------------

const _text = (type: string | WriterFunction | undefined): string => (Predicate.isString(type) ? type : 'any');

const _description = (docs: InterfaceDeclarationStructure['docs']): string =>
    Array.join(
        Array.map(Array.flatten(Array.fromNullishOr(docs)), (doc) => String.trim(Predicate.isString(doc) ? doc : _text(doc.description))),
        ' ',
    );

const _declaration =
    (names: HashSet.HashSet<string>) =>
    (name: string, parents: readonly ExpressionWithTypeArguments[], node: ClassDeclaration | InterfaceDeclaration): Declaration => {
        const union = (type: string | WriterFunction | undefined): string => {
            const parts = String.split(_text(type), ' & ');
            return parts.length > 1 && Array.every(parts, (part) => HashSet.has(names, part)) ? Array.join(parts, ' | ') : _text(type);
        };
        return {
            kind: StructureKind.Interface,
            name,
            isExported: true,
            docs: Array.map(node.getJsDocs(), (doc) => doc.getStructure()),
            extends: Array.map(parents, (parent) => parent.getText()),
            properties: Array.map(node.getProperties(), (property) => {
                const structure = property.getStructure();
                return { ...Struct.pick(structure, ['name', 'isReadonly', 'hasQuestionToken', 'docs']), type: union(structure.type) };
            }),
            getAccessors: Array.map(node.getGetAccessors(), (accessor) => {
                const structure = accessor.getStructure();
                return { ...structure, returnType: union(structure.returnType) };
            }),
            setAccessors: Array.map(node.getSetAccessors(), (accessor) => {
                const structure = accessor.getStructure();
                return { ...structure, parameters: Array.map(Array.flatten(Array.fromNullishOr(structure.parameters)), (parameter) => ({ ...parameter, type: union(parameter.type) })) };
            }),
            methods: Array.map(node.getMethods(), (method) => {
                const structure = method.getStructure();
                return {
                    ...Struct.pick(structure, ['docs', 'hasQuestionToken']),
                    name: method.getName(),
                    parameters: Array.map(Array.flatten(Array.fromNullishOr(structure.parameters)), (parameter) => ({ ...parameter, type: union(parameter.type) })),
                    returnType: union(structure.returnType),
                };
            }),
        };
    };

const _members = (row: Declaration): readonly Slot[] => [
    ...Array.map(row.properties, (field) => ({ name: field.name, type: _text(field.type), description: _description(field.docs) })),
    ...Array.map(row.getAccessors, (getter) => ({ name: getter.name, type: _text(getter.returnType), description: _description(getter.docs) })),
];

const _slots = (row: Declaration): readonly Slot[] => [
    ..._members(row),
    ...Array.map(row.setAccessors, (setter) => ({
        name: setter.name,
        type: _text(Option.getOrElse(Option.map(Array.head(Array.flatten(Array.fromNullishOr(setter.parameters))), Struct.get('type')), () => 'any')),
        description: _description(setter.docs),
    })),
];

const _table = (row: Declaration): Readonly<Record<string, Slot>> =>
    Record.fromIterableBy(
        [..._members(row), ...Array.map(row.methods, (method) => ({ name: method.name, type: `(${JSON.stringify(method.parameters)}) => ${JSON.stringify(method.returnType)}`, description: '' }))],
        Struct.get('name'),
    );

const _lookup = (rows: Readonly<Record<string, Declaration>>): Lookup => ({ tables: Record.map(rows, _table), parents: Record.map(rows, Struct.get('extends')) });

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

const _parent = (lookup: Lookup, row: Declaration, parent: string): string =>
    Array.match(_sorted(Record.keys(Record.filter(_table(row), (own, name) => Option.exists(_find(lookup, parent, name), (inherited) => inherited.type !== own.type)))), {
        onEmpty: () => parent,
        onNonEmpty: (names) =>
            `Omit<${parent}, ${Array.join(
                Array.map(names, (name) => `'${name}'`),
                ' | ',
            )}>`,
    });

const _candidates =
    (lookup: Lookup, adobeNames: readonly string[]) =>
    (pair: { readonly klass: SdefClass; readonly property: SdefProperty }): readonly string[] =>
        Option.match(_find(lookup, _pascal(pair.klass.attributes.name), _camel(pair.property.attributes.name)), {
            onNone: () => [],
            onSome: (slot) => Array.intersection([...Array.map(String.split(slot.type, '|'), String.trim), ..._mentioned(slot.description)], adobeNames),
        });

const _nameOf =
    (adobe: Enumerations, referenced: HashSet.HashSet<string>, candidates: (code: string) => readonly string[]) =>
    (row: SdefEnumeration): Result.Result<{ readonly name: string; readonly row: SdefEnumeration }, string> =>
        Result.fromOption(
            Option.map(
                Option.orElse(
                    _identity(
                        adobe,
                        Array.match(candidates(row.attributes.code), { onEmpty: () => Record.keys(adobe), onNonEmpty: identity }),
                        Array.map(row.enumerator, (member) => _fourcc(member.attributes.code)),
                    ),
                    () => (HashSet.has(referenced, row.attributes.code) ? Option.some(_pascal(row.attributes.name)) : Option.none()),
                ),
                (name) => ({ name, row }),
            ),
            () => row.attributes.code,
        );

const _sdefType =
    (named: (name: string) => string) =>
    (property: SdefProperty): string =>
        Array.match(property.type, {
            onEmpty: () => named(Option.getOrElse(property.attributes.type, () => 'any')),
            onNonEmpty: (rows) => Array.join(Array.dedupe(Array.map(rows, (type) => (Option.isSome(type.attributes.list) ? `${named(type.attributes.type)}[]` : named(type.attributes.type)))), ' | '),
        });

const _fields =
    (sdefType: (property: SdefProperty) => string) =>
    (row: SdefClass): OptionalKind<PropertySignatureStructure>[] =>
        Array.map(
            Array.filter(row.property, (property) => Option.isNone(property.attributes.hidden)),
            (property) => ({
                name: _camel(property.attributes.name),
                type: sdefType(property),
                isReadonly: Option.isSome(property.attributes.access),
                docs: Array.filter([property.attributes.description], String.isNonEmpty),
            }),
        );

const _identity = (adobe: Enumerations, candidates: readonly string[], values: readonly number[]): Option.Option<string> => {
    const ranked = Array.sort(
        Array.filterMap(
            candidates,
            Filter.fromPredicateOption((name) =>
                Option.map(Record.get(adobe, name), (constants) => ({ name, count: Array.intersection(Record.values(constants), values).length, size: Record.size(constants) })),
            ),
        ),
        Order.Struct({ count: Order.flip(Order.Number), size: Order.Number }),
    );
    return Option.map(
        Option.filter(Array.head(ranked), (best) => best.count > 0 && Option.match(Array.get(ranked, 1), { onNone: () => true, onSome: (next) => next.count < best.count || next.size > best.size })),
        Struct.get('name'),
    );
};

// --- [GENERATE] ------------------------------------------------------------------------

const _generate = Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const bundle = yield* _bundle;
    const dictionary = yield* Effect.flatMap(Effect.orDie(reply(ChildProcess.make('sdef', [bundle]))), (xml) => Schema.decodeUnknownEffect(_Dictionary)(_parser.parse(xml)));
    const project = new Project({ useInMemoryFileSystem: true, manipulationSettings: { indentationText: IndentationText.FourSpaces, newLineKind: NewLineKind.LineFeed, quoteKind: QuoteKind.Single } });
    const adobe = project.createSourceFile('adobe.d.ts', yield* Effect.orDie(fs.readFileString(path.join(bundle, ..._DECLARATIONS))));
    const classes = Array.filter(Array.flatMap(dictionary.dictionary.suite, Struct.get('class')), (row) => Option.isNone(row.attributes.hidden));
    const enumerations = Record.values(Record.fromIterableBy(Array.flatMap(dictionary.dictionary.suite, Struct.get('enumeration')), (row) => row.attributes.code));
    const adobeEnumerations: Enumerations = Record.fromIterableWith(yield* Schema.decodeUnknownEffect(Schema.Array(_Enum))(Array.map(adobe.getEnums(), (node) => node.getStructure())), (row) => [
        row.name,
        Record.fromIterableWith(row.members, (member) => [member.name, member.initializer]),
    ]);
    const adobeNames = Record.keys(adobeEnumerations);
    const declaration = _declaration(HashSet.fromIterable([...Array.map(adobe.getClasses(), (node) => node.getNameOrThrow()), ...Array.map(adobe.getInterfaces(), (node) => node.getName())]));
    const declared: Readonly<Record<string, Declaration>> = Record.fromIterableBy(
        [
            ...Array.map(adobe.getClasses(), (node) => declaration(node.getNameOrThrow(), Array.fromNullishOr(node.getExtends()), node)),
            ...Array.map(adobe.getInterfaces(), (node) => declaration(node.getName(), node.getExtends(), node)),
        ],
        Struct.get('name'),
    );
    const lookup = _lookup(declared);
    const pairs = Array.flatMap(classes, (klass) => Array.map(klass.property, (property) => ({ klass, property })));
    const candidates = _candidates(lookup, adobeNames);
    const [unidentified, identified] = Array.separate(
        Array.map(
            enumerations,
            _nameOf(adobeEnumerations, HashSet.fromIterable(Array.flatMap(pairs, (pair) => _typeNames(pair.property))), (code) =>
                _sorted(
                    Array.flatMap(
                        Array.filter(pairs, (pair) => Array.contains(_typeNames(pair.property), code)),
                        candidates,
                    ),
                ),
            ),
        ),
    );
    const enumerationNames = Record.fromIterableWith(identified, ({ name, row }) => [row.attributes.code, name]);
    const classNames = Record.fromIterableWith(classes, (row) => [row.attributes.name, _pascal(row.attributes.name)]);
    const sdefTable: Enumerations = Record.fromIterableWith(identified, ({ name, row }) => [
        name,
        Record.fromIterableWith(row.enumerator, (member) => [_constant(member.attributes.name), _fourcc(member.attributes.code)]),
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
    const completed: Enumerations = Record.union(adobeEnumerations, sdefTable, (theirs, sdef) => ({ ...theirs, ..._only(sdef, theirs) }));
    const fields = _fields(
        _sdefType((name) =>
            pipe(
                Record.get(enumerationNames, name),
                Option.orElse(() => Record.get(classNames, name)),
                Option.orElse(() => Record.get(_PRIMITIVES, name)),
                Option.getOrElse(() => 'any'),
            ),
        ),
    );
    const additions = pipe(
        Array.flatMap(classes, (row) => Array.map(fields(row), (field) => ({ owner: _pascal(row.attributes.name), field }))),
        Array.filter(({ owner, field }) => Record.has(declared, owner) && Option.isNone(_find(lookup, owner, field.name))),
        Array.dedupeWith((left, right) => left.owner === right.owner && left.field.name === right.field.name),
        Array.groupBy(Struct.get('owner')),
    );
    const missing = Array.filter(classes, (row) => !Record.has(declared, _pascal(row.attributes.name)));
    const widened: Readonly<Record<string, Declaration>> = {
        ...Record.map(declared, (row) => ({ ...row, properties: [...row.properties, ...Array.map(Array.flatten(Option.toArray(Record.get(additions, row.name))), Struct.get('field'))] })),
        ...Record.fromIterableBy(
            Array.map(
                missing,
                (row): Declaration => ({
                    kind: StructureKind.Interface,
                    name: _pascal(row.attributes.name),
                    isExported: true,
                    docs: Array.filter([row.attributes.description], String.isNonEmpty),
                    extends: Array.map(Option.toArray(row.attributes.inherits), _pascal),
                    properties: fields(row),
                    getAccessors: [],
                    setAccessors: [],
                    methods: [],
                }),
            ),
            Struct.get('name'),
        ),
    };
    const widenedLookup = _lookup(widened);
    const properties = Record.map(Array.groupBy(Array.flatMap(Record.values(widened), _slots), Struct.get('name')), (group) => {
        const types = _sorted(Array.flatMap(group, (slot) => Array.map(String.split(slot.type, '|'), String.trim)));
        return {
            types,
            list: Array.some(types, String.endsWith('[]')),
            enumerations: _sorted(Array.intersection([...types, ..._mentioned(Array.join(Array.map(group, Struct.get('description')), ' '))], Record.keys(completed))),
        };
    });
    const collections = Record.fromIterableWith(
        Array.filterMap(
            classes,
            Filter.fromPredicateOption((row) => row.attributes.plural),
        ),
        (plural) => [_camel(plural), _pascal(plural)],
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
                        Array.map(Record.keys(constants), (constant) => `readonly ${constant}: ${name};`),
                        ' ',
                    )} };`,
            ),
            { kind: StructureKind.TypeAlias, name: 'Real', type: 'number' },
            { kind: StructureKind.TypeAlias, name: 'Strings', type: 'readonly string[]' },
            ...Array.map(adobe.getTypeAliases(), (node) => node.getStructure()),
            ...Array.map(Record.values(widened), (row) => ({ ...row, extends: Array.map(row.extends, (parent) => _parent(widenedLookup, row, parent)) })),
            { kind: StructureKind.VariableStatement, declarationKind: VariableDeclarationKind.Const, hasDeclareKeyword: true, isExported: true, declarations: [{ name: 'app', type: 'Application' }] },
            `export const enumerations: Readonly<Record<string, Readonly<Record<string, number>>>> = ${JSON.stringify(completed, null, 4)};`,
            `export const phrases: Readonly<Record<string, Readonly<Record<string, string>>>> = ${JSON.stringify(phrases, null, 4)};`,
            `export const properties: Readonly<Record<string, { readonly types: readonly string[]; readonly list: boolean; readonly enumerations: readonly string[] }>> = ${JSON.stringify(properties, null, 4)};`,
            `export const collections: Readonly<Record<string, string>> = ${JSON.stringify(collections, null, 4)};`,
        ],
    });
    yield* fs.writeFileString(path.join(import.meta.dirname, 'indesign.ts'), out.getFullText());
    yield* Console.log(
        JSON.stringify(
            {
                dictionary: dictionary.dictionary.attributes.title,
                classes: {
                    sdef: classes.length,
                    adobe: Record.size(declared),
                    added: Array.map(missing, (row) => _pascal(row.attributes.name)),
                    addedProperties: Array.reduce(Record.values(additions), 0, (total, group) => total + group.length),
                },
                enumerations: {
                    sdef: Record.size(sdefTable),
                    adobe: Record.size(adobeEnumerations),
                    added: Record.keys(_only(sdefTable, adobeEnumerations)),
                    addedConstants: Array.flatMap(shared, ([name, row]) => Array.map(row.added, (constant) => `${name}.${constant}`)),
                    valueMismatches: Array.flatMap(shared, ([name, row]) => Array.map(row.mismatched, (constant) => `${name}.${constant}`)),
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

// --- [SERVER] --------------------------------------------------------------------------

const _bytes = (chunk: Uint8Array | string): Uint8Array => (Predicate.isString(chunk) ? new TextEncoder().encode(chunk) : chunk);

const _protocol: Layer.Layer<RpcClient.Protocol, PlatformError.PlatformError, ChildProcessSpawner.ChildProcessSpawner | Path.Path> = Layer.unwrap(
    Effect.gen(function* () {
        const path = yield* Path.Path;
        const spawner = yield* ChildProcessSpawner.ChildProcessSpawner;
        const stdin = yield* Queue.make<Uint8Array, Cause.Done>();
        const handle = yield* spawner.spawn(ChildProcess.make('node', ['main.ts'], { cwd: path.resolve(import.meta.dirname, '..', 'server'), stdin: Stream.fromQueue(stdin), stderr: 'inherit' }));
        const socket = Socket.make({
            reader: Effect.map(Stream.toPull(handle.stdout), (pull) => ({
                pull: Effect.mapError(
                    pull,
                    (error) => new Socket.SocketError({ reason: Cause.isDone(error) ? new Socket.SocketCloseError({ code: 1000 }) : new Socket.SocketReadError({ cause: error }) }),
                ),
                upgrade: Socket.SocketUpgradeError.unsupported,
            })),
            writer: Effect.succeed({
                write: (chunk: Uint8Array | string | Socket.CloseEvent) => Effect.asVoid(Socket.isCloseEvent(chunk) ? Queue.end(stdin) : Queue.offer(stdin, _bytes(chunk))),
                writeAll: (chunks: Array.NonEmptyReadonlyArray<Uint8Array | string>) => Effect.asVoid(Queue.offerAll(stdin, Array.map(chunks, _bytes))),
            }),
        });
        return RpcClient.layerProtocolSocket().pipe(Layer.provide(Layer.succeed(Socket.Socket, socket)), Layer.provide(RpcSerialization.layerNdJsonRpc()));
    }),
);

const _Health = Schema.Struct({
    hosts: Schema.Array(Schema.Struct({ host: Schema.Struct({ id: Schema.String }), link: Schema.OptionFromNullOr(Link), probe: Schema.toCodecJson(Schema.Result(Schema.Json, BridgeError)) })),
});
const _Answer = Schema.Struct({
    result: Schema.Union([Schema.Struct({ kind: Schema.Literal('value'), value: Schema.Json }), Schema.Struct({ kind: Schema.Literal('error'), error: BridgeError })]).pipe(
        Schema.toTaggedUnion('kind'),
    ),
});

const _server = Effect.gen(function* () {
    const protocol = yield* RpcClient.Protocol;
    const client = yield* RpcClient.make(McpSchema.ClientRpcs);
    yield* client.initialize({ protocolVersion: _PROTOCOL, capabilities: {}, clientInfo: { name: manifest.id, version: manifest.version } });
    yield* protocol.send(0, { _tag: 'Request', id: '', tag: McpSchema.InitializedNotification._tag, payload: null, headers: [], isNotification: true });
    const call = Effect.fnUntraced(function* <S extends Schema.Top>(tool: string, args: Readonly<Record<string, Schema.Json>>, schema: S) {
        const answer = yield* client['tools/call']({ name: tool, arguments: args });
        return yield* Effect.mapError(Schema.decodeUnknownEffect(schema)(answer.structuredContent), () => AutomationError.cases.toolFailed.make({ tool, text: JSON.stringify(answer.content) }));
    });
    return {
        health: call('health', { host: _HOST.id }, _Health),
        execute: (code: string) =>
            Effect.retry(
                Effect.flatMap(call('indesign_execute', { code }, _Answer), ({ result }) => (result.kind === 'value' ? Effect.succeed(result.value) : Effect.fail(result.error))),
                { while: Predicate.isTagged('hostSaturated'), schedule: _POLL },
            ),
    };
});

// --- [DEPLOY] --------------------------------------------------------------------------

const _Registry = Schema.fromJsonString(Schema.Struct({ plugins: Schema.Array(Schema.Record(Schema.String, Schema.Json)) }));

type HealthRow = (typeof _Health)['Type']['hosts'][number];

const _until = <E, R>(health: Effect.Effect<(typeof _Health)['Type'], E, R>, ready: Predicate.Predicate<HealthRow>): Effect.Effect<HealthRow, E | (typeof AutomationError)['Type'], R> =>
    Effect.flatMap(health, (answer) => Effect.fromOption(Array.findFirst(answer.hosts, ready), () => AutomationError.cases.notReady.make({ hosts: JSON.stringify(answer.hosts) }))).pipe(
        Effect.retry({ while: Predicate.isTagged('notReady'), schedule: _POLL }),
    );

const _deploy = Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const uxp = path.join(yield* Config.String('HOME'), 'Library', 'Application Support', 'Adobe', 'UXP');
    const external = path.join(uxp, 'Plugins', 'External');
    const folder = `${manifest.id}_${manifest.version}`;
    const root = path.resolve(import.meta.dirname, '..', '..', '..');
    yield* Effect.forEach(Array.filter(yield* fs.readDirectory(external), String.startsWith(`${manifest.id}_`)), (entry) => fs.remove(path.join(external, entry), { recursive: true }));
    yield* fs.copy(path.join(root, '.artifacts', path.relative(root, import.meta.dirname)), path.join(external, folder));
    const registry = path.join(uxp, 'PluginsInfo', 'v1', 'ID.json');
    const rows = yield* Schema.decodeEffect(_Registry)(yield* fs.readFileString(registry));
    const row = {
        hostMinVersion: host.minVersion,
        name: manifest.name,
        path: `$localPlugins/External/${folder}`,
        pluginId: manifest.id,
        status: 'enabled',
        type: 'uxp',
        versionString: manifest.version,
    };
    yield* fs.writeFileString(registry, yield* Schema.encodeEffect(_Registry)({ plugins: [...Array.filter(rows.plugins, (kept) => kept['pluginId'] !== manifest.id), row] }));
    const server = yield* _server;
    yield* Effect.asVoid(read(_HOST.id, _HOST.bundleId, _QUIT_MS, 'quit saving ask', Option.none())).pipe(Effect.catchTag('hostNotRunning', () => Effect.void));
    yield* Effect.asVoid(Effect.repeat(read(_HOST.id, _HOST.bundleId, _RUNNING_MS, '', Option.none()), _POLL)).pipe(Effect.catchTag('hostNotRunning', () => Effect.void));
    yield* Effect.orDie(reply(ChildProcess.make('open', ['-b', _HOST.bundleId])));
    const launchedAt = yield* Clock.currentTimeMillis;
    yield* read(
        _HOST.id,
        _HOST.bundleId,
        _RUNNING_MS,
        `${doScript(`var action = app.menuActions.itemByName(${JSON.stringify(panel.label.default)}); if (!action.checked) action.invoke(); action.checked`)} language javascript`,
        Option.none(),
    ).pipe(Effect.retry({ while: Predicate.or(Predicate.isTagged('hostNotRunning'), Predicate.isTagged('hostUnresponsive')), schedule: _POLL }));
    const shownAt = yield* Clock.currentTimeMillis;
    const attached = yield* _until(server.health, (entry) => Option.exists(entry.link, Predicate.isTagged('attached')));
    const attachedAt = yield* Clock.currentTimeMillis;
    const probed = yield* _until(server.health, (entry) => Result.isSuccess(entry.probe));
    const probedAt = yield* Clock.currentTimeMillis;
    const scope = yield* server.execute('return { TextEncoder: typeof TextEncoder, TextDecoder: typeof TextDecoder, queueMicrotask: typeof queueMicrotask };');
    yield* Console.log(
        JSON.stringify(
            {
                folder,
                registry: row,
                panelShownAfterMs: shownAt - launchedAt,
                attachedAfterPanelMs: attachedAt - shownAt,
                probedAfterAttachMs: probedAt - attachedAt,
                link: attached.link,
                probe: probed.probe,
                scope,
            },
            null,
            4,
        ),
    );
});

// --- [RECONCILE] -----------------------------------------------------------------------

const _Runtime = Schema.Struct({ enumerations: Schema.Array(Schema.Struct({ name: Schema.String, constants: _strings })), exports: _strings });

const _missing: (self: readonly string[], that: readonly string[]) => readonly string[] = Array.difference;

const _qualified = ([name, constants]: readonly [string, readonly string[]]): readonly string[] => Array.map(constants, (constant) => `${name}.${constant}`);

const _absent = (
    self: Readonly<Record<string, readonly string[]>>,
    that: Readonly<Record<string, readonly string[]>>,
): { readonly enumerations: readonly string[]; readonly constants: readonly string[] } => ({
    enumerations: Array.difference(Record.keys(self), Record.keys(that)),
    constants: Array.flatMap(Record.toEntries(Record.intersection(self, that, _missing)), _qualified),
});

const _reconcile = Effect.gen(function* () {
    const server = yield* _server;
    yield* _until(server.health, (entry) => Result.isSuccess(entry.probe));
    const generated = yield* Effect.promise(() => import('./indesign.ts'));
    const runtime = yield* Schema.decodeUnknownEffect(_Runtime)(
        yield* server.execute(`return { enumerations: (${reflected.toString()})(require('indesign')), exports: Object.getOwnPropertyNames(require('indesign')) };`),
    );
    const registered = Record.map(Record.fromIterableBy(runtime.enumerations, Struct.get('name')), (row) => _sorted(row.constants));
    const declared = Record.map(generated.enumerations, flow(Record.keys, _sorted));
    const report = {
        undeclared: _absent(registered, declared),
        unregistered: _absent(declared, registered),
        generated: Record.size(declared),
        runtime: Record.size(registered),
        exports: runtime.exports.length,
    };
    yield* Console.log(JSON.stringify(report, null, 4));
    return yield* report.undeclared.enumerations.length + report.undeclared.constants.length === 0
        ? Console.log('Generated declarations cover every runtime enumeration and constant')
        : Effect.fail(AutomationError.cases.unreconciled.make({ undeclared: report.undeclared }));
});

// --- [ENTRY] ---------------------------------------------------------------------------

Command.run(
    Command.make('automation').pipe(
        Command.withSubcommands([
            Command.make('generate', {}, () => _generate),
            Command.make('deploy', {}, () => Effect.provide(Effect.scoped(_deploy), _protocol)),
            Command.make('reconcile', {}, () => Effect.provide(Effect.scoped(_reconcile), _protocol)),
        ]),
    ),
    { version: manifest.version },
).pipe(
    Effect.tapError((error) => Console.error(Cause.pretty(Cause.fail(error)))),
    Effect.tapDefect(flow(Cause.die, Cause.pretty, Console.error)),
    Effect.provide(NodeServices.layer),
    NodeRuntime.runMain({ disableErrorReporting: true }),
);
