import Service from './Service'

class Allergy extends Service {
  /**
   * The constructor for the ArtistProxy.
   *
   * @param {Object} parameters The query parameters.
   */
  constructor (parameters = {}) {
    super('api/Unlock/Vip/', parameters)
  }
}

export default Allergy
