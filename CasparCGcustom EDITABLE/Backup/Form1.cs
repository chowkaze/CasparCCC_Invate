using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Svt.Caspar;
using Svt.Network;
using System.Xml;
using System.Xml.XPath;
using FotbollsVMKlocka.Controls;
using System.IO;

namespace FotbollsVMKlocka
{
    public partial class Form1 : Form
    {

        // deletages
        private delegate void UpdateGUI(object parameter);

        //För att kommunicera med Caspar.
        CasparDevice caspar_ = new CasparDevice();
        CasparCGDataCollection cgData = new CasparCGDataCollection();
        
        // Spara-dialogruta
        private SaveDialog saveDialog = null;

        // data set for teams
        DataSet teamsDs = new DataSet();
        DataTable teamsDt = new DataTable("TEAMS");
        DataView team1Dv = new DataView();
        DataView team2Dv = new DataView();

        public Form1()
        {
            InitializeComponent();

            disableControls();

            // skapa ny spara-ruta
            this.saveDialog = new SaveDialog();

            //Fixa till datarelationer för lagen.
            team1Dv.Table = teamsDt;
            team2Dv.Table = teamsDt;
            cb1Teams.DataSource = team1Dv;
            cb2Teams.DataSource = team2Dv;

            //Fixa till datarelationer för logos.
            cbTableLogos11.DataSource = this.GetLogoDataSource();
            cbTableLogos12.DataSource = this.GetLogoDataSource();
            cbTableLogos21.DataSource = this.GetLogoDataSource();
            cbTableLogos22.DataSource = this.GetLogoDataSource();
            cbTableLogos31.DataSource = this.GetLogoDataSource();
            cbTableLogos32.DataSource = this.GetLogoDataSource();
            cbTableLogos41.DataSource = this.GetLogoDataSource();
            cbTableLogos42.DataSource = this.GetLogoDataSource();
            cbTableLogos51.DataSource = this.GetLogoDataSource();
            cbTableLogos52.DataSource = this.GetLogoDataSource();

            // initiera och fyll med data
            InitDatatable();
            FillDatatable();

            this.clearAllData();
            this.initFieldsData();

            //Hanterare för events från Caspar.
            caspar_.Connected += new EventHandler<NetworkEventArgs>(caspar__Connected);
            caspar_.FailedConnect += new EventHandler<NetworkEventArgs>(caspar__FailedConnected);
            caspar_.Disconnected += new EventHandler<NetworkEventArgs>(caspar__Disconnected);
            //caspar_.UpdatedTemplates += new EventHandler<EventArgs>(caspar__UpdatedTemplates);
            //caspar_.UpdatedMediafiles += new EventHandler<EventArgs>(caspar__UpdatedMediafiles);
            caspar_.DataRetrieved += new EventHandler<DataEventArgs>(caspar__DataRetrieved);
            caspar_.UpdatedDatafiles += new EventHandler<EventArgs>(caspar__UpdatedDataFiles);

            updateConnectButtonText();

            // Show the CheckBox and display the control as an up-down control.

            dtCountDownTime.ShowUpDown = true;
            dtCountDownTime.Format = DateTimePickerFormat.Custom;
            //dtCountDownTime.CustomFormat = "yyyy-MM-dd HH:mm";


            FillFacebookComments(20);

           
        }

        private void FillFacebookComments(int numComments)
        {

            for (int i = 0; i < numComments; i++)
            {
                this.AddFBCommentRow(i);
            }

        }

        private void AddFBCommentRow(int i)
        {
            FaceBookItem fbItem;

            fbItem = new FaceBookItem();
            fbItem.ItemIndex = i;
            fbItem.Location = new Point(6, 8 + (i * fbItem.Height));
            fbItem.SetLabel(Convert.ToString(i + 1));

            panelComments.Controls.Add(fbItem);
        }

        private object GetLogoDataSource()
        {
            object [] rowArray = new object[Properties.Settings.Default.Logos.Count];
            for (int i = 0; i < Properties.Settings.Default.Logos.Count; i++)
            {
                rowArray[i] = Properties.Settings.Default.Logos[i];
            }
            return rowArray;
        }

        private void initFieldsData()
        {
            this.tbCasparServer.Text = Properties.Settings.Default.Hostname;
            this.tbBottomFlarp.Text = Properties.Settings.Default.BottomFlarp;
            this.cbTickerScrollSpeed.SelectedIndex = 2;
        }

        #region key handling
        private void Form1_Load(object sender, System.EventArgs e)
        {
            // Set these when the form loads:
            // Have the form capture keyboard events first.
            this.KeyPreview = true;
            // Assign the event handler to the form.
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (caspar_.IsConnected)
            {
                if (e.KeyCode == Keys.F3)
                {
                    System.Diagnostics.Debug.WriteLine(e.KeyCode + ": gameTimeStartStop");
                    this.gameTimeStartStop();
                }
                else if (e.KeyCode == Keys.F4)
                {
                    System.Diagnostics.Debug.WriteLine(e.KeyCode + ": showHideClock");
                    this.showHideClock();
                }
                else if (e.KeyCode == Keys.F5)
                {
                    System.Diagnostics.Debug.WriteLine(e.KeyCode + ": bottomFlarpShowHide");
                    this.bottomFlarpShowHide();
                }
                else if (e.KeyCode == Keys.F6)
                {
                    System.Diagnostics.Debug.WriteLine(e.KeyCode + ": showOvertime");
                    this.showOvertime();
                }
                else if (e.KeyCode == Keys.F7)
                {
                    System.Diagnostics.Debug.WriteLine(e.KeyCode + ": showTickerText");
                    this.showTickerText();
                }
            }
        }
        #endregion

        #region Fill and init data tables
        private void FillDatatable()
        {
            DataRow relation;
            // Declare the array variable.
            object [] rowArray = new object[2];
            // Create 10 new rows and add to DataRowCollection.
            for(int i = 0; i <Properties.Settings.Default.Teams.Count; i++)
            {
                String[] teamDescription = Properties.Settings.Default.Teams[i].Split((new Char[] { '#', ',' }));
                if (teamDescription.Length == 2)
                {
                    rowArray[0] = teamDescription[0];
                    rowArray[1] = teamDescription[1];
                    relation = teamsDt.NewRow();
                    relation.ItemArray = rowArray;
                    teamsDt.Rows.Add(relation);
                }
            }

        }

