using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NSN.GNOCN.Tools.AWE.NSS;

namespace WindowsFormsApplication1
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private void btn_Raw_Gen_Click(object sender, EventArgs e)
        {
            try
            {
                MessageBox.Show("Raw Generation process started..");
                clsCSGetRawProcess obj = new clsCSGetRawProcess();
                obj.nodeTypeMain = new string[3];
                obj.nodeTypeMain[0] = "MSS";
                obj.nodeTypeMain[1] = "GCS";
                obj.nodeTypeMain[2] = "MGW";
                obj.groupName = "group1";
                obj.StartRawGeneration();

                clsCSGetRawProcess obj1 = new clsCSGetRawProcess();
                obj1.nodeTypeMain = new string[3];
                obj1.nodeTypeMain[0] = "MSS";
                obj1.nodeTypeMain[1] = "GCS";
                obj1.nodeTypeMain[2] = "MGW";
                obj1.groupName = "group2";
                obj1.StartRawGeneration();


                clsCSGetRawProcess obj2 = new clsCSGetRawProcess();
                obj2.nodeTypeMain = new string[3];
                obj2.nodeTypeMain[0] = "MSS";
                obj2.nodeTypeMain[1] = "GCS";
                obj2.nodeTypeMain[2] = "MGW";
                obj2.groupName = "group3";
                obj2.StartRawGeneration();
                MessageBox.Show("Raw Generation process Completed.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
    }
}
