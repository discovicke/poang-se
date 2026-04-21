<script setup lang="ts">
import type { GamePlayer, ClaimInfo } from '../types/game'

const props = defineProps<{
  players: GamePlayer[]
  canSwitchClaim: boolean
  claim: ClaimInfo | null
  showPicker: boolean
  connectionId: string | null
}>()

const emit = defineEmits<{
  (e: 'claim', playerId: string | null): void
  (e: 'unclaim'): void
}>()

function isPlayerClaimed(p: GamePlayer): boolean {
  return p.claimedByConnectionId != null
}

function isClaimedByMe(p: GamePlayer): boolean {
  return p.claimedByConnectionId === props.connectionId
}

const getPlayerColor = (index: number) => {
  const colors = ['--color-secondary', '--color-primary', '--color-error', '--color-tertiary']
  return colors[index % colors.length]
}
</script>

<template>
  <div class="claim-wrapper">
    <!-- Claim Picker Modal/Overlay Style -->
    <div v-if="showPicker && players.length && (canSwitchClaim || !claim)" class="glass-card picker-container">
      <div class="picker-header">
        <h2 class="headline-md">Vem är du?</h2>
        <p class="body-md text-on-surface-variant">Välj din profil för att kunna redigera poäng.</p>
      </div>

      <div class="picker-grid">
        <button
          v-for="(p, index) in players" :key="p.playerId"
          class="player-choice-card"
          :class="{ 'is-taken': isPlayerClaimed(p) && !isClaimedByMe(p), 'is-me': isClaimedByMe(p) }"
          :disabled="isPlayerClaimed(p) && !isClaimedByMe(p)"
          @click="emit('claim', p.playerId)"
        >
          <div class="avatar" :style="{ backgroundColor: `var(${getPlayerColor(index)})` }">
            {{ p.playerName.charAt(0).toUpperCase() }}
          </div>
          <span class="font-bold">{{ p.playerName }}</span>
          <span v-if="isPlayerClaimed(p)" class="status-tag">
            {{ isClaimedByMe(p) ? 'DU' : 'TAGEN' }}
          </span>
        </button>

        <button class="player-choice-card spectator" @click="emit('claim', null)">
          <div class="avatar spectator-avatar">
            <span class="material-symbols-outlined">visibility</span>
          </div>
          <span class="font-bold">Åskådare</span>
        </button>
      </div>
    </div>

    <!-- Current Identity Bar -->
    <div v-if="claim && !showPicker" class="identity-bar glass-card">
      <div class="identity-info">
        <div class="user-icon">
          <span class="material-symbols-outlined text-primary">account_circle</span>
        </div>
        <div class="text-content">
          <span v-if="claim.role === 'creator'" class="role-badge">Admin</span>
          <span v-else-if="claim.role === 'player'" class="role-badge player">Spelare</span>
          <p class="body-md">
            {{ claim.role === 'creator' ? 'Du är spelskapare' : `Inloggad som ${claim.playerName}` }}
          </p>
        </div>
      </div>
      <button v-if="canSwitchClaim" class="secondary-btn btn-sm" @click="emit('unclaim')">
        <span class="material-symbols-outlined">swap_horiz</span>
        BYT
      </button>
    </div>
  </div>
</template>

<style scoped>
.claim-wrapper {
  width: 100%;
}

/* Picker Grid */
.picker-container {
  padding: 32px;
  display: flex;
  flex-direction: column;
  gap: 32px;
}

.picker-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(140px, 1fr));
  gap: 16px;
}

.player-choice-card {
  background-color: var(--color-surface-container-high);
  border: 1px solid rgba(70, 72, 75, 0.1);
  border-radius: var(--radius-xl);
  padding: 24px 16px;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
  cursor: pointer;
  transition: all 200ms ease-out;
  position: relative;
}

.player-choice-card:hover:not(:disabled) {
  transform: translateY(-4px);
  background-color: var(--color-surface-container-highest);
  border-color: var(--color-primary);
}

.player-choice-card.is-me {
  border-color: var(--color-primary);
  background-color: rgba(132, 173, 255, 0.1);
}

.player-choice-card.is-taken {
  opacity: 0.4;
  cursor: not-allowed;
  grayscale: 1;
}

.avatar {
  width: 48px;
  height: 48px;
  border-radius: var(--radius-full);
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 900;
  font-size: 20px;
  color: var(--color-on-primary-fixed);
}

.spectator-avatar {
  background-color: var(--color-surface-container-highest);
  color: var(--color-outline);
}

.status-tag {
  font-size: 10px;
  font-weight: 900;
  padding: 2px 8px;
  background-color: var(--color-surface-container-highest);
  border-radius: var(--radius-xs);
  letter-spacing: 0.1em;
}

.is-me .status-tag {
  background-color: var(--color-primary);
  color: var(--color-on-primary-fixed);
}

/* Identity Bar */
.identity-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 20px;
}

.identity-info {
  display: flex;
  align-items: center;
  gap: 16px;
}

.user-icon span {
  font-size: 32px;
}

.text-content {
  display: flex;
  flex-direction: column;
}

.role-badge {
  font-size: 9px;
  font-weight: 900;
  text-transform: uppercase;
  letter-spacing: 0.1em;
  color: var(--color-secondary);
}

.role-badge.player {
  color: var(--color-primary);
}

.secondary-btn {
  background: transparent;
  border: 1px solid var(--color-outline-variant);
  color: var(--color-on-surface);
  border-radius: var(--radius-lg);
  padding: 6px 12px;
  display: flex;
  align-items: center;
  gap: 6px;
  cursor: pointer;
  font-weight: 700;
  font-size: 12px;
}

.secondary-btn:hover {
  background-color: var(--color-surface-container-high);
}
</style>
