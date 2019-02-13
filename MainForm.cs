using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows.Forms;
using AVSPEED;
using iConfRTCModel;
using iConfRTCWinForm;
using Owf.Controls;
using TheArtOfDev.HtmlRenderer.WinForms;
using WebRTCWinformTest.Properties;

namespace WebRTCWinformTest
{
    /// <summary>
    ///     iConfRTC Demo for Windows Forms
    ///     A simple demo that show how you can use the iConfRTC to quickly build
    ///     a webrtc enabled video conferencing application
    ///     The ip address of the signaling server that this demo uses is located in the app.config.
    ///     Our signaling server should only be used for demo purposes
    ///     The signaling server is open source, so you can host your own
    ///     For questions email support@avspeed.com
    /// </summary>
    public partial class FrmMain : Form
    {
        protected RTCControl iconfRTC;
        protected RTCControl iconfRTCMediaSharing;

        /// <summary>
        ///     the currParticipants List is used to store a list of participants of a meeting
        /// </summary>
        protected List<CurrentParticipants> currParticipants;

        protected Panel sharingPanel;

        protected frmSettings settings;

        protected HtmlPanel rtBox;

        protected Font chatFont;

        protected SoundPlayer snd;

        protected bool isFullScreen;

        public FrmMain()
        {
            //Always intialize the WebRTC engine first!
            RTC.Init();

            InitializeComponent();

            currParticipants = new List<CurrentParticipants>();

            snd = new SoundPlayer();

            ShowStatus("Initializing RTC..");

            iconfRTC = new RTCControl {Dock = DockStyle.Fill};

            settings = new frmSettings(iconfRTC);

            //setup the signaling
            iconfRTC.SignalingUrl = ConfigurationManager.AppSettings["SignalingUrl"];
            iconfRTC.SignalingType = SignalingTypes.Socketio;


            #region iConfRTC Events

            iconfRTC.DoubleClick += IconfRTC_DoubleClick;
            iconfRTC.RTCInitialized += IconfRTC_RTCInitialized;
            iconfRTC.IJoinedMeeting += IconfRTC_IJoinedMeeting;
            iconfRTC.UserJoinedMeeting += IconfRTC_UserJoinedMeeting;
            iconfRTC.UserLeftMeeting += IconfRTC_UserLeftMeeting;
            iconfRTC.ILeftMeeting += IconfRTC_ILeftMeeting;
            iconfRTC.NewDevices += IconfRTC_NewDevices;
            iconfRTC.MeetingMessageReceived += IconfRTC_MeetingMessageReceived;

            #endregion


            var pnlMyViewerParent = new OutlookPanelEx() {Width = 480, Height = 360, Visible = true, HeaderText = ""};

            
            pnlMyViewerParent.Controls.Add(iconfRTC);
            




            pnlLayout.Controls.Add(pnlMyViewerParent);

            //rtf text  box for chat
            rtBox = new HtmlPanel {Parent = pnlFill, Dock = DockStyle.Fill, AutoScroll = true};

            chatFont = new Font(Font, FontStyle.Bold);

            pnlLayout.Show();

            PositionJoinPanel();
        }

        private void IconfRTC_DoubleClick(object sender, EventArgs e)
        {
            //toggle full screen
            isFullScreen = !isFullScreen;
            iconfRTC.ToggleFullScreen();
        }

        /// <summary>
        ///     set the position of the join meeting panel
        /// </summary>
        private void PositionJoinPanel()
        {
            pnlJoinCenter.Left = (pnlLayoutContainer.Width - pnlJoinCenter.Width) / 2;
            pnlJoinCenter.Top = (pnlLayoutContainer.Height - pnlJoinCenter.Height) / 2;
        }

        /// <summary>
        ///     play a sound from resource
        /// </summary>
        /// <param name="resource"></param>
        private void PlaySound(UnmanagedMemoryStream resource)
        {
            snd.Stream = resource;
            snd.Play();
        }

