using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace RequestValidator.Controllers
{
    [Route("api/[controller]")]
    public class MessageController : Controller
    {
        
        private HMACSHA1 _hmac;
        private readonly string _authtoken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");

        [HttpPost]
        public IActionResult SendMessage(IFormCollection fc)
        {
            // The Twilio request URL
            const string url = "https://31123028.ngrok.io/api/Message";
            
            // Initialize the validator
            var validator = new Twilio.Security.RequestValidator(_authtoken);
            
            // The X-Twilio-Signature header attached to the request
            var twilioSignature = "empty";
            StringValues headerValues;
            if (Request.Headers.TryGetValue("X-Twilio-Signature", out headerValues))
            {
                twilioSignature = headerValues.FirstOrDefault();
            }
            
            Console.WriteLine(twilioSignature);
        
            var parameters = fc.Keys.ToDictionary<string, string, string>(key => key, key => fc[key]);
        
            var hashedParameters = GetHashedParameters(url, parameters);
            Console.WriteLine(hashedParameters);
            Console.WriteLine(validator.Validate(url, parameters, twilioSignature));
        
            return Content("blah");
        }

        // POST api/Message/SendMessage

        private string GetHashedParameters(string url, IDictionary<string, string> parameters)
        {
            var stringBuilder = new StringBuilder(url);
            var stringList = new List<string>(parameters.Keys);
            var ordinal = StringComparer.Ordinal;
            stringList.Sort(ordinal);
            using (var enumerator = stringList.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    stringBuilder.Append(current).Append(parameters[current] ?? "");
                }
            }
            
            _hmac = new HMACSHA1(Encoding.UTF8.GetBytes(_authtoken));
            return Convert.ToBase64String(_hmac.ComputeHash(Encoding.UTF8.GetBytes(stringBuilder.ToString())));
        }
    }
}