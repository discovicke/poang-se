<script setup lang="ts">
</script>
<template>
  <aside class="app-sidebar">
    <div class="sidebar-header">
      <router-link to="/" class="logo">Poäng.se</router-link>
      <p class="version">Poängräkning för de viktigaste tillfällena</p>
    </div>

    <nav class="sidebar-nav">
      <router-link to="/history" class="nav-item">
        <span class="material-symbols-outlined">leaderboard</span>
        Resultat
      </router-link>
      <router-link to="/players" class="nav-item">
        <span class="material-symbols-outlined">groups</span>
        Spelare
      </router-link>
      <router-link to="/stats" class="nav-item">
        <span class="material-symbols-outlined">insights</span>
        Statistik
      </router-link>
      <router-link to="/settings" class="nav-item">
        <span class="material-symbols-outlined">tune</span>
        Inställningar
      </router-link>
    </nav>

    <div class="sidebar-footer">
      <button class="primary-btn" @click="$router.push('/new')">
        <span class="material-symbols-outlined">add_circle</span>
        Ny Match
      </button>
    </div>
  </aside>
</template>

<style scoped>
.app-sidebar {
  display: flex;
  flex-direction: column;
  height: 100vh;
  padding: 16px;
  background-color: var(--color-background);
  border-right: 1px solid var(--color-outline-variant);
  width: 256px;
  position: sticky;
  top: 0;
  transition: all 300ms ease-in-out;
}

.sidebar-header {
  margin-bottom: 40px;
  padding: 0 8px;
}

.logo {
  color: var(--color-primary);
  font-family: 'Space Grotesk', sans-serif;
  font-weight: 900;
  font-size: 24px;
  letter-spacing: -0.05em;
  text-transform: uppercase;
  text-decoration: none;
  display: block;
}

.version {
  font-family: 'Manrope', sans-serif;
  font-weight: 500;
  text-transform: uppercase;
  letter-spacing: 0.15em;
  font-size: 10px;
  color: var(--color-on-surface-variant);
  margin-top: 4px;
}

.sidebar-nav {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 16px;
  color: var(--color-on-surface-variant);
  text-decoration: none;
  font-family: 'Manrope', sans-serif;
  font-weight: 500;
  text-transform: uppercase;
  letter-spacing: 0.1em;
  font-size: 12px;
  border-radius: var(--radius-lg);
  transition: all 300ms ease-in-out;
}

.nav-item:hover {
  color: var(--color-on-surface);
  background-color: var(--color-surface-container-low);
}

.nav-item.router-link-active {
  color: var(--color-primary);
  background-color: var(--color-surface-container-high);
  font-weight: 700;
  border-right: 4px solid var(--color-primary);
  border-radius: var(--radius-lg) 0 0 var(--radius-lg);
}

.nav-item .material-symbols-outlined {
  font-size: 24px;
}

.nav-item.router-link-active .material-symbols-outlined {
  font-variation-settings: 'FILL' 1;
}

.sidebar-footer {
  margin-top: auto;
  padding-top: 24px;
  border-top: 1px solid var(--color-outline-variant);
}

.primary-btn {
  width: 100%;
  padding: 16px;
  background-color: var(--color-primary);
  color: var(--color-on-primary-fixed);
  font-weight: 700;
  border-radius: var(--radius-xl);
  border: none;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  cursor: pointer;
  transition: all 200ms ease-out;
}

.primary-btn:hover {
  filter: brightness(1.1);
  box-shadow: 0 0 20px rgba(132, 173, 255, 0.2);
}

.primary-btn:active {
  transform: scale(0.95);
}

@media (max-width: 768px) {
  .app-sidebar {
    display: none;
  }
}
</style>
