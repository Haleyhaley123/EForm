<template>
  <div class="a4-page box" id="printMe" v-if="DataSubmit">
    <table width="100%">
      <tr>
        <td width="10%" valign="top">
          <img src="/static/Logo Vinmec International Hospitall 1.png" />
        </td>
        <td width="85%" valign="top" class="text-center">
          <div class="font20 font-bold text-center">
            BẢN CAM KẾT XIN LẤY BÁNH RAU <br> KHÔNG THEO QUY ĐỊNH QUẢN LÝ CỦA BỆNH VIỆN
          </div>
          <div class="font16 font-bold text-center" style="font-style: italic">
            (COMMITMENT PAPER TO TAKE THE PLACENTA OUT OF HOSPITAL <br> BYEON THE REGULATIONS OF PLACENTA MANAGEMENT OF THE HOSPITAL)
          </div>
        </td>
      </tr>
    </table>
    <div class="box-center mrb10 text-center" style="border: 1px solid black; padding: 5px; width: 307px; margin: auto">
      <div class="flex" style="justify-content: center; margin-left: 100px;">
        <VueBarcode v-if="DataSubmit.data.PID" v-bind:value="DataSubmit.data.PID" :height="40" :displayValue="false" :textAlign="'left'" :width="1" :marginTop="5" :fontSize="15"/>
      </div>
      <div class="mrb10">Họ và tên/ <i>Full name</i>: {{DataSubmit.data.FullNameNB || 'N/A'}}</div>
      <div class="mrb10">Số PID/ <i>PID No</i>: {{DataSubmit.data.PID || 'N/A'}}</div>
      <div class="mrb10">Ngày tháng năm sinh/ <i>Date of birth</i>: {{DataSubmit.data.DateOfBirth ? getFTime2(DataSubmit.data.DateOfBirth) : 'N/A'}}</div>
      <div>Giới tính/ <i>Gender</i>: {{GENDER[DataSubmit.data.Gender] || 'N/A'}}</div>
    </div>
    <br>
    <div class="box form-content">
      <div class="box-body">
        <p v-if="CustomerInfo">Họ và tên sản phụ/ <i>Full name of patient: </i>{{CustomerInfo.CustomerName}}</p>
        <p v-if="CustomerInfo">Số CMND/Hộ Chiếu/ Thẻ căn cước/ <i>Identify number/Passport number: </i>{{CustomerInfo.CustomerPassportNumber}}</p>
        <p v-if="CustomerInfo">Địa chỉ/ <i>Address: </i>{{CustomerInfo.CustomerAddress}}</p>
        <p v-if="CustomerInfo">Số điện thoại liên lạc/ <i>The telephone number to contact: </i>{{CustomerInfo.CustomerPhone}}</p>
        <p v-if="CustomerInfo && CustomerInfo.CustomerRelationship">Họ tên chồng sản phụ/ <i>The name of the husband: </i>{{CustomerInfo.CustomerRelationship.ContactName}}</p>
        <p v-else>Họ tên chồng sản phụ/ <i>The name of the husband: </i></p>
        <p v-if="MASTERDATA['CPTTTP01']">Số CMND/Hộ Chiếu/ Thẻ căn cước/ <i>Identify number/Passport number: </i>{{MASTERDATA['CPTTTP01'].Items[0].Value}}</p>
        <p v-else>Số CMND/Hộ Chiếu/ Thẻ căn cước/ <i>Identify number/Passport number: </i></p>
        <p v-if="CustomerInfo && CustomerInfo.CustomerRelationship">Địa chỉ/ <i>Address: </i>{{CustomerInfo.CustomerRelationship.ContactAddress}}</p>
        <p v-else>Địa chỉ/ <i>Address: </i></p>
        <p v-if="CustomerInfo && CustomerInfo.CustomerRelationship">Số điện thoại liên lạc/ <i>The telephone number to contact: </i>{{CustomerInfo.CustomerRelationship.ContactPhoneNumber}}</p>
        <p v-else>Số điện thoại liên lạc/ <i>The telephone number to contact: </i></p>
        <p v-if="MASTERDATA['CPTTTP03']">Phương pháp sinh/ <i>Method of delivery:</i>{{MASTERDATA['CPTTTP03'].Items[0].Value}}</p>
        <p v-else>Phương pháp sinh/ <i>Method of delivery:</i></p>
        <p>Giờ sinh con/ <i>Time of delivery: </i>{{time}}&emsp;&emsp;&emsp;&emsp;&emsp;&emsp; Ngày sinh con/ <i>Date of delivery: </i>{{day}}</p>
        <p>Sau khi sinh con, Tôi có nguyện vọng xin được mang bánh nhau về nhà/ <i>After giving birth, I have intentions to bring home the placenta</i></p>
        <p v-if="MASTERDATA['CPTTTP09']">Lý do xin nhau thai/ <i>The reasons to collect the placenta: </i> {{MASTERDATA['CPTTTP09'].Items[0].Value}}</p>
        <p v-else>Lý do xin nhau thai/ <i>The reasons to collect the placenta: </i> </p>
        <p>Mặc dù đã được BS giải thích về trách nhiệm kiểm soát nhiễm khuẩn và chịu trách nhiệm sử dụng bánh nhau cho mục đích cá nhân, tôi vẫn mong muốn xin lấy bánh nhau về/ <i>Although it has been explained about the responsibilities of infection control and utilization of placenta for personal purposes, I still request to take the placenta home.</i></p>
        <p>Tôi cam kết tuân thủ các quy định của bệnh viện và xác nhận đã hiểu rõ sự giải thích và tự nguyện viết giấy cam kết này làm bằng chứng chịu hoàn toàn trách nhiệm với việc sử dụng nhau thai không theo quy định của bệnh viện/ <i>I commit to comply with the regulations of the hospital and confirm that I have understood the explanation and voluntarily to write the commitment as an evidence to hold full responsibility for the utilization of the placenta that is beyond the regulations of the hospital.</i></p>
        <table style="width: 100%;">
          <tr>
            <td></td>
            <td></td>
            <td>
              <p style="text-align: end;">{{hour}} giờ/ hours {{minute}} ngày/ date {{date}}</p>
            </td>
          </tr>
        </table>
        <br>
        <table style="width: 100%;">
          <tr>
            <td class="text-center">
              <div class="text-center"><b>Họ và tên sản phụ/chồng/ <br><i>Name and Signature of the husband</i></b></div>
              <div class="text-center"><i>(Ký và ghi rõ họ tên/ <br> Fullname and Signature)</i></div>
              <div>
                <br><br><br><br><br><br><br><br>
              </div>
            </td>
            <td class="text-center">
              <div class="text-center"><b>Người đại diện bệnh  viện/ <br><i>Name and Signature of Hospital's Representative</i></b></div>
              <div class="text-center"><i>(Ký và ghi rõ họ tên/ <br> Fullname and Signature)</i></div>
              <div class="text-center" v-if="DaiDienBenhVien">
                <br><br><br><br><br><br><br>
                <div class="text-center">{{DaiDienBenhVien.Note}}</div>
              </div>
              <div v-else>
                <br><br><br><br><br><br><br><br>
              </div>
            </td>
            <td >
              <div class="text-center"><b>Nhân viên y tế đỡ đẻ/ <br><i>Doctor (assisting delivery)</i></b></div>
              <div class="text-center"><i>(Ký và ghi rõ họ tên/ <br> Fullname and Signature)</i></div>
              <div class="text-center" v-if="NhanVienDoDe">
                <br><br><br><br><br><br><br>
                <div class="text-center">{{NhanVienDoDe.Note}}</div>
              </div>
              <div v-else>
                <br><br><br><br><br><br><br><br>
              </div>
            </td>
          </tr>
        </table>
      </div>
    </div>
  </div>
