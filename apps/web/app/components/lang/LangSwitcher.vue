<script setup lang="ts">
import { useLang } from '~/composables/useLang'

const { lang, setLang } = useLang()
</script>

<template>
  <div class="flex items-center gap-0.5 font-mono text-[0.68rem]">
    <button
      class="rounded px-1.5 py-0.5 transition-colors"
      :class="
        lang === 'ru' ? 'text-foreground' : 'hover:text-foreground/70 text-[var(--text-secondary)]'
      "
      @click="setLang('ru')"
    >
      RU
    </button>
    <span class="text-[var(--text-secondary)]/30">|</span>
    <button
      class="rounded px-1.5 py-0.5 transition-colors"
      :class="
        lang === 'en' ? 'text-foreground' : 'hover:text-foreground/70 text-[var(--text-secondary)]'
      "
      @click="setLang('en')"
    >
      EN
    </button>
  </div>
</template>
