<template>
  <div class="main-content" id="iam-page">
    <br/>
    <h1 class="title-page">{{ __t('Chi tiết bệnh án') }} </h1>
    <br/>
    <div class="flex" style="cursor: pointer;">
      <div class="col-md-1" :class="getActiveMenu(item.Code) ? 'backColor' : ''"  @click="filterMenu(item.Code)" style="height:74px;padding: 8px;border: 0.5px solid #efefef;border-radius: 8px;" v-for="item in groupMenu" :key="item.value">{{__t(item.title)}}</div>
    </div>
    <div class="mb-10 mt-5">
      <input type="text" class="form-control" v-model="search" :placeholder="__t('Gõ để tìm tên phiếu...')"/>
    </div>
    <div v-if="loading" class="text-center"><v-loading/></div>
    <div v-else :key="index" v-for="(form, index) in finedForm" class="mb-10">
      <div v-if="form.Datas && form.Datas.length">
        <component v-if="'DischargeAgaintsMedicalAdvice' in $options.components" :Form="form" v-bind:is="form.Type" :version="form.Datas[0].Version" :VisitType="'IPD'" :VisitId="VisitId || $route.params.Id"/>
      </div>
    </div>
    <!-- <div v-else class="text-center"><v-loading/></div> -->
    <!-- <retail-service-patient :EdId="this.$route.params.Id" :readonlyview="readonlyview"/>
    <er0 :EdId="this.$route.params.Id" :readonlyview="readonlyview"/>
    <patient-progress-notes :VisitId="this.$route.params.Id" :Type="'ED'" :readonlyview="true"/>
    <di :EdId="this.$route.params.Id" :readonlyview="readonlyview"/> -->
  </div>
