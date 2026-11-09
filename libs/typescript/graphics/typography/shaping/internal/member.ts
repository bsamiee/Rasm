import { Array, Data, Effect, Option, Record, Schema, Struct } from 'effect';
import type { Font } from 'fontkit';
import { type AxisInfo, type Blob, Face } from 'harfbuzzjs';
import { Names } from '#/shaping/internal/names';
import type { Tables } from '#/shaping/internal/tables';

// --- [TYPES] ---------------------------------------------------------------------------

type Row = ReturnType<Names['namedInstance']> & { readonly variations: Record.ReadonlyRecord<string, number> };

// --- [MODELS] --------------------------------------------------------------------------

class Member extends Data.Class<{
    readonly index: number;
    readonly sfnt: Font;
    readonly handle: Face;
    readonly names: Names;
    readonly axisInfos: readonly AxisInfo[];
    readonly base: Row;
    readonly namedInstances: readonly Row[];
}> {
    static readonly of = (blob: Blob, sfnt: Font, instances: (typeof Tables.Type)['fvar']['instance'], index: number) => {
        const handle = new Face(blob, index);
        const names = Names.of(handle);
        const axisInfos = Object.values(handle.getAxisInfos());
        const postscriptName = names.find('postscriptName');
        const tags = axisInfos.map(Struct.get('tag'));
        return new Member({
            index,
            sfnt,
            handle,
            names,
            axisInfos,
            base: { names: { postscriptName, fullName: names.find('fullName'), style: names.find('style') }, keys: Option.toArray(postscriptName), variations: Record.fromIterableWith(axisInfos, ({ tag, default: fallback }) => [tag, fallback]) },
            namedInstances: instances.map((record) => ({ ...names.namedInstance(record), variations: Record.fromEntries(Array.zip(tags, record.coord)) })),
        });
    };
    static readonly generated = Effect.fnUntraced(function* (members: readonly Member[], wanted: string) {
        const { prefix, variations: decoded } = yield* Schema.decodeUnknownEffect(Names.Generated)(wanted);
        const member = yield* Effect.fromOption(Array.findFirst(members, (candidate) => Option.contains(candidate.names.prefix, prefix) && Array.isReadonlyArrayEmpty(Array.difference(Record.keys(decoded), Record.keys(candidate.base.variations)))));
        return { member, variations: { ...member.base.variations, ...decoded } };
    });

    get rows() {
        return [this.base, ...this.namedInstances].map((row) => ({ member: this, ...row }));
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Member };
