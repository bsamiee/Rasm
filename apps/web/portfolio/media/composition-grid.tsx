import { CustomProperties } from '@rasm/ui';
import { Number } from 'effect';
import type { ComponentProps } from 'react';
import { MediaFigure } from '#/media/media-figure';
import { type Composition, placementRatio } from '#/model/placement';

// --- [COMPOSITION] ---------------------------------------------------------------------

const CompositionGrid = ({ composition, renderMedia, active, presentation }: { composition: typeof Composition.Type; renderMedia: boolean; active: boolean; presentation: ComponentProps<typeof MediaFigure>['presentation'] }) => {
    const [first, second] = composition.items;
    const ratios = composition.items.map(placementRatio);
    return (
        <div className="composition" data-align={composition.align} data-presentation={presentation} data-scale={composition.scale}>
            <CustomProperties properties={{ '--ratio-max': Math.max(...ratios), '--ratio-sum': Number.sumAll(ratios), '--pair-columns': ratios.map((ratio) => `minmax(0, ${ratio / Math.min(...ratios)}fr)`).join(' ') }}>
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
