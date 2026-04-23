<script setup lang="ts">
import {ref, watch, computed, nextTick} from 'vue'
import type {Game} from '../types/game'

const vFocus = {
  mounted: (el: HTMLElement) => nextTick(() => el.focus())
}

const props = defineProps<{
  game: Game
  isCreator: boolean
}>()

const emit = defineEmits<{
  (e: 'save', settings: Record<string, unknown>): void
  (e: 'addTeam', name: string): void
  (e: 'removeTeam', teamId: string): void
  (e: 'addPlayer', name: string, teamId: string | null): void
  (e: 'assignTeam', playerId: string, teamId: string | null): void
  (e: 'removePlayer', playerId: string): void
  (e: 'renamePlayer', playerId: string, name: string): void
  (e: 'renameTeam', teamId: string, name: string): void
  (e: 'start'): void
  (e: 'resume'): void
  (e: 'share'): void
}>()

/* -- Local State for Settings -- */
const lobbyMaxRounds = ref<number | null>(props.game.maxRounds)
const lobbyIncrement = ref<number>(props.game.scoreIncrement)
const lobbyLowerIsBetter = ref(props.game.lowerIsBetter)
const lobbyCreatorOnly = ref(props.game.creatorOnly)
const lobbyTeamBasedWinner = ref(props.game.teamBasedWinner)
const lobbyGameMode = ref<string | null>(props.game.gameMode ?? '')
const lobbyGameModeValue = ref<number | null>(props.game.gameModeValue)
const lobbyGameModeTarget = ref<string>(props.game.gameModeTarget ?? 'points')
const lobbyStartingScore = ref<number>(props.game.startingScore)

watch(() => props.game, (g) => {
  lobbyMaxRounds.value = g.maxRounds
  lobbyIncrement.value = g.scoreIncrement
  lobbyLowerIsBetter.value = g.lowerIsBetter
  lobbyCreatorOnly.value = g.creatorOnly
  lobbyTeamBasedWinner.value = g.teamBasedWinner
  lobbyGameMode.value = g.gameMode ?? ''
  lobbyGameModeValue.value = g.gameModeValue
  lobbyGameModeTarget.value = g.gameModeTarget ?? 'points'
  lobbyStartingScore.value = g.startingScore
}, {deep: true})

function emitSave() {
  if (!props.isCreator) return
  emit('save', {
    // Skicka null för maxRounds när spelläge är BestOf eller FirstTo
    // (rundantalet ska växa organiskt tills vinstvillkoret uppfylls)
    maxRounds: lobbyGameMode.value
      ? null
      : lobbyMaxRounds.value,
    scoreIncrement: lobbyIncrement.value,
    lowerIsBetter: lobbyLowerIsBetter.value,
    creatorOnly: lobbyCreatorOnly.value,
    teamBasedWinner: lobbyTeamBasedWinner.value,
    gameMode: lobbyGameMode.value,
    gameModeValue: lobbyGameModeValue.value,
    gameModeTarget: lobbyGameModeTarget.value,
    startingScore: lobbyStartingScore.value
  })
}

/* -- UI State -- */
const newPlayerName = ref('')
const newTeamName = ref('')

// Inline editing
const editingPlayerId = ref<string | null>(null)
const editingPlayerName = ref('')
const editingTeamId = ref<string | null>(null)
const editingTeamName = ref('')

function startEditPlayer(id: string, name: string) {
  editingPlayerId.value = id
  editingPlayerName.value = name
}

function savePlayerName(id: string) {
  if (editingPlayerId.value !== id)
    return  // guard: blur efter Enter ska ej köra igen
  const name = editingPlayerName.value.trim()
  editingPlayerId.value = null  // nollställer INNAN emit för att blockera blur-dubbelfire och race condition
  if (name) emit('renamePlayer', id, name)
}

function startEditTeam(id: string, name: string) {
  editingTeamId.value = id
  editingTeamName.value = name
}

