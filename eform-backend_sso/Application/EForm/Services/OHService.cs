using DataAccess.Models.GeneralModel;
using EForm.BaseControllers;
using EForm.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Configuration;
using DataAccess.Models;
using EForm.Client;

namespace EForm.Services
{
    public class OHService : BaseApiController
    {
        private readonly string DrsPrefix = ConfigurationManager.AppSettings["DRS_PREFIX"] != null ? ConfigurationManager.AppSettings["DRS_PREFIX"].ToString() : "DRS";
        private readonly int MaximumNumberOfItemPerRequest = ConfigurationManager.AppSettings["MaximumNumberOfItemPerRequest"] != null ? int.Parse(ConfigurationManager.AppSettings["MaximumNumberOfItemPerRequest"].ToString()) : 20;
        #region Lab service
        public OHServiceResult PlaceLabOrder(Guid[] chargeItemId, Guid? ChargeId = null)
        {
            var ok = 0;
            var failed = 0;
            var listChargeItem = ChargeId == null ? unitOfWork.ChargeItemRepository.Find(c => chargeItemId.Contains(c.Id)).ToList() : unitOfWork.ChargeItemRepository.Find(c => c.ChargeId == ChargeId && c.ChargeItemType == Constant.ChargeItemType.Lab).ToList();
            var sum = listChargeItem.Count;
            var total = listChargeItem.Count;

            if (sum > 0)
            {
                var firstChargeItem = listChargeItem[0];
                var position = 0;

                do
                {
                    var count = sum > MaximumNumberOfItemPerRequest ? MaximumNumberOfItemPerRequest : sum;
                    var listChargeI = listChargeItem.GetRange(position, count);
                    string docTemplate = @"<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"" xmlns:lab=""http://orionhealth.com/laboratory"" xmlns:cor=""http://schemas.orionhealth.com/2020/08/01/ws/platform/coretypes"" xmlns:dir=""http://schemas.orionhealth.com/2017/08/01/ws/common/directory"" xmlns:cpoe=""http://orionhealth.com/ancillaryservices/cpoe"">
                       <soap:Header xmlns=""http://schemas.orionhealth.com/2019/12/01/ws/platform/authentication"" xmlns:wsa=""http://www.w3.org/2005/08/addressing"">
                        <SecurityHeader><ActAsUser>vingroup\{ActAsUser}</ActAsUser></SecurityHeader>
                        <wsa:Action>{OHLaboratoryServiceAddOrderURL}</wsa:Action>
                        <wsa:To>{OHLaboratoryServiceURL}</wsa:To>
                       </soap:Header>
                       <soap:Body>
                          <lab:AddPlacerOrder>
                             <lab:laborder>
                                <lab:Id>{Id}</lab:Id>
                                <lab:PatientId>{PatientId}</lab:PatientId>
                                <lab:PatientVisitId>{PatientVisitId}</lab:PatientVisitId>
                                <lab:PatientLocationId>{PatientLocationId}</lab:PatientLocationId>
                                <lab:When>{When}</lab:When>
                                <lab:EnteredBy>
                                   <cor:Value>{EnteredBy}</cor:Value>
                                   <cor:System>clinical-portal-user</cor:System>
                                   <dir:IdentityType>UserName</dir:IdentityType>
                                </lab:EnteredBy>
                                <lab:RequestedBy>
                                   <cor:Value>{RequestedBy}</cor:Value>
                                   <cor:System>clinical-portal-user</cor:System>
                                   <dir:IdentityType>UserName</dir:IdentityType>
                                </lab:RequestedBy>
                                <lab:Orders>
                                   {OrderAction}
                                </lab:Orders>
                             </lab:laborder>
                          </lab:AddPlacerOrder>
                       </soap:Body>
                    </soap:Envelope>";
                    docTemplate = docTemplate.Replace("{OHLaboratoryServiceAddOrderURL}", ConfigurationManager.AppSettings["OHLaboratoryServiceAddOrderURL"]);
                    docTemplate = docTemplate.Replace("{OHLaboratoryServiceURL}", ConfigurationManager.AppSettings["OHLaboratoryServiceURL"]);
                    docTemplate = docTemplate.Replace("{ActAsUser}", firstChargeItem.CreatedBy);
                    docTemplate = docTemplate.Replace("{Id}", firstChargeItem.ChargeId.ToString());
                    docTemplate = docTemplate.Replace("{PatientId}", firstChargeItem.PatientId);
                    docTemplate = docTemplate.Replace("{PatientVisitId}", firstChargeItem.PatientVisitId.ToString());
                    docTemplate = docTemplate.Replace("{PatientLocationId}", firstChargeItem.PatientLocationId.ToString());
                    docTemplate = docTemplate.Replace("{When}", firstChargeItem.CreatedAt.Value.ToString("s"));
                    docTemplate = docTemplate.Replace("{EnteredBy}", firstChargeItem.CreatedBy);
                    docTemplate = docTemplate.Replace("{RequestedBy}", firstChargeItem.CreatedBy);
                    string orders = string.Empty;
                    foreach (var c in listChargeI)
                    {
                        string orderActionTemplate = @"<cpoe:OrderAction>
                                      <cpoe:PlacedBy>
                                         <cor:Value>{PlacedBy}</cor:Value>
	                                    <cor:System>clinical-portal-user</cor:System>
	                                    <dir:IdentityType>UserName</dir:IdentityType>
                                      </cpoe:PlacedBy>
                                      <cpoe:RequestedBy>
                                         <cor:Value>{RequestedBy}</cor:Value>
	                                    <cor:System>clinical-portal-user</cor:System>
	                                    <dir:IdentityType>UserName</dir:IdentityType>
                                      </cpoe:RequestedBy>
                                      <cpoe:PlacerOrderableId>{PlacerOrderableId}</cpoe:PlacerOrderableId>
                                      <cpoe:ServiceDepartmentId>{ServiceDepartmentId}</cpoe:ServiceDepartmentId>
                                      <cpoe:Priority>{Priority}</cpoe:Priority>
                                      <cpoe:Comments>{Comments}</cpoe:Comments>
                                      <cpoe:Instructions>{Instructions}</cpoe:Instructions>
                                      <cpoe:Reason>{Reason}</cpoe:Reason>
                                      <cpoe:PlacerNumber>{PlacerNumber}</cpoe:PlacerNumber>
                                      <cpoe:AdditionalInformation>{AdditionalInformation}</cpoe:AdditionalInformation>
                                   </cpoe:OrderAction>";
                        orderActionTemplate = orderActionTemplate.Replace("{PlacedBy}", c.CreatedBy);
                        orderActionTemplate = orderActionTemplate.Replace("{RequestedBy}", c.CreatedBy);
                        orderActionTemplate = orderActionTemplate.Replace("{PlacerOrderableId}", c.PlacerOrderableId.ToString());
                        orderActionTemplate = orderActionTemplate.Replace("{ServiceDepartmentId}", c.ServiceDepartmentId.ToString());
                        orderActionTemplate = orderActionTemplate.Replace("{Priority}", c.Priority);
                        orderActionTemplate = orderActionTemplate.Replace("{Comments}", EscapeXml(c.Comments));
                        orderActionTemplate = orderActionTemplate.Replace("{Instructions}", EscapeXml(c.Instructions));
                        orderActionTemplate = orderActionTemplate.Replace("{Reason}", EscapeXml(c.Reason));
                        orderActionTemplate = orderActionTemplate.Replace("{PlacerNumber}", DrsPrefix + c.PlacerIdentifyNumber.ToString());
                        orderActionTemplate = orderActionTemplate.Replace("{AdditionalInformation}", EscapeXml(c.InitialDiagnosis));
                        orders += orderActionTemplate;
                    }
                    if (orders != string.Empty)
                    {
                        docTemplate = docTemplate.Replace("{OrderAction}", orders);
                        var res = MakeSoapRequest(docTemplate);
                        if (res.Key == HttpStatusCode.OK)
                        {
                            var xmlResponse = XDocument.Parse(res.Value);
                            XNamespace b = "http://orionhealth.com/ancillaryservices/cpoe";
                            XNamespace c = "http://schemas.orionhealth.com/2020/08/01/ws/platform/coretypes";
                            foreach (XElement element in xmlResponse.Descendants(b + "Order"))
                            {
                                var placerNumber = element.Element(b + "Placer").Element(c + "Value").Value;
                                var filler = element.Element(b + "Filler").Element(c + "Value").Value;
                                var fillerGroup = element.Element(b + "FillerGroup").Element(c + "Value").Value;
                                var findCharge = listChargeItem.SingleOrDefault(i => placerNumber == DrsPrefix + i.PlacerIdentifyNumber);
                                if (findCharge != null)
                                {
                                    findCharge.Filler = filler;
                                    findCharge.FillerGroup = fillerGroup;
                                    findCharge.Status = Constant.ChargeItemStatus.Placed;
                                    // unitOfWork.ChargeItemRepository.Update(findCharge);
                                }
                            }
                            unitOfWork.Commit();
                            ok += listChargeI.Count;
                        }
                        else
                        {
                            var reason = string.Empty;
                            try
                            {
                                var xmlResponse = XDocument.Parse(res.Value);
                                if (xmlResponse != null)
                                {
                                    XNamespace s = "http://www.w3.org/2003/05/soap-envelope";
                                    foreach (XElement element in xmlResponse.Descendants(s + "Fault"))
                                    {
                                        reason = element.Element(s + "Reason").Element(s + "Text").Value;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                CustomLog.Error(string.Format("Parsing XML Response error: {0}", ex.Message));
                                reason = res.Value;
                            }
                            foreach (var ci in listChargeI)
                            {
                                ci.Status = Constant.ChargeItemStatus.Failed;
                                ci.FailedReason = res.Key.ToString() + (reason != string.Empty ? ", " + reason : string.Empty);
                                // unitOfWork.ChargeItemRepository.Update(ci);
                            }
                            unitOfWork.Commit();
                            failed += listChargeI.Count;
                        }
                    }
                    sum = sum - count;
                    position += count;
                } while (sum > 0);
            }
            unitOfWork.Commit();
            return new OHServiceResult(ok, failed, total);
        }

        public OHServiceResult CancelLabOrder(List<ChargeItem> listChargeItem)
        {
            var ok = 0;
            var failed = 0;
            var username = getUsername();
            if (listChargeItem.Count > 0)
            {
                var firstChargeItem = listChargeItem[0];
                string docTemplate = @"<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"" xmlns:lab=""http://orionhealth.com/laboratory"" xmlns:cor=""http://schemas.orionhealth.com/2020/08/01/ws/platform/coretypes"" xmlns:dir=""http://schemas.orionhealth.com/2017/08/01/ws/common/directory"" xmlns:cpoe=""http://orionhealth.com/ancillaryservices/cpoe"">
                       <soap:Header xmlns=""http://schemas.orionhealth.com/2019/12/01/ws/platform/authentication"" xmlns:wsa=""http://www.w3.org/2005/08/addressing"">
                        <SecurityHeader><ActAsUser>vingroup\{ActAsUser}</ActAsUser></SecurityHeader>
                        <wsa:Action>{OHLaboratoryServiceCancelURL}</wsa:Action>
                        <wsa:To>{OHLaboratoryServiceURL}</wsa:To>
                       </soap:Header>
                       <soap:Body>
                          <lab:CancelPlacerOrder>
                            {Order}
                          </lab:CancelPlacerOrder>
                       </soap:Body>
                    </soap:Envelope>";
                docTemplate = docTemplate.Replace("{ActAsUser}", firstChargeItem.DeletedBy);
                docTemplate = docTemplate.Replace("{OHLaboratoryServiceCancelURL}", ConfigurationManager.AppSettings["OHLaboratoryServiceCancelURL"]);
                docTemplate = docTemplate.Replace("{OHLaboratoryServiceURL}", ConfigurationManager.AppSettings["OHLaboratoryServiceURL"]);
                string orders = string.Empty;
                foreach (var c in listChargeItem)
                {
                    string orderActionTemplate = @"<lab:order>
                                                    <cpoe:FillerNumber>{Filler}</cpoe:FillerNumber>
                                                    <cpoe:CanceledOn>{CanceledOn}</cpoe:CanceledOn>
                                                    <cpoe:CreditCharge>1</cpoe:CreditCharge>
                                                    <cpoe:SuppressResult>0</cpoe:SuppressResult>
                                                    <cpoe:CancelReasonCode>8F14D</cpoe:CancelReasonCode>
                                                    <cpoe:Comment>{Comment}</cpoe:Comment>
                                                 </lab:order>";
                    orderActionTemplate = orderActionTemplate.Replace("{Filler}", c.Filler);
                    orderActionTemplate = orderActionTemplate.Replace("{CanceledOn}", DateTime.Now.ToString("s"));
                    orderActionTemplate = orderActionTemplate.Replace("{Comments}", EscapeXml(c.Comments));
                    orders = orderActionTemplate;
                    var body = docTemplate.Replace("{Order}", orders);
                    var res = MakeSoapRequest(body);
                    if (res.Key == HttpStatusCode.OK)
                    {
                        var xmlResponse = XDocument.Parse(res.Value);
                        XNamespace s = "http://orionhealth.com/laboratory";
                        XNamespace i = "http://schemas.orionhealth.com/2017/08/01/ws/common/directory";

                        foreach (XElement element in xmlResponse.Descendants(s + "CancelPlacerOrderResult"))
                        {
                            var filler = element.Element(i + "Identifier").Value;
                            var resultDateTime = element.Element(i + "ResultDateTime").Value;
                            var statusCode = element.Element(i + "StatusCode").Value;
                            var statusMessage = element.Element(i + "StatusMessage").Value;
                            if (statusCode == "CR")
                            {
                                c.CancelFailedReason = string.Empty;
                                c.Status = Constant.ChargeItemStatus.Cancelled;
                                c.CancelBy = username;
                                ok++;
                            }
                            else
                            {
                                c.CancelFailedReason = statusMessage;
                                c.FailedReason = c.CancelFailedReason;
                                c.DeletedBy = null;
                                failed++;
                            }

                            unitOfWork.ChargeItemRepository.Update(c);
                        }
                    }
                    else
                    {
                        var reason = string.Empty;
                        try
                        {
                            var xmlResponse = XDocument.Parse(res.Value);
                            if (xmlResponse != null)
                            {
                                XNamespace s = "http://www.w3.org/2003/05/soap-envelope";
                                foreach (XElement element in xmlResponse.Descendants(s + "Fault"))
                                {
                                    reason = element.Element(s + "Reason").Element(s + "Text").Value;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            CustomLog.Error(string.Format("Parsing XML Response error: {0}", ex.Message));
                            reason = res.Value;
                        }
                        c.CancelFailedReason = res.Key.ToString() + (reason != string.Empty ? ", " + reason : string.Empty);
                        c.FailedReason = c.CancelFailedReason;
                        unitOfWork.ChargeItemRepository.Update(c);
                        failed++;
                    }
                }
                unitOfWork.Commit();
            }
            return new OHServiceResult(ok, failed, listChargeItem.Count);
        }
        #endregion

        #region Allied service
        public OHServiceResult PlaceAlliedOrder(Guid[] chargeItemId, Guid? ChargeId = null)
        {
            var ok = 0;
            var failed = 0;
            var listChargeItem = ChargeId == null ? unitOfWork.ChargeItemRepository.Find(c => chargeItemId.Contains(c.Id)).ToList() : unitOfWork.ChargeItemRepository.Find(c => c.ChargeId == ChargeId && c.ChargeItemType == Constant.ChargeItemType.Allies).ToList();
            var sum = listChargeItem.Count;
            var total = listChargeItem.Count;
            if (sum > 0)
            {
                var firstChargeItem = listChargeItem[0];
                var position = 0;
                var customer = unitOfWork.CustomerRepository.FirstOrDefault(c => c.PID == firstChargeItem.PatientId && !c.IsDeleted);
                do
                {
                    var count = sum > MaximumNumberOfItemPerRequest ? MaximumNumberOfItemPerRequest : sum;
                    var listChargeI = listChargeItem.GetRange(position, count);
                    string docTemplate = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:vm=""http://www.vinmec.com/EnterpriseOrderService/"">
                                           <soapenv:Header/>
                                           <soapenv:Body>
                                              <CreateAlliedServiceOrder>
                                                 <alliedServiceOrder>
                                                    <orderId>{OrderId}</orderId>
                                                    <patient>
                                                       <patientId>{PatientId}</patientId>
                                                       <name>
                                                          <!--Optional:-->
                                                          <first></first>
                                                          <!--Optional:-->
                                                          <last></last>
                                                          <!--Optional:-->
                                                          <middle></middle>
                                                       </name>
                                                       <dateOfBirth>{DateOfBirth}</dateOfBirth>
                                                       <sex>{Sex}</sex>
                                                       <!--Optional:-->
                                                       <streetAddress></streetAddress>
                                                       <!--Optional:-->
                                                       <city></city>
                                                       <!--Optional:-->
                                                       <stateOrProvince></stateOrProvince>
                                                       <!--Optional:-->
                                                       <zipCode></zipCode>
                                                       <!--Optional:-->
                                                       <country></country>
                                                       <!--Optional:-->
                                                       <phone></phone>
                                                    </patient>
                                                    <encounter>
                                                       <facility>{Facility}</facility>
                                                       <visitType>{VisitType}</visitType>
                                                       <visitCode>{VisitCode}</visitCode>
                                                       <attendingDoctor>{AttendingDoctor}</attendingDoctor>
                                                       <patientType></patientType>
                                                       <!--Optional:-->
                                                       <admissionDate>{AdmissionDate}</admissionDate>
                                                       <location></location>
                                                       <!--Optional:-->
                                                       <specialty></specialty>
                                                    </encounter>
                                                    <orderItem>
                                                       {Items}
                                                    </orderItem>
                                                    <enteredBy>{EnteredBy}</enteredBy>
                                                    <orderDate>{OrderDate}</orderDate>
                                                    <requestedDate>{RequestedDate}</requestedDate>
                                                    <!--Optional:-->
                                                    <reason>{Reason}</reason>
                                                    <initialDiagnosis>{InitialDiagnosis}</initialDiagnosis>
                                                    <state>{State}</state>
                                                 </alliedServiceOrder>
                                              </CreateAlliedServiceOrder>
                                           </soapenv:Body>
                                        </soapenv:Envelope>";
                    docTemplate = docTemplate.Replace("{OrderId}", firstChargeItem.ChargeId.ToString());
                    docTemplate = docTemplate.Replace("{PatientId}", firstChargeItem.PatientId);
                    docTemplate = docTemplate.Replace("{DateOfBirth}", customer.DateOfBirth != null ? customer.DateOfBirth.Value.ToString("yyyy-MM-dd") : string.Empty);
                    docTemplate = docTemplate.Replace("{Sex}", customer.Gender == 1 ? "M" : "F");
                    docTemplate = docTemplate.Replace("{Facility}", firstChargeItem.HospitalCode);
                    docTemplate = docTemplate.Replace("{VisitType}", firstChargeItem.VisitType);
                    docTemplate = docTemplate.Replace("{VisitCode}", firstChargeItem.VisitCode);
                    docTemplate = docTemplate.Replace("{AttendingDoctor}", firstChargeItem.DoctorAD);
                    docTemplate = docTemplate.Replace("{AdmissionDate}", firstChargeItem.CreatedAt.Value.ToString("yyyy-MM-dd"));
                    docTemplate = docTemplate.Replace("{EnteredBy}", firstChargeItem.CreatedBy);
                    docTemplate = docTemplate.Replace("{OrderDate}", firstChargeItem.CreatedAt.Value.ToString("s"));
                    docTemplate = docTemplate.Replace("{RequestedDate}", firstChargeItem.CreatedAt.Value.ToString("s"));
                    docTemplate = docTemplate.Replace("{Reason}", EscapeXml(firstChargeItem.Reason));
                    docTemplate = docTemplate.Replace("{InitialDiagnosis}", EscapeXml(firstChargeItem.InitialDiagnosis));
                    docTemplate = docTemplate.Replace("{State}", Constant.AlliesRadRequestStatus.New);

                    string orders = string.Empty;
                    foreach (var c in listChargeI)
                    {
                        string orderActionTemplate = @"<item>
                                                        <orderLineId>{OrderLineId}</orderLineId>
                                                        <itemCode>{ItemCode}</itemCode>
                                                        <itemName>{ItemName}</itemName>
                                                        <serviceCategoryCode>ALLI</serviceCategoryCode>
                                                        <serviceCategoryName>Allied Service</serviceCategoryName>
                                                        <dicomModality></dicomModality>
                                                        <orderStatus>{OrderStatus}</orderStatus>
                                                        <orderMessage></orderMessage>
                                                        <priority>{Priority}</priority>
                                                        <quantity>{quantity}</quantity>
                                                    </item>";
                        orderActionTemplate = orderActionTemplate.Replace("{OrderLineId}", DrsPrefix + c.PlacerIdentifyNumber.ToString());
                        orderActionTemplate = orderActionTemplate.Replace("{ItemCode}", EscapeXml(c.ServiceCode));
                        orderActionTemplate = orderActionTemplate.Replace("{ItemName}", EscapeXml(c.ServiceCode));
                        orderActionTemplate = orderActionTemplate.Replace("{OrderStatus}", Constant.AlliesRadRequestStatus.New);
                        orderActionTemplate = orderActionTemplate.Replace("{Priority}", c.Priority);
                        orderActionTemplate = orderActionTemplate.Replace("{quantity}", c.Quantity);
                        orders += orderActionTemplate;
                    }
                    if (orders != string.Empty)
                    {
                        docTemplate = docTemplate.Replace("{Items}", orders);
                        var res = MakeSoapRequestBasicAuth(docTemplate, ConfigurationManager.AppSettings["OHAlliesServiceURL"], ConfigurationManager.AppSettings["OHAlliesServiceAddOrderURL"]);
                        if (res.Key == HttpStatusCode.OK)
                        {
                            var xmlResponse = XDocument.Parse(res.Value);
                            foreach (XElement element in xmlResponse.Descendants("item"))
                            {
                                var placerNumber = element.Element("orderLineId").Value;
                                var orderStatus = element.Element("orderStatus").Value;
                                var orderMessage = element.Element("orderMessage").Value;
                                var findCharge = listChargeItem.SingleOrDefault(i => placerNumber == DrsPrefix + i.PlacerIdentifyNumber);
                                if (findCharge != null)
                                {
                                    if (orderStatus == Constant.AlliesRadResponseStatus.Placed)
                                    {
                                        findCharge.ChargeDetailId = new Guid(orderMessage);
                                        findCharge.Status = Constant.ChargeItemStatus.Placed;
                                        ok++;
                                    }
                                    else
                                    {
                                        findCharge.FailedReason = orderMessage;
                                        findCharge.Status = Constant.ChargeItemStatus.Failed;
                                        failed++;
                                    }
                                    // unitOfWork.ChargeItemRepository.Update(findCharge);
                                }
                            }
                            unitOfWork.Commit();
                        }
                        else
                        {
                            var reason = string.Empty;
                            try
                            {
                                var xmlResponse = XDocument.Parse(res.Value);
                                if (xmlResponse != null)
                                {
                                    XNamespace b = "http://www.w3.org/2003/05/soap-envelope";
                                    foreach (XElement element in xmlResponse.Descendants(b + "Reason"))
                                    {
                                        reason = element.Element(b + "Text").Value;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                CustomLog.Error(string.Format("Parsing XML Response error: {0}", ex.Message));
                                reason = res.Value;
                            }

                            foreach (var ci in listChargeI)
                            {
                                ci.Status = Constant.ChargeItemStatus.Failed;
                                ci.FailedReason = res.Key.ToString() + (reason != string.Empty ? ", " + reason : string.Empty);
                                // unitOfWork.ChargeItemRepository.Update(ci);
                            }
                            unitOfWork.Commit();
                            failed += listChargeI.Count;
                        }
                    }
                    sum = sum - count;
                    position += count;
                } while (sum > 0);
            }
            unitOfWork.Commit();
            return new OHServiceResult(ok, failed, total);
        }

        public OHServiceResult CancelAlliedOrder(List<ChargeItem> listChargeItem)
        {
            var ok = 0;
            var failed = 0;
            var username = getUsername();
            var sum = listChargeItem.Count;
            var total = listChargeItem.Count;
            if (sum > 0)
            {
                var firstChargeItem = listChargeItem[0];
                var customer = unitOfWork.CustomerRepository.FirstOrDefault(c => c.PID == firstChargeItem.PatientId && !c.IsDeleted);
                var position = 0;
                do
                {
                    var count = sum > MaximumNumberOfItemPerRequest ? MaximumNumberOfItemPerRequest : sum;
                    var listChargeI = listChargeItem.GetRange(position, count);
                    string docTemplate = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:vm=""http://www.vinmec.com/EnterpriseOrderService/"">
                                           <soapenv:Header/>
                                           <soapenv:Body>
                                              <CancelAlliedServiceOrder>
                                                {OrderItem}
                                              </CancelAlliedServiceOrder>
                                           </soapenv:Body>
                                        </soapenv:Envelope>";

                    string orders = string.Empty;
                    foreach (var c in listChargeI)
                    {
                        if (c.ChargeDetailId != null && c.Status == Constant.ChargeItemStatus.Placed)
                        {
                            string orderActionTemplate = @"<orderItem>
                                                    <chargeDetailId>{ChargeDetailId}</chargeDetailId>
                                                    <enteredBy>{EnteredBy}</enteredBy>
                                                    <orderStatus>{State}</orderStatus>
                                                    <orderMessage></orderMessage>
                                                </orderItem>";
                            orderActionTemplate = orderActionTemplate.Replace("{ChargeDetailId}", c.ChargeDetailId.ToString());
                            orderActionTemplate = orderActionTemplate.Replace("{EnteredBy}", c.DeletedBy);
                            docTemplate = docTemplate.Replace("{State}", Constant.AlliesRadRequestStatus.Cancel);
                            orders += orderActionTemplate;
                        }
                    }
                    if (orders != string.Empty)
                    {
                        docTemplate = docTemplate.Replace("{OrderItem}", orders);
                        var res = MakeSoapRequestBasicAuth(docTemplate, ConfigurationManager.AppSettings["OHAlliesServiceURL"], ConfigurationManager.AppSettings["OHAlliesServiceCancelURL"]);
                        if (res.Key == HttpStatusCode.OK)
                        {
                            var xmlResponse = XDocument.Parse(res.Value);
                            foreach (XElement element in xmlResponse.Descendants("orderItem"))
                            {
                                var chargeDetailId = element.Element("chargeDetailId").Value;
                                var orderStatus = element.Element("orderStatus").Value;
                                var orderMessage = element.Element("orderMessage").Value;
                                var findCharge = listChargeItem.SingleOrDefault(i => i.ChargeDetailId.ToString() == chargeDetailId);
                                if (findCharge != null)
                                {
                                    if (orderStatus == Constant.AlliesRadResponseStatus.Cancelled)
                                    {
                                        findCharge.Status = Constant.ChargeItemStatus.Cancelled;
                                        findCharge.CancelBy = username;
                                        ok++;
                                    }
                                    else
                                    {
                                        findCharge.CancelFailedReason = orderMessage;
                                        findCharge.FailedReason = findCharge.CancelFailedReason;
                                        findCharge.DeletedBy = null;
                                        failed++;
                                    }
                                    unitOfWork.ChargeItemRepository.Update(findCharge);
                                }
                            }
                            unitOfWork.Commit();
                        }
                        else
                        {
                            var reason = string.Empty;
                            try
                            {
                                var xmlResponse = XDocument.Parse(res.Value);
                                if (xmlResponse != null)
                                {
                                    XNamespace b = "http://www.w3.org/2003/05/soap-envelope";

                                    foreach (XElement element in xmlResponse.Descendants(b + "Reason"))
                                    {
                                        reason = element.Element(b + "Text").Value;
                                    }
                                    XNamespace s = "http://www.orionhealth.com/rhapsody/2009/11/faultDetails";
                                    var el = xmlResponse.Descendants(s + "Message").FirstOrDefault();
                                    if (el != null)
                                    {
                                        reason = el.Value;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                CustomLog.Error(string.Format("Parsing XML Response error: {0}", ex.Message));
                                reason = res.Value;
                            }

                            foreach (var ci in listChargeItem)
                            {
                                ci.CancelFailedReason = res.Key.ToString() + (reason != string.Empty ? ", " + reason : string.Empty);
                                ci.FailedReason = ci.CancelFailedReason;
                                unitOfWork.ChargeItemRepository.Update(ci);
                            }
                            unitOfWork.Commit();
                            failed += listChargeI.Count;
                        }
                    }
                    sum = sum - count;
                    position += count;
                } while (sum > 0);
            }
            return new OHServiceResult(ok, failed, total);
        }
        #endregion

        #region Radiology service
        public OHServiceResult PlaceRadOrder(Guid[] chargeItemId, Guid? ChargeId = null)
        {
            var ok = 0;
            var failed = 0;
            var listChargeItem = ChargeId == null ? unitOfWork.ChargeItemRepository.Find(c => chargeItemId.Contains(c.Id)).ToList() : unitOfWork.ChargeItemRepository.Find(c => c.ChargeId == ChargeId && c.ChargeItemType == Constant.ChargeItemType.Rad).ToList();
            var sum = listChargeItem.Count;
            var total = listChargeItem.Count;
            if (sum > 0)
            {
                var firstChargeItem = listChargeItem[0];
                var customer = unitOfWork.CustomerRepository.FirstOrDefault(c => c.PID == firstChargeItem.PatientId && !c.IsDeleted);
                var position = 0;
                do
                {
                    var count = sum > MaximumNumberOfItemPerRequest ? MaximumNumberOfItemPerRequest : sum;
                    var listChargeI = listChargeItem.GetRange(position, count);
                    string docTemplate = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:vm=""http://www.vinmec.com/EnterpriseOrderService/"">
                                           <soapenv:Header/>
                                           <soapenv:Body>
                                              <CreateRadiologyOrder  xmlns=""http://www.vinmec.com/EnterpriseOrderService/"">
                                                 <radiologyOrder>
                                                    <orderId>{OrderId}</orderId>
                                                    <patient>
                                                       <patientId>{PatientId}</patientId>
                                                       <name>
                                                          <!--Optional:-->
                                                          <first></first>
                                                          <!--Optional:-->
                                                          <last></last>
                                                          <!--Optional:-->
                                                          <middle></middle>
                                                       </name>
                                                       <dateOfBirth>{DateOfBirth}</dateOfBirth>
                                                       <sex>{Sex}</sex>
                                                       <!--Optional:-->
                                                       <streetAddress></streetAddress>
                                                       <!--Optional:-->
                                                       <city></city>
                                                       <!--Optional:-->
                                                       <stateOrProvince></stateOrProvince>
                                                       <!--Optional:-->
                                                       <zipCode></zipCode>
                                                       <!--Optional:-->
                                                       <country></country>
                                                       <!--Optional:-->
                                                       <phone></phone>
                                                    </patient>
                                                    <encounter>
                                                       <facility>{Facility}</facility>
                                                       <visitType>{VisitType}</visitType>
                                                       <visitCode>{VisitCode}</visitCode>
                                                       <attendingDoctor>{AttendingDoctor}</attendingDoctor>
                                                       <patientType>{PatientType}</patientType>
                                                       <!--Optional:-->
                                                       <admissionDate>{AdmissionDate}</admissionDate>
                                                       <location>{Location}</location>
                                                       <!--Optional:-->
                                                       <specialty></specialty>
                                                    </encounter>
                                                    <orderItem>
                                                       {Items}
                                                    </orderItem>
                                                    <enteredBy>{EnteredBy}</enteredBy>
                                                    <orderDate>{OrderDate}</orderDate>
                                                    <requestedDate>{RequestedDate}</requestedDate>
                                                    <!--Optional:-->
                                                    <reason>{Reason}</reason>
                                                    <initialDiagnosis>{InitialDiagnosis}</initialDiagnosis>
                                                    <state>{State}</state>
                                                 </radiologyOrder>
                                              </CreateRadiologyOrder>
                                           </soapenv:Body>
                                        </soapenv:Envelope>";
                    docTemplate = docTemplate.Replace("{OrderId}", firstChargeItem.ChargeId.ToString());
                    docTemplate = docTemplate.Replace("{PatientId}", firstChargeItem.PatientId);
                    docTemplate = docTemplate.Replace("{DateOfBirth}", customer.DateOfBirth != null ? customer.DateOfBirth.Value.ToString("s") : string.Empty);
                    docTemplate = docTemplate.Replace("{Sex}", customer.Gender == 1 ? "M" : "F");
                    docTemplate = docTemplate.Replace("{Facility}", firstChargeItem.HospitalCode);
                    docTemplate = docTemplate.Replace("{VisitType}", firstChargeItem.VisitType);
                    docTemplate = docTemplate.Replace("{VisitCode}", firstChargeItem.VisitCode);
                    docTemplate = docTemplate.Replace("{AttendingDoctor}", firstChargeItem.DoctorAD);
                    docTemplate = docTemplate.Replace("{AdmissionDate}", firstChargeItem.CreatedAt.Value.ToString("yyyy-MM-ddT00:00:00"));
                    docTemplate = docTemplate.Replace("{EnteredBy}", firstChargeItem.CreatedBy);
                    docTemplate = docTemplate.Replace("{OrderDate}", firstChargeItem.CreatedAt.Value.ToString("s"));
                    docTemplate = docTemplate.Replace("{RequestedDate}", firstChargeItem.CreatedAt.Value.ToString("s"));
                    docTemplate = docTemplate.Replace("{Reason}", EscapeXml(firstChargeItem.Reason));
                    docTemplate = docTemplate.Replace("{InitialDiagnosis}", EscapeXml(firstChargeItem.InitialDiagnosis));
                    docTemplate = docTemplate.Replace("{Location}", firstChargeItem.PatientLocationCode);
                    docTemplate = docTemplate.Replace("{PatientType}", !string.IsNullOrEmpty(firstChargeItem.VisitGroupType) ? firstChargeItem.VisitGroupType[0].ToString() : string.Empty);
                    docTemplate = docTemplate.Replace("{State}", Constant.AlliesRadRequestStatus.New);
                    string orders = string.Empty;
                    foreach (var c in listChargeI)
                    {
                        if (c.RadiologyProcedurePlan != null && customer.DateOfBirth != null)
                        {
                            string orderActionTemplate = @"<item>
                                                    <orderLineId>{OrderLineId}</orderLineId>
                                                    <itemCode>{ItemCode}</itemCode>
                                                    <itemName>{ItemName}</itemName>
                                                    <serviceCategoryCode>{ServiceCategoryCode}</serviceCategoryCode>
                                                    <serviceCategoryName>{ServiceCategoryName}</serviceCategoryName>
                                                    <dicomModality>{DicomModality}</dicomModality>
                                                    <orderStatus>{OrderStatus}</orderStatus>
                                                    <orderMessage></orderMessage>
                                                    <priority>{Priority}</priority>
                                                </item>";
                            orderActionTemplate = orderActionTemplate.Replace("{OrderLineId}", DrsPrefix + c.PlacerIdentifyNumber.ToString());
                            orderActionTemplate = orderActionTemplate.Replace("{ItemCode}", c.RadiologyProcedurePlan.ShortCode);
                            orderActionTemplate = orderActionTemplate.Replace("{ItemName}", EscapeXml(c.RadiologyProcedurePlan.RadiologyProcedureNameL));
                            orderActionTemplate = orderActionTemplate.Replace("{ServiceCategoryCode}", c.RadiologyProcedurePlan.ServiceCategoryCode);
                            orderActionTemplate = orderActionTemplate.Replace("{ServiceCategoryName}", EscapeXml(c.RadiologyProcedurePlan.ServiceCategoryNameL));
                            orderActionTemplate = orderActionTemplate.Replace("{DicomModality}", c.RadiologyProcedurePlan.DicomModality);
                            orderActionTemplate = orderActionTemplate.Replace("{OrderStatus}", Constant.AlliesRadRequestStatus.New);
                            orderActionTemplate = orderActionTemplate.Replace("{Priority}", c.Priority);
                            orders += orderActionTemplate;
                        }
                        else
                        {
                            c.Status = Constant.ChargeItemStatus.Failed;
                            c.FailedReason = c.RadiologyProcedurePlan == null ? "Missing Radiology Procedure plan" : "Missing customer's date of birth";
                            // unitOfWork.ChargeItemRepository.Update(c);
                            failed++;
                        }
                    }

                    unitOfWork.Commit();

                    if (orders != string.Empty)
                    {
                        docTemplate = docTemplate.Replace("{Items}", orders);
                        var res = MakeSoapRequestBasicAuth(docTemplate, ConfigurationManager.AppSettings["OHRadiologyServiceURL"], ConfigurationManager.AppSettings["OHRadiologyServiceAddOrderURL"]);
                        if (res.Key == HttpStatusCode.OK)
                        {
                            var xmlResponse = XDocument.Parse(res.Value);
                            XNamespace s = "http://www.vinmec.com/EnterpriseOrderService/";
                            foreach (XElement element in xmlResponse.Descendants(s + "item"))
                            {
                                var placerNumber = element.Element(s + "orderLineId").Value;
                                var orderStatus = element.Element(s + "orderStatus").Value;
                                var orderMessage = element.Element(s + "orderMessage").Value;
                                var findCharge = listChargeItem.SingleOrDefault(i => placerNumber == DrsPrefix + i.PlacerIdentifyNumber);
                                if (findCharge != null)
                                {
                                    if (orderStatus != Constant.AlliesRadResponseStatus.Placing)
                                    {
                                        findCharge.FailedReason = orderMessage;
                                        findCharge.Status = Constant.ChargeItemStatus.Failed;
                                        failed++;
                                    }
                                    else
                                    {
                                        ok++;
                                    }
                                    // unitOfWork.ChargeItemRepository.Update(findCharge);
                                }
                            }
                            unitOfWork.Commit();
                        }
                        else
                        {
                            var reason = string.Empty;
                            try
                            {
                                var xmlResponse = XDocument.Parse(res.Value);
                                if (xmlResponse != null)
                                {
                                    XNamespace b = "http://www.w3.org/2003/05/soap-envelope";
                                    foreach (XElement element in xmlResponse.Descendants(b + "Reason"))
                                    {
                                        reason = element.Element(b + "Text").Value;
                                    }
                                    if (reason == string.Empty)
                                    {
                                        foreach (XElement element in xmlResponse.Descendants("faultstring"))
                                        {
                                            reason = element.Value;
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                CustomLog.Error(string.Format("Parsing XML Response error: {0}", ex.Message));
                                reason = res.Value;
                            }

                            foreach (var ci in listChargeItem)
                            {
                                ci.Status = Constant.ChargeItemStatus.Failed;
                                ci.FailedReason = res.Key.ToString() + (reason != string.Empty ? ", " + reason : string.Empty);
                                // unitOfWork.ChargeItemRepository.Update(ci);
                            }
                            unitOfWork.Commit();
                            failed += listChargeI.Count;
                        }
                    }
                    sum = sum - count;
                    position += count;
                } while (sum > 0);
            }
            unitOfWork.Commit();
            return new OHServiceResult(ok, failed, total);
        }

        public OHServiceResult CancelRadOrder(List<ChargeItem> listChargeItem)
        {
            var ok = 0;
            var failed = 0;
            var username = getUsername();
            var sum = listChargeItem.Count;
            var total = listChargeItem.Count;
            if (sum > 0)
            {
                var firstChargeItem = listChargeItem[0];
                var customer = unitOfWork.CustomerRepository.FirstOrDefault(c => c.PID == firstChargeItem.PatientId && !c.IsDeleted);
                var position = 0;
                do
                {
                    var count = sum > MaximumNumberOfItemPerRequest ? MaximumNumberOfItemPerRequest : sum;
                    var listChargeI = listChargeItem.GetRange(position, count);
                    string docTemplate = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:vm=""http://www.vinmec.com/EnterpriseOrderService/"">
                                           <soapenv:Header/>
                                           <soapenv:Body>
                                              <CancelRadiologyOrder  xmlns=""http://www.vinmec.com/EnterpriseOrderService/"">
                                                 <radiologyOrder>
                                                    <orderId>{OrderId}</orderId>
                                                    <patient>
                                                       <patientId>{PatientId}</patientId>
                                                       <name>
                                                          <!--Optional:-->
                                                          <first></first>
                                                          <!--Optional:-->
                                                          <last></last>
                                                          <!--Optional:-->
                                                          <middle></middle>
                                                       </name>
                                                       <dateOfBirth>{DateOfBirth}</dateOfBirth>
                                                       <sex>{Sex}</sex>
                                                       <!--Optional:-->
                                                       <streetAddress></streetAddress>
                                                       <!--Optional:-->
                                                       <city></city>
                                                       <!--Optional:-->
                                                       <stateOrProvince></stateOrProvince>
                                                       <!--Optional:-->
                                                       <zipCode></zipCode>
                                                       <!--Optional:-->
                                                       <country></country>
                                                       <!--Optional:-->
                                                       <phone></phone>
                                                    </patient>
                                                    <encounter>
                                                       <facility>{Facility}</facility>
                                                       <visitType>{VisitType}</visitType>
                                                       <visitCode>{VisitCode}</visitCode>
                                                       <attendingDoctor>{AttendingDoctor}</attendingDoctor>
                                                       <patientType>{PatientType}</patientType>
                                                       <!--Optional:-->
                                                       <admissionDate>{AdmissionDate}</admissionDate>
                                                       <location>{Location}</location>
                                                       <!--Optional:-->
                                                       <specialty></specialty>
                                                    </encounter>
                                                    <orderItem>
                                                       {Items}
                                                    </orderItem>
                                                    <enteredBy>{EnteredBy}</enteredBy>
                                                    <orderDate>{OrderDate}</orderDate>
                                                    <requestedDate>{RequestedDate}</requestedDate>
                                                    <!--Optional:-->
                                                    <reason>{Reason}</reason>
                                                    <initialDiagnosis>{InitialDiagnosis}</initialDiagnosis>
                                                    <state>{State}</state>
                                                 </radiologyOrder>
                                              </CancelRadiologyOrder>
                                           </soapenv:Body>
                                        </soapenv:Envelope>";
                    docTemplate = docTemplate.Replace("{OrderId}", firstChargeItem.ChargeId.ToString());
                    docTemplate = docTemplate.Replace("{PatientId}", firstChargeItem.PatientId);
                    docTemplate = docTemplate.Replace("{DateOfBirth}", customer.DateOfBirth != null ? customer.DateOfBirth.Value.ToString("s") : string.Empty);
                    docTemplate = docTemplate.Replace("{Sex}", customer.Gender == 1 ? "M" : "F");
                    docTemplate = docTemplate.Replace("{Facility}", firstChargeItem.HospitalCode);
                    docTemplate = docTemplate.Replace("{VisitType}", firstChargeItem.VisitType);
                    docTemplate = docTemplate.Replace("{VisitCode}", firstChargeItem.VisitCode);
                    docTemplate = docTemplate.Replace("{AttendingDoctor}", firstChargeItem.DoctorAD);
                    docTemplate = docTemplate.Replace("{AdmissionDate}", firstChargeItem.CreatedAt.Value.ToString("s"));
                    docTemplate = docTemplate.Replace("{EnteredBy}", firstChargeItem.CreatedBy);
                    docTemplate = docTemplate.Replace("{OrderDate}", firstChargeItem.CreatedAt.Value.ToString("s"));
                    docTemplate = docTemplate.Replace("{RequestedDate}", firstChargeItem.CreatedAt.Value.ToString("s"));
                    docTemplate = docTemplate.Replace("{Reason}", firstChargeItem.Reason);
                    docTemplate = docTemplate.Replace("{InitialDiagnosis}", EscapeXml(firstChargeItem.Reason));
                    docTemplate = docTemplate.Replace("{Location}", firstChargeItem.PatientLocationCode);
                    docTemplate = docTemplate.Replace("{PatientType}", !string.IsNullOrEmpty(firstChargeItem.VisitGroupType) ? firstChargeItem.VisitGroupType[0].ToString() : string.Empty);
                    docTemplate = docTemplate.Replace("{State}", Constant.AlliesRadRequestStatus.Cancel);
                    string orders = string.Empty;
                    foreach (var c in listChargeI)
                    {
                        if (c.RadiologyProcedurePlan != null && customer.DateOfBirth != null)
                        {
                            string orderActionTemplate = @"<item>
                                                    <orderLineId>{OrderLineId}</orderLineId>
                                                    <itemCode>{ItemCode}</itemCode>
                                                    <itemName>{ItemName}</itemName>
                                                    <serviceCategoryCode>{ServiceCategoryCode}</serviceCategoryCode>
                                                    <serviceCategoryName>{ServiceCategoryName}</serviceCategoryName>
                                                    <dicomModality>{DicomModality}</dicomModality>
                                                    <orderStatus>{OrderStatus}</orderStatus>
                                                    <orderMessage></orderMessage>
                                                    <priority>{Priority}</priority>
                                                </item>";
                            orderActionTemplate = orderActionTemplate.Replace("{OrderLineId}", DrsPrefix + c.PlacerIdentifyNumber.ToString());
                            orderActionTemplate = orderActionTemplate.Replace("{ItemCode}", c.RadiologyProcedurePlan.ShortCode);
                            orderActionTemplate = orderActionTemplate.Replace("{ItemName}", EscapeXml(c.RadiologyProcedurePlan.RadiologyProcedureNameL));
                            orderActionTemplate = orderActionTemplate.Replace("{ServiceCategoryCode}", c.RadiologyProcedurePlan.ServiceCategoryCode);
                            orderActionTemplate = orderActionTemplate.Replace("{ServiceCategoryName}", EscapeXml(c.RadiologyProcedurePlan.ServiceCategoryNameL));
                            orderActionTemplate = orderActionTemplate.Replace("{DicomModality}", c.RadiologyProcedurePlan.DicomModality);
                            orderActionTemplate = orderActionTemplate.Replace("{OrderStatus}", Constant.AlliesRadRequestStatus.Cancel);
                            orderActionTemplate = orderActionTemplate.Replace("{Priority}", c.Priority);
                            orders += orderActionTemplate;
                        }
                    }
                    if (orders != string.Empty)
                    {
                        docTemplate = docTemplate.Replace("{Items}", orders);
                        var res = MakeSoapRequestBasicAuth(docTemplate, ConfigurationManager.AppSettings["OHRadiologyServiceURL"], ConfigurationManager.AppSettings["OHRadiologyServiceAddOrderURL"]);
                        if (res.Key == HttpStatusCode.OK)
                        {
                            var xmlResponse = XDocument.Parse(res.Value);
                            XNamespace s = "http://www.vinmec.com/EnterpriseOrderService/";
                            foreach (XElement element in xmlResponse.Descendants(s + "item"))
                            {
                                var placerNumber = element.Element(s + "orderLineId").Value;
                                var orderStatus = element.Element(s + "orderStatus").Value;
                                var orderMessage = element.Element(s + "orderMessage").Value;
                                var findCharge = listChargeItem.SingleOrDefault(i => placerNumber == DrsPrefix + i.PlacerIdentifyNumber);
                                if (findCharge != null)
                                {
                                    if (orderStatus == Constant.AlliesRadResponseStatus.Cancelling)
                                    {
                                        findCharge.Status = Constant.ChargeItemStatus.Cancelling;
                                        findCharge.CancelBy = username;
                                        unitOfWork.ChargeItemRepository.Update(findCharge);
                                    }
                                }
                            }
                            unitOfWork.Commit();
                            ok += listChargeI.Count;
                        }
                        else
                        {
                            var reason = string.Empty;
                            try
                            {
                                var xmlResponse = XDocument.Parse(res.Value);
                                if (xmlResponse != null)
                                {
                                    XNamespace b = "http://www.w3.org/2003/05/soap-envelope";
                                    foreach (XElement element in xmlResponse.Descendants(b + "Reason"))
                                    {
                                        reason = element.Element(b + "Text").Value;
                                    }
                                    if (reason == string.Empty)
                                    {
                                        foreach (XElement element in xmlResponse.Descendants("faultstring"))
                                        {
                                            reason = element.Value;
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                CustomLog.Error(string.Format("Parsing XML Response error: {0}", ex.Message));
                                reason = res.Value;
                            }

                            foreach (var ci in listChargeItem)
                            {
                                ci.Status = Constant.ChargeItemStatus.Failed;
                                ci.FailedReason = res.Key.ToString() + (reason != string.Empty ? ", " + reason : string.Empty);
                                unitOfWork.ChargeItemRepository.Update(ci);
                            }
                            unitOfWork.Commit();
                            failed += listChargeI.Count;
                        }
                    }
                    else
                    {
                        unitOfWork.Commit();
                    }
                    sum = sum - count;
                    position += count;
                } while (sum > 0);
            }
            return new OHServiceResult(ok, failed, total);
        }
        #endregion
        private KeyValuePair<HttpStatusCode, string> MakeSoapRequest(string strXml)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            var certificateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationManager.AppSettings["OHCertificateFilePath"]);
            var certificateFilePassword = ConfigurationManager.AppSettings["OHCertificatePassword"];
            X509Certificate2 certificate = new X509Certificate2(certificateFilePath, certificateFilePassword);
            WebRequestHandler handler = new WebRequestHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateValidationCallback = (a, b, c, d) => { return true; };
            handler.ClientCertificates.Add(certificate);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(strXml);

            LogTmp log = new LogTmp
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                Ip = ":1",
                URI = ConfigurationManager.AppSettings["OHLaboratoryServiceURL"],
                Action = "INFO",
                Request = strXml,
                Reason = "DONE"
            };

            using (HttpClient client = new HttpClient(handler))
            {
                // CustomLog.Info(string.Format("<Making SOAP request with data: {0}", strXml));
                var content = new StringContent(doc.InnerXml, Encoding.UTF8, "application/soap+xml");
                try
                {
                    client.Timeout = TimeSpan.FromSeconds(int.Parse(ConfigurationManager.AppSettings["HIS_API_TIMEOUT"].ToString()));

                    HttpResponseMessage response = AsyncHelper.RunSync(() => client.PostAsync(ConfigurationManager.AppSettings["OHLaboratoryServiceURL"], content));

                    var receiveStream = response.Content.ReadAsStringAsync().Result;

                    log.Response = string.Format("{0}-{1}", response.StatusCode, receiveStream);
                    log.UpdatedAt = DateTime.Now;
                    CustomLog.Info(log);

                    return new KeyValuePair<HttpStatusCode, string>(response.StatusCode, receiveStream);
                }
                catch (Exception ex)
                {
                    while (ex.InnerException != null) ex = ex.InnerException;
                    // CustomLog.Info(string.Format("<Making SOAP error: {0}", ex.GetBaseException().Message));

                    log.Response = string.Format("{0}-{1}", "500", ex.GetBaseException().Message);
                    log.UpdatedAt = DateTime.Now;
                    log.Reason = "ERROR";
                    CustomLog.Info(log);
                    return new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.InternalServerError, ex.GetBaseException().Message);
                }
            }
        }

        private KeyValuePair<HttpStatusCode, string> MakeSoapRequestBasicAuth(string strXml, string serviceURL, string action)
        {
            var authenticationString = $"{ConfigurationManager.AppSettings["OHServiceUsername"]}:{ConfigurationManager.AppSettings["OHServicePassword"]}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(strXml);

            LogTmp log = new LogTmp
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                Ip = ":1",
                URI = serviceURL,
                Action = "INFO",
                Request = strXml,
                Reason = "DONE"
            };

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
                client.DefaultRequestHeaders.Add("SOAPAction", action);
                client.Timeout = TimeSpan.FromSeconds(int.Parse(ConfigurationManager.AppSettings["HIS_API_TIMEOUT"].ToString()));
                // CustomLog.Info(string.Format("<Making SOAP request with data: {0}", strXml));
                var content = new StringContent(doc.InnerXml, Encoding.UTF8, "text/xml");
                try
                {
                    HttpResponseMessage response = AsyncHelper.RunSync(() => client.PostAsync(serviceURL, content));
                    // System.Threading.Tasks.Task<HttpResponseMessage> response = System.Threading.Tasks.Task.Run(() => client.PostAsync(serviceURL, content));
                    var receiveStream = response.Content.ReadAsStringAsync().Result;

                    log.Response = string.Format("{0}-{1}", response.StatusCode, receiveStream);
                    log.UpdatedAt = DateTime.Now;
                    CustomLog.Info(log);

                    return new KeyValuePair<HttpStatusCode, string>(response.StatusCode, receiveStream);
                }
                catch (Exception ex)
                {
                    while (ex.InnerException != null) ex = ex.InnerException;

                    log.Response = string.Format("{0}-{1}", "500", ex.GetBaseException().Message);
                    log.UpdatedAt = DateTime.Now;
                    log.Reason = "ERROR";
                    CustomLog.Info(log);

                    return new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.InternalServerError, ex.GetBaseException().Message);
                }
            }
        }
        public static string EscapeXml(string s)
        {
            string toxml = s;
            if (!string.IsNullOrEmpty(toxml))
            {
                // replace literal values with entities
                toxml = toxml.Replace("&", "&amp;");
                toxml = toxml.Replace("'", "&apos;");
                toxml = toxml.Replace("\"", "&quot;");
                toxml = toxml.Replace(">", "&gt;");
                toxml = toxml.Replace("<", "&lt;");
                toxml = toxml.Replace(System.Environment.NewLine, " ");
                toxml = toxml.Replace("\n", " ");
                toxml = toxml.Replace("\r", " ");
            }
            return toxml;
        }
    }

    public class OHServiceResult
    {
        public int OK { get; set; }
        public int Failed { get; set; }
        public int Total { get; set; }
        public OHServiceResult()
        {

        }
        public OHServiceResult(int oK, int failed, int total)
        {
            OK = oK;
            Failed = failed;
            Total = total;
        }
    }
}
