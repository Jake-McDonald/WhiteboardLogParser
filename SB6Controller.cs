using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public class SB6Controller
    {
        private List<int> xSheetResistances = new List<int>();
        private List<int> ySheetResistances = new List<int>();
        private int xDelta;
        private int yDelta;
        private String serialNumber;
        private List<string> constantContactMessages = new List<string>();
        private List<string> outOfRangeMessages = new List<string>();
        private int numConstantContactErrors = 0;

        public SB6Controller(String serial)
        {
            this.serialNumber = serial;
        }
        public void addXValue(int value)
        {
            xSheetResistances.Add(value);
        }
        public void addYValue(int value)
        {
            ySheetResistances.Add(value);
        }
        public int getXDelta()
        {
            return xDelta;
        }
        public int getYDelta()
        {
            return yDelta;
        }
        public int getNumEntries()
        {
            if(xSheetResistances.Count >= ySheetResistances.Count)
            {
                return xSheetResistances.Count;
            }
            else return ySheetResistances.Count;
        }
        public void calculateDeltas()
        {
            if (xSheetResistances.Count > 0)
            {
                xDelta = Math.Abs(xSheetResistances.Max() - xSheetResistances.Min());

            }
            if (ySheetResistances.Count > 0)
            {
                yDelta = Math.Abs(ySheetResistances.Max() - ySheetResistances.Min());
            }
        }
        public String getSerial()
        {
            return serialNumber;
        }
        public List<string> getOutOfRangeMessages()
        {
            return outOfRangeMessages;
        }
        public int getNumConstantContactErrors()
        {
            return numConstantContactErrors;
        }
        public void addOutOfRange(String message)
        {
            outOfRangeMessages.Add(message);
        }
        public void incrementConstantContactError()
        {
            numConstantContactErrors++;
        }
    }
}
