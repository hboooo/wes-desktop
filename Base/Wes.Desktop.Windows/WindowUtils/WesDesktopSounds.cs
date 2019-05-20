using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using Wes.Desktop.Windows.TCP;
using Wes.Utilities;
using Wes.Utilities.Xml;

namespace Wes.Desktop.Windows
{
    public class WesDesktopSounds
    {
        public static void Beep()
        {
            System.Media.SystemSounds.Beep.Play();
        }

        public static void Asterisk()
        {
            System.Media.SystemSounds.Asterisk.Play();
        }

        public static void Exclamation()
        {
            System.Media.SystemSounds.Exclamation.Play();
        }

        public static void Hand()
        {
            System.Media.SystemSounds.Hand.Play();
        }

        public static void Question()
        {
            System.Media.SystemSounds.Question.Play();
        }

        public static void Play(SoundType sound)
        {
            switch (sound)
            {
                case SoundType.Beep:
                    System.Media.SystemSounds.Beep.Play();
                    break;
                case SoundType.Asterisk:
                    System.Media.SystemSounds.Asterisk.Play();
                    break;
                case SoundType.Exclamation:
                    System.Media.SystemSounds.Exclamation.Play();
                    break;
                case SoundType.Hand:
                    System.Media.SystemSounds.Hand.Play();
                    break;
                case SoundType.Question:
                    System.Media.SystemSounds.Question.Play();
                    break;
                default:
                    break;
            }
        }

        public static void Success()
        {
            SoundPlayer player = new SoundPlayer();
            player.SoundLocation = System.IO.Path.Combine(AppPath.BasePath, "Voice", "ScanOkSound.wav");
            player.Load(); //同步加载声音
            player.Play(); //启用新线程播放

            //player.PlayLooping(); //循环播放模式
            //player.PlaySync(); //UI线程同步播放

        }

        public static void Failed()
        {
            SoundPlayer player = new SoundPlayer();
            player.SoundLocation = System.IO.Path.Combine(AppPath.BasePath, "Voice", "ScanFailSound.wav");
            player.Load(); //同步加载声音
            player.Play();
        }


        /// <summary>
        /// tcp通知報警器
        /// </summary>
        public static void Alarm()
        {
            string ip = EnvironmetService.GetValue("General", "AlarmDeviceIP", null, "172.16.3.4");
            string portStr = EnvironmetService.GetValue("General", "AlarmDevicePort", null, "80");
            Task.Factory.StartNew(() =>
            {
                int.TryParse(portStr, out int port);
                TcpSimple.SendAlarmDevice(ip, port, TcpSimple.ALARM_OPEN);
            });
        }
    }

    public enum SoundType
    {
        Beep,
        Asterisk,
        Exclamation,
        Hand,
        Question
    }
}
