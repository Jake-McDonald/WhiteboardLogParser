using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{

    public class FirmwareChecker
    {
        private Dictionary<string, string> latestVersion;

        public FirmwareChecker()
        {
            this.latestVersion = new Dictionary<string, string>()
             {
                 {"SC12", "1.3.10.0"},
                 {"SC12A", "3.0.3.0" },
                 {"SC9", "5.3.0.0" },
                 {"SC14", "2.0.3.0" },
                 {"SC15", "12.0.5.0" },
                 {"SPNL_CONTROLLER", "2.1.5.3" },
                 {"DSP", "7.2.31.0" }
             };
        }
        public bool isLatest(String controller, String currentVersion)
        {
            String latest = "";
            if(latestVersion.TryGetValue(controller, out latest))
            {
                if (currentVersion.Equals(latest)) return true;
                else return false;
            }
            return true;
        }
        public bool isValidController(String controller)
        {
            String latest = "";
            if (latestVersion.TryGetValue(controller, out latest))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public String getLatestVersion(String controller)
        {
            String latest = "";
            if(latestVersion.TryGetValue(controller, out latest))
            {
                return latest;
            }
            else return latest;
        }
    }
}
