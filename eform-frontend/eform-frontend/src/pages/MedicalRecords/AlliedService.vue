<template>
  <div>
    <div class="mrb10 flex align-center" style="justify-content: end;">
      <div class="fw600 mrr5">{{__t('Từ ngày ')}}:</div>
      <VDateTimePicker5 class="mrr20" :max="getMax()" v-model="parameters.FromDate" :format="vDateTimeFormat" :placeholder="__t('Từ ngày')" :showshortcuts="false"/>
      <div class="fw600 mrr5">{{__t('Đến ngày ')}}:</div>
      <VDateTimePicker5 class="mrr20" v-model="parameters.ToDate" :format="vDateTimeFormat" :placeholder="__t('Đến ngày ')" :showshortcuts="false"/>
      <div class="fw600 mrr5" v-if="!$route.params.VisitCode">{{__t('Lượt tiếp nhận')}}:</div>
      <VNumberInput class="mrr20" v-if="!$route.params.VisitCode" style="width: 200px" maxlength="10" v-model="parameters.VisitCode" :placeholder="__t('nhập')"/>
      <VSelect  :disabeleclear="false" v-model="parameters.ServiceGroupId" :placeholder="__t('Chọn loại dịch vụ   ')" :items="ServiceGroupIds"/>
      <div class="flex align-center" style="justify-content: end;flex-grow: 1;">
        <button class="btn v-yellow-btn mrr10" type="button" @click="getData">{{__t('home.loc')}}</button>
        <button class="btn btn-default" type="button" @click="resetFilter">{{__t('home.reset')}}</button>
      </div>
    </div>
    <br>
    <table class="table table-striped table-bordered v-table none-row-curso" id="customer-tbl">
      <thead class="bg-1375ba">
        <tr>
          <th width="1px" class="text-center">{{__t('STT')}}</th>
          <th class="text-center">{{__t('Tên chỉ định')}}</th>
          <th width="150px" class="text-center">{{__t('Người chỉ định')}}</th>
          <th width="120px" class="text-center">{{__t('Ngày chỉ định')}}</th>
          <th width="120px" class="text-center">{{__t('Lượt tiếp nhận ')}}</th>
          <th width="200px" class="text-center">{{__t('Nơi chỉ định ')}}</th>
        </tr>
      </thead>
      <tbody v-if="DataTable.length > 0">
        <tr  :key="index" v-for="(item, index) in DataTable">
          <td class="text-center">
            {{ (parameters.PageNumber - 1) * parameters.PageSize + (index + 1) }}
          </td>
          <td>{{getServiceName(item)}}</td>
          <td><ad-Info :ad="item.ServiceBy" /></td>
          <td>{{item.ServiceAt | formatDateWithoutSecon}}</td>
          <td>{{item.VisitCode}}</td>
          <td>{{item.LocationViName}}</td>
        </tr>
      </tbody>
      <tbody v-else>
        <tr>
          <td colspan="6" class="text-center">{{ __t("Không có thông tin! ") }}</td>
        </tr>
      </tbody>
      <tfoot v-if="DataTable && DataTable.length > 0">
        <tr>
          <td colspan="9">
          <template>
            <div class="flex space-between">
            <div>
              <paginate
              :clickHandler="getData"
              :container-class="'pagination pagination-sm no-margin v-pagination'"
              :next-text="'»'"
              :page-count="parameters.Pages"
              :prev-text="'«'"
              v-if="DataTable.length > 0"
              v-model="parameters.PageNumber"
              >
              </paginate>
            </div>
            <div class="box-record">
              <p>{{__t('Show')}}: {{ DataTable.length + "/" + count }} {{__t('chỉ định')}}</p>
            </div>
            </div>
          </template>
          </td>
        </tr>
      </tfoot>
    </table>
  </div>
</template>

<script>
import MedicalRecordAlliedService from '@/services/MedicalRecordAlliedService'
import MixinMasterData from '@/mixins/masterdata.js'
import MixinForm from '@/mixins/form.js'
import moment from 'moment'
import VDateTimePicker5 from '@/components/VDateTimePicker5.vue'
import VSelect from '@/components/VSelect.vue'
import DoctorPlaceDiagnosticsOrder from '@/services/DoctorPlaceDiagnosticsOrder'

