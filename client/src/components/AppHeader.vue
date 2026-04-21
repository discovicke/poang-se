<script setup lang="ts">
// TopAppBar Component
defineProps<{
  matchContext?: string;
  matchName?: string;
}>()
</script>

<template>
  <header class="app-header">
    <div class="header-left">
      <template v-if="matchName">
        <div class="match-info md-only">
          <span class="label-sm text-on-surface-variant">{{ matchContext || 'Pågående Match' }}</span>
          <h2 class="headline-sm text-primary">{{ matchName }}</h2>
        </div>
        <span class="logo mobile-only">Poängtavlan</span>
      </template>
      <template v-else>
        <span class="logo">Poängtavlan</span>
      </template>
    </div>

    <nav class="header-nav md-only" v-if="!matchName">
      <router-link to="/history" class="nav-link">Resultat</router-link>
      <router-link to="/players" class="nav-link">Spelare</router-link>
      <router-link to="/stats" class="nav-link">Statistik</router-link>
    </nav>

    <div class="header-actions">
      <button class="icon-btn" aria-label="Historik">
        <span class="material-symbols-outlined">history</span>
      </button>
      <button class="icon-btn" aria-label="Inställningar">
        <span class="material-symbols-outlined">settings</span>
      </button>
    </div>
  </header>
</template>

<style scoped>
.app-header {
  background-color: var(--color-background);
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
  padding: 16px 24px;
  position: sticky;
  top: 0;
  z-index: 50;
  height: 72px;
}

.logo {
  font-family: 'Space Grotesk', sans-serif;
  font-weight: 900;
  font-size: 24px;
  color: var(--color-primary);
  letter-spacing: -0.05em;
  text-transform: uppercase;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 16px;
}

.match-info {
  display: flex;
  flex-direction: column;
}

.header-nav {
  display: flex;
  gap: 32px;
  font-family: 'Space Grotesk', sans-serif;
  font-weight: 700;
}

.nav-link {
  color: var(--color-on-surface-variant);
  transition: all 200ms ease-out;
  padding: 4px 12px;
  border-radius: var(--radius-lg);
  text-decoration: none;
}

.nav-link:hover {
  background-color: var(--color-surface-container-high);
  color: var(--color-on-surface);
}

.header-actions {
  display: flex;
  gap: 8px;
}

.icon-btn {
  background: transparent;
  border: none;
  color: var(--color-primary);
  padding: 8px;
  border-radius: var(--radius-full);
  cursor: pointer;
  transition: all 200ms ease-out;
  display: flex;
  align-items: center;
  justify-content: center;
}

.icon-btn:hover {
  background-color: var(--color-surface-container-high);
}

.icon-btn:active {
  transform: scale(0.95);
}

.text-primary { color: var(--color-primary); }
.text-on-surface-variant { color: var(--color-on-surface-variant); }

/* Visibility Utilities */
.mobile-only { display: block; }
.md-only { display: none; }

@media (min-width: 768px) {
  .mobile-only { display: none; }
  .md-only { display: flex; }
}
</style>
