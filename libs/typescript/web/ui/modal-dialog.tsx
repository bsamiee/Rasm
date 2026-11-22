import { Button, type ButtonProps, Dialog, type DialogProps, DialogTrigger, Heading, Modal, ModalOverlay, type ModalOverlayProps, Text } from 'react-aria-components';
import { Counted } from '#/open-dialogs.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const ModalDialog = Object.assign(
    ({ modal, dialog, children, ...overlay }: Omit<ModalOverlayProps, 'children'> & Pick<DialogProps, 'children'> & { readonly modal?: Omit<ModalOverlayProps, 'children'>; readonly dialog?: Omit<DialogProps, 'children'> }) => (
        <ModalOverlay {...overlay}>
            <Counted>
                <Modal {...modal}>
                    <Dialog {...dialog}>{children}</Dialog>
                </Modal>
            </Counted>
        </ModalOverlay>
    ),
    {
        Confirm: ({ label, description, cancel, onConfirm, isDisabled = false, trigger }: { readonly label: string; readonly description: string; readonly cancel: string; readonly onConfirm: () => void; readonly isDisabled?: boolean; readonly trigger?: Omit<ButtonProps, 'children' | 'isDisabled'> }) => (
            <DialogTrigger>
                <Button {...trigger} isDisabled={isDisabled}>
                    {label}
                </Button>
                <ModalDialog dialog={{ role: 'alertdialog' }}>
                    <Heading slot="title">{label}</Heading>
                    <Text elementType="p" slot="description">
                        {description}
                    </Text>
                    <div>
                        <Button autoFocus slot="close">
                            {cancel}
                        </Button>
                        <Button onPress={onConfirm} slot="close">
                            {label}
                        </Button>
                    </div>
                </ModalDialog>
            </DialogTrigger>
        ),
    },
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { ModalDialog };
