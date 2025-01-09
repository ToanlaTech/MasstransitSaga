using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MasstransitSaga.Core.Context;

namespace MasstransitReactApp.Server.Controllers
{
    [Route("api/worlddata")]
    [ApiController]
    public class WorldDataController : ControllerBase
    {
        private readonly WorldDbContext _context;

        public WorldDataController(WorldDbContext context)
        {
            _context = context;
        }

        // 1. Join Cities with Countries
        /// <summary>
        /// Kết hợp bảng Cities và Countries để lấy thông tin về thành phố, quốc gia, dân số, và vùng.
        /// </summary>
        /// <returns></returns>
        [HttpGet("cities-countries")]
        public async Task<IActionResult> GetCitiesWithCountries()
        {
            var result = await _context.Cities
                .Join(_context.Countries,
                      city => city.CountryCode,
                      country => country.Code,
                      (city, country) => new
                      {
                          CityName = city.Name,
                          CountryName = country.Name,
                          Population = city.Population,
                          Region = country.Region
                      })
                      .Take(100)
                .ToListAsync();
            return Ok(result);
        }

        // 2. Join CountryLanguage with Country
        /// <summary>
        /// Kết hợp bảng CountryLanguages và Countries để lấy thông tin ngôn ngữ, quốc gia, tỷ lệ sử dụng, và trạng thái chính thức.
        /// </summary>
        /// <returns></returns>
        [HttpGet("languages-countries")]
        public async Task<IActionResult> GetLanguagesWithCountries()
        {
            var result = await _context.CountryLanguages
                .Join(_context.Countries,
                      lang => lang.CountryCode,
                      country => country.Code,
                      (lang, country) => new
                      {
                          Language = lang.Language,
                          CountryName = country.Name,
                          IsOfficial = lang.IsOfficial,
                          Percentage = lang.Percentage
                      })
                      .Take(100)
                .ToListAsync();
            return Ok(result);
        }

        // 3. Multi-table Join: Cities, Countries, and CountryLanguages
        /// <summary>
        /// Kết hợp 3 bảng Cities, Countries, và CountryLanguages để lấy thông tin chi tiết nhất về thành phố, quốc gia, và ngôn ngữ.
        /// </summary>
        /// <returns></returns>
        [HttpGet("detailed-cities")]
        public async Task<IActionResult> GetDetailedCities()
        {
            var result = await _context.Cities
                .Join(_context.Countries,
                      city => city.CountryCode,
                      country => country.Code,
                      (city, country) => new { city, country })
                .Join(_context.CountryLanguages,
                      combined => combined.country.Code,
                      lang => lang.CountryCode,
                      (combined, lang) => new
                      {
                          CityName = combined.city.Name,
                          CountryName = combined.country.Name,
                          Language = lang.Language,
                          Population = combined.city.Population,
                          IsOfficial = lang.IsOfficial
                      })
                .Take(100)
                .ToListAsync();
            return Ok(result);
        }

        // 4. Aggregate Query: Total Population by Region
        /// <summary>
        /// Thực hiện nhóm dữ liệu (Group By) để tính tổng dân số theo vùng.
        /// </summary>
        /// <returns></returns>
        [HttpGet("population-by-region")]
        public async Task<IActionResult> GetPopulationByRegion()
        {
            var result = await _context.Countries
                .GroupBy(c => c.Region)
                .Select(group => new
                {
                    Region = group.Key,
                    TotalPopulation = group.Sum(c => c.Population)
                })
                .OrderByDescending(r => r.TotalPopulation)
                .Take(100)
                .ToListAsync();
            return Ok(result);
        }

        // 5. Filtered Join: Official Languages in Highly Populated Countries
        /// <summary>
        /// Lọc các quốc gia có dân số lớn hơn 50 triệu và chỉ chọn các ngôn ngữ chính thức.
        /// </summary>
        /// <returns></returns>
        [HttpGet("official-languages-large-countries")]
        public async Task<IActionResult> GetOfficialLanguagesInLargeCountries()
        {
            var result = await _context.CountryLanguages
                .Join(_context.Countries,
                      lang => lang.CountryCode,
                      country => country.Code,
                      (lang, country) => new { lang, country })
                .Where(combined => combined.country.Population > 50000000 && combined.lang.IsOfficial == "T")
                .Select(combined => new
                {
                    CountryName = combined.country.Name,
                    Language = combined.lang.Language,
                    Population = combined.country.Population
                })
                .Take(100)
                .ToListAsync();
            return Ok(result);
        }

