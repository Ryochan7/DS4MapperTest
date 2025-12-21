using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DS4MapperTest
{
    public class ArgumentParser
    {
        private string profilePath;
        public string ProfilePath
        {
            get => profilePath;
        }

        private string virtualkbmHandler = string.Empty;
        public string VirtualkbmHandler { get => virtualkbmHandler; }

        private Dictionary<string, string> errors =
            new Dictionary<string, string>();

        public Dictionary<string, string> Errors { get => errors; }
        public bool HasErrors => errors.Count > 0;

        public void Parse(string[] args)
        {
            errors.Clear();
            //foreach (string arg in args)
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                switch(arg)
                {
                    case "-virtualkbm":
                        if (i + 1 < args.Length)
                        {
                            i++;
                            string temp = args[i];
                            virtualkbmHandler = temp;
                        }

                        break;

                    default:
                        if (i+1 == args.Length && File.Exists(arg))
                        {
                            profilePath = arg;
                        }

                        break;
                }
            }
        }
    }
}
