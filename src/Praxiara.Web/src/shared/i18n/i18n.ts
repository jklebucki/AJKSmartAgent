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
        navigation: {
          label: 'Nawigacja główna',
          workspace: 'Obszar roboczy',
          ifsEndpoints: 'Endpointy IFS',
        },
        authentication: { login: 'Zaloguj się' },
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
          manage: 'Zarządzaj',
        },
        ifsManagement: {
          title: 'Zarządzanie endpointami IFS',
          description: 'Konfiguruj dozwolone środowiska, metodę uwierzytelniania i projekcje OData.',
          newEnvironment: 'Nowe środowisko',
          configured: 'Skonfigurowane środowiska',
          create: 'Dodaj środowisko',
          edit: 'Edycja środowiska',
          id: 'Identyfikator środowiska',
          baseUri: 'Adres bazowy IFS',
          tenant: 'Tenant',
          locale: 'Ustawienia regionalne',
          kind: 'Rodzaj środowiska',
          authentication: 'Metoda uwierzytelniania',
          projections: 'Dozwolone projekcje',
          projectionsHint: 'Wpisz po jednej nazwie projekcji w wierszu lub rozdziel je przecinkami.',
          secretFile: 'Ścieżka do pliku sekretu',
          secretFileHint: 'Pozostaw puste, aby zachować dotychczasową referencję sekretu.',
          tokenEndpoint: 'Endpoint tokenu OAuth',
          clientId: 'Client ID',
          save: 'Zapisz zmiany',
          delete: 'Usuń środowisko',
          saveFailed: 'Nie udało się zapisać konfiguracji środowiska.',
          deleteFailed: 'Nie udało się usunąć środowiska.',
          metadataTitle: 'Weryfikacja metadanych',
          metadataSelect: 'Wybierz środowisko z listy, aby sprawdzić metadane dozwolonej projekcji.',
        },
      },
    },
  },
})

export default i18n
