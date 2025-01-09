using MasstransitSaga.Core.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MasstransitReactApp.Server.Controllers
{
    [Route("api/country")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        private readonly WorldDbContext _context;
        public CountryController(WorldDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Đếm tổng số quốc gia. Truy vấn nhẹ, kiểm tra khả năng phản hồi.
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        [HttpGet("count")]
        public async Task<IActionResult> GetCountryCount()
        {
            var count = await _context.Countries.CountAsync();
            return Ok(count);
        }

        /// <summary>
        /// Lọc quốc gia theo vùng, kiểm tra truy vấn có điều kiện.
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        [HttpGet("by-region/{region}")]
        public async Task<IActionResult> GetCountriesByRegion(string region)
        {
            var countries = await _context.Countries
                .Where(c => c.Region == region)
                .ToListAsync();
            return Ok(countries);
        }

        /// <summary>
        /// Lấy 10 quốc gia đông dân nhất, kiểm tra sắp xếp và giới hạn dữ liệu.
        /// </summary>
        /// <returns></returns>
        [HttpGet("largest-population")]
        public async Task<IActionResult> GetLargestPopulationCountries()
        {
            var largestPopulations = await _context.Countries
                .OrderByDescending(c => c.Population)
                .Take(10)
                .ToListAsync();
            return Ok(largestPopulations);
        }

        /// <summary>
        /// Tính GDP bình quân đầu người, truy vấn có tính toán phức tạp.
        /// </summary>
        /// <returns></returns>
        [HttpGet("gdp-per-capita")]
        public async Task<IActionResult> GetGDPPerCapita()
        {
            var gdpPerCapita = await _context.Countries
                .Select(c => new
                {
                    c.Name,
                    GDPPerCapita = c.Gnp / (c.Population > 0 ? c.Population : 1)
                })
                .OrderByDescending(c => c.GDPPerCapita)
                .ToListAsync();
            return Ok(gdpPerCapita);
        }

        /// <summary>
        /// Gom nhóm quốc gia theo vùng và đếm số lượng, kiểm tra GroupBy.
        /// </summary>
        /// <returns></returns>
        [HttpGet("group-by-region")]
        public async Task<IActionResult> GetCountriesGroupedByRegion()
        {
            var groupedCountries = await _context.Countries
                .GroupBy(c => c.Region)
                .Select(group => new
                {
                    Region = group.Key,
                    CountryCount = group.Count()
                })
                .ToListAsync();
            return Ok(groupedCountries);
        }

        /// <summary>
        /// Tính tổng diện tích của các quốc gia, kiểm tra hiệu suất với Sum.
        /// </summary>
        /// <returns></returns>
        [HttpGet("total-surface-area")]
        public async Task<IActionResult> GetTotalSurfaceArea()
        {
            var totalSurfaceArea = await _context.Countries.SumAsync(c => c.SurfaceArea);
            return Ok(totalSurfaceArea);
        }

        /// <summary>
        /// Lọc quốc gia có GNP lớn hơn trung bình, kiểm tra Average và Where.
        /// </summary>
        /// <returns></returns>
        [HttpGet("above-average-gdp")]
        public async Task<IActionResult> GetCountriesAboveAverageGDP()
        {
            var averageGNP = await _context.Countries.AverageAsync(c => c.Gnp);
            var countries = await _context.Countries
                .Where(c => c.Gnp > averageGNP)
                .OrderByDescending(c => c.Gnp)
                .ToListAsync();
            return Ok(new { AverageGNP = averageGNP, Countries = countries });
        }

    }
}
