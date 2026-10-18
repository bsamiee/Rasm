import { Match, Record, Schema, Struct } from 'effect';
import { descriptionLabel, sheetLabel } from '../media/display.ts';
import { type AssetCollection, ImageAsset, sheets } from '../model/asset.ts';
import type { Placement } from '../model/placement.ts';
import { Field, MenuButton, SelectField, SliderField } from './controls.tsx';

// --- [COMPOSITION] ---------------------------------------------------------------------

const PlacementFields = ({ value, assets, onChange, onHero, onCover }: { value: typeof Placement.Type; assets: typeof AssetCollection.Type; onChange: (value: typeof Placement.Type) => void; onHero?: (placement: typeof Placement.Type) => void; onCover?: (placement: typeof Placement.Type) => void }) => (
    <div className="flex flex-col gap-3.25 border-line border-t pt-4.5">
        <span className="wrap-anywhere text-caption">{value.asset.name}</span>
        <Field label="Caption" onChange={(caption) => onChange({ ...value, caption })} value={value.caption} />
        {Match.value(value).pipe(
            Match.discriminatorsExhaustive('kind')({
                pdf: (sheet) => (
                    <MenuButton label="Sheet" onAction={({ page }) => onChange({ ...sheet, page })} options={sheets(sheet.asset).map((page) => ({ id: String(page.number), label: sheetLabel(page, sheet.asset.pages.length), page }))}>
                        {sheetLabel(sheet.page, sheet.asset.pages.length)}
                    </MenuButton>
                ),
                image: (image) => {
                    const cover = image.role === 'photograph' && image.framing.mode === 'cover' ? image.framing : undefined;
                    return (
                        <>
                            <SelectField
                                label="Image use"
                                onChange={({ id }) => onChange({ ...image, role: id, framing: { mode: 'contain' } })}
                                options={Record.toEntries({ drawing: 'Drawing — complete sheet', photograph: 'Photograph' } satisfies Record.ReadonlyRecord<typeof image.role, string>).map(([id, label]) => ({ id, label }))}
                                value={image.role}
                            />
                            {image.role === 'photograph' && (
                                <>
                                    <SelectField
                                        label="Framing"
                                        onChange={({ framing }) => onChange({ ...image, framing })}
                                        options={
                                            [
                                                { id: 'contain', label: 'Show complete image', framing: { mode: 'contain' } },
                                                { id: 'cover', label: 'Crop to frame', framing: { mode: 'cover', x: 50, y: 50 } },
                                            ] as const
                                        }
                                        value={image.framing.mode}
                                    />
                                    {cover && (
                                        <div className="field-pair">
                                            <SliderField label="Focal point, horizontal" onChange={(x) => onChange({ ...image, framing: { ...cover, x } })} value={cover.x} />
                                            <SliderField label="Focal point, vertical" onChange={(y) => onChange({ ...image, framing: { ...cover, y } })} value={cover.y} />
                                        </div>
                                    )}
                                </>
                            )}
                        </>
                    );
                },
                video: (film) => (
                    <div className="flex flex-col gap-3">
                        <MenuButton
                            label="Video poster"
                            onAction={({ asset }) => onChange({ ...film, poster: asset })}
                            options={Object.values(assets)
                                .filter(Schema.is(ImageAsset))
                                .map((asset) => ({ id: asset.id, label: asset.name, asset }))}
                        >
                            {film.poster === undefined ? 'Choose poster image' : `Poster: ${film.poster.name}`}
                        </MenuButton>
                        {film.poster && (
                            <button className="button button-ghost self-start" onClick={() => onChange(Struct.omit(film, ['poster']))} type="button">
                                Clear poster
                            </button>
                        )}
                        <p className="hint">Choose an uploaded image for the video’s still preview. For speech or meaningful audio, export the video with captions burned in. A transcript alone does not provide synchronized captions.</p>
                    </div>
                ),
            }),
        )}
        <Field label="Accessible description" onChange={(alt) => onChange({ ...value, alt })} placeholder="What does this image or sheet communicate?" value={value.alt ?? ''} />
        <Field label={descriptionLabel(value)} multiline={true} onChange={(description) => onChange({ ...value, description })} value={value.description ?? ''} />
        {onHero && onCover && (
            <div className="actions">
                <button className="button button-outline" onClick={() => onHero(value)} type="button">
                    Use as hero
                </button>
                <button className="button button-outline" onClick={() => onCover(value)} type="button">
                    Use as chapter thumbnail
                </button>
            </div>
        )}
    </div>
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { PlacementFields };
