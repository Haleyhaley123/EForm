<template>
  <div class="main-content" id="patientRecords-page">
    <template v-if="loaded">
      <!-- <template v-if="hasNote || ImSuperAdmin()"> -->
        <div class="box-view-table-timeline flex align-center mrb10" v-if="hasRole('SHOWTIMELINE')">
          <button type="button" class="mini-btn" :class="timeline ? 'active' : ''" @click="timeline = !timeline">{{__t('Mốc thời gian')}}</button>
          <button type="button" class="mini-btn" :class="!timeline ? 'active' : ''" @click="timeline = !timeline">{{__t('Bảng')}}</button>
        </div>
        <hr v-if="tomtatthongtinnguoibenh" style="border-bottom: 2px solid #1375ba;margin-top: -11px;"/>
        <template v-if="hasRole('SHOWTIMELINE')">
          <MDTimeLine class="mrb10 box-timeline" v-if="timeline" :dataCustomer="dataCustomer" :dataTimeLine="DataTimeLine" :class="!timeline ? 'show' : ''"/>
        </template>
        <div class="box-table" v-if="!timeline" :class="timeline ? 'show' : ''">
          <h1 class="title-page">{{__t('Bệnh án')}}</h1>
          <table class="table table-striped table-bordered v-table" id="customer-tbl">
            <thead>
              <tr>
                <td colspan="10">
                  <div>
                    <label>{{__t('Lọc theo')}}:</label>
                  </div>
                  <div class="row">
                    <div class="col-md-10">
                      <div class="row" style="margin-bottom: 10px;">
                        <div class="col-md-3 padding-right-0">
                          <v-select :multi="true" v-model="parameters.status" :items="ListStatus" :placeholder="__t('Trạng thái')" class="select-full-w"/>
                        </div>
                        <div class="col-md-3 padding-right-0">
                          <input type="text" class="form-control" v-model="parameters.recordCode" :placeholder="__t('Mã hồ sơ')">
                        </div>
                        <div class="col-md-3 padding-right-0">
                          <input type="text" class="form-control" v-model="parameters.visitCode" :placeholder="__t('Số tiếp nhận')">
                        </div>
                        <div class="col-md-3 padding-right-0">
                          <v-select :multi="true" v-model="parameters.visitTypeGroupCode" :items="ListVisitType" :placeholder="__t('Loại thăm khám')" class="select-full-w"/>
                        </div>
                      </div>
                      <div class="row">
                        <div class="col-md-6 padding-right-0">
                          <v-select :multi="true" v-model="parameters.specialty" :items="ListSpecialties" :placeholder="__t('Chuyên khoa')" class="select-full-w"/>
                        </div>
                        <div class="col-md-3 padding-right-0">
                          <v-date-picker v-model="parameters.startExaminationTime" :format="vDateTimeFormat" :placeholder="__t('Từ ngày')"/>
                        </div>
                        <div class="col-md-3 padding-right-0">
                          <v-date-picker v-model="parameters.endExaminationTime" :format="vDateTimeFormat" :placeholder="__t('Tới ngày')"/>
                        </div>
                      </div>
                    </div>
                    <div class="col-md-2">
                      <div class="col-md-4">
                        <button class="btn v-yellow-btn" type="button" @click="getData">{{__t('home.loc')}}</button>
                      </div>
                      <div class="col-md-8">
                        <button class="btn btn-default" type="button" @click="resetFilter">{{__t('home.reset')}}</button>
                      </div>
                    </div>
                  </div>
                </td>
              </tr>
            </thead>
            <tbody>
              <template v-if="Records.Visit.length">
                <tr class="custom-table-head" style="background-color: #337AB7 !important;">
                  <td><b>{{__t('Stt')}}</b></td>
                  <td><b>{{__t('Mã hồ sơ')}}</b></td>
                  <td><b>{{__t('Thời gian thăm khám')}}</b></td>
                  <td><b>{{__t('Mã tiếp nhận')}}</b></td>
                  <td><b>{{__t('Trạng thái')}}</b></td>
                  <td align="center"><b>{{__t('Loại thăm khám')}}</b></td>
                  <td><b>{{__t('Chuyên khoa khám')}}</b></td>
                  <td><b>{{__t('BS Chính')}}</b></td>
                  <td><b>{{__t('ĐD Tiếp nhận')}}</b></td>
                  <td><b>{{__t('Chi tiết')}}</b></td>
                </tr>
                <tr :data="item" :key="((parameters.pageNumber - 1) * parameters.PageSize) + (index + 1)" v-for="(item ,index) in Records.Visit.filter(e => e.Assessment !== 'Điều trị nội trú')">
                  <td width="40" align="center">{{((parameters.pageNumber - 1) * parameters.PageSize) + (index + 1)}}</td>
                  <td width="150">
                    {{ item.RecordCode }}
                    <button v-permissions="'GUNLK1'" v-if="['OPD'].includes(item.Type)" @click="unlock(item)" title="Mở khóa hồ sơ cho bác sĩ chính" class="btn v-yellow-btn btn-xs" type="button">
                      <i aria-hidden="true" class="fa fa-key"></i>
                    </button>
                    <button v-if="item.Type !== 'EHOS'" v-permissions="'ADMINUPDATESTATUS'" title="Chỉnh sửa trạng thái" class="btn v-green-btn btn-xs" @click="handleChangeStatus(item.Type, item.Id, item.StatusId)"><i aria-hidden="true" class="fa fa-edit"></i></button>
                  </td>
                  <td width="170">{{ item.ExaminationTime | formatDateTime}}</td>
                  <td width="150">{{ item.VisitCode }}</td>
                  <td width="150">{{__label(item.Status)}}</td>
                  <td width="130" align="center" v-if="item.Type !== 'EHOS'"><span :class="'visit-type-label label-' + item.Type">{{ item.Type }}</span></td>
                  <td width="130" align="center" v-else><span :class="'visit-type-label label-' + item.Type">{{ item.Type }}</span></td>
                  <td width="150">
                    <span v-if="item.Type == 'EHOS'">{{__label(item)}}</span>
                    <span v-else>{{__label(item.Specialty)}} - {{item.Specialty.Site}}</span>
                  </td>
                  <td>
                    <ad-Info :ad="item.Type == 'EHOS' ? item.Username : item.DoctorUsername"/> <template v-if="item.AuthorizedDoctorUsername">/ <ad-Info :ad="item.AuthorizedDoctorUsername"/></template>
                  </td>
                  <td>
                    <ad-Info :ad="item.NurseUsername" />
                  </td>
                  <td v-if="tomtatthongtinnguoibenh" width="100" style="text-align: center;">
                    <router-link target='_blank' :VisitType="item.Type" :to="{name: 'LichSuKhamTaiVinmec', params: { Id: item.Id, Type: item.Type, VisitCode: item.VisitCode || 'N/A' }}">
                      <button class="btn v-yellow-btn" type="button">{{__t('Chi tiết')}}</button>
                    </router-link>
                  </td>
                  <td v-else width="100" style="text-align: center;">
                    <router-link target='_blank' v-if="item.Type == 'OPD' && item.IsPreAnesthesia === true" :to="{name: 'OPDRECORDCheck', params: { Id: item.Id, isPreAnesthesia: item.IsPreAnesthesia }}">
                      <button class="btn v-yellow-btn" type="button">{{__t('Chi tiết')}}</button>
                    </router-link>
                    <router-link target='_blank' v-if="item.Type == 'OPD' && item.IsPreAnesthesia === false" :to="{name: 'OPDRECORD', params: { Id: item.Id, isPreAnesthesia: item.IsPreAnesthesia }}">
                      <button class="btn v-yellow-btn" type="button">{{__t('Chi tiết')}}</button>
                    </router-link>
                    <router-link target='_blank' v-if="item.Type == 'ED'" :to="{name: 'EDRECORD', params: { Id: item.Id }}">
                      <button class="btn v-yellow-btn" type="button">{{__t('Chi tiết')}}</button>
                    </router-link>
                    <router-link target='_blank' v-if="item.Type == 'IPD'" :to="{name: 'IPDVIEWDETAIL', params: { Id: item.Id }}">
                      <button class="btn v-yellow-btn" type="button">{{__t('Chi tiết')}}</button>
                    </router-link>
                    <router-link target='_blank' v-if="item.Type == 'EOC'" :to="{name: 'EOCRECORD', params: { Id: item.Id }}">
                      <button class="btn v-yellow-btn" type="button">{{__t('Chi tiết')}}</button>
                    </router-link>
                    <button v-if="item.Type == 'EHOS'" @click="openEhosView(item)" class="btn v-yellow-btn" type="button">{{__t('Chi tiết')}}</button>
                    <!-- GUNLK1 -->
                  </td>
                </tr>
              </template>
              <template v-else>
                <tr>
                  <td colspan="9" class="text-center">{{__t('Không có dữ liệu')}}</td>
                </tr>
              </template>
            </tbody>
            <!-- <tfoot>
              <tr>
                <td colspan="5">
                  <paginate
                  :clickHandler="getData"
                  :container-class="'pagination pagination-sm no-margin v-pagination'"
                  :next-text="'»'"
                  :page-count="parameters.pages"
                  :prev-text="'«'"
                  v-if="Records.Visit.length > 0"
                  v-model="parameters.pageNumber">
                  </paginate>
                </td>
              </tr>
            </tfoot> -->
          </table>
        </div>
      <!-- </template> -->
      <!-- <div v-else>
        <div class="box border-box">
          <div class="box-body">
            <div class="row">
              <div class="col-md-12 col-sm-12">
                <div class="form-group">
                  <label>{{__t('Chọn lý do xem hồ sơ')}}</label>
                  <div class="group-radio no-flex" v-if="Notes">
                    <template  v-for="(status, index) in Notes">
                      <input :data="status" :key="index" type="checkbox" :id="'a' + index" name="status" v-model="status.selected">
                      <label :key="'a' + index" :for="'a' + index">{{__label(status)}}</label>
                    </template>
                  </div>
                </div>
              </div>
            </div>
            <div class="row">
              <div class="col-md-12 col-sm-12">
                <div class="form-group">
                  <label>{{__t('Mô tả chi tiết lý do nếu có')}}</label>
                  <textarea-autosize rows="3" :placeholder="__t('Nhập lý do khác')" class="form-control" v-model="OtherNote"/>
                </div>
              </div>
            </div>
            <div class="row">
              <div class="col-md-12 col-sm-12">
                <div class="form-group text-center">
                  <button class="btn v-yellow-btn" :disabled="!hasSelectNote()" type="button" @click="submitNote">{{__t('Xem danh sách hồ sơ')}}</button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div> -->
      <modal name="ehosView" width="800" transition="pop-out" height="auto">
        <div class="box v-model-content-popup">
          <div class="box-header text-center">
            <h3 class="box-title">{{__t('Thông tin lượt khám từ EHOS')}}</h3>
          </div>
          <div class="box-body padding-15">
            <EhosView :Data="ehosItem" />
          </div>
          <div class="box-footer padding-15">
            <div class="row">
              <div class="col-md-12"><button type="button" class="btn btn-block" @click="$modal.hide('ehosView')">{{__t('Đóng')}}</button></div>
            </div>
          </div>
        </div>
      </modal>
      <popup-change-status :VisitType="TypeBN" :DataStatus="DataStatus" :PID="PID" @handleResetData="getData"/>
    </template>
    <div v-else class="loading-text"><v-loading/></div>
  </div>
