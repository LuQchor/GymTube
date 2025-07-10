<template>
  <section class="flex flex-col items-center justify-center min-h-screen px-4">
    <div class="w-full max-w-md bg-base-100 rounded-xl shadow p-8">
      <h2 class="text-2xl font-bold mb-6 text-center">Prijava</h2>
      
      <!-- Regular Login Form -->
      <form class="flex flex-col gap-4 mb-6" @submit.prevent="onLogin">
        <div class="form-control">
          <input 
            v-model="email" 
            type="email" 
            placeholder="Email" 
            class="input input-bordered input-lg w-full" 
            required 
          />
        </div>
        <div class="form-control">
          <input 
            v-model="password" 
            type="password" 
            placeholder="Lozinka" 
            class="input input-bordered input-lg w-full" 
            required 
          />
        </div>
        <button class="btn btn-primary btn-lg w-full" :disabled="loading">
          <span v-if="loading" class="loading loading-spinner"></span>
          Prijavi se
        </button>
      </form>

      <!-- Divider -->
      <div class="divider">ili</div>

      <!-- Google Login Button -->
      <button 
        @click="signInWithGoogle" 
        class="btn btn-outline btn-lg w-full" 
        :disabled="googleLoading"
      >
        <span v-if="googleLoading" class="loading loading-spinner"></span>
        <svg v-else class="w-5 h-5 mr-2" viewBox="0 0 24 24">
          <path fill="currentColor" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"/>
          <path fill="currentColor" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"/>
          <path fill="currentColor" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"/>
          <path fill="currentColor" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"/>
        </svg>
        Prijavi se s Googleom
      </button>

      <div v-if="error" class="alert alert-error mt-4">{{ error }}</div>
      
      <div class="text-center mt-4">
        <span>Nemaš račun?</span>
        <NuxtLink to="/register" class="link link-primary ml-1">Registriraj se</NuxtLink>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { mapUserApiToFrontend } from '~/utils/userMapper'
import { useAuth } from '~/composables/useAuth'
import { useRuntimeConfig } from '#app'

const email = ref('')
const password = ref('')
const error = ref('')
const loading = ref(false)
const googleLoading = ref(false)
const router = useRouter()
const { refreshUserData } = useAuth()
const config = useRuntimeConfig()
const GOOGLE_CLIENT_ID = config.public.googleClientId
const REDIRECT_URI = config.public.googleRedirectUri
const apiBaseUrl = config.public.apiBaseUrl

async function onLogin() {
  error.value = ''
  loading.value = true
  
  try {
    const response = await fetch(`${apiBaseUrl}/api/auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        email: email.value,
        password: password.value
      })
    })
    
    const data = await response.json()
    
    if (response.ok) {
      // Spremi JWT token
      if (typeof data.token === 'string') {
      localStorage.setItem('authToken', data.token)
      }
      
      // "Dekodiraj" JWT da dohvatiš korisničke podatke
      const tokenPayload = JSON.parse(atob(data.token.split('.')[1]));
      const user = mapUserApiToFrontend({
        id: tokenPayload.sub,
        email: tokenPayload.email,
        name: tokenPayload.name,
        is_premium: tokenPayload.is_premium,
        role: tokenPayload.role,
        profile_image_url: tokenPayload.profile_image_url
      });
      if (user && typeof user === 'object') {
        const userString = JSON.stringify(user)
        if (typeof userString === 'string') {
          localStorage.setItem('user', userString)
        }
      }
      
      // Ažuriraj stanje navigacije
      window.dispatchEvent(new CustomEvent('user-logged-in', {
        detail: { user: user }
      }));
      
      // Osvježi korisničke podatke s backenda (za ispravan premium status)
      await refreshUserData();
      
      // Preusmjeri na dashboard
      router.push('/dashboard')
    } else {
      error.value = data.error || 'Pogrešan email ili lozinka.'
    }
  } catch (e) {
    error.value = 'Došlo je do greške. Pokušaj ponovno.'
  } finally {
    loading.value = false
  }
}

function signInWithGoogle() {
  googleLoading.value = true
  
  // Generate random state for security
  const state = Math.random().toString(36).substring(2, 15)
  
  // Store state in localStorage for verification
  localStorage.setItem('googleOAuthState', state)
  
  // Build Google OAuth URL
  const googleAuthUrl = new URL('https://accounts.google.com/o/oauth2/v2/auth')
  googleAuthUrl.searchParams.set('client_id', typeof GOOGLE_CLIENT_ID === 'string' ? GOOGLE_CLIENT_ID : '')
  googleAuthUrl.searchParams.set('redirect_uri', typeof REDIRECT_URI === 'string' ? REDIRECT_URI : '')
  googleAuthUrl.searchParams.set('response_type', 'code')
  googleAuthUrl.searchParams.set('scope', 'openid email profile')
  googleAuthUrl.searchParams.set('state', state)
  
  // Redirect to Google
  window.location.href = googleAuthUrl.toString()
}

// Handle OAuth callback
onMounted(() => {
  const urlParams = new URLSearchParams(window.location.search)
  const code = urlParams.get('code')
  const state = urlParams.get('state')
  const error = urlParams.get('error')
  
  if (error) {
    return
  }
  
  if (code && state) {
    // Verify state
    const storedState = localStorage.getItem('googleOAuthState')
    if (state !== storedState) {
      return
    }
    
    // Clear state
    localStorage.removeItem('googleOAuthState')
    
    // Exchange code for token
    exchangeCodeForToken(code)
  }
})

async function exchangeCodeForToken(code: string) {
  googleLoading.value = true
  error.value = ''
  
  try {
    const response = await fetch(`${apiBaseUrl}/api/auth/google-signin`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ authorizationCode: code })
    })
    
    const data = await response.json()
    
    if (response.ok) {
      // Store JWT token
      if (typeof data.token === 'string') {
      localStorage.setItem('authToken', data.token)
      }
      
      // "Dekodiraj" JWT da dohvatiš korisničke podatke
      const tokenPayload = JSON.parse(atob(data.token.split('.')[1]));
      const user = mapUserApiToFrontend({
        id: tokenPayload.sub,
        email: tokenPayload.email,
        name: tokenPayload.name,
        is_premium: tokenPayload.is_premium,
        role: tokenPayload.role,
        profile_image_url: tokenPayload.profile_image_url
      });
      if (user && typeof user === 'object') {
        const userString = JSON.stringify(user)
        if (typeof userString === 'string') {
          localStorage.setItem('user', userString)
        }
      }
      
      // Ažuriraj stanje navigacije
      window.dispatchEvent(new CustomEvent('user-logged-in', {
        detail: { user: user }
      }));
      
      // Osvježi korisničke podatke s backenda (za ispravan premium status)
      await refreshUserData();
      
      // Redirect to dashboard
      router.push('/dashboard')
    } else {
      error.value = data.error || 'Google prijava nije uspjela.'
    }
  } catch (e) {
    error.value = 'Došlo je do greške prilikom prijave.'
  } finally {
    googleLoading.value = false
  }
}
</script>
