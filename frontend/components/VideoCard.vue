<template>
  <div class="card bg-base-100 shadow-xl hover:shadow-2xl transition-all duration-300 hover:scale-105">
    <!-- Video Thumbnail -->
    <figure class="relative h-48 bg-base-300">
      <!-- Loading State -->
      <div v-if="video.uploadStatus === 'pending'" class="absolute inset-0 flex items-center justify-center bg-base-200/80">
        <div class="text-center">
          <div class="loading loading-spinner loading-lg mb-3"></div>
          <p class="text-sm font-medium">Obrađuje se...</p>
          <p class="text-xs text-base-content/60 mt-1">Molimo pričekajte</p>
        </div>
      </div>
      
      <!-- Video Thumbnail -->
      <div v-else-if="video.muxPlaybackId" class="absolute inset-0">
        <img 
          :src="`https://image.mux.com/${video.muxPlaybackId}/thumbnail.jpg?time=0`" 
          :alt="video.title"
          class="w-full h-full object-cover"
        />
        <div class="absolute inset-0 bg-gradient-to-t from-black/50 to-transparent opacity-0 hover:opacity-100 transition-opacity duration-300 flex items-center justify-center">
          <button @click="$emit('play', video)" class="btn btn-circle btn-primary" :disabled="mode !== 'dashboard' && video.isPremium && !userIsPremium">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M14.828 14.828a4 4 0 01-5.656 0M9 10h1m4 0h1m-6 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </button>
        </div>
      </div>
      
      <!-- No Thumbnail -->
      <div v-else class="absolute inset-0 flex items-center justify-center">
        <svg xmlns="http://www.w3.org/2000/svg" class="h-16 w-16 text-base-content/30" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 10l4.553-2.276A1 1 0 0121 8.618v6.764a1 1 0 01-1.447.894L15 14M5 18h8a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z" />
        </svg>
      </div>
      
      <!-- Status Badge (Dashboard mode only) -->
      <div v-if="mode === 'dashboard' && video.uploadStatus" class="absolute top-3 right-3">
        <div class="badge" :class="getStatusBadgeClass(video.uploadStatus)">
          {{ getStatusText(video.uploadStatus) }}
        </div>
      </div>
      
      <!-- Premium Badge (Public mode only) -->
      <div v-if="mode === 'public' && video.isPremium" class="absolute top-3 left-3">
        <PremiumBadge />
      </div>
    </figure>

    <!-- Video Info -->
    <div class="card-body p-4">
      <h3 class="card-title text-lg mb-2 line-clamp-2">{{ video.title }}</h3>
      <p v-if="video.description" class="text-base-content/70 text-sm line-clamp-2 mb-3">{{ video.description }}</p>
      
      <!-- Premium/Privatni badgeovi (samo za dashboard mod) -->
      <div v-if="mode === 'dashboard'" class="flex gap-2 mb-2">
        <PremiumBadge v-if="video.isPremium" />
        <div v-if="video.isPrivate" class="badge badge-secondary">Privatno</div>
      </div>
      
      <!-- Informacije o autoru (samo za javni mod) -->
      <div v-if="mode === 'public' && video.user" class="flex items-center gap-2 mb-3">
        <UserAvatar :user="video.user" size="xs" />
        <span class="text-sm font-medium">{{ video.user.name || 'Nepoznati autor' }}</span>
      </div>
      
      <!-- Statistika videa -->
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

      <!-- Akcijski gumbovi -->
      <div class="flex flex-col gap-2">
        <div class="flex gap-2">
          <!-- Gumb za reprodukciju -->
          <button
            class="btn btn-primary btn-sm flex-1"
            @click="$emit('play', video)"
            v-if="video.muxPlaybackId"
            :disabled="mode !== 'dashboard' && video.isPremium && !userIsPremium"
          >
            <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M14.828 14.828a4 4 0 01-5.656 0M9 10h1m4 0h1m-6 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            Reproduciraj
          </button>
          
          <!-- Gumbovi za glasanje (samo za javni mod) -->
          <div v-if="mode === 'public'" class="flex gap-1">
            <button 
              class="btn btn-sm" 
              :class="video.userVote === 'like' ? 'btn-success' : 'btn-outline'"
              @click="$emit('vote', video.id, 'like')"
              :disabled="!isAuthenticated || (video.isPremium && !userIsPremium)"
            >
              <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M14 10h4.764a2 2 0 011.789 2.894l-3.5 7A2 2 0 0115.263 21h-4.017c-.163 0-.326-.02-.485-.06L7 20m7-10V5a2 2 0 00-2-2h-.095c-.5 0-.905.405-.905.905 0 .714-.211 1.412-.608 2.006L7 11v9m7-10h-2M7 20H5a2 2 0 01-2-2v-6a2 2 0 012-2h2.5" />
              </svg>
            </button>
            <button 
              class="btn btn-sm" 
              :class="video.userVote === 'dislike' ? 'btn-error' : 'btn-outline'"
              @click="$emit('vote', video.id, 'dislike')"
              :disabled="!isAuthenticated || (video.isPremium && !userIsPremium)"
            >
              <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 14H5.236a2 2 0 01-1.789-2.894l3.5-7A2 2 0 018.736 3h4.018c.163 0 .326.02.485.06L17 4m-7 10v5a2 2 0 002 2h.095c.5 0 .905-.405-.905-.905 0-.714.211-1.412-.608 2.006L17 13V4m-7 10h2" />
              </svg>
            </button>
          </div>
          
          <!-- Akcije za dashboard -->
          <button v-if="mode === 'dashboard'" class="btn btn-outline btn-sm flex-1" @click="$emit('copy-link', video)">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z" />
            </svg>
            Link
          </button>
        </div>
        
        <!-- Akcije za dashboard (Uredi, Obriši) -->
        <div v-if="mode === 'dashboard'" class="flex justify-end gap-2">
          <button class="btn btn-outline btn-sm" @click="$emit('edit', video)">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16.862 3.487a2.25 2.25 0 113.182 3.182l-9.193 9.193a2.25 2.25 0 01-.878.547l-3.372 1.124a.75.75 0 01-.948-.948l1.124-3.372a2.25 2.25 0 01.547-.878l9.193-9.193z" />
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19.5 12v6.75A2.25 2.25 0 0117.25 21H6.75A2.25 2.25 0 014.5 18.75V7.5A2.25 2.25 0 016.75 5.25h6.75" />
            </svg>
          </button>
          <button class="btn btn-error btn-sm" @click="$emit('delete', video)">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
            </svg>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import { formatDate, formatDuration } from '@/utils/dateUtils'
