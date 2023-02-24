import Service from './Service'

class Clinics extends Service {
  /**
   * The constructor for the ArtistProxy.
   *
   * @param {Object} parameters The query parameters.
   */
  constructor (parameters = {}) {
    super('api/GetLISInforByPID', parameters)
  }
}

export default Clinics
