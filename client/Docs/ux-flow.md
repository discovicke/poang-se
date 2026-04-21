# UX Flow
### Startsida / Skapa ny match
- Namn på match
- Tillfällig match? [ ] Checkbox
- Lösenordsskyddad match? [ ] Checkbox
- Skapa match-knapp

### Waiting / Matchlobby
- Sätta upp regler
  - Högre eller lägre poäng vinner
  - Ställa in startpoäng (0 är standard)
  - Poänginkrement (1 är standard)
  - Spelläge:
    - Standardläge (Avslutas efter X antal rundor)
    - Bäst av X (Avslutas efter att en spelare vunnit majoritet av rundor, t.ex. 2 av 3, 3 av 5, 4 av 7, osv)
    - Först till X (Avslutas efter att en spelare vunnit X rundor, eller fått X antal poäng), behöver bli en switch mellan rundor och poängmål för detta alternativ.
- Skapa spelare
  - Möjlighet att redigera spelare
  - Möjlighet att ta bort spelare
  - Möjlighet att spelare får välja sin färg (color picker eller color wheel?)
- Skapa lag
  - Möjlighet att redigera lag
  - Möjlighet att ta bort lag
  - Tilldela spelare till lag
    - Möjlighet att flytta spelare mellan lag
    - Möjlighet att slumpa alla spelare till lag
- SignalR-kopplingar har möjlighet att claima sitt namn, så jag har ansvar över min egen spelarentitet och kan redigera den och min kompis har möjlighet att göra detsamma med sin spelarentitet.
- Lås spelare när matchen startar? [ ] Checkbox
- Möjlighet för speladmin att välja om alla spelare ska ha möjlighet att redigera alla spelares poäng under pågående match eller inte. Standard är att Admin (spelskaparen) kan redigera allas poäng men vanlig användare kan bara redigera sina egna poäng varje runda
- Möjlighet att välja om man ska ha tidtagning för matchen eller inte.
- Starta match-knapp (Fortsätt match om matchen är startad och sedan pausad)

### Active / Match pågående
- Avancera runda för runda
- Man ger poäng till spelare under aktiv runda, ingen möjlighet att redigera historik om man inte är speladmin eller checkboxen för att alla kan redigera var iklickad vid spelskapandet
- Man ger poäng per inkrement eller "fritext" (alltså kan ange siffra i poängfältet istället för att bara klicka +inkrement eller -inkrement)
- Pausa match-knapp (Matchen sätts i waiting-läge)
- Avsluta match-knapp (Avslutar oavsett hur många rundor som gått)
- Avancera runda-knapp (om sista rundan så står det avsluta match)

### Finished / Match avslutad
- Poängtabell med vinnare
- Poänggrafvy med `chart.js`
- Starta ny match-knapp (ny länk med samma inställningar och spelare)
- Starta om match-knapp (samma länk med samma inställningar och spelare)
