**Target user**: En grupp kompisar (2-8 pers) som precis ska börja spela ett sällskapsspel. Glada, otåliga, och använder troligtvis mobil. Ingen vill scrolla genom en setup-wizard.

**Core insight**: Friktionen måste vara nollnoll för de som klickar länken. Skaparen tål lite mer setup, men de andra ska vara igång på 5 sekunder.

### Förslaget: 3-fas-modell + claim
#### Fas 1 - Lobby (admin)
Skaparen är admin och sätter upp:

- Spelarnamn (lägger till platser, t.ex. "Viktor", "Anna", "Spelare 3")
- Antal rundor, poängregler
- En "Starta match"-knapp som låser inställningarna

#### Fas 2 - Claim (gäster)
När någon klickar länken ser de direkt:
```
Vem är du?
[ Linus ]  [ Elton ]  [ Jesper ]  [ Jag är bara åskådare ]
```
De trycker på sig själva, klart. Slipper login och formulär. Claim sparas i localStorage så de inte kastas ut vid refresh.
#### Fas 3 - Aktiv match

- Varje spelare kan bara ändra sina egna poäng (eller om admin tillåter fri redigering)
- Admin kan editera allt och rätta misstag
- SignalR pushar live-updates till alla ("Anna lade till 20 poäng" som en liten toast)


### Konkreta förbättringar
#### Snabbval för poäng (inte ett textfält)
````
[ -5 ] [ -1 ] [ +1 ] [ +5 ] [ +10 ] [ ✏️ Eget ]
````
Stora touch-targets, mobilen är primär enhet här.
#### Admin-skyddsåtgärder

Admin kan överföra admin-rollen (ifall deras telefon dör mitt i matchen)

Eventuellt: majoritetsröstning för att öppna upp inställningar om admin försvinner

#### Lobby-länken är "living"
Innan matchen startat ser alla i lobbyn vilka som jobbat sig in (claiment en plats) live via SignalR. Skapar lite pre-game buzz.

#### Spara gamestate
GUID i URL + sessionStorage/backend-state = om sidan kraschar kan man komma tillbaka till exakt samma match.

#### Vad jag skulle pusha tillbaka på
Att alla alltid kan ändra allt är faktiskt okej för MVP men skalas dåligt när det uppstår konflikter ("vem fan ändrade mina poäng?"). Claim-systemet löser detta elegant utan att kräva konton.

Störst UX-risk: Om admin-telefonen dör och ingen kan ändra inställningarna mitt i matchen. Bygg in admin-transfer tidigt, det är en enkel feature med stor impact.

### Sammanfattning av prioriteringsordning

- Claim-flödet (klick-länk → vem är du? → klar på 3 sek)
- Touch-vänliga poängknappar (inte textinput)
- SignalR live-toasts ("X lade till Y poäng")
- Admin-transfer
- Lobby-live (se vem som jobbat sig in)
