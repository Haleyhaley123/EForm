<template>
  <div id="BloodGasAnalysisEG7-Page" class="main-content disable_ccp">
    <h2 class="text-center cap mrb30 fw600">{{__t('Xét nghiệm tại chỗ - Khí máu và điện giải EG7+')}}</h2>
    <template v-if="loaded">
      <div class="v-tab v-tab-2" v-if="List.length">
        <template>
          <ul class="nav nav-tabs flex-containerx no-border" role="tablist">
            <li :data="item" :key="index" v-for="(item ,index) in List" class="text-center flex-containerx">
              <router-link class="no-border" style="min-height: 71px;" :to="{name:`${_VisitType}BloodGasAnalysisEG7Item`, params: { Id: _VisitId, Item: item.Id }}">
                <div class="box-info-item">
                  <div>{{__t('Phiếu')}} {{index + 1}}</div>
                  <div><ad-Info v-if="item.CreatedBy" :ad="item.CreatedBy" /></div>
                  <div>{{item.CreatedAt | formatDateWithoutSecon}}</div>
                </div>
              </router-link>
            </li>
            <li class="text-center flex-container no-border" v-if="!readonly">
              <a  @click="confirmPopup()" class="add-btn-active no-border" style="height: 71px; padding-top: 13px;">
                <div class="box-info-item">
                  <div><i class="fa fa-plus" aria-hidden="true"></i></div>
                  <div>{{__t('Thêm phiếu')}}</div>
                </div>
              </a>
            </li>
          </ul>
          <div class="tab-content">
            <transition name="page" mode="out-in">
              <router-view :IsLocked="IsLocked"/>
            </transition>
          </div>
        </template>
      </div>
      <div class="text-center" v-else>
        <NewForm @createForm="confirmPopup" :IsLocked="IsLocked" :TitleForm="''"/>
        <!-- <h4>{{__t('Chưa có Xét nghiệm tại chỗ - Khí máu và điện giải EG7+')}}</h4>
        <button class="btn v-yellow-btn" v-if="!readonly" @click="confirmPopup()">{{__t('Tạo mới')}}</button> -->
      </div>
    </template>
    <div v-else class="loading-text"><v-loading/></div>
  </div>
</template>
<script>
import $ from 'jquery'
import storage from '@/lib/storage'
import FormApi from '@/services/FormAPI.js'
import Item from './Item.vue'

export default {
  name: 'BloodGasAnalysisEG7',
  props: ['viewOnly', 'VisitId', 'VisitType'],
  data () {
    return {
      loaded: false,
      List: [],
      IsLocked: false,
      APIService: null
    }
  },
  components: {
    Item
  },
  mounted () {
    $('.disable_ccp' + (storage.get('allowcopypaste') ? 'fc' : '')).bind('cut copy paste', e => {
      this.toastedError('Bạn không thể thực hiện thao tác này trên hệ thống!')
      return false
    })
    this.APIService = new FormApi({
      VisitType: this._VisitType,
      VisitId: this._VisitId,
      FormCode: 'A03_165_061222_V'
    })
    this.getData()
  },
  computed: {
    condition () {
      let name = ''
      if (this.$store.state.account.Position.includes('Nurse')) {
        name = 'Nurse'
      }
      if (this.$store.state.account.Position.includes('Doctor')) {
        name = 'Doctor'
      }
      return name
    },
    readonly () {
      return this.IsLocked || this.viewOnly
    }
  },
  methods: {
    // lấy dữ liệu visitID
    getData () {
      this.APIService.GetList().then(res => {
        // lấy thông tin khoá 24h
        this.IsLocked = res.IsLock24h
        if (res.FORM_NOT_FOUND && res.FORM_NOT_FOUND.ViMessage === 'Form không tồn tại') {
        } else {
          // lấy danh sách
          this.List = res.Datas
          if (res.Datas && res.Datas.length > 0 && !this.$route.params.Item) {
            // đẩy bắn sang items mới nhất
            this.$router.push(
              {
                name: `${this._VisitType}BloodGasAnalysisEG7Item`,
                params: { Id: this._VisitId, Item: res.Datas[res.Datas.length - 1].Id }
              }
            )
          }
        }
        this.loaded = true
      }).catch((e) => {
        this.IsLocked = e.data.IsLock24h
        this.loaded = true
      })
    },
    // xác nhận tạo mới
    confirmPopup () {
      this.$modal.show('dialog', {
        clickToClose: false,
        title: this.$t('Xét nghiệm tại chỗ - Khí máu và điện giải EG7+'),
        text: this.$t('Tạo mới Xét nghiệm tại chỗ - Khí máu và điện giải EG7+\nMáy: iSTAT'),
        class: 'v-dialog v-dialog-warning',
        buttons: [
          {
            title: this.$t('Tôi xác nhận'),
            class: 'btn',
            handler: () => {
              this.$modal.hide('dialog')
              this.handleCreate()
            }
          },
          {
            title: this.$t('Bỏ qua'),
            class: 'btn btn-warning',
            handler: () => {
              this.$modal.hide('dialog')
            }
          }
        ]
      })
    },
    // tạo mới form
    handleCreate () {
      this.APIService.CreateForm({}).then(res => {
        this.toastedSuccess()
        this.$router.push(
          {
            name: `${this._VisitType}BloodGasAnalysisEG7Item`,
            params: { Id: this._VisitId, Item: res.Id }
          }
        )
      }).catch(e => {
        console.log(e)
      })
    }
  }
}
</script>
<style scoped>
  .flex-containerx {
    margin-right: 0px!important;
  }
</style>
