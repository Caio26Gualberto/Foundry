import { useEffect, useState } from 'react';
import i18n from '../i18n';

// Call this hook inside components that still use the global `translate()`.
// It subscribes to i18n language changes and forces a re-render.
export function useI18nRerender() {
  const [, setTick] = useState(0);

  useEffect(() => {
    const handler = () => setTick((x) => x + 1);
    i18n.on('languageChanged', handler);
    return () => {
      i18n.off('languageChanged', handler);
    };
  }, []);

  return i18n.language;
}
