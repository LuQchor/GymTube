<template>
  <div class="avatar">
    <div v-if="user?.profileImageUrl" :class="avatarClasses">
      <img 
        :src="user.profileImageUrl" 
        :alt="user.name || 'User'"
        class="w-full h-full object-cover"
      />
    </div>
    <div v-else :class="[avatarClasses, 'bg-gradient-to-br from-primary to-secondary text-primary-content relative']">
      <span :class="[textClasses, 'absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 font-bold']">
        {{ userInitial }}
      </span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface User {
  name?: string
  profileImageUrl?: string
}

const props = defineProps<{
  user?: User | null
  size?: 'xs' | 'sm' | 'md' | 'lg' | 'xl' | '2xl' | '3xl'
  showRing?: boolean
}>()

const userInitial = computed(() => {
  return props.user?.name?.charAt(0).toUpperCase() || 'U'
})

const avatarClasses = computed(() => {
  const baseClasses = 'rounded-full overflow-hidden shadow-lg'
  const ringClasses = props.showRing ? 'ring-4 ring-primary/30' : ''
  
  const sizeClasses = {
    xs: 'w-6 h-6',
    sm: 'w-8 h-8', 
    md: 'w-12 h-12',
    lg: 'w-16 h-16',
    xl: 'w-20 h-20',
    '2xl': 'w-24 h-24',
    '3xl': 'w-32 h-32'
  }
  
  return `${baseClasses} ${sizeClasses[props.size || 'md']} ${ringClasses}`
})

const textClasses = computed(() => {
  const sizeClasses = {
    xs: 'text-xs',
    sm: 'text-sm',
    md: 'text-lg',
    lg: 'text-xl',
    xl: 'text-2xl',
    '2xl': 'text-3xl',
    '3xl': 'text-5xl'
  }
  
  return sizeClasses[props.size || 'md']
})
</script> 