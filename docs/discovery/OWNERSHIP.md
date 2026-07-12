# Ownership discovery Praxiary

## Status

`BLOKADA`: repozytorium nie zawiera zatwierdzonej imiennej obsady ról ani zastępstw. Poniższa macierz definiuje wymagane odpowiedzialności i uprawnienia, ale wymaga decyzji sponsora produktu.

## Macierz odpowiedzialności

| Rola | Odpowiedzialność decyzyjna | Główny owner | Zastępca | Wymagany dowód zatwierdzenia |
|---|---|---|---|---|
| Właściciel produktu | zakres, priorytety, ryzyko biznesowe, wejście do beta/GA | nieprzypisany | nieprzypisany | akceptacja zakresu i bram faz |
| Architekt platformy | granice modułów, ADR, kontrakty i topology | nieprzypisany | nieprzypisany | review ADR i zmian przekrojowych |
| Właściciel bezpieczeństwa | threat model, egress, identity, approvals, risk acceptance | nieprzypisany | nieprzypisany | akceptacja security gates i wyjątków |
| Właściciel danych/DPO | klasyfikacja, retencja, RODO i legal hold | nieprzypisany | nieprzypisany | akceptacja data inventory i retencji |
| Ekspert IFS | projection discovery, locale, customizations i test sandbox | nieprzypisany | nieprzypisany | podpis route matrix i compatibility |
| Właściciel procesu finansowego | semantyka faktur, verifier i klasy ryzyka | nieprzypisany | nieprzypisany | podpis procesów R0/R2/R4 |
| Właściciel SRE | SLO, capacity, deployment, backup, DR i on-call | nieprzypisany | nieprzypisany | akceptacja capacity i runbooków |

## Zasady zatwierdzania

- Jedna osoba może pełnić kilka ról w development, ale produkcyjne R4/R5 wymagają rozdzielenia autora, approvera i review bezpieczeństwa.
- Zastępca ma jawny zakres i nie dziedziczy uprawnień poza okresem delegacji.
- Wyjątek bezpieczeństwa, licencji lub retencji ma ownera, uzasadnienie, termin wygaśnięcia i dowód ponownego przeglądu.
- Imienne dane należy utrzymywać w zatwierdzonym narzędziu organizacji; ten dokument przechowuje role i decyzje, nie prywatne dane kontaktowe.

## Warunek zamknięcia `DISC-001`

Sponsor produktu musi przypisać głównego ownera i zastępcę do każdej roli oraz zatwierdzić macierz. Do tego czasu `DISC-001` i zależne biznesowe akceptacje pozostają otwarte.
