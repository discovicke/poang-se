import { createRouter, createWebHistory } from 'vue-router'
import HomePage from './pages/HomePage.vue'
import GamePage from './pages/GamePage.vue'

const routes = [
  { path: '/', component: HomePage },
  { path: '/games/:id', component: GamePage, props: true },
]

export const router = createRouter({
  history: createWebHistory(),
  routes,
})

