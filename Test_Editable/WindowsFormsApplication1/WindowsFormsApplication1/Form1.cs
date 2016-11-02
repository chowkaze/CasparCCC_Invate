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
using System.IO;
using System.Threading;
using System.Net.Sockets;
using System.Timers;

//Interop
using System.Runtime.InteropServices;

//BMD API
using BMDSwitcherAPI;


namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {

        private delegate void UpdateGUI(object parameter);


        CasparDevice caspar_ = new CasparDevice();
        CasparCGDataCollection cgData = new CasparCGDataCollection();
        private string titlelwthst = "";
        private string desclwthst = "";
        private List<string> dukdui = new List<string>(new string[] {"Match 1","Match 2","Match 3","Match 4","Match 5"});
        ReadWriteFile rw = new ReadWriteFile();
        private List<string> Teamname = new List<string>();
        private List<string> Playername = new List<string>();
        private List<string> Playerstat = new List<string>();
        private List<string> Preset = new List<string>();
        private List<string> Castername = new List<string>();

        /*  //BMD Objects
          private IBMDSwitcherDiscovery m_switcherDiscovery;
          private IBMDSwitcher m_switcher;
          private IBMDSwitcherMixEffectBlock m_mixEffectBlock1;

          private SwitcherMonitor m_switcherMonitor;
          private MixEffectBlockMonitor m_mixEffectBlockMonitor;

          private bool m_moveSliderDownwards = false;
          private bool m_currentTransitionReachedHalfway = false;

          private List<InputMonitor> m_inputMonitors = new List<InputMonitor>();*/


        public Form1()
        {
            InitializeComponent();
            disableControls();
            caspar_.Connected += new EventHandler<NetworkEventArgs>(caspar__Connected);
            caspar_.FailedConnect += new EventHandler<NetworkEventArgs>(caspar__FailedConnected);
            caspar_.Disconnected += new EventHandler<NetworkEventArgs>(caspar__Disconnected);
            updateConnectButtonText();
            MatchCB.Enabled = false;
            rw.CreateFileOrFolder();
            Teamname = rw.read_file("Team");
            Playername = rw.read_file("Player");
            Playerstat = rw.read_file("Stat");
            Preset = rw.read_file("Preset");
            Castername = rw.read_file("Caster");
            foreach(string team in Teamname)
            {
                comboBox2.Items.Add(team);
            }
            popVidBox();



            /*
                        //Initializing BMD ATEM
                        // note: this invoke pattern ensures our callback is called in the main thread. We are making double
                        // use of lambda expressions here to achieve this.
                        // Essentially, the events will arrive at the callback class (implemented by our monitor classes)
                        // on a separate thread. We must marshell these to the main thread, and we're doing this by calling
                        // invoke on the Windows Forms object. The lambda expression is just a simplification.
                        m_switcherMonitor = new SwitcherMonitor();
                        m_switcherMonitor.SwitcherDisconnected += new SwitcherEventHandler((s, a) => this.Invoke((Action)(() => SwitcherDisconnected())));

                        m_mixEffectBlockMonitor = new MixEffectBlockMonitor();
                        m_mixEffectBlockMonitor.ProgramInputChanged += new SwitcherEventHandler((s, a) => this.Invoke((Action)(() => UpdateProgramButtonSelection())));
                        m_mixEffectBlockMonitor.PreviewInputChanged += new SwitcherEventHandler((s, a) => this.Invoke((Action)(() => UpdatePreviewButtonSelection())));
                        m_mixEffectBlockMonitor.TransitionFramesRemainingChanged += new SwitcherEventHandler((s, a) => this.Invoke((Action)(() => UpdateTransitionFramesRemaining())));
                        m_mixEffectBlockMonitor.TransitionPositionChanged += new SwitcherEventHandler((s, a) => this.Invoke((Action)(() => UpdateSliderPosition())));
                        m_mixEffectBlockMonitor.InTransitionChanged += new SwitcherEventHandler((s, a) => this.Invoke((Action)(() => OnInTransitionChanged())));

                        m_switcherDiscovery = new CBMDSwitcherDiscovery();
                        if (m_switcherDiscovery == null)
                        {
                            MessageBox.Show("Could not create Switcher Discovery Instance.\nATEM Switcher Software may not be installed.", "Error");
                            Environment.Exit(1);
                        }

                        SwitcherDisconnected();		// start with switcher disconnected*/
        }

        #region caspar connection
        
        //button handlers
       private void Connect_Button_Click_1(object sender, EventArgs e)
        {
            Connect_Button.Enabled = false;

            if (!caspar_.IsConnected)
            {
                caspar_.Settings.Hostname = this.IPhostserver.Text; // Properties.Settings.Default.Hostname;
                caspar_.Settings.Port = 5250;
                caspar_.Connect();
            }
            else
            {
                caspar_.Disconnect();
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
            Connect_Button.Enabled = true;
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
            Connect_Button.Enabled = true;
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
            Connect_Button.Enabled = true;
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
                Connect_Button.Text = "Connect";// to " + Properties.Settings.Default.Hostname;
                ConnectedStatus.Text = "Disconnected";
            }
            else
            {
                Connect_Button.Text = "Disconnect"; // from " + Properties.Settings.Default.Hostname;
                ConnectedStatus.Text = "Connected";
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
        #region Countdown
        #endregion
        private void Form1_Load(object sender, EventArgs e)
        {

        }
        #region Functional_Program



        private void ShowRunCG(String[] Types,String[] InputSource,int layer,int channel,String TempName)
        {
            try
            {
                // Clear old data
                cgData.Clear();

                // build data
                for(int i=0;i<Types.Length;i++)
                {
                    cgData.SetData(Types[i], InputSource[i]);
                }
                
            }
            catch
            {

            }
            finally
            {
                if (caspar_.IsConnected && caspar_.Channels.Count > 0)
                {
                    caspar_.Channels[channel].CG.Add(layer, TempName, true, cgData);
                    System.Diagnostics.Debug.WriteLine("Add");
                    System.Diagnostics.Debug.WriteLine(layer);
                    System.Diagnostics.Debug.WriteLine(TempName);
                    System.Diagnostics.Debug.WriteLine(cgData.ToXml());
                }
            }
        }
        private void ShowClock_Click(object sender, EventArgs e)
        {
            try
            {
                // Clear old data
                cgData.Clear();

                // build data
                cgData.SetData("title", titlelwthst);
                cgData.SetData("desc", desclwthst);
            }
            catch
            {

            }
            finally
            {
                if (caspar_.IsConnected && caspar_.Channels.Count > 0)
                {
                    caspar_.Channels[0].CG.Add(2, "Two-Tier-LowerThirds", true, cgData);
                    System.Diagnostics.Debug.WriteLine("Add");
                    System.Diagnostics.Debug.WriteLine(2);
                    System.Diagnostics.Debug.WriteLine("Two - Tier - LowerThirds");
                    System.Diagnostics.Debug.WriteLine(cgData.ToXml());
                }
            }
        }

        private void HideClock_Click(object sender, EventArgs e)
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
                    caspar_.Channels[0].CG.Stop(2);
                    System.Diagnostics.Debug.WriteLine("Stop");
                    System.Diagnostics.Debug.WriteLine(2);
                }
            }
        }


        private void SetClock_Click(object sender, EventArgs e)
        {
            titlelwthst = TitleLWTH.Text;
            desclwthst = DescLWTH.Text;

        }
        #endregion

        #region ATEMControls
       /* private void OnInputLongNameChanged(object sender, object args)
        {
            this.Invoke((Action)(() => UpdatePopupItems()));
        }

        //When switcher is connected
        private void SwitcherConnected()
        {
            bmdConnect.Enabled = false;
            groupBox4.Enabled = true;

            // Install SwitcherMonitor callbacks:
            m_switcher.AddCallback(m_switcherMonitor);

            // We create input monitors for each input. To do this we iterator over all inputs:
            // This will allow us to update the combo boxes when input names change:
            IBMDSwitcherInputIterator inputIterator;
            if (SwitcherAPIHelper.CreateIterator(m_switcher, out inputIterator))
            {
                IBMDSwitcherInput input;
                inputIterator.Next(out input);
                while (input != null)
                {
                    InputMonitor newInputMonitor = new InputMonitor(input);
                    input.AddCallback(newInputMonitor);
                    newInputMonitor.LongNameChanged += new SwitcherEventHandler(OnInputLongNameChanged);

                    m_inputMonitors.Add(newInputMonitor);

                    inputIterator.Next(out input);
                }
            }

            // We want to get the first Mix Effect block (ME 1). We create a ME iterator,
            // and then get the first one:
            m_mixEffectBlock1 = null;
            IBMDSwitcherMixEffectBlockIterator meIterator;
            SwitcherAPIHelper.CreateIterator(m_switcher, out meIterator);

            if (meIterator != null)
            {
                meIterator.Next(out m_mixEffectBlock1);
            }

            if (m_mixEffectBlock1 == null)
            {
                MessageBox.Show("Unexpected: Could not get first mix effect block", "Error");
                return;
            }

            // Install MixEffectBlockMonitor callbacks:
            m_mixEffectBlock1.AddCallback(m_mixEffectBlockMonitor);

            MixEffectBlockSetEnable(true);
            UpdatePopupItems();
            UpdateTransitionFramesRemaining();
            UpdateSliderPosition();
        }

        //When switcher is disconnected
        private void SwitcherDisconnected()
        {
            bmdConnect.Enabled = true;
            groupBox4.Enabled = false;


            MixEffectBlockSetEnable(false);

            // Remove all input monitors, remove callbacks
            foreach (InputMonitor inputMon in m_inputMonitors)
            {
                inputMon.Input.RemoveCallback(inputMon);
                inputMon.LongNameChanged -= new SwitcherEventHandler(OnInputLongNameChanged);
            }
            m_inputMonitors.Clear();

            if (m_mixEffectBlock1 != null)
            {
                // Remove callback
                m_mixEffectBlock1.RemoveCallback(m_mixEffectBlockMonitor);

                // Release reference
                m_mixEffectBlock1 = null;
            }

            if (m_switcher != null)
            {
                // Remove callback:
                m_switcher.RemoveCallback(m_switcherMonitor);

                // release reference:
                m_switcher = null;
            }
        }

        //Enable M/E
        private void MixEffectBlockSetEnable(bool enable)
        {
            //ENABLE
            buttonAuto.Enabled = enable;
            buttonCut.Enabled = enable;
            trackBarTransitionPos.Enabled = enable;
            for (int i = 1; i <= 8; i++)
            {
                panelPrev.Controls["prevBtn" + i].Enabled = false;
                panelProg.Controls["progBtn" + i].Enabled = false;
            }
        }

        //Update the buttons with the text
        private void UpdatePopupItems()
        {
            // Clear the combo boxes:


            // Get an input iterator. We use the SwitcherAPIHelper to create the iterator for us:
            IBMDSwitcherInputIterator inputIterator;
            if (!SwitcherAPIHelper.CreateIterator(m_switcher, out inputIterator))
                return;

            string[] ignore = { "Black", "Color Bars", "Color 1", "Color 2", "Media Player 1", "Media Player 1 Key", "Media Player 2", "Media Player 2 Key", "Program", "Preview", "Clean Feed 1", "Clean Feed 2" };

            IBMDSwitcherInput input;
            inputIterator.Next(out input);
            while (input != null)
            {
                string inputName;
                long inputId;

                input.GetInputId(out inputId);
                input.GetString(_BMDSwitcherInputPropertyId.bmdSwitcherInputPropertyIdLongName, out inputName);

                // Add items to list
                if (!ignore.Contains(inputName))
                {
                    panelPrev.Controls["prevBtn" + inputId].Text = inputName;
                    panelProg.Controls["progBtn" + inputId].Text = inputName;

                    groupBox5.Controls["btnLogo" + inputId].Text = inputName;

                    panelPrev.Controls["prevBtn" + inputId].Tag = inputId;
                    panelProg.Controls["progBtn" + inputId].Tag = inputId;

                    panelPrev.Controls["prevBtn" + inputId].Enabled = true;
                    panelProg.Controls["progBtn" + inputId].Enabled = true;
                }

                inputIterator.Next(out input);
            }

            UpdateProgramButtonSelection();
            UpdatePreviewButtonSelection();
        }

        //Reset all program buttons to non-active colors
        private void resetProgBtns()
        {
            for (int i = 1; i <= 8; i++)
            {
                panelProg.Controls["progBtn" + i].BackColor = System.Drawing.SystemColors.Control;
            }
        }

        //Reset all preview buttons to non-active colors
        private void resetPrevBtns()
        {
            for (int i = 1; i <= 8; i++)
            {
                panelPrev.Controls["prevBtn" + i].BackColor = System.Drawing.SystemColors.Control;
            }
        }

        //Update program buttons
        private void UpdateProgramButtonSelection()
        {
            long programId;

            m_mixEffectBlock1.GetInt(_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdProgramInput, out programId);

            resetProgBtns();

            panelProg.Controls["progBtn" + programId].BackColor = System.Drawing.Color.Red;

            if (checkLogoAuto.Checked)
            {
                changeLogo((int)programId);
            }
        }

        //Update preview buttons
        private void UpdatePreviewButtonSelection()
        {
            long previewId;

            m_mixEffectBlock1.GetInt(_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdPreviewInput, out previewId);

            resetPrevBtns();

            panelPrev.Controls["prevBtn" + previewId].BackColor = System.Drawing.Color.LawnGreen;
        }

        //Update the frames remaining text field
        private void UpdateTransitionFramesRemaining()
        {
            long framesRemaining;

            m_mixEffectBlock1.GetInt(_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdTransitionFramesRemaining, out framesRemaining);

            textBoxTransFramesRemaining.Text = String.Format("{0}", framesRemaining);
        }

        //Constantly update the slider when the auto has been used
        private void UpdateSliderPosition()
        {
            double transitionPos;

            m_mixEffectBlock1.GetFloat(_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdTransitionPosition, out transitionPos);

            m_currentTransitionReachedHalfway = (transitionPos >= 0.50);

            if (m_moveSliderDownwards)
                trackBarTransitionPos.Value = 100 - (int)(transitionPos * 100);
            else
                trackBarTransitionPos.Value = (int)(transitionPos * 100);
        }

        //When a transition has been completed
        private void OnInTransitionChanged()
        {
            int inTransition;

            m_mixEffectBlock1.GetFlag(_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdInTransition, out inTransition);

            if (inTransition == 0)
            {
                // Toggle the starting orientation of slider handle if a transition has passed through halfway
                if (m_currentTransitionReachedHalfway)
                {
                    m_moveSliderDownwards = !m_moveSliderDownwards;
                    UpdateSliderPosition();
                }
                m_currentTransitionReachedHalfway = false;
            }
        }

        //Change a preview button
        private void changePrev(object sender, EventArgs e)
        {
            long inputId = (long)Convert.ToDouble(((Button)sender).Tag);

            if (m_mixEffectBlock1 != null)
            {
                m_mixEffectBlock1.SetInt(_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdPreviewInput,
                    inputId);
            }
        }

        //Change a program button
        private void changeProg(object sender, EventArgs e)
        {
            long inputId = (long)Convert.ToDouble(((Button)sender).Tag);

            if (m_mixEffectBlock1 != null)
            {
                m_mixEffectBlock1.SetInt(_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdProgramInput,
                    inputId);
            }
        }

        //When connect is pressed on the ATEM connect
        private void bmdConnect_Click(object sender, EventArgs e)
        {
            _BMDSwitcherConnectToFailure failReason = 0;
            string address = textBoxIP.Text;

            try
            {
                // Note that ConnectTo() can take several seconds to return, both for success or failure,
                // depending upon hostname resolution and network response times, so it may be best to
                // do this in a separate thread to prevent the main GUI thread blocking.
                m_switcherDiscovery.ConnectTo(address, out m_switcher, out failReason);
            }
            catch (COMException)
            {
                // An exception will be thrown if ConnectTo fails. For more information, see failReason.
                switch (failReason)
                {
                    case _BMDSwitcherConnectToFailure.bmdSwitcherConnectToFailureNoResponse:
                        MessageBox.Show("No response from Switcher", "Error");
                        break;
                    case _BMDSwitcherConnectToFailure.bmdSwitcherConnectToFailureIncompatibleFirmware:
                        MessageBox.Show("Switcher has incompatible firmware", "Error");
                        break;
                    default:
                        MessageBox.Show("Connection failed for unknown reason", "Error");
                        break;
                }
                return;
            }

            SwitcherConnected();
        }

        //Perform auto
        private void buttonAuto_Click(object sender, EventArgs e)
        {
            if (m_mixEffectBlock1 != null)
            {
                m_mixEffectBlock1.PerformAutoTransition();
            }
        }

        //Perform cut
        private void buttonCut_Click(object sender, EventArgs e)
        {
            if (m_mixEffectBlock1 != null)
            {
                m_mixEffectBlock1.PerformCut();
            }
        }

        //Scrolling the track bar (fade)
        private void trackBarTransitionPos_Scroll(object sender, EventArgs e)
        {
            if (m_mixEffectBlock1 != null)
            {
                double position = trackBarTransitionPos.Value / 100.0;
                if (m_moveSliderDownwards)
                    position = (100 - trackBarTransitionPos.Value) / 100.0;

                m_mixEffectBlock1.SetFloat(_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdTransitionPosition,
                    position);
            }
        }

        /// <summary>
        /// Used for putting other object types into combo boxes.
        /// </summary>
        struct StringObjectPair<T>
        {
            public string name;
            public T value;

            public StringObjectPair(string name, T value)
            {
                this.name = name;
                this.value = value;
            }

            public override string ToString()
            {
                return name;
            }
        }


    */
        #endregion

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (caspar_.IsConnected)
            {
                if(e.KeyCode == Keys.F1)
                {

                }


            }
        }

        private void Deathmatchbox_CheckedChanged(object sender, EventArgs e)
        {

            if(Deathmatchbox.Checked)
            {
                MatchCB.Enabled = true;
                
                    MatchCB.Items.Add(dukdui[0]);
               
                BO3box.Enabled = false;
                BO5box.Enabled = false;


            }
            else
            {
                MatchCB.Enabled = false;
                MatchCB.Items.Clear();
                BO3box.Enabled = true;
                BO5box.Enabled = true;
            }
        }

        private void BO3box_CheckedChanged(object sender, EventArgs e)
        {
            if(BO3box.Checked)
            {
                MatchCB.Enabled = true;

                MatchCB.Items.Add(dukdui[0]);
                MatchCB.Items.Add(dukdui[1]);
                MatchCB.Items.Add(dukdui[2]);
                Deathmatchbox.Enabled = false;
                BO5box.Enabled = false;
            }
            else
            {
                MatchCB.Enabled = false;
                MatchCB.Items.Clear();
                BO5box.Enabled = true;
                Deathmatchbox.Enabled = true;
            }
        }

        private void BO5box_CheckedChanged(object sender, EventArgs e)
        {
            if (BO5box.Checked)
            {
                MatchCB.Enabled = true;

                MatchCB.Items.Add(dukdui[0]);
                MatchCB.Items.Add(dukdui[1]);
                MatchCB.Items.Add(dukdui[2]);
                MatchCB.Items.Add(dukdui[3]);
                MatchCB.Items.Add(dukdui[4]);
                Deathmatchbox.Enabled = false;
                BO3box.Enabled = false;
            }
            else
            {
                MatchCB.Enabled = false;
                MatchCB.Items.Clear();
                BO3box.Enabled = true;
                Deathmatchbox.Enabled = true;
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            List<string> TN = new List<string>();
             List<string> PN = new List<string>();
            List<string> PST = new List<string>();
            List<string> Check = new List<string>();
            bool tick = true;
            bool tickplayer = true;
            int playercount = 0;
            int[] playerinteam = { 0, 0, 0, 0, 0, 0, 0 };
            string[] playerteam = {"","","","","","",""} ;
            string[] playerstat = new string[66];

            playerstat[0] = textBox4.Text;
            playerstat[1] = textBox5.Text;
            playerstat[2] = textBox6.Text;
            playerstat[3] = textBox46.Text;
            playerstat[4] = textBox7.Text;
            playerstat[5] = textBox47.Text;
            playerstat[6] = textBox8.Text;
            playerstat[7] = textBox48.Text;
            playerstat[8] = textBox9.Text;
            playerstat[9] = textBox49.Text;
            playerstat[10] = textBox10.Text;
            playerstat[11] = textBox17.Text;
            playerstat[12] = textBox16.Text;
            playerstat[13] = textBox15.Text;
            playerstat[14] = textBox50.Text;
            playerstat[15] = textBox14.Text;
            playerstat[16] = textBox51.Text;
            playerstat[17] = textBox13.Text;
            playerstat[18] = textBox52.Text;
            playerstat[19] = textBox12.Text;
            playerstat[20] = textBox53.Text;
            playerstat[21] = textBox11.Text;
            playerstat[22] = textBox24.Text;
            playerstat[23] = textBox23.Text;
            playerstat[24] = textBox22.Text;
            playerstat[25] = textBox54.Text;
            playerstat[26] = textBox21.Text;
            playerstat[27] = textBox55.Text;
            playerstat[28] = textBox20.Text;
            playerstat[29] = textBox56.Text;
            playerstat[30] = textBox19.Text;
            playerstat[31] = textBox57.Text;
            playerstat[32] = textBox18.Text;
            playerstat[33] = textBox31.Text;
            playerstat[34] = textBox30.Text;
            playerstat[35] = textBox29.Text;
            playerstat[36] = textBox58.Text;
            playerstat[37] = textBox28.Text;
            playerstat[38] = textBox59.Text;
            playerstat[39] = textBox27.Text;
            playerstat[40] = textBox60.Text;
            playerstat[41] = textBox26.Text;
            playerstat[42] = textBox61.Text;
            playerstat[43] = textBox25.Text;
            playerstat[44] = textBox38.Text;
            playerstat[45] = textBox37.Text;
            playerstat[46] = textBox36.Text;
            playerstat[47] = textBox62.Text;
            playerstat[48] = textBox35.Text;
            playerstat[49] = textBox63.Text;
            playerstat[50] = textBox34.Text;
            playerstat[51] = textBox64.Text;
            playerstat[52] = textBox33.Text;
            playerstat[53] = textBox65.Text;
            playerstat[54] = textBox32.Text;
            playerstat[55] = textBox45.Text;
            playerstat[56] = textBox44.Text;
            playerstat[57] = textBox43.Text;
            playerstat[58] = textBox66.Text;
            playerstat[59] = textBox42.Text;
            playerstat[60] = textBox67.Text;
            playerstat[61] = textBox41.Text;
            playerstat[62] = textBox68.Text;
            playerstat[63] = textBox40.Text;
            playerstat[64] = textBox69.Text;
            playerstat[65] = textBox39.Text;


            foreach (string i in Teamname)
            {
                if(i == textBox3.Text)
                {
                    tick = false;
                }
            }

            if (textBox4.Text != "" )
            {
                playercount++;
                playerteam[0] = textBox4.Text;
            }
            if (textBox17.Text != "")
            {
                playercount++;
                playerteam[1] = textBox17.Text;
            }
            if (textBox24.Text != "")
            {
                playercount++;
                playerteam[2] = textBox4.Text;
            }
            if (textBox31.Text != "")
            {
                playercount++;
                playerteam[3] = textBox4.Text;
            }
            if (textBox38.Text != "")
            {
                playercount++;
                playerteam[4] = textBox4.Text;
            }
            if (textBox45.Text != "")
            {
                playercount++;
                playerteam[5] = textBox4.Text;
            }
            if (textBox4.Text != "")
            {
                playercount++;
                playerteam[6] = textBox4.Text;
            }
            foreach (string i in Playername)
            {
                
                if(!tick)
                {
                    if (i != textBox3.Text && tickplayer)
                    {
                        PN.Add(i);
                    }
                    else
                    {
                        if (i == textBox4.Text)
                        {
                            PN.Add(i);
                            playerinteam[0] = 1;
                        }
                        else
                        {
                            if (i == textBox17.Text)
                            {
                                PN.Add(i);
                                playerinteam[1] = 1;
                            }
                            else
                            {
                                if (i == textBox24.Text)
                                {
                                    PN.Add(i);
                                    playerinteam[2] = 1;
                                }
                                else
                                {
                                    if (i == textBox31.Text)
                                    {
                                        PN.Add(i);
                                        playerinteam[3] = 1;
                                    }
                                    else
                                    {
                                        if (i == textBox38.Text)
                                        {
                                            PN.Add(i);
                                            playerinteam[4] = 1;
                                        }
                                        else
                                        {
                                            if (i == textBox45.Text)
                                            {
                                                PN.Add(i);
                                                playerinteam[5] = 1;
                                            }
                                            else
                                            {
                                                if (i == textBox4.Text)
                                                {
                                                    PN.Add(i);
                                                    playerinteam[6] = 1;
                                                }
                                                

                                            }
                                        }
                                    }
                                }
                            }
                        }
                     

                        if (i == "End"+textBox3.Text)
                        {
                            for(int j = 0; j<7; j++)
                            {
                                if (playerinteam[j] == 0 && playerteam[j]!= "")
                                {
                                    PN.Add(playerteam[j]);
                                }
                            }
                            tickplayer = true;
                            PN.Add(i);

                        }
                    }
                }
                else
                {

                     PN.Add(i);

                }
                 
            }

            foreach (string i in Playerstat)
             {
                if (!tick)
                {
                    if(i == playerteam[0])
                    {
                      
                    }
                    if (i == playerteam[11])
                    {
                     
                    }
                    if (i == playerteam[22])
                    {
              
                    }
                    if (i == playerteam[33])
                    {
                    }
                    if (i == playerteam[44])
                    {
                      
                    }
                    if (i == playerteam[55])
                    {
                       
                    }

                }
                else
                {

                }
            }


            if (!tick)
            {
                rw.delete_file("Team");
                rw.delete_file("Player");
                rw.delete_file("Stat");
                rw.write_file(TN, "Team");
                rw.write_file(PN, "Player");
                rw.write_file(PST, "Stat");


            }
            else
            {
                TN.Add(textBox3.Text);
                PN.Add(textBox3.Text);
                PN.Add(textBox4.Text);
                PN.Add(textBox17.Text);
                PN.Add(textBox24.Text);
                PN.Add(textBox31.Text);
                PN.Add(textBox38.Text);
                PN.Add(textBox45.Text);
                PN.Add(textBox4.Text);
                PN.Add("End" + textBox3.Text);

                PST.Add(textBox4.Text);
                PST.Add(textBox5.Text);
                PST.Add(textBox6.Text);
                PST.Add(textBox46.Text);
                PST.Add(textBox7.Text);
                PST.Add(textBox47.Text);
                PST.Add(textBox8.Text);
                PST.Add(textBox48.Text);
                PST.Add(textBox9.Text);
                PST.Add(textBox49.Text);
                PST.Add(textBox10.Text);
                PST.Add("End" + textBox4.Text);

                PST.Add(textBox17.Text);
                PST.Add(textBox16.Text);
                PST.Add(textBox15.Text);
                PST.Add(textBox50.Text);
                PST.Add(textBox14.Text);
                PST.Add(textBox51.Text);
                PST.Add(textBox13.Text);
                PST.Add(textBox52.Text);
                PST.Add(textBox12.Text);
                PST.Add(textBox53.Text);
                PST.Add(textBox11.Text);
                PST.Add("End" + textBox17.Text);

                PST.Add(textBox24.Text);
                PST.Add(textBox23.Text);
                PST.Add(textBox22.Text);
                PST.Add(textBox54.Text);
                PST.Add(textBox21.Text);
                PST.Add(textBox55.Text);
                PST.Add(textBox20.Text);
                PST.Add(textBox56.Text);
                PST.Add(textBox19.Text);
                PST.Add(textBox57.Text);
                PST.Add(textBox18.Text);
                PST.Add("End" + textBox24.Text);

                PST.Add(textBox31.Text);
                PST.Add(textBox30.Text);
                PST.Add(textBox29.Text);
                PST.Add(textBox58.Text);
                PST.Add(textBox28.Text);
                PST.Add(textBox59.Text);
                PST.Add(textBox27.Text);
                PST.Add(textBox60.Text);
                PST.Add(textBox26.Text);
                PST.Add(textBox61.Text);
                PST.Add(textBox25.Text);
                PST.Add("End" + textBox31.Text);

                PST.Add(textBox38.Text);
                PST.Add(textBox37.Text);
                PST.Add(textBox36.Text);
                PST.Add(textBox62.Text);
                PST.Add(textBox35.Text);
                PST.Add(textBox63.Text);
                PST.Add(textBox34.Text);
                PST.Add(textBox64.Text);
                PST.Add(textBox33.Text);
                PST.Add(textBox63.Text);
                PST.Add(textBox32.Text);
                PST.Add("End" + textBox38.Text);

                PST.Add(textBox45.Text);
                PST.Add(textBox44.Text);
                PST.Add(textBox43.Text);
                PST.Add(textBox66.Text);
                PST.Add(textBox42.Text);
                PST.Add(textBox67.Text);
                PST.Add(textBox41.Text);
                PST.Add(textBox68.Text);
                PST.Add(textBox40.Text);
                PST.Add(textBox69.Text);
                PST.Add(textBox39.Text);
                PST.Add("End" + textBox45.Text);
                rw.write_file(TN, "Team");
                rw.write_file(PN, "Player");
                rw.write_file(PST, "Stat");


            }
           

            
            
           



            
            
        }

        private void button7_Click(object sender, EventArgs e)
        {
            string selected = this.comboBox2.GetItemText(this.comboBox2.SelectedItem);
            bool tick = false;
            bool tick2 = false;
            string[] fillbox = new string[70];
            string namecol = "";
            int j = 0;
            foreach (string i in Playername)
            {
                if(i == selected)
                {
                    tick = true;
                }
                if(i == "End"+selected)
                {
                    tick = false;
                    break;
                }
                if(tick)
                {
                    fillbox[j] = i;
                    foreach (string k in Playerstat)
                    {
                        if(k == i)
                        {
                            tick2 = true;
                        }
                        if(k == "End"+namecol)
                        {
                            tick2 = false;
                            break;
                        }
                        if(tick2)
                        {
                            fillbox[j] = i;
                            j++;
                        }
                    }
                    j++;
                }
                
            }

            textBox3.AppendText(fillbox[0]);
            textBox4.AppendText(fillbox[1]);
            textBox5.AppendText(fillbox[2]);
            textBox6.AppendText(fillbox[3]);
            textBox46.AppendText(fillbox[4]);
            textBox7.AppendText(fillbox[5]);
            textBox47.AppendText(fillbox[6]);
            textBox8.AppendText(fillbox[7]);
            textBox48.AppendText(fillbox[8]);
            textBox9.AppendText(fillbox[9]);
            textBox49.AppendText(fillbox[10]);
            textBox10.AppendText(fillbox[11]);
            textBox17.AppendText(fillbox[12]);
            textBox16.AppendText(fillbox[13]);
            textBox15.AppendText(fillbox[14]);
            textBox50.AppendText(fillbox[15]);
            textBox14.AppendText(fillbox[16]);
            textBox51.AppendText(fillbox[17]);
            textBox13.AppendText(fillbox[18]);
            textBox52.AppendText(fillbox[19]);
            textBox12.AppendText(fillbox[20]);
            textBox53.AppendText(fillbox[21]);
            textBox11.AppendText(fillbox[22]);
            textBox24.AppendText(fillbox[23]);
            textBox23.AppendText(fillbox[24]);
            textBox22.AppendText(fillbox[25]);
            textBox54.AppendText(fillbox[26]);
            textBox21.AppendText(fillbox[27]);
            textBox55.AppendText(fillbox[28]);
            textBox20.AppendText(fillbox[29]);
            textBox56.AppendText(fillbox[30]);
            textBox19.AppendText(fillbox[31]);
            textBox57.AppendText(fillbox[32]);
            textBox18.AppendText(fillbox[33]);
            textBox31.AppendText(fillbox[34]);
            textBox30.AppendText(fillbox[35]);
            textBox29.AppendText(fillbox[36]);
            textBox58.AppendText(fillbox[37]);
            textBox28.AppendText(fillbox[38]);
            textBox59.AppendText(fillbox[39]);
            textBox27.AppendText(fillbox[40]);
            textBox60.AppendText(fillbox[41]);
            textBox26.AppendText(fillbox[42]);
            textBox61.AppendText(fillbox[43]);
            textBox25.AppendText(fillbox[44]);
            textBox38.AppendText(fillbox[45]);
            textBox37.AppendText(fillbox[46]);
            textBox36.AppendText(fillbox[47]);
            textBox62.AppendText(fillbox[48]);
            textBox35.AppendText(fillbox[49]);
            textBox63.AppendText(fillbox[50]);
            textBox34.AppendText(fillbox[51]);
            textBox64.AppendText(fillbox[52]);
            textBox33.AppendText(fillbox[53]);
            textBox65.AppendText(fillbox[54]);
            textBox32.AppendText(fillbox[55]);
            textBox45.AppendText(fillbox[56]);
            textBox44.AppendText(fillbox[57]);
            textBox43.AppendText(fillbox[58]);
            textBox66.AppendText(fillbox[59]);
            textBox42.AppendText(fillbox[60]);
            textBox67.AppendText(fillbox[61]);
            textBox41.AppendText(fillbox[62]);
            textBox68.AppendText(fillbox[63]);
            textBox40.AppendText(fillbox[64]);
            textBox69.AppendText(fillbox[65]);
            textBox39.AppendText(fillbox[66]);
        }

        private void popVidBox()
        {
            int range = caspar_.Mediafiles.Count;
            for (int i = 0; i < range; i++)
            {
                MediaInfo item = caspar_.Mediafiles[i];

                string filename = item.ToString();
                string filetype = item.Type.ToString();

                if (filetype == "MOVIE")
                {
                    comboBox3.Items.Add(filename);
                    comboBox4.Items.Add(filename);
                    comboBox6.Items.Add(filename);
                    comboBox5.Items.Add(filename);
                    comboBox8.Items.Add(filename);
                    comboBox7.Items.Add(filename);
                    comboBox10.Items.Add(filename);
                    comboBox9.Items.Add(filename);
                    comboBox12.Items.Add(filename);
                    comboBox11.Items.Add(filename);
                    comboBox14.Items.Add(filename);
                    comboBox13.Items.Add(filename);

                }
                else
                {
                    imageBox.Items.Add(filename);
                }
            }
        }
    }
}
