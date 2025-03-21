using Microsoft.AspNetCore.Mvc;
using Weather;

namespace PricingService.Controllers;

[ApiController]
[Route("/api/delivery/pricing")]
public class PricingServiceController : ControllerBase
{
    protected WeatherDbContext _dbContext;

    public PricingServiceController(WeatherDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Returns the price in euro of a delivery.
    /// </summary>
    /// <param name="city">The city to deliver to.</param>
    /// <param name="vehicle">The vehicle to use for the delivery. </param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<float> Get(string city, VehicleType vehicle)
    {
        try
        {
            float price = PricingCalculator.GetDeliveryPrice(city, vehicle, _dbContext);
            return price;
        } catch (InvalidDeliveryException e)
        {
            return BadRequest(e.Message);
        }
    }
}
