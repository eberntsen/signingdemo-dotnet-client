using System.Threading.Tasks;

namespace Console {
    public class Program {
        private static async Task Main(string[] args) {
            await SampleRSA.Run(args);
        }
    }
} 