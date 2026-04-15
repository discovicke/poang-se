using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace server.Service
{
    /// <summary>
    /// Affärslogik för poäng. Erbjuder generell hämtning och direkt sparande av <see cref="Score"/>-entiteter.
    /// Spelspecifik poänglogik (kumulativa värden, StartingScore m.m.) hanteras av <see cref="GameServices"/>.
    /// </summary>
    public class ScoreServices(AppDbContext db)
    {
        /// <summary>Returnerar alla poängregistreringar i databasen.</summary>
        public async Task<List<Score>> GetAllScores()
        {
            return await db.Scores.ToListAsync();
        }

        /// <summary>Sparar en ny poäng direkt utan att beräkna kumulativa värden.</summary>
        public async Task<Score> AddScore(Score score)
        {
            db.Scores.Add(score);
            await db.SaveChangesAsync();
            return score;
        }
    }
}
