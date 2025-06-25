<template>
  <section class="container mx-auto px-4 py-8">
    <!-- Loading State -->
    <div v-if="loading" class="flex justify-center items-center py-12">
      <div class="loading loading-spinner loading-lg"></div>
      <span class="ml-4">Učitavam profil...</span>
    </div>
    
    <div v-else-if="error" class="alert alert-error mb-8">
      <svg xmlns="http://www.w3.org/2000/svg" class="stroke-current shrink-0 h-6 w-6" fill="none" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
      </svg>
      <span>{{ error }}</span>
    </div>
    
    <div v-else-if="userProfile" class="space-y-8">
      <!-- Profile Header -->
      <div class="card bg-gradient-to-br from-primary/10 to-secondary/10 border border-primary/20">
        <div class="card-body">
          <div class="flex flex-col lg:flex-row items-center lg:items-start gap-6">
            <!-- Profile Image -->
            <div class="avatar">
              <div v-if="userProfile.profileImageUrl" class="w-32 h-32 rounded-full ring-4 ring-primary/30 overflow-hidden shadow-lg">
                <img :src="userProfile.profileImageUrl" :alt="userProfile.name" class="w-full h-full object-cover" />
              </div>
              <div v-else class="w-32 h-32 rounded-full bg-gradient-to-br from-primary to-secondary text-primary-content relative ring-4 ring-primary/30 shadow-lg">
                <span class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 text-5xl font-bold">
                  {{ userProfile.name.charAt(0).toUpperCase() }}
                </span>
              </div>
            </div>
            
            <!-- User Info -->
            <div class="flex-1 text-center lg:text-left">
              <h1 class="text-3xl font-bold mb-2">{{ userProfile.name }}</h1>
              <p class="text-lg text-base-content/70 mb-3">{{ userProfile.email }}</p>
              
              <div class="flex flex-wrap gap-3 justify-center lg:justify-start items-center mb-4">
                <PremiumBadge v-if="userProfile.is_premium" class="badge-lg h-10 px-4 text-base" />
                <div v-else class="badge badge-lg badge-neutral h-10 px-4 flex items-center text-base">
                  <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5.121 17.804A9 9 0 1112 21a9 9 0 01-6.879-3.196z" />
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                  </svg>
                  Basic korisnik
                </div>
                
                <div class="badge badge-outline h-10 flex items-center text-base">
                  <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 10l4.553-2.276A1 1 0 0121 8.618v6.764a1 1 0 01-1.447.894L15 14M5 18h8a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z" />
                  </svg>
                  {{ commonCounts.video(videos.length) }}
                </div>
              </div>
              
              <!-- Profile Details -->
              <div v-if="!userProfile.is_profile_private" class="space-y-2">
                <p v-if="userProfile.description" class="text-base-content/80">{{ userProfile.description }}</p>
                <div v-if="userProfile.gender || userProfile.birthDate" class="flex flex-wrap gap-4 text-sm text-base-content/70">
                  <span>
                    Dob:
                    <template v-if="calculateAge(userProfile.birthDate) !== null">
                      {{ calculateAge(userProfile.birthDate) }}
                    </template>
                    <template v-else>
                      -
                    </template>
                  </span>
                  <span v-if="userProfile.gender">Spol: {{ translateGender(userProfile.gender) }}</span>
                </div>
              </div>
              
              <!-- Private Profile Notice -->
              <div v-else class="alert alert-info">
                <svg xmlns="http://www.w3.org/2000/svg" class="stroke-current shrink-0 h-6 w-6" fill="none" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                <span>Ovaj profil je privatan.</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Videos Section -->
      <div v-if="!userProfile.is_profile_private">
        <div class="flex justify-between items-center mb-6">
          <h2 class="text-2xl font-bold">Video sadržaj</h2>
          <div class="text-sm text-base-content/60">
            {{ commonCounts.video(videos.length) }}
          </div>
        </div>

        <!-- Videos Grid -->
        <div v-if="videos.length > 0" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
          <div v-for="video in videos" :key="video.id" class="card bg-base-100 shadow-xl hover:shadow-2xl transition-all duration-300 hover:scale-105">
            <!-- Video Thumbnail -->
            <figure class="relative h-48 bg-base-300">
              <div v-if="video.muxPlaybackId" class="absolute inset-0">
                <img 
                  :src="`https://image.mux.com/${video.muxPlaybackId}/thumbnail.jpg?time=0`" 
                  :alt="video.title"
                  class="w-full h-full object-cover"
                />
                <div class="absolute inset-0 bg-gradient-to-t from-black/50 to-transparent opacity-0 hover:opacity-100 transition-opacity duration-300 flex items-center justify-center">
                  <button @click="playVideo(video)" class="btn btn-circle btn-primary" :disabled="video.isPremium && !userIsPremium">
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M14.828 14.828a4 4 0 01-5.656 0M9 10h1m4 0h1m-6 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                  </button>
                </div>
              </div>
              <div v-else class="absolute inset-0 flex items-center justify-center">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-16 w-16 text-base-content/30" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 10l4.553-2.276A1 1 0 0121 8.618v6.764a1 1 0 01-1.447.894L15 14M5 18h8a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z" />
                </svg>
              </div>
              
              <!-- Premium Badge -->
              <div v-if="video.isPremium" class="absolute top-3 left-3">
                <PremiumBadge />
              </div>
            </figure>

            <!-- Video Info -->
            <div class="card-body p-4">
              <h3 class="card-title text-lg mb-2 line-clamp-2">{{ video.title }}</h3>
              <p v-if="video.description" class="text-base-content/70 text-sm line-clamp-2 mb-3">{{ video.description }}</p>
              
              <!-- Video Stats -->
              <div class="flex items-center justify-between text-xs text-base-content/60 mb-4">
                <div class="flex items-center gap-4">
                  <span v-if="video.duration" class="flex items-center">
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-3 w-3 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                    {{ formatDuration(video.duration) }}
                  </span>
                  <span class="flex items-center">
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-3 w-3 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
                    </svg>
                    {{ video.likes || 0 }}
                  </span>
                </div>
                <span class="text-xs">{{ formatDate(video.createdAt) }}</span>
              </div>

              <!-- Action Buttons -->
              <div class="flex gap-2">
                <button
                  class="btn btn-primary btn-sm flex-1"
                  @click="playVideo(video)"
                  v-if="video.muxPlaybackId"
                  :disabled="video.isPremium && !userIsPremium"
                >
                  <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M14.828 14.828a4 4 0 01-5.656 0M9 10h1m4 0h1m-6 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                  Reproduciraj
                </button>
                
                <!-- Vote Buttons -->
                <div class="flex gap-1">
                  <button 
                    class="btn btn-sm" 
                    :class="video.userVote === 'like' ? 'btn-success' : 'btn-outline'"
                    @click="voteOnVideo(video.id, 'like')"
                    :disabled="!isAuthenticated || (video.isPremium && !userIsPremium)"
                  >
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M14 10h4.764a2 2 0 011.789 2.894l-3.5 7A2 2 0 0115.263 21h-4.017c-.163 0-.326-.02-.485-.06L7 20m7-10V5a2 2 0 00-2-2h-.095c-.5 0-.905.405-.905.905 0 .714-.211 1.412-.608 2.006L7 11v9m7-10h-2M7 20H5a2 2 0 01-2-2v-6a2 2 0 012-2h2.5" />
                    </svg>
                  </button>
                  <button 
                    class="btn btn-sm" 
                    :class="video.userVote === 'dislike' ? 'btn-error' : 'btn-outline'"
                    @click="voteOnVideo(video.id, 'dislike')"
                    :disabled="!isAuthenticated || (video.isPremium && !userIsPremium)"
                  >
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 14H5.236a2 2 0 01-1.789-2.894l3.5-7A2 2 0 018.736 3h4.018c.163 0 .326.02.485.06L17 4m-7 10v5a2 2 0 002 2h.095c.5 0 .905-.405.905-.905 0-.714.211-1.412.608-2.006L17 13V4m-7 10h2" />
                    </svg>
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Empty State -->
        <div v-else class="text-center py-16">
          <div class="card bg-base-100 shadow-xl max-w-md mx-auto">
            <div class="card-body text-center">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-24 w-24 mx-auto text-base-content/30 mb-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 10l4.553-2.276A1 1 0 0121 8.618v6.764a1 1 0 01-1.447.894L15 14M5 18h8a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z" />
              </svg>
              <h3 class="text-2xl font-bold mb-3">Nema videa</h3>
              <p class="text-base-content/70">Ovaj korisnik još nema uploadanih videa.</p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Video Player Modal -->
    <VideoPlayerModal 
      :visible="showPlayer"
      :playback-id="currentPlaybackId"
      @close="closePlayer"
    />

    <!-- Premium Modal -->
    <div v-if="showPremiumModal" class="fixed inset-0 bg-black/80 flex items-center justify-center z-50 p-4">
      <div class="modal-box text-center">
        <h3 class="font-bold text-lg text-primary">
          <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6 inline-block mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6.364-6.364l-1.414-1.414M2.32 12h2m15.36 0h2M12 2.32V4.32m7.07-1.95l-1.414 1.414M5.636 5.636L4.222 4.222m12.122 12.122l1.414 1.414M18.364 5.636l-1.414 1.414" />
          </svg>
          Premium Sadržaj
        </h3>
        <p class="py-4">Ovaj video je dostupan samo za premium korisnike. Nadogradite svoj račun za pristup ekskluzivnom sadržaju!</p>
        <div class="modal-action justify-center gap-4">
          <NuxtLink to="/subscription" class="btn btn-primary">Nadogradi na Premium</NuxtLink>
          <button @click="showPremiumModal = false" class="btn btn-outline">Zatvori</button>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute } from 'vue-router'
