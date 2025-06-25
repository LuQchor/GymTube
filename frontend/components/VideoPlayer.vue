<template>
  <div class="w-full max-w-2xl mx-auto">
    <video
      ref="videoRef"
      class="video-js vjs-default-skin vjs-big-play-centered rounded-lg shadow-lg"
      controls
      preload="auto"
      :poster="posterUrl"
      :data-setup="'{}'"
      style="width: 100%; height: 360px;"
    ></video>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount, watch } from 'vue'
import videojs from 'video.js'
import type Player from 'video.js/dist/types/player'
import 'video.js/dist/video-js.css'
import VideoPlayer from '~/components/VideoPlayer.vue'

const props = defineProps<{
  playbackId: string
  posterUrl?: string
}>()

const videoRef = ref<HTMLVideoElement | null>(null)
let player: Player | null = null

const muxUrl = `https://stream.mux.com/${props.playbackId}.m3u8`

const showPlayer = ref(false)
const currentPlaybackId = ref<string | null>(null)

onMounted(() => {
  if (videoRef.value) {
    player = videojs(videoRef.value, {
      sources: [
        {
          src: muxUrl,
          type: 'application/x-mpegURL',
        },
      ],
      controls: true,
      responsive: true,
      fluid: true,
      aspectRatio: '16:9',
      poster: props.posterUrl || undefined,
    })
  }
})

onBeforeUnmount(() => {
  if (player) {
    player.dispose()
    player = null
  }
})

// Ako se promijeni playbackId, ponovno učitaj izvor
watch(() => props.playbackId, (newId) => {
  if (player && newId) {
    player.src({ src: `https://stream.mux.com/${newId}.m3u8`, type: 'application/x-mpegURL' })
    player.load()
    player.play()
  }
})

function playVideo(video: any) {
  if (video.muxPlaybackId) {
    currentPlaybackId.value = video.muxPlaybackId
    showPlayer.value = true
  }
}
function closePlayer() {
  showPlayer.value = false
  currentPlaybackId.value = null
}
</script>