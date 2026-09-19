using Autodesk.AutoCAD.Runtime;

namespace GoogleMapPlugin
{
    public class GoogleMapCommand
    {
        [CommandMethod("GGMAP")]
        public void GGMAP()
        {
            GoogleMapPalette.Show();
        }
    }
}