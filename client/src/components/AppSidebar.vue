<script setup lang="ts">
</script>

<template>
  <aside class="app-sidebar">
    <div class="sidebar-header">
      <router-link to="/" class="logo">Poäng.se</router-link>
      <p class="version">Poängräkning för de viktigaste tillfällena</p>
    </div>

    <div class="sidebar-footer">
      <router-link to="/" class="primary-btn">
        <span class="material-symbols-outlined">add_circle</span>
        Ny Match
      </router-link>
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
}

.sidebar-header {
  flex: 1;
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

.sidebar-footer {
  padding-top: 24px;
  border-top: 1px solid var(--color-outline-variant);
}

.primary-btn {
  width: 100%;
  padding: 16px;
  background-color: var(--color-primary);
  color: var(--color-on-primary-fixed);
  font-family: 'Space Grotesk', sans-serif;
  font-weight: 700;
  border-radius: var(--radius-xl);
  border: none;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  cursor: pointer;
  text-decoration: none;
  transition: all 200ms ease-out;
}

.primary-btn:hover {
  filter: brightness(1.1);
  box-shadow: 0 0 20px rgba(132, 173, 255, 0.2);
}

.primary-btn:active {
  transform: scale(0.95);
}

@media (max-width: 1024px) {
  .app-sidebar {
    display: none;
  }
}
</style>
