import type { CSSProperties, ReactElement } from 'react';
import type { Composition, Placement } from '../model/placement.ts';
import { mediaRatio } from './display.ts';
import { MediaFigure } from './figure.tsx';

// --- [CONSTANTS] -----------------------------------------------------------------------

const scales = {
    full: '[--scale:1]',
    medium: '[--scale:0.78] max-sm:[--scale:0.9]',
    small: '[--scale:0.58] max-sm:[--scale:0.75]',
} as const;
const alignment = { left: 'justify-start', center: 'justify-center', right: 'justify-end' } as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

function CompositionItem({ placement, renderMedia, active, preview }: { placement: typeof Placement.Type; renderMedia: boolean; active: boolean; preview: boolean }): ReactElement {
    return (
        <div className="flex min-w-0 justify-center">
            {renderMedia ? (
                <MediaFigure active={active} placement={placement} preload={!preview} presentation={preview ? 'preview' : 'expandable'} priority={false} />
            ) : (
                // biome-ignore lint/nursery/noInlineStyles: Aspect ratio comes from the uploaded dimensions
                <div className="max-h-(--media-height) w-full bg-surface" style={{ aspectRatio: mediaRatio(placement) }} />
            )}
        </div>
    );
}
function CompositionGrid({ composition, renderMedia, active, preview }: { composition: typeof Composition.Type; renderMedia: boolean; active: boolean; preview: boolean }): ReactElement {
    const [first, second] = composition.items;
    const style: CSSProperties & Record<'--pair-columns', string> = { '--pair-columns': composition.items.map((placement) => `${mediaRatio(placement)}fr`).join(' ') };
    return (
        <div
            className={`flex items-center ${scales[composition.scale]} ${alignment[composition.align]} ${preview ? 'border border-line bg-surface p-5 [--media-height:calc(280px*var(--scale))] max-sm:p-3 max-sm:[--media-height:calc(210px*var(--scale))] [&_figure>div]:bg-background' : 'min-h-[32svh] py-3 [--media-height:calc(64svh*var(--scale))] max-sm:min-h-0 max-sm:[--media-height:calc(58svh*var(--scale))]'}`}
            // biome-ignore lint/nursery/noInlineStyles: Pair column ratios come from uploaded dimensions
            style={style}
        >
            <div className={`grid w-[calc(100%*var(--scale))] items-center gap-[30px] max-sm:gap-[26px] max-md:gap-5 ${second ? 'grid-cols-(--pair-columns) max-sm:grid-cols-1' : ''}`}>
                <CompositionItem active={active} placement={first} preview={preview} renderMedia={renderMedia} />
                {second && <CompositionItem active={active} placement={second} preview={preview} renderMedia={renderMedia} />}
            </div>
        </div>
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { CompositionGrid };
