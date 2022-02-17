using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Communication.Email.SharedClients.Models;
using ACS_SendEmail.Models;
using Microsoft.AspNetCore.Http.Extensions;

namespace ACS_SendEmail
{
    public static class sendACSEmail
    {
        [FunctionName("sendACSEmail")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string responseMessage = "";
            try
            {

                string connectionString = req.Headers["ACSConnectionString"];
                /*
                //For testing only. If you set a function key called ACSConnectionString, you can get the value from the Environment
                if (string.IsNullOrEmpty(connectionString))
                {
                   connectionString = Environment.GetEnvironmentVariable("CUSTOMCONNSTR_ACSConnectionString");
                }
                */
           
                if (string.IsNullOrEmpty(connectionString))
                {
                    responseMessage = "ACSConnectionString must be specified in the header\n";
                    return new BadRequestObjectResult(responseMessage);   

                }

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);

                string toRecipients = data?.toRecipients;
                string emailSubject = data?.emailSubject;
                string emailBody = data?.emailBody;
                string fromSender = data?.senderEmail;
  
                EmailFunction emailToSend = new EmailFunction(connectionString)
                {
                    toRecipients = toRecipients,
                    subject = emailSubject,
                    emailMsg = emailBody,
                    fromSender = fromSender,
                };

                

                if (string.IsNullOrEmpty(toRecipients) || string.IsNullOrEmpty(emailSubject) || string.IsNullOrEmpty(emailBody) || string.IsNullOrEmpty(fromSender))
                {
                    responseMessage = "toRecipients, emailSubject, and emailBody are all required in the request body.";
                    return new BadRequestObjectResult(responseMessage);
                }
                else
                {
                    emailToSend.sendEmailMsg();
                    responseMessage = $"To: {toRecipients}\nSubject: {emailSubject} \nMessage: {emailBody}\n";
                    responseMessage += emailToSend.status;
                    
                }
  
            }
            catch (Exception ex)
            {
                log.LogInformation(ex.Message);
            }
            
            return new OkObjectResult(responseMessage);
        }
    }
}
