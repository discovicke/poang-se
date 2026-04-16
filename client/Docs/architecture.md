# Klientarkitektur

### Hur Vue-appen är uppdelad

## Översikt

Applikationen är uppdelad i fyra lager som var och en har ett tydligt ansvar.
Tänk på det ungefär som separationen i ett C#-projekt:

| Vue-lager      | C#-motsvarighet                               |
|----------------|-----------------------------------------------|
| `types/`       | DTO-klasser / interfaces                      |
| `composables/` | Services (affärslogik & extern kommunikation) |
| `components/`  | Återanvändbara UI-kontroller                  |
| `pages/`       | Controllers / Views – knyter ihop allt        |

```
src/
├── types/          <- Interfaces (datatyper)
├── composables/    <- Services (logik, API, SignalR)
├── components/     <- Återanvändbara UI-delar
└── pages/          <- Sidor (orchestrators)
```

---

## 1. `types/` Datatyperna

**Ansvar:** Definiera hur data ser ut. Inget annat.

Tänk på dessa som C# DTO-klasser, de håller bara struktur, ingen logik.

```ts
// types/game.ts
export interface Game {
  id: string
  name: string
  status: string        // "Waiting" | "Active" | "Finished"
  startingScore: number
  players: GamePlayer[]
  scores: ScoreEntry[]
  // ...
}

export interface ScoreEntry {
  id: string
  playerId: string
  round: number | null
  value: number
  cumulativeValue: number | null
}
```

> **Varför en egen fil?**
> Alla andra lager importerar samma typer. Om man ändrar en typ ser man direkt var den används tack vare TypeScript,
> precis som `ctrl+klick` på en C#-klass.

---

## 2. `composables/` Logiken (Services)

En **composable** är en vanlig TypeScript-funktion med prefixet `use` som kapslar in reaktiv logik med Vue's Composition
API (`ref`, `computed` m.m.).

> **C#-analogi:** En composable är som en `service`-klass med constructor-injection, fast i funktionsform. Man
> instansierar den i en komponent och får tillbaka metoder och reaktiva värden.

Vi har tre composables:

---

### `useGameApi` REST-kommunikation

**Ansvar:** Prata med backend-API:t. Inget annat.

Varje metod gör ett HTTP-anrop och returnerar ett uppdaterat `Game`-objekt.

```ts
const api = useGameApi(gameId)

// Hämta spelet
const game = await api.fetchGame()

// Uppdatera en poäng (PUT)
const updatedGame = await api.updateScore(playerId, round, newValue)

// Starta spelet (PUT)
const updatedGame = await api.startGame()
```

Internt bygger den bara upp `fetch()`-anrop mot `/api/games/{id}/...`.
Den vet ingenting om UI, SignalR eller Vue-reaktivitet (förutom `loading`-flaggan).

---

### `useSignalR` Realtidskommunikation

**Ansvar:** Hantera SignalR-anslutningen och spelarclaims.

```ts
const hub = useSignalR(gameId)

// Anslut och lyssna på uppdateringar
await hub.connect(refreshCallback, playerId)

// Claima en spelare (byter anslutning)
await hub.claimPlayer(playerId, refreshCallback)

// Returnerar reaktiva värden:
hub.claim.value        // Vem är du i spelet?
hub.showClaimPicker.value  // Ska claim-dialogen visas?
hub.connection.value   // Selva SignalR-objektet
```

Servern skickar `GameUpdated` eller `ScoreAdded` -> composablen kallar `refreshCallback` -> `GamePage` hämtar ny data
från API:t.

---

### `useGameState` Beräknad state

**Ansvar:** Beräkna UI-data ut ur ett `Game`-objekt. Inget HTTP, inget SignalR.

Tar emot en reaktiv `game`-ref och returnerar `computed`-värden som automatiskt räknas om när `game` ändras. Ungefär som
read-only properties i C# som beräknas ur andra fält, fast att Vue håller koll på beroenden och räknar om automatiskt.

```csharp
// C#: manuell beräkning, inget händer automatiskt
public class Game {
    public List<Score> Scores { get; set; }
    public int TotalScore => Scores.Sum(s => s.Value); // räknas varje gång du läser
}
```

```ts
// Vue computed: räknas om automatiskt när game.value ändras
const totalScore = computed(() =>
  game.value?.scores.reduce((sum, s) => sum + s.value, 0) ?? 0
)
```

Skillnaden är att i Vue behöver man inte anropa något, Vue vet när `game` ändras och uppdaterar UI:t av sig självt.

```ts
const state = useGameState(game, gameId)

state.scoreboard.value      // Sorterad ställningslista
state.scoreMatrix.value     // { [playerId]: { [round]: ScoreEntry } }
state.rounds.value          // [1, 2, 3, ..., maxRounds]
state.winnerName.value      // "Alice" eller null
state.isCreator.value       // Är jag skaparen av spelet?
state.playerDisplayTotal(id) // 501 - råpoäng (för dart-läge)
```

