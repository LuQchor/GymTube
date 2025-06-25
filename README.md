# GymTube

**GymTube** je moderna full-stack web aplikacija za dijeljenje, upravljanje i monetizaciju fitness video sadržaja. Omogućuje korisnicima upload, uređivanje, brisanje i pregled videa, napredne korisničke profile, premium pretplate (Stripe), Google OAuth prijavu, privatnost profila, lajk/dislajk sustav, te bogato korisničko sučelje prilagođeno desktopu i mobitelu.

---

## Tehnologije

- **Frontend:** Nuxt 3 (Vue 3), Tailwind CSS v4, DaisyUI, TypeScript, Video.js
- **Backend:** .NET 9 Web API (C#), MediatR, Stripe.NET, Mux video API, Cloudinary, SQL Server
- **Autentikacija:** JWT, Google OAuth 2.0
- **Plaćanja:** Stripe Subscriptions (mjesečno/godišnje)
- **Video streaming:** Mux (upload, transcoding, streaming, thumbnaili)
- **Slike:** Cloudinary (upload, optimizacija, CDN)
- **Baza:** SQL Server (ORM: custom repo pattern)
- **Ostalo:** Docker (za backend), REST API, CORS, Swagger

---

## Funkcionalnosti

### 1. Korisnički račun i autentikacija
- Registracija i prijava putem emaila/lozinke
- Prijava putem Google računa (OAuth 2.0)
- JWT autentikacija i autorizacija
- Promjena i postavljanje lozinke (uključujući Google korisnike)
- Uređivanje korisničkog imena, slike, opisa, spola, datuma rođenja
- Privatni profili (samo osnovni podaci vidljivi drugima)
- Upload i uklanjanje profilne slike
- **Brisanje korisničkog računa** (potvrda lozinkom, automatsko brisanje svih videa i podataka)

### 2. Video sadržaj
- Upload videa (MP4, MOV, AVI, WMV, max 2GB) – direktno na Mux
- Automatska obrada, generiranje thumbnaila, streaming
- Uređivanje naslova, opisa, premium/privatnog statusa
- Brisanje videa (potvrda lozinkom)
- Prikaz vlastitih videa (dashboard) i javnih videa (homepage)
- Privatni videi vidljivi samo vlasniku
- Premium videi dostupni samo premium korisnicima
- Statistika: broj pregleda, lajkova, dislajkova, trajanje, datum

### 3. Premium pretplate
- Stripe integracija: mjesečna i godišnja pretplata
- Automatsko upravljanje premium statusom i istekom
- Povijest plaćanja i status pretplate
- Webhookovi za sinkronizaciju statusa
- Premium badge i ekskluzivan sadržaj

### 4. Društvene funkcije
- Lajk/dislajk sustav za videe (samo prijavljeni korisnici)
- Pretraga korisnika i videa (po imenu, emailu, naslovu, opisu)
- Prikaz javnih profila drugih korisnika
- Prikaz dobi i spola na profilu

### 5. Sigurnost i privatnost
- Svi osjetljivi endpointi zaštićeni JWT-om
- Lozinke hashirane bcrypt-om (sa saltom)
- Potvrda lozinke za brisanje videa i promjenu lozinke
- **Potvrda lozinke za brisanje korisničkog računa** (osim za Google korisnike bez lozinke)
- **Sprječavanje brisanja premium korisnika** (mora prvo otkazati pretplatu)
- Google korisnici mogu naknadno postaviti lozinku
- Privatni profili i videi
- CORS zaštita, HTTPS, validacija inputa

### 6. Korisničko sučelje
- Responsive dizajn (Tailwind, DaisyUI)
- Tamni/svijetli način rada
- Notifikacije, dijalozi, potvrde, loading stanja
- Komponente: Navbar, Footer, VideoCard, VideoPlayer, EditVideoModal, UserAvatar, PremiumBadge, UiDialog
- Prilagođene forme za login, registraciju, profil, upload, pretplatu

---

## Struktura projekta

- **frontend/** – Nuxt 3 aplikacija (Vue, Tailwind, DaisyUI, Video.js)
  - `pages/` – sve glavne stranice (index, dashboard, profile, upload, login, register, subscription, korisnički profili)
  - `components/` – UI komponente (VideoCard, EditVideoModal, UserAvatar, PremiumBadge, itd.)
  - `composables/` – custom hooks za auth i user logiku
  - `utils/` – pomoćne funkcije (mapiranje, datumi, gramatika)
- **backend/** – .NET 9 Web API
  - `Controllers/` – Auth, Videos, Payments, Webhooks
  - `Domain/` – modeli: User, Video, Vote
  - `Repositories/` – pristup bazi (UserRepository, VideoRepository)
  - `Services/` – poslovna logika (JwtService, GoogleAuthService, MuxService, ImageService, PasswordHelper)
  - `Application/Commands, Queries` – MediatR handleri za CQRS pattern

---

## API Endpointi (primjeri)

- `POST /api/auth/register` – registracija
- `POST /api/auth/login` – prijava
- `POST /api/auth/google-signin` – Google OAuth prijava
- `GET /api/auth/current-user` – dohvat trenutnog korisnika
- `PUT /api/auth/update-name` – promjena korisničkog imena
- `PUT /api/auth/update-password` – promjena lozinke
- `PUT /api/auth/set-password` – postavljanje lozinke za Google korisnike
- `PUT /api/auth/update-profile` – ažuriranje profila
- `GET /api/auth/search` – pretraga korisnika
- `GET /api/auth/profile/{identifier}` – dohvat profila korisnika
- `POST /api/auth/upload-profile-image` – upload profilne slike
- `DELETE /api/auth/remove-profile-image` – uklanjanje profilne slike
- `DELETE /api/auth/delete-account` – brisanje korisničkog računa (potvrda lozinkom)
- `GET /api/videos/my-videos` – moji videi
- `POST /api/videos/initiate-upload` – iniciranje uploada (Mux)
- `PUT /api/videos/{id}` – uređivanje videa
- `PUT /api/videos/{id}/toggle-premium` – promjena premium statusa videa
- `DELETE /api/videos/{id}` – brisanje videa (potvrda lozinkom)
- `POST /api/videos/{id}/vote` – lajk/dislajk
- `GET /api/videos/{id}/votes` – dohvat glasova za video
- `GET /api/videos/user/{identifier}` – videi određenog korisnika
- `GET /api/videos/status/{assetId}` – provjera statusa videa
- `POST /api/videos/webhook` – Mux webhook
- `POST /api/payments/create-checkout-session` – Stripe checkout
- `GET /api/payments/subscription` – status pretplate
- `POST /api/payments/cancel-subscription` – otkazivanje pretplate
- `GET /api/payments/payment-history` – povijest plaćanja
- `POST /api/webhooks/stripe` – Stripe webhook

---

## Uloge korisnika

- **Obični korisnik:** može uploadati, uređivati i brisati vlastite videe, uređivati profil, lajkati/dislajkati, koristiti besplatne funkcije
- **Premium korisnik:** sve kao obični + pristup premium videima, upload premium sadržaja, premium badge
- **Google korisnik:** može koristiti sve funkcije, ali mora postaviti lozinku za brisanje videa/promjenu imena
- **Admin:** (opcionalno, može se proširiti) – dodatne privilegije

---

## Pokretanje projekta

### Backend (.NET 9)
1. Uđi u `backend/`
2. Postavi connection string u `appsettings.json`
3. Pokreni migracije i seedanje baze (ako je potrebno)
4. Pokreni API:
   ```
   dotnet run --project src/GymTube.API/GymTube.API.csproj
   ```
   API je na `http://localhost:5011`

### Frontend (Nuxt 3)
1. Uđi u `frontend/`
2. Instaliraj ovisnosti:
   ```
   npm install
   ```
3. Pokreni dev server:
   ```
   npm run dev
   ```
   Frontend je na `http://localhost:3000`

---

## Copyright

Sav kod i dizajn su vlasništvo autora.  
Za korištenje, kontaktirajte:  
**Email:** luka.malnar04@gmail.com  
**Telefon:** +385915816256

---

## Kontakt i podrška

Za pitanja, prijedloge ili suradnju, slobodno se javi na email ili LinkedIn.

---

Ako želiš, mogu dodati i slike/screenshotove, ERD dijagram baze, ili dodatne tehničke detalje (npr. deployment, CI/CD, testiranje).  
Javi ako trebaš README i na engleskom ili dodatne sekcije!

---

## VAŽNO: API ključevi i konfiguracija

Svi API ključevi i tajni podaci (Google OAuth, Stripe, Mux, itd.) **nisu uključeni** u repozitorij zbog sigurnosti.

Prije pokretanja projekta, svaki korisnik mora:
- Registrirati vlastite aplikacije na Google, Stripe, Mux itd.
- Unijeti svoje ključeve i tajne u `login.vue` (frontend) i `appsettings.json` (backend).
- Bez toga, funkcionalnosti poput prijave putem Googlea, plaćanja i video uploada NEĆE raditi.

Primjeri konfiguracije i potrebnih varijabli su opisani u kodu i komentarima.

---

## Testiranje Stripe pretplata

Za testiranje premium pretplata koristite **Stripe test mode** s testnim karticama:

### Testne kartice:
- **Visa:** `4242 4242 4242 4242`
- **Mastercard:** `5555 5555 5555 4444`
- **American Express:** `3782 822463 10005`

### Testni podaci:
- **Datum isteka:** Bilo koji budući datum (npr. `12/25`)
- **CVC:** Bilo koji 3-znamenkasti broj (npr. `123`)
- **Poštanski broj:** Bilo koji 5-znamenkasti broj (npr. `12345`)


### Simulacija grešaka:
- **Kartica odbijena:** `4000 0000 0000 0002`
- **Nedovoljna sredstva:** `4000 0000 0000 9995`
- **Neispravan CVC:** `4000 0000 0000 0127`

---

> **Napomena:** Verzija aplikacije koju deployam na GitHub koristi Docker za SQL Server bazu (lokalno), dok produkcijska verzija na internetu koristi Azure SQL Database.
