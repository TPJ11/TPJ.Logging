using Microsoft.AspNetCore.Mvc;

namespace TPJ.LoggingTestAPI.Controllers
{
    public class Divide
    {
        public int ValueOne { get; set; }
        public int ValueTwo { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly TPJ.Logging.ILogger _logger;

        public TestController(TPJ.Logging.ILogger logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Post(Divide divide)
        {
            try
            {
                return Ok(divide.ValueOne / divide.ValueTwo);
            }
            catch (Exception e)
            {
                _logger.Log(System.Reflection.MethodBase.GetCurrentMethod(), e, divide);
                return BadRequest(e);
            }
        }
    }
}