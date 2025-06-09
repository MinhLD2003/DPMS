-- Seed Users
INSERT INTO Users (Id, FullName, Dob, Email, IsPasswordConfirmed, IsEmailConfirmed, UserName, Status, CreatedAt, LastModifiedAt, Salt, Password) VALUES
(
	'E02EC95C-5C94-470A-88EA-29CC32BCA9D5',
	'ADMIN',
	'2003/07/16',
	'admin@gmail.com',
	1,
	1,
	'Admin',
	1,
	GETDATE(),
	GETDATE(),
	'zEPs7tOhpajqv28jPmSs6McxNeBfTV65ZWj91BFTOnef/uGdhXOeI/UwSNuyhxNQ9mrDDR5hLsRSdZLUpUuUeR75gocaaerUxOEpApudRo7K87t3Xhb1ApT56xDXQ4g+Y/AdOmr88vSEmKmUvVQ//bC5M6a29i+OAmWNBHC8PZY=',
	'uiUmGH8wfDNr56NqnmQso3afhM0+MdGBb/IW+KnCJMU=' -- Password: 1672003Dts@
),
(
	'E02EC95C-5C94-470A-88EA-29CC32BCA9F5',
	'SKADICE',
	'2003/09/26',
	'Skadice1@gmail.com',
	1,
	1,
	'SKADICE',
	1,
	GETDATE(),
	GETDATE(),
	'zEPs7tOhpajqv28jPmSs6McxNeBfTV65ZWj91BFTOnef/uGdhXOeI/UwSNuyhxNQ9mrDDR5hLsRSdZLUpUuUeR75gocaaerUxOEpApudRo7K87t3Xhb1ApT56xDXQ4g+Y/AdOmr88vSEmKmUvVQ//bC5M6a29i+OAmWNBHC8PZY=',
	'uiUmGH8wfDNr56NqnmQso3afhM0+MdGBb/IW+KnCJMU=' -- Password: 1672003Dts@
);
GO -- Password: !672003Dts@
INSERT [dbo].[Users] ([Id], [UserName], [FullName], [Dob], [Email], [Salt], [Password], [IsEmailConfirmed], [IsPasswordConfirmed], [LastTimeLogin], [Status], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'cb46df4a-fe3e-4012-bc01-1bdf46329b86', N'Product_Dev_SyTT2', N'Trương Tiến Sỹ', CAST(N'0001-01-01T00:00:00.0000000' AS DateTime2), N'sy_product_dev@gmail.com', N'/mUqNq5+LRjbGrV0CrAp0OzcbqX7phPtm1zWrW1Z4zheUoQg+MbSnSP6chugRnDINS40RzfFxymZeuEsmyN0lyDFnQZkDvmfJGmuITRSUW+SzBSGblbWFBbJqXUM3gkYVShbJTUrDCkrAJJ/ORmpgLIpSmxpKQiDtvWU2RkaCmc=', N'BKrZ8u+Ov+mtve0NXfVGuaqgoGdhtUSnEP05gKuiAiA=', 0, 1, NULL, 0, CAST(N'2025-03-21T22:40:27.9642586' AS DateTime2), CAST(N'2025-03-21T22:40:27.9642586' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[Users] ([Id], [UserName], [FullName], [Dob], [Email], [Salt], [Password], [IsEmailConfirmed], [IsPasswordConfirmed], [LastTimeLogin], [Status], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'51e846db-3e8a-4f7a-820b-e371a8542015', N'MinhLD_BO', N'Lã Đức Minh', CAST(N'0001-01-01T00:00:00.0000000' AS DateTime2), N'Business_Owner_MinhLD@gmail.com', N'8YAdFQk0vqAT5tFnoXEIMMTKCA3Fy5gLZgzGN1vkMB0oboJUOxw33ciOzBTRZOqNpUs89wVR+phhCrlhWHj5IkL+wYVjFruHd51wbeFYw+X7hvsWvHH11hr122PX+A53V6PtN/skdWPBWLIi6k3ntwfUnKO5Y3rmOgKf/GRa/jg=', N'CVpidF2eEMR1++f9EcfeaqZS0SrYAz1moR++ievqq5A=', 0, 1, NULL, 0, CAST(N'2025-03-21T22:41:17.2849362' AS DateTime2), CAST(N'2025-03-21T22:41:17.2849362' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
-- DPO
INSERT [dbo].[Users] ([Id], [UserName], [FullName], [Dob], [Email], [Salt], [Password], [IsEmailConfirmed], [IsPasswordConfirmed], [LastTimeLogin], [Status], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'b1f5f161-81be-4761-a72e-ff8572adb15f', N'HungTD47', N'Trần Đức Hùng', CAST(N'0001-01-01T00:00:00.0000000' AS DateTime2), N'hungtd_qa@gmail.com', N'/JGnClKupStFdS8uzI5SmuPGVs2/VkprSm5JhBfYQvBj1x09mIZGsTtfycSXPEcpKgK9D+Gr5OoAnfN7ID1o2DED/86hlAtsC18HQI8nbZyt5mQa+0ObQpqk5PsojmNacM8a2SE3DJ1cxPKuok4J0vhjD5fXKqPbcNDXASFlWcM=', N'qykwgPp/xkrfq84aYQ2LQlKNwtrMWJ/AnaxCkaBOnFM=', 0, 1, NULL, 0, CAST(N'2025-03-22T16:25:44.6990417' AS DateTime2), CAST(N'2025-03-22T16:25:44.6990417' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[Users] ([Id], [UserName], [FullName], [Dob], [Email], [Salt], [Password], [IsEmailConfirmed], [IsPasswordConfirmed], [LastTimeLogin], [Status], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'b6d22dfe-23c3-4c74-8baf-b0af560ad251', N'ManhNC5', N'Nguyễn Cường Mạnh', CAST(N'0001-01-01T00:00:00.0000000' AS DateTime2), N'manhnc5_dpo@gmail.com', N'Az2h8fz5+0MLfnOhR8Z/GQVr2FAzKovNHjoNR+ISLxDQXAkbwFkWVdRSrfL1BfxyisGLjF91WQzBIl0uabFaFpYZkvSxfobMI48giRF5bladnEHxxFoGlaxtbL2NXV99vWTb6m6qi3I+/TRfTV/TqB1HNBZbFFAPW2yS0Aw58pg=', N'DFkVJA0/negVKWxj7uyYzmuj0TNehae5cC719ApSA6E=', 0, 1, CAST(N'2025-03-24T17:05:34.7567910' AS DateTime2), 1, CAST(N'2025-03-24T16:20:15.4510255' AS DateTime2), CAST(N'2025-03-24T17:05:34.7569998' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
--GO
----INSERT [dbo].[Users] ([Id], [UserName], [FullName], [Dob], [Email], [Salt], [Password], [IsEmailConfirmed], [IsPasswordConfirmed], [LastTimeLogin], [Status], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'423e65e4-7017-4699-a9c9-1870dd3418a6', N'TaiNT', N'Nguyễn Trung Tài', CAST(N'0001-01-01T00:00:00.0000000' AS DateTime2), N'taint_pd_edunext@gmail.com', N'6Yjyqv9TrZrjudLxqBKPrEaO/K+ZT1Gl9UpgLp89hPrYZYZnqlT2nOyA6o15jXC7akPyYy1U6R4baS1b4naScJAxPCMkg700Y5Dq/YtcGPCnJOJgEa4ZVRK6nomAjsSR/eDuSHQTUgwo8CUiaWtAWV2dyJGDHSmzzIOdwerUUvM=', N'PmiqS9rptqUyInRBkWXSvstNhNHayEP9kJ37yVjRCDs=', 0, 1, CAST(N'2025-03-24T16:47:28.9731369' AS DateTime2), 1, CAST(N'2025-03-24T16:38:12.4270633' AS DateTime2), CAST(N'2025-03-24T16:47:28.9732290' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
--GO
--INSERT [dbo].[Users] ([Id], [UserName], [FullName], [Dob], [Email], [Salt], [Password], [IsEmailConfirmed], [IsPasswordConfirmed], [LastTimeLogin], [Status], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'8de957ed-0d26-4bda-a706-e430519b14b2', N'DuongPX', N'Phùng Xuân Dương', CAST(N'0001-01-01T00:00:00.0000000' AS DateTime2), N'duongpx_bo_edunext@gmail.com', N'cf4iSwnOZTwjS21COv19Kd+Wr7wxVV7KJfA+G2iKAVrkPYkLV83r/aJgVzfNMCwpFFpzEDzlQeAyNgaSIIQcxv2Gg0rmOU+lMqmx8w4nwALVpAYFvmtzrQ1COehwoCMOx/d88oKPw60hj4vAe8JfIBG4e6TVT+g16Dulcs+fMeI=', N'd+PEt9yqsLIk0lF0qsbFJK+CpneUy5zfBzc0hmUSaLU=', 0, 1, NULL, 1, CAST(N'2025-03-24T16:38:49.1018560' AS DateTime2), CAST(N'2025-03-24T16:38:49.1018560' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO

-- Seed Groups
INSERT INTO Groups(Id, Name, Description, IsGlobal, CreatedAt, LastModifiedAt) VALUES
(
	'15AA9BA9-B966-4507-AE74-E515CDB22DF4',
	'admin_group',
	N'Quản trị viên DPMS',
	1,
	GETDATE(),
	GETDATE()
),
(
	'2681C3B3-6A8D-46DD-0480-08DD63757738',
	'BusinessOwner',
	N'Group global cho BO',
	1,
	GETDATE(),
	GETDATE()
),
(
	'53C81624-AA17-4F1F-0481-08DD63757738',
	'ProductDeveloper',
	N'Group global cho PD',
	1,
	GETDATE(),
	GETDATE()
),
(
	'F09C3AA7-8AFD-4CF9-44FB-08DD637AF9AD',
	'DPO',
	N'Group global cho DPO tổ chức giáo dục FPT Edu',
	1,
	GETDATE(),
	GETDATE()
);
GO
INSERT [Groups] ([Id], [Name], [Description], [IsGlobal], [SystemId], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'b0243eb8-e021-41ed-f64b-08dd70369347', N'IT Manager', N'IT Manager', 1, NULL, CAST(N'2025-03-31T16:29:45.8033659' AS DateTime2), CAST(N'2025-03-31T16:29:45.8033659' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Groups] ([Id], [Name], [Description], [IsGlobal], [SystemId], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'b33833c3-0fbc-4cc8-f64c-08dd70369347', N'CTO/CIO', N'Ban lãnh đạo', 1, NULL, CAST(N'2025-03-31T16:29:56.8556907' AS DateTime2), CAST(N'2025-03-31T16:29:56.8556907' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Groups] ([Id], [Name], [Description], [IsGlobal], [SystemId], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'57881a5f-66b5-4a75-f64d-08dd70369347', N'QA', N'Quality Assurance', 1, NULL, CAST(N'2025-03-31T16:30:12.7508113' AS DateTime2), CAST(N'2025-03-31T16:30:12.7508113' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Groups] ([Id], [Name], [Description], [IsGlobal], [SystemId], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'6b5f1412-2f21-429a-f64e-08dd70369347', N'QA Manager', N'Trưởng bộ phận QA (Quality Assurance)', 1, NULL, CAST(N'2025-03-31T16:30:40.1302314' AS DateTime2), CAST(N'2025-03-31T16:30:40.1302314' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO

-- Seed dữ liệu ExternalSystems
GO
INSERT [ExternalSystems] ([Id], [Name], [Description], [Status], [ApiKeyHash], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'1c9b5694-f2ff-4648-b50f-a93bbe30d941', N'DPMS', N'Data Privacy Management System for FPT Education', 1, N'cc2e161dce98ef507c453c3bf7403028c3c0e6a788d8766d89d522995543b965', CAST(N'2025-03-21T22:50:59.1447608' AS DateTime2), CAST(N'2025-04-07T12:30:49.5727167' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
-- Insert Group BO/PD cho External System FAP (DEMO)
GO
INSERT [dbo].[Groups] ([Id], [Name], [Description], [IsGlobal], [SystemId], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'f1dd5e45-416c-46ff-940b-2f44cce829e0', N'BO DPMS', N'Business Owner Group for DPMS', 0, N'1c9b5694-f2ff-4648-b50f-a93bbe30d941', CAST(N'2025-03-22T16:09:10.9442531' AS DateTime2), CAST(N'2025-03-22T16:09:10.9442531' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[Groups] ([Id], [Name], [Description], [IsGlobal], [SystemId], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'2c8339ec-bcf4-48ad-b996-3afddc2987f0', N'PD DPMS', N'Product Developer Group for DPMS', 0, N'1c9b5694-f2ff-4648-b50f-a93bbe30d941', CAST(N'2025-03-22T16:09:10.9442531' AS DateTime2), CAST(N'2025-03-22T16:09:10.9442531' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
---- EDUNEXT
--INSERT [dbo].[ExternalSystems] ([Id], [Name], [Description], [Status], [ApiKeyHash], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'0b39b8d6-670a-4543-9da3-2d0ff1ef1871', N'EduNext', N'Hệ thống học tập kiến tạo', 1, NULL, CAST(N'2025-03-24T16:41:41.7893919' AS DateTime2), CAST(N'2025-03-24T16:41:41.7893919' AS DateTime2), N'b6d22dfe-23c3-4c74-8baf-b0af560ad251', N'b6d22dfe-23c3-4c74-8baf-b0af560ad251')
---- BO/PD Group cho EDUNEXT
--INSERT [dbo].[Groups] ([Id], [Name], [Description], [IsGlobal], [SystemId], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'66058b14-2e5c-454e-8bac-8b8d79ca81ab', N'PD EduNext', N'Product Developer Group for EduNext', 0, N'0b39b8d6-670a-4543-9da3-2d0ff1ef1871', CAST(N'2025-03-24T16:41:41.8159856' AS DateTime2), CAST(N'2025-03-24T16:41:41.8159856' AS DateTime2), N'b6d22dfe-23c3-4c74-8baf-b0af560ad251', N'b6d22dfe-23c3-4c74-8baf-b0af560ad251')
--GO
--INSERT [dbo].[Groups] ([Id], [Name], [Description], [IsGlobal], [SystemId], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'fed54f07-3e74-45a8-9f92-8f28c4be9119', N'BO EduNext', N'Business Owner Group for EduNext', 0, N'0b39b8d6-670a-4543-9da3-2d0ff1ef1871', CAST(N'2025-03-24T16:41:41.8159856' AS DateTime2), CAST(N'2025-03-24T16:41:41.8159856' AS DateTime2), N'b6d22dfe-23c3-4c74-8baf-b0af560ad251', N'b6d22dfe-23c3-4c74-8baf-b0af560ad251')

-- Insert HungDT47 vào Group DPO
INSERT [dbo].[UserGroups] ([Id], [UserId], [GroupId], [IsPic], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'c6be09bd-eef0-40bb-83e3-4f477eab5fd1', N'b1f5f161-81be-4761-a72e-ff8572adb15f', N'f09c3aa7-8afd-4cf9-44fb-08dd637af9ad', 0, CAST(N'2025-03-22T16:27:03.8049578' AS DateTime2), CAST(N'2025-03-22T16:27:03.8049578' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
-- Insert ManhNC5 vào DPO
INSERT [dbo].[UserGroups] ([Id], [UserId], [GroupId], [IsPic], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'b963bca5-607e-4279-a647-1dfd9c131139', N'b6d22dfe-23c3-4c74-8baf-b0af560ad251', N'f09c3aa7-8afd-4cf9-44fb-08dd637af9ad', 0, CAST(N'2025-03-24T16:35:42.7630353' AS DateTime2), CAST(N'2025-03-24T16:35:42.7630353' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
---- Insert TaiNT vào PD EDUNEXT và PD global
--INSERT [dbo].[UserGroups] ([Id], [UserId], [GroupId], [IsPic], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'00680428-e43e-4b5a-b5ab-df326650c8a6', N'423e65e4-7017-4699-a9c9-1870dd3418a6', N'66058b14-2e5c-454e-8bac-8b8d79ca81ab', 0, CAST(N'2025-03-24T16:41:41.8574519' AS DateTime2), CAST(N'2025-03-24T16:41:41.8574519' AS DateTime2), N'b6d22dfe-23c3-4c74-8baf-b0af560ad251', N'b6d22dfe-23c3-4c74-8baf-b0af560ad251')
--GO
--INSERT [dbo].[UserGroups] ([Id], [UserId], [GroupId], [IsPic], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'5b005496-b08a-4b85-9970-e15b69afc2a9', N'423e65e4-7017-4699-a9c9-1870dd3418a6', N'53c81624-aa17-4f1f-0481-08dd63757738', 0, CAST(N'2025-03-24T16:40:16.2601584' AS DateTime2), CAST(N'2025-03-24T16:40:16.2601584' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
--GO
---- Insert DuongPX vào BO EduNext và BO Global
--INSERT [dbo].[UserGroups] ([Id], [UserId], [GroupId], [IsPic], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'7697b8c1-7c42-4d58-8a60-8fd9895d4713', N'8de957ed-0d26-4bda-a706-e430519b14b2', N'2681c3b3-6a8d-46dd-0480-08dd63757738', 0, CAST(N'2025-03-24T16:40:08.3515798' AS DateTime2), CAST(N'2025-03-24T16:40:08.3515798' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
--GO
--INSERT [dbo].[UserGroups] ([Id], [UserId], [GroupId], [IsPic], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'1ba71c5c-a579-4602-b808-f4d0598af191', N'8de957ed-0d26-4bda-a706-e430519b14b2', N'fed54f07-3e74-45a8-9f92-8f28c4be9119', 0, CAST(N'2025-03-24T16:41:41.8574519' AS DateTime2), CAST(N'2025-03-24T16:41:41.8574519' AS DateTime2), N'b6d22dfe-23c3-4c74-8baf-b0af560ad251', N'b6d22dfe-23c3-4c74-8baf-b0af560ad251')
--GO


-- Seed UserInGroups
INSERT INTO UserGroups (Id, UserId, GroupId, IsPic, CreatedAt, LastModifiedAt) VALUES
( -- Gán admin vào admin_group
	'25FF451F-79C2-49E9-9A2D-3AF75007F253',
	'E02EC95C-5C94-470A-88EA-29CC32BCA9D5',
	'15AA9BA9-B966-4507-AE74-E515CDB22DF4',
	0,
	GETDATE(),
	GETDATE()
);
GO
INSERT [dbo].[UserGroups] ([Id], [UserId], [GroupId], [IsPic], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'b2f37686-c2bd-4dc5-a47d-4ca26942ab45', N'51e846db-3e8a-4f7a-820b-e371a8542015', N'2681c3b3-6a8d-46dd-0480-08dd63757738', 0, CAST(N'2025-03-21T22:42:05.3649115' AS DateTime2), CAST(N'2025-03-21T22:42:05.3649115' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[UserGroups] ([Id], [UserId], [GroupId], [IsPic], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'65b62088-48ad-4024-9877-77dd32a9c743', N'cb46df4a-fe3e-4012-bc01-1bdf46329b86', N'53c81624-aa17-4f1f-0481-08dd63757738', 0, CAST(N'2025-03-21T22:42:10.5138677' AS DateTime2), CAST(N'2025-03-21T22:42:10.5138677' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[UserGroups] ([Id], [UserId], [GroupId], [IsPic], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'fc82448e-2e2c-456d-aa35-fcc60a917884', N'cb46df4a-fe3e-4012-bc01-1bdf46329b86', N'f1dd5e45-416c-46ff-940b-2f44cce829e0', 0, CAST(N'2025-03-21T22:50:59.3959710' AS DateTime2), CAST(N'2025-03-21T22:50:59.3959710' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[UserGroups] ([Id], [UserId], [GroupId], [IsPic], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'79f9cedd-b28f-444a-9297-ffee7d4df828', N'51e846db-3e8a-4f7a-820b-e371a8542015', N'2c8339ec-bcf4-48ad-b996-3afddc2987f0', 0, CAST(N'2025-03-21T22:50:59.3959710' AS DateTime2), CAST(N'2025-03-21T22:50:59.3959710' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO

-- Seed dữ liệu Features
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'f51a0ccc-928c-45b3-af19-01443946f073', N'ManageUsers', N'Quản lý người dùng', NULL, 1, N'', NULL, CAST(N'2025-03-16T21:41:29.7400000' AS DateTime2), CAST(N'2025-03-16T21:41:29.7400000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'10ec4d72-e9ce-48b5-87aa-06852206a167', N'ManagePurpose', N'Quản lý purpose', NULL, 0, NULL, NULL, CAST(N'2025-04-13T16:19:55.4144614' AS DateTime2), CAST(N'2025-04-13T16:19:55.4144614' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'65cd3b28-dfe2-4a2d-922a-05eefd83b43c', N'DPIAManagement', N'Quản lý DPIA', NULL, 0, NULL, NULL, CAST(N'2025-04-13T16:36:39.3922809' AS DateTime2), CAST(N'2025-04-13T16:36:39.3922809' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'1ce0f64c-21ff-4f25-9c51-0a2054cdc562', N'ManageConsent', N'Quản lý thu thập sự đồng ý', NULL, 0, NULL, NULL, CAST(N'2025-03-31T16:36:07.0474349' AS DateTime2), CAST(N'2025-03-31T16:36:07.0474349' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'91678d66-0828-4981-a5b0-0b8b9f55a444', N'DSARManagement', N'Quản lý yêu cầu của chủ thể dữ liệu (DSAR)', NULL, 0, NULL, NULL, CAST(N'2025-04-13T03:26:40.0264149' AS DateTime2), CAST(N'2025-04-13T10:26:40.0270285' AS DateTime2), NULL, N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'31c1ad41-5238-42c8-88fa-2eaad0acb7ce', N'ManageGroups', N'Quản lý Groups', NULL, 1, N'', NULL, CAST(N'2025-03-16T21:41:29.7400000' AS DateTime2), CAST(N'2025-03-16T21:41:29.7400000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'7200c019-5e23-4493-8e20-44867d0b3e33', N'ManageFeatures', N'Quản lý Features', NULL, 1, N'', NULL, CAST(N'2025-03-16T21:41:29.7400000' AS DateTime2), CAST(N'2025-03-16T21:41:29.7400000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'3754c65e-86b1-451e-83e8-a0510e658d96', N'ManageTickets', N'Quản lý Tickets', NULL, 1, N'', NULL, CAST(N'2025-03-16T21:41:29.7400000' AS DateTime2), CAST(N'2025-03-16T21:41:29.7400000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'213b812f-2973-4c73-8b0f-bd6f9a195396', N'ManageExternalSystems', N'Quản lý External System', NULL, 0, NULL, NULL, CAST(N'2025-04-13T17:59:27.6896720' AS DateTime2), CAST(N'2025-04-13T17:59:27.6896720' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'1791ec64-40c4-4fb6-b934-ccfdcac23324', N'ManagePrivacyPolicy', N'Quản lý Privacy Policy', NULL, 0, NULL, NULL, CAST(N'2025-04-13T16:25:27.9316555' AS DateTime2), CAST(N'2025-04-13T16:25:27.9316555' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'a24ffad0-5e1c-49c0-9465-da3bb76ae090', N'ManageForms', N'Quản lý Forms (FIC +DPIA)', NULL, 1, N'', NULL, CAST(N'2025-03-16T21:41:29.7400000' AS DateTime2), CAST(N'2025-03-16T21:41:29.7400000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'0da130db-3cce-44c2-98dc-e104383c2e18', N'FileManagement', N'Quản lý File trên S3', NULL, 0, NULL, NULL, CAST(N'2025-04-13T15:46:28.6262889' AS DateTime2), CAST(N'2025-04-13T15:46:28.6262889' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'8cd84955-4dd5-47f3-8f04-c8e447a223aa', N'ManageRisk', N'Quản lý rủi ro', NULL, 0, NULL, NULL, CAST(N'2025-03-31T16:47:02.0631619' AS DateTime2), CAST(N'2025-03-31T16:47:02.0631619' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'e52ee063-84f1-412b-a68b-f58fa882d19e', N'ResponsibilityManagement', N'Quản lý Responsibility', NULL, 0, NULL, NULL, CAST(N'2025-04-13T18:13:37.4428507' AS DateTime2), CAST(N'2025-04-13T18:13:37.4428507' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO

INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'18e78f1f-218f-4ae9-b68d-014c7f941a95', N'Get DPIA Members', N'Retrieve members involved in a DPIA', N'65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, N'/api/DPIA/{id}/members', 0, CAST(N'2025-04-13T17:54:45.8100000' AS DateTime2), CAST(N'2025-04-13T17:54:45.8100000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'd92643e3-b8d5-4a8e-9caf-0440510353c0', N'Export consent logs', N'Export dữ liệu consent log.', N'1ce0f64c-21ff-4f25-9c51-0a2054cdc562', 0, N'/api/Consent/export-logs', 0, CAST(N'2025-03-31T16:37:03.2064798' AS DateTime2), CAST(N'2025-03-31T16:37:03.2064798' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'd0666efe-9509-4ae0-82e7-0456272fd7ba', N'UpdatePrivacyPolicy', N'Cập nhật chính sách quyền riêng tư', N'1791ec64-40c4-4fb6-b934-ccfdcac23324', 1, N'/api/PrivacyPolicy/{id}', 2, CAST(N'2025-04-13T16:27:33.8033333' AS DateTime2), CAST(N'2025-04-13T16:27:33.8033333' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'3b0419bf-b1a9-44d1-942e-06cb3292b7f3', N'Update Responsibility', N'Update an existing responsibility', N'e52ee063-84f1-412b-a68b-f58fa882d19e', 1, N'/api/Responsibility/{id}', 2, CAST(N'2025-04-13T18:15:24.2766667' AS DateTime2), CAST(N'2025-04-13T18:15:24.2766667' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'4886f1be-de11-48dd-a2ad-0726004953a0', N'Export FIC Submission', N'Export FIC Submission', N'a24ffad0-5e1c-49c0-9465-da3bb76ae090', 0, N'/api/Form/export-submission/{id}', 0, CAST(N'2025-03-31T16:42:33.2629137' AS DateTime2), CAST(N'2025-03-31T16:42:33.2629137' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'4328fbcf-7308-4f91-8ab1-076373a47c1c', N'ResolveRisk', N'Cập nhật trạng thái giải quyết rủi ro', N'8cd84955-4dd5-47f3-8f04-c8e447a223aa', 1, N'/api/Risk/resolve-risk/{id}', 2, CAST(N'2025-04-13T15:57:38.6766667' AS DateTime2), CAST(N'2025-04-13T15:57:38.6766667' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'fd6c6c44-1f74-44fc-8b9e-0918f41f0b31', N'UpdateDSARStatus', N'Thay đổi status của DSAR', N'91678d66-0828-4981-a5b0-0b8b9f55a444', 0, N'/api/Dsar/update-status', 2, CAST(N'2025-04-13T10:31:45.8349295' AS DateTime2), CAST(N'2025-04-13T10:31:45.8349295' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'97826bc5-9080-4cd5-a30a-09ab207ad4de', N'Upload Documents to Responsibility', N'Upload documents for a DPIA responsibility', N'65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 0, N'/api/DPIA/{id}/{id}/upload-documents', 1, CAST(N'2025-04-13T10:55:40.1981272' AS DateTime2), CAST(N'2025-04-13T17:55:40.2200295' AS DateTime2), NULL, N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'ba1b049d-8ac9-4bbb-91d8-0c9f523759d5', N'Get Form Details', N'Get form details by ID', N'a24ffad0-5e1c-49c0-9465-da3bb76ae090', 1, N'/api/Form/{id}', 0, CAST(N'2025-04-13T18:10:27.1766667' AS DateTime2), CAST(N'2025-04-13T18:10:27.1766667' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'1dcf5dc6-31e5-495b-8b71-0df515bc6f2c', N'ChangePassword', N'Đổi mật khẩu', N'f51a0ccc-928c-45b3-af19-01443946f073', 0, N'/api/User/change-password', 1, CAST(N'2025-04-13T10:14:03.3057693' AS DateTime2), CAST(N'2025-04-13T10:14:03.3057693' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'e6629950-fa4e-48aa-b066-15c3b04ac795', N'Save Form', N'Save form', N'a24ffad0-5e1c-49c0-9465-da3bb76ae090', 1, N'/api/Form/save', 1, CAST(N'2025-04-13T18:10:27.1766667' AS DateTime2), CAST(N'2025-04-13T18:10:27.1766667' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'c4f96dae-ebfa-45d6-9fe9-17636f902d36', N'DSARDetails', N'Xem chi tiết DSAR', N'91678d66-0828-4981-a5b0-0b8b9f55a444', 0, N'/api/Dsar/{id}', 0, CAST(N'2025-04-13T10:30:49.2693434' AS DateTime2), CAST(N'2025-04-13T10:30:49.2693434' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'7aa2f2e3-7557-48c3-bb73-187d71cc67cd', N'Delete DPIA Responsibility', N'Delete a specific DPIA responsibility', N'65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 0, N'/api/DPIA/{id}/{id}', 3, CAST(N'2025-04-13T10:55:55.8631222' AS DateTime2), CAST(N'2025-04-13T17:55:55.8636596' AS DateTime2), NULL, N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'd7ca87db-4e71-40ff-bec4-18a16505003e', N'Remove Purpose from External System', N'Removes a single purpose from an external system', N'213b812f-2973-4c73-8b0f-bd6f9a195396', 1, N'/api/ExternalSystem/remove-purpose', 3, CAST(N'2025-04-13T18:06:32.1866667' AS DateTime2), CAST(N'2025-04-13T18:06:32.1866667' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'8564bdaa-3fe1-4b02-9435-19c54b5ecd5f', N'Update DPIA Comment', N'Update an existing DPIA comment by ID', N'65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 0, N'/api/DPIA/{id}/comments/{id}', 2, CAST(N'2025-04-13T10:56:05.2754572' AS DateTime2), CAST(N'2025-04-13T17:56:05.2758745' AS DateTime2), NULL, N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'bf25958b-e2a7-4dee-a69b-1a899b1d2ff9', N'Create Responsibility', N'Create a new responsibility', N'e52ee063-84f1-412b-a68b-f58fa882d19e', 1, N'/api/Responsibility', 1, CAST(N'2025-04-13T18:15:24.2766667' AS DateTime2), CAST(N'2025-04-13T18:15:24.2766667' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'2ff4d3ec-a495-40da-b7dd-1d1dd5f2522d', N'GetActivePrivacyPolicy', N'Lấy chính sách đang được kích hoạt (chỉ một)', N'1791ec64-40c4-4fb6-b934-ccfdcac23324', 1, N'/api/PrivacyPolicy/get-policy', 0, CAST(N'2025-04-13T16:27:33.8033333' AS DateTime2), CAST(N'2025-04-13T16:27:33.8033333' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'aa791680-39fa-4c89-aa6d-1e17d5d6a6ee', N'Delete Form', N'Delete a form by ID', N'a24ffad0-5e1c-49c0-9465-da3bb76ae090', 1, N'/api/Form/{id}', 3, CAST(N'2025-04-13T18:10:27.1766667' AS DateTime2), CAST(N'2025-04-13T18:10:27.1766667' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'6785d893-3377-499c-bbaa-21981f57e034', N'Get Responsibilities', N'Retrieve all responsibilities', N'e52ee063-84f1-412b-a68b-f58fa882d19e', 1, N'/api/Responsibility', 0, CAST(N'2025-04-13T18:15:24.2733333' AS DateTime2), CAST(N'2025-04-13T18:15:24.2733333' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'b369baf1-72f4-4c35-808e-23cd581413f7', N'Request Approval for DPIA', N'Request approval in the DPIA process', N'65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, N'/api/DPIA/{id}/request-approval', 1, CAST(N'2025-04-13T17:54:45.8166667' AS DateTime2), CAST(N'2025-04-13T17:54:45.8166667' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'0285d9a1-60df-4985-965b-26af7358ed1e', N'Get All Members for DPIA', N'Retrieve all possible members assignable to a DPIA', N'65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, N'/api/DPIA/members-for-dpia', 0, CAST(N'2025-04-13T17:54:45.8133333' AS DateTime2), CAST(N'2025-04-13T17:54:45.8133333' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'd598fcee-3e98-4420-a2dd-27c5fa8b4f31', N'GetAllConsentLogs', N'Lấy danh sách toàn bộ mục đích xin phép', N'1ce0f64c-21ff-4f25-9c51-0a2054cdc562', 1, N'/api/Consent/consent-log', 0, CAST(N'2025-04-13T16:32:54.6300000' AS DateTime2), CAST(N'2025-04-13T16:32:54.6300000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'7e49a296-fa29-455b-9559-2b5d940e5db5', N'CreateTicket', N'Tạo mới Ticket', N'3754c65e-86b1-451e-83e8-a0510e658d96', 0, N'/api/IssueTicket', 1, CAST(N'2025-04-13T11:12:29.5795124' AS DateTime2), CAST(N'2025-04-13T11:12:29.5795124' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'72065d1f-e757-44c5-a7f9-2c24ea49d8e3', N'GetRiskById', N'Lấy thông tin rủi ro theo ID', N'8cd84955-4dd5-47f3-8f04-c8e447a223aa', 1, N'/api/Risk/{id}', 0, CAST(N'2025-04-13T15:57:38.6766667' AS DateTime2), CAST(N'2025-04-13T15:57:38.6766667' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'9ad8bab5-0c15-4964-b2fc-2dbc081c2887', N'Add DPIA Members', N'Add members to a DPIA', N'65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, N'/api/DPIA/{id}/members', 1, CAST(N'2025-04-13T17:54:45.8100000' AS DateTime2), CAST(N'2025-04-13T17:54:45.8100000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'9a755003-1c8a-4c1e-bfa3-2fcd67335cb4', N'Submit Form', N'Submit form', N'a24ffad0-5e1c-49c0-9465-da3bb76ae090', 1, N'/api/Form/submit', 1, CAST(N'2025-04-13T18:10:27.1766667' AS DateTime2), CAST(N'2025-04-13T18:10:27.1766667' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'02f34591-e00d-41d7-90e4-2feb581d0ded', N'GetGroups', N'Lấy danh sách Groups', N'31c1ad41-5238-42c8-88fa-2eaad0acb7ce', 0, N'/api/Group', 0, CAST(N'2025-03-20T18:10:43.4167148' AS DateTime2), CAST(N'2025-03-20T18:10:43.4167148' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'b2df3353-3a21-4510-a295-31df747dc558', N'AddFeature', N'Thêm mới Feature', N'7200c019-5e23-4493-8e20-44867d0b3e33', 0, N'/api/Feature', 1, CAST(N'2025-03-20T11:06:37.9183997' AS DateTime2), CAST(N'2025-03-20T11:06:37.9183997' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'ea4761e5-d843-4150-bc3c-32d8fddaff71', N'Update DPIA', N'Update an existing DPIA record', N'65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, N'/api/DPIA/{id}', 2, CAST(N'2025-04-13T17:54:45.8100000' AS DateTime2), CAST(N'2025-04-13T17:54:45.8100000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'fb360569-16b5-4d04-898c-34d3926cd1dd', N'DeleteGroup', N'Xoá group', N'31c1ad41-5238-42c8-88fa-2eaad0acb7ce', 0, N'/api/Group/{id}', 3, CAST(N'2025-04-13T10:53:59.0974837' AS DateTime2), CAST(N'2025-04-13T10:53:59.0974837' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'ed53628e-63c8-4335-9edf-3afc20f00bee', N'UpdateUsersInGroup', NULL, N'31c1ad41-5238-42c8-88fa-2eaad0acb7ce', 0, N'/api/Group/update-users-in-group', 2, CAST(N'2025-04-13T11:08:12.6951238' AS DateTime2), CAST(N'2025-04-13T11:08:12.6951238' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'849a9547-b993-468b-84c3-3c335691faa3', N'GetConsentByEmailSystem', N'Lấy consent hiện tại theo email + systemId', N'1ce0f64c-21ff-4f25-9c51-0a2054cdc562', 1, N'/api-consent/get-consent/{email}/{id}', 0, CAST(N'2025-04-13T16:32:54.6300000' AS DateTime2), CAST(N'2025-04-13T16:32:54.6300000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'8a90896b-4181-459a-87d9-41a7d4f1e0d3', N'Get DPIA Responsibility', N'Retrieve responsibility detail in DPIA by responsibility ID', N'65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 0, N'/api/DPIA/{id}/{id}', 0, CAST(N'2025-04-13T10:56:18.2144201' AS DateTime2), CAST(N'2025-04-13T17:56:18.2153314' AS DateTime2), NULL, N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'b04163d0-973b-4b71-9129-42c174f929d6', N'Update Form Status', N'Update the status of a form', N'a24ffad0-5e1c-49c0-9465-da3bb76ae090', 1, N'/api/Form/update-status/{id}', 2, CAST(N'2025-04-13T18:10:27.1733333' AS DateTime2), CAST(N'2025-04-13T18:10:27.1733333' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'e4d9f063-4808-4f23-b6ad-456db2ba5d92', N'Update DPIA Members', N'Update members involved in a DPIA', N'65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, N'/api/DPIA/{id}/members', 2, CAST(N'2025-04-13T17:54:45.8100000' AS DateTime2), CAST(N'2025-04-13T17:54:45.8100000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'465f8c6b-2a5c-4710-949e-469ab8422240', N'UpdatePurposeStatus', N'Cập nhật trạng thái Purpose (Active hoặc Inactive)', N'10ec4d72-e9ce-48b5-87aa-06852206a167', 1, N'/api/Purpose/{id}/status', 2, CAST(N'2025-04-13T16:24:06.6500000' AS DateTime2), CAST(N'2025-04-13T16:24:06.6500000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'7ace24c4-a434-4180-9055-46c55bcd82f4', N'GroupDetails', N'Xem chi tiết Group', N'31c1ad41-5238-42c8-88fa-2eaad0acb7ce', 0, N'/api/Group/{id}/detail', 0, CAST(N'2025-04-13T10:51:01.1002091' AS DateTime2), CAST(N'2025-04-13T10:51:01.1002091' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'a139c5aa-9697-4fad-ab58-4a4e86db8fb6', N'Update Member Responsibility Status', N'Update status of a member responsibility in the DPIA process', N'65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, N'/api/DPIA/{id}/update-member-responsibility-status', 2, CAST(N'2025-04-13T17:54:45.8133333' AS DateTime2), CAST(N'2025-04-13T17:54:45.8133333' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'e5d35725-a6c9-4849-8274-4a9617381526', N'Bulk Add Purposes', N'Bulk adds purposes to an external system', N'213b812f-2973-4c73-8b0f-bd6f9a195396', 1, N'/api/ExternalSystem/bulk-add-purposes', 1, CAST(N'2025-04-13T18:06:32.1866667' AS DateTime2), CAST(N'2025-04-13T18:06:32.1866667' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'1eeab12a-5464-4f93-82cb-4a9b9d15f1e2', N'GetUsersInGroup', N'Xem người dùng trong group', N'31c1ad41-5238-42c8-88fa-2eaad0acb7ce', 0, N'/api/Group/get-users-in-group', 0, CAST(N'2025-04-13T10:55:47.6621248' AS DateTime2), CAST(N'2025-04-13T10:55:47.6621248' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'0e1e09a0-9111-4eb5-bbbf-57661b62fd1d', N'RemoveUserFromGroup', N'Xoá người dùng khỏi Group', N'31c1ad41-5238-42c8-88fa-2eaad0acb7ce', 0, N'/api/Group/remove-user-from-group', 3, CAST(N'2025-04-13T11:09:17.6616091' AS DateTime2), CAST(N'2025-04-13T11:09:17.6616091' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'a960c57f-7ffa-471d-8b33-597273fed5f5', N'Add Purpose to External System', N'Adds a purpose to an external system', N'213b812f-2973-4c73-8b0f-bd6f9a195396', 1, N'/api/ExternalSystem/add-purpose', 1, CAST(N'2025-04-13T18:06:32.1866667' AS DateTime2), CAST(N'2025-04-13T18:06:32.1866667' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'874a2e3e-1f30-4094-b512-5a2c9359f187', N'TicketStatus', N'Chuyển trạng thái Ticket', N'3754c65e-86b1-451e-83e8-a0510e658d96', 0, N'/api/IssueTicket/{id}/update-status', 2, CAST(N'2025-04-13T11:16:26.4577417' AS DateTime2), CAST(N'2025-04-13T11:16:26.4577417' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'185df56c-6a84-4582-9249-5f101e715f18', N'Update Form (New Version)', N'Update form and create a new version of it', N'a24ffad0-5e1c-49c0-9465-da3bb76ae090', 1, N'/api/Form/update', 1, CAST(N'2025-04-13T18:10:27.1766667' AS DateTime2), CAST(N'2025-04-13T18:10:27.1766667' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'41f16e79-5753-4cbf-82a7-60a81cb24add', N'Get Users of External System', N'Gets all users of an external system', N'213b812f-2973-4c73-8b0f-bd6f9a195396', 1, N'/api/ExternalSystem/{id}/get-users', 0, CAST(N'2025-04-13T18:06:32.1900000' AS DateTime2), CAST(N'2025-04-13T18:06:32.1900000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'68958d31-d0ea-4625-974f-658a177ff81a', N'ImportDSAR', N'Import DSAR', N'91678d66-0828-4981-a5b0-0b8b9f55a444', 0, N'/api/Dsar/import', 1, CAST(N'2025-04-13T10:33:30.3632168' AS DateTime2), CAST(N'2025-04-13T10:33:30.3632168' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'e0a242fa-db16-40ff-8150-68b86c780435', N'AddGroup', N'Thêm mới global Group', N'31c1ad41-5238-42c8-88fa-2eaad0acb7ce', 0, N'/api/Group', 1, CAST(N'2025-03-20T18:11:16.9095679' AS DateTime2), CAST(N'2025-03-20T18:11:16.9095679' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'0ec827fb-d1df-401e-9b50-69055737eac0', N'Bulk Remove Purposes', N'Bulk removes purposes from an external system', N'213b812f-2973-4c73-8b0f-bd6f9a195396', 1, N'/api/ExternalSystem/bulk-remove-purposes', 3, CAST(N'2025-04-13T18:06:32.1900000' AS DateTime2), CAST(N'2025-04-13T18:06:32.1900000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'40dd1810-61d3-46f7-93e6-6e9484412ab5', N'Restart Responsibility', N'Restart a specific DPIA responsibility', N'65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 0, N'/api/DPIA/{id}/restart-responsibility/{id}', 2, CAST(N'2025-04-13T10:56:31.3497630' AS DateTime2), CAST(N'2025-04-13T17:56:31.3510122' AS DateTime2), NULL, N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'c60a0b21-0722-4d96-9660-6f316915eb5e', N'GetLinkForConsent', N'API public: Lấy link lấy consent theo email và system', N'1ce0f64c-21ff-4f25-9c51-0a2054cdc562', 1, N'/api-consent/get-link/{email}/{id}', 0, CAST(N'2025-04-13T16:32:54.6300000' AS DateTime2), CAST(N'2025-04-13T16:32:54.6300000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'05d633b5-c4c3-4440-8993-72aa857110d0', N'Restart DPIA', N'Restart the DPIA process', N'65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, N'/api/DPIA/{id}/restart', 1, CAST(N'2025-04-13T17:54:45.8200000' AS DateTime2), CAST(N'2025-04-13T17:54:45.8200000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'5bb2947c-3b5c-4f8b-9b4a-72d380cdcead', N'GetDsarImportTemplate', N'Download DSAR Import template', N'91678d66-0828-4981-a5b0-0b8b9f55a444', 0, N'/api/Dsar/download-import-template', 0, CAST(N'2025-04-13T10:32:49.3073965' AS DateTime2), CAST(N'2025-04-13T10:32:49.3073965' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'cb0817aa-bb02-4212-b1d8-7340cb5e507f', N'Get System Details', N'Gets the details of an external system.', N'213b812f-2973-4c73-8b0f-bd6f9a195396', 1, N'/api/ExternalSystem/{id}/get-system-details', 0, CAST(N'2025-04-13T18:06:32.1900000' AS DateTime2), CAST(N'2025-04-13T18:06:32.1900000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'dd6ebde9-da17-4303-acb0-73e52cedbde7', N'DeletePrivacyPolicy', N'Xóa chính sách (chỉ khi chưa được kích hoạt)', N'1791ec64-40c4-4fb6-b934-ccfdcac23324', 1, N'/api/PrivacyPolicy/{id}', 3, CAST(N'2025-04-13T16:27:33.8033333' AS DateTime2), CAST(N'2025-04-13T16:27:33.8033333' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'44b66d82-9c9d-4977-b6a1-74832286c80f', N'Update System Users', N'Updates the users of an external system.', N'213b812f-2973-4c73-8b0f-bd6f9a195396', 1, N'/api/ExternalSystem/update-system-users', 2, CAST(N'2025-04-13T18:06:32.1900000' AS DateTime2), CAST(N'2025-04-13T18:06:32.1900000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'243f9c99-3daa-40b8-8100-75609896f5a7', N'UpdateRisk', N'Cập nhật thông tin rủi ro', N'8cd84955-4dd5-47f3-8f04-c8e447a223aa', 1, N'/api/Risk/{id}', 2, CAST(N'2025-04-13T15:57:38.6766667' AS DateTime2), CAST(N'2025-04-13T15:57:38.6766667' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'6752a3ad-35f8-4ad0-a147-77a401c497f8', N'Get Submission Details', N'Get FIC submission details', N'a24ffad0-5e1c-49c0-9465-da3bb76ae090', 1, N'/api/Form/submission/{id}', 0, CAST(N'2025-04-13T18:10:27.1800000' AS DateTime2), CAST(N'2025-04-13T18:10:27.1800000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'd29f66f0-6513-4f72-add7-79954a08d2e2', N'CreatePrivacyPolicy', N'Tạo mới chính sách quyền riêng tư', N'1791ec64-40c4-4fb6-b934-ccfdcac23324', 1, N'/api/PrivacyPolicy', 1, CAST(N'2025-04-13T16:27:33.8033333' AS DateTime2), CAST(N'2025-04-13T16:27:33.8033333' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'11d57e03-a656-4fdf-bb4c-7b054542ae83', N'GetListFeatures', N'Lấy danh sách Features. API này internally cho API add-feature-to-group', N'7200c019-5e23-4493-8e20-44867d0b3e33', 0, N'/api/Feature/get-list-features/{id}', 0, CAST(N'2025-03-20T18:05:40.6093658' AS DateTime2), CAST(N'2025-03-20T18:05:40.6093658' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'c5ea4956-4fd8-47d3-9d7d-7c2de88b9062', N'SetActivePrivacyPolicy', N'Đặt chính sách quyền riêng tư là đang hoạt động', N'1791ec64-40c4-4fb6-b934-ccfdcac23324', 1, N'/get-active/{id}', 2, CAST(N'2025-04-13T16:27:33.8033333' AS DateTime2), CAST(N'2025-04-13T16:27:33.8033333' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'ca4c5cc4-87f6-4fc4-86be-7fb1afcb5428', N'UpdateGroup', N'Update group', N'31c1ad41-5238-42c8-88fa-2eaad0acb7ce', 0, N'/api/Group/{id}', 2, CAST(N'2025-04-13T10:51:38.8171819' AS DateTime2), CAST(N'2025-04-13T10:51:38.8171819' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'3f5b6e4d-e5b1-492e-b330-7fbe273d92f1', N'Get DPIA History', N'Retrieve history of a DPIA record', N'65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, N'/api/DPIA/{id}/history', 0, CAST(N'2025-04-13T17:54:45.8100000' AS DateTime2), CAST(N'2025-04-13T17:54:45.8100000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'99dc630d-1d30-408d-9110-821b5e14fb6e', N'UserDetails', N'API xem chi tiết User', N'f51a0ccc-928c-45b3-af19-01443946f073', 0, N'/api/User/{id}', 0, CAST(N'2025-04-13T03:25:51.8727690' AS DateTime2), CAST(N'2025-04-13T10:25:51.8731903' AS DateTime2), NULL, N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'4348bb6e-9677-4991-a2d4-8288231190c2', N'ViewProfile', N'Xem profile', N'f51a0ccc-928c-45b3-af19-01443946f073', 0, N'/api/Account/profile/{id}', 0, CAST(N'2025-03-31T16:59:43.1621367' AS DateTime2), CAST(N'2025-03-31T16:59:43.1621367' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'8f87d307-7915-4895-94e0-83015d3667c0', N'Get Submissions', N'Get all FIC submissions (for an external system or all DPMS systems)', N'a24ffad0-5e1c-49c0-9465-da3bb76ae090', 1, N'/api/Form/get-submissions', 0, CAST(N'2025-04-13T18:10:27.1800000' AS DateTime2), CAST(N'2025-04-13T18:10:27.1800000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'26284d0d-c8cb-4fad-a43e-8325bd154f2f', N'Add External System', N'Adds a new external system', N'213b812f-2973-4c73-8b0f-bd6f9a195396', 1, N'/api/ExternalSystem', 1, CAST(N'2025-04-13T18:06:32.1800000' AS DateTime2), CAST(N'2025-04-13T18:06:32.1800000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'4c4703fe-e6d4-429c-85c1-84f835d4d693', N'UpdateFeature', N'Update Feature', N'7200c019-5e23-4493-8e20-44867d0b3e33', 0, N'/api/Feature/{id}', 2, CAST(N'2025-03-20T11:42:43.6432470' AS DateTime2), CAST(N'2025-03-20T11:42:43.6432470' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'34ffdce4-31c9-45f7-97ef-87d06db16b55', N'Update Responsibility Status', N'Update status for a DPIA responsibility', N'65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, N'/api/DPIA/{id}/update-responsibility-status', 2, CAST(N'2025-04-13T17:54:45.8133333' AS DateTime2), CAST(N'2025-04-13T17:54:45.8133333' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'16f8a0f0-9aa9-4a29-a7a3-89e5fb71c1f3', N'Update Active Status', N'Updates the status of an external system.', N'213b812f-2973-4c73-8b0f-bd6f9a195396', 1, N'/api/ExternalSystem/update-active-status', 2, CAST(N'2025-04-13T18:06:32.1900000' AS DateTime2), CAST(N'2025-04-13T18:06:32.1900000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'd56ad5fa-30da-4ff7-9b4d-8d9dcf9fe0c6', N'Get DPIAs', N'Retrieve all DPIA records', N'65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, N'/api/DPIA', 0, CAST(N'2025-04-13T17:54:45.8033333' AS DateTime2), CAST(N'2025-04-13T17:54:45.8033333' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'f37f08d8-30d8-4ddf-94bf-8f5f1185b998', N'Reject DPIA', N'Reject the DPIA process', N'65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, N'/api/DPIA/{id}/reject', 1, CAST(N'2025-04-13T17:54:45.8200000' AS DateTime2), CAST(N'2025-04-13T17:54:45.8200000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'0a9276d8-d243-4124-82e9-90a0aa01ba8c', N'SubmitConsent', N'Người dùng gửi consent (insert vào bảng Consent & ConsentPurpose)', N'1ce0f64c-21ff-4f25-9c51-0a2054cdc562', 1, N'/api/Consent', 1, CAST(N'2025-04-13T16:32:54.6300000' AS DateTime2), CAST(N'2025-04-13T16:32:54.6300000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'4dbc6f06-d14d-49e0-980e-934b9b5a8198', N'GetPurposeById', N'Lấy Purpose theo ID', N'10ec4d72-e9ce-48b5-87aa-06852206a167', 1, N'/api/Purpose/{id}', 0, CAST(N'2025-04-13T16:24:06.6500000' AS DateTime2), CAST(N'2025-04-13T16:24:06.6500000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'ad661f28-9be2-4e4e-94e6-93a92a3096cf', N'DeleteTicket', N'Xoá ticket', N'3754c65e-86b1-451e-83e8-a0510e658d96', 0, N'/api/IssueTicket/{id}', 3, CAST(N'2025-04-13T11:15:41.8960531' AS DateTime2), CAST(N'2025-04-13T11:15:41.8960531' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'8f18d54e-1503-44a9-b39c-95c75e6de83a', N'Approve DPIA', N'Approve the DPIA process', N'65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, N'/api/DPIA/{id}/approve', 1, CAST(N'2025-04-13T17:54:45.8200000' AS DateTime2), CAST(N'2025-04-13T17:54:45.8200000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'1c190838-bfec-41db-8b51-96d4043e4640', N'Delete Document', N'Delete a specific document in a DPIA process', N'65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 0, N'/api/DPIA/{id}/documents/{id}', 3, CAST(N'2025-04-13T10:56:46.3854191' AS DateTime2), CAST(N'2025-04-13T17:56:46.3860579' AS DateTime2), NULL, N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'2ba6af57-f5a3-4725-abab-9800d27c7003', N'DeleteFile', N'Xoá file trên S3', N'0da130db-3cce-44c2-98dc-e104383c2e18', 0, N'/api/File', 3, CAST(N'2025-04-13T15:47:36.4152494' AS DateTime2), CAST(N'2025-04-13T15:47:36.4152494' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'96ebed85-5c68-4a22-80d0-98503eda26a1', N'Edit Form', N'Only edit forms with Draft Status (latest version); will not create a new version', N'a24ffad0-5e1c-49c0-9465-da3bb76ae090', 1, N'/api/Form/edit', 1, CAST(N'2025-04-13T18:10:27.1766667' AS DateTime2), CAST(N'2025-04-13T18:10:27.1766667' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'e3e41789-c7a8-43ab-918c-994c13e70253', N'CreatePurpose', N'Tạo mới Purpose', N'10ec4d72-e9ce-48b5-87aa-06852206a167', 1, N'/api/Purpose', 1, CAST(N'2025-04-13T16:24:06.6500000' AS DateTime2), CAST(N'2025-04-13T16:24:06.6500000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'20fb37d8-df77-48d9-a859-99fae4fa3b36', N'ViewTicketList', N'Xem danh sách Ticket', N'3754c65e-86b1-451e-83e8-a0510e658d96', 0, N'/api/IssueTicket', 0, CAST(N'2025-04-13T11:13:39.5340815' AS DateTime2), CAST(N'2025-04-13T11:13:39.5340815' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'd4bc974e-7bc1-468a-b3b4-9ff370ac3864', N'Delete Responsibility', N'Delete a responsibility by ID', N'e52ee063-84f1-412b-a68b-f58fa882d19e', 1, N'/api/Responsibility/{id}', 3, CAST(N'2025-04-13T18:15:24.2766667' AS DateTime2), CAST(N'2025-04-13T18:15:24.2766667' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'dc58c43e-c573-4a49-b4bc-a2c3750e7eba', N'UpdateTicket', N'Update ticket', N'3754c65e-86b1-451e-83e8-a0510e658d96', 0, N'/api/IssueTicket/{id}', 2, CAST(N'2025-04-13T11:15:09.6432749' AS DateTime2), CAST(N'2025-04-13T11:15:09.6432749' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'c5276483-0ffa-4fd0-abe1-a55a0d3a345c', N'UploadFile', N'Tải file lên S3', N'0da130db-3cce-44c2-98dc-e104383c2e18', 0, N'/api/File', 1, CAST(N'2025-04-13T15:47:11.3613502' AS DateTime2), CAST(N'2025-04-13T15:47:11.3613502' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'16d8aa8b-5a63-4f0b-8bbc-a5ef4f355486', N'GetConsentBanner', N'Lấy nội dung banner xin phép người dùng', N'1ce0f64c-21ff-4f25-9c51-0a2054cdc562', 1, N'/api/Consent/get-banner/{uniqueIdentifier}/{token}', 0, CAST(N'2025-04-13T16:32:54.6300000' AS DateTime2), CAST(N'2025-04-13T16:32:54.6300000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'f84966b3-9444-4bf2-b796-a896a2cd77ff', N'ViewAccountList', N'Xem danh sách tài khoản', N'f51a0ccc-928c-45b3-af19-01443946f073', 0, N'/api/Account', 0, CAST(N'2025-03-22T16:23:46.0173807' AS DateTime2), CAST(N'2025-03-22T16:23:46.0173807' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'eba69ffb-019c-45c0-adf6-b9a05347cb9c', N'Get DPIA Comments', N'Retrieve all comments for a DPIA record', N'65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, N'/api/DPIA/{id}/comments', 0, CAST(N'2025-04-13T17:54:45.8100000' AS DateTime2), CAST(N'2025-04-13T17:54:45.8100000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'64b358d8-e274-42c7-8fb2-b9eb15ec7e57', N'DownloadConsentTemplate', N'Tải file mẫu nhập consent cho hệ thống', N'1ce0f64c-21ff-4f25-9c51-0a2054cdc562', 1, N'/api/Consent/download-template/{id}', 0, CAST(N'2025-04-13T16:32:54.6300000' AS DateTime2), CAST(N'2025-04-13T16:32:54.6300000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'2c4033c1-c9d7-4870-aaa9-bb22c0173cea', N'TicketDetails', N'Xem chi tiết Ticket', N'3754c65e-86b1-451e-83e8-a0510e658d96', 0, N'/api/IssueTicket/{id}', 0, CAST(N'2025-04-13T11:14:17.3418525' AS DateTime2), CAST(N'2025-04-13T11:14:17.3418525' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'0643b256-f453-4a05-93f5-bb289c809195', N'Delete External System', N'Removes an external system', N'213b812f-2973-4c73-8b0f-bd6f9a195396', 1, N'/api/ExternalSystem', 3, CAST(N'2025-04-13T18:06:32.1833333' AS DateTime2), CAST(N'2025-04-13T18:06:32.1833333' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'2e441abe-3624-4bac-89ad-bb5a8a5e0e20', N'GetSystemConsentLogs', N'Lấy danh sách mục đích tại một hệ thống cụ thể', N'1ce0f64c-21ff-4f25-9c51-0a2054cdc562', 1, N'/api/Consent/consent-log/{id}', 0, CAST(N'2025-04-13T16:32:54.6300000' AS DateTime2), CAST(N'2025-04-13T16:32:54.6300000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'2b8d5850-20fe-46cc-959f-bf2b09c628ac', N'Delete DPIA', N'Delete a DPIA record by ID', N'65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, N'/api/DPIA/{id}', 3, CAST(N'2025-04-13T17:54:45.8100000' AS DateTime2), CAST(N'2025-04-13T17:54:45.8100000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'852f80cf-3107-4f84-8690-bf61872df0b7', N'Export risk', N'Export dữ liệu risk', N'8cd84955-4dd5-47f3-8f04-c8e447a223aa', 0, N'/api/Risk/export', 0, CAST(N'2025-03-31T16:47:20.6829722' AS DateTime2), CAST(N'2025-03-31T16:47:20.6829722' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'9c977223-b32c-40b4-a748-c27327167925', N'Start DPIA', N'Start the DPIA process', N'65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, N'/api/DPIA/{id}/start-dpia', 1, CAST(N'2025-04-13T17:54:45.8166667' AS DateTime2), CAST(N'2025-04-13T17:54:45.8166667' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'8559fdcb-ea47-4e35-a3b0-c63da6b45ce6', N'GetPrivacyPolicyById', N'Lấy chính sách quyền riêng tư theo ID', N'1791ec64-40c4-4fb6-b934-ccfdcac23324', 1, N'/api/PrivacyPolicy/{id}', 0, CAST(N'2025-04-13T16:27:33.8033333' AS DateTime2), CAST(N'2025-04-13T16:27:33.8033333' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'0d0a5371-1815-4ba0-af3c-c7033d8425d5', N'Update Members Responsibilities', N'Update responsibilities of DPIA members', N'65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, N'/api/DPIA/{id}/update-members-responsibilities', 2, CAST(N'2025-04-13T17:54:45.8133333' AS DateTime2), CAST(N'2025-04-13T17:54:45.8133333' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'c57fddce-4b52-4399-940e-ca12d5ccd683', N'Add DPIA', N'Create a new DPIA record', N'65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, N'/api/DPIA', 1, CAST(N'2025-04-13T17:54:45.8066667' AS DateTime2), CAST(N'2025-04-13T17:54:45.8066667' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'923e09f1-7ccb-4f91-a949-cb1b18e174e7', N'Update External System', N'Updates an external system.', N'213b812f-2973-4c73-8b0f-bd6f9a195396', 1, N'/api/ExternalSystem/update-system', 2, CAST(N'2025-04-13T18:06:32.1900000' AS DateTime2), CAST(N'2025-04-13T18:06:32.1900000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'dde9e840-eab5-469f-a0c9-cc379cdb1922', N'Get DPIA Detail', N'Retrieve detailed DPIA information by ID', N'65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, N'/api/DPIA/dpia-detail/{id}', 0, CAST(N'2025-04-13T17:54:45.8066667' AS DateTime2), CAST(N'2025-04-13T17:54:45.8066667' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'83448ab8-5f62-4503-aa57-d4c50e6101a8', N'AddAccount', N'Tạo mới tài khoản', N'f51a0ccc-928c-45b3-af19-01443946f073', 0, N'/api/Account', 1, CAST(N'2025-03-22T16:24:06.8760348' AS DateTime2), CAST(N'2025-03-22T16:24:06.8760348' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'd3beba1f-5609-4815-aeb2-d71b389522b5', N'ViewFile', N'Xem file', N'0da130db-3cce-44c2-98dc-e104383c2e18', 0, N'/api/File', 0, CAST(N'2025-04-13T15:48:06.2865668' AS DateTime2), CAST(N'2025-04-13T15:48:06.2865668' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'7e79550f-5cd0-4c08-8de7-d893c4d1b212', N'AddFeatureToGroup', N'Thêm quyền vào Group of Users', N'7200c019-5e23-4493-8e20-44867d0b3e33', 0, N'/api/Feature/add-feature-to-group', 1, CAST(N'2025-03-20T11:44:58.9447571' AS DateTime2), CAST(N'2025-03-20T11:44:58.9447571' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'c8cf744f-e003-47e4-a976-d94d172fefdd', N'Upload Document to DPIA', N'Upload documents for a DPIA process', N'65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, N'/api/DPIA/{id}/upload-document', 1, CAST(N'2025-04-13T17:54:45.8166667' AS DateTime2), CAST(N'2025-04-13T17:54:45.8166667' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'f23c398c-bcd9-4380-a910-d97258d5a941', N'Get API Key', N'Generates an API key for an external system.', N'213b812f-2973-4c73-8b0f-bd6f9a195396', 1, N'/api/ExternalSystem/get-key/{id}', 0, CAST(N'2025-04-13T18:06:32.1933333' AS DateTime2), CAST(N'2025-04-13T18:06:32.1933333' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'34de3fc6-c600-40fe-a713-da47ad9ad3cf', N'UsersNotInGroup', N'Lấy danh sách người dùng không trong một Group', N'31c1ad41-5238-42c8-88fa-2eaad0acb7ce', 0, N'/api/Group/users-not-in-group/{id}', 0, CAST(N'2025-04-13T11:10:11.7470849' AS DateTime2), CAST(N'2025-04-13T11:10:11.7470849' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'0beec1e8-40b5-498b-bbb0-dbe201a8fa36', N'FetchGlobalUsers', N'Fetch users in global groups', N'31c1ad41-5238-42c8-88fa-2eaad0acb7ce', 0, N'/api/Group/fetch-all-global-users', 0, CAST(N'2025-04-13T11:06:41.7817253' AS DateTime2), CAST(N'2025-04-13T11:06:41.7817253' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'0763f314-85b2-4d6d-8abb-dc7b4fbeae4a', N'FeatureDetails', N'Lấy thông tin chi tiết Feature', N'7200c019-5e23-4493-8e20-44867d0b3e33', 0, N'/api/Feature/{id}', 0, CAST(N'2025-03-20T11:47:30.3467210' AS DateTime2), CAST(N'2025-03-20T11:47:30.3467210' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'2fcd48a5-9ce0-4fb2-be7d-dc87c7c157a6', N'GetRiskRegister', N'Lấy danh sách đăng ký rủi ro', N'8cd84955-4dd5-47f3-8f04-c8e447a223aa', 1, N'/api/Risk', 0, CAST(N'2025-04-13T15:57:38.6766667' AS DateTime2), CAST(N'2025-04-13T15:57:38.6766667' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'f50eefb2-5c87-4965-8c84-deee6eefe3e4', N'Update DPIA Responsibility Members', N'Update members assigned to a specific responsibility in DPIA', N'65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 0, N'/api/DPIA/{id}/{id}/update-responsibility-members', 2, CAST(N'2025-04-13T10:57:00.7547908' AS DateTime2), CAST(N'2025-04-13T17:57:00.7551786' AS DateTime2), NULL, N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'800974c5-44c0-40a6-8c6c-e20aa93eeb57', N'Add DPIA Comment', N'Add a comment to a DPIA record', N'65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, N'/api/DPIA/{id}/comments', 1, CAST(N'2025-04-13T17:54:45.8100000' AS DateTime2), CAST(N'2025-04-13T17:54:45.8100000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'45106c7a-be67-4905-80b5-e3b469005811', N'AddUserToGroup', N'Thêm người vào Group', N'31c1ad41-5238-42c8-88fa-2eaad0acb7ce', 0, N'/api/Group/add-user-to-group', 1, CAST(N'2025-04-13T10:54:51.2909325' AS DateTime2), CAST(N'2025-04-13T10:54:51.2909325' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'803a8038-acb1-4fd6-88c2-e49679ce0673', N'GetPurposes', N'Lấy danh sách Purpose', N'10ec4d72-e9ce-48b5-87aa-06852206a167', 1, N'/api/Purpose', 0, CAST(N'2025-04-13T16:24:06.6500000' AS DateTime2), CAST(N'2025-04-13T16:24:06.6500000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'83d48ca5-9816-49fa-b5fc-e60a18b4acca', N'Get Purposes of External System', N'Gets all purposes of an external system', N'213b812f-2973-4c73-8b0f-bd6f9a195396', 1, N'/api/ExternalSystem/{id}/purposes', 0, CAST(N'2025-04-13T18:06:32.1866667' AS DateTime2), CAST(N'2025-04-13T18:06:32.1866667' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'cf99efc3-8f92-4aef-95e6-e82a9840ffa2', N'Get Responsibility by ID', N'Retrieve a responsibility by its ID', N'e52ee063-84f1-412b-a68b-f58fa882d19e', 1, N'/api/Responsibility/{id}', 0, CAST(N'2025-04-13T18:15:24.2766667' AS DateTime2), CAST(N'2025-04-13T18:15:24.2766667' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'20326a79-2ec7-4704-b524-e8fd8e6b3e83', N'DeleteFeature', N'Xoá Feature', N'7200c019-5e23-4493-8e20-44867d0b3e33', 0, N'/api/Feature/{id}', 3, CAST(N'2025-03-20T11:43:23.3819650' AS DateTime2), CAST(N'2025-03-20T11:43:23.3819650' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'9c97ac71-920e-4d0d-84b9-ebb07d52c3d2', N'ViewFeatures', N'Xem danh sách Features', N'7200c019-5e23-4493-8e20-44867d0b3e33', 0, N'/api/Feature', 0, CAST(N'2025-03-20T11:05:53.7960367' AS DateTime2), CAST(N'2025-03-20T11:05:53.7960367' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'982fd20a-3be6-4f7e-ad01-ed707f04829d', N'ChangeStatus', N'Update User''s status (active/deactive)', N'f51a0ccc-928c-45b3-af19-01443946f073', 0, N'/api/User/update-user-status', 2, CAST(N'2025-04-13T10:20:07.9706700' AS DateTime2), CAST(N'2025-04-13T10:20:07.9706700' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'50206ad9-c6a2-42a5-a928-f0dedeace5ef', N'Get External Systems', N'Gets all external systems', N'213b812f-2973-4c73-8b0f-bd6f9a195396', 1, N'/api/ExternalSystem', 0, CAST(N'2025-04-13T18:06:32.1800000' AS DateTime2), CAST(N'2025-04-13T18:06:32.1800000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'c54d7ccb-dbb4-4c58-b72e-f314ebbc4593', N'UpdateProfile', N'Update profile', N'f51a0ccc-928c-45b3-af19-01443946f073', 0, N'/api/Account/profile', 2, CAST(N'2025-04-13T10:08:18.6885814' AS DateTime2), CAST(N'2025-04-13T10:08:18.6885814' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'e4aeb5bc-3d18-4b27-afcc-f3658cdf34a6', N'ImportConsent', N'Nhập consent từ file Excel', N'1ce0f64c-21ff-4f25-9c51-0a2054cdc562', 1, N'/api/Consent/import-consent', 1, CAST(N'2025-04-13T16:32:54.6300000' AS DateTime2), CAST(N'2025-04-13T16:32:54.6300000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'513820ed-bd6e-4144-aba3-f3fae0ae40d1', N'GetPrivacyPolicies', N'Lấy danh sách chính sách quyền riêng tư', N'1791ec64-40c4-4fb6-b934-ccfdcac23324', 1, N'/api/PrivacyPolicy', 0, CAST(N'2025-04-13T16:27:33.8033333' AS DateTime2), CAST(N'2025-04-13T16:27:33.8033333' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'da233f54-bc04-4d73-9a79-f4ff572a9807', N'Get Form Templates', N'Get all form templates', N'a24ffad0-5e1c-49c0-9465-da3bb76ae090', 1, N'/api/Form/get-templates', 0, CAST(N'2025-04-13T18:10:27.1700000' AS DateTime2), CAST(N'2025-04-13T18:10:27.1700000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'89808191-2525-4b39-a62e-f65b3e92a7e6', N'GetDpmsConsentByEmail', N'Chỉ dùng nội bộ DPMS để lấy consent theo email', N'1ce0f64c-21ff-4f25-9c51-0a2054cdc562', 1, N'/api/Consent/dpms-consent/{email}', 0, CAST(N'2025-04-13T16:32:54.6300000' AS DateTime2), CAST(N'2025-04-13T16:32:54.6300000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'57de3d68-0828-4af5-acf0-f7df30b56bef', N'RegisterRisk', N'Đăng ký rủi ro mới', N'8cd84955-4dd5-47f3-8f04-c8e447a223aa', 1, N'/api/Risk', 1, CAST(N'2025-04-13T15:57:38.6766667' AS DateTime2), CAST(N'2025-04-13T15:57:38.6766667' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'14a55c7e-869e-44a6-8fd8-f835fb47c484', N'DeletePurpose', N'Xóa Purpose (chỉ khi ở trạng thái Draft)', N'10ec4d72-e9ce-48b5-87aa-06852206a167', 1, N'/api/Purpose/{id}', 3, CAST(N'2025-04-13T16:24:06.6500000' AS DateTime2), CAST(N'2025-04-13T16:24:06.6500000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'59c79bf8-dbaf-4ecb-a028-faad809fc307', N'UpdatePurpose', N'Cập nhật Purpose (chỉ khi ở trạng thái Draft)', N'10ec4d72-e9ce-48b5-87aa-06852206a167', 1, N'/api/Purpose/{id}', 2, CAST(N'2025-04-13T16:24:06.6500000' AS DateTime2), CAST(N'2025-04-13T16:24:06.6500000' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'f848c736-2cb9-4d80-8450-fdd26db16abe', N'DeleteRisk', N'Xóa rủi ro', N'8cd84955-4dd5-47f3-8f04-c8e447a223aa', 1, N'/api/Risk/{id}', 3, CAST(N'2025-04-13T15:57:38.6766667' AS DateTime2), CAST(N'2025-04-13T15:57:38.6766667' AS DateTime2), NULL, NULL)
GO
INSERT [Features] ([Id], [FeatureName], [Description], [ParentId], [State], [Url], [HttpMethod], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'd3dcab3f-e8eb-4aeb-a9f7-fe73e0542b89', N'DSARList', N'Danh sách DSAR của một hệ thống', N'91678d66-0828-4981-a5b0-0b8b9f55a444', 0, N'/api/Dsar/get-list/{id}', 0, CAST(N'2025-04-13T10:30:11.9140088' AS DateTime2), CAST(N'2025-04-13T10:30:11.9140088' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO

-- Insert quyền cho admin_group
GO
INSERT [dbo].[GroupFeatures] ([Id], [GroupId], [FeatureId], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'f2efe4d5-9af8-40d8-becd-05e30680142c', N'15aa9ba9-b966-4507-ae74-e515cdb22df4', N'b2df3353-3a21-4510-a295-31df747dc558', CAST(N'2025-03-20T11:35:59.8113627' AS DateTime2), CAST(N'2025-03-20T11:35:59.8113627' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[GroupFeatures] ([Id], [GroupId], [FeatureId], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'6203c5cf-46b1-4786-a842-29fc7610fc10', N'15aa9ba9-b966-4507-ae74-e515cdb22df4', N'e0a242fa-db16-40ff-8150-68b86c780435', CAST(N'2025-03-20T18:11:58.5954311' AS DateTime2), CAST(N'2025-03-20T18:11:58.5954311' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[GroupFeatures] ([Id], [GroupId], [FeatureId], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'9782ce12-b26a-4757-bb96-6a3e14da3990', N'15aa9ba9-b966-4507-ae74-e515cdb22df4', N'0763f314-85b2-4d6d-8abb-dc7b4fbeae4a', CAST(N'2025-03-20T18:03:51.8980820' AS DateTime2), CAST(N'2025-03-20T18:03:51.8980820' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[GroupFeatures] ([Id], [GroupId], [FeatureId], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'c020078b-107a-4bce-b627-75fde9bb1511', N'15aa9ba9-b966-4507-ae74-e515cdb22df4', N'4c4703fe-e6d4-429c-85c1-84f835d4d693', CAST(N'2025-03-20T18:03:51.8980820' AS DateTime2), CAST(N'2025-03-20T18:03:51.8980820' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[GroupFeatures] ([Id], [GroupId], [FeatureId], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'fbbe50ad-e686-4621-b34a-a5dbb793144e', N'15aa9ba9-b966-4507-ae74-e515cdb22df4', N'9c97ac71-920e-4d0d-84b9-ebb07d52c3d2', CAST(N'2025-03-20T11:35:59.8113627' AS DateTime2), CAST(N'2025-03-20T11:35:59.8113627' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[GroupFeatures] ([Id], [GroupId], [FeatureId], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'c4793d7a-9940-438d-8234-a81560002e53', N'15aa9ba9-b966-4507-ae74-e515cdb22df4', N'02f34591-e00d-41d7-90e4-2feb581d0ded', CAST(N'2025-03-20T18:11:58.5954311' AS DateTime2), CAST(N'2025-03-20T18:11:58.5954311' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[GroupFeatures] ([Id], [GroupId], [FeatureId], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'96b36ae2-98d6-4b81-83d2-b2efdd409527', N'15aa9ba9-b966-4507-ae74-e515cdb22df4', N'20326a79-2ec7-4704-b524-e8fd8e6b3e83', CAST(N'2025-03-20T18:03:51.8980820' AS DateTime2), CAST(N'2025-03-20T18:03:51.8980820' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[GroupFeatures] ([Id], [GroupId], [FeatureId], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'936f56a7-6602-4f87-ac2c-bb3ecaff67f4', N'15aa9ba9-b966-4507-ae74-e515cdb22df4', N'11d57e03-a656-4fdf-bb4c-7b054542ae83', CAST(N'2025-03-20T18:06:14.5710557' AS DateTime2), CAST(N'2025-03-20T18:06:14.5710557' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[GroupFeatures] ([Id], [GroupId], [FeatureId], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'ec9d0488-727b-4fff-bc97-e7912187c75e', N'15aa9ba9-b966-4507-ae74-e515cdb22df4', N'7e79550f-5cd0-4c08-8de7-d893c4d1b212', CAST(N'2025-03-20T18:03:51.8980820' AS DateTime2), CAST(N'2025-03-20T18:03:51.8980820' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[GroupFeatures] ([Id], [GroupId], [FeatureId], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'809b2fe1-c912-4f6a-b387-588989dc3038', N'15aa9ba9-b966-4507-ae74-e515cdb22df4', N'f84966b3-9444-4bf2-b796-a896a2cd77ff', CAST(N'2025-03-22T16:24:48.7051999' AS DateTime2), CAST(N'2025-03-22T16:24:48.7051999' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[GroupFeatures] ([Id], [GroupId], [FeatureId], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'719a6fdb-69eb-4c67-87d2-5bed0f6dfd93', N'15aa9ba9-b966-4507-ae74-e515cdb22df4', N'83448ab8-5f62-4503-aa57-d4c50e6101a8', CAST(N'2025-03-22T16:24:48.7051999' AS DateTime2), CAST(N'2025-03-22T16:24:48.7051999' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO

-- Seed dữ liệu purpose
GO
INSERT [dbo].[Purposes] ([Id], [Name], [Description], [Status], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'338cf197-5e07-4b96-9593-705aec2bee47', N'Vận hành và cung cấp Dịch vụ', N'FPT Edu sử dụng thông tin cá nhân của Chủ thể dữ liệu để vận hành, cung cấp và cải thiện Dịch vụ.', 1, CAST(N'2025-03-21T10:08:01.9066667' AS DateTime2), CAST(N'2025-03-21T10:08:01.9066667' AS DateTime2), NULL, NULL)
GO
INSERT [dbo].[Purposes] ([Id], [Name], [Description], [Status], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'4af4b370-07f8-488e-9192-7354bd747325', N'Tuân thủ nghĩa vụ pháp lý', N'FPT Edu sử dụng thông tin cá nhân của Chủ thể dữ liệu để thực hiện các nghĩa vụ pháp lý theo quy định hiện hành.', 1, CAST(N'2025-03-21T10:08:01.9066667' AS DateTime2), CAST(N'2025-03-21T10:08:01.9066667' AS DateTime2), NULL, NULL)
GO
INSERT [dbo].[Purposes] ([Id], [Name], [Description], [Status], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'75601f27-a7ef-470e-9775-cf2d1cb4af35', N'Truyền thông, quảng bá Dịch vụ và chương trình giáo dục', N'FPT Edu sử dụng thông tin cá nhân của Chủ thể dữ liệu để thực hiện công tác truyền thông, quảng bá về Dịch vụ và các chương trình giáo dục theo quy định của pháp luật.', 1, CAST(N'2025-03-21T10:08:01.9066667' AS DateTime2), CAST(N'2025-03-21T10:08:01.9066667' AS DateTime2), NULL, NULL)
GO
INSERT [dbo].[Purposes] ([Id], [Name], [Description], [Status], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'd2d4e250-30f1-488e-8044-e34984a7e103', N'Phân tích hiệu quả và cải thiện chất lượng Dịch vụ', N'FPT Edu sử dụng thông tin cá nhân của Chủ thể dữ liệu để đo lường việc sử dụng, phân tích hiệu quả để cải thiện chất lượng và phát triển Dịch vụ.', 1, CAST(N'2025-03-21T10:07:58.4966667' AS DateTime2), CAST(N'2025-03-21T10:07:58.4966667' AS DateTime2), NULL, NULL)
GO
INSERT [dbo].[Purposes] ([Id], [Name], [Description], [Status], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'ea1f0f91-1eb1-44aa-a054-eacd5bd9edc1', N'Đề xuất Dịch vụ cho Chủ thể dữ liệu', N'FPT Edu sử dụng thông tin cá nhân của Chủ thể dữ liệu để đề xuất Dịch vụ mà Chủ thể dữ liệu có thể quan tâm và tham gia trải nghiệm.', 1, CAST(N'2025-03-21T10:08:01.9066667' AS DateTime2), CAST(N'2025-03-21T10:08:01.9066667' AS DateTime2), NULL, NULL)
GO
INSERT [dbo].[Purposes] ([Id], [Name], [Description], [Status], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'3cda706c-8089-48d3-af23-fa472de69588', N'Trao đổi và tương tác với Chủ thể dữ liệu', N'FPT Edu sử dụng thông tin cá nhân của Chủ thể dữ liệu để trao đổi, tương tác và hỗ trợ khi cần thiết.', 1, CAST(N'2025-03-21T10:07:58.4966667' AS DateTime2), CAST(N'2025-03-21T10:07:58.4966667' AS DateTime2), NULL, NULL)
GO

-- Seed dữ liệu ExternalSystemPurposes
GO
INSERT [dbo].[ExternalSystemPurposes] ([Id], [ExternalSystemId], [PurposeId], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'7e88c159-1c63-45a4-8e3e-3632f6e1ada1', N'1c9b5694-f2ff-4648-b50f-a93bbe30d941', N'3cda706c-8089-48d3-af23-fa472de69588', CAST(N'2025-03-21T23:00:49.0114101' AS DateTime2), CAST(N'2025-03-21T23:00:49.0114101' AS DateTime2), NULL, NULL)
GO
INSERT [dbo].[ExternalSystemPurposes] ([Id], [ExternalSystemId], [PurposeId], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'64986eb2-c30c-468c-a9bb-473a395ad7f9', N'1c9b5694-f2ff-4648-b50f-a93bbe30d941', N'75601f27-a7ef-470e-9775-cf2d1cb4af35', CAST(N'2025-03-21T23:00:49.0114101' AS DateTime2), CAST(N'2025-03-21T23:00:49.0114101' AS DateTime2), NULL, NULL)
GO
INSERT [dbo].[ExternalSystemPurposes] ([Id], [ExternalSystemId], [PurposeId], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'24f7f262-1d3a-463f-b5a6-903deb232551', N'1c9b5694-f2ff-4648-b50f-a93bbe30d941', N'ea1f0f91-1eb1-44aa-a054-eacd5bd9edc1', CAST(N'2025-03-21T23:00:49.0114101' AS DateTime2), CAST(N'2025-03-21T23:00:49.0114101' AS DateTime2), NULL, NULL)
GO
INSERT [dbo].[ExternalSystemPurposes] ([Id], [ExternalSystemId], [PurposeId], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'aa672628-f0df-44a1-8797-ff6e4ccd2ec4', N'1c9b5694-f2ff-4648-b50f-a93bbe30d941', N'd2d4e250-30f1-488e-8044-e34984a7e103', CAST(N'2025-03-21T23:00:49.0114101' AS DateTime2), CAST(N'2025-03-21T23:00:49.0114101' AS DateTime2), NULL, NULL)
GO

------- SEED dữ liệu FIC ---------
ALTER TABLE [FormElements] NOCHECK CONSTRAINT FK_FormElements_FormElements_ParentId; -- Tắt tạm thời FK để tránh INSERT lỗi do FK ParentId
GO
INSERT [dbo].[Forms] ([Id], [Name], [Version], [FormType], [Status], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'Form FIC ban hành 2025 FPT EDU', 1, 0, 1, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'7a9f23af-b0a2-4ad7-82a5-03a38cff60a5', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'ae8e66df-2eb1-4390-b3a8-261caa9971e8', N'2.6 CTTV có thu thập thông tin vị trí của cá nhân được xác định qua dịch vụ định vị không?', 0, 5, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'f51501af-e37c-478c-9762-08b7b08db481', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'ec00a567-aaf2-400f-94c8-3023b35fae1f', N'(a) Dữ liệu cá nhân có được lưu trữ tập trung không?', 0, 0, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'597dc1a0-7680-4bdf-a984-206d3ca6f6aa', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'ae8e66df-2eb1-4390-b3a8-261caa9971e8', N'2.5 "CTTV có thu thập thông tin dữ liệu cá nhân liên quan tín dụng, dịch vụ trung gian thanh toán, gồm các thông tin sau không? 
- Mẫu chữ ký, chữ ký điện tử 
- Tên giao dịch đầy đủ, tên viết tắt, giấy phép hoặc quyết định thành lập, giấy chứng nhận đăng ký doanh nghiệp.
- Thông tin về tài khoản: Tên tài khoản, số hiệu tài khoản, số dư tài khoản, thông tin liên quan đến giao dịch nộp tiền, rút tiền, chuyển tiền,...."', 0, 4, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'98dff7ae-5765-4c72-b6b7-218f2e18dfc2', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'8d1aa2b7-8d62-49b3-80b6-5024a36cffb2', N'(b) Truy cập, Truy xuất, Chuyển giao, ….', 0, 2, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'ae8e66df-2eb1-4390-b3a8-261caa9971e8', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'4f3175d2-7a25-485f-8b57-293c8fa5809d', N'2. Dữ liệu cá nhân nhạy cảm', NULL, 1, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'22ee788e-f05f-41c2-9ff7-28a0bc31c3cf', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'89289ddd-0899-454b-b339-c68c68e175f0', N'9. Ghi chú', 1, 8, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'4f3175d2-7a25-485f-8b57-293c8fa5809d', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', NULL, N'I. Các loại Dữ liệu cá nhân CTTV đang thu thập, lưu trữ', NULL, 0, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'ec00a567-aaf2-400f-94c8-3023b35fae1f', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'5ba980e8-c3d2-4265-8e3f-899c6b86e6fd', N'5', NULL, 2, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'dfc1486c-429b-49ae-85f9-350c181a9d21', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'ae8e66df-2eb1-4390-b3a8-261caa9971e8', N'2.3 "CTTV có thu thập tình trạng sức khỏe và đời tư được ghi trong hồ sơ bệnh án, không bao gồm thông tin về nhóm máu không?
(Các thông tin liên quan kết quả chẩn đoán bệnh, loại bệnh, quá trình điều trị, thuốc uống,… của khách hàng hoặc CBNV)."', 0, 2, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'421bdc8b-3751-4ec6-8fc8-4b2e411e059b', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'ae8e66df-2eb1-4390-b3a8-261caa9971e8', N'2.2 CTTV có thu thập các Dữ liệu cá nhân liên quan tín ngưỡng, tôn giáo như: Đạo Phật, đạo Thiên chúa,…. Hoặc các quan điểm chính trị (nếu có) không?', 0, 1, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'1d740404-82a9-4e38-aa80-4c55f750f655', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'89289ddd-0899-454b-b339-c68c68e175f0', N'5. Ngôn ngữ lập trình sử dụng (.NET,Java, PHP,…)', 1, 4, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'd2a143cf-e307-466a-b384-4e3d0e4b4765', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'89289ddd-0899-454b-b339-c68c68e175f0', N'7. Business Owner ( Người phụ trách nghiệp vụ)', 1, 6, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'8d1aa2b7-8d62-49b3-80b6-5024a36cffb2', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'5ba980e8-c3d2-4265-8e3f-899c6b86e6fd', N'6', NULL, 3, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'cbbd5510-1d31-40b7-a64c-5065ac4e984f', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'93ea4ac0-f59b-4b74-8e06-fa7293cbb3a0', N'"(b) Dữ liệu cá nhân được lưu trên Cloud hoặc Server,…không? 
(Nếu có liệt kê chi tiết)"', 0, 1, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'd76fb0e6-2e00-451a-b1ba-54327a2a57ef', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'89289ddd-0899-454b-b339-c68c68e175f0', N'3. Người phụ trách hệ thống (Prroduction Developer)', 1, 2, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'c31db221-ea76-49f6-ab14-55fb1a1fa485', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'ae8e66df-2eb1-4390-b3a8-261caa9971e8', N'2.7 Dữ liệu cá nhân khác (Nếu có)', 0, 6, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'94bfffe1-9655-407e-9014-5fb78f96b1ca', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'4f3175d2-7a25-485f-8b57-293c8fa5809d', N'1. Dữ liệu cá nhân cơ bản', NULL, 0, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'10723a13-8fc2-4b4b-9f6c-612e0d57ae57', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'89289ddd-0899-454b-b339-c68c68e175f0', N'6. Hệ quản trị CSDL (SQL,mySQL, postgreSQL, Oracle, DB2, MongoDB,…)', 1, 5, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'2b57506a-86c7-4207-b581-6412337cff29', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'94bfffe1-9655-407e-9014-5fb78f96b1ca', N'1.1 CTTV có thu thập dữ liệu cá nhân gồm: Họ tên; ngày tháng năm sinh; giới tính; địa chỉ; Số điện thoại không?', 0, 0, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'dcf60b59-1cf4-413d-a2be-65a2d8544efb', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'89289ddd-0899-454b-b339-c68c68e175f0', N'2. Đã thưc hiện', 0, 1, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'cc413c81-67d3-4450-b6d9-67af37e55f09', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'8d1aa2b7-8d62-49b3-80b6-5024a36cffb2', N'Hệ thống có lưu trữ và ghi lại các hành động xử lý Dữ liệu cá nhân không?', 0, 0, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'0ea2e559-fd67-41bc-8a26-69293d5dc03a', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'ae8e66df-2eb1-4390-b3a8-261caa9971e8', N'2.1 "CTTV có thu thập dữ liệu cá nhân liên quan nguồn gốc chủng tộc, dân tộc không?
- Nguồn gốc chủng tộc (nếu có): người da vàng, người da trắng, người da đen,…
- Nguồn gốc Dân tộc: Dân tộc Kinh, Thái, Tày,…."', 0, 0, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'b3f9ab2e-b40e-4678-b130-6c584cdcb263', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'8d1aa2b7-8d62-49b3-80b6-5024a36cffb2', N'"(c) Hoặc các hành động khác có liên quan: Phân tích, Chia sẻ, Thu hồi,….
(Nếu có liệt kê cụ thể hành động)"', 0, 3, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'cb212557-b5cb-411d-a164-6eee7fd5c128', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'94bfffe1-9655-407e-9014-5fb78f96b1ca', N'1.4 CTTV có thu thập dữ liệu cá nhân gồm: Mã số thuế cá nhân; Số bảo hiểm xã hội, số thẻ bảo hiểm y tế; số tài khoản ngân hàng,…không?', 0, 3, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'5ba980e8-c3d2-4265-8e3f-899c6b86e6fd', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', NULL, N'II. Xử lý dữ liệu', NULL, 1, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'49e9c7d1-cde3-4d17-94de-9cea112f9579', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'5ba980e8-c3d2-4265-8e3f-899c6b86e6fd', N'3', NULL, 0, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'b2a02992-ccc4-4342-990f-a0042b3c9d1f', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'94bfffe1-9655-407e-9014-5fb78f96b1ca', N'1.2 CTTV có thu thập dữ liệu cá nhân gồm: CCCD/số hộ chiếu/số định danh; Quốc tịch không?', 0, 1, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'bdd10141-2997-449f-99e8-a1f111655ade', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'ae8e66df-2eb1-4390-b3a8-261caa9971e8', N'2.4 "CTTV có thu thập thông tin về thuộc tính vật lý, đặc điểm sinh học riêng của cá nhân không?
Ví dụ: đặc điểm khuôn mặt, ảnh chụp võng mạc, giọng nói, dấu vân tay,…"', 0, 3, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'a5576b44-c1db-4219-988e-aaf8c2a9fce4', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'49e9c7d1-cde3-4d17-94de-9cea112f9579', N'(a) Hệ thống có cho phép Khách hàng hoặc CBNV truy cập xem dữ liệu cá nhân không?', 0, 0, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'9e54112e-c3db-4e16-83f6-aba8a8be9057', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'8d1aa2b7-8d62-49b3-80b6-5024a36cffb2', N'(a) Thu thập, ghi, lưu trữ, Chỉnh sửa, xóa, sao chép', 0, 1, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'dd080ad5-1f21-4fb3-a94a-b2643880afdf', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'94bfffe1-9655-407e-9014-5fb78f96b1ca', N'1.5 "Các thông Dữ liệu cá nhân khác: Số giấy phép lái xe, Số biển số xe, Thông tin về tài khoản số của cá nhân; dữ liệu cá nhân phản ánh hoạt động, lịch sử hoạt động trên không gian mạng (lịch sử truy cập web, tìm kiếm và tra cứu thông tin, mua sắm hoặc thanh toán trực tuyến,...)
(Nếu có liệt kê chi tiết)"', 0, 4, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'26eec54d-b48f-4176-bbd2-b7138444eb4b', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'93ea4ac0-f59b-4b74-8e06-fa7293cbb3a0', N'"(d) Thiết bị/server lưu trữ có đặt tại nước ngoài không?
(Nếu có ghi chú địa điểm)"', 0, 3, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'75105c17-ed08-4a9b-8832-b82dd7965fa9', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'89289ddd-0899-454b-b339-c68c68e175f0', N'1. Ứng dụng này đang sử dụng triển khai (Consent Record) về thu thập dữ liệu', 0, 0, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'aa5090dd-ceb1-46f9-9f8b-bc1cde8bea23', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'49e9c7d1-cde3-4d17-94de-9cea112f9579', N'(b) Hệ thống có cho phép Khách hàng hoặc CBNV chỉnh sửa dữ liệu cá nhân không?', 0, 1, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'89289ddd-0899-454b-b339-c68c68e175f0', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', NULL, N'III. Thông tin quản lý', NULL, 2, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'b973cd07-c642-4a66-bb78-c946addf7ddf', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'89289ddd-0899-454b-b339-c68c68e175f0', N'4. Tần suất sao lưu backup', 1, 3, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'98f10322-2d75-4b7b-a2fb-ced921370e7c', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'49e9c7d1-cde3-4d17-94de-9cea112f9579', N'(c) Hệ thống có cho phép tạo yêu cầu chỉnh sửa/rút lại/xóa/hạn chế/phản đối Dữ liệu cá nhân không?', 0, 2, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'4c232583-7441-4e45-bc58-d09eef0d92ca', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'93ea4ac0-f59b-4b74-8e06-fa7293cbb3a0', N'(a) Dữ liệu cá nhân được lưu trữ bằng Phần cứng hoặc thiết bị vật lý không?', 0, 0, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'8c4d6507-ef16-40c1-bd36-d870c32274e6', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'89289ddd-0899-454b-b339-c68c68e175f0', N'8. Unit/Department', 1, 7, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'28307f64-ea83-4d18-86c7-d9be62922019', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'93ea4ac0-f59b-4b74-8e06-fa7293cbb3a0', N'(c) Thiết bị/server lưu trữ có được đặt tại trong nước không?', 0, 2, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'fca177e2-4a1d-440f-8365-eb1701ea48a8', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'94bfffe1-9655-407e-9014-5fb78f96b1ca', N'1.3 CTTV có thu thập dữ liệu cá nhân gồm: Hình ảnh của cá nhân; Tình trạng hôn nhân; Thông tin về mối quan hệ gia đình (cha mẹ, con cái) không?', 0, 2, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'416273fd-de9d-4286-99b4-f29a0b9b4793', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'ec00a567-aaf2-400f-94c8-3023b35fae1f', N'"(b) Dữ liệu cá nhân được lưu trữ phân tán, riêng lẻ không?
(Nếu có liệt kê chi tiết)"', 0, 1, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
INSERT [dbo].[FormElements] ([Id], [FormId], [ParentId], [Name], [DataType], [OrderIndex], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById]) VALUES (N'93ea4ac0-f59b-4b74-8e06-fa7293cbb3a0', N'74d50280-52fb-47fc-88ad-e8b3c7f5ff5f', N'5ba980e8-c3d2-4265-8e3f-899c6b86e6fd', N'4', NULL, 1, CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), CAST(N'2025-03-20T20:32:21.9583414' AS DateTime2), N'e02ec95c-5c94-470a-88ea-29cc32bca9d5', N'e02ec95c-5c94-470a-88ea-29cc32bca9d5')
GO
ALTER TABLE [FormElements] WITH CHECK CHECK CONSTRAINT FK_FormElements_FormElements_ParentId; -- Bật lại FK\


INSERT INTO [dbo].[Responsibilities] ([Id], [Title], [Description], [CreatedAt], [LastModifiedAt], [CreatedById], [LastModifiedById])
VALUES 
(NEWID(), 'DPIA Coordinator', 'Responsible for coordinating DPIA processes and ensuring compliance with data protection regulations.', GETDATE(), GETDATE(), NULL, NULL),
(NEWID(), 'Privacy Analyst', 'Analyze data processing activities to identify potential risks to personal data.', GETDATE(), GETDATE(), NULL, NULL),
(NEWID(), 'Risk Assessment Specialist', 'Perform risk assessments as part of the DPIA to identify potential threats and vulnerabilities.', GETDATE(), GETDATE(), NULL, NULL),
(NEWID(), 'Data Protection Officer', 'Oversee DPIA activities and ensure compliance with GDPR and other data protection laws.', GETDATE(), GETDATE(), NULL, NULL),
(NEWID(), 'Data Governance Specialist', 'Develop and maintain policies related to DPIA and data protection.', GETDATE(), GETDATE(), NULL, NULL),
(NEWID(), 'Compliance Officer', 'Monitor and enforce data protection compliance, including conducting DPIAs.', GETDATE(), GETDATE(), NULL, NULL),
(NEWID(), 'Risk Mitigation Expert', 'Develop and implement strategies to mitigate risks identified during DPIA.', GETDATE(), GETDATE(), NULL, NULL),
(NEWID(), 'Data Privacy Consultant', 'Advise on DPIA processes and best practices for data protection.', GETDATE(), GETDATE(), NULL, NULL),
(NEWID(), 'Legal Advisor', 'Provide legal guidance on data protection and DPIA requirements.', GETDATE(), GETDATE(), NULL, NULL),
(NEWID(), 'Data Privacy Auditor', 'Audit DPIA activities and ensure compliance with data protection standards.', GETDATE(), GETDATE(), NULL, NULL);


-- Seed dữ liệu Privacy Policy
GO
INSERT INTO [dbo].[PrivacyPolicies]
           ([Id], [PolicyCode], [Title], [Description], [Content], [Status], [CreatedAt], [LastModifiedAt])
     VALUES
           (NEWID(), 'PP004', N'FE Privacy Policy', N'FE Privacy policy', N'<p style="margin-top:0in;margin-right:0in;margin-bottom:8.0pt;margin-left:0in;font-size:13.0pt;font-family:"Times New Roman",serif;text-align:center;"><strong>QUY ĐỊNH BẢO VỆ DỮ LIỆU C&Aacute; NH&Acirc;N</strong></p>
<p style="margin-top:0in;margin-right:0in;margin-bottom:8.0pt;margin-left:0in;font-size:13.0pt;font-family:"Times New Roman",serif;text-align:center;"><em>(Ban h&agrave;nh k&egrave;m theo Quyết định số 175/QĐ-CTGDẸPT ng&agrave;y 21/11/2024 của Tổng Gi&aacute;m đốc C&ocirc;ng ty TNHH Gi&aacute;o dục FPT)</em></p>
<p style="margin-top:0in;margin-right:0in;margin-bottom:8.0pt;margin-left:0in;font-size:13.0pt;font-family:"Times New Roman",serif;"><strong>Lời n&oacute;i đầu&nbsp;</strong></p>
<p style="margin-top:0in;margin-right:0in;margin-bottom:8.0pt;margin-left:0in;font-size:13.0pt;font-family:"Times New Roman",serif;">Quy định n&agrave;y m&ocirc; tả c&aacute;ch thức C&ocirc;ng ty TNHH Gi&aacute;o dục FPT (Tổ chức Gi&aacute;o dục FPT - FPT Edu) v&agrave; c&aacute;c cơ sở gi&aacute;o dục trong FPT Edu thu thập v&agrave; sử dụng th&ocirc;ng tin c&aacute; nh&acirc;n của Chủ thể dữ liệu li&ecirc;n quan đến c&aacute;c trang web, ứng dụng, sản phẩm v&agrave; dịch vụ của FPT Edu (gọi chung l&agrave; &ldquo;Dịch vụ&rdquo;).</p>
<p style="margin-top:0in;margin-right:0in;margin-bottom:8.0pt;margin-left:0in;font-size:13.0pt;font-family:"Times New Roman",serif;"><strong>Điều 1. Mục đ&iacute;ch:</strong></p>
<p style="margin-top:0in;margin-right:0in;margin-bottom:8.0pt;margin-left:0in;font-size:13.0pt;font-family:"Times New Roman",serif;">Tu&acirc;n thủ quy định về Bảo vệ dữ liệu c&aacute; nh&acirc;n theo Nghị định số 13/2023/NĐ-CP.</p>
<p style="margin-top:0in;margin-right:0in;margin-bottom:8.0pt;margin-left:0in;font-size:13.0pt;font-family:"Times New Roman",serif;"><strong>Điều 2. Phạm vi &aacute;p dụng:</strong></p>
<p style="margin-top:0in;margin-right:0in;margin-bottom:8.0pt;margin-left:0in;font-size:13.0pt;font-family:"Times New Roman",serif;"><strong>Điều 2. Phạm vi &aacute;p dụng (tiếp):</strong></p>
<ol style="list-style-type: decimal;">
    <li>Quy chế n&agrave;y cụ thể h&oacute;a một số quy định về bảo vệ dữ liệu c&aacute; nh&acirc;n được &aacute;p dụng tại <strong>FPT Edu v&agrave; c&aacute;c đơn vị th&agrave;nh vi&ecirc;n của FPT Edu.</strong></li>
    <li>C&aacute;c đơn vị th&agrave;nh vi&ecirc;n thuộc FPT Edu c&oacute; tr&aacute;ch nhiệm thực hiện theo c&aacute;c quy định tại <strong>Nghị định số 13/2023/NĐ-CP&nbsp;</strong>ng&agrave;y 17/4/2023 của Ch&iacute;nh phủ về Bảo vệ dữ liệu c&aacute; nh&acirc;n <strong>v&agrave; c&aacute;c điểm cụ thể trong quy định n&agrave;y.</strong></li>
    <li>Quy định n&agrave;y &aacute;p dụng đối với <strong>người học, cựu người học, phụ huynh của người học&nbsp;</strong>đang học tại c&aacute;c cơ sở đ&agrave;o tạo thuộc FPT Edu.</li>
</ol>
<p style="margin-top:0in;margin-right:0in;margin-bottom:8.0pt;margin-left:0in;font-size:13.0pt;font-family:"Times New Roman",serif;"><strong>Điều 3. Quy định bảo vệ:</strong></p>
<p style="margin-top:0in;margin-right:0in;margin-bottom:8.0pt;margin-left:0in;font-size:13.0pt;font-family:"Times New Roman",serif;">Tổ chức Gi&aacute;o dục FPT (FPT Edu) th&ocirc;ng b&aacute;o đến Phụ huynh, Người học v&agrave; Cựu người học (gọi chung l&agrave; Chủ thể dữ liệu) v&agrave; <strong>cần sự đồng &yacute; của Chủ thể dữ liệu để xử l&yacute;, thu thập v&agrave; lưu trữ dữ liệu c&aacute; nh&acirc;n của Chủ thể dữ liệu trong suốt qu&aacute; tr&igrave;nh sử dụng c&aacute;c trang web, ứng dụng, sản phẩm v&agrave; dịch vụ (sau đ&acirc;y gọi tắt l&agrave; Dịch vụ&rdquo;) của FPT Edu tại Quy định n&agrave;y.</strong> FPT Edu cam kết tu&acirc;n thủ v&agrave; giữ th&ocirc;ng tin của Chủ thể dữ liệu ri&ecirc;ng tư v&agrave; an to&agrave;n theo đ&uacute;ng c&aacute;c quy định của ph&aacute;p luật.</p>
<p style="margin-top:0in;margin-right:0in;margin-bottom:8.0pt;margin-left:0in;font-size:13.0pt;font-family:"Times New Roman",serif;"><strong>Điều 4. C&aacute;c dữ liệu c&aacute; nh&acirc;n được FPT Edu thu thập:</strong></p>
<p style="margin-top:0in;margin-right:0in;margin-bottom:8.0pt;margin-left:0in;font-size:13.0pt;font-family:"Times New Roman",serif;">FPT Edu thu thập th&ocirc;ng tin c&aacute; nh&acirc;n của Chủ thể dữ liệu trong qu&aacute; tr&igrave;nh cung cấp Dịch vụ giảng dạy v&agrave; học tập của FPT Edu.</p>
<p style="margin-top:0in;margin-right:0in;margin-bottom:8.0pt;margin-left:0in;font-size:13.0pt;font-family:"Times New Roman",serif;">Dưới đ&acirc;y l&agrave; c&aacute;c nguồn th&ocirc;ng tin được FPT Edu thu thập:</p>
<ol style="list-style-type: decimal;">
    <li><strong>C&aacute;c th&ocirc;ng tin nhập học</strong> theo quy định của c&aacute;c cơ quan quản l&yacute; nh&agrave; nước.</li>
    <li><strong>C&aacute;c th&ocirc;ng tin ph&aacute;t sinh</strong> trong qu&aacute; tr&igrave;nh cung cấp dịch vụ.</li>
    <li><strong>C&aacute;c th&ocirc;ng tin cần cung cấp&nbsp;</strong>theo y&ecirc;u cầu của c&aacute;c cơ quan quản l&yacute; nh&agrave; nước c&oacute; thẩm quyền.</li>
</ol>
<p style="margin-top:0in;margin-right:0in;margin-bottom:8.0pt;margin-left:0in;font-size:13.0pt;font-family:"Times New Roman",serif;"><strong>Điều 5. Sử dụng th&ocirc;ng tin c&aacute; nh&acirc;n:</strong></p>
<p style="margin-top:0in;margin-right:0in;margin-bottom:8.0pt;margin-left:0in;font-size:13.0pt;font-family:"Times New Roman",serif;">FPT Edu sử dụng th&ocirc;ng tin c&aacute; nh&acirc;n của Chủ thể dữ liệu để vận h&agrave;nh, cung cấp v&agrave; cải thiện Dịch vụ. Mục đ&iacute;ch sử dụng th&ocirc;ng tin c&aacute; nh&acirc;n của FPT Edu bao gồm:</p>
<ol style="list-style-type: decimal;">
    <li>FPT Edu sử dụng th&ocirc;ng tin c&aacute; nh&acirc;n của Chủ thể dữ liệu để xử l&yacute; c&aacute;c vấn đề li&ecirc;n quan trong qu&aacute; tr&igrave;nh cung cấp v&agrave; sử dụng Dịch vụ của FPT Edu.</li>
    <li>FPT Edu sử dụng th&ocirc;ng tin c&aacute; nh&acirc;n của Chủ thể dữ liệu để đo lường việc sử dụng, ph&acirc;n t&iacute;ch hiệu quả để cải thiện chất lượng v&agrave; ph&aacute;t triển Dịch vụ.</li>
    <li>FPT Edu sử dụng th&ocirc;ng tin c&aacute; nh&acirc;n của Chủ thể dữ liệu để đề xuất Dịch vụ m&agrave; Chủ thể dữ liệu c&oacute; thể quan t&acirc;m v&agrave; tham gia trải nghiệm.</li>
    <li>Trong một số trường hợp nhất định, FPT Edu c&oacute; nghĩa vụ ph&aacute;p l&yacute; để thu thập, sử dụng hoặc lưu trữ th&ocirc;ng tin c&aacute; nh&acirc;n của Chủ thể dữ liệu.</li>
    <li>FPT Edu sử dụng th&ocirc;ng tin c&aacute; nh&acirc;n của Chủ thể dữ liệu để li&ecirc;n lạc v&agrave; để trả lời y&ecirc;u cầu của Chủ thể dữ liệu.</li>
    <li>FPT Edu sử dụng th&ocirc;ng tin c&aacute; nh&acirc;n của Chủ thể dữ liệu để thực hiện c&ocirc;ng t&aacute;c truyền th&ocirc;ng, quảng b&aacute; về Dịch vụ v&agrave; c&aacute;c chương tr&igrave;nh gi&aacute;o dục theo quy định của ph&aacute;p luật. <strong>Trong trường hợp một số mục đ&iacute;ch cụ thể kh&aacute;c chưa được n&ecirc;u trong quy định n&agrave;y cần sự đồng &yacute; của Chủ thể dữ liệu, FPT Edu sẽ xin chấp thuận của Chủ thể dữ liệu về việc sử dụng th&ocirc;ng tin c&aacute; nh&acirc;n cho mục đ&iacute;ch n&agrave;y.</strong> Tuy nhi&ecirc;n, trong trường hợp kh&ocirc;ng cung cấp một số th&ocirc;ng tin nhất định, Chủ thể dữ liệu c&oacute; thể kh&ocirc;ng sử dụng được một số Dịch vụ.</li>
</ol>
<p style="margin-top:0in;margin-right:0in;margin-bottom:8.0pt;margin-left:0in;font-size:13.0pt;font-family:"Times New Roman",serif;"><strong>Điều 6. Th&ocirc;ng tin c&aacute;c tổ chức, c&aacute; nh&acirc;n được xử l&yacute; dữ liệu:</strong></p>
<p style="margin-top:0in;margin-right:0in;margin-bottom:8.0pt;margin-left:0in;font-size:13.0pt;font-family:"Times New Roman",serif;">Th&ocirc;ng tin về Chủ thể dữ liệu l&agrave; một phần quan trọng trong hoạt động của FPT Edu. FPT Edu kh&ocirc;ng b&aacute;n th&ocirc;ng tin c&aacute; nh&acirc;n của Chủ thể dữ liệu cho người kh&aacute;c. FPT Edu chia sẻ th&ocirc;ng tin c&aacute; nh&acirc;n cho c&aacute;c tổ chức, c&aacute; nh&acirc;n nhằm phục vụ qu&aacute; tr&igrave;nh cung cấp dịch vụ của FPT Edu. C&aacute;c tổ chức, c&aacute; nh&acirc;n n&agrave;y phải tu&acirc;n thủ nghi&ecirc;m ngặt Quy định bảo vệ dữ liệu c&aacute; nh&acirc;n của ch&uacute;ng t&ocirc;i v&agrave; c&aacute;c quy định ph&aacute;p luật hiện h&agrave;nh về quyền ri&ecirc;ng tư v&agrave; bảo vệ dữ liệu c&aacute; nh&acirc;n.</p>
<p style="margin-top:0in;margin-right:0in;margin-bottom:8.0pt;margin-left:0in;font-size:13.0pt;font-family:"Times New Roman",serif;">Ngo&agrave;i ra, FPT Edu c&ograve;n chia sẻ th&ocirc;ng tin c&aacute; nh&acirc;n cho: C&ocirc;ng ty Cổ phần FPT; c&aacute;c C&ocirc;ng ty th&agrave;nh vi&ecirc;n; c&aacute;c c&ocirc;ng ty li&ecirc;n kết thuộc Tập đo&agrave;n FPT; Ch&uacute;ng t&ocirc;i c&oacute; thể sử dụng v&agrave;/hoặc hợp t&aacute;c với c&aacute;c đơn vị th&agrave;nh vi&ecirc;n của Tập đo&agrave;n FPT để triển khai một số c&ocirc;ng việc v&agrave; chương tr&igrave;nh d&agrave;nh cho Chủ thể dữ liệu, bao gồm c&aacute;c chương tr&igrave;nh ưu đ&atilde;i, b&aacute;n h&agrave;ng ch&eacute;o. C&aacute;c hoạt động c&oacute; thể bao gồm: gửi th&ocirc;ng tin li&ecirc;n lạc, xử l&yacute; thanh to&aacute;n, đ&aacute;nh gi&aacute; rủi ro t&iacute;n dụng, ph&acirc;n t&iacute;ch dữ liệu c&aacute; nh&acirc;n để tu&acirc;n thủ quy định, cung cấp hỗ trợ tiếp thị v&agrave; b&aacute;n h&agrave;ng (bao gồm quản l&yacute; quảng c&aacute;o v&agrave; sự kiện), quản l&yacute; quan hệ Chủ thể dữ liệu v&agrave; cung cấp đ&agrave;o tạo. C&aacute;c đơn vị được FPT Edu chia sẻ dữ liệu c&aacute; nh&acirc;n n&ecirc;u tr&ecirc;n sẽ chỉ được truy cập v&agrave;o th&ocirc;ng tin c&aacute; nh&acirc;n cần thiết để thực hiện c&aacute;c chức năng được giao, v&agrave; kh&ocirc;ng được ph&eacute;p sử dụng th&ocirc;ng tin n&agrave;y cho bất kỳ mục đ&iacute;ch n&agrave;o kh&aacute;c. Đồng thời, c&aacute;c đơn vị n&agrave;y phải tu&acirc;n thủ nghi&ecirc;m ngặt Quy định bảo vệ dữ liệu c&aacute; nh&acirc;n của ch&uacute;ng t&ocirc;i v&agrave; c&aacute;c quy định ph&aacute;p luật hiện h&agrave;nh về quyền ri&ecirc;ng tư v&agrave; bảo vệ dữ liệu c&aacute; nh&acirc;n.</p>
<p style="margin-top:0in;margin-right:0in;margin-bottom:8.0pt;margin-left:0in;font-size:13.0pt;font-family:"Times New Roman",serif;"><strong>Điều 7. Quyền v&agrave; nghĩa vụ của Chủ thể dữ liệu li&ecirc;n quan đến dữ liệu c&aacute; nh&acirc;n cung cấp cho FPT:</strong></p>
<ol style="list-style-type: decimal;">
    <li>Chủ thể dữ liệu c&oacute; quyền được biết về hoạt động xử l&yacute; dữ liệu c&aacute; nh&acirc;n của m&igrave;nh, trừ trường hợp ph&aacute;p luật c&oacute; quy định kh&aacute;c.<br>&nbsp;a) Chủ thể dữ liệu được đồng &yacute; hoặc kh&ocirc;ng đồng &yacute; cho ph&eacute;p xử l&yacute; dữ liệu c&aacute; nh&acirc;n của m&igrave;nh, trừ trường hợp luật c&oacute; quy định kh&aacute;c.<br>&nbsp;b) Chủ thể dữ liệu c&oacute; c&aacute;c quyền kh&aacute;c theo quy định của ph&aacute;p luật hiện h&agrave;nh để bảo vệ dữ liệu c&aacute; nh&acirc;n của m&igrave;nh.</li>
    <li>Nghĩa vụ của Chủ thể dữ liệu<br>&nbsp;a) Tự bảo vệ dữ liệu c&aacute; nh&acirc;n của m&igrave;nh trong qu&aacute; tr&igrave;nh sử dụng dịch vụ của FPT Edu, bao gồm c&aacute;c th&ocirc;ng tin li&ecirc;n quan đến mật khẩu đăng nhập v&agrave;o t&agrave;i khoản của Chủ thể dữ liệu, OTP; v&agrave; thực hiện c&aacute;c biện ph&aacute;p bảo mật th&ocirc;ng tin như bảo quản thiết bị điện tử trong qu&aacute; tr&igrave;nh sử dụng; kh&oacute;a, đăng xuất, hoặc tho&aacute;t khỏi t&agrave;i khoản tr&ecirc;n website hoặc Ứng dụng của FPT v&agrave; FPT Edu khi kh&ocirc;ng sử dụng; v&agrave; thực hiện c&aacute;c biện ph&aacute;p bảo mật kh&aacute;c.<br>&nbsp;b) Thường xuy&ecirc;n cập nhật c&aacute;c quy định, ch&iacute;nh s&aacute;ch bảo vệ dữ liệu c&aacute; nh&acirc;n của FPT Edu từng thời kỳ, được th&ocirc;ng b&aacute;o tới Chủ thể dữ liệu hoặc đăng tải tr&ecirc;n c&aacute;c website v&agrave; hoặc c&aacute;c k&ecirc;nh giao dịch kh&aacute;c của FPT Edu.<br>&nbsp;c) Tu&acirc;n thủ c&aacute;c quy định của ph&aacute;p luật, quy định, hướng dẫn của FPT Edu li&ecirc;n quan đến xử l&yacute; dữ liệu c&aacute; nh&acirc;n của Chủ thể dữ liệu.</li>
</ol>
<p style="margin-top:0in;margin-right:0in;margin-bottom:8.0pt;margin-left:0in;font-size:13.0pt;font-family:"Times New Roman",serif;"><strong>Điều 8. Lưu trữ dữ liệu c&aacute; nh&acirc;n</strong></p>
<p style="margin-top:0in;margin-right:0in;margin-bottom:8.0pt;margin-left:0in;font-size:13.0pt;font-family:"Times New Roman",serif;">FPT Edu cam kết sẽ chỉ lưu trữ dữ liệu c&aacute; nh&acirc;n của Chủ thể dữ liệu trong trường hợp li&ecirc;n quan đến c&aacute;c mục đ&iacute;ch được n&ecirc;u trong Quy định n&agrave;y. Thời hạn lưu trữ dữ liệu c&aacute; nh&acirc;n do FPT Edu quyết định đảm bảo thực hiện được c&aacute;c mục đ&iacute;ch n&ecirc;u tr&ecirc;n.</p>
<p style="margin-top:0in;margin-right:0in;margin-bottom:8.0pt;margin-left:0in;font-size:13.0pt;font-family:"Times New Roman",serif;"><strong>Điều 9. Hậu quả, thiệt hại kh&ocirc;ng mong muốn c&oacute; thể xảy ra</strong></p>
<p style="margin-top:0in;margin-right:0in;margin-bottom:8.0pt;margin-left:0in;font-size:13.0pt;font-family:"Times New Roman",serif;">Hiện tại, FPT Edu chưa thấy hậu quả, thiệt hại kh&ocirc;ng mong muốn n&agrave;o c&oacute; thể xảy ra, FPT Edu sẽ th&ocirc;ng b&aacute;o nếu xảy ra trường hợp cụ thể. Trong qu&aacute; tr&igrave;nh xử l&yacute; dữ liệu c&aacute; nh&acirc;n, c&oacute; thể xảy ra lỗi, lọt dữ liệu c&aacute; nh&acirc;n do:</p>
<p style="margin-top:0in;margin-right:0in;margin-bottom:8.0pt;margin-left:0in;font-size:13.0pt;font-family:"Times New Roman",serif;">a) Từ ph&iacute;a Chủ thể dữ liệu: Chủ thể dữ liệu l&agrave;m lộ, lọt dữ liệu c&aacute; nh&acirc;n do: bất cẩn hoặc bị lừa đảo; truy cập c&aacute;c website/tải c&aacute;c ứng dụng c&oacute; chứa phần mềm độc hại, vv... Chủ thể dữ liệu tự thực hiện c&aacute;c biện ph&aacute;p để chủ động bảo vệ dữ liệu c&aacute; nh&acirc;n của m&igrave;nh.<br>&nbsp;b) Từ ph&iacute;a FPT Edu: FPT Edu cam kết sử dụng c&aacute;c c&ocirc;ng nghệ bảo mật th&ocirc;ng tin nhằm bảo vệ Dữ liệu c&aacute; nh&acirc;n của Chủ thể dữ liệu. Tuy nhi&ecirc;n, FPT Edu kh&ocirc;ng chịu tr&aacute;ch nhiệm trong c&aacute;c trường hợp mất m&aacute;t dữ liệu c&aacute; nh&acirc;n của Chủ thể dữ liệu do lỗi phần cứng, phần mềm trong qu&aacute; tr&igrave;nh xử l&yacute; dữ liệu; hoặc lỗ hổng bảo mật nằm ngo&agrave;i khả năng kiểm so&aacute;t của FPT Edu, hệ thống c&oacute; li&ecirc;n quan bị hacker tấn c&ocirc;ng g&acirc;y lộ, lọt dữ liệu c&aacute; nh&acirc;n...</p>
<p style="margin-top:0in;margin-right:0in;margin-bottom:8.0pt;margin-left:0in;font-size:13.0pt;font-family:"Times New Roman",serif;"><strong>Điều 10. Th&ocirc;ng tin li&ecirc;n lạc, th&ocirc;ng b&aacute;o v&agrave; sửa đổi</strong></p>
<p style="margin-top:0in;margin-right:0in;margin-bottom:8.0pt;margin-left:0in;font-size:13.0pt;font-family:"Times New Roman",serif;">Quy định bảo vệ dữ liệu c&aacute; nh&acirc;n của FPT Edu li&ecirc;n tục được cập nhật. Bản cập nhật Quy định n&agrave;y được th&ocirc;ng b&aacute;o tới c&aacute;c Chủ thể dữ liệu th&ocirc;ng qua website v&agrave; c&aacute;c k&ecirc;nh giao dịch của FPT Edu trước khi &aacute;p dụng. Việc Chủ thể dữ liệu tiếp tục sử dụng dịch vụ sau thời hạn th&ocirc;ng b&aacute;o về c&aacute;c nội dung sửa đổi, bổ sung trong từng thời kỳ đồng nghĩa với việc Chủ thể dữ liệu đ&atilde; chấp nhận c&aacute;c nội dung sửa đổi, bổ sung của Quy định bảo vệ dữ liệu n&agrave;y.</p>
<p style="margin-top:0in;margin-right:0in;margin-bottom:8.0pt;margin-left:0in;font-size:13.0pt;font-family:"Times New Roman",serif;">Nếu Chủ thể dữ liệu c&oacute; c&acirc;u hỏi n&agrave;o về Quy định bảo vệ n&agrave;y hoặc nếu c&oacute; thắc mắc n&agrave;o kh&aacute;c li&ecirc;n quan đến c&aacute;ch FPT Edu quản l&yacute;, bảo vệ v&agrave; /hoặc xử l&yacute; dữ liệu c&aacute; nh&acirc;n, vui l&ograve;ng li&ecirc;n hệ với FPT Edu để được hỗ trợ.</p>
<p style="margin-top:0in;margin-right:0in;margin-bottom:8.0pt;margin-left:0in;font-size:13.0pt;font-family:"Times New Roman",serif;"><strong>Điều 11. Điều khoản thi h&agrave;nh</strong></p>
<p style="margin-top:0in;margin-right:0in;margin-bottom:8.0pt;margin-left:0in;font-size:13.0pt;font-family:"Times New Roman",serif;">Quy định n&agrave;y gồm 11 điều được &aacute;p dụng cho hoạt động bảo vệ dữ liệu c&aacute; nh&acirc;n tại Tổ chức gi&aacute;o dục FPT. Việc thay đổi nội dung trong Quy định n&agrave;y do Tổng Gi&aacute;m đốc C&ocirc;ng ty TNHH Gi&aacute;o dục FPT quyết định.</p>
<p style="margin-top:0in;margin-right:0in;margin-bottom:8.0pt;margin-left:0in;font-size:13.0pt;font-family:"Times New Roman",serif;">&nbsp;</p>
<p style="margin-top:0in;margin-right:0in;margin-bottom:8.0pt;margin-left:0in;font-size:13.0pt;font-family:"Times New Roman",serif;">&nbsp;</p>
<div id="envidictionary">
    <div class="o-search-mobile" style="top: 0px; left: 0px; display: none;"><img alt="ENVI" width="27" height="27" src="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAADYAAAA2CAYAAACMRWrdAAAACXBIWXMAADsOAAA7DgHMtqGDAAAAIGNIUk0AAHolAACAgwAA+f8AAIDpAAB1MAAA6mAAADqYAAAXb5JfxUYAAAx2SURBVHja1Jp5kFzFfYC/7n7XXLsze2hXKyFxCCSEhIwMAnSAwAkVEwNOAnE5roqTcqpM2XFiKBIfxJUQu5xKnNhJxbhcuVw5bALmpky45BgMJuEqJIyEBLrRrvaa3dk539Hd+WOWEdKuxK7YtZSumj9m3pv3+uvf/esW1loAuAPQMJwJ+blfJKjYXEPpPyDh101ilmW7PMcNpDZ68v5TNzwQvlLm6ypVvd0qcLXFVDTVBlz6+RUAOK3bTfMjgDhJfs3G9l4TWymA5q+n07AkCV+OyoUX3bp5sGIszvlZFvfK1h0tsJpjCFxJKW58Om5E31XCQSKxVmINNAVrTx82KQlqjRW4kgMruuhb6bJoEVPBXswPYWJzcTTR+K4bZamH7QihcdwGxoCOPUhXTxsulRjilGRgVSeVtItXM8A0ErM6EUmVPw3H+sjkRzlj2RYKC3eRbhvGC0YZG1zPwFs34bhlEEdLzpr5mbyQx7/mhJrB8/KfK3d7v5kaTITOyNuqCVsyzjFgZqjjurAhPnLGqqdZfvndpHv2EI/3UJvoQbkhUkXYY2xNKEFYSjCJRXpi7jRVgAktypN4bQo7jcOyzan0yYi+IBAMluxT//WquenGi+W9R0tMVi9dsfFhll39j1QPXsDWh75IsX85YTWP1g7tvYJ0voRJjjw8KmvalwYs2ZBDKIFJ5oZMOgITWw48O0F5IMJNTS86YZpLLURTa0xkPwQcDbbmV7+xoOvslxl45TpefOiPqJY82jrrBNlRHDTKyYKVk+4TrGlCLN2UI9vrzYsqLtmQ4+d3j2DNidXS2iac4+BOsbGuJa+pQ9uu4dm7vkD3kgOsuuoxCgt3EWRH8dLDDO69hv3bPo3rjzdtzIJQzdVtedZtrxP3D2C1xjZCcldfgY0i4sEh0heuwsYxjTd3ozJpooFBMpesRSiFqVSpv74Dp6sTU6uTWr2yJTkhxaQ3FjOLA8eC9W/fbLc//UlWbnqE1dfcCVIz/vYFFPtXoNQS6hOLkE58/Odby6E//gpJcQxvUR/hgYNkLrmIyk+f5+0/+SoX7n0NjOHQl+6g+7O/x6Ev/zln/vO3Sa9ZzdhDP2Lshw9S+I3rKX7/Hs597P6TjG5HZtcCG3jrMlZs/AFnrL+Lw69cx56Xr6NSXEwSBxgtyfdCpjCBTqYns9pggcXf+Bq5Kze862XP09i3i9F/u4vO3/44KEn2sksIVpzLxONbSK9ZTeWZn5G7Yj3BecsQqWD2vkaANlCPqE4BW7buftoX7uDNx3+fN577BEImBNkivtvAGovju1grjxukhZKo9jaKP3yA2tbXMNUaC790K0Ipcus2MP7oE2Q3Xo7KZlFtbeSu3Ej5x88AEO7dR/dnPoWtNxBSzhjGGkkSg2koAteOrlrEPVPAsp0H2PfyDbz+zCdJtw/ievUmyKR6ixmouHAc4rf7IYoxYQRAUhyj4+M3Eg8c5vBffguns4CNItquvpLxRx6j9PgWRBCQXrOa0mNPzlhKRnu4/gjZ/EESbH9XPrdx5dK+ve9oYwtseO9adr98PUFmHNdrTEpnFvqtDXq8RO8XbiF3xfrW78nQME5XJz2f/ww71l9DatX5mDDCP+csnI4CA1/9K3KbNzYnW63NAiyHn95Kvvt/MK7f5we9XxkN193Wl15R5N05yK7/vREdB/jpEtaeRNIrBXqsxPh9DzN238MMfvNOTLUKxtLYvhN3YS+5KzdQefo5bKMBQHDuORSfe4L0xWsB0KUycf/hGRqWwRoPrduR9FCuhr/72sBTt0xRxUa52zpe9eSgACEl7df/CrWXX6V4933Eh4fIbriU7BWXE+7ZD0DPH96M8Bxwmq9tu+Zqend8iuylFwPgn7WU3C9tnl2KgsBai5IuUqreKWCOXxPY91eeLPzirSe8nr5oDWf+w9+3vmcuWcvZ//FPre+5qzaRu2rT+8jEZAtAnvRTpMBEltpwMm8ZfHWkmYcKMfsFd056dQQ4Kcmep8Yp7mmgfDltsnpSa6YEcV0zvreBm5Ezr3OtMO8bDEB5Ah1bhrZVZ5H1zDAxkuDnFNIVxymLBAiDEDFCJJPh1eycEzBrmqsbFJx5U8eZ1HpWq4hUeLPtHPnenICd0gpalWhULqA4cCHBOOVKp3lpsDvhqvftPE7xEFKjdZqo0UFoOjrlaNe2nq29H+P/O1izBjMoBcLROEKTbujNc6eK9p0O1i+25zGdL0nUNIXmybr8JLSYZJaTmKHTUB4oX8ymWWTnBCycMCzamGLBWp9oYg5bVQLctOTAj2sUd0R4OTlT9RRzAuYEgvLbSVNq0dzqo3QE4ZjBCcSMRSXcaQrNk26TxRZds3PWoWpNVE02jI7DJTUkKUmtC/yqAs2Qm/CD9wCz6DhFHKYxRmG0xM9FeOky6CNqkdQt7We5LPjg/KjiwS01akMaLzeVLk6naTt4AG9oCKOdwxmpLl/gZ/ZxwepjwSxCWKJ6jijMks0fptD3Bn56DOnUMeZsGtXzEKLR+offLun/WZ2D/11HqLl1HiaxuBmJn5dYPfV61NZB56sPUHj932mkent93/laR967hZv+ZfjohqlR1MpdBNlRVl7yAH0rfkoqPwjagWCY4Z03sOvFtXhBhJjMNcOSoW99igUf8InK8yCxn9QovjG98xDaoIM09XwvJtNDxdpPlCrh/vPg9qPAquML6V6ylQ98+G9J9exmZPtV7H7po9TLXQirkV4vfqrUapgCSFfQGNOMvxWRNOycgilPENcM0hUnjjdCNB2HEDhBasEUG1t+2d2cve4+hICX/vMvGNr7QXTiNXv2RlBYHJFur6HjI6vnZgTjb8aMbI3mXBWtAS8rcDNiWlU8bjoyBWzD92lUOnnhwdsp9i8n23EIpRKsbQZIx3WxRh3VfrManJTASc3fxuCMoY70Co4GC6sdbH/6dyj2L6e9e19zw8+K02qv770FdiRHaenVnleuY/jAGrKF/lm33k5lItwsnJui8i07pkjs4PbNOG4DOal+x8Y1IZtF5ekyEimRuobSVawWRC5/NtqV/dbiY8GUEyOEngolmkuSRBYwre2jU15oulVqHeu+mXr7udv8+mBn3esdKWX8qc5DSH1cexIS6uPJ6aOCxuLXRqD9/NF4+W12ya6/GQka+/GSZPZdKnE6nYhQgrBi8fUhGtlzOHjubaTGf4KQ/uzArJ2My/KdjbjpHa3VFk6QuM5J7BZNDXJ8RWU0svGBPYylCiB/C2cknAHY5A5LY0xjYotSBpFOI/N50FODi3AcdKmEKZcRjjMvHlBICAoOQgqiqqY+nlirBSIuARPEAHSfGEwIqI9o8ud4LNqUwTsjT+3J5yn/69dRnR1T7tfDw2Q/dhPpj1yNHirNi7T8dsXOe8cY2lZDugIBSF9O6xuOC9YoajrO97no5i7cyXA30VnEvvUjnHDxFKOL9+yho20ThR6Ie1JzDuWhKJciRt5okEQGz1NNsziOw3OmS0fs5CGXM385h4OgXI5xcw5R7BIHC7Bu5xSwJKgSJT51IKnqOQdzMpJDL1WpDsRkF7nvmRE574rByZH8zOKlFV5GEWGONGr8ALWgB9nZNQVM1iNEKjV/TgOIq2bGjZ13SUwMv7MMwhGE45racExHr0dEc+NceC4y34Fsy0/xfKrQQPj+vKWWBvDzCqlmCaZC8YLxDFZOGqUr2P3oBNklLm3tHhJIXIGOQmTSPBYhrMZO1is6DHEdyABJZu69okKw9PIcu84ep3woIsgrhJqBjdUL9hGpedQJ7bUg8NsVtcMJr/zdCL3r0jg9XYjXa3hphRUWJym1hBY77XhZxciuOoMFEOMT85BugNeuyJ/po+sWpEXH9rj5awusUHWNE+s7QhVeW3McpLYEHYq4atjz8AQN5dMbV1jujxBFIcX8Zor5D5EvPUvX2JP4wQhvbatwcGeVlCnNC5i1kFng0LEsIK4ZSodCEYdmWrgW2P5KikTygu+5n+2U9TuNmSwwUxInJQlkHZ2cx+jwFTT8Mxjs/ihaZqgHS9AqR6a+k6jrAvJeDWXmdxMnrhqkJ0gtcGxU0ScGe7HQThU4x0TfWVptjJR9ezeC1lE+pavEToH9iz7X/GNSxjPDGOkz3PlhhrkWZeo4utKyu/kcWhvcDrEr6HKn3Ts4coAlASnAFyBDcY90zBORJ2+VRtwgsEutUK40oRY2RlgLWKxwEFbjJiWsEAhrfhFQbpIyQWrU/es13+u4X0US7b6LbPJAxP8NAC7oZKX0Fn34AAAAAElFTkSuQmCC" style="width: 27px; height: 27px;"></div>
    <div class="o-popup-tag o-bg-white o-border o-rounded o-shadow" style="width: 400px; top: 0px; left: 0px; display: none;">
        <div>
            <ul class="o-nav o-nav-tabs o-pop-nav">
                <li><span><svg aria-hidden="true" data-icon="volume-high" xmlns="http://www.w3.org/2000/svg" class="o-pop-speak o-link-secondary o-svg-inline--fa">&nbsp;<path fill="currentColor" d="M533.6 32.5C598.5 85.3 640 165.8 640 256s-41.5 170.8-106.4 223.5c-10.3 8.4-25.4 6.8-33.8-3.5s-6.8-25.4 3.5-33.8C557.5 398.2 592 331.2 592 256s-34.5-142.2-88.7-186.3c-10.3-8.4-11.8-23.5-3.5-33.8s23.5-11.8 33.8-3.5zM473.1 107c43.2 35.2 70.9 88.9 70.9 149s-27.7 113.8-70.9 149c-10.3 8.4-25.4 6.8-33.8-3.5s-6.8-25.4 3.5-33.8C475.3 341.3 496 301.1 496 256s-20.7-85.3-53.2-111.8c-10.3-8.4-11.8-23.5-3.5-33.8s23.5-11.8 33.8-3.5zm-60.5 74.5C434.1 199.1 448 225.9 448 256s-13.9 56.9-35.4 74.5c-10.3 8.4-25.4 6.8-33.8-3.5s-6.8-25.4 3.5-33.8C393.1 284.4 400 271 400 256s-6.9-28.4-17.7-37.3c-10.3-8.4-11.8-23.5-3.5-33.8s23.5-11.8 33.8-3.5zM301.1 34.8C312.6 40 320 51.4 320 64V448c0 12.6-7.4 24-18.9 29.2s-25 3.1-34.4-5.3L131.8 352H64c-35.3 0-64-28.7-64-64V224c0-35.3 28.7-64 64-64h67.8L266.7 40.1c9.4-8.4 22.9-10.4 34.4-5.3z"></path>&nbsp;</svg></span></li>
                <li class="o-nav-item">
                    <div data-bs-toggle="tab" aria-selected="true" class="o-nav-link o-active">Tra cứu</div>
                </li>
                <li class="o-nav-item">
                    <div data-bs-toggle="tab" aria-selected="false" class="o-nav-link">Dịch</div>
                </li>
            </ul>
            <div class="o-selected-result o-pt-1">
                <div>Đang t&igrave;m kiếm ...</div>
            </div>
        </div>
    </div>
</div>
<div id="envidictionaryOff"><br></div>
<div id="mttContainer" class="bootstrapiso notranslate" data-original-title="" title="" style="transform: translate(130px, 10px);"><br></div>
<p><br></p>', 1, GETDATE(), GETDATE());
GO


----------------------------------

--SELECT NEWID();
-- Code check quyền người dùng
SELECT
	Users.Email,
	users.FullName,
	Groups.Name AS [GroupName],
	FeatureName,
	Features.Url,
	Features.Description AS [FeatureDesc],
	features.HttpMethod
FROM Users JOIN UserGroups ON Users.Id = UserGroups.UserId
	JOIN Groups ON Groups.Id = UserGroups.GroupId
	JOIN  GroupFeatures ON GroupFeatures.GroupId = Groups.Id
	JOIN Features ON Features.Id  = GroupFeatures.FeatureId;

-- Code liệt kê người dùng, và group của người đó
SELECT
	Users.Email,
	users.FullName,
	Groups.Name AS [GroupName]
FROM Users JOIN UserGroups ON Users.Id = UserGroups.UserId
	JOIN Groups ON Groups.Id = UserGroups.GroupId;

-- Lấy các Feature của từng Group
SELECT
	Groups.Name AS [GroupName],
	FeatureName AS [FeatureName]
FROM Features JOIN GroupFeatures ON Features.Id = GroupFeatures.FeatureId
	JOIN Groups ON Groups.Id = GroupFeatures.GroupId;

-- Lấy các purpose của system
SELECT
	ExternalSystems.Id,
	ExternalSystems.Name,
	Purposes.Name
FROM ExternalSystems JOIN ExternalSystemPurposes ON ExternalSystems.Id = ExternalSystemPurposes.ExternalSystemId
	JOIN Purposes ON Purposes.Id = ExternalSystemPurposes.PurposeId;

SELECT * FROM Users;

-- Update rows in table 'Features'
UPDATE Features
SET
	[Url] = NULL,
	HttpMethod = NULL
	-- add more columns and values here
WHERE Id = '91678d66-0828-4981-a5b0-0b8b9f55a444' 	/* add search conditions here */
GO

SELECT * FROM Groups;

SELECT * FROM features
WHERE FeatureName = 'ResponsibilityManagement'
-- where ParentId = '1ce0f64c-21ff-4f25-9c51-0a2054cdc562'
-- where FeatureName like '%dsar%'
-- where id = '1ce0f64c-21ff-4f25-9c51-0a2054cdc562';

--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES 
--(NEWID(), 'GetRiskRegister', N'Lấy danh sách đăng ký rủi ro', '8cd84955-4dd5-47f3-8f04-c8e447a223aa', 1, '/api/Risk', 0, GETDATE(), GETDATE()),
--(NEWID(), 'RegisterRisk', N'Đăng ký rủi ro mới', '8cd84955-4dd5-47f3-8f04-c8e447a223aa', 1, '/api/Risk', 1, GETDATE(), GETDATE()),
--(NEWID(), 'ResolveRisk', N'Cập nhật trạng thái giải quyết rủi ro', '8cd84955-4dd5-47f3-8f04-c8e447a223aa', 1, '/api/Risk/resolve-risk/{id}', 2, GETDATE(), GETDATE()),
--(NEWID(), 'GetRiskById', N'Lấy thông tin rủi ro theo ID', '8cd84955-4dd5-47f3-8f04-c8e447a223aa', 1, '/api/Risk/{id}', 0, GETDATE(), GETDATE()),
--(NEWID(), 'UpdateRisk', N'Cập nhật thông tin rủi ro', '8cd84955-4dd5-47f3-8f04-c8e447a223aa', 1, '/api/Risk/{id}', 2, GETDATE(), GETDATE()),
--(NEWID(), 'DeleteRisk', N'Xóa rủi ro', '8cd84955-4dd5-47f3-8f04-c8e447a223aa', 1, '/api/Risk/{id}', 3, GETDATE(), GETDATE());

--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES 
--(NEWID(), 'GetPurposes', N'Lấy danh sách Purpose', '10ec4d72-e9ce-48b5-87aa-06852206a167', 1, '/api/Purpose', 0, GETDATE(), GETDATE()),
--(NEWID(), 'CreatePurpose', N'Tạo mới Purpose', '10ec4d72-e9ce-48b5-87aa-06852206a167', 1, '/api/Purpose', 1, GETDATE(), GETDATE()),
--(NEWID(), 'GetPurposeById', N'Lấy Purpose theo ID', '10ec4d72-e9ce-48b5-87aa-06852206a167', 1, '/api/Purpose/{id}', 0, GETDATE(), GETDATE()),
--(NEWID(), 'UpdatePurpose', N'Cập nhật Purpose (chỉ khi ở trạng thái Draft)', '10ec4d72-e9ce-48b5-87aa-06852206a167', 1, '/api/Purpose/{id}', 2, GETDATE(), GETDATE()),
--(NEWID(), 'DeletePurpose', N'Xóa Purpose (chỉ khi ở trạng thái Draft)', '10ec4d72-e9ce-48b5-87aa-06852206a167', 1, '/api/Purpose/{id}', 3, GETDATE(), GETDATE()),
--(NEWID(), 'UpdatePurposeStatus', N'Cập nhật trạng thái Purpose (Active hoặc Inactive)', '10ec4d72-e9ce-48b5-87aa-06852206a167', 1, '/api/Purpose/{id}/status', 2, GETDATE(), GETDATE());

--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES 
--(NEWID(), 'GetPrivacyPolicies', N'Lấy danh sách chính sách quyền riêng tư', '1791ec64-40c4-4fb6-b934-ccfdcac23324', 1, '/api/PrivacyPolicy', 0, GETDATE(), GETDATE()),
--(NEWID(), 'CreatePrivacyPolicy', N'Tạo mới chính sách quyền riêng tư', '1791ec64-40c4-4fb6-b934-ccfdcac23324', 1, '/api/PrivacyPolicy', 1, GETDATE(), GETDATE()),
--(NEWID(), 'GetActivePrivacyPolicy', N'Lấy chính sách đang được kích hoạt (chỉ một)', '1791ec64-40c4-4fb6-b934-ccfdcac23324', 1, '/api/PrivacyPolicy/get-policy', 0, GETDATE(), GETDATE()),
--(NEWID(), 'GetPrivacyPolicyById', N'Lấy chính sách quyền riêng tư theo ID', '1791ec64-40c4-4fb6-b934-ccfdcac23324', 1, '/api/PrivacyPolicy/{id}', 0, GETDATE(), GETDATE()),
--(NEWID(), 'UpdatePrivacyPolicy', N'Cập nhật chính sách quyền riêng tư', '1791ec64-40c4-4fb6-b934-ccfdcac23324', 1, '/api/PrivacyPolicy/{id}', 2, GETDATE(), GETDATE()),
--(NEWID(), 'DeletePrivacyPolicy', N'Xóa chính sách (chỉ khi chưa được kích hoạt)', '1791ec64-40c4-4fb6-b934-ccfdcac23324', 1, '/api/PrivacyPolicy/{id}', 3, GETDATE(), GETDATE()),
--(NEWID(), 'SetActivePrivacyPolicy', N'Đặt chính sách quyền riêng tư là đang hoạt động', '1791ec64-40c4-4fb6-b934-ccfdcac23324', 1, '/get-active/{id}', 2, GETDATE(), GETDATE());

--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES
--(NEWID(), 'GetConsentBanner', N'Lấy nội dung banner xin phép người dùng', '1ce0f64c-21ff-4f25-9c51-0a2054cdc562', 1, '/api/Consent/get-banner/{uniqueIdentifier}/{token}', 0, GETDATE(), GETDATE()),
--(NEWID(), 'GetDpmsConsentByEmail', N'Chỉ dùng nội bộ DPMS để lấy consent theo email', '1ce0f64c-21ff-4f25-9c51-0a2054cdc562', 1, '/api/Consent/dpms-consent/{email}', 0, GETDATE(), GETDATE()),
--(NEWID(), 'GetLinkForConsent', N'API public: Lấy link lấy consent theo email và system', '1ce0f64c-21ff-4f25-9c51-0a2054cdc562', 1, '/api-consent/get-link/{email}/{systemId}', 0, GETDATE(), GETDATE()),
--(NEWID(), 'GetConsentByEmailSystem', N'Lấy consent hiện tại theo email + systemId', '1ce0f64c-21ff-4f25-9c51-0a2054cdc562', 1, '/api-consent/get-consent/{email}/{systemId}', 0, GETDATE(), GETDATE()),
--(NEWID(), 'GetAllConsentLogs', N'Lấy danh sách toàn bộ mục đích xin phép', '1ce0f64c-21ff-4f25-9c51-0a2054cdc562', 1, '/api/Consent/consent-log', 0, GETDATE(), GETDATE()),
--(NEWID(), 'GetSystemConsentLogs', N'Lấy danh sách mục đích tại một hệ thống cụ thể', '1ce0f64c-21ff-4f25-9c51-0a2054cdc562', 1, '/api/Consent/consent-log/{systemId}', 0, GETDATE(), GETDATE()),
--(NEWID(), 'SubmitConsent', N'Người dùng gửi consent (insert vào bảng Consent & ConsentPurpose)', '1ce0f64c-21ff-4f25-9c51-0a2054cdc562', 1, '/api/Consent', 1, GETDATE(), GETDATE()),
--(NEWID(), 'DownloadConsentTemplate', N'Tải file mẫu nhập consent cho hệ thống', '1ce0f64c-21ff-4f25-9c51-0a2054cdc562', 1, '/api/Consent/download-template/{systemId}', 0, GETDATE(), GETDATE()),
--(NEWID(), 'ImportConsent', N'Nhập consent từ file Excel', '1ce0f64c-21ff-4f25-9c51-0a2054cdc562', 1, '/api/Consent/import-consent', 1, GETDATE(), GETDATE());

---- GET /api/DPIA
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Get DPIAs', 'Retrieve all DPIA records', '65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, '/api/DPIA', 0, GETDATE(), GETDATE());

---- POST /api/DPIA
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Add DPIA', 'Create a new DPIA record', '65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, '/api/DPIA', 1, GETDATE(), GETDATE());

---- GET /api/DPIA/dpia-detail/{id}
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Get DPIA Detail', 'Retrieve detailed DPIA information by ID', '65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, '/api/DPIA/dpia-detail/{id}', 0, GETDATE(), GETDATE());

---- GET /api/DPIA/{id}/comments
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Get DPIA Comments', 'Retrieve all comments for a DPIA record', '65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, '/api/DPIA/{id}/comments', 0, GETDATE(), GETDATE());

---- POST /api/DPIA/{id}/comments
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Add DPIA Comment', 'Add a comment to a DPIA record', '65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, '/api/DPIA/{id}/comments', 1, GETDATE(), GETDATE());

---- GET /api/DPIA/{id}/history
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Get DPIA History', 'Retrieve history of a DPIA record', '65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, '/api/DPIA/{id}/history', 0, GETDATE(), GETDATE());

---- PUT /api/DPIA/{id}
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Update DPIA', 'Update an existing DPIA record', '65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, '/api/DPIA/{id}', 2, GETDATE(), GETDATE());

---- DELETE /api/DPIA/{id}
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Delete DPIA', 'Delete a DPIA record by ID', '65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, '/api/DPIA/{id}', 3, GETDATE(), GETDATE());

---- GET /api/DPIA/{id}/members
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Get DPIA Members', 'Retrieve members involved in a DPIA', '65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, '/api/DPIA/{id}/members', 0, GETDATE(), GETDATE());

---- PUT /api/DPIA/{id}/members
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Update DPIA Members', 'Update members involved in a DPIA', '65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, '/api/DPIA/{id}/members', 2, GETDATE(), GETDATE());

---- POST /api/DPIA/{id}/members
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Add DPIA Members', 'Add members to a DPIA', '65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, '/api/DPIA/{id}/members', 1, GETDATE(), GETDATE());

---- GET /api/DPIA/members-for-dpia
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Get All Members for DPIA', 'Retrieve all possible members assignable to a DPIA', '65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, '/api/DPIA/members-for-dpia', 0, GETDATE(), GETDATE());

---- PUT /api/DPIA/{id}/update-members-responsibilities
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Update Members Responsibilities', 'Update responsibilities of DPIA members', '65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, '/api/DPIA/{id}/update-members-responsibilities', 2, GETDATE(), GETDATE());

---- PUT /api/DPIA/{dpiaId}/comments/{commentId}
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Update DPIA Comment', 'Update an existing DPIA comment by ID', '65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, '/api/DPIA/{id}/comments/{id}', 2, GETDATE(), GETDATE());

---- GET /api/DPIA/{dpiaId}/{responsibilityId}
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Get DPIA Responsibility', 'Retrieve responsibility detail in DPIA by responsibility ID', '65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, '/api/DPIA/{id}/{id}', 0, GETDATE(), GETDATE());

---- DELETE /api/DPIA/{dpiaId}/{responsibilityId}
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Delete DPIA Responsibility', 'Delete a specific DPIA responsibility', '65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, '/api/DPIA/{id}/{id}', 3, GETDATE(), GETDATE());

---- PUT /api/DPIA/{dpiaId}/{responsibilityId}/update-responsibility-members
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Update DPIA Responsibility Members', 'Update members assigned to a specific responsibility in DPIA', '65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, '/api/DPIA/{id}/{id}/update-responsibility-members', 2, GETDATE(), GETDATE());

---- Update Member Responsibility Status
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Update Member Responsibility Status', 'Update status of a member responsibility in the DPIA process', '65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, '/api/DPIA/{id}/update-member-responsibility-status', 2, GETDATE(), GETDATE());

---- Update Responsibility Status
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Update Responsibility Status', 'Update status for a DPIA responsibility', '65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, '/api/DPIA/{id}/update-responsibility-status', 2, GETDATE(), GETDATE());

---- Restart Responsibility
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Restart Responsibility', 'Restart a specific DPIA responsibility', '65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, '/api/DPIA/{id}/restart-responsibility/{id}', 2, GETDATE(), GETDATE());

---- Upload Documents for Responsibility
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Upload Documents to Responsibility', 'Upload documents for a DPIA responsibility', '65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, '/api/DPIA/{id}/{id}/upload-documents', 1, GETDATE(), GETDATE());

---- Delete Document
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Delete Document', 'Delete a specific document in a DPIA process', '65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, '/api/DPIA/{id}/documents/{id}', 3, GETDATE(), GETDATE());

---- Upload Document (Generic)
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Upload Document to DPIA', 'Upload documents for a DPIA process', '65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, '/api/DPIA/{id}/upload-document', 1, GETDATE(), GETDATE());

---- Start DPIA
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Start DPIA', 'Start the DPIA process', '65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, '/api/DPIA/{id}/start-dpia', 1, GETDATE(), GETDATE());

---- Request Approval
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Request Approval for DPIA', 'Request approval in the DPIA process', '65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, '/api/DPIA/{id}/request-approval', 1, GETDATE(), GETDATE());

---- Approve DPIA
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Approve DPIA', 'Approve the DPIA process', '65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, '/api/DPIA/{id}/approve', 1, GETDATE(), GETDATE());

---- Reject DPIA
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Reject DPIA', 'Reject the DPIA process', '65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, '/api/DPIA/{id}/reject', 1, GETDATE(), GETDATE());

---- Restart DPIA
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Restart DPIA', 'Restart the DPIA process', '65cd3b28-dfe2-4a2d-922a-05eefd83b43c', 1, '/api/DPIA/{id}/restart', 1, GETDATE(), GETDATE());

---- EXTERNAL SYSTEM
---- Get All External Systems
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Get External Systems', 'Gets all external systems', '213b812f-2973-4c73-8b0f-bd6f9a195396', 1, '/api/ExternalSystem', 0, GETDATE(), GETDATE());

---- Add New External System
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Add External System', 'Adds a new external system', '213b812f-2973-4c73-8b0f-bd6f9a195396', 1, '/api/ExternalSystem', 1, GETDATE(), GETDATE());

---- Delete External System
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Delete External System', 'Removes an external system', '213b812f-2973-4c73-8b0f-bd6f9a195396', 1, '/api/ExternalSystem', 3, GETDATE(), GETDATE());

---- Add Purpose to External System
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Add Purpose to External System', 'Adds a purpose to an external system', '213b812f-2973-4c73-8b0f-bd6f9a195396', 1, '/api/ExternalSystem/add-purpose', 1, GETDATE(), GETDATE());

---- Bulk Add Purposes
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Bulk Add Purposes', 'Bulk adds purposes to an external system', '213b812f-2973-4c73-8b0f-bd6f9a195396', 1, '/api/ExternalSystem/bulk-add-purposes', 1, GETDATE(), GETDATE());

---- Get All Purposes for System
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Get Purposes of External System', 'Gets all purposes of an external system', '213b812f-2973-4c73-8b0f-bd6f9a195396', 1, '/api/ExternalSystem/{systemId}/purposes', 0, GETDATE(), GETDATE());

---- Remove a Single Purpose
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Remove Purpose from External System', 'Removes a single purpose from an external system', '213b812f-2973-4c73-8b0f-bd6f9a195396', 1, '/api/ExternalSystem/remove-purpose', 3, GETDATE(), GETDATE());

---- Bulk Remove Purposes
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Bulk Remove Purposes', 'Bulk removes purposes from an external system', '213b812f-2973-4c73-8b0f-bd6f9a195396', 1, '/api/ExternalSystem/bulk-remove-purposes', 3, GETDATE(), GETDATE());

---- Get Users of External System
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Get Users of External System', 'Gets all users of an external system', '213b812f-2973-4c73-8b0f-bd6f9a195396', 1, '/api/ExternalSystem/{systemId}/get-users', 0, GETDATE(), GETDATE());

---- Manage External System - Get system details
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Get System Details', 'Gets the details of an external system.', '213b812f-2973-4c73-8b0f-bd6f9a195396', 1, '/api/ExternalSystem/{systemId}/get-system-details', 0, GETDATE(), GETDATE());

---- Manage External System - Update system users
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Update System Users', 'Updates the users of an external system.', '213b812f-2973-4c73-8b0f-bd6f9a195396', 1, '/api/ExternalSystem/update-system-users', 2, GETDATE(), GETDATE());

---- Manage External System - Update active status
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Update Active Status', 'Updates the status of an external system.', '213b812f-2973-4c73-8b0f-bd6f9a195396', 1, '/api/ExternalSystem/update-active-status', 2, GETDATE(), GETDATE());

---- Manage External System - Update system
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Update External System', 'Updates an external system.', '213b812f-2973-4c73-8b0f-bd6f9a195396', 1, '/api/ExternalSystem/update-system', 2, GETDATE(), GETDATE());

---- Manage External System - Get API Key
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Get API Key', 'Generates an API key for an external system.', '213b812f-2973-4c73-8b0f-bd6f9a195396', 1, '/api/ExternalSystem/get-key/{systemId}', 0, GETDATE(), GETDATE());

-------- FORM FIC -------
---- Get All Form Templates
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Get Form Templates', 'Get all form templates', 'a24ffad0-5e1c-49c0-9465-da3bb76ae090', 1, '/api/Form/get-templates', 0, GETDATE(), GETDATE());

---- Update Form Status
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Update Form Status', 'Update the status of a form', 'a24ffad0-5e1c-49c0-9465-da3bb76ae090', 1, '/api/Form/update-status/{id}', 2, GETDATE(), GETDATE());

---- Get Form Details
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Get Form Details', 'Get form details by ID', 'a24ffad0-5e1c-49c0-9465-da3bb76ae090', 1, '/api/Form/{id}', 0, GETDATE(), GETDATE());

---- Delete Form
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Delete Form', 'Delete a form by ID', 'a24ffad0-5e1c-49c0-9465-da3bb76ae090', 1, '/api/Form/{id}', 3, GETDATE(), GETDATE());

---- Save Form
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Save Form', 'Save form', 'a24ffad0-5e1c-49c0-9465-da3bb76ae090', 1, '/api/Form/save', 1, GETDATE(), GETDATE());

---- Edit Form
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Edit Form', 'Only edit forms with Draft Status (latest version); will not create a new version', 'a24ffad0-5e1c-49c0-9465-da3bb76ae090', 1, '/api/Form/edit', 1, GETDATE(), GETDATE());

---- Update Form (New Version)
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Update Form (New Version)', 'Update form and create a new version of it', 'a24ffad0-5e1c-49c0-9465-da3bb76ae090', 1, '/api/Form/update', 1, GETDATE(), GETDATE());

---- Submit Form
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Submit Form', 'Submit form', 'a24ffad0-5e1c-49c0-9465-da3bb76ae090', 1, '/api/Form/submit', 1, GETDATE(), GETDATE());

---- Get Submissions
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Get Submissions', 'Get all FIC submissions (for an external system or all DPMS systems)', 'a24ffad0-5e1c-49c0-9465-da3bb76ae090', 1, '/api/Form/get-submissions', 0, GETDATE(), GETDATE());

---- Get Submission Details
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Get Submission Details', 'Get FIC submission details', 'a24ffad0-5e1c-49c0-9465-da3bb76ae090', 1, '/api/Form/submission/{id}', 0, GETDATE(), GETDATE());

---- Get All Responsibilities
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Get Responsibilities', 'Retrieve all responsibilities', 'e52ee063-84f1-412b-a68b-f58fa882d19e', 1, '/api/Responsibility', 0, GETDATE(), GETDATE());

---- Create Responsibility
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Create Responsibility', 'Create a new responsibility', 'e52ee063-84f1-412b-a68b-f58fa882d19e', 1, '/api/Responsibility', 1, GETDATE(), GETDATE());

---- Get Responsibility by ID
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Get Responsibility by ID', 'Retrieve a responsibility by its ID', 'e52ee063-84f1-412b-a68b-f58fa882d19e', 1, '/api/Responsibility/{id}', 0, GETDATE(), GETDATE());

---- Update Responsibility
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Update Responsibility', 'Update an existing responsibility', 'e52ee063-84f1-412b-a68b-f58fa882d19e', 1, '/api/Responsibility/{id}', 2, GETDATE(), GETDATE());

---- Delete Responsibility
--INSERT INTO Features (Id, FeatureName, Description, ParentId, State, Url, HttpMethod, CreatedAt, LastModifiedAt)
--VALUES (NEWID(), 'Delete Responsibility', 'Delete a responsibility by ID', 'e52ee063-84f1-412b-a68b-f58fa882d19e', 1, '/api/Responsibility/{id}', 3, GETDATE(), GETDATE());

