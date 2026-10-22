import { Array, Match, Option, SchemaIssue } from 'effect';
import type { Atom } from 'effect/reactivity';
import { ChevronDown } from 'lucide-react';
import type { ReactNode } from 'react';
import { Button, Dialog, DialogTrigger, Heading, Input, Label, ListBox, ListBoxItem, Menu, MenuItem, MenuTrigger, Modal, ModalOverlay, Popover, Select, SelectValue, Slider, SliderOutput, SliderThumb, SliderTrack, Text, TextArea, TextField, type TextFieldProps } from 'react-aria-components';
import type { deleteRequest, draft, saveRequest } from './services.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type RequestFailure = Atom.Failure<typeof saveRequest>['error'] | Atom.Failure<typeof draft> | Atom.Failure<typeof deleteRequest>;
interface Choice {
    readonly id: string;
    readonly label: string;
}

// --- [COMPOSITION] ---------------------------------------------------------------------

const Field = ({ label, placeholder, description, multiline, ...props }: Omit<TextFieldProps, 'children' | 'className'> & { readonly label: string; readonly placeholder?: string; readonly description?: string; readonly multiline?: boolean }) => (
    <TextField {...props} className="field">
        <Label>{label}</Label>
        {multiline === true ? <TextArea className="control min-h-22.5 resize-y" placeholder={placeholder} /> : <Input className="control" {...(placeholder === undefined ? {} : { placeholder })} />}
        {description === undefined ? null : (
            <Text className="hint mt-3.25" slot="description">
                {description}
            </Text>
        )}
    </TextField>
);
const SelectField = <T extends Choice>({ label, value, options, onChange }: { label: string; value: T['id']; options: readonly T[]; onChange: (choice: T) => void }) => (
    <Select
        className="field"
        onChange={(key) =>
            Option.map(
                Array.findFirst(options, (item) => item.id === key),
                onChange,
            )
        }
        value={value}
    >
        <Label>{label}</Label>
        <Button className="control flex items-center justify-between gap-2 text-left">
            <SelectValue className="placeholder-shown:text-muted min-w-0 truncate" />
            <ChevronDown className="size-4 shrink-0" />
        </Button>
        <Popover className="editor-popover">
            <ListBox className="outline-none" items={options}>
                {(item) => (
                    <ListBoxItem className="editor-option selected:underline selected:underline-offset-4" id={item.id} textValue={item.label}>
                        {item.label}
                    </ListBoxItem>
                )}
            </ListBox>
        </Popover>
    </Select>
);
const SliderField = ({ label, value, onChange }: { label: string; value: number; onChange: (value: number) => void }) => (
    <Slider className="field" maxValue={100} minValue={0} onChange={onChange} value={value}>
        <div className="flex justify-between">
            <Label>{label}</Label>
            <SliderOutput />
        </div>
        <SliderTrack className="before:bg-control-line relative h-11 w-full before:absolute before:top-1/2 before:h-0.5 before:w-full before:-translate-y-1/2">
            <SliderThumb className="bg-accent-text dragging:bg-accent top-1/2 size-4 rounded-full" />
        </SliderTrack>
    </Slider>
);
const MenuButton = <T extends Choice>({ label, options, onAction, children }: { label: string; options: readonly T[]; onAction: (choice: T) => void; children: ReactNode }) => (
    <MenuTrigger>
        <Button className="button button-outline max-w-full min-w-0 text-left wrap-anywhere whitespace-normal" isDisabled={options.length === 0}>
            {children}
        </Button>
        <Popover className="editor-popover">
            <Menu aria-label={label} className="outline-none" dependencies={[onAction]} items={options}>
                {(item) => (
                    <MenuItem className="editor-option" id={item.id} onAction={() => onAction(item)} textValue={item.label}>
                        {item.label}
                    </MenuItem>
                )}
            </Menu>
        </Popover>
    </MenuTrigger>
);
const ConfirmDialog = ({ label, description, onConfirm, disabled }: { label: string; description: string; onConfirm: () => void; disabled: boolean }) => (
    <DialogTrigger>
        <Button className="button button-ghost" isDisabled={disabled}>
            {label}
        </Button>
        <ModalOverlay className="bg-foreground/55 fixed inset-0 z-60 flex items-center justify-center p-4">
            <Modal className="bg-background max-h-full w-full max-w-120 overflow-auto">
                <Dialog className="p-7 outline-none" role="alertdialog">
                    <Heading className="text-heading tracking-display" slot="title">
                        {label}?
                    </Heading>
                    <Text className="hint mt-3.5 mb-6" elementType="p" slot="description">
                        {description}
                    </Text>
                    <div className="flex flex-wrap justify-end gap-2.5">
                        <Button autoFocus={true} className="button button-outline" slot="close">
                            Keep
                        </Button>
                        <Button className="button" onPress={onConfirm} slot="close">
                            {label}
                        </Button>
                    </div>
                </Dialog>
            </Modal>
        </ModalOverlay>
    </DialogTrigger>
);
const RequestAlert = <E extends RequestFailure>({ error, conflict, onField }: { error: E; conflict?: string; onField?: (path: readonly PropertyKey[]) => void } & ([Extract<E, { readonly _tag: 'Conflict' }>] extends [never] ? unknown : { conflict: string })) => (
    <div className="border-accent-text bg-surface border-l-3 p-4 text-sm" role="alert">
        {Match.type<RequestFailure>().pipe(
            Match.tag('SchemaError', ({ issue }) => (
                <>
                    <p>Review these fields:</p>
                    <ul>
                        {SchemaIssue.makeFormatterStandardSchemaV1()(issue).issues.map(({ path: segments, message }) => {
                            const path = segments?.map((segment) => (typeof segment === 'object' ? segment.key : segment)) ?? [];
                            const text = `${path.map(String).join(' › ')}: ${message}`;
                            return (
                                <li key={text}>
                                    {onField === undefined ? (
                                        message
                                    ) : (
                                        <button className="text-accent-text block text-left underline" onClick={() => onField(path)} type="button">
                                            {text}
                                        </button>
                                    )}
                                </li>
                            );
                        })}
                    </ul>
                </>
            )),
            Match.tag('PreconditionFailed', () => <p>A newer draft was saved in another tab. Your changes remain here. Reloading replaces these unsaved edits with that draft.</p>),
            Match.tag('Forbidden', () => <p>Sign in with the owner account to edit.</p>),
            Match.tag('Conflict', () => <p>{conflict}</p>),
            Match.tag('ServiceUnavailable', () => <p>The service is unavailable. Your changes remain here.</p>),
            Match.tag('HttpClientError', () => <p>The request could not finish. Your changes remain here. Try again.</p>),
            Match.exhaustive,
        )(error)}
    </div>
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { ConfirmDialog, Field, MenuButton, RequestAlert, SelectField, SliderField };