        private void InitDatatable()
        {
            if (teamsDt.Columns.Count == 0)
            {
                teamsDt.Columns.Add("TeamNameAbbrevation");
                teamsDt.Columns[0].DataType = System.Type.GetType("System.String");
                teamsDt.Columns.Add("TeamNameFull");
                teamsDt.Columns[0].DataType = System.Type.GetType("System.String");
            }

            cb1Teams.ValueMember = "TeamNameAbbrevation";
            cb1Teams.DisplayMember = "TeamNameFull";

            cb2Teams.ValueMember = "TeamNameAbbrevation";
            cb2Teams.DisplayMember = "TeamNameFull";
        }
        #endregion


        #region caspar connection
        //button handlers
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            buttonConnect.Enabled = false;

            if (!caspar_.IsConnected)
            {
                caspar_.Settings.Hostname = this.tbCasparServer.Text; // Properties.Settings.Default.Hostname;
                caspar_.Settings.Port = 5250;
                caspar_.Connect();
            }
            else
            {
                caspar_.Disconnect();
            }
        }


        
        //caspar event - data retrieved
        void caspar__UpdatedDataFiles(object sender, EventArgs e)
        {
            if (InvokeRequired)
                BeginInvoke(new UpdateGUI(OnDataFilesUpdated), e);
            else
                OnDataRetrieved(e);
        }
        void OnDataFilesUpdated(object param)
        {
            EventArgs d = param as EventArgs;
            System.Diagnostics.Debug.WriteLine("OnDataFilesUpdated");
            System.Diagnostics.Debug.WriteLine(d.ToString());


            List<CGDataItem> dataFileItems = new List<CGDataItem>();
            foreach (string dataFile in caspar_.Datafiles)
            {
                CGDataItem cgDI = new CGDataItem(dataFile);
                dataFileItems.Add(cgDI);
            }


            cbTableOldSavings.DataSource = dataFileItems;
            gpExistingTimeTables.Enabled = true;
        }

        private bool IsTableDataFile(string dataFile)
        {
            return true;
        }


        //caspar event - data retrieved
        void caspar__DataRetrieved(object sender, DataEventArgs e)
        {
            if (InvokeRequired)
                BeginInvoke(new UpdateGUI(OnDataRetrieved), e);
            else
                OnDataRetrieved(e);
        }
        void OnDataRetrieved(object param)
        {
            DataEventArgs d = param as DataEventArgs;
            System.Diagnostics.Debug.WriteLine(d.Data);
            FindDataHandler(d.Data);
        }

        private void FindDataHandler(string data)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(data);
                
                XmlNodeList nodes = xmlDoc.SelectNodes("/templateData/componentData");  
                    
