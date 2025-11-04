using System.Runtime.CompilerServices;
using System.Text;

namespace PurAnalyzer.Tests;

public static class EncodingInit
{
    [ModuleInitializer]
    public static void Init()
    {
        // Enables Windows-1250 / ISO-8859-2, etc. for the whole test process
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }
}
