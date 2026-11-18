import { omv } from '@rasm/object-model/node';
import { Effect, Option } from 'effect';
import { describe, expect, it } from 'vitest';

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = (name: string, texts: readonly [string, ...string[]]) =>
    Effect.runPromise(
        Effect.map(
            Effect.flatMap(omv(texts), ({ page }) => page(name)),
            (page) => ('fields' in page ? { constructible: page.constructible, fields: page.fields.map(({ name: field }) => field), methods: page.methods.map(({ name: method, parameters }) => [method, ...parameters.map(({ type }) => Option.getOrUndefined(type))]) } : page),
        ),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('omv', () => {
    const core = `<dictionary><package>
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
    const host = `<dictionary><package>
<classdef name="Point" dynamic="true"><elements type="class"><property name="anchor"><datatype><type>Number</type><array/></datatype></property></elements></classdef>
<classdef name="Layer" dynamic="true"><elements type="instance"><property name="name"><datatype><type>String</type></datatype></property></elements></classdef>
</package></dictionary>`;

    it('should read a class with a constructor group as constructible, list no constructor among its methods, and join the datatypes a parameter states', async () => {
        await expect(read('File', [core])).resolves.toStrictEqual({ constructible: true, fields: ['length'], methods: [['write', 'String | Number[]']] });
    });

    it('should read a class without a constructor group as not constructible in a file that states constructors', async () => {
        await expect(read('Math', [core])).resolves.toStrictEqual({ constructible: false, fields: ['PI'], methods: [] });
    });

    it.each([
        ['Point', true],
        ['Layer', false],
    ] as const)('should read %s as constructible %s by its instance group in a file that states no constructor', async (name, constructible) => {
        await expect(read(name, [host])).resolves.toMatchObject({ constructible });
    });

    it('should take a class two files declare from the later file and index the classes of both', async () => {
        const index = await Effect.runPromise(Effect.map(omv([core, host]), (catalog) => catalog.index.classes));
        await expect(read('Point', [core, host])).resolves.toStrictEqual({ constructible: true, fields: ['anchor'], methods: [] });
        expect(index).toStrictEqual(['File', 'Layer', 'Math', 'Point']);
    });
});
