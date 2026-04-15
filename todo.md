# Poängappen
### Uppstart
- [X] Skapa endpoint för spel
- [X] Skapa repository som sparar:
  - [X] spelare
  - [X] poäng
- [X] Skapa endpoint för att hämta poäng
- [X] Skapa endpoint för att uppdatera poäng
- [X] Skapa endpoint för att ta bort poäng
- [X] Skapa UI-skelett så att det går att använda API

### Kravspecifikation

#### Build & Deploy
- [ ] `npm run build` i root bygger klienten till `wwwroot`
- [ ] `dotnet run` efter build serverar hela appen

#### Språk
- [x] Sidan är på svenska som standard
- [ ] ~~(Bonus) Automatisk språkdetektering via webbläsarpreferens, svenska som fallback~~

#### Startsida `/`
- [x] Textfält för matchnamn (ex. "Fredagsbiljard")
- [x] Inställning: höga eller låga poäng är bättre
- [x] Inställning: startpoäng (standard 0)
- [x] Möjlighet att lägga till förbestämda spelare
- [x] Inställning: lås spelare när matchen är igång (visas om minst en spelare lagts till)
- [x] Knapp: Skapa match
  - [x] Genererar slumpad URL (ej ren sifferföljd, förslagsvis GUID/kort hash)
  - [x] Navigerar användaren till matchsidan

#### Matchsida `/{matchId}`
- [ ] 404-sida om matchId inte finns
- [x] Poängtabell för alla spelare
- [x] Redigerbara poängvärden
- [ ] Spelarredigering (om ej låst): namnbyte + lägg till ny spelare
- [ ] Knapp: Starta om match (återställer till originalvärden)
- [ ] Knapp: Skapa ny match utifrån denna (ny URL, samma spelare + inställningar)

#### Övrigt
- [x] Ingen inloggning krävs för att använda appen
- [ ] "Starta spel" borde vara disabled på riktigt när inte kraven är uppfyllda. T.ex. ljusgrå istället för med färg.
- [ ] Ta en funderare på hur vi ska tackla problemet med spel man räknar baklänges, t.ex. 501 i dart. Jag tänkte snett och slog ihop det med "Lägst vinner"-metoden nu vilket är fel. De borde vara separerade! **/Viktor**

---

### Skrytpoäng
- [x] Lagstöd (individuella poäng summeras per lag)
  - [x] Möjlighet att få ut vinnare baserat på lagpoäng
  - [ ] Det fungerade inte som jag trodde och gav upp, någon får ta över här om de vill. **/Viktor**
- [ ] Matchstruktur med omgångar (frames, bowling-stil osv)
- [x] "Bäst av X" / "Först till X" poäng
  - [ ] "Bäst av X" fungerar inte riktigt...
- [ ] Turneringar / säsonger
- [ ] Tillfälliga matcher med automatisk borttagning
- [ ] Anpassad statistik (ex. antal missade pilar)
- [ ] Grafvy över poängutveckling
- [ ] Lösenordsskyddad match (JWT i cookie/localstorage)
- [x] Databas för persistent lagring
