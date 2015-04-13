using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ScreenCaptureWrapper
{
    public class ScreenCaptureConfig
    {
        public static readonly string ConfigFilename = "config.yml";

        [YamlAlias("ffmpeg_path")]
        public string FFmpegPath { get; set; }

        [YamlAlias("ffmpeg_arguments")]
        public string FFmpegArguments { get; set; }

        public static ScreenCaptureConfig ReadConfig(string path) {
            var deserializer = new YamlDotNet.Serialization.Deserializer(namingConvention: new CamelCaseNamingConvention());
            using (var io = System.IO.File.OpenText(path))
            {
                var config = deserializer.Deserialize<ScreenCaptureConfig>(io);
                return config;
            }
        }
    }
}