import UserAvatar from './UserAvatar.vue'
import PremiumBadge from './PremiumBadge.vue'

interface Video {
  id: string
  title: string
  description?: string
  muxPlaybackId?: string
  duration?: number
  likes?: number
  dislikes?: number
  userVote?: string
  isPremium?: boolean
  isPrivate?: boolean
  uploadStatus?: string
  createdAt: string
  user?: {
    name?: string
    profileImageUrl?: string
  }
}

const props = defineProps<{
  video: Video
  mode: 'public' | 'dashboard' | 'user-profile'
  userIsPremium?: boolean
  isAuthenticated?: boolean
}>()

defineEmits<{
  play: [video: Video]
  vote: [videoId: string, voteType: 'like' | 'dislike']
  'copy-link': [video: Video]
  edit: [video: Video]
  delete: [video: Video]
}>()

function getStatusBadgeClass(status: string) {
  switch (status) {
    case 'ready': return 'badge-success'
    case 'pending': return 'badge-warning'
    case 'failed': return 'badge-error'
    default: return 'badge-ghost'
  }
}

function getStatusText(status: string) {
  switch (status) {
    case 'ready': return 'Spreman'
    case 'pending': return 'Obrađuje se'
    case 'failed': return 'Neuspjeh'
    default: return 'Nepoznato'
  }
}
</script>

<style scoped>
.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  line-clamp: 2;
}
</style> 