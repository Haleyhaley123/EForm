<template>
    <div class="main-content" id="OPD-VIEW">
      <br/>
      <h1 class="title-page">{{ __t('Chi tiết bệnh án') }}</h1>
      <br/>
      <div class="flex" style="cursor: pointer;">
      <div class="col-md-1" :class="getActiveMenu(item.Code) ? 'backColor' : ''"  @click="filterMenu(item.value, item.Code, activeMenu)" style="height:74px;padding: 8px;border: 0.5px solid #efefef;border-radius: 8px;" v-for="item in groupMenu" :key="item.value">{{__t(item.title)}}</div>
    </div>
      <div class="mb-10">
        <input type="text" class="form-control" v-model="search" :placeholder="__t('Gõ để tìm tên phiếu...')"/>
      </div>
      <div v-if="loading" class="text-center"><v-loading/></div>
      <div v-else :key="index" v-for="(form, index) in finedForm" class="mb-10">
        <div v-if="form.Datas && form.Datas.length">
          <!-- {{form.Type}} -->
          <component v-if="form.Type in $options.components" :Form="form" v-bind:is="form.Type" :VisitId="VisitId || $route.params.Id" :VisitType="'OPD'"/>
        </div>
      </div>
      <!-- <h1 class="text-center">{{__t('general.thong_tin_benh_nhan')}}</h1>
      <for-short-term :OpdId="OpdId || $route.params.Id" :readonlyview="readonlyview"/>
      <for-on-going :OpdId="OpdId || $route.params.Id" :readonlyview="readonlyview"/>
      <telehealth :OpdId="$route.params.Id" :readonlyview="readonlyview"/>
      <outpatient-examination-note :OpdId="OpdId || $route.params.Id" :readonlyview="readonlyview"/>
      <div class="form-content-2 no-bsorder no-bg">
        <div class="row">
          <div class="col-md-9">
            <div class="form-group">
              <button class="btn pull-left btn-back v-white-btn" type="button" @click="back()">{{__t('btn.quay_lai')}}</button>
            </div>
          </div>
          <div class="col-md-3" v-if="!readonlyview">
            <div class="form-group">
              <button class="btn btn-block v-yellow-btn" type="button" @click="$router.push({name: 'OPDMedicalReport', params: {Id: $route.params.Id}})">Báo cáo y tế</button>
            </div>
          </div>
        </div>
      </div> -->
    </div>
</template>
<script>
/* ============
 * Add cusotmer Page
 * ============
 *
 * The home index page.
 */
