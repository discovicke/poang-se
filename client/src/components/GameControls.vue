<script setup lang="ts">
import type {Game} from '../types/game'

defineProps<{
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
</script>

<template>
  <div class="card action-bar">
    <div class="round-indicator">
      Runda <strong>{{ game.currentRound }}</strong> av {{ game.maxRounds ?? '∞' }}
    </div>
    <button v-if="canEdit" class="btn-primary" @click="emit('advanceRound')"
            :disabled="game.maxRounds != null && game.currentRound >= game.maxRounds">
      Nästa runda →
    </button>
    <button v-if="isCreator" class="btn-warning" @click="emit('pause')">⏸ Pausa</button>
    <button v-if="isCreator" class="btn-danger" @click="emit('finish')">⏹ Avsluta</button>
    <button @click="emit('share')" class="btn-copy">📋 Länk</button>
  </div>
</template>

