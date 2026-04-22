<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import type { Game } from '../types/game'

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
  (e: 'start'): void
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
  lobbyGameMode.value = g.gameMode
  lobbyGameModeValue.value = g.gameModeValue
  lobbyGameModeTarget.value = g.gameModeTarget ?? 'points'
  lobbyStartingScore.value = g.startingScore
}, { deep: true })

function emitSave() {
  if (!props.isCreator) return
  emit('save', {
    maxRounds: lobbyMaxRounds.value,
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
const showTeamManager = ref(false)

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
  const shuffledPlayers = [...props.game.players].sort(() => Math.random() - 0.5)
  shuffledPlayers.forEach((p, i) => {
    const team = props.game.teams[i % props.game.teams.length]
    emit('assignTeam', p.playerId, team.id)
  })
}

const getPlayerColor = (index: number) => {
  const colors = ['--color-secondary', '--color-primary', '--color-error', '--color-tertiary']
  return colors[index % colors.length]
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
          <span class="material-symbols-outlined text-primary">tune</span>
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
              >HÖGA POÄNG</button>
              <button 
                class="pill-btn" 
                :class="{ active: lobbyLowerIsBetter }"
                @click="lobbyLowerIsBetter = true; emitSave()"
              >LÅGA POÄNG</button>
            </div>
          </div>

          <!-- Base Values -->
          <div class="grid-2">
            <div class="setting-group">
              <label class="label-sm">Startpoäng</label>
              <input type="number" v-model.number="lobbyStartingScore" @change="emitSave" class="primary-input" />
            </div>
            <div class="setting-group">
              <label class="label-sm">Poängsteg</label>
              <input type="number" v-model.number="lobbyIncrement" @change="emitSave" class="primary-input" />
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
            <input type="number" v-model.number="lobbyMaxRounds" @change="emitSave" class="primary-input" />
          </div>


          <div v-if="lobbyGameMode === 'FirstTo'" class="setting-group animate-slide">
            <label class="label-sm">Måltyp</label>
            <div class="toggle-pills">
              <button 
                class="pill-btn" 
                :class="{ active: lobbyGameModeTarget === 'points' }"
                @click="lobbyGameModeTarget = 'points'; emitSave()"
              >POÄNG</button>
              <button 
                class="pill-btn" 
                :class="{ active: lobbyGameModeTarget === 'rounds' }"
                @click="lobbyGameModeTarget = 'rounds'; emitSave()"
              >RUNDOR</button>
            </div>
          </div>

          <div v-if="lobbyGameMode" class="setting-group animate-slide">
            <label class="label-sm">
              {{ lobbyGameMode === 'BestOf' ? 'Antal matcher (X)' : (lobbyGameModeTarget === 'rounds' ? 'Antal vinster' : 'Poängmål') }}
            </label>
            <input type="number" v-model.number="lobbyGameModeValue" @change="emitSave" class="primary-input" />
          </div>

          <!-- Admin Toggles -->
          <div class="switches-list">
             <label class="switch-item">
               <div class="switch">
                 <input type="checkbox" v-model="lobbyCreatorOnly" @change="emitSave">
                 <span class="slider"></span>
               </div>
               <div class="switch-info">
                 <span class="font-bold">Endast jag redigerar</span>
                 <span class="label-xs text-on-surface-variant">Standard är att alla kan redigera sina egna</span>
               </div>
             </label>

             <label v-if="game.teams.length > 0" class="switch-item mt-md">
               <div class="switch">
                 <input type="checkbox" v-model="lobbyTeamBasedWinner" @change="emitSave">
                 <span class="slider"></span>
               </div>
               <div class="switch-info">
                 <span class="font-bold">Lagvinnare</span>
                 <span class="label-xs text-on-surface-variant">Summera poäng per lag istället för spelare</span>
               </div>
             </label>
          </div>
        </div>
      </div>
    </section>

    <!-- Section 2: Players & Teams -->
    <section class="players-section">
      <div class="glass-card players-bento">
        <header class="section-header">
          <div class="tabs">
            <button class="tab-btn" :class="{ active: !showTeamManager }" @click="showTeamManager = false">Spelare</button>
            <button class="tab-btn" :class="{ active: showTeamManager }" @click="showTeamManager = true">Lag</button>
          </div>
          <button v-if="showTeamManager && game.players.length > 0" @click="randomizeTeams" class="icon-btn-text">
            <span class="material-symbols-outlined">shuffle</span> SLUMPA
          </button>
        </header>

        <div class="bento-content">
          <!-- Spelarvyn -->
          <div v-if="!showTeamManager" class="view-container">
            <div class="items-list scrollable">
              <div v-for="(p, index) in game.players" :key="p.playerId" class="list-item player-item">
                <div class="item-main">
                  <div class="avatar" :style="{ backgroundColor: `var(${getPlayerColor(index)})` }">
                    {{ p.playerName.charAt(0).toUpperCase() }}
                  </div>
                  <div class="item-info">
                    <span class="font-bold">{{ p.playerName }}</span>
                    <span class="label-xs text-on-surface-variant">{{ p.teamName || 'Inget lag' }}</span>
                  </div>
                </div>
                <div class="item-actions">
                  <span v-if="p.claimedByConnectionId" class="online-dot" title="Online"></span>
                  <button v-if="isCreator" @click="emit('removePlayer', p.playerId)" class="remove-btn">
                    <span class="material-symbols-outlined">close</span>
                  </button>
                </div>
              </div>
              <div v-if="!game.players.length" class="empty-state">
                <span class="material-symbols-outlined">person_add</span>
                <p class="label-sm italic opacity-50">Inga spelare ännu...</p>
              </div>
            </div>
            
            <div v-if="isCreator" class="add-item-bar">
              <input v-model="newPlayerName" placeholder="Spelarnamn..." class="primary-input" @keyup.enter="submitPlayer" />
              <button @click="submitPlayer" class="add-btn"><span class="material-symbols-outlined">add</span></button>
            </div>
          </div>

          <!-- Lagvyn -->
          <div v-else class="view-container">
            <div class="items-list scrollable">
              <div v-for="t in game.teams" :key="t.id" class="list-item team-item">
                <div class="item-info">
                  <span class="font-bold">{{ t.name }}</span>
                  <span class="label-xs text-on-surface-variant">
                    {{ game.players.filter(p => p.teamId === t.id).length }} spelare
                  </span>
                </div>
                <button v-if="isCreator" @click="emit('removeTeam', t.id)" class="remove-btn">
                  <span class="material-symbols-outlined">delete</span>
                </button>
              </div>
              <div v-if="!game.teams.length" class="empty-state">
                <span class="material-symbols-outlined">groups</span>
                <p class="label-sm italic opacity-50">Skapa lag först</p>
              </div>
            </div>

            <div v-if="isCreator" class="add-item-bar">
              <input v-model="newTeamName" placeholder="Lagnamn..." class="primary-input" @keyup.enter="submitTeam" />
              <button @click="submitTeam" class="add-btn"><span class="material-symbols-outlined">group_add</span></button>
            </div>
          </div>
        </div>
        <div class="lobby-footer mt-xl">
          <button 
            v-if="isCreator" 
            class="start-match-btn glow-primary" 
            @click="emit('start')" 
            :disabled="!canStart"
          >
            <span class="material-symbols-outlined">play_circle</span>
            {{ game.currentRound > 1 ? 'FORTSÄTT MATCH' : 'STARTA MATCH' }}
          </button>
        </div>
      </div>
      
      <!-- Action Footer -->
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
}

@media (min-width: 1024px) {
  .lobby-grid {
    grid-template-columns: 5fr 7fr;
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
  align-items: center;
  gap: 16px;
  cursor: pointer;
}

.switch-info {
  display: flex;
  flex-direction: column;
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

.start-match-btn:disabled { opacity: 0.3; cursor: not-allowed; }

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

.empty-state span { font-size: 48px; }

/* Transitions */
.animate-slide {
  animation: slideDown 300ms ease-out;
}

@keyframes slideDown {
  from { opacity: 0; transform: translateY(-10px); }
  to { opacity: 1; transform: translateY(0); }
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
</style>
