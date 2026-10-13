import type { ComponentProps, CSSProperties, ReactElement } from 'react';
import type { Composition } from '../model/placement.ts';
import { mediaRatio } from './display.ts';
import { MediaFigure } from './figure.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

function CompositionGrid({ composition, renderMedia, active, presentation }: { composition: typeof Composition.Type; renderMedia: boolean; active: boolean; presentation: ComponentProps<typeof MediaFigure>['presentation'] }): ReactElement {
    const [first, second] = composition.items;
    const ratios = composition.items.map(mediaRatio);
    const minimumRatio = Math.min(...ratios);
    const style: CSSProperties & Record<'--stack-width' | '--spread-width' | '--pair-columns', string> = {
        '--stack-width': `calc(var(--media-height) * ${Math.max(...ratios)})`,
        '--spread-width': `calc(var(--media-height) * ${ratios.reduce((sum, ratio) => sum + ratio, 0)} + var(--composition-gap) * ${ratios.length - 1})`,
        '--pair-columns': ratios.map((ratio) => `minmax(0, ${ratio / minimumRatio}fr)`).join(' '),
    };
    return (
        <div className="composition" data-align={composition.align} data-presentation={presentation} data-scale={composition.scale}>
            <div
                className="composition-items"
                // biome-ignore lint/nursery/noInlineStyles: Composition dimensions and column proportions come from uploaded dimensions
                style={style}
            >
                <MediaFigure active={active} placement={first} presentation={presentation} priority={false} renderMedia={renderMedia} />
                {second && <MediaFigure active={active} placement={second} presentation={presentation} priority={false} renderMedia={renderMedia} />}
            </div>
        </div>
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { CompositionGrid };
