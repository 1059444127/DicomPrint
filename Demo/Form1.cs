﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DicomPrint;
using DicomPrint.PrintSCP;
using DicomPrint.PrintSCU;

namespace Demo
{
    public partial class Form1 : Form
    {
        private List<PrintTaskInfo> _printTasks = new List<PrintTaskInfo>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            PrintSCPService.AETitle = "PRINTSCP";
            PrintSCPService.DicomPath = "D:\\PrintFolder";
            PrintSCPService.LogPath = "C:\\PrintSCPLog";
            PrintSCPService.Port = 8431;
            PrintSCPService.SCPType = PrintSCPType.GrayScaleColours;

            PrintSCPService.PrintTaskEvent += PrintSCPService_PrintTaskEvent;
            PrintSCPService.EchoEvent += PrintSCPService_EchoEvent;
            //set replace tags
            List<ReplaceTag> replaceTags = new List<ReplaceTag>()
            {
                new ReplaceTag(0x2000, 0x0020, "HIGH"), //Print Priority
                new ReplaceTag(0x2010, 0x0050, "17INX17IN "), //Film Size ID
                new ReplaceTag(0x2020, 0x0010, "2") //Image position
            };
            PrintSCPService.ReplaceTags = replaceTags;

            PrintSCPService.Start();

            statusLabel.Text = "Print SCP Service started!";

            //setup print SCU
            PrintSCUService.LogPath = "C:\\PrintSCULog";
            PrintSCUService.PrintTaskEvent += PrintSCUService_PrintTaskEvent;
        }

        private void gridViewTasks_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewColumn column = gridViewTasks.Columns[e.ColumnIndex];
                if (column is DataGridViewButtonColumn)
                {
                    string taskPath = gridViewTasks.Rows[e.RowIndex].Cells[3].Value as string;

                    //Send to dicom print by using PrintSCUService
                    string callingAE = "PRINTSCU";
                    string calledIP = "localhost";
                    string calledAE = "PRINTSCP";
                    int calledPort = 8430;

                    PrintSCUService.SendPrintTask(callingAE, calledAE, calledIP, calledPort, taskPath);
                }
            }
        }

        private void PrintSCPService_EchoEvent(EchoInfo echo)
        {
            Invoke((MethodInvoker)(() =>
            {
                string strMsg = string.Format("Get echo, CallingAE: {0}, CallingIP: {1}", echo.CallingAETitle, echo.CallingIP);
                statusLabel.Text = strMsg;

                MessageBox.Show(strMsg);
            }));
        }

        private void PrintSCPService_PrintTaskEvent(PrintTaskInfo task)
        {
            Invoke((MethodInvoker)(() =>
            {
                //new print task created by PrintSCP Service
                _printTasks.Add(task);

                bindingSource1.DataSource = null;
                bindingSource1.DataSource = _printTasks;
            }));
        }

        void PrintSCUService_PrintTaskEvent(PrintTaskInfo task)
        {
            Invoke((MethodInvoker)(() =>
            {
                if (task.HasError)
                {
                    MessageBox.Show("Send print task failed due to " + task.ErrorMessage);
                }
                else
                {
                    MessageBox.Show("Success print task success:  " + task.TaskPath);
                }
            }));
        }

    }
}
