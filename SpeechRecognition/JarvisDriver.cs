#region using directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Windows;
using System.Diagnostics;
using System.Threading.Tasks;

#endregion


namespace SpeechRecognition
{
    class JarvisDriver
    {
        #region locals

        /// <summary>
        /// the engine
        /// </summary>
        SpeechRecognitionEngine speechRecognitionEngine = null;

        /// <summary>
        /// The speech synthesizer
        /// </summary>
        SpeechSynthesizer speechSynthesizer = null;

        /// <summary>
        /// list of predefined commands
        /// </summary>
        List<Word> words = new List<Word>();

        /// <summary>
        /// The last command
        /// </summary>
        string lastCommand = "";

        /// <summary>
        /// The name to call commands
        /// </summary>
        string aiName = "do";

        /// <summary> HARDCODED COMMAND VARS
        /// variables used to switch between hardcoded commands
        /// </summary>
        string READ_SUTTA = "0";

        int speechrate = 0;

        #endregion

        #region ctor

        public void Start()
        {

            try
            {
                // create the engine
                speechRecognitionEngine = createSpeechEngine("en-US");

                // hook to event
                speechRecognitionEngine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(engine_SpeechRecognized);

                // load dictionary
                loadGrammarAndCommands();

                // use the system's default microphone
                speechRecognitionEngine.SetInputToDefaultAudioDevice();

                // start listening
                speechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);

                //Create the speech synthesizer
                speechSynthesizer = new SpeechSynthesizer();
                speechSynthesizer.Rate = speechrate;
                speechSynthesizer.SelectVoiceByHints(VoiceGender.Female);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Voice recognition failed " + ex.Message);
            }

            /*// Print voice options for reference
            foreach (var v in speechSynthesizer.GetInstalledVoices().Select(v => v.VoiceInfo))
            {
                Console.WriteLine("Name:{0}", v.Description);
                Console.WriteLine("Gender:{0}", v.Gender);
                Console.WriteLine("Age:{0}\n", v.Age);
                
            }*/

            //Keeps the command prompt going until you say jarvis quit
            while (lastCommand.ToLower() != "quit" && lastCommand.ToLower() != "exit")
            {

            }

        }

        #endregion

        #region internal functions and methods

        /// <summary>
        /// Creates the speech engine.
        /// </summary>
        /// <param name="preferredCulture">The preferred culture.</param>
        /// <returns></returns>
        private SpeechRecognitionEngine createSpeechEngine(string preferredCulture)
        {
            foreach (RecognizerInfo config in SpeechRecognitionEngine.InstalledRecognizers())
            {
                if (config.Culture.ToString() == preferredCulture)
                {
                    speechRecognitionEngine = new SpeechRecognitionEngine(config);
                    break;
                }
            }

            // if the desired culture is not found, then load default
            if (speechRecognitionEngine == null)
            {
                Console.WriteLine("The desired culture is not installed on this machine, the speech-engine will continue using "
                    + SpeechRecognitionEngine.InstalledRecognizers()[0].Culture.ToString() + " as the default culture.",
                    "Culture " + preferredCulture + " not found!");
                speechRecognitionEngine = new SpeechRecognitionEngine(SpeechRecognitionEngine.InstalledRecognizers()[0]);
            }

            return speechRecognitionEngine;
        }

        /// <summary>
        /// Loads the grammar and commands.
        /// </summary>
        private void loadGrammarAndCommands()
        {
            try
            {
                Choices texts = new Choices();
                texts.Add(aiName);
                string[] lines = File.ReadAllLines(Environment.CurrentDirectory + "\\brains.txt");
                foreach (string line in lines)
                {
                    // skip commentblocks and empty lines..
                    if (line.StartsWith("--") || line == String.Empty) continue;

                    // split the line
                    string[] parts = line.Split('|');

                    // get alias if it exits
                    string text, alias = "";
                    if (parts[0].Contains("#"))
                    {
                        int temp;
                        temp = parts[0].IndexOf("#") + 1;
                        alias = parts[0].Substring(temp);
                        text = parts[0].Substring(0, temp - 1).Trim();
                    }
                    else
                        text = parts[0].Trim();
                    /*
                    Console.WriteLine(text);
                    Console.WriteLine(parts[1]);
                    */
                    // add commandItem to the list for later lookup or execution
                    words.Add(new Word() { Text = text, AttachedText = parts[1], IsShellCommand = (parts[2] == "true"), Alias = alias });

                    // add the text to the known choices of speechengine
                    texts.Add(parts[0]);
                }
                Grammar wordsList = new Grammar(new GrammarBuilder(texts));
                speechRecognitionEngine.LoadGrammar(wordsList);

                DictationGrammar dict = new DictationGrammar();
                speechRecognitionEngine.LoadGrammar(dict);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Gets the known command.
        /// </summary>
        /// <param name="command">The order.</param>
        /// <returns></returns>
        private string getKnownTextOrExecute(string command)
        {
            try
            {
                Console.WriteLine(command);
                Word cmd = words.Where(c => (c.Text == command.ToLower())).FirstOrDefault();

                if (cmd == null)
                    return "";

                if (cmd.AttachedText == READ_SUTTA)
                {
                    RandSutta rs = new RandSutta();
                    string ret_str = rs.read_sutta();
                    lastCommand = command;
                    if (ret_str == null)
                        return "";
                    Console.WriteLine(ret_str);
                    return ret_str;
                }
                else if (cmd.IsShellCommand)
                {
                    Process proc = new Process();
                    proc.EnableRaisingEvents = false;
                    proc.StartInfo.FileName = cmd.AttachedText;
                    proc.Start();
                    lastCommand = command;

                    if (cmd.Alias != "")
                    {
                        if (cmd.Alias == "-")
                            return "";
                        return cmd.Alias;
                    }
                    else
                        return "I've started : " + command;
                }
                else if (cmd.AttachedText != null)
                {
                    lastCommand = command;
                    return cmd.AttachedText;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                lastCommand = command;
                return "";
            }


        }

        #endregion

        #region speechEngine events

        /// <summary>
        /// Handles the SpeechRecognized event of the engine control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Speech.Recognition.SpeechRecognizedEventArgs"/> instance containing the event data.</param>
        void engine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string command = getKnownTextOrExecute(e.Result.Text);
            
            if (command != "")
            {
                speechSynthesizer.SpeakAsync(command);
            }                   
            
        }
        
        #endregion

    }
}
