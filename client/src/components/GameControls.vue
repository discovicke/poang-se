<script setup lang="ts">
import type { Game } from '../types/game'
import { computed } from 'vue'

const props = defineProps<{
  game: Game
  canEdit: boolean
  isCreator: boolean
}>()

const emit = defineEmits<{
  (e: 'advanceRound'): void
  (e: 'pause'): void
  (e: 'finish'): void
  (e: 'share'): void
}>()

const isLastRound = computed(() => {
  if (props.game.gameMode === 'BestOf' || props.game.gameMode === 'FirstTo')
    return false
  if (props.game.maxRounds === null)
    return false
  return props.game.currentRound >= props.game.maxRounds
})

const nextBtnLabel = computed(() => {
  if (isLastRound.value)
    return 'AVSLUTA MATCH'
  return 'NÄSTA RUNDA'
})

const nextBtnIcon = computed(() => {
  if (isLastRound.value)
    return 'stop'
  return 'fast_forward'
})

function handleNext() {
  if (isLastRound.value) {
    emit('finish')
  } else {
    emit('advanceRound')
  }
}
</script>

<template>
  <div class="controls-container glass-card">
    <div class="round-display">
      <span class="label-xs text-outline">Nuvarande Runda</span>
      <div class="round-value">
        <span class="headline-md text-primary">{{ game.currentRound }}</span>
        <span class="headline-sm text-outline">/ {{ game.maxRounds ?? '∞' }}</span>
      </div>
    </div>

    <div class="control-actions">
      <button
        v-if="canEdit"
        class="primary-btn"
        @click="handleNext"
      >
        <span class="material-symbols-outlined">{{ nextBtnIcon }}</span>
        {{ nextBtnLabel }}
      </button>

      <div v-if="isCreator" class="creator-actions">
        <button class="icon-btn warning" @click="emit('pause')" aria-label="Pausa">
          <span class="material-symbols-outlined">pause</span>
        </button>
        <button class="icon-btn danger" @click="emit('finish')" aria-label="Avsluta">
          <span class="material-symbols-outlined">stop</span>
        </button>
      </div>

      <button class="icon-btn" @click="emit('share')" aria-label="Dela">
        <span class="material-symbols-outlined">share</span>
      </button>
    </div>
  </div>
</template>

<style scoped>
.controls-container {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 24px;
}

.round-display {
  display: flex;
  flex-direction: column;
}

.round-value {
  display: flex;
  align-items: baseline;
  gap: 4px;
}

.control-actions {
  display: flex;
  align-items: center;
  gap: 16px;
}

.primary-btn {
  background-color: var(--color-primary);
  color: var(--color-on-primary-fixed);
  border: none;
  border-radius: var(--radius-lg);
  padding: 12px 20px;
  font-family: 'Space Grotesk', sans-serif;
  font-weight: 700;
  font-size: 14px;
  letter-spacing: 0.05em;
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  transition: all 200ms ease-out;
}

.primary-btn:hover:not(:disabled) {
  box-shadow: 0 0 20px rgba(132, 173, 255, 0.3);
}

.primary-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.creator-actions {
  display: flex;
  gap: 8px;
  padding: 0 16px;
  border-left: 1px solid var(--color-outline-variant);
  border-right: 1px solid var(--color-outline-variant);
}

.icon-btn {
  background: transparent;
  border: none;
  color: var(--color-on-surface-variant);
  width: 40px;
  height: 40px;
  border-radius: var(--radius-full);
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 200ms;
}

.icon-btn:hover {
  background-color: var(--color-surface-container-high);
  color: var(--color-on-surface);
}

.icon-btn.warning:hover {
  color: var(--color-secondary);
}

.icon-btn.danger:hover {
  color: var(--color-error);
}

@media (max-width: 640px) {
  .controls-container {
    flex-direction: column;
    gap: 20px;
    align-items: stretch;
  }

  .control-actions {
    justify-content: space-between;
  }

  .creator-actions {
    border: none;
    padding: 0;
  }
}

/* Utils */
.text-primary { color: var(--color-primary); }
.text-outline { color: var(--color-outline); }

.primary-btn:focus-visible,
.icon-btn:focus-visible {
  outline: 2px solid rgba(132, 173, 255, 0.6);
  outline-offset: 2px;
}
</style>
