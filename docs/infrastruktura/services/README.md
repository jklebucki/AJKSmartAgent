# Usługi zewnętrzne

Dokumenty w tym katalogu definiują sposób przygotowania, uruchomienia, weryfikacji, zabezpieczenia i utrzymania usług zewnętrznych Praxiara. Polecenia zakładają przyszły katalog `deploy/compose` opisany w [profilach Compose](../PROFILE-COMPOSE.md).

Każda instrukcja rozdziela trzy tryby:

- development/CI: wygoda, dane nietrwałe lub łatwe do odtworzenia, porty tylko na `127.0.0.1`;
- hardened single-node: trwałe wolumeny, TLS, sekrety, backup i limity, lecz bez HA hosta;
- multi-node/HA: osobne failure domains, procedura failover i restore.

Lista usług znajduje się w [głównym indeksie infrastruktury](../README.md).
