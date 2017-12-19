using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

namespace Parser
{
    public class Board
    {
        //Member variables
        private string serialNum = "N/A";
        private string controllerSerialNum;
        private bool multipleBoards = false;
        private string driverVersionNumber;
        private string firmwareVersion;
        private string boardModel;
        private string[] lines;
        private string controller;
        private Camera cam0 = new Camera(0);
        private Camera cam1 = new Camera(1);
        private Camera cam2 = new Camera(2);
        private Camera cam3 = new Camera(3);
        private SB6Controller SB6Cont;
        private List<SB6Controller> controllerList = new List<SB6Controller>();
        private List<int> xSheetResistances = new List<int>();
        private List<int> ySheetResistances = new List<int>();
        private List<string> warnings = new List<string>();
        private FirmwareChecker firmwareChecker = new FirmwareChecker();
        private string dateRange = String.Empty;
        private string startYear = String.Empty;
        private string endYear = String.Empty;
        private string startDayMonth = String.Empty;
        private string endDayMonth = String.Empty;


        //Regexes
        public const string sbxFormat = @"S?(B|M)(X|B|E)88(0|5)-(R2|H2|M2|V2|MP)-(A|\d)?(\d{6,7})";
        public const string spnlFormat = @"(L|K|M)(\d{3})(\w{2})(\d{2}\w\d{4})";     //Not tested in actual log yet
        //SBX880 short: G012HW25Y0576
        //SBX885 short: G012HW22U0598
        //Y indicates SBX880. U indicates SBX885.
        public const string sbxShortFormat = @"G\d{3}\w{2}\d{2}\w\d{4}";
        public const string sb6Format = @": (T|M|F)(\d{7,8})(\w)?(\d{0,8})(-F)?";
        public const string hardwareModelFormat = @"Hardware Model: (?<model>\w{1,6}((-\w{1,6})?))";
        public const string swVersionFormat = @"\(Version (?<version>\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})";
        public const string controllerFormat = @"\((?<version>SC(\d{1,2}))\)";
        public const string firmwareFormat = @"application version (?<version>\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})";
        public const string sheetResistanceFormat = @"(?<sheet>\w) Sheet resistance  is (?<inOrNot>good||OUT OF RANGE) at (?<resistance>\d{1,3}) ohms";
        //public const string cameraError = @"^(?<lineNumber>\d{1,6}): \((?<date>\d{1,2}\/\d{1,2}) .{1,15}\) - Error \d: \w{3} Camera (?<cameraNum>\d) - (?<error>[\w\s]+)\., Error Count: \w{1,3}";
        public const string cameraError = @"^(?<lineNumber>\d{1,6}): \((?<date>\d{1,2}\/\d{1,2}) .{1,15}\) - Error \d: \w{3} Camera (?<cameraNum>\d) - (?<error>[a-zA-Z0-9/\s]+)\., Error Count: \w{1,3}"; //Fixed a bug that was causing EEPROM errors to not be detected.
        public const string yearFormat = @"service (?<year1>\d{1,4})(\/|-|\s|\.)(\d{1,2})(\/|-|\s|\.)(?<year2>(\d{1,4}))";
        public const string dayMonthFormat = @"\d{1,5}: \((?<date>\d{1,2}\/\d{1,2})";


        //Constructor
        public Board(String[] log)
        { 
            lines = log;

            findController();
            findBoardModel();
            findSWVersion();
            findFirmware();
            findDateRange();

            if (boardModel.Equals("SB600"))
            {
                findSheetResistance();
                if (firmwareVersion.Substring(0, 1) == "3")
                {
                    controller = "SC12A";
                }
            }
            else if (boardModel.Equals("SBX800"))
            {
                serialNum = findSBXSerial();
                cameraErrors();

                if (serialNum.Substring(0, 1) == "G")
                {
                    boardModel = boardModel + " - Version 2 (SHORT)";
                }
                else
                {
                    if (controller.Equals("SC14"))
                    {
                        boardModel = boardModel + " - Version 1";
                    }
                    else
                    {
                        boardModel = boardModel + " - Version 2";
                    }
                }
            }
            else if (boardModel.Substring(0, 4).Equals("SPNL")) //Not tested to make sure length is correct
            {
                //add SPNL serial number checking (I guess this doesn't actually work)
                controller = "SPNL_CONTROLLER";
            }
            else if (boardModel.Equals("SBM600"))
            {
                controller = "DSP";
            }
            if (firmwareChecker.isValidController(controller))
            {
                if (firmwareChecker.isLatest(controller, firmwareVersion))
                {
                    firmwareVersion += " (Up-to-date)";
                }
                else
                {
                    firmwareVersion += " (Newer version available: " + firmwareChecker.getLatestVersion(controller) + ")";
                }
            }
            else
            {
                firmwareVersion += " (Controller type not found in firmware definitions)";
            }
        }

