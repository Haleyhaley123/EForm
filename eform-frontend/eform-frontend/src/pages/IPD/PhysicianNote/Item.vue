<template>
  <div>
    <v-loading v-if="!loaded" />
    <div class="main-content dioPhysician" v-else>
      <button v-if="$store.state.account.Username === 'hunglq25'" @click="Version = 10">Version</button>
      <GetInfoPid v-if="FormIdNewborn" :readonly="readonly" class="mrb20" :Customer="Customer" @updateInfo="updateInfo" :isGetVisitCode="false"/>
      <form class="form-inline form-inline2 full-width" style="display: flex;justify-content: space-between;" v-if="!ViewOnly">
        <div class="flex space-between full-width">
          <div>
            <div class="mrr5 inline-block mrb10">
              <span class="white-space mrr5">{{__t('Từ')}}:</span>
              <v-date-picker style="max-width: 176px;" :format="vDateTimeFormat" v-model="search.StartAt"/>
            </div>
            <div class="mrr5 inline-block mrb10">
              <span class="white-space mrr5">{{__t('Đến')}}:</span>
              <v-date-picker style="max-width: 176px;" :format="vDateTimeFormat"  v-model="search.EndAt"/>
            </div>
            <div class="mrr5 inline-block mrb10">
              <span class="white-space mrr5">{{__t('Người tạo')}}:</span>
              <UserAd class="inline-block" v-model="search.CreatedBy" :position="'Doctor'" />
            </div>
            <div class="mrr10 inline-block mrb10" style="min-width: 150px;">
              <v-select
                :unsetWidth="true"
                class='select-full-w mrr5'
                v-model="search.Sort"
                :placeholder="__t('Chọn')"
                :items="Sort"/>
            </div>
          </div>
          <div class="mrb10 box-btn-action">
            <button class="btn v-yellow-btn mrr5 inline-block" type="button" @click="getData">{{__t('home.loc')}}</button>
            <button class="btn btn-default inline-block" type="button" @click="resetFilter">{{__t('home.reset')}}</button>
          </div>
        </div>
      </form>
      <div v-if="loaded">
        <template v-if="DataSubmit">
          <div class="form-content-box">
            <table class="standing-order-tbl table-auto-counter table-striped">
              <thead>
                <tr>
                  <th width="1" class="no-wrap status_custom">{{__t('STT')}}</th>
                  <th width="1" class="no-wrap date_custom">{{__t('Ngày')}}</th>
                  <th class="no-wrap examination_custom" >{{__t('Khám')}}</th>
                  <th class="no-wrap examination_custom" >{{__t('Điều trị')}}</th>
                  <th width="1" class="no-wrap treatment">{{__t('Người tạo')}}</th>
                  <!-- <th width="1" class="no-wrap treatment">{{__t('Hành động')}}</th> -->
                </tr>
              </thead>
              <template v-if="DataSubmit.Datas && DataSubmit.Datas.length">
                <tbody>
                  <tr v-for="(item, index) in DataSubmit.Datas" :key="index + '_' + item.Id">
                    <td style="position: relative">
                      <div style="text-align: center;"> {{++index}} </div>
                    </td>
                    <td align="center">
                      <div v-if="item.Id != ''">{{item.NoteTime}}</div>
                      <v-date-picker :format="vDateTimeFormat" v-model="item.NoteTime"  v-else :placeholder="___t('Chọn ngày')"/>
                    </td>
                    <td style="vertical-align: text-top;">
                      <div><p>{{item.Examination}}</p></div>
                    </td>
                    <td style="vertical-align: text-top;">
                      <div><p>{{item.Treatment}}</p></div>
                    </td>
                    <td class="no-wrap">
                      <template v-if="Version >= 10">
                        <div class="flex-field thee-btn mrb10" v-if="checkShowDetail(item)">
                            <button @click="openForm(item)" class="btn btn-warning"><i class="fa fa-pencil"></i>{{__t('btn.sua')}}</button>
                        </div>
                        <SingleConfirm
                          :kind="'PhysicianNote_USERCREATED'"
                          :FormCode="'IPDTT'"
                          :item="item" :readonly="readonly2(item)"
                          :viewOnly="ViewOnly"
                          :dataVerOld="item.CreatedBy"
                          @success="success"
                          class="mrb10"
                          :styleBtnCopy="'top: 0px!important; right: -50px!important;'"
                          :hideLabel="true"/>
                      </template>
                      <template v-else>
                        <div class="flex-field customcss">
                            <ad-Info :ad="item.CreatedBy" />
                        </div>
                        <div class="flex-field thee-btn" v-if="!ViewOnly && !DataSubmit.IsLocked">
                            <button @click="openForm(item)" v-if="IsUpdate === item.CreatedBy" class="btn btn-warning"><i class="fa fa-pencil"></i>{{__t('btn.sua')}}</button>
                        </div>
                      </template>
                    </td>
                  </tr>
                </tbody>
              </template>
              <template v-else>
                <tr>
                  <td colspan="8" class="text-center hide-counter">{{__t('Chưa có thông tin')}}</td>
                </tr>
              </template>
            </table>
            <modal name="EditNotePhy" transition="pop-out" height="auto" :clickToClose="false">
              <div class="box v-model-content-popup">
                <div class="box-header text-center grabbable"
                    @mousedown="handleMouseDown"
                    @touchstart="handleMouseDown"

                    @mouseleave="endPaintEvent"
                    @touchleave="endPaintEvent"

                    @mousemove="handleMouseMove"
                    @touchmove="handleMouseMove"

                    @mouseup="endPaintEvent"
                    @touchend="endPaintEvent"
                >
                  <h3 class="box-title grabbable">{{__t('Phiếu điều trị')}}</h3>
                </div>
                <div class="box-body padding-15">
                  <div class="form-group">
                    <label>{{__t('Chọn ngày')}}</label>
                      <v-date-picker :format="vDateTimeFormat" v-model="editItem.NoteTime" :placeholder="___t('Chọn ngày')"/>
                  </div>
                  <div class="form-group ct-form-group">
                    <label>{{__t('Khám')}}</label>
                    <textarea-autosize :maxHeight="300" v-model="editItem.Examination" class="form-control" rows="3"/>
                  </div>
                  <div class="form-group ct-form-group">
                    <label>{{__t('Điều trị')}}</label>
                    <textarea-autosize :maxHeight="300" v-model="editItem.Treatment" class="form-control" rows="3"/>
                  </div>
                </div>
                <div class="box-footer padding-15">
                  <div class="row">
                    <div class="col-md-6"><button type="button" class="btn btn-block" @click="close">{{__t('btn.huy')}}</button></div>
                    <div class="col-md-6"><button v-shortkey="['ctrl', 's']" @shortkey="submit()" :disabled="!editItem.NoteTime || !editItem.Examination || !editItem.Treatment" type="button" class="btn btn-block v-yellow-btn" @click="submit()"><i class="fa fa-floppy-o" aria-hidden="true"></i> {{__t('btn.luu')}}</button></div>
                  </div>
                </div>
              </div>
          </modal>
          <!-- <FloatBlock> -->
          <FloatBlock v-if="!ViewOnly">
            <template slot='buttons'>
              <div class="col-md-2 col-sm-2 col-2" v-if="!DataSubmit.IsLocked">
                <div class="form-group1">
                </div>
              </div>
              <div class="col-md-2 col-sm-2 col-2" v-if="!DataSubmit.IsLocked">
                <div class="form-group1">
                  <p class="hidden-text">hidden-text</p>
                </div>
              </div>
              <div class="col-md-4 col-sm-4 col-4" v-if="!isConfirmed && !DataSubmit.IsLocked">
                <template v-if="Version2">
                  <button v-if="!readonly" @click="openForm()" class="btn v-white  -btn pull-right btn-block"><i aria-hidden="true" class="fa fa-plus"></i> {{__t('btn.them_dieu_tri')}}</button>
                </template>
                <template v-else>
                  <button @click="openForm()" class="btn v-white  -btn pull-right btn-block"><i aria-hidden="true" class="fa fa-plus"></i> {{__t('btn.them_dieu_tri')}}</button>
                </template>
              </div>
              <div class="col-md-4 col-sm-4 col-4">
                <div class="form-group1">
                  <button class="btn v-white-btn pull-right btn-block" type="button"  v-shortkey="['ctrl', 'p']" @shortkey="print()" @click="print()"><i class="fa fa-print" aria-hidden="true"></i> {{__t('In')}}</button>
                </div>
              </div>
            </template>
          </FloatBlock>
          </div>
          <!-- <Printers /> -->
          <print v-if="!ViewOnly" :customer="Customer" :Version="Version" :datasubmit="datasubmit" :class="$store.state.account.Username === 'hunglq25' ? '' : 'none'"/>
        </template>
      </div>
      <div v-else class="loading-text"><v-loading/></div>
      <br/>
      <div>
        <p>A01_066_050919_VE</p>
        <LogForm :DataSubmit="DataSubmit"/>
      </div>
    </div>
  </div>
