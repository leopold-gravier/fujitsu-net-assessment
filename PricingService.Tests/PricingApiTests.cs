using PricingService.Controllers;
using Microsoft.AspNetCore.Mvc;
using Weather;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace PricingService.Tests
{
    [TestClass]
    public sealed class PricingApiTests
    {
        private static WeatherDbContext BuildMockDb()
        {
            DbContextOptionsBuilder<WeatherDbContext> optionsBuilder = new();
            optionsBuilder.UseInMemoryDatabase($"weather");
            
            WeatherDbContext dbContext = new(optionsBuilder.Options);
            dbContext.PopulatePhenomena();
            dbContext.Records.RemoveRange(dbContext.Records);
            return dbContext;
        }


        [TestMethod]
        public void TestUnknownCity()
        {
            WeatherDbContext db = BuildMockDb();
            PricingServiceController service = new PricingServiceController(db);

            ActionResult<float> result = service.Get("Paris", VehicleType.Scooter);

            Assert.IsNotNull(result.Result);
            Assert.IsTrue(result.Result is BadRequestObjectResult);
        }


        [TestMethod]
        public void TestTallinnNoCrash()
        {
            WeatherDbContext db = BuildMockDb();

            Station station = new Station { Name = "Tallinn-Harku" };
            Phenomenon phenomenon = new Phenomenon { Name = "Clear" };
            db.ImportRecord(new Record { Station = station, Phenomenon = phenomenon, AirTemp = 5, WindSpeed = 10 });
            db.SaveChanges();

            PricingServiceController service = new PricingServiceController(db);

            ActionResult<float> result = service.Get("Tallinn", VehicleType.Car);

            Assert.IsNull(result.Result);
        }


        [TestMethod]
        public void TestTartuNoCrash()
        {
            WeatherDbContext db = BuildMockDb();

            Station station = new Station { Name = "Tartu-Tõravere" };
            Phenomenon phenomenon = new Phenomenon { Name = "Clear" };
            db.ImportRecord(new Record { Station = station, Phenomenon = phenomenon, AirTemp = 5, WindSpeed = 10 });
            db.SaveChanges();

            PricingServiceController service = new PricingServiceController(db);

            ActionResult<float> result = service.Get("Tartu", VehicleType.Scooter);

            Assert.IsNull(result.Result);
        }


        [TestMethod]
        public void TestParnuNoCrash()
        {
            WeatherDbContext db = BuildMockDb();

            Station station = new Station { Name = "Pärnu" };
            Phenomenon phenomenon = new Phenomenon { Name = "Clear" };
            db.ImportRecord(new Record { Station = station, Phenomenon = phenomenon, AirTemp = 5, WindSpeed = 10 });
            db.SaveChanges();

            PricingServiceController service = new PricingServiceController(db);

            ActionResult<float> result = service.Get("Parnu", VehicleType.Bike);

            Assert.IsNull(result.Result);
        }


        [TestMethod]
        public void TestExampleCalculation()
        {
            WeatherDbContext db = BuildMockDb();

            Station station = new Station { Name = "Tartu-Tõravere" };
            Phenomenon phenomenon = new Phenomenon { Name = "Light snow shower" };
            db.ImportRecord(new Record { Station = station, Phenomenon = phenomenon, AirTemp = -2.1f, WindSpeed = 4.7f });
            db.SaveChanges();

            PricingServiceController service = new PricingServiceController(db);

            ActionResult<float> result = service.Get("Tartu", VehicleType.Bike);

            Assert.IsNull(result.Result);
            Assert.AreEqual(4.0f, result.Value);
        }


        [TestMethod]
        public void TestExcessWindForbidsBike()
        {
            WeatherDbContext db = BuildMockDb();

            Station station = new Station { Name = "Tartu-Tõravere" };
            Phenomenon phenomenon = new Phenomenon { Name = "Clear" };
            db.ImportRecord(new Record { Station = station, Phenomenon = phenomenon, AirTemp = 5, WindSpeed = 25 });
            db.SaveChanges();

            PricingServiceController service = new PricingServiceController(db);

            ActionResult<float> result = service.Get("Tartu", VehicleType.Bike);

            Assert.IsNotNull(result.Result);
            BadRequestObjectResult? error400 = result.Result as BadRequestObjectResult;
            Assert.IsNotNull(error400);
            Assert.AreEqual("Usage of selected vehicle type is forbidden", error400.Value);
        }
    }
}
