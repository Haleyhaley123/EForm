import Service from '../../Service'

class GetListAllMonitorSheetForPatient extends Service {
  /**
   * The constructor for the ArtistProxy.
   *
   * @param {Object} parameters The query parameters.
   */
  constructor (parameters = {}) {
    super('api/IPD/MonitoringSheetForPatientsWithExtravasationOfCancerDrugs', parameters)
  }
}

export default GetListAllMonitorSheetForPatient
