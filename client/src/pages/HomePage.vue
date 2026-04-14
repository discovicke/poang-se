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

interface Player {
  id: string
  userName: string
}

const games = ref<Game[]>([])
const players = ref<Player[]>([])

// Create game form
const gameName = ref('')
const lowerIsBetter = ref(false)
const maxRounds = ref<number | null>(1)

async function fetchGames() {
  const res = await fetch('/api/games')
  games.value = await res.json()
}

async function fetchPlayers() {
  const res = await fetch('/api/players')
  players.value = await res.json()
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
    }),
  })
  if (res.ok) {
    const game = await res.json()
    gameName.value = ''
    lowerIsBetter.value = false
    maxRounds.value = 1
    router.push(`/games/${game.id}`)
  }
}

// Skapa spelare
const newPlayerName = ref('')

async function createPlayer() {
  if (!newPlayerName.value.trim()) return
  const res = await fetch('/api/players', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ userName: newPlayerName.value }),
  })
  if (res.ok) {
    newPlayerName.value = ''
    await fetchPlayers()
  }
}

function badgeClass(status: string) {
  return `badge badge-${status.toLowerCase()}`
}

onMounted(() => {
  fetchGames()
  fetchPlayers()
})
</script>

<template>
  <div class="page">
    <h1>POÄNG.SE</h1>

    <div class="card">
      <h2>Spelare</h2>
      <form @submit.prevent="createPlayer" class="form-row">
        <input v-model="newPlayerName" placeholder="Spelarnamn" />
        <button type="submit" class="btn-primary">Lägg till spelare</button>
      </form>
      <ul v-if="players.length" class="player-list">
        <li v-for="p in players" :key="p.id">{{ p.userName }}</li>
      </ul>
      <p v-else class="empty">Inga spelare ännu.</p>
    </div>

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

