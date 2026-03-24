using Denon;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DenonControl
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Taste_SelectV1.Content = "V: " + Denon.Properties.Settings.Default.QSetVol1;
            Taste_SelectV2.Content = "V: " + Denon.Properties.Settings.Default.QSetVol2;
            Taste_SelectV3.Content = "V: " + Denon.Properties.Settings.Default.QSetVol3;
            Taste_SelectV4.Content = "V: " + Denon.Properties.Settings.Default.QSetVol4;

            Taste_Select1.Content = Denon.Properties.Settings.Default.QSName1;
            Taste_Select2.Content = Denon.Properties.Settings.Default.QSName2;
            Taste_Select3.Content = Denon.Properties.Settings.Default.QSName3;
            Taste_Select4.Content = Denon.Properties.Settings.Default.QSName4;

            DenonAVRName.Content = Denon.Properties.Settings.Default.Name;
            Title = "Denon " + Denon.Properties.Settings.Default.Name;

            Loaded += async (_, __) =>
            {
                string answer = await SendCommandAsync("PW?");
                if (answer == "PWON\r")
                {
                    Taste_On.Background = ButtonOn();
                    await StatusAsync();
                }
                else
                {
                    Taste_On.Background = ButtonOff();
                    Voice.Content = "V:        ";
                }
            };
        }

        private void UpdateDenonSettings()
        {
            Taste_SelectV1.Content = "V: " + Denon.Properties.Settings.Default.QSetVol1;
            Taste_SelectV2.Content = "V: " + Denon.Properties.Settings.Default.QSetVol2;
            Taste_SelectV3.Content = "V: " + Denon.Properties.Settings.Default.QSetVol3;
            Taste_SelectV4.Content = "V: " + Denon.Properties.Settings.Default.QSetVol4;

            Taste_Select1.Content = Denon.Properties.Settings.Default.QSName1;
            Taste_Select2.Content = Denon.Properties.Settings.Default.QSName2;
            Taste_Select3.Content = Denon.Properties.Settings.Default.QSName3;
            Taste_Select4.Content = Denon.Properties.Settings.Default.QSName4;

            DenonAVRName.Content = Denon.Properties.Settings.Default.Name;
            Title = "Denon " + Denon.Properties.Settings.Default.Name;
        }

        private void btnDenonSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new DenonSettings();

            // Fenster modal öffnen
            bool? result = settingsWindow.ShowDialog();

            if (result == true)
            {
                // Benutzer hat OK gedrückt → Update durchführen
                UpdateDenonSettings();
            }
        }


        private void btnMinimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void btnClose_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        // ---------------------------------------------------------
        // POWER
        // ---------------------------------------------------------
        private async void Taste_OnOff_click(object sender, RoutedEventArgs e)
        {
            string answer = await SendCommandAsync("PW?");

            if (answer == "PWON\r")
            {
                Taste_On.Background = ButtonOff();
                await SendCommandAsync("PWSTANDBY");

                Voice.Content = "";
                WVoice.Content = "";
                Taste_Select3.Background = ButtonOff();
                Taste_Select4.Background = ButtonOff();
            }
            else
            {
                Taste_On.Background = ButtonOn();
                await SendCommandAsync("PWON");
                await Task.Delay(1500);
                await StatusAsync();
            }
        }

        // ---------------------------------------------------------
        // MUTE
        // ---------------------------------------------------------
        private async void Taste_Mute_click(object sender, RoutedEventArgs e)
        {
            string answer = await SendCommandAsync("MU?");
            bool isMuted = answer == "MUON\r";

            Taste_Mute.Background = isMuted ? ButtonOff() : ButtonOn();
            await SendCommandAsync(isMuted ? "MUOFF" : "MUON");
        }

        // ---------------------------------------------------------
        // MAIN VOLUME
        // ---------------------------------------------------------
        private async void Taste_VPlus_Click(object sender, RoutedEventArgs e)
        {
            Vol(await SendCommandAsync("MVUP"));
        }

        private async void Taste_VMinus_Click(object sender, RoutedEventArgs e)
        {
            Vol(await SendCommandAsync("MVDOWN"));
        }

        private async void Taste_VPlus2_Click(object sender, RoutedEventArgs e)
        {
            await AdjustVolumeAsync(+2);
        }

        private async void Taste_VMinus2_Click(object sender, RoutedEventArgs e)
        {
            await AdjustVolumeAsync(-2);
        }

        private async Task AdjustVolumeAsync(int step)
        {
            string answer = await SendCommandAsync("MV?");
            if (string.IsNullOrWhiteSpace(answer)) return;

            // Original: "MV" abschneiden
            try { answer = answer.Remove(0, 2); }
            catch { return; }

            int stringlenght = answer.Length;
            answer = answer.Substring(0, stringlenght - 1); // \r entfernen

            if (!double.TryParse(answer, out double Volume))
                return;

            if (step < 0) // MINUS
            {
                if (stringlenght == 3) // z.B. "45\r"
                {
                    if (Volume > 11)
                        Volume -= 2;
                    else if (Volume > 10)
                        Volume = 10;
                }
                else if (stringlenght == 4) // z.B. "455\r"
                {
                    if (Volume > 115)
                        Volume -= 20;
                    else if(Volume < 110)
                        Volume = 0;
                }
            }
            else // PLUS
            {
                if (stringlenght == 3)
                {
                    if (Volume >= 10)
                        Volume += 2;
                    else if (Volume < 10)
                        Volume = 10;
                }
                else if (stringlenght == 4) // z.B. "455\r"
                {
                    if (Volume >= 80)
                        Volume += 20;
                    else if (Volume < 80)
                        Volume = 10;
                }
            }

            await SendCommandAsync("MV" + Volume);
            await Task.Delay(100);

            Vol(await SendCommandAsync("MV?"));
        }


        // ---------------------------------------------------------
        // QUICK SELECT
        // ---------------------------------------------------------
        private async void Taste_Select1_Click(object sender, RoutedEventArgs e)
        {
            // UI setzen
            Taste_On.Background = ButtonOn();
            Taste_Select1.Background = ButtonOn();
            Taste_Select2.Background = ButtonOff();
            Taste_Select3.Background = ButtonOff();
            Taste_Select4.Background = ButtonOff();

            // Quick-Select aus Settings holen
            int Q_Select1 = Denon.Properties.Settings.Default.QSelect1;

            // Befehl zusammenbauen
            string Message = "MSQUICK" + Q_Select1;

            // An Denon senden
            await SendCommandAsync(Message);

            // Kurze Pause wie früher
            await Task.Delay(300);

            // Anzeigen aktualisieren
            Vol(await SendCommandAsync("MV?"));
            await SendCommandAsync("PSGEQ OFF");
        }

        private async void Taste_Select2_Click(object sender, RoutedEventArgs e)
        {
            // UI setzen
            Taste_On.Background = ButtonOn();
            Taste_Select1.Background = ButtonOff();
            Taste_Select2.Background = ButtonOn();
            Taste_Select3.Background = ButtonOff();
            Taste_Select4.Background = ButtonOff();

            // Quick-Select aus Settings holen
            int Q_Select2 = Denon.Properties.Settings.Default.QSelect2;

            // Befehl zusammenbauen
            string Message = "MSQUICK" + Q_Select2;

            // An Denon senden
            await SendCommandAsync(Message);

            // Kurze Pause wie früher
            await Task.Delay(300);

            // Anzeigen aktualisieren
            Vol(await SendCommandAsync("MV?"));
            await SendCommandAsync("PSGEQ OFF");
        }


        private async void Taste_Select3_Click(object sender, RoutedEventArgs e)
        {
            // UI setzen
            Taste_On.Background = ButtonOn();
            Taste_Select1.Background = ButtonOff();
            Taste_Select2.Background = ButtonOff();
            Taste_Select3.Background = ButtonOn();
            Taste_Select4.Background = ButtonOff();

            // Quick-Select aus Settings holen
            int Q_Select3 = Denon.Properties.Settings.Default.QSelect3;

            // Befehl zusammenbauen
            string Message = "MSQUICK" + Q_Select3;

            // An Denon senden
            await SendCommandAsync(Message);

            // Kurze Pause wie früher
            await Task.Delay(300);

            // Anzeigen aktualisieren
            Vol(await SendCommandAsync("MV?"));
            await SendCommandAsync("PSGEQ OFF");
        }


        private async void Taste_Select4_Click(object sender, RoutedEventArgs e)
        {
            // UI setzen
            Taste_On.Background = ButtonOn();
            Taste_Select1.Background = ButtonOff();
            Taste_Select2.Background = ButtonOff();
            Taste_Select3.Background = ButtonOff();
            Taste_Select4.Background = ButtonOn();

            // Quick-Select aus Settings holen
            int Q_Select4 = Denon.Properties.Settings.Default.QSelect4;

            // Befehl zusammenbauen
            string Message = "MSQUICK" + Q_Select4;

            // An Denon senden
            await SendCommandAsync(Message);

            // Kurze Pause wie früher
            await Task.Delay(300);

            // Anzeigen aktualisieren
            Vol(await SendCommandAsync("MV?"));
            await SendCommandAsync("PSGEQ OFF");
        }

        // ---------------------------------------------------------
        // WOOFER VOLUME
        // ---------------------------------------------------------
        private async void Taste_WVPlus_Click(object sender, RoutedEventArgs e)
        {
            string answer = await SendCommandAsync("PSSWL UP");

            try
            {
                // Original: "SW" abschneiden
                answer = answer.Remove(0, 2);
            }
            catch { return; }

            WVol(answer);
        }

        private async void Taste_WVMinus_Click(object sender, RoutedEventArgs e)
        {
            string answer = await SendCommandAsync("PSSWL DOWN");

            try
            {
                // Original: "SW" abschneiden
                answer = answer.Remove(0, 2);
            }
            catch { return; }

            WVol(answer);
        }


        private async void Taste_WVPlus2_Click(object sender, RoutedEventArgs e)
        {
            await AdjustWooferAsync(+2);
        }

        private async void Taste_WVMinus2_Click(object sender, RoutedEventArgs e)
        {
            await AdjustWooferAsync(-2);
        }

        private async Task AdjustWooferAsync(int step)
        {
            string answer = await SendCommandAsync("PSSWL ?");
            if (string.IsNullOrWhiteSpace(answer)) return;

            // Original: "PSSWL " abschneiden
            try { answer = answer.Remove(0, 6); }
            catch { return; }

            int stringlenght = answer.Length;
            answer = answer.Substring(0, stringlenght - 1); // \r entfernen

            if (!double.TryParse(answer, out double Volume))
                return;

            if (step < 0) // MINUS
            {
                if (Volume <= 40 || Volume == 385 || Volume == 395)
                {
                    Volume = 38; // Original-Reset
                }
                else if (stringlenght == 3)
                {
                    if (Volume > 39)
                        Volume -= 2;
                }
                else if (stringlenght == 4)
                {
                    if (Volume > 395)
                        Volume -= 20;
                }
            }
            else // PLUS
            {
                if (Volume >= 605 || Volume == 60 || Volume == 61)
                {
                    Volume = 62; // Original-Reset
                }
                else if (stringlenght == 3)
                {
                    Volume += 2;
                }
                else if (stringlenght == 4)
                {
                    Volume += 20;
                }
            }

            await SendCommandAsync("PSSWL " + Volume);
            await Task.Delay(100);

            WVol(await SendCommandAsync("PSSWL ?"));
        }


        // ---------------------------------------------------------
        // STATUS
        // ---------------------------------------------------------
        public async Task StatusAsync()
        {
            await Task.Delay(100);
            Vol(await SendCommandAsync("MV?"));

            await Task.Delay(100);
            WVol(await SendCommandAsync("PSSWL ?"));

            await Task.Delay(100);
            string quick = await SendCommandAsync("MSQUICK ?");
            Taste_Select3.Background = quick == "MSQUICK3\r" ? ButtonOn() : ButtonOff();
            Taste_Select4.Background = quick == "MSQUICK4\r" ? ButtonOn() : ButtonOff();

            await Task.Delay(100);
            string mute = await SendCommandAsync("MU?");
            Taste_Mute.Background = mute == "MUON\r" ? ButtonOn() : ButtonOff();
        }

        // ---------------------------------------------------------
        // PARSING
        // ---------------------------------------------------------
        public void Vol(string answer)
        {
            if (string.IsNullOrWhiteSpace(answer))
                return;

            try
            {
                // wie im Original: "MV" abschneiden
                answer = answer.Remove(0, 2);
            }
            catch
            {
                return;
            }

            int stringlenght = answer.Length;

            if (stringlenght == 3)
            {
                // z.B. "45\r" -> "45" -> 45.0
                answer = answer.Substring(0, stringlenght - 1);
                Voice.Content = "V: " + answer + ".0";
            }
            else if (stringlenght == 4)
            {
                // z.B. "455\r" -> "45" -> 45.5
                answer = answer.Substring(0, stringlenght - 2);
                Voice.Content = "V: " + answer + ".5";
            }
        }


        public void WVol(string answer)
        {
            if (answer == "")
            {
                return;
            }
            try
            {
                answer = answer.Remove(0, 6);
            }
            catch
            {
                return;
            }
            int stringlenght = answer.Length;
            if (stringlenght == 3)
            {
                answer = answer.Substring(0, stringlenght - 1);
                double douWVol = Convert.ToDouble(answer) - 38;
                WVoice.Content = ("W: " + douWVol + ".0");
            }
            if (stringlenght == 4)
            {
                answer = answer.Substring(0, stringlenght - 2);
                double douWVol = Convert.ToDouble(answer) - 38;
                WVoice.Content = ("W: " + douWVol + ".5");
            }
        }

        // ---------------------------------------------------------
        // BUTTON COLORS
        // ---------------------------------------------------------
        public RadialGradientBrush ButtonOn() => new RadialGradientBrush
        {
            GradientOrigin = new Point(0.5, 0.5),
            Center = new Point(0.5, 0.5),
            RadiusX = 0.7,
            RadiusY = 0.7,
            GradientStops =
            {
                new GradientStop(Color.FromArgb(225, 0, 255, 0), 0.0),
                new GradientStop(Color.FromArgb(225, 0, 225, 0), 0.2),
                new GradientStop(Color.FromArgb(225, 0, 175, 0), 0.5),
                new GradientStop(Color.FromArgb(225, 0, 125, 0), 0.75),
                new GradientStop(Colors.DimGray, 1.0)
            }
        };

        public RadialGradientBrush ButtonOff() => new RadialGradientBrush
        {
            GradientOrigin = new Point(0.5, 0.5),
            Center = new Point(0.5, 0.5),
            RadiusX = 0.8,
            RadiusY = 0.7,
            GradientStops =
            {
                new GradientStop(Color.FromArgb(225, 255, 0, 0), 0.0),
                new GradientStop(Color.FromArgb(225, 225, 0, 0), 0.2),
                new GradientStop(Color.FromArgb(225, 175, 0, 0), 0.5),
                new GradientStop(Color.FromArgb(225, 125, 0, 0), 0.75),
                new GradientStop(Colors.DimGray, 1.0)
            }
        };

        // ---------------------------------------------------------
        // TCP COMMUNICATION (ASYNC)
        // ---------------------------------------------------------
        public async Task<string> SendCommandAsync(string cmd)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    await client.ConnectAsync(
                        Denon.Properties.Settings.Default.IP_Address,
                        Denon.Properties.Settings.Default.Port);

                    using (NetworkStream stream = client.GetStream())
                    {
                        stream.WriteTimeout = 1000;
                        stream.ReadTimeout = 1000;

                        byte[] send = Encoding.ASCII.GetBytes(cmd);
                        await stream.WriteAsync(send, 0, send.Length);

                        byte[] buffer = new byte[client.ReceiveBufferSize];
                        int read = await stream.ReadAsync(buffer, 0, buffer.Length);

                        return Encoding.ASCII.GetString(buffer, 0, read);
                    }
                }
            }
            catch
            {
                return "";
            }
        }

        private async void Taste_SelectV1_Click(object sender, RoutedEventArgs e)
        {
            int Q_SetVol1 = Denon.Properties.Settings.Default.QSetVol1;
            await SendCommandAsync("MV" + Q_SetVol1);
            await Task.Delay(100);

            Vol(await SendCommandAsync("MV?"));
        }

        private async void Taste_SelectV2_Click(object sender, RoutedEventArgs e)
        {
            int Q_SetVol2 = Denon.Properties.Settings.Default.QSetVol2;
            await SendCommandAsync("MV" + Q_SetVol2);
            await Task.Delay(100);

            Vol(await SendCommandAsync("MV?"));
        }

        private async void Taste_SelectV3_Click(object sender, RoutedEventArgs e)
        {
            int Q_SetVol3 = Denon.Properties.Settings.Default.QSetVol3;
            await SendCommandAsync("MV" + Q_SetVol3);
            await Task.Delay(100);

            Vol(await SendCommandAsync("MV?"));
        }

        private async void Taste_SelectV4_Click(object sender, RoutedEventArgs e)
        {
            int Q_SetVol4 = Denon.Properties.Settings.Default.QSetVol4;
            await SendCommandAsync("MV" + Q_SetVol4);
            await Task.Delay(100);

            Vol(await SendCommandAsync("MV?"));
        }
    }
}
