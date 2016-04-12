// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Core;

namespace Blinky
{
    public enum LEDCOLOR
    {
        Grey = 0,
        Red = 1,
        Blue = 2
    }

    public enum PIN
    {
        DHT = 4,
        BLUE = 5,
        RED = 6,
        LIGHT = 13,
    }

    class DHT11
    {
        private GpioPin mPin;
        private GpioPinDriveMode mInputDriveMode;

        public void init(GpioPin pin)
        {
            mInputDriveMode = pin.IsDriveModeSupported(GpioPinDriveMode.InputPullUp) ? GpioPinDriveMode.InputPullUp : GpioPinDriveMode.Input;
            pin.SetDriveMode(mInputDriveMode);
            mPin = pin;
        }

        public uint readSensorData()
        {
            uint crc;
            uint i;
            bool flag = false;

            mPin.SetDriveMode(GpioPinDriveMode.Output); // set mode to output  
            mPin.Write(GpioPinValue.Low); // output a high level   

            //delay

            return 0;
        }
    }

    public sealed partial class MainPage : Page
    {
        private GpioPin pin_r, pin_b, pin_light, pin_dht;
        private DHT11 DHT = new DHT11();

        private SolidColorBrush redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private SolidColorBrush blueBrush = new SolidColorBrush(Windows.UI.Colors.Blue);
        private SolidColorBrush grayBrush = new SolidColorBrush(Windows.UI.Colors.LightGray);

        private DispatcherTimer timer;

        public MainPage()
        {
            InitializeComponent();
            InitGPIO();

            DHT.init(pin_dht);
            DHT.readSensorData();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(2000);
            timer.Stop();
            timer.Tick += (s,e) => 
            {
                SetLED(LEDCOLOR.Red);
                timer.Stop();
            };
        }

        private void InitGPIO()
        {
            pin_r = null;
            pin_b = null;
            pin_light = null;
            pin_dht = null;

            // Show an error if there is no GPIO controller
            var gpio = GpioController.GetDefault();
            if (gpio == null)
            {
                GpioStatus.Text = "There is no GPIO controller on this device.";
                return;
            }

            pin_r = gpio.OpenPin(Convert.ToInt16(PIN.RED));
            pin_b = gpio.OpenPin(Convert.ToInt16(PIN.BLUE));
            pin_light = gpio.OpenPin(Convert.ToInt16(PIN.LIGHT));
            pin_dht = gpio.OpenPin(Convert.ToInt16(PIN.DHT));

            pin_r.SetDriveMode(GpioPinDriveMode.Output);
            pin_b.SetDriveMode(GpioPinDriveMode.Output);

            SetLED(LEDCOLOR.Grey);

            pin_light.SetDriveMode(GpioPinDriveMode.Input);
            pin_light.ValueChanged += Light_ValueChanged;

            GpioStatus.Text = "GPIO pins initialized correctly.";
        }

        private void Light_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            if (e.Edge == GpioPinEdge.FallingEdge) //release
            {
                SetLED(LEDCOLOR.Grey);
                var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    timer.Stop();
                });
            }
            else //pressed
            {
                SetLED(LEDCOLOR.Blue);
                var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    timer.Start();
                });
            }
        }

        private void Timer_Tick(object sender, object e)
        {
            SetLED(LEDCOLOR.Red);
            timer.Stop();
        }

        private void SetLED(LEDCOLOR color)
        {
            pin_r.Write(GpioPinValue.High);
            pin_b.Write(GpioPinValue.High);

            if (LEDCOLOR.Red == color)
            {
                pin_r.Write(GpioPinValue.Low);
            }
            else
            if (LEDCOLOR.Blue == color)
            {
                pin_b.Write(GpioPinValue.Low);
            }
            else
            {
            }

            SetUIByColor(color);
        }

        private void SetUIByColor(LEDCOLOR color)
        {
            var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (LEDCOLOR.Red == color)
                {
                    LED.Fill = redBrush;
                }
                else
                if (LEDCOLOR.Blue == color)
                {
                    LED.Fill = blueBrush;
                }
                else
                {
                    LED.Fill = grayBrush;
                }
            });
        }
    }
}
