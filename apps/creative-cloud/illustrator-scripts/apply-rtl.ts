/// <reference path="./prelude.ts"/>

// --- [HOST] ----------------------------------------------------------------------------

declare const $: $;
declare const app: Application;

declare global {
    interface CharacterAttributes {
        [name: string]: unknown;
    }

    interface ParagraphAttributes {
        [name: string]: unknown;
    }
}

// --- [PRELUDE] -------------------------------------------------------------------------

const { contains, items, names, run, select, visit }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [WRITES] --------------------------------------------------------------------------

interface Write {
    readonly scope: 'character' | 'paragraph';
    readonly property: string;
    readonly value: { readonly enumeration: string; readonly member: string } | { readonly literal: number | boolean };
}

interface Rows {
    readonly applied: JsonObject[];
    readonly unavailable: JsonObject[];
}

const resolved = (value: Write['value']): unknown => ('enumeration' in value ? $.global[value.enumeration][value.member] : value.literal);

const rendered = (value: unknown): Json => (typeof value === 'number' || typeof value === 'boolean' ? value : String(value));

const written = (rows: Rows, frame: number, range: TextRange, write: Write): void => {
    const attributes: CharacterAttributes | ParagraphAttributes = write.scope === 'character' ? range.characterAttributes : range.paragraphAttributes;
    if (!contains(names(attributes), write.property)) {
        rows.unavailable.push({ frame, scope: write.scope, property: write.property, reason: 'absentFromReflect' });
        return;
    }
    const value = resolved(write.value);
    attributes[write.property] = value;
    const readback = attributes[write.property];
    rows[String(readback) === String(value) ? 'applied' : 'unavailable'].push(
        String(readback) === String(value)
            ? { frame, scope: write.scope, property: write.property, value: rendered(readback) }
            : { frame, scope: write.scope, property: write.property, reason: 'readbackDiffers', value: rendered(readback) },
    );
};

// --- [ENTRY] ---------------------------------------------------------------------------

const applyRtl = (request: { readonly target: 'selection' | 'document'; readonly writes: Write[] }, _at: Site): Reading<JsonObject> => {
    const doc = app.activeDocument;
    const frames = request.target === 'document' ? items<TextFrame>(doc.textFrames) : select(items<PageItem>(doc.selection), (item): boolean => item.typename === 'TextFrame');
    const rows: Rows = { applied: [], unavailable: [] };
    visit(frames, (frame, index): void => {
        const range = (frame as TextFrame).textRange;
        visit(request.writes, (write): void => written(rows, index, range, write));
    });
    return { value: { kind: 'rtlApplied', frames: frames.length, applied: rows.applied, unavailable: rows.unavailable }, unavailable: [] };
};

run(applyRtl);
