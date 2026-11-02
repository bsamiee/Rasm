import { CustomProperties } from '@rasm/ui/custom-properties';
import { Array, Number } from 'effect';
import type { ComponentProps } from 'react';
import { MediaFigure } from '#/media/media-figure';
import { type Composition, placementRatio } from '#/model/placement';

// --- [COMPOSITION] ---------------------------------------------------------------------

const CompositionGrid = ({ composition, active, presentation, renderMedia = true }: { composition: typeof Composition.Type; active: boolean; presentation: ComponentProps<typeof MediaFigure>['presentation']; renderMedia?: boolean }) => {
    const [first, second] = composition.items;
    const ratios = Array.map(composition.items, placementRatio);
    return (
        <div className="composition" data-align={composition.align} data-presentation={presentation} data-scale={composition.scale}>
            <CustomProperties properties={{ '--ratio-max': Array.max(ratios, Number.Order), '--ratio-sum': Number.sumAll(ratios), '--pair-columns': ratios.map((ratio) => `minmax(0, ${ratio / Array.min(ratios, Number.Order)}fr)`).join(' ') }}>
                {(scope) => (
                    <div className="composition-items" {...scope}>
                        <MediaFigure active={active} placement={first} presentation={presentation} renderMedia={renderMedia} />
                        {second && <MediaFigure active={active} placement={second} presentation={presentation} renderMedia={renderMedia} />}
                    </div>
                )}
            </CustomProperties>
        </div>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { CompositionGrid };