import MixinMasterData from '@/mixins/masterdata.js'
import MixinForm from '@/mixins/form.js'
import MedicalRecord from '@/services/MedicalRecord.js'
import InitialAssessmentForShortTerm from '@/components/OPD/ForShortTerm.vue'
import InitialAssessmentForOnGoing from '@/components/OPD/ForOnGoing.vue'
// import FallRiskScreening from '@/components/OPD/FallRiskScreen.vue'
import FallRiskScreening from '@/pages/OPD/InitialAssessment/FallRiskScreening/View.vue'
import InitialAssessmentForTelehealth from '@/components/OPD/Telehealth.vue'
import OutpatientExaminationNote from '@/components/OPD/OutpatientExaminationNote.vue'
import StandingOrder from '@/pages/StandingOrder/View.vue'
import ComplexOutpatientCaseSummary from '@/pages/ComplexOutpatientCaseSummary/View.vue'
import HandOverCheckList from '@/pages/OPD/HandOverCheckList/ViewOnly.vue'
import CardiacArrestRecord from '@/pages/CardiacArrestRecord/View.vue'
import JointConsultationGroupMinutes from '@/pages/JointConsultationGroupMinutes/View.vue'
import PatientProgressNote from '@/pages/OPD/ProgressNote/View.vue'
import SurgicalProcedureSafetyChecklist from '@/pages/OPD/SurgicalProcedureSafetyChecklist/View.vue'
import OPDPYCKCKHHTP from '@/pages/OPD/CKHoHapTienPhau/View.vue'
// import ProcedureSummary from '@/pages/ProcedureSummary/View.vue'
import SurgeryAndProcedureSummary from '@/pages/ProcedureSummary/View.vue'
import PHC from '@/pages/ED/PreOperativeProcedureHandoverChecklists/View.vue'
import SkinTestResult from '@/pages/ED/SkinTestResult/View.vue'
import StandingOrderForRetailService from '@/pages/RetailService/StandingOrderView.vue'
import AssessmentForRetailServicePatient from '@/pages/RetailService/AssessmentView.vue'
import MedicalReport from '@/pages/OPD/MedicalReport/View.vue'
import ReferralLetter from '@/pages/OPD/ReferralLetter/View.vue'
import TransferLetter from '@/pages/OPD/TransferLetter/View.vue'
import NCCNBROV1 from '@/pages/OPD/NCCNBROV1/View.vue'
import CBE from '@/pages/OPD/ClinicalBreastExamNote/View.vue'
import CoordinatingWithThePatient from '@/pages/OPD/CoordinatingWithThePatient/View.vue'
import PatientInformationTheRiskAssessmentForCancer from '@/pages/OPD/BreastCancerRiskAssessment/View.vue'
import GENBRCA from '@/pages/OPD/GENBRCA1/View.vue'
import TrickSummary from '@/pages/TrickSummary/View.vue'
import PFEF from '@/pages/PatientAndFamilyEducation/View.vue'
import DiseasesCertification from '@/pages/OPD/DiseasesCertification/View.vue'
import PROMForCoronaryDisease from '@/pages/IPD/PromForCoronaryDisease/View.vue'
import PROMForheartFailure from '@/pages/IPD/PROMForHeartFailure/View.vue'
import ConsentForOperationOrrProcedure from '@/pages/ConsentForOperation/View.vue'
import HIVTestingConsent from '@/pages/HIVTestingConsent/View.vue'
import JointConsultationForApprovalOfSurgery from '@/pages/JointConsultationForApprovalOfSurgery/View.vue'
import PregnancyFollowUpRecord from '@/pages/OPD/PregnancyFollowUpRecord/View.vue'
import CatridgeKaolinACT from '@/pages/IPD/CartridgeKaolinACT/View.vue'
import OPD_A01_204_030320_VE from '@/pages/OPD/RequestForPreSurgeryCardiologyConsultation/View.vue'
import PreAnesthesiaConsultation from '@/pages/OPD/PreAnesthesiaConsultation/View.vue'
import UploadImage from '@/pages/IPD/UploadImage/View.vue'
import ProcedureSafetyChecklist from '@/pages/FormLienKhoa/A02_053_OR_201022_V/View.vue'

