import { MenuButton } from '@rasm/ui/collection/menu-button';
import { Choice } from '@rasm/ui/form/choice';
import { Field, SliderField } from '@rasm/ui/form/field';
import { Match, Record, Schema, Struct } from 'effect';
import type { ReactNode } from 'react';
import { descriptionLabel, sheetLabel } from '#/media/labels';
import { type AssetCollection, ImageAsset, sheets } from '#/model/asset';
import { type Placement, percentBounds } from '#/model/placement';

// --- [CONSTANTS] -----------------------------------------------------------------------

const imageRoles = Record.collect({ drawing: 'Drawing (full sheet)', photograph: 'Photograph' } satisfies Record.ReadonlyRecord<Extract<typeof Placement.Type, { readonly kind: 'image' }>['role'], string>, (id, label) => ({ id, label }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const PlacementFields = ({ value, assets, onChange, children }: { readonly value: typeof Placement.Type; readonly assets: typeof AssetCollection.Type; readonly onChange: (value: typeof Placement.Type) => void; readonly children?: ReactNode }) => (
    <div className="border-line flex flex-col gap-3.25 border-t pt-4.5">
        <span className="text-caption wrap-anywhere">{value.asset.name}</span>
        <Field label="Caption" onChange={(caption) => onChange({ ...value, caption })} value={value.caption} />
        {Match.value(value).pipe(
            Match.discriminatorsExhaustive('kind')({
                pdf: (sheet) => (
                    <MenuButton aria-label="Sheet" onAction={({ page }) => onChange({ ...sheet, page })} options={sheets(sheet.asset).map((page) => ({ id: String(page.number), label: sheetLabel(page, sheet.asset.pages.length), page }))}>
                        {sheetLabel(sheet.page, sheet.asset.pages.length)}
                    </MenuButton>
                ),
                image: (image) => {
                    const { framing } = image;
                    return (
                        <>
                            <Choice label="Image use" onChange={({ id }) => onChange({ ...image, role: id, framing: { mode: 'contain' } })} options={imageRoles} value={image.role} />
                            {image.role === 'photograph' && (
                                <Choice
                                    label="Framing"
                                    onChange={(choice) => onChange({ ...image, framing: choice.framing })}
                                    options={
                                        [
                                            { id: 'contain', label: 'Fit whole image', framing: { mode: 'contain' } },
                                            { id: 'cover', label: 'Crop to frame', framing: { mode: 'cover', x: 50, y: 50 } },
                                        ] as const
                                    }
                                    value={framing.mode}
                                />
                            )}
                            {image.role === 'photograph' && framing.mode === 'cover' && (
                                <div className="field-pair">
                                    <SliderField label="Horizontal focal point" maxValue={percentBounds.maximum} minValue={percentBounds.minimum} onChange={(x) => onChange({ ...image, framing: { ...framing, x } })} value={framing.x} />
                                    <SliderField label="Vertical focal point" maxValue={percentBounds.maximum} minValue={percentBounds.minimum} onChange={(y) => onChange({ ...image, framing: { ...framing, y } })} value={framing.y} />
                                </div>
                            )}
                        </>
                    );
                },
                video: (film) => (
                    <div className="flex flex-col gap-3">
                        <MenuButton
                            aria-label="Video poster"
                            onAction={({ asset }) => onChange({ ...film, poster: asset })}
                            options={Object.values(assets)
                                .filter(Schema.is(ImageAsset))
                                .map((asset) => ({ id: asset.id, label: asset.name, asset }))}
                        >
                            {film.poster === undefined ? 'Choose poster image' : `Poster · ${film.poster.name}`}
                        </MenuButton>
                        {film.poster && (
                            <button className="button button-ghost self-start" onClick={() => onChange(Struct.omit(film, ['poster']))} type="button">
                                Clear poster
                            </button>
                        )}
                        <p className="hint">Poster shows before playback. Videos with speech or meaningful audio need burned-in captions. Transcripts do not replace captions</p>
                    </div>
                ),
            }),
        )}
        <Field label="Accessible description" onChange={(alt) => onChange({ ...value, alt })} placeholder="What image or sheet conveys" value={value.alt ?? ''} />
        <Field label={descriptionLabel(value)} multiline={true} onChange={(description) => onChange({ ...value, description })} value={value.description ?? ''} />
        {children}
    </div>
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { PlacementFields };
