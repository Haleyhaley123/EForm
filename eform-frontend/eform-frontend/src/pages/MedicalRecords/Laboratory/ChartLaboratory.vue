<template>
  <div id="PageChart-Page" class="page-chart">
    <div class="box-content">
      <div class="box-chart">
        <line-chart
          :series="series"
          :chartOptionsArea="chartOptionsArea"
          :hasBrushChart="false"
        />
      </div>
    </div>
  </div>
</template>
<script>
import LineChart from '@/components/chart/AplexChart.vue'
import moment from 'moment'
import { cloneDeep } from 'lodash'
export default {
  name: 'PageChart',
  components: { LineChart },
  props: {
    Datas: {}
  },
  // watch: {
  //   series: {
  //     handler (val) {
  //       console.log('series', val)
  //     },
  //     deep: true
  //   },
  //   chartOptionsArea: {
  //     handler (val) {
  //       console.log('chartOptionsArea', val)
  //     },
  //     deep: true
  //   }
  // },
  data () {
    return {
      DataChart: {},
      totalLstBreathRate: [],
      dates: [],
      labelsMach: [],
      dataNhipTho: [],
      series: [
        {
          name: 'Nhịp thở',
          data: []
        }
      ],
      chartOptionsArea: {
        chart: {
          id: 'chartAreaNhipTho',
          toolbar: {
            autoSelected: 'pan',
            show: false
          }
        },
        colors: ['#4BC0C0'],
        stroke: {
          width: 3
        },
        dataLabels: {
          enabled: true
        },
        fill: {
          opacity: 1
        },
        markers: {
          size: 0
        },
        legend: {
          show: true,
          position: 'top'
        },
        yaxis: {
          title: {
            text: '',
            style: {
              fontWeight: 'bold',
              fontFamily: 'Helvetica Neue Light,HelveticaNeue-Light,Helvetica Neue,Calibri,Helvetica,Arial'
            }
          }
        },
        xaxis: {
          // type: 'datetime',
          title: {
            text: 'Ngày trả kết quả',
            style: {
              fontWeight: 'bold',
              fontFamily: 'Helvetica Neue Light,HelveticaNeue-Light,Helvetica Neue,Calibri,Helvetica,Arial'
            }
          },
          // labels: {
          //   formatter: function (value) {
          //     // return moment(value).format('HH:mm DD/MM/YYYY')
          //     return value
          //   }
          // },
          categories: []
        }
      }
    }
  },
  mounted () {
    this.getDataChart()
  },
  methods: {
    updateChart () {
      let dates = this.dates.map(date => {
        return moment(date).format('HH:mm DD/MM/YYYY')
      })
      let totalLstBreathRate = this.totalLstBreathRate
      let result = []

      for (let i = 0; i < Math.min(dates.length); i++) {
        // result.push([new Date(dates[i]).getTime(), totalLstBreathRate[i]])
        result.push(totalLstBreathRate[i])
      }
      this.series = [{
        name: this.DataChart[0].TestNameV,
        data: result
      }]
      this.chartOptionsArea = { ...this.chartOptionsArea,
        ...{
          dataLabels: {
            enabled: true,
            formatter: function (val, opt) {
              return val
            }
          },
          xaxis: {
            categories: dates
          }
        }
      }
    },
    async getDataChart () {
      this.DataChart = []
      this.DataChart = await cloneDeep(this.Datas)
      if (this.DataChart) {
        this.dates = await this.DataChart.map(d => d.ResultAt ? moment(d.ResultAt).format() : null)
        this.totalLstBreathRate = await this.DataChart.map(item => item.Result ? item.Result : 'N/A')
      }
      this.updateChart()
    }
  }
}
</script>
<style lang="stylus" scoped>
</style>
