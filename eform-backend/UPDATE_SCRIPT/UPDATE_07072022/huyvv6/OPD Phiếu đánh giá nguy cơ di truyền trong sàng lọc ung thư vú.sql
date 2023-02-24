﻿INSERT INTO [dbo].[Forms]([Id], [IsDeleted], [CreatedAt], [UpdatedAt], [Name], [Code], [VisitTypeGroupCode]) VALUES (NEWID(), 0, '2022-03-03 00:00:00.000', '2022-03-03 00:00:00.000',N'Phiếu đánh giá nguy cơ di truyền trong sàng lọc ung thư vú', 'A01_201_201119_V', 'OPD')
insert into Actions  (Id, CreatedAt, UpdatedAt, IsDeleted, Name , Code, VisitTypeGroupId) values (NEWID(), GETDATE(), GETDATE(), 'False',N'[OPD] [BS CHỈNH SỬA] Phiếu đánh giá nguy cơ di truyền trong sàng lọc ung thư vú' , N'NCCNBROV1POST', (select id from VisitTypeGroups where Code = 'OPD'));
insert into Actions  (Id, CreatedAt, UpdatedAt, IsDeleted, Name , Code, VisitTypeGroupId) values (NEWID(), GETDATE(), GETDATE(), 'False',N'[OPD] [VIEW] Phiếu đánh giá nguy cơ di truyền trong sàng lọc ung thư vú' , N'NCCNBROV1GET', (select id from VisitTypeGroups where Code = 'OPD'));
insert into MasterDatas(Id,ViName,EnName,Code,[Group],Note,IsDeleted,CreatedAt,[Level],[Order],UpdatedAt,[Version],Form) VALUES (newId(),N'Phiếu đánh giá nguy cơ di truyền trong sàng lọc ung thư vú','NCCN BR/OV-1','A01_201_201119_V','MedicalRecords','OPD',0,GETDATE(),0,0,GETDATE(),1,'A01_201_201119_V')

