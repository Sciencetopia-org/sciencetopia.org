using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
using Sciencetopia.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sciencetopia.Controllers
{
    [Authorize]
    public class RecommendationsController : Controller
    {
        private readonly IDriver _driver;

        public RecommendationsController(IDriver driver)
        {
            _driver = driver;
        }

        public async Task<IActionResult> Index()
        {
            var recommendations = await GetRecommendationsForUserAsync(User.Identity.Name);
            return View(recommendations);
        }

        private async Task<List<Recommendation>> GetRecommendationsForUserAsync(string userName)
        {
            var recommendations = new List<Recommendation>();

            using (var session = _driver.AsyncSession())
            {
                var result = await session.ReadTransactionAsync(async tx =>
                {
                    // 在这里编写您的推荐查询逻辑
                    // 例如：找出用户最常访问的资源类型，并返回与之相关的推荐资源
                    var cypherQuery = @"
                        MATCH (u:User {UserName: $UserName})-[r:VISITED]->(res:Resource)
                        RETURN res.Title as Title, res.Url as Url
                        ORDER BY r.Count DESC
                        LIMIT 5";
                    
                    return await tx.RunAsync(cypherQuery, new { UserName = userName });
                });

                await foreach (var record in result)
                {
                    recommendations.Add(new Recommendation
                    {
                        Title = record["Title"].As<string>(),
                        Url = record["Url"].As<string>()
                    });
                }
            }

            return recommendations;
        }
    }
}
