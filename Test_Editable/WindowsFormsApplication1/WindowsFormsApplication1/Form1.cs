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
        private String titlelwthst = "";
        private String desclwthst = "";

        //BMD Objects
        private IBMDSwitcherDiscovery m_switcherDiscovery;
        private IBMDSwitcher m_switcher;
        private IBMDSwitcherMixEffectBlock m_mixEffectBlock1;

        private SwitcherMonitor m_switcherMonitor;
        private MixEffectBlockMonitor m_mixEffectBlockMonitor;

        private bool m_moveSliderDownwards = false;
        private bool m_currentTransitionReachedHalfway = false;

        private List<InputMonitor> m_inputMonitors = new List<InputMonitor>();


        public Form1()
        {
            InitializeComponent();
            disableControls();
            caspar_.Connected += new EventHandler<NetworkEventArgs>(caspar__Connected);
            caspar_.FailedConnect += new EventHandler<NetworkEventArgs>(caspar__FailedConnected);
            caspar_.Disconnected += new EventHandler<NetworkEventArgs>(caspar__Disconnected);
            updateConnectButtonText();

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

            SwitcherDisconnected();		// start with switcher disconnected
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
            }
            else
            {
                Connect_Button.Text = "Disconnect"; // from " + Properties.Settings.Default.Hostname;
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
        private void OnInputLongNameChanged(object sender, object args)
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
        #endregion




    }
}
