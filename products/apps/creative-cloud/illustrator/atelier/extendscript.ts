import 'es5-shim';
import 'es5-shim/es5-sham.js';
import { serve } from '@rasm/extendscript/host';
import { containment, open, rasterize, session } from '@rasm/illustrator/extendscript';
import type { Open, Rasterize } from '@rasm/object-model';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default serve(
    { containment, session },
    {
        open: ({ file }: typeof Open.Encoded) => open(file),
        rasterize: ({ target, longestSide, options }: typeof Rasterize.Encoded, { resolve, grouped }) => {
            const path = `${Folder.temp.fsName}/${new File().name}.png`;
            grouped(() => {
                rasterize(resolve(target), longestSide, options, path);
            });
            return path;
        },
    },
);
