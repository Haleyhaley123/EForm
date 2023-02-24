-- Đánh giá nguy cơ vi khuẩn đa kháng
Insert into Forms(Id, CreatedAt, UpdatedAt, IsDeleted, Name, Code, VisitTypeGroupCode)Values(NEWID(), GETDATE(), GETDATE(), 0, N'Đánh giá nguy cơ vi khuẩn đa kháng (MDRO)', 'A02_006_221221_V', 'IPD')
Insert into Forms(Id, CreatedAt, UpdatedAt, IsDeleted, Name, Code, VisitTypeGroupCode)Values(NEWID(), GETDATE(), GETDATE(), 0, N'Đánh giá nguy cơ vi khuẩn đa kháng (MDRO)', 'A02_006_221221_V', 'OPD')
Insert into Forms(Id, CreatedAt, UpdatedAt, IsDeleted, Name, Code, VisitTypeGroupCode)Values(NEWID(), GETDATE(), GETDATE(), 0, N'Đánh giá nguy cơ vi khuẩn đa kháng (MDRO)', 'A02_006_221221_V', 'EOC')
Insert into Forms(Id, CreatedAt, UpdatedAt, IsDeleted, Name, Code, VisitTypeGroupCode)Values(NEWID(), GETDATE(), GETDATE(), 0, N'Đánh giá nguy cơ vi khuẩn đa kháng (MDRO)', 'A02_006_221221_V', 'ED')

