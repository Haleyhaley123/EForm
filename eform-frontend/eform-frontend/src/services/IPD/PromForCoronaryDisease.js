import Service from '../Service'

class PromForCoronaryDisease extends Service {
  /**
   * The constructor for the ArtistProxy.
   *
   * @param {Object} parameters The query parameters.
   */
  constructor (parameters = {}) {
    super(`api/${parameters.VisitType}/PROMForCoronaryDisease`, parameters)
  }
}

export default PromForCoronaryDisease
