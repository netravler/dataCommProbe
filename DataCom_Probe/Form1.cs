using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//
using System.Net;



namespace DataCom_Probe
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void probeToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void probeToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // ok this is a simple test app prior to creating a web service.
            // first we get the webpage to probe then loop through incrementing page by page until 
            // we get an error.  Ok yes the first pages does tell you the number of pages
            // but I'm lazy deal with it.
            /*
             * this is a representitive sample and our test victim.
             * page url decode:
             *      first page https://connect.data.com/directory/company/list/jKpOTyCSHEU8JBeIGLJECA/people-s-united-bank
             *      each following page notice the page count on the end: https://connect.data.com/directory/company/list/jKpOTyCSHEU8JBeIGLJECA/people-s-united-bank?page=2
            */

            int pageNumber = 1;
            string probePage = textBox1.Text;

            try
            {
                while (pageNumber > 0)
                {
                    WebRequest request = WebRequest.Create(@probePage);
                    WebResponse response = request.GetResponse();
                    Stream data = response.GetResponseStream();
                    string html = String.Empty;
                    using (StreamReader sr = new StreamReader(data))
                    {
                        richTextBox2.AppendText(sr.ReadToEnd());
                        richTextBox2.Refresh();
                        // 
                        pageNumber = Convert.ToInt32(label2.Text);
                        label2.Text = Convert.ToString(pageNumber + 1);
                        label2.Refresh();
                        data.Close();
                    }

                    // ok done with this one close the webrequest make a new page entry and run again.

                    if (richTextBox2.Find(@"Sorry.") > 0)
                    {
                        pageNumber = 0;
                    }

                    probePage = textBox1.Text + "?page=" + Convert.ToString(label2.Text);
                    
                }

                // clean the data here.  This won't work for the web service

                foreach (String lineRTB2 in richTextBox2.Lines)
                {
                    if (lineRTB2.Contains("span title") || lineRTB2.Contains("<td>"))
                    {
                        richTextBox1.AppendText(lineRTB2 + "\n");
                        richTextBox1.Refresh();
                    }
                }

                cleanRTB(); // ok clean it up and make ready for csv

            }
            catch
            {
                richTextBox1.AppendText("ERRERRERRERRERRERRERR");
            }
        }

        private void cleanRTB() 
        {
            // ok, removes all the html tags and replaces them with a header line as the first line.  Then
            // will concatinate in richTextBox2 in groups of 7 so that it maybe exported as a csv file.

            // var for counting in groups of 7
            int memberNumber = 1;
            string catString = "";

            richTextBox2.Clear();

            // we will consume richTextBox1 line by line into richTextBox2 then clear richTextBox2 and move back over.
            // yes there are other ways of doing this but I'm frigg'in lazy:)

            richTextBox2.AppendText(@"Last|First|Title|Department|City|State" + "\n");  // send the header on over before bulk process

            foreach (string lineInRTB1 in richTextBox1.Lines)
            {
                // concatinate string
                catString += pullHTML(lineInRTB1.Trim());

                memberNumber += 1;

                // reset group'er
                if (memberNumber == 8)
                {
                    richTextBox2.AppendText(catString);
                    richTextBox2.AppendText(Environment.NewLine);

                    catString = "";
                    memberNumber = 1;
                }
                else
                {
                    catString += "|";
                }
            }

            // move the finished product to richTextBox1
            richTextBox1.Clear();

            foreach (string lineInRTB2 in richTextBox2.Lines)
            {
                richTextBox1.AppendText(lineInRTB2);
                richTextBox1.AppendText(Environment.NewLine);
            }

        }

        private string pullHTML(string inString)
        {
            inString = inString.Replace("<span title=", "");
            inString = inString.Replace("</span>", "");

            inString = inString.Replace("<td>", "");
            inString = inString.Replace("</td>", "");

            inString = inString.Replace("&amp;", "");

            inString = inString.Substring(inString.IndexOf(">") + 1);

            // when it is done
            return inString;
        }

        private void textBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
