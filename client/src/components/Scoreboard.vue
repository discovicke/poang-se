<script setup lang="ts">
import type {ScoreboardRow} from '../types/game'

defineProps<{
  scoreboard: ScoreboardRow[]
  hasTeams: boolean
  startingScore: number
  winnerId: string | null
  showCrown?: boolean
}>()
</script>

<template>
  <div class="card">
    <h2>
      <slot name="title">Ställning</slot>
    </h2>
    <table v-if="scoreboard.length">
      <thead>
      <tr>
        <th>#</th>
        <th>Spelare</th>
        <th v-if="hasTeams">Lag</th>
        <th class="text-right">{{
            startingScore
              ? 'Kvar'
              : 'Total'
          }}
        </th>
      </tr>
      </thead>
      <tbody>
      <tr v-for="(s, i) in scoreboard" :key="s.playerId"
          :class="{ 'winner-row': winnerId
                        ? s.playerId === winnerId
                        : (showCrown && i === 0 && scoreboard[0].total !== 0) }">
        <td>{{ i + 1 }}</td>
        <td>
          {{ s.name }}
          <span v-if="winnerId
            ? s.playerId === winnerId
            : (showCrown && i === 0 && scoreboard[0].total !== 0)">
              {{
              winnerId
                ? '🏆'
                : '👑'
            }}
            </span>
        </td>
        <td v-if="hasTeams">{{ s.teamName ?? '–' }}</td>
        <td class="text-right">{{ s.displayTotal }}</td>
      </tr>
      </tbody>
    </table>
    <p v-else class="empty">Inga poäng ännu.</p>
  </div>
</template>

