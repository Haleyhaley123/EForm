<template>
  <div class="main-content" v-if="loaded">
    <div class="box-body-bordered">
      <div style="display: flex">
        <p style="width: 50%" v-if="CustomerInfo"><b>{{__t("Họ và tên sản phụ")}}:</b> {{CustomerInfo.CustomerName}}</p>
        <p style="width: 50%" v-if="CustomerInfo"><b>{{__t("Số CMND/Hộ Chiếu/ Thẻ căn cước")}}:</b> {{CustomerInfo.CustomerPassportNumber}}</p>
      </div>
      <div style="display: flex">
        <p style="width: 50%" v-if="CustomerInfo"><b>{{__t("Địa chỉ")}}:</b> {{CustomerInfo.CustomerAddress}}</p>
        <p style="width: 50%" v-if="CustomerInfo"><b>{{__t("Số điện thoại liên lạc")}}:</b> {{CustomerInfo.CustomerPhone}}</p>
      </div>
      <div style="display: flex; border-top: 1px solid gray; padding-top: 10px;">
        <p style="width: 50%; margin: auto;" v-if="CustomerInfo && CustomerInfo.CustomerRelationship"><b>{{__t("Họ tên chồng sản phụ")}}:</b> {{CustomerInfo.CustomerRelationship.ContactName}}</p>
        <p style="width: 50%; margin: auto;" v-else><b>{{__t("Họ tên chồng sản phụ")}}:</b></p>
        <!-- Số CMND/Hộ Chiếu/ Thẻ căn cước -->
        <div style="width: 50%" v-if="MASTERDATA['CPTTTP01']">
          <div class="form-group" style="display: flex">
            <label style="margin: auto 10px auto 0px">{{__t("Số CMND/Hộ Chiếu/ Thẻ căn cước")}}: </label>
            <div style="width: 50%" :data="item" :key="index" v-for="(item, index) in MASTERDATA['CPTTTP01'].Items">
              <textarea-autosize :readonly="readonly" :data-code="item.Code" :id="item.Code" :data="item" class="form-control" rows="1" :placeholder="__label(item)" v-model="item.Value"/>
            </div>
          </div>
        </div>
      </div>
      <div style="display: flex">
        <p style="width: 50%;" v-if="CustomerInfo && CustomerInfo.CustomerRelationship"><b>{{__t("Địa chỉ")}}:</b> {{CustomerInfo.CustomerRelationship.ContactAddress}}</p>
        <p style="width: 50%;" v-else><b>{{__t("Địa chỉ")}}:</b></p>
        <p style="width: 50%;" v-if="CustomerInfo && CustomerInfo.CustomerRelationship"><b>{{__t("Số điện thoại liên lạc")}}:</b> {{CustomerInfo.CustomerRelationship.ContactPhoneNumber}}</p>
        <p style="width: 50%;" v-else><b>{{__t("Số điện thoại liên lạc")}}:</b></p>
      </div>
      <!-- Phương pháp sinh -->
      <div class="row" v-if="MASTERDATA['CPTTTP03']">
        <div class="col-md-12">
          <div class="form-group">
            <label >{{__t("Phương pháp sinh")}}</label>
            <div :data="item" :key="index" v-for="(item, index) in MASTERDATA['CPTTTP03'].Items">
              <textarea-autosize :readonly="readonly" :data-code="item.Code" :id="item.Code" :data="item" class="form-control" rows="3" :placeholder="__label(item)" v-model="item.Value"/>
            </div>
          </div>
        </div>
      </div>
      <!-- Thời gian sinh con -->
      <div style="display: flex;" v-if="MASTERDATA['CPTTTP05']">
        <label style="margin: auto 10px auto 0px">{{ __t(__label(MASTERDATA["CPTTTP05"])) }}:</label>
        <div :data="item" :key="index" v-for="(item, index) in MASTERDATA['CPTTTP05'].Items">
          <VDateTimePicker5 :readonly="readonly" class="mrr20" v-model="item.Value" :max="new Date()" :format="vDateTimeFormat" :placeholder="__t('Chọn ngày')"/>
        </div>
      </div>
      <br>
      <p>{{__t("Sau khi sinh con, Tôi có nguyện vọng xin được mang bánh nhau về nhà.")}}</p>
      <!-- Lý do xin nhau thai -->
      <div class="row" v-if="MASTERDATA['CPTTTP09']">
        <div class="col-md-12">
          <div class="form-group">
            <label>{{__t("Lý do xin nhau thai")}}</label>
            <div :data="item" :key="index" v-for="(item, index) in MASTERDATA['CPTTTP09'].Items">
              <textarea-autosize :readonly="readonly" :data-code="item.Code" :id="item.Code" :data="item" class="form-control" rows="3" :placeholder="__label(item)" v-model="item.Value"/>
            </div>
          </div>
        </div>
      </div>
      <p>{{__t("Mặc dù đã được BS giải thích về trách nhiệm kiểm soát nhiễm khuẩn và chịu trách nhiệm sử dụng bánh nhau cho mục đích cá nhân, tôi vẫn mong muốn xin lấy bánh nhau về.")}}</p>
      <p>{{__t("Tôi cam kết tuân thủ các quy định của bệnh viện và xác nhận đã hiểu rõ sự giải thích và tự nguyện viết giấy cam kết này làm bằng chứng chịu hoàn toàn trách nhiệm với việc sử dụng nhau thai không theo quy định của bệnh viện.")}}</p>
      <br><br>
      <div class="row" style="width: 100%; display: flex;">
        <div style="width: 50%;" class="text-center">
          <template v-if="DaiDienBenhVien">
            <p class="text-center">{{DaiDienBenhVien.ConfirmAt | formatDateWithoutSecon}}</p>
            <eform-signature :ad="DaiDienBenhVien.ConfirmBy" :title="'Người đại diện bệnh viện'" />
          </template>
          <template v-else>
            <p><b>{{__t("Người đại diện bệnh  viện")}}</b></p>
            <p><i>({{__t("Ký và ghi rõ họ tên")}})</i></p>
            <p>
              <i v-if="viewOnly">{{__t('Chưa xác nhận')}}</i>
              <button v-else @click="showDoctorConfirm('DaiDienBenhVien', 'Người đại diện bệnh viện')" class="btn v-white-btn">{{__t('Xác nhận')}}</button>
            </p>
          </template>
        </div>
        <div style="width: 50%;" class="text-center">
          <template v-if="NhanVienDoDe">
            <p class="text-center">{{NhanVienDoDe.ConfirmAt | formatDateWithoutSecon}}</p>
            <eform-signature :ad="NhanVienDoDe.ConfirmBy" :title="'Nhân viên y tế đỡ đẻ'" />
          </template>
          <template v-else>
            <p><b>{{__t("Nhân viên y tế đỡ đẻ ")}}</b></p>
            <p><i>({{__t("Ký và ghi rõ họ tên")}})</i></p>
            <p>
              <i v-if="viewOnly">{{__t('Chưa xác nhận')}}</i>
              <button v-else @click="showDoctorConfirm('NhanVienDoDe', 'Nhân viên y tế đỡ đẻ')" class="btn v-white-btn">{{__t('Xác nhận')}}</button>
            </p>
          </template>
        </div>
      </div>
      <!-- UPLOAD ảnh -->
      <div class="mt-10 ml-20 mr-20">
        <div class="pomc-upload-area mb-10" v-if="MASTERDATA['CPTTTP07']">
          <div class="row">
            <div class="col-md-7 col-sm-7" v-if="!viewOnly">
              <p><b>{{__t('Upload ảnh')}}:</b></p>
              <p><button @click="openFile()" class="btn v-white-btn font16 font-bold btn-upload"><i class="fa fa-upload" aria-hidden="true"></i> {{__t('UPLOAD TỪ THIẾT BỊ')}}</button></p>
              <p>{{__t('Sau khi phiếu có xác nhận/ chữ ký từ người bệnh/ người đại diện. Người dùng chụp ảnh phiếu và tải lên hệ thống')}}</p>
            </div>
            <div  class="col-md-5 col-sm-5">
              <drop-files :readonly="viewOnly" id="mini-upload" v-model="MASTERDATA['CPTTTP07'].Items[0].Value" :dataDropfile="dataDropfile"/>
            </div>
          </div>
        </div>
      </div>
      <div>
        <p class="mrt10">A01_159_050919_VE</p>
        <p v-if="DataSubmit">{{__t('Chỉnh sửa lần cuối bởi')}} <ad-info :ad="DataSubmit.data.UpdatedBy" /> {{__t('lúc')}} {{DataSubmit.data.UpdatedAt | formatDateWithoutSecon}}</p>
      </div>
      <Print v-if="DataSubmit" :DataSubmit="DataSubmit" :MASTERDATA="MASTERDATA" :dataPicture="dataPicture" :CustomerInfo="CustomerInfo" hidden />
      <FloatBlock v-if="!viewOnly" :openBack="true" @handleBack="handleBack">
        <template slot="buttons">
          <div class="col-md-2 col-sm-2"><div class="form-group1"></div></div>
          <div class="col-md-4 col-sm-4">
            <div class="form-group1">
            <button
              v-if="checkShowPrintButton()"
              class="btn btn-block v-white-btn"
              type="button"
              v-shortkey="['ctrl', 'p']"
              @shortkey="print();"
              @click="print();"
            >
              <i class="fa fa-print" aria-hidden="true"></i> {{ __t("In") }}
            </button>
            </div>
          </div>
          <div class="col-md-6 col-sm-6" v-if="!IsLocked">
            <div class="flex form-group1">
            <button v-if="!IsLocked"
              class="btn v-yellow-btn pull-right btn-block hvr-ripple-out"
              type="button"
              v-shortkey="['ctrl', 's']"
              @click="handleSubmit();"
            >
              <i class="fa fa-floppy-o" aria-hidden="true"></i> {{ __t("btn.luu") }}
            </button>
            </div>
          </div>
        </template>
      </FloatBlock>
      <DoctorConfirm :popupTitle="popupTitle" :TypeConfirm="TypeConfirm" @popupSave="confirm"/>
    </div>
  </div>
  <div v-else class="text-center"><v-loading /></div>