function saveTeamName(id: string) {
  if (editingTeamId.value !== id)
    return  // guard: blur efter Enter ska ej köra igen
  const name = editingTeamName.value.trim()
  editingTeamId.value = null
  if (name) emit('renameTeam', id, name)
}

function onTeamSelectChange(playerId: string, event: Event) {
  const val = (event.target as HTMLSelectElement).value
  emit('assignTeam', playerId, val || null)
}

function onGameModeValueChange() {
  // BestOf kräver udda tal jämna avrundas upp till närmaste udda
  if (lobbyGameMode.value === 'BestOf' && lobbyGameModeValue.value != null && lobbyGameModeValue.value % 2 === 0) {
    lobbyGameModeValue.value = lobbyGameModeValue.value + 1
  }
  emitSave()
}

function submitPlayer() {
  if (!newPlayerName.value.trim()) return
  emit('addPlayer', newPlayerName.value.trim(), null)
  newPlayerName.value = ''
}

function submitTeam() {
  if (!newTeamName.value.trim()) return
  emit('addTeam', newTeamName.value.trim())
  newTeamName.value = ''
}

function randomizeTeams() {
  if (!props.game.teams.length || !props.game.players.length) return
  const shuffled = [...props.game.players].sort(() => Math.random() - 0.5)
  shuffled.forEach((p, i) => {
    const team = props.game.teams[i % props.game.teams.length]
    emit('assignTeam', p.playerId, team.id)
  })
}

const playersInTeam = (teamId: string) => props.game.players.filter(p => p.teamId === teamId)
const unassignedPlayers = computed(() => props.game.players.filter(p => !p.teamId))

const teamColors = ['--color-secondary', '--color-primary', '--color-error', '--color-tertiary']

const getTeamColor = (teamId: string): string => {
  const index = props.game.teams.findIndex(t => t.id === teamId)
  return teamColors[index % teamColors.length]
}

const getPlayerColor = (player: { teamId?: string | null }, fallbackIndex: number): string => {
  if (player.teamId) return getTeamColor(player.teamId)
  return teamColors[fallbackIndex % teamColors.length]
}

const canStart = computed(() => props.game.players.length >= 2)
</script>

