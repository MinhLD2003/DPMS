import React, { createContext, useState, useContext, ReactNode } from 'react';
import ConsentModal from '../pages/Consent/modules/HasConsentOnLogin';

interface ConsentModalContextProps {
  showConsentModal: (url?: string, type?: number) => void;  // Update type to accept optional string
  hideConsentModal: () => void;
}

const ConsentModalContext = createContext<ConsentModalContextProps | undefined>(undefined);

export const ConsentModalProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [visible, setVisible] = useState(false);
  const [consentUrl, setConsentUrl] = useState<string>(''); // Initialize with empty string
  const [consentType, setConsentType] = useState<number>(1); // Add type state
  const showConsentModal = (url?: string, type?: number) => {
    if (url) {
      setConsentUrl(url); // Store the URL string
    }
    if (type) {
      setConsentType(type); // Store the URL string
    }
    setVisible(true);
  };

  const hideConsentModal = () => {
    setVisible(false);
    setConsentUrl(''); // Clear URL when hiding modal
  };

  return (
    <ConsentModalContext.Provider value={{ showConsentModal, hideConsentModal }}>
      {children}
      <ConsentModal
        visible={visible}
        onClose={hideConsentModal}
        consentUrl={consentUrl}
        type = {consentType}
      />
    </ConsentModalContext.Provider>
  );
};

export const useConsentModal = () => {
  const context = useContext(ConsentModalContext);
  if (!context) {
    throw new Error('useConsentModal must be used within ConsentModalProvider');
  }
  return context;
};