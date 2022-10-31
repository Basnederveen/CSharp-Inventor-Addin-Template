using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace $safeprojectname$
{
    internal static class Inv
    {
        public static Inventor.Application Application { get; set; }
        public static Inventor.TransientGeometry TG { get => Application.TransientGeometry; }
        public static Inventor.TransientObjects TO { get => Application.TransientObjects; }

        public static void Connect()
        {
            try
            {
                Application = (Inventor.Application)Marshal.GetActiveObject("Inventor.Application");
            }
            catch
            {
                try
                {
                    Type invAppType = Type.GetTypeFromProgID("Inventor.Application");
                    Application = (Inventor.Application)System.Activator.CreateInstance(invAppType);
                    Application.Visible = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to connect to inventor. \n\n{ex.StackTrace}");
                }
            }
        }
    }
}
