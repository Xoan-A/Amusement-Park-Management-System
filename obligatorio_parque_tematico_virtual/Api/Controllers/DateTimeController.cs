using Microsoft.AspNetCore.Mvc;
using IBusinessLogic;
using Models.In;
using Models.Out;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/datetime")]
    public class DateTimeController : ControllerBase
    {
        private readonly IDateTimeLogic _dateTimeLogic;

        public DateTimeController(IDateTimeLogic dateTimeLogic)
        {
            _dateTimeLogic = dateTimeLogic;
        }

        [HttpGet]
        public IActionResult GetDateTime()
        {
            DateTime currentDateTime = _dateTimeLogic.GetCurrentDateTime();

            DateTimeResponse response = new DateTimeResponse
            {
                CurrentDateTime = currentDateTime
            };

            return Ok(response);
        }

        [HttpPut]
        public IActionResult SetDateTime([FromBody] SetDateTimeRequest request)
        {
            DateTime dateTime = DateTime.Parse(request.DateTime);
            _dateTimeLogic.SetDateTime(dateTime);

            return Ok();
        }
    }
}