export default {
  /**
   * The name of the page.
   */
  name: 'ViewPreAnesthesiaConsultation',
  props: ['VisitId', 'checkAnesthesia'],
  data () {
    return {
      activeMenu: 0,
      formResponse: [],
      groupMenu: [
        {
          title: this.$t('Tất cả'),
          value: 0,
          Code: 'TC'
        },
        {
          title: this.$t('Hành chính, Cam kết'),
          value: 1,
          Code: 'HCCK'
        },
        {
          title: this.$t('Bệnh án hội chẩn'),
          value: 2,
          Code: 'BAHC'
        },
        {
          title: this.$t('Y lệnh'),
          value: 3,
          Code: 'YL'
        },
        {
          title: this.$t('Y lệnh khác'),
          value: 4,
          Code: 'YLK'
        },
        {
          title: this.$t('Theo dõi điều dưỡng'),
          value: 5,
          Code: 'TDDD'
        },
        {
          title: this.$t('Phẫu thuật, Thủ thuật'),
          value: 6,
          Code: 'PTTT'
        },
        {
          title: this.$t('Kết quả xét nghiệm'),
          value: 7,
          Code: 'KQXN'
        },
        {
          title: this.$t('Kết quả CĐHA'),
          value: 8,
          Code: 'KQCDHA'
        },
        {
          title: this.$t('Ra viện'),
          value: 9,
          Code: 'RAVIEN'
        },
        {
          title: this.$t('Khác'),
          value: 10,
          Code: 'KHAC'
        },
        {
          title: this.$t('Biểu mẫu không thuộc HSBA'),
          value: 11,
          Code: 'BMKTHSBA'
        }],
      data: {
      },
      OpdId: null,
      readonlyview: false,
      Forms: [],
      tabActive: ['TC'],
      search: '',
      loading: true,
      IsAnesthesia: false
    }
  },
  mixins: [MixinForm, MixinMasterData],
  /**
   * The components that the page can use.
   */
  components: {
    UploadImage,
    PreAnesthesiaConsultation,
    TrickSummary,
    GENBRCA,
    InitialAssessmentForShortTerm,
    InitialAssessmentForOnGoing,
    OutpatientExaminationNote,
    InitialAssessmentForTelehealth,
    FallRiskScreening,
    StandingOrder,
    ComplexOutpatientCaseSummary,
    HandOverCheckList,
    CardiacArrestRecord,
    JointConsultationGroupMinutes,
    PatientProgressNote,
    SurgicalProcedureSafetyChecklist,
    // ProcedureSummary,
    SurgeryAndProcedureSummary,
    PHC,
    SkinTestResult,
    StandingOrderForRetailService,
    AssessmentForRetailServicePatient,
    MedicalReport,
    ReferralLetter,
    TransferLetter,
    NCCNBROV1,
    CoordinatingWithThePatient,
    CBE,
    PatientInformationTheRiskAssessmentForCancer,
    PFEF,
    DiseasesCertification,
    PROMForCoronaryDisease,
    PROMForheartFailure,
    ConsentForOperationOrrProcedure,
    HIVTestingConsent,
    JointConsultationForApprovalOfSurgery,
    PregnancyFollowUpRecord,
    CatridgeKaolinACT,
    OPDPYCKCKHHTP,
    OPD_A01_204_030320_VE,
    ProcedureSafetyChecklist
  },
  mounted () {
    this.IsAnesthesia = !this.checkAnesthesia ? false : !!this.$store.state.account.IsAnesthesia
    console.log('visitid', this.VisitId, this.$route.params.Id)
    this.readonlyview = this.$router.currentRoute.name === 'OPDRECORD'
    this.getData()
    // if (this.$store.state.account.Position.includes('Nurse') || this.$store.state.account.Position.includes('Doctor')) {
    //   this.activeMenu = 2
    // }
  },
  computed: {
    finedForm: function () {
      this.Forms.filter(item => {
        item.UpdatedAt = this.$options.filters.formatDateWithoutSecon(item.UpdatedAt)
        console.log('1', this.$options.filters.formatDateWithoutSecon(item.UpdatedAt))
      })
      if (!this.search) return this.Forms
      return this.Forms.filter(item => {
        if (this.$i18n.locale === 'vi') {
          return this.mapingStr(this.xoaDau(this.search), this.xoaDau(item.ViName))
        } else {
          return this.mapingStr(this.xoaDau(this.search), this.xoaDau(item.EnName))
        }
      })
    }
  },
  methods: {
    getActiveMenu (code) {
      if (this.tabActive.filter(name => name === code).length > 0) {
        return true
      } else {
        return false
      }
    },
    filterMenu (i, code, activeMenu) {
      if (this.tabActive.filter(name => name === code).length > 0) {
        this.tabActive = this.tabActive.filter(name => name !== code)
      } else {
        if (code === 'TC') {
          this.tabActive = ['TC']
        } else {
          this.tabActive.push(code)
          this.tabActive = this.tabActive.filter(name => name !== 'TC')
        }
      }
      if (this.tabActive.length > 0) {
        if (this.tabActive.filter(name => name.includes('TC')).length > 0) {
          this.Forms = this.formResponse
        } else {
          var list1 = []
          for (let i = 0; i < this.tabActive.length; i++) {
            var list = this.MASTERDATA[this.tabActive[i]].Items
            list1 = list1.concat(list)
          }
          var arr = this.formResponse.filter((el) => {
            return list1.some((f) => {
              return f.Code === el.Type
            })
          })
          this.Forms = arr
        }
      } else {
        this.Forms = []
      }
      if (this.IsAnesthesia) {
        this.Forms = this.Forms.filter(item => item.Type !== 'OutpatientExaminationNote' && item.Type !== 'MedicalReport')
      }
    },
    getData () {
      new MedicalRecord().find('OPD/' + (this.VisitId || this.$route.params.Id)).then(response => {
        if (this.IsAnesthesia) {
          this.formResponse = response.filter(item => item.Type !== 'OutpatientExaminationNote' && item.Type !== 'MedicalReport')
          this.Forms = response.filter(item => item.Type !== 'OutpatientExaminationNote' && item.Type !== 'MedicalReport')
        } else {
          this.formResponse = response
          this.Forms = response
        }
        // this.Forms = response
        this.Forms = this.Forms.sort(function (a, b) {
          let c = new Date(a.UpdatedAt)
          let d = new Date(b.UpdatedAt)
          return d - c
        })
        this.formResponse = response
        this.getMasterDatas({Form: 'NHOMBIEUMAU'}, () => {
        })
      })
      this.loading = false
    }
  }
}
</script>
<style scoped>
   .backColor {
    background-color: #337ab7;
    color: #fff;
  }
</style>
