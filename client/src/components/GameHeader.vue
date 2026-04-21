<script setup lang="ts">
import type { Game } from '../types/game'

defineProps<{
  game: Game
  winnerName: string | null
}>()

function statusBadge(status: string) {
  return `badge-${status.toLowerCase()}`
}
</script>

<template>
  <div class="game-header-banner">
    <!-- Winner Banner -->
    <div v-if="game.status === 'Finished'" class="winner-hero glass-card leader-glow">
      <div class="hero-content">
        <div class="crown-icon">
          <span class="material-symbols-outlined text-secondary">emoji_events</span>
        </div>
        <div class="hero-text">
          <span class="label-sm text-secondary">Vinnare utsedd</span>
          <h1 class="display text-on-surface">{{ winnerName || 'Oavgjort!' }}</h1>
          <p v-if="game.teamBasedWinner" class="label-md text-on-surface-variant">Lagseger</p>
        </div>
      </div>
      <div class="hero-meta">
        <span class="label-xs text-outline">Matchen avslutades {{ new Date(game.finishedAt!).toLocaleString('sv-SE') }}</span>
      </div>
    </div>

    <!-- Active/Waiting Header (Simplified) -->
    <div v-else class="header-meta glass-card">
      <div class="meta-item">
        <span class="label-xs text-outline">Status</span>
        <div class="status-indicator">
          <span class="dot" :class="statusBadge(game.status)"></span>
          <span class="headline-sm">{{ game.status }}</span>
        </div>
      </div>

      <div class="meta-divider"></div>

      <div class="meta-item">
        <span class="label-xs text-outline">Regler</span>
        <span class="body-md font-bold">
          {{ game.lowerIsBetter ? 'Låga poäng' : 'Höga poäng' }}
        </span>
      </div>

      <div class="meta-divider"></div>

      <div v-if="game.gameMode" class="meta-item">
        <span class="label-xs text-outline">Läge</span>
        <span class="body-md font-bold text-primary">
          {{ game.gameMode === 'BestOf' ? `Bäst av ${game.gameModeValue}` : `Först till ${game.gameModeValue}` }}
        </span>
      </div>
      
      <div v-else class="meta-item">
        <span class="label-xs text-outline">Rundor</span>
        <span class="body-md font-bold">{{ game.maxRounds || '∞' }}</span>
      </div>

      <div v-if="game.isTemporary && game.expiresAt" class="meta-item temporary-warning">
        <span class="material-symbols-outlined text-error">timer</span>
        <span class="label-xs text-error">Tillfällig</span>
      </div>
    </div>
  </div>
</template>

<style scoped>
.game-header-banner {
  width: 100%;
  margin-bottom: 32px;
}

/* Winner Hero */
.winner-hero {
  padding: 48px;
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  gap: 24px;
  position: relative;
  overflow: hidden;
}

.hero-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 16px;
  z-index: 2;
}

.crown-icon span {
  font-size: 64px;
}

.hero-meta {
  margin-top: 16px;
  opacity: 0.6;
}

/* Header Meta */
.header-meta {
  display: flex;
  align-items: center;
  padding: 16px 24px;
  gap: 24px;
  flex-wrap: wrap;
}

.meta-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.status-indicator {
  display: flex;
  align-items: center;
  gap: 8px;
}

.dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
}

.badge-active { background-color: var(--color-primary); box-shadow: 0 0 10px var(--color-primary); }
.badge-waiting { background-color: var(--color-secondary); }
.badge-finished { background-color: var(--color-outline); }

.meta-divider {
  width: 1px;
  height: 32px;
  background-color: var(--color-outline-variant);
  opacity: 0.2;
}

.temporary-warning {
  margin-left: auto;
  flex-direction: row;
  align-items: center;
}

@media (max-width: 640px) {
  .header-meta {
    gap: 16px;
  }
  .meta-divider {
    display: none;
  }
}

/* Utils */
.text-on-surface { color: var(--color-on-surface); }
.text-on-surface-variant { color: var(--color-on-surface-variant); }
.text-primary { color: var(--color-primary); }
.text-secondary { color: var(--color-secondary); }
.text-error { color: var(--color-error); }
.text-outline { color: var(--color-outline); }
</style>
