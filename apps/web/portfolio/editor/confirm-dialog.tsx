import { ModalOverlay } from '@rasm/ui/dialog/overlay';
import { Button, Dialog, DialogTrigger, Heading, Modal, Text } from 'react-aria-components';

// --- [COMPOSITION] ---------------------------------------------------------------------

const ConfirmDialog = ({ label, onConfirm, description = 'Source files stay in library. Published work changes on next publish', disabled = false }: { readonly label: string; readonly onConfirm: () => void; readonly description?: string; readonly disabled?: boolean }) => (
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

// --- [EXPORTS] -------------------------------------------------------------------------

export { ConfirmDialog };
