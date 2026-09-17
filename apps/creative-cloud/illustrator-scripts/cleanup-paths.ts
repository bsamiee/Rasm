/// <reference path="./prelude.ts"/>

// --- [HOST] ----------------------------------------------------------------------------

declare const $: $;
declare const app: Application;

declare global {
    enum ElementPlacement {}
}

// --- [PRELUDE] -------------------------------------------------------------------------

const { collect, flatten, items, run, select, visit }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

// --- [OPERATIONS] ----------------------------------------------------------------------

type Operation = 'strayPoints' | 'cleanUp' | 'simplify' | 'outlineStroke' | 'expand' | 'expandAppearance' | 'trimMasks';

interface Commands {
    readonly strayPoints: string;
    readonly cleanUp: string;
    readonly simplify: string;
    readonly outlineStroke: string;
    readonly expand: string;
    readonly expandAppearance: string;
    readonly noCompoundPath: string;
    readonly ungroup: string;
    readonly compoundPath: string;
    readonly livePathfinderCrop: string;
}

const clippingGroups = (list: PageItem[]): GroupItem[] =>
    select(
        collect(list, (item): PageItem => item),
        (item): boolean => item.typename === 'GroupItem' && (item as GroupItem).clipped,
    ) as GroupItem[];

const outlined = (group: GroupItem): void => {
    visit(items(group.textFrames), (frame): void => {
        const fill = frame.textRange.characterAttributes.fillColor;
        const outline = frame.createOutline();
        visit(items(outline.pathItems), (path): void => {
            const glyph = path;
            glyph.fillColor = fill;
        });
    });
};

const trimmed = (group: GroupItem, keepFilledMask: boolean, commands: Commands): void => {
    visit(items(group.pathItems), (path): void => {
        const member = path;
        member.evenodd = false;
    });
    outlined(group);
    visit(items(group.compoundPathItems), (compound): void => {
        app.activeDocument.selection = [compound];
        app.executeMenuCommand(commands.noCompoundPath);
        app.executeMenuCommand(commands.ungroup);
        app.executeMenuCommand(commands.compoundPath);
    });
    const [mask] = select(items(group.pathItems), (path): boolean => path.clipping);
    if (keepFilledMask && mask?.filled === true) {
        mask.duplicate(group, ElementPlacement.PLACEAFTER);
    }
    const { opacity, blendingMode } = group;
    app.activeDocument.selection = [group];
    app.executeMenuCommand(commands.livePathfinderCrop);
    app.executeMenuCommand(commands.expandAppearance);
    visit(items<PageItem>(app.activeDocument.selection), (item): void => {
        const result = item;
        result.opacity = opacity;
        result.blendingMode = blendingMode;
    });
};

// --- [ENTRY] ---------------------------------------------------------------------------

const cleanupPaths = (request: { readonly operations: Operation[]; readonly keepFilledMask: boolean; readonly commands: Commands }, _at: Site): Reading<JsonObject> => {
    const doc = app.activeDocument;
    const selected = items<PageItem>(doc.selection);
    const applied: JsonObject[] = [];
    visit(request.operations, (operation): void => {
        if (operation === 'trimMasks') {
            const groups = clippingGroups(flatten(selected).concat(selected));
            visit(groups, (group): void => trimmed(group, request.keepFilledMask, request.commands));
            applied.push({ operation, count: groups.length });
            return;
        }
        app.activeDocument.selection = selected;
        app.executeMenuCommand(request.commands[operation]);
        applied.push({ operation, count: selected.length });
    });
    return { value: { kind: 'pathsCleaned', applied }, unavailable: [] };
};

run(cleanupPaths);
