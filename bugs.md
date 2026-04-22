# Buggar
### Matchrundor
- [x] Vid "först till X" så sätts rundor till 10 rundor, det bör rimligtvis bara skapas en runda och sedan plussa på extrarundor tills vinstvillkoret är uppnått.
- [ ] När man pausar ett spel och har avancerat rundor så startar man på runda 1 igen. Poängen är kvar men staten håller inte koll på vilken runda som det pausades på.
- [ ] Vid "först till X" så borde man avancera rundor tills vinstvillkoret är uppnått från serversidan. Nu skapas enbart en runda och spelaren kan avsluta, även om man försöker spela "Först till 4 rundor" t.ex.

### Multiplayer
- [ ] När en speladmin väljer att enbart den ska kunna redigera poäng så är allt låst för övriga spelare under ett aktivt spel.
  - Rimligtvis tänker jag att övriga spelare under denna matchform fortfarande borde kunna lägga till och ta bort poäng för sin egen spelarentitet under sagd runda.
  - Spelaren ser enbart rundan spelet är på och redigerar den poängen, spelet avancerar till ny runda och då ser spelaren den nya rundan och kan redigera den poängen.
    - Ska spelaren ha möjlighet att "byta flik" i sidebar/FAB för att se poängmatrisen över hela matchen? Isåfall tänker jag att i "aktiv match"-komponenten ser den spelaren enbart en poängkomponent och knapparna för att avancera, pausa och avsluta ett spel.
    - dvs så flyttas poängmatrisen ut till en annan "flik".

### Poängräkning
- [ ] Spelet kunde ibland låsa sig och enbart ge [1] eller [3] poäng till en spelare vid spamtryck av poäng. Jag har inte stött på buggen senaste 24h och jag har inte kunnat felsöka vad som har orsakat problemet.
  - Slött system vid uppstart (läs: något som inte startats igång innan spamanrop) eller är det state som låser sig?
  - Anropen ser normala ut i webbläsarens Networkflik.
  - Jag tyckte också att serverns loggsystem såg normalt ut vid varje tillfället.

- [x] Poänggrafen ger felaktigt antal spelade rundor för en spelare och den spelaren har fått 0p någon runda.
  - Spelarens rundor med poäng sammanställs korrekt, men den hoppar över alla rundor utan poäng vilket ger intrycket att spelaren har spelat färre rundor än övriga spelare.
  - Grafen borde visa att spelarens graflinje är horisontellt rak (stagnerad poängutveckling) under de rundor som 0p är registrerade. Jag gissar att det är CumulativeValue-uträkningen i databasen och service-filen på servern som spökar till det för frontend.

- [X] När spelet är inställt på att beräkna vinnare för lagpoäng istället för spelarpoäng fungerar inte. Frontend visar spelarvinst oavsett om spelaren tillhör vinnande eller förlorande lag. Inget lag deklareras som vinnare.

- [X] Poänggrafvyn borde starta på Runda 0 (start) så att man alltid får ut en graf, även vid ett spel med enbart en runda. För tillfället får man bara en prick på y-axeln, tror det kan bli snyggare om det alltid blir en linje mellan runda 0 (start) och runda 1 (avslut). Det blir ett bättre helhetsintryck när den komponenten ser lika ut oavsett om det är en runda eller fjorton rundor.

### Styling
- [X] Checkbox och rubriken för inställningarna "Endast jag kan redigera" och "Lagvinnare" bör vara på samma nivå horisontellt. Rubrikerna bör vara fetmarkerade (eller ha en högre visuell hierarki). Texten under bör vara "gråare" och ha en lägre visuell hierarki samt vara placerade under checkbox & rubrik.
T.ex:
````
[ ] **ENDAST JAG REDIGERAR**
  Standard är att alla kan regiera sina egna poäng.
[ ]  **LAGVINNARE**
  Summera poäng per lag istället för spelare.
````

- [X] Matchlobbyns header borde vara synlig i mobilläge.

- [X] Gömma ikonen bredvid matchregler.

- [x] "Dela spel" borde vara bredvid headerns titel och inte under. Samma horisontella nivå.

- [X] Standard borde vara vald per automatik i konfigurationen, inte behöva klicka en gång för att välja den.

- [X] Claimpicker på mobilen syns inte, det borde den rimligtvis göra.

- [X] Fixa padding i botten så allt syns i mobilläge.

- [X] ClaimPicker borde alltid vara synlig för att inte förvirra innan spelare skapats, men med tydlig instruktion när den är i tom state.

- [X] Lag borde också tilldelas en färg, spelarnas färg borde korrelera med lagets färg när de är indelade i lag.

- [X] Lag och spelare ska kunna flytta runt i lobbyn. Möjlighet att välja vilken spelare som tillhör vilket lag.
  - Lagen är som "rubriker" med radslots under som man kan flytta spelare till genom att dra eller välja med dropdown.

- [X] Lag och spelares namn måste kunna redigeras efter skapande i lobbyn.
  - [ ] Lag och spelare har en redigeringsruta, men det skickas inte iväg något event när man sparar så att servern kan döpa om spelaren/laget. Jag blir galen och har gett upp /Viktor.
  - Kanske räcker med att det går att radera en spelare/ lag så kan man lägga till en ny?
- [X] Visuell hierarki för deklarering av lag och spelare, och dess kopplingar. Det är stökigt och ostyleat just nu. Hur ska vi lösa det? Står still för mig hur man gör det snyggt.

- [X] Lag och spelare ska kunna flytta runt i lobbyn.

- [ ] Om en spelare som är claimad raderas så borde signalR uppdatera och ta bort claim åt en spelare så man kan välja om, just nu så försvinner claimpicker-rutan när en spelare raderas utan möjlighet till omval.

- [X] Det är lite overflow-problem i aktiva match när det är många spelare, måste ha en overhaul hur poängmatrisen visas i mobilläge när det är många spelare. Jag vill undvika en horisontell scroll, hur ska det lösas? Ska man se runda för runda och ha "kort" liknande claimpickern eller att spelarna hamnar under varandra varje angiven runda som i en lista istället för att stå på horisontell rad?
  - [ ] Bredden är fucked fortfarande på mobilläge (och delvis desktop), minska den.
- [ ] I mobilläge går det bara att avancera rundor fram till sista rundan, sen är man fast. På sista rundan borde man kunna avsluta matchen.
- [ ] I mobilläge behöver vi en knapp för att pausa matchen.
