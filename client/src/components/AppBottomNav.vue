<template>
  <nav class="app-bottom-nav">
    <router-link to="/history" class="nav-item">
      <span class="material-symbols-outlined">leaderboard</span>
      <span class="label">Match</span>
    </router-link>
    <router-link to="/players" class="nav-item">
      <span class="material-symbols-outlined">groups</span>
      <span class="label">Spelare</span>
    </router-link>
    <router-link to="/stats" class="nav-item">
      <span class="material-symbols-outlined">insights</span>
      <span class="label">Stats</span>
    </router-link>
    <router-link to="/settings" class="nav-item">
      <span class="material-symbols-outlined">tune</span>
      <span class="label">Mer</span>
    </router-link>
  </nav>
</template>

<style scoped>
.app-bottom-nav {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  height: 64px;
  background-color: var(--color-surface-container-low);
  border-top: 1px solid var(--color-outline-variant);
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  z-index: 90;
  padding: 0 8px;
}

.nav-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 4px;
  color: var(--color-on-surface-variant);
  text-decoration: none;
  transition: all 200ms ease-out;
}

.nav-item.router-link-active {
  color: var(--color-primary);
}

.nav-item .material-symbols-outlined {
  font-size: 24px;
}

.nav-item.router-link-active .material-symbols-outlined {
  font-variation-settings: 'FILL' 1;
}

.label {
  font-family: 'Manrope', sans-serif;
  font-size: 10px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.1em;
}

@media (min-width: 768px) {
  .app-bottom-nav {
    display: none;
  }
}
</style>
