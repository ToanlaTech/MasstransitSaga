using MasstransitSaga.Core.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MasstransitReactApp.Server.Controllers
{
    [Route("api/countrylanguage")]
    [ApiController]
    public class CountryLanguageController : ControllerBase
    {
        private readonly WorldDbContext _context;
        public CountryLanguageController(WorldDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Đếm tổng số ngôn ngữ. Truy vấn nhẹ để kiểm tra phản hồi.
        /// </summary>
        /// <returns></returns>
        [HttpGet("count")]
        public async Task<IActionResult> GetLanguageCount()
        {
            var count = await _context.CountryLanguages.CountAsync();
            return Ok(count);
        }

        /// <summary>
        /// Lọc các ngôn ngữ theo mã quốc gia, kiểm tra truy vấn có điều kiện.
        /// </summary>
        /// <param name="countryCode"></param>
        /// <returns></returns>
        [HttpGet("by-country/{countryCode}")]
        public async Task<IActionResult> GetLanguagesByCountry(string countryCode)
        {
            var languages = await _context.CountryLanguages
                .Where(cl => cl.CountryCode == countryCode)
                .ToListAsync();
            return Ok(languages);
        }

        /// <summary>
        /// Lấy 10 ngôn ngữ được nói nhiều nhất, kiểm tra OrderByDescending và Take.
        /// </summary>
        /// <returns></returns>
        [HttpGet("most-spoken")]
        public async Task<IActionResult> GetMostSpokenLanguages()
        {
            var mostSpokenLanguages = await _context.CountryLanguages
                .OrderByDescending(cl => cl.Percentage)
                .Take(10)
                .ToListAsync();
            return Ok(mostSpokenLanguages);
        }

        /// <summary>
        /// Lọc danh sách các ngôn ngữ chính thức, kiểm tra truy vấn với điều kiện Boolean.
        /// </summary>
        /// <returns></returns>
        [HttpGet("official-languages")]
        public async Task<IActionResult> GetOfficialLanguages()
        {
            var officialLanguages = await _context.CountryLanguages
                .Where(cl => cl.IsOfficial == "T")
                .ToListAsync();
            return Ok(officialLanguages);
        }

        /// <summary>
        /// Gom nhóm ngôn ngữ theo quốc gia và đếm số lượng, kiểm tra GroupBy.
        /// </summary>
        /// <returns></returns>
        [HttpGet("group-by-country")]
        public async Task<IActionResult> GetLanguagesGroupedByCountry()
        {
            var groupedLanguages = await _context.CountryLanguages
                .GroupBy(cl => cl.CountryCode)
                .Select(group => new
                {
                    CountryCode = group.Key,
                    LanguageCount = group.Count()
                })
                .ToListAsync();
            return Ok(groupedLanguages);
        }

        /// <summary>
        /// Gom nhóm ngôn ngữ, tính số quốc gia sử dụng và phần trăm trung bình, kiểm tra truy vấn tổng hợp
        /// </summary>
        /// <returns></returns>
        [HttpGet("language-distribution")]
        public async Task<IActionResult> GetLanguageDistribution()
        {
            var languageDistribution = await _context.CountryLanguages
                .GroupBy(cl => cl.Language)
                .Select(group => new
                {
                    Language = group.Key,
                    Countries = group.Count(),
                    AveragePercentage = group.Average(cl => cl.Percentage)
                })
                .OrderByDescending(ld => ld.Countries)
                .ToListAsync();
            return Ok(languageDistribution);
        }

        /// <summary>
        /// Lọc các ngôn ngữ có phần trăm sử dụng cao hơn mức trung bình, kiểm tra Average và Where.
        /// </summary>
        /// <returns></returns>
        [HttpGet("above-average-usage")]
        public async Task<IActionResult> GetLanguagesAboveAverageUsage()
        {
            var averageUsage = await _context.CountryLanguages.AverageAsync(cl => cl.Percentage);
            var languages = await _context.CountryLanguages
                .Where(cl => cl.Percentage > averageUsage)
                .OrderByDescending(cl => cl.Percentage)
                .ToListAsync();
            return Ok(new { AverageUsage = averageUsage, Languages = languages });
        }

    }
}