</template>
<script>
import SingleConfirm from '@/components/Form/SingleConfirm.vue'
import PhysicianNote from '@/services/PhysicianNote'
import VDatePicker from '@/components/VDateTimePicker.vue'
import VSelect from '@/components/VSelect.vue'
import Print from './Print.vue'
import MixinForm from '@/mixins/form.js'
import Logs from '@/components/Logs.vue'
import $ from 'jquery'
import DatePicker from 'vue2-datepicker'
import moment from 'moment'
import CustomersIPD from '@/services/IPD/Customer'
import ContraintNewbornAndPregnantWoman from '@/services/ContraintNewbornAndPregnantWoman'
export default {
  name: 'PhysicianNoteIPDItem',
  props: ['VisitId', 'VisitType', 'ViewOnly', 'FormIdNewborn', 'dataItem', 'condition', 'Version2'],
  mixins: [MixinForm],
  data () {
    return {
      search: {
        StartAt: moment().subtract(7, 'day').startOf('day').format(process.env.DATETIME_FORMAT),
        EndAt: new Date(),
        CreatedBy: this.$store.state.account.Username,
        Sort: 1
      },
      user: {},
      editItem: {},
      DataSubmit: null,
      datasubmit: null,
      DataPrint: null,
      loaded: false,
      IsChange: false,
      IsChangeID: '',
      IsUpdate: '',
      users: [
      ],
      progressDatas: [],
      popupTitle: '',
      Sort: [
        {
          value: 1,
          label: 'Thời gian giảm dần'
        },
        {
          value: 2,
          label: 'Thời gian tăng dần'
        }
      ],
      Move: false,
      left: 0,
      top: 0,
      Customer: {
        PID: '',
        Fullname: '',
        Gender: '',
        DateOfBirth: ''
      },
      Version: 1
    }
  },
  watch: {
    FormIdNewborn () {
      this.search = {
        StartAt: moment().subtract(7, 'day').startOf('day').format(process.env.DATETIME_FORMAT),
        EndAt: new Date(),
        CreatedBy: this.$store.state.account.Username,
        Sort: 1
      }
      this.getData()
    }
  },
  components: {
    Logs,
    VSelect,
    VDatePicker,
    DatePicker,
    Print,
    SingleConfirm
  },
  mounted () {
    this.getData()
  },
  computed: {
    isConfirmed () {
      return this.ViewOnly || (this.DataSubmit.DoctorTime || this.DataSubmit.HeadOfDepartmentTime || this.DataSubmit.HeadOfPharmacyTime)
    },
    firstTotal () {
      if (this.DataSubmit) {
        return this.DataSubmit.Datas.filter(e => !e.Removed).length
      }
      return 0
    },
    secondTotals () {
      var arr = []
      for (let index = 0; index <= this.firstTotal; index++) {
        arr.push({
          value: String(index),
          label: String(index)
        })
      }
      return arr
    },
    readonly () {
      return this.ViewOnly || !this.hasRole('IPDPDT02') || (this.DataSubmit.DoctorTime || this.DataSubmit.HeadOfDepartmentTime || this.DataSubmit.HeadOfPharmacyTime || this.DataSubmit.IsLocked)
    }
  },
  methods: {
    openForm (item) {
      this.editItem = {...item}
      if (this.editItem.NoteTime === undefined) {
        this.editItem.NoteTime = moment().format('HH:mm DD/MM/YYYY')
      }
      this.$modal.show('EditNotePhy')
    },
    close () {
      this.editItem = {}
      this.$modal.hide('EditNotePhy')
    },
    submit () {
      var formData = this.editItem
      formData.FormIdNewborn = this.FormIdNewborn
      var url = 'create/' + this._VisitId
      if (formData.Id) url = 'update/' + formData.Id
      return new PhysicianNote(this.search, this._VisitType).update(url, formData).then(response => {
        this.toastedSuccess()
        this.getData()
      }).catch(e => {
        // this.back()
      })
    },
    print () {
      this.$htmlToPaper('printMePhysicianNoteIPD', false, 'A01_066_050919_VE')
    },
    resetFilter () {
      this.search = {StartAt: '', EndAt: '', CreatedBy: null, Sort: ''}
      this.getData()
    },
    async confirm () {
      if (this.edited) {
        await this.save()
      }
      new PhysicianNote(this._VisitType).update(this._VisitId, this.user, 'Confirm/').then(response => {
        this.getData()
        this.toastedSuccess().scroll2Bottom()
      }).catch(e => {
        // this.back()
      })
    },
    async getData () {
      if (this.search.EndAt) {
        this.search.EndAt = `${this.search.EndAt}`
        if (this.search.EndAt.includes('Invalid')) {
          this.search.EndAt = ''
        }
        if (this.search.EndAt.includes('(')) {
          this.search.EndAt = this.$options.filters.formatDateWithoutSecon(this.search.EndAt)
        }
      }
      this.loaded = false
      if (this.ViewOnly === true) {
        this.search = {StartAt: '', EndAt: '', CreatedBy: null}
      }
      this.IsUpdate = this.$store.state.account.Username
      await new CustomersIPD().find(this._VisitId).then(res => {
        this.Customer = res
      }).catch(e => {
      })
      await new PhysicianNote(this.ViewOnly ? {FormIdNewborn: this.FormIdNewborn} : {...this.search, FormIdNewborn: this.FormIdNewborn}, this._VisitType).find('list/' + this._VisitId).then(response => {
        this.DataSubmit = response
        this.datasubmit = response.Datas
        this.Version = response.Version
        if (this.Customer && response.NewbornCustomer && this.FormIdNewborn) {
          this.Customer = {
            PID: response.NewbornCustomer.PID,
            Fullname: response.NewbornCustomer.Fullname,
            Gender: response.NewbornCustomer.Gender,
            DateOfBirth: response.NewbornCustomer.DateOfBirth,
            AgeFormated: response.NewbornCustomer.AgeFormated
          }
        }
        console.log('customer', {FormIdNewborn: this.FormIdNewborn, Customer: this.Customer})
        this.$emit('getIsLocked', response.IsLocked)
        this.loaded = true
      }).catch(e => {
        this.loaded = true
      })
    },
    save () {
      this.DataSubmit.FirstTotal = this.firstTotal
      this.DataSubmit.Datas = this.DataSubmit.Datas.filter(e => !e.Removed)
      return new PhysicianNote(this._VisitType).update('update/' + this._VisitId, this.DataSubmit).then(response => {
        this.toastedSuccess()
        this.getData()
        this.edited = false
      }).catch(e => {
        // this.back()
      })
    },
    create () {
      new PhysicianNote(this._VisitType).update('create/' + this._VisitId, {}).then(response => {
        this.reload()
      }).catch(e => {
        // this.back()
      })
    },
    handleMouseDown (e) {
      this.Move = true
      this.left = e.layerX
      this.top = e.layerY
    },
    endPaintEvent (e) {
      this.Move = false
    },
    handleMouseMove (e) {
      if (this.Move) {
        $('.v--modal-box').css('left', e.clientX - this.left)
        $('.v--modal-box').css('top', e.clientY - this.top)
      }
      // e.preventDefault()
    },
    updateInfo (cus) {
      new ContraintNewbornAndPregnantWoman().update('Updated/' + this.FormIdNewborn + (this.Customer.PID ? '/' + this.Customer.PID : '')).then(res => {
        this.toastedSuccess()
        this.$emit('getListSoSinh')
        this.getData()
      }).catch(e => {
        console.log(e)
      })
    },
    checkShowDetail (item) {
      let check = false
      if (!this.ViewOnly && (this.IsUpdate === item.CreatedBy || this.ImSuperAdmin()) && !item.IsLocked && ((item.ConfirmCreated && !item.ConfirmCreated.ConfirmAt) || !item.ConfirmCreated)) {
        check = true
      }
      return check
    },
    readonly2 (item) {
      let check = false
      if (this.ViewOnly || item.IsLocked || (item.ConfirmCreated && item.ConfirmCreated.ConfirmAt)) {
        check = true
      }
      return check
    },
    success () {
      this.getData()
    }
  }
}
</script>
<style lang="stylus" scoped>
table .form-control {
    padding: 5px;
    width: 100%;
}
table .form-readonlycss {
    background-color: white !important;
}
.mx-datepicker {
    width: 100% !important;
}
.form-content-box {
    margin: 0 auto;
    padding: 0;
    border: none !important;
}
.btn-pos-left {
    position: absolute;
    left: -30px;
    background-color: #f39c12;
    border: none;
    height: 100%;
    top: 0;
    width: 30px;
}
.btn-pos-left i {
    font-size: 25px;
}
.flex-field {
    display: flex;
}
.flex-field .childfl {
    display: flex;
    border: none;
    background: no-repeat;
}
.flex-field .childfl i {
    position: relative;
    top: 3px;
    left: -5px;
}
.flex-field .readonlycss {
  background-color: white !important;
}
.format-il {
  display: flex !important;
}
.form-group .fldp {
  display: flex;
  padding-right: 10px;
}
.label-filter {
    margin: 0 auto;
    padding-top: 5px;
}
.form-group .fldp .inputCustom {
   margin-left: 10px;
}
.form-group .fldp .inputCustom {
   margin-left: 10px;
}
.form-group .fldp .fintname input {
  width: 250px;
  height: 33px;
  border: 1px solid lightgray;
}
.form-inline2 .form-group {
    margin-bottom: 10px !important;
    height: 33px;
}
.customselectd {
  padding-left: 15px;
}
.thee-btn {
  display: flex;
  justify-content: space-around;
  padding-top: 5px;
}
.tile {
  padding-bottom: 50px;
}
.ct-form-group textarea {
  border: 1px solid lightgray;
}
.rowCT {
  display: flex;
  justify-content: space-between;
}
.table-auto-counter tr:hover {
  background-color: #dff9f7 ;
}
.standing-order-tbl td {
    border: 1px solid #DADCE0;
    cursor: pointer;
}
.mx-input-wrapper {
    width: 100%;
    max-width: 180px;
}
.pull-right {
  margin-left: 43px;
}
.customcss {
  justify-content: center;
}
.standing-order-tbl th {
    text-align: center;
}
.table-auto-counter tr td:not(.hide-counter):first-child::before {
    text-align: center;
    display: none !important;
}
.dioPhysician .form-content-box {
  padding: 0;
  border: none;
}
.grabbable {
  cursor: grab;
}
.box-btn-action {
  min-width: 155px;
}
@media (max-width: 680px) {
  .box-btn-action {
    min-width: 94px!important;
  }
}
</style>