</template>
<script>
import MixinMasterData from '@/mixins/masterdata.js'
import MedicalRecordServices from '@/services/MedicalRecord.js'
// eslint-disable-next-line import/no-duplicates
import InitialAssessmentForAudult from '@/pages/IPD/InitialAssessment/ForAdultView.vue'
import InitialAssessmentForFrailElderly from '@/pages/IPD/InitialAssessment/ForFrailElderlyView.vue'
import InitialAssessmentForChemotherapy from '@/pages/IPD/InitialAssessment/ForChemotherapyView.vue'
import PFEF from '@/pages/PatientAndFamilyEducation/View.vue'
import PatientProgressNote from '@/pages/IPD/_ProgressNote/View.vue'
import CardiacArrestRecord from '@/pages/CardiacArrestRecord/View.vue'
// eslint-disable-next-line import/no-duplicates
import FallRiskAssessmentForAdult from '@/pages/IPD/FallRiskAssessment/AdultView.vue'
// eslint-disable-next-line import/no-duplicates
import FallRiskAssessmentInPediatricInpatients from '@/pages/IPD/FallRiskAssessment/ChildView.vue'
import GlamorganScaleForScreening from '@/pages/IPD/GlamorganPressure/ChildView.vue'
import FallRiskAssessmentForObstetric from '@/pages/IPD/FallRiskAssessment/ObstetricView.vue'
import PlanOfCare from '@/pages/IPD/PlanOfCare/View.vue'
// eslint-disable-next-line import/no-duplicates
import MedicalRecord from '@/pages/IPD/MedicalRecord/View.vue'
import GuggingSwallowingScreen from '@/pages/IPD/GuggingSwallowingScreen/View.vue'
// import ProcedureSummary from '@/pages/ProcedureSummary/View.vue'
import SurgeryAndProcedureSummary from '@/pages/ProcedureSummary/View.vue'
import JointConsultationGroupMinutes from '@/pages/JointConsultationGroupMinutes/View.vue'
import JointConsultationForApprovalOfSurgery from '@/pages/JointConsultationForApprovalOfSurgery/View.vue'
import TakeCareOfPatientsWithCovid19 from '@/pages/IPD/TakeCareOfPatientsWithCovid19/View.vue'
import CareNote from '@/pages/IPD/TakeCareNote/View.vue'
import PhysicianNote from '@/pages/IPD/PhysicianNote/View.vue'
import PHC from '@/pages/ED/PreOperativeProcedureHandoverChecklists/View.vue'
import SurgicalProcedureSafetyChecklist from '@/pages/OPD/SurgicalProcedureSafetyChecklist/View.vue'
// import ConsultationDrugWithAnAsteriskMark from '@/pages/ED/ConsultationDrugWithAnAsteriskMark/View.vue'
import ConsultationDrugWithAnAsteriskMark from '@/pages/IPD/ConsultationDrugWithAnAsteriskMarkUpForm/View.vue'
import PatientOwnMedicationsChart from '@/pages/PatientOwnMedicationsChart/View.vue'
import BloodRequestSupplyAndConfirmation from '@/pages/ED/BloodRequestSupplyAndConfirmation/View.vue'
import BloodTransfusionChecklist from '@/pages/ED/BloodTransfusionChecklist/View.vue'
import ExternalTransportationAssessment from '@/pages/IPD/ExternalTransportationAssessment/View.vue'
import DischargePreparationChecklist from '@/pages/IPD/HospitalDischargeTable/View.vue'
import DischargeAgaintsMedicalAdvice from '@/pages/IPD/ConfirmDischargeWithoutDirect/View.vue'
import MortalityReport from '@/pages/IPD/MortalityReport/View.vue'
import SurgeryCertificate from '@/pages/IPD/SurgeryCertificate/View.vue'
import VitalSignsForAdult from '@/pages/IPD/VitalSigns/AdultVitalSigns/View.vue'
import VitalSignsForPregnantWoman from '@/pages/IPD/VitalSigns/PregnantVitalSigns/View.vue'
import NeonatalObservationChart from '@/pages/IPD/VitalSigns/NeonatalVitalSigns/View.vue'
import VitalSignFor1To3 from '@/pages/IPD/VitalSigns/Pediatric1ToUnder3Month/View.vue'
import VitalSignFor1To4 from '@/pages/IPD/VitalSigns/Pediatric1ToUnder4Old/View.vue'
import VitalSignFor3To12 from '@/pages/IPD/VitalSigns/Pediatric3To12Month/View.vue'
import VitalSignFor4To12 from '@/pages/IPD/VitalSigns/Pediatric4To12Old/View.vue'
import VitalSignForOver12 from '@/pages/IPD/VitalSigns/PediatricOver12Old/View.vue'
import BradenScale from '@/pages/IPD/BradenScoreboard/View.vue'
import SumaryOf15DayTreatment from '@/pages/IPD/SummaryOf15DayTreatment/View.vue'
// eslint-disable-next-line import/no-duplicates
import MonitoringSheetForPatientsWith from '@/pages/IPD/MonitorSheetForPatient/View.vue'
import MedicationHistoryForm from '@/pages/IPD/MedicationHistoryForm/View.vue'
import MedicationHistoryFormPediatricPatient from '@/pages/IPD/MedicationHistoryFormForNewBorn/View.vue'
// eslint-disable-next-line import/no-duplicates
import MedicalRecordPediatric from '@/pages/IPD/MedicalRecord/View.vue'
// eslint-disable-next-line import/no-duplicates
import MedicalRecordNeonatal from '@/pages/IPD/MedicalRecord/View.vue'
import MedicalReport from '@/pages/IPD/MedicalReport/View.vue'
import DischargeMedicalReport from '@/pages/IPD/DischargeMedicalReport/View.vue'
import ReferralLetter from '@/pages/IPD/ReferralLetter/View.vue'
import InjuryCertificate from '@/pages/IPD/InjuryCertificate/View.vue'
import TransferLetter from '@/pages/IPD/TransferLetter/View.vue'
import HandOverCheckList from '@/pages/IPD/HandOverCheckList/View2.vue'
// eslint-disable-next-line import/no-duplicates
import InitialAssessmentForPediatricInPatient from '@/pages/IPD/InitialAssessment/ForAdultView.vue'
import InitialAssessmentForNeonatal from '@/pages/IPD/InitialAssessment/ForNewBornView.vue'
import ThrombosisRiskFactorAssessment from '@/pages/IPD/ThrombosisRiskFactorAssessment/View.vue'
import ThrombosisRiskFactorAssessmentFor from '@/pages/IPD/ThrombosisRiskFactorAssessmentForGeneralSurgeryPatients/View.vue'
import InitialAssessmentForNeonatalMaternity from '@/pages/IPD/InitialAssessment/ForNeonatalMaternity/NeonatalMaternityView.vue'
// eslint-disable-next-line camelcase
import ForUterineLife2Hours_Obstetrics from '@/pages/IPD/AssessmentOfInfantStatusDuringTheFirst2HoursOfExtrauterineLife/View.vue'
// eslint-disable-next-line camelcase, import/no-duplicates
import ForNeonatalMaternity_Obstetrics from '@/pages/IPD/InitialAssessment/ForNeonatalMaternity/NeonatalMaternityViewV2.vue'
// eslint-disable-next-line import/no-duplicates
import ForNeonatalMaternity from '@/pages/IPD/InitialAssessment/ForNeonatalMaternity/NeonatalMaternityViewV2.vue'
// eslint-disable-next-line import/no-duplicates
import MedicalRecordObstetrics from '@/pages/IPD/MedicalRecord/View.vue'
import HighlyRestrictedAntimicrobialConsult from '@/pages/HighlyRestrictedAntimicrobialConsult/View.vue'
import InitialAssessmentForPregnantWomen from '@/pages/IPD/InitialAssessment/ForWomenInLabourView.vue'
// eslint-disable-next-line import/no-duplicates
import MonitorSheetForPatient from '@/pages/IPD/MonitorSheetForPatient/View.vue'
// eslint-disable-next-line import/no-duplicates
import MedicalRecordGynecological from '@/pages/IPD/MedicalRecord/View.vue'
// eslint-disable-next-line import/no-duplicates
import MedicalRecordOncology from '@/pages/IPD/MedicalRecord/View.vue'
// eslint-disable-next-line import/no-duplicates
import A01_039_050919_V from '@/pages/IPD/MedicalRecord/View.vue'
// eslint-disable-next-line import/no-duplicates
import A01_040_050919_V from '@/pages/IPD/MedicalRecord/View.vue'
// eslint-disable-next-line import/no-duplicates
import TheSurgicalMedicalRecord from '@/pages/IPD/MedicalRecord/View.vue'
// eslint-disable-next-line import/no-duplicates
import MedicalRecordEye from '@/pages/IPD/MedicalRecord/View.vue'
// eslint-disable-next-line import/no-duplicates
import CardiovascularForm from '@/pages/IPD/MedicalRecord/View.vue'
import VerbalOrderForm from '@/pages/ED/EmergencyTriageRecord/OrderTableView.vue'
import CoronaryIntervention from '@/pages/IPD/CoronaryIntervention/View.vue'
import Discharged from '@/pages/IPD/DischargeCertificate/View.vue'
import CoordinatingWithThePatient from '@/pages/IPD/CoordinatingWithThePatient/View.vue'
import TrickSummary from '@/pages/TrickSummary/View.vue'
import IPDScaleForAssessmentOfSuicideIntent from '@/pages/IPD/ScaleForAssessmentOfSuicideIntent/View.vue'
import ArterialBloodGasTest from '@/pages/ED/PointOfCareTesting/ArterialBloodGasTestView.vue'
import ChemicalBiologyTest from '@/pages/ED/PointOfCareTesting/ChemicalBiologyTestView.vue'
import StandingOrder from '@/pages/StandingOrder/View.vue'
import PROMForCoronaryDisease from '@/pages/IPD/PromForCoronaryDisease/View.vue'
import PROMForheartFailure from '@/pages/IPD/PROMForHeartFailure/View.vue'
import ConsentForOperationOrrProcedure from '@/pages/ConsentForOperation/View.vue'
import RequestResuscitation from '@/pages/IPD/Consent/View.vue'
import ConsentForTransfusionOfBlood from '@/pages/ConsentForTransfusionOfBloodAndBloodDerivedProducts/View.vue'
import HIVTestingConsent from '@/pages/HIVTestingConsent/View.vue'
import CartridgeCelite from '@/pages/IPD/ACT/View.vue'
import A03_165_061222_V from '@/pages/IPD/BloodGasAnalysisEG7/View.vue'
import IPD_A02_052_050919_V from '@/pages/IPD/AskThePatientBeforeSurgery/View.vue'
import IPD_A01_159_050919_VE from '@/pages/CommitmentPaperToTakeThePlacenta/View.vue'
import CatridgeKaolinACT from '@/pages/IPD/CartridgeKaolinACT/View.vue'
import UploadImage from '@/pages/IPD/UploadImage/View.vue'
import ProcedureSafetyChecklist from '@/pages/FormLienKhoa/A02_053_OR_201022_V/View.vue'
export default {
  name: 'IPD-Detail',
  mixins: [MixinMasterData],
  props: ['VisitId'],
  components: {
    UploadImage,
    ForNeonatalMaternity,
    ForNeonatalMaternity_Obstetrics,
    ConsentForTransfusionOfBlood,
    StandingOrder,
    IPDScaleForAssessmentOfSuicideIntent,
    TrickSummary,
    BloodRequestSupplyAndConfirmation,
    BloodTransfusionChecklist,
    PatientOwnMedicationsChart,
    InitialAssessmentForAudult,
    InitialAssessmentForFrailElderly,
    InitialAssessmentForChemotherapy,
    PFEF,
    PatientProgressNote,
    CardiacArrestRecord,
    FallRiskAssessmentForAdult,
    FallRiskAssessmentInPediatricInpatients,
    FallRiskAssessmentForObstetric,
    PlanOfCare,
    MedicalRecord,
    GuggingSwallowingScreen,
    // ProcedureSummary,
    SurgeryAndProcedureSummary,
    JointConsultationForApprovalOfSurgery,
    JointConsultationGroupMinutes,
    TakeCareOfPatientsWithCovid19,
    CareNote,
    PhysicianNote,
    PHC,
    ConsultationDrugWithAnAsteriskMark,
    SurgicalProcedureSafetyChecklist,
    ExternalTransportationAssessment,
    DischargePreparationChecklist,
    DischargeAgaintsMedicalAdvice,
    MortalityReport,
    VitalSignsForAdult,
    VitalSignsForPregnantWoman,
    NeonatalObservationChart,
    VitalSignFor1To3,
    VitalSignFor1To4,
    VitalSignFor3To12,
    VitalSignFor4To12,
    VitalSignForOver12,
    BradenScale,
    SurgeryCertificate,
    SumaryOf15DayTreatment,
    MedicationHistoryForm,
    MedicalRecordPediatric,
    DischargeMedicalReport,
    ReferralLetter,
    InjuryCertificate,
    TransferLetter,
    MedicalReport,
    HandOverCheckList,
    InitialAssessmentForPediatricInPatient,
    MedicalRecordNeonatal,
    InitialAssessmentForNeonatal,
    ThrombosisRiskFactorAssessment,
    ThrombosisRiskFactorAssessmentFor,
    InitialAssessmentForNeonatalMaternity,
    ForUterineLife2Hours_Obstetrics,
    MedicalRecordObstetrics,
    HighlyRestrictedAntimicrobialConsult,
    InitialAssessmentForPregnantWomen,
    MonitorSheetForPatient,
    MedicalRecordGynecological,
    MonitoringSheetForPatientsWith,
    TheSurgicalMedicalRecord,
    VerbalOrderForm,
    MedicalRecordOncology,
    CoronaryIntervention,
    A01_039_050919_V,
    A01_040_050919_V,
    MedicalRecordEye,
    GlamorganScaleForScreening,
    Discharged,
    CoordinatingWithThePatient,
    MedicationHistoryFormPediatricPatient,
    ArterialBloodGasTest,
    ChemicalBiologyTest,
    PROMForCoronaryDisease,
    PROMForheartFailure,
    CardiovascularForm,
    ConsentForOperationOrrProcedure,
    RequestResuscitation,
    HIVTestingConsent,
    A03_165_061222_V,
    CartridgeCelite,
    IPD_A02_052_050919_V,
    IPD_A01_159_050919_VE,
    CatridgeKaolinACT,
    ProcedureSafetyChecklist
  },
  data () {
    return {
      itemActive: 0,
      isClick: false,
      activeMenu: 55,
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
      Forms: [
      ],
      tabActive: ['TC'],
      readonlyview: false,
      search: '',
      loading: true
    }
  },
  mounted () {
    this.readonlyview = true
    this.getData()
    // if (this.$store.state.account.Position.includes('Nurse') || this.$store.state.account.Position.includes('Doctor')) {
    //   this.activeMenu = 2
    // }
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
    filterMenu (code) {
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
      new MedicalRecordServices().find('IPD/' + (this.VisitId || this.$route.params.Id)).then(response => {
        this.Forms = response
        this.Forms.map(e => {
          if (e.Type === 'CardiovascularForm') {
            e.ViName = 'Bệnh án tim mạch'
            e.EnName = 'Cardiology medical record for inpatient'
          }
        })
        this.Forms = this.Forms.sort(function (a, b) {
          let c = new Date(a.UpdatedAt)
          let d = new Date(b.UpdatedAt)
          return d - c
        })
        this.formResponse = response
        this.getMasterDatas({Form: 'NHOMBIEUMAU'}, () => {
        })
        this.loading = false
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
