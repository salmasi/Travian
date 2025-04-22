<!-- views/LoginView.vue -->
<template>
  <form @submit.prevent="handleSubmit">
    <input v-model="usernameOrEmail" placeholder="نام کاربری یا ایمیل">
    <input v-model="password" type="password" placeholder="رمز عبور">
    <button type="submit">ورود</button>
  </form>
</template>

<script>
import { ref } from 'vue'
import { useAuthStore } from '@/stores/auth'

export default {
  setup() {
    const auth = useAuthStore()
    const usernameOrEmail = ref('')
    const password = ref('')

    const handleSubmit = async () => {
      try {
        await auth.login(usernameOrEmail.value, password.value)
      } catch (error) {
        alert(error)
      }
    }

    return { usernameOrEmail, password, handleSubmit }
  }
}
</script>