</template>
<script>

import VueBarcode from 'vue-barcode'
import constants from '@/constants'
const GENDER = constants.GENDER
export default {
  props: ['MASTERDATA', 'DataSubmit', 'dataPicture', 'CustomerInfo'],
  components: {VueBarcode},
  data () {
    return {
      userData: {},
      customer: this.$store.state.account.CurrentPatientObj,
      time: null,
      day: null,
      hour: null,
      minute: null,
      date: null,
      GENDER: GENDER,
      DaiDienBenhVien: null,
      NhanVienDoDe: null
    }
  },
  mounted () {
    this.getData()
  },
  methods: {
    getData () {
      let value = this.moment2String(this.MASTERDATA['CPTTTP05'].Items[0].Value)
      if (value) {
        let timeAndDate = value.split(' ')
        this.time = timeAndDate[0]
        this.day = timeAndDate[1]
      }

      if (this.DataSubmit) {
        let lastUpdateValue = this.DataSubmit.data.UpdatedAt.split(' ')
        let hourAndMinute = lastUpdateValue[0].split(':')
        this.hour = hourAndMinute[0]
        this.minute = hourAndMinute[1]
        this.date = lastUpdateValue[1]
      }

      if (this.DataSubmit.data.ConfirmInfos) {
        this.DaiDienBenhVien = this.DataSubmit.data.ConfirmInfos.find(e => e.ConfirmType === 'DaiDienBenhVien')
        this.NhanVienDoDe = this.DataSubmit.data.ConfirmInfos.find(e => e.ConfirmType === 'NhanVienDoDe')
      }
    }
  }
}
</script>
