namespace DPMS_WebAPI.Constants
{
    /// <summary>
    /// List of all Features (which actually are Permissions) on DPMS
    /// </summary>
    public readonly struct Policies
    {
        /// <summary>
        /// only required user to logged in
        /// </summary>
        public const string Authenticated = "Authenticated";
        /// <summary>
        /// require user has certain permission to access
        /// </summary>
        public const string FeatureRequired = "FeatureRequired";
        
		
	}
}
