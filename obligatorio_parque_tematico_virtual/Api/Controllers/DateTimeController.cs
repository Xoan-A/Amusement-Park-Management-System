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
        public async Task<IActionResult> GetDateTime()
        {
            var currentDateTime = await _dateTimeLogic.GetCurrentDateTime();

            var response = new DateTimeResponse
            {
                CurrentDateTime = currentDateTime
            };

            return Ok(response);
        }

        [HttpPut]
        public async Task<IActionResult> SetDateTime([FromBody] SetDateTimeRequest request)
        {
            var dateTime = DateTime.Parse(request.DateTime);
            await _dateTimeLogic.SetDateTime(dateTime);

            return Ok();
        }
    }
}