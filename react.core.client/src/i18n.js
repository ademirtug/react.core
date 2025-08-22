// src/i18n.js
import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import LanguageDetector from 'i18next-browser-languagedetector';
import { initPackageTranslations } from '@selestra11/react.login';

i18n
    .use(LanguageDetector)
    .use(initReactI18next)
    .init({
        fallbackLng: 'en',
        debug: false,
        ns: ['translation', 'login'],
        defaultNS: 'translation',
        interpolation: {
            escapeValue: false,
        },

        detection: {
            order: ['navigator'], 
            caches: [],
        },
    });

initPackageTranslations(i18n);

export default i18n;