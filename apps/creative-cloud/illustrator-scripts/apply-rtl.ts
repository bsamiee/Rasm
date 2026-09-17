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

const { has, items, only, reference, run, typed, visit }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

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

const written = (rows: Rows, frame: number, range: TextRange, write: Write, at: Site): void => {
    const attributes: CharacterAttributes | ParagraphAttributes = write.scope === 'character' ? range.characterAttributes : range.paragraphAttributes;
    if (!has(attributes, write.property)) {
        rows.unavailable.push({ frame, scope: write.scope, property: write.property, reason: 'absentFromReflect' });
        return;
    }
    const value = resolved(write.value);
    attributes[write.property] = value;
    const readback = attributes[write.property];
    const rendered = reference(readback, at).value;
    if (String(readback) === String(value)) {
        rows.applied.push({ frame, scope: write.scope, property: write.property, value: rendered });
        return;
    }
    rows.unavailable.push({ frame, scope: write.scope, property: write.property, reason: 'readbackDiffers', value: rendered });
};

// --- [ENTRY] ---------------------------------------------------------------------------

const applyRtl = (request: { readonly target: 'selection' | 'document'; readonly writes: Write[] }, at: Site): Reading<JsonObject> => {
    const doc = app.activeDocument;
    const frames = request.target === 'document' ? items<TextFrame>(doc.textFrames) : only(items<PageItem>(doc.selection), typed<TextFrame>('TextFrame'));
    const rows: Rows = { applied: [], unavailable: [] };
    visit(frames, (frame, index): void => visit(request.writes, (write): void => written(rows, index, frame.textRange, write, at)));
    return { value: { kind: 'rtlApplied', frames: frames.length, applied: rows.applied, unavailable: rows.unavailable }, unavailable: [] };
};

run(applyRtl);
