using Microsoft.AspNetCore.Mvc;
using RestApiTask.Models;
using RestApiTask.Services;

namespace RestApiTask.Controllers;

[ApiController]
[Route("api/submitapirequest")]
public class RestApiController : ControllerBase
{
    private readonly RestApiService _restApiService;

    public RestApiController(RestApiService restApiService)
    {
        _restApiService = restApiService;
    }

    [HttpPost]
    public IActionResult SubmitRestApi([FromBody] RestApiRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(new RestApiResponse
            {
                Result = 0,
                ResultMessage = string.Join("; ", errors)
            });
        }

        var response = _restApiService.ProcessRestApi(request);
        return Ok(response);
    }
}
