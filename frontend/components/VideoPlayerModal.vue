<template>
  <div v-if="visible" class="fixed inset-0 bg-black/80 flex items-center justify-center z-50 p-4" @click.self="close">
    <div class="bg-base-200 rounded-lg shadow-2xl max-w-4xl w-full max-h-[90vh] overflow-hidden">
      <div class="p-4 border-b border-base-300 flex justify-between items-center">
        <h3 class="text-lg font-semibold">Video Player</h3>
        <button @click="close" class="btn btn-ghost btn-sm btn-circle">
          <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>
      <div class="p-4">
        <VideoPlayer v-if="playbackId" :playback-id="playbackId" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import VideoPlayer from '~/components/VideoPlayer.vue'

const props = defineProps<{
  visible: boolean
  playbackId: string | null
}>()

const emit = defineEmits(['close'])

function close() {
  emit('close')
}
</script> 