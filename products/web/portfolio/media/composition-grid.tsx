import { Array, Number } from 'effect';
import type { ComponentProps } from 'react';
import { MediaFigure } from '#/media/media-figure.tsx';
import { type Composition, Placement } from '#/model/placement';

// --- [COMPOSITION] ---------------------------------------------------------------------

const CompositionGrid = ({ composition, active, presentation, renderMedia }: { readonly composition: typeof Composition.Type; readonly active: boolean; readonly presentation: ComponentProps<typeof MediaFigure>['presentation']; readonly renderMedia: boolean }) => {
    const [first, second] = composition.items;
    const ratios = Array.map(composition.items, Placement.ratio);
    return (
        <div className="composition" data-align={composition.align} data-presentation={presentation} data-scale={composition.scale}>
            <div className="composition-items" style={{ '--ratio-max': Array.max(ratios, Number.Order), '--ratio-sum': Number.sumAll(ratios) }}>
                <MediaFigure active={active} placement={first} presentation={presentation} priority={false} renderMedia={renderMedia} />
                {second && <MediaFigure active={active} placement={second} presentation={presentation} priority={false} renderMedia={renderMedia} />}
            </div>
        </div>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { CompositionGrid };