</template>

<script>
import MedicalRecords from '@/services/MedicalRecords'
import UnlockForm from '@/services/UnlockForm'
import EDStatus from '@/services/ED/EDStatus'
import Specialty from '@/services/Specialities'
import VDatePicker from '@/components/VDateTimePicker.vue'
import VSelect from '@/components/VSelect.vue'
import storage from '@/lib/storage'
import EhosView from './EhosView.vue'
import PopupChangeStatus from './popup/PopupChangeStatus.vue'
import MDTimeLine from '@/pages/MedicalRecords/MDTimeLine.vue'
import $ from 'jquery'
// import moment from 'moment'
import Logs from '@/services/Logs'
export default {
  name: 'Patient-Records',
  props: ['tomtatthongtinnguoibenh'],
  data () {
    return {
      Notes: [
        {
          id: 1,
          ViName: 'Khám bệnh/ Thực hiện phẫu thuật/thủ thuật tại bệnh viện',
          EnName: 'Khám bệnh/ Thực hiện phẫu thuật/thủ thuật tại bệnh viện',
          selected: false
        },
        {
          id: 2,
          ViName: 'Nhập viện nội trú',
          EnName: 'Nhập viện nội trú',
          selected: false
        },
        {
          id: 3,
          ViName: 'Hội chẩn',
          EnName: 'Hội chẩn',
          selected: false
        },
        {
          id: 4,
          ViName: 'Cấp lại giấy tờ cho bệnh nhân',
          EnName: 'Cấp lại giấy tờ cho bệnh nhân',
          selected: false
        },
        {
          id: 5,
          ViName: 'Giải quyết sự cố hoặc phàn nàn của bệnh nhân',
          EnName: 'Giải quyết sự cố hoặc phàn nàn của bệnh nhân',
          selected: false
        },
        {
          id: 6,
          ViName: 'Đánh giá tuân thủ hồ sơ bệnh án',
          EnName: 'Đánh giá tuân thủ hồ sơ bệnh án',
          selected: false
        },
        {
          id: 7,
          ViName: 'Đánh giá tuân thủ quy trình chuyên môn',
          EnName: 'Đánh giá tuân thủ quy trình chuyên môn',
          selected: false
        },
        {
          id: 8,
          ViName: 'Giải trình cho Bảo hiểm',
          EnName: 'Giải trình cho Bảo hiểm',
          selected: false
        },
        {
          id: 9,
          ViName: 'Kiểm tra định kỳ/đột xuất (JCI, SYT, Bảo hiểm..v.v..)',
          EnName: 'Kiểm tra định kỳ/đột xuất (JCI, SYT, Bảo hiểm..v.v..)',
          selected: false
        },
        {
          id: 10,
          ViName: 'Nghiên cứu khoa học',
          EnName: 'Nghiên cứu khoa học',
          selected: false
        },
        {
          id: 12,
          ViName: 'IT hỗ trợ người dùng',
          EnName: 'IT hỗ trợ người dùng',
          selected: false
        },
        {
          id: 11,
          ViName: 'Khác (Ghi rõ…..)',
          EnName: 'Khác (Ghi rõ…..)',
          selected: false
        }
      ],
      OtherNote: '',
      hasNote: false,
      parameters: {
        status: [],
        recordCode: null,
        visitCode: null,
        visitTypeGroupCode: [],
        specialty: [],
        startExaminationTime: null,
        endExaminationTime: null,
        pageNumber: 1,
        pages: 1,
        PageSize: 99999
      },
      Records: {
        Visit: []
      },
      ListStatus: [],
      ListSpecialties: [],
      ListVisitType: [
        {
          label: 'ED - Cấp cứu',
          value: 'ED'
        },
        {
          label: 'OPD - Ngoại trú',
          value: 'OPD'
        },
        {
          label: 'IPD - Nội trú',
          value: 'IPD'
        }
      ],
      Id: '',
      ehosItem: {},
      DataStatus: [],
      TypeBN: '',
      PID: '',
      DataTimeLine: null,
      timeline: false,
      dataCustomer: {},
      loaded: false
    }
  },
  components: {
    VDatePicker, VSelect, EhosView, PopupChangeStatus, MDTimeLine
  },
  mounted () {
    if (this.CurrentPatientObj && this.CurrentPatientObj.PID) {
      this.Id = this.CurrentPatientObj.PID
    } else {
      this.Id = this.$route.params.Id
    }
    this.getStatus()
    this.getSpecialties()
    this.getData()
    this.getTimeLine()
  },
  computed: {
    width () {
      return $('.container').width() / 2
    }
  },
  methods: {
    unlock (item) {
      if (item.Fullname) {
        this.$modal.show('dialog', {
          title: 'Mở khóa PHIẾU KHÁM NGOẠI TRÚ',
          text: 'Thao tác này sẽ mở khóa PHIẾU KHÁM NGOẠI TRÚ cho bác sĩ ' + item.Fullname,
          class: 'v-dialog v-dialog-warning',
          buttons: [
            {
              title: 'OK',
              class: 'btn btn-warning',
              handler: () => {
                this.$modal.hide('dialog')
                this.unlockDoctor(item)
              }
            },
            {
              title: 'Hủy',
              class: 'btn',
              handler: () => {
                this.$modal.hide('dialog')
              }
            }
          ]
        })
      } else {
        this.alert('Thông báo', 'Hồ sơ này chưa có bác sĩ')
      }
    },
    unlockDoctor (item) {
      console.log(item)
      new UnlockForm().create({Id: item.Id}).then(() => {
        this.toastedSuccess('Mở khóa hồ sơ thành công')
      })
    },
    openEhosView (item) {
      this.$modal.show('ehosView')
      this.ehosItem = item
    },
    hasSelectNote () {
      var hasOtherNote = this.Notes.find(e => e.selected && e.id === 11)
      var hasNote = this.Notes.find(e => e.selected && e.id !== 11)
      var OtherNote = this.OtherNote
      return (hasOtherNote && OtherNote) || (!hasOtherNote && hasNote)
    },
    submitNote () {
      var note = this.Notes.filter(e => e.selected).map(e => {
        return e.ViName
      })
      new Logs({})
        .update('', {
          url: window.location.href,
          name: 'Kho HSBA - BN ' + this.Records.Customer.Fullname + ' PID ' + this.Records.Customer.PID,
          reason: note.join(',') + ',' + this.OtherNote
        })
        .then(response => {
          this.hasNote = true
        }).catch(e => {
        })
    },
    resetFilter () {
      this.parameters = {
        status: [],
        visitCode: null,
        recordCode: null,
        visitTypeGroupCode: [],
        specialty: [],
        startExaminationTime: null,
        endExaminationTime: null,
        pageNumber: 1,
        pages: 1,
        PageSize: process.env.PAGE_SIZE
      }
      this.getData()
    },
    getStatus () {
      var fromStorage = storage.get('AllStatus')
      if (!fromStorage) {
        new EDStatus({'visit_type_group': ''}).all().then(r => {
          r.forEach(option => {
            this.ListStatus.push({
              label: option.Code + ' - ' + option.ViName,
              EnLabel: option.Code + ' - ' + option.EnName,
              value: option.Id,
              group: option.Code,
              StatusCode: option.StatusCode
            })
          })
          storage.set('AllStatus', JSON.stringify(this.ListStatus))
        })
      } else {
        this.ListStatus = fromStorage
      }
    },
    getSpecialties () {
      new Specialty().all().then(r => {
        r.forEach(option => {
          this.ListSpecialties.push({
            label: option.SiteName + ' - ' + option.ViName,
            EnLabel: option.SiteName + ' - ' + option.EnName,
            value: option.Id
          })
        })
      })
    },
    updateQuery () {
      return Object.assign({}, this.parameters, {
        status: this.parameters.status.map(e => { return e.value }),
        visitCode: this.parameters.visitCode,
        recordCode: this.parameters.recordCode,
        visitTypeGroupCode: this.parameters.visitTypeGroupCode.map(e => { return e.value }),
        specialty: this.parameters.specialty.map(e => { return e.value }),
        startExaminationTime: this.parameters.startExaminationTime,
        endExaminationTime: this.parameters.endExaminationTime
      })
    },
    getData () {
      new MedicalRecords(this.updateQuery())
        .find(this.Id)
        .then(response => {
          this.dataCustomer = response.Customer
          this.Records = response || []
          var bonussssss = 1
          if ((response.count % this.parameters.PageSize) === 0) {
            bonussssss = 0
          }
          this.parameters.pages = parseInt(response.count / this.parameters.PageSize) + bonussssss
          this.loaded = true
        }).catch(e => {
          this.loaded = true
        })
    },
    getTimeLine () {
      new MedicalRecords(this.updateQuery())
        .find('V2/' + this.Id)
        .then(response => {
          this.DataTimeLine = response
        }).catch(e => {
        })
    },
    handleChangeStatus (type, id, statusId) {
      this.TypeBN = type
      this.PID = id
      var DataStatus = storage.get('AllStatus')
      this.DataStatus = DataStatus.map(e => {
        e.DataType = 'Radio'
        e.Value = statusId === e.value
        return e
      }).filter(e => e.group === type)
      this.$modal.show('ChangeStatus')
    }
  }
}
</script>
<style scoped lang="stylus">
  // .box-table, .box-timeline {
  //   transition: transform 0.5s ease-out;
  //   transform-origin: top left;
  // }
  // .show {
  //   transform: scaleY(0);
  // }
  #patientRecords-page {
    .btn-change-status {
      width: 116px;
      margin-left: 10px;
      outline: none;
    }
    .mini-btn {
      width: 127px!important;
      height: 40px!important;
      font-weight: bold!important;
      color: #1a1a1a!important;
      border-radius: 4px!important;
      line-height: 0.8!important;
      box-shadow: 1px 0px 2px 0px #ccc!important;
      background: #fff!important;
      // border: 1px solid black;
    }
    .active {
      background-color: #1375ba!important;
      color: #fff!important;
    }
  }
</style>
