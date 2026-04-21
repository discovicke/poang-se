<script setup lang="ts">

import { Chart as ChartJS, CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend } from 'chart.js'
import type { ChartOptions } from 'chart.js'
import { Line } from 'vue-chartjs';
import {ref, onMounted} from 'vue'
import { useGameApi } from '../composables/useGameApi'
import type {ScoreChartData} from '../types/game'

ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend)

const props = defineProps<{
  gameId: string
}>()
 
const chartData = ref<ScoreChartData | null>(null)
const { drawScoreChart } = useGameApi(props.gameId)

onMounted(async () => {
  chartData.value = await drawScoreChart({ gameId: props.gameId })
})

const chartOptions: ChartOptions<'line'> = {
    responsive: true,
    plugins: {
        legend: {
            position: 'top' as const,
            labels: {
                color: '#000',
                font: {
                    size: 12,
                    weight: 'bold',
                    
                },
            },
        },
        title: {
            display: true,
            text: 'Poängutveckling per spelare',
            font: {
                size: 16,
                weight: 'bold',
            },
             color: '#000',
        },
        tooltip: {
            callbacks: {
                label: function(context) {
                    return `${context.dataset.label}: ${context.parsed.y} poäng`;
                }
            },
             backgroundColor: 'rgba(0, 0, 0, 0.7)',
             padding: 10,
             titleColor: '#fff',
             bodyColor: '#fff',
             borderColor: '#000',
             borderWidth: 1,
        }
    },
    scales: {
        y: {
            beginAtZero: true,
            grid: {
                
                color: '#000',
                drawOnChartArea: true,
                lineWidth: 1,
                display: true,
            },
            title: {
                display: true,
                text: 'Poäng',
                color: '#000',
                font: {
                    size: 14,
                    weight: 'bold',
                },
            },
        },
        x: {
            grid: {
                color: '#000',
                drawOnChartArea: true,
                lineWidth: 1,
                display: true,
            },
            title: {
                display: true,
                text: 'Rundor',
                color: '#000',
                font: {
                    size: 14,
                    weight: 'bold',
                },
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