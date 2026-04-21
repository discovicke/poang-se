<script setup lang="ts">
defineEmits<{
  (e: 'click'): void
}>()
</script>

<template>
  <button class="app-fab" @click="$emit('click')" aria-label="Ny Match">
    <span class="material-symbols-outlined">add</span>
  </button>
</template>

<style scoped>
.app-fab {
  position: fixed;
  bottom: 24px;
  right: 24px;
  width: 56px;
  height: 56px;
  border-radius: var(--radius-full);
  background-color: var(--color-primary);
  color: var(--color-on-primary-fixed);
  border: none;
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 100;
  box-shadow: 0 0 40px rgba(132, 173, 255, 0.4);
  cursor: pointer;
  transition: all 200ms ease-out;
}

.app-fab:active {
  transform: scale(0.9);
}

.app-fab .material-symbols-outlined {
  font-size: 32px;
}

@media (min-width: 768px) {
  .app-fab {
    display: none;
  }
}
</style>
