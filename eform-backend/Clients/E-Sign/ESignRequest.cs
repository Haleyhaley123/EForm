using Clients.EsignService;
using EMRModels;
using Helper;
using Helper.ESign;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Clients.E_Sign
{
    public class ESignRequest
    {
        public static EsignResponse SendRequet(Guid id, EsignModel data)
        {
            var signed = Execute(id, data);
            if (signed.code == "200")
            {
                return new EsignResponse()
                {
                    base64 = signed.data,
                    b_id = id.ToString(),
                    code = signed.code
                };
            }
            else
            {
                return new EsignResponse()
                {
                    base64 = null,
                    b_id = id.ToString(),
                    code = signed.code
                };
            }
        }
        public static responseReturn Execute(Guid id, EsignModel data)
        {
            string b_Id = id.ToString();
            BasicHttpBinding binding = new BasicHttpBinding();
            binding.MaxReceivedMessageSize = 20000000;
            //Specify the address to be used for the client.
            EndpointAddress address = new EndpointAddress("http://10.111.125.83/Service/kyso/serviceEsign.asmx");
            var client = new serviceSignFileSoapClient(binding, address);
            var sign = client.API_ESIGN_SERVER(data.userName,
                    data.passWord,
                    data.b_dvi,
                    data.b_vaitro,
                    b_Id,
                    data.dataBase64,
                    "",
                    "pdf",
                    data.typeSign,
                    data.locationKey,
                    "",
                    "",
                    data.withImg,
                    data.heightImg,
                    data.pageIndex,
                    data.bottompos,
                    data.sw,
                    data.typefollow,
                    data.userNameky,
                    "",
                    data.followCode,
                    data.link_callback,
                    data.signobj,
                    data.tempmail_code,
                    data.invisible);
            HandleApiILog.ESignToTmpLog(data.userName, data.passWord, data.b_dvi, data.b_vaitro, b_Id, data.dataBase64, "", "pdf", data.typeSign, data.locationKey, "", "", data.withImg, data.heightImg, data.pageIndex, data.bottompos, data.sw, data.typefollow, data.userNameky, "", data.followCode, data.link_callback, data.signobj, data.tempmail_code, data.invisible, sign.code);
            return sign;
        }
        public static string CheckStatus(Guid id, StatusRequestModel data)
        {
            string b_Id = id.ToString();
            BasicHttpBinding binding = new BasicHttpBinding();
            binding.MaxReceivedMessageSize = 20000000;
            //Specify the address to be used for the client.
            EndpointAddress address =
               new EndpointAddress("http://10.111.125.83/Service/kyso/serviceEsign.asmx");
            var client = new EsignService.serviceSignFileSoapClient(binding, address);
            var sign = client.Fs_TRANGTHAI(data.userName, data.passWord, b_Id);
            return sign;
        }
        public static ArrayOfAnyType CancelSign(Guid id, StatusRequestModel data)
        {
            string b_Id = id.ToString();
            BasicHttpBinding binding = new BasicHttpBinding();
            binding.MaxReceivedMessageSize = 20000000;
            //Specify the address to be used for the client.
            EndpointAddress address =
               new EndpointAddress("http://10.111.125.83/Service/kyso/serviceEsign.asmx");
            var client = new EsignService.serviceSignFileSoapClient(binding, address);
            var sign = client.Fs_QUYTRINH_KC(data.userName, data.passWord, b_Id);
            return sign;
        }
    }
}
