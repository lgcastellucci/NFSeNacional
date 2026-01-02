using System.Drawing;

namespace NFSe_Nacional.Services
{
    public class LogService
    {
        public static void Log(string texto, Color? cor = null)
        {
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + ": " + texto);
        }
    }
}
