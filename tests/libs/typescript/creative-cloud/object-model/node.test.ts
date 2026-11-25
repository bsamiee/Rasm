import { layer } from '@effect/platform-node/NodeServices';
import { assert, describe, it } from '@effect/vitest';
import { readDocuments, readManifest, readOmv, readTypeScript } from '@rasm/object-model/node';
import { Array, Effect, FileSystem, Option, Path, Schema, Stream, Struct } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const CORE = `<dictionary><package>
<classdef name="File" dynamic="true">
    <elements type="constructor"><method name="File"><parameters><parameter name="path" optional="true"><datatype><type>String</type></datatype></parameter></parameters></method></elements>
    <elements type="instance">
        <property name="length" rwaccess="readonly"><datatype><type>Number</type></datatype></property>
        <method name="write"><parameters><parameter name="text"><datatype><type>String</type></datatype><datatype><type>Number</type><array size="4"/></datatype></parameter></parameters><datatype><type>Boolean</type></datatype></method>
    </elements>
</classdef>
<classdef name="Math" dynamic="true"><elements type="class"><property name="PI" rwaccess="readonly"><datatype><type>Number</type></datatype></property></elements></classdef>
<classdef name="Point" dynamic="true"><elements type="instance"><property name="left"><datatype><type>Number</type></datatype></property></elements></classdef>
</package></dictionary>`;
const HOST = `<dictionary><package>
<classdef name="Point" dynamic="true"><elements type="class"><property name="anchor"><datatype><type>Number</type><array/></datatype></property></elements></classdef>
<classdef name="Layer" dynamic="true"><elements type="instance"><property name="name"><datatype><type>String</type></datatype></property></elements></classdef>
<classdef name="BlendMode" enumeration="true"><elements type="class"><property name="MULTIPLY"><datatype><type>number</type><value>2</value></datatype></property></elements></classdef>
</package></dictionary>`;
const PRESS = `export declare enum SaveOptions { YES = 1, NO = 2 }
export declare class Document {
    readonly name: string;
    close(saving: SaveOptions): void;
}
export declare const app: { readonly activeDocument: Document };
`;
const DOCUMENTED = [
    '{"name":"Layer","description":"A layer","properties":[{"Name":"opacity","Type":"number","Access":"RW","Min Version":"25.0","Description":"Opacité de la couche ✓"}],"methods":[]}',
    '{"name":"Overview","description":null,"properties":[],"methods":[]}',
    '{"name":"Comp","description":null,"properties":[],"methods":[{"name":"duplicate","returns":"Comp","since":null,"description":null,"parameters":[]}]}',
].join('\n');
const SCRIPTED = '[{"name":"EncoderManager","properties":null,"methods":[{"propKind":"constructor"},{"name":"encode","dataType":"boolean","propKind":"instance","async":"false","params":[{"name":"preset","dataType":"","optional":"true"},{"dataType":"string","optional":"false"}]}]}]';

// --- [MODELS] --------------------------------------------------------------------------

const Json = Schema.fromJsonString(Schema.Unknown);

// --- [OPERATIONS] ----------------------------------------------------------------------