</template>
<script>

import MixinMasterData from '@/mixins/masterdata.js'
import MixinForm from '@/mixins/form.js'
import TakePlacentaServices from '@/services/TakePlacentaServices'
import VDateTimePicker5 from '@/components/VDateTimePicker5.vue'
import moment from 'moment'
import _ from 'lodash'
import $ from 'jquery'
import FormApi from '@/services/FormAPI.js'
import DropFiles from '@/components/DropFiles.vue'
import DoctorConfirm from './popup/DoctorConfirm.vue'
import Print from './Print.vue'
import UploadFileService from '@/services/IPD/UploadFileService'

export default {
  name: 'CommitmentPaperToTakeThePlacentaItem',
  props: ['formId', 'viewOnly'],
  components: { VDateTimePicker5, DropFiles, DoctorConfirm, Print },
  mixins: [MixinMasterData, MixinForm],
  data () {
    return {
      dataDropfile: {},
      loaded: false,
      dataPicture: [],
      DataSubmit: null,
      CustomerInfo: null,
      IsLocked: false,
      enableConfirm: true,
      TypeConfirm: '',
      popupTitle: '',
      isConfirmed: false,
      DaiDienBenhVien: null,
      NhanVienDoDe: null
    }
  },
  computed: {
    _ItemId: function () {
      return this.formId || this.$route.params.Item
    },
    readonly: function () {
      return this.viewOnly || this.IsLocked || this.checkUser || this.isConfirmed
    }
  },
  mounted () {
    this.APIService = new FormApi({
      VisitType: 'IPD',
      VisitId: this._VisitId,
      FormCode: 'A01_159_050919_VE'
    })
    this.getMasterDatas({ Form: 'A01_159_050919_VE' }, () => {
      this.getData()
    })
    this.checkShowPrintButton()
  },
  beforeRouteUpdate (to, from, next) {
    if (to.query.plan || !this.edited) {
      next()
      return
    }
    this.$modal.show('dialog', {
      title: this.$t('Cảnh báo!'),
      text: this.$t('Dữ liệu bạn đang làm có thể bị mất nếu chưa lưu'),
      class: 'v-dialog v-dialog-warning',
      buttons: [
        {
          title: this.$t('Đồng ý'),
          class: 'btn',
          handler: () => {
            next()
            this.$modal.hide('dialog')
          }
        },
        {
          title: this.$t('Hủy bỏ'),
          class: 'btn bg-yellow',
          handler: () => {
            next(false)
            this.$modal.hide('dialog')
          }
        }
      ]
    })
  },
  watch: {
    formId: function (val) {
      if (val) {
        this._ItemId = val
        this.getData()
      }
    }
  },
  methods: {
    async getData () {
      this.loaded = false

      if (this.hasRole('APICFIPDA01_159_050919_VE')) {
        this.enableConfirm = true
      } else {
        this.enableConfirm = false
      }

      this.APIService.GetDetail(this._ItemId).then(res => {
        this.DataSubmit = res
        this.dataDropfile = {
          ...this.dataDropfile,
          contentImg: 'Tóm tắt thủ thuật can thiệp động mạch vành',
          titleImg: 'Hình ảnh được lấy từ biểu mẫu Tóm tắt thủ thuật can thiệp động mạch vành',
          visittypeImg: this._VisitType,
          visitidImg: this._VisitId,
          formidImg: res.data.Id || res.data.ID
        }
        console.log('dataDropfile', this.dataDropfile)
        if (this.DataSubmit.data.ConfirmInfos) {
          this.DaiDienBenhVien = this.DataSubmit.data.ConfirmInfos.find(e => e.ConfirmType === 'DaiDienBenhVien')
          this.NhanVienDoDe = this.DataSubmit.data.ConfirmInfos.find(e => e.ConfirmType === 'NhanVienDoDe')
        }
        this.IsLocked = res.IsLock24h
        if (res.data && res.data.ConfirmInfos.length > 0) {
          this.isConfirmed = true
        }

        for (const property in this.MASTERDATA) {
          if (this.MASTERDATA[property].Items) {
            this.MASTERDATA[property].Items.forEach(item => {
              var code = item.Code
              var exited = _.find(this.DataSubmit.data.Datas, { Code: code })
              if (exited) {
                if (exited.Code === 'CPTTTP06') {
                  item.Value = new Date(moment(exited.Value, 'HH:mm DD/MM/YYYY'))
                } else if (exited.Value && item.Code === 'CPTTTP08') {
                  item.Value = (this.JSONParse(exited.Value, true) || [])
                } else if (exited.Value && item.Code === 'CPTTTP05' && !this.isConfirmed) {
                  item.Value = this.string2Moment(exited.Value)
                } else {
                  item.Value = exited.Value
                }
              } else {
                item.Value = null
              }
            })
          }
        }
        this.dataMaster = JSON.stringify(this.MASTERDATA)
        this.loaded = true
      })
      new TakePlacentaServices().find(this._VisitId).then(response => {
        this.CustomerInfo = response
      })
    },
    openFile () {
      $('#dropzone').click()
    },
    handleBack () {
      if (this.dataMaster !== JSON.stringify(this.MASTERDATA)) {
        this.confirmPopupBack()
      } else {
        this.back()
      }
    },
    confirmPopupBack () {
      this.$modal.show('dialog', {
        clickToClose: false,
        title: this.__t('Cảnh báo'),
        text: this.__t('Dữ liệu bạn đang làm có thể bị mất nếu chưa lưu'),
        class: 'v-dialog v-dialog-warning',
        buttons: [
          {
            title: this.__t('Đồng ý'),
            class: 'btn',
            handler: () => {
              this.$modal.hide('dialog')
              this.back()
            }
          },
          {
            title: this.__t('Hủy bỏ'),
            class: 'btn btn-warning',
            handler: () => {
              this.$modal.hide('dialog')
            }
          }
        ]
      })
    },
    checkShowPrintButton () {
      if (this.IsLocked && this.DataSubmit) {
        return true
      } else if (!this.DataSubmit) {
        return false
      } else if (this.IsLocked && !this.DataSubmit) {
        return false
      } else if (!this.IsLocked && this.DataSubmit) {
        return true
      }
    },
    removedFile (arr) {
      new UploadFileService({hidemsg: true}).update('File/DeleteFileFromForms/' + this._ItemId, {Urls: arr.toString()}).then(response => {
      }).catch(err => {
        console.log(err)
      })
    },
    handleSubmit () {
      this.save()
    },
    save () {
      var arr = this.MASTERDATA['CPTTTP07'].Items[0].Value ? this.MASTERDATA['CPTTTP07'].Items[0].Value : []
      var obj = {}
      this.DataSubmit.Datas = []
      for (const property in this.MASTERDATA) {
        if (this.MASTERDATA[property].Items) {
          this.MASTERDATA[property].Items.forEach(item => {
            var code = item.Code
            var exited = _.find(this.DataSubmit.Datas, {Code: code})
            if (exited) {
              if (item.Code === 'CPTTTP0824') {
                item.Value = JSON.stringify(item.Value)
              } else if (item.Code === 'CPTTTP05' && typeof item.Value === 'object') {
                item.Value = this.moment2String(item.Value) || ''
              } else {
                item.Value = exited.Value
              }
            }

            this.DataSubmit.Datas.push({
              Code: item.Code,
              Value: item.Value,
              Group: property
            })
            obj[item.Code] = item.Value
          })
        }
      }
      this.APIService.UpdateForm(this._ItemId, this.DataSubmit).then(response => {
        this.removedFile(arr)
        this.toastedSuccess()
        this.reload()
      }).catch(e => {
      })
    },
    showDoctorConfirm (TypeConfirm, title) {
      this.popupTitle = title
      this.TypeConfirm = TypeConfirm
      this.$modal.show('doctorConfirmV3')
    },
    async confirm (data) {
      // var arr = this.MASTERDATA['CPTTTP07'].Items[0].Value ? this.MASTERDATA['CPTTTP07'].Items[0].Value : []
      this.checkConfirm = true
      // new SurgeryAndProcedureSummaryV3({}, this._VisitType).find('GetListItemsByVisitId/' + this._VisitId + '?hidemsg=' + true).then(response => {
      this.APIService.ConfirmForm(this._ItemId, {...data, kind: data.TypeConfirm})
        .then(response => {
          this.$modal.hide('doctorConfirmV3')
          this.toastedSuccess(this.$t('Xác nhận thành công'))
          this.checkConfirm = false
          // this.removedFile(arr)
          this.save()
          setTimeout(() => {
            this.reload()
          }, 500)
          // this.reload()
        })
        .catch(e => {
        })
    },
    print () {
      this.$htmlToPaper('printMe', false, 'A01_159_050919_VE')
    }
  }
}
</script>
