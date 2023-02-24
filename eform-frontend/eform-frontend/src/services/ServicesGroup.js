import Service from './Service'

class ServicesGroup extends Service {
  /**
   * The constructor for the ArtistProxy.
   *
   * @param {Object} parameters The query parameters.
   */
  constructor (parameters = {}) {
    super('api/DoctorPlaceDiagnosticsOrder/ServicesGroup', parameters)
  }
}

export default ServicesGroup
