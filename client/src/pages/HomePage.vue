<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()

interface Game {
  id: string
  name: string
  status: string
  createdAt: string
}

const games = ref<Game[]>([])

// Create game form
const gameName = ref('')
const lowerIsBetter = ref(false)
const maxRounds = ref<number | null>(1)
const startingScore = ref<number>(0)
const isPrivate = ref(false)
const gamePassword = ref('')

async function fetchGames() {
  const res = await fetch('/api/games')
  games.value = await res.json()
}

async function createGame() {
  if (!gameName.value.trim()) return
  const res = await fetch('/api/games', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      name: gameName.value,
      lowerIsBetter: lowerIsBetter.value,
      maxRounds: maxRounds.value,
      startingScore: startingScore.value,
      isPrivate: isPrivate.value,
      gamePassword: isPrivate.value ? gamePassword.value : null,
    }),
  })
  if (res.ok) {
    const game = await res.json()
    gameName.value = ''
    lowerIsBetter.value = false
    maxRounds.value = 1
    startingScore.value = 0
    isPrivate.value = false
    gamePassword.value = ''
    router.push(`/games/${game.id}`)
  }
}

function badgeClass(status: string) {
  return `badge badge-${status.toLowerCase()}`
}

onMounted(fetchGames)
</script>

<template>
  <div class="page">
    <h1>POÄNG.SE</h1>

    <div class="card">
      <h2>Skapa nytt spel</h2>
      <form @submit.prevent="createGame" class="form-col">
        <input v-model="gameName" placeholder="Spelnamn" required />
        <label>
          <input type="checkbox" v-model="lowerIsBetter" />
          Lägre poäng är bättre (t.ex. golf)
        </label>
        <div class="label">
          Max antal rundor
          <input type="number" v-model.number="maxRounds" min="1" />
        </div>
        <div class="label">
          Startpoäng
          <input type="number" v-model.number="startingScore" step="any" />
        </div>
        <label>
          <input type="checkbox" v-model="isPrivate" />
          Privat match (lösenordsskyddad)
        </label>
        <div v-if="isPrivate" class="label">
          Lösenord
          <input type="password" v-model="gamePassword" placeholder="Ange lösenord" required />
        </div>
        <button type="submit" class="btn-primary">Skapa spel</button>
      </form>
    </div>

    <div class="card">
      <h2>Alla spel</h2>
      <p v-if="!games.length" class="empty">Inga spel ännu.</p>
      <table v-else class="games-table">
        <thead>
          <tr>
            <th>Namn</th>
            <th>Status</th>
            <th>Skapad</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="g in games" :key="g.id">
            <td>{{ g.name }}</td>
            <td><span :class="badgeClass(g.status)">{{ g.status }}</span></td>
            <td>{{ new Date(g.createdAt).toLocaleString('sv-SE') }}</td>
            <td>
              <router-link :to="`/games/${g.id}`">Öppna →</router-link>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

