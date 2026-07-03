# Arhitektura sistema

## Dijagram


## Servisi

### Gateway (Stateless)
- Ulazna tačka za sve zahtjeve sa frontenda
- YARP reverse proxy rutiranje na odgovarajući mikroservis
- JWT validacija tokena

### UserService (Stateless)
- Registracija i login korisnika
- JWT generisanje
- Upravljanje korisničkim profilima i ulogama
- BCrypt heširanje lozinki

### TravelService (Stateful)
- CRUD operacije za putne planove
- Upravljanje destinacijama i aktivnostima
- Checklist funkcionalnost
- Share token generisanje

### FinanceService (Stateless)
- Evidencija troškova po kategorijama
- Praćenje budžeta
- Izvještaj o potrošnji

## Tok autentifikacije

```
1. Korisnik  -->  POST /api/auth/login  -->  Gateway
2. Gateway   -->  UserService (port 8360)
3. UserService  -->  Provjeri kredencijale  -->  Generiši JWT
4. JWT  -->  Gateway  -->  Frontend
5. Frontend  -->  Svaki zahtjev sa Bearer tokenom
6. Gateway  -->  Validira JWT  -->  Proslijedi servisu
```

## Rutiranje kroz Gateway (YARP)

| Ruta | Servis |
|------|--------|
| /api/auth/** | UserService |
| /api/users/** | UserService |
| /api/travel-plans/{id}/expenses/** | FinanceService |
| /api/travel-plans/** | TravelService |
| /api/share/** | TravelService |

## Baze podataka

Svaki servis ima svoju izoliranu bazu podataka:

- **UserDB** — korisnici, uloge, lozinke
- **TravelDB** — planovi, destinacije, aktivnosti, checklist, share tokeni
- **FinanceDB** — troškovi po kategorijama

## Sigurnost

- Lozinke heširane BCrypt algoritmom
- JWT tokeni sa potpisom i rokom trajanja
- Role-based authorization (User / Admin)
- Soft delete — podaci se ne brišu fizički iz baze
