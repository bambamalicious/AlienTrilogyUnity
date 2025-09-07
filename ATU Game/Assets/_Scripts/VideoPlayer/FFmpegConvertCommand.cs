using FFmpegUnityBind2;
using System.Linq;
using static FFmpegUnityBind2.Instructions;

class FFmpegConvertCommand : BaseCommand
{

    public FFmpegConvertCommand(string inputPath, string outputPath) : base(inputPath, outputPath)
    {

    }

    //-i input.avi output.mp4
    public override string ToString()
    {
        return $"{REWRITE_INSTRUCTION} {INPUT_INSTRUCTION} {InputPaths.First()} {OutputPath}";
    }
}
