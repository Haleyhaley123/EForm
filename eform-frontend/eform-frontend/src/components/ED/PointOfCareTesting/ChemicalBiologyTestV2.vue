<template>
  <div class="main-content disable_ccp" id="di0-page-print">
    <!--  -->
    <p v-if="Data && Data.Specialty" class="text-center textUppercase">KHOA: {{Data.Specialty.ViName}}</p>
    <p v-else class="text-center textUppercase">KHOA: . . . . . . . . . . . . . . . . . . . . . . . . .</p>
    <div class="text-center"><img class="top-logo" src="/static/Logo Vinmec International Hospitall 1.png" /></div>
    <div class="text-center">
      <table style="width: 400px;margin: 10px auto;" class="tbl-b">
        <tr>
          <td style="padding-left:5px">
            <div style="display: flex; justify-content: center;">
              <!-- <CustomerBarCode v-if="customer" :customer="customer"/> -->
              <VueBarcode v-if="customer.PID" v-bind:value="customer.PID" :height="40" :displayValue="true" :textAlign="'left'" :width="1" :marginTop="5" :fontSize="15"/>
            </div>
            <div>Họ và tên: {{Data.Fullname}}</div>
            <div>Số PID:  {{Data.PID}}</div>
            <div>Ngày tháng năm sinh: {{Data.DateOfBirth}}</div>
            <div style="margin: 0;">Giới tính: {{Data.Gender}}</div>
          </td>
        </tr>
      </table>
    </div>
    <div class="text-center textUppercase font-bold" style="text-align: center; margin-bottom: 10px;">Xét nghiệm tại chỗ/ Point- Of- Care Testing (POCT)</div>
    <p class="text-center font-bold font20" style="margin-bottom: 0;">Sinh hóa máu/ Chemistry (Catridge CHEM8+)</p>
    <p class="text-center font-bold font22" style="margin-bottom: 10px;">Máy/ Equiqment: iSTAT</p>
    <table class="table tbl-b tbl-b2">
      <tr>
        <th rowspan="2">
          <p class="text-center">STT</p>
          <p class="text-center">No.</p>
        </th>
        <th colspan="2" rowspan="2">
          <p class="text-center">Thông số/</p>
          <p class="text-center">Parameters</p>
        </th>
        <th rowspan="2" width="120px">
          <p class="text-center">Khoảng tham chiếu/</p>
          <p class="text-center">Reference Interval</p>
        </th>
        <th colspan="2">
          <p class="text-center">Chỉ số báo động/</p>
          <p class="text-center">Critical results</p>
        </th>
        <th colspan="3">
          <p class="text-center">Kết quả/ Results</p>
        </th>
      </tr>
      <tr>
        <th class="nowrap">
          <p class="text-center">Giá trị thấp/ Low</p>
        </th>
        <th class="nowrap">
          <p class="text-center">Giá trị cao/ High</p>
        </th>
        <th class="nowrap">
          <p class="text-center">Kết quả/</p>
          <p class="text-center">Results</p>
        </th>
        <th>
          <p class="text-center">Đơn vị/</p>
          <p class="text-center">Unit</p>
        </th>
        <th class="text-center" width="30px" style="text-align: left" :rowspan="Data.Datas.length + 1">
          <img style="float: right;" width="20px" src="/static/print-note.png"/>
        </th>
      </tr>
      <tr :data="item" :key="index" v-for="(item, index) in Data.Datas">
        <template v-if="index === 8 || index === 9 || index === 7">
          <td class="align-middle text-center" rowspan="3" v-if="index === 7">{{item.Order}}</td>
          <td class="align-middle text-center" rowspan="3" v-if="index === 7">{{item.Name}}</td>
        </template>
        <template v-else-if="index === 3 || index === 4">
          <td class="align-middle text-center" rowspan="2" v-if="index === 3">{{item.Order}}</td>
          <td class="align-middle text-center" rowspan="2" v-if="index === 3">Ion Calcium (iCa<sup>2+</sup>)</td>
        </template>
        <template v-else-if="index === 2">
          <td class="align-middle text-center" v-if="index === 2">{{item.Order}}</td>
          <td class="align-middle text-center" v-if="index === 2">Cl<sup>-</sup></td>
        </template>
        <template v-else>
          <td class="text-center">{{item.Order}}</td>
          <td class="text-center">{{item.Name}}</td>
        </template>
        <td style="width: 80px;">{{item.ViAge}}</td>
        <template v-if="index === 8 || index === 9 || index === 7">
          <td class="align-middle" rowspan="3" v-if="index === 7">
            {{item.LowerLimit || 'Không áp dụng'}} - {{item.HigherLimit || 'Không áp dụng'}}
          </td>
          <td class="align-middle" rowspan="3" v-if="index === 7">{{item.LowerAlert ? '≤' + item.LowerAlert : 'Không áp dụng'}}</td>
        </template>
        <template v-else-if="index === 1">
          <td class="align-middle" v-if="index === 1">
            {{item.LowerLimit || 'Không áp dụng'}} - {{item.HigherLimit || 'Không áp dụng'}}
          </td>
          <td>{{item.LowerAlert ? '≤' + item.LowerAlert : 'Không áp dụng'}}</td>
          <td>{{item.HigherAlert ? '≥' + item.HigherAlert + '.0' : 'Không áp dụng'}}</td>
        </template>
        <template v-else-if="index === 2">
          <td class="align-middle" v-if="index === 2">
            {{item.LowerLimit || 'Không áp dụng'}} - {{item.HigherLimit || 'Không áp dụng'}}
          </td>
          <td class="align-middle text-center" colspan="2" v-if="index === 2">{{item.LowerAlert ? '≤' + item.LowerAlert : 'Không áp dụng'}}</td>
        </template>
        <template v-else-if="index === 6">
          <td class="align-middle" v-if="index === 6">
            {{item.LowerLimit || 'Không áp dụng'}} - {{item.HigherLimit || 'Không áp dụng'}}
          </td>
          <td class="align-middle text-center" colspan="2" v-if="index === 6">{{item.LowerAlert ? '≤' + item.LowerAlert : 'Không áp dụng'}}</td>
        </template>
        <template v-else>
          <template v-if="index === 3 || index === 4">
            <td class="align-middle" rowspan="2" v-if="index === 3">{{item.LowerLimit || 'Không áp dụng'}} - {{item.HigherLimit || 'Không áp dụng'}}</td>
          </template>
          <template v-else>
            <td>
              {{item.LowerLimit || 'Không áp dụng'}} - {{item.HigherLimit || 'Không áp dụng'}}
            </td>
          </template>
          <td>{{item.LowerAlert ? '≤' + item.LowerAlert : 'Không áp dụng'}}</td>
        </template>
        <td :class="{'only-web-page': index === 6 || index === 2 || index === 1}">{{item.HigherAlert ? '≥' + item.HigherAlert : 'Không áp dụng'}}</td>
        <td>
          {{item.Result}}
        </td>
        <template v-if="index === 8 || index === 9 || index === 7">
          <td class="align-middle" rowspan="3" v-if="index === 7">{{item.Unit}}</td>
        </template>
        <template v-else>
          <td>{{item.Unit}}</td>
        </template>
        <!-- <td width="100px" v-if="index === 0" :rowspan="Data.Datas.length">
        </td> -->
      </tr>
    </table>
    <br/><br/>
    <table class="table">
      <tr>
        <td width="50%" class="text-center">
          <template v-if="Data.ExecutionTime">
            {{Data.ExecutionTime | viDisplayformatDateTime}}
          </template>
          <template v-else>
            .....giờ......., ngày....... tháng ....... năm ......
          </template>
          <p class="text-center font-bold">Người làm xét nghiệm</p>
          <br/><br/><br/>
          <p class="text-center" v-if="Data.ExecutionUser">{{handleName(Data.ExecutionUser.Fullname)}}</p>
        </td>
        <td class="text-center">
          <template v-if="Data.AcceptTime">
            {{Data.AcceptTime | viDisplayformatDateTime}}
          </template>
          <template v-else>
            .....giờ......., ngày....... tháng ....... năm ......
          </template>
          <p class="text-center font-bold">Bác sĩ chỉ định</p>
          <br/><br/><br/>
          <p v-if="Data.DoctorAccept && Data.DoctorAccept.Fullname" class="text-center">{{handleName(Data.DoctorAccept.Fullname)}}</p>
          <p v-else class="text-center">{{'..............................'}}</p>
        </td>
      </tr>
    </table>
    <template v-if="Data.Upload && Data.Upload.length">
      <div style="page-break-before: always"></div>
      <p>Hình ảnh kết quả xét nghiệm sinh hóa máu</p>
      <div :data="img" :key="index" v-for="(img, index) in Data.Upload">
        <p class="text-center">
          <img style="max-width: 70%" :src="getImg(img)" />
        </p>
      </div>
    </template>
  </div>
</template>
<script>
/*
 * The home index page.
 */
import VueBarcode from 'vue-barcode'
// import moment from 'moment'
export default {
  /**
   * The name of the page.
   */
  name: 'ChemicalBiologyTestV2',
  components: {
    VueBarcode
  },
  props: ['Data'],
  data () {
    return {
      customer: {}
    }
  },
  mounted () {
    this.customer = this.$store.state.account.CurrentPatientObj
    // if (this.Data) {
    //   this.Data.UpdateAt = moment(this.Data.UpdateAt).format('HH:MM DD/MM/YYYY')
    // }
  },
  computed: {
  },
  methods: {
    getImg (img) {
      let url = ''
      if (img) {
        url = img.split('').reverse().join('').replace('.', '/').split('').reverse().join('')
      }
      return url
    },
    handleName (Fullname) {
      if (Fullname) {
        return Fullname.split('(')[0]
      }
      return ''
    }
  }
}
</script>
