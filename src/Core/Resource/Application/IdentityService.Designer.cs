﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GamaEdtech.Resource.Application {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class IdentityService {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal IdentityService() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("GamaEdtech.Resource.Application.IdentityService", typeof(IdentityService).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error in retrieving settings..
        /// </summary>
        public static string ApplicationSettingsError {
            get {
                return ResourceManager.GetString("ApplicationSettingsError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Username already exists..
        /// </summary>
        public static string DuplicateUsername {
            get {
                return ResourceManager.GetString("DuplicateUsername", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The domain field is invalid..
        /// </summary>
        public static string InvalidDomain {
            get {
                return ResourceManager.GetString("InvalidDomain", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The user&apos;s IP is not present in the list of allowed IPs..
        /// </summary>
        public static string InvalidIpAddress {
            get {
                return ResourceManager.GetString("InvalidIpAddress", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Last admin cannot be removed..
        /// </summary>
        public static string LastAdminCantBeRemoved {
            get {
                return ResourceManager.GetString("LastAdminCantBeRemoved", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Password changes are limited to local users..
        /// </summary>
        public static string PasswordCantBeChanged {
            get {
                return ResourceManager.GetString("PasswordCantBeChanged", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The password field is required..
        /// </summary>
        public static string PasswordIsRequired {
            get {
                return ResourceManager.GetString("PasswordIsRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to If the domain field is selected, the password field should be left empty..
        /// </summary>
        public static string PasswordShouldBeEmpty {
            get {
                return ResourceManager.GetString("PasswordShouldBeEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Since this user account is in use, it can&apos;t be removed. Please consider deactivating the user instead..
        /// </summary>
        public static string UserCantBeRemoved {
            get {
                return ResourceManager.GetString("UserCantBeRemoved", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Your user account has been temporarily deactivated. Please try again in a few minutes..
        /// </summary>
        public static string UserLockedOut {
            get {
                return ResourceManager.GetString("UserLockedOut", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Your user account has been deactivated..
        /// </summary>
        public static string UserNotEnabled {
            get {
                return ResourceManager.GetString("UserNotEnabled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to User account was not found..
        /// </summary>
        public static string UserNotFound {
            get {
                return ResourceManager.GetString("UserNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Wrong username or password..
        /// </summary>
        public static string WrongUsernameOrPassword {
            get {
                return ResourceManager.GetString("WrongUsernameOrPassword", resourceCulture);
            }
        }
    }
}
