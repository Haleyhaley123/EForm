<template>
  <div>
    <modal name="DetailPopup" transition="pop-out" id="DetailPopup" height="auto" :clickToClose="false" :width="modalWidth" >
      <div class="box v-model-content-popup" v-if="item">
        <div class="box-header text-center">
          <h3 class="box-title">{{item.Service.Code}} - {{item.Service.ViName}}.</h3>
        </div>
        <div class="box-body padding-15">
          <div class="row">
            <div class="col-sm-12 col-md-12">
              <table class="table v-table-1 no-margin col-1-1">
                <tr>
                  <th width="1"><div class="w160">{{__t('Yêu cầu')}}</div></th>
                  <td>
                    <p class="fake-input">{{item.AdditionalInformation}}</p>
                  </td>
                </tr>
                <tr>
                  <th width="1"><div class="w160">{{__t('Ghi chú')}}</div></th>
                  <td>
                    <p class="fake-input">{{item.Comments}}</p>
                  </td>
                </tr>
                <tr v-if="item.Microbiology">
                  <th width="1"><div class="w160">{{__t('Thông tin vi sinh')}}</div></th>
                  <td v-if="!item.Microbiology.IsNotUse">
                    <div class="flex-box flex-center">
                      <label class="text-left no-wrap mr-10 lb-w2" required>{{__t('Loại mẫu')}}:</label>
                      <div class="input-group mr-5" style="flex-grow: 1">
                        <span class="fake-input disable">{{(TypeGroupList.find(e => e.code === item.Microbiology.STypeGroupID) || {}).label}}, {{(((TypeGroupList.find(e => e.code === item.Microbiology.STypeGroupID) || {}).Items || []).find(e => item.Microbiology.STypeID === e.Code) || {}).ViName}}</span>
                      </div>
                    </div>
                    <div class="flex-box flex-center mt-10 mb-10">
                      <label class="text-left no-wrap mr-10 lb-w2" required>{{__t('Phân tầng kháng sinh')}}:</label>
                      <div class="input-group mr-5" style="flex-grow: 1">
                        <span class="fake-input disable">{{(GroupList.find(e => e.code === item.Microbiology.Stratified) || {}).label}}</span>
                      </div>
                    </div>
                  </td>
                  <td v-else>{{__t('Không dùng')}}</td>
                </tr>
                <tr v-if="item.Pathology">
                  <th width="1"><div class="w160">{{__t('Thông tin giải phẫu bệnh')}}</div></th>
                  <td>
                    <b>{{(PathologyTypes.find(e => e.Code === item.Pathology.PathologyType) || {}).ViName}}</b>
                    <table class="table v-table-1 no-margin col-1-1 mt-15" v-if="item.Pathology.PathologyType !== '001'">
                      <tr v-if="item.Pathology.PathologyType === '003'">
                        <th width="1"><div class="w160">{{__t('Chu kì kinh nguyệt cuối')}}<span class="required">*</span></div></th>
                        <td>
                          <div class="flex-box flex-center">
                            <VTextarea :readonly="true" class="form-control" rows="1" :placeholder="__t('Nhập')" v-model="item.Pathology.LatestMenstrualPeriod"/>
                            <b class="mr-5 ml-5">{{__t('Ngày hành kinh cuối')}}:</b>
                            <div class="no-wrap">
                              {{item.Pathology.TheLastDayOfLatestMenstrualPeriod}}
                            </div>
                          </div>
                        </td>
                      </tr>
                      <tr v-if="item.Pathology.PathologyType === '003'">
                        <th width="1"><div class="w160">{{__t('Sau mãn kinh')}}</div></th>
                        <td>
                          <VTextarea :readonly="true" class="form-control" rows="1" :placeholder="__t('Nhập')" v-model="item.Pathology.PostMenopause"/>
                        </td>
                      </tr>
                      <tr v-if="['002', '003', '004', '005', '006'].includes(item.Pathology.PathologyType)">
                        <th width="1"><div class="w160">{{__t('Phương pháp phẫu thuật')}}<span v-if="['005'].includes(item.Pathology.PathologyType)" class="required">*</span></div></th>
                        <td>
                          <VTextarea :readonly="true" class="form-control" rows="2" :placeholder="__t('Nhập')" v-model="item.Pathology.SurgeryMethod"/>
                        </td>
                      </tr>
                      <tr v-if="['002', '003', '004', '005', '006'].includes(item.Pathology.PathologyType)">
                        <th width="1"><div class="w160">{{__t('Vị trí lấy bệnh phẩm')}}<span class="required">*</span></div></th>
                        <td>
                          <VTextarea :readonly="true" class="form-control" rows="2" :placeholder="__t('Nhập')" v-model="item.Pathology.SiteOfSpecimen"/>
                        </td>
                      </tr>
                      <tr v-if="['002', '003', '004', '005', '006'].includes(item.Pathology.PathologyType)">
                        <th width="1"><div class="w160">{{__t('Thời gian lấy mẫu')}}<span class="required">*</span></div></th>
                        <td>
                          {{item.Pathology.CollectionTime}}
                        </td>
                      </tr>
                      <tr v-if="['002', '003', '004'].includes(item.Pathology.PathologyType)">
                        <th width="1"><div class="w160">{{__t('Cố định bằng dung dịch ')}}<span class="required">*</span></div></th>
                        <td>
                          <span class="fake-input disable" v-if="true">{{(StoreSpecimenWithSolutionOption.find(e => e.code === item.Pathology.StoreSpecimenWithSolution) || {}).label}}</span>
                        </td>
                      </tr>
                      <tr v-if="['002', '003', '004'].includes(item.Pathology.PathologyType)">
                        <th width="1"><div class="w160">{{__t('TG mẫu được cố định')}}<span class="required">*</span></div></th>
                        <td>
                          {{item.Pathology.TimeOfStore}}
                        </td>
                      </tr>
                      <tr v-if="['002', '004', '005', '006'].includes(item.Pathology.PathologyType)">
                        <th width="1"><div class="w160">{{__t('Định hướng mẫu BP')}}</div></th>
                        <td>
                          <VTextarea :readonly="true" class="form-control" rows="2" :placeholder="__t('Nhập')" v-model="item.Pathology.SpecimenOrientation"/>
                        </td>
                      </tr>
                      <tr v-if="['002', '004', '005', '006'].includes(item.Pathology.PathologyType)">
                        <th width="1"><div class="w160">{{__t('Tóm tắt dấu hiệu lâm sàng chính và các xét nghiệm khác')}}<span class="required">*</span></div></th>
                        <td>
                          <VTextarea :readonly="true" class="form-control" rows="2" :placeholder="__t('Nhập')" v-model="item.Pathology.ClinicalHistoryAndLabTests"/>
                        </td>
                      </tr>
                      <tr v-if="['002', '004', '005', '006'].includes(item.Pathology.PathologyType)">
                        <th width="1"><div class="w160">{{__t('Quá trình điều trị')}}<span class="required">*</span></div></th>
                        <td>
                          <VTextarea :readonly="true" class="form-control" rows="2" :placeholder="__t('Nhập')" v-model="item.Pathology.Treatment"/>
                        </td>
                      </tr>
                      <tr v-if="['002', '004', '005', '006'].includes(item.Pathology.PathologyType)">
                        <th width="1"><div class="w160">{{__t('Nhận xét đại thể')}}<span class="required">*</span></div></th>
                        <td>
                          <VTextarea :readonly="true" class="form-control" rows="2" :placeholder="__t('Nhập')" v-model="item.Pathology.GrosDescription"/>
                        </td>
                      </tr>
                      <tr v-if="['002', '004', '005', '006'].includes(item.Pathology.PathologyType)">
                        <th width="1"><div class="w160">{{__t('Kết quả lần trước')}}<span class="required">*</span></div></th>
                        <td>
                          <VTextarea :readonly="true" class="form-control" rows="2" :placeholder="__t('Nhập')" v-model="item.Pathology.PreviousResults"/>
                        </td>
                      </tr>
                      <tr v-if="['003'].includes(item.Pathology.PathologyType)">
                        <th width="1"><div class="w160">{{__t('Tiền sử xét nghiệm tế bào, cổ tử cung và điều trị trước đó')}}<span class="required">*</span></div></th>
                        <td>
                          <VTextarea :readonly="true" class="form-control" rows="2" :placeholder="__t('Nhập')" v-model="item.Pathology.CervicalCytologyTestHistoryAndTreatmentBefore"/>
                        </td>
                      </tr>
                      <tr v-if="['003'].includes(item.Pathology.PathologyType)">
                        <th width="1"><div class="w160">{{__t('Tiền sử ung thư tế bào biều mô trước đó')}}</div></th>
                        <td>
                          <VTextarea :readonly="true" class="form-control" rows="2" :placeholder="__t('Nhập')" v-model="item.Pathology.HistoryOfSquamousCellCarcinoma"/>
                        </td>
                      </tr>
                      <tr v-if="['002', '003', '004', '005', '006'].includes(item.Pathology.PathologyType)">
                        <th width="1"><div class="w160">{{__t('Chẩn đoán lâm sàng')}}<span class="required">*</span></div></th>
                        <td>
                          <VTextarea :readonly="true" class="form-control" rows="2" :placeholder="__t('Nhập')" v-model="item.Pathology.ClinicalDiagnosis"/>
                        </td>
                      </tr>
                      <tr v-if="['003'].includes(item.Pathology.PathologyType)">
                        <td colspan="2">
                          <div class="service-list d-flex x23" :class="{'disable': true}">
                            <div class="service-item d-flex" style="width: 33%" :key="index" :data="service" v-for="(service, index) in GynecologicalCytologyTypes">
                              <div class="v-checkbox v-checkbox-2">
                                <input type="checkbox" :id="'xntbhpk-' + index" v-model="item.Pathology[service.Key]">
                                <label :for="'xntbhpk-' + index"></label>
                              </div>
                              <span>{{__label(service)}}</span>
                            </div>
                          </div>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
                <tr>
                  <th width="1"><div class="w160">{{__t('Trạng thái')}}:</div></th>
                  <td>
                    <div><b>{{item.Status}}</b></div>
                    <div v-if="item.RadiologyScheduledStatus">Radiology Scheduled Status: {{RADIOLOGYSCHEDULEDPROCEDURESTATUS[item.RadiologyScheduledStatus]}}</div>
                    <div v-if="item.PaymentStatus">Payment Status: {{CHARGEPAYMENTSTATUS[item.PaymentStatus]}}</div>
                    <div v-if="item.PlacerOrderStatus">Placer Order Status: {{CPOEPLACERORDERSTATUS[item.PlacerOrderStatus]}}</div>
                    <div v-if="item.SpecimenStatus">Specimen Status: {{SPECIMENSTATUSES[item.SpecimenStatus]}}</div>
                    <div v-if="item.DiagnosticReported">Diagnostic Report: {{__t('Đã tiếp nhận')}}</div>
                  </td>
                </tr>
                <tr>
                  <th width="1"><div class="w160">{{__t('DRS Code')}}:</div></th>
                  <td>
                    <div><b>DRS{{item.PlacerIdentifyNumber}}</b></div>
                  </td>
                </tr>
              </table>
            </div>
          </div>
        </div>
        <div class="box-footer padding-15">
          <div class="row">
            <div class="col-sm-12 col-md-12"><button type="button"  class="btn btn-block v-yellow-btn" @click="close()">{{__t('Đồng ý')}}</button></div>
          </div>
        </div>
      </div>
    </modal>
  </div>
