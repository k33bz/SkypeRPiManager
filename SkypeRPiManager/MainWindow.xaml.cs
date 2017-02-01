using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Lync.Model;
using System.ComponentModel;
using System.Net.Sockets;
using System.Diagnostics;

namespace SkypeRPiManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        RPi RPi;
        public MainWindow()
        {
            InitializeComponent();
            RPi = new RPi();
            DataContext = RPi;

            textBoxServer.Text = RPi.GetServer();
            textBoxPort.Text = RPi.GetPort().ToString();

            RPi.sendData(RPi.getStatus());

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            //RPi.SetServer(textBoxServer.Text.ToString());
            try
            {
                RPi.SetServer(textBoxServer.Text.ToString());
                RPi.setPort(Int32.Parse(textBoxPort.Text.ToString()));

                Debug.WriteLine(RPi.GetServer() + ":" + RPi.GetPort());

                RPi.sendData("testing");
            }
            catch (FormatException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            // Output: Input string was not in a correct format.
        }
    }
    public class RPi : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public Client _client;
        public string SelfSipAddress { get; private set; }
        private string message;
        private int PORT_NO = 5000;
        private string SERVER_IP = "raspberrypi";


        public RPi()
        {
            Debug.WriteLine("Entering RPi");
            _client = LyncClient.GetClient();
            _client.StateChanged += _client_StateChanged;
            SubscribetoPresenceIfSignedIn(_client.State);
        }

        void _client_StateChanged(object sender, ClientStateChangedEventArgs e)
        {
            SubscribetoPresenceIfSignedIn(e.NewState);
        }

        private void SubscribetoPresenceIfSignedIn(ClientState state)
        {
            if (state == ClientState.SignedIn)
            {
                SelfSipAddress = _client.Self.Contact.Uri;
                Debug.WriteLine(SelfSipAddress.ToString());
                _client.Self.Contact.ContactInformationChanged += Contact_ContactInformationChanged;
             }
            else
            {
                //remove event handler (i.e. from previous logins etc)
                //_client.Self.Contact.ContactInformationChanged -= Contact_ContactInformationChanged;
            }
        }

        private void Contact_ContactInformationChanged(object sender, ContactInformationChangedEventArgs e)
        {
            if (e.ChangedContactInformation.Contains(ContactInformationType.Activity) || e.ChangedContactInformation.Contains(ContactInformationType.Availability))
            {
                var activity = _client.Self.Contact.GetContactInformation(ContactInformationType.ActivityId);

                // ContactAvailability availability = (ContactAvailability)_client.Self.Contact.GetContactInformation(ContactInformationType.Availability);

                /* ContactAvailability
                 * 
                 * Member name	Description
                 * None	            A flag indicating that the contact state is to be reset to the default
                 *                  availability that is calculated by Lync and based on current user activity and calendar state.
                 * Free	            A flag indicating that the contact is available.
                 * FreeIdle	        idle states are machine state and can not be published as user state.
                 * Busy	            A flag indicating that the contact is busy and inactive.
                 * BusyIdle	        idle states are machine state and can not be published as user state.
                 * DoNotDisturb 	A flag indicating that the contact does not want to be disturbed.
                 * TemporarilyAway	A flag indicating that the contact is temporarily un-alertable.
                 * Away	            A flag indicating that the contact cannot be alerted.
                 * Offline	        A flag indicating that the contact is not available.
                 * 
                 *  Taken from: https://msdn.microsoft.com/en-us/library/microsoft.lync.model.contactinformationtype_di_3_uc_ocs14mreflyncwpf.aspx
                 * 
                 * ActivityID - This doesn't seem to be documented anywher on MSDN. Generating the hard way - cycling through options.
                 * 
                 * Available - "Free"
                 * Busy - "Busy"
                 * In a call - "on-the-phone"
                 * Presenting - "in-presentation"
                 * Do not disturb - "DoNotDisturb"
                 * Be right back - "BeRightBack"
                 * Inactive - "Inactive"
                 * Away - "Away"
                 * Off work - "off-work"
                 * In a meeting - "in-a-meeting"
                 * In a conference call - "in-a-conference"
                 */
                sendData(activity.ToString().ToLower());
                
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public string getStatus()
        {
            return _client.Self.Contact.GetContactInformation(ContactInformationType.ActivityId).ToString().ToLower();
        }

        public string sendData(string message)
        {

            if (this.message != message)
            {
                this.message = message;

                try
                {
                    TcpClient TCPclient = new TcpClient(SERVER_IP, PORT_NO);
                    NetworkStream nwStream = TCPclient.GetStream();
                    byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(message);
                    //---send the text---
                    Debug.WriteLine("Sending : " + message);
                    nwStream.Write(bytesToSend, 0, bytesToSend.Length);

                    //---read back the text---
                    byte[] bytesToRead = new byte[TCPclient.ReceiveBufferSize];
                    int bytesRead = nwStream.Read(bytesToRead, 0, TCPclient.ReceiveBufferSize);
                    message = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                    Console.WriteLine("Received : " + Encoding.ASCII.GetString(bytesToRead, 0, bytesRead));
                    Console.ReadLine();

                    nwStream.Close();
                    TCPclient.Close();
                }
                catch (System.Net.Sockets.SocketException e) { Debug.WriteLine("Exception source: {0}", e.ToString()); }
                catch (Exception e) { Debug.WriteLine("Something went wrong: {0}", e.ToString()); }
            }

            return message;
        }

        public void SetServer(string SERVER_IP)
        {
            this.SERVER_IP = SERVER_IP;
        }

        public void setPort(int PORT_NO)
        {
            this.PORT_NO = PORT_NO;
        }

        public string GetServer()
        {
            return SERVER_IP;
        }

        public int GetPort()
        {
            return PORT_NO;
        }
    }
}
