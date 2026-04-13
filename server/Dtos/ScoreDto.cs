using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.Dtos
{
    public class ScoreDto
    {
        public Guid Id { get; set; }
        public Guid PlayerId { get; set; }
        public double Value { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; } = "";
    }
}