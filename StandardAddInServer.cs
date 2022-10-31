
using Inventor;
using log4net;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace $safeprojectname$
{
    [ProgId("$safeprojectname$.StandardAddInServer")]
    [Guid("$guid1$")]
    public class StandardAddInServer : StandardAddInServerBase
    {
        // Declare all buttons here
        ButtonDefinition SampleButtonDef;

        // Declare tab and panel here. Should be deleted on de-activate otherwise an error occurs on reloading
        private RibbonTab tab;
        private RibbonPanel panel;

        // Store the AddInClientID here
        // Required for setup tab/panel methods in base class
        public override string AddInClientID { get => typeof(StandardAddInServer).GetCustomAttributes(typeof(GuidAttribute), false)[0].ToString(); }
        public bool ValidUser { get; private set; }

        // This method is called by Inventor when it loads the AddIn. The AddInSiteObject provides access  
        // to the Inventor Application object. The FirstTime flag indicates if the AddIn is loaded for
        // the first time. However, with the introduction of the ribbon this argument is always true.
        public override void Activate(ApplicationAddInSite addInSiteObject, bool firstTime)
        {
            try
            {
                // Initialize AddIn members.
                Inv.Application = addInSiteObject.Application;

                // Initialize logging
                InitializeLogger();
                Logger.Info($"Logging initialized");

                ValidUser = true;
                ValidUser = AutodeskEntitlement.ValidUser(Inv.Application, AddInClientID);

                CreateButtonDefinitions();

                // Add to the user interface, if it's the first time.
                // If this add-in doesn't have a UI but runs in the background listening
                // to events, you can delete this.
                if (firstTime)
                {
                    AddToUserInterface();
                    Logger.Info($"Added $safeprojectname$ button definitions to user interface");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected failure in the activation of the $safeprojectname$ add-in.\n" +
                                $"Please restart inventor. \n\n {ex.StackTrace}");
            }
        }

        // This method is called by Inventor when the AddIn is unloaded. The AddIn will be
        // unloaded either manually by the user or when the Inventor session is terminated.
        public override void Deactivate()
        {
            SampleButtonDef.Delete();
            SampleButtonDef = null;

            // Must call this method in base class to deactivate the default members
            BaseDeactivate();
        }


        // Initialize the logger
        protected override void InitializeLogger()
        {
            var logRepository = LogManager.GetRepository();
            var executingAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            Logging.InitializeLogger(logRepository, executingAssembly);
        }

        // Adds whatever is needed by this add-in to the user-interface.  This is 
        // called when the add-in loaded and also if the user interface is reset.
        protected override void AddToUserInterface()
        {
            Ribbon asmRibbon = Inv.Application.UserInterfaceManager.Ribbons["Assembly"];
            tab = asmRibbon.RibbonTabs["id_TabAssemble"];
            panel = SetupPanel("Sample", "BAS_SAMPLE_PANEL", tab);

            panel.CommandControls.AddButton(SampleButtonDef);
        }

        // This method can be used to create all button definitions
        private void CreateButtonDefinitions()
        {
            SampleButtonDef = new UIButton(AddInClientID,
                                        "Sample",
                                        "BAS_SAMPLE_CMD",
                                        null,       // icon to be added
                                        $safeprojectname$).ButtonDef;
        }


        public void $safeprojectname$(string s)
        {
            MessageBox.Show($"This is a sample");
        }
    }
}