import { mapVideoApiToFrontend } from '~/utils/videoMapper'
import { mapUserApiToFrontend } from '~/utils/userMapper'
import { commonCounts } from '~/utils/grammar'
import VideoPlayerModal from '~/components/VideoPlayerModal.vue'
import PremiumBadge from '~/components/PremiumBadge.vue'
import { useAuth } from '~/composables/useAuth'
import { useRuntimeConfig } from '#app'

const route = useRoute()
const userProfile = ref<any>(null)
const videos = ref<any[]>([])
const loading = ref(true)
const error = ref('')
const showPlayer = ref(false)
const showPremiumModal = ref(false)
const currentPlaybackId = ref<string | null>(null)

const { isAuthenticated, userIsPremium } = useAuth()
const config = useRuntimeConfig()
const apiBaseUrl = config.public.apiBaseUrl

onMounted(async () => {
  await fetchUserProfile()
})

async function fetchUserProfile() {
  try {
    const identifier = route.params.identifier
    const token = localStorage.getItem('authToken')
    let fetchOptions = {}
    if (token) {
      fetchOptions = { headers: { 'Authorization': `Bearer ${token}` } }
    }
    
    const response = await fetch(`${apiBaseUrl}/api/videos/user/${identifier}`, fetchOptions)
    if (!response.ok) {
      throw new Error('Korisnik nije pronađen ili došlo je do greške pri dohvaćanju profila.')
    }
    const data = await response.json()
    userProfile.value = mapUserApiToFrontend(data.user)
    videos.value = data.videos.map(mapVideoApiToFrontend)
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Došlo je do greške.'
  } finally {
    loading.value = false
  }
}