        private void IconfRTC_NewDevices(object sender, DeviceEventArgs e)
        {
            //settings is a generic combo box

            //the NewDevices event is fired when the RTCControl has finished building
            //a list of devices. From here you can enumerate them and show them in a List etc ..
            switch (e.DeviceType)
            {
                case DeviceType.AudioIn:
                    settings.cbAudioDevices.DataSource = e.Devices;
                    settings.cbAudioDevices.DisplayMember = "label";
                    settings.cbAudioDevices.ValueMember = "Id";
                    break;
                case DeviceType.Video:
                    settings.cbVideoDevices.DataSource = e.Devices;
                    settings.cbVideoDevices.DisplayMember = "label";
                    settings.cbVideoDevices.ValueMember = "Id";
                    break;
            }
        }

        /// <summary>
        ///     processes an incoming chat message
        /// </summary>
        /// <param name="fromUser">user that the message is from</param>
        /// <param name="message">the actual message</param>
        private void ProcessChatMessage(string fromUser, string message)
        {
            if (fromUser != iconfRTC.MyUserName)
                rtBox.Text = rtBox.GetHtml() + "<div style='background-color:#C4E6F7; margin-left:5px'>" + fromUser +
                             "<br/>" + message + "</div>";
            else
                rtBox.Text = rtBox.GetHtml() + "<div style='background-color:#DCF2FA; margin-left:10px'>" + fromUser +
                             "<br/>" + message + "</div>";

            // Return focus to message text box
            txtSendMessage.Focus();
        }

        private void IconfRTC_MeetingMessageReceived(object sender, MeetingMessageEventArgs e)
        {
            ProcessChatMessage(e.FromUser, e.Message);

            PlaySound(Resources.newalert);
        }


        private void IconfRTC_ILeftMeeting(object sender, UserArgs e)
        {
            btnLeaveMeeting.Visible = false;
            pnlJoin.Enabled = true;
            pnlLayout.Hide();
            this.Text = "iConfRTC Winform Demo App";

            // Remove all viewer controls from panelLayout

            foreach (var item in currParticipants)
            {
                item.RTCControl.Dispose();
                pnlLayout.Controls.Remove(item.PanelLayout);
                item.PanelLayout.Dispose();
            }

            currParticipants.Clear();
        }

        private void IconfRTC_UserLeftMeeting(object sender, UserArgs e)
        {
            var participant = currParticipants.FirstOrDefault(x => x.Session == e.Session);

            if (participant != null)
            {
                var ctrlToRemove = participant.PanelLayout;

                //dispose of the RTC Control
                participant.RTCControl.Dispose();

                //remove the appropriate panel layout
                pnlLayout.Controls.Remove(ctrlToRemove);

                ctrlToRemove.Dispose();

                //a user has left the meeting .. remove the RTCControl that is showing its session
                currParticipants.RemoveAll(x => x.Session == e.Session);
            }
        }

        private void IconfRTC_UserJoinedMeeting(object sender, UserArgs e)
        {
            ProcessParticipants(e.Participants, e.Session, e.Sharing);
        }

