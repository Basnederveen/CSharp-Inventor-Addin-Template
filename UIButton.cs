using Inventor;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace $safeprojectname$.
{
    internal class UIButton
    { 
        public static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public ButtonDefinition ButtonDef { get; internal set; }
        public string AddInClientID { get; }
        public string DisplayName { get; }
        public string InternalName { get; }

        private Action<string> handler;

        public UIButton(string AddInClientID,
                        string DisplayName,
                        string InternalName,
                        System.Drawing.Icon Icon,
                        Action<string> Handler)
        {
            this.AddInClientID = AddInClientID;
            this.DisplayName = DisplayName;
            this.InternalName = InternalName;
            this.handler = Handler;

            var testDef = RetrieveDefinition(InternalName);
            if (testDef != null) { ButtonDef = testDef; return; }

            CreateButtonDefinition(DisplayName, InternalName, Icon, AddInClientID);
        }

        private void CreateButtonDefinition(string DisplayName, string InternalName, System.Drawing.Icon icon, string AddInClientID)
        {
            try
            {
                // Get the ControlDefinitions collection.
                ControlDefinitions controlDefs = Inv.Application.CommandManager.ControlDefinitions;

                // Create the command defintion.
                var icon16 = PictureDispConverter.ToIPictureDisp(new System.Drawing.Icon(icon, 16, 16));
                var icon32 = PictureDispConverter.ToIPictureDisp(new System.Drawing.Icon(icon, 32, 32));
                ButtonDef = controlDefs.AddButtonDefinition(DisplayName, InternalName,
                                Inventor.CommandTypesEnum.kShapeEditCmdType, AddInClientID, "", "",
                                icon16, icon32);

                // Add the handler
                ButtonDef.OnExecute += BtnDef_OnExecute;
            }
            catch
            {
                Logger.Error($"Failed to create the button definition");
                return;
            }
        }

        private static ButtonDefinition RetrieveDefinition(string InternalName)
        {
            // Check to see if a command already exists is the specified internal name.
            ButtonDefinition testDef = null;
            try
            {
                testDef = (Inventor.ButtonDefinition)Inv.Application.CommandManager.ControlDefinitions[InternalName];
            }
            catch
            {
            }

            if (testDef != null)
            {
                Logger.Error($"Error when adding the command {InternalName}. " +
                    "A command already exists with the same internal name. Each command must have a unique internal name. " +
                    "Change the internal name in the call to CreateButtonDefinition.");
                return testDef;
            }
            else
            {
                return null;
            }
        }

        private void BtnDef_OnExecute(NameValueMap Context)
        {
            this.handler(InternalName);
        }
    }
}
