<script setup lang="ts">
import type {GamePlayer, ClaimInfo} from '../types/game'


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
</script>

<template>
  <!-- Claim picker dialog -->
  <div v-if="showPicker && players.length && canSwitchClaim" class="card">
    <h2>Vem är du?</h2>
    <div class="claim-buttons">
      <button
        v-for="p in players" :key="p.playerId"
        :class="isPlayerClaimed(p)
                ? 'btn-claimed'
                : 'btn-primary'"
        :disabled="isPlayerClaimed(p) && !isClaimedByMe(p)"
        @click="emit('claim', p.playerId)"
      >
        {{ p.playerName }}
        <span v-if="isPlayerClaimed(p)" class="claimed-tag">✓ tagen</span>
      </button>
      <button class="btn-secondary" @click="emit('claim', null)">👁 Åskådare</button>
    </div>
  </div>

  <!-- Nuvarance claim info bar -->
  <div v-if="claim && !showPicker" class="claim-info">
    <span v-if="claim.role === 'creator'">Du är speladmin</span>
    <span v-else-if="claim.role === 'player'">Inloggad som <strong>{{ claim.playerName }}</strong></span>
    <span v-else>👁 Åskådare</span>
    <button v-if="canSwitchClaim" class="btn-sm btn-secondary" @click="emit('unclaim')">Byt</button>
  </div>
</template>

