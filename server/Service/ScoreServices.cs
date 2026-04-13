using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.Service
{
    public class ScoreServices
    {
        private readonly AppDbContext _db;

        public ScoreServices(AppDbContext db)
        {
            _db = db;
        }
        public async Task<List<Score>> GetAllScores()
        {


        }

    }
}