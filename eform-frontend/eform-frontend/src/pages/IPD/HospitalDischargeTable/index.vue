<template>
  <div
    id="HospitalDischargeTable-Page"
    class="main-content disable_ccp hospital_dischargeTable"
  >
    <!-- dua phieu GDSK cho NB -->
    <div v-if="$route.name === 'IpdHospitalDischargeTable'">
      <div :key="index" v-for="(form, index) in Forms" class="mb-10">
        <div v-if="form.Datas">
          <component
            v-if="form.Type in $options.components"
            :Form="form"
            v-bind:is="form.Type"
            :VisitId="VisitId"
          />
        </div>
      </div>
    </div>
    <h2 class="text-center cap mrb30 fw600">
      {{ __t("HospitalDischargeTable.bigTitle") }}
    </h2>
    <div class="content-body">
      <page-docter
        :condition="condition"
        :isNull="isNullDocter"
        @reloadIsNull="reloadIsNullDocter"
        :viewOnly="viewOnly"
        :viewOnly2="viewOnlyDoctor"
        :VisitId="VisitId"
      />
      <page-nurse
        :condition="condition"
        :isNull="isNullNurse"
        @reloadIsNull="reloadIsNullNurse"
        @reloadTimeUpdate="reloadTimeUpdate"
        :viewOnly="viewOnly"
        :viewOnly2="viewOnlyNurse"
        :visitStatus="visitStatus"
        :VisitId="VisitId"
      />
    </div>
    <div>
      <p>A03_046_050919_VE</p>
      <div v-if="!isNullDocter && condition && condition === 'Doctor'">
        <p>{{__t('Chỉnh sửa lần cuối bởi')}} <AdInfo :ad="updatedByDoctor" /> {{__t('lúc')}} {{updatedAtDoctor}}</p>
      </div>
      <div v-if="!isNullNurse && condition && condition === 'Nurse'">
        <p>{{__t('Chỉnh sửa lần cuối bởi')}} <AdInfo :ad="updatedByNurse" /> {{__t('lúc')}} {{updatedAtNurse}}</p>
      </div>
    </div>
  </div>
</template>
<script>
import $ from 'jquery'
import storage from '@/lib/storage'
import PageDocter from './PageDocter.vue'
import PageNurse from './PageNurse.vue'
import IPDDischargePreparationChecklist from '@/services/IPD/HospitalDischargeTable/GetProfileDoctor'
import IPDDischargePreparationChecklistNurse from '@/services/IPD/HospitalDischargeTable/GetProfileNurse'
import MedicalRecord from '@/services/MedicalRecord.js'
import PFEF from '@/pages/PatientAndFamilyEducation/View.vue'

export default {
  name: 'HospitalDischargeTable',
  props: ['VisitId', 'VisitType', 'viewOnly'],
  data () {
    return {
      isNullDocter: true,
      isNullNurse: true,
      updatedAtDoctor: '',
      updatedByDoctor: '',
      updatedAtNurse: '',
      updatedByNurse: '',
      visitStatus: '',
      viewOnlyDoctor: false,
      viewOnlyNurse: false,
      Forms: []
    }
  },
  mixins: [],
  components: {
    PageDocter, PageNurse, PFEF
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
    }
  },
  mounted () {
    $('.disable_ccp' + (storage.get('allowcopypaste') ? 'fc' : '')).bind('cut copy paste', e => {
      this.toastedError('Bạn không thể thực hiện thao tác này trên hệ thống!')
      return false
    })
    new MedicalRecord().find('IPD/' + this._VisitId).then(response => {
      this.Forms = response
    })
    this.getDataDocter()
  },
  methods: {
    getDataDocter () {
      new IPDDischargePreparationChecklist().hideErrorMsg()
        .find(this._VisitId)
        .then((res) => {
          if (res) {
            this.updatedAtDoctor = res.UpdatedAt
            this.updatedByDoctor = res.UpdatedBy
            this.viewOnlyDoctor = res.IsLocked
          }
          this.isNullDocter = false
        }).catch(e => {
          if (e.status === 404) {
            this.isNullDocter = true
          }
        })
      new IPDDischargePreparationChecklistNurse().hideErrorMsg()
        .find(this._VisitId)
        .then((res) => {
          if (res) {
            this.updatedAtNurse = res.UpdatedAt
            this.updatedByNurse = res.UpdatedBy
            this.visitStatus = res.VisitStatus.EnName
            this.viewOnlyNurse = res.IsLocked
          }
          this.isNullNurse = false
        }).catch(e => {
          if (e.status === 404) {
            this.isNullNurse = true
          }
        })
    },
    reloadIsNullDocter () {
      this.isNullDocter = false
    },
    reloadIsNullNurse () {
      this.isNullNurse = false
    },
    reloadTimeUpdate () {
      this.getDataDocter()
    }
  }
}
</script>
