using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
using Parser;

namespace WindowsFormsApplication3
{

    public partial class Form1 : Form
    {
        private String messageText = "";
        public const string nLine = "\r\n";
        public Form1()
        {
            InitializeComponent();
            textBox1.Font = new Font(textBox1.Font, FontStyle.Bold);
            textBox1.Text = "Please open a log file";
            
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void printErrors(List<string> errors)
        {
            for (int i = 0; i < errors.Count; i++)
            {
                messageText += (errors[i] + nLine);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox1.Text);
            textBox1.Focus();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = String.Empty;
            messageText = String.Empty;
            OpenFileDialog theDialog = new OpenFileDialog();
            //theDialog.InitialDirectory = "c:\\";
            theDialog.InitialDirectory = "C:\\Users\\SMART\\Documents\\Logs"; //Only for testing. Remove later.
            theDialog.Title = "Open Text File";
            theDialog.Filter = "Log files |*.log;*.txt";
            if (theDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((theDialog.OpenFile()) != null)
                    {
                        //textBox1.Visible = true;
                        String fileName = theDialog.FileName;
                        this.Text ="Log File Parser - " + theDialog.SafeFileName;

                        string[] log = File.ReadAllLines(fileName);

                        var Board = new Board(log);
                        //Default info for all boards
                        messageText += ("Date Range: " + Board.getDateRange() + nLine);
                        messageText += ("Board Model: " + Board.getBoardModel() + nLine);
                        messageText += ("Board Serial Number: " + Board.getSerialNumber() + nLine);
                        messageText += ("Driver version: " + Board.getSWVersion() + nLine);
                        messageText += ("Controller Model: " + Board.getController() + nLine);
                        messageText += ("Firmware Version: " + Board.getFirmwareVersion() + nLine + nLine);
                        //SB600
                        if (Board.getBoardModel().Equals("SB600"))
                        {
                            List<SB6Controller> controllerList = Board.getControllerList();
                            if (Board.hasDuplicates())
                            {
                                textBox1.Text += ("Multiple Boards/Controllers detected!" + nLine + nLine);
                            }
                            foreach (SB6Controller controller in controllerList)
                            {

                                messageText += ("Controller serial number: " + controller.getSerial() + nLine);
                                messageText += ("Sheet resistance entries: " + controller.getNumEntries() + nLine);
                                messageText += ("X Delta: " + controller.getXDelta() + nLine);
                                messageText += ("Y Delta: " + controller.getYDelta() + nLine);

                                if (controller.getOutOfRangeMessages().Count > 0)
                                {
                                    messageText += ("Warnings: \r\n");
                                    for (int i = 0; i < controller.getOutOfRangeMessages().Count; i++)
                                    {
                                        messageText += (controller.getOutOfRangeMessages()[i] + nLine);
                                    }
                                }
                                if (controller.getNumConstantContactErrors() > 0)
                                {
                                    messageText += controller.getNumConstantContactErrors() 
                                        + " Constant Contact errors found." + nLine + nLine;
                                }
                                else
                                {
                                    messageText += nLine;
                                }
                            }
                        }
                        //SBX800
                        else if (Board.getBoardModel().Equals("SBX800") ||
                            Board.getBoardModel().Equals("SBX800 - Version 1") ||
                            Board.getBoardModel().Equals("SBX800 - Version 2") ||
                            Board.getBoardModel().Equals("SBX800 - Version 2 (SHORT)"))
                        {
                            messageText += ("Camera 0 Errors:" + nLine);

                            if (Board.getCam0ErrorCount() > 0)
                            {
                                printErrors(Board.getCam0Errors());
                            }
                            else
                            {
                                messageText += ("No errors found." + nLine);
                            }
                            messageText += (nLine);

                            messageText += ("Camera 1 Errors:" + nLine);
                            if (Board.getCam1ErrorCount() > 0)
                            {
                                printErrors(Board.getCam1Errors());
                            }
                            else
                            {
                                messageText += ("No errors found." + nLine);
                            }
                            messageText += (nLine);

                            messageText += ("Camera 2 Errors:" + nLine);
                            if (Board.getCam2ErrorCount() > 0)
                            {
                                printErrors(Board.getCam2Errors());
                            }
                            else
                            {
                                messageText += ("No errors found." + nLine);
                            }
                            messageText += (nLine);

                            messageText += ("Camera 3 Errors:" + nLine);
                            if (Board.getCam3ErrorCount() > 0)
                            {
                                printErrors(Board.getCam3Errors());
                            }
                            else
                            {
                                messageText += ("No errors found." + nLine);
                            }
                        }
                    }
                    textBox1.AppendText(messageText);
                    textBox1.Focus();
                }
                catch (FileNotFoundException ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }
    }
}

