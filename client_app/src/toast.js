import toast from 'react-hot-toast';

export const showSuccess = (msg) => toast.success(msg, { duration: 3000, position: 'top-center' });
export const showError = (msg) => toast.error(msg, { duration: 4000, position: 'top-center' });
export const showLoading = (msg) => toast.loading(msg, { position: 'top-center' });
export const dismissToast = (id) => toast.dismiss(id);
