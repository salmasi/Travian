<script>
export default {
  props: {
    building: Object,
    villageId: Number
  },
  data() {
    return {
      isUpgrading: false
    }
  },
  methods: {
    async upgrade() {
      this.isUpgrading = true
      await fetch(`/api/village/${this.villageId}/buildings/${this.building.id}/upgrade`, {
        method: 'POST'
      })
      this.$emit('building-updated')
      this.isUpgrading = false
    }
  }
}
</script>

<template>
  <div class="building">
    <h3>{{ building.name }} (Level {{ building.level }})</h3>
    <button @click="upgrade" :disabled="isUpgrading">
      {{ isUpgrading ? 'Upgrading...' : 'Upgrade' }}
    </button>
  </div>
</template>