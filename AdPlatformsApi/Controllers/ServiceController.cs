using AdPlatforms.Classes;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AdPlatforms.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceController : Controller
    {
        private const int maxBodySize = 10 * 1024 * 1024; //Максимальный размер для тела запроса и файла - 10 Мб

        private readonly DataHandler dataHandler;

        public ServiceController(DataHandler dataHandler)
        {
            this.dataHandler = dataHandler;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [RequestFormLimits(MultipartBodyLengthLimit = maxBodySize, ValueLengthLimit = maxBodySize)]
        public IActionResult UploadFile([Required] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл не загружен");
            else if (Path.GetExtension(file.FileName)?.ToLower() != ".txt")
                return BadRequest("Файл должен быть в формате .txt");

            try
            {
                using (var stream = new StreamReader(file.OpenReadStream()))
                {
                    var data = stream.ReadToEnd();
                    dataHandler.ParseFile(data);
                }
                return Ok();
            }
            catch (FormatException fEx)
            {
                return BadRequest(fEx.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка при загрузке рекламных площадок из файла: " + ex.Message);
            }
        }

        [HttpGet("getplatforms")]
        public IActionResult GetPlatforms([Required][FromQuery] string location)
        {
            if (string.IsNullOrWhiteSpace(location))
                return BadRequest("Не указана локация");

            var platforms = dataHandler.GetPlatforms(location);
            if (platforms.Count() > 0)
                return Json(platforms);
            else
                return NotFound($"Не найдено платформ для локации {location}");

        }
    }
}