<template>
  <div class="lobby-grid">
    <!-- Section 1: Rules & Settings -->
    <section class="rules-section">
      <div class="glass-card settings-bento">
        <header class="section-header">
          <h3 class="headline-sm">Matchregler</h3>
        </header>

        <div class="settings-content">
          <!-- Win Condition -->
          <div class="setting-group">
            <label class="label-sm">Vinstvillkor</label>
            <div class="toggle-pills">
              <button
                class="pill-btn"
                :class="{ active: !lobbyLowerIsBetter }"
                @click="lobbyLowerIsBetter = false; emitSave()"
              >HÖGA POÄNG
              </button>
              <button
                class="pill-btn"
                :class="{ active: lobbyLowerIsBetter }"
                @click="lobbyLowerIsBetter = true; emitSave()"
              >LÅGA POÄNG
              </button>
            </div>
          </div>

          <!-- Base Values -->
          <div class="grid-2">
            <div class="setting-group">
              <label class="label-sm">Startpoäng</label>
              <input type="number" v-model.number="lobbyStartingScore" @change="emitSave" class="primary-input"/>
            </div>
            <div class="setting-group">
              <label class="label-sm">Poängsteg</label>
              <input type="number" v-model.number="lobbyIncrement" @change="emitSave" class="primary-input"/>
            </div>
          </div>

          <!-- Game Mode -->
          <div class="setting-group">
            <label class="label-sm">Spelläge</label>
            <select v-model="lobbyGameMode" @change="emitSave" class="primary-select">
              <option value="">Standard (X rundor)</option>
              <option value="BestOf">Bäst av X</option>
              <option value="FirstTo">Först till X</option>
            </select>
          </div>

          <!-- Rundor input för Standard läge -->
          <div v-if="!lobbyGameMode" class="setting-group animate-slide">
            <label class="label-sm">Antal rundor</label>
            <input type="number" v-model.number="lobbyMaxRounds" @change="emitSave" class="primary-input"/>
          </div>


          <div v-if="lobbyGameMode === 'FirstTo'" class="setting-group animate-slide">
            <label class="label-sm">Måltyp</label>
            <div class="toggle-pills">
              <button
                class="pill-btn"
                :class="{ active: lobbyGameModeTarget === 'points' }"
                @click="lobbyGameModeTarget = 'points'; emitSave()"
              >POÄNG
              </button>
              <button
                class="pill-btn"
                :class="{ active: lobbyGameModeTarget === 'rounds' }"
                @click="lobbyGameModeTarget = 'rounds'; emitSave()"
              >RUNDOR
              </button>
            </div>
          </div>

          <div v-if="lobbyGameMode" class="setting-group animate-slide">
            <label class="label-sm">
              {{
                lobbyGameMode === 'BestOf'
                  ? 'Antal matcher'
                  : (lobbyGameModeTarget === 'rounds'
                    ? 'Antal vinster'
                    : 'Poängmål')
              }}
            </label>
            <p v-if="lobbyGameMode === 'BestOf'" class="hint-text">Måste vara ett udda tal, t.ex. 3, 5, 7.</p>
            <input
              type="number"
              v-model.number="lobbyGameModeValue"
              :step="lobbyGameMode === 'BestOf'
              ? 2
              : 1"
              :min="lobbyGameMode === 'BestOf'
              ? 1
              : 1"
              @change="onGameModeValueChange"
              class="primary-input"
            />
            <p v-if="lobbyGameMode === 'BestOf' && lobbyGameModeValue != null && lobbyGameModeValue % 2 === 0"
               class="hint-text error-hint">
              Bäst av X kräver ett udda tal. Justerat till {{ lobbyGameModeValue + 1 }}.
            </p>
          </div>

          <!-- Admin Toggles -->
          <div class="switches-list">
            <label class="switch-item">
              <div class="switch-item-top">
                <div class="switch">
                  <input type="checkbox" v-model="lobbyCreatorOnly" @change="emitSave">
                  <span class="slider"></span>
                </div>
                <span class="switch-title">Endast jag redigerar</span>
              </div>
              <p class="switch-desc">Standard är att alla kan redigera sina egna poäng.</p>
            </label>

            <label v-if="game.teams.length > 0" class="switch-item mt-md">
              <div class="switch-item-top">
                <div class="switch">
                  <input type="checkbox" v-model="lobbyTeamBasedWinner" @change="emitSave">
                  <span class="slider"></span>
                </div>
                <span class="switch-title">Lagvinnare</span>
              </div>
              <p class="switch-desc">Summera poäng per lag istället för spelare.</p>
            </label>
          </div>
        </div>
      </div>
    </section>

    <!-- Section 2: Players & Teams -->
    <section class="players-section">
      <div class="glass-card players-bento">
        <header class="section-header">
          <h3 class="headline-sm">Spelare & Lag</h3>
          <button
            v-if="isCreator && game.teams.length > 0 && game.players.length > 0"
            @click="randomizeTeams"
            class="icon-btn-text"
          >
            <span class="material-symbols-outlined">shuffle</span> SLUMPA
          </button>
        </header>

        <div class="hierarchy-view scrollable">
          <!-- Teams as group headers -->
          <div v-for="team in game.teams" :key="team.id" class="team-group">
            <div class="team-header" :style="{ '--team-color': `var(${getTeamColor(team.id)})` }">
              <div class="team-identity">
                <span class="team-pip"></span>
                <template v-if="editingTeamId === team.id">
                  <input
                    v-model="editingTeamName"
                    v-focus
                    class="inline-input"
                    @keyup.enter="saveTeamName(team.id)"
                    @keyup.esc="editingTeamId = null"
                  />
                  <button @click="saveTeamName(team.id)" class="micro-btn save-btn" title="Spara">
                    <span class="material-symbols-outlined">check</span>
                  </button>
                  <button @click="editingTeamId = null" class="micro-btn" title="Avbryt">
                    <span class="material-symbols-outlined">close</span>
                  </button>
                </template>
                <span v-else class="team-name">{{ team.name }}</span>
                <span class="count-badge">{{ playersInTeam(team.id).length }}</span>
              </div>
              <div v-if="isCreator" class="team-actions">
                <button @click="startEditTeam(team.id, team.name)" class="micro-btn" title="Byt namn">
                  <span class="material-symbols-outlined">edit</span>
                </button>
                <button @click="emit('removeTeam', team.id)" class="micro-btn danger" title="Ta bort lag">
                  <span class="material-symbols-outlined">delete</span>
                </button>
              </div>
            </div>

            <div class="team-player-list">
              <div v-for="p in playersInTeam(team.id)" :key="p.playerId" class="player-row">
                <div class="player-row-left">
                  <div class="avatar-sm" :style="{ backgroundColor: `var(${getTeamColor(team.id)})` }">
                    {{ p.playerName.charAt(0).toUpperCase() }}
                  </div>
                  <template v-if="editingPlayerId === p.playerId">
                    <input
                      v-model="editingPlayerName"
                      v-focus
                      class="inline-input"
                      @keyup.enter="savePlayerName(p.playerId)"
                      @keyup.esc="editingPlayerId = null"
                    />
                    <button @click="savePlayerName(p.playerId)" class="micro-btn save-btn" title="Spara">
                      <span class="material-symbols-outlined">check</span>
                    </button>
                    <button @click="editingPlayerId = null" class="micro-btn" title="Avbryt">
                      <span class="material-symbols-outlined">close</span>
                    </button>
                  </template>
                  <span v-else class="player-name">{{ p.playerName }}</span>
                  <span v-if="p.claimedByConnectionId" class="online-dot" title="Online"></span>
                </div>
                <div v-if="isCreator" class="player-row-actions">
                  <select class="team-select" :value="p.teamId ?? ''" @change="onTeamSelectChange(p.playerId, $event)">
                    <option value="">Utan lag</option>
                    <option v-for="t in game.teams" :key="t.id" :value="t.id">{{ t.name }}</option>
                  </select>
                  <button v-if="editingPlayerId !== p.playerId" @click="startEditPlayer(p.playerId, p.playerName)"
                          class="micro-btn" title="Byt namn">
                    <span class="material-symbols-outlined">edit</span>
                  </button>
                  <button v-if="editingPlayerId !== p.playerId" @click="emit('removePlayer', p.playerId)"
                          class="micro-btn danger" title="Ta bort">
                    <span class="material-symbols-outlined">close</span>
                  </button>
                </div>
              </div>
              <div v-if="!playersInTeam(team.id).length" class="slot-empty">
                <span class="material-symbols-outlined">person_add</span>
                Flytta en spelare hit
              </div>
            </div>
          </div>

          <!-- Unassigned / No teams -->
          <div class="team-group">
            <div class="team-header unassigned">
              <div class="team-identity">
                <span class="team-pip neutral"></span>
                <span class="team-name">{{ game.teams.length ? 'Utan lag' : 'Spelare' }}</span>
                <span class="count-badge">{{ unassignedPlayers.length }}</span>
              </div>
            </div>
            <div class="team-player-list">
              <div v-for="(p, idx) in unassignedPlayers" :key="p.playerId" class="player-row">
                <div class="player-row-left">
                  <div class="avatar-sm" :style="{ backgroundColor: `var(${getPlayerColor(p, idx)})` }">
                    {{ p.playerName.charAt(0).toUpperCase() }}
                  </div>
                  <template v-if="editingPlayerId === p.playerId">
                    <input
                      v-model="editingPlayerName"
                      v-focus
                      class="inline-input"
                      @keyup.enter="savePlayerName(p.playerId)"
                      @keyup.esc="editingPlayerId = null"
                    />
                    <button @click="savePlayerName(p.playerId)" class="micro-btn save-btn" title="Spara">
                      <span class="material-symbols-outlined">check</span>
                    </button>
                    <button @click="editingPlayerId = null" class="micro-btn" title="Avbryt">
                      <span class="material-symbols-outlined">close</span>
                    </button>
                  </template>
                  <span v-else class="player-name">{{ p.playerName }}</span>
                  <span v-if="p.claimedByConnectionId" class="online-dot" title="Online"></span>
                </div>
                <div v-if="isCreator" class="player-row-actions">
                  <select v-if="game.teams.length" class="team-select" value=""
                          @change="onTeamSelectChange(p.playerId, $event)">
                    <option value="">Utan lag</option>
                    <option v-for="t in game.teams" :key="t.id" :value="t.id">{{ t.name }}</option>
                  </select>
                  <button v-if="editingPlayerId !== p.playerId" @click="startEditPlayer(p.playerId, p.playerName)"
                          class="micro-btn" title="Byt namn">
                    <span class="material-symbols-outlined">edit</span>
                  </button>
                  <button v-if="editingPlayerId !== p.playerId" @click="emit('removePlayer', p.playerId)"
                          class="micro-btn danger" title="Ta bort">
                    <span class="material-symbols-outlined">close</span>
                  </button>
                </div>
              </div>
              <div v-if="!unassignedPlayers.length && game.teams.length" class="slot-empty muted">
                <span class="material-symbols-outlined">check_circle</span>
                Alla spelare har lag
              </div>
              <div v-if="!game.players.length" class="slot-empty">
                <span class="material-symbols-outlined">person_add</span>
                Inga spelare ännu
              </div>
            </div>
          </div>
        </div>

        <!-- Add forms -->
        <div v-if="isCreator" class="add-forms">
          <div class="add-item-bar">
            <input v-model="newPlayerName" placeholder="Lägg till spelare..." class="primary-input"
                   @keyup.enter="submitPlayer"/>
            <button @click="submitPlayer" class="add-btn" title="Lägg till spelare">
              <span class="material-symbols-outlined">person_add</span>
            </button>
          </div>
          <div class="add-item-bar">
            <input v-model="newTeamName" placeholder="Lägg till lag..." class="primary-input"
                   @keyup.enter="submitTeam"/>
            <button @click="submitTeam" class="add-btn" title="Skapa lag">
              <span class="material-symbols-outlined">group_add</span>
            </button>
          </div>
        </div>

        <div class="lobby-footer mt-xl">
          <button
            v-if="isCreator"
            class="start-match-btn glow-primary"
            @click="game.currentRound > 1
            ? emit('resume')
            : emit('start')"
            :disabled="!canStart"
          >
            <span class="material-symbols-outlined">play_circle</span>
            {{
              game.currentRound > 1
                ? 'FORTSÄTT MATCH'
                : 'STARTA MATCH'
            }}
          </button>
        </div>
      </div>
    </section>
  </div>
