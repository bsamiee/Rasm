import { handle, illegal, unmatched } from '@rasm/cep/extendscript';

// --- [COMPOSITION] ---------------------------------------------------------------------

// biome-ignore lint/style/noDefaultExport: the CEP bundle assigns the default export to global handle
export default handle({
    base: 1,
    suppressed: (body) => {
        app.beginSuppressDialogs();
        try {
            return body();
        } finally {
            app.endSuppressDialogs(false);
        }
    },
    undone: (label, body) => {
        app.beginUndoGroup(label);
        try {
            return body();
        } finally {
            app.endUndoGroup();
        }
    },
    open: ({ file: url }, { path }) => {
        const project = app.open(new File(path(url)));
        if (project === null) {
            throw unmatched('project opened from file', url, []);
        }
        return { type: 'Project', specifier: '/project', valid: true, ...(project.file === null ? {} : { name: project.file.name }) };
    },
    render: ({ target, longestSide, options, file: url }, { resolve, path }) => {
        const composition = resolve(target);
        if (!(composition instanceof CompItem)) {
            throw illegal(`${target} names no composition`);
        }
        if (options !== undefined) {
            throw illegal('Composition frames take no PNG options');
        }
        const held = composition.resolutionFactor;
        const factor = Math.max(1, Math.ceil(Math.max(composition.width, composition.height) / longestSide));
        composition.resolutionFactor = [factor, factor];
        try {
            composition.saveFrameToPng(composition.time, new File(path(url)));
        } finally {
            composition.resolutionFactor = held;
        }
    },
});
