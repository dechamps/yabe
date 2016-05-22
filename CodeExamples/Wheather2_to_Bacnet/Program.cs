/**************************************************************************
*                           MIT License
* 
* Copyright (C) 2016 Frederic Chaxel <fchaxel@free.fr>
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
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using System.Threading;
using System.ServiceProcess;
using System.Configuration.Install;
using Microsoft.Win32;
using BaCSharp;
using AnotherStorageImplementation;
using System.IO.BACnet;

namespace Wheather2_to_Bacnet
{
    public class MyService : System.ServiceProcess.ServiceBase
    {
        ManualResetEvent StopSrv = new ManualResetEvent(false);

        const uint deviceId = 12345; // could be a parameter
        DeviceObject device;
        AnalogInput<int> Temp, Windspeed, Humidity, Pressure;
        TrendLog TrendTemp;
        CharacterString Windsdir, WheatherDescr;

        // An alternative is to embbed data into code, or to use another way (configuration file, ...)
        string UserAccessKey = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wheather2_to_Bacnet", "UserAccessKey", null);
        string Latitude = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wheather2_to_Bacnet", "Latitude", null);
        string Longitude = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wheather2_to_Bacnet", "Longitude", null);

        public bool RunAsConsoleApp()
        {
            if ((UserAccessKey == null) || (Latitude == null) || (Longitude == null))
                return false;
            new Thread(WorkingLoop).Start();
            return true;
        }

        const string XMLREP_TEST = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><weather><curren_weather><temp>14</temp><temp_unit>c</temp_unit><wind><speed>5</speed><dir>SW</dir><wind_unit>kph</wind_unit></wind><humidity>77</humidity><pressure>1018</pressure><weather_text>Mostly cloudy</weather_text><weather_code>1</weather_code></curren_weather><forecast><date>2016-05-20</date><temp_unit>c</temp_unit><day_max_temp>16</day_max_temp><night_min_temp>13</night_min_temp><day><weather_text>Overcast skies</weather_text><weather_code>3</weather_code><wind><speed>25</speed><dir>S</dir><dir_degree>188</dir_degree><wind_unit>kph</wind_unit></wind></day><night><weather_text>Overcast skies</weather_text><weather_code>3</weather_code><wind><speed>36</speed><dir>SSW</dir><dir_degree>201</dir_degree><wind_unit>kph</wind_unit></wind></night></forecast><forecast><date>2016-05-21</date><temp_unit>c</temp_unit><day_max_temp>17</day_max_temp><night_min_temp>12</night_min_temp><day><weather_text>Moderate rain</weather_text><weather_code>63</weather_code><wind><speed>36</speed><dir>S</dir><dir_degree>180</dir_degree><wind_unit>kph</wind_unit></wind></day><night><weather_text>Clear skies</weather_text><weather_code>0</weather_code><wind><speed>32</speed><dir>WSW</dir><dir_degree>255</dir_degree><wind_unit>kph</wind_unit></wind></night></forecast></weather>";
        private string Wheather2_Request(string UserAccessKey, string Lat, string Long)
        {
            //return XMLREP_TEST;

            try
            {
                string Url = "http://www.myweather2.com/developer/forecast.ashx?output=xml?uac=" + UserAccessKey + "&query=" + Lat + "," + Long;

                WebRequest req = WebRequest.Create(Url);
                WebResponse resp = req.GetResponse();

                StreamReader respReader = new StreamReader(resp.GetResponseStream());
                String response = respReader.ReadToEnd();

                return response;
            }
            catch
            {
                return null;
            }
        }

        private void ParseWheather2_Response(String Rep)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(Rep);

                XmlNode node = doc.SelectSingleNode("/weather/curren_weather/temp");
                Temp.internal_PROP_PRESENT_VALUE= Convert.ToInt32(node.InnerText);
                TrendTemp.AddValue(Temp.internal_PROP_PRESENT_VALUE, 0);

                node = doc.SelectSingleNode("/weather/curren_weather/wind/speed");
                Windspeed.internal_PROP_PRESENT_VALUE = Convert.ToInt32(node.InnerText);

                node = doc.SelectSingleNode("/weather/curren_weather/wind/dir");
                Windsdir.internal_PROP_PRESENT_VALUE = node.InnerText;

                node = doc.SelectSingleNode("/weather/curren_weather/humidity");
                Humidity.internal_PROP_PRESENT_VALUE= Convert.ToInt32(node.InnerText);

                node = doc.SelectSingleNode("/weather/curren_weather/pressure");
                Pressure.internal_PROP_PRESENT_VALUE= Convert.ToInt32(node.InnerText);

                node = doc.SelectSingleNode("/weather/curren_weather/weather_text");
                WheatherDescr.internal_PROP_PRESENT_VALUE = node.InnerText;
            }
            catch { }
        }

        void InitBacnetDictionary()
        {
            device = new DeviceObject(deviceId, "Wheather2 to Bacnet ", "Wheather2 data", false);
            if ((UserAccessKey != null) && (Latitude != null) && (Longitude != null))
            {
                Temp = new AnalogInput<int>
                (
                    0,
                    "Temperature",
                    "Temperature",
                    0,
                    BacnetUnitsId.UNITS_DEGREES_CELSIUS
                );

                TrendTemp = new TrendLog(0, "Temperature Trend", "Temperature Trend", 6 * 24, BacnetTrendLogValueType.TL_TYPE_SIGN);

                Windspeed = new AnalogInput<int>
                (
                    1,
                    "Windspeed",
                    "Wind speed",
                    0,
                    BacnetUnitsId.UNITS_KILOMETERS_PER_HOUR
                );
                Humidity = new AnalogInput<int>
                (
                    2,
                    "Humidity",
                    "Humidity",
                    0,
                    BacnetUnitsId.UNITS_PERCENT
                );
                Pressure = new AnalogInput<int>
                (
                    3,
                    "Pressure",
                    "Pressure",
                    0,
                    BacnetUnitsId.UNITS_HECTOPASCALS
                );

                Windsdir = new CharacterString
                (0, "Windir", "Wind Direction", "Not available", false);

                WheatherDescr = new CharacterString
                (1, "WheatherDescr", "Wheather Description", "Not available", false);

                device.AddBacnetObject(Temp);
                device.AddBacnetObject(TrendTemp);
                device.AddBacnetObject(Windspeed);
                device.AddBacnetObject(Humidity);
                device.AddBacnetObject(Pressure);
                device.AddBacnetObject(Windsdir);
                device.AddBacnetObject(WheatherDescr);

                device.AddBacnetObject(new NotificationClass(0, "Notification", "Notification"));

                // Force the JIT compiler to make some job before network access
                device.Cli2Native();
            }
            BacnetActivity.StartActivity(device);
        }

        void WorkingLoop()
        {
            InitBacnetDictionary();

            if ((UserAccessKey == null) || (Latitude == null) || (Longitude == null))
                return;

            for (; ; )
            {
                // Read wheather data from the webservice
                String xmlRep = Wheather2_Request(UserAccessKey, Latitude, Longitude);
                if (xmlRep != null)                
                    ParseWheather2_Response(xmlRep);
                
                // Wait 10 minutes or a stop condition
                if (StopSrv.WaitOne(new TimeSpan(0, 10, 0)) == true)
                    return;
            }
        }

        Thread m_thread;
        protected override void OnStart(string[] args)
        {
            m_thread = new Thread(WorkingLoop);
            m_thread.Start();
            base.OnStart(args);
        }

        protected override void OnStop()
        {
            StopSrv.Set();
            // wait for the thread to stop giving it 20 seconds
            m_thread.Join(20000);

            base.OnStop();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Console Mode detected via a param
            if (args.Length > 0)
            {
                Console.WriteLine("Wheather2_to_Bacnet Running as a console application");
                Console.WriteLine("Can be installed and started as a windows services");
                MyService ConsoleModeApp = new MyService();
                if (ConsoleModeApp.RunAsConsoleApp() == false)
                {
                    Console.WriteLine("\nError : Unable to find Parameters in the windows registry");
                    Console.WriteLine("\t   see the corresponding Readme file and");
                    Console.WriteLine("\t   Wheather2config.reg in "+Directory.GetCurrentDirectory());
                }
            }
            else
            {
                ServiceBase[] servicesToRun;
                servicesToRun = new ServiceBase[] { new MyService() };
                ServiceBase.Run(servicesToRun);
            }
        }
    }

    [System.ComponentModel.RunInstaller(true)]
    public class MyServiceInstaller : System.Configuration.Install.Installer
    {
        public MyServiceInstaller()
        {
            ServiceProcessInstaller process = new ServiceProcessInstaller();

            process.Account = ServiceAccount.LocalSystem;

            ServiceInstaller serviceAdmin = new ServiceInstaller();

            serviceAdmin.StartType = ServiceStartMode.Automatic;
            serviceAdmin.ServiceName = "WheatherBacnet";
            serviceAdmin.DisplayName = "Wheather2 to Bacnet";
            serviceAdmin.Description = "Bridge Wheather2 to Bacnet";

            Installers.Add(process);
            Installers.Add(serviceAdmin);
        }
    }
}
