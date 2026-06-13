# Travel Planner

Web aplikacija za planiranje putovanja razvijena kao mikroservisna arhitektura na Microsoft Service Fabric platformi.

## Tehnologije

- **Frontend:** React + TypeScript
- **Backend:** ASP.NET Core 8, Microsoft Service Fabric
- **Baza podataka:** Microsoft SQL Server
- **Autentikacija:** JWT Bearer tokens
- **ORM:** Entity Framework Core

## Arhitektura

Sistem se sastoji od 4 servisa:

| Servis | Port | Tip | Opis |
|--------|------|-----|------|
| Gateway | 8640 | Stateless | Reverse proxy (YARP), ulazna tačka |
| UserService | 8360 | Stateless | Autentikacija i upravljanje korisnicima |
| TravelService | 8700 | Stateful | Upravljanje putnim planovima |
| FinanceService | 8625 | Stateless | Upravljanje troškovima |

## Preduslovi

- Windows 10/11
- Visual Studio 2022
- .NET 8 SDK
- Node.js 18+
- Microsoft Service Fabric SDK
- SQL Server 2019+
- SQL Server Management Studio (SSMS)

## Pokretanje

### 1. Kloniranje repozitorijuma

```bash
git clone <repo-url>
cd TravelPlanner
```

### 2. Pokretanje lokalnog Service Fabric klastera

Otvori **Service Fabric Local Cluster Manager** iz System Tray i pokreni lokalni klaster (1 Node).

### 3. Build i deploy backend servisa

Otvori `TravelPlanner.sln` u Visual Studio 2022.

Rebuild Solution:
```
Build -> Rebuild Solution
```
Build -> Deploy Solution


### 4. Provjera da servisi rade

```powershell
netstat -ano | findstr "8360\|8625\|8640\|8700"
```

Svi portovi trebaju biti u statusu LISTENING.

### 5. Kreiranje admin korisnika

Registruj korisnika:

```powershell
Invoke-RestMethod -Uri "http://localhost:8360/api/auth/register" `
  -Method Post `
  -ContentType "application/json" `
  -Body '{"email":"admin@admin.com","password":"Admin123!","firstName":"Admin","lastName":"Admin"}'
```

U SSMS pokreni:

```sql
USE UserDB;
UPDATE Users SET Role = 'Admin' WHERE Email = 'admin@admin.com';
```

### 6. Pokretanje frontenda

```bash
cd frontend
npm install
npm start
```

Frontend je dostupan na: `http://localhost:3000`

### 7. Service Fabric Explorer

Dostupan na: `http://localhost:19080`

## Korisničke uloge

| Uloga | Opis |
|-------|------|
| User | Kreira i upravlja svojim putnim planovima |
| Admin | Pristup svim planovima, upravljanje korisnicima |

## API Endpointi

| Metoda | Endpoint | Opis |
|--------|----------|------|
| POST | /api/auth/register | Registracija |
| POST | /api/auth/login | Login |
| GET | /api/travel-plans | Svi planovi |
| POST | /api/travel-plans | Kreiranje plana |
| GET | /api/travel-plans/{id} | Detalji plana |
| PUT | /api/travel-plans/{id} | Izmjena plana |
| DELETE | /api/travel-plans/{id} | Brisanje plana |
| GET | /api/travel-plans/{id}/destinations | Destinacije |
| POST | /api/travel-plans/{id}/destinations | Dodaj destinaciju |
| GET | /api/travel-plans/{id}/expenses | Troškovi |
| POST | /api/travel-plans/{id}/expenses | Dodaj trošak |
| GET | /api/travel-plans/{id}/share | Share tokeni |
| POST | /api/travel-plans/{id}/share/generate | Generiši share link |
