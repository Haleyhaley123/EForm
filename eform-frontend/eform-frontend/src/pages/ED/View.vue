<template>
  <div class="main-content" id="iam-page">
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
      <div v-if="(form.Datas && form.Datas.length)">
        <component v-if="form.Type in $options.components" :Form="form" v-bind:is="form.Type" :VisitType="'ED'" :VisitId="VisitId || $route.params.Id"/>
        <!-- <div v-else>No FOUND {{form.Type}} {{form.ViName}}</div> -->
      </div>
      <!-- <div v-else>No DATA {{form.Type}} {{form.ViName}}</div> -->
    </div>
    <!-- <retail-service-patient :EdId="this.$route.params.Id" :readonlyview="readonlyview"/>
    <er0 :EdId="this.$route.params.Id" :readonlyview="readonlyview"/>
    <patient-progress-notes :VisitId="this.$route.params.Id" :Type="'ED'" :readonlyview="true"/>
    <di :EdId="this.$route.params.Id" :readonlyview="readonlyview"/> -->
  </div>
</template>
<script>
/* ============
 * Home Index Page
 * ============
 *
 * The home index page.
 */
import MixinMasterData from '@/mixins/masterdata.js'
import MedicalRecord from '@/services/MedicalRecord.js'
import EmergencyTriageRecord from '@/pages/ED/EmergencyTriageRecord/Triage.vue'
import FallRiskScreening from '@/pages/ED/EmergencyTriageRecord/FallRiskScreening/View.vue'
import ER0 from '@/pages/ED/EmergencyRecord/View.vue'
import AssessmentForRetailServicePatient from '@/pages/RetailService/AssessmentView.vue'
import DI0 from '@/pages/ED/DischargeInformation/View.vue'
import StandingOrder from '@/pages/StandingOrder/View.vue'
import PatientProgressNote from '@/pages/ED/ProgressNote/View.vue'
import StandingOrderForRetailService from '@/pages/RetailService/StandingOrderView.vue'
import AmbulanceRunReport from '@/pages/ED/AmbulanceRunReport/View.vue'
import ArterialBloodGasTest from '@/pages/ED/PointOfCareTesting/ArterialBloodGasTestView.vue'
import ChemicalBiologyTest from '@/pages/ED/PointOfCareTesting/ChemicalBiologyTestView.vue'
import PFEF from '@/pages/PatientAndFamilyEducation/View.vue'
import ExternalTransportationAssessment from '@/pages/IPD/ExternalTransportationAssessment/View.vue'
import SkinTestResult from '@/pages/ED/SkinTestResult/View.vue'
// eslint-disable-next-line import/no-duplicates
import MortalityReport from '@/pages/ED/MortalityReport/View.vue'
// eslint-disable-next-line import/no-duplicates
import MortalityReportV2 from '@/pages/ED/MortalityReport/View.vue'
import BloodRequestSupplyAndConfirmation from '@/pages/ED/BloodRequestSupplyAndConfirmation/View.vue'
import BloodTransfusionChecklist from '@/pages/ED/BloodTransfusionChecklist/View.vue'
import CardiacArrestRecord from '@/pages/CardiacArrestRecord/View.vue'
import PatientOwnMedicationsChart from '@/pages/PatientOwnMedicationsChart/View.vue'
import ConsultationDrugWithAnAsteriskMark from '@/pages/IPD/ConsultationDrugWithAnAsteriskMarkUpForm/View.vue'
import JointConsultationForApprovalOfSurgery from '@/pages/JointConsultationForApprovalOfSurgery/View.vue'
import VerbalOrderForm from '@/pages/ED/EmergencyTriageRecord/OrderTableView.vue'
import ComplexOutpatientCaseSummary from '@/pages/ComplexOutpatientCaseSummary/View.vue'
import JointConsultationGroupMinutes from '@/pages/JointConsultationGroupMinutes/View.vue'
import MCA from '@/pages/ED/MonitoringChartAndHandoverForms/View.vue'
import PHC from '@/pages/ED/PreOperativeProcedureHandoverChecklists/View.vue'
import HandOverCheckList from '@/pages/ED/HandOverCheckList/ViewOnly.vue'
import SelfHarmRiskScreeningTool from '@/pages/ED/SelfHarmRiskScreeningToolUpForm/View.vue'
import SurgeryAndProcedureSummary from '@/pages/ProcedureSummary/View.vue'
// import ProcedureSummary from '@/pages/ProcedureSummary/View.vue'
import CareNote from '@/pages/TakeCareNote/View.vue'
import PhysicianNote from '@/pages/PhysicianNote/View.vue'
import HighlyRestrictedAntimicrobialConsult from '@/pages/HighlyRestrictedAntimicrobialConsult/View.vue'
import MedicalReport from '@/pages/ED/MedicalReport/View.vue'
import DischargeMedicalReport from '@/pages/ED/DischargeMedicalReport/View.vue'
import ReferralLetter from '@/pages/ED/ReferralLetter/View.vue'
import EMCO from '@/pages/ED/EmergencyConfirmation/View.vue'
import Discharged from '@/pages/ED/DischargeCertificate/View.vue'
import InjuryCertificate from '@/pages/ED/InjuryCertificate/View.vue'
import TransferLetter from '@/pages/ED/TranferLetter/View.vue'
import TrickSummary from '@/pages/TrickSummary/View.vue'
import ConsentForOperationOrrProcedure from '@/pages/ConsentForOperation/View.vue'
import ConsentForTransfusionOfBlood from '@/pages/ConsentForTransfusionOfBloodAndBloodDerivedProducts/View.vue'
import RequestResuscitation from '@/pages/IPD/Consent/View.vue'
import HIVTestingConsent from '@/pages/HIVTestingConsent/View.vue'
import CartridgeCelite from '@/pages/IPD/ACT/View.vue'
import CatridgeKaolinACT from '@/pages/IPD/CartridgeKaolinACT/View.vue'
import A03_165_061222_V from '@/pages/IPD/BloodGasAnalysisEG7/View.vue'
import UploadImage from '@/pages/IPD/UploadImage/View.vue'
import ProcedureSafetyChecklist from '@/pages/FormLienKhoa/A02_053_OR_201022_V/View.vue'

