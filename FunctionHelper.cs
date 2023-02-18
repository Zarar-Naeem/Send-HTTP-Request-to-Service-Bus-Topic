using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RCK.CloudPlatform.Model.SalesOrder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using VSI.CloudPlatform.Common;
using VSI.CloudPlatform.Core.Db;
using VSI.CloudPlatform.Db;
using VSI.CloudPlatform.Model;
using VSI.CloudPlatform.Model.Jobs;
using VSI.Common;
using VSI.Model;
using static VSI.Contants.Global;

namespace FunctionApp.RCK.ECOM.OrderReceive
{
    public class FunctionHelper
    {
        internal static EDI_STAGING EdiStageObject(ApiParams apiParams)
        {
            var stageData = new EDI_STAGING
            {
                Transaction_Id = Utilities.GetTransactionId(),
                Transaction_Set_Id = apiParams.TransactionSetId,
                Transaction_Set_Code = apiParams.TransactionSetCode,
                Customer_Id = apiParams.PartnershipCustomerId,
                Status = Status.Active,
                Created_Date = DateTime.Now,
                Direction = TransactionDirection.Inbound,
                PartnerShip_Code = apiParams.PartnershipCode,
                Customer_Code = apiParams.PartnerShipCustomerCode,
                Company_Code = apiParams.PartnerShipCompanyCode,
                Partnership_Id = apiParams.PartnershipId,
                Company_Id = apiParams.PartnerShipCompanyId,
                State = TransactionState.StagingStates.Candidate,
                ISAControlNum = Utilities.GetISAControlNumber().ToString(),
                EPOCCreatedDateTime = Utilities.ConvertToUnixTimestamp(DateTime.Now),
                HasBlobPaths = true,
                NoOfSteps = apiParams.TotalSteps,
                BatchId = Utilities.GetTransactionId(),
                TransactionName = apiParams.TransactionName,
                TransactionType = apiParams.TransactionType,
                StepsDetails = new List<Steps>()
                {
                    new Steps
                    {
                        StepName = apiParams.TransactionStep,
                        StartDate = DateTime.Now.ToString(),
                        StepOrder = 1,
                        LastStep = false
                    }
                }
            };
            stageData.GS06 = stageData.ISAControlNum;

            return stageData;
        }

        internal static ApiParams GetApiParams(Function stepParams, PartnerShip partnership)
        {
            var functionSettings = stepParams.Settings;

            return new ApiParams
            {
                TransactionStep = stepParams.TransactionStep,
                TransactionName = stepParams.TransactionName,
                PartnershipCode = partnership.Partnership_Code,
                PartnerShipCompanyCode = partnership.Company_Code,
                PartnerShipCompanyId = partnership.Company_Id,
                PartnerShipCustomerCode = partnership.Customer_Code,
                PartnershipCustomerId = partnership.Customer_Id,
                PartnershipId = partnership.Partnership_Id,
                TotalSteps = stepParams.TotalSteps,
                TransactionSetCode = partnership.Transaction_Set_Code,
                TransactionSetId = partnership.Transaction_Set_Id,
                TransactionType = stepParams.TransactionType,

                KeyIdentifier = functionSettings.GetValue("KeyIdentifier", ""),
                TargetTopic = functionSettings.GetValue("Service Bus Target Topic", "")
            };
        }

        public static ProcessFlow GetProcessFlowByName(string processName, ICloudDb cloudDb)
        {
            return cloudDb.ExecuteQuery("process_flow", "SELECT * FROM c where c.entity_name = 'flow' and c.Name = '" + processName + "'").ToObject<List<ProcessFlow>>().FirstOrDefault();
        }

        //public static KorberConfigurations GetProcessConfiguration(ICloudDb cloudDb)
        //{
        //     return cloudDb.ExecuteQuery("configuration_data", "SELECT * FROM c where c.entity_name = 'KorberRequestConfigurations'").ToObject<List<KorberConfigurations>>().FirstOrDefault();
        //}

        internal static string LogSuccessStageMessage(ICloudDb cloudDb, EDI_STAGING stage, ApiParams apiParams, string blobPath)
        {
            stage.State = TransactionState.StagingStates.Pass;
            stage.OverallStatus = OverallStatus.IN_PROGRESS;
            stage.InboundData = blobPath;
            stage.EDI_STD_Document_ORIGINAL = blobPath;
            stage.Data = blobPath;

            var step = stage.StepsDetails.FirstOrDefault(s => s.StepName == apiParams.TransactionStep);

            step.EndDate = DateTime.Now.ToString();
            step.Status = MessageStatus.COMPLETED;
            step.FileUrl = blobPath;

            stage.EDI_STANDARD_DOCUMENT = blobPath;

            return StageDataDbHelper.AddStageData(stage, cloudDb);
        }

        internal static string GetKeyIdentifierFromXML(string xmlContent, string keyIdentifier)
        {
            var document = new XmlDocument();
            document.LoadXml(xmlContent);

            var dataKey = document?.SelectSingleNode(keyIdentifier)?.InnerText;

            return string.IsNullOrWhiteSpace(dataKey) ? string.Empty : dataKey;
        }

        internal static string GetKeyIdentifierFromJson(string jsonContent, string keyIdentifier)
        {
            var data = (JObject)JsonConvert.DeserializeObject(jsonContent);
            string dataKey = data[keyIdentifier].Value<string>();

            return string.IsNullOrWhiteSpace(dataKey) ? string.Empty : dataKey;
        }
    }

    //public class ApiParams
    //{
    //    public string TargetTopic { get; set; }
    //    public string TransactionStep { get; set; }
    //    public string TransactionName { get; set; }
    //    public string PartnershipCode { get; set; }
    //    public string KeyIdentifier { get; set; }
    //    public string TransactionType { get; set; }
    //    public int TotalSteps { get; set; }
    //    public int PartnerShipCompanyId { get; set; }
    //    public string PartnershipId { get; set; }
    //    public string PartnerShipCompanyCode { get; set; }
    //    public string PartnerShipCustomerCode { get; set; }
    //    public int PartnershipCustomerId { get; set; }
    //    public string TransactionSetCode { get; set; }
    //    public int TransactionSetId { get; set; }
    //}

}
