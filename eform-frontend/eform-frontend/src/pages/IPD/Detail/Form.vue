<template>
  <div class="main-content">
    <div :key="index" v-for="(form, index) in $options.components" class="mb-10">
      <component v-if="form.Type === Type" :Form="form" v-bind:is="form.Type" :VisitType="'IPD'" :VisitId="VisitId"/>
    </div>
  </div>
</template>
<script>
// eslint-disable-next-line import/no-duplicates
import InitialAssessmentForAudult from '@/pages/IPD/InitialAssessment/ForAdultView.vue'
import InitialAssessmentForFrailElderly from '@/pages/IPD/InitialAssessment/ForFrailElderlyView.vue'
import InitialAssessmentForChemotherapy from '@/pages/IPD/InitialAssessment/ForChemotherapyView.vue'
import PFEF from '@/pages/PatientAndFamilyEducation/View.vue'
import PatientProgressNote from '@/pages/ProgressNote/View.vue'
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
import ProcedureSummary from '@/pages/ProcedureSummary/View.vue'
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
import VerbalOrderForm from '@/pages/ED/EmergencyTriageRecord/OrderTableView.vue'
import CoronaryIntervention from '@/pages/IPD/CoronaryIntervention/View.vue'
import Discharged from '@/pages/IPD/DischargeCertificate/View.vue'
import CoordinatingWithThePatient from '@/pages/IPD/CoordinatingWithThePatient/View.vue'
import TrickSummary from '@/pages/TrickSummary/View.vue'
import IPDScaleForAssessmentOfSuicideIntent from '@/pages/IPD/ScaleForAssessmentOfSuicideIntent/View.vue'
import ArterialBloodGasTest from '@/pages/ED/PointOfCareTesting/ArterialBloodGasTestView.vue'
import ConsentForTransfusionOfBlood from '@/pages/ConsentForTransfusionOfBloodAndBloodDerivedProducts/View.vue'

export default {
  /**
   * The name of the page.
   */
  name: 'IPD-DetailV2',
  props: ['VisitId', 'Type', 'VisitType'],
  components: {
    ConsentForTransfusionOfBlood,
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
    ProcedureSummary,
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
    ArterialBloodGasTest
    // DischargeAdultVitalSigns
  },
  data () {
    return {
    }
  },
  mounted () {
  },
  computed: {
  },
  methods: {
  }
}
</script>