export default {
  /**
   * The name of the page.
   */
  name: 'ED-View',
  props: ['VisitId'],
  mixins: [MixinMasterData],
  data () {
    return {
      Forms: [],
      activeMenu: 0,
      formResponse: [],
      tabActive: ['TC'],
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
      readonlyview: false,
      search: '',
      loading: true
    }
  },
  /**
   * The components that the page can use.
   */
  components: {
    UploadImage,
    ConsentForTransfusionOfBlood,
    TrickSummary,
    EmergencyTriageRecord,
    FallRiskScreening,
    ER0,
    AssessmentForRetailServicePatient,
    DI0,
    StandingOrder,
    PatientProgressNote,
    StandingOrderForRetailService,
    AmbulanceRunReport,
    ArterialBloodGasTest,
    ChemicalBiologyTest,
    PFEF,
    ExternalTransportationAssessment,
    SkinTestResult,
    MortalityReport,
    MortalityReportV2,
    BloodRequestSupplyAndConfirmation,
    BloodTransfusionChecklist,
    CardiacArrestRecord,
    PatientOwnMedicationsChart,
    ConsultationDrugWithAnAsteriskMark,
    JointConsultationForApprovalOfSurgery,
    VerbalOrderForm,
    ComplexOutpatientCaseSummary,
    JointConsultationGroupMinutes,
    MCA,
    PHC,
    HandOverCheckList,
    // ProcedureSummary,
    SurgeryAndProcedureSummary,
    HighlyRestrictedAntimicrobialConsult,
    SelfHarmRiskScreeningTool,
    CareNote,
    PhysicianNote,
    MedicalReport,
    DischargeMedicalReport,
    ReferralLetter,
    EMCO,
    InjuryCertificate,
    TransferLetter,
    Discharged,
    ConsentForOperationOrrProcedure,
    RequestResuscitation,
    HIVTestingConsent,
    CartridgeCelite,
    CatridgeKaolinACT,
    A03_165_061222_V,
    ProcedureSafetyChecklist
  },
  mounted () {
    this.readonlyview = this.$router.currentRoute.name === 'EDRECORD'
    // if (this.$store.state.account.Position.includes('Nurse') || this.$store.state.account.Position.includes('Doctor')) {
    //   this.activeMenu = 2
    // }
    this.getData()
  },
  computed: {
    finedForm: function () {
      this.Forms.filter(item => {
        item.UpdatedAt = this.$options.filters.formatDateWithoutSecon(item.UpdatedAt)
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
    },
    getData () {
      new MedicalRecord().find('ED/' + (this.VisitId || this.$route.params.Id)).then(response => {
        this.Forms = response
        this.Forms = this.Forms.sort(function (a, b) {
          let c = new Date(a.UpdatedAt)
          let d = new Date(b.UpdatedAt)
          return d - c
        })
        this.formResponse = response
        this.loading = false
        this.getMasterDatas({Form: 'NHOMBIEUMAU'}, () => {
        })
      })
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
