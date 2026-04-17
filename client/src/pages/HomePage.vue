<script setup lang="ts">
import {ref, onMounted} from 'vue'
import {useRouter} from 'vue-router'

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
const creatorOnly = ref(false)
const scoreIncrement = ref<number>(1)
const gameMode = ref<string | null>(null)
const gameModeValue = ref<number | null>(null)
const isPrivate = ref(false)
const gamePassword = ref('')
const isTemporary = ref(false)
const expiresAt = ref<string | null>(null)

async function fetchGames() {
  const res = await fetch('/api/games')
  games.value = await res.json()
}

async function createGame() {
  if (!gameName.value.trim()) return
  const res = await fetch('/api/games', {
    method: 'POST',
    headers: {'Content-Type': 'application/json'},
    body: JSON.stringify({
      name: gameName.value,
      lowerIsBetter: lowerIsBetter.value,
      maxRounds: maxRounds.value,
      startingScore: startingScore.value,
      creatorOnly: creatorOnly.value,
      scoreIncrement: scoreIncrement.value,
      gameMode: gameMode.value,
      gameModeValue: gameModeValue.value,
      isPrivate: isPrivate.value,
      gamePassword: isPrivate.value ? gamePassword.value : null,
      isTemporary: isTemporary.value,
      expiresAt: isTemporary.value ? expiresAt.value : null,
    }),
  })
  if (res.ok) {
    const game = await res.json()
    // Spara creatorSecret i localStorage
    localStorage.setItem(`creator:${game.id}`, game.creatorSecret)
    gameName.value = ''
    lowerIsBetter.value = false
    maxRounds.value = 1
    startingScore.value = 0
    creatorOnly.value = false
    scoreIncrement.value = 1
    gameMode.value = null
    gameModeValue.value = null
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
        <input v-model="gameName" placeholder="Spelnamn" required/>

        <label>
          <input type="checkbox" v-model="lowerIsBetter"/>
          Lägre poäng är bättre (t.ex. golf)
        </label>

        <label>
          <input type="checkbox" v-model="creatorOnly"/>
          Bara skaparen kan redigera poäng
        </label>

        <label>
          <input type="checkbox" v-model="isPrivate"/>
          Lösenordsskyddad match
        </label>
        <div v-if="isPrivate" class="label">
          Lösenord
          <input type="password" v-model="gamePassword" placeholder="Ange lösenord" required/>
        </div>

        <label>
          <input type="checkbox" v-model="isTemporary"/>
          Tillfällig match (avslutas efter angivet datum)
        </label>
        <div v-if="isTemporary" class="label">
          Tillfällig match - Hur länge ska matchen vara aktiv?
          <input type="datetime-local" v-model="expiresAt"/>
        </div>

        <div class="form-row">
          <div class="label">
            Max antal rundor
            <input type="number" v-model.number="maxRounds" min="1"/>
          </div>
          <div class="label">
            Startpoäng
            <input type="number" v-model.number="startingScore" step="any"/>
          </div>
          <div class="label">
            Poäng per klick
            <input type="number" v-model.number="scoreIncrement" min="0.1" step="any"/>
          </div>
        </div>

        <div class="label">
          Spelläge
          <select v-model="gameMode">
            <option :value="null">Standard</option>
            <option value="BestOf">Bäst av X</option>
            <option value="FirstTo">Först till X</option>
          </select>
        </div>
        <div v-if="gameMode" class="label">
          {{
            gameMode === 'BestOf'
              ? 'Bäst av (antal rundor)'
              : 'Först till (poäng)'
          }}
          <input type="number" v-model.number="gameModeValue" min="1"/>
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

