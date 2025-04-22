<!-- در BuildingUpgrade.vue نیاز به اضافه کردن: -->
<template>
  <button :disabled="isUpgrading">
    {{ isUpgrading ? 'Upgrading...' : `Upgrade (${cost})` }}
  </button>
</template>