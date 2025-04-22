import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useGameStore = defineStore('game', () => {
  const player = ref(null)
  const currentVillage = ref(null)
  const resources = ref({
    wood: 500,
    clay: 500,
    iron: 500,
    crop: 500
  })

  async function login(username, password) {
    const response = await fetch('/api/auth/login', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ username, password })
    })
    player.value = await response.json()
  }

  async function loadVillage(villageId) {
    const response = await fetch(`/api/village/${villageId}`)
    currentVillage.value = await response.json()
  }

  return { player, currentVillage, resources, login, loadVillage }
})