export default {
  name: 'AlliedService',
  props: ['PID'],
  mixins: [MixinMasterData, MixinForm],
  components: { VDateTimePicker5, VSelect },
  data () {
    return {
      Loaded: false,
      DataTable: [],
      parameters: {
        hidemsg: true,
        notRedirect: true,
        PID: this.$route.params.Id,
        PageNumber: 1,
        Pages: 1,
        Type: '2',
        FromDate: moment(moment().subtract(7, 'day').startOf('day')).format(process.env.DATETIME_FORMAT),
        ToDate: moment(new Date()).format(process.env.DATETIME_FORMAT),
        PageSize: 30,
        VisitCode: '',
        ServiceGroupId: ''
      },
      count: 0,
      ServiceGroupIds: []
    }
  },
  mounted () {
    if (this.$route.params.VisitCode) {
      this.parameters.FromDate = ''
      this.parameters.ToDate = ''
    }
    this.getData()
    this.getCategoryList()
  },
  methods: {
    getData () {
      if (this.parameters.FromDate) {
        this.parameters.FromDate = `${this.parameters.FromDate}`
        if (this.parameters.FromDate.includes('Invalid')) {
          this.parameters.FromDate = ''
        }
        if (this.parameters.FromDate.includes('(')) {
          this.parameters.FromDate = this.$options.filters.formatDateWithoutSecon(this.parameters.FromDate)
        }
      }
      if (this.parameters.ToDate) {
        this.parameters.ToDate = `${this.parameters.ToDate}`
        if (this.parameters.ToDate.includes('Invalid')) {
          this.parameters.ToDate = ''
        }
        if (this.parameters.ToDate.includes('(')) {
          this.parameters.ToDate = this.$options.filters.formatDateWithoutSecon(this.parameters.ToDate)
        }
      }
      if (this.PID) {
        this.parameters.PID = this.PID
      }
      if (this.$route.params.VisitCode) {
        if (this.$route.params.VisitCode === 'N/A') {
          this.parameters.VisitCode = ''
        } else {
          this.parameters.VisitCode = this.$route.params.VisitCode
        }
      }
      new MedicalRecordAlliedService(this.parameters).find('').then(response => {
        if (response) {
          if (response.Data) {
            this.DataTable = response.Data
            this.count = response.Count
            var bonus = 1
            if ((response.Count % this.parameters.PageSize) === 0) {
              bonus = 0
            }
            this.parameters.Pages = parseInt(response.Count / this.parameters.PageSize) + bonus
          } else {
            console.log('abc')
            this.DataTable = []
          }
        } else {
          this.DataTable = []
        }
        this.Loaded = false
      }).catch(e => {
        console.log(e)
        this.DataTable = []
        this.Loaded = false
      })
    },
    getMax () {
      if (this.parameters.ToDate && this.parameters.ToDate !== 'Invalid date') {
        return this.parameters.ToDate
      }
      return ''
    },
    resetFilter () {
      this.parameters = {
        PID: this.$route.params.Id,
        PageNumber: 1,
        Pages: 1,
        Type: '2',
        FromDate: '',
        ToDate: '',
        PageSize: 30
      }
      this.getData()
    },
    getServiceName (item) {
      let name = ''
      if (item && item.ViName) {
        if (item.Code) {
          name += item.Code + ' - '
        }
        name += item.ViName
      }
      return name
    },
    getCategoryList () {
      new DoctorPlaceDiagnosticsOrder({
        Type: '0',
        PageSize: 2000
      }).find('ServicesGroup?Type=2&PageSize=2000').then(response => {
        response.items.map(item => this.ServiceGroupIds.push({
          value: item.Id,
          label: item.ViName
        }))
      }).catch((e) => {
        console.log(e)
      })
      console.log(this.ServiceGroupIds)
    }
  }
}
</script>
