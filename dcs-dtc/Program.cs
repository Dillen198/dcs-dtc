namespace DTC;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        // Use --winforms flag to run the legacy WinForms UI (for debugging/comparison)
        bool useWinForms = args.Contains("--winforms");

        if (useWinForms)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new New.UI.Base.MainForm());
        }
        else
        {
            var app = new New.WPF.App();
            app.Run();
        }
    }
}
