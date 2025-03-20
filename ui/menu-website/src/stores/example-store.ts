import { defineStore, acceptHMRUpdate } from 'pinia';
import { ref, computed } from 'vue';

export const useCounterStore = defineStore('counter', () => {
  const counter = ref(0);
  const doubleCount = computed(() => counter.value * 2);

  const increment = () => {
    counter.value++;
  };

  return { counter, doubleCount, increment };
});

if (import.meta.hot) {
  import.meta.hot.accept(acceptHMRUpdate(useCounterStore, import.meta.hot));
}