        //Member functions
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
        private string findSPNLSerial()
        {
            string sNumber = "Serial not found222";
            Regex r = new Regex(spnlFormat, RegexOptions.IgnoreCase);

            foreach (var line in lines)
            {
                var match = r.Match(line);

                if (match.Success)
                {
                    sNumber = match.Value;
                }
            }
            return sNumber;
        }

        //Finds the last driver version entry in the log
        private void findSWVersion()
        {
            Regex r = new Regex(swVersionFormat, RegexOptions.IgnoreCase);
            String version = "";

            foreach (var line in lines)
            {
                var match = r.Match(line);

                if (match.Success)
                {
                    version = match.Groups["version"].Value;
                }
            }
            if (!version.Equals(""))
            {

                driverVersionNumber = version;
            }
            else
            {
                driverVersionNumber = "Version not found";
            }
        }
        //Finds board model from the "Hardware Model: " string that appears in the log.
        //The constructor further refine board model using controller type or serial number.
        private void findBoardModel()
        {
            Regex r = new Regex(hardwareModelFormat, RegexOptions.IgnoreCase);
            String model = "";

            foreach (var line in lines)
            {
                var match = r.Match(line);

                if (match.Success)
                {
                    model = match.Groups["model"].Value;
                }
            }
            if (!model.Equals(""))
            {

                boardModel = model;
            }
            else
            {
                boardModel = "Model not found";
            }
        }
        //Finds the last Application firmware version entry in the log
        private void findFirmware()
        {
            Regex r = new Regex(firmwareFormat, RegexOptions.IgnoreCase);
            String firmware = "";

            foreach (var line in lines)
            {
                var match = r.Match(line);

                if (match.Success)
                {
                    firmware = match.Groups["version"].Value;
                }
            }
            if (!firmware.Equals(""))
            {

                firmwareVersion = firmware;
            }
            else
            {
                firmwareVersion = "Version not found.";
            }
        }
        //Initializes controllerList, which is a List of SB6Controller objects.
        //Each SB6Controller has its own deltas, controller serial number, and
        //a list of out of range messages.
        private void findSheetResistance()
        {
            SB6Cont = new SB6Controller("No serial");
            var hashset = new HashSet<string>();
            Regex r = new Regex(sheetResistanceFormat, RegexOptions.IgnoreCase);

            Regex r2 = new Regex(sb6Format, RegexOptions.IgnoreCase);

            //Find the first 
            foreach (var line in lines)
            {
                var serialMatch = r2.Match(line);

                if (serialMatch.Success && controllerList.Count < 1)
                {
                    SB6Cont = new SB6Controller(serialMatch.Value);
                    break;
                }
            }
            foreach (var line in lines)
            {
                var serialMatch = r2.Match(line);

                if (serialMatch.Success)
                {
                    //Check if the controller serial number has changed
                    if (!SB6Cont.getSerial().Equals(serialMatch.Value))
                    {
                        //Calculates the deltas for the SB6Controller object
                        SB6Cont.calculateDeltas();
                        //Adds the current SB6Controller to the list.
                        controllerList.Add(SB6Cont);
                        //Set multipleBoards to true for warning message in form
                        multipleBoards = true;
                        //Create a new SB6Controller object with the new controller's serial number
                        SB6Cont = new SB6Controller(serialMatch.Value);
                    }
                }

                var match = r.Match(line);

                if (match.Success)
                {
                    //Check if the sheet resistance value is out of range
                    if (match.Groups["inOrNot"].Value.Equals("OUT OF RANGE"))
                    {
                        //Checks if the sheet resistance value is unique
                        //to avoid spamming the log.
                        if (hashset.Add(match.Value))
                        {
                            //Adds to log if value is unique
                            SB6Cont.addOutOfRange(match.Value);
                        }
                    }
                    else
                    {
                        if (match.Groups["sheet"].Value.Equals("X"))
                        {
                            SB6Cont.addXValue(Int32.Parse(match.Groups["resistance"].Value));
                        }
                        else if (match.Groups["sheet"].Value.Equals("Y"))
                        {
                            SB6Cont.addYValue(Int32.Parse(match.Groups["resistance"].Value));
                        }
                    }
                }
                else if(line.Contains("Constant contact detected"))
                {
                    SB6Cont.incrementConstantContactError(); ;
                }
               
            }
                //Entire log has been processed at this point.
                //Calculate the deltas for the SB6Controller object
                //and then add it to the list.
                SB6Cont.calculateDeltas();
                controllerList.Add(SB6Cont);
        }
        private void findController()
        {
            Regex r = new Regex(controllerFormat, RegexOptions.IgnoreCase);
            String controllerType = "";

            foreach (var line in lines)
            {
                var match = r.Match(line);

                if (match.Success)
                {
                    controllerType = match.Groups["version"].Value;
                }
            }
            if (!controllerType.Equals(""))
            {

                controller = controllerType;
            }
            else
            {
                controller = "Controller model not found.";
            }
        }