-- phân quyền Đánh giá nguy cơ vi khuẩn đa kháng
insert into Actions (Id, CreatedAt, UpdatedAt, IsDeleted, Name , Code, VisitTypeGroupId) values (NEWID(), GETDATE(), GETDATE(), 'False',N'[ED][Tạo mới] BS Đánh giá nguy cơ vi khuẩn đa kháng' , N'APICREDA02_006_221221_V', NULL);
insert into Actions (Id, CreatedAt, UpdatedAt, IsDeleted, Name , Code, VisitTypeGroupId) values (NEWID(), GETDATE(), GETDATE(), 'False',N'[ED][Chỉnh sửa] BS Đánh giá nguy cơ vi khuẩn đa kháng' , N'APIUDEDA02_006_221221_V', NULL);
insert into Actions (Id, CreatedAt, UpdatedAt, IsDeleted, Name , Code, VisitTypeGroupId) values (NEWID(), GETDATE(), GETDATE(), 'False',N'[ED][Xác nhận] BS Đánh giá nguy cơ vi khuẩn đa kháng' , N'APICFEDA02_006_221221_V', NULL);
insert into Actions (Id, CreatedAt, UpdatedAt, IsDeleted, Name , Code, VisitTypeGroupId) values (NEWID(), GETDATE(), GETDATE(), 'False',N'[ED][Xem] BS Đánh giá nguy cơ vi khuẩn đa kháng' , N'APIGDTEDA02_006_221221_V', NULL);
insert into Actions (Id, CreatedAt, UpdatedAt, IsDeleted, Name , Code, VisitTypeGroupId) values (NEWID(), GETDATE(), GETDATE(), 'False',N'[IPD][Tạo mới] BS Đánh giá nguy cơ vi khuẩn đa kháng' , N'APICRIPDA02_006_221221_V', NULL);
insert into Actions (Id, CreatedAt, UpdatedAt, IsDeleted, Name , Code, VisitTypeGroupId) values (NEWID(), GETDATE(), GETDATE(), 'False',N'[IPD][Chỉnh sửa] BS Đánh giá nguy cơ vi khuẩn đa kháng' , N'APIUDIPDA02_006_221221_V', NULL);
insert into Actions (Id, CreatedAt, UpdatedAt, IsDeleted, Name , Code, VisitTypeGroupId) values (NEWID(), GETDATE(), GETDATE(), 'False',N'[IPD][Xác nhận] BS Đánh giá nguy cơ vi khuẩn đa kháng' , N'APICFIPDA02_006_221221_V', NULL);
insert into Actions (Id, CreatedAt, UpdatedAt, IsDeleted, Name , Code, VisitTypeGroupId) values (NEWID(), GETDATE(), GETDATE(), 'False',N'[IPD][Xem] BS Đánh giá nguy cơ vi khuẩn đa kháng' , N'APIGDTIPDA02_006_221221_V', NULL);
insert into Actions (Id, CreatedAt, UpdatedAt, IsDeleted, Name , Code, VisitTypeGroupId) values (NEWID(), GETDATE(), GETDATE(), 'False',N'[EOC][Tạo mới] BS Đánh giá nguy cơ vi khuẩn đa kháng' , N'APICREOCA02_006_221221_V', NULL);
insert into Actions (Id, CreatedAt, UpdatedAt, IsDeleted, Name , Code, VisitTypeGroupId) values (NEWID(), GETDATE(), GETDATE(), 'False',N'[EOC][Chỉnh sửa] BS Đánh giá nguy cơ vi khuẩn đa kháng' , N'APIUDEOCA02_006_221221_V', NULL);
insert into Actions (Id, CreatedAt, UpdatedAt, IsDeleted, Name , Code, VisitTypeGroupId) values (NEWID(), GETDATE(), GETDATE(), 'False',N'[EOC][Xác nhận] BS Đánh giá nguy cơ vi khuẩn đa kháng' , N'APICFEOCA02_006_221221_V', NULL);
insert into Actions (Id, CreatedAt, UpdatedAt, IsDeleted, Name , Code, VisitTypeGroupId) values (NEWID(), GETDATE(), GETDATE(), 'False',N'[EOC][Xem] BS Đánh giá nguy cơ vi khuẩn đa kháng' , N'APIGDTEOCA02_006_221221_V', NULL);
insert into Actions (Id, CreatedAt, UpdatedAt, IsDeleted, Name , Code, VisitTypeGroupId) values (NEWID(), GETDATE(), GETDATE(), 'False',N'[OPD][Tạo mới] BS Đánh giá nguy cơ vi khuẩn đa kháng' , N'APICROPDA02_006_221221_V', NULL);
insert into Actions (Id, CreatedAt, UpdatedAt, IsDeleted, Name , Code, VisitTypeGroupId) values (NEWID(), GETDATE(), GETDATE(), 'False',N'[OPD][Chỉnh sửa] BS Đánh giá nguy cơ vi khuẩn đa kháng' , N'APIUDOPDA02_006_221221_V', NULL);
insert into Actions (Id, CreatedAt, UpdatedAt, IsDeleted, Name , Code, VisitTypeGroupId) values (NEWID(), GETDATE(), GETDATE(), 'False',N'[OPD][Xác nhận] BS Đánh giá nguy cơ vi khuẩn đa kháng' , N'APICFOPDA02_006_221221_V', NULL);
insert into Actions (Id, CreatedAt, UpdatedAt, IsDeleted, Name , Code, VisitTypeGroupId) values (NEWID(), GETDATE(), GETDATE(), 'False',N'[OPD][Xem] BS Đánh giá nguy cơ vi khuẩn đa kháng' , N'APIGDTOPDA02_006_221221_V', NULL);

-- nhóm HSBA Đánh giá nguy cơ vi khuẩn đa kháng
Insert into MasterDatas (Id, CreatedAt, UpdatedAt, IsDeleted,ViName, EnName, Code, [Group], Form, [Level], [Order], DataType, Note, IsReadOnly,Data, Clinic, [Version]) values (NEWID(), GETDATE(), GETDATE(), 'False', N'Đánh giá nguy cơ vi khuẩn đa kháng',N'Đánh giá nguy cơ vi khuẩn đa kháng',N'MultidrugResistant',N'BAHC',N'NHOMBIEUMAU',N'2',N'25',N'Lable',N'',N'',N'',N'', '');