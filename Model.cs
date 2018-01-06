using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebRTCWinformTest
{
    public class CurrentParticipants
    {
        /// <summary>
        /// The Session that can be used to view the user
        /// </summary>
        public string Session { get; set; }

        /// <summary>
        /// The User name of the participant
        /// </summary>
        public string UserName { get; set; }

        public Control PanelLayout { get; set; }

        public iConfRTCWinForm.RTCControl RTCControl { get; set; }
    }
}
