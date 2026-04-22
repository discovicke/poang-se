<script setup lang="ts">
import type { ScoreboardRow } from '../types/game'

defineProps<{
  scoreboard: ScoreboardRow[]
  hasTeams: boolean
  startingScore: number
  winnerId: string | null
  showCrown?: boolean
}>()
</script>

<template>
  <div class="scoreboard-container">
    <div class="header-row mb-lg">
      <h2 class="headline-md text-on-surface">
        <slot name="title">Ställning</slot>
      </h2>
    </div>

    <div v-if="scoreboard.length" class="scoreboard-kinetic">
      <!-- Leader Card (1st Place) -->
      <div
        v-if="scoreboard[0]"
        class="leader-card group leader-glow"
        :class="{ 'is-winner': winnerId === scoreboard[0].rowId }"
      >
        <div class="leader-icon-bg">
          <span class="material-symbols-outlined">emoji_events</span>
        </div>

        <div class="leader-content">
          <div class="leader-badge-row">
            <span class="badge-leader">Leader</span>
            <span class="label-sm text-secondary">1:a Placering</span>
          </div>
          <h3 class="headline-lg text-on-surface">{{ scoreboard[0].name }}</h3>
          <p v-if="scoreboard[0].memberNames?.length" class="member-names mt-sm">
            <span class="material-symbols-outlined member-icon">group</span>
            {{ scoreboard[0].memberNames.join(' · ') }}
          </p>
          <p v-else-if="hasTeams" class="label-sm text-on-surface-variant mt-sm">Lag: {{ scoreboard[0].teamName || '–' }}</p>
        </div>

        <div class="leader-score">
          <span class="display text-primary">{{ scoreboard[0].displayTotal }}</span>
          <div class="score-label label-xs text-on-surface-variant">Poäng</div>
        </div>
      </div>

      <!-- Rank 2 & 3 (Grid) -->
      <div v-if="scoreboard.length > 1" class="rank-secondary-grid mt-lg">
        <div
          v-for="(s, i) in scoreboard.slice(1, 3)"
          :key="s.rowId"
          class="rank-card"
          :class="i === 0 ? 'rank-2' : 'rank-3'"
        >
          <div class="rank-info">
            <span class="rank-number headline-sm">{{ i === 0 ? '2:a' : '3:e' }}</span>
            <h4 class="headline-sm text-on-surface mt-xs">{{ s.name }}</h4>
            <p v-if="s.memberNames?.length" class="member-names-sm">{{ s.memberNames.join(' · ') }}</p>
            <p v-else-if="hasTeams" class="label-xs text-on-surface-variant">{{ s.teamName || '–' }}</p>
          </div>
          <div class="rank-score">
            <span class="headline-lg text-on-surface-variant">{{ s.displayTotal }}</span>
            <span class="label-xs text-outline">Poäng</span>
          </div>
        </div>
      </div>

      <!-- Rank 4+ (Rows) -->
      <div v-if="scoreboard.length > 3" class="rank-list mt-lg">
        <div
          v-for="(s, i) in scoreboard.slice(3)"
          :key="s.rowId"
          class="rank-row"
        >
          <div class="row-info">
            <span class="row-number headline-sm text-outline">{{ i + 4 }}</span>
            <span class="headline-sm text-on-surface">{{ s.name }}</span>
            <span v-if="s.memberNames?.length" class="label-xs text-on-surface-variant ml-md">{{ s.memberNames.join(' · ') }}</span>
            <span v-else-if="hasTeams" class="label-xs text-on-surface-variant ml-md">({{ s.teamName || '–' }})</span>
          </div>
          <div class="row-score">
            <span class="headline-sm text-on-surface-variant">{{ s.displayTotal }}</span>
          </div>
        </div>
      </div>
    </div>

    <div v-else class="empty-state glass-card">
      <span class="material-symbols-outlined">leaderboard</span>
      <p class="label-sm italic text-on-surface-variant">Inga poäng registrerade ännu.</p>
    </div>
  </div>
</template>

<style scoped>
.scoreboard-container {
  width: 100%;
}

/* Leader Card */
.leader-card {
  position: relative;
  background-color: var(--color-surface-container-high);
  border-radius: var(--radius-2xl);
  padding: 32px;
  border-left: 4px solid var(--color-secondary);
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  overflow: hidden;
  transition: all 300ms ease-out;
}

.leader-card:hover {
  transform: translateY(-4px);
}

.leader-icon-bg {
  position: absolute;
  top: -32px;
  right: -16px;
  opacity: 0.05;
  pointer-events: none;
}

.leader-icon-bg span {
  font-size: 160px;
}

.leader-badge-row {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 8px;
}

.badge-leader {
  background-color: var(--color-secondary);
  color: var(--color-on-secondary-fixed);
  font-size: 10px;
  font-weight: 900;
  text-transform: uppercase;
  letter-spacing: 0.15em;
  padding: 2px 8px;
  border-radius: var(--radius-xs);
}

.leader-score {
  text-align: right;
  line-height: 1;
}

.score-label {
  margin-top: -8px;
}

/* Rank 2 & 3 */
.rank-secondary-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 24px;
}

@media (min-width: 640px) {
  .rank-secondary-grid {
    grid-template-columns: 1fr 1fr;
  }
}

.rank-card {
  background-color: var(--color-surface-container-low);
  border: 1px solid rgba(70, 72, 75, 0.1);
  border-radius: var(--radius-xl);
  padding: 24px;
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  transition: all 200ms;
}

.rank-card:hover {
  background-color: var(--color-surface-container-high);
}

.rank-number {
  color: var(--color-on-surface-variant);
}

.rank-2 .rank-number { color: var(--color-tertiary-fixed-dim); }
.rank-3 .rank-number { color: var(--color-outline); }

.rank-score {
  text-align: right;
  display: flex;
  flex-direction: column;
}

/* Rank Rows */
.rank-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.rank-row {
  background-color: rgba(17, 20, 23, 0.5);
  padding: 16px 24px;
  border-radius: var(--radius-xl);
  display: flex;
  justify-content: space-between;
  align-items: center;
  transition: all 200ms;
}

.rank-row:hover {
  background-color: var(--color-surface-container-high);
}

.row-info {
  display: flex;
  align-items: center;
  gap: 24px;
}

.row-number {
  width: 32px;
}

/* Empty State */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 48px;
  gap: 16px;
  opacity: 0.6;
}

.empty-state span {
  font-size: 48px;
  color: var(--color-outline);
}

/* Utils */
.text-on-surface { color: var(--color-on-surface); }
.text-on-surface-variant { color: var(--color-on-surface-variant); }
.text-primary { color: var(--color-primary); }
.text-secondary { color: var(--color-secondary); }
.text-outline { color: var(--color-outline); }
.mb-lg { margin-bottom: 24px; }
.mt-lg { margin-top: 24px; }
.mt-xs { margin-top: 4px; }
.ml-md { margin-left: 16px; }

.member-names {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  font-weight: 500;
  color: var(--color-on-surface-variant);
}

.member-icon {
  font-size: 16px;
  color: var(--color-on-surface-variant);
}

.member-names-sm {
  font-size: 11px;
  font-weight: 500;
  color: var(--color-on-surface-variant);
  margin-top: 4px;
}
</style>
