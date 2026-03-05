<script setup lang="ts">
import { feedListInfiniteOptions } from '../client/@tanstack/vue-query.gen'
import { useInfiniteQuery } from '@tanstack/vue-query'

const response = useInfiniteQuery({
  ...feedListInfiniteOptions({
    query: {
      lang: 'ru',
    },
  }),
  getNextPageParam: (lastPage) => lastPage.nextCursor,
})

console.log('@data', response.data.value?.pages[0]?.items[0])
</script>

<template>
  <div>
    <NuxtRouteAnnouncer />
    <NuxtWelcome />
    <pre>{{ response.data.value?.pages[0]?.items[0] }}</pre>
  </div>
</template>
