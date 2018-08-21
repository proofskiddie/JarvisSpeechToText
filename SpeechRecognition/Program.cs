using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechRecognition
{
    class Program
    {
        static void Main(string[] args)
        {
            JarvisDriver jarvis = new JarvisDriver();
            jarvis.Start();
        }
    }
}
