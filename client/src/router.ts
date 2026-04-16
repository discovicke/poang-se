import { createRouter, createWebHistory } from 'vue-router'
import HomePage from './pages/HomePage.vue'
import GamePage from './pages/GamePage.vue'
import PageNotFound from './pages/PageNotFound.vue'

const routes = [
  { path: '/', component: HomePage },
  { path: '/games/:id', component: GamePage, props: true },
  { path: '/:pathMatch(.*)*', name: 'NotFound', component: PageNotFound },
]

export const router = createRouter({
  history: createWebHistory(),
  routes,
})