        [HttpGet("complex-query")]
        public async Task<IActionResult> GetComplexQuery()
        {
            var result = await _context.Cities
                .Join(_context.Countries,
                      city => city.CountryCode,
                      country => country.Code,
                      (city, country) => new { city, country })
                .Join(_context.CountryLanguages,
                      combined => combined.country.Code,
                      lang => lang.CountryCode,
                      (combined, lang) => new
                      {
                          CityName = combined.city.Name,
                          CountryName = combined.country.Name,
                          Language = lang.Language,
                          Population = combined.city.Population,
                          IsOfficial = lang.IsOfficial,
                          GDP = combined.country.Gnp,
                          PopulationDensity = combined.country.Population > 0
                              ? combined.country.Gnp / combined.country.Population
                              : 0
                      })
                .Where(data => data.Population > 50000 && data.IsOfficial == "T")
                .OrderByDescending(data => data.GDP)
                .ThenBy(data => data.CityName)
                .Select(data => new
                {
                    data.CityName,
                    data.CountryName,
                    data.Language,
                    data.Population,
                    data.GDP,
                    data.PopulationDensity
                })
                .Take(100) // Chỉ lấy 100 dòng đầu tiên
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("advanced-complex-query")]
        public async Task<IActionResult> GetAdvancedComplexQuery()
        {
            var result = await _context.Cities
                .Join(_context.Countries,
                      city => city.CountryCode,
                      country => country.Code,
                      (city, country) => new { city, country })
                .Join(_context.CountryLanguages,
                      combined => combined.country.Code,
                      lang => lang.CountryCode,
                      (combined, lang) => new
                      {
                          City = combined.city,
                          Country = combined.country,
                          Language = lang.Language,
                          IsOfficial = lang.IsOfficial,
                          LanguagePercentage = lang.Percentage
                      })
                .Where(data => data.City.Population > 100000 &&
                               data.IsOfficial == "T" &&
                               data.LanguagePercentage > 10)
                .GroupBy(data => new
                {
                    data.Country.Code,
                    data.Country.Name,
                    data.Country.Region
                })
                .Select(group => new
                {
                    CountryCode = group.Key.Code,
                    CountryName = group.Key.Name,
                    Region = group.Key.Region,
                    TotalCities = group.Count(),
                    TotalPopulation = group.Sum(data => data.City.Population),
                    AverageCityPopulation = group.Average(data => data.City.Population),
                    DominantLanguage = group
                        .OrderByDescending(data => data.LanguagePercentage)
                        .Select(data => data.Language)
                        .FirstOrDefault(),
                    DominantLanguagePercentage = group
                        .OrderByDescending(data => data.LanguagePercentage)
                        .Select(data => data.LanguagePercentage)
                        .FirstOrDefault()
                })
                .Take(100)
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("hyper-complex-query")]
        public async Task<IActionResult> GetHyperComplexQuery()
        {
            var result = await _context.Cities
                // Join Cities với Countries
                .Join(_context.Countries,
                      city => city.CountryCode,
                      country => country.Code,
                      (city, country) => new { city, country })
                // Join kết quả trên với CountryLanguages
                .Join(_context.CountryLanguages,
                      combined => combined.country.Code,
                      lang => lang.CountryCode,
                      (combined, lang) => new
                      {
                          City = combined.city,
                          Country = combined.country,
                          Language = lang.Language,
                          IsOfficial = lang.IsOfficial,
                          LanguagePercentage = lang.Percentage
                      })
                // Join thêm với bảng Orders
                .GroupJoin(_context.Orders,
                           data => data.Country.Code,
                           order => order.CountryCode,
                           (data, orders) => new { data, orders })
                .SelectMany(joined => joined.orders.DefaultIfEmpty(), // Xử lý Orders null
                            (joined, order) => new
                            {
                                joined.data.City,
                                joined.data.Country,
                                joined.data.Language,
                                joined.data.IsOfficial,
                                joined.data.LanguagePercentage,
                                OrderAmount = order != null ? order.TotalAmount : 0,
                                OrderDate = order != null ? order.OrderDate : (DateTime?)null
                            })
                // Lọc dữ liệu dựa trên nhiều điều kiện
                .Where(data => data.City.Population > 100000 &&
                               data.IsOfficial == "T" &&
                               data.LanguagePercentage > 10 &&
                               data.OrderAmount > 5000)
                // Gom nhóm dữ liệu theo quốc gia và ngôn ngữ
                .GroupBy(data => new
                {
                    data.Country.Code,
                    data.Country.Name,
                    data.Country.Region,
                    data.Language
                })
                // Tính toán trên nhóm dữ liệu
                .Select(group => new
                {
                    CountryCode = group.Key.Code,
                    CountryName = group.Key.Name,
                    Region = group.Key.Region,
                    Language = group.Key.Language,
                    TotalCities = group.Count(),
                    TotalPopulation = group.Sum(data => data.City.Population),
                    TotalOrderAmount = group.Sum(data => data.OrderAmount),
                    AverageOrderAmount = group.Average(data => data.OrderAmount),
                    DominantCity = group.OrderByDescending(data => data.City.Population)
                                        .Select(data => data.City.Name)
                                        .FirstOrDefault()
                })
                // Sắp xếp và giới hạn kết quả
                .OrderByDescending(group => group.TotalOrderAmount)
                .ThenBy(group => group.CountryName)
                .Take(100) // Lấy 100 dòng đầu tiên
                .ToListAsync();

            return Ok(result);
        }

    }
}
