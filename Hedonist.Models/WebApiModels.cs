﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hedonist.Models {

    public class AuthenticationData {
        public string Password { get; set;}
        public string DeviceIdentifier { get; set;}
        public string TerminalName { get; set; }

    }
    public class QuizData {
        public List<Question> Questions { get; set; }
        public List<Answer> Answers { get; set; }
    }
}
