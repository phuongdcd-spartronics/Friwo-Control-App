﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FriwoControl.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.3.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string ICT_Log_Folder {
            get {
                return ((string)(this["ICT_Log_Folder"]));
            }
            set {
                this["ICT_Log_Folder"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("60")]
        public int ICT_Log_Interval {
            get {
                return ((int)(this["ICT_Log_Interval"]));
            }
            set {
                this["ICT_Log_Interval"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string ATE_Log_Folder {
            get {
                return ((string)(this["ATE_Log_Folder"]));
            }
            set {
                this["ATE_Log_Folder"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("60")]
        public int ATE_Log_Interval {
            get {
                return ((int)(this["ATE_Log_Interval"]));
            }
            set {
                this["ATE_Log_Interval"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("200")]
        public int UltSonic_Clean_Max {
            get {
                return ((int)(this["UltSonic_Clean_Max"]));
            }
            set {
                this["UltSonic_Clean_Max"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int UltSonic_Clean_Quantity {
            get {
                return ((int)(this["UltSonic_Clean_Quantity"]));
            }
            set {
                this["UltSonic_Clean_Quantity"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Sounds/success-sound.mp3")]
        public string Success_Sound_Uri {
            get {
                return ((string)(this["Success_Sound_Uri"]));
            }
            set {
                this["Success_Sound_Uri"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Sounds/error-sound.mp3")]
        public string Error_Sound_Uri {
            get {
                return ((string)(this["Error_Sound_Uri"]));
            }
            set {
                this["Error_Sound_Uri"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string Box_Printer {
            get {
                return ((string)(this["Box_Printer"]));
            }
            set {
                this["Box_Printer"] = value;
            }
        }
    }
}