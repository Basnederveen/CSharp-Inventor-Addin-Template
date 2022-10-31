using Inventor;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace $safeprojectname$
{
    public abstract class StandardAddInServerBase : ApplicationAddInServer
    {
        public static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // Declaration of the object for the UserInterfaceEvents to be able to handle
        // if the user resets the ribbon so the button can be added back in.
        private UserInterfaceEvents _uiEvents;

        private List<RibbonTab> tabs = new List<RibbonTab>();
        private List<RibbonPanel> panels = new List<RibbonPanel>();

        public UserInterfaceEvents uiEvents
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _uiEvents;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_uiEvents != null)
                {
                    _uiEvents.OnResetRibbonInterface -= m_uiEvents_OnResetRibbonInterface;
                }

                _uiEvents = value;
                if (_uiEvents != null)
                {
                    _uiEvents.OnResetRibbonInterface += m_uiEvents_OnResetRibbonInterface;
                }
            }
        }

        // This property is provided to allow the AddIn to expose an API of its own to other 
        // programs. Typically, this  would be done by implementing the AddIn's API
        // interface in a class and returning that class object through this property.
        // Typically it's not used, like in this case, and returns Nothing.
        public object Automation
        {
            get
            {
                return null;
            }
        }

        public abstract string AddInClientID { get; }

        // This method is called by Inventor when the AddIn is unloaded. The AddIn will be
        // unloaded either manually by the user or when the Inventor session is terminated.
        public abstract void Deactivate();

        protected void BaseDeactivate()
        {
            uiEvents = null;
            Inv.Application = null;

            try
            {
                foreach (var panel in panels)
                {
                    try
                    {
                        panel.Delete();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Failed to delete panel {panel.DisplayName}", ex);
                        throw;
                    }
                }

                foreach (var tab in tabs)
                {
                    try
                    {
                        tab.Delete();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Failed to delete tab {tab.DisplayName}", ex);
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to delete panels or tabs", ex);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        // Note:this method is now obsolete, you should use the 
        // ControlDefinition functionality for implementing commands.
        public void ExecuteCommand(int commandID)
        {
        }

        protected abstract void InitializeLogger();

        private void m_uiEvents_OnResetRibbonInterface(NameValueMap Context)
        {
            // The ribbon was reset, so add back the add-ins user-interface.
            AddToUserInterface();
        }

        protected abstract void AddToUserInterface();

        protected RibbonPanel SetupPanel(string displayName, string internalName, RibbonTab ribbonTab)
        {
            RibbonPanel ribbonPanel = null;
            try
            {
                ribbonPanel = ribbonTab.RibbonPanels[internalName];
                Logger.Info($"Retrieved existing panel {internalName}");
            }
            catch
            {
            }

            if (ribbonPanel == null)
            {
                ribbonPanel = ribbonTab.RibbonPanels.Add(displayName, internalName, AddInClientID);
                Logger.Info($"Created new panel {internalName}");
            }

            panels.Add(ribbonPanel);
            return ribbonPanel;
        }


        protected RibbonTab SetupTab(string displayName, string internalName, Ribbon inventorRibbon)
        {
            RibbonTab ribbonTab = null;
            try
            {
                ribbonTab = inventorRibbon.RibbonTabs[internalName];
                Logger.Info($"Retrieved existing tab {internalName}");
            }
            catch
            {
            }

            if (ribbonTab == null)
            {
                ribbonTab = inventorRibbon.RibbonTabs.Add(displayName, internalName, AddInClientID);
                Logger.Info($"Created new tab {internalName}");
            }

            tabs.Add(ribbonTab);
            return ribbonTab;
        }

        public abstract void Activate(ApplicationAddInSite AddInSiteObject, bool FirstTime);
    }
}
