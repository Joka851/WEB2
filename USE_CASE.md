# Use Case Dijagram

## Akteri

- **Neautentifikovani korisnik** - login i pregled dijeljenog plana
- **Korisnik (User)** — upravlja svojim putnim planovima
- **Admin** — upravlja svim korisnicima i planovima

## Use Cases

### Neautentifikovani korisnik
- UC01: Registracija
- UC02: Login
- UC03: Pregled dijeljenog plana (VIEW link)

### Korisnik (User)
- UC04: Kreiranje putnog plana
- UC05: Pregled svojih planova
- UC06: Izmjena putnog plana
- UC07: Brisanje putnog plana
- UC08: Dodavanje destinacije
- UC09: Izmjena destinacije
- UC10: Brisanje destinacije
- UC11: Dodavanje aktivnosti
- UC12: Pregled aktivnosti na kalendaru
- UC13: Izmjena aktivnosti
- UC14: Brisanje aktivnosti
- UC15: Dodavanje troška
- UC16: Pregled budžeta i potrošnje
- UC17: Izmjena troška
- UC18: Brisanje troška
- UC19: Upravljanje checklistom (dodavanje, označavanje, brisanje)
- UC20: Generisanje share linka (VIEW/EDIT)
- UC21: Brisanje share linka

### Admin (nasljeđuje sve UC korisnika +)
- UC22: Pregled svih korisnika
- UC23: Promjena uloge korisnika (User/Admin)
- UC24: Deaktivacija/aktivacija korisnika
- UC25: Brisanje korisnika
- UC26: Pregled svih putnih planova (svih korisnika)

## Dijagram

```
+=====================================================+
|                   TRAVEL PLANNER                   |
+=====================================================+

+------------------------+
| Neautentifikovani      |
| korisnik               |
|                        |
|  (UC01) Registracija   |
|  (UC02) Login          |
|  (UC03) Shared link    |
+------------------------+

+------------------------+
| Korisnik (User)        |
|                        |
| Putni planovi:         |
|  (UC04) Novi plan      |
|  (UC05) Moji planovi   |
|  (UC06) Edit plan      |
|  (UC07) Delete plan    |
|                        |
| Destinacije:           |
|  (UC08) Dodaj dest.    |
|  (UC09) Edit dest.     |
|  (UC10) Obrisi dest.   |
|                        |
| Aktivnosti:            |
|  (UC11) Dodaj akt.     |
|  (UC12) Kalendar       |
|  (UC13) Edit akt.      |
|  (UC14) Obrisi akt.    |
|                        |
| Troškovi:              |
|  (UC15) Dodaj trosak   |
|  (UC16) Budzet pregled |
|  (UC17) Edit trosak    |
|  (UC18) Obrisi trosak  |
|                        |
| Ostalo:                |
|  (UC19) Checklist      |
|  (UC20) Share link     |
|  (UC21) Brisi share    |
+------------------------+

+------------------------+
| Admin                  |
| (+ sve od Korisnika)   |
|                        |
|  (UC22) Svi korisnici  |
|  (UC23) Promj. ulogu   |
|  (UC24) Deak./Aktiv.   |
|  (UC25) Brisi usera    |
|  (UC26) Svi planovi    |
+------------------------+
```

## Opisi ključnih Use Cases

### UC01 — Registracija
- **Akter:** Neautentifikovani korisnik
- **Preduslovi:** Korisnik nije ulogovan
- **Tok:** Korisnik unosi ime, email, lozinku → Sistem validira → Kreira nalog
- **Postuslov:** Korisnik može da se uloguje

### UC02 — Login
- **Akter:** Neautentifikovani korisnik
- **Preduslovi:** Korisnik ima nalog
- **Tok:** Korisnik unosi email i lozinku → Sistem generiše JWT token
- **Postuslov:** Korisnik dobija pristup aplikaciji

### UC04 — Kreiranje putnog plana
- **Akter:** Korisnik
- **Preduslovi:** Korisnik je ulogovan
- **Tok:** Unosi naziv, opis, datume, budžet → Sistem kreira plan
- **Validacije:** EndDate >= StartDate, Budget >= 0

### UC15 — Dodavanje troška
- **Akter:** Korisnik
- **Preduslovi:** Postoji putni plan
- **Tok:** Unosi naziv, kategoriju, iznos, datum → Sistem bilježi trošak i ažurira preostali budžet
- **Kategorije:** Transport, Accommodation, Food, Activities, Shopping, Insurance, Other

### UC20 — Generisanje share linka
- **Akter:** Korisnik
- **Preduslovi:** Postoji putni plan
- **Tok:** Bira tip pristupa (VIEW/EDIT) i rok trajanja → Sistem generiše token i QR kod
- **Napomena:** EDIT link zahtijeva da primalac bude ulogovan

## Napomene

- Brisanje putnog plana kaskadno briše sve destinacije, aktivnosti i checklist stavke (soft delete)
- Share tokeni imaju rok trajanja nakon kojeg više nisu validni
- Admin vidi sve planove svih korisnika, User vidi samo svoje
