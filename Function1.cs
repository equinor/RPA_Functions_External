using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;


namespace RPA_Azure_Func_External
{
    public class PC243_Webservice
    {
        private MaterialDeliveryTableOperations mdTableOps = new MaterialDeliveryTableOperations();

        // Will be called by the Customer WWW Interface (EXPOSED TO INTERNET)
        [FunctionName("PC243_GetMaterialDelivery")]
        public HttpResponseMessage GetMaterialDelivery(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "PC243_MaterialDelivery/{webid}")] HttpRequest req,
           ILogger log, string webid)
        {
            log.LogInformation("PC243 Get task trigged");

            string materialDeliveryHTML = HtmlTemplate.GetPage(mdTableOps.QueryMaterialDeliveryOnWebid(webid, false).Result);

            Console.Write(materialDeliveryHTML);
            if (materialDeliveryHTML != null && materialDeliveryHTML.Length > 0)
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(materialDeliveryHTML);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
                return response;
            }
            else
            {
                var response = new HttpResponseMessage(HttpStatusCode.NotFound);
                return null;

            }
        }

        // Will be called by the Customer WWW Interface (EXPOSED TO INTERNET)
        [FunctionName("PC243_PostMaterialDeliveryUpdate")]
        public async Task<IActionResult> PostMaterialDeliveryUpdate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "PC243_PostMaterialDeliveryUpdate/{webid}/{guid}")] HttpRequest req,
            string webid,
            string guid,
            ILogger log)
        {
            log.LogInformation("PC243 post material delivery  request received");

            dynamic bodyData = JsonConvert.DeserializeObject(await new StreamReader(req.Body).ReadToEndAsync());

            Object retVal = await mdTableOps.UpdateMaterialDelivery(guid, bodyData);

            return (ActionResult)new OkObjectResult(retVal);
        }
    }
}
