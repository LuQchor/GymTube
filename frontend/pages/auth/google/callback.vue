<template>
  <div class="flex items-center justify-center min-h-screen">
    <div class="text-center">
      <div v-if="loading" class="space-y-4">
        <div class="loading loading-spinner loading-lg"></div>
        <p class="text-lg">Obrađujem Google prijavu...</p>
      </div>
      
      <div v-else-if="error" class="space-y-4">
        <div class="alert alert-error">
          <svg xmlns="http://www.w3.org/2000/svg" class="stroke-current shrink-0 h-6 w-6" fill="none" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <span>{{ error }}</span>
        </div>
        <NuxtLink to="/login" class="btn btn-primary">Povratak na prijavu</NuxtLink>
      </div>
      
      <div v-else-if="success" class="space-y-4">
        <div class="alert alert-success">
          <svg xmlns="http://www.w3.org/2000/svg" class="stroke-current shrink-0 h-6 w-6" fill="none" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <span>Uspješna prijava! Preusmjeravam...</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { mapUserApiToFrontend } from '~/utils/userMapper'
import { useRuntimeConfig } from '#app'

const router = useRouter()
const config = useRuntimeConfig()
const apiBaseUrl = config.public.apiBaseUrl
const loading = ref(true)
const error = ref('')
const success = ref(false)

onMounted(async () => {
  try {
    const urlParams = new URLSearchParams(window.location.search)
    const code = urlParams.get('code')
    const state = urlParams.get('state')
    const oauthError = urlParams.get('error')
    
    if (oauthError) {
      error.value = `Google greška: ${oauthError}`
      loading.value = false
      return
    }
    
    if (!code || !state) {
      error.value = 'Nedostaju potrebni podaci za prijavu.'
      loading.value = false
      return
    }
    
    // Verify state
    const storedState = localStorage.getItem('googleOAuthState')
    if (state !== storedState) {
      error.value = 'Sigurnosna greška: neispravan state.'
      loading.value = false
      return
    }
    
    // Clear state
    localStorage.removeItem('googleOAuthState')
    
    // Exchange code for token
    const response = await fetch(`${apiBaseUrl}/api/auth/google-signin`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ authorizationCode: code })
    })
    
    const data = await response.json()
    
    if (response.ok) {
      // Store JWT token and user data
      localStorage.setItem('authToken', data.token)

      // "Decode" JWT to get user info
      const tokenPayload = JSON.parse(atob(data.token.split('.')[1]));
      const user = mapUserApiToFrontend({
        id: tokenPayload.sub,
        email: tokenPayload.email,
        name: tokenPayload.name,
        is_premium: tokenPayload.is_premium,
        role: tokenPayload.role,
        profile_image_url: tokenPayload.profile_image_url
      });
      localStorage.setItem('user', JSON.stringify(user));
      
      // Update navbar state
      window.dispatchEvent(new CustomEvent('user-logged-in', {
        detail: { user: user }
      }));
      
      success.value = true
      
      // Redirect to dashboard after a short delay
      setTimeout(() => {
        router.push('/dashboard')
      }, 1500)
    } else {
      error.value = data.error + (data.details ? ` (${data.details})` : '') || 'Google prijava nije uspjela.'
    }
  } catch (e) {
    error.value = 'Došlo je do greške prilikom prijave.'
  } finally {
    loading.value = false
  }
})
</script> 