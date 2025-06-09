-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		ThangHQ
-- Create date: 23/03/2025
-- Description: Export dữ liệu FIC
-- =============================================
CREATE PROCEDURE sp_dpms_export_fic_submission
	-- Add the parameters for the stored procedure here
	@SubmissionId UNIQUEIDENTIFIER = NULL
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	WITH FormHierarchy AS (
		-- Step 1: Select Parent Elements (Top-Level Nodes)
		SELECT 
			Id, 
			ParentId, 
			FormId,
			Name, 
			OrderIndex, 
			0 AS HierarchyLevel,  -- Root level
			CAST(OrderIndex AS VARCHAR(MAX)) AS SortPath
		FROM FormElements
		WHERE ParentId IS NULL  -- Root elements (no parent)

		UNION ALL

		-- Step 2: Recursively Select Child Elements
		SELECT 
			fe.Id, 
			fe.ParentId, 
			fe.FormId,
			fe.Name, 
			fe.OrderIndex, 
			fh.HierarchyLevel + 1 AS HierarchyLevel, 
			CAST(fh.SortPath + '.' + CAST(fe.OrderIndex AS VARCHAR(MAX)) AS VARCHAR(MAX)) AS SortPath
		FROM FormElements fe
		INNER JOIN FormHierarchy fh ON fe.ParentId = fh.Id
	)

	-- Step 3: Order by Hierarchy (Parents First, then Children by OrderIndex)
	SELECT 
		FormHierarchy.*,
		Submissions.Id AS [SubmissionId],
		FormResponses.Value
	FROM FormHierarchy
		LEFT JOIN Submissions ON FormHierarchy.FormId = Submissions.FormId
		LEFT JOIN FormResponses ON (submissions.Id = FormResponses.SubmissionId AND FormHierarchy.Id = FormResponses.FormElementId) 
	WHERE
		Submissions.Id = @SubmissionId
	ORDER BY SortPath;
END
GO
