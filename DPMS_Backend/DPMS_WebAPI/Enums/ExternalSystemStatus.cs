namespace DPMS_WebAPI.Enums
{
	public enum ExternalSystemStatus
	{
		/// <summary>
		/// Khi he thong moi tao (chua dien FIC)
		/// </summary>
		WaitingForFIC = 1,
		/// <summary>
		/// BO hoac PD dien xong FIC
		/// </summary>
		WaitingForDPIA = 2,
		/// <summary>
		/// DPIA duoc tao cho he thong. DEPRECATED: THIS status is no longer in used
		/// </summary>
		DPIACreated = 3,
		/// <summary>
		/// DPIA cho he thong do duoc chap thuan
		/// </summary>
		Active = 4,
		/// <summary>
		/// ???
		/// </summary>
		
		Inactive = 5,
	}
}
