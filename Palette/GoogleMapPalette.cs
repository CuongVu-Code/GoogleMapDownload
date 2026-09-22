using Autodesk.AutoCAD.Windows;
using GoogleMapPlugin;//.Palette;
using System;
using System.Drawing;

namespace GoogleMapPlugin
{
    public static class GoogleMapPalette
    {
        private static PaletteSet _palette;

        public static void Show()
        {
            if (_palette == null)
            {
                _palette = new PaletteSet("Google Map");
                _palette.Size = new Size(300, 600);
                _palette.MinimumSize = new Size(300, 500);

                _palette.KeepFocus = true;

                GoogleMapControl control = new GoogleMapControl();

                _palette.Add("Google Map",control);
            }
            _palette.Visible = true;
        }
        public static void SetKeepFocus(bool value)
        {
            if (_palette != null)
            {
                _palette.KeepFocus = value;
            }
        }

    }
}