<script setup lang="ts">
import { useRoute } from 'vue-router'
import AppHeader from './components/AppHeader.vue'
import AppSidebar from './components/AppSidebar.vue'
import AppBottomNav from './components/AppBottomNav.vue'


const route = useRoute()
</script>

<template>
  <div class="app-shell">
    <!-- Desktop Sidebar -->
    <AppSidebar class="md-only" />

    <div class="main-container">
      <!-- Shared Header -->
      <AppHeader />

      <main class="content-area">
        <router-view :key="route.fullPath" />
      </main>

    </div>

    <!-- Mobile Bottom Nav -->
    <AppBottomNav class="mobile-only" />
  </div>
</template>

<style>
.app-shell {
  display: flex;
  min-height: 100vh;
}

.main-container {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-width: 0; /* Förhindrar flex-overflow */
}

.content-area {
  flex: 1;
}

/* Visibility Utilities */
.mobile-only { display: block; }
.md-only { display: none; }

@media (min-width: 768px) {
  .mobile-only { display: none; }
  .md-only { display: flex; }
}
</style>
