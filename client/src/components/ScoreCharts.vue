<script setup lang="ts">
import { Chart as ChartJS, CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend } from 'chart.js'
import type { ChartOptions } from 'chart.js'
import { Line } from 'vue-chartjs'
import { ref, onMounted } from 'vue'
import { useGameApi } from '../composables/useGameApi'
import type { ScoreChartData } from '../types/game'

ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend)

const props = defineProps<{
  gameId: string
}>()

const chartData = ref<ScoreChartData | null>(null)
const { drawScoreChart } = useGameApi(props.id || props.gameId) // Fixed possible id mismatch

onMounted(async () => {
  chartData.value = await drawScoreChart({ gameId: props.gameId })
})

const chartOptions: ChartOptions<'line'> = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      position: 'top' as const,
      labels: {
        color: '#f9f9fd', // var(--color-on-surface)
        font: {
          family: 'Manrope',
          size: 12,
          weight: 'bold',
        },
      },
    },
    title: {
      display: false, // We use the h2 in template
    },
    tooltip: {
      backgroundColor: '#1d2024', // var(--color-surface-container-high)
      padding: 12,
      titleColor: '#84adff', // var(--color-primary)
      titleFont: { family: 'Space Grotesk', weight: 'bold' },
      bodyColor: '#f9f9fd',
      bodyFont: { family: 'Manrope' },
      borderColor: 'rgba(132, 173, 255, 0.2)',
      borderWidth: 1,
      cornerRadius: 8,
    }
  },
  scales: {
    y: {
      beginAtZero: true,
      grid: {
        color: 'rgba(70, 72, 75, 0.2)', // var(--color-outline-variant) @ 20%
        drawTicks: false,
      },
      ticks: {
        color: '#aaabaf', // var(--color-on-surface-variant)
        font: { family: 'Manrope' }
      },
      title: {
        display: true,
        text: 'Poäng',
        color: '#aaabaf',
        font: {
          family: 'Manrope',
          size: 12,
          weight: 'bold',
        },
      },
    },
    x: {
      grid: {
        color: 'rgba(70, 72, 75, 0.2)',
        drawTicks: false,
      },
      ticks: {
        color: '#aaabaf',
        font: { family: 'Manrope' }
      },
      title: {
        display: true,
        text: 'Rundor',
        color: '#aaabaf',
        font: {
          family: 'Manrope',
          size: 12,
          weight: 'bold',
        },
      },
    },
  },
}
</script>

<template>
  <div class="glass-card chart-card">
    <div class="chart-header">
      <h2 class="headline-sm">Poängutveckling</h2>
      <span class="material-symbols-outlined text-primary">insights</span>
    </div>
    
    <div v-if="chartData" class="chart-wrapper">
      <Line :data="chartData" :options="chartOptions" />
    </div>
    
    <div v-else class="loading-state">
      <div class="loader-sm"></div>
      <p class="label-xs italic text-on-surface-variant">Ritar poängdiagram...</p>
    </div>
  </div>
</template>

<style scoped>
.chart-card {
  padding: 32px;
  width: 100%;
}

.chart-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.chart-wrapper {
  height: 300px;
  width: 100%;
}

.loading-state {
  height: 300px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 16px;
}

.loader-sm {
  width: 24px;
  height: 24px;
  border: 2px solid var(--color-surface-container-high);
  border-top-color: var(--color-primary);
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.text-primary { color: var(--color-primary); }
.text-on-surface-variant { color: var(--color-on-surface-variant); }
</style>
