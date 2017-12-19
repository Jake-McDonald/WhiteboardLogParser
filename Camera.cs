using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public class Camera
    {
        private int camNumber;
        private List<String> camErrors= new List<string>();
        private int errorCount = 0;

        //Constructor
        public Camera(int number)
        {
            this.camNumber = number;
        }
        //Public Methods
        public void addError(string error)
        {
            camErrors.Add(error);
            errorCount++;
        }
        //Getters
        public int getCamNumber()
        {
            return camNumber;
        }
        public List<String> getCamErrors()
        {
            return camErrors;
        }
        public int getErrorCount()
        {
            return errorCount;
        }
    }
}