        private void ProcessParticipants(List<MeetingParticipants> participants, string sessionJoined, bool isSharing)
        {
            //when a user joins a meeting we receive a list of meeting participants ( including yourself ) 
            //we go through the list , create viewers (RTCControls) for each participant and call ViewSession, 
            //passing along the participant's sessionId

            foreach (var participant in participants)
                    
            
                if (participant.Session != iconfRTC.MySession) //you are already seeing yourself :)
                {

                    if (participant.UserName == "sharing-" + iconfRTC.MyUserName)
                        return;

                    var sessionExists = currParticipants.Any(p => p.Session == participant.Session);

                    if (!sessionExists)
                    {
                        var viewer = new RTCControl
                        {
                            SignalingType = SignalingTypes.Socketio,
                            SignalingUrl = iconfRTC.SignalingUrl,
                            Dock = DockStyle.Fill
                        };

                        


                        var pnlViewerParent = new OutlookPanelEx()
                        {
                            Tag = participant.Session,
                            Width = 480,
                            Height = 360
                        };


                        pnlViewerParent.Controls.Add(viewer);

                        if (participant.Session == sessionJoined && isSharing)
                        {
                            //overlay sharing picturebox
                            var overlayPicture = new PictureBox
                            {
                                SizeMode = PictureBoxSizeMode.AutoSize,
                                Image = pbSharing.Image,
                                Dock = DockStyle.Fill,
                                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
                            };
                            overlayPicture.Show();
                            pnlViewerParent.Controls.Add(overlayPicture);
                            overlayPicture.BringToFront();
                            var workingArea = pnlViewerParent.Bounds;
                            overlayPicture.Location = new Point(workingArea.Right - Size.Width,
                                workingArea.Bottom - Size.Height);
                        }

                        pnlViewerParent.Visible = true;
                        pnlViewerParent.HeaderText = participant.UserName;
                        pnlLayout.Controls.Add(pnlViewerParent);

                        currParticipants.Add(new CurrentParticipants
                        {
                            Session = participant.Session,
                            UserName = participant.UserName,
                            PanelLayout = pnlViewerParent,
                            RTCControl = viewer
                        });


                        //only call webrtc functions when WebRTC is ready!!
                        viewer.RTCInitialized += a =>
                        {
                            //you can add additional ice servers if you'd like
                           //  viewer.AddIceServer(url: "numb.viagenie.ca", userName: "support@avspeed.com", password: "avspeedwebrtc", clearFirst: false, type: "turn");
                            //viewer.AddIceServer("stun.voiparound.com");

                            //webrtc is ready, connect to signaling
                            viewer.ViewSession(participant.UserName, participant.Session);
                        };
                    }
                }
        }

        private void ShowStatus(string status)
        {
            lblStatus.Text = status;
            lblStatus.Visible = true;
        }

        private void HideStatus()
        {
            lblStatus.Text = string.Empty;
            lblStatus.Visible = false;
        }


        private void IconfRTC_IJoinedMeeting(object sender, UserArgs e)
        {
            //I have joined a meeting
            iconfRTC.Visible = true;
            pnlJoin.Hide();
            pnlLayout.Show();
            ((OutlookPanelEx)pnlLayout.Controls[0]).HeaderText = iconfRTC.MyUserName;

            Text = @"iConfRTC Winform Demo App -" + e.UserName + @", Welcome to " + e.MeetingID;

            btnLeaveMeeting.Visible = true;

            ShowStatus("You joined " + e.MeetingID);

            //joined the meeting now lets start the video
            iconfRTC.StartVideo();

            iconfRTC.ShowDev();

            //I need to add the video of the particpants to the layout control.
            ProcessParticipants(e.Participants, e.Session, e.Sharing);
        }

        private void IconfRTC_RTCInitialized(object sender)
        {
            HideStatus();
            pnlLayout.Hide();
            pnlJoin.Show();
            pnlJoin.Enabled = true;
        }

        private void btnJoin_Click(object sender, EventArgs e)
        {
            var user = txtUser.Text;
            var meetingId = txtMeetingID.Text;

            var valid = ValidateJoinInfo();

            lblValidateMessage.Visible = !valid;

            if (!valid) return;

            pnlJoin.Enabled = false;
            iconfRTC.JoinMeeting(user, meetingId);
        }

        private bool ValidateJoinInfo()
        {
            return txtMeetingID.Text.Trim() != string.Empty && txtUser.Text.Trim() != string.Empty;
        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            PositionJoinPanel();
        }

        private void btnLeaveMeeting_Click(object sender, EventArgs e)
        {
            iconfRTC.LeaveMeeting();
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            settings.ShowDialog();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            iconfRTC.ShowDev();
        }

        private void btnChat_Click(object sender, EventArgs e)
        {
            pnlChatContainer.Visible = btnChat.Checked;
        }

        private void txtSendMessage_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\r') return;
            e.Handled = true;

            iconfRTC.SendMessageToMeeting(txtSendMessage.Text);
            txtSendMessage.Clear();
        }

        private void btnMuteVideo_CheckedChanged(object sender, EventArgs e)
        {
            iconfRTC.MuteVideo(btnMuteVideo.Checked);
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            //exit meeting first, if in one
            if (iconfRTC.MyMeetingId != string.Empty)
                iconfRTC.LeaveMeeting();
        }
    }
}