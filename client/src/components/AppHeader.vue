<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount } from 'vue'
import { useRouter } from 'vue-router'
import { useActivityFeed } from '../composables/useActivityFeed'

defineProps<{
  matchContext?: string
  matchName?: string
}>()

const router = useRouter()
const { state: feed, markAllRead, timeAgo } = useActivityFeed()

const showNotifs = ref(false)
const panelRef = ref<HTMLElement | null>(null)

function toggleNotifs() {
  showNotifs.value = !showNotifs.value
  if (showNotifs.value) markAllRead()
}

function onClickOutside(e: MouseEvent) {
  if (panelRef.value && !panelRef.value.contains(e.target as Node)) {
    showNotifs.value = false
  }
}

onMounted(() => document.addEventListener('mousedown', onClickOutside))
onBeforeUnmount(() => document.removeEventListener('mousedown', onClickOutside))
</script>

<template>
  <header class="app-header">
    <div class="header-left">
      <div class="logo-block">
        <router-link to="/" class="logo">
          <span class="logo-main">POÄNG</span><span class="logo-suffix">.se</span>
        </router-link>
        <span class="tagline">Det roligaste är poängen</span>
      </div>
      <div v-if="matchName" class="match-info">
        <span class="label-sm text-on-surface-variant">{{ matchContext || 'Pågående Match' }}</span>
        <h2 class="headline-sm text-primary">{{ matchName }}</h2>
      </div>
    </div>

    <div class="header-actions">
      <!-- When on a game page: home button -->
      <button v-if="matchName" class="icon-btn" @click="router.push('/')" title="Ny match">
        <span class="material-symbols-outlined">home</span>
      </button>

      <!-- Notification Bell -->
      <div class="notif-wrapper" ref="panelRef">
        <button class="icon-btn notif-btn" @click="toggleNotifs" title="Aktivitet">
          <span class="material-symbols-outlined">notifications</span>
          <span v-if="feed.unreadCount > 0" class="badge">{{ feed.unreadCount > 9 ? '9+' : feed.unreadCount }}</span>
        </button>

        <Transition name="panel">
          <div v-if="showNotifs" class="notif-panel glass-card">
            <div class="notif-header">
              <span class="label-sm">Aktivitet</span>
              <span class="notif-count label-sm">{{ feed.events.length }} händelser</span>
            </div>

            <div v-if="feed.events.length" class="notif-list">
              <div
                v-for="ev in feed.events"
                :key="ev.id"
                class="notif-item"
                :class="{ unread: !ev.read }"
              >
                <span class="material-symbols-outlined notif-icon">{{ ev.icon }}</span>
                <div class="notif-body">
                  <p class="notif-msg">{{ ev.message }}</p>
                  <span class="notif-time">{{ timeAgo(ev.timestamp) }}</span>
                </div>
              </div>
            </div>

            <div v-else class="notif-empty">
              <span class="material-symbols-outlined">notifications_off</span>
              <p class="label-sm">Inga händelser ännu</p>
            </div>
          </div>
        </Transition>
      </div>
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
  border-bottom: 1px solid var(--color-outline-variant);
}

.logo-block {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.logo {
  font-family: 'Space Grotesk', sans-serif;
  font-weight: 900;
  font-size: 24px;
  letter-spacing: -0.05em;
  text-transform: uppercase;
  text-decoration: none;
  line-height: 1;
}

.logo-main {
  color: var(--color-primary-container);
}

.logo-suffix {
  color: var(--color-secondary);
}

.tagline {
  display: block;
  font-family: 'Manrope', sans-serif;
  font-weight: 500;
  font-size: 10px;
  letter-spacing: 0.15em;
  text-transform: uppercase;
  color: var(--color-on-surface-variant);
}

.header-left {
  display: flex;
  align-items: center;
  gap: 24px;
}

.match-info {
  display: flex;
  flex-direction: column;
  padding-left: 24px;
  border-left: 1px solid var(--color-outline-variant);
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 8px;
}

.icon-btn {
  background: transparent;
  border: none;
  color: var(--color-on-surface-variant);
  padding: 8px;
  border-radius: var(--radius-full);
  cursor: pointer;
  transition: all 200ms ease-out;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
}

.icon-btn:hover {
  background-color: var(--color-surface-container-high);
  color: var(--color-primary);
}

.icon-btn:active {
  transform: scale(0.95);
}

/* Badge */
.badge {
  position: absolute;
  top: 2px;
  right: 2px;
  background-color: var(--color-error);
  color: #fff;
  font-size: 9px;
  font-weight: 900;
  min-width: 16px;
  height: 16px;
  border-radius: var(--radius-full);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0 3px;
  line-height: 1;
}

/* Notification panel */
.notif-wrapper {
  position: relative;
}

.notif-panel {
  position: absolute;
  top: calc(100% + 12px);
  right: 0;
  width: 320px;
  max-height: 420px;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  padding: 0;
  z-index: 200;
  border-radius: var(--radius-2xl);
  box-shadow: 0 16px 48px rgba(0, 0, 0, 0.4);
}

.notif-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 20px 12px;
  border-bottom: 1px solid var(--color-outline-variant);
}

.notif-count {
  color: var(--color-on-surface-variant);
}

.notif-list {
  overflow-y: auto;
  flex: 1;
}

.notif-item {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  padding: 12px 20px;
  border-bottom: 1px solid rgba(70, 72, 75, 0.3);
  transition: background-color 150ms;
}

.notif-item:last-child {
  border-bottom: none;
}

.notif-item.unread {
  background-color: rgba(132, 173, 255, 0.05);
}

.notif-item.unread::before {
  content: '';
  position: absolute;
  left: 8px;
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background-color: var(--color-primary);
  margin-top: 6px;
}

.notif-icon {
  font-size: 18px;
  color: var(--color-primary);
  flex-shrink: 0;
  margin-top: 1px;
}

.notif-body {
  display: flex;
  flex-direction: column;
  gap: 2px;
  flex: 1;
  min-width: 0;
}

.notif-msg {
  font-size: 13px;
  font-weight: 500;
  color: var(--color-on-surface);
  line-height: 1.4;
}

.notif-time {
  font-size: 11px;
  color: var(--color-on-surface-variant);
}

.notif-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 40px 20px;
  color: var(--color-on-surface-variant);
  opacity: 0.5;
}

.notif-empty span {
  font-size: 36px;
}

/* Panel transition */
.panel-enter-active,
.panel-leave-active {
  transition: opacity 150ms ease, transform 150ms ease;
}

.panel-enter-from,
.panel-leave-to {
  opacity: 0;
  transform: translateY(-8px) scale(0.97);
}

.text-primary { color: var(--color-primary); }
.text-on-surface-variant { color: var(--color-on-surface-variant); }


</style>