</template>

<style scoped>
.lobby-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 32px;
  max-width: 1200px;
  margin: 0 auto;
  padding-bottom: 60px;
}

@media (min-width: 1024px) {
  .lobby-grid {
    grid-template-columns: 5fr 7fr;
    padding-bottom: 0;
  }
}

.glass-card {
  padding: 32px;
  height: 100%;
  display: flex;
  flex-direction: column;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 32px;
}

.settings-content {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.setting-group {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.grid-2 {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.toggle-pills {
  display: flex;
  background-color: var(--color-surface-container-high);
  padding: 4px;
  border-radius: var(--radius-lg);
}

.pill-btn {
  flex: 1;
  padding: 10px;
  border: none;
  background: transparent;
  color: var(--color-on-surface-variant);
  font-weight: 700;
  font-size: 12px;
  border-radius: var(--radius-md);
  cursor: pointer;
  transition: all 200ms;
}

.pill-btn.active {
  background-color: var(--color-primary);
  color: var(--color-on-primary-fixed);
}

.primary-input, .primary-select {
  background-color: var(--color-surface-container-high);
  border: none;
  border-radius: var(--radius-lg);
  padding: 14px 16px;
  color: var(--color-on-surface);
  font-size: 16px;
  width: 100%;
}

.switches-list {
  border-top: 1px solid var(--color-outline-variant);
  padding-top: 24px;
  margin-top: 8px;
}

.switch-item {
  display: flex;
  flex-direction: column;
  gap: 6px;
  cursor: pointer;
}

.switch-item-top {
  display: flex;
  align-items: center;
  gap: 12px;
}

.switch-title {
  font-family: 'Space Grotesk', sans-serif;
  font-weight: 700;
  font-size: 14px;
  color: var(--color-on-surface);
}

.switch-desc {
  font-size: 12px;
  color: var(--color-on-surface-variant);
  line-height: 1.4;
}

/* Tabs */
.tabs {
  display: flex;
  gap: 24px;
}

.tab-btn {
  background: transparent;
  border: none;
  font-family: 'Space Grotesk', sans-serif;
  font-weight: 700;
  font-size: 18px;
  color: var(--color-on-surface-variant);
  cursor: pointer;
  padding-bottom: 4px;
  border-bottom: 2px solid transparent;
  transition: all 200ms;
}

.tab-btn.active {
  color: var(--color-primary);
  border-bottom-color: var(--color-primary);
}

/* Items List */
.view-container {
  display: flex;
  flex-direction: column;
  height: 100%;
}

.items-list {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 12px;
  margin-bottom: 24px;
  min-height: 300px;
}

.scrollable {
  max-height: 500px;
  overflow-y: auto;
  padding-right: 8px;
}

.list-item {
  background-color: var(--color-surface-container-high);
  padding: 16px;
  border-radius: var(--radius-xl);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.item-main {
  display: flex;
  align-items: center;
  gap: 16px;
}

.team-color-dot {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  flex-shrink: 0;
}

.avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  color: var(--color-on-primary-fixed);
}

.item-actions {
  display: flex;
  align-items: center;
  gap: 12px;
}

.online-dot {
  width: 8px;
  height: 8px;
  background-color: var(--color-primary);
  border-radius: 50%;
  box-shadow: 0 0 8px var(--color-primary);
}

.remove-btn {
  background: transparent;
  border: none;
  color: var(--color-outline);
  cursor: pointer;
}

.add-item-bar {
  display: flex;
  gap: 8px;
}

.add-btn {
  background-color: var(--color-primary-container);
  color: var(--color-on-primary-container);
  border: none;
  border-radius: var(--radius-lg);
  padding: 0 16px;
  cursor: pointer;
}

/* Footer Actions */
.lobby-footer {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.start-match-btn {
  background-color: var(--color-primary);
  color: var(--color-on-primary-fixed);
  border: none;
  border-radius: var(--radius-xl);
  padding: 24px;
  height: 64px;

  font-family: 'Space Grotesk', sans-serif;
  font-weight: 900;
  font-size: 20px;
  letter-spacing: 0.1em;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
}

.start-match-btn:disabled {
  opacity: 0.3;
  cursor: not-allowed;
}

.share-btn {
  background-color: transparent;
  border: 1px solid var(--color-outline-variant);
  color: var(--color-on-surface);
  padding: 16px;
  border-radius: var(--radius-xl);
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  cursor: pointer;
}

.empty-state {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  opacity: 0.3;
}

.empty-state span {
  font-size: 48px;
}

/* Transitions */
.animate-slide {
  animation: slideDown 300ms ease-out;
}

@keyframes slideDown {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.icon-btn-text {
  background: transparent;
  border: none;
  color: var(--color-primary);
  font-weight: 700;
  font-size: 12px;
  display: flex;
  align-items: center;
  gap: 4px;
  cursor: pointer;
}

/* Hierarchy View */
.hierarchy-view {
  display: flex;
  flex-direction: column;
  gap: 8px;
  flex: 1;
  overflow-y: auto;
  max-height: 520px;
  padding-right: 4px;
  margin-bottom: 24px;
}

.team-group {
  display: flex;
  flex-direction: column;
  border-radius: var(--radius-xl);
  overflow-y: auto;
  border: 1px solid var(--color-outline-variant);
}

.team-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 14px;
  background-color: var(--color-surface-container-high);
  border-left: 3px solid var(--team-color, var(--color-outline-variant));
}

.team-header.unassigned {
  border-left-color: var(--color-outline-variant);
}

.team-identity {
  display: flex;
  align-items: center;
  gap: 10px;
}

.team-pip {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background-color: var(--team-color, var(--color-outline));
  flex-shrink: 0;
}

.team-pip.neutral {
  background-color: var(--color-outline);
}

.team-name {
  font-family: 'Space Grotesk', sans-serif;
  font-weight: 700;
  font-size: 13px;
  letter-spacing: 0.05em;
  text-transform: uppercase;
  color: var(--color-on-surface);
}

.count-badge {
  font-size: 11px;
  font-weight: 700;
  background-color: var(--color-surface-container-highest);
  color: var(--color-on-surface-variant);
  padding: 2px 8px;
  border-radius: var(--radius-full);
}

.team-actions {
  display: flex;
  gap: 4px;
}

.micro-btn {
  background: transparent;
  border: none;
  color: var(--color-on-surface-variant);
  cursor: pointer;
  padding: 4px;
  border-radius: var(--radius-sm);
  display: flex;
  align-items: center;
  transition: all 150ms;
}

.micro-btn span {
  font-size: 16px;
}

.micro-btn:hover {
  color: var(--color-primary);
  background-color: var(--color-surface-container-highest);
}

.micro-btn.danger:hover {
  color: var(--color-error);
  background-color: rgba(255, 113, 108, 0.1);
}

.team-player-list {
  display: flex;
  flex-direction: column;
  background-color: var(--color-surface-container);
}

.player-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 14px 10px 24px;
  border-top: 1px solid var(--color-outline-variant);
  gap: 8px;
}

.player-row-left {
  display: flex;
  align-items: center;
  gap: 10px;
  flex: 1;
  min-width: 0;
}

.avatar-sm {
  width: 30px;
  height: 30px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 12px;
  color: var(--color-on-primary-fixed);
  flex-shrink: 0;
}

.player-name {
  font-size: 14px;
  font-weight: 600;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.player-row-actions {
  display: flex;
  align-items: center;
  gap: 4px;
  flex-shrink: 0;
}

.team-select {
  background-color: var(--color-surface-container-high);
  border: 1px solid var(--color-outline-variant);
  border-radius: var(--radius-md);
  color: var(--color-on-surface);
  font-size: 11px;
  font-weight: 600;
  padding: 4px 8px;
  cursor: pointer;
  max-width: 100px;
}

.inline-input {
  background-color: var(--color-surface-container-highest);
  border: 1px solid var(--color-primary);
  border-radius: var(--radius-sm);
  color: var(--color-on-surface);
  font-size: 13px;
  font-weight: 700;
  font-family: 'Space Grotesk', sans-serif;
  padding: 2px 8px;
  outline: none;
  width: 120px;
}

.slot-empty {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 24px;
  font-size: 12px;
  color: var(--color-on-surface-variant);
  opacity: 0.5;
  border-top: 1px solid var(--color-outline-variant);
}

.slot-empty span {
  font-size: 16px;
}

.slot-empty.muted {
  opacity: 0.35;
}

.hint-text {
  font-size: 11px;
  color: var(--color-on-surface-variant);
}

.error-hint {
  color: var(--color-error);
}

/* Add forms */
.add-forms {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

/* Keep these from before */
.add-item-bar {
  display: flex;
  gap: 8px;
}

.add-btn {
  background-color: var(--color-surface-container-highest);
  color: var(--color-on-surface);
  border: 1px solid var(--color-outline-variant);
  border-radius: var(--radius-lg);
  padding: 0 16px;
  cursor: pointer;
  display: flex;
  align-items: center;
  transition: all 150ms;
}

.micro-btn.save-btn:hover {
  color: var(--color-primary);
  background-color: rgba(132, 173, 255, 0.1);
}

.add-btn:hover {
  background-color: var(--color-primary);
  color: var(--color-on-primary-fixed);
  border-color: var(--color-primary);
}
</style>
