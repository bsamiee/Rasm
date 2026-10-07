import type { ComponentProps, CSSProperties, ReactElement } from 'react';
import type { Composition } from '../model/placement.ts';
import { mediaRatio } from './display.ts';
import { MediaFigure } from './figure.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

function CompositionGrid({ composition, renderMedia, active, presentation }: { composition: typeof Composition.Type; renderMedia: boolean; active: boolean; presentation: ComponentProps<typeof MediaFigure>['presentation'] }): ReactElement {
    const [first, second] = composition.items;
    const ratios = composition.items.map(mediaRatio);
    const minimumRatio = Math.min(...ratios);
    const scales = { full: '[--scale:1]', medium: '[--scale:0.78] @max-[600px]:[--scale:0.9]', small: '[--scale:0.58] @max-[600px]:[--scale:0.75]' };
    const alignment = { left: 'justify-start [&>div]:justify-items-start', center: 'justify-center [&>div]:justify-items-center', right: 'justify-end [&>div]:justify-items-end' };
    const style: CSSProperties & Record<'--stack-width' | '--spread-width' | '--pair-columns', string> = {
        '--stack-width': `calc(var(--media-height) * ${Math.max(...ratios)})`,
        '--spread-width': `calc(var(--media-height) * ${ratios.reduce((sum, ratio) => sum + ratio, 0)} + var(--composition-gap) * ${ratios.length - 1})`,
        '--pair-columns': ratios.map((ratio) => `minmax(0, ${ratio / minimumRatio}fr)`).join(' '),
    };
    return (
        <div
            className={`@container group/composition flex items-center ${alignment[composition.align]} data-[presentation=thumbnail]:aspect-[4/3] data-[presentation=expandable]:min-h-[32svh] data-[presentation=preview]:border data-[presentation=preview]:border-line data-[presentation=preview]:bg-surface data-[presentation=preview]:p-5 data-[presentation=expandable]:py-3 data-[presentation=expandable]:max-sm:min-h-0 data-[presentation=preview]:max-sm:p-3 data-[presentation=preview]:[&_figure>div]:bg-background`}
            data-presentation={presentation}
        >
            <div
                className={`grid @min-[600px]:w-[min(calc(100%*var(--scale)),var(--spread-width))] w-[min(calc(100%*var(--scale)),var(--stack-width))] @min-[600px]:grid-cols-(--pair-columns) grid-cols-1 items-start gap-(--composition-gap) [--composition-gap:clamp(20px,3cqi,30px)] group-data-[presentation=thumbnail]/composition:grid-cols-(--pair-columns) ${scales[composition.scale]} @max-[600px]:[--media-height:calc(58svh*var(--scale))] [--media-height:calc(64svh*var(--scale))] group-data-[presentation=preview]/composition:@max-[600px]:[--media-height:calc(210px*var(--scale))] group-data-[presentation=preview]/composition:[--media-height:calc(280px*var(--scale))] group-data-[presentation=thumbnail]/composition:[--composition-gap:0.5rem] group-data-[presentation=thumbnail]/composition:[--media-height:75cqw]`}
                // biome-ignore lint/nursery/noInlineStyles: Composition dimensions and column proportions come from uploaded dimensions
                style={style}
            >
                <MediaFigure active={active} placement={first} preload={presentation === 'expandable'} presentation={presentation} priority={false} renderMedia={renderMedia} />
                {second && <MediaFigure active={active} placement={second} preload={presentation === 'expandable'} presentation={presentation} priority={false} renderMedia={renderMedia} />}
            </div>
        </div>
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { CompositionGrid };
