import { CustomProperties } from '@rasm/ui';
import { Array, Number } from 'effect';
import type { ComponentProps } from 'react';
import { MediaFigure } from '#/media/media-figure.tsx';
import { type Composition, Placement } from '#/model/placement';

// --- [COMPOSITION] ---------------------------------------------------------------------

const CompositionGrid = ({ composition, active, presentation, renderMedia }: { composition: typeof Composition.Type; active: boolean; presentation: ComponentProps<typeof MediaFigure>['presentation']; renderMedia: boolean }) => {
    const [first, second] = composition.items;
    const ratios = Array.map(composition.items, Placement.ratio);
    return (
        <div className="composition" data-align={composition.align} data-presentation={presentation} data-scale={composition.scale}>
            <CustomProperties properties={{ '--ratio-max': Array.max(ratios, Number.Order), '--ratio-sum': Math.sumPrecise(ratios), '--pair-columns': ratios.map((ratio) => `minmax(0, ${ratio / Array.min(ratios, Number.Order)}fr)`).join(' ') }}>
                {(scope) => (
                    <div className="composition-items" {...scope}>
                        <MediaFigure active={active} placement={first} presentation={presentation} priority={false} renderMedia={renderMedia} />
                        {second && <MediaFigure active={active} placement={second} presentation={presentation} priority={false} renderMedia={renderMedia} />}
                    </div>
                )}
            </CustomProperties>
        </div>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { CompositionGrid };
