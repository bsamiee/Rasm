import type { ReactNode } from 'react';
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

function PlacementFields({ value, onChange, onHero, onCover }: { value: typeof Placement.Type; onChange: (value: typeof Placement.Type) => void; onHero?: () => void; onCover?: () => void }): ReactNode {
    const photograph = value.kind === 'image' && value.role === 'photograph' ? value : undefined;
    const cover = photograph?.framing.mode === 'cover' ? photograph.framing : undefined;
    return (
        <div className="flex flex-col gap-[13px] border-line border-t pt-[18px]">
            <span className="wrap-anywhere text-[13px]">{value.asset.name}</span>
            <Field label="Caption" onChange={(caption): void => onChange({ ...value, caption })} value={value.caption} />
            {value.kind === 'pdf' && (
                <MenuButton label="Sheet" onAction={({ page }): void => onChange({ ...value, page })} options={value.asset.pages.map((page, index) => ({ id: String(index + 1), label: `${page.label || `Sheet ${index + 1}`} of ${value.asset.pages.length}`, page: { ...page, number: index + 1 } }))}>
                    Sheet {value.page.number}
                </MenuButton>
            )}
            {value.kind === 'image' && <SelectField label="Image use" onChange={({ id }): void => onChange({ ...value, role: id, framing: { mode: 'contain' } })} options={roles} value={value.role} />}
            {photograph && <SelectField label="Framing" onChange={({ framing }): void => onChange({ ...photograph, framing })} options={framings} value={photograph.framing.mode} />}
            {photograph && cover && (
                <div className="field-pair">
                    <SliderField label="Focal point, horizontal" onChange={(x): void => onChange({ ...photograph, framing: { ...cover, x } })} value={cover.x} />
                    <SliderField label="Focal point, vertical" onChange={(y): void => onChange({ ...photograph, framing: { ...cover, y } })} value={cover.y} />
                </div>
            )}
            <Field label="Accessible description" onChange={(alt): void => onChange({ ...value, alt })} placeholder="What does this image or sheet communicate?" value={value.alt ?? ''} />
            <Field label={value.kind === 'video' ? 'Description / transcript' : 'Detailed drawing description'} multiline={true} onChange={(description): void => onChange({ ...value, description })} value={value.description ?? ''} />
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
        </div>
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { PlacementFields };
