<!-- components/AttackForm.vue -->
<template>
  <div class="attack-form">
    <h3>حمله به روستای {{ targetVillageName }}</h3>
    
    <div class="troop-selection">
      <div v-for="troop in availableTroops" :key="troop.type" class="troop-item">
        <label>{{ troop.name }}</label>
        <input 
          type="number" 
          v-model.number="selectedTroops[troop.type]" 
          :max="troop.quantity"
          min="0">
        <span>دارایی: {{ troop.quantity }}</span>
      </div>
    </div>

    <button @click="launchAttack" :disabled="isAttacking">
      {{ isAttacking ? 'در حال ارسال...' : 'شروع حمله' }}
    </button>

    <div v-if="attackResult" class="result">
      <p>حمله با موفقیت برنامه‌ریزی شد!</p>
      <p>زمان رسیدن: {{ formatTime(attackResult.arrivalTime) }}</p>
    </div>
  </div>
</template>

<script>
export default {
  props: ['targetVillageId', 'targetVillageName'],
  data() {
    return {
      availableTroops: [],
      selectedTroops: {},
      isAttacking: false,
      attackResult: null
    }
  },
  async created() {
    const response = await fetch(`/api/village/${this.$route.params.villageId}/troops`);
    this.availableTroops = await response.json();
    
    // Initialize selected troops
    this.availableTroops.forEach(troop => {
      this.$set(this.selectedTroops, troop.type, 0);
    });
  },
  methods: {
    async launchAttack() {
      this.isAttacking = true;
      
      const troops = Object.entries(this.selectedTroops)
        .filter(([_, count]) => count > 0)
        .map(([type, quantity]) => ({ troopType: type, quantity }));
      
      const attackData = {
        attackerVillageId: this.$route.params.villageId,
        defenderVillageId: this.targetVillageId,
        attackType: 'attack',
        troops
      };

      try {
        const response = await fetch('/api/battle/attack', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(attackData)
        });
        
        this.attackResult = await response.json();
      } catch (error) {
        alert('خطا در ارسال حمله');
      } finally {
        this.isAttacking = false;
      }
    },
    formatTime(timeString) {
      return new Date(timeString).toLocaleString();
    }
  }
}
</script>