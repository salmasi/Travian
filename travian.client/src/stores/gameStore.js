// stores/gameStore.js
export const useGameStore = defineStore('game', () => {
  const resources = ref({
    wood: 500,
    clay: 500,
    iron: 500,
    crop: 500
  });

  const buildings = ref([]);
  let updateInterval;

  // محاسبه تولید در دقیقه
  const productionRates = computed(() => {
    const rates = { wood: 0, clay: 0, iron: 0, crop: 0 };
    
    buildings.value.forEach(building => {
      const rate = building.baseRate + (building.ratePerLevel * building.level);
      
      switch(building.type) {
        case 'woodcutter': rates.wood += rate; break;
        case 'clay_pit': rates.clay += rate; break;
        case 'iron_mine': rates.iron += rate; break;
        case 'crop_farm': rates.crop += rate; break;
      }
    });
    
    return rates;
  });

  // شروع تولید خودکار
  function startProduction() {
    updateInterval = setInterval(async () => {
      await fetch('/api/village/update-resources', { method: 'POST' });
      await loadVillageData(); // بروزرسانی وضعیت
    }, 60000); // هر 1 دقیقه
  }

  function stopProduction() {
    clearInterval(updateInterval);
  }

  return { 
    resources, 
    buildings,
    productionRates,
    startProduction,
    stopProduction
  };
});