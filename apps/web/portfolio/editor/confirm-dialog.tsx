import { Backdrop } from '@rasm/ui';
import { Button, Dialog, DialogTrigger, Heading, Modal, Text } from 'react-aria-components';

// --- [COMPOSITION] ---------------------------------------------------------------------

const ConfirmDialog = ({ label, onConfirm, description = 'Source files stay in library. Published work changes on next publish', disabled = false }: { readonly label: string; readonly onConfirm: () => void; readonly description?: string; readonly disabled?: boolean }) => (
    <DialogTrigger>
        <Button className="button button-ghost" isDisabled={disabled}>
            {label}
        </Button>
        <Backdrop className="fixed inset-0 z-60 flex items-center justify-center bg-foreground/55 p-4">
            <Modal className="max-h-full w-full max-w-120 overflow-auto bg-background">
                <Dialog className="p-7 outline-none" role="alertdialog">
                    <Heading className="text-heading tracking-display" slot="title">
                        {label}?
                    </Heading>
                    <Text className="mt-3.5 mb-6 hint" elementType="p" slot="description">
                        {description}
                    </Text>
                    <div className="flex flex-wrap justify-end gap-2.5">
                        <Button autoFocus className="button button-outline" slot="close">
                            Keep
                        </Button>
                        <Button className="button" onPress={onConfirm} slot="close">
                            {label}
                        </Button>
                    </div>
                </Dialog>
            </Modal>
        </Backdrop>
    </DialogTrigger>
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { ConfirmDialog };
