<template>
  <span>
    <template v-if="unit === 'pCO2'">pCO<sub>2</sub></template>
    <template v-else-if="unit === 'pO2'">pO<sub>2</sub></template>
    <template v-else-if="unit === 'SO2'">SO<sub>2</sub></template>
    <template v-else-if="unit === 'HCO3'">HCO<sub>3</sub></template>
    <template v-else-if="unit === 'TCO2'">TCO<sub>2</sub></template>
    <template v-else-if="unit === 'tempC'"><sup>o</sup>C</template>
    <template v-else>{{unit}}</template>
  </span>
</template>
<script>
export default {
  name: 'Unit',
  props: ['unit'],
  mixins: [],
  data () {
    return {
    }
  },
  computed: {
  },
  created () {
  },
  methods: {
  }
}
</script>
