using iConfRTCWinForm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebRTCWinformTest
{
    public partial class frmSettings : Form
    {
        private RTCControl iconfRTC;
        public frmSettings( RTCControl iConfRTCControl)
        {
            InitializeComponent();
            iconfRTC = iConfRTCControl;
            cbAudioDevices.SelectionChangeCommitted += SelectedDeviceValueChanged;
            cbVideoDevices.SelectionChangeCommitted += SelectedDeviceValueChanged;
        }

        private void SelectedDeviceValueChanged(object sender, EventArgs e)
        {
            var cb = ((ComboBox)sender);
            iconfRTC.SelectDevice(cb.SelectedValue.ToString());
        }
    }
}
