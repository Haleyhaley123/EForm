<template>
  <div class="main-content">
    <div :key="index" v-for="(form, index) in $options.components" class="mb-10">
      <component v-if="form.name === Type" :FormV2="FormV2" :Form="form" v-bind:is="Type" :VisitType="VisitType" :VisitId="VisitId"/>
    </div>
  </div>
</template>
<script>
// ipd
import IPDInitialAssessmentForAudult from '@/pages/IPD/InitialAssessment/ForAdultViewV2.vue'
import IPDInitialAssessmentForFrailElderly from '@/pages/IPD/InitialAssessment/ForFrailElderlyView.vue'
import IPDInitialAssessmentForChemotherapy from '@/pages/IPD/InitialAssessment/ForChemotherapyView.vue'
import IPDPFEF from '@/pages/PatientAndFamilyEducation/View.vue'
import EDPFEF from '@/pages/PatientAndFamilyEducation/ViewED.vue'
import EOCPFEF from '@/pages/PatientAndFamilyEducation/ViewEOC.vue'
import OPDPFEF from '@/pages/PatientAndFamilyEducation/ViewOPD.vue'
import IPDPatientProgressNote from '@/pages/IPD/_ProgressNote/View.vue'
import IPDCardiacArrestRecord from '@/pages/CardiacArrestRecord/View.vue'
import IPDFallRiskAssessmentForAdult from '@/pages/IPD/FallRiskAssessment/AdultView.vue'
import IPDFallRiskAssessmentInPediatricInpatients from '@/pages/IPD/FallRiskAssessment/ChildView.vue'
import IPDGlamorganScaleForScreening from '@/pages/IPD/GlamorganPressure/ChildView.vue'
import IPDFallRiskAssessmentForObstetric from '@/pages/IPD/FallRiskAssessment/ObstetricView.vue'
import IPDPlanOfCare from '@/pages/IPD/PlanOfCare/View.vue'
import IPDMedicalRecord from '@/pages/IPD/MedicalRecord/BANoiKhoaViewV2.vue'
import IPDGuggingSwallowingScreen from '@/pages/IPD/GuggingSwallowingScreen/View.vue'
import IPDJointConsultationGroupMinutes from '@/pages/JointConsultationGroupMinutes/View.vue'
import EDJointConsultationGroupMinutes from '@/pages/JointConsultationGroupMinutes/ViewED.vue'
import EOCJointConsultationGroupMinutes from '@/pages/JointConsultationGroupMinutes/ViewEOC.vue'
import OPDJointConsultationGroupMinutes from '@/pages/JointConsultationGroupMinutes/ViewOPD.vue'
import IPDJointConsultationForApprovalOfSurgery from '@/pages/JointConsultationForApprovalOfSurgery/View.vue'
import EDJointConsultationForApprovalOfSurgery from '@/pages/JointConsultationForApprovalOfSurgery/ViewED.vue'
import EOCJointConsultationForApprovalOfSurgery from '@/pages/JointConsultationForApprovalOfSurgery/ViewEOC.vue'
import OPDJointConsultationForApprovalOfSurgery from '@/pages/JointConsultationForApprovalOfSurgery/ViewOPD.vue'
import IPDTakeCareOfPatientsWithCovid19 from '@/pages/IPD/TakeCareOfPatientsWithCovid19/View.vue'
import IPDCareNote from '@/pages/IPD/TakeCareNote/View.vue'
import EDCareNote from '@/pages/TakeCareNote/View.vue'
import IPDPhysicianNote from '@/pages/IPD/PhysicianNote/View.vue'
import EDPhysicianNote from '@/pages/PhysicianNote/View.vue'
import IPDPHC from '@/pages/ED/PreOperativeProcedureHandoverChecklists/View.vue'
import EDPHC from '@/pages/ED/PreOperativeProcedureHandoverChecklists/ViewED.vue'
import EOCPHC from '@/pages/ED/PreOperativeProcedureHandoverChecklists/ViewEOC.vue'
import OPDPHC from '@/pages/ED/PreOperativeProcedureHandoverChecklists/ViewOPD.vue'
import IPDSurgicalProcedureSafetyChecklist from '@/pages/OPD/SurgicalProcedureSafetyChecklist/View.vue'
import EOCSurgicalProcedureSafetyChecklist from '@/pages/OPD/SurgicalProcedureSafetyChecklist/ViewEOC.vue'
import OPDSurgicalProcedureSafetyChecklist from '@/pages/OPD/SurgicalProcedureSafetyChecklist/ViewOPD.vue'
import IPDPatientOwnMedicationsChart from '@/pages/PatientOwnMedicationsChart/View.vue'
import EDPatientOwnMedicationsChart from '@/pages/PatientOwnMedicationsChart/ViewED.vue'
import IPDBloodRequestSupplyAndConfirmation from '@/pages/ED/BloodRequestSupplyAndConfirmation/View.vue'
import EDBloodRequestSupplyAndConfirmation from '@/pages/ED/BloodRequestSupplyAndConfirmation/ViewED.vue'
import IPDExternalTransportationAssessment from '@/pages/IPD/ExternalTransportationAssessment/View.vue'
import EDExternalTransportationAssessment from '@/pages/IPD/ExternalTransportationAssessment/ViewED.vue'
import IPDDischargePreparationChecklist from '@/pages/IPD/HospitalDischargeTable/View.vue'
import IPDDischargeAgaintsMedicalAdvice from '@/pages/IPD/ConfirmDischargeWithoutDirect/View.vue'
import IPDMortalityReport from '@/pages/IPD/MortalityReport/View.vue'
import IPDSurgeryCertificate from '@/pages/IPD/SurgeryCertificate/View.vue'
import IPDVitalSignsForAdult from '@/pages/IPD/VitalSigns/AdultVitalSigns/View.vue'
import IPDVitalSignsForPregnantWoman from '@/pages/IPD/VitalSigns/PregnantVitalSigns/View.vue'
import IPDNeonatalObservationChart from '@/pages/IPD/VitalSigns/NeonatalVitalSigns/View.vue'
import IPDVitalSignFor1To3 from '@/pages/IPD/VitalSigns/Pediatric1ToUnder3Month/View.vue'
import IPDVitalSignFor1To4 from '@/pages/IPD/VitalSigns/Pediatric1ToUnder4Old/View.vue'
import IPDVitalSignFor3To12 from '@/pages/IPD/VitalSigns/Pediatric3To12Month/View.vue'
import IPDVitalSignFor4To12 from '@/pages/IPD/VitalSigns/Pediatric4To12Old/View.vue'
import IPDVitalSignForOver12 from '@/pages/IPD/VitalSigns/PediatricOver12Old/View.vue'
import IPDBradenScale from '@/pages/IPD/BradenScoreboard/View.vue'
import IPDSumaryOf15DayTreatment from '@/pages/IPD/SummaryOf15DayTreatment/View.vue'
import IPDMonitoringSheetForPatientsWith from '@/pages/IPD/MonitorSheetForPatient/View.vue'
import IPDMedicationHistoryForm from '@/pages/IPD/MedicationHistoryForm/View.vue'
import IPDMedicationHistoryFormPediatricPatient from '@/pages/IPD/MedicationHistoryFormForNewBorn/View.vue'
import IPDMedicalReport from '@/pages/IPD/MedicalReport/ViewIPD.vue'
import OPDMedicalReport from '@/pages/OPD/MedicalReport/BCYTOPD.vue'
import EDMedicalReport from '@/pages/ED/MedicalReport/BCYTED.vue'
import IPDDischargeMedicalReport from '@/pages/IPD/DischargeMedicalReport/View.vue'
import EDDischargeMedicalReport from '@/pages/ED/DischargeMedicalReport/View.vue'
import IPDReferralLetter from '@/pages/IPD/ReferralLetter/View.vue'
import IPDInjuryCertificate from '@/pages/IPD/InjuryCertificate/View.vue'
import IPDTransferLetter from '@/pages/IPD/TransferLetter/View.vue'
import IPDHandOverCheckList from '@/pages/IPD/HandOverCheckList/View2.vue'
import IPDInitialAssessmentForPediatricInPatient from '@/pages/IPD/InitialAssessment/ForChildViewV2.vue'
import IPDInitialAssessmentForNeonatal from '@/pages/IPD/InitialAssessment/ForNewBornView.vue'
import IPDThrombosisRiskFactorAssessment from '@/pages/IPD/ThrombosisRiskFactorAssessment/View.vue'
import IPDThrombosisRiskFactorAssessmentFor from '@/pages/IPD/ThrombosisRiskFactorAssessmentForGeneralSurgeryPatients/View.vue'
import IPDInitialAssessmentForNeonatalMaternity from '@/pages/IPD/InitialAssessment/ForNeonatalMaternity/NeonatalMaternityView.vue'
import IPDInitialAssessmentForPregnantWomen from '@/pages/IPD/InitialAssessment/ForWomenInLabourView.vue'
import IPDTheSurgicalMedicalRecord from '@/pages/IPD/MedicalRecord/BANKView.vue'
import IPDMedicalRecordEye from '@/pages/IPD/MedicalRecord/BAMatView.vue'
import IPDCardiovascularForm from '@/pages/IPD/MedicalRecord/BMTMView.vue'
import IPDVerbalOrderForm from '@/pages/ED/EmergencyTriageRecord/OrderTableView.vue'
import IPDCoronaryIntervention from '@/pages/IPD/CoronaryIntervention/View.vue'
import IPDDischarged from '@/pages/IPD/DischargeCertificate/View.vue'
import EDDischarged from '@/pages/ED/DischargeCertificate/View.vue'
import IPDCoordinatingWithThePatient from '@/pages/IPD/CoordinatingWithThePatient/View.vue'
import OPDCoordinatingWithThePatient from '@/pages/OPD/CoordinatingWithThePatient/View.vue'
import IPDIPDScaleForAssessmentOfSuicideIntent from '@/pages/IPD/ScaleForAssessmentOfSuicideIntent/View.vue'
import IPDArterialBloodGasTest from '@/pages/ED/PointOfCareTesting/ArterialBloodGasTestView.vue'
import EDArterialBloodGasTest from '@/pages/ED/PointOfCareTesting/ArterialBloodGasTestViewED.vue'
import IPDChemicalBiologyTest from '@/pages/ED/PointOfCareTesting/ChemicalBiologyTestView.vue'
import EDChemicalBiologyTest from '@/pages/ED/PointOfCareTesting/ChemicalBiologyTestViewED.vue'
import IPDPROMForheartFailure from '@/pages/IPD/PROMForHeartFailure/View.vue'
import OPDPROMForheartFailure from '@/pages/IPD/PROMForHeartFailure/ViewOPD.vue'
import IPDPROMForCoronaryDisease from '@/pages/IPD/PromForCoronaryDisease/View.vue'
import OPDPROMForCoronaryDisease from '@/pages/IPD/PromForCoronaryDisease/ViewOPD.vue'
import IPDMedicalRecordObstetrics from '@/pages/IPD/MedicalRecord/BASANKHOAView.vue'
import IPDMedicalRecordGynecological from '@/pages/IPD/MedicalRecord/BAPKView.vue'
import IPDMedicalRecordPediatric from '@/pages/IPD/MedicalRecord/BANHIKHOAView.vue'
import IPDMedicalRecordNeonatal from '@/pages/IPD/MedicalRecord/BASoSinhView.vue'
import IPDA01_039_050919_V from '@/pages/IPD/MedicalRecord/BATMHView.vue'
import IPDA01_040_050919_V from '@/pages/IPD/MedicalRecord/BARHMView.vue'
import IPDMedicalRecordOncology from '@/pages/IPD/MedicalRecord/ViewBAUngBuou.vue'
import IPDConsentForOperationOrrProcedure from '@/pages/ConsentForOperation/View.vue'
import EDConsentForOperationOrrProcedure from '@/pages/ConsentForOperation/ViewED.vue'
import EOCConsentForOperationOrrProcedure from '@/pages/ConsentForOperation/ViewEOC.vue'
import OPDConsentForOperationOrrProcedure from '@/pages/ConsentForOperation/ViewOPD.vue'
import IPDRequestResuscitation from '@/pages/IPD/Consent/View.vue'
import EDRequestResuscitation from '@/pages/IPD/Consent/ViewED.vue'
import EOCRequestResuscitation from '@/pages/IPD/Consent/ViewEOC.vue'
import OPDRequestResuscitation from '@/pages/IPD/Consent/ViewOPD.vue'
import IPDHIVTestingConsent from '@/pages/HIVTestingConsent/View.vue'
import EDHIVTestingConsent from '@/pages/HIVTestingConsent/ViewED.vue'
import EOCHIVTestingConsent from '@/pages/HIVTestingConsent/ViewEOC.vue'
import OPDHIVTestingConsent from '@/pages/HIVTestingConsent/ViewOPD.vue'
import IPDCartridgeCelite from '@/pages/IPD/ACT/View.vue'
import EDCartridgeCelite from '@/pages/IPD/ACT/ViewED.vue'
import IPDA03_165_061222_V from '@/components/IPD/BloodGasAnalysisEG7/ViewIPD.vue'
import IPDHighlyRestrictedAntimicrobialConsult from '@/pages/HighlyRestrictedAntimicrobialConsult/View.vue'
import IPDConsultationDrugWithAnAsteriskMark from '@/pages/IPD/ConsultationDrugWithAnAsteriskMarkUpForm/View.vue'
import IPDTrickSummary from '@/pages/TrickSummary/View.vue'
import IPDBloodTransfusionChecklist from '@/pages/ED/BloodTransfusionChecklist/View.vue'
import IPDSurgeryAndProcedureSummary from '@/pages/ProcedureSummary/View.vue'
import IPDConsentForTransfusionOfBlood from '@/pages/ConsentForTransfusionOfBloodAndBloodDerivedProducts/ViewIPD.vue'
// opd
import OPDInitialAssessmentForShortTerm from '@/components/OPD/ForShortTerm.vue'
import EOCInitialAssessmentForShortTerm from '@/components/EOC/ForShortTerm.vue'
import OPDInitialAssessmentForOnGoing from '@/components/OPD/ForOnGoing.vue'
import EOCInitialAssessmentForOnGoing from '@/components/EOC/ForOnGoing.vue'
import OPDFallRiskScreening from '@/pages/OPD/InitialAssessment/FallRiskScreening/View.vue'
import EOCFallRiskScreening from '@/pages/EOC/InitialAssessment/FallRiskScreening/View.vue'
import OPDInitialAssessmentForTelehealth from '@/components/OPD/Telehealth.vue'
import OPDOutpatientExaminationNote from '@/components/OPD/OutpatientExaminationNote.vue'
import OPDTelehealthExaminationNote from '@/components/OPD/TelehealthExaminationNote.vue'
import EOCOutpatientExaminationNote from '@/components/EOC/OutpatientExaminationNote.vue'
import OPDHandOverCheckList from '@/pages/OPD/HandOverCheckList/ViewOnly.vue'
import EDHandOverCheckList from '@/pages/ED/HandOverCheckList/ViewOnly.vue'
import EDCardiacArrestRecord from '@/pages/CardiacArrestRecord/ViewED.vue'
import OPDPatientProgressNote from '@/pages/OPD/ProgressNote/View.vue'
import EDSkinTestResult from '@/pages/ED/SkinTestResult/View.vue'
import EOCSkinTestResult from '@/pages/ED/SkinTestResult/ViewEOC.vue'
import OPDSkinTestResult from '@/pages/ED/SkinTestResult/ViewOPD.vue'
import EDAssessmentForRetailServicePatient from '@/pages/RetailService/AssessmentView.vue'
import OPDAssessmentForRetailServicePatient from '@/pages/RetailService/AssessmentViewOPD.vue'
import EOCMedicalReport from '@/pages/EOC/Documents/MedicalReport/BCYTEOC.vue'
import OPDReferralLetter from '@/pages/OPD/ReferralLetter/View.vue'
import OPDTransferLetter from '@/pages/OPD/TransferLetter/View.vue'
import OPDNCCNBROV1 from '@/pages/OPD/NCCNBROV1/View.vue'
import OPDCBE from '@/pages/OPD/ClinicalBreastExamNote/View.vue'
import OPDPatientInformationTheRiskAssessmentForCancer from '@/pages/OPD/BreastCancerRiskAssessment/View.vue'
import OPDGENBRCA from '@/pages/OPD/GENBRCA1/View.vue'
import EDTrickSummary from '@/pages/TrickSummary/ViewED.vue'
import EOCTrickSummary from '@/pages/TrickSummary/ViewEOC.vue'
import OPDTrickSummary from '@/pages/TrickSummary/ViewOPD.vue'
import OPDDiseasesCertification from '@/pages/OPD/DiseasesCertification/View.vue'
import OPDSurgeryAndProcedureSummary from '@/pages/ProcedureSummary/ViewOPD.vue'
import OPDPatientHandOverRecord from '@/pages/ED/HandOverCheckList/ViewOnlyOPD.vue'
import OPDStandingOrderForRetailService from '@/pages/RetailService/StandingOrderViewOPD.vue'
// ed
import EDEmergencyTriageRecord from '@/pages/ED/EmergencyTriageRecord/Triage.vue'
import EDER0 from '@/pages/ED/EmergencyRecord/BACCED.vue'
import EDDI0 from '@/pages/ED/DischargeInformation/View.vue'
import EDVerbalOrderForm from '@/pages/ED/EmergencyTriageRecord/OrderTableViewED.vue'
import EDPatientProgressNote from '@/pages/ED/ProgressNote/View.vue'
import EDStandingOrderForRetailService from '@/pages/RetailService/StandingOrderView.vue'
import EDAmbulanceRunReport from '@/pages/ED/AmbulanceRunReport/View.vue'
import EDBloodTransfusionChecklist from '@/pages/ED/BloodTransfusionChecklist/ViewED.vue'
import EOCCardiacArrestRecord from '@/pages/CardiacArrestRecord/ViewEOC.vue'
import EDConsultationDrugWithAnAsteriskMark from '@/pages/IPD/ConsultationDrugWithAnAsteriskMarkUpForm/ViewED.vue'
import EDMCA from '@/pages/ED/MonitoringChartAndHandoverForms/View.vue'
import EDPatientHandOverRecord from '@/pages/ED/HandOverCheckList/ViewOnlyPatient.vue'
import EOCPatientHandOverRecord from '@/pages/ED/HandOverCheckList/ViewOnlyEOC.vue'
import EDSelfHarmRiskScreeningTool from '@/pages/ED/SelfHarmRiskScreeningToolUpForm/View.vue'
import EDSurgeryAndProcedureSummary from '@/pages/ProcedureSummary/ViewED.vue'
import EOCSurgeryAndProcedureSummary from '@/pages/ProcedureSummary/ViewEOC.vue'
import EDHighlyRestrictedAntimicrobialConsult from '@/pages/HighlyRestrictedAntimicrobialConsult/ViewED.vue'
import EDReferralLetter from '@/pages/ED/ReferralLetter/View.vue'
import EDEMCO from '@/pages/ED/EmergencyConfirmation/View.vue'
import EDInjuryCertificate from '@/pages/ED/InjuryCertificate/View.vue'
import EDTransferLetter from '@/pages/ED/TranferLetter/View.vue'
import EDConsentForTransfusionOfBlood from '@/pages/ConsentForTransfusionOfBloodAndBloodDerivedProducts/View.vue'
import EDA03_165_061222_V from '@/components/IPD/BloodGasAnalysisEG7/ViewED.vue'
import EDMortalityReport from '@/pages/ED/MortalityReport/View.vue'
import EDMortalityReportV2 from '@/pages/ED/MortalityReport/ViewV2.vue'
// eoc
import IPDStandingOrder from '@/pages/StandingOrder/View.vue'
import OPDStandingOrder from '@/pages/StandingOrder/ViewOPD.vue'
import EDStandingOrder from '@/pages/StandingOrder/ViewED.vue'
import EOCStandingOrder from '@/pages/StandingOrder/ViewEOC.vue'
import OPDComplexOutpatientCaseSummary from '@/pages/ComplexOutpatientCaseSummary/View.vue'
import EOCComplexOutpatientCaseSummary from '@/pages/ComplexOutpatientCaseSummary/ViewEOC.vue'
import EOCHandOverCheckList from '@/pages/EOC/HandOverCheckList/ViewOnly.vue'
import OPDCardiacArrestRecord from '@/pages/CardiacArrestRecord/ViewOPD.vue'
import EOCPatientProgressNote from '@/pages/ProgressNote/View.vue'
import EOCReferralLetter from '@/pages/EOC/Documents/ReferralLetter/View.vue'
import EOCTransferLetter from '@/pages/EOC/Documents/TranferLetter/View.vue'
import EDFallRiskScreening from '@/pages/ED/EmergencyTriageRecord/FallRiskScreening/View.vue'
import IPDIPD_A02_052_050919_V from '@/pages/IPD/AskThePatientBeforeSurgery/View.vue'
import IPDCatridgeKaolinACT from '@/pages/IPD/CartridgeKaolinACT/View.vue'
import EDCatridgeKaolinACT from '@/pages/IPD/CartridgeKaolinACT/ViewED.vue'
import OPDCatridgeKaolinACT from '@/pages/IPD/CartridgeKaolinACT/ViewOPD.vue'
import EOCCatridgeKaolinACT from '@/pages/IPD/CartridgeKaolinACT/ViewEOC.vue'
import OPDOPDPYCKCKHHTP from '@/pages/OPD/CKHoHapTienPhau/View.vue'
import OPDOPD_A01_204_030320_VE from '@/pages/OPD/RequestForPreSurgeryCardiologyConsultation/View.vue'
import OPDPregnancyFollowUpRecord from '@/pages/OPD/PregnancyFollowUpRecord/View.vue'
import IPDIPD_A01_159_050919_VE from '@/pages/CommitmentPaperToTakeThePlacenta/View.vue'
// eslint-disable-next-line camelcase
import IPDForNeonatalMaternity_Obstetrics from '@/pages/IPD/InitialAssessment/ForNeonatalMaternity/NeonatalMaternityViewV2.vue'
// eslint-disable-next-line camelcase
import IPDForUterineLife2Hours_Obstetrics from '@/pages/IPD/AssessmentOfInfantStatusDuringTheFirst2HoursOfExtrauterineLife/View.vue'
import OPDPreAnesthesiaConsultation from '@/pages/OPD/PreAnesthesiaConsultation/View.vue'
import IPDUploadImage from '@/pages/IPD/UploadImage/View.vue'
import EDUploadImage from '@/pages/IPD/UploadImage/ViewED.vue'
import EOCUploadImage from '@/pages/IPD/UploadImage/ViewEOC.vue'
import OPDUploadImage from '@/pages/IPD/UploadImage/ViewOPD.vue'
import IPDProcedureSafetyChecklist from '@/pages/FormLienKhoa/A02_053_OR_201022_V/View.vue'
import EDProcedureSafetyChecklist from '@/pages/FormLienKhoa/A02_053_OR_201022_V/EDView.vue'
import OPDProcedureSafetyChecklist from '@/pages/FormLienKhoa/A02_053_OR_201022_V/OPDView.vue'
import EOCProcedureSafetyChecklist from '@/pages/FormLienKhoa/A02_053_OR_201022_V/EOCView.vue'
export default {
  name: 'IPD-DetailV2',
  props: ['VisitId', 'Type', 'VisitType', 'Form', 'Label', 'UpdateAt', 'UpdatedBy', 'Code', 'Version'],
  components: {
    IPDUploadImage,
    EDUploadImage,
    EOCUploadImage,
    OPDUploadImage,
    OPDPreAnesthesiaConsultation,
    IPDForNeonatalMaternity_Obstetrics,
    IPDIPD_A02_052_050919_V,
    IPDMedicalRecordEye,
    OPDInitialAssessmentForTelehealth,
    EDHandOverCheckList,
    EOCTransferLetter,
    IPDTransferLetter,
    EDTransferLetter,
    OPDTransferLetter,
    EOCReferralLetter,
    OPDReferralLetter,
    EDReferralLetter,
    IPDReferralLetter,
    EDMCA,
    IPDInitialAssessmentForAudult,
    IPDInitialAssessmentForFrailElderly,
    IPDInitialAssessmentForChemotherapy,
    IPDPFEF,
    EDPFEF,
    EOCPFEF,
    OPDPFEF,
    IPDPatientProgressNote,
    OPDPatientProgressNote,
    EDPatientProgressNote,
    EDStandingOrderForRetailService,
    OPDStandingOrderForRetailService,
    EDAmbulanceRunReport,
    EOCPatientProgressNote,
    IPDCardiacArrestRecord,
    EDCardiacArrestRecord,
    EOCCardiacArrestRecord,
    OPDCardiacArrestRecord,
    IPDFallRiskAssessmentForAdult,
    IPDFallRiskAssessmentForObstetric,
    IPDPlanOfCare,
    IPDGuggingSwallowingScreen,
    IPDJointConsultationForApprovalOfSurgery,
    EDJointConsultationForApprovalOfSurgery,
    EOCJointConsultationForApprovalOfSurgery,
    OPDJointConsultationForApprovalOfSurgery,
    IPDJointConsultationGroupMinutes,
    EDJointConsultationGroupMinutes,
    EOCJointConsultationGroupMinutes,
    OPDJointConsultationGroupMinutes,
    IPDTakeCareOfPatientsWithCovid19,
    IPDCareNote,
    EDCareNote,
    IPDPhysicianNote,
    EDPhysicianNote,
    IPDPatientOwnMedicationsChart,
    EDPatientOwnMedicationsChart,
    IPDBloodRequestSupplyAndConfirmation,
    EDBloodRequestSupplyAndConfirmation,
    IPDBloodTransfusionChecklist,
    EDBloodTransfusionChecklist,
    IPDPHC,
    EDPHC,
    EOCPHC,
    OPDPHC,
    IPDConsultationDrugWithAnAsteriskMark,
    EDConsultationDrugWithAnAsteriskMark,
    IPDSurgicalProcedureSafetyChecklist,
    EOCSurgicalProcedureSafetyChecklist,
    OPDSurgicalProcedureSafetyChecklist,
    IPDExternalTransportationAssessment,
    EDExternalTransportationAssessment,
    IPDDischargePreparationChecklist,
    IPDMortalityReport,
    EDMortalityReport,
    EDMortalityReportV2,
    IPDBradenScale,
    IPDSurgeryCertificate,
    IPDVitalSignsForAdult,
    IPDTrickSummary,
    EDTrickSummary,
    EOCTrickSummary,
    OPDTrickSummary,
    OPDNCCNBROV1,
    OPDCBE,
    OPDPatientInformationTheRiskAssessmentForCancer,
    OPDGENBRCA,
    IPDSumaryOf15DayTreatment,
    IPDMedicationHistoryForm,
    IPDThrombosisRiskFactorAssessment,
    IPDInitialAssessmentForNeonatalMaternity,
    IPDFallRiskAssessmentInPediatricInpatients,
    IPDInitialAssessmentForPregnantWomen,
    IPDMonitoringSheetForPatientsWith,
    IPDVerbalOrderForm,
    EDVerbalOrderForm,
    IPDTheSurgicalMedicalRecord,
    IPDThrombosisRiskFactorAssessmentFor,
    IPDCoronaryIntervention,
    IPDGlamorganScaleForScreening,
    IPDNeonatalObservationChart,
    IPDCoordinatingWithThePatient,
    OPDCoordinatingWithThePatient,
    IPDMedicationHistoryFormPediatricPatient,
    IPDArterialBloodGasTest,
    EDArterialBloodGasTest,
    IPDIPDScaleForAssessmentOfSuicideIntent,
    IPDChemicalBiologyTest,
    EDChemicalBiologyTest,
    IPDSurgeryAndProcedureSummary,
    EDSurgeryAndProcedureSummary,
    EOCSurgeryAndProcedureSummary,
    OPDSurgeryAndProcedureSummary,
    IPDPROMForheartFailure,
    OPDPROMForheartFailure,
    IPDPROMForCoronaryDisease,
    OPDPROMForCoronaryDisease,
    IPDMedicalReport,
    OPDMedicalReport,
    EDMedicalReport,
    EOCMedicalReport,
    IPDHandOverCheckList,
    OPDHandOverCheckList,
    EOCHandOverCheckList,
    IPDInjuryCertificate,
    EDInjuryCertificate,
    EDEMCO,
    EDConsentForTransfusionOfBlood,
    IPDConsentForTransfusionOfBlood,
    IPDMedicalRecord,
    IPDMedicalRecordObstetrics,
    IPDMedicalRecordGynecological,
    IPDMedicalRecordPediatric,
    IPDMedicalRecordNeonatal,
    IPDA01_039_050919_V,
    IPDA01_040_050919_V,
    IPDMedicalRecordOncology,
    IPDInitialAssessmentForPediatricInPatient,
    IPDInitialAssessmentForNeonatal,
    IPDCardiovascularForm,
    IPDVitalSignFor1To3,
    IPDVitalSignFor1To4,
    IPDVitalSignFor3To12,
    IPDVitalSignForOver12,
    IPDVitalSignFor4To12,
    IPDHighlyRestrictedAntimicrobialConsult,
    EDHighlyRestrictedAntimicrobialConsult,
    IPDVitalSignsForPregnantWoman,
    IPDDischargeAgaintsMedicalAdvice,
    IPDStandingOrder,
    EDStandingOrder,
    EOCStandingOrder,
    OPDComplexOutpatientCaseSummary,
    EOCComplexOutpatientCaseSummary,
    OPDStandingOrder,
    IPDDischargeMedicalReport,
    EDDischargeMedicalReport,
    IPDDischarged,
    EDDischarged,
    OPDInitialAssessmentForShortTerm,
    EOCInitialAssessmentForShortTerm,
    OPDInitialAssessmentForOnGoing,
    EOCInitialAssessmentForOnGoing,
    OPDFallRiskScreening,
    EOCFallRiskScreening,
    EDFallRiskScreening,
    EDER0,
    EDDI0,
    EDEmergencyTriageRecord,
    OPDDiseasesCertification,
    OPDOutpatientExaminationNote,
    OPDTelehealthExaminationNote,
    EOCOutpatientExaminationNote,
    EDSkinTestResult,
    EOCSkinTestResult,
    OPDSkinTestResult,
    IPDConsentForOperationOrrProcedure,
    EDConsentForOperationOrrProcedure,
    OPDConsentForOperationOrrProcedure,
    EOCConsentForOperationOrrProcedure,
    IPDHIVTestingConsent,
    OPDHIVTestingConsent,
    IPDCartridgeCelite,
    EDCartridgeCelite,
    EOCHIVTestingConsent,
    EDHIVTestingConsent,
    EDAssessmentForRetailServicePatient,
    OPDAssessmentForRetailServicePatient,
    IPDRequestResuscitation,
    OPDRequestResuscitation,
    EDRequestResuscitation,
    EOCRequestResuscitation,
    EDSelfHarmRiskScreeningTool,
    EDPatientHandOverRecord,
    EOCPatientHandOverRecord,
    OPDPatientHandOverRecord,
    IPDCatridgeKaolinACT,
    EDCatridgeKaolinACT,
    OPDCatridgeKaolinACT,
    EOCCatridgeKaolinACT,
    OPDOPDPYCKCKHHTP,
    OPDOPD_A01_204_030320_VE,
    OPDPregnancyFollowUpRecord,
    IPDIPD_A01_159_050919_VE,
    IPDForUterineLife2Hours_Obstetrics,
    IPDProcedureSafetyChecklist,
    EDProcedureSafetyChecklist,
    OPDProcedureSafetyChecklist,
    EOCProcedureSafetyChecklist,
    IPDA03_165_061222_V,
    EDA03_165_061222_V
  },
  data () {
    return {
      FormV2: {
        title: this.Label,
        UpdateAt: this.UpdateAt,
        UpdatedBy: this.UpdatedBy,
        Version: this.Version,
        Code: this.Code
      }
    }
  },
  mounted () {
    console.log('indexall', this.Version)
  },
  computed: {
  },
  methods: {
  }
}
</script>
