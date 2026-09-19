/// <reference path="./prelude.ts"/>

// --- [PRELUDE] -------------------------------------------------------------------------

const { assign, collect, each, flatMap, flatten, fold, items, present, reference, reflection, run, select, split, typed, visit }: Prelude = $.evalFile(
    new File(`${new File($.fileName).path}/prelude.jsx`),
);

// --- [WRITES] --------------------------------------------------------------------------

interface Write {
    readonly scope: 'character' | 'paragraph';
    readonly property: string;
    readonly value: { readonly enumeration: string; readonly member: string } | { readonly literal: number | boolean };
}

const written = (frame: number, range: TextRange, write: Write, at: Site): JsonObject => {
    const attributes = write.scope === 'character' ? range.characterAttributes : range.paragraphAttributes;
    if (!fold(reflection(attributes), false, (found, reflected): boolean => found || reflected.find(write.property) !== null)) {
        return { frame, scope: write.scope, property: write.property, reason: 'absentFromReflect' };
    }
    const value = 'enumeration' in write.value ? $.global[write.value.enumeration][write.value.member] : write.value.literal;
    assign(attributes, write.property, value);
    const readback = attributes[write.property as keyof typeof attributes];
    const rendered = reference(readback, at).value;
    return readback === value
        ? { frame, scope: write.scope, property: write.property, value: rendered }
        : { frame, scope: write.scope, property: write.property, value: rendered, reason: 'readbackDiffers' };
};

// --- [ENTRY] ---------------------------------------------------------------------------

const applyRtl = (request: { readonly target: 'selection' | 'document'; readonly writes: Write[] }, at: Site): Reading<JsonObject> => {
    const doc = app.activeDocument;
    const ranges = request.target === 'selection' ? fold<Story, TextRange[]>(items(doc.stories), [], (selected, story): TextRange[] => selected.concat(story.textSelection)) : [];
    if (ranges.length === 0) {
        const frames = request.target === 'document' ? items<TextFrame>(doc.textFrames) : select(flatten(items<PageItem>(doc.selection)), typed<TextFrame>('TextFrame'));
        visit(frames, (frame): void => {
            ranges.push(frame.textRange);
        });
    }
    const writes = flatMap(ranges, (range, index) => collect(request.writes, (write) => ({ index, range, write })));
    const reading = each(at, writes, ({ index, range, write }, site): Reading<JsonObject> => present(written(index, range, write, site)));
    return { value: split(reading.value), unavailable: reading.unavailable };
};

run(applyRtl);
