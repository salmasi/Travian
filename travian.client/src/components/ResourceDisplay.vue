<!-- components/ResourceDisplay.vue -->
<template>
  <div class="resources">
    <div v-for="(value, resource) in resources" :key="resource" class="resource">
      <span class="resource-name">{{ $t(`resources.${resource}`) }}:</span>
      <span class="resource-value">{{ Math.floor(value) }}</span>
      <span class="resource-rate">({{ productionRates[resource] }}/min)</span>
    </div>
  </div>
</template>

<script>
export default {
  computed: {
    resources() {
      return this.$gameStore.resources;
    },
    productionRates() {
      return this.$gameStore.productionRates;
    }
  }
}
</script>

<style>
.resources {
  display: flex;
  gap: 1rem;
}
.resource {
  background: rgba(0,0,0,0.1);
  padding: 0.5rem;
  border-radius: 4px;
}
.resource-rate {
  color: #4CAF50;
  font-size: 0.8em;
}
</style>