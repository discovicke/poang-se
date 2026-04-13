using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace server.Service
{
    public class ScoreServices(AppDbContext db)
    {
        public async Task<List<Score>> GetAllScores()
        {
            return await db.Scores.ToListAsync();
        }

        public async Task<Score> AddScore(Score score)
        {
            db.Scores.Add(score);
            await db.SaveChangesAsync();
            return score;
        }
    }
}