                foreach(XmlNode node in nodes)  
                {  
                    String componentDataID = (string)node.Attributes["id"].Value;

                    switch (componentDataID)
                    {
                        case "f0":
                            this.tbTableTopH1.Text = node.SelectSingleNode("data").Attributes["value"].Value;
                            break;
                        case "f1":
                            this.tbTableTopH2.Text = node.SelectSingleNode("data").Attributes["value"].Value;
                            break;
                        case "f2":
                            this.tbTableBottomH1.Text = node.SelectSingleNode("data").Attributes["value"].Value;
                            break;
                        case "f4":
                            FillTimeTable(node.SelectSingleNode("data").SelectSingleNode("timeTable"));
                            break;
                        default:
                            break;

                    }
                }  

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + data, "There was an error when loading data");
            }
        }

        private void FillTimeTable(XmlNode timeTable)
        {
            int rowCounter = 1;
            foreach (XmlNode rableRow in timeTable)
            {
                string time = rableRow.Attributes["time"].Value;
                string textH1 = rableRow.Attributes["textH1"].Value;
                string textH2 = rableRow.Attributes["textH2"].Value;
                string logo1 = rableRow.Attributes["logo1"].Value;
                string logo2 = rableRow.Attributes["logo2"].Value;

                this.SetValueOnControl(gpTimeTable, "tbTableTime" + rowCounter, time);
                this.SetValueOnControl(gpTimeTable, "tbTableH1" + rowCounter, textH1);
                this.SetValueOnControl(gpTimeTable, "tbTableH2" + rowCounter, textH2);
                this.SetValueOnControl(gpTimeTable, "cbTableLogos" + rowCounter + "1", logo1);
                this.SetValueOnControl(gpTimeTable, "cbTableLogos" + rowCounter + "2", logo2);

                rowCounter++;
            }
        }

        private void SetValueOnControl(Control container, string cName, string value)
        {
            Control[] cc;

            cc = container.Controls.Find(cName, true);
            if (cc.Length > 0)
            {
                if (cc[0] is TextBox)
                {
                    TextBox tb = cc[0] as TextBox;
                    tb.Text = value;
                }
                else if (cc[0] is ComboBox)
                {
                    ComboBox cb = cc[0] as ComboBox;
                    cb.SelectedItem = value;
                }
            }
        }

        //caspar event - connected
        void caspar__Connected(object sender, NetworkEventArgs e)
        {
            if (InvokeRequired)
                BeginInvoke(new UpdateGUI(OnCasparConnected), e);
            else
                OnCasparConnected(e);
        }
        void OnCasparConnected(object param)
        {
            buttonConnect.Enabled = true;
            updateConnectButtonText();

            caspar_.RefreshMediafiles();
            caspar_.RefreshDatalist();

            NetworkEventArgs e = (NetworkEventArgs)param;
            statusStrip1.BackColor = Color.LightGreen;
            toolStripStatusLabel1.Text = "Connected to " + caspar_.Settings.Hostname; // Properties.Settings.Default.Hostname;

            enableControls();
        }

        

        //caspar event - failed connect
        void caspar__FailedConnected(object sender, NetworkEventArgs e)
        {
            if (InvokeRequired)
                BeginInvoke(new UpdateGUI(OnCasparFailedConnect), e);
            else
                OnCasparFailedConnect(e);
        }
        void OnCasparFailedConnect(object param)
        {
            buttonConnect.Enabled = true;
            updateConnectButtonText();

            NetworkEventArgs e = (NetworkEventArgs)param;
            statusStrip1.BackColor = Color.LightCoral;
            toolStripStatusLabel1.Text = "Failed to connect to " + caspar_.Settings.Hostname; // Properties.Settings.Default.Hostname;

            disableControls();
        }

        //caspar event - disconnected
        void caspar__Disconnected(object sender, NetworkEventArgs e)
        {
            if (InvokeRequired)
                BeginInvoke(new UpdateGUI(OnCasparDisconnected), e);
            else
                OnCasparDisconnected(e);
        }
        void OnCasparDisconnected(object param)
        {
            buttonConnect.Enabled = true;
            updateConnectButtonText();

            NetworkEventArgs e = (NetworkEventArgs)param;
            statusStrip1.BackColor = Color.LightCoral;
            toolStripStatusLabel1.Text = "Disconnected from " + caspar_.Settings.Hostname; // Properties.Settings.Default.Hostname;
            
            disableControls();
        }

        // update text on button
        private void updateConnectButtonText()
        {
            if (!caspar_.IsConnected)
            {
                buttonConnect.Text = "Connect";// to " + Properties.Settings.Default.Hostname;
            }
            else
            {
                buttonConnect.Text = "Disconnect"; // from " + Properties.Settings.Default.Hostname;
            }
        }

        #endregion



        #region control enabling
        private void disableControls()
        {
            tabControl1.Enabled = false;
        }

        private void enableControls()
        {
            tabControl1.Enabled = true;
        }
        #endregion


        #region file handling
        string currentFilename_;
        string CurrentFilename
        {
            get
            {
                return currentFilename_;
            }
            set
            {
                currentFilename_ = value;
                //UpdateTitle();
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //TODO
            if (CheckSaveBefore() != DialogResult.Cancel)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Multiselect = false;
                ofd.Filter = "World Cup 2010 Caspar Controller (*.vmc)|*.vmc";
                if (ofd.ShowDialog() == DialogResult.OK)
                    if (LoadFile(ofd.FileName))
                        CurrentFilename = ofd.FileName;
            }
        }

        DialogResult CheckSaveBefore()
        {
            //if (IsDirty)
            //{
                DialogResult result = MessageBox.Show("Do you want to save first?", "Confirm", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                    sparaToolStripMenuItem_Click(this, EventArgs.Empty);

                return result;
           // }
           // else
            //    return DialogResult.No;
        }

        private bool LoadFile(string filename)
        {
            bool returnvalue = true;
            try
            {
                this.clearAllData();
                XPathDocument xpDoc = new XPathDocument(filename);
                XPathNavigator xpNav = xpDoc.CreateNavigator();
                XPathNodeIterator items = xpNav.Select("/vmklocka/ticker");
                while (items.MoveNext())
                {
                    try
                    {
                        String tickerText = (string)items.Current.Evaluate("string(@Text)");
                        String tickerName = (string)items.Current.Evaluate("string(@Name)");
                        
                        Control[] cc = Controls.Find(tickerName, true);
                        if (cc.Length > 0)
                        {
                            TextBox tbTicker = cc[0] as TextBox;
                            tbTicker.Text = tickerText;
                        }
                    }
                    catch 
                    {
                        
                    }
                }

            }
            catch (Exception ex)
            {
                returnvalue = false;
                MessageBox.Show(ex.Message, "There was an error when loading rundown");
            }

            //IsDirty = !returnvalue;
            return returnvalue;
        }

        private void clearAllData()
        {
            //lag
            this.cb1Teams.SelectedIndex = 0;
            this.cb2Teams.SelectedIndex = 0;
            
            //resultat
            this.tBScoreTeam1.Text = "0";
            this.tBScoreTeam2.Text = "0";

            //matchtid
            this.tbTimeMin.Text = "00";
            this.tbTimeSec.Text = "00";

            //halvlek
            this.numHalfNum.Value = 1;

            //extratidstext
            this.tbBottomFlarp.Text = Properties.Settings.Default.BottomFlarp;

            //tilläggstid
            this.numOvertime.Value = Convert.ToDecimal(0);

            // tickertext
            tbTicker1.Text = "";
            tbTicker2.Text = "";
            tbTicker3.Text = "";
            tbTicker4.Text = "";
            tbTicker5.Text = "";
            tbTicker6.Text = "";
            tbTicker7.Text = "";
            tbTicker8.Text = "";
            tbTicker9.Text = "";
            tbTicker10.Text = "";
        }

        private void sparaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(CurrentFilename))
                SaveFile(CurrentFilename);
            else
                sparaSomToolStripMenuItem_Click(sender, e);
        }


        private void sparaSomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "World Cup 2010 Caspar Controller (*.vmc)|*.vmc";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                if (SaveFile(sfd.FileName))
                    CurrentFilename = sfd.FileName;
            }
        }

        private bool SaveFile(string filename)
        {
            bool returnvalue = true;
            try
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                using (XmlWriter writer = XmlWriter.Create(filename, settings))
                {
                    writer.WriteStartElement("vmklocka");

                    // write ticker
                    this.WriteTickerXML(writer, tbTicker1);
                    this.WriteTickerXML(writer, tbTicker2);
                    this.WriteTickerXML(writer, tbTicker3);
                    this.WriteTickerXML(writer, tbTicker4);
                    this.WriteTickerXML(writer, tbTicker5);
                    this.WriteTickerXML(writer, tbTicker6);
                    this.WriteTickerXML(writer, tbTicker7);
                    this.WriteTickerXML(writer, tbTicker8);
                    this.WriteTickerXML(writer, tbTicker9);
                    this.WriteTickerXML(writer, tbTicker10);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "There was an error when the saving the file");
                returnvalue = false;
            }

            //IsDirty = !returnvalue;
            return returnvalue;
        }

        private void WriteTickerXML(XmlWriter writer, TextBox tbTicker1)
        {
            writer.WriteStartElement("ticker");
            writer.WriteAttributeString("Text", tbTicker1.Text);
            writer.WriteAttributeString("Name", tbTicker1.Name);
            writer.WriteEndElement();
        }


        #endregion

        private bool hasValidClockData()
        {
            return true;
        }


        private void updateTeams(object sender, EventArgs e)
        {
            /*
             Check for valid field values
             */
            if (!this.hasValidClockData())
            {
                return;
            }

            try
            {
                // Clear old data
                cgData.Clear();

                // build data
                cgData.SetData("team1Name", cb1Teams.SelectedValue.ToString());
                cgData.SetData("team2Name", cb2Teams.SelectedValue.ToString());
            }
            catch
            {

            }
            finally
            {
                if (caspar_.IsConnected && caspar_.Channels.Count > 0)
                {
                    caspar_.Channels[Properties.Settings.Default.CasparChannel].CG.Update(Properties.Settings.Default.GraphicsLayerClock, cgData);
                    System.Diagnostics.Debug.WriteLine("Update");
                    System.Diagnostics.Debug.WriteLine(Properties.Settings.Default.GraphicsLayerClock);
                    System.Diagnostics.Debug.WriteLine(cgData.ToXml());
                }
            }
        }


        private void updateResults(object sender, EventArgs e)
        {
            /*
             Check for valid field values
             */
            if (!this.hasValidClockData())
            {
                return;
            }

            try
            {
                // Clear old data
                cgData.Clear();

                // build data
                cgData.SetData("team1Score", tBScoreTeam1.Text);
                cgData.SetData("team2Score", tBScoreTeam2.Text);
            }
            catch
            {

            }
            finally
            {
                if (caspar_.IsConnected && caspar_.Channels.Count > 0)
                {
                    caspar_.Channels[Properties.Settings.Default.CasparChannel].CG.Update(Properties.Settings.Default.GraphicsLayerClock, cgData);
                    System.Diagnostics.Debug.WriteLine("Update");
                    System.Diagnostics.Debug.WriteLine(Properties.Settings.Default.GraphicsLayerClock);
                    System.Diagnostics.Debug.WriteLine(cgData.ToXml());
                }
            }
        }

        

        private void stopClock()
        {
            try
            {

            }
            catch
            {

            }
            finally
            {
                if (caspar_.IsConnected && caspar_.Channels.Count > 0)
                {
                    caspar_.Channels[Properties.Settings.Default.CasparChannel].CG.Stop(Properties.Settings.Default.GraphicsLayerClock);
                    System.Diagnostics.Debug.WriteLine("Stop");
                    System.Diagnostics.Debug.WriteLine(Properties.Settings.Default.GraphicsLayerClock);
                }
            }
        }

        private void startClock()
        {
            /*
             Check for valid field values
             */
            if (!this.hasValidClockData())
            {
                return;
            }


            try
            {
                // Clear old data
                cgData.Clear();

                // build data
                cgData.SetData("team1Name", cb1Teams.SelectedValue.ToString());
                cgData.SetData("team2Name", cb2Teams.SelectedValue.ToString());
                cgData.SetData("team1Score", tBScoreTeam1.Text);
                cgData.SetData("team2Score", tBScoreTeam2.Text);
                cgData.SetData("halfNum", Convert.ToString(numHalfNum.Value));
                cgData.SetData("gameTime", this.getGameTimeCGData());
            }
            catch
            {

            }
            finally
            {
                if (caspar_.IsConnected && caspar_.Channels.Count > 0)
                {
                    caspar_.Channels[Properties.Settings.Default.CasparChannel].CG.Add(Properties.Settings.Default.GraphicsLayerClock, Properties.Settings.Default.TemplateNameClock, true, cgData);
                    System.Diagnostics.Debug.WriteLine("Add");
                    System.Diagnostics.Debug.WriteLine(Properties.Settings.Default.GraphicsLayerClock);
                    System.Diagnostics.Debug.WriteLine(Properties.Settings.Default.TemplateNameClock);
                    System.Diagnostics.Debug.WriteLine(cgData.ToXml());
                }
            }
        }

        private void btnStartClock_Click(object sender, EventArgs e)
        {
            this.startClock();
        }

        private void btnStopGraphics_Click(object sender, EventArgs e)
        {
            this.stopClock();
        }

        #region team and result buttons
        private void btnTeam1MinusScore_Click(object sender, EventArgs e)
        {
            /// get score
            int currentScore = Convert.ToInt16(tBScoreTeam1.Text);

            /// update score
            if (currentScore > 0)
            {
                currentScore--;
                tBScoreTeam1.Text = Convert.ToString(currentScore);
            }

        }

        private void btnTeam1AddScore_Click(object sender, EventArgs e)
        {
            /// get score
            int currentScore = Convert.ToInt16(tBScoreTeam1.Text);

            /// update score
            currentScore++;
            tBScoreTeam1.Text = Convert.ToString(currentScore);

        }

        private void btnTeam2MinusScore_Click(object sender, EventArgs e)
        {
            /// get score
            int currentScore = Convert.ToInt16(tBScoreTeam2.Text);

            /// update score
            if (currentScore > 0)
            {
                currentScore--;
                tBScoreTeam2.Text = Convert.ToString(currentScore);
            }
        }

        private void btnTeam2AddScore_Click(object sender, EventArgs e)
        {
            /// get score
            int currentScore = Convert.ToInt16(tBScoreTeam2.Text);

            /// update score
            currentScore++;
            tBScoreTeam2.Text = Convert.ToString(currentScore);
        }
        #endregion


        #region game time
        

        private void tbTimeMin_Leave(object sender, EventArgs e)
        {
            //this.updateGameTime();
        }

        private void tbTimeSec_Leave(object sender, EventArgs e)
        {
            //this.updateGameTime();
        }

        private void updateGameTime()
        {
            /*
             TODO: Check for valid field values
             */


            try
            {
                // Clear old data
                cgData.Clear();

                // build data
                cgData.SetData("gameTime", this.getGameTimeCGData());
            }
            catch
            {

            }
            finally
            {
                if (caspar_.IsConnected && caspar_.Channels.Count > 0)
                {
                    caspar_.Channels[Properties.Settings.Default.CasparChannel].CG.Update(Properties.Settings.Default.GraphicsLayerClock, cgData);
                    System.Diagnostics.Debug.WriteLine("Update");
                    System.Diagnostics.Debug.WriteLine(Properties.Settings.Default.GraphicsLayerClock);
                    System.Diagnostics.Debug.WriteLine(cgData.ToXml());
                }
            }
        }

        private string getGameTimeCGData()
        {
            return tbTimeMin.Text + ":" + tbTimeSec.Text;
        }
        #endregion


        private void btnShowHideClock_Click(object sender, EventArgs e)
        {
            this.showHideClock();
        }

        private void showHideClock()
        {
            try
            {

            }
            catch
            {

            }
            finally
            {
                if (caspar_.IsConnected && caspar_.Channels.Count > 0)
                {
                    caspar_.Channels[Properties.Settings.Default.CasparChannel].CG.Invoke(Properties.Settings.Default.GraphicsLayerClock, "clockShowHide");
                    System.Diagnostics.Debug.WriteLine("Invoke");
                    System.Diagnostics.Debug.WriteLine("clockShowHide");
                }
            }
        }


        private void btnSetGameTime_Click(object sender, EventArgs e)
        {
            this.updateGameTime();
        }

        private void btnSetOvertime_Click(object sender, EventArgs e)
        {
            this.showOvertime();
        }

        private void showOvertime()
        {
            /*
             TODO: Check for valid field values
             */


            try
            {
                // Clear old data
                cgData.Clear();

                // build data
                cgData.SetData("overtime", Convert.ToString(numOvertime.Value));
            }
            catch
            {

            }
            finally
            {
                if (caspar_.IsConnected && caspar_.Channels.Count > 0)
                {
                    caspar_.Channels[Properties.Settings.Default.CasparChannel].CG.Update(Properties.Settings.Default.GraphicsLayerClock, cgData);
                    System.Diagnostics.Debug.WriteLine("Update");
                    System.Diagnostics.Debug.WriteLine(Properties.Settings.Default.GraphicsLayerClock);
                    System.Diagnostics.Debug.WriteLine(cgData.ToXml());
                    caspar_.Channels[Properties.Settings.Default.CasparChannel].CG.Invoke(Properties.Settings.Default.GraphicsLayerClock, "overtimeShowHide");
                    System.Diagnostics.Debug.WriteLine("Invoke");
                    System.Diagnostics.Debug.WriteLine("overtimeShowHide");
                }
            }
        }

        

        private void showTickerText()
        {
            // show selected ticker text.
            // data: crawlText
            // funktion: crawlShowHide

            /*
             TODO: Check for valid field values
             */


            try
            {
                // Clear old data
                cgData.Clear();

                // build data

                String tickerText = this.getTickerText();

                cgData.SetData("crawlText", tickerText);
                cgData.SetData("scrollSpeed", getTickerScrollSpeed());
                cgData.SetData("scrollMax", numTickerScrollMax.Value.ToString());

            }
            catch
            {

            }
            finally
            {
                if (caspar_.IsConnected && caspar_.Channels.Count > 0)
                {
                    caspar_.Channels[Properties.Settings.Default.CasparChannel].CG.Update(Properties.Settings.Default.GraphicsLayerClock, cgData);
                    System.Diagnostics.Debug.WriteLine("Update");
                    System.Diagnostics.Debug.WriteLine(Properties.Settings.Default.GraphicsLayerClock);
                    System.Diagnostics.Debug.WriteLine(cgData.ToXml());
                    caspar_.Channels[Properties.Settings.Default.CasparChannel].CG.Invoke(Properties.Settings.Default.GraphicsLayerClock, "crawlShowHide");
                    System.Diagnostics.Debug.WriteLine("Invoke");
                    System.Diagnostics.Debug.WriteLine("crawlShowHide");
                }
            }

        }

        private string getTickerText()
        {
            String tickerText = "";
            if (rbTicker1.Checked)
                tickerText = tbTicker1.Text;
            if (rbTicker2.Checked)
                tickerText = tbTicker2.Text;
            if (rbTicker3.Checked)
                tickerText = tbTicker3.Text;
            if (rbTicker4.Checked)
                tickerText = tbTicker4.Text;
            if (rbTicker5.Checked)
                tickerText = tbTicker5.Text;
            if (rbTicker6.Checked)
                tickerText = tbTicker6.Text;
            if (rbTicker7.Checked)
                tickerText = tbTicker7.Text;
            if (rbTicker8.Checked)
                tickerText = tbTicker8.Text;
            if (rbTicker9.Checked)
                tickerText = tbTicker9.Text;
            if (rbTicker10.Checked)
                tickerText = tbTicker10.Text;

            return tickerText;
        }

        private void tbTicker_Enter(object sender, EventArgs e)
        {
            if (sender is TextBox)
            {
                TextBox currentTextBox = sender as TextBox;
                if (currentTextBox.Name.EndsWith("r1"))
                    rbTicker1.Checked = true;
                if (currentTextBox.Name.EndsWith("r2"))
                    rbTicker2.Checked = true;
                if (currentTextBox.Name.EndsWith("r3"))
                    rbTicker3.Checked = true;
                if (currentTextBox.Name.EndsWith("r4"))
                    rbTicker4.Checked = true;
                if (currentTextBox.Name.EndsWith("r5"))
                    rbTicker5.Checked = true;
                if (currentTextBox.Name.EndsWith("r6"))
                    rbTicker6.Checked = true;
                if (currentTextBox.Name.EndsWith("r7"))
                    rbTicker7.Checked = true;
                if (currentTextBox.Name.EndsWith("r8"))
                    rbTicker8.Checked = true;
                if (currentTextBox.Name.EndsWith("r9"))
                    rbTicker9.Checked = true;
                if (currentTextBox.Name.EndsWith("r10"))
                    rbTicker10.Checked = true;
            }
            
        }

        private void btnTimeStartStop_Click(object sender, EventArgs e)
        {
            this.gameTimeStartStop();
        }

        private void gameTimeStartStop()
        {
            try
            {

            }
            catch
            {

            }
            finally
            {
                if (caspar_.IsConnected && caspar_.Channels.Count > 0)
                {
                    caspar_.Channels[Properties.Settings.Default.CasparChannel].CG.Invoke(Properties.Settings.Default.GraphicsLayerClock, "gameTimeStartStop");
                    System.Diagnostics.Debug.WriteLine("Invoke");
                    System.Diagnostics.Debug.WriteLine("gameTimeStartStop");
                }
            }
        }

        private void nyttToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CheckSaveBefore() != DialogResult.Cancel)
            {
                this.clearAllData();
                //IsDirty = false;
            }
        }

        private void avslutaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CheckSaveBefore() != DialogResult.Cancel)
            {
                Application.Exit();
            }
            
        }

        private void btnBottomFlarpShowHide_Click(object sender, EventArgs e)
        {
            this.bottomFlarpShowHide();
        }

        private void bottomFlarpShowHide()
        {
            /*
             TODO: Check for valid field values
             */

            try
            {
                // Clear old data
                cgData.Clear();

                // build data
                cgData.SetData("bottomFlarp", tbBottomFlarp.Text);
            }
            catch
            {

            }
            finally
            {
                if (caspar_.IsConnected && caspar_.Channels.Count > 0)
                {
                    caspar_.Channels[Properties.Settings.Default.CasparChannel].CG.Update(Properties.Settings.Default.GraphicsLayerClock, cgData);
                    System.Diagnostics.Debug.WriteLine("Update");
                    System.Diagnostics.Debug.WriteLine(Properties.Settings.Default.GraphicsLayerClock);
                    System.Diagnostics.Debug.WriteLine(cgData.ToXml());
                    caspar_.Channels[Properties.Settings.Default.CasparChannel].CG.Invoke(Properties.Settings.Default.GraphicsLayerClock, "bottomFlarpShowHide");
                    System.Diagnostics.Debug.WriteLine("Invoke");
                    System.Diagnostics.Debug.WriteLine("bottomFlarpShowHide");
                }
            }
        }

        private void btnSaveHalfNum_Click(object sender, EventArgs e)
        {
            this.saveHalfNum();
        }

        private void saveHalfNum()
        {
            // spara halvtid
            /*
             TODO: Check for valid field values
             */

            try
            {
                // Clear old data
                cgData.Clear();

                // build data
                cgData.SetData("halfNum", Convert.ToString(numHalfNum.Value));
            }
            catch
            {

            }
            finally
            {
                if (caspar_.IsConnected && caspar_.Channels.Count > 0)
                {
                    caspar_.Channels[Properties.Settings.Default.CasparChannel].CG.Update(Properties.Settings.Default.GraphicsLayerClock, cgData);
                    System.Diagnostics.Debug.WriteLine("Update");
                    System.Diagnostics.Debug.WriteLine(Properties.Settings.Default.GraphicsLayerClock);
                    System.Diagnostics.Debug.WriteLine(cgData.ToXml());
                }
            }
        }

        private void numHalfNum_ValueChanged(object sender, EventArgs e)
        {
            /*
            if (numHalfNum.Value == 1)
            {
                this.tbTimeMin.Text = "00";
                this.tbTimeSec.Text = "00";
            }
            else if (numHalfNum.Value == 2)
            {
                this.tbTimeMin.Text = "45";
                this.tbTimeSec.Text = "00";
            }
            else if (numHalfNum.Value == 3)
            {
                this.tbTimeMin.Text = "90";
                this.tbTimeSec.Text = "00";
            }
            else if (numHalfNum.Value == 4)
            {
                this.tbTimeMin.Text = "105";
                this.tbTimeSec.Text = "00";
            }
            */
        }

        private void btnTickerUp_Click(object sender, EventArgs e)
        {
            this.showTickerTextExternal(Properties.Settings.Default.TemplateNameTickerUp, Properties.Settings.Default.FieldNameTickerUp);
        }

        private void btnTickerDownShow_Click(object sender, EventArgs e)
        {
            this.showTickerTextExternal(Properties.Settings.Default.TemplateNameTickerNere, Properties.Settings.Default.FieldNameTickerDown);
        }


        private void showTickerTextExternal(string templateName, string fieldName)
        {
            /*
             TODO: Check for valid field values
             */


            try
            {
                // Clear old data
                cgData.Clear();

                // build data

                String tickerText = this.getTickerText();

                cgData.SetData(fieldName, tickerText);
                cgData.SetData("scrollSpeed", getTickerScrollSpeed());
                cgData.SetData("scrollMax", numTickerScrollMax.Value.ToString());

            }
            catch
            {

            }
            finally
            {
                if (caspar_.IsConnected && caspar_.Channels.Count > 0)
                {
                    caspar_.Channels[Properties.Settings.Default.CasparChannel].CG.Add(Properties.Settings.Default.GraphicsLayerExternalTicker, templateName, true, cgData);
                    System.Diagnostics.Debug.WriteLine("Add");
                    System.Diagnostics.Debug.WriteLine(Properties.Settings.Default.GraphicsLayerExternalTicker);
                    System.Diagnostics.Debug.WriteLine(templateName);
                    System.Diagnostics.Debug.WriteLine(cgData.ToXml());
                }
            }
        }

        private string getTickerScrollSpeed()
        {
            string retVal = "";

            switch (cbTickerScrollSpeed.SelectedItem.ToString().ToLower())
            {
                case "very slow":
                    retVal = "50";
                    break;
                case "slow":
                    retVal = "75";
                    break;
                case "normal":
                    retVal = "100";
                    break;
                case "high":
                    retVal = "150";
                    break;
                case "very high":
                    retVal = "200";
                    break;
                default:
                    retVal = "100";
                    break;
            }

            return retVal;
        }

        private void btnTickerUpHide_Click(object sender, EventArgs e)
        {
            this.StopTickerExternal();
        }

        private void btnTickerDownHide_Click(object sender, EventArgs e)
        {
            this.StopTickerExternal();
        }

        private void StopTickerExternal()
        {
            try
            {

            }
            catch
            {

            }
            finally
            {
                if (caspar_.IsConnected && caspar_.Channels.Count > 0)
                {
                    caspar_.Channels[Properties.Settings.Default.CasparChannel].CG.Stop(Properties.Settings.Default.GraphicsLayerExternalTicker);
                    System.Diagnostics.Debug.WriteLine("Stop");
                    System.Diagnostics.Debug.WriteLine(Properties.Settings.Default.GraphicsLayerExternalTicker);
                }
            }
        }

        private void btnShowTickerText_Click(object sender, EventArgs e)
        {
            this.showTickerText();

        }

        

        private void ShowNameExternal(string templateName)
        {
            /*
              TODO: Check for valid field values
              */


            try
            {
                // Clear old data
                cgData.Clear();

                // build data

                String nameText = this.getTickerText();

                String[] nameTextArray = nameText.Split(new Char[] { '#' });
                if (nameTextArray.Length == 2)
                {
                    cgData.SetData("f0", nameTextArray[0]);
                    cgData.SetData("f1", nameTextArray[1]);
                }
                else
                {
                    cgData.SetData("f0", nameText);
                }

            }
            catch
            {

            }
            finally
            {
                if (caspar_.IsConnected && caspar_.Channels.Count > 0)
                {
                    caspar_.Channels[Properties.Settings.Default.CasparChannel].CG.Add(Properties.Settings.Default.GraphicsLayerExternalTicker, templateName, true, cgData);
                    System.Diagnostics.Debug.WriteLine("Add");
                    System.Diagnostics.Debug.WriteLine(Properties.Settings.Default.GraphicsLayerExternalTicker);
                    System.Diagnostics.Debug.WriteLine(templateName);
                    System.Diagnostics.Debug.WriteLine(cgData.ToXml());
                }
            }
        }

        private void btnName1RowDownShow_Click(object sender, EventArgs e)
        {
            this.ShowNameExternal(Properties.Settings.Default.Name1RowDown);
        }

        private void btnName1RowDownHide_Click(object sender, EventArgs e)
        {
            this.StopNameExternal();
        }

        private void StopNameExternal()
        {
            // same layer as ticker..
            System.Diagnostics.Debug.WriteLine("Stopping name on same layer as ticker...");
            this.StopTickerExternal();
        }

        private void btnName1RowUpShow_Click(object sender, EventArgs e)
        {
            this.ShowNameExternal(Properties.Settings.Default.Name1RowUp);
        }

        private void btnName1RowUpHide_Click(object sender, EventArgs e)
        {
            // same layer as ticker..
            System.Diagnostics.Debug.WriteLine("Stopping name on same layer as ticker...");
            this.StopTickerExternal();
        }

        private void btnSaveTableToCaspar_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                if (this.saveDialog.ShowDialog() == DialogResult.OK)
                {
                    Cursor.Current = Cursors.WaitCursor;

                    string storageName = this.saveDialog.StorageName;
                    if (!string.IsNullOrEmpty(storageName))
                    {
                        if (this.caspar_.IsConnected)
                        {
                            // Clear old data
                            cgData = this.getCGDataForTimeTable();

                            System.Diagnostics.Debug.WriteLine("StoreData");
                            System.Diagnostics.Debug.WriteLine(storageName);
                            System.Diagnostics.Debug.WriteLine(cgData.ToXml());

                            this.caspar_.StoreData(storageName, cgData);

#if DEBUG
                            //File.WriteAllText(storageName + ".dat", cgData.ToAMCPEscapedXml());
#endif
                            this.caspar_.RefreshDatalist();
                        }
                        else
                        {
                            MessageBox.Show("There is no Caspar connected.", "Caspar", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private CasparCGDataCollection getCGDataForTimeTable()
        {
            CasparCGDataCollection cgCollection = new CasparCGDataCollection();
            cgCollection.SetData("f0", this.tbTableTopH1.Text);
            cgCollection.SetData("f1", this.tbTableTopH2.Text);
            cgCollection.SetData("f2", this.tbTableBottomH1.Text);

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = false;
            settings.OmitXmlDeclaration = true;
            StringBuilder sb = new StringBuilder();

            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                writer.WriteStartElement("timeTable");

                // write tableRows
                this.WriteTimeTableRowXML(writer, tbTableTime1, tbTableH11, tbTableH21, cbTableLogos11, cbTableLogos12);
                this.WriteTimeTableRowXML(writer, tbTableTime2, tbTableH12, tbTableH22, cbTableLogos21, cbTableLogos22);
                this.WriteTimeTableRowXML(writer, tbTableTime3, tbTableH13, tbTableH23, cbTableLogos31, cbTableLogos32);
                this.WriteTimeTableRowXML(writer, tbTableTime4, tbTableH14, tbTableH24, cbTableLogos41, cbTableLogos42);
                this.WriteTimeTableRowXML(writer, tbTableTime5, tbTableH15, tbTableH25, cbTableLogos51, cbTableLogos52);

                writer.WriteEndElement();
            }

            cgCollection.SetData("f4", new CGXmlData(sb.ToString()));

            return cgCollection;
        }

        private CasparCGDataCollection getCGDataForFacebookComments(double scrollSpeed, int numberOfItems)
        {
            CasparCGDataCollection cgCollection = new CasparCGDataCollection();
            cgCollection.SetData("f0", Convert.ToString(scrollSpeed)); // scrollSpeed
            cgCollection.SetData("f1", Convert.ToString(numberOfItems)); // numberOfItems

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = false;
            settings.OmitXmlDeclaration = true;
            settings.NewLineHandling = NewLineHandling.None;

            StringBuilder sb = new StringBuilder();
            FaceBookItem fbItem;

            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                writer.WriteStartElement("comments");

                // write tableRows

                foreach (Control c in panelComments.Controls) 
                {
                    if (c is FaceBookItem)
                    {
                        fbItem = c as FaceBookItem;
                        if (fbItem.IsChecked)
                        {
                            this.WriteFBCommentRowXML(writer, fbItem.FBName, fbItem.FBComment);
                        }
                    }
                }

                writer.WriteEndElement();
            }

            cgCollection.SetData("f4", new CGXmlData(sb.ToString()));

            return cgCollection;
        }

        private void WriteFBCommentRowXML(XmlWriter writer, string name, string comment)
        {
            writer.WriteStartElement("comment");
            writer.WriteAttributeString("name", name);
            writer.WriteAttributeString("comment", comment);
            writer.WriteAttributeString("image", "");
            writer.WriteEndElement();
        }


        private void WriteTimeTableRowXML(XmlWriter writer, TextBox tbTableTime, TextBox tbTableH1, TextBox tbTableH2, ComboBox cbTableLogos1, ComboBox cbTableLogos2)
        {
            writer.WriteStartElement("rableRow");
            writer.WriteAttributeString("time", tbTableTime.Text);
            writer.WriteAttributeString("textH1", tbTableH1.Text);
            writer.WriteAttributeString("textH2", tbTableH2.Text);
            writer.WriteAttributeString("logo1", cbTableLogos1.SelectedValue.ToString());
            writer.WriteAttributeString("logo2", cbTableLogos2.SelectedValue.ToString());
            writer.WriteEndElement();
        }

        private void btnCountDownShow_Click(object sender, EventArgs e)
        {
            
            /*
              TODO: Check for valid field values
              */

            try
            {
                // Clear old data
                cgData.Clear();

                // build data
                cgData.SetData("f0", dtCountDownTime.Value.Year.ToString());
                cgData.SetData("f1", dtCountDownTime.Value.Month.ToString());
                cgData.SetData("f2", dtCountDownTime.Value.Day.ToString());

                cgData.SetData("f3", dtCountDownTime.Value.Hour.ToString());
                cgData.SetData("f4", dtCountDownTime.Value.Minute.ToString());
                cgData.SetData("f5", "0");
            }
            catch
            {

            }
            finally
            {
                if (caspar_.IsConnected && caspar_.Channels.Count > 0)
                {
                    caspar_.Channels[Properties.Settings.Default.CasparChannel].CG.Add(Properties.Settings.Default.GraphicsLayerExternalTicker, Properties.Settings.Default.TemplateNameCountDown, true, cgData);
                    System.Diagnostics.Debug.WriteLine("Add");
                    System.Diagnostics.Debug.WriteLine(Properties.Settings.Default.GraphicsLayerExternalTicker);
                    System.Diagnostics.Debug.WriteLine(Properties.Settings.Default.TemplateNameCountDown);
                    System.Diagnostics.Debug.WriteLine(cgData.ToXml());
                }
            }

        }

        private void btnCountDownHide_Click(object sender, EventArgs e)
        {
            this.StopNameExternal();
        }

        private void btnFBFullShow_Click(object sender, EventArgs e)
        {
            try
            {
                
                if (this.caspar_.IsConnected)
                {
                    // Clear old data
                    cgData = this.getCGDataForFacebookComments(5, 3);

                    System.Diagnostics.Debug.WriteLine("Add");
                    System.Diagnostics.Debug.WriteLine(cgData.ToXml());
                    this.caspar_.Channels[Properties.Settings.Default.CasparChannel].CG.Add(Properties.Settings.Default.GraphicsLayerExternalTicker, Properties.Settings.Default.TemplateNameFacebookFull, true, cgData);
                }
                else
                {
                    MessageBox.Show("There is no Caspar connected.", "Caspar", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace, "Fel", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnFBSingleDownShow_Click(object sender, EventArgs e)
        {
            try
            {

                if (this.caspar_.IsConnected)
                {
                    // Clear old data
                    cgData = this.getCGDataForFacebookSingleComment();

                    System.Diagnostics.Debug.WriteLine("Add");
                    System.Diagnostics.Debug.WriteLine(cgData.ToXml());
                    this.caspar_.Channels[Properties.Settings.Default.CasparChannel].CG.Add(Properties.Settings.Default.GraphicsLayerExternalTicker, Properties.Settings.Default.TemplateNameFacebookBottom, true, cgData);
                }
                else
                {
                    MessageBox.Show("There is no Caspar connected.", "Caspar", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private CasparCGDataCollection getCGDataForFacebookSingleComment()
        {
            CasparCGDataCollection cgCollection = new CasparCGDataCollection();

            FaceBookItem fbItem = null;
            Boolean foundRow = false;

            foreach (Control c in panelComments.Controls)
            {
                if (c is FaceBookItem)
                {
                    fbItem = c as FaceBookItem;
                    if (fbItem.IsChecked)
                    {
                        foundRow = true;
                        break;
                    }
                }
            }

            if (foundRow && fbItem != null)
            {
                cgCollection.SetData("f0", fbItem.FBName); 
                cgCollection.SetData("f1", fbItem.FBComment);
            }

            return cgCollection;
        }

        private void btnFBFullHide_Click(object sender, EventArgs e)
        {
            // same layer as ticker..
            this.StopNameExternal();
        }

        private void btnFBSingleDownHide_Click(object sender, EventArgs e)
        {
            // same layer as ticker..
            this.StopNameExternal();
        }

        private void btnFetchComments_Click(object sender, EventArgs e)
        {
            //facebook.txt
            int counter = 0;
            string line;

            // Read the file and display it line by line.
            System.IO.StreamReader file = new System.IO.StreamReader(Properties.Settings.Default.FileNameFacebookComments);

            Boolean foundNewName = false;
            string lastName = "";
            string lastComment = "";

            while ((line = file.ReadLine()) != null)
            {
                if (!line.StartsWith("#"))
                {
                    if (line.StartsWith("@"))
                    {
                        // save last comment
                        if (foundNewName)
                        {
                            // save new
                            // line is current comment
                            // lastName is last name
                            Console.WriteLine(counter + ": " + lastName + ": " + lastComment);
                            this.FillFBComment(counter, lastName, lastComment);

                            // init for next
                            foundNewName = false;
                            lastComment = "";
                            lastName = "";
                            counter++;
                        }
                        
                        // set new
                        foundNewName = true;
                        lastName = line.Substring(1, line.Length - 1);
                    }
                    else
                    {
                        if (lastComment.Trim().Length > 0)
                            lastComment += "\r\n" + line;
                        else
                            lastComment = line;
                    }
                }
            }

            // save last row as well...
            if (foundNewName)
            {
                Console.WriteLine(counter + ": " + lastName + ": " + lastComment);
                this.FillFBComment(counter, lastName, lastComment);
            }

            file.Close();

        }

        private void FillFBComment(int toIndex, string name, string comment)
        {
            FaceBookItem fbItem;
            Boolean foundIndex = false;

            foreach (Control c in panelComments.Controls)
            {
                if (c is FaceBookItem)
                {
                    fbItem = c as FaceBookItem;
                    if (fbItem.ItemIndex == toIndex)
                    {
                        fbItem.FBName = name.Trim();
                        fbItem.FBComment = comment.Trim();
                        foundIndex = true;
                    }
                }
            }

            if (!foundIndex)
            {
                this.AddFBCommentRow(toIndex);
                this.FillFBComment(toIndex, name, comment);
            }

        }

        private void btnClearComments_Click(object sender, EventArgs e)
        {
            this.panelComments.Controls.Clear();
            FillFacebookComments(20);
        }

        private void btnLockTeams_Click(object sender, EventArgs e)
        {
            this.cb1Teams.Enabled = !this.cb1Teams.Enabled;
            this.cb2Teams.Enabled = !this.cb2Teams.Enabled;

            if (this.cb1Teams.Enabled)
            {
                this.btnLockTeams.Image = FotbollsVMKlocka.Properties.Resources.lock_unlock;
            }
            else
            {
                this.btnLockTeams.Image = FotbollsVMKlocka.Properties.Resources.lock_lock;
            }

        }

        private void btnTableLoad_Click(object sender, EventArgs e)
        {
            this.caspar_.RetrieveData(cbTableOldSavings.Text);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            this.caspar_.RefreshDatalist();
        }

        private void btnClearGraphics_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Clear on: " + Properties.Settings.Default.CasparChannel);
            try
            {
                this.caspar_.Channels[Properties.Settings.Default.CasparChannel].CG.Clear();
                this.caspar_.Channels[Properties.Settings.Default.CasparChannel].Clear();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }
           
        }


    }
}
