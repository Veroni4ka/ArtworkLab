using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace ArtworkLab.Controllers
{
    public class HomeController : Controller
    {
        private static string url = "https://eastus2.api.cognitive.microsoft.com/customvision/v3.0/Prediction/e040674c-b4ba-452b-832d-795e1e03a1a7/classify/iterations/Iteration2/image";
        private static string predictionKey = "8197d29258b34dadb0e5557036ff96fb";

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(HttpPostedFileBase fileInput)
        {
            Session["img"] = fileInput.FileName;
            HttpResponseMessage message = new HttpResponseMessage();
            string result = string.Empty;
            if (fileInput != null && fileInput.ContentLength > 0)
            { 
                try
                {
                    byte[] image = new byte[fileInput.ContentLength];
                    fileInput.InputStream.Read(image, 0, image.Length);

                    message = await MakePredictionRequest(image);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("File wasn't uploaded");
                }
            }

            if (message.IsSuccessStatusCode)
            {
                var resultStr = await message.Content.ReadAsStringAsync();
                JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                dynamic dobj = jsonSerializer.Deserialize<dynamic>(resultStr);
                result = dobj["predictions"][0]["tagName"] + " " + dobj["predictions"][0]["probability"].ToString("P1");
            }
            return View(model: result);
        }

        public static async Task<HttpResponseMessage> MakePredictionRequest(byte[] imageFile)
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Prediction-Key", predictionKey);

            HttpResponseMessage response;

            using (var content = new ByteArrayContent(imageFile))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(url, content);
            }

            return response;
        }

        private static byte[] GetImageAsByteArray(string imageFilePath)
        {
            FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }
    }
}