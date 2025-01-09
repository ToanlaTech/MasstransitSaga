using MasstransitSaga.Core.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MasstransitReactApp.Server.Controllers
{
    [Route("api/city")]
    [ApiController]
    public class CityController : ControllerBase
    {
        private readonly WorldDbContext _context;
        public CityController(WorldDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Đếm tổng số thành phố. Truy vấn nhẹ, kiểm tra khả năng phản hồi.
        /// </summary>
        /// <returns></returns>
        [HttpGet("count")]
        public async Task<IActionResult> GetCityCount()
        {
            var count = await _context.Cities.CountAsync();
            return Ok(count);
        }

        /// <summary>
        /// Lọc các thành phố theo mã quốc gia, kiểm tra truy vấn có điều kiện.
        /// </summary>
        /// <param name="countryCode"></param>
        /// <returns></returns>
        [HttpGet("by-country/{countryCode}")]
        public async Task<IActionResult> GetCitiesByCountry(string countryCode)
        {
            var cities = await _context.Cities
                .Where(c => c.CountryCode == countryCode)
                .ToListAsync();
            return Ok(cities);
        }

        /// <summary>
        /// Lấy 10 thành phố có dân số lớn nhất, kiểm tra hiệu quả của OrderBy và Take.
        /// </summary>
        /// <returns></returns>
        [HttpGet("largest-cities")]
        public async Task<IActionResult> GetLargestCities()
        {
            var largestCities = await _context.Cities
                .OrderByDescending(c => c.Population)
                .Take(10)
                .ToListAsync();
            return Ok(largestCities);
        }

        /// <summary>
        /// Gom nhóm và đếm số lượng thành phố theo quốc gia, kiểm tra GroupBy
        /// </summary>
        /// <returns></returns>
        [HttpGet("group-by-country")]
        public async Task<IActionResult> GetCitiesGroupedByCountry()
        {
            var groupedCities = await _context.Cities
                .GroupBy(c => c.CountryCode)
                .Select(group => new
                {
                    CountryCode = group.Key,
                    CityCount = group.Count()
                })
                .ToListAsync();
            return Ok(groupedCities);
        }

        /// <summary>
        /// Tính tổng dân số, kiểm tra hiệu năng của Sum
        /// </summary>
        /// <returns></returns>
        [HttpGet("total-population")]
        public async Task<IActionResult> GetTotalPopulation()
        {
            var totalPopulation = await _context.Cities.SumAsync(c => c.Population);
            return Ok(totalPopulation);
        }

        /// <summary>
        /// Phân trang dữ liệu, kiểm tra khả năng xử lý lượng dữ liệu lớn theo từng phần.
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("paged/{pageNumber}/{pageSize}")]
        public async Task<IActionResult> GetPagedCities(int pageNumber, int pageSize)
        {
            var cities = await _context.Cities
                .OrderBy(c => c.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return Ok(cities);
        }

        /// <summary>
        /// Lọc các thành phố có dân số lớn hơn mức trung bình, truy vấn phức tạp với Average và Where
        /// </summary>
        /// <returns></returns>
        [HttpGet("above-average-population")]
        public async Task<IActionResult> GetCitiesAboveAveragePopulation()
        {
            var averagePopulation = await _context.Cities.AverageAsync(c => c.Population);
            var cities = await _context.Cities
                .Where(c => c.Population > averagePopulation)
                .OrderByDescending(c => c.Population)
                .ToListAsync();
            return Ok(new { AveragePopulation = averagePopulation, Cities = cities });
        }

    }
}
