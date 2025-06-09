namespace DPMS_WebAPI.Constants
{
	/// <summary>
	/// User's group that essential to DPMS (always have those values in database). This does not
	/// include custom group created by Admin
	/// </summary>
	public readonly struct PermissionGroup
	{
		/// <summary>
		/// Admin group
		/// </summary>
		public const string ADMIN_DPMS = "admin_group";
		/// <summary>
		/// DPO (Data Protection Officer)
		/// </summary>
		public const string DPO = "DPO";
		public const string QAManager = "QA Manager"; // QA_Manager may edit member in DPIA, Risk Register, etc. 
        public const string BO = "BusinessOwner";
		public const string PD = "ProductDeveloper";
        public const string QA = "QA";
        public const string IT = "IT Manager";
		public const string Auditor = "Auditor"; // Auditor, ngang Permission QA_Manager
		public const string CTO_CIO = "CTO/CIO"; // CTO/CIO, ngang Permission DPO
    }
}
