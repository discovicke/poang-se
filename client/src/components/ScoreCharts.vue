<script setup lang="ts">

import { Chart as ChartJS, CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend } from 'chart.js'
import { Line } from 'vue-chartjs';
import {ref, onMounted} from 'vue'
import { useGameApi } from '../composables/useGameApi'
import type {ScoreChartData} from '../types/game'

ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend)

// För att hämta all data för respektive i en respektive match.
const props = defineProps<{
  gameId: string
}>()
 
const chartData = ref<ScoreChartData | null>(null)
const { drawScoreChart } = useGameApi(props.gameId)

onMounted(async () => {
  chartData.value = await drawScoreChart({ gameId: props.gameId })
})

const chartOptions = {
    responsive: true,
    plugins: {
        legend: {
            position: 'top' as const,
        },
        title: {
            display: true,
            text: 'Poängutveckling per spelare',
        },
    },
    scales: {
        y: {
            beginAtZero: true,
            title: {
                display: true,
                text: 'Poäng',
            },
        },
        x: {
            title: {
                display: true,
                text: 'Rundor',
            },
        },
    },
}
</script>
<template>
    <div class="card">
        <h2>Poängutveckling</h2>
        <div v-if="chartData" class="chart-container">
            <Line :data="chartData" :options="chartOptions" />
        </div>
        <div v-else class="loading">
            <p>Ritar poängdiagram...</p>

        </div>
    </div>
</template>