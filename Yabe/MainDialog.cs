/**************************************************************************
*                           MIT License
* 
* Copyright (C) 2014 Morten Kvistgaard <mk@pch-engineering.dk>
* Copyright (C) 2015 Frederic Chaxel <fchaxel@free.fr>
*
* Permission is hereby granted, free of charge, to any person obtaining
* a copy of this software and associated documentation files (the
* "Software"), to deal in the Software without restriction, including
* without limitation the rights to use, copy, modify, merge, publish,
* distribute, sublicense, and/or sell copies of the Software, and to
* permit persons to whom the Software is furnished to do so, subject to
* the following conditions:
*
* The above copyright notice and this permission notice shall be included
* in all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
* EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
* MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
* IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
* CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
* TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
* SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*
*********************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO.BACnet;
using System.IO;
using System.IO.BACnet.Storage;
using System.Xml.Serialization;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Media;
using System.Linq;
using System.Collections;
using System.Reflection;
using ZedGraph;

namespace Yabe
{
    public partial class YabeMainDialog : Form
    {

        private const int MIN_POLL_PERIOD = 100; //ms
        private const int MAX_POLL_PERIOD = 120000; //ms

        private Dictionary<BacnetClient, BacnetDeviceLine> m_devices = new Dictionary<BacnetClient, BacnetDeviceLine>();

        private object _selectedNode = null;
        private TreeNode _selectedDevice = null;

        public delegate bool CustomPropertyGetter(int id, out string propertyDescription);
        public List<CustomPropertyGetter> _customPropertyGetters = new List<CustomPropertyGetter>();

        List<BacnetObjectId> _structuredViewParents = null;

        public int DeviceCount 
        { 
            get {
                int count = 0;

                foreach (var entry in m_devices)
                    count = count + entry.Value.Devices.Count;

                return count; 
            } 
        }

        private Dictionary<string, ListViewItem> m_subscription_list = new Dictionary<string, ListViewItem>();
        private Dictionary<string, RollingPointPairList> m_subscription_points = new Dictionary<string, RollingPointPairList>();        
        Color[] GraphColor = {Color.Red, Color.Blue, Color.Green, Color.Violet, Color.Chocolate, Color.Orange};
        GraphPane Pane;
        private ManualResetEvent _plotterPause;
        private bool _plotterPauseFlag = true; // Change this one initial value to make the graphs start paused (false) or in play mode (true).
        private const string PLAY_BUTTON_TEXT_WHEN_RUNNING = "Pause Plotter";
        private const string PLAY_BUTTON_TEXT_WHEN_PAUSED = "Resume Plotter";
        private Random _rand = new Random();

        // Memory of all object names already discovered, first string in the Tuple is the device network address hash
        // The tuple contains two value types, so it's ok for cross session
        public Dictionary<Tuple<String, BacnetObjectId>, String> DevicesObjectsName = new Dictionary<Tuple<String, BacnetObjectId>, String>();

        public bool objectNamesChangedFlag = false;

        public Dictionary<BacnetClient, BacnetDeviceLine> DiscoveredDevices { get { return m_devices; } }

        private uint m_next_subscription_id = 0;

        private static DeviceStorage m_storage;
        private List<BacnetObjectDescription> objectsDescriptionExternal, objectsDescriptionDefault;

        YabeMainDialog yabeFrm; // Ref to itself, already affected, usefull for plugin developpmenet inside this code, before exporting it

        public class BacnetDeviceLine
        {
            public BacnetClient Line;
            public List<KeyValuePair<BacnetAddress, uint>> Devices = new List<KeyValuePair<BacnetAddress, uint>>();
            public HashSet<byte> mstp_sources_seen = new HashSet<byte>();
            public HashSet<byte> mstp_pfm_destinations_seen = new HashSet<byte>();
            public BacnetDeviceLine(BacnetClient comm)
            {
                Line = comm;
            }
        }

        private int AsynchRequestId=0;

        public YabeMainDialog()
        {
            yabeFrm = this;

            InitializeComponent();
            Trace.Listeners.Add(new MyTraceListener(this));

            if(_plotterPauseFlag)
            {
                btnPlay.Text = PLAY_BUTTON_TEXT_WHEN_RUNNING;
            }
            else
            {
                btnPlay.Text = PLAY_BUTTON_TEXT_WHEN_PAUSED;
            }

            pollRateSelector.Minimum = MIN_POLL_PERIOD;
            pollRateSelector.Maximum = MAX_POLL_PERIOD;
            pollRateSelector.Value = Math.Max(MIN_POLL_PERIOD, Math.Min(Properties.Settings.Default.Subscriptions_ReplacementPollingPeriod, MAX_POLL_PERIOD));

            pollRateSelector.Enabled = Properties.Settings.Default.UsePollingByDefault;
            CovOpn.Checked = !Properties.Settings.Default.UsePollingByDefault;
            PollOpn.Checked = Properties.Settings.Default.UsePollingByDefault;

            m_DeviceTree.ExpandAll();

            // COV Graph
            Pane = CovGraph.GraphPane;
            Pane.Title.Text = null;
            CovGraph.IsShowPointValues = true;
            // X Axis
            Pane.XAxis.Type = AxisType.Date;
            Pane.XAxis.Title.Text = null;
            Pane.XAxis.MajorGrid.IsVisible = true;
            Pane.XAxis.MajorGrid.Color = Color.Gray;
            // Y Axis
            Pane.YAxis.Title.Text = null;
            Pane.YAxis.MajorGrid.IsVisible = true;
            Pane.YAxis.MajorGrid.Color = Color.Gray;
            CovGraph.AxisChange();
            CovGraph.IsAutoScrollRange = true;

            _plotterPause = new ManualResetEvent(_plotterPauseFlag);
            CovGraph.PointValueEvent += new ZedGraphControl.PointValueHandler(CovGraph_PointValueEvent);

            //load splitter setup & SubsciptionView columns order&size
            try
            {

                if (Properties.Settings.Default.SettingsUpgradeRequired)
                {
                    Properties.Settings.Default.Upgrade();
                    Properties.Settings.Default.SettingsUpgradeRequired = false;
                    Properties.Settings.Default.Save();
                }

                if (Properties.Settings.Default.GUI_FormSize != new Size(0, 0))
                    this.Size = Properties.Settings.Default.GUI_FormSize;
                FormWindowState state = (FormWindowState)Enum.Parse(typeof(FormWindowState), Properties.Settings.Default.GUI_FormState);
                if (state != FormWindowState.Minimized)
                    this.WindowState = state;
                if (Properties.Settings.Default.GUI_SplitterButtom != -1)
                    m_SplitContainerButtom.SplitterDistance = Properties.Settings.Default.GUI_SplitterButtom;
                if (Properties.Settings.Default.GUI_SplitterMiddle != -1)
                    m_SplitContainerLeft.SplitterDistance = Properties.Settings.Default.GUI_SplitterMiddle;
                if (Properties.Settings.Default.GUI_SplitterLeft != -1)
                    splitContainer4.SplitterDistance = Properties.Settings.Default.GUI_SplitterLeft;
                if (Properties.Settings.Default.GUI_SplitterRight != -1)
                    m_SplitContainerRight.SplitterDistance = Properties.Settings.Default.GUI_SplitterRight;
                
                if(Properties.Settings.Default.Vertical_Object_Splitter_Orientation)
                {
                    splitContainer4.Orientation = Orientation.Vertical;
                }
                else
                {
                    splitContainer4.Orientation = Orientation.Horizontal;
                }

                // m_SubscriptionView Columns order & size
                if (Properties.Settings.Default.GUI_SubscriptionColumns != null)
                {
                    string[] colprops = Properties.Settings.Default.GUI_SubscriptionColumns.Split(';');

                    if (colprops.Length != m_SubscriptionView.Columns.Count * 2)
                        return;

                    for (int i = 0; i < colprops.Length / 2; i++)
                    {
                        m_SubscriptionView.Columns[i].DisplayIndex = Convert.ToInt32(colprops[i * 2]);
                        m_SubscriptionView.Columns[i].Width = Convert.ToInt32(colprops[i * 2+1]);
                    }

                    m_SubscriptionView.Refresh();
                }

            }
            catch
            {
                //ignore
            }

            int intervalMinutes = Math.Max(Math.Min(Properties.Settings.Default.Auto_Store_Period_Minutes, 480), 1);
            if (intervalMinutes != Properties.Settings.Default.Auto_Store_Period_Minutes)
                Properties.Settings.Default.Auto_Store_Period_Minutes = intervalMinutes;
            SaveObjectNamesTimer.Interval = intervalMinutes * 60000;
            
            SaveObjectNamesTimer.Enabled = true;
        }

        string CovGraph_PointValueEvent(ZedGraphControl sender, GraphPane pane, CurveItem curve, int iPt)
        {
            PointPair point= curve[iPt];

            String Name = (String)curve.Tag;
            XDate X = new XDate(point.X);
            string tooltip = Name + Environment.NewLine + X.ToString() + "    " + point.Y.ToString();
            return tooltip;
        }

        private static string ConvertToText(IList<BacnetValue> values)
        {
            if (values == null)
                return "[null]";
            else if (values.Count == 0)
                return "";
            else if (values.Count == 1)
                return values[0].Value.ToString();
            else
            {
                string ret = "{";
                foreach (BacnetValue value in values)
                    ret += value.Value.ToString() + ",";
                ret = ret.Substring(0, ret.Length - 1);
                ret += "}";
                return ret;
            }
        }

        private void ChangeTreeNodePropertyName(TreeNode tn, String Name)
        {
            // Tooltip not set is not null, strange !
            if (tn.ToolTipText=="")
                tn.ToolTipText = tn.Text;
            if (Properties.Settings.Default.DisplayIdWithName)
                tn.Text = Name + " (" + System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(tn.ToolTipText.ToLower())+")";
            else
                tn.Text = Name;
        }

        private void SetSubscriptionStatus(ListViewItem itm, string status)
        {
            if (itm.SubItems[6].Text == status) return;
            itm.SubItems[6].Text = status;
            itm.SubItems[5].Text = DateTime.Now.ToString(Properties.Settings.Default.COVTimeFormater);
        }

        private string EventTypeNiceName(BacnetEventNotificationData.BacnetEventStates state)
        {
            return state.ToString().Substring(12);
        }


        private void OnEventNotify(BacnetClient sender, BacnetAddress adr, byte invoke_id, BacnetEventNotificationData EventData, bool need_confirm)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new BacnetClient.EventNotificationCallbackHandler(OnEventNotify), new object[] { sender, adr, invoke_id, EventData, need_confirm });
                return;
            }

            string sub_key = EventData.initiatingObjectIdentifier.instance + ":" + EventData.eventObjectIdentifier.type + ":" + EventData.eventObjectIdentifier.instance;
           
            ListViewItem itm=null;
            // find the Event in the View
            foreach (ListViewItem l in m_SubscriptionView.Items)
            {
                if (l.Tag.ToString() == sub_key)
                {
                    itm = l;
                    break;
                }
            }

            if (itm == null)
            {
                itm = m_SubscriptionView.Items.Add(EventData.initiatingObjectIdentifier.instance.ToString());
                itm.Tag = sub_key;
                itm.SubItems.Add("");
                itm.SubItems.Add("DEVICE:" + EventData.initiatingObjectIdentifier.instance.ToString());
                itm.SubItems.Add(EventData.eventObjectIdentifier.type + ":" + EventData.eventObjectIdentifier.instance);   //name
                itm.SubItems.Add(EventTypeNiceName(EventData.fromState) + " to " + EventTypeNiceName(EventData.toState));
                itm.SubItems.Add(EventData.timeStamp.Time.ToString(Properties.Settings.Default.COVTimeFormater));   //time
                itm.SubItems.Add(EventData.notifyType.ToString());   //status
            }
            else
            {
                itm.SubItems[4].Text = EventTypeNiceName(EventData.fromState) + " to " + EventTypeNiceName(EventData.toState);
                itm.SubItems[5].Text = EventData.timeStamp.Time.ToString("HH:mm:ss");   //time
                itm.SubItems[6].Text = EventData.notifyType.ToString();   //status
            }

            AddLogAlarmEvent(itm);

            //send ack
            if (need_confirm)
            {
                sender.SimpleAckResponse(adr, BacnetConfirmedServices.SERVICE_CONFIRMED_EVENT_NOTIFICATION, invoke_id);
            }

        }

        private void OnCOVNotification(BacnetClient sender, BacnetAddress adr, byte invoke_id, uint subscriberProcessIdentifier, BacnetObjectId initiatingDeviceIdentifier, BacnetObjectId monitoredObjectIdentifier, uint timeRemaining, bool need_confirm, ICollection<BacnetPropertyValue> values, BacnetMaxSegments max_segments)
        {
            string sub_key = adr.ToString() + ":" + initiatingDeviceIdentifier.instance + ":" + subscriberProcessIdentifier;

            lock (m_subscription_list)
            {
                if (m_subscription_list.ContainsKey(sub_key))
                {
                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        try
                        {
                            ListViewItem itm;
                            lock (m_subscription_list)
                            {
                                itm = m_subscription_list[sub_key];
                            }
                            foreach (BacnetPropertyValue value in values)
                            {

                                switch ((BacnetPropertyIds)value.property.propertyIdentifier)
                                {
                                    case BacnetPropertyIds.PROP_PRESENT_VALUE:
                                        itm.SubItems[4].Text = ConvertToText(value.value);
                                        itm.SubItems[5].Text = DateTime.Now.ToString(Properties.Settings.Default.COVTimeFormater);
                                        if (itm.SubItems[6].Text == "Not started") itm.SubItems[6].Text = "OK";
                                        try
                                        {
                                            //  try convert from string
                                            bool Ybool;
                                            bool isBool = bool.TryParse(itm.SubItems[4].Text, out Ybool);
                                            double Y = double.NaN;
                                            if (isBool)
                                            {
                                                Y = Ybool ? 1.0 : 0.0;
                                            }
                                            else
                                            {
                                                Y = Convert.ToDouble(itm.SubItems[4].Text);
                                            }
                                            XDate X = new XDate(DateTime.Now);
                                            //if (!String.IsNullOrWhiteSpace(itm.SubItems[9].Text) && bool.Parse(itm.SubItems[9].Text))
                                            //{
                                            Pane.Title.Text = "";

                                            if ((Properties.Settings.Default.GraphLineStep) && (m_subscription_points[sub_key].Count != 0))
                                            {
                                                PointPair p = m_subscription_points[sub_key].Peek();
                                                m_subscription_points[sub_key].Add(X, p.Y);
                                            }
                                            m_subscription_points[sub_key].Add(X, Y);
                                            CovGraph.AxisChange();
                                            CovGraph.Invalidate();
                                            //}
                                        }
                                        catch { }
                                        break;
                                    case BacnetPropertyIds.PROP_STATUS_FLAGS:
                                        if (value.value != null && value.value.Count > 0)
                                        {
                                            BacnetStatusFlags status = (BacnetStatusFlags)((BacnetBitString)value.value[0].Value).ConvertToInt();
                                            string status_text = "";
                                            if ((status & BacnetStatusFlags.STATUS_FLAG_FAULT) == BacnetStatusFlags.STATUS_FLAG_FAULT)
                                                status_text += "FAULT,";
                                            else if ((status & BacnetStatusFlags.STATUS_FLAG_IN_ALARM) == BacnetStatusFlags.STATUS_FLAG_IN_ALARM)
                                                status_text += "ALARM,";
                                            else if ((status & BacnetStatusFlags.STATUS_FLAG_OUT_OF_SERVICE) == BacnetStatusFlags.STATUS_FLAG_OUT_OF_SERVICE)
                                                status_text += "OOS,";
                                            else if ((status & BacnetStatusFlags.STATUS_FLAG_OVERRIDDEN) == BacnetStatusFlags.STATUS_FLAG_OVERRIDDEN)
                                                status_text += "OR,";
                                            if (status_text != "")
                                            {
                                                status_text = status_text.Substring(0, status_text.Length - 1);
                                                itm.SubItems[6].Text = status_text;
                                            }
                                            else
                                                itm.SubItems[6].Text = "OK";
                                        }

                                        break;
                                    default:
                                        //got something else? ignore it
                                        break;
                                }
                            }

                            AddLogAlarmEvent(itm);
                        }
                        catch (Exception ex)
                        {
                            Trace.TraceError("Exception in subcribed value: " + ex.Message);
                        }
                    });
                }
            }
            //send ack
            if (need_confirm)
            {
                sender.SimpleAckResponse(adr, BacnetConfirmedServices.SERVICE_CONFIRMED_COV_NOTIFICATION, invoke_id);
            }
        }

        #region " Trace Listner "
        private class MyTraceListener : TraceListener
        {
            private YabeMainDialog m_form;

            public MyTraceListener(YabeMainDialog form)
                : base("MyListener")
            {
                m_form = form;
            }

            public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
            {
                if ((this.Filter != null) && !this.Filter.ShouldTrace(eventCache, source, eventType, id, message, null, null, null)) return;

                ConsoleColor color;
                switch (eventType)
                {
                    case TraceEventType.Error:
                        color = ConsoleColor.Red;
                        break;
                    case TraceEventType.Warning:
                        color = ConsoleColor.Yellow;
                        break;
                    case TraceEventType.Information:
                        color = ConsoleColor.DarkGreen;
                        break;
                    default:
                        color = ConsoleColor.Gray;
                        break;
                }

                WriteColor(message + Environment.NewLine, color);
            }

            public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
            {
                if ((this.Filter != null) && !this.Filter.ShouldTrace(eventCache, source, eventType, id, format, args, null, null)) return;

                ConsoleColor color;
                switch (eventType)
                {
                    case TraceEventType.Error:
                        color = ConsoleColor.Red;
                        break;
                    case TraceEventType.Warning:
                        color = ConsoleColor.Yellow;
                        break;
                    case TraceEventType.Information:
                        color = ConsoleColor.DarkGreen;
                        break;
                    default:
                        color = ConsoleColor.Gray;
                        break;
                }

                WriteColor(string.Format(format, args) + Environment.NewLine, color);
            }

            public override void Write(string message)
            {
                WriteColor(message, ConsoleColor.Gray);
            }
            public override void WriteLine(string message)
            {
                WriteColor(message + Environment.NewLine, ConsoleColor.Gray);
            }

            private void WriteColor(string message, ConsoleColor color)
            {
                if (!m_form.IsHandleCreated) return;

                m_form.m_LogText.BeginInvoke((MethodInvoker)delegate { m_form.m_LogText.AppendText(message); });
            }
        }
        #endregion

        private void MainDialog_Load(object sender, EventArgs e)
        {
            //start renew timer at half lifetime
            int lifetime = (int)Properties.Settings.Default.Subscriptions_Lifetime;
            if (lifetime > 0)
            {
                m_subscriptionRenewTimer.Interval = (lifetime / 2) * 1000;
                m_subscriptionRenewTimer.Enabled = true;
            }

            //display nice floats in propertygrid
            Utilities.CustomSingleConverter.DontDisplayExactFloats = true;

            // Plugins
            m_DeviceTree.TreeViewNodeSorter = new NodeSorter();

            string[] listPlugins = Properties.Settings.Default.Plugins.Split(new char[] { ',', ';' });

            foreach (string pluginname in listPlugins)
            {
                try
                {
                    // string path = Path.GetDirectoryName(Application.ExecutablePath);
                    string name = pluginname.Replace(" ", String.Empty);
                    // Assembly myDll = Assembly.LoadFrom(path + "\\" + name + ".dll");
                    Assembly myDll = Assembly.LoadFrom(name + ".dll");
                    Trace.WriteLine(String.Format("Loaded plugin \"{0}\".", pluginname));
                    Type[] types = myDll.GetExportedTypes();
                    IYabePlugin plugin = (IYabePlugin)myDll.CreateInstance(name + ".Plugin", true);
                    plugin.Init(this);
                }
                catch(Exception ex)
                {
                    Trace.WriteLine(String.Format("Error loading plugin \"{0}\". {1}",pluginname,ex.Message));
                }
            }

            if (pluginsToolStripMenuItem.DropDownItems.Count == 0) pluginsToolStripMenuItem.Visible = false;


            // Object Names
            if (Properties.Settings.Default.Auto_Store_Object_Names)
            {
                string fileTotal = Properties.Settings.Default.Auto_Store_Object_Names_File;
                if (!string.IsNullOrWhiteSpace(fileTotal))
                {
                    try
                    {
                        string file = Path.GetFileName(fileTotal);
                        string directory = Path.GetDirectoryName(fileTotal);
                        if (string.IsNullOrWhiteSpace(file))
                        {
                            file = "Auto_Stored_Object_Names.YabeMap";
                            fileTotal = Path.Combine(directory, file);
                            Properties.Settings.Default.Auto_Store_Object_Names_File = fileTotal;
                        }

                        if (File.Exists(fileTotal))
                        {
                            // Try to open the current (if exist) object Id<-> object name mapping file
                            Stream stream = File.Open(fileTotal, FileMode.Open);
                            BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                            var d = (Dictionary<Tuple<String, BacnetObjectId>, String>)bf.Deserialize(stream);
                            stream.Close();

                            if (d != null)
                            {
                                DevicesObjectsName = d;
                                objectNamesChangedFlag = false;
                                Trace.TraceInformation("Loaded object names from \""+ fileTotal + "\".");
                            }
                        }
                        else
                        {
                            if (!Directory.Exists(directory))
                            {
                                try
                                {
                                    Directory.CreateDirectory(directory);
                                    Trace.TraceInformation("Created directory \"" + directory + "\".");
                                }
                                catch(UnauthorizedAccessException)
                                {
                                    Trace.TraceError("Error trying to setup the auto-save object names function: The directory \"" + directory + "\" does not exist, and Yabe does not have permissions to automatically create this directory. Try changing the Auto_StoreObject_Names_File setting to a different path.");                                    Properties.Settings.Default.Auto_Store_Object_Names = false;
                                }
                            }
                            //Trace.TraceError("Error trying to auto-load object names from file: The file \"" + file + "\" does not exist. Try resetting the Auto_StoreObject_Names_File setting to a valid file path, or disable auto-store.");
                        }

                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("Exception trying to setup the auto-save object names function: " + ex.Message + ". Try resetting the Auto_StoreObject_Names_File setting to a valid file path.");
                        Properties.Settings.Default.Auto_Store_Object_Names = false;
                    }
                }
                else
                {
                    Properties.Settings.Default.Auto_Store_Object_Names = false;
                }
            }
        }

        private TreeNode FindCommTreeNode(BacnetClient comm)
        {
            foreach (TreeNode node in m_DeviceTree.Nodes[0].Nodes)
            {
                BacnetClient c = node.Tag as BacnetClient;
                if(c != null && c.Equals(comm)) return node;
            }
            return null;
        }

        private TreeNode FindCommTreeNode(IBacnetTransport transport)
        {
            foreach (TreeNode node in m_DeviceTree.Nodes[0].Nodes)
            {
                BacnetClient c = node.Tag as BacnetClient;
                if (c != null && c.Transport.Equals(transport)) return node;
            }
            return null;
        }

        // Only the see Yabe on the net
        void OnWhoIs(BacnetClient sender, BacnetAddress adr, int low_limit, int high_limit)
        {
            uint myId =(uint) Properties.Settings.Default.YabeDeviceId;

            if (low_limit != -1 && myId < low_limit) return;
            else if (high_limit != -1 && myId > high_limit) return;
            sender.Iam(myId, BacnetSegmentations.SEGMENTATION_BOTH, 61440);
        }

        void OnWhoIsIgnore(BacnetClient sender, BacnetAddress adr, int low_limit, int high_limit)
        {
            //ignore whois responses from other devices (or loopbacks)
        }

        private void OnReadPropertyRequest(BacnetClient sender, BacnetAddress adr, byte invoke_id, BacnetObjectId object_id, BacnetPropertyReference property, BacnetMaxSegments max_segments)
        {
            lock (m_storage)
            {
                try
                {
                    IList<BacnetValue> value;
                    DeviceStorage.ErrorCodes code = m_storage.ReadProperty(object_id, (BacnetPropertyIds)property.propertyIdentifier, property.propertyArrayIndex, out value);
                    if (code == DeviceStorage.ErrorCodes.Good)
                        sender.ReadPropertyResponse(adr, invoke_id, sender.GetSegmentBuffer(max_segments), object_id, property, value);
                    else
                        sender.ErrorResponse(adr, BacnetConfirmedServices.SERVICE_CONFIRMED_READ_PROPERTY, invoke_id, BacnetErrorClasses.ERROR_CLASS_DEVICE, BacnetErrorCodes.ERROR_CODE_OTHER);
                }
                catch (Exception)
                {
                    sender.ErrorResponse(adr, BacnetConfirmedServices.SERVICE_CONFIRMED_READ_PROPERTY, invoke_id, BacnetErrorClasses.ERROR_CLASS_DEVICE, BacnetErrorCodes.ERROR_CODE_OTHER);
                }
            }
        }
        private static void OnReadPropertyMultipleRequest(BacnetClient sender, BacnetAddress adr, byte invoke_id, IList<BacnetReadAccessSpecification> properties, BacnetMaxSegments max_segments)
        {
            lock (m_storage)
            {
                try
                {
                    IList<BacnetPropertyValue> value;
                    List<BacnetReadAccessResult> values = new List<BacnetReadAccessResult>();
                    foreach (BacnetReadAccessSpecification p in properties)
                    {
                        if (p.propertyReferences.Count == 1 && p.propertyReferences[0].propertyIdentifier == (uint)BacnetPropertyIds.PROP_ALL)
                        {
                            if (!m_storage.ReadPropertyAll(p.objectIdentifier, out value))
                            {
                                sender.ErrorResponse(adr, BacnetConfirmedServices.SERVICE_CONFIRMED_READ_PROP_MULTIPLE, invoke_id, BacnetErrorClasses.ERROR_CLASS_OBJECT, BacnetErrorCodes.ERROR_CODE_UNKNOWN_OBJECT);
                                return;
                            }
                        }
                        else
                            m_storage.ReadPropertyMultiple(p.objectIdentifier, p.propertyReferences, out value);
                        values.Add(new BacnetReadAccessResult(p.objectIdentifier, value));
                    }

                    sender.ReadPropertyMultipleResponse(adr, invoke_id, sender.GetSegmentBuffer(max_segments), values);

                }
                catch (Exception)
                {
                    sender.ErrorResponse(adr, BacnetConfirmedServices.SERVICE_CONFIRMED_READ_PROP_MULTIPLE, invoke_id, BacnetErrorClasses.ERROR_CLASS_DEVICE, BacnetErrorCodes.ERROR_CODE_OTHER);
                }
            }
        }

        void OnIam(BacnetClient sender, BacnetAddress adr, uint device_id, uint max_apdu, BacnetSegmentations segmentation, ushort vendor_id)
        {
            DoReceiveIamImplementation(sender, adr, device_id);
        }

        private void DoReceiveIamImplementation(BacnetClient sender, BacnetAddress adr, uint device_id)
        {
            KeyValuePair<BacnetAddress, uint> new_entry = new KeyValuePair<BacnetAddress, uint>(adr, device_id);
            lock (m_devices)
            {
                if (!m_devices.ContainsKey(sender)) return;
                if (!m_devices[sender].Devices.Contains(new_entry))
                    m_devices[sender].Devices.Add(new_entry);
                else
                    return;
            }

            //update GUI
            this.BeginInvoke((MethodInvoker)delegate
            {
                TreeNode parent = FindCommTreeNode(sender);
                if (parent == null) return;

                bool Prop_Object_NameOK = false;
                String Identifier = null;

                lock (DevicesObjectsName)
                    Prop_Object_NameOK = DevicesObjectsName.TryGetValue(new Tuple<String, BacnetObjectId>(adr.FullHashString(), new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE, device_id)), out Identifier);

                //update existing (this can happen in MSTP)
                foreach (TreeNode s in parent.Nodes)
                {
                    KeyValuePair<BacnetAddress, uint>? entry = s.Tag as KeyValuePair<BacnetAddress, uint>?;
                    if (entry != null && entry.Value.Key.Equals(adr))
                    {
                        s.Text = "Device " + new_entry.Value + " - " + new_entry.Key.ToString(s.Parent.Parent != null);
                        s.Tag = new_entry;
                        if (Prop_Object_NameOK)
                        {
                            s.ToolTipText = s.Text;
                            s.Text = Identifier + " [" + device_id.ToString() + "] ";
                        }
                        else
                        {
                            s.ToolTipText = "";
                        }

                        return;
                    }
                }
                // Try to add it under a router if any 
                foreach (TreeNode s in parent.Nodes)
                {
                    KeyValuePair<BacnetAddress, uint>? entry = s.Tag as KeyValuePair<BacnetAddress, uint>?;
                    if (entry != null && entry.Value.Key.IsMyRouter(adr))
                    {
                        TreeNode node = new TreeNode("Device " + new_entry.Value + " - " + new_entry.Key.ToString(true));
                        node.ImageIndex = 2;
                        node.SelectedImageIndex = node.ImageIndex;
                        node.Tag = new_entry;
                        if (Prop_Object_NameOK)
                        {
                            node.ToolTipText = node.Text;
                            node.Text = Identifier + " [" + device_id.ToString() + "] ";
                        }
                        else
                        {
                            node.ToolTipText = "";
                        }
                        s.Nodes.Add(node);
                        m_DeviceTree.ExpandAll();
                        return;
                    }
                }

                //add simply
                TreeNode basicnode = new TreeNode("Device " + new_entry.Value + " - " + new_entry.Key.ToString(false));
                basicnode.ImageIndex = 2;
                basicnode.SelectedImageIndex = basicnode.ImageIndex;
                basicnode.Tag = new_entry;
                if (Prop_Object_NameOK)
                {
                    basicnode.ToolTipText = basicnode.Text;
                    basicnode.Text = Identifier + " [" + device_id.ToString() + "] ";
                }
                else
                {
                    basicnode.ToolTipText = "";
                }
                parent.Nodes.Add(basicnode);
                m_DeviceTree.ExpandAll();
            });
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string product;
                
            Assembly currentAssem = this.GetType().Assembly;
            object[] attribs = currentAssem.GetCustomAttributes(typeof(AssemblyProductAttribute), true);
            if (attribs.Length > 0)
            {
                product = ((AssemblyProductAttribute)attribs[0]).Product;
            }
            else
            {
                product = this.GetType().Assembly.GetName().Name;
            }

            MessageBox.Show(this, product + "\nVersion " + this.GetType().Assembly.GetName().Version + "\nBy Morten Kvistgaard - Copyright 2014-2017\nBy Frederic Chaxel - Copyright 2015-2022\n" +
                "\nReferences:"+
                "\nhttp://bacnet.sourceforge.net/" + 
                "\nhttp://www.unified-automation.com/products/development-tools/uaexpert.html" +
                "\nhttp://www.famfamfam.com/"+
                "\nhttp://sourceforge.net/projects/zedgraph/"+
                "\nhttp://www.codeproject.com/Articles/38699/A-Professional-Calendar-Agenda-View-That-You-Will"+
                "\nhttps://github.com/chmorgan/sharppcap"+
                "\nhttps://sourceforge.net/projects/mstreeview"
                
                , "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void addDevicesearchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            labelDrop1.Visible = labelDrop2.Visible = false;

            SearchDialog dlg = new SearchDialog();
            if (dlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                BacnetClient comm = dlg.Result;
                try
                {
                    m_devices.Add(comm, new BacnetDeviceLine(comm));
                }
                catch { return ; }

                //add to tree
                TreeNode node = m_DeviceTree.Nodes[0].Nodes.Add(comm.ToString());
                node.Tag = comm;
                switch (comm.Transport.Type)
                {
                    case BacnetAddressTypes.IP:
                        node.ImageIndex = 3;
                        break;
                    case BacnetAddressTypes.MSTP:
                        node.ImageIndex = 1;
                        break;
                    default:
                        node.ImageIndex = 8;
                        break;
                }
                node.SelectedImageIndex = node.ImageIndex;
                m_DeviceTree.ExpandAll(); m_DeviceTree.SelectedNode = node;

                try
                {
                    //start BACnet
                    comm.ProposedWindowSize = Properties.Settings.Default.Segments_ProposedWindowSize;
                    comm.Retries = (int)Properties.Settings.Default.DefaultRetries;
                    comm.Timeout = (int)Properties.Settings.Default.DefaultTimeout;
                    comm.MaxSegments = BacnetClient.GetSegmentsCount(Properties.Settings.Default.Segments_Max);
                    if (Properties.Settings.Default.YabeDeviceId >= 0) // If Yabe get a Device id
                    {
                        if (m_storage == null)
                        {
                            // Load descriptor from the embedded xml resource
                            m_storage = m_storage = DeviceStorage.Load("Yabe.YabeDeviceDescriptor.xml", (uint)Properties.Settings.Default.YabeDeviceId);
                            // A fast way to change the PROP_OBJECT_LIST
                            Property Prop = Array.Find<Property>(m_storage.Objects[0].Properties, p => p.Id == BacnetPropertyIds.PROP_OBJECT_LIST);
                            Prop.Value[0] = "OBJECT_DEVICE:" + Properties.Settings.Default.YabeDeviceId.ToString();
                            // change PROP_FIRMWARE_REVISION
                            Prop = Array.Find<Property>(m_storage.Objects[0].Properties, p => p.Id == BacnetPropertyIds.PROP_FIRMWARE_REVISION);
                            Prop.Value[0] = this.GetType().Assembly.GetName().Version.ToString();
                            // change PROP_APPLICATION_SOFTWARE_VERSION
                            Prop = Array.Find<Property>(m_storage.Objects[0].Properties, p => p.Id == BacnetPropertyIds.PROP_APPLICATION_SOFTWARE_VERSION);
                            Prop.Value[0] = this.GetType().Assembly.GetName().Version.ToString();
                        }
                        comm.OnWhoIs += new BacnetClient.WhoIsHandler(OnWhoIs);
                        comm.OnReadPropertyRequest += new BacnetClient.ReadPropertyRequestHandler(OnReadPropertyRequest);
                        comm.OnReadPropertyMultipleRequest += new BacnetClient.ReadPropertyMultipleRequestHandler(OnReadPropertyMultipleRequest);
                    }
                    else
                    {
                        comm.OnWhoIs += new BacnetClient.WhoIsHandler(OnWhoIsIgnore);
                    }
                    comm.OnIam += new BacnetClient.IamHandler(OnIam);
                    comm.OnCOVNotification += new BacnetClient.COVNotificationHandler(OnCOVNotification);
                    comm.OnEventNotify += new BacnetClient.EventNotificationCallbackHandler(OnEventNotify);
                    comm.Start();

                    // WhoIs Min & Max limits
                    int IdMin = -1, IdMax = -1;
                    Int32.TryParse(dlg.WhoLimitLow.Text, out IdMin); Int32.TryParse(dlg.WhoLimitHigh.Text, out IdMax);
                    if (IdMin == 0) IdMin = -1; if (IdMax == 0) IdMax = -1;
                    if ((IdMin!=-1)&&(IdMax==-1)) IdMax=0x3FFFFF;
                    if ((IdMax != -1) && (IdMin == -1)) IdMin = 0;

                    //start search
                    if (comm.Transport.Type == BacnetAddressTypes.IP || comm.Transport.Type == BacnetAddressTypes.Ethernet 
                        || comm.Transport.Type == BacnetAddressTypes.IPV6 
                        || (comm.Transport is BacnetMstpProtocolTransport && ((BacnetMstpProtocolTransport)comm.Transport).SourceAddress != -1) 
                        || comm.Transport.Type == BacnetAddressTypes.PTP)
                    {
                        System.Threading.ThreadPool.QueueUserWorkItem((o) =>
                        {
                            for (int i = 0; i < comm.Retries; i++)
                            {
                                comm.WhoIs(IdMin,IdMax);
                                System.Threading.Thread.Sleep(comm.Timeout);
                            }
                        }, null);
                    }

                    //special MSTP auto discovery
                    if (comm.Transport is BacnetMstpProtocolTransport)
                    {
                        ((BacnetMstpProtocolTransport)comm.Transport).FrameRecieved += new BacnetMstpProtocolTransport.FrameRecievedHandler(MSTP_FrameRecieved);
                    }
                }
                catch (Exception ex)
                {
                    m_devices.Remove(comm);
                    node.Remove();
                    MessageBox.Show(this, "Couldn't start Bacnet communication: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void MSTP_FrameRecieved(BacnetMstpProtocolTransport sender, BacnetMstpFrameTypes frame_type, byte destination_address, byte source_address, int msg_length)
        {
            try
            {
                if (this.IsDisposed) return;
                BacnetDeviceLine device_line = null;
                foreach (BacnetDeviceLine l in m_devices.Values)
                {
                    if (l.Line.Transport == sender)
                    {
                        device_line = l;
                        break;
                    }
                }
                if (device_line == null) return;
                lock (device_line.mstp_sources_seen)
                {
                    if (!device_line.mstp_sources_seen.Contains(source_address))
                    {
                        device_line.mstp_sources_seen.Add(source_address);

                        //find parent node
                        TreeNode parent = FindCommTreeNode(sender);

                        //find "free" node. The "free" node might have been added
                        TreeNode free_node = null;
                        foreach (TreeNode n in parent.Nodes)
                        {
                            if (n.Text == "free" + source_address)
                            {
                                free_node = n;
                                break;
                            }
                        }

                        //update gui
                        this.Invoke((MethodInvoker)delegate
                        {
                            TreeNode node = parent.Nodes.Add("device" + source_address);
                            node.ImageIndex = 2;
                            node.SelectedImageIndex = node.ImageIndex;
                            node.Tag = new KeyValuePair<BacnetAddress, uint>(new BacnetAddress(BacnetAddressTypes.MSTP, 0, new byte[] { source_address }), 0xFFFFFFFF);
                            if (free_node != null) free_node.Remove();
                            m_DeviceTree.ExpandAll();
                        });

                        //detect collision
                        if (source_address == sender.SourceAddress)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                MessageBox.Show(this, "Selected source address seems to be occupied!", "Collision detected", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            });
                        }
                    }
                    if (frame_type == BacnetMstpFrameTypes.FRAME_TYPE_POLL_FOR_MASTER && !device_line.mstp_pfm_destinations_seen.Contains(destination_address) && sender.SourceAddress != destination_address)
                    {
                        device_line.mstp_pfm_destinations_seen.Add(destination_address);
                        if (!device_line.mstp_sources_seen.Contains(destination_address) && Properties.Settings.Default.MSTP_DisplayFreeAddresses)
                        {
                            TreeNode parent = FindCommTreeNode(sender);
                            if (this.IsDisposed) return;
                            this.Invoke((MethodInvoker)delegate
                            {
                                TreeNode node = parent.Nodes.Add("free" + destination_address);
                                node.ImageIndex = 9;
                                node.SelectedImageIndex = node.ImageIndex;
                                m_DeviceTree.ExpandAll();
                            });
                        }
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                //we're closing down ... ignore
            }
        }

        private void m_SearchToolButton_Click(object sender, EventArgs e)
        {
            addDevicesearchToolStripMenuItem_Click(this, null);
        }

        private void RemoveSubscriptions(BacnetAddress adr, uint deviceId, BacnetClient comm)
        {
            LinkedList<string> deletes = new LinkedList<string>();
            foreach (KeyValuePair<string, ListViewItem> entry in m_subscription_list)
            {
                Subscription sub = (Subscription)entry.Value.Tag;
                if (((sub.adr == adr) && (sub.device_id.instance == deviceId)) || (sub.comm == comm))
                {
                    m_SubscriptionView.Items.Remove(entry.Value);
                    deletes.AddLast(sub.sub_key);
                }
            }
            foreach (string sub_key in deletes)
            {
                m_subscription_list.Remove(sub_key);
                try
                {
                    RollingPointPairList points = m_subscription_points[sub_key];
                    foreach (LineItem l in Pane.CurveList)
                        if (l.Tag == points)
                        {
                            Pane.CurveList.Remove(l);
                            break;
                        }

                    m_subscription_points.Remove(sub_key);
                }
                catch { }
            }

            CovGraph.AxisChange();
            CovGraph.Invalidate();
        }        

        private void removeDeviceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_DeviceTree.SelectedNode == null) return;
            else if (m_DeviceTree.SelectedNode.Tag == null) return;
            KeyValuePair<BacnetAddress, uint>? device_entry = m_DeviceTree.SelectedNode.Tag as KeyValuePair<BacnetAddress, uint>?;
            BacnetClient comm_entry; 
            if (m_DeviceTree.SelectedNode.Tag is BacnetClient)    
                comm_entry = m_DeviceTree.SelectedNode.Tag as BacnetClient;
            else
                 comm_entry = m_DeviceTree.SelectedNode.Parent.Tag as BacnetClient;

            if (device_entry != null)
            {
                if (MessageBox.Show(this, "Delete this device?", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    BacnetClient comm;
                    if (m_DeviceTree.SelectedNode.Parent.Tag is BacnetClient)
                        comm = m_DeviceTree.SelectedNode.Parent.Tag as BacnetClient;
                    else
                        comm = m_DeviceTree.SelectedNode.Parent.Parent.Tag as BacnetClient; // device under a router

                    m_devices[comm].Devices.Remove((KeyValuePair<BacnetAddress, uint>)device_entry);

                    m_DeviceTree.Nodes.Remove(m_DeviceTree.SelectedNode);
                    RemoveSubscriptions(device_entry.Value.Key, device_entry.Value.Value, null);
                }
            }
            else if (comm_entry != null)
            {
                if (MessageBox.Show(this, "Delete this transport?", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    m_devices.Remove(comm_entry);
                    m_DeviceTree.Nodes.Remove(m_DeviceTree.SelectedNode);
                    RemoveSubscriptions(null, 0, comm_entry);
                    comm_entry.Dispose();
                }
            }
        }

        private void m_RemoveToolButton_Click(object sender, EventArgs e)
        {
            removeDeviceToolStripMenuItem_Click(this, null);
        }

        public static int GetIconNum(BacnetObjectTypes object_type)
        {
            switch (object_type)
            {
                case BacnetObjectTypes.OBJECT_DEVICE:
                    return 2;
                case BacnetObjectTypes.OBJECT_FILE:
                    return 5;
                case BacnetObjectTypes.OBJECT_ANALOG_INPUT:
                case BacnetObjectTypes.OBJECT_ANALOG_OUTPUT:
                case BacnetObjectTypes.OBJECT_ANALOG_VALUE:
                    return 6;
                case BacnetObjectTypes.OBJECT_BINARY_INPUT:
                case BacnetObjectTypes.OBJECT_BINARY_OUTPUT:
                case BacnetObjectTypes.OBJECT_BINARY_VALUE:
                    return 7;
                case BacnetObjectTypes.OBJECT_GROUP:
                    return 10;
                case BacnetObjectTypes.OBJECT_STRUCTURED_VIEW:
                    return 11;
                case BacnetObjectTypes.OBJECT_TRENDLOG:
                    return 12;
                case BacnetObjectTypes.OBJECT_TREND_LOG_MULTIPLE:
                    return 12;
                case BacnetObjectTypes.OBJECT_NOTIFICATION_CLASS:
                    return 13;
                case BacnetObjectTypes.OBJECT_SCHEDULE:
                    return 14;
                case BacnetObjectTypes.OBJECT_CALENDAR:
                    return 15;
                default:
                    return 4;
            }
        }
        private void SetNodeIcon(BacnetObjectTypes object_type, TreeNode node)
        {
            node.ImageIndex = GetIconNum(object_type);            
            node.SelectedImageIndex = node.ImageIndex;
        }

#if DEBUG
        private int depth = 0;
        private const int maxDepth = 5;
#endif
        private void AddObjectEntry(BacnetClient comm, BacnetAddress adr, string name, BacnetObjectId object_id, TreeNodeCollection nodes)
        {
            bool iAmTheCreator = false;
            bool recursionDetected = false;
            if (object_id.type==BacnetObjectTypes.OBJECT_STRUCTURED_VIEW)
            {
                if(_structuredViewParents==null)
                {
                    _structuredViewParents = new List<BacnetObjectId>();
                    iAmTheCreator = true;
                }

                if(_structuredViewParents.Contains(object_id))
                {
                    recursionDetected = true;
#if DEBUG
                    depth++;
#endif
                }
                else
                {
                    _structuredViewParents.Add(object_id);
                }
            }

            if (string.IsNullOrEmpty(name)) name = object_id.ToString();

            TreeNode node;

            if (name.StartsWith("OBJECT_"))
                node = nodes.Add(name.Substring(7));
            else
                node = nodes.Add("PROPRIETARY:" + object_id.Instance.ToString() + " (" + name + ")");  // Propertary Objects not in enum appears only with the number such as 584:0

            node.Tag = object_id;

            //icon
            SetNodeIcon(object_id.type, node);

            // Get the property name if already known
            String PropName;

            lock (DevicesObjectsName)
                if (DevicesObjectsName.TryGetValue(new Tuple<String, BacnetObjectId>(adr.FullHashString(), object_id), out PropName) == true)
                {
                    ChangeTreeNodePropertyName(node, PropName); ;
                }

            //fetch sub properties
            if (object_id.type == BacnetObjectTypes.OBJECT_GROUP)
            {
                FetchGroupProperties(comm, adr, object_id, node.Nodes);
            }
            else if ((object_id.type == BacnetObjectTypes.OBJECT_STRUCTURED_VIEW) && (Properties.Settings.Default.Address_Space_Structured_View == AddressTreeViewType.Structured || Properties.Settings.Default.Address_Space_Structured_View == AddressTreeViewType.Both))
            {
                if (recursionDetected)
                {
#if DEBUG
                    if (depth > maxDepth)
                    {
#endif
                        TreeNode recursiveNode = node.Nodes.Add("WARNING: RECURSIVE NODE DETECTED");
                        recursiveNode.ImageIndex = 16;
                        recursiveNode.SelectedImageIndex = 16;
#if DEBUG
                    }
                    else
                    {
                        FetchViewObjects(comm, adr, object_id, node.Nodes);
                    }
                    depth--;
#endif
                }
                else
                {
                    FetchViewObjects(comm, adr, object_id, node.Nodes);
                }
            }
            else if ((object_id.type == BacnetObjectTypes.OBJECT_DEVICE) && (node.Parent == null) && (Properties.Settings.Default.Address_Space_Structured_View == AddressTreeViewType.Structured || Properties.Settings.Default.Address_Space_Structured_View == AddressTreeViewType.Both))
            {
                FetchStructuredObjects(comm, adr, object_id.Instance, node.Nodes);
            }

            if (object_id.type == BacnetObjectTypes.OBJECT_STRUCTURED_VIEW)
            {
                if (_structuredViewParents != null)
                {
                    if(_structuredViewParents.Contains(object_id))
                    {
                        _structuredViewParents.Remove(object_id);
                    }
                    if (iAmTheCreator)
                    {
                        _structuredViewParents = null;
                    }
                }
                
            }
        }

        

        private IList<BacnetValue> FetchStructuredObjects(BacnetClient comm, BacnetAddress adr, uint device_id, TreeNodeCollection nodes)
        {
            IList<BacnetValue> ret;
            int old_reties = comm.Retries;
            try
            {
                comm.Retries = 1;       //only do 1 retry
                if (!comm.ReadPropertyRequest(adr, new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE, device_id), BacnetPropertyIds.PROP_STRUCTURED_OBJECT_LIST, out ret))
                {
                    Trace.TraceInformation("Didn't get response from 'Structured Object List'");
                    return null;
                }
                else
                {
                    List<BacnetObjectId> objectList = SortBacnetObjects(ret);
                    foreach (BacnetObjectId objid in objectList)
                        AddObjectEntry(comm, adr, null, objid, nodes);
                }
            }
            catch (Exception)
            {
                Trace.TraceInformation("Got exception from 'Structured Object List'");
                return null;
            }
            finally
            {
                comm.Retries = old_reties;
            }
            return ret;
        }

        private void AddObjectListOneByOneAsync(BacnetClient comm, BacnetAddress adr, uint device_id, uint count, int AsynchRequestId)
        {
            System.Threading.ThreadPool.QueueUserWorkItem((o) =>
            {
                IList<BacnetValue> value_list;
                try
                {
                    for (int i = 1; i <= count; i++)
                    {
                        value_list = null;
                        if (!comm.ReadPropertyRequest(adr, new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE, device_id), BacnetPropertyIds.PROP_OBJECT_LIST, out value_list, 0, (uint)i))
                        {
                            MessageBox.Show("Couldn't fetch object list index", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (AsynchRequestId != this.AsynchRequestId) return; // Selected device is no more the good one

                        //add to tree
                        foreach (BacnetValue value in value_list)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                if (AsynchRequestId != this.AsynchRequestId) return;  // another test in the GUI thread
                                AddObjectEntry(comm, adr, null, (BacnetObjectId)value.Value, m_AddressSpaceTree.Nodes);
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error during read: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            });
        }

        public List<BacnetObjectId> SortBacnetObjects(IList<BacnetValue> RawList)
        {

            List<BacnetObjectId> SortedList = new List<BacnetObjectId>();
            foreach (BacnetValue value in RawList)
                if (value.Value is BacnetObjectId) // with BacnetObjectId
                    SortedList.Add((BacnetObjectId)value.Value);
                else // with Subordinate_List for StructuredView
                {
                    BacnetDeviceObjectReference v = (BacnetDeviceObjectReference)value.Value;
                    SortedList.Add(v.objectIdentifier); // ignore deviceIdentifier
                }

            SortedList.Sort();

            return SortedList;
        }

        

        private void m_DeviceTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            AsynchRequestId++; // disabled a possible thread pool work (update) on the AddressSpaceTree
            TreeNode node = e.Node;
            _selectedDevice = null;
            KeyValuePair<BacnetAddress, uint>? entry = e.Node.Tag as KeyValuePair<BacnetAddress, uint>?;
            if (entry != null)
            {
                m_AddressSpaceTree.Nodes.Clear();   //clear
                AddSpaceLabel.Text = "Address Space";

                BacnetClient comm ;

                if (e.Node.Parent.Tag is BacnetClient)  // A 'basic node'
                    comm = (BacnetClient)e.Node.Parent.Tag;
                else  // A routed node
                    comm = (BacnetClient)e.Node.Parent.Parent.Tag;

                BacnetAddress adr = entry.Value.Key;
                uint device_id = entry.Value.Value;

                //unconfigured MSTP?
                if (comm.Transport is BacnetMstpProtocolTransport && ((BacnetMstpProtocolTransport)comm.Transport).SourceAddress == -1)
                {
                    if (MessageBox.Show("The MSTP transport is not yet configured. Would you like to set source_address now?", "Set Source Address", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.No) return;

                    //find suggested address
                    byte address = 0xFF;
                    BacnetDeviceLine line = m_devices[comm];
                    lock (line.mstp_sources_seen)
                    {
                        foreach (byte s in line.mstp_pfm_destinations_seen)
                        {
                            if (s < address && !line.mstp_sources_seen.Contains(s))
                                address = s;
                        }
                    }

                    //display choice
                    SourceAddressDialog dlg = new SourceAddressDialog();
                    dlg.SourceAddress = address;
                    if( dlg.ShowDialog(this) == System.Windows.Forms.DialogResult.Cancel) return;
                    ((BacnetMstpProtocolTransport)comm.Transport).SourceAddress = dlg.SourceAddress;
                    Application.DoEvents();     //let the interface relax
                }

                //update "address space"?
                this.Cursor = Cursors.WaitCursor;
                Application.DoEvents();
                int old_timeout = comm.Timeout;
                IList<BacnetValue> value_list = null;

                try
                {
                    if (Properties.Settings.Default.Address_Space_Structured_View==AddressTreeViewType.Structured)
                    {
                        value_list = FetchStructuredObjects(comm,adr,device_id, m_AddressSpaceTree.Nodes);

                        BacnetObjectId bobj_id = new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE, device_id);

                        // If the Device name not set, try to update it
                        if (node.ToolTipText == "")   // already update with the device name
                        {
                            bool Prop_Object_NameOK = false;
                            String Identifier;

                            lock (DevicesObjectsName)
                                Prop_Object_NameOK = DevicesObjectsName.TryGetValue(new Tuple<String, BacnetObjectId>(adr.FullHashString(), bobj_id), out Identifier);
                            if (Prop_Object_NameOK)
                            {
                                node.ToolTipText = node.Text;
                                node.Text = Identifier + " [" + bobj_id.Instance.ToString() + "] ";
                            }
                            else
                                try
                                {
                                    IList<BacnetValue> values;
                                    if (comm.ReadPropertyRequest(adr, bobj_id, BacnetPropertyIds.PROP_OBJECT_NAME, out values))
                                    {
                                        node.ToolTipText = node.Text;   // IP or MSTP node id -> in the Tooltip
                                        node.Text = values[0].ToString() + " [" + bobj_id.Instance.ToString() + "] ";  // change @ by the Name    
                                        lock (DevicesObjectsName)
                                        {
                                            Tuple<String, BacnetObjectId> t = new Tuple<String, BacnetObjectId>(adr.FullHashString(), bobj_id);
                                            if(DevicesObjectsName.ContainsKey(t))
                                            {
                                                if(!DevicesObjectsName[t].Equals(values[0].ToString()))
                                                {
                                                    DevicesObjectsName.Remove(t);
                                                    DevicesObjectsName.Add(t, values[0].ToString());
                                                    objectNamesChangedFlag = true;
                                                }
                                            }
                                            else
                                            {
                                                DevicesObjectsName.Add(t, values[0].ToString());
                                                objectNamesChangedFlag = true;
                                            }
                                        }
                                    }
                                }
                                catch { }
                        }
                    }
                    if(value_list!=null)
                    {
                        AddSpaceLabel.Text = "Address Space : " + value_list.Count.ToString() + " objects";
                    }
                    else
                    {
                    //fetch normal list
                        try
                        {
                            if (!comm.ReadPropertyRequest(adr, new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE, device_id), BacnetPropertyIds.PROP_OBJECT_LIST, out value_list))
                            {
                                Trace.TraceWarning("Didn't get response from 'Object List'");
                                value_list = null;
                            }
                        }
                        catch (Exception)
                        {
                            Trace.TraceWarning("Got exception from 'Object List'");
                            value_list = null;
                        }


                        //fetch list one-by-one
                        if (value_list == null)
                        {
                            try
                            {
                                //fetch object list count
                                if (!comm.ReadPropertyRequest(adr, new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE, device_id), BacnetPropertyIds.PROP_OBJECT_LIST, out value_list, 0, 0))
                                {
                                    MessageBox.Show(this, "Couldn't fetch objects", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(this, "Error during read: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            if (value_list != null && value_list.Count == 1 && value_list[0].Value is ulong)
                            {
                                uint list_count = (uint)(ulong)value_list[0].Value;
                                AddSpaceLabel.Text = "Address Space : " + list_count.ToString() + " objects";
                                AddObjectListOneByOneAsync(comm, adr, device_id, list_count, AsynchRequestId);
                                return;
                            }
                            else
                            {
                                MessageBox.Show(this, "Couldn't read 'Object List' count", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }

                        List<BacnetObjectId> objectList = SortBacnetObjects(value_list);

                        AddSpaceLabel.Text = "Address Space : " + objectList.Count.ToString() + " objects";
                        //add to tree
                        foreach (BacnetObjectId bobj_id in objectList)
                        {
                            // Add FC
                            // If the Device name not set, try to update it
                            if (bobj_id.type == BacnetObjectTypes.OBJECT_DEVICE)
                            {
                                // If the Device name not set, try to update it
                                if (node.ToolTipText == "")   // already update with the device name
                                {
                                    bool Prop_Object_NameOK = false;
                                    String Identifier;

                                    lock (DevicesObjectsName)
                                        Prop_Object_NameOK = DevicesObjectsName.TryGetValue(new Tuple<String, BacnetObjectId>(adr.FullHashString(), bobj_id), out Identifier);
                                    if (Prop_Object_NameOK)
                                    {
                                        node.ToolTipText = node.Text;
                                        node.Text = Identifier + " [" + bobj_id.Instance.ToString() + "] ";
                                    }
                                    else
                                        try
                                        {
                                            IList<BacnetValue> values;
                                            if (comm.ReadPropertyRequest(adr, bobj_id, BacnetPropertyIds.PROP_OBJECT_NAME, out values))
                                            {
                                                node.ToolTipText = node.Text;   // IP or MSTP node id -> in the Tooltip
                                                node.Text = values[0].ToString() + " [" + bobj_id.Instance.ToString() + "] ";  // change @ by the Name    
                                                lock (DevicesObjectsName)
                                                {
                                                    Tuple<String, BacnetObjectId> t = new Tuple<String, BacnetObjectId>(adr.FullHashString(), bobj_id);
                                                    if (DevicesObjectsName.ContainsKey(t))
                                                    {
                                                        if (!DevicesObjectsName[t].Equals(values[0].ToString()))
                                                        {
                                                            DevicesObjectsName.Remove(t);
                                                            DevicesObjectsName.Add(t, values[0].ToString());
                                                            objectNamesChangedFlag = true;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        DevicesObjectsName.Add(t, values[0].ToString());
                                                        objectNamesChangedFlag = true;
                                                    }
                                                }
                                            }
                                        }
                                        catch { }
                                }
                            }

                            AddObjectEntry(comm, adr, null, bobj_id, m_AddressSpaceTree.Nodes);//AddObjectEntry(comm, adr, null, bobj_id, e.Node.Nodes); 
                        }
                    }
                    _selectedDevice = node;
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                    _selectedNode = null;
                    m_DataGrid.SelectedObject = null;
                }
            }
        }

        private void FetchViewObjects(BacnetClient comm, BacnetAddress adr, BacnetObjectId object_id, TreeNodeCollection nodes)
        {
            try
            {
                IList<BacnetValue> values;
                if (comm.ReadPropertyRequest(adr, object_id, BacnetPropertyIds.PROP_SUBORDINATE_LIST, out values))
                {
                    List<BacnetObjectId> objectList = SortBacnetObjects(values);
                    foreach (BacnetObjectId objid in objectList)
                        AddObjectEntry(comm, adr, null, objid, nodes);
                }
                else
                {
                    Trace.TraceWarning("Couldn't fetch view members");
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Couldn't fetch view members: " + ex.Message);
            }
        }

        private void FetchGroupProperties(BacnetClient comm, BacnetAddress adr, BacnetObjectId object_id, TreeNodeCollection nodes)
        {
            try
            {
                IList<BacnetValue> values;
                if (comm.ReadPropertyRequest(adr, object_id, BacnetPropertyIds.PROP_LIST_OF_GROUP_MEMBERS, out values))
                {
                    foreach (BacnetValue value in values)
                    {
                        if (value.Value is BacnetReadAccessSpecification)
                        {
                            BacnetReadAccessSpecification spec = (BacnetReadAccessSpecification)value.Value;
                            foreach (BacnetPropertyReference p in spec.propertyReferences)
                            {
                                AddObjectEntry(comm, adr, spec.objectIdentifier.ToString() + ":" + ((BacnetPropertyIds)p.propertyIdentifier).ToString(), spec.objectIdentifier, nodes);
                            }
                        }
                    }
                }
                else
                {
                    Trace.TraceWarning("Couldn't fetch group members");
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Couldn't fetch group members: " + ex.Message);
            }
        }

        private void addDeviceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addDevicesearchToolStripMenuItem_Click(this, null);
        }

        private void removeDeviceToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            removeDeviceToolStripMenuItem_Click(this, null);
        }

        public string GetNiceName(BacnetPropertyIds property, bool showNumberAlways = false)
        {
            string name = property.ToString();
            if (name.StartsWith("PROP_"))
            {
                name = name.Substring(5);
                name = name.Replace('_', ' ');
                name = (showNumberAlways ? String.Format("{0} - ", (int)property) : string.Empty) + System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(name.ToLower());
            }
            
            else
            {
                if (_customPropertyGetters.Count > 0)
                {
                    foreach (CustomPropertyGetter pg in _customPropertyGetters)
                    {
                        if (pg((int)property, out name))
                        {
                            return name;
                        }
                    }
                }
                //name = "Proprietary (" + property.ToString() + ")";
                name = property.ToString() + " - Proprietary";
            }
            return name;
        }

        private bool ReadProperty(BacnetClient comm, BacnetAddress adr, BacnetObjectId object_id, BacnetPropertyIds property_id, ref IList<BacnetPropertyValue> values, uint array_index = System.IO.BACnet.Serialize.ASN1.BACNET_ARRAY_ALL)
        {
            BacnetPropertyValue new_entry = new BacnetPropertyValue();
            new_entry.property = new BacnetPropertyReference((uint)property_id, array_index);
            IList<BacnetValue> value;
            try
            {
                if (!comm.ReadPropertyRequest(adr, object_id, property_id, out value, 0, array_index))
                    return false;     //ignore
            }
            catch
            {
                return false;         //ignore
            }
            new_entry.value = value;

            values.Add(new_entry);
            return true;
        }

        public bool ReadAllPropertiesBySingle(BacnetClient comm, BacnetAddress adr, BacnetObjectId object_id, out IList<BacnetReadAccessResult> value_list)
        {

            if (objectsDescriptionDefault == null)  // first call, Read Objects description from internal & optional external xml file
            {
                StreamReader sr;
                XmlSerializer xs = new XmlSerializer(typeof(List<BacnetObjectDescription>));

                // embedded resource
                System.Reflection.Assembly _assembly;
                _assembly = System.Reflection.Assembly.GetExecutingAssembly();
                sr = new StreamReader(_assembly.GetManifestResourceStream("Yabe.ReadSinglePropDescrDefault.xml"));
                objectsDescriptionDefault = (List<BacnetObjectDescription>)xs.Deserialize(sr);

                try  // External optional file
                {
                    sr = new StreamReader("ReadSinglePropDescr.xml");
                    objectsDescriptionExternal = (List<BacnetObjectDescription>)xs.Deserialize(sr);
                }
                catch { }

            }

            value_list = null;

            IList<BacnetPropertyValue> values = new List<BacnetPropertyValue>();

            int old_retries = comm.Retries;
            comm.Retries = 1;       //we don't want to spend too much time on non existing properties
            try
            {
                // PROP_LIST was added as an addendum to 135-2010
                // Test to see if it is supported, otherwise fall back to the the predefined delault property list.
                bool objectDidSupplyPropertyList = ReadProperty(comm, adr, object_id, BacnetPropertyIds.PROP_PROPERTY_LIST, ref values);

                //Used the supplied list of supported Properties, otherwise fall back to using the list of default properties.
                if (objectDidSupplyPropertyList)
                {
                    var proplist = values.Last();

                    foreach (var enumeratedValue in proplist.value)
                    {
                        BacnetPropertyIds bpi = (BacnetPropertyIds)(uint)enumeratedValue.Value;
                        // read all specified properties given by the PROP_PROPERTY_LIST, except the 3 previous one
                        ReadProperty(comm, adr, object_id, bpi, ref values);
                    }

                    // 3 required properties not in the list
                    ReadProperty(comm, adr, object_id, BacnetPropertyIds.PROP_OBJECT_NAME, ref values);
                    ReadProperty(comm, adr, object_id, BacnetPropertyIds.PROP_OBJECT_TYPE, ref values);
                    ReadProperty(comm, adr, object_id, BacnetPropertyIds.PROP_OBJECT_IDENTIFIER, ref values);

                }
                else
                {
                    // Three mandatory common properties to all objects : PROP_OBJECT_IDENTIFIER,PROP_OBJECT_TYPE, PROP_OBJECT_NAME

                    // ReadProperty(comm, adr, object_id, BacnetPropertyIds.PROP_OBJECT_IDENTIFIER, ref values)
                    // No need to query it, known value
                    BacnetPropertyValue new_entry = new BacnetPropertyValue();
                    new_entry.property = new BacnetPropertyReference((uint)BacnetPropertyIds.PROP_OBJECT_IDENTIFIER, System.IO.BACnet.Serialize.ASN1.BACNET_ARRAY_ALL);
                    new_entry.value = new BacnetValue[] { new BacnetValue(object_id) };
                    values.Add(new_entry);

                    // ReadProperty(comm, adr, object_id, BacnetPropertyIds.PROP_OBJECT_TYPE, ref values);
                    // No need to query it, known value
                    new_entry = new BacnetPropertyValue();
                    new_entry.property = new BacnetPropertyReference((uint)BacnetPropertyIds.PROP_OBJECT_TYPE, System.IO.BACnet.Serialize.ASN1.BACNET_ARRAY_ALL);
                    new_entry.value = new BacnetValue[] { new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_ENUMERATED, (uint)object_id.type) };
                    values.Add(new_entry);

                    // We do not know the value here
                    ReadProperty(comm, adr, object_id, BacnetPropertyIds.PROP_OBJECT_NAME, ref values);

                    // for all other properties, the list is comming from the internal or external XML file

                    BacnetObjectDescription objDescr = new BacnetObjectDescription(); ;

                    int Idx = -1;
                    // try to find the Object description from the optional external xml file
                    if (objectsDescriptionExternal != null)
                        Idx = objectsDescriptionExternal.FindIndex(o => o.typeId == object_id.type);

                    if (Idx != -1)
                        objDescr = objectsDescriptionExternal[Idx];
                    else
                    {
                        // try to find from the embedded resoruce
                        Idx = objectsDescriptionDefault.FindIndex(o => o.typeId == object_id.type);
                        if (Idx != -1)
                            objDescr = objectsDescriptionDefault[Idx];
                    }

                    if (Idx != -1)
                        foreach (BacnetPropertyIds bpi in objDescr.propsId)
                            // read all specified properties given by the xml file
                            ReadProperty(comm, adr, object_id, bpi, ref values);
                }
            }
            catch { }

            comm.Retries = old_retries;
            value_list = new BacnetReadAccessResult[] { new BacnetReadAccessResult(object_id, values) };
            return true;
        }

        private String UpdateGrid(BacnetClient comm, BacnetAddress adr, BacnetObjectId object_id)
        {
            string ReturnPROP_OBJECT_NAME = null;
            try
            {
                m_DataGrid.SelectedObject = null;   //clear

                BacnetPropertyReference[] properties = new BacnetPropertyReference[] { new BacnetPropertyReference((uint)BacnetPropertyIds.PROP_ALL, System.IO.BACnet.Serialize.ASN1.BACNET_ARRAY_ALL) };
                IList<BacnetReadAccessResult> multi_value_list;
                try
                {
                    //fetch properties. This might not be supported (ReadMultiple) or the response might be too long.
                    if (!comm.ReadPropertyMultipleRequest(adr, object_id, properties, out multi_value_list))
                    {
                        Trace.TraceWarning("Couldn't perform ReadPropertyMultiple ... Trying ReadProperty instead");
                        if (!ReadAllPropertiesBySingle(comm, adr, object_id, out multi_value_list))
                        {
                            MessageBox.Show(this, "Couldn't fetch properties", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return ReturnPROP_OBJECT_NAME;
                        }
                    }
                }
                catch (Exception)
                {
                    Trace.TraceWarning("Couldn't perform ReadPropertyMultiple ... Trying ReadProperty instead");
                    Application.DoEvents();
                    try
                    {
                        //fetch properties with single calls
                        if (!ReadAllPropertiesBySingle(comm, adr, object_id, out multi_value_list))
                        {
                            MessageBox.Show(this, "Couldn't fetch properties", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return ReturnPROP_OBJECT_NAME;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, "Error during read: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return ReturnPROP_OBJECT_NAME;
                    }
                }

                //update grid
                Utilities.DynamicPropertyGridContainer bag = new Utilities.DynamicPropertyGridContainer();
                foreach (BacnetPropertyValue p_value in multi_value_list[0].values)
                {
                    object value = null;
                    BacnetValue[] b_values = null;
                    if (p_value.value != null)
                    {

                        b_values = new BacnetValue[p_value.value.Count];

                        p_value.value.CopyTo(b_values, 0);
                        if (b_values.Length > 1)
                        {
                            object[] arr = new object[b_values.Length];
                            for (int j = 0; j < arr.Length; j++)
                                arr[j] = b_values[j].Value;
                            value = arr;
                        }
                        else if (b_values.Length == 1)
                            value = b_values[0].Value;
                    }
                    else
                        b_values = new BacnetValue[0];

                    switch ((BacnetPropertyIds)p_value.property.propertyIdentifier)
                    {
                        // PROP_PRESENT_VALUE can be write at null value to clear the prioroityarray if exists
                        case BacnetPropertyIds.PROP_PRESENT_VALUE:
                            // change to the related nullable type
                            Type t = value.GetType();
                            try
                            {
                                if (t != typeof(String)) // a bug on linuxmono where the folling instruction generates a wrong type
                                    t = Type.GetType("System.Nullable`1[" + value.GetType().FullName + "]");
                            }
                            catch { }
                            bag.Add(new Utilities.CustomProperty(GetNiceName((BacnetPropertyIds)p_value.property.propertyIdentifier), value, t != null ? t : typeof(string), false, "", b_values.Length > 0 ? b_values[0].Tag : (BacnetApplicationTags?)null, null, p_value.property));
                            break;

                        default:
                            bag.Add(new Utilities.CustomProperty(GetNiceName((BacnetPropertyIds)p_value.property.propertyIdentifier), value, value != null ? value.GetType() : typeof(string), false, "", b_values.Length > 0 ? b_values[0].Tag : (BacnetApplicationTags?)null, null, p_value.property));
                            break;
                    }

                    // The Prop Name replace the PropId into the Treenode 
                    if (p_value.property.propertyIdentifier == (byte)BacnetPropertyIds.PROP_OBJECT_NAME)
                    {
                        ReturnPROP_OBJECT_NAME = value.ToString();
                    }
                }

                m_DataGrid.SelectedObject = bag;
            }
            catch { }

            return ReturnPROP_OBJECT_NAME;
        }


        private void UpdateGrid(TreeNode selected_node)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                _selectedNode = null;
                //fetch end point
                if (m_DeviceTree.SelectedNode == null) return;
                else if (m_DeviceTree.SelectedNode.Tag == null) return;
                else if (!(m_DeviceTree.SelectedNode.Tag is KeyValuePair<BacnetAddress, uint>)) return;
                KeyValuePair<BacnetAddress, uint> entry = (KeyValuePair<BacnetAddress, uint>)m_DeviceTree.SelectedNode.Tag;
                BacnetAddress adr = entry.Key;
                BacnetClient comm;

                if (m_DeviceTree.SelectedNode.Parent.Tag is BacnetClient)
                    comm = (BacnetClient)m_DeviceTree.SelectedNode.Parent.Tag;
                else
                    comm = (BacnetClient)m_DeviceTree.SelectedNode.Parent.Parent.Tag;  // routed node

                if (selected_node.Tag is BacnetObjectId)
                {
                    m_DataGrid.SelectedObject = null;   //clear

                    BacnetObjectId object_id = (BacnetObjectId)selected_node.Tag;

                    String NewObjectName = UpdateGrid(comm, adr, object_id);

                    if (NewObjectName != null)
                    {
                        ChangeTreeNodePropertyName(selected_node, NewObjectName);// Update the object name if needed
                        lock (DevicesObjectsName)
                        {
                            Tuple<String, BacnetObjectId> t = new Tuple<String, BacnetObjectId>(adr.FullHashString(), object_id);
                            if (DevicesObjectsName.ContainsKey(t))
                            {
                                if (!DevicesObjectsName[t].Equals(NewObjectName))
                                {
                                    DevicesObjectsName.Remove(t);
                                    DevicesObjectsName.Add(t, NewObjectName);
                                    objectNamesChangedFlag = true;
                                }
                            }
                            else
                            {
                                DevicesObjectsName.Add(t, NewObjectName);
                                objectNamesChangedFlag = true;
                            }
                        }
                    }

                    _selectedNode = selected_node;
                }
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void UpdateGrid(Subscription subscription)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                _selectedNode = null;
                BacnetAddress adr = subscription.adr;
                BacnetClient comm = subscription.comm;

                m_DataGrid.SelectedObject = null;   //clear

                BacnetObjectId object_id = subscription.object_id;

                UpdateGrid(comm, adr, object_id);

                _selectedNode = subscription;

            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }



        // Fixed a small problem when a right click is down in a Treeview
        private void TreeView_MouseDown(object sender, MouseEventArgs e)
        {
            //if (e.Button != MouseButtons.Right)
            //    return;
            // Store the selected node (can deselect a node).
            //(sender as TreeView).SelectedNode = (sender as TreeView).GetNodeAt(e.X, e.Y);
        }

        private void m_DataGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                KeyValuePair<BacnetAddress, uint> entry;
                BacnetAddress adr;
                BacnetClient comm;

                //fetch object_id
                BacnetObjectId object_id;

                if(_selectedNode!=null)
                {
                    if(_selectedNode is Subscription)
                    {
                        Subscription subscription=_selectedNode as Subscription;
                        //fetch object_id
                        object_id = subscription.object_id;

                        //fetch end point
                        comm = subscription.comm;
                        adr = subscription.adr;
                    }
                    else if(_selectedNode is TreeNode)
                    {
                        TreeNode selectedObject=_selectedNode as TreeNode;
                        if(_selectedDevice != null)
                        {
                            //fetch end point
                            if (_selectedDevice == null)
                            {
                                _selectedNode = null;
                                m_DataGrid.SelectedObject = null;
                                return;
                            }
                            else if (_selectedDevice.Tag == null)
                            {
                                _selectedNode = null;
                                m_DataGrid.SelectedObject = null;
                                return;
                            }
                            else if (!(_selectedDevice.Tag is KeyValuePair<BacnetAddress, uint>))
                            {
                                _selectedNode = null;
                                m_DataGrid.SelectedObject = null;
                                return;
                            }

                            entry = (KeyValuePair<BacnetAddress, uint>)_selectedDevice.Tag;
                            adr = entry.Key;

                            if (_selectedDevice.Parent.Tag is BacnetClient)
                                comm = (BacnetClient)_selectedDevice.Parent.Tag;
                            else
                                comm = (BacnetClient)_selectedDevice.Parent.Parent.Tag; // a node under a router
                            if (selectedObject.Tag == null) return;
                            else if (!(selectedObject.Tag is BacnetObjectId)) return;
                            object_id = (BacnetObjectId)selectedObject.Tag;
                        }
                        else
                        {
                            _selectedNode = null;
                            m_DataGrid.SelectedObject = null;
                            return;
                        }
                    }
                    else
                    {
                        _selectedNode = null;
                        m_DataGrid.SelectedObject = null;
                        return;
                    }
                }
                else
                {
                    _selectedDevice = null;
                    m_DataGrid.SelectedObject = null;
                    return;
                }


                PropertyGrid pg = null;
                if (s is PropertyGrid)
                {
                    pg = (PropertyGrid)s;
                }

                Utilities.CustomPropertyDescriptor c=null;
                GridItem gridItem=e.ChangedItem;
                // Go up to the Property (could be a sub-element)

                do
                {
                    if (gridItem.PropertyDescriptor is Utilities.CustomPropertyDescriptor)
                        c = (Utilities.CustomPropertyDescriptor)gridItem.PropertyDescriptor;
                    else
                        gridItem = gridItem.Parent;

                } while ((c == null) && (gridItem != null));

                if (c==null) return; // never occur normaly
 
                //fetch property
                BacnetPropertyReference property = (BacnetPropertyReference)c.CustomProperty.Tag;
                //new value
                object new_value = gridItem.Value;

                //convert to bacnet
                BacnetValue[] b_value = null;
                try
                {
                    if (new_value != null && new_value.GetType().IsArray && new_value.GetType() != typeof(byte[]))
                    {
                        Array arr = (Array)new_value;
                        b_value = new BacnetValue[arr.Length];
                        for (int i = 0; i < arr.Length; i++)
                            b_value[i] = new BacnetValue(arr.GetValue(i));
                    }
                    else
                    {
                        {
                            // Modif FC
                            b_value = new BacnetValue[1];
                            if ((BacnetApplicationTags)c.CustomProperty.bacnetApplicationTags != BacnetApplicationTags.BACNET_APPLICATION_TAG_NULL)
                            {
                                b_value[0] = new BacnetValue((BacnetApplicationTags)c.CustomProperty.bacnetApplicationTags, new_value);
                            }
                            else
                            {
                                object o=null;
                                TypeConverter t = new TypeConverter();
                                // try to convert to the simplest type
                                String[] typelist = { "Boolean", "UInt32", "Int32", "Single", "Double" };

                                foreach (String typename in typelist)
                                {
                                    try
                                    {
                                        o=Convert.ChangeType(new_value, Type.GetType("System."+typename));
                                        break;
                                    }
                                    catch { }
                                }
                                
                                if (o==null)
                                    b_value[0] = new BacnetValue(new_value);
                                else
                                    b_value[0] = new BacnetValue(o);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Couldn't convert property: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //write
                try
                {
                    comm.WritePriority = (uint)Properties.Settings.Default.DefaultWritePriority;
                    if (!comm.WritePropertyRequest(adr, object_id, (BacnetPropertyIds)property.propertyIdentifier, b_value))
                    {
                        MessageBox.Show(this, "Couldn't write property", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Error during write: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                //reload
                if (_selectedNode != null)
                {
                    if (_selectedNode is Subscription)
                    {
                        Subscription subscription = _selectedNode as Subscription;
                        UpdateGrid(subscription);
                        if(pg!=null)
                        {
                            pg.SelectedGridItem = gridItem;
                        }

                    }
                    else if (_selectedNode is TreeNode)
                    {
                        TreeNode selectedObject= _selectedNode as TreeNode;
                        if (_selectedDevice!=null)
                        {
                            UpdateGrid(selectedObject);
                            if (pg != null)
                            {
                                pg.SelectedGridItem = gridItem;
                            }
                        }
                        else
                        {
                            _selectedNode = null;
                            m_DataGrid.SelectedObject = null;
                            return;
                        }
                    }
                    else
                    {
                        _selectedNode = null;
                        m_DataGrid.SelectedObject = null;
                        return;
                    }
                }
                else
                {
                    _selectedDevice = null;
                    m_DataGrid.SelectedObject = null;
                    return;
                }

            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        public bool GetObjectLink(out BacnetClient comm, out BacnetAddress adr, out BacnetObjectId object_id, BacnetObjectTypes ExpectedType)
        {

            comm = null;
            adr = new BacnetAddress(BacnetAddressTypes.None, 0, null);
            object_id = new BacnetObjectId();

            try
            {
                if (m_DeviceTree.SelectedNode == null) return false;
                else if (m_DeviceTree.SelectedNode.Tag == null) return false;
                else if (!(m_DeviceTree.SelectedNode.Tag is KeyValuePair<BacnetAddress, uint>)) return false;
                KeyValuePair<BacnetAddress, uint> entry = (KeyValuePair<BacnetAddress, uint>)m_DeviceTree.SelectedNode.Tag;
                adr = entry.Key;
                if (m_DeviceTree.SelectedNode.Parent.Tag is BacnetClient)
                    comm = (BacnetClient)m_DeviceTree.SelectedNode.Parent.Tag;
                else  // a routed node
                    comm = (BacnetClient)m_DeviceTree.SelectedNode.Parent.Parent.Tag;
            }
            catch
            {
                if (ExpectedType!=BacnetObjectTypes.MAX_BACNET_OBJECT_TYPE)
                    MessageBox.Show(this, "This is not a valid node", "Wrong node", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            //fetch object_id
            if (
                m_AddressSpaceTree.SelectedNode == null ||
                !(m_AddressSpaceTree.SelectedNode.Tag is BacnetObjectId) ||
                !(((BacnetObjectId)m_AddressSpaceTree.SelectedNode.Tag).type == ExpectedType))
            {
                String S = ExpectedType.ToString().Substring(7).ToLower();
                if (ExpectedType != BacnetObjectTypes.MAX_BACNET_OBJECT_TYPE)
                {
                    MessageBox.Show(this, "The marked object is not a " + S, S, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }
            }

            if (m_AddressSpaceTree.SelectedNode != null)
            {
                if (m_AddressSpaceTree.SelectedNode.Tag == null) return false;
                object_id = (BacnetObjectId)m_AddressSpaceTree.SelectedNode.Tag;
                return true;
            }

            return false;
        }

        private void downloadFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                //fetch end point
                BacnetClient comm = null;
                BacnetAddress adr;             
                BacnetObjectId object_id;
                if (GetObjectLink(out comm, out adr, out object_id, BacnetObjectTypes.OBJECT_FILE) == false) return;

                //where to store file?
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.FileName = Properties.Settings.Default.GUI_LastFilename;
                if (dlg.ShowDialog(this) != System.Windows.Forms.DialogResult.OK) return;
                string filename = dlg.FileName;
                Properties.Settings.Default.GUI_LastFilename = filename;

                //get file size
                int filesize = FileTransfers.ReadFileSize(comm, adr, object_id);
                if (filesize < 0)
                {
                    MessageBox.Show(this, "Couldn't read file size", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //display progress
                ProgressDialog progress = new ProgressDialog();
                progress.Text = "Downloading file ...";
                progress.Label = "0 of " + (filesize / 1024) + " kb ... (0.0 kb/s)";
                progress.Maximum = filesize;
                progress.Show(this);

                DateTime start = DateTime.Now;
                double kb_per_sec = 0;
                FileTransfers transfer = new FileTransfers();
                EventHandler cancel_handler = (s, a) => { transfer.Cancel = true; };
                progress.Cancel += cancel_handler;
                Action<int> update_progress = (position) =>
                {
                    kb_per_sec = (position / 1024) / (DateTime.Now - start).TotalSeconds;
                    progress.Value = position;
                    progress.Label = string.Format((position / 1024) + " of " + (filesize / 1024) + " kb ... ({0:F1} kb/s)", kb_per_sec);
                };
                Application.DoEvents();
                try
                {
                    if(Properties.Settings.Default.DefaultDownloadSpeed == 2)
                        transfer.DownloadFileBySegmentation(comm, adr, object_id, filename, update_progress);
                    else if(Properties.Settings.Default.DefaultDownloadSpeed == 1)
                        transfer.DownloadFileByAsync(comm, adr, object_id, filename, update_progress);
                    else
                        transfer.DownloadFileByBlocking(comm, adr, object_id, filename, update_progress);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Error during download file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                finally
                {
                    progress.Hide();
                }
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }

            //information
            try
            {
                MessageBox.Show(this, "Done", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
            }
        }

        private void uploadFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                //fetch end point
                BacnetClient comm = null;
                BacnetAddress adr;
                BacnetObjectId object_id;
                if (GetObjectLink(out comm, out adr, out object_id, BacnetObjectTypes.OBJECT_FILE) == false) return;

                //which file to upload?
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.FileName = Properties.Settings.Default.GUI_LastFilename;
                if (dlg.ShowDialog(this) != System.Windows.Forms.DialogResult.OK) return;
                string filename = dlg.FileName;
                Properties.Settings.Default.GUI_LastFilename = filename;

                //display progress
                int filesize = (int)(new System.IO.FileInfo(filename)).Length;
                ProgressDialog progress = new ProgressDialog();
                progress.Text = "Uploading file ...";
                progress.Label = "0 of " + (filesize / 1024) + " kb ... (0.0 kb/s)";
                progress.Maximum = filesize;
                progress.Show(this);

                FileTransfers transfer = new FileTransfers();
                DateTime start = DateTime.Now;
                double kb_per_sec = 0;
                EventHandler cancel_handler = (s, a) => { transfer.Cancel = true; };
                progress.Cancel += cancel_handler;
                Action<int> update_progress = (position) =>
                {
                    kb_per_sec = (position / 1024) / (DateTime.Now - start).TotalSeconds;
                    progress.Value = position;
                    progress.Label = string.Format((position / 1024) + " of " + (filesize / 1024) + " kb ... ({0:F1} kb/s)", kb_per_sec);
                };
                try
                {
                    transfer.UploadFileByBlocking(comm, adr, object_id, filename, update_progress);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Error during upload file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                finally
                {
                    progress.Hide();
                }
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }

            //information
            MessageBox.Show(this, "Done", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // FC
        private void showTrendLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                //fetch end point
                BacnetClient comm;
                BacnetAddress adr;
                BacnetObjectId object_id;

                if (GetObjectLink(out comm, out adr, out object_id, BacnetObjectTypes.OBJECT_TRENDLOG) == false)
                    if (GetObjectLink(out comm, out adr, out object_id, BacnetObjectTypes.OBJECT_TREND_LOG_MULTIPLE) == false) return;             

                new TrendLogDisplay(comm, adr, object_id).ShowDialog();

            }
            catch(Exception ex)
            {
                Trace.TraceError("Error loading TrendLog : " + ex.Message);
            }
        }
        // FC
        private void showScheduleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                //fetch end point
                BacnetClient comm;
                BacnetAddress adr;
                BacnetObjectId object_id;

                if (GetObjectLink(out comm, out adr, out object_id, BacnetObjectTypes.OBJECT_SCHEDULE) == false) return;

                new ScheduleDisplay(m_AddressSpaceTree.ImageList, comm, adr, object_id).ShowDialog();

            }
            catch(Exception ex) { Trace.TraceError("Error loading Schedule : " + ex.Message); }
        }

        private void deleteObjectToolStripMenuItem_Click(object sender, EventArgs e)
        {

            try
            {
                //fetch end point
                BacnetClient comm;
                BacnetAddress adr;
                BacnetObjectId object_id;

                GetObjectLink(out comm, out adr, out object_id, BacnetObjectTypes.MAX_BACNET_OBJECT_TYPE);

                if (MessageBox.Show("Are you sure you want to delete this object ?", object_id.ToString(), MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    comm.DeleteObjectRequest(adr, object_id);
                    m_DeviceTree_AfterSelect(null, new TreeViewEventArgs(m_DeviceTree.SelectedNode));
                }

            }
            catch (Exception ex) 
            {
                Trace.TraceError("Error : " + ex.Message);
                MessageBox.Show("Fail to Delete Object", "DeleteObject", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void showCalendarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                //fetch end point
                BacnetClient comm;
                BacnetAddress adr;
                BacnetObjectId object_id;

                if (GetObjectLink(out comm, out adr, out object_id, BacnetObjectTypes.OBJECT_CALENDAR) == false) return;

                new CalendarEditor(comm, adr, object_id).ShowDialog();

            }
            catch (Exception ex) { Trace.TraceError("Error loading Calendar : " + ex.Message); }
        }

        //FC
        private void showNotificationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                //fetch end point
                BacnetClient comm;
                BacnetAddress adr;
                BacnetObjectId object_id;

                if (GetObjectLink(out comm, out adr, out object_id, BacnetObjectTypes.OBJECT_NOTIFICATION_CLASS) == false) return;

                new NotificationEditor(comm, adr, object_id).ShowDialog();

            }
            catch (Exception ex) { Trace.TraceError("Error loading Notification : " + ex.Message); }
        }


        private void m_AddressSpaceTree_ItemDrag(object sender, ItemDragEventArgs e)
        {
            m_AddressSpaceTree.DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void m_SubscriptionView_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private string GetObjectName(BacnetClient comm, BacnetAddress adr, BacnetObjectId object_id)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                IList<BacnetValue> value;
                if (!comm.ReadPropertyRequest(adr, object_id, BacnetPropertyIds.PROP_OBJECT_NAME, out value))
                    return "[Timed out]";
                if (value == null || value.Count == 0)
                    return "";
                else
                    return value[0].Value.ToString();
            }
            catch (Exception ex)
            {
                return "[Error: " + ex.Message + " ]";
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private class Subscription
        {
            public BacnetClient comm;
            public BacnetAddress adr;
            public BacnetObjectId device_id, object_id;
            public string sub_key;
            public uint subscribe_id;
            public bool is_active_subscription = true; // false if subscription is refused

            public Subscription(BacnetClient comm, BacnetAddress adr, BacnetObjectId device_id, BacnetObjectId object_id, string sub_key, uint subscribe_id)
            {
                this.comm = comm;
                this.adr = adr;
                this.device_id = device_id;
                this.object_id = object_id;
                this.sub_key = sub_key;
                this.subscribe_id = subscribe_id;
            }
        }

        private string ShortenObjectId(string objectId)
        {
            string result = objectId;

            if(result.StartsWith("OBJECT_"))
            {
                result = result.Substring(7);
            }

            if(result.Contains("ANALOG_INPUT"))
            {
                result = result.Replace("ANALOG_INPUT", "AI");
            }
            if (result.Contains("ANALOG_OUTPUT"))
            {
                result = result.Replace("ANALOG_OUTPUT", "AO");
            }
            if (result.Contains("ANALOG_VALUE"))
            {
                result = result.Replace("ANALOG_VALUE", "AV");
            }
            if (result.Contains("BINARY_INPUT"))
            {
                result = result.Replace("BINARY_INPUT", "BI");
            }
            if (result.Contains("BINARY_OUTPUT"))
            {
                result = result.Replace("BINARY_OUTPUT", "BO");
            }
            if (result.Contains("BINARY_VALUE"))
            {
                result = result.Replace("BINARY_VALUE", "BV");
            }
            if (result.Contains("MULTI_STATE_INPUT"))
            {
                result = result.Replace("MULTI_STATE_INPUT", "MI");
            }
            if (result.Contains("MULTI_STATE_OUTPUT"))
            {
                result = result.Replace("MULTI_STATE_OUTPUT", "MO");
            }
            if (result.Contains("MULTI_STATE_VALUE"))
            {
                result = result.Replace("MULTI_STATE_VALUE", "MV");
            }

            return result;
        }

        private bool CreateSubscription(BacnetClient comm, BacnetAddress adr, uint device_id, BacnetObjectId object_id, bool WithGraph, int pollPeriod = -1)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                String CurveToolTip;
                //fetch device_id if needed
                if (device_id >= System.IO.BACnet.Serialize.ASN1.BACNET_MAX_INSTANCE)
                {
                    device_id = FetchDeviceId(comm, adr);
                }

                m_next_subscription_id++;
                string sub_key = adr.ToString() + ":" + device_id + ":" + m_next_subscription_id;
                Subscription sub = new Subscription(comm, adr, new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE, device_id), object_id, sub_key, m_next_subscription_id);

                string obj_id = object_id.ToString().Substring(7);
                obj_id = ShortenObjectId(obj_id);

                CurveToolTip = GetObjectName(comm, adr, object_id);

                DialogResult useCov;
                
                if (pollPeriod<0)
                {
                    /*useCov = MessageBox.Show(String.Format("Do you want to use COV notifications for {0}?", CurveToolTip), "COV Subscription", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    if (useCov == DialogResult.Cancel)
                    {
                        return false;
                    }*/

                    if(CovOpn.Checked)
                    {
                        useCov = DialogResult.Yes;
                    }
                    else
                    {
                        useCov = DialogResult.No;
                    }

                }
                else if(pollPeriod==0)
                {
                    useCov = DialogResult.Yes;
                }
                else
                {
                    useCov = DialogResult.No;
                }


                //add to list
                ListViewItem itm = m_SubscriptionView.Items.Add("");//device_id.ToString());
                // Always a blank on [0] to allow for the "Show" Column


                // device id is index [1]
                itm.SubItems.Add(device_id.ToString()); 
                itm.SubItems.Add(obj_id); // object id [2]
                itm.SubItems.Add(CurveToolTip);   //name [3]
                itm.SubItems.Add("");   //value [4]
                itm.SubItems.Add("");   //time [5]
                itm.SubItems.Add("Not started");   //status [6]
                if (Properties.Settings.Default.ShowDescriptionWhenUsefull)
                {
                    IList<BacnetValue> values;
                    if (comm.ReadPropertyRequest(adr, object_id, BacnetPropertyIds.PROP_DESCRIPTION, out values))
                    {
                        itm.SubItems.Add(values[0].Value.ToString());   // Description [7]
                        CurveToolTip = CurveToolTip + Environment.NewLine + values[0].Value.ToString();
                    }
                }
                else
                    itm.SubItems.Add(""); // Description [7]

                itm.SubItems.Add("");   // Graph Line Color [8]
                itm.SubItems.Add(WithGraph.ToString());   // With Graph? [9]
                itm.SubItems.Add("-1");   // COV or Polled with Period [10]
                itm.Tag = sub;
                lock (m_subscription_list)
                {
                    m_subscription_list.Add(sub_key, itm);
                    if (WithGraph)
                    {
                        itm.Checked = true;
                    }
                    RollingPointPairList points = new RollingPointPairList(10000);
                    m_subscription_points.Add(sub_key, points);
                    Color color= GraphColor[Pane.CurveList.Count%GraphColor.Length];
                    LineItem l = Pane.AddCurve("", points, color, Properties.Settings.Default.GraphDotStyle);
                    l.IsVisible = itm.Checked;
                    l.Tag = CurveToolTip; // store the Name to display it in the Tooltip
                    itm.SubItems[8].BackColor = color;
                    itm.UseItemStyleForSubItems = false;
                    CovGraph.Invalidate();
                    //}
                }

                //add to device

                bool SubscribeOK = false;

                if (useCov == DialogResult.Yes)
                {
                    try
                    {
                        SubscribeOK = comm.SubscribeCOVRequest(adr, object_id, m_next_subscription_id, false, Properties.Settings.Default.Subscriptions_IssueConfirmedNotifies, Properties.Settings.Default.Subscriptions_Lifetime);
                    }
                    catch { }
                }

                if (SubscribeOK == false) // echec : launch period acquisiton in the ThreadPool
                {
                    //double boxSize = 1.0;
                    string prompt = String.Empty;
                    if(useCov == DialogResult.No)
                    {
                        prompt = String.Format("Point will be polled - enter poll period in milliseconds.", CurveToolTip);
                        //boxSize = 2.0;
                    }
                    else
                    {
                        prompt = String.Format("Failed to subscribe to COV for {0}. Point will be polled instead - enter poll period in milliseconds.", CurveToolTip);
                        Trace.TraceWarning(String.Format("Failed to subscribe to COV for {0}. Point will be polled instead - enter poll period in milliseconds.", CurveToolTip));
                        //boxSize = 4.0;
                    }
                    sub.is_active_subscription = false;

                    //DialogResult rep;
                    //GenericInputBox<NumericUpDown> Qst = null;
                    int period = -1;
                    if (pollPeriod>0)
                    {
                        period = pollPeriod;
                    }
                    else
                    {
                        /*Qst = new GenericInputBox<NumericUpDown>("Polling period (ms)",prompt,
                              (o) =>
                              {
                                  o.Minimum = MIN_POLL_PERIOD; o.Maximum = MAX_POLL_PERIOD; o.Value = Math.Max(Math.Min(Properties.Settings.Default.Subscriptions_ReplacementPollingPeriod, MAX_POLL_PERIOD), MIN_POLL_PERIOD);
                              },
                              boxSize);

                        rep = Qst.ShowDialog();*/

                        period = (int)pollRateSelector.Value;
                        //Properties.Settings.Default.Subscriptions_ReplacementPollingPeriod = (uint)period;

                        /*if (rep == DialogResult.OK)
                        {
                            if (Qst != null) { period = (int)Qst.genericInput.Value; }
                            Properties.Settings.Default.Subscriptions_ReplacementPollingPeriod = (uint)period;

                        }
                        else
                        {
                            lock (m_subscription_list)
                            {
                                m_subscription_list.Remove(sub_key);
                                //remove from interface
                                m_SubscriptionView.Items.Remove(itm);
                                //if (WithGraph)
                                //{
                                try
                                {
                                    RollingPointPairList points = m_subscription_points[sub_key];
                                    foreach (LineItem l in Pane.CurveList)
                                        if (l.Points == points)
                                        {
                                            Pane.CurveList.Remove(l);
                                            break;
                                        }

                                    m_subscription_points.Remove(sub_key);
                                }
                                catch { }
                                //}
                            }

                            return false;
                        }*/
                    }

                    lock (m_subscription_list)
                    {
                        itm.SubItems[10].Text = period.ToString();
                    }

                    ThreadPool.QueueUserWorkItem(a => ReadPropertyPoolingRemplacementToCOV(sub, period));
                }
                else
                {
                    // COV - set period indicator to 0
                    lock (m_subscription_list)
                    {
                        itm.SubItems[10].Text = "0";
                    }
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                this.Cursor = Cursors.Default;                
            }

            return true;
        }

        // COV echec, PROP_PRESENT_VALUE read replacement method
        // x seconds poolling period
        private void ReadPropertyPoolingRemplacementToCOV(Subscription sub, int period)
        {
            int errorCount = 0;
            bool wasPaused = !_plotterPauseFlag;
            for (; ; )
            {
                IList<BacnetPropertyValue> presentValueValues = new List<BacnetPropertyValue>();
                IList<BacnetPropertyValue> statusFlagValues = new List<BacnetPropertyValue>();

                BacnetPropertyReference[] propertiesToPoll = new BacnetPropertyReference[] { new BacnetPropertyReference((uint)BacnetPropertyIds.PROP_PRESENT_VALUE, System.IO.BACnet.Serialize.ASN1.BACNET_ARRAY_ALL), new BacnetPropertyReference((uint)BacnetPropertyIds.PROP_STATUS_FLAGS, System.IO.BACnet.Serialize.ASN1.BACNET_ARRAY_ALL) };
                IList<BacnetReadAccessResult> multi_value_list;


                if (!sub.comm.ReadPropertyMultipleRequest(sub.adr, sub.object_id, propertiesToPoll, out multi_value_list))
                {

                    if (!ReadProperty(sub.comm, sub.adr, sub.object_id, BacnetPropertyIds.PROP_PRESENT_VALUE, ref presentValueValues))
                        return; // maybe here we could not go away
                    if (!ReadProperty(sub.comm, sub.adr, sub.object_id, BacnetPropertyIds.PROP_STATUS_FLAGS, ref statusFlagValues))
                        return; // maybe here we could not go away

                    List<BacnetPropertyValue> presentValueAndStatusFlagsValues = new List<BacnetPropertyValue>();
                    if (presentValueValues.Count > 0)
                    {
                        presentValueAndStatusFlagsValues.Add(presentValueValues[0]);
                    }
                    if (statusFlagValues.Count > 0)
                    {
                        presentValueAndStatusFlagsValues.Add(statusFlagValues[0]);
                    }

                    if(presentValueAndStatusFlagsValues.Count>0)
                    {
                        try
                        {
                            lock (m_subscription_list)
                            {
                                if (m_subscription_list.ContainsKey(sub.sub_key))
                                {
                                    OnCOVNotification(sub.comm, sub.adr, 0, sub.subscribe_id, sub.device_id, sub.object_id, 0, false, presentValueAndStatusFlagsValues, BacnetMaxSegments.MAX_SEG0);
                                    errorCount = 0;
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            errorCount++;
                            if (errorCount >= 4)
                            {
                                Trace.TraceError(String.Format("The Notify function (while polling of device {0}, object {1} using ReadProperty) failed - last error was {2} - {3}.", sub.device_id.instance.ToString(), sub.object_id.ToString(), ex.GetType().Name, ex.Message));
                                return;
                            }
                        }
                    }
                    

                }
                else
                {
                    try
                    {
                        lock (m_subscription_list)
                        {
                            if (m_subscription_list.ContainsKey(sub.sub_key))
                            {
                                OnCOVNotification(sub.comm, sub.adr, 0, sub.subscribe_id, sub.device_id, sub.object_id, 0, false, multi_value_list[0].values, BacnetMaxSegments.MAX_SEG0);
                                errorCount = 0;
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        errorCount++;
                        if(errorCount>=4)
                        {
                            Trace.TraceError(String.Format("The Notify function (while polling device {0}, object {1} using ReadPropertyMultiple) failed - last error was {2} - {3}.", sub.device_id.instance.ToString(), sub.object_id.ToString(), ex.GetType().Name, ex.Message ));
                            return;
                        }
                    }
                }

                Thread.Sleep(Math.Max(Math.Min(MAX_POLL_PERIOD, period), MIN_POLL_PERIOD));

                if(!_plotterPause.WaitOne(0))
                {
                    wasPaused = true;
                    _plotterPause.WaitOne();
                }

                if(wasPaused)
                {
                    Thread.Sleep(Math.Max(Math.Min(MAX_POLL_PERIOD, _rand.Next(0, 250)), MIN_POLL_PERIOD));
                }
            }
        }

        private void TogglePlotter()
        {

            if (_plotterPauseFlag)
            {
                _plotterPauseFlag = false;
                btnPlay.Text = PLAY_BUTTON_TEXT_WHEN_PAUSED;
                _plotterPause.Reset();
            }
            else
            {
                _plotterPauseFlag = true;
                btnPlay.Text = PLAY_BUTTON_TEXT_WHEN_RUNNING;
                _plotterPause.Set();
            }
        }

        private void ExportCovGraph()
        {
            StringBuilder sb = new StringBuilder();
            int count=0;
            foreach (KeyValuePair<string,ListViewItem> subscription in m_subscription_list)
            {
                // sub_key = adr.ToString() + ":" + device_id + ":" + m_next_subscription_id;
                bool hasGraph = false;
                if(!string.IsNullOrWhiteSpace(subscription.Value.SubItems[9].Text))
                {
                    bool graphBoolParsed;
                    if(bool.TryParse(subscription.Value.SubItems[9].Text, out graphBoolParsed))
                    {
                        hasGraph = graphBoolParsed;
                    }
                }

                sb.Append(hasGraph ? "P" : "T");
                sb.Append(';');

                string key = subscription.Key;
                string[] keyComponents = key.Split(':');
                if(keyComponents.Length!=4)
                {
                    continue;
                }
                sb.Append(keyComponents[2]);
                sb.Append(';');
                string value = string.Empty;
                try
                {
                    value = ((Subscription)subscription.Value.Tag).object_id.ToString();
                    //value = subscription.Value.SubItems[2].Text;
                }
                catch { continue; }
                if(value.Length==0 || !value.Contains(':'))
                {
                    continue;
                }
                sb.Append(value);
                sb.Append(';');
                sb.AppendLine(subscription.Value.SubItems[10].Text);
                count++;
            }
            if (count==0)
            {
                MessageBox.Show("No valid setup on COV graph to write to file.", "Write to file fail", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            string path = string.Empty;
            string fullPath = string.Empty;
            if (!String.IsNullOrWhiteSpace(Properties.Settings.Default.COV_Export_Path) && Properties.Settings.Default.COV_Export_Path.Length>0)
            {
                path = Path.GetDirectoryName(Properties.Settings.Default.COV_Export_Path);
                while(path.StartsWith("\\"))
                {
                    path = path.Substring(1);
                }
                if(!String.IsNullOrWhiteSpace(path) && !Directory.Exists(path))
                {
                    // Attempt to create
                    try
                    {
                        Directory.CreateDirectory(path);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(String.Format("Failed to create directory \"{0}\". {1} - {2}", path, e.GetType().ToString(), e.Message), "Write to file error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }
            DateTime now = DateTime.Now;
            string fileName = String.Format("COV_Graph_Setup_Export_{0:0000}-{1:00}-{2:00}_{3:00}.{4:00}.{5:00}.txt",
                    now.Year,             /* Year in which the file was created */
                    now.Month,            /* Month in which the file was created */
                    now.Day,              /* Day in which the file was created */
                    now.Hour,             /* Hour in which the file was created */
                    now.Minute,           /* Minute in which the file was created */
                    now.Second);          /* Second in which the file was created */

            if(string.IsNullOrWhiteSpace(path))
            {
                fullPath = fileName;
            }
            else
            {
                fullPath = Path.Combine(path, fileName);
            }

            try
            {
                File.WriteAllText(fullPath, sb.ToString());
            }
            catch(Exception e)
            {
                MessageBox.Show(String.Format("Failed to write COV graph setup data to file \"{0}\". {1} - {2}", fullPath, e.GetType().ToString(), e.Message), "Write to file error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show(String.Format("Wrote COV graph setup data to file \"{0}\".", fullPath), "Write to file success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void m_SubscriptionView_DragDrop(object sender, DragEventArgs e)
        {
            // Drop from the adress space
            if (e.Data.GetDataPresent("CodersLab.Windows.Controls.NodesCollection", false))
            {
                //fetch end point
                if (m_DeviceTree.SelectedNode == null) return;
                else if (m_DeviceTree.SelectedNode.Tag == null) return;
                else if (!(m_DeviceTree.SelectedNode.Tag is KeyValuePair<BacnetAddress, uint>)) return;
                KeyValuePair<BacnetAddress, uint> entry = (KeyValuePair<BacnetAddress, uint>)m_DeviceTree.SelectedNode.Tag;
                BacnetAddress adr = entry.Key;

                BacnetClient comm;
                if (m_DeviceTree.SelectedNode.Parent.Tag is BacnetClient)
                    comm = (BacnetClient)m_DeviceTree.SelectedNode.Parent.Tag;
                else  // a routed device
                    comm = (BacnetClient)m_DeviceTree.SelectedNode.Parent.Parent.Tag;

                //fetch object_id
                var nodes = (CodersLab.Windows.Controls.NodesCollection)e.Data.GetData("CodersLab.Windows.Controls.NodesCollection");
                //node[0]

                // Nodes are in a non controlable order, so puts the objectIds in order
                List<BacnetObjectId> Bobjs = new List<BacnetObjectId>();
                for (int i = 0; i < nodes.Count; i++)
                {
                    if ((nodes[i].Tag != null) && (nodes[i].Tag is BacnetObjectId))
                        Bobjs.Add((BacnetObjectId)nodes[i].Tag);
                }

                Bobjs.Sort();

                for (int i = 0; i < Bobjs.Count; i++)
                {
                    if (CreateSubscription(comm, adr, entry.Value, Bobjs[i], sender==CovGraph) == false)
                        break;
                }
            }

            // Drop a file deviceId;object:Id
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length != 1) return;
                try
                {
                    StreamReader sr = new StreamReader(files[0]);
                    while (!sr.EndOfStream)
                    {
                        string line=sr.ReadLine();
                        if ((line.Length > 0) && (line[0] != '#'))
                        {

                            string[] description = line.Split(';');
                            if (description.Length == 3)
                            {
                                try
                                {
                                    uint deviceId;
                                    deviceId = Convert.ToUInt32(description[1]);
                                    string objectIdString = description[2];
                                    if(!objectIdString.StartsWith("OBJECT_"))
                                    {
                                        objectIdString = "OBJECT_" + objectIdString;
                                    }
                                    BacnetObjectId objectId = BacnetObjectId.Parse(objectIdString);

                                    foreach (var E in m_devices)
                                    {
                                        var comm = E.Value.Devices;
                                        foreach (var deviceEntry in comm)
                                        {
                                            if (deviceEntry.Value == deviceId)
                                            {
                                                CreateSubscription(E.Key, deviceEntry.Key, deviceId, objectId, description[0].Equals("P",StringComparison.OrdinalIgnoreCase));
                                                break;
                                            }
                                        }

                                    }
                                }
                                catch { }

                            }
                            else if (description.Length == 4)
                            {
                                try
                                {
                                    uint deviceId;
                                    deviceId = Convert.ToUInt32(description[1]);
                                    string objectIdString = description[2];
                                    if (!objectIdString.StartsWith("OBJECT_"))
                                    {
                                        objectIdString = "OBJECT_" + objectIdString;
                                    }
                                    BacnetObjectId objectId = BacnetObjectId.Parse(objectIdString);
                                    int period = Int32.Parse(description[3]);
                                    foreach (var E in m_devices)
                                    {
                                        var comm = E.Value.Devices;
                                        foreach (var deviceEntry in comm)
                                        {
                                            if (deviceEntry.Value == deviceId)
                                            {
                                                CreateSubscription(E.Key, deviceEntry.Key, deviceId, objectId, description[0].Equals("P", StringComparison.OrdinalIgnoreCase), period);
                                                break;
                                            }
                                        }

                                    }
                                }
                                catch { }

                            }
                        }
                    }
                    sr.Close();

                }
                catch { }

            }
        }

        private void MainDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                //commit setup
                Properties.Settings.Default.GUI_SplitterButtom = m_SplitContainerButtom.SplitterDistance;
                Properties.Settings.Default.GUI_SplitterMiddle = m_SplitContainerLeft.SplitterDistance;
                Properties.Settings.Default.GUI_SplitterRight = m_SplitContainerRight.SplitterDistance;
                Properties.Settings.Default.GUI_SplitterLeft = splitContainer4.SplitterDistance;
                Properties.Settings.Default.GUI_FormSize = this.Size;
                Properties.Settings.Default.GUI_FormState = this.WindowState.ToString();

                StringBuilder s=new StringBuilder();
                for (int i = 0; i < m_SubscriptionView.Columns.Count;i++)
                    s.Append(m_SubscriptionView.Columns[i].DisplayIndex.ToString() + ";" + m_SubscriptionView.Columns[i].Width.ToString() + ";");
                s.Remove(s.Length-1,1);

                Properties.Settings.Default.GUI_SubscriptionColumns=s.ToString();

                //save
                Properties.Settings.Default.Save();

                // save object name<->id file
                DoSaveObjectNamesIfNecessary();

            }
            catch
            {
                //ignore
            }
        }

        private void m_SubscriptionView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete) return;

            if (m_SubscriptionView.SelectedItems.Count >= 1)
            {
                foreach (ListViewItem itm in m_SubscriptionView.SelectedItems)
                {
                    //ListViewItem itm = m_SubscriptionView.SelectedItems[0];
                    if (itm.Tag is Subscription)    // It's a subscription or not (Event/Alarm)
                    {
                        Subscription sub = (Subscription)itm.Tag;
                        if (m_subscription_list.ContainsKey(sub.sub_key))
                        {
                            //remove from device
                            try
                            {
                                if (sub.is_active_subscription)
                                    if (!sub.comm.SubscribeCOVRequest(sub.adr, sub.object_id, sub.subscribe_id, true, false, 0))
                                    {
                                        MessageBox.Show(this, "Couldn't unsubscribe", "Communication Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                        return;
                                    }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(this, "Couldn't delete subscription: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }
                        //remove from interface
                        m_SubscriptionView.Items.Remove(itm);
                        lock (m_subscription_list)
                        {
                            m_subscription_list.Remove(sub.sub_key);
                            try
                            {
                                RollingPointPairList points = m_subscription_points[sub.sub_key];
                                foreach (LineItem l in Pane.CurveList)
                                    if (l.Points == points)
                                    {
                                        Pane.CurveList.Remove(l);
                                        break;
                                    }

                                m_subscription_points.Remove(sub.sub_key);
                            }
                            catch { }
                        }

                        CovGraph.AxisChange();
                        CovGraph.Invalidate();
                        //m_SubscriptionView.Items.Remove(itm);
                    }

                }
            }
        }

        private void sendWhoIsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                BacnetClient comm = (BacnetClient)m_DeviceTree.SelectedNode.Tag;
                comm.WhoIs();
            }
            catch
            {
                MessageBox.Show(this, "Please select a \"transport\" node first", "Wrong node", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void AddRemoteIpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //fetch end point
            BacnetClient comm = null;
            try
            {
                if (m_DeviceTree.SelectedNode == null) return;
                else if (m_DeviceTree.SelectedNode.Tag == null) return;
                else if (!(m_DeviceTree.SelectedNode.Tag is BacnetClient)) return;
                comm = (BacnetClient)m_DeviceTree.SelectedNode.Tag;

                if (comm.Transport is BacnetIpUdpProtocolTransport) // only IPv4 today, v6 maybe a day
                {

                    var Input =
                        new GenericInputBox<TextBox>("Ipv4/Udp Bacnet Node", "DeviceId - xx.xx.xx.xx:47808",
                          (o) =>
                          {
                              // adjustment to the generic control
                          }, 1, true, "Unknown device Id can be replaced by 4194303 or ?");
                    DialogResult res = Input.ShowDialog();

                    if (res == DialogResult.OK)
                    {
                        string[] entry=Input.genericInput.Text.Split('-');
                        if (entry[0][0] == '?') entry[0] = "4194303";
                        OnIam(comm, new BacnetAddress(BacnetAddressTypes.IP, entry[1].Trim()), Convert.ToUInt32(entry[0]), 0, BacnetSegmentations.SEGMENTATION_NONE, 0);
                    }
                }
                else
                {
                    MessageBox.Show(this, "Please select an \"IPv4 transport\" node first", "Wrong node", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch 
            {
                MessageBox.Show(this, "Invalid parameter", "Wrong node or IP @", MessageBoxButtons.OK, MessageBoxIcon.Information);          
            }
        }

        private void helpToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string readme_path = Path.GetDirectoryName(Application.ExecutablePath)+"/README.txt";
            System.Diagnostics.Process.Start(readme_path);
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool prevVertOrientation = Properties.Settings.Default.Vertical_Object_Splitter_Orientation;

            SettingsDialog dlg = new SettingsDialog();
            dlg.SelectedObject = Properties.Settings.Default;
            dlg.ShowDialog(this);

            bool changedOrientation = prevVertOrientation ^ Properties.Settings.Default.Vertical_Object_Splitter_Orientation;

            if(changedOrientation)
            {
                if (Properties.Settings.Default.Vertical_Object_Splitter_Orientation)
                {
                    splitContainer4.Orientation = Orientation.Vertical;
                    Properties.Settings.Default.GUI_SplitterLeft = (int)(m_SplitContainerLeft.SplitterDistance * 0.45f);
                }
                else
                {
                    splitContainer4.Orientation = Orientation.Horizontal;
                    Properties.Settings.Default.GUI_SplitterLeft = m_SplitContainerButtom.SplitterDistance / 2;
                }
                splitContainer4.SplitterDistance = Properties.Settings.Default.GUI_SplitterLeft;

            }

        }

        /// <summary>
        /// This will download all values from a given device and store it in a xml format, fit for the DemoServer
        /// This can be a good way to test serializing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exportDeviceDBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //fetch end point
            BacnetClient comm = null;
            BacnetAddress adr;
            uint device_id;

            FetchEndPoint(out comm, out adr, out device_id);

            if (comm == null)
            {
                MessageBox.Show(this, "Please select a device node", "Wrong node", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
  

            //select file to store
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "xml|*.xml";
            if (dlg.ShowDialog(this) != System.Windows.Forms.DialogResult.OK) return;

            this.Cursor = Cursors.WaitCursor;
            Application.DoEvents();

            bool removeObject = false;

            try
            {
                //get all objects
                System.IO.BACnet.Storage.DeviceStorage storage = new System.IO.BACnet.Storage.DeviceStorage();
                IList<BacnetValue> value_list;
                comm.ReadPropertyRequest(adr, new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE, device_id), BacnetPropertyIds.PROP_OBJECT_LIST, out value_list);
                LinkedList<BacnetObjectId> object_list = new LinkedList<BacnetObjectId>();
                foreach (BacnetValue value in value_list)
                {
                    if (Enum.IsDefined(typeof(BacnetObjectTypes), ((BacnetObjectId)value.Value).Type))
                        object_list.AddLast((BacnetObjectId)value.Value);
                    else
                        removeObject = true;
                }

                foreach (BacnetObjectId object_id in object_list)
                {
                    //read all properties
                    IList<BacnetReadAccessResult> multi_value_list;
                    BacnetPropertyReference[] properties = new BacnetPropertyReference[] { new BacnetPropertyReference((uint)BacnetPropertyIds.PROP_ALL, System.IO.BACnet.Serialize.ASN1.BACNET_ARRAY_ALL) };
                    comm.ReadPropertyMultipleRequest(adr, object_id, properties, out multi_value_list);

                    //store
                    foreach (BacnetPropertyValue value in multi_value_list[0].values)
                    {
                        try
                        {
                            storage.WriteProperty(object_id, (BacnetPropertyIds)value.property.propertyIdentifier, value.property.propertyArrayIndex, value.value, true);
                        }
                        catch { }
                    }
                }

                //save to disk
                storage.Save(dlg.FileName);

                //display
                MessageBox.Show(this, "Done", "Export done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Error during export: " + ex.Message, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                if (removeObject == true)
                    Trace.TraceWarning("All proprietary Objects removed from export");
            }
        }

        private uint FetchDeviceId(BacnetClient comm, BacnetAddress adr)
        {
            IList<BacnetValue> value;
            if (comm.ReadPropertyRequest(adr, new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE, System.IO.BACnet.Serialize.ASN1.BACNET_MAX_INSTANCE), BacnetPropertyIds.PROP_OBJECT_IDENTIFIER, out value))
            {
                if (value != null && value.Count > 0 && value[0].Value is BacnetObjectId)
                {
                    BacnetObjectId object_id = (BacnetObjectId)value[0].Value;
                    return object_id.instance;
                }
                else
                    return 0xFFFFFFFF;
            }
            else
                return 0xFFFFFFFF;
        }

        private void subscribeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //fetch end point
            if (m_DeviceTree.SelectedNode == null) return;
            else if (m_DeviceTree.SelectedNode.Tag == null) return;
            else if (!(m_DeviceTree.SelectedNode.Tag is KeyValuePair<BacnetAddress, uint>)) return;
            KeyValuePair<BacnetAddress, uint> entry = (KeyValuePair<BacnetAddress, uint>)m_DeviceTree.SelectedNode.Tag;
            BacnetAddress adr = entry.Key;
            BacnetClient comm;
            if (m_DeviceTree.SelectedNode.Parent.Tag is BacnetClient)
                comm = (BacnetClient)m_DeviceTree.SelectedNode.Parent.Tag;
            else
                comm = (BacnetClient)m_DeviceTree.SelectedNode.Parent.Parent.Tag; // When device is under a Router
            uint device_id = entry.Value;

            //test object_id with the last selected node
            if (
                m_AddressSpaceTree.SelectedNode == null ||
                !(m_AddressSpaceTree.SelectedNode.Tag is BacnetObjectId))
            {
                MessageBox.Show(this, "The marked object is not an object", "Not an object", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            // advise all selected nodes, stop at the first COV reject (even if a period polling is done)
            foreach (TreeNode t in m_AddressSpaceTree.SelectedNodes)
            {
                BacnetObjectId object_id = (BacnetObjectId)t.Tag;
                //create 
                if (CreateSubscription(comm, adr, device_id, object_id, false) == false)
                    return;
            }
        }

        private void m_subscriptionRenewTimer_Tick(object sender, EventArgs e)
        {
            // don't want to lock the list for a while
            // so get element one by one using the indexer            
            int ItmCount;
            lock (m_subscription_list)
                ItmCount = m_subscription_list.Count;

            for (int i = 0; i < ItmCount; i++)
            {
                ListViewItem itm = null;

                // lock another time the list to get the item by indexer
                try
                {
                    lock (m_subscription_list)
                        itm = m_subscription_list.Values.ElementAt(i);
                }
                catch { }

                if (itm != null)
                {
                    try
                    {
                        Subscription sub = (Subscription)itm.Tag;

                        if (sub.is_active_subscription == false) // not needs to renew, periodic pooling in operation (or nothing) due to COV subscription refused by the remote device
                            return;

                        if (!sub.comm.SubscribeCOVRequest(sub.adr, sub.object_id, sub.subscribe_id, false, Properties.Settings.Default.Subscriptions_IssueConfirmedNotifies, Properties.Settings.Default.Subscriptions_Lifetime))
                        {
                            SetSubscriptionStatus(itm, "Offline");
                            Trace.TraceWarning("Couldn't renew subscription " + sub.subscribe_id);
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("Exception during renew subscription: " + ex.Message);
                    }
                }
            }
        }

        private void sendWhoIsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            sendWhoIsToolStripMenuItem_Click(this, null);
        }

        private void exportDeviceDBToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            exportDeviceDBToolStripMenuItem_Click(this, null);
        }

        private void downloadFileToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            downloadFileToolStripMenuItem_Click(this, null);
        }

        private void showTrendLogToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            showTrendLogToolStripMenuItem_Click(null, null);
        }

        private void showScheduleToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            showScheduleToolStripMenuItem_Click(null, null);
        }

        private void showCalendarToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            showCalendarToolStripMenuItem_Click(null, null);
        }

        private void showNotificationToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            showNotificationToolStripMenuItem_Click(null, null);
        }
        private void uploadFileToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            uploadFileToolStripMenuItem_Click(this, null);
        }

        private void subscribeToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            subscribeToolStripMenuItem_Click(this, null);
        }

        private void timeSynchronizeToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            timeSynchronizeToolStripMenuItem_Click(this, null);
        }

        // retreive the BacnetClient, BacnetAddress, device id of the selected node
        private void FetchEndPoint(out BacnetClient comm, out BacnetAddress adr, out uint device_id)
        {
            comm = null; adr = null; device_id = 0;
            try
            {
                if (m_DeviceTree.SelectedNode == null) return;
                else if (m_DeviceTree.SelectedNode.Tag == null) return;
                else if (!(m_DeviceTree.SelectedNode.Tag is KeyValuePair<BacnetAddress, uint>)) return;
                KeyValuePair<BacnetAddress, uint> entry = (KeyValuePair<BacnetAddress, uint>)m_DeviceTree.SelectedNode.Tag;
                adr = entry.Key;
                device_id = entry.Value;
                if (m_DeviceTree.SelectedNode.Parent.Tag is BacnetClient)
                    comm = (BacnetClient)m_DeviceTree.SelectedNode.Parent.Tag;
                else
                    comm = (BacnetClient)m_DeviceTree.SelectedNode.Parent.Parent.Tag; // When device is under a Router
            }
            catch
            {
   
            }
        }

        private void timeSynchronizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //fetch end point
            BacnetClient comm = null;
            BacnetAddress adr;
            uint device_id;

            FetchEndPoint(out comm, out adr, out device_id);

            if (comm == null)
            {
                MessageBox.Show(this, "Please select a device node", "Wrong node", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //send
            if(Properties.Settings.Default.TimeSynchronize_UTC)
                comm.SynchronizeTime(adr, DateTime.Now.ToUniversalTime(), true);
            else
                comm.SynchronizeTime(adr, DateTime.Now, false);

            //done
            MessageBox.Show(this, "OK", "Time Synchronize", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void communicationControlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //fetch end point
            BacnetClient comm = null;
            BacnetAddress adr;
            uint device_id;

            FetchEndPoint(out comm, out adr, out device_id);

            if (comm == null)
            {
                MessageBox.Show(this, "Please select a device node", "Wrong node", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //Options
            DeviceCommunicationControlDialog dlg = new DeviceCommunicationControlDialog();
            if (dlg.ShowDialog(this) != System.Windows.Forms.DialogResult.OK) return;

            if (dlg.IsReinitialize)
            {
                //Reinitialize Device
                if (!comm.ReinitializeRequest(adr, dlg.ReinitializeState, dlg.Password))
                    MessageBox.Show(this, "Couldn't perform device communication control", "Device Communication Control", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show(this, "OK", "Device Communication Control", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                //Device Communication Control
                if (!comm.DeviceCommunicationControlRequest(adr, dlg.Duration, dlg.DisableCommunication ? (uint)1 : (uint)0, dlg.Password))
                    MessageBox.Show(this, "Couldn't perform device communication control", "Device Communication Control", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show(this, "OK", "Device Communication Control", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void communicationControlToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            communicationControlToolStripMenuItem_Click(this, null);
        }

        // Base on https://www.big-eu.org/s/big_ede_2_3.zip
        // This will download all values from a given device and store it in 2 csv files : EDE and StateText (for Binary and Multistate objects)
        // Ede files for Units and ObjectTypes are common when all values are coming from the standard
        private void exportDeviceEDEFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //fetch end point
            BacnetClient comm = null;
            BacnetAddress adr;
            uint device_id;

            FetchEndPoint(out comm, out adr, out device_id);

            if (comm == null)
            {
                MessageBox.Show(this, "Please select a device node", "Wrong node", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //select file to store
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "csv|*.csv";
            if (dlg.ShowDialog(this) != System.Windows.Forms.DialogResult.OK) return;

            this.Cursor = Cursors.WaitCursor;
            Application.DoEvents();
            try
            {

                int StateTextCount = 0;

                // Read 6 properties even if not existing in the given object
                BacnetPropertyReference[] propertiesWithText = new BacnetPropertyReference[6] 
                                                                    {   
                                                                        new BacnetPropertyReference((uint)BacnetPropertyIds.PROP_OBJECT_NAME, System.IO.BACnet.Serialize.ASN1.BACNET_ARRAY_ALL), 
                                                                        new BacnetPropertyReference((uint)BacnetPropertyIds.PROP_DESCRIPTION, System.IO.BACnet.Serialize.ASN1.BACNET_ARRAY_ALL),
                                                                        new BacnetPropertyReference((uint)BacnetPropertyIds.PROP_UNITS, System.IO.BACnet.Serialize.ASN1.BACNET_ARRAY_ALL),
                                                                        new BacnetPropertyReference((uint)BacnetPropertyIds.PROP_STATE_TEXT, System.IO.BACnet.Serialize.ASN1.BACNET_ARRAY_ALL),
                                                                        new BacnetPropertyReference((uint)BacnetPropertyIds.PROP_INACTIVE_TEXT, System.IO.BACnet.Serialize.ASN1.BACNET_ARRAY_ALL),
                                                                        new BacnetPropertyReference((uint)BacnetPropertyIds.PROP_ACTIVE_TEXT, System.IO.BACnet.Serialize.ASN1.BACNET_ARRAY_ALL),
                                                                    };

                String FileName = dlg.FileName.Remove(dlg.FileName.Length - 4, 4);

                StreamWriter Sw_EDE = new StreamWriter(FileName + "_EDE.csv");
                StreamWriter Sw_StateText = new StreamWriter(FileName + "_StateTexts.csv");

                Sw_EDE.WriteLine("#Engineering-Data-Exchange - B.I.G.-EU");
                Sw_EDE.WriteLine("PROJECT_NAME");
                Sw_EDE.WriteLine("VERSION_OF_REFERENCEFILE");
                Sw_EDE.WriteLine("TIMESTAMP_OF_LAST_CHANGE;" + DateTime.Now.ToShortDateString());
                Sw_EDE.WriteLine("AUTHOR_OF_LAST_CHANGE;YABE Yet Another Bacnet Explorer");
                Sw_EDE.WriteLine("VERSION_OF_LAYOUT;2.3");
                Sw_EDE.WriteLine("#mandatory;mandator;mandatory;mandatory;mandatory;optional;optional;optional;optional;optional;optional;optional;optional;optional;optional;optional");
                Sw_EDE.WriteLine("# keyname;device obj.-instance;object-name;object-type;object-instance;description;present-value-default;min-present-value;max-present-value;settable;supports COV;hi-limit;low-limit;state-text-reference;unit-code;vendor-specific-addres");

                Sw_StateText.WriteLine("#State Text Reference");
                // Some colums, certainly enough. User need to add it manualy in the csv file if it's to few.
                Sw_StateText.WriteLine("#Reference Number;Text 1 or Inactive-Text;Text 2 or Active-Text;Text 3;Text 4;Text 5;Text 6;Text 7;Text 8;Text 9;Text 10;Text 11;Text 12;Text 13");

                bool ReadPropertyMultipleSupported = true; // For the first request assume it's OK

                // Object list is already in the AddressSpaceTree, so no need to query it again
                foreach (TreeNode t in m_AddressSpaceTree.Nodes)
                {
                    BacnetObjectId Bacobj = (BacnetObjectId)t.Tag;
                    string Identifier = "";
                    string Description = "";
                    String UnitCode = "";
                    String InactiveText = "";
                    String ActiveText = "";
   
                    IList<BacnetValue> State_Text = null;

                    if (ReadPropertyMultipleSupported) 
                    {
                        try
                        {
  
                            IList<BacnetReadAccessResult> multi_value_list;
                            BacnetReadAccessSpecification[] propToRead = new BacnetReadAccessSpecification[] { new BacnetReadAccessSpecification(Bacobj, propertiesWithText) };
                            comm.ReadPropertyMultipleRequest(adr, propToRead, out multi_value_list);
                            BacnetReadAccessResult br = multi_value_list[0];

                            foreach (BacnetPropertyValue pv in br.values)
                            {

                                if ((BacnetPropertyIds)pv.property.propertyIdentifier == BacnetPropertyIds.PROP_OBJECT_NAME)
                                    Identifier = pv.value[0].Value.ToString();
                                if ((BacnetPropertyIds)pv.property.propertyIdentifier == BacnetPropertyIds.PROP_DESCRIPTION)
                                    if (!(pv.value[0].Value is BacnetError))
                                        Description = pv.value[0].Value.ToString();
                                if ((BacnetPropertyIds)pv.property.propertyIdentifier == BacnetPropertyIds.PROP_UNITS)
                                    if (!(pv.value[0].Value is BacnetError))
                                        UnitCode = pv.value[0].Value.ToString();
                                if ((BacnetPropertyIds)pv.property.propertyIdentifier == BacnetPropertyIds.PROP_STATE_TEXT)
                                    if (!(pv.value[0].Value is BacnetError))
                                        State_Text = pv.value;
                                if ((BacnetPropertyIds)pv.property.propertyIdentifier == BacnetPropertyIds.PROP_INACTIVE_TEXT)
                                    if (!(pv.value[0].Value is BacnetError))
                                        InactiveText = pv.value[0].Value.ToString();
                                if ((BacnetPropertyIds)pv.property.propertyIdentifier == BacnetPropertyIds.PROP_ACTIVE_TEXT)
                                    if (!(pv.value[0].Value is BacnetError))
                                        ActiveText = pv.value[0].Value.ToString();
                            }
                        }
                        catch 
                        { 
                            ReadPropertyMultipleSupported = false; // assume the error is due to that 
                        }
                    }
                    if (!ReadPropertyMultipleSupported)
                    {
                        IList<BacnetValue> out_value;

                        bool Prop_Object_NameOK = false;
                        // Maybe we already have the name
                        lock (DevicesObjectsName)
                            Prop_Object_NameOK = DevicesObjectsName.TryGetValue(new Tuple<String, BacnetObjectId>(adr.FullHashString(), Bacobj), out Identifier);

                        if (!Prop_Object_NameOK)
                        {
                            comm.ReadPropertyRequest(adr, Bacobj, BacnetPropertyIds.PROP_OBJECT_NAME, out out_value);
                            Identifier = out_value[0].Value.ToString();
                        }

                        try
                        {
                            comm.ReadPropertyRequest(adr, Bacobj, BacnetPropertyIds.PROP_DESCRIPTION, out out_value);
                            if (!(out_value[0].Value is BacnetError))
                                Description = out_value[0].Value.ToString();

                            // OBJECT_MULTI_STATE_INPUT, OBJECT_MULTI_STATE_OUTPUT, OBJECT_MULTI_STATE_VALUE
                            if ((Bacobj.type >= BacnetObjectTypes.OBJECT_MULTI_STATE_INPUT) && (Bacobj.type <= BacnetObjectTypes.OBJECT_MULTI_STATE_INPUT+2))
                            {
                                comm.ReadPropertyRequest(adr, Bacobj, BacnetPropertyIds.PROP_STATE_TEXT, out State_Text);
                                if (State_Text[0].Value is BacnetError) State_Text = null;
                            }

                            // OBJECT_BINARY_INPUT, OBJECT_BINARY_OUTPUT, OBJECT_BINARY_VALUE
                            if ((Bacobj.type >= BacnetObjectTypes.OBJECT_BINARY_INPUT) && (Bacobj.type <= BacnetObjectTypes.OBJECT_BINARY_INPUT+2))
                            {
                                comm.ReadPropertyRequest(adr, Bacobj, BacnetPropertyIds.PROP_INACTIVE_TEXT, out out_value);
                                if (!(out_value[0].Value is BacnetError))
                                    InactiveText = out_value[0].Value.ToString();
                                comm.ReadPropertyRequest(adr, Bacobj, BacnetPropertyIds.PROP_ACTIVE_TEXT, out out_value);
                                if (!(out_value[0].Value is BacnetError))
                                    ActiveText = out_value[0].Value.ToString();
                            }
                        }
                        catch { }
                    }

                    if ((State_Text == null)&&(InactiveText==""))
                        Sw_EDE.WriteLine(Bacobj.ToString() + ";" + device_id.ToString() + ";" + Identifier + ";" + ((int)Bacobj.type).ToString() + ";" + Bacobj.instance.ToString() + ";" + Description + ";;;;;;;;;" + UnitCode);
                    else
                    {
                        Sw_EDE.WriteLine(Bacobj.ToString() + ";" + device_id.ToString() + ";" + Identifier + ";" + ((int)Bacobj.type).ToString() + ";" + Bacobj.instance.ToString() + ";" + Description + ";;;;;;;;" + StateTextCount + ";" + UnitCode);

                        Sw_StateText.Write(StateTextCount++);

                        if (State_Text!=null)
                            foreach (var v in State_Text)
                                Sw_StateText.Write(";" + v.Value.ToString());
                        else
                            Sw_StateText.Write(";"+InactiveText+";"+ActiveText);

                        Sw_StateText.WriteLine();
                    }
                    // Update also the Dictionary of known object name and the threenode
                    if (t.ToolTipText == "")
                    {
                        lock (DevicesObjectsName)
                        {
                            DevicesObjectsName.Add(new Tuple<String, BacnetObjectId>(adr.FullHashString(), Bacobj), Identifier);
                            objectNamesChangedFlag = true;

                            Tuple<string,BacnetObjectId> adrHash = new Tuple<string, BacnetObjectId>(adr.FullHashString(), Bacobj);

                            if (DevicesObjectsName.ContainsKey(adrHash))
                            {
                                if (!DevicesObjectsName[adrHash].Equals(Identifier.ToString()))
                                {
                                    DevicesObjectsName.Remove(adrHash);
                                    DevicesObjectsName.Add(adrHash, Identifier.ToString());
                                    objectNamesChangedFlag = true;
                                }
                            }
                            else
                            {
                                DevicesObjectsName.Add(adrHash, Identifier.ToString());
                                objectNamesChangedFlag = true;
                            }

                        }

                        ChangeTreeNodePropertyName(t, Identifier);
                    }
                }

                Sw_EDE.Close();
                Sw_StateText.Close();

                //display
                MessageBox.Show(this, "Done", "Export done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Error during export: " + ex.Message, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void foreignDeviceRegistrationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //fetch end point
            BacnetClient comm = null;
            try
            {
                if (m_DeviceTree.SelectedNode == null) return;
                else if (m_DeviceTree.SelectedNode.Tag == null) return;
                else if (!(m_DeviceTree.SelectedNode.Tag is BacnetClient)) return;
                comm = (BacnetClient)m_DeviceTree.SelectedNode.Tag;
            }
            finally
            {

                if (comm == null) MessageBox.Show(this, "Please select an \"IP transport\" node first", "Wrong node", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            Form F = new ForeignRegistry(comm);
            F.ShowDialog();
        }

        private void alarmSummaryToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            alarmSummaryToolStripMenuItem_Click(sender, e);
        }

        private void alarmSummaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //fetch end point
            BacnetClient comm = null;
            BacnetAddress adr;
            uint device_id;

            FetchEndPoint(out comm, out adr, out device_id);

            if (comm == null)
            {
                MessageBox.Show(this, "Please select a device node", "Wrong node", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            new AlarmSummary(m_AddressSpaceTree.ImageList, comm, adr, device_id, DevicesObjectsName).ShowDialog();
        }

        // Read the Adress Space, and change all object Id by name
        // Popup ToolTipText Get Properties Name
        private void readPropertiesNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //fetch end point
            BacnetClient comm = null;
            BacnetAddress adr;
            uint device_id;

            FetchEndPoint(out comm, out adr, out device_id);

            if (comm == null)
            {
                MessageBox.Show(this, "Please select a device node", "Wrong node", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
           
            // Go
            ChangeObjectIdByName(m_AddressSpaceTree.Nodes, comm, adr);

        }

        // In the Objects TreeNode, get all elements without the Bacnet PROP_OBJECT_NAME not Read out
        private void GetRequiredObjectName(TreeNodeCollection tnc, List<BacnetReadAccessSpecification> bras)
        {
            foreach (TreeNode tn in tnc)
            {
                if ((tn.ToolTipText == "")&&(tn.Tag!=null))
                {
                    if (!bras.Exists(o => o.objectIdentifier.Equals((BacnetObjectId)tn.Tag)))
                        bras.Add(new BacnetReadAccessSpecification((BacnetObjectId)tn.Tag, new BacnetPropertyReference[] { new BacnetPropertyReference((uint)BacnetPropertyIds.PROP_OBJECT_NAME, System.IO.BACnet.Serialize.ASN1.BACNET_ARRAY_ALL) }));
                }
                if (tn.Nodes != null)
                    GetRequiredObjectName(tn.Nodes, bras);
            }
        }
        // In the Objects TreeNode, set all elements with the ReadPropertyMultiple response
        private void SetObjectName(TreeNodeCollection tnc, IList<BacnetReadAccessResult> result, BacnetAddress adr)
        {
            foreach (TreeNode tn in tnc)
            {
                BacnetObjectId b=(BacnetObjectId)tn.Tag;

                try
                {
                    if (tn.ToolTipText == "")
                    {
                        BacnetReadAccessResult r = result.Single(o => o.objectIdentifier.Equals(b));
                        ChangeTreeNodePropertyName(tn, r.values[0].value[0].ToString());
                        lock (DevicesObjectsName)
                        {
                            var t = new Tuple<String, BacnetObjectId>(adr.FullHashString(), (BacnetObjectId)tn.Tag);
                            DevicesObjectsName.Remove(t); // sometimes the same object appears at several place (in Groups for instance).
                            DevicesObjectsName.Add(t, r.values[0].value[0].ToString());
                            objectNamesChangedFlag = true;
                        }
                    }
                }
                catch { }

                if (tn.Nodes != null)
                    SetObjectName(tn.Nodes, result, adr);
            }

        }
        // Try a ReadPropertyMultiple for all PROP_OBJECT_NAME not already known
        private void ChangeObjectIdByName(TreeNodeCollection tnc, BacnetClient comm, BacnetAddress adr)
        {
            int _retries = comm.Retries;
            comm.Retries = 1;
            bool IsOK = false;

            List<BacnetReadAccessSpecification> bras = new List<BacnetReadAccessSpecification>();
            GetRequiredObjectName(tnc, bras);

            if (bras.Count==0)
                IsOK = true;
            else
                try
                {
                    IList<BacnetReadAccessResult> result = null;
                    if (comm.ReadPropertyMultipleRequest(adr, bras, out result) == true)
                    {
                        SetObjectName(tnc, result, adr);
                        IsOK = true;
                    }
                }
                catch{}

            // Fail, so go One by One, in a background thread
            if (!IsOK)
                System.Threading.ThreadPool.QueueUserWorkItem((o) =>
                {
                    ChangeObjectIdByNameOneByOne(m_AddressSpaceTree.Nodes, comm, adr, AsynchRequestId);
                });  

            comm.Retries = _retries;
        }

        private void ChangeObjectIdByNameOneByOne(TreeNodeCollection tnc, BacnetClient comm, BacnetAddress adr, int AsynchRequestId)
        {
            int _retries = comm.Retries;
            comm.Retries = 1;

            foreach (TreeNode tn in tnc)
            {
                if ((tn.ToolTipText == "")&&(tn.Tag!=null))
                {
                    IList<BacnetValue> name;
                    try
                    {
                        if (comm.ReadPropertyRequest(adr, (BacnetObjectId)tn.Tag, BacnetPropertyIds.PROP_OBJECT_NAME, out name) == true)
                        {
                            if (AsynchRequestId != this.AsynchRequestId) // Selected device is no more the good one
                            {
                                comm.Retries = _retries;
                                return;
                            }

                            this.Invoke((MethodInvoker)delegate
                            {
                                if (AsynchRequestId != this.AsynchRequestId) return; // another test in the GUI thread

                                ChangeTreeNodePropertyName(tn, name[0].Value.ToString());

                                lock (DevicesObjectsName)
                                {
                                    var t = new Tuple<String, BacnetObjectId>(adr.FullHashString(), (BacnetObjectId)tn.Tag);
                                    DevicesObjectsName.Remove(t); // sometimes the same object appears at several place (in Groups for instance).
                                    DevicesObjectsName.Add(t, name[0].Value.ToString());
                                    objectNamesChangedFlag = true;
                                }
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceWarning("Failed to obtain object name for object " + tn.Tag + ": " + ex);
                    }
                }

                if (tn.Nodes != null)
                    ChangeObjectIdByNameOneByOne(tn.Nodes, comm, adr, AsynchRequestId);

                comm.Retries = _retries;
            }
        }

        // Open a serialized Dictionnay of object id <-> object name file
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //which file to upload?
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = Path.GetDirectoryName(Properties.Settings.Default.Auto_Store_Object_Names_File);
            dlg.DefaultExt = "YabeMap";
            dlg.Filter = "Yabe Map files (*.YabeMap)|*.YabeMap|All files (*.*)|*.*";
            if (dlg.ShowDialog(this) != System.Windows.Forms.DialogResult.OK) return;
            string filename = dlg.FileName;
            

            try
            {
                Stream stream = File.Open(filename, FileMode.Open);
                BinaryFormatter bf = new BinaryFormatter();
                var d = (Dictionary<Tuple<String, BacnetObjectId>, String>)bf.Deserialize(stream);
                stream.Close();

                if (d != null)
                {
                    DevicesObjectsName = d;
                    objectNamesChangedFlag = true;
                    Trace.TraceInformation("Loaded object names from \"" + filename + "\".");
                }
            }
            catch
            {
                MessageBox.Show(this, "File error", "Wrong file", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // save a serialized Dictionnay of object id <-> object name file
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.InitialDirectory = Path.GetDirectoryName(Properties.Settings.Default.Auto_Store_Object_Names_File);
            dlg.DefaultExt = "YabeMap";
            dlg.Filter = "Yabe Map files (*.YabeMap)|*.YabeMap|All files (*.*)|*.*";
            if (dlg.ShowDialog(this) != System.Windows.Forms.DialogResult.OK) return;
            string filename = dlg.FileName;
            try
            {
                Stream stream = File.Open(filename, FileMode.Create);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(stream, DevicesObjectsName);
                stream.Close();
                Trace.TraceInformation("Saved object names to \"" + filename + "\".");
            }
            catch
            {
                MessageBox.Show(this, "File error", "Wrong file", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }

        private void createObjectToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            createObjectToolStripMenuItem_Click(sender, e);
        }
        private void createObjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //fetch end point
            BacnetClient comm = null;
            BacnetAddress adr;
            uint device_id;

            FetchEndPoint(out comm, out adr, out device_id);

            if (comm == null)
                return;

            CreateObject F = new CreateObject();
            if (F.ShowDialog() == DialogResult.OK)
            {

                try
                {
                    BacnetPropertyValue[] initialvalues = null;

                    if (F.ObjectName.Text != null) // Add the initial propery name
                    {
                        initialvalues = new BacnetPropertyValue[1];
                        initialvalues[0] = new BacnetPropertyValue();
                        initialvalues[0].property.propertyIdentifier = (uint)BacnetPropertyIds.PROP_OBJECT_NAME;
                        initialvalues[0].value = new BacnetValue[1];
                        initialvalues[0].value[0] = new BacnetValue(F.ObjectName.Text);
                    }
                    comm.CreateObjectRequest(adr, new BacnetObjectId((BacnetObjectTypes)F.ObjectType.SelectedIndex, (uint)F.ObjectId.Value), initialvalues);

                    m_DeviceTree_AfterSelect(null, new TreeViewEventArgs(m_DeviceTree.SelectedNode));
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Error : " + ex.Message);
                    MessageBox.Show("Fail to Create Object","CreateObject", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

        }

        private void editBBMDTablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BacnetClient comm = null;
            BacnetAddress adr;
            uint device_id;

            FetchEndPoint(out comm, out adr, out device_id);

            if ((comm != null) && (comm.Transport is BacnetIpUdpProtocolTransport) && (adr != null) && (adr.RoutedSource == null))
                new BBMDEditor(comm, adr).ShowDialog();
            else
                MessageBox.Show("An IPv4 device is required", "Wrong device", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void cleanToolStripMenuItem_Click(object sender, EventArgs e)
        {             
            DialogResult res = MessageBox.Show(this, "Clean all "+DevicesObjectsName.Count.ToString()+" entries from \""+Properties.Settings.Default.Auto_Store_Object_Names_File+"\", really?", "Name database suppression", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            if (res == DialogResult.OK)
            {
                DevicesObjectsName = new Dictionary<Tuple<String, BacnetObjectId>, String>();
                Trace.TraceInformation("Created new object names dictionary.");
                objectNamesChangedFlag = true;
                DoSaveObjectNames();
                // Enumerate each Transport Layer:
                foreach (TreeNode transport in m_DeviceTree.Nodes[0].Nodes)
                {
                    //Enumerate each Parent Device:
                    foreach (TreeNode node in transport.Nodes)
                    {
                        try
                        {
                            KeyValuePair<BacnetAddress, uint>? entryNullable = node.Tag as KeyValuePair<BacnetAddress, uint>?;
                            if(entryNullable!=null)
                            {
                                KeyValuePair<BacnetAddress, uint> entry = entryNullable.Value;

                                node.Text = "Device " + entry.Value + " - " + entry.Key.ToString(false);
                                node.ToolTipText = "";
                            }

                        }
                        catch(Exception)
                        {

                        }

                        //Enumerate routed nodes
                        foreach (TreeNode subNode in node.Nodes)
                        {
                            try
                            {
                                KeyValuePair<BacnetAddress, uint>? entryNullable2 = subNode.Tag as KeyValuePair<BacnetAddress, uint>?;
                                if(entryNullable2!=null)
                                {
                                    KeyValuePair<BacnetAddress, uint> entry2 = entryNullable2.Value;
                                    subNode.Text = "Device " + entry2.Value + " - " + entry2.Key.ToString(true);
                                    subNode.ToolTipText = "";
                                }
                                
                            }
                            catch(Exception)
                            {

                            }
                        }
                    }
                }

                m_DeviceTree.SelectedNode = null;
                m_AddressSpaceTree.SelectedNode = null;
                m_AddressSpaceTree.Nodes.Clear();
                m_DataGrid.SelectedObject = null;
                _selectedDevice = null;
                _selectedNode = null;
            }
        }

        // Change the WritePriority Value
        private void MainDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Modifiers == (Keys.Control | Keys.Alt)))
            {

                if ((e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9) || (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9))
                {
                    string s = e.KeyCode.ToString();
                    int i = Convert.ToInt32(s[s.Length-1]) - 48;

                    Properties.Settings.Default.DefaultWritePriority = (BacnetWritePriority)i;
                    SystemSounds.Beep.Play();
                    Trace.WriteLine("WritePriority change to level " + i.ToString() + " : " + ((BacnetWritePriority)i).ToString());
                }
            }
        }

        #region "Alarm Logger"

        StreamWriter AlarmFileWritter;
        object lockAlarmFileWritter = new object();

        private void EventAlarmLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (AlarmFileWritter != null)
            {
                lock (lockAlarmFileWritter)
                {
                    AlarmFileWritter.Close();
                    EventAlarmLogToolStripMenuItem.Text = "Start saving Cov/Event/Alarm Log";
                    AlarmFileWritter = null;
                }
                return;

            }

            //which file to use ?
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.DefaultExt = ".csv";
            dlg.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            if (dlg.ShowDialog(this) != System.Windows.Forms.DialogResult.OK) return;
            string filename = dlg.FileName;

            try
            {
                AlarmFileWritter = new StreamWriter(filename);
                AlarmFileWritter.WriteLine("Device;ObjectId;Name;Value;Time;Status;Description");
                EventAlarmLogToolStripMenuItem.Text="Stop saving Cov/Event/Alarm Log";                
            }
            catch
            {
                MessageBox.Show(this, "File error", "Unable to open the file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AlarmFileWritter = null;
            }
        }
        // Event/Alarm logging
        private void AddLogAlarmEvent(ListViewItem itm)
        {
            lock (lockAlarmFileWritter)
            {
                if (AlarmFileWritter != null)
                {
                    for (int i = 0; i < itm.SubItems.Count; i++)
                    {
                        AlarmFileWritter.Write(((i != 0) ? ";" : "") + itm.SubItems[i].Text);
                    }
                    AlarmFileWritter.WriteLine();
                    AlarmFileWritter.Flush();
                }
            }
        }

        #endregion

        private void btnExport_Click(object sender, EventArgs e)
        {
            ExportCovGraph();
        }

        private void m_AddressSpaceTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            this.m_SubscriptionView.SelectedItems.Clear();
            UpdateGrid(e.Node);
            BacnetClient cl; BacnetAddress ba; BacnetObjectId objId;

            // Hide all elements in the toolstip menu
            foreach (object its in m_AddressSpaceMenuStrip.Items)
                (its as ToolStripMenuItem).Visible = false;
            // Set Subscribe always visible
            m_AddressSpaceMenuStrip.Items[0].Visible = true;
            // Set Search always visible
            m_AddressSpaceMenuStrip.Items[8].Visible = true;

            // Get the node type
            GetObjectLink(out cl, out ba, out objId, BacnetObjectTypes.MAX_BACNET_OBJECT_TYPE);
            // Set visible some elements depending of the object type
            switch (objId.type)
            {
                case BacnetObjectTypes.OBJECT_FILE:
                    m_AddressSpaceMenuStrip.Items[1].Visible = true;
                    m_AddressSpaceMenuStrip.Items[2].Visible = true;
                    break;

                case BacnetObjectTypes.OBJECT_TRENDLOG:
                case BacnetObjectTypes.OBJECT_TREND_LOG_MULTIPLE:
                    m_AddressSpaceMenuStrip.Items[3].Visible = true;
                    break;

                case BacnetObjectTypes.OBJECT_SCHEDULE:
                    m_AddressSpaceMenuStrip.Items[4].Visible = true;
                    break;

                case BacnetObjectTypes.OBJECT_NOTIFICATION_CLASS:
                    m_AddressSpaceMenuStrip.Items[5].Visible = true;
                    break;

                case BacnetObjectTypes.OBJECT_CALENDAR:
                    m_AddressSpaceMenuStrip.Items[6].Visible = true;
                    break;
            }

            // Allows delete menu 
            if (objId.type != BacnetObjectTypes.OBJECT_DEVICE)
                m_AddressSpaceMenuStrip.Items[7].Visible = true;
        }

        private void m_SubscriptionView_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection selectedSubscriptions = this.m_SubscriptionView.SelectedItems;
            if(selectedSubscriptions==null || selectedSubscriptions.Count==0)
            {
                return;
            }

            this.m_AddressSpaceTree.SelectedNode = null;
            this.m_AddressSpaceTree.SelectedNodes.Clear();

            ListViewItem itm = selectedSubscriptions[0];
            Subscription subscription = (Subscription)itm.Tag;

            UpdateGrid(subscription);
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            TogglePlotter();
        }

        private void m_SubscriptionView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            Subscription sub = (Subscription)e.Item.Tag;
            if(sub!=null)
            {
                lock (m_subscription_list)
                {
                    try
                    {
                        RollingPointPairList points = m_subscription_points[sub.sub_key];
                        foreach (LineItem li in Pane.CurveList)
                            if (li.Points == points)
                            {
                                li.IsVisible = e.Item.Checked;
                                e.Item.SubItems[9].Text = e.Item.Checked.ToString();
                                CovGraph.AxisChange();
                                CovGraph.Invalidate();
                                break;
                            }
                    }
                    catch { }
                }
            }
            
        }

        private void ClearPlotterButton_Click(object sender, EventArgs e)
        {
            lock (m_subscription_list)
            {
                foreach (RollingPointPairList p in m_subscription_points.Values)
                {
                    try
                    {
                        p.Clear();
                    }
                    catch { }
                }
                CovGraph.AxisChange();
                CovGraph.Invalidate();
            }
        }

        private void pollRateSelector_ValueChanged(object sender, EventArgs e)
        {
            uint period = Math.Max(Math.Min((uint)((NumericUpDown)sender).Value, MAX_POLL_PERIOD), MIN_POLL_PERIOD);
            Properties.Settings.Default.Subscriptions_ReplacementPollingPeriod = period;
        }

        private void PollOpn_CheckedChanged(object sender, EventArgs e)
        {
            if(PollOpn.Checked)
            {
                pollRateSelector.Enabled = true;
                Properties.Settings.Default.UsePollingByDefault = true;
            }
            else
            {
                pollRateSelector.Enabled = false;
                Properties.Settings.Default.UsePollingByDefault = false;
            }
        }

        private int _saveFaultCount = 0;
        private void SaveObjectNamesTimer_Tick(object sender, EventArgs e)
        {
            int intervalMinutes = Math.Max(Math.Min(Properties.Settings.Default.Auto_Store_Period_Minutes, 480), 1);
            if (intervalMinutes != Properties.Settings.Default.Auto_Store_Period_Minutes)
                Properties.Settings.Default.Auto_Store_Period_Minutes = intervalMinutes;
            SaveObjectNamesTimer.Interval = intervalMinutes * 60000;
            
            DoSaveObjectNamesIfNecessary();
        }

        private void DoSaveObjectNamesIfNecessary(string path = null)
        {
            if (Properties.Settings.Default.Auto_Store_Object_Names)
            {
                if (objectNamesChangedFlag)
                {
                    DoSaveObjectNames();
                }
            }
        }

        private void DoSaveObjectNames(string path = null)
        {
            string fileTotal;
            if(string.IsNullOrWhiteSpace(path))
            {
                fileTotal = Properties.Settings.Default.Auto_Store_Object_Names_File;
            }
            else
            {
                fileTotal = path;
            }

            if (!string.IsNullOrWhiteSpace(fileTotal))
            {
                try
                {
                    string file = Path.GetFileName(fileTotal);
                    string directory = Path.GetDirectoryName(fileTotal);
                    if (string.IsNullOrWhiteSpace(file))
                    {
                        if (path == null)
                        {
                            file = "Auto_Stored_Object_Names.YabeMap";
                        }
                        else
                        {
                            DateTime d = DateTime.Now;
                            file = "New_Object_Names_File_" + d.Year.ToString() + "-" + d.Month.ToString() + "-" + d.Day.ToString() + "_" + d.Hour.ToString() + "_" + d.Minute.ToString() + ".YabeMap";
                        }
                        fileTotal = Path.Combine(directory, file);
                        Properties.Settings.Default.Auto_Store_Object_Names_File = fileTotal;
                    }

                    if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                    {
                        try
                        {
                            Directory.CreateDirectory(directory);
                        }
                        catch (UnauthorizedAccessException)
                        {
                            Trace.TraceError("Error trying to auto-save object names to file: The directory \"" + directory + "\" does not exist, and Yabe does not have permissions to automatically create this directory. Try changing the Auto_StoreObject_Names_File setting to a different path.");
                            Properties.Settings.Default.Auto_Store_Object_Names = false;
                            return;
                        }
                    }

                }
                catch (Exception ex)
                {
                    Trace.TraceError("Exception trying to auto-save object names to file: " + ex.Message + ". Try resetting the Auto_StoreObject_Names_File setting to a valid file path.");
                    Properties.Settings.Default.Auto_Store_Object_Names = false;
                    return;
                }

                try
                {
                    Stream stream = File.Open(fileTotal, FileMode.Create);
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(stream, DevicesObjectsName);
                    stream.Close();
                    objectNamesChangedFlag = false;
                    _saveFaultCount = 0;
                    Trace.TraceInformation("Saved object names to \"" + fileTotal + "\".");
                }
                catch (Exception ex)
                {
                    _saveFaultCount++;
                    int maxFault = 3;
                    if (_saveFaultCount >= maxFault)
                    {
                        Trace.TraceError(String.Format("Exception trying to auto-save object names to file: " + ex.Message + ". We will retry {0} more time(s).", maxFault - _saveFaultCount));
                    }
                    else
                    {
                        Trace.TraceError(String.Format("Exception trying to auto-save object names to file: " + ex.Message + ". This error happened {0} times, so auto-save is being disabled. Try resetting the Auto_StoreObject_Names_File setting to a valid file path.", _saveFaultCount));
                        Properties.Settings.Default.Auto_Store_Object_Names = false;
                        return;
                    }
                }
            }
            else
            {
                Trace.TraceError("Error trying to auto-save object names to file: There is no file specified. Try resetting the Auto_StoreObject_Names_File setting to a valid file path.");
                Properties.Settings.Default.Auto_Store_Object_Names = false;
                return;
            }
        }

        private void searchToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            GenericInputBox<TextBox> search = new GenericInputBox<TextBox>("Search object", "Name",     (o) =>
            {
                o.Text = m_AddressSpaceTree.SelectedNode.Text;
            });

            if (search.ShowDialog() == DialogResult.OK)
            {
                string find = search.genericInput.Text.ToLower();
                foreach (TreeNode tn in m_AddressSpaceTree.Nodes)
                {
                    if (tn.Text.ToLower().Contains(find))
                    {
                        tn.EnsureVisible();
                        m_AddressSpaceTree.SelectedNode = tn;                       
                        break;
                    }
                }
            }
        }

    }

    // Used to sort the devices Tree by device_id
    public class NodeSorter : IComparer
    {
        public int Compare(object x, object y)
        {
            TreeNode tx = x as TreeNode;
            TreeNode ty = y as TreeNode;           


            KeyValuePair<BacnetAddress, uint>? entryx = tx.Tag as KeyValuePair<BacnetAddress, uint>?;
            KeyValuePair<BacnetAddress, uint>? entryy = ty.Tag as KeyValuePair<BacnetAddress, uint>?;

            // Two device, compare the device_id
            if ((entryx != null) && (entryy != null))
                return entryx.Value.Value.CompareTo(entryy.Value.Value);
            else // something must be provide
                return tx.Text.CompareTo(ty.Text);
        }
    }

    public enum AddressTreeViewType
    {
        List,
        Structured,
        Both
    }
}
