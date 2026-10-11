import { Struct } from 'effect';
import type { ReactNode } from 'react';
import { type AssetCollection, ImageAsset, sheets } from '../model/asset.ts';
import type { Placement } from '../model/placement.ts';
import { Field, MenuButton, SelectField, SliderField } from './controls.tsx';

// --- [CONSTANTS] -----------------------------------------------------------------------

const roles = [
    { id: 'drawing', label: 'Drawing — complete sheet' },
    { id: 'photograph', label: 'Photograph' },
] as const;
const framings = [
    { id: 'contain', label: 'Show complete image', framing: { mode: 'contain' } },
    { id: 'cover', label: 'Crop to frame', framing: { mode: 'cover', x: 50, y: 50 } },
] as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

function PlacementFields({ value, assets, onChange, onHero, onCover }: { value: typeof Placement.Type; assets: typeof AssetCollection.Type; onChange: (value: typeof Placement.Type) => void; onHero?: () => void; onCover?: () => void }): ReactNode {
    const photograph = value.kind === 'image' && value.role === 'photograph' ? value : undefined;
    const cover = photograph?.framing.mode === 'cover' ? photograph.framing : undefined;
    return (
        <div className="flex flex-col gap-[13px] border-line border-t pt-[18px]">
            <span className="wrap-anywhere text-[13px]">{value.asset.name}</span>
            <Field label="Caption" onChange={(caption): void => onChange({ ...value, caption })} value={value.caption} />
            {value.kind === 'pdf' && (
                <MenuButton label="Sheet" onAction={({ page }): void => onChange({ ...value, page })} options={sheets(value.asset).map((page) => ({ id: String(page.number), label: `${page.number} of ${value.asset.pages.length}${page.label ? ` · ${page.label}` : ''}`, page }))}>
                    Sheet {value.page.number}
                </MenuButton>
            )}
            {value.kind === 'image' && <SelectField label="Image use" onChange={({ id }): void => onChange({ ...value, role: id, framing: { mode: 'contain' } })} options={roles} value={value.role} />}
            {value.kind === 'video' && (
                <div className="flex flex-col gap-3">
                    <MenuButton
                        label="Video poster"
                        onAction={({ asset }): void => onChange({ ...value, poster: asset })}
                        options={Object.values(assets)
                            .filter((asset) => asset instanceof ImageAsset)
                            .map((asset) => ({ id: asset.id, label: asset.name, asset }))}
                    >
                        {value.poster ? `Poster: ${value.poster.name}` : 'Choose poster image'}
                    </MenuButton>
                    {value.poster && (
                        <button className="button button-ghost self-start" onClick={(): void => onChange(Struct.omit(value, ['poster']))} type="button">
                            Clear poster
                        </button>
                    )}
                    <p className="hint">Choose an uploaded image for the video’s still preview. For speech or meaningful audio, export the video with captions burned in; a transcript alone does not provide synchronized captions.</p>
                </div>
            )}
            {photograph && <SelectField label="Framing" onChange={({ framing }): void => onChange({ ...photograph, framing })} options={framings} value={photograph.framing.mode} />}
            {photograph && cover && (
                <div className="field-pair">
                    <SliderField label="Focal point, horizontal" onChange={(x): void => onChange({ ...photograph, framing: { ...cover, x } })} value={cover.x} />
                    <SliderField label="Focal point, vertical" onChange={(y): void => onChange({ ...photograph, framing: { ...cover, y } })} value={cover.y} />
                </div>
            )}
            <Field label="Accessible description" onChange={(alt): void => onChange({ ...value, alt })} placeholder="What does this image or sheet communicate?" value={value.alt ?? ''} />
            <Field label={value.kind === 'video' ? 'Description / transcript' : 'Detailed description'} multiline={true} onChange={(description): void => onChange({ ...value, description })} value={value.description ?? ''} />
            {(onHero || onCover) && (
                <div className="actions">
                    {onHero && (
                        <button className="button button-outline" onClick={onHero} type="button">
                            Use as hero
                        </button>
                    )}
                    {onCover && (
                        <button className="button button-outline" onClick={onCover} type="button">
                            Use as chapter thumbnail
                        </button>
                    )}
                </div>
            )}
        </div>
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { PlacementFields };
