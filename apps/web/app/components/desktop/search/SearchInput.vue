<script setup lang="ts">
const model = defineModel<string>({ required: true })
</script>

<template>
  <label class="search-box">
    <span class="sr-only">Пошук</span>
    <Icon
      name="lucide:search"
      class="icon"
    />
    <input
      v-model="model"
      type="text"
      class="input"
      placeholder="Пошук тем, методів, API..."
      spellcheck="false"
    />
  </label>
</template>

<style scoped>
.search-box {
  display: flex;
  align-items: center;
  gap: 10px;
  background: var(--dt-panel-bg);
  border: 1px solid #161616;
  border-radius: 6px;
  padding: 0 16px;
  flex-shrink: 0;
  transition:
    border-color 0.15s ease,
    background 0.15s ease;
}
.search-box:focus-within {
  border-color: #1f3224;
  background: #080808;
}
.icon {
  width: 13px;
  height: 13px;
  color: #333;
  flex-shrink: 0;
}
.input {
  width: 100%;
  background: none;
  border: none;
  outline: none;
  padding: 12px 0;
  font-family: var(--font-mono);
  font-size: 13px;
  color: #c8c8c0;
  caret-color: var(--kind-example);
}
.input::placeholder {
  color: #1a1a1a;
}
</style>
