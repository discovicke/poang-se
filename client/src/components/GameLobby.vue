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
  (e: 'renameTeam', teamId: string, name: string): void
  (e: 'removeTeam', teamId: string): void
  (e: 'addPlayer', name: string, teamId: string | null): void
  (e: 'assignTeam', playerId: string, teamId: string | null): void
  (e: 'renamePlayer', playerId: string, name: string): void
  (e: 'removePlayer', playerId: string): void
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
const lobbyGameModeTarget = ref<string>(props.game.gameModeTarget ?? 'points')

// Synkar när parent game ändras (t.ex. efter en server round-trip)
watch(() => props.game, (g) => {
  lobbyMaxRounds.value = g.maxRounds
  lobbyIncrement.value = g.scoreIncrement
  lobbyLowerIsBetter.value = g.lowerIsBetter
  lobbyCreatorOnly.value = g.creatorOnly
  lobbyTeamBasedWinner.value = g.teamBasedWinner
  lobbyGameMode.value = g.gameMode
  lobbyGameModeValue.value = g.gameModeValue
  lobbyGameModeTarget.value = g.gameModeTarget ?? 'points'
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
    gameModeTarget: lobbyGameModeTarget.value,
  })
}

/* -- Form inputs -- */
const newTeamName = ref('')
const newPlayerName = ref('')
const selectedTeamId = ref<string | null>(null)

// Inline-redigering
const editingTeamId = ref<string | null>(null)
const editingTeamName = ref('')
const editingPlayerId = ref<string | null>(null)
const editingPlayerName = ref('')

function startEditTeam(id: string, name: string) {
  editingTeamId.value = id
  editingTeamName.value = name
}
function confirmEditTeam(id: string) {
  if (editingTeamName.value.trim())
    emit('renameTeam', id, editingTeamName.value.trim())
  editingTeamId.value = null
}

function startEditPlayer(id: string, name: string) {
  editingPlayerId.value = id
  editingPlayerName.value = name
}
function confirmEditPlayer(id: string) {
  if (editingPlayerName.value.trim())
    emit('renamePlayer', id, editingPlayerName.value.trim())
  editingPlayerId.value = null
}

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
        <div v-if="!lobbyGameMode" class="label">
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

        <!-- Bäst av X: bara ett heltalsvärde -->
        <div v-if="lobbyGameMode === 'BestOf'" class="label">
          Antal matcher (X)
          <input type="number" v-model.number="lobbyGameModeValue" min="1" step="2" @change="emitSave"
                 placeholder="t.ex. 3"/>
          <small>Vinner {{ lobbyGameModeValue ? Math.floor(lobbyGameModeValue / 2) + 1 : '?' }} rundor</small>
        </div>

        <!-- Först till X: välj poäng eller rundor + värde -->
        <template v-if="lobbyGameMode === 'FirstTo'">
          <div class="label">
            Mål
            <div class="toggle-group">
              <button type="button"
                      :class="lobbyGameModeTarget === 'points'
                      ? 'btn-primary'
                      : 'btn-secondary'"
                      @click="lobbyGameModeTarget = 'points'; emitSave()">
                Poäng
              </button>
              <button type="button"
                      :class="lobbyGameModeTarget === 'rounds'
                      ? 'btn-primary'
                      : 'btn-secondary'"
                      @click="lobbyGameModeTarget = 'rounds'; emitSave()">
                Rundor
              </button>
            </div>
          </div>
          <div class="label">
            {{
              lobbyGameModeTarget === 'rounds'
                ? 'Vinna X rundor'
                : 'Nå X poäng'
            }}
            <input type="number" v-model.number="lobbyGameModeValue" min="1" @change="emitSave"
                   :placeholder="lobbyGameModeTarget === 'rounds'
                   ? 't.ex. 3'
                   : 't.ex. 21'"/>
          </div>
        </template>
      </div>
    </div>
  </div>

  <!-- Lag -->
  <div class="card">
    <h2>Lag</h2>
    <ul v-if="game.teams.length" class="player-list">
      <li v-for="t in game.teams" :key="t.id" class="editable-row">
        <template v-if="isCreator && editingTeamId === t.id">
          <input v-model="editingTeamName" @keyup.enter="confirmEditTeam(t.id)"
                 @keyup.escape="editingTeamId = null" autofocus class="inline-input"/>
          <button class="btn-sm btn-primary" @click="confirmEditTeam(t.id)">✓</button>
          <button class="btn-sm btn-secondary" @click="editingTeamId = null">✗</button>
        </template>
        <template v-else>
          <span>{{ t.name }}</span>
          <span v-if="isCreator" class="row-actions">
            <button class="btn-sm btn-secondary" @click="startEditTeam(t.id, t.name)">✎</button>
            <button class="btn-sm btn-danger" @click="emit('removeTeam', t.id)">🗑</button>
          </span>
        </template>
      </li>
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
        <th v-if="isCreator"></th>
      </tr>
      </thead>
      <tbody>
      <tr v-for="p in game.players" :key="p.playerId">
        <td>
          <template v-if="isCreator && editingPlayerId === p.playerId">
            <input v-model="editingPlayerName" @keyup.enter="confirmEditPlayer(p.playerId)"
                   @keyup.escape="editingPlayerId = null" autofocus class="inline-input"/>
            <button class="btn-sm btn-primary" @click="confirmEditPlayer(p.playerId)">✓</button>
            <button class="btn-sm btn-secondary" @click="editingPlayerId = null">✗</button>
          </template>
          <template v-else>
            {{ p.playerName }}
            <button v-if="isCreator" class="btn-sm btn-secondary"
                    @click="startEditPlayer(p.playerId, p.playerName)">✎</button>
          </template>
        </td>
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
        <td v-if="isCreator">
          <button class="btn-sm btn-danger" @click="emit('removePlayer', p.playerId)">🗑</button>
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

