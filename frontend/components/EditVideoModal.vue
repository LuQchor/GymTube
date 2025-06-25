<template>
  <div v-if="visible" class="modal modal-open">
    <div class="modal-box w-full max-w-lg">
      <h3 class="font-bold text-lg mb-4">Uredi video</h3>
      <form @submit.prevent="onSave">
        <div class="mb-4">
          <label class="label">Naslov</label>
          <input type="text" class="input input-bordered w-full" v-model="localTitle" required maxlength="100" />
        </div>
        <div class="mb-4">
          <label class="label">Opis</label>
          <textarea class="textarea textarea-bordered w-full" v-model="localDescription" rows="4" maxlength="1000"></textarea>
        </div>
        <div class="mb-4" v-if="canEditPremium">
          <label class="label cursor-pointer">
            <span class="label-text">Premium video</span>
            <input type="checkbox" class="toggle toggle-primary" v-model="localIsPremium" />
          </label>
        </div>
        <div class="mb-4">
          <label class="label cursor-pointer">
            <span class="label-text">Privatni video</span>
            <input type="checkbox" class="toggle toggle-secondary" v-model="localIsPrivate" />
          </label>
        </div>
        <div class="flex gap-2 justify-end mt-6">
          <button type="button" class="btn btn-outline" @click="$emit('cancel')">Odustani</button>
          <button type="submit" class="btn btn-primary" :disabled="saving">
            <span v-if="saving" class="loading loading-spinner loading-xs"></span>
            Spremi
          </button>
          <button type="button" class="btn btn-error ml-auto" @click="onDelete" :disabled="deleting">
            <span v-if="deleting" class="loading loading-spinner loading-xs"></span>
            <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
            </svg>
            Obriši
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue'

const props = defineProps<{
  visible: boolean,
  video: any,
  canEditPremium: boolean,
  saving?: boolean,
  deleting?: boolean
}>()
const emit = defineEmits(['save', 'cancel', 'delete'])

const localTitle = ref('')
const localDescription = ref('')
const localIsPremium = ref(false)
const localIsPrivate = ref(false)

watch(() => props.video, (video) => {
  if (video) {
    localTitle.value = video.title || ''
    localDescription.value = video.description || ''
    localIsPremium.value = !!video.isPremium
    localIsPrivate.value = !!video.isPrivate
  } else {
    localTitle.value = ''
    localDescription.value = ''
    localIsPremium.value = false
    localIsPrivate.value = false
  }
}, { immediate: true })

function onSave() {
  emit('save', {
    title: localTitle.value.trim(),
    description: localDescription.value.trim(),
    isPremium: localIsPremium.value,
    isPrivate: localIsPrivate.value
  })
}

async function onDelete() {
  emit('delete')
}
</script> 