</template>
<script>
import MixinPlaceDiagnostics from '@/mixins/DoctorPlaceDiagnosticsOrder.js'
import MixinMasterData from '@/mixins/masterdata.js'
import EventBus from '@/lib/eventBus'
import constants from '@/constants'
const MODAL_WIDTH = 970
export default {
  name: 'DetailPopup',
  mixins: [MixinPlaceDiagnostics, MixinMasterData],
  data () {
    return {
      rawObj: {},
      showGuild: false,
      item: null,
      TYPEGROUPLIST: null,
      modalWidth: MODAL_WIDTH,
      GroupList: [
        {label: 'Nhóm 1', code: '01'},
        {label: 'Nhóm 2', code: '02'},
        {label: 'Nhóm 3', code: '03'}
      ],
      GynecologicalCytologyTypes: [
        {
          ViName: 'Dụng cụ tránh thai',
          EnName: 'Dụng cụ tránh thai',
          Value: 1,
          Key: 'IsBirthControlMethod'
        },
        {
          ViName: 'Điều trị xạ trị',
          EnName: 'Điều trị xạ trị',
          Value: 2,
          Key: 'IsRadiationTheorapy'
        },
        {
          ViName: 'Hậu sản',
          EnName: 'Hậu sản',
          Value: 3,
          Key: 'IsPostpartum'
        },
        {
          ViName: 'Điều trị hormone',
          EnName: 'Điều trị hormone',
          Value: 4,
          Key: 'IsHormoneTreatment'
        },
        {
          ViName: 'Chảy máu sau mãn kinh',
          EnName: 'Chảy máu sau mãn kinh',
          Value: 5,
          Key: 'IsPostMenopauseBleeding'
        },
        {
          ViName: 'Dùng thuốc tránh thai',
          EnName: 'Dùng thuốc tránh thai',
          Value: 6,
          Key: 'IsBirthControlPills'
        },
        {
          ViName: 'Cắt tử cung',
          EnName: 'Cắt tử cung',
          Value: 7,
          Key: 'IsUterusremoval'
        },
        {
          ViName: 'Mang thai',
          EnName: 'Mang thai',
          Value: 8,
          Key: 'IsPregnant'
        },
        {
          ViName: 'Đang cho con bú',
          EnName: 'Đang cho con bú',
          Value: 9,
          Key: 'IsBreastfeeding'
        }
      ],
      StoreSpecimenWithSolutionOption: [
        {
          label: 'Không',
          code: '0'
        }, {
          label: 'NBF 10%',
          code: '1'
        }, {
          label: 'Cellfix',
          code: '2'
        }, {
          label: 'Cồn 95',
          code: '3'
        }, {
          label: 'Khác',
          code: '4'
        }
      ],
      PathologyTypes: [{
        ViName: 'Không dùng',
        EnName: 'Không dùng',
        Code: '001',
        Id: '001'
      }, {
        ViName: 'PXN Tế bào',
        EnName: 'PXN Tế bào',
        Code: '002',
        Id: '002'
      }, {
        ViName: 'XN tế bào học phụ khoa',
        EnName: 'XN tế bào học phụ khoa',
        Code: '003',
        Id: '003'
      }, {
        ViName: 'PXN mô bệnh học',
        EnName: 'PXN mô bệnh học',
        Code: '004',
        Id: '004'
      }, {
        ViName: 'PXN khối tế bào',
        EnName: 'PXN khối tế bào',
        Code: '005',
        Id: '005'
      }, {
        ViName: 'PXN sinh thiết lạnh',
        EnName: 'PXN sinh thiết lạnh',
        Code: '006',
        Id: '006'
      }],
      CHARGEPAYMENTSTATUS: constants.CHARGEPAYMENTSTATUS,
      RADIOLOGYSCHEDULEDPROCEDURESTATUS: constants.RADIOLOGYSCHEDULEDPROCEDURESTATUS,
      CPOEPLACERORDERSTATUS: constants.CPOEPLACERORDERSTATUS,
      SPECIMENSTATUSES: constants.SPECIMENSTATUSES
    }
  },
  components: {
  },
  computed: {
    STypeIDList () {
      if (this.form.Microbiology.STypeGroupID === null) return []
      return ((this.TypeGroupList.find(e => this.form.Microbiology.STypeGroupID && e.code === this.form.Microbiology.STypeGroupID.code) || {}).Items || []).map(e => {
        return {
          label: e.Code,
          code: e.Code,
          Items: e.Items,
          name: e.ViName
        }
      })
    },
    TypeGroupList () {
      var arr = []
      var md = []
      if (this.TYPEGROUPLIST) {
        md = this.TYPEGROUPLIST.map(e => {
          return {
            label: e.ViName,
            code: e.Code,
            Items: e.Items
          }
        })
      }
      return [...new Set(arr.concat(md))]
    }
  },
  watch: {
  },
  created () {
    this.getTypeGroupList()
    this.modalWidth = window.innerWidth < MODAL_WIDTH
      ? '90%'
      : MODAL_WIDTH
    EventBus.$on('openDetailPopup', this.open)
  },
  beforeDestroy () {
    EventBus.$off('openDetailPopup')
  },
  methods: {
    getTypeGroupList () {
      if (this.TYPEGROUPLIST) return
      this.getRawMasterDatas({Form: 'Microbiology'}, (data) => {
        this.TYPEGROUPLIST = data
      })
    },
    close () {
      this.$modal.hide('DetailPopup')
      this.showGuild = false
    },
    ok () {
      this.form.IsOk = true
      this.update({form: this.form, type: this.form.type})
      this.$modal.hide('DetailPopup')
      this.showGuild = false
    },
    open (form) {
      this.item = this.cloneObj(form)
      console.log(form)
      this.$modal.show('DetailPopup')
    }
  }
}
</script>
