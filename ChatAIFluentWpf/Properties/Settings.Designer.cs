﻿//------------------------------------------------------------------------------
// <auto-generated>
//     このコードはツールによって生成されました。
//     ランタイム バージョン:4.0.30319.42000
//
//     このファイルへの変更は、以下の状況下で不正な動作の原因になったり、
//     コードが再生成されるときに損失したりします。
// </auto-generated>
//------------------------------------------------------------------------------

namespace ChatAIFluentWpf.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.5.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://uchi-chataiwpf-kv.vault.azure.net/")]
        public string AzureKeyVaultUri {
            get {
                return ((string)(this["AzureKeyVaultUri"]));
            }
            set {
                this["AzureKeyVaultUri"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("5deaea0f-acc6-4c71-aa8b-ccf5f1f3a343")]
        public string AzureClientID {
            get {
                return ((string)(this["AzureClientID"]));
            }
            set {
                this["AzureClientID"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("39ff9aae-e121-4388-83f4-b66586365fdd")]
        public string AzureTenantID {
            get {
                return ((string)(this["AzureTenantID"]));
            }
            set {
                this["AzureTenantID"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("IoU8Q~P-KiAcFvaEY~08BI~rgyhg_mmyywp~rbIB")]
        public string AzureClientSecret {
            get {
                return ((string)(this["AzureClientSecret"]));
            }
            set {
                this["AzureClientSecret"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int VoiceVoxSpeakerId {
            get {
                return ((int)(this["VoiceVoxSpeakerId"]));
            }
            set {
                this["VoiceVoxSpeakerId"] = value;
            }
        }
    }
}
