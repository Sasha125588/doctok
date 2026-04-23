<script setup lang="ts">
import DesktopSidePanel from './DesktopSidePanel.vue'
import { useFeedView } from '~/composables/useFeedView'
import { useNotes } from '~/composables/useNotes'

const props = defineProps<{ activePostId: number }>()

const { activePanel } = useFeedView()
const { get, set } = useNotes()

const text = ref('')

watch(
  () => [props.activePostId, activePanel.value === 'notes'] as const,
  ([id, open]) => {
    if (open && id != null) text.value = get(id)
  },
  { immediate: true }
)

const save = () => set(props.activePostId, text.value)

const isOpen = computed(() => activePanel.value === 'notes')
</script>

<template>
  <DesktopSidePanel
    :open="isOpen"
    title="note"
  >
    <div class="area">
      <textarea
        v-model="text"
        class="textarea"
        placeholder="Твої нотатки до цього поста..."
      />
      <div class="hint">// зберігається локально</div>
      <button
        class="save"
        @click="save"
      >
        зберегти →
      </button>
    </div>
  </DesktopSidePanel>
</template>

<style scoped>
.area {
  flex: 1;
  padding: 12px;
  display: flex;
  flex-direction: column;
  gap: 8px;
  overflow: hidden;
}
.textarea {
  background: var(--dt-panel-bg);
  border: 1px solid var(--dt-panel-border);
  border-radius: 4px;
  outline: none;
  font-family: var(--font-mono);
  font-size: 10px;
  color: #8ab8e8;
  padding: 7px 9px;
  flex: 1;
  min-height: 120px;
  caret-color: var(--kind-example);
  resize: none;
}
.textarea::placeholder {
  color: var(--dt-text-quaternary);
}
.hint {
  font-family: var(--font-mono);
  font-size: 10px;
  color: var(--dt-text-quaternary);
  line-height: 1.6;
}
.save {
  font-family: var(--font-mono);
  font-size: 9px;
  padding: 5px 12px;
  border-radius: 2px;
  cursor: pointer;
  letter-spacing: 0.06em;
  color: var(--chart-4);
  background: #1a0f35;
  border: 1px solid color-mix(in oklab, var(--chart-4) 20%, transparent);
  transition: all 0.15s;
}
.save:hover {
  background: #22143f;
}
</style>