const pieces = (text: string, size: number) => Stream.fromIterable(Array.map(Array.chunksOf(Array.fromIterable(new TextEncoder().encode(text)), size), (chunk) => Uint8Array.from(chunk)));
const read = Effect.fnUntraced(function* (texts: Array.NonEmptyReadonlyArray<string>, name: string) {
    const { page } = yield* readOmv(texts);
    const found = yield* page(name);
    return found._tag === 'Class'
        ? { constructible: found.constructible, fields: Array.map(found.fields, Struct.get('name')), methods: Array.map(found.methods, ({ name: method, parameters }) => [method, ...Array.map(parameters, ({ type }) => Option.getOrUndefined(type))]) }
        : { members: Array.map(found.members, ({ name: member, value }) => [member, Option.getOrUndefined(value)]) };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

describe(readOmv, () => {
    it.effect.each([
        { behavior: 'reads a class with a constructor group as constructible, keeps its constructor out of its methods, and joins the datatypes a parameter states', texts: [CORE], name: 'File', page: { constructible: true, fields: ['length'], methods: [['write', 'String | Number[]']] } },
        { behavior: 'reads a class without a constructor group as not constructible in a file stating constructors', texts: [CORE], name: 'Math', page: { constructible: false, fields: ['PI'], methods: [] } },
        { behavior: 'reads a class with an instance group as not constructible in a file stating no constructor', texts: [HOST], name: 'Layer', page: { constructible: false, fields: ['name'], methods: [] } },
        { behavior: 'takes a class two dictionaries declare from the later one, constructible there for lacking an instance group', texts: [CORE, HOST], name: 'Point', page: { constructible: true, fields: ['anchor'], methods: [] } },
        { behavior: 'reads an enumeration with each member and its value', texts: [HOST], name: 'BlendMode', page: { members: [['MULTIPLY', '2']] } },
    ] as const)('$behavior', ({ texts, name, page }) => Effect.map(read(texts, name), (found) => assert.deepStrictEqual<unknown>(found, page)));
});

it.layer(layer)('readTypeScript', (methods) => {
    methods.effect(
        'refuses a member its declarations lack and names the class declaring the object read',
        Effect.fnUntraced(function* () {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const modules = yield* fs.makeTempDirectoryScoped();
            yield* fs.makeDirectory(path.join(modules, 'press'));
            yield* fs.writeFileString(path.join(modules, 'press', 'package.json'), yield* Schema.encodeEffect(Json)({ name: 'press', types: 'index.d.ts' }));
            yield* fs.writeFileString(path.join(modules, 'press', 'index.d.ts'), PRESS);
            const open = yield* readTypeScript(Option.none(), modules);
            const { check, catalog } = yield* open({ root: 'press', types: [], lib: ['es2024'], preamble: "declare function require(id: 'press'): typeof import('press');", enumeration: Option.none() });
            const [refused, accepted, { index }] = yield* Effect.all([check("const held = require('press').app.activeDocument;\nheld.margin;"), check("require('press').app.activeDocument.close(1);"), catalog]);
            assert.deepStrictEqual([Array.map(refused, Struct.pick(['code', 'line', 'declared'])), accepted, index.classes, index.enumerations], [[{ code: 2339, line: Option.some(2), declared: Option.some('Document') }], [], ['Document'], ['SaveOptions']]);
        }, Effect.scoped),
    );
});

describe(readDocuments, () => {
    it.effect.prop(
        'reads each NDJSON page with members as a class whatever byte boundaries split its rows and characters, and indexes no page without members',
        { size: Schema.Int.check(Schema.isBetween({ minimum: 1, maximum: 64 })) },
        Effect.fnUntraced(function* ({ size }) {
            const { index, page } = yield* readDocuments(pieces(DOCUMENTED, size));
            const [layered, composed] = yield* Effect.all([page('Layer'), page('Comp')]);
            assert.deepStrictEqual(
                [index.classes, Array.map(layered.fields, Struct.pick(['name', 'readonly', 'version', 'description'])), Array.map(composed.methods, Struct.get('name'))],
                [['Comp', 'Layer'], [{ name: 'opacity', readonly: false, version: Option.some('25.0'), description: Option.some('Opacité de la couche ✓') }], ['duplicate']],
            );
        }),
    );
});

describe(readManifest, () => {
    it.effect(
        'reads a class with a constructor entry as constructible and names an unnamed parameter by its position',
        Effect.fnUntraced(function* () {
            const { page } = yield* readManifest(SCRIPTED);
            const { constructible, methods } = yield* page('EncoderManager');
            assert.deepStrictEqual(
                [constructible, Array.map(methods, Struct.get('name')), Array.map(methods[0]?.parameters ?? [], Struct.pick(['name', 'type', 'optional']))],
                [
                    true,
                    ['encode'],
                    [
                        { name: 'preset', type: Option.none(), optional: true },
                        { name: 'arguments[1]', type: Option.some('string'), optional: false },
                    ],
                ],
            );
        }),
    );
});
