<script setup lang="ts">
import {ref, watch} from 'vue'
import type {Game} from '../types/game'

const props = defineProps<{
  game: Game
  isCreator: boolean
}>()

const emit = defineEmits<{
  (e: 'save', settings: Record<string, unknown>): void
  (e: 'addTeam', name: string): void
  (e: 'addPlayer', name: string, teamId: string | null): void
  (e: 'assignTeam', playerId: string, teamId: string | null): void
  (e: 'start'): void
  (e: 'share'): void
}>()

/* -- Lobby inställningar (Lokal kopia för redigering) -- */
const lobbyMaxRounds = ref<number | null>(props.game.maxRounds)
const lobbyIncrement = ref<number>(props.game.scoreIncrement)
const lobbyLowerIsBetter = ref(props.game.lowerIsBetter)
const lobbyCreatorOnly = ref(props.game.creatorOnly)
const lobbyTeamBasedWinner = ref(props.game.teamBasedWinner)
const lobbyGameMode = ref<string | null>(props.game.gameMode)
const lobbyGameModeValue = ref<number | null>(props.game.gameModeValue)

// Synkar när parent game ändras (t.ex. efter en server round-trip)
watch(() => props.game, (g) => {
  lobbyMaxRounds.value = g.maxRounds
  lobbyIncrement.value = g.scoreIncrement
  lobbyLowerIsBetter.value = g.lowerIsBetter
  lobbyCreatorOnly.value = g.creatorOnly
  lobbyTeamBasedWinner.value = g.teamBasedWinner
  lobbyGameMode.value = g.gameMode
  lobbyGameModeValue.value = g.gameModeValue
}, {deep: true})

function emitSave() {
  emit('save', {
    maxRounds: lobbyMaxRounds.value,
    scoreIncrement: lobbyIncrement.value,
    lowerIsBetter: lobbyLowerIsBetter.value,
    creatorOnly: lobbyCreatorOnly.value,
    teamBasedWinner: lobbyTeamBasedWinner.value,
    gameMode: lobbyGameMode.value,
    gameModeValue: lobbyGameModeValue.value,
  })
}

/* -- Form inputs -- */
const newTeamName = ref('')
const newPlayerName = ref('')
const selectedTeamId = ref<string | null>(null)

function submitTeam() {
  if (!newTeamName.value.trim()) return
  emit('addTeam', newTeamName.value.trim())
  newTeamName.value = ''
}

function submitPlayer() {
  if (!newPlayerName.value.trim()) return
  emit('addPlayer', newPlayerName.value.trim(), selectedTeamId.value)
  newPlayerName.value = ''
  selectedTeamId.value = null
}

function isPlayerClaimed(p: { claimedByConnectionId: string | null }): boolean {
  return p.claimedByConnectionId != null
}
</script>

<template>
  <!-- Inställningar (spelskaparen endast) -->
  <div v-if="isCreator" class="card">
    <h2>⚙ Spelinställningar</h2>
    <div class="lobby-settings">
      <div class="form-row">
        <div class="label">
          Max rundor
          <input type="number" v-model.number="lobbyMaxRounds" min="1" @change="emitSave"/>
        </div>
        <div class="label">
          Poäng per klick
          <input type="number" v-model.number="lobbyIncrement" min="0.1" step="any" @change="emitSave"/>
        </div>
      </div>
      <div class="form-row" style="margin-top: 8px;">
        <label><input type="checkbox" v-model="lobbyLowerIsBetter" @change="emitSave"/> Lägre = bättre</label>
        <label><input type="checkbox" v-model="lobbyCreatorOnly" @change="emitSave"/> Bara jag redigerar poäng</label>
        <label v-if="game.teams.length"><input type="checkbox" v-model="lobbyTeamBasedWinner" @change="emitSave"/>
          Lagvinnare</label>
      </div>
      <div class="form-row" style="margin-top: 8px;">
        <div class="label">
          Spelläge
          <select v-model="lobbyGameMode" @change="emitSave">
            <option :value="null">Standard</option>
            <option value="BestOf">Bäst av X</option>
            <option value="FirstTo">Först till X</option>
          </select>
        </div>
        <div v-if="lobbyGameMode" class="label">
          {{
            lobbyGameMode === 'BestOf'
              ? 'Antal rundor'
              : 'Poängmål'
          }}
          <input type="number" v-model.number="lobbyGameModeValue" min="1" @change="emitSave"/>
        </div>
      </div>
    </div>
  </div>

  <!-- Lag -->
  <div class="card">
    <h2>Lag</h2>
    <ul v-if="game.teams.length" class="player-list">
      <li v-for="t in game.teams" :key="t.id">{{ t.name }}</li>
    </ul>
    <p v-else class="empty">Inga lag (individuellt spel)</p>
    <form v-if="isCreator" @submit.prevent="submitTeam" class="form-row" style="margin-top: 10px;">
      <input v-model="newTeamName" placeholder="Lagnamn"/>
      <button type="submit" class="btn-primary">+ Lag</button>
    </form>
  </div>

  <!-- Spelare -->
  <div class="card">
    <h2>Spelare</h2>
    <table v-if="game.players.length">
      <thead>
      <tr>
        <th>Namn</th>
        <th>Lag</th>
        <th>Status</th>
      </tr>
      </thead>
      <tbody>
      <tr v-for="p in game.players" :key="p.playerId">
        <td>{{ p.playerName }}</td>
        <td>
          <select v-if="isCreator && game.teams.length" :value="p.teamId"
                  @change="emit('assignTeam', p.playerId, ($event.target as HTMLSelectElement).value || null)">
            <option :value="''">Inget lag</option>
            <option v-for="t in game.teams" :key="t.id" :value="t.id">{{ t.name }}</option>
          </select>
          <span v-else>{{ p.teamName ?? '–' }}</span>
        </td>
        <td>
          <span v-if="isPlayerClaimed(p)" class="badge badge-active">Ansluten</span>
          <span v-else class="badge badge-waiting">Väntar</span>
        </td>
      </tr>
      </tbody>
    </table>
    <p v-else class="empty">Inga spelare ännu.</p>

    <form @submit.prevent="submitPlayer" class="form-row" style="margin-top: 10px;">
      <input v-model="newPlayerName" placeholder="Spelarnamn"/>
      <select v-model="selectedTeamId" v-if="game.teams.length">
        <option :value="null">Inget lag</option>
        <option v-for="t in game.teams" :key="t.id" :value="t.id">{{ t.name }}</option>
      </select>
      <button type="submit" class="btn-primary" :disabled="!newPlayerName.trim()">+ Spelare</button>
    </form>
  </div>

  <!-- Start / Share -->
  <div class="card action-bar">
    <button v-if="isCreator" class="btn-success btn-lg" @click="emit('start')" :disabled="game.players.length < 2">
      ▶ Starta spel
    </button>
    <span v-if="game.players.length < 2" class="text-muted">Minst 2 spelare krävs</span>
    <button @click="emit('share')" class="btn-copy">📋 Kopiera länk</button>
  </div>
</template>

