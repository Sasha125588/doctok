<script setup lang="ts">
import SearchCatalogRow from './SearchCatalogRow.vue'
import SearchCourseCard from './SearchCourseCard.vue'
import SearchTopicCard from './SearchTopicCard.vue'
import { mockCategories, mockMiniCourses } from '~/lib/searchMockData'
</script>

<template>
  <div class="catalog">
    <SearchCatalogRow
      title="★ міні-курси"
      action-label="всі курси →"
    >
      <SearchCourseCard
        v-for="course in mockMiniCourses"
        :key="course.name"
        :course="course"
      />
    </SearchCatalogRow>

    <SearchCatalogRow
      v-for="category in mockCategories"
      :key="category.name"
      :title="category.name"
      action-label="показати всі →"
    >
      <SearchTopicCard
        v-for="topic in category.topics"
        :key="topic.name"
        :topic="topic"
      />
    </SearchCatalogRow>
  </div>
</template>

<style scoped>
.catalog {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 18px;
  overflow-y: auto;
  min-height: 0;
}
.catalog::-webkit-scrollbar {
  width: 2px;
}
.catalog::-webkit-scrollbar-thumb {
  background: #1a1a1a;
}
</style>
