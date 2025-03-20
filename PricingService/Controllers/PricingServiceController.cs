using Microsoft.AspNetCore.Mvc;

namespace PricingService.Controllers;

[ApiController]
[Route("/api/delivery/pricing")]
public class PricingServiceController : ControllerBase
{
    private readonly ILogger<PricingServiceController> _logger;

    public PricingServiceController(ILogger<PricingServiceController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<float> Get(string city, VehicleType vehicle)
    {
        try
        {
            float price = PricingCalculator.GetDeliveryPrice(city, vehicle);
            return price;
        } catch (InvalidDeliveryException e)
        {
            return BadRequest(e.Message);
        }
    }
}
