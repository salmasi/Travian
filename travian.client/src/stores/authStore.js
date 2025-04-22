// stores/authStore.js
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'

export const useAuthStore = defineStore('auth', () => {
  const user = ref(null)
  const isAuthenticated = ref(false)
  const router = useRouter()

  async function register(username, email, password) {
    try {
      const response = await axios.post('/api/auth/register', {
        username,
        email,
        password
      })
      
      // هدایت به صفحه تایید ایمیل
      router.push('/verify-email')
    } catch (error) {
      throw error.response?.data?.message || 'خطا در ثبت نام'
    }
  }

  async function login(usernameOrEmail, password) {
    try {
      const response = await axios.post('/api/auth/login', {
        usernameOrEmail,
        password
      })

      // ذخیره توکن در localStorage
      localStorage.setItem('token', response.data.token)
      localStorage.setItem('refreshToken', response.data.refreshToken)

      // دریافت اطلاعات کاربر
      await fetchUser()

      // هدایت به صفحه اصلی
      router.push('/')
    } catch (error) {
      throw error.response?.data?.message || 'خطا در ورود'
    }
  }

  async function fetchUser() {
    try {
      const response = await axios.get('/api/me')
      user.value = response.data
      isAuthenticated.value = true
    } catch (error) {
      logout()
    }
  }

  function logout() {
    localStorage.removeItem('token')
    localStorage.removeItem('refreshToken')
    user.value = null
    isAuthenticated.value = false
    router.push('/login')
  }

  return { user, isAuthenticated, register, login, logout, fetchUser }
})