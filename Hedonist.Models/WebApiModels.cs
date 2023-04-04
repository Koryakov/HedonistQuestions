using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hedonist.Models {
    public class QuestionsModel {
        public List<Question> Questions { get; set; } = new List<Question>();
        public bool IsPasswordValid { get; set; } = false;
        public string Ticket { get; set; } = string.Empty;

    }
}
