/// <reference path="./prelude.ts"/>

// --- [HOST] ----------------------------------------------------------------------------

declare const $: $;
declare const app: Application;

declare global {
    enum ElementPlacement {}
}

// --- [PRELUDE] -------------------------------------------------------------------------

const { collect, flatten, items, only, run, select, typed, visit }: Prelude = $.evalFile(new File(`${new File($.fileName).path}/prelude.jsx`));

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

const clippingGroups = (list: PageItem[]): GroupItem[] => only(list, (item): item is GroupItem => typed<GroupItem>('GroupItem')(item) && item.clipped);

const outlined = (group: GroupItem): void => {
    visit(items(group.textFrames), (frame): void => {
        const fill = frame.textRange.characterAttributes.fillColor;
        visit(items(frame.createOutline().pathItems), (path): void => {
            const glyph = path;
            glyph.fillColor = fill;
        });
    });
};

const trimmed = (groups: GroupItem[], keepFilledMask: boolean, commands: Commands): number => {
    const doc = app.activeDocument;
    visit(groups, (group): void => {
        visit(items(group.pathItems), (path): void => {
            const member = path;
            member.evenodd = false;
        });
        outlined(group);
        visit(items(group.compoundPathItems), (compound): void => {
            doc.selection = [compound];
            app.executeMenuCommand(commands.noCompoundPath);
            app.executeMenuCommand(commands.ungroup);
            app.executeMenuCommand(commands.compoundPath);
        });
        const [filledMask] = select(items(group.pathItems), (path): boolean => path.clipping && path.filled);
        if (keepFilledMask && filledMask !== undefined) {
            filledMask.duplicate(group, ElementPlacement.PLACEAFTER);
        }
        const { opacity, blendingMode } = group;
        doc.selection = [group];
        app.executeMenuCommand(commands.livePathfinderCrop);
        app.executeMenuCommand(commands.expandAppearance);
        visit(items<PageItem>(doc.selection), (item): void => {
            const result = item;
            result.opacity = opacity;
            result.blendingMode = blendingMode;
        });
    });
    return groups.length;
};

// --- [ENTRY] ---------------------------------------------------------------------------

const cleanupPaths = (request: { readonly operations: Operation[]; readonly keepFilledMask: boolean; readonly commands: Commands }, _at: Site): Reading<JsonObject> => {
    const doc = app.activeDocument;
    const selected = items<PageItem>(doc.selection);
    const applied = collect(request.operations, (operation): JsonObject => {
        if (operation === 'trimMasks') {
            return { operation, count: trimmed(clippingGroups(flatten(selected).concat(selected)), request.keepFilledMask, request.commands) };
        }
        doc.selection = selected;
        app.executeMenuCommand(request.commands[operation]);
        return { operation, count: selected.length };
    });
    return { value: { kind: 'pathsCleaned', applied }, unavailable: [] };
};

run(cleanupPaths);
