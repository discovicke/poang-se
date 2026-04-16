import {ref} from 'vue'
import * as signalR from '@microsoft/signalr'
import type {ClaimInfo} from '../types/game'


/**
 * Composable som hanterar SignalR-anslutningen till en spel.
 * Hanterar anslutning, claim/unclaim av spelare och kopplar upp
 * till SignalR-hubben för att hantera realtidsuppdateringar av spelet.
 */
export function useSignalR(gameId: string) {
  const connection = ref<signalR.HubConnection | null>(null)
  const claim = ref<ClaimInfo | null>(null)
  const showClaimPicker = ref(false)

  /**
   * Bygger och startar en SignalR-anslutning. Kan valfritt försöka claima en specifik spelare.
   * @param onGameUpdated callback som anropas när servern pushar en uppdatering
   * @param playerId valfri spelare att försöka claima vid anslutning
   */
  async function connect(
    onGameUpdated: () => void | Promise<void>,
    playerId?: string,
  ) {
    const secret = localStorage.getItem(`creator:${gameId}`) ?? ''
    let url = `/gamehub?gameId=${gameId}`
    if (playerId) url += `&playerId=${playerId}`
    if (secret) url += `&creatorSecret=${secret}`

    const conn = new signalR.HubConnectionBuilder()
      .withUrl(url)
      .withAutomaticReconnect()
      .build()

    conn.on('ClaimAccepted', (c: ClaimInfo) => {
      claim.value = c
      localStorage.setItem(`claim:${gameId}`, JSON.stringify(c))
    })

    conn.on('ClaimRejected', () => {
      showClaimPicker.value = true
    })

    conn.on('GameUpdated', onGameUpdated)
    conn.on('ScoreAdded', onGameUpdated)

    await conn.start()
    connection.value = conn
  }

  /** Avslutar nuvarande anslutning, sen återansluter (valfritt försöker claima en annan spelare). */
  async function claimPlayer(
    playerId: string | null,
    onGameUpdated: () => void | Promise<void>,
  ) {
    showClaimPicker.value = false
    if (connection.value) {
      try {
        await connection.value.stop()
      } catch { /* ignore */
      }
    }
    await connect(onGameUpdated, playerId ?? undefined)
  }

  /** Släpper den aktuella claimen. */
  async function unclaimPlayer() {
    if (connection.value && claim.value?.playerId) {
      await connection.value.invoke('UnclaimPlayer', gameId, claim.value.playerId)
      claim.value = null
      localStorage.removeItem(`claim:${gameId}`)
      showClaimPicker.value = true
    }
  }

  /** Lämnar spelet och stänger anslutningen. */
  async function disconnect() {
    if (connection.value) {
      try {
        await connection.value.invoke('LeaveGame', gameId)
        await connection.value.stop()
      } catch { /* ignore */
      }
    }
  }

  return {
    connection,
    claim,
    showClaimPicker,
    connect,
    claimPlayer,
    unclaimPlayer,
    disconnect,
  }
}

