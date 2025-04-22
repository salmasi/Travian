<!-- views/RegisterView.vue -->
<template>
  <form @submit.prevent="handleSubmit">
    <input v-model="username" placeholder="نام کاربری">
    <input v-model="email" type="email" placeholder="ایمیل">
    <input v-model="password" type="password" placeholder="رمز عبور">
    <input v-model="confirmPassword" type="password" placeholder="تکرار رمز عبور">
    <button type="submit">ثبت نام</button>
  </form>
</template>

<script>
import { ref } from 'vue'
import { useAuthStore } from '@/stores/auth'

export default {
  setup() {
    const auth = useAuthStore()
    const username = ref('')
    const email = ref('')
    const password = ref('')
    const confirmPassword = ref('')

    const handleSubmit = async () => {
      if (password.value !== confirmPassword.value) {
        alert('رمز عبور و تکرار آن مطابقت ندارند')
        return
      }

      try {
        await auth.register(username.value, email.value, password.value)
      } catch (error) {
        alert(error)
      }
    }

    return { username, email, password, confirmPassword, handleSubmit }
  }
}
</script>