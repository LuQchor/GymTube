import { ref, computed, onMounted, onUnmounted } from 'vue'
import { mapUserApiToFrontend } from '~/utils/userMapper'
import { useRouter } from 'vue-router'
import { useRuntimeConfig } from '#app'

export function useAuth() {
  const isAuthenticated = ref(false)
  const user = ref<any>(null)
  const userIsPremium = ref(false)
  const loading = ref(true)
  const router = useRouter()
  const config = useRuntimeConfig()

  const userInitial = computed(() => user.value?.name?.charAt(0).toUpperCase() || 'U')

  function checkAuthStatus() {
    const userData = localStorage.getItem('user')
    const token = localStorage.getItem('authToken')
    
    if (userData && token) {
      try {
        const userObj = JSON.parse(userData)
        isAuthenticated.value = true
        user.value = userObj
        userIsPremium.value = userObj.is_premium === true
      } catch (e) {
        // Invalid user data, clear it
        localStorage.removeItem('user')
        localStorage.removeItem('authToken')
        isAuthenticated.value = false
        user.value = null
        userIsPremium.value = false
      }
    } else {
      isAuthenticated.value = false
      user.value = null
      userIsPremium.value = false
    }
    loading.value = false
  }

  async function refreshUserData() {
    if (!isAuthenticated.value) {
      return
    }
    
    try {
      const token = localStorage.getItem('authToken')
      const response = await fetch(`${config.public.apiBaseUrl}/api/auth/current-user`, {
        method: 'GET',
        headers: { 'Authorization': `Bearer ${token}` }
      })
      
      if (response.ok) {
        const data = await response.json()
        const mappedUser = mapUserApiToFrontend(data.user)
        localStorage.setItem('authToken', data.token)
        localStorage.setItem('user', JSON.stringify(mappedUser))
        user.value = mappedUser
        userIsPremium.value = mappedUser.is_premium === true
        isAuthenticated.value = true
        
        // Emit custom event za osvježavanje navbar-a
        window.dispatchEvent(new CustomEvent('user-data-updated', {
          detail: { user: mappedUser }
        }))
      }
    } catch (e) {
      // Fallback: check localStorage
      const userData = localStorage.getItem('user')
      if (userData) {
        try {
          const userObj = JSON.parse(userData)
          userIsPremium.value = userObj.is_premium === true
        } catch (e) {
          userIsPremium.value = false
        }
      }
    }
  }

  function logout() {
    localStorage.removeItem('authToken')
    localStorage.removeItem('user')
    isAuthenticated.value = false
    user.value = null
    userIsPremium.value = false
    
    // Dispatch logout event
    window.dispatchEvent(new CustomEvent('user-logged-out'))
    // Redirect to index page
    router.push('/')
  }

  function handleVisibilityChange() {
    if (!document.hidden) {
      // User returned to the app, refresh user data
      refreshUserData()
    }
  }

  onMounted(() => {
    checkAuthStatus()
    
    // Listen for login/logout events
    window.addEventListener('user-logged-in', (event: any) => {
      isAuthenticated.value = true
      user.value = event.detail.user
      userIsPremium.value = event.detail.user.is_premium === true
    })
    
    window.addEventListener('user-logged-out', () => {
      isAuthenticated.value = false
      user.value = null
      userIsPremium.value = false
    })
    
    // Listen for user data updates
    window.addEventListener('user-data-updated', (event: any) => {
      if (event.detail.user) {
        user.value = event.detail.user
        userIsPremium.value = event.detail.user.is_premium === true
        isAuthenticated.value = true
      }
    })
    
    // Listen for storage changes (when localStorage is modified from other tabs)
    window.addEventListener('storage', (event) => {
      if (event.key === 'authToken' || event.key === 'user') {
        checkAuthStatus()
      }
    })
    
    // Listen for visibility change to refresh data when user returns to the app
    document.addEventListener('visibilitychange', handleVisibilityChange)
  })

  onUnmounted(() => {
    document.removeEventListener('visibilitychange', handleVisibilityChange)
    window.removeEventListener('user-data-updated', () => {})
  })

  return {
    isAuthenticated,
    user,
    userIsPremium,
    userInitial,
    loading,
    checkAuthStatus,
    refreshUserData,
    logout
  }
} 