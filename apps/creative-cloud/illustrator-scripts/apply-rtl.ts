/// <reference path="./prelude.ts"/>

// --- [PRELUDE] -------------------------------------------------------------------------

const { assign, collect, flatMap, fold, items, present, reference, reflection, run, select, split, typed }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [WRITES] --------------------------------------------------------------------------

interface Write {
    readonly scope: 'character' | 'paragraph';
    readonly property: string;
    readonly value: { readonly enumeration: string; readonly member: string } | { readonly literal: number | boolean };
}

const written = (frame: number, range: TextRange, write: Write, at: Site): JsonObject => {
    const attributes = write.scope === 'character' ? range.characterAttributes : range.paragraphAttributes;
    if (!fold(reflection(attributes), false, (_none, reflected): boolean => reflected.find(write.property) !== null)) {
        return { frame, scope: write.scope, property: write.property, reason: 'absentFromReflect' };
    }
    const value = 'enumeration' in write.value ? $.global[write.value.enumeration][write.value.member] : write.value.literal;
    assign(attributes, write.property, value);
    const readback = attributes[write.property as keyof typeof attributes];
    const rendered = reference(readback, at).value;
    return String(readback) === String(value)
        ? { frame, scope: write.scope, property: write.property, value: rendered }
        : { frame, scope: write.scope, property: write.property, value: rendered, reason: 'readbackDiffers' };
};

// --- [ENTRY] ---------------------------------------------------------------------------

const applyRtl = (request: { readonly target: 'selection' | 'document'; readonly writes: Write[] }, at: Site): Reading<JsonObject> => {
    const doc = app.activeDocument;
    const frames = request.target === 'document' ? items<TextFrame>(doc.textFrames) : select(items<PageItem>(doc.selection), typed<TextFrame>('TextFrame'));
    return present(split(flatMap(frames, (frame, index): JsonObject[] => collect(request.writes, (write): JsonObject => written(index, frame.textRange, write, at)))));
};

run(applyRtl);
