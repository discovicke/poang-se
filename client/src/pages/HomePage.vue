<script setup lang="ts">
import {ref} from 'vue'
import {useRouter} from 'vue-router'

const router = useRouter()

const gameName = ref('')
const isPrivate = ref(false)
const gamePassword = ref('')
const isTemporary = ref(false)
const expiresAt = ref<string | null>(null)

async function createGame() {
  if (!gameName.value.trim()) return

  const res = await fetch('/api/games', {
    method: 'POST',
    headers: {'Content-Type': 'application/json'},
    body: JSON.stringify({
      name: gameName.value,
      isPrivate: isPrivate.value,
      gamePassword: isPrivate.value ? gamePassword.value : null,
      isTemporary: isTemporary.value,
      expiresAt: isTemporary.value ? expiresAt.value : null,
      // Default values that will be refined in the lobby
      lowerIsBetter: false,
      maxRounds: 1,
      startingScore: 0,
      scoreIncrement: 1
    }),
  })

  if (res.ok) {
    const game = await res.json()
    localStorage.setItem(`creator:${game.id}`, game.creatorSecret)
    router.push(`/games/${game.id}`)
  }
}
</script>

<template>
  <div class="home-page">
    <div class="container">
      <header class="page-header text-center">
        <h1 class="display text-primary mb-md">POÄNG.se</h1>
        <p class="body-lg text-on-surface-variant max-w-md mx-auto">
          Starta en ny match på under 30 sekunder. Inget krångel, bara poäng.
        </p>
      </header>

      <main class="main-content">
        <div class="glass-card start-card">
          <div class="input-group">
            <label class="label-sm">Vad ska matchen heta?</label>
            <input
              v-model="gameName"
              class="primary-input font-headline"
              type="text"
              placeholder="t.ex. Fredagsdart"
              autofocus
            />
          </div>

          <div class="settings-grid mt-xl">
            <div class="setting-item">
              <label class="switch-item">
                <div class="switch-item-top">
                  <div class="switch">
                    <input type="checkbox" v-model="isTemporary">
                    <span class="slider"></span>
                  </div>
                  <span class="switch-title">Tillfällig match</span>
                </div>
                <p class="switch-desc">Matchen raderas automatiskt efter valt datum.</p>
              </label>
              <div v-if="isTemporary" class="mt-md animate-fade-in">
                <input type="datetime-local" v-model="expiresAt" class="secondary-input"/>
              </div>
            </div>

            <div class="setting-item">
              <label class="switch-item">
                <div class="switch-item-top">
                  <div class="switch">
                    <input type="checkbox" v-model="isPrivate">
                    <span class="slider"></span>
                  </div>
                  <span class="switch-title">Lösenordsskyddad</span>
                </div>
                <p class="switch-desc">Bara för inbjudna - kräver lösenord för att gå med.</p>
              </label>
              <div v-if="isPrivate" class="mt-md animate-fade-in">
                <input type="password" v-model="gamePassword" placeholder="Välj lösenord" class="secondary-input"/>
              </div>
            </div>
          </div>

          <button @click="createGame" class="submit-btn glow-primary mt-xl" :disabled="!gameName.trim()">
            SKAPA MATCH
          </button>
        </div>
      </main>
    </div>
  </div>
</template>

<style scoped>
.home-page {
  padding: 80px 24px;
  min-height: calc(100vh - 72px - 80px);
  display: flex;
  align-items: center;
  justify-content: center;
}

.container {
  max-width: 600px;
  width: 100%;
}

.start-card {
  padding: 48px;
  display: flex;
  flex-direction: column;
}

.primary-input {
  background-color: var(--color-surface-container-high);
  border: none;
  margin-top: 10px;
  border-radius: var(--radius-xl);
  padding: 24px;
  color: var(--color-on-surface);
  font-size: 24px;
  width: 100%;
  text-align: center;
  transition: all 200ms;
}

.primary-input:focus {
  outline: none;
  box-shadow: 0 0 0 2px var(--color-primary);
  background-color: var(--color-surface-container-highest);
}

.secondary-input {
  background-color: var(--color-surface-container-highest);
  border: 1px solid var(--color-outline-variant);
  border-radius: var(--radius-lg);
  padding: 12px 16px;
  color: var(--color-on-surface);
  width: 100%;
}

.settings-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 24px;
}

.switch-item {
  display: flex;
  flex-direction: column;
  background-color: var(--color-surface-container-high);
  border-radius: var(--radius-lg);
  padding: 16px;
  transition: background-color 200ms;
}

.switch-item:hover {
  background-color: var(--color-surface-container-highest);
}

.switch-item-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 8px;
}

.switch-title {
  font-family: 'Space Grotesk', sans-serif;
  font-weight: 700;
  font-size: 14px;
  color: var(--color-on-surface);
}

.switch-desc {
  font-size: 12px;
  color: var(--color-on-surface-variant);
  line-height: 1.4;
  padding-left: 0;
}

.switch {
  position: relative;
  width: 44px;
  height: 24px;
  flex-shrink: 0;
}

.switch input {
  opacity: 0;
  width: 0;
  height: 0;
}

.slider {
  position: absolute;
  inset: 0;
  background-color: var(--color-surface-container-highest);
  border-radius: 24px;
  transition: .4s;
}

.slider:before {
  position: absolute;
  content: "";
  height: 20px;
  width: 20px;
  left: 2px;
  bottom: 2px;
  background-color: white;
  transition: .4s;
  border-radius: 50%;
}

input:checked + .slider {
  background-color: var(--color-primary);
}

input:checked + .slider:before {
  transform: translateX(20px);
}

.submit-btn {
  background-color: var(--color-primary);
  color: var(--color-on-primary-fixed);
  border: none;
  border-radius: var(--radius-xl);
  padding: 24px;
  font-family: 'Space Grotesk', sans-serif;
  font-weight: 900;
  font-size: 20px;
  letter-spacing: 0.2em;
  cursor: pointer;
  transition: all 200ms;
}

.submit-btn:hover:not(:disabled) {
  transform: scale(1.02);
}

.submit-btn:active:not(:disabled) {
  transform: scale(0.98);
}

.submit-btn:disabled {
  opacity: 0.3;
  cursor: not-allowed;
}

.animate-fade-in {
  animation: fadeIn 300ms ease-out;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.text-center {
  text-align: center;
}

.mx-auto {
  margin-left: auto;
  margin-right: auto;
}

.max-w-md {
  max-width: 448px;
}

.mb-md {
  margin-bottom: 16px;
}

.mt-xl {
  margin-top: 32px;
}

.mt-md {
  margin-top: 16px;
}
</style>
