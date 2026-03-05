<script setup lang="ts">
import { feedListInfiniteOptions } from '../client/@tanstack/vue-query.gen'
import { useInfiniteQuery } from '@tanstack/vue-query'

const response = useInfiniteQuery({
  ...feedListInfiniteOptions({
    query: { lang: 'ru' },
  }),
  getNextPageParam: (lastPage) => lastPage.nextCursor,
})
</script>

<template>
  <div>
    <NuxtRouteAnnouncer />
    <NuxtWelcome />
    <div
      style="
        display: flex;
        flex-direction: column;
        gap: 10px;
        border: 1px solid #ccc;
        padding: 10px;
        border-radius: 5px;
      "
      v-for="post in response.data.value?.pages[0]?.items"
      :key="post.id"
    >
      <h2>{{ post.title }}</h2>
      <p>{{ post.body }}</p>
      <p>{{ post.position }}</p>
      <p>{{ post.likeCount }}</p>
      <p>{{ post.dislikeCount }}</p>
      <p>{{ post.commentCount }}</p>
      <p>{{ post.topicSlug }}</p>
      <p>{{ post.topicTitle }}</p>
      <p>{{ post.myVote }}</p>
      <p>{{ post.popularity }}</p>
      <p>{{ post.id }}</p>
      <p>{{ post.kind }}</p>
    </div>
  </div>
</template>