function playVideo(video: any) {
  if (video.isPremium && !userIsPremium.value) {
    showPremiumModal.value = true
    return
  }

  if (video.muxPlaybackId) {
    currentPlaybackId.value = video.muxPlaybackId
    showPlayer.value = true
  }
}

function closePlayer() {
  showPlayer.value = false
  currentPlaybackId.value = null
}

async function voteOnVideo(videoId: string, voteType: 'like' | 'dislike') {
  if (!isAuthenticated.value) {
    alert('Morate biti prijavljeni da možete glasati.')
    return
  }
  
  try {
    const token = localStorage.getItem('authToken')
    const response = await fetch(`${apiBaseUrl}/api/videos/${videoId}/vote`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({ voteType })
    })
    
    if (response.ok) {
      const result = await response.json()
      // Ažuriraj video u listi
      const videoIndex = videos.value.findIndex(v => v.id === videoId)
      if (videoIndex !== -1) {
        videos.value[videoIndex].likes = result.likes
        videos.value[videoIndex].dislikes = result.dislikes
        videos.value[videoIndex].userVote = result.userVote
      }
    } else {
      const data = await response.json()
      alert(data.error || 'Greška pri glasanju.')
    }
  } catch (e) {
    alert('Došlo je do greške pri glasanju.')
  }
}

function formatDate(dateString: string) {
  if (!dateString) return 'Nepoznato'
  const date = new Date(dateString)
  return new Intl.DateTimeFormat('hr-HR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  }).format(date)
}

function formatDuration(seconds: number) {
  if (!seconds) return ''
  
  const hours = Math.floor(seconds / 3600)
  const minutes = Math.floor((seconds % 3600) / 60)
  const secs = seconds % 60
  
  if (hours > 0) {
    return `${hours}:${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`
  }
  return `${minutes}:${secs.toString().padStart(2, '0')}`
}

function translateGender(gender: string) {
  switch (gender) {
    case 'Male': return 'Muško';
    case 'Female': return 'Žensko';
    case 'Other': return 'Drugo';
    default: return gender || '';
  }
}

function calculateAge(birthDate: string): number | null {
  if (!birthDate) return null;
  const date = new Date(birthDate);
  if (isNaN(date.getTime())) return null;
  const today = new Date();
  let age = today.getFullYear() - date.getFullYear();
  const m = today.getMonth() - date.getMonth();
  if (m < 0 || (m === 0 && today.getDate() < date.getDate())) {
    age--;
  }
  return age;
}
</script>

<style scoped>
.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  line-clamp: 2;
  overflow: hidden;
}
</style> 