import i18n from 'i18next'
import { initReactI18next } from 'react-i18next'

void i18n.use(initReactI18next).init({
  lng: 'pl',
  fallbackLng: 'pl',
  interpolation: { escapeValue: false },
  resources: {
    pl: {
      translation: {
        product: { category: 'Kontrolowana automatyzacja przeglądarki' },
        environment: {
          label: 'Stan środowiska',
          notConnected: 'Brak połączenia',
        },
        workspace: { label: 'Obszar roboczy agenta' },
        chat: {
          title: 'Zadanie',
          placeholder: 'Opisz, co mam wykonać…',
          submit: 'Utwórz zadanie',
          hint: 'Agent najpierw przedstawi plan i poprosi o zgodę przed operacją skutkową.',
        },
        browser: {
          title: 'Przeglądarka',
          emptyTitle: 'Sesja nie została uruchomiona',
          emptyBody: 'Po utworzeniu zadania zobaczysz tutaj izolowaną sesję Chromium.',
          takeover: 'Przejmij sterowanie',
        },
        timeline: {
          title: 'Przebieg',
          created: 'Oczekiwanie na nowe zadanie',
        },
        approval: {
          title: 'Zatwierdzenie',
          empty: 'Brak operacji oczekujących na decyzję.',
        },
        ifsEnvironments: {
          title: 'Środowiska IFS',
          loading: 'Pobieranie dozwolonych środowisk IFS…',
          unavailable: 'Środowiska IFS są niedostępne lub wymagają roli operatorskiej.',
          empty: 'Nie skonfigurowano jeszcze środowiska IFS.',
          environment: 'Środowisko',
          projection: 'Projekcja',
          metadata: 'Sprawdź metadane',
          metadataReady: 'Metadane projekcji zostały zweryfikowane.',
          metadataFailed: 'Nie udało się pobrać metadanych projekcji.',
        },
      },
    },
  },
})

export default i18n
