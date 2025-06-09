import i18n from "i18next";
import { initReactI18next } from "react-i18next";
import en from "../locales/en.json";
import vie from "../locales/vie.json";

i18n.use(initReactI18next).init({
  resources: {
    en: { translation: en },
    vie: { translation: vie }
  },
  lng: localStorage.getItem("language") || "vie", // Default language from storage
  fallbackLng: "vie",
  interpolation: { escapeValue: false }
});

export default i18n;