---

## 3. `components/` UI-delarna

Återanvändbara Vue-komponenter. Var och en ritar en specifik del av sidan.

**Att tänka på:** En komponent tar emot data via **props** och kommunicerar uppåt via **events** (`emit`). Den muterar
aldrig data direkt, det är `pages/` jobb.

> De tar emot en modell, ritar HTML, och anropar inte databasen själva. Komponenterna är interaktiva och kan skicka
> tillbaka events när användaren gör något.

| Komponent          | Visar                                                           |
|--------------------|-----------------------------------------------------------------|
| `GameHeader.vue`   | Spelets namn, metadata, vinnarbanner                            |
| `ClaimPicker.vue`  | "Vem är du?"-dialog + inloggad-som-bar                          |
| `GameLobby.vue`    | Inställningar, lag, spelare, start-knapp (Waiting)              |
| `GameControls.vue` | Nästa runda, pausa, avsluta (Active)                            |
| `Scoreboard.vue`   | Ställningstabell, återanvänds i Active och Finished             |
| `ScoreMatrix.vue`  | Poängmatris med +/− knappar – återanvänds i Active och Finished |

**Exempel på hur en komponent kommunicerar:**

```vue
<!-- GameControls.vue tar emot props och emittar events uppåt -->
<script setup lang="ts">
  defineProps<{ game: Game; canEdit: boolean; isCreator: boolean }>()
  const emit = defineEmits<{
    (e: 'advanceRound'): void
    (e: 'pause'): void
    (e: 'finish'): void
  }>()
</script>

<template>
  <button @click="emit('advanceRound')">Nästa runda →</button>
  <button v-if="isCreator" @click="emit('pause')">⏸ Pausa</button>
</template>
```

---

## 4. `pages/` Sidorna (Orchestrators)

En **page** är en Vue-komponent som svarar mot en URL-route.
Den kopplar ihop composables + komponenter men innehåller i princip ingen egen logik.

> **C#-analogin:** Tänk på sidan som en Controller, den vet inte hur UI:t ritas och den vet inte hur API:t fungerar,
> men den sätter ihop rätt saker på rätt plats.

Vi har två sidor:

- **`HomePage.vue`**: Lista spel + skapa nytt spel
- **`GamePage.vue`**: Hela matchvyn (lobby -> aktiv -> klar)

**`GamePage.vue` i korthet:**

```ts
// 1. Instansiera composables
const api = useGameApi(props.id)   // HTTP
const hub = useSignalR(props.id)   // SignalR
const state = useGameState(game, props.id)  // Computed

// 2. Skapa en refresh-funktion som SignalR kan kalla
async function refresh() {
  const g = await api.fetchGame()
  if (g) game.value = g
}

// 3. Hantera events från komponenter
async function onAdjustScore(playerId, round, delta) {
  const existing = state.scoreMatrix.value[playerId]?.[round]
  if (existing)
    await api.updateScore(playerId, round, existing.value + delta)
  else
    await api.addScore(playerId, teamId, round, delta)
}
```

```vue
<!-- Mallen delegerar allt till komponenter -->
<template>
  <GameHeader :game="game"/>
  <ClaimPicker :claim="hub.claim.value" @claim="onClaim"/>
  <GameLobby v-if="game.status === 'Waiting'" @start="onStart"/>
  <GameControls v-if="game.status === 'Active'" @advance-round="onAdvanceRound"/>
  <Scoreboard :scoreboard="state.scoreboard.value"/>
  <ScoreMatrix :score-matrix="state.scoreMatrix.value" @adjust="onAdjustScore"/>
</template>
```

---

## Flödet, från klick till UI

```
Användaren klickar "+"-knapp i ScoreMatrix
        │
        v
ScoreMatrix emittar 'adjust' (playerId, round, +1)
        │
        v
GamePage.onAdjustScore() tar emot eventet
        │
        v
useGameApi.updateScore() -> PUT /api/games/{id}/scores
        │
        v
Backend svarar -> fetchGame() -> game.value uppdateras
        │
        v (parallellt via SignalR)
Server pushar 'ScoreAdded' -> useSignalR kallar refresh()
        │
        v
useGameState räknar om scoreboard, scoreMatrix (computed)
        │
        v
Vue ritar om bara de delar av UI:t som ändrades
```

---

## Sammanfattning

- **`types/`** –> Vet hur data *ser ut*
- **`composables/`** –> Vet hur man *pratar med omvärlden* och *beräknar state*
- **`components/`** –> Vet hur man *ritar* en del av UI:t
- **`pages/`** –> Vet hur man *kopplar ihop* allt

