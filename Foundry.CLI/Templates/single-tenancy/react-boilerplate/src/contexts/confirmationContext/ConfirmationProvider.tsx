import { createContext, useState, useContext, type ReactNode, type FC } from 'react';
import { Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle } from '@mui/material';
import { translate } from '../../i18n';

interface ConfirmOptions {
    title: string;
    message: string;
}

type ConfirmFunction = (options: ConfirmOptions) => Promise<boolean>;
const ConfirmationContext = createContext<ConfirmFunction | undefined>(undefined);

export const ConfirmationProvider: FC<{ children: ReactNode }> = ({ children }) => {
    const [options, setOptions] = useState<ConfirmOptions | null>(null);
    const [resolve, setResolve] = useState<(value: boolean) => void>(() => () => { });

    const confirm: ConfirmFunction = (options: ConfirmOptions) => {
        return new Promise((resolve) => {
            setOptions(options);
            setResolve(() => resolve);
        });
    };

    const handleClose = () => {
        setOptions(null);
    };

    const handleConfirm = () => {
        resolve(true);
        handleClose();
    };

    const handleCancel = () => {
        resolve(false);
        handleClose();
    };

    return (
        <>
            <ConfirmationContext.Provider value={confirm}>
                {children}
            </ConfirmationContext.Provider>

            <Dialog open={options !== null} onClose={handleClose}>
                <DialogTitle>{options?.title}</DialogTitle>
                <DialogContent>
                    <DialogContentText>{options?.message}</DialogContentText>
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleCancel}>{translate("button.cancel")}</Button>
                    <Button onClick={handleConfirm} color="primary" autoFocus>
                        {translate("button.confirm")}
                    </Button>
                </DialogActions>
            </Dialog>
        </>
    );
};

// eslint-disable-next-line react-refresh/only-export-components
export const useConfirmation = () => {
    const context = useContext(ConfirmationContext);
    if (context === undefined) {
        throw new Error('useConfirmation must be used within a ConfirmationProvider');
    }
    return context;
};