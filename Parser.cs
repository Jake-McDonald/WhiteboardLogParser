using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication3
{
    public class Log
    {
        private List<List<string>> logSections = new List<List<string>>();





        //Parse the log and break it up by serial number/board model.
        //Place each portion of the log in a separate of strings.
        public void parseLog()
        {
            /*Duplicate Regex or move it over from Board class. Need to check
            if serial number or board model changes. If it changes then the current portion of the log
            needs to be added to a List of log sections and a new log section needs to be created. The
            Form class can then loop through an array of boards and display each board
            */
        }
        /*
         * Three items that could potentially change. Serial number, board model, controller serial number. 
         * 
         * 
        private String findSBXSerial()
        {
            int matches = 0;
            String sNumber = "";

            //Check for SBX800 v1 or v2 serial numbers
            Regex r = new Regex(sbxFormat, RegexOptions.IgnoreCase);

            foreach (var line in lines)
            {
                var match = r.Match(line);

                if (match.Success)
                {
                    sNumber = match.Value;
                    matches++;
                }
            }
            if (matches >= 1)
            {
                return sNumber;
            }
            else
            {
                //Check for SBX800 v3 (SBX8 Short) serail numbers
                r = new Regex(sbxShortFormat, RegexOptions.IgnoreCase);

                foreach (var line in lines)
                {
                    var match = r.Match(line);

                    if (match.Success)
                    {
                        sNumber = match.Value;
                        matches++;
                    }
                }
                if (matches >= 1)
                {
                    return sNumber;
                }
                else return "Not found.";
            }
            
        }
        */
    }
}
