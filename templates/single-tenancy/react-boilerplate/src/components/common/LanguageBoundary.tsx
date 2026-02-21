import React, { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';

interface Props {
  children: React.ReactNode;
}

export const LanguageBoundary: React.FC<Props> = ({ children }) => {
  const { i18n } = useTranslation();
  const [lng, setLng] = useState(i18n.language);

  useEffect(() => {
    const handler = (l: string) => setLng(l);
    i18n.on('languageChanged', handler);
    return () => {
      i18n.off('languageChanged', handler);
    };
  }, [i18n]);

  return <React.Fragment key={lng}>{children}</React.Fragment>;
};

export default LanguageBoundary;
