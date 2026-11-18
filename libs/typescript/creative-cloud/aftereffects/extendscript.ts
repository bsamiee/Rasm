import { illegalArgument, keys, preserve, within } from '@rasm/extendscript/host';
import { type Containment, read } from '@rasm/object-model/host';

// --- [CONSTANTS] -----------------------------------------------------------------------

const COARSEST = 99;
const containment: Containment = {
    root: app,
    type: (value) => Object.prototype.toString.call(value).slice('[object '.length, -1),
    kinds: {
        project: { is: (value) => value === app.project },
        item: { collection: 'items', base: 1, is: (value) => value instanceof CompItem || value instanceof FolderItem || value instanceof FootageItem, owner: () => app.project },
        layer: { collection: 'layers', base: 1, is: (value) => read(value, 'containingComp') instanceof CompItem && typeof read(value, 'id') === 'number', owner: (value) => read(value, 'containingComp') },
    },
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const session = () => {
    app.beginSuppressDialogs();
    return () => {
        app.endSuppressDialogs(false);
    };
};
const undo = (label: string) => {
    app.beginUndoGroup(label);
    return () => {
        app.endUndoGroup();
    };
};
const open = (path: string): object => {
    const project = app.open(new File(path));
    if (project === null) {
        throw illegalArgument(`${path} opened no project`);
    }
    return project;
};
const rasterize = (composition: object, longestSide: number, options: Readonly<Record<string, unknown>> | undefined, path: string): void => {
    const accepted = CompItem.name;
    if (!(composition instanceof CompItem)) {
        throw illegalArgument(`${containment.type(composition)} is no ${accepted}, rasterize takes a ${accepted}`);
    }
    if (options !== undefined) {
        throw illegalArgument(`${accepted} ${composition.name} takes no PNG options, received ${keys(options).join(', ')}`);
    }
    const longest = Math.max(composition.width, composition.height);
    const factor = Math.ceil(longest / longestSide);
    if (factor > COARSEST) {
        throw illegalArgument(`${accepted} ${composition.name} spans ${longest} px and resolutionFactor stops at ${COARSEST}, longestSide takes ${Math.ceil(longest / COARSEST)} or more`);
    }
    within(
        () => preserve(composition, { resolutionFactor: [factor, factor] }),
        () => {
            // oxlint-disable-next-line typescript/no-confusing-void-expression -- types-for-adobe declares saveFrameToPng void, After Effects returns a deferred call holding wait()
            const deferred: unknown = composition.saveFrameToPng(composition.time, new File(path));
            const wait = read(deferred, 'wait');
            if (typeof wait !== 'function') {
                throw new TypeError(`saveFrameToPng returned ${typeof deferred} without wait, the PNG write of ${path} is unconfirmed`);
            }
            wait.call(deferred);
        },
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { containment, open, rasterize, session, undo };
