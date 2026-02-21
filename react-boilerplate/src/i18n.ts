import i18n from "i18next";
import { initReactI18next } from "react-i18next";
import HttpBackend from "i18next-http-backend";
import LanguageDetector from "i18next-browser-languagedetector";

i18n
    .use(HttpBackend)
    .use(LanguageDetector)
    .use(initReactI18next)
    .init({
        lng: "en-us",
        fallbackLng: "en-us",
        supportedLngs: ["en-us", "pt-br", "zh-cn", "es-es"],
        lowerCaseLng: true,
        load: "all",
        debug: false,
        interpolation: {
            escapeValue: false,
        },
        detection: {
            order: ["localStorage", "navigator", "htmlTag"],
            caches: ["localStorage"],
        },
        backend: {
            loadPath: "/locales/{{lng}}.json",
        },
    });

export default i18n;

export const translate = (key: string, vars?: Record<string, any>) => i18n.t(key, vars);

