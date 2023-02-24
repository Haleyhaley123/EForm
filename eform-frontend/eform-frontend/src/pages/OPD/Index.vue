<template>
  <v-layout>
    <section class="content-header">
      <div class="container">
        <ol class="breadcrumb">
          <li v-if="this.$store.state.account.Specialty"><router-link :to="{path: '/OPD'}"><i class="fa fa-dashboard"></i> {{__label(this.$store.state.account.Specialty)}}</router-link></li>
          <li v-if="this.$route.meta.localKey" class="active">{{ $t(this.$route.meta.localKey) }}</li>
        </ol>
      </div>
    </section>
    <div class="container">
      <section class="content">
        <opd-component v-if="this.$route.params.Id" :OpdId="this.$route.params.Id" @onChange="getDataDone"/>
        <transition name="page" mode="out-in" v-if="hasData">
          <router-view/>
        </transition>
        <div v-else class="text-center"><v-loading/></div>
      </section>
    </div>
  </v-layout>
</template>
<script>
/* ============
 * Home Index Page
 * ============
 *
 * The home index page.
 */

import VLayout from '@/layouts/Default.vue'
import OpdComponent from '@/components/OPD/VIEW_OPD.vue'
// import NProgress from 'nprogress'
export default {
  /**
   * The name of the page.
   */
  name: 'OPD-PAGE',

  /**
   * The components that the page can use.
   */
  components: {
    VLayout, OpdComponent
  },
  data () {
    return {
      hasData: false
    }
  },
  created () {
    this.hasData = !this.$route.params.Id
  },
  mounted () {
  },
  methods: {
    getDataDone (data) {
      console.log(data)
      setTimeout(() => {
        this.hasData = true
      }, 300)
    }
  }
}
</script>
