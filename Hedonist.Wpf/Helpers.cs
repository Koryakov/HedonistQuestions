using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hedonist.Wpf.Helpers {
    internal class AppSettingsHelper {
        public static Settings Settings { get; set; } = new Settings();
        static AppSettingsHelper() {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();
            Settings = config.GetRequiredSection("Settings").Get<Settings>();
        }
    }
}