        private void cameraErrors()
        {
            Regex r = new Regex(cameraError, RegexOptions.IgnoreCase);

            foreach (var line in lines)
            {
                var match = r.Match(line);

                if (match.Success)
                {
                    int camNum = Int32.Parse(match.Groups["cameraNum"].Value);
                    switch(camNum)
                    {
                        case 0:
                            cam0.addError(camErrorMessage(match));
                            break;
                        case 1:
                            cam1.addError(camErrorMessage(match));
                            break;
                        case 2:
                            cam2.addError(camErrorMessage(match));
                            break;
                        case 3:
                            cam3.addError(camErrorMessage(match));
                            break;
                        default:
                            break;         
                    }
                }
            }
        }
        //Helper method to format the camera error message
        private String camErrorMessage(Match m)
        {
            return "Line: " + m.Groups["lineNumber"].Value + " " +
                m.Groups["date"].Value + " ERROR: " +
                m.Groups["error"];
        }

        private void findDateRange()
        {
            Regex r = new Regex(yearFormat, RegexOptions.IgnoreCase);
            Regex r2 = new Regex(dayMonthFormat, RegexOptions.IgnoreCase);

            string dayMonthText = String.Empty;
            string yearText = String.Empty;
            string year1 = String.Empty;
            string year2 = String.Empty;

            bool yearNotFound = true;
            foreach (var line in lines)
            {
                
                var yearMatch = r.Match(line);

                if (yearMatch.Success)
                {
                    year1 = yearMatch.Groups["year1"].Value;
                    year2 = yearMatch.Groups["year2"].Value;

                    if (year1.Length > year2.Length)
                    {
                        yearText = year1;
                    }
                    //dd/mm/yy
                    else if(year1.Length == year2.Length)
                    {
                     
                        yearText = "20" + year2;
                    }
                    else
                    {
                        yearText = year2;
                    }

                    if (yearNotFound)
                    {
                        startYear = yearText;
                        yearNotFound = false;
                    }
                    else
                    {
                        endYear = yearText;
                    }
                }

                var dayMonthMatch = r2.Match(line);

                if (dayMonthMatch.Success)
                {
                    dayMonthText = dayMonthMatch.Groups["date"].Value;

                    if (startDayMonth.Equals(String.Empty))
                    {
                        startDayMonth = dayMonthText;
                    }
                    else
                    {
                        endDayMonth = dayMonthText;
                    }
                }
            }
            //Log only covers one year
            if(startYear.Equals(endYear))
            {
                dateRange = startDayMonth +  " - " + endDayMonth + " " + startYear;
            }
            else
            {
                //Only one date entry that included year found
                if(endYear.Equals(String.Empty))
                {
                    dateRange = startDayMonth + " - " + endDayMonth + " " + startYear;
                }
                //Multiple years found in log
                else
                {
                    dateRange = startDayMonth + " " + startYear + " - " + endDayMonth + " " + endYear;
                }
            }
        }

        //Getters
        public String getSWVersion()
        {
            return driverVersionNumber;
        }
        public String getBoardModel()
        {
            return boardModel;
        }
        public String getSerialNumber()
        {
            return serialNum;
        }
        public String getController()
        {
            return controller;
        }
        public String getFirmwareVersion()
        {
            return firmwareVersion;
        }
        public List<String> getCam0Errors()
        {
            return cam0.getCamErrors();
        }
        public List<string> getCam1Errors()
        {
            return cam1.getCamErrors();
        }
        public List<string> getCam2Errors()
        {
            return cam2.getCamErrors();
        }
        public List<string> getCam3Errors()
        {
            return cam3.getCamErrors();
        }
        public int getCam0ErrorCount()
        {
            return cam0.getErrorCount();
        }
        public int getCam1ErrorCount()
        {
            return cam1.getErrorCount();
        }
        public int getCam2ErrorCount()
        {
            return cam2.getErrorCount();
        }
        public int getCam3ErrorCount()
        {
            return cam3.getErrorCount();
        }
        public bool hasDuplicates()
        {
            return multipleBoards;
        }
        public List<int> getXSheetResistances()
        {
            return xSheetResistances;
        }
        public List<int> getYSheetResistances()
        {
            return ySheetResistances;
        }
        public List<SB6Controller> getControllerList()
        {
            return controllerList;
        }
        public List<String> getWarnings()
        {
            return warnings;
        }
        public string getDateRange()
        {
            return dateRange;
        }
    }
}
