import '@rasm/extendscript/shim';
import { containment, declared, open, rasterize, session, undo } from '@rasm/aftereffects/extendscript';
import { serve } from '@rasm/extendscript/host';
import type { ObjectModel, Open, Rasterize } from '@rasm/object-model';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default serve(
    { containment, session, undo },
    {
        open: ({ file }: typeof Open.Encoded) => open(file),
        rasterize: ({ target, longestSide, options }: typeof Rasterize.Encoded, { resolve, grouped }) => {
            const path = `${Folder.temp.fsName}/${new File().name}.png`;
            grouped(() => {
                rasterize(resolve(target), longestSide, options, path);
            });
            return path;
        },
        object_model: ({ name }: typeof ObjectModel.Encoded) => declared(name),
    },
);
